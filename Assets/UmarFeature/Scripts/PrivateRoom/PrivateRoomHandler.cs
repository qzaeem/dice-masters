using System.Linq;
using System.Threading.Tasks;
using DiceGame.Game;
using DiceGame.ScriptableObjects;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrivateRoomHandler : NetworkBehaviour, IPlayerLeft
{
    public static PrivateRoomHandler Instance;

    [SerializeField] private PlayerInfoVariable playerInfo;
    [SerializeField] private PlayerManager playerManagerPrefab;
    [SerializeField] private PlayersListVariable playersList;

    [SerializeField] private GameObject otherPlayerNamePrefab, loadingMenu;
    [SerializeField] private Transform playerListParent;
    [SerializeField] private TextMeshProUGUI playerCountText, sessionNameText, localPlayerNameText;
    [SerializeField] private Button startGameButton;
    private void OnEnable()
    {
        startGameButton.onClick.AddListener(StartGame);
        playersList.onListValueChange += AddPlayer;
    }
    private void OnDisable()
    {
        startGameButton.onClick.RemoveListener(StartGame);
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
        playerCountText.text = $"Players: {Runner.SessionInfo.PlayerCount.ToString()}";
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
    }
    //public override void Despawned(NetworkRunner runner, bool hasState)
    //{
    //    startGameButton.onClick.RemoveListener(StartGame);
    //    playersList.onListValueChange -= AddPlayer;
    //    if (Runner.IsMasterClient())
    //    {
    //        AssignNewHost();
    //    }
    //}
    public void PlayerLeft(PlayerRef player)
    {
        if (Runner.IsMasterClient() && player == Runner.LocalPlayer)
        {
            AssignNewHost();
        }
    }
    private async void AssignNewHost()
    {
        await Task.Delay(500); // 0.5s delay
        if (!Runner.IsRunning || !Runner.ActivePlayers.Any())
            return;

        PlayerRef newHost = Runner.ActivePlayers.FirstOrDefault(); // Get first active player
        if (newHost == default) return; // No players available
        Debug.Log($"New host assigned: {newHost}");
        RPC_SetNewHost(newHost);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetNewHost(PlayerRef newHost)
    {
        if (Runner.LocalPlayer == newHost)
        {
            Debug.Log("I am now the new Host!");
            // Perform host-specific actions here
        }
    }
}