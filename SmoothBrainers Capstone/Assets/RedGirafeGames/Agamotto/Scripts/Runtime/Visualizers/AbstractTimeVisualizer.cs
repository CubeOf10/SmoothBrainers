using System;
using System.Runtime.InteropServices.WindowsRuntime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using UnityEditor;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Scripts.Runtime.Visualizers
{
    /// <summary>
    /// Base component to make <see cref="TimeStone"/>'s visualizers.
    /// <para>Override callbacks <see cref="OnSimulationStart"/>, <see cref="OnTimeLineChange"/>, etc. and use <see cref="visualizationEnabled"/> to code your visualizer</para>
    /// <para>See <see cref="PathVisualizer"/> for an example</para>
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(TimeAgent))]
    public class AbstractTimeVisualizer : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="TimeStone"/> visualized.
        /// <para>Unlike <see cref="TimeAgent"/>, a visualizer can only be used by one stone.</para>
        /// </summary>
        /// <remarks>
        /// <para>Use <see cref="autoAssignStone"/> to automatically define the stone from the first stone interacting with the <see cref="timeAgent"/></para>
        /// </remarks>
        [Header("Base config")] public TimeStone stone;

        /// <summary>
        /// Automatically assigns the <see cref="stone"/> from the first stone interacting with the <see cref="timeAgent"/>
        /// </summary>
        [Tooltip("Automatically assigns the timeStone from the first stone interacting with the timeAgent")]
        public bool autoAssignStone = true;

        /// <summary>
        /// The timeAgent. Should be present on the GameObject and will be automatically populated
        /// </summary>
        [HideInInspector] public TimeAgent timeAgent;

        /// <summary>
        /// The visualizer listens to <see cref="OnSimulationComplete"/> UnityEvent to update wen received.
        /// </summary>
        [Space] [Tooltip("The visualizer listens to OnSimulationComplete UnityEvent to update wen received.")]
        public bool autoUpdateOnSimulationComplete = true;

        /// <summary>
        /// The visualizer listens to <see cref="OnTimeLineChange"/> UnityEvent to update when received. This
        /// is mostly used to update during a recording
        /// </summary>
        [Tooltip(
            "The visualizer listens to OnTimeLineChange UnityEvent to update when received. This is mostly used to update during a recording")]
        public bool autoUpdateOnTimeLineChange = true;


        /// <summary>
        /// Log all messages
        /// </summary>
        [Space] public bool logDebug = false;

        /// <summary>
        /// Flag to know if the visualization must be visible. It will be set to false if critical data are missing, like the timeAgent or the timeStone for example.
        /// </summary>
        [Tooltip(
            "Flag to know if the visualization must be visible. It will be set to false if critical data are missing, like the timeAgent or the timeStone for example.")]
        public bool visualizationEnabled = true;

        /// <summary>
        /// Flag to know if the stone's listeners are defined
        /// </summary>
        private bool _hasStoneListeners;

        protected void Awake()
        {
            if (timeAgent == null)
            {
                timeAgent = GetComponent<TimeAgent>();
                // Listens on stone time agents initialization to auto assign if needed
                timeAgent.onInitTimeAgentsList.AddListener(OnStoneInitTimeAgentsList);
                timeAgent.onSetDataTick.AddListener(OnTimeAgentSetDataTick);
            }

            // Clone used in simulation should be disabled
            TryDisableClone();
        }

        /// <summary>
        /// Callback to TimeAgent's event InitTimeAgentsList
        /// Only used with <see cref="autoAssignStone"/> = true, to automatically set the TimeStone if it's not already defined
        /// The first stone to use this agent will be automatically assigned
        /// 
        /// </summary>
        /// <param name="eventStone"></param>
        public virtual void OnStoneInitTimeAgentsList(TimeStone eventStone)
        {
            if (isActiveAndEnabled && autoAssignStone && stone == null)
            {
                if (logDebug)
                    Debug.Log("[AbstractTimeVisualizer] " + name + " : Auto assigning stone <" + eventStone.name + ">");
                stone = eventStone;

                AddStoneListeners();

                UpdateVisualizationStatus();
            }

            timeAgent.onInitTimeAgentsList.RemoveListener(OnStoneInitTimeAgentsList);
        }

        /// <summary>
        /// Callback to TimeAgent's event OnSetDataTick
        /// Can be useful if the visualizer is dependent to the current tick. See OnionSkinVisualizer for example.
        /// </summary>
        /// <param name="eventStone"></param>
        /// <param name="tick"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void OnTimeAgentSetDataTick(TimeStone eventStone, int tick)
        {
            
        }

        private void OnEnable()
        {
            AddStoneListeners();
        }

        private void OnDisable()
        {
            RemoveStoneListeners();
        }

        /// <summary>
        /// Add listeners to the TimeStone
        /// </summary>
        public virtual void AddStoneListeners()
        {
            if (_hasStoneListeners || stone == null || timeAgent == null || timeAgent.IsClone)
                return;

            stone.onInitTimeAgentsList.AddListener(OnInitTimeAgentsList);

            if (autoUpdateOnSimulationComplete)
            {
                stone.onSimulationStart.AddListener(OnSimulationStart);
                stone.onSimulationComplete.AddListener(OnSimulationComplete);
            }

            if (autoUpdateOnTimeLineChange)
            {
                stone.onRecordStart.AddListener(OnRecordStart);
                stone.onRecordStop.AddListener(OnRecordStop);
                stone.onTimeLineChange.AddListener(OnTimeLineChange);
                stone.onTimeLineClear.AddListener(OnTimeLineClear);
            }

            _hasStoneListeners = true;
        }

        /// <summary>
        /// Remove listeners from the TimeStone
        /// </summary>
        public virtual void RemoveStoneListeners()
        {
            if (!_hasStoneListeners || stone == null)
                return;

            stone.onInitTimeAgentsList.RemoveListener(OnInitTimeAgentsList);

            if (autoUpdateOnSimulationComplete)
            {
                stone.onRecordStart.RemoveListener(OnRecordStart);
                stone.onRecordStop.RemoveListener(OnRecordStop);
                stone.onSimulationStart.RemoveListener(OnSimulationStart);
                stone.onSimulationComplete.RemoveListener(OnSimulationComplete);
            }

            if (autoUpdateOnTimeLineChange)
            {
                stone.onTimeLineChange.RemoveListener(OnTimeLineChange);
                stone.onTimeLineClear.RemoveListener(OnTimeLineClear);
            }

            _hasStoneListeners = false;
        }

        /// <summary>
        /// Visualizer is only used by the Original Time Agent.
        /// But when a TimeAgent is cloned for simulation, its Visualizers components are cloned with it.
        /// So if the time agent is a clone (used by simulation), visualizer is disabled to optimize
        /// </summary>
        public virtual void TryDisableClone()
        {
            if (timeAgent == null || !timeAgent.IsClone)
            {
                return;
            }

            enabled = false;
            visualizationEnabled = false;
            RemoveStoneListeners();
        }

        /// <summary>
        /// Sets flag <see cref="visualizationEnabled"/> in case the visualizer misses a critical data to be displayed.
        /// This flag must be used in the Component extending AbstractTimeVisualizer
        /// </summary>
        public virtual void UpdateVisualizationStatus()
        {
            visualizationEnabled = stone != null && timeAgent != null && stone.HasTimeAgent(timeAgent) &&
                                   !timeAgent.IsClone;
        }

        /// <summary>
        /// Callback to TimeStone's event InitTimeAgentsList
        /// </summary>
        public virtual void OnInitTimeAgentsList()
        {
            UpdateVisualizationStatus();
        }


        /// <summary>
        /// Callback to TimeStone's event OnRecordStart
        /// </summary>
        public virtual void OnRecordStop()
        {
            if (logDebug) Debug.Log("[AbstractTimeVisualizer] " + name + " : On Record Stop");
        }

        /// <summary>
        /// Callback to TimeStone's event OnRecordStop
        /// </summary>
        public virtual void OnRecordStart()
        {
            if (logDebug) Debug.Log("[AbstractTimeVisualizer] " + name + " : On Record Start");
        }

        /// <summary>
        /// Callback to TimeStone's event OnSimulationStart
        /// </summary>
        public virtual void OnSimulationStart()
        {
            if (logDebug) Debug.Log("[AbstractTimeVisualizer] " + name + " : On Simulation start");
            // Disable can't be made only in Awake, because if the clone GameObject was disabled, it didn't receive the Awake callback
            TryDisableClone();
        }

        /// <summary>
        /// Callback to TimeStone's event OnTimeLineChange
        /// </summary>
        /// <param name="origin">The origin of the change, Record or Simulation</param>
        public virtual void OnTimeLineChange(TimeStone.TimeTickOrigin origin)
        {
            if (logDebug)
                Debug.Log("[AbstractTimeVisualizer] " + name + " : On Time Line change from <" + origin + ">");
        }

        /// <summary>
        /// Callback to TimeStone's event OnTimeLineClear
        /// </summary>
        public virtual void OnTimeLineClear()
        {
            if (logDebug)
                Debug.Log("[AbstractTimeVisualizer] " + name + " : On Time Line clear");
        }

        /// <summary>
        /// Callback to TimeStone's event OnSimulationComplete
        /// </summary>
        public virtual void OnSimulationComplete()
        {
            if (logDebug)
                Debug.Log("[AbstractTimeVisualizer] " + name + " : On Simulation complete");
        }

        /// <summary>
        /// Base method to clear this visualizer.
        /// Override in extending component.
        /// </summary>
        /// <param name="fromIndex"></param>
        public virtual void ClearVisualization(int fromIndex = 0)
        {
            if (logDebug)
                Debug.Log("[AbstractTimeVisualizer] " + name + " : Clear Visualization from <" + fromIndex + ">");
        }
    }
}