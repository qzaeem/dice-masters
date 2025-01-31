using DiceGame.ScriptableObjects;
using DiceGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomMatching : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private MainMenuCanvas mainMenu;
    [SerializeField] private PlayerInfoVariable playerInfo;

    private void OnEnable()
    {
        startButton.onClick.AddListener(StartRandomMatch);
        playerNameText.text = playerInfo.value.playerName;
    }
    private void OnDisable()
    {
        startButton.onClick.RemoveListener(StartRandomMatch);
    }
    private void StartRandomMatch()
    {
        //mainMenu.JoinRoom(GameMode.AutoHostOrClient, string.Empty);
    }
}
