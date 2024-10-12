using UnityEngine;
using System.Linq;
using DiceGame.Game;
using System;
using Fusion;

namespace DiceGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayersListVariable", menuName = "Variable/PlayersListVariable")]
    public class PlayersListVariable : ListVariable<PlayerManager>
    {
        public Action changeMasterAction;
        public Action<PlayerRef> playerRemovedAction;

        public override void Add(PlayerManager thing)
        {
            base.Add(thing);
            value = value.OrderBy(p => p.playerRef.PlayerId).ToList();
        }

        public override void Remove(PlayerManager thing)
        {
            if (value == null || value.Count() == 0)
                return;

            playerRemovedAction?.Invoke(thing.playerRef);
            bool masterLeft = thing.isMasterClient;
            base.Remove(thing);
            if (masterLeft) changeMasterAction?.Invoke();
        }
    }
}
