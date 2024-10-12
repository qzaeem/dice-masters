using UnityEngine;
using DiceGame.UI;
using System.Linq;
using DiceGame.Game;
using Fusion;

namespace DiceGame.ScriptableObjects
{
    public abstract class GameModeBase : ActionSO
    {
        [HideInInspector] public GameManager gameManager;

        [Header("Fields")]
        public GameModeName mode;
        public DiceRollManager diceRollManager;
        public ActionSO onDiceRollComplete;
        public ActionSO masterUpdatedAction;
        public ActionSO playerBankedScoreAction;
        public IntVariable gameScore;
        public IntVariable roundVariable;
        public IntVariable rollVariable;
        public PlayersListVariable players;
        public int numberOfDice;

        [Header("Prefabs")]
        public MenuBase gameModeMenuPrefab;

        [Header("Settings")]
        public bool isMultiplayer = true;
        public bool showScoresOnEnd = false;

        protected MenuBase gameMenu;
        protected bool _hasGameEnded = false;

        public bool HasGameEnded { get { return _hasGameEnded; } set { _hasGameEnded = value; } }
        public bool IsRolling { get { return gameManager.IsRolling; } }
        public int GameScore { get { return gameScore.value; } }
        public PlayerRef ActivePlayerTurn { get { return gameManager.ActivePlayerTurn; } }

        public override void Initialize()
        {
            gameScore.Set(gameManager.GameScore);
            _hasGameEnded = false;
            onDiceRollComplete.executeAction += OnDiceRollComplete;
            masterUpdatedAction.executeAction += OnMasterChanged;
            playerBankedScoreAction.executeAction += OnPlayerBankedScore;
            roundVariable.onValueChange += OnRoundChanged;
        }

        public override void OnDestroy()
        {
            onDiceRollComplete.executeAction -= OnDiceRollComplete;
            masterUpdatedAction.executeAction -= OnMasterChanged;
            playerBankedScoreAction.executeAction -= OnPlayerBankedScore;
            roundVariable.onValueChange -= OnRoundChanged;
        }

        public virtual MenuBase SpawnGameModeMenu()
        {
            gameMenu = Instantiate(gameModeMenuPrefab);
            gameMenu.localPlayerManager = PlayerManager.LocalPlayer;
            return gameMenu;
        }

        public virtual int GetNumberOfDice()
        {
            return numberOfDice;
        }

        public virtual void UpdateGameScore(int score)
        {
            if (!PlayerManager.LocalPlayer.isMasterClient)
                return;

            gameManager.SetGameScore(score);
        }

        public virtual void RollDice()
        {
            gameManager.RollDice();
        }

        public virtual void BankScore()
        {
            gameManager.TryBankingScr(gameScore.value);
        }

        public virtual void OnDiceRollComplete()
        {
            gameMenu.OnDiceRollComplete(diceRollManager.dice);
        }

        public virtual void IncrementRound()
        {
            gameManager.IncreaseRound();
        }

        public virtual void OnMasterChanged()
        {
            gameMenu.OnMasterChanged();
        }

        public virtual void OnGameEnded()
        {
            _hasGameEnded = true;
        }

        public virtual void TooLateToBank()
        {
            gameMenu.ShowRollMessage("Too Late!");
        }

        public virtual void ChangePlayerTurn()
        {
            var activePlayer = players.value.FirstOrDefault(p => p.playerRef == gameManager.ActivePlayerTurn);

            if(activePlayer == null)
            {
                gameManager.ChangePlayerTurn(players.value[0].playerRef);
                return;
            }

            bool everyoneBanked = players.value.All(p => p.hasBankedScore);
            if (everyoneBanked)
            {
                IncrementRound();
                return;
            }

            var index = players.value.IndexOf(activePlayer);
            int nextIndex = index >= players.value.Count() - 1 ? 0 : index + 1;
            for(int i = nextIndex; i < players.value.Count(); i++)
            {
                if (!players.value[i].hasBankedScore)
                {
                    nextIndex = i;
                    break;
                }
                if (i >= players.value.Count() - 1)
                    i = -1;
            }

            gameManager.ChangePlayerTurn(players.value[nextIndex].playerRef);
        }

        public virtual void OnRoundChanged(int round)
        {
            gameMenu.OnRoundChanged(round);
            PlayerManager.LocalPlayer.hasBankedScore = false;
        }

        public void ResetGameScore()
        {
            if (!PlayerManager.LocalPlayer.isMasterClient)
                return;

            gameManager.SetGameScore(0);
        }

        public virtual void OnPlayerBankedScore()
        {

        }

        public abstract string GetSettingsJson();
        public abstract void SetSettingsFromJson(string jsonString);
        public abstract void EndGame();
    }

    public enum GameModeName { BankrollBattle = 0, Greed = 1, Mexico = 2, KnockEmDown = 3 }
}

[System.Serializable]
public class SettingsData
{
    public bool isMultiplayer;
    public bool showScoresOnEnd;
}
