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
        [Networked, OnChangedRender(nameof(OnRiskedScore))] public bool hasRiskedScore { get; set; }
        [Networked] public string playerName { get; set; } // Default length is 16

        [Networked, OnChangedRender(nameof(OnTotalScoreChanged))]
        public int totalScore { get; set; }
        #endregion

        [SerializeField] private PlayersListVariable players;
        [SerializeField] private ActionSO changeMasterAction;
        [SerializeField] private ActionSO masterUpdatedAction;
        [SerializeField] private ActionSO updateScoresUI;
        [SerializeField] private ActionSO onRiskedScore;

        public static PlayerManager LocalPlayer { get; set; }

        public override void Spawned()
        {
            players.Add(this);
            changeMasterAction.executeAction += OnMasterLeft;

            if(Object.HasStateAuthority)
                totalScore = 0;

            if (!Runner.IsNetworkObjectOfMasterClient(Object))
                return;

            isMasterClient = true;
        }

        private void OnMasterLeft()
        {
            isMasterClient = Runner.IsNetworkObjectOfMasterClient(Object);
            masterUpdatedAction.Execute();
        }

        private void OnTotalScoreChanged()
        {
            updateScoresUI.Execute();
        }

        private void OnRiskedScore()
        {
            if (hasRiskedScore)
            {
                onRiskedScore.Execute();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            changeMasterAction.executeAction -= OnMasterLeft;
            players.Remove(this);
        }
    }
}
