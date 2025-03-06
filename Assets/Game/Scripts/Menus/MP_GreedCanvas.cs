using DiceGame.ScriptableObjects;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine.UI;
using DiceGame.Game;
using Newtonsoft.Json;

namespace DiceGame.UI
{
    public class MP_GreedCanvas : MenuBase
    {
        [Header("Fields")]
        [SerializeField] private BoolVariable canBankScoreVariable;
        [SerializeField] private PlayersListVariable players;
        [SerializeField] private PlayerRefVariable changeTurnVariable;
        [SerializeField] private RollRecordActionSO greedRollRecordAction;

        [Header("Local Player")]
        [SerializeField] private DiceScorePanelGreed diceScorePanel;
        [SerializeField] private TMP_Text localPlayerNameTMP;
        [SerializeField] private TMP_Text localPlayerScoreTMP;
        [SerializeField] private TMP_Text winnerNameTMP;
        [SerializeField] private TMP_Text winnerScoreTMP;
        [SerializeField] private TMP_Text roundTMP;
        [SerializeField] private TMP_Text rollButonTMP;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color highlightedColor;

        [Header("Other Players")]
        [SerializeField] private PlayerInfoEntryUI playerInfoEntryUIPrefab;
        [SerializeField] private Transform playerInfoEntryContainer;
        private Dictionary<PlayerRef, PlayerInfoEntryUI> otherPlayersEntries
            = new Dictionary<PlayerRef, PlayerInfoEntryUI>();

        private bool _isHighlighted;

        public override void OnEnable()
        {
            base.OnEnable();
            players.onListValueChange += UpdatePlayerInfoEntries;
            //changeTurnVariable.onValueChange += OnPlayerTurnChange;
            canBankScoreVariable.onValueChange += OnChangeBankable;
            greedRollRecordAction.executeAction += OnReceivedRollRecord;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            players.onListValueChange -= UpdatePlayerInfoEntries;
            //changeTurnVariable.onValueChange -= OnPlayerTurnChange;
            canBankScoreVariable.onValueChange -= OnChangeBankable;
            greedRollRecordAction.executeAction -= OnReceivedRollRecord;
        }

        public override void Start()
        {
            base.Start();
            diceScorePanel.Initialize(gameMode.numberOfDice, OnSubmittedScore); 
            diceScorePanel.gameObject.SetActive(false);
            localPlayerNameTMP.text = localPlayerManager.playerName;
            roundTMP.text = $"ROUND {roundVariable.value}";
            UpdatePlayerInfoEntries(null);
            OnPlayerTurnChange(gameMode.ActivePlayerTurn);
            if (gameMode.HasGameEnded)
            {
                EndGame();
            }
        }

        public void SetDiceRollButton(bool active, string text)
        {
            rollButonTMP.text = text;
            rollDiceButton.gameObject.SetActive(active);
        }

        public override void OnRoundChanged(int value)
        {
            roundTMP.text = $"ROUND {value}";
            bankScoreButton.interactable = false;
            SetDiceRollButton(true, "Roll Dice");

            foreach(var infoEntry in otherPlayersEntries.Values)
            {
                infoEntry.ClearAllDropdownEntries();
            }
        }

        public override void BankScore()
        {
            base.BankScore();
            bankScoreButton.interactable = false;
            rollDiceButton.gameObject.SetActive(false);
        }

        public override void RollDice()
        {
            base.RollDice();
            bankScoreButton.interactable = false;
        }

        public override void OnMasterChanged()
        {

        }

        public override void EndGame()
        {
            gameMode.OnGameEnded();
            var winner = gameMode.players.value.OrderByDescending(p => p.totalScore).FirstOrDefault();
            winnerNameTMP.text = winner.playerName;
            winnerScoreTMP.text = winner.totalScore.ToString();
            rollDiceButton.gameObject.SetActive(false);
            gameResultPanel.SetActive(true);
            bankScoreButton.gameObject.SetActive(false);

            foreach (var entry in otherPlayersEntries)
            {
                entry.Value.ShowScore();
            }
        }

        public override void OnUpdateScoresUI()
        {
            localPlayerScoreTMP.text = localPlayerManager.totalScore.ToString();

            foreach (var entry in otherPlayersEntries)
            {
                var player = gameMode.players.value.FirstOrDefault(p => p.playerRef == entry.Key);

                if (player)
                {
                    entry.Value.UpdateEntry(player.playerName, player.totalScore, !gameMode.showScoresOnEnd);
                }
            }
        }

        public void OnPlayerTurnChange(PlayerRef playerRef)
        {
            SetDiceRollButton(true, "Roll Dice");
            backgroundImage.color = highlightedColor;
            _isHighlighted = true;

            /*
            _isHighlighted = false;
            backgroundImage.color = normalColor;
            foreach (var kvp in otherPlayersEntries) kvp.Value.SetHighlight(false);

            if (playerRef == PlayerManager.LocalPlayer.playerRef)
            {
                SetDiceRollButton(true, "Roll Dice");
                backgroundImage.color = highlightedColor;
                _isHighlighted = true;
                return;
            }

            SetDiceRollButton(false, "Roll Dice");
            var entry = otherPlayersEntries.FirstOrDefault(kvp => kvp.Key == playerRef).Value;

            if (entry != null)
            {
                entry.SetHighlight(true);
            }
            */
        }

        public void EnableDiceScorePanel(bool enabled)
        {
            diceScorePanel.gameObject.SetActive(enabled);
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
            }

            OnUpdateScoresUI();
        }

        private void OnChangeBankable(bool value)
        {
            bankScoreButton.interactable = value
                && !PlayerManager.LocalPlayer.hasBankedScore
                && rollVariable.value >= 3;
        }

        private void OnSubmittedScore(int combinedScore)
        {
            if (gameMode.diceRollManager.AllDiceLocked)
            {
                gameMode.diceRollManager.UnlockAllDice();
                SetDiceRollButton(true, "Re-Roll Dice");
            }
            else
            {
                SetDiceRollButton(true, "Roll Dice");
            }

            UpdateDiceRecord(gameMode.diceRollManager.dice);

            bankScoreButton.interactable = true;
            gameMode.UpdateGameScore(combinedScore);
            EnableDiceScorePanel(false);
        }

        private void OnReceivedRollRecord()
        {
            var playerRef = greedRollRecordAction.playerRef;
            var playerRoll = JsonConvert.DeserializeObject<PlayerRollRecord>(greedRollRecordAction.rollJson);

            if(otherPlayersEntries.TryGetValue(playerRef, out var otherPlayer))
            {
                otherPlayer.AddDropdownEntry(playerRoll);
            }
        }
    }
}
