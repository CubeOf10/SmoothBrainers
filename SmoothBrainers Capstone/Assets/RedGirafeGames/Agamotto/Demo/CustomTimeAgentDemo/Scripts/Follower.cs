using RedGirafeGames.Agamotto.Scripts.Runtime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using RedGirafeGames.Agamotto.Scripts.Runtime.Utils;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Demo.CustomTimeAgentDemo.Scripts
{
    /// <summary>
    /// TimeAgent following the target
    /// This agent is moved using CharacterController to demonstrate its use with Agamotto
    /// </summary>
    public class Follower : MonoBehaviour
    {
        public Target target;
        public float speed = 3;
        
        private GameLogic _gameLogic;
        private CharacterController _controller;
        private TimeAgent _timeAgent;

        private void Start()
        {
            _timeAgent = GetComponent<TimeAgent>();
            _timeAgent.onSimulationStart.AddListener(OnSimulationStart);
            _timeAgent.onSimulationUpdate.AddListener(OnSimulationUpdate);

            _controller = GetComponent<CharacterController>();
            _gameLogic = FindObjectOfType<GameLogic>();

            // Target is dynamically defined
            // Only the original Time Agent can init its target in MonoBehaviour.Start()
            if (!_timeAgent.IsClone)
            {
                // If SimulateOnStart is activated, the clone can already be instantiated, so we need to avoid using it
                target = TimeStoneUtils.FindObjectOfTypeWithoutSimulationAgents<Target>();
            }
        }

        private void Update()
        {
            // Making sure that when time is stopped, the Agent is not moving
            // We also have to check for isClone in case the simulation is asynchronous, in that case the simulation agent will run Update callbacks
            if (_gameLogic.stopTime || _timeAgent.IsClone)
            {
                _controller.enabled = false;
                return;
            }

            RotateAndMove(Time.deltaTime);
        }

        /// <summary>
        /// Move and Rotate the Object.
        /// VERY IMPORTANT here is to use a parameter deltaTime and NOT Time.deltaTime.
        /// Because if this method is called in a Simulation, Time.deltaTime could be wrong (and the simulation incoherent).
        /// </summary>
        /// <param name="deltaTime"></param>
        private void RotateAndMove(float deltaTime)
        {
            _controller.enabled = true;
            transform.LookAt(target.transform.position);
            // Here use CharacterController.Move and not CharacterController.SimpleMove, or you'll have surprises in your simulation
            _controller.Move(transform.forward * (deltaTime * speed));
        
            // Useless here, but just an example of how jump can be handled
            // if (_jumping && transform.position.y <= 0)
            // {
            //     _jumping = false;
            //     _animator.SetBool(AnimatorJumpingParam, false);
            //     _playerVelocity.y = 0;
            // }
            // Changes the height position of the player..
            // if (_jumpAsked)
            // {
            //     _jumping = true;
            //     _animator.SetBool(AnimatorJumpingParam, true);
            //     _playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            //     _jumpAsked = false;
            // }
            //
            // _playerVelocity.y += gravityValue * Time.deltaTime;

        }

        private void OnSimulationStart(TimeStone stone)
        {
            // we need to use the cloned target gameObject from the simulation scene
            // we can find it easily using TimeStoneUtils wich search only in Simulation objects
            target = TimeStoneUtils.FindObjectOfTypeInSimulationAgents<Target>(stone);
        }

        private void OnSimulationUpdate(TimeStone stone, float step)
        {
            // We let default SimulationCallbackPolicy.CloneOnly, so no need to check isClone here, only the simulation agent will execute this code
            RotateAndMove(step);
        }
    }
}