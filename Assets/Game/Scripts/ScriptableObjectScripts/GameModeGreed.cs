using UnityEngine;
using Newtonsoft.Json;
using DiceGame.Game;
using System.Linq;
using DiceGame.UI;
using Fusion;

namespace DiceGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Greed", menuName = "Game Mode/Greed")]
    public class GameModeGreed : GameModeBase
    {
        public enum WinningScore { ShortGame = 5000, NormalGame = 10000, LongGame = 20000, SuperLongGame = 40000 }

        [SerializeField] private WinningScore winningScore;
        [SerializeField] private BoolVariable canBankScoreVariable;
        [SerializeField] private PlayerRefVariable changeTurnVariable;

        private GreedCombinationManager combinationManager = new GreedCombinationManager();
        private MP_GreedCanvas greedMenu;

        public override void Initialize()
        {
            base.Initialize();
            changeTurnVariable.onValueChange += OnPlayerTurnChange;
            combinationManager = new GreedCombinationManager();
            greedMenu = gameMenu as MP_GreedCanvas;
            //gameManager.ChangePlayerTurn(PlayerManager.LocalPlayer.playerRef);
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

        public override void BankScore()
        {
            PlayerManager.LocalPlayer.totalScore += gameScore.value;
            PlayerManager.LocalPlayer.hasBankedScore = true;
            greedMenu.SetDiceRollButton(false, "DiceRoll");
            gameScore.Set(0);
        }

        public void Farkle()
        {
            PlayerManager.LocalPlayer.totalScore += 0;
            PlayerManager.LocalPlayer.hasBankedScore = true;
            greedMenu.SetDiceRollButton(false, "DiceRoll");
            gameScore.Set(0);
        }

        public override void OnDiceRollComplete()
        {
            base.OnDiceRollComplete();
            canBankScoreVariable.Set(true);

            var dice = diceRollManager.dice.Where(d => d.IsRollable).ToList();
            var combinations = combinationManager.GetAllCombinations(dice);
            if(combinations.Count > 0)
            {
                greedMenu.EnableDiceScorePanel(true);
            }
            else
            {
                gameMenu.ShowRollMessage("BUST!");
                Farkle();
            }
        }

        public override void OnPlayerBankedScore()
        {
            if (!players.value.All(p => p.hasBankedScore))
                return;

            bool gameScoreAchieved = players.value.Any(p => p.totalScore >= (int)winningScore);

            if (gameScoreAchieved)
            {
                EndGame();
                return;
            }

            IncrementRound();

            //var activePlayer = players.value.FirstOrDefault(p => p.playerRef == gameManager.ActivePlayerTurn);

            //if (players.value.All(p => p.hasBankedScore) || activePlayer.hasBankedScore)
            //    ChangePlayerTurn();
        }

        public override void IncrementRound()
        {
            base.IncrementRound();
            ResetGameScore();
        }

        public override string GetSettingsJson()
        {
            var greedData = new GreedData()
            {
                isMultiplayer = isMultiplayer,
                showScoresOnEnd = showScoresOnEnd,
                hasGameEnded = HasGameEnded,
                winningScore = (int)winningScore
            };

            var jsonString = JsonConvert.SerializeObject(greedData);
            return jsonString;
        }

        public override void SetSettingsFromJson(string jsonString)
        {
            var greedData = JsonConvert.DeserializeObject<GreedData>(jsonString);
            isMultiplayer = greedData.isMultiplayer;
            showScoresOnEnd = greedData.showScoresOnEnd;
            HasGameEnded = greedData.hasGameEnded;
            winningScore = (WinningScore)greedData.winningScore;
        }

        public override void EndGame()
        {
            gameMenu.EndGame();
        }

        public override void OnRoundChanged(int round)
        {
            //gameManager.ChangePlayerTurn(players.value[0].playerRef);
            base.OnRoundChanged(round);
            diceRollManager.UnlockAllDice();
        }

        public void OnPlayerTurnChange(PlayerRef playerRef)
        {
            if (playerRef == PlayerManager.LocalPlayer.playerRef)
                diceRollManager.UnlockAllDice();
        }

        public override void OnPlayerRemoved(PlayerRef playerRef)
        {
            
        }
    }
}

[System.Serializable]
public class GreedData : SettingsData
{
    public int winningScore;
}