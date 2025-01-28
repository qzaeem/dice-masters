using UnityEngine;
using DiceGame.Game;
using System.Linq;

namespace DiceGame.Testing
{
    public class CombinationsTest : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            TestDie die1 = new TestDie { CurrentValue = 1, dieID = 0 };
            TestDie die2 = new TestDie { CurrentValue = 1, dieID = 1 };
            TestDie die3 = new TestDie { CurrentValue = 1, dieID = 2 };
            TestDie die4 = new TestDie { CurrentValue = 1, dieID = 3 };
            TestDie die5 = new TestDie { CurrentValue = 5, dieID = 4 };
            TestDie die6 = new TestDie { CurrentValue = 5, dieID = 5 };
            GreedCombinationManager greedCombination = new GreedCombinationManager();
            var combinations = greedCombination.GetAllCombinations(new System.Collections.Generic.List<IPlayableDice>() { die1, die2, die3, die4, die5, die6 });
            int score = greedCombination.FilterCombinations(combinations).Sum(c => c.totalScore);
            combinations.ForEach(c => Debug.Log($"Combination: {c.combinationName}"));
            Debug.Log($"Score: {score}");
        }
    }

    public class TestDie : IPlayableDice
    {
        public int dieID { get; set; }
        public bool IsRollable { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public bool IsRolling => throw new System.NotImplementedException();

        public int CurrentValue { get; set; }
        public Vector3 Position { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public void LockDie(bool isLocked)
        {

        }

        public void RequestRoll()
        {

        }
    }
}
