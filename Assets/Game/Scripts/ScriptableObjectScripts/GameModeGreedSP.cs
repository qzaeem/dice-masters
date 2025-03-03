using UnityEngine;
using Newtonsoft.Json;
using DiceGame.Game;
using System.Linq;
using DiceGame.UI;
using Fusion;
using System.Collections.Generic;

namespace DiceGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "GreedSP", menuName = "Game Mode/GreedSP")]
    public class GameModeGreedSP : GameModeBase
    {
        public enum WinningScore { ShortGame = 5000, NormalGame = 10000, LongGame = 20000, SuperLongGame = 40000 }
        //new getter and setter for winning score
        public WinningScore MaxWinningScore { get => winningScore; set => winningScore = value; }

        [SerializeField] private WinningScore winningScore;
        [SerializeField] private BoolVariable canBankScoreVariable;
        [SerializeField] private PlayerRefVariable changeTurnVariable;
        [SerializeField] private StringListVariable playerNames;

        private GreedCombinationManager combinationManager = new GreedCombinationManager();
        private Dictionary<int, PlayerGameData> _playersData = new Dictionary<int, PlayerGameData>();
        private SP_GreedCanvas greedMenu;
        private int _activePlayerID;

        public Dictionary<int, PlayerGameData> PlayersData { get { return _playersData; } }
        public int ActivePlayerID { get { return _activePlayerID; } }

        private void SpawnPlayers()
        {
            for (int i = 0; i < playerNames.value.Count(); i++)
            {
                var playerData = new PlayerGameData()
                {
                    id = i,
                    totalScore = 0,
                    hasBankedScore = false,
                    playerName = playerNames.value[i],
                    entryUI = greedMenu.SpawnPlayer(i, playerNames.value[i])
                };
                _playersData.Add(i, playerData);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            combinationManager = new GreedCombinationManager();
            greedMenu = gameMenu as SP_GreedCanvas;
            SpawnPlayers();
            _activePlayerID = 0;
            greedMenu.OnPlayerTurnChange(_activePlayerID);
        }

        public void BankScore(int id)
        {
            _playersData[id].totalScore += gameScore.value;
            _playersData[id].hasBankedScore = true;
            OnPlayerBankedScore();
            ChangePlayerTurn();
            greedMenu.OnUpdateScoresUI();
            gameScore.Set(0);
        }

        public override void RollDice()
        {
            diceRollManager.Execute();
            canBankScoreVariable.Set(false);
        }

        public override void UpdateGameScore(int score)
        {
            gameScore.Set(score + gameScore.value);
        }

        public void Farkle()
        {
            int id = _activePlayerID;
            _playersData[id].totalScore += 0;
            _playersData[id].hasBankedScore = true;
            gameScore.Set(0);
            OnPlayerBankedScore();
        }

        public override void OnDiceRollComplete()
        {
            base.OnDiceRollComplete();
            canBankScoreVariable.Set(true);

            var dice = diceRollManager.dice.Where(d => d.IsRollable).ToList();
            var combinations = combinationManager.GetAllCombinations(dice);
            if (combinations.Count > 0)
            {
                greedMenu.EnableDiceScorePanel(true);
            }
            else
            {
                gameMenu.ShowRollMessage("BUST!");
                Farkle();
                ChangePlayerTurn();
            }
        }

        public override void ChangePlayerTurn()
        {
            var activePlayer = _playersData[_activePlayerID];

            bool everyoneBanked = _playersData.Values.All(p => p.hasBankedScore);
            if (everyoneBanked)
            {
                bool gameScoreAchieved = _playersData.Values.Any(p => p.totalScore >= (int)winningScore);

                if (gameScoreAchieved)
                {
                    EndGame();
                }
                else
                    IncrementRound();

                return;
            }

            var index = _activePlayerID;
            int nextIndex = index >= _playersData.Count() - 1 ? 0 : index + 1;
            for (int i = nextIndex; i < _playersData.Count(); i++)
            {
                if (!_playersData[i].hasBankedScore)
                {
                    nextIndex = i;
                    break;
                }
                if (i >= _playersData.Count() - 1)
                    i = -1;
            }

            _activePlayerID = nextIndex;
            diceRollManager.UnlockAllDice();
            greedMenu.OnPlayerTurnChange(nextIndex);
        }

        public override void OnPlayerBankedScore()
        {
            
        }

        public override void IncrementRound()
        {
            base.IncrementRound();
            ResetGameScore();
        }

        public override void EndGame()
        {
            gameMenu.EndGame();
        }

        public void SetSettings(bool showScoresAtEnd)
        {
            showScoresOnEnd = showScoresAtEnd;
        }

        public override void OnRoundChanged(int round)
        {
            foreach (var kvp in _playersData)
            {
                kvp.Value.hasBankedScore = false;
            }

            _activePlayerID = 0;
            greedMenu.OnPlayerTurnChange(0);
            gameMenu.OnRoundChanged(round);
            diceRollManager.UnlockAllDice();
        }

        public override void OnPlayerRemoved(PlayerRef playerRef)
        {

        }

        public override string GetSettingsJson()
        {
            return "";
        }

        public override void SetSettingsFromJson(string jsonString)
        {
            
        }
    }
}