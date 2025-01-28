using System.Collections.Generic;
using DiceGame.Game;
using UnityEngine;
using System.Linq;

namespace DiceGame.ScriptableObjects
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Actions/DiceRollManagerAction")]
    public class DiceRollManager : ActionSO
    {
        [SerializeField] private ActionSO onDieRollComplete;
        [SerializeField] private ActionSO onDiceRollComplete;
        public List<IPlayableDice> dice;

        public bool AllDiceLocked => dice.All(d => !d.IsRollable);

        public override void Initialize()
        {
            onDieRollComplete.executeAction += OnDieCompletedRoll;
            if(dice != null)
            {
                dice.Clear();
                return;
            }
            dice = new List<IPlayableDice>();
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

        public void AddDie(IPlayableDice die)
        {
            if (!dice.Contains(die))
            {
                dice.Add(die);
            }
        }

        public void RemoveDie(IPlayableDice die)
        {
            if (!dice.Contains(die))
            {
                return;
            }
            dice.Remove(die);
        }

        public void LockDice(List<int> diceIds, bool isLocked)
        {
            diceIds.ForEach(id =>
            {
                var die = dice.FirstOrDefault(d => d.dieID == id);

                if (die != null)
                    die.LockDie(isLocked);
            });
        }

        public void UnlockAllDice()
        {
            dice.ForEach(d =>
            {
                d.LockDie(false);
            });
        }

        public void OnDieCompletedRoll()
        {
            bool rollComplete = true;
            foreach(var die in dice)
            {
                if(!die.IsRollable || !die.IsRolling)
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
