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
    public class SP_GreedCanvas : MenuBase
    {
        [Header("Fields")]
        [SerializeField] private BoolVariable canBankScoreVariable;
        [SerializeField] private StringListVariable playerNames;
        [SerializeField] private PlayerRefVariable changeTurnVariable;
        [SerializeField] private RollRecordActionSO greedRollRecordAction;
        [SerializeField] private DiceScorePanelGreed diceScorePanel;
        [SerializeField] private TMP_Text winnerNameTMP;
        [SerializeField] private TMP_Text winnerScoreTMP;
        [SerializeField] private TMP_Text roundTMP;
        [SerializeField] private TMP_Text rollButonTMP;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color highlightedColor;

        [Header("Players Panel")]
        [SerializeField] private PlayerInfoEntryUI playerInfoEntryUIPrefab;
        [SerializeField] private Transform playerInfoEntryContainer;
        private Dictionary<int, PlayerInfoEntryUI> playerEntries
            = new Dictionary<int, PlayerInfoEntryUI>();

        private GameModeGreedSP gameModeGreedSP;
        private int _activePlayerId;

        public override void OnEnable()
        {
            base.OnEnable();
            //changeTurnVariable.onValueChange += OnPlayerTurnChange;
            canBankScoreVariable.onValueChange += OnChangeBankable;
            greedRollRecordAction.executeAction += OnReceivedRollRecord;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            //changeTurnVariable.onValueChange -= OnPlayerTurnChange;
            canBankScoreVariable.onValueChange -= OnChangeBankable;
            greedRollRecordAction.executeAction -= OnReceivedRollRecord;
        }

        public override void Start()
        {
            base.Start();
            diceScorePanel.Initialize(gameMode.numberOfDice, OnSubmittedScore);
            diceScorePanel.gameObject.SetActive(false);
            roundTMP.text = $"ROUND {roundVariable.value}";
            gameModeGreedSP = gameMode as GameModeGreedSP;
        }

        public PlayerInfoEntryUI SpawnPlayer(int id, string name, bool showScore = true)
        {
            var entry = Instantiate(playerInfoEntryUIPrefab, playerInfoEntryContainer);
            entry.InitializeForSinglePlayer(id, OnBankButtonPressed);
            entry.UpdateEntry(name, 0);
            playerEntries.Add(id, entry);
            return entry;
        }

        private void OnBankButtonPressed(int id)
        {
            playerEntries[id].BankButtonInteractable(false);
            gameModeGreedSP.BankScore(id);
        }

        public void SetDiceRollButton(bool active, string text)
        {
            rollButonTMP.text = text;
            rollDiceButton.gameObject.SetActive(active);
        }

        public override void OnRoundChanged(int value)
        {
            roundTMP.text = $"ROUND {value}";
            
            SetDiceRollButton(true, "Roll Dice");

            foreach (var infoEntry in playerEntries.Values)
            {
                infoEntry.BankButtonInteractable(false);
            }
        }

        public override void BankScore()
        {
            //base.BankScore();

            //int activePlayer = gameModeGreedSP.ActivePlayerID;
            //if (playerEntries.TryGetValue(activePlayer, out PlayerInfoEntryUI activePlayerEntry))
            //{
            //    activePlayerEntry.BankButtonInteractable(false);
            //}

            //rollDiceButton.gameObject.SetActive(false);
        }

        public override void RollDice()
        {
            base.RollDice();

            foreach (var infoEntry in playerEntries.Values)
            {
                infoEntry.BankButtonInteractable(false);
            }
        }

        public override void OnMasterChanged()
        {

        }

        public override void EndGame()
        {
            gameMode.OnGameEnded();
            var winner = gameModeGreedSP.PlayersData.Values.OrderByDescending(p => p.totalScore).FirstOrDefault();
            winnerNameTMP.text = winner.playerName;
            winnerScoreTMP.text = winner.totalScore.ToString();
            rollDiceButton.gameObject.SetActive(false);
            gameResultPanel.SetActive(true);

            foreach (var infoEntry in playerEntries.Values)
            {
                infoEntry.BankButtonInteractable(false);
                infoEntry.SetHighlight(false);
            }
        }

        public override void OnUpdateScoresUI()
        {
            foreach (var entry in playerEntries)
            {
                var player = gameModeGreedSP.PlayersData[entry.Key];

                if (player != null)
                {
                    entry.Value.UpdateEntry(player.playerName, player.totalScore);
                }
            }
        }

        public void OnPlayerTurnChange(int id)
        {
            rollDiceButton.gameObject.SetActive(true);
            foreach (var entry in playerEntries)
            {
                entry.Value.SetHighlight(false);
            }
            playerEntries[id].SetHighlight(true);
            _activePlayerId = id;
        }

        public void EnableDiceScorePanel(bool enabled)
        {
            diceScorePanel.gameObject.SetActive(enabled);
        }

        private void OnChangeBankable(bool value)
        {
            if (playerEntries.TryGetValue(_activePlayerId, out PlayerInfoEntryUI activePlayerEntry))
            {
                activePlayerEntry.BankButtonInteractable(value);
            }
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

            if (playerEntries.TryGetValue(_activePlayerId, out PlayerInfoEntryUI activePlayerEntry))
            {
                activePlayerEntry.BankButtonInteractable(true);
            }

            gameMode.UpdateGameScore(combinedScore);
            EnableDiceScorePanel(false);
        }

        private void OnReceivedRollRecord()
        {
            var playerRoll = JsonConvert.DeserializeObject<PlayerRollRecord>(greedRollRecordAction.rollJson);

            if (playerEntries.TryGetValue(_activePlayerId, out var otherPlayer))
            {
                otherPlayer.AddDropdownEntry(playerRoll);
            }
        }
    }
}
