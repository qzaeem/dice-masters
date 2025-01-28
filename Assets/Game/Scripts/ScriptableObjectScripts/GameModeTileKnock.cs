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
    [CreateAssetMenu(fileName = "Knock Em Down", menuName = "Game Mode/KnockEmDown")]
    public class GameModeTileKnock : GameModeBase
    {
        [SerializeField] private BoolVariable canBankScoreVariable;
        [SerializeField] private PlayerRefVariable changeTurnVariable;
        [SerializeField] public uint maxRounds, totalTiles;
        private MP_TileKnockCanvas tileKEDMenu;
        private Dictionary<uint, uint> tilesDictionary = new Dictionary<uint, uint>();
        private string tilesJsonString;
        private int currentGameScore, scoreThisTurn;

        public override void Initialize()
        {
            base.Initialize();
            tileKEDMenu = gameMenu as MP_TileKnockCanvas;
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

            for(int i = 1; i <= maxStraights; i++)
            {
                tiles.Add(i);
            }

            int remainingTiles = (int)totalTiles - maxStraights;
            for(int i = 0; i < remainingTiles; i++)
            {
                int randomVal = Random.Range(1, 13);
                tiles.Add(randomVal > 9 && tiles.Contains(randomVal) ? Random.Range(1, 10) : randomVal);
            }

            if(totalTiles > 12) Shuffle(tiles);

            for(int i = 0; i < tiles.Count; i++)
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

        public override void OnPlayerBankedScore()
        {
            if (!players.value.All(p => p.roundComplete))
                return;

            if (roundVariable.value == maxRounds)
            {
                EndGame();
                return;
            }

            IncrementRound();
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

            currentGameScore = diceRollManager.dice.Sum(d => d.CurrentValue);
            scoreThisTurn = currentGameScore;
            tileKEDMenu.SetCurrentScore(currentGameScore);

            bool canPlayFurther = (tilesDictionary.Count == 1
                && tilesDictionary.ElementAt(0).Value == 1
                && diceRollManager.dice.All(d => d.CurrentValue == 1))
                || HasSubsetWithSum(tilesDictionary.Values.ToList(), currentGameScore);

            if (!canPlayFurther)
            {
                await Task.Delay(3000);
                gameMenu.ShowRollMessage("Round Ended!");
                PlayerManager.LocalPlayer.UpdateRoundScoreRpc(gameScore.value);
                return;
            }

            tileKEDMenu.ActivateTiles(true);
        }

        public override string GetSettingsJson()
        {
            var mexicoData = new TileKnockData()
            {
                isMultiplayer = isMultiplayer,
                showScoresOnEnd = showScoresOnEnd,
                hasGameEnded = HasGameEnded,
                maxRounds = maxRounds,
                totalTiles = totalTiles,
                tilesJsonString = tilesJsonString
            };

            var jsonString = JsonConvert.SerializeObject(mexicoData);
            return jsonString;
        }

        public override void SetSettingsFromJson(string jsonString)
        {
            var tileKnockData = JsonConvert.DeserializeObject<TileKnockData>(jsonString);
            isMultiplayer = tileKnockData.isMultiplayer;
            showScoresOnEnd = tileKnockData.showScoresOnEnd;
            HasGameEnded = tileKnockData.hasGameEnded;
            maxRounds = tileKnockData.maxRounds;
            totalTiles = tileKnockData.totalTiles;
            tilesJsonString = tileKnockData.tilesJsonString;

            SpawnTiles(tilesJsonString);
        }

        public override void EndGame()
        {
            tileKEDMenu.EndGame();
        }

        public override void OnRoundChanged(int round)
        {
            base.OnRoundChanged(round);

            if (!PlayerManager.LocalPlayer.isMasterClient)
                return;

            var tilesList = new TilesList
            {
                tiles = GetNewTilesList()
            };
            tilesJsonString = JsonConvert.SerializeObject(tilesList);
            gameManager.SpawnTilesRpc(tilesJsonString);
        }

        public void SubmitTiles(List<uint> tileIds)
        {
            foreach(var id in tileIds)
            {
                tilesDictionary.Remove(id);
            }

            canBankScoreVariable.Set(false);
            tileKEDMenu.ActivateTiles(false);
            gameScore.Set(gameScore.value - scoreThisTurn);
            PlayerManager.LocalPlayer.totalScore = gameScore.value;

            if (tilesDictionary.Count == 0)
            {
                gameMenu.ShowRollMessage("Round Ended!");
                PlayerManager.LocalPlayer.UpdateRoundScoreRpc(gameScore.value);
                return;
            }

            tileKEDMenu.SetDiceRollButton(true);
        }

        public bool SelectTile(uint id)
        {
            if(currentGameScore >= tilesDictionary[id])
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
            currentGameScore += (int)tilesDictionary[id];
            tileKEDMenu.SetCurrentScore(currentGameScore);
            PlayerManager.LocalPlayer.totalScore = currentGameScore;
            canBankScoreVariable.Set(currentGameScore == 0);
        }

        public void SpawnTiles(string tilesJsonString)
        {
            this.tilesJsonString = tilesJsonString;
            TilesList tilesList = JsonConvert.DeserializeObject<TilesList>(tilesJsonString);
            tilesDictionary = tilesList.tiles;
            tileKEDMenu.SpawnTiles(tilesDictionary);

            int score = tilesDictionary.Values.Select(t => (int)t).Sum();
            gameScore.Set(score);
            PlayerManager.LocalPlayer.totalScore = gameScore.value;
            tileKEDMenu.SetCurrentScore(0);
            players.value.ForEach(p => p.roundComplete = false);
        }

        private class TilesList
        {
            public Dictionary<uint, uint> tiles = new Dictionary<uint, uint>();
        }
    }
}

[System.Serializable]
public class TileKnockData : SettingsData
{
    public uint maxRounds;
    public uint totalTiles;
    public string tilesJsonString;
}