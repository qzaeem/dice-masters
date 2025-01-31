using DiceGame.UI;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomController : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private MainMenuCanvas mainMenuCanvas;
    string roomKey;
    private void OnEnable()
    {

        createButton.onClick.AddListener(CreateRoom);
    }
    private void OnDisable()
    {
        createButton.onClick.RemoveListener(CreateRoom);
    }
    private void CreateRoom()
    {
        mainMenuCanvas.StartGame();
    }
}
