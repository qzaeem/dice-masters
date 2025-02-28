using DiceGame.Network;
using DiceGame.UI;
using UnityEngine;
using UnityEngine.UI;

public class RoomOptions : MonoBehaviour
{
    [SerializeField] private MainMenuCanvas mainMenu;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private GameObject createRoomMenu;
    [SerializeField] private GameObject joinRoomMenu;

    private void OnEnable()
    {
        createRoomButton.onClick.AddListener(EnableCreateRoomMenu);
        joinRoomButton.onClick.AddListener(EnableJoinRoomMenu);
    }
    private void OnDisable()
    {
        createRoomButton.onClick.RemoveListener(EnableCreateRoomMenu);
        joinRoomButton.onClick.RemoveListener(EnableJoinRoomMenu);
    }
    //Enter lobby session to join random match
    private async void EnableCreateRoomMenu()
    {
        //createRoomMenu.SetActive(true);
        this.gameObject.SetActive(false);
        //mainMenu.OpenMenu(mainMenu.MPMenus.createRoom);
        //mainMenu.OpenModeSelectionMenu(false);
        await NetworkManager.Instance.EnterLobby();
        mainMenu.OpenMenu(mainMenu.MPMenus.LobbyMenu);
    }
    private void EnableJoinRoomMenu()
    {
        this.gameObject.SetActive(false);
        mainMenu.OpenMenu(mainMenu.MPMenus.JoinRoom);
        //joinRoomMenu.SetActive(true);
    }
}
