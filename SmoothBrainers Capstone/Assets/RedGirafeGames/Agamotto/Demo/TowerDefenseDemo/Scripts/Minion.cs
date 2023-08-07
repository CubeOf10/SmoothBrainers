using System;
using RedGirafeGames.Agamotto.Scripts.Runtime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using RedGirafeGames.Agamotto.Scripts.Runtime.Utils;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Demo.TowerDefenseDemo.Scripts
{
    public class Minion : MonoBehaviour
    {
        public bool dead;
        public int life = 10;
        public float speed = 2;

        public GameLogic gameLogic;
        public TimeStone timeStone;
        [HideInInspector] public Transform endPoint;
        [HideInInspector] public TimeAgent timeAgent;

        // Start is called before the first frame update
        void Awake()
        {
            endPoint = GameObject.Find("End").transform;
            timeStone = FindObjectOfType<TimeStone>();

            timeAgent = GetComponent<TimeAgent>();
            timeAgent.onSimulationUpdate.AddListener(SimulationUpdate);

            // We need to populate the gameLogic field, which is also
            // a TimeAgent
            // Here are the things to think about
            // - Because GameLogic is a TimeAgent, we must be sure to pick the Clone for a simulation and the Original for the normal case
            // - We can't initialize here in Awake, because Awake could be called by Unity before the IsClone field is initialized by the TimeStone
            // and we wouldn't be able to differentiate the Original and Clone
            // - We can't use the timeAgent.onSimulationStart UnityEvent because this object is inactive in pool, and inactive objects
            // don't receive UnityEvents
            // - So finally we must use the Start method
            
        }
        
        private void Start()
        {
            // GameLogic is a TimeAgent, so we have to be sure we pick the right one if this TimeAgent is a simulation clone
            // Here we are sure that timeAgent.IsClone is defined well
            if (!timeAgent.IsClone)
            {
                // Search for instance EXCLUDING simulation clones
                gameLogic = TimeStoneUtils.FindObjectOfTypeWithoutSimulationAgents<GameLogic>();
            }
            else
            {
                gameLogic = TimeStoneUtils.FindObjectOfTypeInSimulationAgents<GameLogic>(timeStone);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (timeAgent.IsClone)
                return;

            if (gameLogic.run)
            {
                Move(Time.deltaTime);
            }
        }

        private void SimulationUpdate(TimeStone stone, float deltaTime)
        {
            Move(deltaTime);
        }

        public void Init()
        {
            life = 10;
            dead = false;
        }

        public void TakeDamage(int damages)
        {
            Debug.Log("Minion " + name + " Takes <" + damages + "> damages");
            life -= damages;
            if (life <= 0)
            {
                dead = true;
                gameLogic.MinionDead(this);
            }
        }

        /// <summary>
        /// Move the agent every frame.
        /// BE CAREFUL here not to use Time.deltaTime or the simulation will be wrong, only use the argument deltaTime. 
        /// </summary>
        /// <param name="deltaTime">The delta time, </param>
        private void Move(float deltaTime)
        {
            if (!gameObject.activeSelf || dead)
                return;

            if (endPoint.position.z < transform.position.z)
            {
                gameLogic.MinionPassed(this);
            }
            else
            {
                var t = transform;
                t.position += t.forward * (deltaTime * speed);
            }
        }
    }
}