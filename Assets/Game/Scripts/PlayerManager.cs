using UnityEngine;
using Fusion;
using DiceGame.ScriptableObjects;

namespace DiceGame.Game
{
    public class PlayerManager : NetworkBehaviour
    {
        #region Networked Variables
        [Networked] public PlayerRef playerRef { get; set; }
        #endregion

        [SerializeField] private PlayersListVariable players;

        public override void Spawned()
        {
            players.Add(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            players.Remove(this);
        }
    }
}
