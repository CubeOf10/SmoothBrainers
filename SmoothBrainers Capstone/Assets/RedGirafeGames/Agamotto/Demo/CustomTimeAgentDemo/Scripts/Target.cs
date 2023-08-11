using System.Collections.Generic;
using RedGirafeGames.Agamotto.Scripts.Runtime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Demo.CustomTimeAgentDemo.Scripts
{
    /// <summary>
    /// TimeAgent following waypoints
    /// </summary>
    public class Target : MonoBehaviour
    {
        public float speed = 4;
        public List<Transform> waypoints;
        
        /// <summary>
        /// The current waypoint targeted.
        /// This data must be persisted in the TimeStone, so that when going back in time, it's value is up-to-date
        /// </summary>
        public int waypointIndex;

        private GameLogic _gameLogic;
        
        /// <summary>
        /// We use GenericTimeAgent to add waypointIndex in TimeStone's data easily (see in editor)
        /// Without GenericTimeAgent, we would need to use OnPersistData and SetDataTick UnityEvents or override TimeAgent
        /// </summary>
        private GenericTimeAgent _timeAgent;

        private void Start()
        {
            _gameLogic = FindObjectOfType<GameLogic>();
            _timeAgent = GetComponent<GenericTimeAgent>();
            _timeAgent.onSimulationStart.AddListener(OnSimulationStart);
            _timeAgent.onSimulationUpdate.AddListener(OnSimulationUpdate);
        }
        
        private void OnSimulationStart(TimeStone stone)
        {
            // If waypointIndex was a private field, we would have to initialize its value manually on init.
            // Because cloning does not copy private fields.
            // waypointIndex = (Original as TargetTimeAgent).waypointIndex;
        }

        private void Update()
        {
            // Making sure that when time is stopped, the Agent is not moving
            // We also make sure that simulationAgent does not run this code.
            if (_gameLogic.stopTime || _timeAgent.IsClone)
                return;

            RotateAndMove(Time.deltaTime);
        } 

        /// <summary>
        /// Update code for simulation
        /// </summary>
        /// <param name="step"></param>
        /// <param name="stone"></param>
        private void OnSimulationUpdate(TimeStone stone, float step)
        {
            RotateAndMove(step);
        }
        
        /// <summary>
        /// Move and rotate the agent every frame.
        /// This code is called from MonoBehaviour.Update and the TimeStone's simulation, so it BE CAREFUL here not to
        /// use Time.deltaTime or the simulation will be wrong, only use the argument deltaTime. 
        /// </summary>
        /// <param name="deltaTime">The delta time</param>
        private void RotateAndMove(float deltaTime)
        {
            // Get current targeted waypoint
            var waypointTarget = waypoints[waypointIndex];
            // Check if it's reached and update if reached
            if (Vector3.Distance(waypointTarget.position, transform.position) < 0.1f)
            {
                waypointIndex = waypointIndex == waypoints.Count - 1 ? 0 : waypointIndex + 1;
                waypointTarget = waypoints[waypointIndex];
            }

            // go toward current waypoint
            var t = transform;
            t.LookAt(waypointTarget);
            t.position += t.forward * (deltaTime * speed);
        }
        
    }
}