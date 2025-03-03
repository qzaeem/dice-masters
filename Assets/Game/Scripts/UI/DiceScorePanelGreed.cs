using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DiceGame.ScriptableObjects;
using TMPro;
using System;
using System.Linq;
using DiceGame.Game;
using Newtonsoft.Json;
using System.Collections;

namespace DiceGame.UI
{
    public class DiceScorePanelGreed : MonoBehaviour
    {
        [Header("Fields")]
        [SerializeField] private TMP_Text scoreTMP;
        [SerializeField] private Transform diceButtonsContainer;
        [SerializeField] private Button diceButtonPrefab;
        [SerializeField] private Button submitButton;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color holdColor;
        [SerializeField] private List<Sprite> dieFaces;

        [Header("Scriptable Objects")]
        [SerializeField] private DiceRollManager diceRollManager;

        private GreedCombinationManager combinationManager;
        private Dictionary<int, Button> diceButtons = new Dictionary<int, Button>(); // int is the ID of the die.
        private List<int> heldDiceButtons = new List<int>();
        private int combinedScore;

        private Action<int> onSubmitScore;

        public void Initialize(int numberOfDice, Action<int> submitScoreAction)
        {
            onSubmitScore = submitScoreAction;
            combinationManager = new GreedCombinationManager();

            for(int i = 0; i < numberOfDice; i++)
            {
                var dieButton = Instantiate(diceButtonPrefab, diceButtonsContainer);
                int id = i + 1;
                dieButton.onClick.AddListener(() =>
                {
                    OnTappedDie(id);
                });
                diceButtons.Add(id, dieButton);
            }
        }

        private void OnEnable()
        {
            submitButton.onClick.AddListener(OnSubmit);
            combinedScore = 0;
            submitButton.interactable = combinedScore > 0;
            scoreTMP.text = combinedScore.ToString();

            foreach (var dieButtonKVP in diceButtons)
            {
                var die = diceRollManager.dice.FirstOrDefault(d => d.dieID == dieButtonKVP.Key);

                if (die != null)
                {
                    dieButtonKVP.Value.interactable = die.IsRollable;
                    dieButtonKVP.Value.image.color = normalColor;

                    if(die.CurrentValue > 0)
                    {
                        dieButtonKVP.Value.image.sprite = dieFaces[die.CurrentValue - 1];
                    }
                }
            }

            heldDiceButtons.Clear();
            StartCoroutine(RefreshUI());
        }

        private void OnDisable()
        {
            submitButton.onClick.RemoveListener(OnSubmit);
            heldDiceButtons.Clear();
        }

        private IEnumerator RefreshUI()
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.GetChild(0));
        }

        private void OnTappedDie(int dieId)
        {
            if (heldDiceButtons.Contains(dieId))
            {
                heldDiceButtons.Remove(dieId);
            }
            else
            {
                heldDiceButtons.Add(dieId);
            }

            var dice = diceRollManager.dice.Where(d => d.IsRollable).ToList();
            var allCombinations = combinationManager.GetAllCombinations(dice);
            if (allCombinations.Count == 0 || !allCombinations.Any(c => c.diceIds.Contains(dieId)))
            {
                if (heldDiceButtons.Contains(dieId))
                {
                    heldDiceButtons.Remove(dieId);
                }
            }

            var heldDice = diceRollManager.dice.Where(d => heldDiceButtons.Contains(d.dieID)).ToList();
            var combinations = heldDice != null ? combinationManager.GetAllCombinations(heldDice) : new List<Combination>();
            combinedScore = combinationManager.FilterCombinations(combinations).Sum(c => c.totalScore);
            scoreTMP.text = combinedScore.ToString();

            submitButton.interactable = combinedScore > 0;

            foreach (var kvp in diceButtons)
            {
                if (kvp.Value.interactable)
                    kvp.Value.image.color = heldDiceButtons.Contains(kvp.Key) ? holdColor : normalColor;
            }
        }

        private void OnSubmit()
        {
            var diceInfos = new List<DieInfoForRoll>();
            diceRollManager.dice.ForEach(d =>
            {
                var dieInfo = new DieInfoForRoll
                {
                    dieValue = d.CurrentValue,
                    isActive = !heldDiceButtons.Contains(d.dieID) && d.IsRollable
                };
                diceInfos.Add(dieInfo);
            });
            PlayerRollRecord playerRollRecord = new PlayerRollRecord
            {
                diceInfo = diceInfos
            };

            string jsonString = JsonConvert.SerializeObject(playerRollRecord);
            PlayerManager.LocalPlayer.RecordPlayerRollRpc(jsonString);

            diceRollManager.LockDice(heldDiceButtons, true);
            onSubmitScore?.Invoke(combinedScore);
        }

        private void OnDestroy()
        {
            foreach (var dieButtonKVP in diceButtons)
            {
                dieButtonKVP.Value.onClick.RemoveAllListeners();
            }
            diceButtons.Clear();
        }
    }

    public class PlayerRollRecord
    {
        public List<DieInfoForRoll> diceInfo;
    }

    public class DieInfoForRoll
    {
        public bool isActive { get; set; }
        public int dieValue { get; set; }
    }
}
