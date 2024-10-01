using UnityEngine;
using System.Linq;
using Fusion;

namespace DiceGame.ScriptableObjects
{
    public abstract class GameModeBase : ActionSO
    {
        public GameModeName mode;
        public DiceRollManager diceRollManager;
        public ActionSO onDiceRollComplete;
        public IntVariable gameScore;
        public PlayersListVariable players;
        public int numberOfDice;

        public override void Initialize()
        {
            ResetGameScore();
            onDiceRollComplete.executeAction += OnDiceRollComplete;
        }

        public override void OnDestroy()
        {
            onDiceRollComplete.executeAction -= OnDiceRollComplete;
        }

        public virtual int GetNumberOfDice()
        {
            return numberOfDice;
        }

        public virtual void BankScore(PlayerRef playerRef)
        {

        }

        public virtual void OnDiceRollComplete()
        {

        }

        public void ResetGameScore() => gameScore.Set(0);
    }

    public enum GameModeName { BankrollBattle, Greed, Mexico, KnockEmDown }
}
