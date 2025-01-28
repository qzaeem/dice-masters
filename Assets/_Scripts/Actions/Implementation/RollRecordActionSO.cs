using UnityEngine;

namespace DiceGame.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Actions/GreedRollRecordActionSO")]
    public class RollRecordActionSO : ActionSO
    {
        public Fusion.PlayerRef playerRef;
        public string rollJson;
    }
}
