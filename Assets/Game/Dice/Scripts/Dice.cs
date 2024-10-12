using UnityEngine;
using Fusion;
using System.Collections;
using DiceGame.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace DiceGame.Game
{
    public class Dice : NetworkBehaviour
    {
        #region Networked Variables
        [Networked] public int dieID { get; set; }
        [Networked] public bool IsRollable { get; set; }
        #endregion

        public enum Direction { UP, DOWN, RIGHT, LEFT, FORWARD, BACKWARD }

        [System.Serializable]
        public class ValueDirection
        {
            public Direction direction;
            public int value;
        }

        [SerializeField] private DiceRollManager diceRollManager;
        [SerializeField] private float upwardForce, maxTorque;
        [SerializeField] private ActionSO onRollComplete;
        [SerializeField] private List<ValueDirection> valueDirections;
        public int currentValue;
        private Rigidbody _rBody;

        private bool _isRolling = false;
        private bool _rollingStarted = false;
        public bool IsRolling { get { return _isRolling; } }

        private void Awake()
        {
            _rBody = GetComponent<Rigidbody>();
        }

        public override void Spawned()
        {
            diceRollManager.AddDie(this);
            currentValue = 0;
            _isRolling = false;
            _rollingStarted = false;

            if (!Runner.IsMasterClient())
                return;

            IsRollable = true;
        }

        public override void Render()
        {
            if (!(_rollingStarted && Runner.IsMasterClient()))
                return;
            
            if(_rBody.angularVelocity == Vector3.zero && _rBody.linearVelocity == Vector3.zero)
            {
                CompleteRollRpc();
            }
        }

        private void Roll()
        {
            if (!Runner.IsMasterClient())
            {
                _isRolling = true;
                return;
            }

            if (!IsRollable || _isRolling)
                return;

            _rBody.isKinematic = false;
            _isRolling = true;
            _rBody.AddForce(Vector3.up * upwardForce);
            float xT = Random.Range(-maxTorque, maxTorque);
            float yT = Random.Range(-maxTorque, maxTorque);
            float zT = Random.Range(-maxTorque, maxTorque);
            _rBody.AddTorque(new Vector3(xT, yT, zT));
        }

        private void SetValue()
        {
            float alignment = 0;
            int value = 0;
            CheckDirection(transform.up, valueDirections.First(d => d.direction == Direction.UP).value,
                ref alignment, ref value);
            CheckDirection(-transform.up, valueDirections.First(d => d.direction == Direction.DOWN).value,
                ref alignment, ref value);
            CheckDirection(transform.right, valueDirections.First(d => d.direction == Direction.RIGHT).value,
                ref alignment, ref value);
            CheckDirection(-transform.right, valueDirections.First(d => d.direction == Direction.LEFT).value,
                ref alignment, ref value);
            CheckDirection(transform.forward, valueDirections.First(d => d.direction == Direction.FORWARD).value,
                ref alignment, ref value);
            CheckDirection(-transform.forward, valueDirections.First(d => d.direction == Direction.BACKWARD).value,
                ref alignment, ref value);
            currentValue = value;
        }

        private void CheckDirection(Vector3 dir, int dirVal, ref float alignment, ref int value)
        {
            float align = Vector3.Dot(Vector3.up, dir);

            if(align > alignment)
            {
                alignment = align;
                value = dirVal;
            }
        }

        private IEnumerator StartRoll()
        {
            Roll();

            while(_rBody.angularVelocity == Vector3.zero || _rBody.linearVelocity == Vector3.zero)
            {
                yield return null;
            }

            _rollingStarted = true;
        }

        public void RequestRoll()
        {
            RollDiceOnMasterRpc();
        }

        #region RPCs
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RollDiceOnMasterRpc()
        {
            StartCoroutine(StartRoll());
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void CompleteRollRpc()
        {
            StopAllCoroutines();
            SetValue();
            _isRolling = false;
            _rollingStarted = false;
            onRollComplete.Execute();
        }
        #endregion
    }
}
