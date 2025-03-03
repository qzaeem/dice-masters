using DiceGame.Game;
using DiceGame.Network;
using DiceGame.ScriptableObjects;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrivateRoomHandler : NetworkBehaviour
{
    public static PrivateRoomHandler Instance;
    public bool IsRoom = true;

    [SerializeField] private PlayerInfoVariable playerInfo;
    [SerializeField] private PlayerManager playerManagerPrefab;
    [SerializeField] private PlayersListVariable playersList;
    //[SerializeField] private GameModeVariable currentGameMode;
    //[SerializeField] private List<GameModeBase> gameModes;

    [SerializeField] private GameObject otherPlayerNamePrefab, loadingMenu;
    [SerializeField] private Transform playerListParent;
    [SerializeField] private TextMeshProUGUI playerCountText, sessionNameText, localPlayerNameText;
    [SerializeField] private Button startGameButton, backButton;

    private void OnEnable()
    {
        startGameButton.gameObject.SetActive(false);
        startGameButton.onClick.AddListener(StartGame);
        backButton.onClick.AddListener(ShutdownSession);
        playersList.onListValueChange += AddPlayer;
    }
    private void OnDisable()
    {
        startGameButton.onClick.RemoveListener(StartGame);
        backButton.onClick.RemoveListener(ShutdownSession);
        playersList.onListValueChange -= AddPlayer;
    }
    private void OnDestroy()
    {
        startGameButton.onClick.RemoveListener(StartGame);
        backButton.onClick.RemoveListener(ShutdownSession);
        playersList.onListValueChange -= AddPlayer;
    }
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        //startGameButton.gameObject.SetActive(false);
        localPlayerNameText.text = playerInfo.value.playerName;
    }
    public override void Spawned()
    {
        //playersList.value.Clear();
        loadingMenu.SetActive(false);
        var player = Runner.Spawn(playerManagerPrefab);
        SetPlayerInfo(player);
        Runner.SetPlayerObject(Runner.LocalPlayer, player.Object);

        UpdatePlayerList();
        SetSessionNameText();
    }
    private void SetPlayerInfo(PlayerManager player)
    {
        PlayerManager.LocalPlayer = player;
        player.playerRef = Runner.LocalPlayer;
        player.playerName = playerInfo.value.playerName;
    }
    public void UpdatePlayerList()
    {
        foreach (Transform t in playerListParent.transform)
        {
            Destroy(t.gameObject);
        }
        foreach (var p in playersList.value)
        {
            GameObject playerNamesObj = Instantiate(otherPlayerNamePrefab, playerListParent);
            var obj = playerNamesObj.GetComponent<PlayerUIEntry>();
            obj.playerNameText.text = p.playerName;
        }
        //playerCountText.text = $"Players: {playersList.value.Count}";
        int currentPlayerCount = Runner.SessionInfo.PlayerCount;
        playerCountText.text = $"Players: {currentPlayerCount.ToString()}";

        //enable start button when max players joined
        if (Runner.IsMasterClient())
        {
            if (currentPlayerCount == Runner.SessionInfo.MaxPlayers)
            {
                startGameButton.gameObject.SetActive(true);
            }
        }
    }

    private void SetSessionNameText()
    {
        if (Runner != null && Runner.SessionInfo.IsValid)
        {
            sessionNameText.text = Runner.SessionInfo.Name;
        }
    }

    private void AddPlayer(PlayerManager player)
    {
        UpdatePlayerList();
    }
    void StartGame()
    {
        Debug.Log($"Starting Game");
        Runner.LoadScene("MainScene");
    }

    //public void PlayerLeft(PlayerRef player)
    //{
    //    if (Runner.IsMasterClient() && player == Runner.LocalPlayer)
    //    {
    //        AssignNewHost();
    //    }
    //}
    //private async void AssignNewHost()
    //{
    //    await Task.Delay(500); // 0.5s delay
    //    if (!Runner.IsRunning || !Runner.ActivePlayers.Any())
    //        return;

    //    PlayerRef newHost = Runner.ActivePlayers.FirstOrDefault(); // Get first active player
    //    if (newHost == default) return; // No players available
    //    Debug.Log($"New host assigned: {newHost}");
    //    RPC_SetNewHost(newHost);
    //}

    //[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    //private void RPC_SetNewHost(PlayerRef newHost)
    //{
    //    if (Runner.LocalPlayer == newHost)
    //    {
    //        Debug.Log("I am now the new Host!");
    //        startGameButton.gameObject.SetActive(true);
    //        // Perform host-specific actions here
    //    }
    //}
    private async void ShutdownSession()
    {
        loadingMenu.SetActive(false);
        if (NetworkManager.Instance._networkRunner != null)
        {
            Debug.Log("🔴 Shutting down session...");
            // Shutdown the network runner
            await NetworkManager.Instance._networkRunner.Shutdown();
            Debug.Log("✅ Session successfully shut down.");
            //// Optionally, destroy the NetworkRunner instance if needed
            //Destroy(NetworkManager.Instance._networkRunner.gameObject);
        }
    }
}