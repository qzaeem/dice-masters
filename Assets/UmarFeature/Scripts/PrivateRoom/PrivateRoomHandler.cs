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
        playerCountText.text = $"Players: {playersList.value.Count}";
    }

    private void SetSessionNameText()
    {
        sessionNameText.text = NetworkManager.Instance.NewRoomKey;
    }

    private void AddPlayer(PlayerManager player)
    {
        UpdatePlayerList();
    }
    void StartGame()
    {
    }
}
