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
    public class MP_TileKnockCanvas : MenuBase
    {
        [Header("Fields")]
        [SerializeField] private Transform tilesContainer;
        [SerializeField] private Transform resultsContainer;
        [SerializeField] private TilePanel tilePanelPrefab;
        [SerializeField] private GameObject roundResultPanelPrefab;
        [SerializeField] private TMP_Text resultTextPrefab;
        [SerializeField] private GridLayoutGroup tilesGridLayoutGroup;
        [SerializeField] private PlayersListVariable players;
        [SerializeField] private BoolVariable canBankScoreVariable;

        [Header("Local Player")]
        [SerializeField] private TMP_Text localPlayerNameTMP;
        [SerializeField] private TMP_Text roundTMP;
        [SerializeField] private TMP_Text currentTurnScoreTMP;

        [Header("Other Players")]
        [SerializeField] private PlayerInfoEntryUI playerInfoEntryUIPrefab;
        [SerializeField] private Transform playerInfoEntryContainer;
        private Dictionary<PlayerRef, PlayerInfoEntryUI> otherPlayersEntries
            = new Dictionary<PlayerRef, PlayerInfoEntryUI>();
        private Dictionary<uint, TilePanel> tilesDictionary = new Dictionary<uint, TilePanel>();
        private GameModeTileKnock gameModeTileKnock;

        public override void OnEnable()
        {
            base.OnEnable();
            canBankScoreVariable.onValueChange += OnChangeBankable;
            players.onListValueChange += UpdatePlayerInfoEntries;
            bankScoreButton.onClick.AddListener(OnSubmitClicked);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            canBankScoreVariable.onValueChange -= OnChangeBankable;
            players.onListValueChange -= UpdatePlayerInfoEntries;
            bankScoreButton.onClick.RemoveListener(OnSubmitClicked);
        }

        public override void Start()
        {
            base.Start();
            gameModeTileKnock = (GameModeTileKnock)gameMode;
            localPlayerNameTMP.text = localPlayerManager.playerName;
            roundTMP.text = $"ROUND {roundVariable.value}";
            UpdatePlayerInfoEntries(null);
            if (gameMode.HasGameEnded)
            {
                EndGame();
            }
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

        private IEnumerator RefreshUI()
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)resultsContainer.transform);
        }

        public override void EndGame()
        {
            gameMode.OnGameEnded();
            gameResultPanel.SetActive(true);

            for (int i = 0; i < gameModeTileKnock.maxRounds; i++)
            {
                var roundPanel = Instantiate(roundResultPanelPrefab, resultsContainer.transform);
                roundPanel.GetComponentInChildren<TMP_Text>().text = $"Round {i + 1}";

                foreach(var player in players.value)
                {
                    var textTMP = Instantiate(resultTextPrefab, roundPanel.transform);
                    textTMP.text = $"{player.playerName}: {player.roundScores[i]}";
                }

                {
                    var winner = players.value.OrderBy(p => p.roundScores[i]).ToArray()[0];
                    var textTMP = Instantiate(resultTextPrefab, roundPanel.transform);
                    textTMP.text = $"{winner.playerName} won with score {winner.roundScores[i]}";
                }
            }

            {
                var roundPanel = Instantiate(roundResultPanelPrefab, resultsContainer.transform);
                var winner = players.value.OrderBy(p => p.roundScores.Sum()).ToArray()[0];
                roundPanel.GetComponentInChildren<TMP_Text>().text = $"{winner.playerName} won the game with score {winner.roundScores.Sum()}";
            }

            StartCoroutine(RefreshUI());
        }

        private void OnChangeBankable(bool value)
        {
            bankScoreButton.interactable = value
                && !PlayerManager.LocalPlayer.hasBankedScore;
        }

        public override void OnUpdateScoresUI()
        {
            foreach (var entry in otherPlayersEntries)
            {
                var player = gameMode.players.value.FirstOrDefault(p => p.playerRef == entry.Key);

                if (player)
                {
                    entry.Value.UpdateEntry(player.playerName, player.totalScore);
                }
            }
        }

        private void UpdatePlayerInfoEntries(PlayerManager player)
        {
            var playerRefs = players.value.Select(p => p.playerRef).ToList();
            var removedPlayers = otherPlayersEntries.Where(op => !playerRefs.Contains(op.Key)).ToList();
            removedPlayers.ForEach(p =>
            {
                otherPlayersEntries.Remove(p.Key);
                Destroy(p.Value.gameObject);
            });

            foreach (var playerRef in playerRefs)
            {
                if (otherPlayersEntries.ContainsKey(playerRef) || localPlayerManager.playerRef == playerRef)
                    continue;

                var entry = Instantiate(playerInfoEntryUIPrefab, playerInfoEntryContainer);
                otherPlayersEntries.Add(playerRef, entry);
                entry.UpdateEntry(player.playerName, player.totalScore);
            }

            OnUpdateScoresUI();
        }

        private void OnSubmitClicked()
        {
            var selectedTiles = tilesDictionary.Values.Where(t => t.IsSelected).ToList();
            gameModeTileKnock.SubmitTiles(selectedTiles.Select(t => t.TileID).ToList());
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
            else if(gameModeTileKnock.SelectTile(id))
            {
                tile.SelectTile();
            }
        }

        public void ActivateTiles(bool activate)
        {
            foreach(var tile in tilesDictionary.Values)
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

            foreach(var kvp in tiles)
            {
                var tile = Instantiate(tilePanelPrefab, tilesContainer);
                tile.InitializeTile(kvp.Key, kvp.Value, OnClickedTile);
                tilesDictionary.Add(kvp.Key, tile);
                tile.ActivateTile(false);
            }
        }

        private void DespawnTiles()
        {
            foreach(var kvp in tilesDictionary)
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
