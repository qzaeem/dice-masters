using UnityEngine;
using DiceGame.ScriptableObjects;
using Fusion;

namespace DiceGame.Game
{
    public class SpawnManager : NetworkBehaviour
    {
        [SerializeField] private PlayerManager playerPrefab;
        [SerializeField] private GameManager gameManagerPrefab;
        [SerializeField] private DiceRollManager diceRollManager;

        public override void Spawned()
        {
            diceRollManager.Initialize();
            var player = Runner.Spawn(playerPrefab);
            player.playerRef = Runner.LocalPlayer;
            Runner.SetPlayerObject(Runner.LocalPlayer, player.Object);

            if(Runner.IsSharedModeMasterClient)
                Runner.Spawn(gameManagerPrefab);
        }

        private void OnDestroy()
        {
            diceRollManager.OnDestroy();
        }
    }
}
