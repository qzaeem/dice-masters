using DiceGame.ScriptableObjects;
using DiceGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomController : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private MainMenuCanvas mainMenuCanvas;
    [SerializeField] private TMP_Dropdown playerCountDropDown;
    int playerCount;
    private void OnEnable()
    {
        createButton.onClick.AddListener(CreateRoom);
        playerCountDropDown.onValueChanged.AddListener(OnSelectPlayerCount);
    }
    private void OnDisable()
    {
        createButton.onClick.RemoveListener(CreateRoom);
        playerCountDropDown.onValueChanged.RemoveListener(OnSelectPlayerCount);
    }
    private void Start()
    {
        SetupDropdownOptions();
    }
    private void CreateRoom()
    {
        mainMenuCanvas.playerCount = playerCount;
        mainMenuCanvas.CreateGame();
    }
    private void SetupDropdownOptions()
    {
        //get the last selected score value
        playerCount = int.Parse(playerCountDropDown.options[0].text);
        playerCountDropDown.value = 0;
        playerCountDropDown.RefreshShownValue();
    }
    private void OnSelectPlayerCount(int index)
    {
        if (index >= 0 && index < playerCountDropDown.options.Count)
        {
            playerCount = int.Parse(playerCountDropDown.options[index].text);
        }
    }
}
