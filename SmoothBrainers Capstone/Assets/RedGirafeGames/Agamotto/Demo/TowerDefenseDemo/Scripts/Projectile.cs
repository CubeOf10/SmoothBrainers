using System;
using System.Collections.Generic;
using System.Linq;
using RedGirafeGames.Agamotto.Scripts.Runtime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using RedGirafeGames.Agamotto.Scripts.Runtime.Utils;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Demo.TowerDefenseDemo.Scripts
{
    /// <summary>
    /// A projectile physics driven
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        public int damage = 5;
        public float damageDistance = 1f;

        public GameLogic gameLogic;
        public TimeStone timeStone;
        public Rigidbody rb;
        [HideInInspector] public TimeAgent timeAgent;

        // Start is called before the first frame update
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            timeStone = FindObjectOfType<TimeStone>();
            
            timeAgent = GetComponent<TimeAgent>();

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

        public void Init()
        {
        }

        public void Throw(Vector3 throwVelocity)
        {
            rb.velocity = throwVelocity;
        }

        /// <summary>
        /// Hit all minions in damage area
        /// </summary>
        /// <param name="other"></param>
        private void OnCollisionEnter(Collision other)
        {
            // gameLogic.minionsActives is modified when a minion dies, so we will have access exception if we make damages
            // from its loop
            List<GameObject> touchedMinions = new List<GameObject>();
            foreach (var activeMinion in gameLogic.minionsPool.Where(m => m.activeSelf))
            {
                if (Vector3.Distance(transform.position, activeMinion.transform.position) < damageDistance)
                {
                    touchedMinions.Add(activeMinion);
                }
            }

            foreach (var touchedMinion in touchedMinions)
            {
                touchedMinion.GetComponent<Minion>().TakeDamage(damage);
            }

            gameLogic.ReturnProjectileInPool(this.gameObject);
        }
    }
}