using UnityEngine;

namespace DiceGame.Game
{
    public enum Direction { UP, DOWN, RIGHT, LEFT, FORWARD, BACKWARD }

    [System.Serializable]
    public class ValueDirection
    {
        public Direction direction;
        public int value;
    }

    public interface IPlayableDice
    {
        public enum Direction { UP, DOWN, RIGHT, LEFT, FORWARD, BACKWARD }

        public int dieID { get; set; }
        public bool IsRollable { get; set; }

        public bool IsRolling { get; }
        public int CurrentValue { get; set; }
        public Vector3 Position { get; set; }

        public void RequestRoll();
        public void LockDie(bool isLocked);
    }
}
