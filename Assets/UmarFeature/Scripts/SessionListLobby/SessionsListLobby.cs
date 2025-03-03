using System.Collections.Generic;
using DiceGame.Network;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class SessionsListLobby : MonoBehaviour
{
    public static SessionsListLobby Instance;
    public GameObject sessionEntityPrefab;
    public GameObject sessionEntitySpawnParent;
    [SerializeField] private Button backButton;

    private void OnEnable()
    {
        backButton.onClick.AddListener(LeaveLobby);
    }
    private void OnDisable()
    {
        backButton.onClick.RemoveListener(LeaveLobby);
    }
    private void Awake()
    {
        Instance = this;
    }

    public void UpdateSessionUIEntry(List<SessionInfo> sessionInfoList)
    {
        RemoveAllEntries();
        CreateEntries(sessionInfoList);
    }

    public void CreateEntries(List<SessionInfo> sessionInfoList)
    {
        foreach (SessionInfo item in sessionInfoList)
        {
            GameObject obj = Instantiate(sessionEntityPrefab, sessionEntitySpawnParent.transform);
            obj.GetComponent<SessionEntity>().sessionKeyText.text = item.Name;
            obj.GetComponent<SessionEntity>().sessionPlayerCountText.text = $"Players: {item.PlayerCount}";
            string gameTypeText = "Unknown";
            if (item.Properties.TryGetValue("gameType", out var gameTypeValue))
            {
                switch ((int)gameTypeValue)
                {
                    case 0:
                        gameTypeText = "Game Type: Bankroll";
                        break;
                    case 1:
                        gameTypeText = "Game Type: Greed";
                        break;
                    case 2:
                        gameTypeText = "Game Type: Mexico";
                        break;
                    case 3:
                        gameTypeText = "Game Type: Knock em down";
                        break;
                }
            }
            obj.GetComponent<SessionEntity>().sessionGameTypeText.text = gameTypeText;
        }
    }
    public void RemoveAllEntries()
    {
        foreach (Transform obj in sessionEntitySpawnParent.transform)
        {
            Destroy(obj.gameObject);
        }
    }
    private async void LeaveLobby()
    {
        if (NetworkManager.Instance._networkRunner != null)
        {
            Debug.Log("🔴 Leaving the lobby...");
            // Shutdown the NetworkRunner to leave the lobby
            await NetworkManager.Instance._networkRunner.Shutdown();
            Debug.Log("✅ Successfully left the lobby.");
        }
    }
}
