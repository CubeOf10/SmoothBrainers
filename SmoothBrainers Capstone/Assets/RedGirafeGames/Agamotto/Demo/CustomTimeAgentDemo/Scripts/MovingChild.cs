using RedGirafeGames.Agamotto.Scripts.Runtime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Demo.CustomTimeAgentDemo.Scripts
{
    /// <summary>
    /// Very basic component validating that a TimeAgent can be used as a child of another TimeAgent
    /// </summary>
    public class MovingChild : MonoBehaviour
    {

        public float rotationSpeed = 50.0f;
        public float moveSpeed = 1.0f;
        public float yMax = 6;
        public float yMin = 4;
        public bool movingUp = true;
        
        private GameLogic _gameLogic;

        private GenericTimeAgent _timeAgent;
        
        private void Start()
        {
        
            _gameLogic = FindObjectOfType<GameLogic>();
            _timeAgent = GetComponent<GenericTimeAgent>();
            _timeAgent.onSimulationUpdate.AddListener(OnSimulationUpdate);
        }

        private void Update()
        {
            // Making sure that when time is stopped, the Agent is not moving
            // We also make sure that simulationAgent does not run this code.
            if (_gameLogic.stopTime || _timeAgent.IsClone)
                return;

            Move(Time.deltaTime);
        }

        /// <summary>
        /// Update code for simulation
        /// </summary>
        /// <param name="step"></param>
        /// <param name="stone"></param>
        private void OnSimulationUpdate(TimeStone stone, float step)
        {
            Move(step);
        }
    

        private void Move(float deltaTime)
        {
            // Making the object rotate on itself
            transform.Rotate(Vector3.up, rotationSpeed * deltaTime);
            
            // Making the object move up and down
            if (transform.localPosition.y <= yMin)
                movingUp = true;
            else if (transform.localPosition.y >= yMax)
                movingUp = false;

            var moveVector = Vector3.up * (moveSpeed * deltaTime);
            moveVector *= movingUp ? 1 : -1;
            transform.Translate(moveVector);
        }
    }
}
