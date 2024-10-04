using UnityEngine;
using Fusion;
using DiceGame.ScriptableObjects;
using DiceGame.Network;
using DiceGame.Game;

public class GameManager : NetworkBehaviour
{
    #region Networked Properties
    [Networked] private bool _isRolling { get; set; }
    [Networked, OnChangedRender(nameof(UpdateCanBankScore))] private bool _canBankScore { get; set; }
    [Networked, OnChangedRender(nameof(OnRoundChanged))] private int _round { get; set; }
    #endregion

    [SerializeField] private GameModeVariable currentGameMode;
    [SerializeField] private DiceRollManager diceRollManager;
    [SerializeField] private ActionSO onDiceRollComplete;
    [SerializeField] private ActionSO onRiskedScore;
    [SerializeField] private IntVariable roundVariable;
    [SerializeField] private BoolVariable canBankScoreVariable;
 
    public bool IsRolling { get { return _isRolling; } }
    public bool CanBankScore { get { return _canBankScore; } }

    public override void Spawned()
    {
        currentGameMode.value.gameManager = this;
        NetworkManager.Instance.onPlayerJoined += OnPlayerJoined;
        onDiceRollComplete.executeAction += OnDiceRollComplete;
        onRiskedScore.executeAction += PlayerRiskedScore;

        if (!Runner.IsSharedModeMasterClient)
        {
            roundVariable.Set(_round);
            return;
        }

        _round = 1;
        _canBankScore = false;
        _isRolling = false;
    }

    private void UpdateCanBankScore()
    {
        canBankScoreVariable.Set(_canBankScore);
    }

    private void BankScore(int score)
    {
        PlayerManager.LocalPlayer.totalScore += score;
        IncreaseRound();
    }

    private void OnDiceRollComplete()
    {
        _isRolling = false;
    }

    private void PlayerRiskedScore()
    {
        if (Runner.IsSharedModeMasterClient)
        {
            currentGameMode.value.PlayerRiskedScore();
        }
    }

    private void OnRoundChanged()
    {
        roundVariable.Set(_round);
    }

    public void IncreaseRound()
    {
        IncrementRoundRpc();
    }

    public void RollDice()
    {
        if (IsRolling)
            return;

        _isRolling = true;
        diceRollManager.Execute();
    }

    public void EnableBankingAbility()
    {
        if(Runner.IsSharedModeMasterClient)
            _canBankScore = true;
    }

    public void TryBankingScr(int score)
    {
        TryBankingScoreRpc(PlayerManager.LocalPlayer.playerRef, score);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        NetworkManager.Instance.onPlayerJoined -= OnPlayerJoined;
        onDiceRollComplete.executeAction -= OnDiceRollComplete;
        onRiskedScore.executeAction -= PlayerRiskedScore;
    }

    #region RPCs
    [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = false)]
    private void SetSettingsRpc([RpcTarget] PlayerRef playerRef, string jsonString)
    {
        currentGameMode.value.SetSettingsFromJson(jsonString);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void BankScoreForPlayerRpc([RpcTarget] PlayerRef banker, int score)
    {
        BankScore(score);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void TryBankingScoreRpc(PlayerRef banker, int score)
    {
        currentGameMode.value.ResetGameScore();

        if (!Runner.IsSharedModeMasterClient || !CanBankScore || IsRolling)
            return;

        _canBankScore = false;
        BankScoreForPlayerRpc(banker, score);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void IncrementRoundRpc()
    {
        if (Runner.IsSharedModeMasterClient)
            _round += 1;
    }
    #endregion

    #region Runner Callbacks
    public void OnPlayerJoined(PlayerRef player)
    {
        if (!Runner.IsSharedModeMasterClient) return;

        var jsonString = currentGameMode.value.GetSettingsJson();
        SetSettingsRpc(player, jsonString);
    }
    #endregion
}
