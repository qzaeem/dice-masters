using DiceGame.ScriptableObjects;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace DiceGame.UI
{
    public class SP_BankrollBattleCanvas : MenuBase
    {
        [Header("Fields")]
        [SerializeField] private BoolVariable canBankScoreVariable;
        [SerializeField] private StringListVariable playerNames;
        [SerializeField] private TMP_Text winnerNameTMP;
        [SerializeField] private TMP_Text winnerScoreTMP;
        [SerializeField] private TMP_Text roundTMP;

        [Header("Players")]
        [SerializeField] private PlayerInfoEntryUI playerInfoEntryUIPrefab;
        [SerializeField] private Transform playerInfoEntryContainer;
        private GameModeBankrollBattleSP gameModeBankrollBattle;
        private Dictionary<int, PlayerInfoEntryUI> playerEntries
            = new Dictionary<int, PlayerInfoEntryUI>();

        public override void OnEnable()
        {
            rollDiceButton.onClick.AddListener(RollDice);
            canBankScoreVariable.onValueChange += OnChangeBankable;
            gameScore.onValueChange += OnUpdateGameScore;
            diceRollingVariable.onValueChange += OnDiceRollChanged;
        }

        public override void OnDisable()
        {
            rollDiceButton.onClick.RemoveListener(RollDice);
            canBankScoreVariable.onValueChange -= OnChangeBankable;
            gameScore.onValueChange -= OnUpdateGameScore;
            diceRollingVariable.onValueChange -= OnDiceRollChanged;
        }

        public override void Start()
        {
            popupManager = PopupManagerCanvas.Instance;
            gameScoreTMP.text = gameMode.gameScore.value.ToString();
            gameResultPanel.SetActive(false);
            gameModeBankrollBattle = gameMode as GameModeBankrollBattleSP;
            roundTMP.text = $"ROUND {roundVariable.value}";
        }

        public PlayerInfoEntryUI SpawnPlayer(int id, string name, bool showScore = true)
        {
            var entry = Instantiate(playerInfoEntryUIPrefab, playerInfoEntryContainer);
            entry.InitializeForSinglePlayer(id, OnBankButtonPressed);
            entry.UpdateEntry(name, 0);
            playerEntries.Add(id, entry);
            return entry;
        }

        public override void OnRoundChanged(int value)
        {
            roundTMP.text = $"ROUND {value}";
            foreach (var entry in playerEntries.Values)
                entry.BankButtonInteractable(false);
        }

        private void OnChangeBankable(bool value)
        {
            foreach(var player in gameModeBankrollBattle.PlayersData.Values)
            {
                player.entryUI.BankButtonInteractable(
                    value
                    && !player.hasBankedScore
                    && rollVariable.value >= 3
                    );
            }
        }

        private void OnBankButtonPressed(int id)
        {
            playerEntries[id].BankButtonInteractable(false);
            gameModeBankrollBattle.BankScore(id);
        }

        public void OnPlayerTurnChange(int id)
        {
            rollDiceButton.gameObject.SetActive(true);
            foreach(var entry in playerEntries)
            {
                entry.Value.SetHighlight(false);
            }
            playerEntries[id].SetHighlight(true);
        }

        public override void EndGame()
        {
            gameMode.OnGameEnded();
            var winner = gameModeBankrollBattle.PlayersData.OrderByDescending(kvp => kvp.Value.totalScore).FirstOrDefault().Value;
            winnerNameTMP.text = winner.playerName;
            winnerScoreTMP.text = winner.totalScore.ToString();
            rollDiceButton.gameObject.SetActive(false);
            gameResultPanel.SetActive(true);

            foreach (var entry in playerEntries.Values)
                entry.BankButtonVisible(false);
        }

        public override void OnUpdateScoresUI()
        {
            foreach (var entry in playerEntries)
            {
                var player = gameModeBankrollBattle.PlayersData[entry.Key];

                if (player != null)
                {
                    entry.Value.UpdateEntry(player.playerName, player.totalScore);
                }
            }
        }

        public override void OnMasterChanged()
        {

        }
    }
}
