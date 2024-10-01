using UnityEngine;

namespace DiceGame.ScriptableObjects
{
    public abstract class GameModeBase : ScriptableObject
    {
        [SerializeField] private GameModeName mode;
        [SerializeField] private int numberOfDice;

        public virtual int GetNumberOfDice()
        {
            return numberOfDice;
        }
    }

    public enum GameModeName { BankrollBattle, Greed, Mexico, KnockEmDown }
}
