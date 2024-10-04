using UnityEngine;
using DiceGame.UI;
using System.Linq;
using DiceGame.Game;

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
        public IntVariable gameScore;
        public PlayersListVariable players;
        public int numberOfDice;

        [Header("Prefabs")]
        public MenuBase gameModeMenu;

        [Header("Settings")]
        public bool isMultiplayer = true;
        public bool showScoresOnEnd = false;

        private MenuBase gameMenu;
        private bool _hasGameEnded = false;

        public bool HasGameEnded { get { return _hasGameEnded; } }

        public override void Initialize()
        {
            ResetGameScore();
            _hasGameEnded = false;
            onDiceRollComplete.executeAction += OnDiceRollComplete;
            masterUpdatedAction.executeAction += OnMasterChanged;
        }

        public override void OnDestroy()
        {
            onDiceRollComplete.executeAction -= OnDiceRollComplete;
            masterUpdatedAction.executeAction -= OnMasterChanged;
        }

        public virtual MenuBase SpawnGameModeMenu()
        {
            gameMenu = Instantiate(gameModeMenu);
            gameMenu.localPlayerManager = PlayerManager.LocalPlayer;
            return gameMenu;
        }

        public virtual int GetNumberOfDice()
        {
            return numberOfDice;
        }

        public virtual void UpdateGameScore(int score)
        {
            gameScore.Set(score);
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

        public virtual void PlayerRiskedScore()
        {
            if(players.value.All(p => p.hasRiskedScore))
            {
                IncrementRound();
            }
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

        public abstract string GetSettingsJson();
        public abstract void SetSettingsFromJson(string jsonString);

        public void ResetGameScore() => gameScore.Set(0);
    }

    public enum GameModeName { BankrollBattle = 0, Greed = 1, Mexico = 2, KnockEmDown = 3 }
}

[System.Serializable]
public class SettingsData
{
    public bool isMultiplayer;
    public bool showScoresOnEnd;
}
