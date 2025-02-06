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
    private void CreateRoom()
    {
        mainMenuCanvas.playerCount = playerCount;
        mainMenuCanvas.StartGame();
    }
    private void OnSelectPlayerCount(int index)
    {
        if (index >= 0 && index < playerCountDropDown.options.Count)
        {
            playerCount = int.Parse(playerCountDropDown.options[index].text);
        }
    }
}
