using UnityEngine;
using Fusion;
using DiceGame.ScriptableObjects;
using DiceGame.Network;
using DiceGame.Game;

public class GameManager : NetworkBehaviour
{
    public static bool isSinglePlayerMode;

    #region Networked Properties
    [Networked, OnChangedRender(nameof(OnDiceRollChanged))] private bool _isRolling { get; set; }
    [Networked, OnChangedRender(nameof(UpdateCanBankScore))] private bool _canBankScore { get; set; }
    [Networked, OnChangedRender(nameof(OnRoundChanged))] private int _round { get; set; }
    [Networked, OnChangedRender(nameof(OnRollChanged))] private int _roll { get; set; }
    [Networked, OnChangedRender(nameof(OnGameScoreChanged))] private int _gameScore { get; set; }
    [Networked, OnChangedRender(nameof(CheckForGameStart))] private bool _hasGameStarted { get; set; }
    [Networked] private PlayerRef _activePlayerTurn { get; set; }
    #endregion

    [SerializeField] private GameModeVariable currentGameMode;
    [SerializeField] private DiceRollManager diceRollManager;
    [SerializeField] private ActionSO onDiceRollComplete;
    [SerializeField] private ActionSO masterUpdatedAction;
    [SerializeField] private PlayerRefVariable changePlayerTurnVariable;
    [SerializeField] private IntVariable gameScoreVariable;
    [SerializeField] private IntVariable roundVariable;
    [SerializeField] private IntVariable rollVariable;
    [SerializeField] private BoolVariable canBankScoreVariable;
    [SerializeField] private BoolVariable diceRollingVariable;
    [SerializeField] private PlayersListVariable players;

    public bool IsRolling { get { return _isRolling; } }
    public bool CanBankScore { get { return _canBankScore; } }
    public int GameScore { get { return _gameScore; } }
    public PlayerRef ActivePlayerTurn { get { return _activePlayerTurn; } }

    private bool _gameStartSet;

    public override void Spawned()
    {
        NetworkManager.Instance.onPlayerJoined += OnPlayerJoined;
        onDiceRollComplete.executeAction += OnDiceRollComplete;
        players.onListValueChange += OnPlayersUpdated;
        masterUpdatedAction.executeAction += () => OnPlayersUpdated(null);

        if (!Runner.IsMasterClient())
        {
            roundVariable.Set(_round);
            return;
        }

        _round = 1;
        _roll = 0;
        _canBankScore = false;
        _isRolling = false;
        diceRollingVariable.Set(false);
    }

    private void UpdateCanBankScore()
    {
        canBankScoreVariable.Set(_canBankScore);
    }

    private void TooLateToBank()
    {
        currentGameMode.value.TooLateToBank();
    }

    private void OnDiceRollChanged()
    {
        diceRollingVariable.Set(_isRolling);
    }

    private void OnDiceRollComplete()
    {
        _isRolling = false;
    }

    private void OnRoundChanged()
    {
        roundVariable.Set(_round);
        currentGameMode.value.ResetGameScore();
    }

    private void OnRollChanged()
    {
        rollVariable.Set(_roll);
    }

    private void OnGameScoreChanged()
    {
        gameScoreVariable.Set(_gameScore);
    }

    public void OnPlayersUpdated(PlayerManager player)
    {
        if (!Runner.IsMasterClient())
        {
            return;
        }

        currentGameMode.value.ShowStartGameButton(players.value.Count >= currentGameMode.value.minimumPlayersToStart && !_hasGameStarted);
        currentGameMode.value.SetMultiplayerCanvas();
    }

    private void CheckForGameStart()
    {
        if(_hasGameStarted && !_gameStartSet)
        {
            _gameStartSet = true;
            currentGameMode.value.DisableStartBlocker();
        }
    }

    public void StartGame()
    {
        _hasGameStarted = true;
        if(currentGameMode.value.isMultiplayer)
            NetworkManager.Instance.CloseGameForNewPlayers();
    }

