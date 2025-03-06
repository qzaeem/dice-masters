using UnityEngine;
using DiceGame.ScriptableObjects;
using Fusion;
using System.Collections.Generic;
using System.Linq;

namespace DiceGame.Game
{
    public class SpawnManager : NetworkBehaviour
    {
        [System.Serializable] public class Grid { public int rows, colums; }
        #region Networked Properties
        [Networked, OnChangedRender(nameof(SpawnGameModeMenu))] public bool isGameInProgress { get; set; }
        #endregion

        [SerializeField] private GameManager gameManager;
        [SerializeField] private PlayerManager playerPrefab;
        [SerializeField] private DiceRollManager diceRollManager;
        [SerializeField] private GameModeVariable currentGameMode;
        [SerializeField] private PlayerInfoVariable playerInfo;
        [SerializeField] private PlayersListVariable players;
        [SerializeField] private StringListVariable playerNames;
        [SerializeField] private Dice diePrefab;
        [SerializeField] private DiceLocal dieLocalPrefab;
        [SerializeField] private float gridSpacing;
        [SerializeField] private List<Grid> grid;

        public override void Spawned()
        {
            players.value.Clear();
            diceRollManager.Initialize();
            var player = Runner.Spawn(playerPrefab);
            SetPlayerInfo(player);
            Runner.SetPlayerObject(Runner.LocalPlayer, player.Object);
            SpawnGameModeMenu();

            if (!Runner.IsMasterClient())
            {
                if (!currentGameMode.value.networkedDice)
                    StartNewGame();
                return;
            }

            StartNewGame();
        }

        private void StartNewGame()
        {
            int totalNumberOfDice = currentGameMode.value.GetNumberOfDice();
            Grid selectedGrid = grid.FirstOrDefault(g => g.rows * g.colums == totalNumberOfDice);
            int rows = selectedGrid != null ? selectedGrid.rows : ((totalNumberOfDice - 1) / 3) + 1;
            int columns = selectedGrid != null ? selectedGrid.colums : totalNumberOfDice / rows;
            SpawnDice(rows, columns);
            isGameInProgress = true;
        }

        private void SpawnGameModeMenu()
        {
            if (!isGameInProgress)
                return;

            currentGameMode.value.gameManager = gameManager;
            currentGameMode.value.SpawnGameModeMenu();
            currentGameMode.value.Initialize();
            gameManager.OnPlayersUpdated(null);
        }

        private void SpawnDice(int rows, int columns)
        {
            float startingRow = (rows - 1) * gridSpacing / 2;
            float startingColumn = (1 - columns) * gridSpacing / 2;
            int dieID = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    IPlayableDice die = currentGameMode.value.networkedDice
                        ? Runner.Spawn(diePrefab) :
                        Instantiate(dieLocalPrefab);
                    float zPos = startingRow - i * gridSpacing;
                    float xPos = startingColumn + j * gridSpacing;
                    dieID++;
                    die.dieID = dieID;
                    die.Position = new Vector3(xPos, die.Position.y, zPos);
                }
            }
        }

        private void SetPlayerInfo(PlayerManager player)
        {
            PlayerManager.LocalPlayer = player;
            player.playerRef = Runner.LocalPlayer;
            player.playerName = playerInfo.value.playerName;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            diceRollManager.OnDestroy();
            currentGameMode.value.OnDestroy();
            players.value.Clear();
        }
    }
}
