using DiceGame.ScriptableObjects;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine.UI;
using DiceGame.Game;
using Newtonsoft.Json;
using System.Collections;

namespace DiceGame.UI
{
    public class SP_TileKnockCanvas : MenuBase
    {
        [Header("Fields")]
        [SerializeField] private Transform tilesContainer;
        [SerializeField] private Transform resultsContainer;
        [SerializeField] private TilePanel tilePanelPrefab;
        [SerializeField] private GameObject roundResultPanelPrefab;
        [SerializeField] private TMP_Text resultTextPrefab;
        [SerializeField] private GridLayoutGroup tilesGridLayoutGroup;
        [SerializeField] private BoolVariable canBankScoreVariable;

        [Header("Round")]
        [SerializeField] private TMP_Text roundTMP;
        [SerializeField] private TMP_Text currentTurnScoreTMP;

        [Header("Players")]
        [SerializeField] private PlayerInfoEntryUI playerInfoEntryUIPrefab;
        [SerializeField] private Transform playerInfoEntryContainer;
        private Dictionary<int, PlayerInfoEntryUI> playerEntries
            = new Dictionary<int, PlayerInfoEntryUI>();
        private Dictionary<uint, TilePanel> tilesDictionary = new Dictionary<uint, TilePanel>();
        private GameModeTileKnockSP gameModeTileKnock;

        public override void OnEnable()
        {
            base.OnEnable();
            canBankScoreVariable.onValueChange += OnChangeBankable;
            bankScoreButton.onClick.AddListener(OnSubmitClicked);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            canBankScoreVariable.onValueChange -= OnChangeBankable;
            bankScoreButton.onClick.RemoveListener(OnSubmitClicked);
        }

        public override void Start()
        {
            base.Start();
            gameModeTileKnock = (GameModeTileKnockSP)gameMode;
            roundTMP.text = $"ROUND {roundVariable.value}";
            if (gameMode.HasGameEnded)
            {
                EndGame();
            }
        }

        public PlayerInfoEntryUI SpawnPlayer(int id, string name, bool showScore = true)
        {
            var entry = Instantiate(playerInfoEntryUIPrefab, playerInfoEntryContainer);
            entry.UpdateEntry(name, 0);
            playerEntries.Add(id, entry);
            return entry;
        }

        public void SetDiceRollButton(bool active)
        {
            rollDiceButton.gameObject.SetActive(active);
        }

        public override void OnRoundChanged(int value)
        {
            roundTMP.text = $"ROUND {value}";
            SetDiceRollButton(true);
        }

        public override void OnMasterChanged()
        {

        }

        public override void BankScore()
        {

        }

        public void OnPlayerTurnChange(int id)
        {
            rollDiceButton.gameObject.SetActive(true);
            foreach (var entry in playerEntries)
            {
                entry.Value.SetHighlight(false);
            }
            playerEntries[id].SetHighlight(true);
        }

        private IEnumerator RefreshUI()
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)resultsContainer.transform);
        }

        public override void EndGame()
        {
            gameMode.OnGameEnded();
            gameResultPanel.SetActive(true);
            var players = gameModeTileKnock.PlayersData.Values;

            for (int i = 0; i < gameModeTileKnock.maxRounds; i++)
            {
                var roundPanel = Instantiate(roundResultPanelPrefab, resultsContainer.transform);
                roundPanel.GetComponentInChildren<TMP_Text>().text = $"Round {i + 1}";

                foreach (var player in players)
                {
                    var textTMP = Instantiate(resultTextPrefab, roundPanel.transform);
                    textTMP.text = $"{player.playerName}: {player.roundScores[i]}";
                }

                {
                    var winner = players.OrderBy(p => p.roundScores[i]).ToArray()[0];
                    var textTMP = Instantiate(resultTextPrefab, roundPanel.transform);
                    textTMP.text = $"{winner.playerName} won with score {winner.roundScores[i]}";
                }
            }

            {
                var roundPanel = Instantiate(roundResultPanelPrefab, resultsContainer.transform);
                var winner = players.OrderBy(p => p.roundScores.Sum()).ToArray()[0];
                roundPanel.GetComponentInChildren<TMP_Text>().text = $"{winner.playerName} won the game with score {winner.roundScores.Sum()}";
            }

            foreach (var entry in playerEntries)
            {
                entry.Value.ShowScore();
            }

            StartCoroutine(RefreshUI());
        }

        private void OnChangeBankable(bool value)
        {
            bankScoreButton.interactable = value
                && !PlayerManager.LocalPlayer.hasBankedScore;
        }

        public void OnUpdateScoresUI(List<int> inactivePlayerIds)
        {
            if(gameModeTileKnock == null)
            {
                gameModeTileKnock = (GameModeTileKnockSP)gameMode;
            }

            foreach (var entry in playerEntries)
            {
                var player = gameModeTileKnock.PlayersData[entry.Key];

                if (player != null)
                {
                    entry.Value.UpdateEntry(player.playerName, player.totalScore, true, !inactivePlayerIds.Exists(id => entry.Key == id));
                }
            }
        }

        private void OnSubmitClicked()
        {
            var selectedTiles = tilesDictionary.Values.Where(t => t.IsSelected).ToList();
            gameModeTileKnock.SubmitTilesForPlayer(selectedTiles.Select(t => t.TileID).ToList());
            selectedTiles.ForEach(t => t.KnockDownTile());
        }

        private void OnClickedTile(uint id)
        {
            var tile = tilesDictionary[id];

            if (tile.IsSelected)
            {
                gameModeTileKnock.DeselectTile(id);
                tile.DeselectTile();
            }
            else if (gameModeTileKnock.SelectTile(id))
            {
                tile.SelectTile();
            }
        }

        public void ActivateTiles(bool activate)
        {
            foreach (var tile in tilesDictionary.Values)
            {
                tile.ActivateTile(!tile.isKnockedDown && activate);
            }
        }

        public void SpawnTiles(Dictionary<uint, uint> tiles)
        {
            if (tilesDictionary.Count > 0)
                DespawnTiles();

            tilesGridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

            switch (tiles.Count)
            {
                case 16:
                    tilesGridLayoutGroup.constraintCount = 4;
                    break;
                case 25:
                    tilesGridLayoutGroup.constraintCount = 5;
                    break;
                default:
                    tilesGridLayoutGroup.constraintCount = 3;
                    break;
            }

            foreach (var kvp in tiles)
            {
                var tile = Instantiate(tilePanelPrefab, tilesContainer);
                tile.InitializeTile(kvp.Key, kvp.Value, OnClickedTile);
                tilesDictionary.Add(kvp.Key, tile);
                tile.ActivateTile(false);
            }
        }

        private void DespawnTiles()
        {
            foreach (var kvp in tilesDictionary)
            {
                Destroy(kvp.Value.gameObject);
            }
            tilesDictionary.Clear();
        }

        public void SetCurrentScore(int score)
        {
            currentTurnScoreTMP.text = score.ToString();
        }
    }
}
