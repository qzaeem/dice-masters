using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceGame.Game
{
    public class GreedCombinationManager
    {
        public List<Combination> GetAllCombinations(List<IPlayableDice> dice)
        {
            List<Combination> combinations = new List<Combination>();
            var ones = Ones(dice);
            var fives = Fives(dice);
            if(ones.totalScore > 0) combinations.Add(ones);
            if (fives.totalScore > 0) combinations.Add(fives);
            combinations.AddRange(ThreeOfAKind(dice));
            combinations.AddRange(ThreePlusOfAKind(dice));
            combinations.AddRange(RunsOfSix(dice));
            combinations.AddRange(ThreePairs(dice));

            return combinations;
        }

        private Combination Ones(List<IPlayableDice> dice)
        {
            Combination combination = new Combination();
            combination.combinationName = "ones";
            combination.diceIds = new List<int>();
            combination.isValid = true;
            combination.totalScore = 0;

            dice.ForEach(d =>
            {
                if (d.CurrentValue == 1)
                {
                    combination.totalScore += 100;
                    combination.diceIds.Add(d.dieID);
                }
            });

            return combination;
        }

        private Combination Fives(List<IPlayableDice> dice)
        {
            Combination combination = new Combination();
            combination.combinationName = "fives";
            combination.diceIds = new List<int>();
            combination.isValid = true;
            combination.totalScore = 0;

            dice.ForEach(d =>
            {
                if (d.CurrentValue == 5)
                {
                    combination.totalScore += 50;
                    combination.diceIds.Add(d.dieID);
                }
            });

            return combination;
        }

        private List<Combination> ThreeOfAKind(List<IPlayableDice> dice)
        {
            List<Combination> combinations = new List<Combination>();

            var threes = dice.Select(d => d.CurrentValue)
                .GroupBy(n => n)
                .Where(g => g.Count() == 3)
                .Select(g => g.Key)
                .OrderByDescending(n => n)
                .ToList();

            if (threes.Count > 0)
            {
                threes.ForEach(t =>
                {
                    Combination combination = new Combination()
                    {
                        combinationName = "threeofkind",
                        diceIds = new List<int>(),
                        isValid = true,
                        totalScore = 0
                    };

                    int num = t;
                    dice.ForEach(d =>
                    {
                        if (d.CurrentValue == num)
                        {
                            combination.diceIds.Add(d.dieID);
                        }
                    });
                    combination.totalScore = num == 1 ? 1000 : num * 100;
                    combinations.Add(combination);
                });
            }

            return combinations;
        }

        private List<Combination> ThreePlusOfAKind(List<IPlayableDice> dice)
        {
            List<Combination> combinations = new List<Combination>();

            var threeplus = dice.Select(d => d.CurrentValue)
                .GroupBy(n => n)
                .Where(g => g.Count() > 3)
                .Select(g => new
                {
                    Number = g.Key,
                    Count = g.Count(),
                    score = g.Key == 1 ? 1000 : g.Key * 100
                })
                .OrderByDescending(g => g.score)
                .ToList();

            if (threeplus.Count > 0)
            {
                threeplus.ForEach(tp =>
                {
                    Combination combination = new Combination()
                    {
                        combinationName = "threepluskind",
                        diceIds = new List<int>(),
                        isValid = true,
                        totalScore = 0
                    };

                    int num = threeplus[0].Number;
                    dice.ForEach(d =>
                    {
                        if (d.CurrentValue == num)
                        {
                            combination.diceIds.Add(d.dieID);
                        }
                    });
                    combination.totalScore = tp.score * (int)Mathf.Pow(2, tp.Count - 3);
                    combinations.Add(combination);
                });
            }

            return combinations;
        }

        private List<Combination> SixOfAKind(List<IPlayableDice> dice)
        {
            List<Combination> combinations = new List<Combination>();

            var sixes = dice.Select(d => d.CurrentValue)
                .GroupBy(n => n)
                .Where(g => g.Count() == 6)
                .Select(g => g.Key)
                .OrderByDescending(n => n)
                .ToList();

            if (sixes.Count > 0)
            {
                sixes.ForEach(s =>
                {
                    Combination combination = new Combination()
                    {
                        combinationName = "sixofkind",
                        diceIds = new List<int>(),
                        isValid = true,
                        totalScore = 0
                    };

                    int num = s;
                    dice.ForEach(d =>
                    {
                        if (d.CurrentValue == num)
                        {
                            combination.diceIds.Add(d.dieID);
                        }
                    });
                    combination.totalScore = num == 1 ? 10000 : num * 1000;
                    combinations.Add(combination);
                });
            }

            return combinations;
        }

        private List<Combination> RunsOfFive(List<IPlayableDice> dice)
        {
            List<Combination> combinations = new List<Combination>();

            // Check if we have all numbers from 1 to 5
            List<int> sequenceToMatch = new List<int> { 1, 2, 3, 4, 5 };
            HashSet<int> numberSet = new HashSet<int>(dice.Select(d => d.CurrentValue));

            while (sequenceToMatch.All(n => numberSet.Contains(n)))
            {
                Combination combination = new Combination()
                {
                    combinationName = "runof5",
                    diceIds = new List<int>(),
                    isValid = true,
                    totalScore = 0
                };

                // Add dice IDs for the current run of 1-5
                foreach (var number in sequenceToMatch)
                {
                    var matchingDice = dice.Where(d => d.CurrentValue == number).ToList();
                    foreach (var die in matchingDice)
                    {
                        combination.diceIds.Add(die.dieID);
                    }
                }

                // Calculate total score (you can customize this logic as needed)
                combination.totalScore = 1000; // Set a fixed score for runs of five, or customize it.

                combinations.Add(combination);

                // Remove the numbers from the set to prevent overlapping runs
                foreach (var number in sequenceToMatch)
                {
                    numberSet.Remove(number);
                }
            }

            return combinations;
        }

        private List<Combination> RunsOfSix(List<IPlayableDice> dice)
        {
            List<Combination> combinations = new List<Combination>();

            // Check if we have all numbers from 1 to 6
            List<int> sequenceToMatch = new List<int> { 1, 2, 3, 4, 5, 6 };
            HashSet<int> numberSet = new HashSet<int>(dice.Select(d => d.CurrentValue));

            while (sequenceToMatch.All(n => numberSet.Contains(n)))
            {
                Combination combination = new Combination()
                {
                    combinationName = "runof6",
                    diceIds = new List<int>(),
                    isValid = true,
                    totalScore = 0
                };

                // Add dice IDs for the current run of 1-6
                foreach (var number in sequenceToMatch)
                {
                    var matchingDice = dice.Where(d => d.CurrentValue == number).ToList();
                    foreach (var die in matchingDice)
                    {
                        combination.diceIds.Add(die.dieID);
                    }
                }

                // Calculate total score (you can customize this logic as needed)
                combination.totalScore = 1000; // Set a fixed score for runs of six, or customize it.

                combinations.Add(combination);

                // Remove the numbers from the set to prevent overlapping runs
                foreach (var number in sequenceToMatch)
                {
                    numberSet.Remove(number);
                }
            }

            return combinations;
        }

        private List<Combination> ThreePairs(List<IPlayableDice> dice)
        {
            List<Combination> combinations = new List<Combination>();

            // Group dice by their values and count occurrences
            var groupedDice = dice
                .GroupBy(d => d.CurrentValue)
                .Where(g => g.Count() >= 2) // Only consider values that appear at least twice
                .Select(g => new
                {
                    Number = g.Key,
                    Count = g.Count(),
                    Dice = g.ToList()
                })
                .ToList();

            // Now find combinations of three pairs
            var pairs = groupedDice
                .SelectMany(g => Enumerable.Range(0, g.Count / 2)
                                           .Select(_ => g.Dice.Take(2).ToList()))
                .ToList();

            // Find all unique combinations of three pairs
            for (int i = 0; i < pairs.Count; i++)
            {
                for (int j = i + 1; j < pairs.Count; j++)
                {
                    for (int k = j + 1; k < pairs.Count; k++)
                    {
                        var selectedPairs = new List<IPlayableDice>
                        {
                            pairs[i][0], pairs[i][1],
                            pairs[j][0], pairs[j][1],
                            pairs[k][0], pairs[k][1]
                        };

                        Combination combination = new Combination()
                        {
                            combinationName = "threepairs",
                            diceIds = selectedPairs.Select(d => d.dieID).ToList(),
                            isValid = true,
                            totalScore = 1000 // You can customize the scoring logic here
                        };

                        combinations.Add(combination);
                    }
                }
            }

            return combinations;
        }

        public List<Combination> FilterCombinations(List<Combination> combinations)
        {
            combinations = combinations.OrderByDescending(c => c.totalScore).ToList();
            List<int> lockedIds = new List<int>();
            List<Combination> combinationsToRemove = new List<Combination>();

            for (int i = 0; i < combinations.Count; i++)
            {
                if(combinations[i].diceIds.Count == 0)
                {
                    combinationsToRemove.Add(combinations[i]);
                    continue;
                }

                foreach (int id in combinations[i].diceIds)
                {
                    if (lockedIds.Contains(id))
                    {
                        combinationsToRemove.Add(combinations[i]);
                        break;
                    }
                }

                if (!combinationsToRemove.Contains(combinations[i]))
                {
                    lockedIds.AddRange(combinations[i].diceIds);
                }
            }

            foreach (var combination in combinationsToRemove)
            {
                combinations.Remove(combination);
            }

            return combinations;
        }
    }

    public class Combination
    {
        public string combinationName;
        public int totalScore;
        public List<int> diceIds;
        public bool isValid;
    }
}
