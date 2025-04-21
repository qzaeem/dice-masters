using TMPro;
using UnityEngine;

public class DiceSkin : MonoBehaviour
{
    public enum DiceType
    {
        SimpleDice, NumberedDice, RedDice
    }
    [SerializeField] private DiceType diceType;
    public int DiceNumber;
    public int DiceCost;
    public bool isUnlocked;
    public bool isSelected;
    public Material Material;
    public TextMeshProUGUI SelectedText;
    public TextMeshProUGUI CostText;
    public GameObject DiceLock;

    private const string numberedDice = "NumberedDiceUnlocked";
    private const string redDice = "RedDiceUnlocked";

    private void OnEnable()
    {
        if (DiceNumber > 1) // first dice is always unlock
        {
            switch (diceType)
            {
                case DiceType.NumberedDice:
                    isUnlocked = (PlayerPrefs.GetString($"UnlockDice{DiceNumber}") == numberedDice) ? true : false;
                    break;
                case DiceType.RedDice:
                    isUnlocked = (PlayerPrefs.GetString($"UnlockDice{DiceNumber}") == redDice) ? true : false;
                    break;
            }
        }
        if (!isUnlocked)
        {
            CostText.text = $"Cost : {DiceCost.ToString()} Keys";
            CostText.gameObject.SetActive(true);
        }
        else
        {
            CostText.gameObject.SetActive(false);
        }
    }
    public void SaveUnlockedDice()
    {
        switch (diceType)
        {
            case DiceType.NumberedDice:
                PlayerPrefs.SetString($"UnlockDice{DiceNumber}", numberedDice);
                break;
            case DiceType.RedDice:
                PlayerPrefs.SetString($"UnlockDice{DiceNumber}", redDice);
                break;
        }
    }
}
