using DiceGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinPrivateRoom : MonoBehaviour
{
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private TMP_InputField roomKey;
    [SerializeField] private MainMenuCanvas mainMenu;

    private void OnEnable()
    {
        joinRoomButton.onClick.AddListener(JoinRoom);
        roomKey.onValueChanged.AddListener(OnRoomKeyChange);
    }
    private void Start()
    {
        joinRoomButton.interactable = false;
    }
    private void OnDisable()
    {
        joinRoomButton.onClick.RemoveListener(JoinRoom);
    }
    private void OnRoomKeyChange(string value)
    {
        joinRoomButton.interactable = value.Length > 0 && value.Length < 17 && !string.IsNullOrWhiteSpace(value);
    }
    void JoinRoom()
    {
        mainMenu.JoinRoom(roomKey.text);
    }
}
