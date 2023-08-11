using System;
using System.Linq;
using RedGirafeGames.Agamotto.Scripts.Runtime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using RedGirafeGames.Agamotto.Scripts.Runtime.Utils;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Demo.TowerDefenseDemo.Scripts
{
    public class Tower : MonoBehaviour
    {
        public float speed = 1;

        public GameLogic gameLogic;
        public TimeStone timeStone;
        [HideInInspector] public TimeAgent timeAgent;

        public Transform projectileStartPoint;

        public float attackTimer = 0f;

        // Start is called before the first frame update
        void Awake()
        {
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
            Init();
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
                Attack(Time.deltaTime);
            }
        }

        private void SimulationUpdate(TimeStone stone, float deltaTime)
        {
            Attack(deltaTime);
        }

        public void Init()
        {
            // Debug.Log("Tower init " + name);
            attackTimer = 0;
        }

        /// <summary>
        /// Try to attack
        /// BE CAREFUL here not to use Time.deltaTime or the simulation will be wrong, only use the parameter deltaTime. 
        /// </summary>
        /// <param name="deltaTime">The delta time, </param>
        private void Attack(float deltaTime)
        {
            attackTimer += deltaTime;
            
            if (!gameObject.activeSelf)
                return;

            if (attackTimer < speed)
                return;

            var projectile = gameLogic.GetProjectileFromPool();
            projectile.transform.position = projectileStartPoint.position;
            projectile.GetComponent<Projectile>().Throw((transform.forward + new Vector3(0, 2, 0)) * 1.3f);
    
            attackTimer = 0;
        }
    }
}