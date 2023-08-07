using System.Collections.Generic;
using System.Linq;
using RedGirafeGames.Agamotto.Scripts.Runtime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using UnityEngine;
using UnityEngine.UI;

namespace RedGirafeGames.Agamotto.Demo.TowerDefenseDemo.Scripts
{
    public class GameLogic : MonoBehaviour
    {
        public bool run;

        public TimeStone timeStone;
        [HideInInspector] public TimeAgent timeAgent;
        [HideInInspector] public Transform startPoint;
        [HideInInspector] public Button runButton;
        [HideInInspector] public Button simButton;

        [HideInInspector] public Button addTowerButton;
        [HideInInspector] public Text runResultText;
        [HideInInspector] public Text simResultText;

        [Header("Minions")] public int minionsCount = 5;
        public Transform minionsPoolParent;
        public List<GameObject> minionsPool = new List<GameObject>();
        public GameObject minionPrefab;

        public float minionSpawnTimer;
        public float minionSpawnInterval = 1.0f;
        public int minionsSpawned;
        public int minionsDead;
        public int minionsPassed;

        [Header("Towers")] public int towersCount = 4;
        public Transform towersPoolParent;
        public List<GameObject> towersPool = new List<GameObject>();
        public GameObject towerPrefab;
        public int towersCreated;

        [Header("Projectiles")] public int projectilesCount = 50;
        public Transform projectilesPoolParent;
        public List<GameObject> projectilesPool = new List<GameObject>();
        public GameObject projectilePrefab;

        public const string minionsSpawnedDataId = "minionsSpawned";
        public const string minionsDeadDataId = "minionsDead";
        public const string minionsPassedDataId = "minionsPassed";
        public const string minionTimerDataId = "minionSpawnTimer";

        // Start is called before the first frame update
        void Start()
        {
            startPoint = GameObject.Find("Start").transform;
            runButton = GameObject.Find("RunButton").GetComponent<Button>();
            simButton = GameObject.Find("SimButton").GetComponent<Button>();
            addTowerButton = GameObject.Find("AddTower").GetComponent<Button>();
            runResultText = GameObject.Find("RunResultText").GetComponent<Text>();
            simResultText = GameObject.Find("SimResultText").GetComponent<Text>();
            timeStone = FindObjectOfType<TimeStone>();

            timeAgent = GetComponent<TimeAgent>();
            timeAgent.onInitTimeAgentsList.AddListener(InitTimeAgentsList);
            timeAgent.onSimulationStart.AddListener(SimulationStart);
            timeAgent.onSimulationUpdate.AddListener(SimulationUpdate);
            timeAgent.onSimulationComplete.AddListener(SimulationComplete);
            timeAgent.onPersistTick.AddListener(PersistTick);
            timeAgent.onSetDataTick.AddListener(SetDataTick);

            // Those are useless actions on simulations clones
            // Even more, if we'd add onClick listeners from clone, they'd be triggered twice
            if (!timeAgent.IsClone)
            {
                runButton.onClick.AddListener(Run);
                simButton.onClick.AddListener(Simulate);
                addTowerButton.onClick.AddListener(AddTower);

                // Init minions pool
                for (var i = 0; i < minionsCount; i++)
                {
                    var minion = Instantiate(minionPrefab, minionsPoolParent);
                    minionsPool.Add(minion);
                    ReturnMinionInPool(minion);
                }

                // Init towers pool
                for (var i = 0; i < towersCount; i++)
                {
                    var tower = Instantiate(towerPrefab, towersPoolParent);
                    towersPool.Add(tower);
                    ReturnTowerInPool(tower);
                }

                // Init projectiles pool
                for (var i = 0; i < projectilesCount; i++)
                {
                    var projectile = Instantiate(projectilePrefab, projectilesPoolParent);
                    projectilesPool.Add(projectile);
                    ReturnProjectileInPool(projectile);
                }
            }
        }

        private void InitTimeAgentsList(TimeStone arg0)
        {
            // Freeze time agents so that we can travel in time using editor's controller safely
            timeStone.FreezeTimeAgents(true);
        }

        private void AddTower()
        {
            if (towersCreated >= towersCount)
                return;

            var tower = GetTowerFromPool();
            tower.transform.position = new Vector3(tower.transform.position.x, tower.transform.position.y,
                tower.transform.position.z + towersCreated);
            towersCreated++;
        }


        private void SimulationStart(TimeStone stone)
        {
            minionsPassed = 0;
            minionsDead = 0;
            minionsSpawned = 0;
            minionSpawnTimer = 0;

            // Updates simulation pools with simulation time agents instead of real ones
            var simulationPool = new List<GameObject>();
            foreach (var originalMinion in minionsPool)
            {
                var ta = originalMinion.GetComponent<TimeAgent>();
                if (ta.SimulationClone != null)
                    simulationPool.Add(originalMinion.GetComponent<TimeAgent>().SimulationClone.gameObject);
            }

            minionsPool = simulationPool;

            simulationPool = new List<GameObject>();
            foreach (var originalTower in towersPool)
            {
                var ta = originalTower.GetComponent<TimeAgent>();
                if (ta.SimulationClone != null)
                    simulationPool.Add(originalTower.GetComponent<TimeAgent>().SimulationClone.gameObject);
            }

            towersPool = simulationPool;

            simulationPool = new List<GameObject>();
            foreach (var originalProjectile in projectilesPool)
            {
                var ta = originalProjectile.GetComponent<TimeAgent>();
                if (ta.SimulationClone != null)
                    simulationPool.Add(originalProjectile.GetComponent<TimeAgent>().SimulationClone.gameObject);
            }

            projectilesPool = simulationPool;
        }

