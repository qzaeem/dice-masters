using UnityEngine;
using Fusion;
using DiceGame.ScriptableObjects;
using System.Collections.Generic;

namespace DiceGame.Game
{
    public class PlayerManager : NetworkBehaviour
    {
        #region Networked Variables
        [Networked, OnChangedRender(nameof(OnPlayerBankedScore))] public bool hasBankedScore { get; set; }
        [Networked, OnChangedRender(nameof(OnTotalScoreChanged))]
        public int totalScore { get; set; }
        [Networked] public PlayerRef playerRef { get; set; }
        [Networked] public bool isMasterClient { get; set; }
        [Networked] public string playerName { get; set; } // Default length is 16
        #endregion

        [SerializeField] private GameModeVariable gameModeVariable;
        [SerializeField] private PlayersListVariable players;
        [SerializeField] private ActionSO masterUpdatedAction;
        [SerializeField] private ActionSO updateScoresUI;
        [SerializeField] private ActionSO playerBankedScoreAction;
        [SerializeField] private ActionSO playerFinishedRollAction;
        [SerializeField] private RollRecordActionSO greedRollRecordAction;

        public List<int> roundScores = new List<int>();
        public bool hasFinishedRoll { get; set; }
        public bool resurrect { get; set; }
        public bool roundComplete { get; set; }
        public uint lives { get; set; }

        public static PlayerManager LocalPlayer { get; set; }

        public override void Spawned()
        {
            players.Add(this);
            players.changeMasterAction += OnMasterLeft;

            if (gameModeVariable.value.mode == GameModeName.Mexico && gameModeVariable.value.isMultiplayer)
            {
                lives = (gameModeVariable.value as GameModeMexico).MaxLives;
            }

            if (Object.HasStateAuthority)
            {
                totalScore = 0;
                hasBankedScore = false;
                hasFinishedRoll = false;
                resurrect = false;
                roundComplete = false;
            }

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

        private void OnPlayerBankedScore()
        {
            if (!hasBankedScore)
                return;

            playerBankedScoreAction.Execute();
        }

        private void OnPlayerFinishedRoll()
        {
            playerFinishedRollAction.Execute();
        }

        private void OnUpdatedGreedRoll(string greedRoll)
        {
            greedRollRecordAction.playerRef = playerRef;
            greedRollRecordAction.rollJson = greedRoll;
            greedRollRecordAction.Execute();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            players.changeMasterAction -= OnMasterLeft;
            players.Remove(this);
        }


        #region RPCs

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RecordPlayerRollRpc(string greedRoll)
        {
            OnUpdatedGreedRoll(greedRoll);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void UpdatePlayerLifeRpc(uint newLives)
        {
            lives = newLives;
            OnTotalScoreChanged();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void ResetDiceRollRpc()
        {
            hasFinishedRoll = false;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void CompleteRollMexicoRpc(bool hasFinishedRoll, bool resurrect, int totalScore)
        {
            this.hasFinishedRoll = hasFinishedRoll;
            this.resurrect = resurrect;
            this.totalScore = totalScore;

            if(hasFinishedRoll)
            {
                OnPlayerFinishedRoll();
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void UpdateRoundScoreRpc(int score)
        {
            roundComplete = true;
            roundScores.Add(score);
            playerBankedScoreAction.Execute();
        }
        #endregion
    }
}
