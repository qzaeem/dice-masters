using UnityEngine;
using Fusion;
using System.Collections;
using DiceGame.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace DiceGame.Game
{
    public class Dice : NetworkBehaviour, IPlayableDice
    {
        #region Networked Variables
        [Networked] public int dieID { get; set; }
        [Networked] public bool IsRollable { get; set; }
        #endregion

        [SerializeField] private DiceRollManager diceRollManager;
        [SerializeField] private float upwardForce, maxTorque;
        [SerializeField] private ActionSO onRollComplete;
        [SerializeField] private List<ValueDirection> valueDirections;
        public int _currentValue;
        private Rigidbody _rBody;

        private bool _isRolling = false;
        private bool _rollingStarted = false;
        public bool IsRolling { get { return _isRolling; } }
        public int CurrentValue { get { return _currentValue; } set { _currentValue = value; } }
        public Vector3 Position { get { return transform.position; } set { transform.position = value; } }

        private void Awake()
        {
            _rBody = GetComponent<Rigidbody>();
        }

        public override void Spawned()
        {
            diceRollManager.AddDie(this);
            _currentValue = 0;
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
            if (!IsRollable || _isRolling)
                return;

            if (!Runner.IsMasterClient())
            {
                _isRolling = true;
                return;
            }

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
            _currentValue = value;
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
            if (!IsRollable)
                return;

            RollDiceOnMasterRpc();
        }

        public void LockDie(bool isLocked)
        {
            LockDieRpc(isLocked);
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

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void LockDieRpc(bool isLocked)
        {
            if (!Runner.IsMasterClient())
                return;

            IsRollable = !isLocked;
        }
        #endregion
    }
}
