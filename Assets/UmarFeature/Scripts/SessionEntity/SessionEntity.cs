using DiceGame.Network;
using DiceGame.UI;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionEntity : MonoBehaviour
{
    public TextMeshProUGUI sessionKeyText;
    public TextMeshProUGUI sessionGameTypeText;
    public TextMeshProUGUI sessionPlayerCountText;
     
    public Button joinBtn;
    private void Start()
    {
        joinBtn.onClick.AddListener(JoinSession);
    }

    //public void UpdateSessionName(string sessionName)
    //{
    //    sessionNameText.text = sessionName;
    //    joinBtn.onClick.AddListener(() => JoinSession(GameMode.Client, sessionNameText.text));

    //}

    public void JoinSession()
    {
        //NetworkManager.Instance.JoinGame(sessionKeyText.text);
        MainMenuCanvas.instance.JoinRoom(sessionKeyText.text);
        //SessionNameUI.instance.sessionNameUI.SetActive(false);
        //SessionNameUI.instance.lobbyUiPanel.SetActive(true);
    }
}
