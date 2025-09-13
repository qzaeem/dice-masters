using UnityEngine;
using Newtonsoft.Json;
using DiceGame.Game;
using System.Linq;

namespace DiceGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Bankroll Battle", menuName = "Game Mode/Bankroll Battle")]
    public class GameModeBankrollBattle : GameModeBase
    {
        [SerializeField] private int maxRounds;
        // new getter and setter for max rounds
        public int MaxRounds { get => maxRounds; set => maxRounds = value; }
        // new getter and setter for show score bool
        public bool ShowScoreOnEnd { get => showScoresOnEnd; set => showScoresOnEnd = value; }

        private int _sevensRolled;

        public override void Initialize()
        {
            base.Initialize();
            _sevensRolled = 0;

            gameManager.ChangePlayerTurn(PlayerManager.LocalPlayer.playerRef);
        }

        public override void BankScore()
        {
            gameManager.TryBankingDirect(gameScore.value);
        }

        public override void OnDiceRollComplete()
        {
            base.OnDiceRollComplete();
            gameManager.EnableBankingAbility();
        }

        public override void OnPlayerBankedScore()
        {
            if(players.value.All(p => p.hasBankedScore))
            {
                Debug.Log("Here 1");
                IncrementRound();
                return;
            }

            Debug.Log("Here 2");
            var activePlayer = players.value.FirstOrDefault(p => p.playerRef == gameManager.ActivePlayerTurn);
            if (activePlayer != null && activePlayer.hasBankedScore)
            {
                var index = players.value.IndexOf(activePlayer);
                int nextIndex = index >= players.value.Count() - 1 ? 0 : index + 1;
                for (int i = nextIndex; i < players.value.Count(); i++)
                {
                    if (!players.value[i].hasBankedScore)
                    {
                        nextIndex = i;
                        break;
                    }
                    if (i >= players.value.Count() - 1)
                        i = -1;
                }

                gameManager.ChangePlayerTurn(players.value[nextIndex].playerRef);
            }
        }

        public override void IncrementRound()
        {
            base.IncrementRound();
            ResetGameScore();

            //if (roundVariable.value > maxRounds || !PlayerManager.LocalPlayer.isMasterClient)
            //    return;

            //gameManager.ChangePlayerTurn(players.value[0].playerRef);
        }

        public int RolledSeven()
        {
            _sevensRolled++;
            return _sevensRolled;
        }

        public void DoubleGameScore()
        {
            base.UpdateGameScore(GameScore * 2);
        }

        public override string GetSettingsJson()
        {
            var bankrollBattleData = new BankrollBattleData()
            {
                isMultiplayer = isMultiplayer,
                showScoresOnEnd = showScoresOnEnd,
                hasGameEnded = HasGameEnded,
                sevensRolled = _sevensRolled,
                maxRounds = maxRounds
            };

            var jsonString = JsonConvert.SerializeObject(bankrollBattleData);
            return jsonString;
        }

        public override void SetSettingsFromJson(string jsonString)
        {
            var bankrollBattleData = JsonConvert.DeserializeObject<BankrollBattleData>(jsonString);
            isMultiplayer = bankrollBattleData.isMultiplayer;
            showScoresOnEnd = bankrollBattleData.showScoresOnEnd;
            HasGameEnded = bankrollBattleData.hasGameEnded;
            _sevensRolled = bankrollBattleData.sevensRolled;
            maxRounds = bankrollBattleData.maxRounds;
        }

        public override void EndGame()
        {
            gameMenu.EndGame();
        }

        public override void OnRoundChanged(int round)
        {
            if(round > maxRounds)
            {
                EndGame();
                return;
            }

            var activePlayer = players.value.FirstOrDefault(p => p.playerRef == gameManager.ActivePlayerTurn);
            var index = players.value.IndexOf(activePlayer == null ? players.value[0] : activePlayer);
            int nextIndex = index >= players.value.Count() - 1 ? 0 : index + 1;

            gameManager.ChangePlayerTurn(players.value[nextIndex].playerRef);

            base.OnRoundChanged(round);
        }
    }
}

[System.Serializable]
public class BankrollBattleData : SettingsData
{
    public int sevensRolled;
    public int maxRounds;
}
