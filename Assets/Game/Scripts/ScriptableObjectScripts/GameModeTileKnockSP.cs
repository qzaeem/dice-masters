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
    [CreateAssetMenu(fileName = "Knock Em Down - Single Player", menuName = "Game Mode/KnockEmDownSP")]
    public class GameModeTileKnockSP : GameModeBase
    {
        [SerializeField] private BoolVariable canBankScoreVariable;
        [SerializeField] private PlayerRefVariable changeTurnVariable;
        [SerializeField] public uint maxRounds, totalTiles;
        [SerializeField] private StringListVariable playerNames;
        //New getter and setter for total tiles and max rounds
        public uint TotalTiles { get => totalTiles; set => totalTiles = value; }
        public uint MaxRounds { get => maxRounds; set => maxRounds = value; }
        public Dictionary<int, PlayerGameDataKnockdown> PlayersData { get { return _playersData; } }

        private SP_TileKnockCanvas tileKEDMenu;
        private Dictionary<int, PlayerGameDataKnockdown> _playersData = new Dictionary<int, PlayerGameDataKnockdown>();
        private int currentGameScore, scoreThisTurn;
        private int _activePlayerID;

        private void SpawnPlayers()
        {
            _playersData.Clear();

            for (int i = 0; i < playerNames.value.Count(); i++)
            {
                var playerData = new PlayerGameDataKnockdown()
                {
                    id = i,
                    totalScore = 0,
                    hasBankedScore = false,
                    isRoundFinished = false,
                    playerName = playerNames.value[i],
                    entryUI = tileKEDMenu.SpawnPlayer(i, playerNames.value[i])
                };
                _playersData.Add(i, playerData);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            tileKEDMenu = gameMenu as SP_TileKnockCanvas;
            SpawnPlayers();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        private Dictionary<uint, uint> GetNewTilesList()
        {
            Dictionary<uint, uint> newTiles = new Dictionary<uint, uint>();
            int maxStraights = totalTiles >= 12 ? 12 : 9;
            List<int> tiles = new List<int>();

            for (int i = 1; i <= maxStraights; i++)
            {
                tiles.Add(i);
            }

            int remainingTiles = (int)totalTiles - maxStraights;
            for (int i = 0; i < remainingTiles; i++)
            {
                int randomVal = Random.Range(1, 13);
                tiles.Add(randomVal > 9 && tiles.Contains(randomVal) ? Random.Range(1, 10) : randomVal);
            }

            if (totalTiles > 12) Shuffle(tiles);

            for (int i = 0; i < tiles.Count; i++)
            {
                newTiles.Add((uint)(i + 1), (uint)tiles[i]);
            }

            return newTiles;
        }

        private void Shuffle(List<int> list)
        {
            System.Random random = new System.Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1); // Random index from 0 to i
                                               // Swap values
                int temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        private bool HasSubsetWithSum(List<uint> numbers, int target)
        {
            return CheckSubset(numbers, target, 0);
        }

        private bool CheckSubset(List<uint> numbers, int target, int index)
        {
            // Base cases
            if (target == 0) return true; // Exact sum found
            if (index >= numbers.Count || target < 0) return false; // Out of bounds or target can't be reached

            // Recursively include or exclude the current number
            return CheckSubset(numbers, target - (int)numbers[index], index + 1) || // Include current number
                   CheckSubset(numbers, target, index + 1);                    // Exclude current number
        }

        private void SetPlayerTurn(int playerId)
        {
            _activePlayerID = playerId;
            _playersData[_activePlayerID].hasBankedScore = false;
            SpawnTilesForPlayer();
            tileKEDMenu.OnPlayerTurnChange(playerId);
            tileKEDMenu.OnUpdateScoresUI(_playersData.Values.Where(p => p.isRoundFinished)?.Select(p => p.id)?.ToList() ?? new List<int>());
        }

        public override void ChangePlayerTurn()
        {
            var activePlayer = _playersData[_activePlayerID];

            bool roundEnded = _playersData.Values.All(p => p.isRoundFinished);

            if (roundEnded)
            {
                if (roundVariable.value < MaxRounds)
                    IncrementRound();
                else
                    EndGame();
                return;
            }

            var index = _activePlayerID;
            int nextIndex = index >= _playersData.Count() - 1 ? 0 : index + 1;
            for (int i = nextIndex; i < _playersData.Count(); i++)
            {
                if (!_playersData[i].isRoundFinished)
                {
                    nextIndex = i;
                    break;
                }
                if (i >= _playersData.Count() - 1)
                    i = -1;
            }

            _activePlayerID = nextIndex;
            SetPlayerTurn(nextIndex);
        }

        public override void RollDice()
        {
            diceRollManager.Execute();
            canBankScoreVariable.Set(false);
            tileKEDMenu.ActivateTiles(false);
        }

        public override async void OnDiceRollComplete()
        {
            base.OnDiceRollComplete();

            var tilesDictionary = _playersData[_activePlayerID].tilesDictionary;
            currentGameScore = diceRollManager.dice.Sum(d => d.CurrentValue);
            scoreThisTurn = currentGameScore;
            tileKEDMenu.SetCurrentScore(currentGameScore);

            bool canPlayFurther = (tilesDictionary.Count == 1
                && tilesDictionary.ElementAt(0).Value == 1
                && diceRollManager.dice.All(d => d.CurrentValue == 1))
                || HasSubsetWithSum(tilesDictionary.Values.ToList(), currentGameScore);

            if (!canPlayFurther)
            {
                gameMenu.ShowRollMessage("Round Ended!");
                _playersData[_activePlayerID].isRoundFinished = true;
                _playersData[_activePlayerID].roundScores.Add(_playersData[_activePlayerID].totalScore);
                tileKEDMenu.OnUpdateScoresUI(_playersData.Values.Where(p => p.isRoundFinished)?.Select(p => p.id)?.ToList() ?? new List<int>());
                await Task.Delay(3000);
                ChangePlayerTurn();
                return;
            }

            tileKEDMenu.ActivateTiles(true);
        }

        public override void EndGame()
        {
            tileKEDMenu.EndGame();
        }

        public override void OnRoundChanged(int round)
        {
            base.OnRoundChanged(round);

            var tilesList = GetNewTilesList();

            foreach(var kvp in _playersData)
            {
                kvp.Value.tilesDictionary.Clear();
                kvp.Value.tilesDictionary = new(tilesList);
                kvp.Value.isRoundFinished = false;
                kvp.Value.hasBankedScore = false;
                kvp.Value.totalScore = tilesList.Values.Select(t => (int)t).Sum();
            }

            SetPlayerTurn(0);
        }

        public async void SubmitTilesForPlayer(List<uint> tileIds)
        {
            var tilesDictionary = _playersData[_activePlayerID].tilesDictionary;

            foreach (var id in tileIds)
            {
                tilesDictionary[id] = 0;
            }

            canBankScoreVariable.Set(false);
            tileKEDMenu.ActivateTiles(false);
            _playersData[_activePlayerID].totalScore -= scoreThisTurn;

            if (tilesDictionary.Values.Sum(t => t) == 0)
            {
                gameMenu.ShowRollMessage("Round Ended!");
                _playersData[_activePlayerID].isRoundFinished = true;
                _playersData[_activePlayerID].roundScores.Add(_playersData[_activePlayerID].totalScore); 
            }

            _playersData[_activePlayerID].hasBankedScore = true;

            tileKEDMenu.OnUpdateScoresUI(_playersData.Values.Where(p => p.isRoundFinished)?.Select(p => p.id)?.ToList() ?? new List<int>());

            await Task.Delay(3000);

            ChangePlayerTurn();
        }

        public bool SelectTile(uint id)
        {
            var tilesDictionary = _playersData[_activePlayerID].tilesDictionary;
            if (currentGameScore >= tilesDictionary[id])
            {
                currentGameScore -= (int)tilesDictionary[id];
                tileKEDMenu.SetCurrentScore(currentGameScore);
                canBankScoreVariable.Set(currentGameScore == 0);
                return true;
            }
            return false;
        }

        public void DeselectTile(uint id)
        {
            var tilesDictionary = _playersData[_activePlayerID].tilesDictionary;
            currentGameScore += (int)tilesDictionary[id];
            tileKEDMenu.SetCurrentScore(currentGameScore);
            canBankScoreVariable.Set(currentGameScore == 0);
        }

        public void SpawnTilesForPlayer()
        {
            var tilesDictionary = _playersData[_activePlayerID].tilesDictionary;
            tileKEDMenu.SpawnTiles(tilesDictionary);

            int score = tilesDictionary.Values.Select(t => (int)t).Sum();
            gameScore.Set(score);
            tileKEDMenu.SetCurrentScore(0);
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

public class PlayerGameDataKnockdown : PlayerGameData
{
    public Dictionary<uint, uint> tilesDictionary = new Dictionary<uint, uint>();
    public List<int> roundScores = new();
    public bool isRoundFinished;
}