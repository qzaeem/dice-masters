using UnityEngine;
using Fusion;
using DiceGame.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace DiceGame.Game
{
    public class GameManager : NetworkBehaviour
    {
        [System.Serializable] public class Grid { public int rows, colums; }
        #region Networked Properties
        private NetworkBool _isRolling { get; set; }
        #endregion

        [SerializeField] private DiceRollManager diceRollManager;
        [SerializeField] private GameModeVariable currentGameMode;
        [SerializeField] private ActionSO onDiceRollComplete;
        [SerializeField] private Dice diePrefab;
        [SerializeField] private float gridSpacing;
        [SerializeField] private List<Grid> grid;

        public override void Spawned()
        {
            onDiceRollComplete.executeAction += OnDiceRollComplete;

            if (!Runner.IsSharedModeMasterClient)
                return;

            _isRolling = false;

            int totalNumberOfDice = currentGameMode.value.GetNumberOfDice();
            Grid selectedGrid = grid.FirstOrDefault(g => g.rows * g.colums == totalNumberOfDice);
            int rows = selectedGrid != null ? selectedGrid.rows : ((totalNumberOfDice - 1) / 3) + 1;
            int columns = selectedGrid != null ? selectedGrid.colums : totalNumberOfDice / rows;
            SpawnDice(rows, columns);
        }

        public void RollDice()
        {
            if (!Runner.IsSharedModeMasterClient)
                return;

            diceRollManager.Execute();
        }

        public void OnDiceRollComplete()
        {
            if (!Runner.IsSharedModeMasterClient)
                return;

            _isRolling = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && !_isRolling)
            {
                _isRolling = true;
                RollDice();
            }
        }

        private void SpawnDice(int rows, int columns)
        {
            float startingRow = (rows - 1) * gridSpacing / 2;
            float startingColumn = (1 - columns) * gridSpacing / 2;
            int dieID = 0;
            for (int i = 0; i < rows; i++)
            {
                for(int j = 0; j < columns; j++)
                {
                    var die = Runner.Spawn(diePrefab);
                    float zPos = startingRow - i * gridSpacing;
                    float xPos = startingColumn + j * gridSpacing;
                    dieID++;
                    die.dieID = dieID;
                    die.transform.position = new Vector3(xPos, die.transform.position.y, zPos);
                }
            }
        }

        private void OnDestroy()
        {
            onDiceRollComplete.executeAction -= OnDiceRollComplete;
        }
    }
}

