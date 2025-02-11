using DiceGame.Network;
using DiceGame.UI;
using UnityEngine;
using UnityEngine.UI;

public class PlayerConnectionMenu : MonoBehaviour
{
    [SerializeField] private MainMenuCanvas mainMenu;
    [SerializeField] private Button randomMatchBtn;
    [SerializeField] private Button privateRoomBtn;
    [SerializeField] private Button enterLobbyBtn;
    //[SerializeField] private GameObject randomMatchMenu;
    [SerializeField] private GameObject privateRoomMenu;

    private void OnEnable()
    {
        randomMatchBtn.onClick.AddListener(() => EnableMenu(1));
        privateRoomBtn.onClick.AddListener(() => EnableMenu(2));
        enterLobbyBtn.onClick.AddListener(() => EnableMenu(3));
    }
    private void OnDisable()
    {
        randomMatchBtn.onClick.RemoveAllListeners();
        privateRoomBtn.onClick.RemoveAllListeners();
        enterLobbyBtn.onClick.RemoveAllListeners();
    }
    private void OnDestroy()
    {
        randomMatchBtn.onClick.RemoveAllListeners();
        privateRoomBtn.onClick.RemoveAllListeners();
        enterLobbyBtn.onClick.RemoveAllListeners();
    }

    private async void EnableMenu(int menu)
    {
        this.gameObject.SetActive(false);
        switch (menu)
        {
            case 1:
                //randomMatchMenu.SetActive(true);
                //mainMenu.OpenMenu(mainMenu.MPMenus.randomMatch);
                mainMenu.OpenModeSelectionMenu(true);
                break;
            case 2:
                privateRoomMenu.SetActive(true);
                mainMenu.OpenMenu(mainMenu.MPMenus.createOrJoinRom);
                break;
            case 3:
               await NetworkManager.Instance.EnterLobby();
                mainMenu.OpenMenu(mainMenu.MPMenus.LobbyMenu);
                break;
        }
    }
}
