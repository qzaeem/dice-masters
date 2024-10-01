using UnityEngine;
using Fusion;
using DiceGame.ScriptableObjects;

namespace DiceGame.Game
{
    public class PlayerManager : NetworkBehaviour
    {
        #region Networked Variables
        [Networked] public PlayerRef playerRef { get; set; }
        [Networked] public bool isMasterClient { get; set; }
        [Networked] public string playerName { get; set; } // Default length is 16

        [Networked, OnChangedRender(nameof(OnTotalScoreChanged))]
        public int totalScore { get; set; }
        #endregion

        [SerializeField] private PlayersListVariable players;
        [SerializeField] private ActionSO changeMasterAction, updateScoresUI;

        public override void Spawned()
        {
            players.Add(this);
            totalScore = 0;
            changeMasterAction.executeAction += OnMasterLeft;

            if (!Runner.IsNetworkObjectOfMasterClient(Object))
                return;

            isMasterClient = true;
        }

        private void OnMasterLeft()
        {
            isMasterClient = Runner.IsNetworkObjectOfMasterClient(Object);
        }

        private void OnTotalScoreChanged()
        {
            updateScoresUI.Execute();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            changeMasterAction.executeAction -= OnMasterLeft;
            players.Remove(this);
        }
    }
}
