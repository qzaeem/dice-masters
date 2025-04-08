using TMPro;
using UnityEngine;

public class DiceSkin : MonoBehaviour
{
    public int DiceNumber;
    [Tooltip("Name of the dice skin. Must be unique to save unlock state.")]
    [SerializeField] private string DiceName;
    public int DiceCost;
    public bool isUnlocked;
    public bool isSelected;
    public Material Material;
    public TextMeshProUGUI SelectedText;
    public TextMeshProUGUI CostText;
    public GameObject DiceLock;

    private void OnEnable()
    {
        if (DiceNumber > 1) // first dice is always unlock
        {
            isUnlocked = (PlayerPrefs.GetString($"UnlockDice{DiceNumber}") == DiceName) ? true : false;
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
        PlayerPrefs.SetString($"UnlockDice{DiceNumber}", DiceName);
    }
}