        private void SimulationComplete(TimeStone stone)
        {
            Debug.Log("Simulation Complete.");
            ToggleUI();
        }

        // Update is called once per frame
        void Update()
        {
            if (run)
            {
                runResultText.text = "Run Result : Killed = " + minionsDead + " / Passed = " + minionsPassed;
                if (CheckGameOver())
                {
                    StopRun();
                }
                else
                    SpawnMinion(Time.deltaTime);
            }
        }

        private void SimulationUpdate(TimeStone stone, float deltaTime)
        {
            simResultText.text = "Simulation Result : Killed = " + minionsDead + " / Passed = " + minionsPassed;
            if (CheckGameOver())
            {
                timeStone.StopSimulation();
            }
            else
                SpawnMinion(deltaTime);
        }

        /// <summary>
        /// Persist custom data in the TimeStone.
        /// Alternatively GenericTimeAgent could be used, but performances will be better using onPersistTick event because
        /// we access members without using Reflection.
        /// </summary>
        /// <param name="stone"></param>
        /// <param name="origin"></param>
        /// <param name="deltaTime"></param>
        private void PersistTick(TimeStone stone, TimeStone.TimeTickOrigin origin, float deltaTime)
        {
            stone.PersistData(minionsDeadDataId, minionsDead);
            stone.PersistData(minionsSpawnedDataId, minionsSpawned);
            stone.PersistData(minionsPassedDataId, minionsPassed);
            stone.PersistData(minionTimerDataId, minionSpawnTimer);
        }

        /// <summary>
        /// Apply custom data when TimeStone's TimeLine is modified
        /// </summary>
        /// <param name="stone"></param>
        /// <param name="tickIndex"></param>
        private void SetDataTick(TimeStone stone, int tickIndex)
        {
            var minionsDeadData = stone.GetDataValue<int>(minionsDeadDataId, out var dataExists);
            if (dataExists)
                minionsDead = minionsDeadData;
            var minionsSpawnedData = stone.GetDataValue<int>(minionsSpawnedDataId, out dataExists);
            if (dataExists)
                minionsSpawned = minionsSpawnedData;
            var minionsPassedData = stone.GetDataValue<int>(minionsPassedDataId, out dataExists);
            if (dataExists)
                minionsPassed = minionsPassedData;
            var minionSpawnTimerData = stone.GetDataValue<float>(minionTimerDataId, out dataExists);
            if (dataExists)
                minionSpawnTimer = minionSpawnTimerData;
        }


        private bool CheckGameOver()
        {
            if (minionsSpawned < minionsCount)
                return false;

            if (minionsDead + minionsPassed == minionsCount)
            {
                return true;
            }

            return false;
        }

        public void SpawnMinion(float deltaTime)
        {
            minionSpawnTimer += deltaTime;

            if (minionsSpawned == minionsCount || minionSpawnTimer < minionSpawnInterval)
                return;

            // Take first inactive

            var minion = GetMinionFromPool();
            minion.transform.position = startPoint.position;

            minionsSpawned++;
            minionSpawnTimer = 0;
        }


        public void MinionPassed(Minion minion)
        {
            minionsPassed++;
            ReturnMinionInPool(minion.gameObject);
        }

        public void MinionDead(Minion minion)
        {
            minionsDead++;
            ReturnMinionInPool(minion.gameObject);
        }

        public GameObject GetMinionFromPool()
        {
            var minion = minionsPool.Find(m => !m.activeSelf);
            minion.SetActive(true);
            return minion;
        }

        public void ReturnMinionInPool(GameObject minion)
        {
            minion.SetActive(false);
            minion.GetComponent<Minion>().Init();
        }

        public GameObject GetTowerFromPool()
        {
            var tower = towersPool.Find(t => !t.activeSelf);
            tower.SetActive(true);
            return tower;
        }

        public void ReturnTowerInPool(GameObject tower)
        {
            tower.SetActive(false);
            tower.GetComponent<Tower>().Init();
        }

        public GameObject GetProjectileFromPool()
        {
            var projectile = projectilesPool.Find(p => !p.activeSelf);
            projectile.SetActive(true);
            return projectile;
        }

        public void ReturnProjectileInPool(GameObject projectile)
        {
            projectile.SetActive(false);
            projectile.GetComponent<Projectile>().Init();
        }

        public void Run()
        {
            Debug.Log("Start run.");
            run = true;
            timeStone.FreezeTimeAgents(false);
            minionSpawnTimer = 0;
            minionsDead = 0;
            minionsSpawned = 0;
            minionsPassed = 0;
            foreach (var tower in towersPool.Where(t => t.activeSelf))
            {
                tower.GetComponent<Tower>().Init();
            }

            ToggleUI();
        }

        private void StopRun()
        {
            Debug.Log("Run finished.");
            foreach (var projectile in projectilesPool)
            {
                ReturnProjectileInPool(projectile);
            }

            // Freeze time agents so that we can travel in time using editor's controller safely
            timeStone.FreezeTimeAgents(true);
            run = false;
            ToggleUI();
            runResultText.text = "Run Result : Killed = " + minionsDead + " / Passed = " + minionsPassed;
        }

        public void Simulate()
        {
            Debug.Log("Simulation start.");
            timeStone.FreezeTimeAgents(true);
            run = false;
            ToggleUI();
            timeStone.Clear();
            timeStone.StartSimulation(true);
        }

        public void ToggleUI()
        {
            runButton.interactable = !runButton.interactable;
            simButton.interactable = !simButton.interactable;
            addTowerButton.interactable = !addTowerButton.interactable;
        }
    }
}