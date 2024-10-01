using System.Collections.Generic;
using DiceGame.Game;
using UnityEngine;

namespace DiceGame.ScriptableObjects
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Actions/DiceRollManagerAction")]
    public class DiceRollManager : ActionSO
    {
        [SerializeField] private ActionSO onDieRollComplete;
        [SerializeField] private ActionSO onDiceRollComplete;
        public List<Dice> dice;

        public override void Initialize()
        {
            onDieRollComplete.executeAction += OnDieCompletedRoll;
            if(dice != null)
            {
                dice.Clear();
                return;
            }
            dice = new List<Dice>();
        }

        public override void OnDestroy()
        {
            if (dice != null)
                dice.Clear();
            onDieRollComplete.executeAction -= OnDieCompletedRoll;
        }

        public override void Execute()
        {
            base.Execute();
            foreach(var die in dice)
            {
                die.RequestRoll();
            }
        }

        public void AddDie(Dice die)
        {
            if (!dice.Contains(die))
            {
                dice.Add(die);
            }
        }

        public void RemoveDie(Dice die)
        {
            if (!dice.Contains(die))
            {
                return;
            }
            dice.Remove(die);
        }

        public void OnDieCompletedRoll()
        {
            bool rollComplete = true;
            foreach(var die in dice)
            {
                if(!die.IsRolling && die.IsRollable)
                {
                    continue;
                }
                rollComplete = false;
            }

            if (rollComplete)
            {
                onDiceRollComplete.Execute();
            }
        }
    }
}
