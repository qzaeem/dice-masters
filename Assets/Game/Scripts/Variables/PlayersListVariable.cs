using UnityEngine;
using Fusion;
using DiceGame.Game;

namespace DiceGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayersListVariable", menuName = "Variable/PlayersListVariable")]
    public class PlayersListVariable : ListVariable<PlayerManager>
    {
        [SerializeField] private ActionSO changeMasterAction;

        public override void Remove(PlayerManager thing)
        {
            bool masterLeft = thing.isMasterClient;
            base.Remove(thing);
            if (masterLeft) changeMasterAction.Execute();
        }
    }
}
