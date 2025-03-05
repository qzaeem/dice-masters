using TMPro;
using UnityEngine;

public class DiceSkin : MonoBehaviour
{
    public int DiceNumber;
    public bool isUnlocked;
    public bool isSelected;
    public Material Material;
    public TextMeshProUGUI SelectedText;
    public GameObject DiceLock;

    private void OnEnable()
    {
        if(DiceNumber > 2)
        {
            isUnlocked = (PlayerPrefs.GetInt("UnlockedDice") == DiceNumber) ? true : false;
        }
    }
}
