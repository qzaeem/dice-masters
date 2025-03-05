using DiceGame.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.UI
{
    public class DiceSelectionPanel : MonoBehaviour
    {
        [SerializeField] private Button leftArrow, rightArrow, selectButton, unlockButton;
        [SerializeField] private DiceSkin[] diceSkins;
        [SerializeField] private DiceSkinVariable diceSkinVariable;
        private const string p_SelectedDice = "SelectedDice";
        private const string p_UnlockedDice = "UnlockedDice";
        private int index;

        private void OnEnable()
        {
            DisableAll();
            index = PlayerPrefs.GetInt(p_SelectedDice, 0);
            diceSkins[index].gameObject.SetActive(true);
            diceSkins[index].SelectedText.gameObject.SetActive(true);

            leftArrow.onClick.AddListener(() => ChangeDice(-1));
            rightArrow.onClick.AddListener(() => ChangeDice(1));
            selectButton.onClick.AddListener(SelectDice);
            unlockButton.onClick.AddListener(UnlockDice);

            // Initialize the first dice
            UpdateDiceSelection();
        }
        private void OnDisable()
        {
            leftArrow.onClick.RemoveAllListeners();
            rightArrow.onClick.RemoveAllListeners();
            selectButton.onClick.RemoveAllListeners();
            unlockButton.onClick.RemoveAllListeners();
        }

        private void OnDestroy()
        {
            leftArrow.onClick.RemoveAllListeners();
            rightArrow.onClick.RemoveAllListeners();
            selectButton.onClick.RemoveAllListeners();
            unlockButton.onClick.RemoveAllListeners();
        }

        private void ChangeDice(int direction)
        {
            DisableAll();
            diceSkins[index].gameObject.SetActive(false); // Deactivate current dice
            index = (int)Mathf.Repeat(index + direction, diceSkins.Length); // Circular index handling

            UpdateDiceSelection();
        }

        private void UpdateDiceSelection()
        {
            // Deselect all dice first
            foreach (var d in diceSkins)
            {
                //d.DiceSkin.isSelected = false;
                d.isSelected = false;
                d.SelectedText.gameObject.SetActive(false);
            }

            // Activate current dice and update UI
            DiceSkin currentDice = diceSkins[index];
            currentDice.gameObject.SetActive(true);
            currentDice.SelectedText.gameObject.SetActive(currentDice.isSelected);
            selectButton.gameObject.SetActive(currentDice.isUnlocked);
            unlockButton.gameObject.SetActive(!currentDice.isUnlocked);
            currentDice.DiceLock.SetActive(!currentDice.isUnlocked);
        }

        private void SelectDice()
        {
            Debug.Log("Dice Selected");
            diceSkins[index].isSelected = true;
            diceSkins[index].SelectedText.gameObject.SetActive(true);
            diceSkinVariable.value = diceSkins[index].Material;
            PlayerPrefs.SetInt(p_SelectedDice, index);
        }
        private void UnlockDice()
        {
            Debug.Log("Dice Unlocked");
            diceSkins[index].isUnlocked = true;
            diceSkins[index].DiceLock.SetActive(true);
            unlockButton.gameObject.SetActive(false);
            selectButton.gameObject.SetActive(true);
            int dice = diceSkins[index].DiceNumber;
            PlayerPrefs.SetInt(p_UnlockedDice, dice);
        }
        private void DisableAll()
        {
            foreach (var d in diceSkins)
            {
                d.gameObject.SetActive(false);
            }
        }
    }
}