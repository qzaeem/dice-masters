using DiceGame.Network;
using DiceGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomController : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private MainMenuCanvas mainMenuCanvas;
    [SerializeField] private TMP_Dropdown selectGameDropDown;
    [SerializeField] private TMP_Dropdown playerCountDropDown;
    private int playerCount;
    private bool isPrivateGame;
    private bool isOpenGame;
    private void OnEnable()
    {
        createButton.onClick.AddListener(CreateRoom);
        playerCountDropDown.onValueChanged.AddListener(OnSelectPlayerCount);
        selectGameDropDown.onValueChanged.AddListener(OnSelectGame);
        SetupDropdownOptions();
    }
    private void OnDisable()
    {
        createButton.onClick.RemoveListener(CreateRoom);
        playerCountDropDown.onValueChanged.RemoveListener(OnSelectPlayerCount);
        selectGameDropDown.onValueChanged.RemoveListener(OnSelectGame);
    }
    private void CreateRoom()
    {
        //isPrivateGame = privateGameCheckMark.currentValue;
        mainMenuCanvas.playerCount = playerCount;
        NetworkManager.Instance.SetGame(isOpenGame, isPrivateGame);
        mainMenuCanvas.CreateGame();
    }
    private void SetupDropdownOptions()
    {
        isOpenGame = true;
        isPrivateGame = false;
        //get the last selected score value
        playerCount = int.Parse(playerCountDropDown.options[0].text);
        playerCountDropDown.value = 0;
        selectGameDropDown.value = 0;
        playerCountDropDown.RefreshShownValue();
    }
    private void OnSelectPlayerCount(int index)
    {
        if (index >= 0 && index < playerCountDropDown.options.Count)
        {
            playerCount = int.Parse(playerCountDropDown.options[index].text);
        }
    }
    private void OnSelectGame(int index)
    {
        if (index >= 0 && index < playerCountDropDown.options.Count)
        {
            if (index == 0)
            {
                isPrivateGame = false;
                isOpenGame = true;
            }
            else if (index == 1)
            {
                isPrivateGame = true;
                isOpenGame = false;
            }
        }
    }
}