    public void IncreaseRound()
    {
        if (!Runner.IsMasterClient())
            return;

        _round += 1;
        _roll = 0;
    }

    public void RollDice()
    {
        if (IsRolling)
            return;

        RollDiceRpc();
    }

    public void EnableBankingAbility()
    {
        if(Runner.IsMasterClient())
            _canBankScore = true;
    }

    public void SetGameScore(int score)
    {
        _gameScore = score;
    }

    public void TryBankingScr(int score)
    {
        if (!CanBankScore)
        {
            TooLateToBank();
            return;
        }

        TryBankingScoreRpc(PlayerManager.LocalPlayer.playerRef, score);
    }

    public void TryBankingDirect(int score)
    {
        if (!CanBankScore)
        {
            TooLateToBank();
            return;
        }

        TryBankingDirectRpc(PlayerManager.LocalPlayer.playerRef, score);
    }

    public void ChangePlayerTurn(PlayerRef playerRef)
    {
        if (!Runner.IsMasterClient())
            return;

        _activePlayerTurn = playerRef;
        ChangePlayerTurnRpc(playerRef);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        NetworkManager.Instance.onPlayerJoined -= OnPlayerJoined;
        onDiceRollComplete.executeAction -= OnDiceRollComplete;
        players.onListValueChange -= OnPlayersUpdated;
        masterUpdatedAction.executeAction = null;
    }

    #region RPCs
    [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = false)]
    private void SetSettingsRpc([RpcTarget] PlayerRef playerRef, string jsonString)
    {
        currentGameMode.value.SetSettingsFromJson(jsonString);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void ChangePlayerTurnRpc(PlayerRef playerRef)
    {
        changePlayerTurnVariable.Set(ActivePlayerTurn);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RollDiceRpc()
    {
        if (!Runner.IsMasterClient() || IsRolling)
            return;

        _isRolling = true;
        _canBankScore = false;
        _roll += 1;
        diceRollManager.Execute();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void BankScoreForPlayerRpc([RpcTarget] PlayerRef banker, int score)
    {
        PlayerManager.LocalPlayer.totalScore += score;
        PlayerManager.LocalPlayer.hasBankedScore = true;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void TooLateToBankRpc([RpcTarget] PlayerRef banker)
    {
        TooLateToBank();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void TryBankingScoreRpc(PlayerRef banker, int score)
    {
        if (!Runner.IsMasterClient() || IsRolling)
            return;

        if (!CanBankScore)
        {
            TooLateToBankRpc(banker);
            return;
        }

        _canBankScore = false;
        BankScoreForPlayerRpc(banker, score);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void TryBankingDirectRpc(PlayerRef banker, int score)
    {
        if (!Runner.IsMasterClient() || IsRolling)
            return;

        if (!CanBankScore)
        {
            TooLateToBankRpc(banker);
            return;
        }

        BankScoreForPlayerRpc(banker, score);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void AnnounceMexicoWinnerRPC(PlayerRef winner)
    {
        (currentGameMode.value as GameModeMexico).AnnounceWinner(winner);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void EnableRerollDiceRpc([RpcTarget] PlayerRef target)
    {
        (currentGameMode.value as GameModeMexico).EnableRerollButton();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ShowRollMessageRpc([RpcTarget] PlayerRef target, string message)
    {
        currentGameMode.value.gameMenu.ShowRollMessage(message);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void SpawnTilesRpc(string tilesJsonString)
    {
        (currentGameMode.value as GameModeTileKnock).SpawnTiles(tilesJsonString);
    }
    #endregion

    #region Runner Callbacks
    public void OnPlayerJoined(PlayerRef player)
    {
        if (!Runner.IsMasterClient()) return;

        var jsonString = currentGameMode.value.GetSettingsJson();
        SetSettingsRpc(player, jsonString);
    }
    #endregion
}
