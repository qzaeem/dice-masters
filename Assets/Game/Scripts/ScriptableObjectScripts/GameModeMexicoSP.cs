using UnityEngine;
using Newtonsoft.Json;
using DiceGame.Game;
using System.Linq;
using DiceGame.UI;
using Fusion;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DiceGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Game Mode - Mexico Single Player", menuName = "Game Mode/Mexico Single Player")]
    public class GameModeMexicoSP : GameModeBase
    {
        [SerializeField] private BoolVariable canBankScoreVariable;
        [SerializeField] private PlayerRefVariable changeTurnVariable;
        [SerializeField] private ActionSO playerFinishedRollAction;
        [SerializeField] private StringListVariable playerNames;
        [SerializeField] private uint maxLives;
        private Dictionary<int, PlayerGameDataMexico> _playersData = new Dictionary<int, PlayerGameDataMexico>();
        private SP_MexicoCanvas mexicoMenu;
        private bool hasGameStarted;
        private int _activePlayerID;

        public Dictionary<int, PlayerGameDataMexico> PlayersData { get { return _playersData; } }
        public uint MaxLives { get { return maxLives; } }

        //New set max lives 
        public uint SetMaxLives { set => maxLives = value; }

        public override void Initialize()
        {
            base.Initialize();
            mexicoMenu = gameMenu as SP_MexicoCanvas;
            hasGameStarted = false;
            SpawnPlayers();
            _activePlayerID = 0;
        }

        private void SpawnPlayers()
        {
            _playersData.Clear();

            for (int i = 0; i < playerNames.value.Count(); i++)
            {
                var playerData = new PlayerGameDataMexico()
                {
                    id = i,
                    totalScore = 0,
                    hasBankedScore = false,
                    resurrect = false,
                    hasTied = false,
                    playerName = playerNames.value[i],
                    lives = maxLives,
                    entryUI = mexicoMenu.SpawnPlayer(i, playerNames.value[i])
                };
                _playersData.Add(i, playerData);
            }
        }

        private void OnPlayerFinishedRoll()
        {
            if(_playersData.Values.Any(p => p.hasTied))
            {
                ChangeTurn();
                return;
            }

            if (!_playersData.Values.All(p => p.hasBankedScore))
            {
                if (_activePlayerID < _playersData.Count - 1)
                {
                    _activePlayerID++;
                    mexicoMenu.OnPlayerTurnChange(_activePlayerID);
                }

                return;
            }

            var activePlayers = _playersData.Values.Where(p => p.lives > 0);
            var minScore = activePlayers.Min(p => p.totalScore);
            var maxScore = activePlayers.Max(p => p.totalScore);
            var lowestScorePlayers = activePlayers.Where(p => p.totalScore == minScore).ToList();
            var winners = activePlayers.Where(p => p.totalScore == maxScore).ToList();

            bool isTie = lowestScorePlayers.Count > 1;
            if (isTie)
            {
                lowestScorePlayers.ForEach(l =>
                {
                    l.hasTied = true;
                    l.hasBankedScore = false;
                });
                mexicoMenu.SetDiceRollButton(true, "Roll Dice");
                gameMenu.ShowRollMessage("TIE!");
                ChangeTurn();
                return;
            }
            else
            {
                lowestScorePlayers[0].lives -= 1;
            }

            IncrementRound();
        }

        public void AnnounceWinner(int winnerId)
        {
            OnGameEnded();
            mexicoMenu.AnnounceWinner(_playersData[winnerId]);
            mexicoMenu.OnUpdateScoresUI();
        }

        public override void RollDice()
        {
            diceRollManager.Execute();
        }

        public override void OnDiceRollComplete()
        {
            base.OnDiceRollComplete();

            var scores = diceRollManager.dice.Select(d => d.CurrentValue).ToList();
            int score = scores.Contains(1) && scores.Contains(2) ? 21 : scores.Sum();
            bool resurrect = false;
            var activePlayer = _playersData[_activePlayerID];

            if (activePlayer.lives == 0 && score == 21)
            {
                gameMenu.ShowRollMessage("Resurrection Roll!");
                resurrect = true;
            }

            gameScore.Set(score);
            activePlayer.hasBankedScore = true;
            activePlayer.hasTied = false;
            activePlayer.resurrect = resurrect;
            activePlayer.totalScore = score;

            mexicoMenu.OnUpdateScoresUI();
            OnPlayerFinishedRoll();
        }

        public override void IncrementRound()
        {
            var winners = _playersData.Values.Where(p => p.lives > 0).ToList();
            //winners.ForEach(w => Debug.Log($"Winner: {w.playerName}, Lives: {w.lives}"));
            if (winners.Count == 1 && !_playersData.Values.Any(p => p.resurrect))
            {
                AnnounceWinner(winners[0].id);
                return;
            }

            base.IncrementRound();
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

        }

        public override async void OnRoundChanged(int round)
        {
            await Task.Delay(hasGameStarted ? 5000 : 0);

            hasGameStarted = true;
            base.OnRoundChanged(round);
            gameScore.Set(0);

            foreach(var playerData in _playersData.Values)
            {
                if(playerData.lives == 0 && playerData.resurrect)
                {
                    playerData.lives = 1;
                }

                playerData.resurrect = false;
                playerData.hasBankedScore = false;
                playerData.hasTied = false;
                playerData.totalScore = 0;
            }

            mexicoMenu.OnUpdateScoresUI();
            ChangeTurn();
        }

        private void ChangeTurn()
        {
            var tiedPlayers = _playersData.Values.Where(p => p.hasTied).ToList();
            var nextPlayer = tiedPlayers.Count > 0 ? tiedPlayers.FirstOrDefault(p => p.lives > 0 && !p.hasBankedScore) : _playersData.Values.First();
            _activePlayerID = nextPlayer.id;
            mexicoMenu.OnPlayerTurnChange(_activePlayerID);
        }

        public override void OnPlayerRemoved(PlayerRef playerRef)
        {

        }
    }
}

public class PlayerGameDataMexico: PlayerGameData
{
    public uint lives;
    public bool resurrect;
    public bool hasTied;
}