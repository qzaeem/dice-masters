using DiceGame.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.UI
{
    public class DiceSelectionPanel : MonoBehaviour
    {
        [SerializeField] private Button leftArrow, rightArrow, selectButton, unlockButton;
        [SerializeField] private TextMeshProUGUI alertText;
        [SerializeField] private DiceSkin[] diceSkins;
        [SerializeField] private DiceSkinVariable diceSkinVariable;
        private const string p_SelectedDice = "SelectedDice";

        private int index;

        private void OnEnable()
        {
            AddListeners();
            DeselectAll();
            Init();
        }
        private void OnDisable()
        {
            RemoveListeners();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }
        private void AddListeners()
        {
            leftArrow.onClick.AddListener(() => ChangeDice(-1));
            rightArrow.onClick.AddListener(() => ChangeDice(1));
            selectButton.onClick.AddListener(SelectDice);
            unlockButton.onClick.AddListener(UnlockDice);
        }
        private void RemoveListeners()
        {
            leftArrow.onClick.RemoveAllListeners();
            rightArrow.onClick.RemoveAllListeners();
            selectButton.onClick.RemoveAllListeners();
            unlockButton.onClick.RemoveAllListeners();
        }
        private void Init()
        {
            DisableAll();
            alertText.gameObject.SetActive(false);
            // start with selected dice skin
            index = PlayerPrefs.GetInt(p_SelectedDice, 0);
            diceSkins[index].isSelected = true;
            diceSkinVariable.value = diceSkins[index].Material;
            // Initialize the first dice
            UpdateDiceSelection();
        }
        private void ChangeDice(int direction)
        {
            DisableAll();
            alertText.gameObject.SetActive(false);
            diceSkins[index].gameObject.SetActive(false); // Deactivate current dice
            index = (int)Mathf.Repeat(index + direction, diceSkins.Length); // Circular index handling
            DeselectAll();
            UpdateDiceSelection();
        }

        private void UpdateDiceSelection()
        {
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
            int keys = KeysController.Instance.GetKeys();
            if (diceSkins[index].DiceCost > keys)
            {
                alertText.gameObject.SetActive(true);
                alertText.text = "Not Enough Keys To Unlock The Dice!";
                return;
            }
            Debug.Log("Dice Unlocked");
            diceSkins[index].isUnlocked = true;
            diceSkins[index].DiceLock.SetActive(false);
            diceSkins[index].CostText.gameObject.SetActive(false);
            unlockButton.gameObject.SetActive(false);
            selectButton.gameObject.SetActive(true);
            int dice = diceSkins[index].DiceNumber;
            diceSkins[index].SaveUnlockedDice();
            KeysController.Instance.SubtractKeys(diceSkins[index].DiceCost);
        }
        private void DisableAll()
        {
            foreach (var d in diceSkins)
            {
                d.gameObject.SetActive(false);
            }
        }
        private void DeselectAll()
        {
            // Deselect all dice first
            foreach (var d in diceSkins)
            {
                //d.DiceSkin.isSelected = false;
                d.isSelected = false;
                d.SelectedText.gameObject.SetActive(false);
            }

        }
    }
}