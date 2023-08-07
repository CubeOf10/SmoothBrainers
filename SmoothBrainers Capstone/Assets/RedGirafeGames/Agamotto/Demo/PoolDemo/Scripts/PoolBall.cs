using RedGirafeGames.Agamotto.Scripts.Runtime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Demo.PoolDemo.Scripts
{
    public class PoolBall : MonoBehaviour
    {
        private TimeAgent _timeAgent;
        private Rigidbody _rigidbody;
        private PoolPlayer _poolPlayer;

        public float minVelocityMagnitude = 0.001f;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _poolPlayer = FindObjectOfType<PoolPlayer>();

            _timeAgent = GetComponent<TimeAgent>();
            _timeAgent.onSimulationStart.AddListener(OnSimulationStart);
        }

        private void OnSimulationStart(TimeStone stone)
        {
            if (_poolPlayer.targetPoolBall == null) return;

            if (_poolPlayer.targetPoolBall.GetComponent<TimeAgent>().SimulationClone == _timeAgent)
            {
                Shoot(_poolPlayer.GetShootDirection());
            }
        }

        private void FixedUpdate()
        {
            if (_rigidbody.velocity.magnitude < 0.001f)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
        }

        public void Shoot(Vector3 direction)
        {
            _rigidbody.velocity = direction;
        }
    }
}