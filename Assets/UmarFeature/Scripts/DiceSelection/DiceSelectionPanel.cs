using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Dice
{
    public DiceSkin DiceSkin;
    //public GameObject ParentDiceObj;
    public TextMeshProUGUI SelectedText;
    public GameObject DiceLock;
}
namespace DiceGame.UI
{
    public class DiceSelectionPanel : MonoBehaviour
    {
        [SerializeField] private Button leftArrow, rightArrow, selectButton;
        [SerializeField] private Dice[] dice;

        private int index;

        private void Awake()
        {
            leftArrow.onClick.AddListener(() => ChangeDice(-1));
            rightArrow.onClick.AddListener(() => ChangeDice(1));
            selectButton.onClick.AddListener(SelectDice);

            // Initialize the first dice
            UpdateDiceSelection();
        }

        private void OnDestroy()
        {
            leftArrow.onClick.RemoveAllListeners();
            rightArrow.onClick.RemoveAllListeners();
            selectButton.onClick.RemoveAllListeners();
        }

        private void ChangeDice(int direction)
        {
            //dice[index].ParentDiceObj.SetActive(false); // Deactivate current dice
            dice[index].DiceSkin.gameObject.SetActive(false); // Deactivate current dice

            index = (int)Mathf.Repeat(index + direction, dice.Length); // Circular index handling

            UpdateDiceSelection();
        }


        private void UpdateDiceSelection()
        {
            // Deselect all dice first
            foreach (var d in dice)
            {
                d.DiceSkin.isSelected = false;
                d.SelectedText.gameObject.SetActive(false);
            }

            // Activate current dice and update UI
            Dice currentDice = dice[index];
            currentDice.DiceSkin.gameObject.SetActive(true);
            currentDice.SelectedText.gameObject.SetActive(currentDice.DiceSkin.isSelected);
            selectButton.interactable = currentDice.DiceSkin.isUnlocked;
            currentDice.DiceLock.SetActive(!currentDice.DiceSkin.isUnlocked);
        }

        private void SelectDice()
        {
            Debug.Log("Select BTn");
            dice[index].DiceSkin.isSelected = true;
            dice[index].SelectedText.gameObject.SetActive(true);
        }
    }
}