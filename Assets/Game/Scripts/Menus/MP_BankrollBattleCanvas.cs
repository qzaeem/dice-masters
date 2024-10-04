using DiceGame.ScriptableObjects;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine.UI;
using DiceGame.Game;

namespace DiceGame.UI
{
    public class MP_BankrollBattleCanvas : MenuBase
    {
        [Header("Fields")]
        [SerializeField] private BoolVariable canBankScoreVariable;
        [SerializeField] private PlayersListVariable players;

        [Header("Local Player")]
        [SerializeField] private Button riskScoreButton;
        [SerializeField] private TMP_Text localPlayerNameTMP;
        [SerializeField] private TMP_Text localPlayerScoreTMP;
        [SerializeField] private TMP_Text winnerNameTMP;
        [SerializeField] private TMP_Text winnerScoreTMP;
        [SerializeField] private TMP_Text roundTMP;

        [Header("Other Players")]
        [SerializeField] private PlayerInfoEntryUI playerInfoEntryUIPrefab;
        [SerializeField] private Transform playerInfoEntryContainer;
        private GameModeBankrollBattle gameModeBankrollBattle;
        private Dictionary<PlayerRef, PlayerInfoEntryUI> otherPlayersEntries
            = new Dictionary<PlayerRef, PlayerInfoEntryUI>();

        public override void OnEnable()
        {
            base.OnEnable();
            riskScoreButton.onClick.AddListener(RiskScore);
            canBankScoreVariable.onValueChange += OnChangeBankable;
            roundVariable.onValueChange += ChangeRound;
            players.onListValueChange += UpdatePlayerInfoEntries;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            riskScoreButton.onClick.RemoveListener(RiskScore);
            canBankScoreVariable.onValueChange -= OnChangeBankable;
            roundVariable.onValueChange -= ChangeRound;
            players.onListValueChange -= UpdatePlayerInfoEntries;
        }

        public override void Start()
        {
            base.Start();
            gameModeBankrollBattle = gameMode as GameModeBankrollBattle;
            localPlayerNameTMP.text = localPlayerManager.playerName;
            riskScoreButton.interactable = false;
            roundTMP.text = $"ROUND {roundVariable.value}";
            UpdatePlayerInfoEntries(null);
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

        public override void ChangeRound(int value)
        {
            roundTMP.text = $"ROUND {value}";

            rollDiceButton.gameObject.SetActive(localPlayerManager.isMasterClient);
            localPlayerManager.hasRiskedScore = false;
        }

        private void OnChangeBankable(bool value)
        {
            bankScoreButton.interactable = value;
            riskScoreButton.interactable = value;
        }

        private void RiskScore()
        {
            bankScoreButton.interactable = false;
            riskScoreButton.interactable = false;
            localPlayerManager.hasRiskedScore = true;
        }

        public override void BankScore()
        {
            base.BankScore();
            riskScoreButton.interactable = false;
            bankScoreButton.interactable = false;
        }

        public override void RollDice()
        {
            base.RollDice();
            bankScoreButton.interactable = false;
            riskScoreButton.interactable = false;
        }

        public override void OnDiceRollComplete(List<Dice> dice)
        {
            var diceValues = dice.Select(d => d.currentValue).ToList();

            if(diceValues.Sum() == 7)
            {
                EndGame();
                return;
            }

            if(diceValues.All(d => d == diceValues[0]))
            {
                ShowRollMessage("DOUBLES!");
                diceValues = diceValues.Select(d => d * 2).ToList();
            }

            gameMode.UpdateGameScore(diceValues.Sum());
            bankScoreButton.interactable = true;
            riskScoreButton.interactable = true;
        }

        public override void OnMasterChanged()
        {
            if (gameMode.HasGameEnded)
            {
                return;
            }

            if (!gameMode.gameManager.IsRolling && localPlayerManager.isMasterClient)
            {
                rollDiceButton.gameObject.SetActive(true);
            }
        }

        private void EndGame()
        {
            gameMode.OnGameEnded();
            ShowRollMessage("\'7\' ends the game!");
            var winner = gameMode.players.value.OrderByDescending(p => p.totalScore).FirstOrDefault();
            winnerNameTMP.text = winner.playerName;
            winnerScoreTMP.text = winner.totalScore.ToString();
            gameResultPanel.SetActive(true);
            bankScoreButton.gameObject.SetActive(false);
            riskScoreButton.gameObject.SetActive(false);
        }

        public override void OnUpdateScoresUI()
        {
            localPlayerScoreTMP.text = localPlayerManager.totalScore.ToString();

            foreach(var entry in otherPlayersEntries)
            {
                var player = gameMode.players.value.FirstOrDefault(p => p.playerRef == entry.Key);

                if (player)
                {
                    entry.Value.UpdateEntry(player.playerName, player.totalScore);
                }
            }
        }
    }
}
