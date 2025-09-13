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
        [SerializeField] private PlayerRefVariable changeTurnVariable;

        [Header("Local Player")]
        [SerializeField] private TMP_Text localPlayerNameTMP;
        [SerializeField] private TMP_Text localPlayerScoreTMP;
        [SerializeField] private TMP_Text winnerNameTMP;
        [SerializeField] private TMP_Text winnerScoreTMP;
        [SerializeField] private TMP_Text roundTMP;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color highlightedColor;

        [Header("Other Players")]
        [SerializeField] private PlayerInfoEntryUI playerInfoEntryUIPrefab;
        [SerializeField] private Transform playerInfoEntryContainer;
        private GameModeBankrollBattle gameModeBankrollBattle;
        private Dictionary<PlayerRef, PlayerInfoEntryUI> otherPlayersEntries
            = new Dictionary<PlayerRef, PlayerInfoEntryUI>();

        private bool _isHighlighted;

        public override void OnEnable()
        {
            base.OnEnable();
            players.onListValueChange += UpdatePlayerInfoEntries;
            changeTurnVariable.onValueChange += OnPlayerTurnChange;
            canBankScoreVariable.onValueChange += OnChangeBankable;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            players.onListValueChange -= UpdatePlayerInfoEntries;
            changeTurnVariable.onValueChange -= OnPlayerTurnChange;
            canBankScoreVariable.onValueChange -= OnChangeBankable;
        }

        public override void Start()
        {
            base.Start();
            gameModeBankrollBattle = gameMode as GameModeBankrollBattle;
            localPlayerNameTMP.text = localPlayerManager.playerName;
            roundTMP.text = $"ROUND {roundVariable.value}";
            UpdatePlayerInfoEntries(null);
            OnPlayerTurnChange(gameMode.ActivePlayerTurn);
            if (gameMode.HasGameEnded)
            {
                EndGame();
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
            }

            OnUpdateScoresUI();
        }

        public override void OnRoundChanged(int value)
        {
            roundTMP.text = $"ROUND {value}";
            bankScoreButton.interactable = false;
        }

        private void OnChangeBankable(bool value)
        {
            bankScoreButton.interactable = value
                && !PlayerManager.LocalPlayer.hasBankedScore
                && rollVariable.value >= 3;
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

        public override void OnDiceRollComplete(List<IPlayableDice> dice)
        {
            if (gameMode.HasGameEnded)
            {
                return;
            }

            base.OnDiceRollComplete(dice);
            var diceValues = dice.Select(d => d.CurrentValue).ToList();
            var sum = diceValues.Sum();

            if (sum == 7)
            {
                //int sevensRolled = gameModeBankrollBattle.RolledSeven(); Don't need number of times 7 is rolled anymore

                if (rollVariable.value > 3)
                {
                    ShowRollMessage("Rolled \'7\'. Round Over!");
                    gameMode.IncrementRound();
                    return;
                }

                sum = 70;
                ShowRollMessage("Rolled a \'7\'. 70 points!");
            }

            gameMode.ChangePlayerTurn();

            if (diceValues.All(d => d == diceValues[0]) && rollVariable.value > 3)
            {
                ShowRollMessage("DOUBLES!");
                gameModeBankrollBattle.DoubleGameScore();
                return;
            }

            ShowSmallAreaMessage($"Rolled a \'{sum}\'");
            gameMode.UpdateGameScore(gameMode.GameScore + sum);
        }

        public void OnPlayerTurnChange(PlayerRef playerRef)
        {
            _isHighlighted = false;
            backgroundImage.color = normalColor;
            foreach (var kvp in otherPlayersEntries) kvp.Value.SetHighlight(false);

            if (playerRef == PlayerManager.LocalPlayer.playerRef)
            {
                rollDiceButton.gameObject.SetActive(true);
                backgroundImage.color = highlightedColor;
                _isHighlighted = true;
                return;
            }

            rollDiceButton.gameObject.SetActive(false);
            var entry = otherPlayersEntries.FirstOrDefault(kvp => kvp.Key == playerRef).Value;

            if (entry != null)
            {
                entry.SetHighlight(true);
            }
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

            foreach(var entry in otherPlayersEntries)
            {
                var player = gameMode.players.value.FirstOrDefault(p => p.playerRef == entry.Key);

                if (player)
                {
                    entry.Value.UpdateEntry(player.playerName, player.totalScore, !gameMode.showScoresOnEnd);
                }
            }
        }
    }
}
