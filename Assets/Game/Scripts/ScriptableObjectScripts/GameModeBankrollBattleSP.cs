using UnityEngine;
using Newtonsoft.Json;
using DiceGame.Game;
using System.Linq;
using DiceGame.UI;
using System.Collections.Generic;

namespace DiceGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Bankroll Battle Single Player", menuName = "Game Mode/Bankroll Battle Single Player")]
    public class GameModeBankrollBattleSP : GameModeBase
    {
        [SerializeField] private int maxRounds;
        [SerializeField] private StringListVariable playerNames;
        private SP_BankrollBattleCanvas bankrollBattleCanvas;
        private Dictionary<int, PlayerGameData> _playersData = new Dictionary<int, PlayerGameData>();
        private int _sevensRolled;
        private int _activePlayerID;

        public Dictionary<int, PlayerGameData> PlayersData { get { return _playersData; } }

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
                    entryUI = bankrollBattleCanvas.SpawnPlayer(i, playerNames.value[i])
                };
                _playersData.Add(i, playerData);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            bankrollBattleCanvas = gameMenu as SP_BankrollBattleCanvas;
            _sevensRolled = 0;
            SpawnPlayers();
            _activePlayerID = 0;
            bankrollBattleCanvas.OnPlayerTurnChange(0);
        }

        public void BankScore(int id)
        {
            _playersData[id].totalScore += gameScore.value;
            _playersData[id].hasBankedScore = true;
            bankrollBattleCanvas.OnUpdateScoresUI();
            OnPlayerBankedScore();
        }

        public override void OnDiceRollComplete()
        {
            gameManager.EnableBankingAbility();

            var dice = diceRollManager.dice;
            var diceValues = dice.Select(d => d.currentValue).ToList();
            var sum = diceValues.Sum();

            if (sum == 7)
            {
                if (rollVariable.value > 3)
                {
                    gameMenu.ShowRollMessage("Rolled \'7\'. Round Over!");
                    IncrementRound();
                    return;
                }

                sum = 70;
                gameMenu.ShowRollMessage("Rolled a \'7\'. 70 points!");
            }

            ChangePlayerTurn();

            if (diceValues.All(d => d == diceValues[0]))
            {
                gameMenu.ShowRollMessage("DOUBLES!");
                DoubleGameScore();
                return;
            }

            UpdateGameScore(GameScore + sum);
        }

        public override void OnPlayerBankedScore()
        {
            if (_playersData.Values.All(p => p.hasBankedScore) || _playersData[_activePlayerID].hasBankedScore)
                ChangePlayerTurn();
        }

        public override void ChangePlayerTurn()
        {
            var activePlayer = _playersData[_activePlayerID];

            bool everyoneBanked = _playersData.Values.All(p => p.hasBankedScore);
            if (everyoneBanked)
            {
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
            bankrollBattleCanvas.OnPlayerTurnChange(nextIndex);
        }

        public override void IncrementRound()
        {
            base.IncrementRound();
            ResetGameScore();
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
            return "";
        }

        public override void SetSettingsFromJson(string jsonString)
        {
            
        }

        public override void EndGame()
        {
            gameMenu.EndGame();
        }

        public override void OnRoundChanged(int round)
        {
            if (round > maxRounds)
            {
                EndGame();
                return;
            }

            foreach(var kvp in _playersData)
            {
                kvp.Value.hasBankedScore = false;
            }

            _activePlayerID = 0;
            bankrollBattleCanvas.OnPlayerTurnChange(0);
            gameMenu.OnRoundChanged(round);
        }
    }
}

public class PlayerGameData
{
    public int id;
    public int totalScore;
    public bool hasBankedScore;
    public string playerName;
    public PlayerInfoEntryUI entryUI;
}
