using UnityEngine;

namespace DiceGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Bankroll Battle", menuName = "Game Mode/Bankroll Battle")]
    public class GameModeBankrollBattle : GameModeBase
    {
        public bool isMultiplayer = true;
        public bool showScoresOnEnd = false;

        public override void OnDiceRollComplete()
        {
            base.OnDiceRollComplete();
        }
    }
}
