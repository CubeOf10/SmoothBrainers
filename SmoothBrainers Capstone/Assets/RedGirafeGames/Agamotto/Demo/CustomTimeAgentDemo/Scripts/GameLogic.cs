using System;
using RedGirafeGames.Agamotto.Scripts.Runtime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Utils;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Demo.CustomTimeAgentDemo.Scripts
{
    /// <summary>
    /// Custom Time Agent Demo Game Logic.
    /// Toggle the start / stop and simulation
    /// </summary>
    public class GameLogic : MonoBehaviour
    {
        public TimeStone stone;

        public bool stopTime = false;

        public void StopStartSimulate()
        {
            Debug.Log("Toggle Stop / Start and simulate");
            stopTime = !stopTime;
            if (stopTime)
            {
                stone.Clear();
                stone.StartSimulation();
                stone.FreezeTimeAgents(true);
            }
            else
            {
                if (stone.Simulating)
                    stone.StopSimulation();

                stone.Clear();
                stone.FreezeTimeAgents(false);
            }
        }
    }
}