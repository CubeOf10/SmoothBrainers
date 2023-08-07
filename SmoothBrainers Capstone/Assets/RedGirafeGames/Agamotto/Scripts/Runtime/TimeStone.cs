using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using RedGirafeGames.Agamotto.Scripts.Runtime.TimeData;
using RedGirafeGames.Agamotto.Scripts.Runtime.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace RedGirafeGames.Agamotto.Scripts.Runtime
{
    /// <summary>
    /// The heart (or the eye...) of Agamotto!
    /// <para>The <see cref="TimeStone"/> is managing data though time for its <see cref="TimeAgent"/> list</para>
    /// <para>The <see cref="TimeStone"/> can record to keep past data or run simulation to generate future data</para>
    /// <para>The <see cref="TimeStone"/> can also playback its data on its <see cref="TimeAgent"/> list</para>
    /// </summary>
    /// <remarks>
    /// <para>You can have multiple stones in the scene</para>
    /// <para>Use classes derived from <see cref="RedGirafeGames.Agamotto.Scripts.Runtime.Visualizers.AbstractTimeVisualizer"/> to visualize TimeStone's data</para>
    /// </remarks>
    [AddComponentMenu("Agamotto/Time Stone")]
    public class TimeStone : MonoBehaviour
    {
        /// <summary>
        /// Activates log messages (except performances messages)
        /// </summary>
        public bool logDebug;

        /// <summary>
        /// Activates performances messages
        /// </summary>
        public bool logPerfs;

        /// <summary>
        /// How new time data is added if there is existing one
        /// </summary>
        public enum TimeLineAddTickBehaviour
        {
            /// <summary>
            /// All data after current cursor position will be removed on adding new one (default behaviour)
            /// This is the most logical behaviour because if you create new data, it is likely that following data are no longer valid 
            /// </summary>
            ClearAfter,

            /// <summary>
            /// New data will be inserted between the previous and the following data
            /// </summary>
            Insert,

            /// <summary>
            /// New data will replace following data at the same index (regardless of the deltaTime, so be careful about the result)
            /// </summary>
            Replace
        }

        /// <summary>
        /// The origin of the time data persisted
        /// </summary>
        public enum TimeTickOrigin
        {
            /// <summary> Time data has been created during recording</summary>
            Record,

            /// <summary> Time data has been created during a simulation</summary>
            Simulation
        }

        /// <summary>
        /// The time scaling used during recording
        /// </summary>
        public enum TimeScale
        {
            /// <summary> Uses <see cref="Time.deltaTime"/></summary>
            ScaledTime,

            /// <summary> Uses <see cref="Time.unscaledDeltaTime"/></summary>
            UnscaledTime
        }

        /// <summary>
        /// The way the time agents list is initialized.
        /// The list is initialized on <see cref="TimeStone.Start"/>, on <see cref="TimeStone.StartSimulation"/> with the parameter set to true or manually calling
        /// <see cref="TimeStone.InitTimeAgentsList"/>
        /// </summary>
        public enum TimeAgentsListInitMode
        {
            /// <summary> The list is defined manually, by code modifing <see cref="TimeStone.timeAgentsManualInitList"/> or directly in the editor</summary>
            Manual,

            /// <summary> The list is getting all <see cref="TimeAgent"/> that have the tag <see cref="TimeStone.timeAgentsListInitTag"/></summary>
            Tag,

            /// <summary> The list is getting all <see cref="TimeAgent"/> that have the layer <see cref="TimeStone.timeAgentsListInitLayer"/></summary>
            Layer
        }

        /// <summary>
        /// Keeps the scene created for the last simulation alive.
        /// Otherwise, the scene is destroyed when the simulation end.
        /// A new scene is created for each simulation, even if this property is true
        /// </summary>
        public bool keepSimulationScene;

        /// <summary>
        /// Should the simulations execute the TimeAgents simulation life cycle callbacks (SimulationUpdate, SimulationComplete, etc.)
        /// </summary>
        public bool simulateUpdate = true;

        /// <summary>
        /// Should the simulations run the physics simulation on time agents
        /// </summary>
        public bool simulatePhysics = true;

        /// <summary>
        /// A simulation is automatically started on <see cref="TimeAgent"/>
        /// </summary>
        public bool simulateOnStart;

        /// <summary>
        /// <para>The current tick index used by the stone to add new data.</para>
        /// <para>If new data are generated by record or simulation, they will be added from this index in the <see cref="timeLine"/>.</para>
        /// <para>You can get corresponding time using <see cref="GetTimeAtTickIndex"/></para>
        /// </summary>
        public int addTickCursor;

        /// <summary>
        /// <para>The current tick index used to playback the TimeLine.</para>
        /// <para>It is also useful to use with a TimeSlider to control what index of the TimeLine to display</para>
        /// <para>You can get corresponding time using <see cref="GetTimeAtTickIndex"/></para>
        /// </summary>
        public int playbackTickCursor;

        /// <summary>
        /// If true, the <see cref="playbackTickCursor"/> is synchronized with the recording state, so that when recording is stopped,
        /// the timeline is synchronized with the time agents states
        /// </summary>
        public bool updatePlayTickCursorOnRecord = true;

        /// <summary>
        /// If true, the <see cref="addTickCursor"/> will be placed at the simulation start tick when simulation ends.
        /// </summary>
        public bool resetAddTickCursorOnSimulationComplete = true;

        /// <summary>
        /// The selected behaviour on time line modification
        /// </summary>
        public TimeLineAddTickBehaviour timeLineAddTickBehaviour = TimeLineAddTickBehaviour.ClearAfter;

        /// <summary>
        /// The simulation maximum duration if <see cref="simulationHasDuration"/> is true.
        /// </summary>
        public float simulationDuration = 3.0f;

        /// <summary>
        /// Defines if the simulation has a maximum duration.
        /// If not defined, simulation must be stopped using <see cref="StopSimulation"/>.
        /// </summary>
        public bool simulationHasDuration = true;

        /// <summary>
        /// The time since the current simulation started
        /// </summary>
        public float simulationTime;

        /// <summary>
        /// The tick index when current simulation started
        /// </summary>
        public int simulationStartTick;

        /// <summary>
        /// The time scale used to execute the simulation
        /// </summary>
        public TimeScale simulationTimeScale = TimeScale.ScaledTime;

        /// <summary>
        /// The simulation speed.
        /// Example : Value of 2.0 will run the simulation twice as fast as real time
        /// <para>This value can be limited by <see cref="simulationMaxMsByFrame"/></para>
        /// </summary>
        public float simulationSpeed = 1.0f;

        /// <summary>
        /// The maximum number of milliseconds consumed by the simulation every frame.
        /// The property let you limit the impact of the simulation on the framerate.
        /// </summary>
        public float simulationMaxMsByFrame = 5.0f;

        /// <summary>
        /// Simulation execution will use <see cref="Time.deltaTime"/> or <see cref="Time.unscaledDeltaTime"/>.
        /// If <see cref="simulationSpeed"/> is higher than 1.0, every simulation tick during a frame will use the same deltaTime.
        /// If false, Simulation uses <see cref="simulationUpdateStep"/>;
        /// </summary>
        public bool simulationUseUpdateDeltaTime = true;

        /// <summary>
        /// Delta time used during a simulation between frames if <see cref="simulationUseUpdateDeltaTime"/> is false
        /// </summary>
        public float simulationUpdateStep = 0.005f;

        /// <summary>
        /// If <see cref="simulationSpeed"/> is higher than 1.0, every simulation tick that should be made during a single frame
        /// will be grouped into one (with adapted delta time).
        /// This option makes the simulation faster but lowers the precision.
        /// </summary>
        public bool simulationGroupFrameTicks;

        /// <summary>
        /// The maximum record duration.
        /// If the TimeLine's duration is higher than this value during recording, oldest data are removed when adding new ones
        /// </summary>
        public float recordDuration = 15.0f;

        /// <summary>
        /// Activates recording on <see cref="Start"/>
        /// If <see cref="simulateOnStart"/> is true, recordOnStart has no effect (because starting a simulation stops recording)
        /// </summary>
        public bool recordOnStart;

        /// <summary>
        /// The time since current recording started.
        /// </summary>
        public float recordTime;

        /// <summary>
        /// The timescale used during recording
        /// </summary>
        public TimeScale recordingTimeScale = TimeScale.ScaledTime;

        /// <summary>
        /// Minimum delta time between record data, 0 or less will record on each <see cref="Update"/> call (which while giving you great precision, will greatly impact performances)
        /// </summary>
        public float recordingStep = 0.005f;

        /// <summary>
        /// Is the stone recording. Use <see cref="StartRecording"/> and <see cref="StopRecording"/> to modify
        /// </summary>
        public bool Recording { get; private set; }

        private bool _simulating;

        /// <summary>
        /// Is the stone simulating. Use <see cref="StartSimulation"/> and <see cref="StopSimulation"/> to modify
        /// </summary>
        public bool Simulating
        {
            get => _simulating && !_stopSimulation;
            private set => _simulating = value;
        }

        /// <summary>
        /// Simulation will use <see cref="Time.fixedDeltaTime"/> for physics simulation's step 
        /// </summary>
        public bool simulationUseUnityFixedDeltaTime = true;

        /// <summary>
        /// If <see cref="simulationUseUnityFixedDeltaTime"/> is false, defines the step physics simulation uses
        /// </summary>
        public float simulationPhysicsStep = 0.02f;

        /// <summary>
        /// Current simulated frame time stack
        /// </summary>
        public float frameTimeStack;

        /// <summary>
        /// The initialization mode for the Time Agents.
        /// </summary>
        public TimeAgentsListInitMode timeAgentsListInitMode = TimeAgentsListInitMode.Manual;

        /// <summary>
        /// The initialization list of the time agent.
        /// Different from the final time agent list because if <see cref="TimeAgent.parseChildren"/> is activated, new
        /// time agents will be added.
        /// </summary>
        public List<TimeAgent> timeAgentsManualInitList = new List<TimeAgent>();

        /// <summary>
        /// The tag used if the initialization mode is <see cref="TimeAgentsListInitMode.Tag"/>
        /// </summary>
        public string timeAgentsListInitTag = "";

        /// <summary>
        /// The layer's name used if the initialization mode is <see cref="TimeAgentsListInitMode.Layer"/>
        /// </summary>
        public string timeAgentsListInitLayer = "";

        /// <summary>
        /// Is the stone's playback active.
        /// </summary>
        public bool playback;

        /// <summary>
        /// The time scale used to playback stone's data
        /// </summary>
        public TimeScale playbackTimeScale = TimeScale.ScaledTime;

        /// <summary>
        /// The playback's speed
        /// </summary>
        public float playbackSpeed = 1.0f;

        /// <summary>
        /// If true, using playback will update the addTickCursor so that it's synchronized with the playbackTickCursor.
        /// As a result, if you start recording or make a simulation after a playback, it will start where you stopped it.
        /// </summary>
        public bool playbackUpdateAddTickCursor = true;

        /// <summary>
        /// The final list of time agents used by the stone.
        /// If you manage your TimeAgents manually, you can't add them directly in this list for simulations.
        /// You have to add them in <see cref="timeAgentsManualInitList"/> and then call <see cref="onInitTimeAgentsList"/> or use
        /// <see cref="StartSimulation"/> with the right parameter to initialize simulation agents
        /// </summary>
        public List<TimeAgent> timeAgents = new List<TimeAgent>();

        /// <summary>
        /// The list of time agents, but without the children searched recursively if <see cref="TimeAgent.parseChildren"/>
        /// is true on some TimeAgents.
        /// </summary>
        public List<TimeAgent> timeAgentsNoChildren = new List<TimeAgent>();

        /// <summary>
        /// The list of simulation time agents generated when <see cref="CreateSimulationScene"/> is called.
        /// </summary>
        public List<TimeAgent> simulationTimeAgents = new List<TimeAgent>();

        /// <summary>
        /// The TimeLine holding the time agents data over time.
        /// </summary>
        public readonly TimeLine timeLine = new TimeLine();

        /// <summary>
        /// Flag that makes sure the very first frame recorded is the frame 0 (and not the the frame 0 + recordingStep) 
        /// </summary>
        private bool _firstRecord = true;

        /// <summary>
        /// Seconds since last recorded tick
        /// </summary>
        private float _secondsSinceLastRecord;

        private bool _initSimulation;
        private bool _initSimulationUpdateAgentsList;

        /// <summary>
        /// Flag indicating that <see cref="StartSimulation"/> was called and a simulation must be executed
        /// </summary>
        private bool _startSimulation;

        /// <summary>
        /// Flag indicating that <see cref="StopSimulation"/> was called and the simulation must be stopped
        /// We don't stop it immediately to finish current frame 
        /// </summary>
        private bool _stopSimulation;

        /// <summary>
        /// Flag used to have different behaviour on the first tick to initialize the simulation.
        /// </summary>
        private bool _simulationDoInit;

        /// <summary>
        /// The frame count when <see cref="StartSimulation"/> is called, used to make sure the simulation is really started
        /// in a different frame so that agents could have there MonoBehaviour's callback (Awake, Start) executed
        /// </summary>
        private int _startFrameCount;

        /// <summary>
        /// The TimeWatch used to get simulations performances
        /// </summary>
        private Stopwatch _simulationTimeWatch;

        /// <summary>
        /// Time since the simulation started.
        /// </summary>
        private float _simulationTimeSinceStartElapsed;

        /// <summary>
        /// Is playback currently active
        /// </summary>
        private bool _playbackActive;

        /// <summary>
        /// The time the playback started. Used to play ticks respecting delta times
        /// </summary>
        private float _playbackStartTime;

        /// <summary>
        /// Time elapsed since playback started
        /// </summary>
        private float _playbackElapsedTimer;

        /// <summary>
        /// Updated during <see cref="PersistTick"/> so that <see cref="PersistData{T}(string,T)"/> can be called without this parameter from time agents
        /// </summary>
        private TimeTickOrigin currentPersistTickOrigin;

        /// <summary>
        /// Updated during <see cref="PersistTick"/> so that <see cref="PersistData{T}(string,T)"/> can be called without this parameter from time agents
        /// </summary>
        private float currentPersistTickDeltaTime;

        /// <summary>
        /// Updated during <see cref="PersistTick"/> so that <see cref="PersistData{T}(string,T)"/> can be called without this parameter from time agents
        /// </summary>
        private TimeAgent currentPersistTickAgent;

        /// <summary>
        /// Updated during <see cref="SetPlaybackTick"/> so that <see cref="GetDataValue{T}"/> can be called without this parameter from time agents
        /// </summary>
        private int currentSetDataTickIndex;

        /// <summary>
        /// Updated during <see cref="SetPlaybackTick"/> so that <see cref="GetDataValue{T}"/> can be called without this parameter from time agents
        /// </summary>
        private TimeAgent currentSetDataAgent;

        /// <summary>
        /// The scene created for the simulation.
        /// A new one is created everytime
        /// </summary>
        public Scene simulationScene;

        /// <summary>
        /// The physics scene of the <see cref="simulationScene"/>
        /// </summary>
        public PhysicsScene simulationPhysicsScene;

        /// <summary>
        /// Flag when simulation is complete
        /// </summary>
        public bool SimulationComplete { get; private set; }

        [FormerlySerializedAs("onInitTimeAgentsList")] [SerializeField]
        private OnInitTimeAgentsListEvent _onInitTimeAgentsList = new OnInitTimeAgentsListEvent();

        /// <summary>
        /// UnityEvent when stone has initialized its TimeAgent list with its initialization mode.
        /// This is the good event to use to initialize things if the stone is making record and simulation as it is called in both cases.
        /// </summary>
        public OnInitTimeAgentsListEvent onInitTimeAgentsList
        {
            get => _onInitTimeAgentsList;
            set => _onInitTimeAgentsList = value;
        }

        [FormerlySerializedAs("onSimulationSceneReady")] [SerializeField]
        private OnSimulationSceneReadyEvent _onSimulationSceneReady = new OnSimulationSceneReadyEvent();

        /// <summary>
        /// UnityEvent when the scene for the simulation is created and the time agents cloned 
        /// </summary>
        public OnSimulationSceneReadyEvent onSimulationSceneReady
        {
            get => _onSimulationSceneReady;
            set => _onSimulationSceneReady = value;
        }

        [FormerlySerializedAs("onTimeLineChange")] [SerializeField]
        private OnTimeLineClearEvent _onTimeLineClear = new OnTimeLineClearEvent();

        /// <summary>
        /// UnityEvent when the TimeLine is cleared by calling <see cref="Clear"/>
        /// </summary>
        public OnTimeLineClearEvent onTimeLineClear
        {
            get => _onTimeLineClear;
            set => _onTimeLineClear = value;
        }

        [FormerlySerializedAs("onTimeLineChange")] [SerializeField]
        private OnTimeLineChangeEvent _onTimeLineChange = new OnTimeLineChangeEvent();

        /// <summary>
        /// UnityEvent when timeLine is modified.
        /// For recording, this is called on every recorded tick.
        /// For simulation, this is called on every frame (multiples ticks simulated on the same frame don't trigger).
        /// </summary>
        public OnTimeLineChangeEvent onTimeLineChange
        {
            get => _onTimeLineChange;
            set => _onTimeLineChange = value;
        }

        [FormerlySerializedAs("onSimulationStart")] [SerializeField]
        private OnSimulationStartEvent _onSimulationStart = new OnSimulationStartEvent();

        /// <summary>
        /// UnityEvent when the simulation is starting to simulate
        /// </summary>
        public OnSimulationStartEvent onSimulationStart
        {
            get => _onSimulationStart;
            set => _onSimulationStart = value;
        }

        [FormerlySerializedAs("onSimulationComplete")] [SerializeField]
        private OnSimulationCompleteEvent _onSimulationComplete = new OnSimulationCompleteEvent();

        /// <summary>
        /// UnityEvent when simulation is complete
        /// </summary>
        public OnSimulationCompleteEvent onSimulationComplete
        {
            get => _onSimulationComplete;
            set => _onSimulationComplete = value;
        }

        [FormerlySerializedAs("onRecordStart")] [SerializeField]
        private OnRecordStartEvent _onRecordStart = new OnRecordStartEvent();

        /// <summary>
        /// UnityEvent when record starts
        /// </summary>
        public OnRecordStartEvent onRecordStart
        {
            get => _onRecordStart;
            set => _onRecordStart = value;
        }

        [FormerlySerializedAs("onRecordStop")] [SerializeField]
        private OnRecordStopEvent _onRecordStop = new OnRecordStopEvent();

        /// <summary>
        /// UnityEvent when record stops
        /// </summary>
        public OnRecordStopEvent onRecordStop
        {
            get => _onRecordStop;
            set => _onRecordStop = value;
        }

        private void Start()
        {
            InitTimeAgentsList();

            if (recordOnStart)
            {
                StartRecording();
            }

            if (simulateOnStart)
            {
                StartSimulation();
            }
        }

        private void Update()
        {
            if (_stopSimulation)
            {
                if (logDebug) Debug.Log("[TimeStone] " + name + " : Confirming StopSimulation from Update");
                OnSimulationComplete();
                _stopSimulation = false;
            }
            else if (_initSimulation)
            {
                InitSimulation(_initSimulationUpdateAgentsList);
                _initSimulation = false;
            }
            // If a simulation must start, it is prioritized 
            // Simulating and recording can't be used simultaneously
            else if (_startSimulation && Time.frameCount > _startFrameCount)
            {
                if (logDebug) Debug.Log("[TimeStone] " + name + " : Confirming StartSimulation from Update");
                // Starts simulation
                DoSimulation();
                _startSimulation = false;
            }

            // Already simulating
            else if (Simulating)
            {
                var frameSimulationIsComplete = DoUpdateAndPhysicsSimulation();
                _stopSimulation |= frameSimulationIsComplete;
            }

            else if (Recording)
            {
                // On recording activation, make sure we don't access destroyed objects
                // This can't be made every call for performance issues, so you can't destroy object during recording (or you'll get a fucking shit ton of exceptions :) )
                if (_firstRecord)
                    CleanTimeAgentsList();

                DoRecord();
            }

            else if (playback)
            {
                // Check if playback field has been set without using Playback() method
                if (!_playbackActive)
                {
                    StartPlayback();
                }

                switch (playbackTimeScale)
                {
                    case TimeScale.ScaledTime:
                        _playbackElapsedTimer += Time.deltaTime * playbackSpeed;
                        break;
                    case TimeScale.UnscaledTime:
                        _playbackElapsedTimer += Time.unscaledDeltaTime * playbackSpeed;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if ((playbackSpeed > 0 && playbackTickCursor < GetTickCount() - 1)
                    || (playbackSpeed < 0 && playbackTickCursor > 0))
                {
                    float playTime = _playbackStartTime + _playbackElapsedTimer;
                    playbackTickCursor = GetTickIndexAtTime(playTime);
                    SetPlaybackTick(playbackTickCursor, playbackUpdateAddTickCursor);
                }
                else
                {
                    StopPlayback();
                }
            }
            else
            {
                // Checks if playback field has been setted without using Stop() method
                if (_playbackActive)
                {
                    StopPlayback();
                }
            }
        }

        /// <summary>
        /// Initializes the time agent list using the defined mode
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void InitTimeAgentsList()
        {
            ClearTimeAgentsList();

            switch (timeAgentsListInitMode)
            {
                case TimeAgentsListInitMode.Manual:
                    if (logDebug)
                        Debug.Log("[TimeStone] " + name + " : Initializing time agents from manual list : " +
                                  timeAgentsManualInitList);

                    timeAgentsNoChildren = new List<TimeAgent>(timeAgentsManualInitList);

                    break;
                case TimeAgentsListInitMode.Tag:

                    if (logDebug)
                        Debug.Log("[TimeStone] " + name + " : Initializing time agents from tag : " +
                                  timeAgentsListInitTag);

                    var timeAgentsWithTag = TimeStoneUtils.GetAllTimeAgentsWithoutSimulationAgents()
                        .Where(ta => ta.CompareTag(timeAgentsListInitTag));
                    timeAgentsNoChildren.AddRange(timeAgentsWithTag.ToList());

                    break;
                case TimeAgentsListInitMode.Layer:

                    if (logDebug)
                        Debug.Log("[TimeStone] " + name + " : Initializing time agents from layer : " +
                                  timeAgentsListInitLayer);

                    var layerId = LayerMask.NameToLayer(timeAgentsListInitLayer);

                    var timeAgentsFromLayer = TimeStoneUtils.GetAllTimeAgentsWithoutSimulationAgents()
                        .Where(ta => ta.gameObject.layer == layerId);
                    timeAgentsNoChildren.AddRange(timeAgentsFromLayer.ToList());

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Remove null instances
            timeAgentsNoChildren = new List<TimeAgent>(timeAgentsNoChildren.Where(ta => ta != null));

            // Initialize and populate final time agents list with children
            timeAgents = new List<TimeAgent>(timeAgentsNoChildren);
            foreach (var timeAgent in timeAgentsNoChildren)
            {
                if (timeAgent.parseChildren)
                    SearchChildTimeAgentToAdd(timeAgents, timeAgent.transform);
            }

            foreach (var timeAgent in timeAgents)
            {
                timeAgent.InitTimeAgentsList(this);
            }

            onInitTimeAgentsList.Invoke();
        }

        /// <summary>
        /// Searches for time agents in children recursively
        /// </summary>
        /// <param name="listTarget">The list the time agents must be added to</param>
        /// <param name="timeAgentTransform">The parent transform from which the search starts</param>
        private void SearchChildTimeAgentToAdd(List<TimeAgent> listTarget, Transform timeAgentTransform)
        {
            var childTimeAgent = timeAgentTransform.GetComponent<TimeAgent>();
            if (childTimeAgent != null && !timeAgents.Contains(childTimeAgent))
            {
                listTarget.Add(childTimeAgent);
            }

            for (var i = 0; i < timeAgentTransform.childCount; i++)
            {
                var grandChildTransform = timeAgentTransform.GetChild(i);
                SearchChildTimeAgentToAdd(listTarget, grandChildTransform);
            }
        }

        /// <summary>
        /// Clears the time agents lists
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void ClearTimeAgentsList()
        {
            timeAgentsNoChildren.Clear();
            timeAgents.Clear();
            simulationTimeAgents.Clear();
        }

        /// <summary>
        /// Remove eventual destroyed agents from the lists
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void CleanTimeAgentsList()
        {
            timeAgentsNoChildren = new List<TimeAgent>(timeAgentsNoChildren.Where(ta => ta != null));
            timeAgents = new List<TimeAgent>(timeAgents.Where(ta => ta != null));
        }

        /// <summary>
        /// Starts recording
        /// </summary>
        public void StartRecording()
        {
            if (logDebug) Debug.Log("[TimeStone] " + name + " : StartRecording");
            Recording = true;
            _firstRecord = true;
            _onRecordStart.Invoke();
        }

        /// <summary>
        /// Stops recording
        /// </summary>
        public void StopRecording()
        {
            if (logDebug) Debug.Log("[TimeStone] " + name + " : StopRecording");
            Recording = false;
            recordTime = 0.0f;
            _firstRecord = true;
            _onRecordStop.Invoke();
        }

        /// <summary>
        /// Starts a new simulation.
        /// The simulation will really start to execute on next frame.
        /// </summary>
        /// <param name="updateAgentsList">If true, the time agents list will be updated before executing the simulation</param>
        public void StartSimulation(bool updateAgentsList = false)
        {
            if (logDebug) Debug.Log("[TimeStone] " + name + " : StartSimulation");

            // There could be a previous simulation still running 
            // But we can't chain current and next simulation in the same frame because current simulation could need to finish
            if (Simulating)
            {
                StopSimulation();
            }

            _initSimulation = true;
            _initSimulationUpdateAgentsList = updateAgentsList;
        }

        /// <summary>
        /// Initialize the simulation data : scene, clone agents, etc.
        /// </summary>
        /// <param name="updateAgentsList"></param>
        public void InitSimulation(bool updateAgentsList)
        {
            _simulationTimeWatch = Stopwatch.StartNew();
            _simulationTimeSinceStartElapsed = 0;

            if (simulationUseUnityFixedDeltaTime)
            {
                simulationPhysicsStep = Time.fixedDeltaTime;
            }

            if (updateAgentsList)
                InitTimeAgentsList();
            else
            {
                // Clean time agent lists for eventual destroyed objects since last init of the list
                CleanTimeAgentsList();
            }

            CreateSimulationScene();

            // We can't keep recording while simulating or we would override simulation data
            StopRecording();
            // We also can't play while recording (not in all cases... but most of them... better to have a security)
            StopPlayback();

            SimulationComplete = false;

            // Simulation will really start next update, so that every cloned object initialize the right way
            // you know... unity needs time to initialize new objects and everything... that's an old boy... a good boy, but old...
            _startSimulation = true;
            _startFrameCount = Time.frameCount;
        }

        /// <summary>
        /// Stops running simulation
        /// </summary>
        public void StopSimulation()
        {
            if (logDebug) Debug.Log("[TimeStone] " + name + " : StopSimulation");

            _startSimulation = false;
            if (Simulating)
            {
                _stopSimulation = true;
            }
        }

        /// <summary>
        /// Try recording the current frame if needed.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void DoRecord()
        {
            switch (recordingTimeScale)
            {
                case TimeScale.ScaledTime:
                    recordTime += Time.deltaTime;
                    _secondsSinceLastRecord += Time.deltaTime;
                    break;
                case TimeScale.UnscaledTime:
                    recordTime += Time.unscaledDeltaTime;
                    _secondsSinceLastRecord += Time.unscaledDeltaTime;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_firstRecord || _secondsSinceLastRecord > recordingStep)
            {
                if (!_firstRecord)
                    addTickCursor++;

                _firstRecord = false;

                if (logDebug)
                    Debug.Log("[TimeStone] " + name + " : Recording tick at <" + addTickCursor + "> (Tick count = " +
                              GetTickCount() + ")");

                PersistTick(TimeTickOrigin.Record, _secondsSinceLastRecord);

                foreach (var timeAgent in timeAgents)
                {
                    timeAgent.TimeLineChange(this, TimeTickOrigin.Record);
                }

                // Check Maximum record data is not reached, or remove oldest data
                if (recordDuration > 0 && GetTimeLineDuration() > recordDuration)
                {
                    var from = 0;
                    var to = GetTickIndexAtTime(GetTimeLineDuration() - recordDuration);
                    RemoveData(from, to);
                }

                onTimeLineChange.Invoke(TimeTickOrigin.Record);

                if (updatePlayTickCursorOnRecord)
                {
                    playbackTickCursor = addTickCursor;
                }

                _secondsSinceLastRecord = 0.0f;
            }
        }


        /// <summary>
        /// Start Executing the simulation.
        /// It is called on the next frame after <see cref="StartSimulation"/> is called to let simulation objects initialize
        /// </summary>
        private void DoSimulation()
        {
            Simulating = true;

            simulationTime = 0;
            frameTimeStack = 0;

            foreach (var timeAgent in timeAgents)
            {
                if (timeAgent.simulationCallbackPolicy == TimeAgent.SimulationCallbackPolicy.CloneOnly)
                    continue;
                timeAgent.SimulationStart(this);
            }

            foreach (var simulationAgent in simulationTimeAgents)
            {
                if (simulationAgent.simulationCallbackPolicy == TimeAgent.SimulationCallbackPolicy.OriginalOnly)
                    continue;
                simulationAgent.SimulationStart(this);
            }

            onSimulationStart.Invoke();

            simulationStartTick = addTickCursor;

            _simulationDoInit = true;
            var frameSimulationIsComplete = DoUpdateAndPhysicsSimulation();
            _stopSimulation |= frameSimulationIsComplete;
        }

        /// <summary>
        /// Called when simulation is complete
        /// </summary>
        private void OnSimulationComplete()
        {
            _simulationTimeWatch.Stop();
            if (logPerfs)
                Debug.Log("[TimeStone] " + name + " : Simulation time : " + _simulationTimeWatch.ElapsedMilliseconds +
                          "ms");
            if (logDebug) Debug.Log("[TimeStone] " + name + " : OnSimulationComplete");

            if (resetAddTickCursorOnSimulationComplete)
            {
                // Move the addTickCursor where the simulation started
                addTickCursor = simulationStartTick;
            }

            Simulating = false;
            SimulationComplete = true;

            foreach (var timeAgent in timeAgents)
            {
                if (timeAgent.simulationCallbackPolicy == TimeAgent.SimulationCallbackPolicy.CloneOnly)
                    continue;
                timeAgent.SimulationComplete(this);
            }

            foreach (var simulationAgent in simulationTimeAgents)
            {
                if (simulationAgent.simulationCallbackPolicy == TimeAgent.SimulationCallbackPolicy.OriginalOnly)
                    continue;
                simulationAgent.SimulationComplete(this);
            }

            onSimulationComplete.Invoke();

            if (!keepSimulationScene)
            {
                if (simulationScene.IsValid())
                {
                    SceneManager.UnloadSceneAsync(simulationScene);
                }
            }

            simulationStartTick = 0;
        }

        /// <summary>
        /// Creates the simulation scene with the cloned time agents in it
        /// </summary>
        private void CreateSimulationScene()
        {
            var timestamp = Time.unscaledTime;
            var csp = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
            var sceneId = "Time Stone <" + name + "> Simulation_" + timestamp;
            simulationScene = SceneManager.CreateScene(sceneId, csp);
            simulationPhysicsScene = simulationScene.GetPhysicsScene();

            simulationTimeAgents.Clear();

            // Switch to the new scene so cloned objects appear directly in it.
            var mainScene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(simulationScene);

            // Cloning from the timeAgentsNoChildren, so we don't clone dynamically found children
            for (var i = timeAgentsNoChildren.Count - 1; i >= 0; i--)
            {
                var timeAgentToClone = timeAgentsNoChildren[i];

                if (logDebug)
                    Debug.Log(
                        "[TimeStone] " + name + " : Cloning <" + timeAgentToClone.gameObject +
                        "> into simulation scene.");

                var clone = timeAgentToClone.GetClone();
                if (clone == null)
                    continue;

                SetupSimulationTimeAgent(timeAgentToClone,
                    clone.transform, timeAgentToClone.parseChildren);
            }

            // Restore the old active scene.
            SceneManager.SetActiveScene(mainScene);

            foreach (var timeAgent in timeAgents)
            {
                timeAgent.SimulationSceneReady(this);
            }

            foreach (var simulationTimeAgent in simulationTimeAgents)
            {
                simulationTimeAgent.SimulationSceneReady(this);
            }

            _onSimulationSceneReady.Invoke();
        }

        /// <summary>
        /// Setup a TimeAgent and its clone then search recursively in children if needed
        /// </summary>
        /// <param name="originalTimeAgent">The original TimeAgent cloned</param>
        /// <param name="cloneTransform">The cloned TimeAgent's Transform</param>
        /// <param name="recursive">Should the setup be made recursively in children</param>
        private void SetupSimulationTimeAgent(TimeAgent originalTimeAgent,
            Transform cloneTransform, bool recursive)
        {
            if (originalTimeAgent == null)
                return;

            cloneTransform.gameObject.name = originalTimeAgent.gameObject.name + "(Simulation Clone)";

            var childClone = cloneTransform.GetComponent<TimeAgent>();
            if (!simulationTimeAgents.Contains(originalTimeAgent))
            {
                simulationTimeAgents.Add(childClone);

                var simulationTimeAgent = childClone.GetComponent<TimeAgent>();
                originalTimeAgent.SimulationClone = simulationTimeAgent;
                simulationTimeAgent.Original = originalTimeAgent;

                // Make sure we have the same id for the clone to bind it to the real agent
                simulationTimeAgent.guid = originalTimeAgent.guid;

                originalTimeAgent.ClonedForSimulation(this);
                simulationTimeAgent.ClonedForSimulation(this);
            }

            if (recursive)
            {
                for (var i = 0; i < originalTimeAgent.transform.childCount; i++)
                {
                    var grandChildTransform = originalTimeAgent.transform.GetChild(i);
                    var grandChild = grandChildTransform.GetComponent<TimeAgent>();
                    var grandChildCloneTransform = cloneTransform.GetChild(i);
                    SetupSimulationTimeAgent(grandChild, grandChildCloneTransform, true);
                }
            }
        }

        /// <summary>
        /// Execute code and physics simulation for current frame
        /// </summary>
        /// <returns> true if simulation is complete, false if it needs more computation</returns>
        private bool DoUpdateAndPhysicsSimulation()
        {
            Stopwatch frameWatch = new Stopwatch();
            frameWatch.Start();
            switch (simulationTimeScale)
            {
                case TimeScale.ScaledTime:
                    _simulationTimeSinceStartElapsed += Time.deltaTime;
                    break;
                case TimeScale.UnscaledTime:
                    _simulationTimeSinceStartElapsed += Time.unscaledDeltaTime;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            while ((simulationTime < simulationDuration || !simulationHasDuration))
            {
                // For the first tick, we just persist actual state
                if (_simulationDoInit)
                {
                    PersistTick(TimeTickOrigin.Simulation, 0);
                    _simulationDoInit = false;
                }
                else
                {
                    var deltaTime = simulationUpdateStep;

                    if (simulationUseUpdateDeltaTime)
                    {
                        switch (simulationTimeScale)
                        {
                            case TimeScale.ScaledTime:
                                deltaTime = Time.deltaTime;
                                break;
                            case TimeScale.UnscaledTime:
                                deltaTime = Time.unscaledDeltaTime;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    if (simulationGroupFrameTicks)
                    {
                        var maxDeltaTimeForFrame = deltaTime * simulationSpeed;
                        while (deltaTime < maxDeltaTimeForFrame)
                        {
                            deltaTime += deltaTime;
                        }
                    }

                    if (simulationSpeed < 1)
                    {
                        deltaTime *= simulationSpeed;
                    }

                    addTickCursor++;

                    SimulateTick(deltaTime);

                    simulationTime += deltaTime;
                    frameTimeStack += deltaTime;
                }

                var currentFrameTime = frameWatch.ElapsedMilliseconds;
                var maxTimeForFrame = _simulationTimeSinceStartElapsed * simulationSpeed;

                if (simulationTime >= maxTimeForFrame ||
                    currentFrameTime > simulationMaxMsByFrame)
                {
                    frameWatch.Stop();

                    foreach (var timeAgent in timeAgents)
                    {
                        if (timeAgent.simulationCallbackPolicy == TimeAgent.SimulationCallbackPolicy.CloneOnly)
                            continue;
                        timeAgent.TimeLineChange(this, TimeTickOrigin.Simulation);
                    }

                    // when simulation is asynchronous, timeLineChange events are sent on frame breaks to allow streaming
                    // from visualizers for example. Sending the timeLineChange on each real simulation tick done would
                    // be a performance killer as many ticks are simulated each frame
                    foreach (var simulationTimeAgent in simulationTimeAgents)
                    {
                        if (simulationTimeAgent.simulationCallbackPolicy ==
                            TimeAgent.SimulationCallbackPolicy.OriginalOnly)
                            continue;
                        simulationTimeAgent.TimeLineChange(this, TimeTickOrigin.Simulation);
                    }

                    onTimeLineChange.Invoke(TimeTickOrigin.Simulation);

                    return false;
                }
            }

            frameWatch.Stop();
            return true;
        }

        /// <summary>
        /// Simulate a tick using the deltaTime
        /// </summary>
        /// <param name="deltaTime">The deltaTime to simulate the tick</param>
        private void SimulateTick(float deltaTime)
        {
            if (simulatePhysics)
                SimulatePhysics();

            if (simulateUpdate)
            {
                foreach (var timeAgent in timeAgents)
                {
                    if (timeAgent.simulationCallbackPolicy == TimeAgent.SimulationCallbackPolicy.CloneOnly)
                        continue;
                    try
                    {
                        timeAgent.SimulationUpdate(deltaTime, this);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[TimeStone] " + name + " : Exception running SimulationUpdate on <" +
                                       timeAgent.name + "> : " +
                                       e);
                    }
                }

                foreach (var simulationAgent in simulationTimeAgents)
                {
                    if (simulationAgent.simulationCallbackPolicy ==
                        TimeAgent.SimulationCallbackPolicy.OriginalOnly)
                        continue;
                    try
                    {
                        simulationAgent.SimulationUpdate(deltaTime, this);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[TimeStone] " + name + " : Exception running SimulationUpdate on <" +
                                       simulationAgent.name +
                                       "> : " + e);
                    }
                }

                foreach (var timeAgent in timeAgents)
                {
                    if (timeAgent.simulationCallbackPolicy == TimeAgent.SimulationCallbackPolicy.CloneOnly)
                        continue;

                    try
                    {
                        timeAgent.SimulationLateUpdate(deltaTime, this);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[TimeStone] " + name + " : Exception running SimulationLateUpdate on <" +
                                       timeAgent.name +
                                       "> : " + e);
                    }
                }

                foreach (var simulationAgent in simulationTimeAgents)
                {
                    if (simulationAgent.simulationCallbackPolicy ==
                        TimeAgent.SimulationCallbackPolicy.OriginalOnly)
                        continue;
                    try
                    {
                        simulationAgent.SimulationLateUpdate(deltaTime, this);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[TimeStone] " + name + " : Exception running SimulationLateUpdate on <" +
                                       simulationAgent.name + "> : " + e);
                    }
                }
            }

            // Persist data that have just been simulated
            PersistTick(TimeTickOrigin.Simulation, deltaTime);
        }

        /// <summary>
        /// Execute physics simulation on simulation's scene
        /// </summary>
        /// <exception cref="DataException"></exception>
        private void SimulatePhysics()
        {
            if (simulationScene.IsValid())
            {
                while (frameTimeStack >= simulationPhysicsStep)
                {
                    frameTimeStack -= simulationPhysicsStep;

                    simulationPhysicsScene.Simulate(simulationPhysicsStep);

                    if (simulateUpdate)
                    {
                        foreach (var timeAgent in timeAgents)
                        {
                            if (timeAgent.simulationCallbackPolicy ==
                                TimeAgent.SimulationCallbackPolicy.CloneOnly)
                                continue;
                            try
                            {
                                timeAgent.SimulationFixedUpdate(simulationPhysicsStep, this);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("[TimeStone] " + name +
                                               " : Exception running SimulationFixedUpdate on <" +
                                               timeAgent.name + "> : " + e);
                            }
                        }

                        foreach (var simulationAgent in simulationTimeAgents)
                        {
                            if (simulationAgent.simulationCallbackPolicy ==
                                TimeAgent.SimulationCallbackPolicy.OriginalOnly)
                                continue;

                            try
                            {
                                simulationAgent.SimulationFixedUpdate(simulationPhysicsStep, this);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("[TimeStone] " + name +
                                               " : Exception running SimulationFixedUpdate on <" +
                                               simulationAgent.name + "> : " + e);
                            }
                        }
                    }
                }
            }
            else
            {
                StopSimulation();
                throw new DataException("[TimeStone] ERROR : Physics scene is invalid, simulation is compromised.");
            }
        }

        /// <summary>
        /// Starts playback.
        /// <para>Playback is applying TimeLine's data to stone's timeAgents respecting the timing, it uses <see cref="SetPlaybackTick"/></para>
        /// <para>You can control the speed of the playback with <see cref="TimeStone.playbackSpeed"/></para>
        /// <para>You can stop playback using <see cref="StopPlayback"/></para>
        /// <para>You can control if the playback synchronizes the addTickCursor with <see cref="playbackUpdateAddTickCursor"/>,
        /// with this option, you can start recording or simulating right after you stop playback and old and new data
        /// will be synchronized</para>
        /// </summary>
        public void StartPlayback()
        {
            if (playbackTickCursor >= GetTickCount() || playback)
                return;

            playback = true;
            _playbackStartTime = GetTimeAtTickIndex(playbackTickCursor);
            _playbackElapsedTimer = 0.0f;
            _playbackActive = true;
        }

        /// <summary>
        /// Stops playback
        /// </summary>
        public void StopPlayback()
        {
            if (!playback)
                return;

            playback = false;
            _playbackStartTime = 0.0f;
            _playbackElapsedTimer = 0.0f;
            _playbackActive = false;
        }

        /// <summary>
        /// Persist current tick
        /// </summary>
        /// <param name="origin">The origin of the data persisted, Record or Simulation</param>
        /// <param name="deltaTime">The deltaTime</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void PersistTick(TimeTickOrigin origin, float deltaTime)
        {
            currentPersistTickOrigin = origin;
            currentPersistTickDeltaTime = deltaTime;

            // 1 -First persist the timeline tick index
            timeLine.PersistTimeDataAt(addTickCursor, TimeLine.tickGuid, TimeLine.TickDeltaTimeDataId, deltaTime,
                deltaTime, origin, timeLineAddTickBehaviour);

            List<TimeAgent> timeAgentsToPersist;
            switch (origin)
            {
                case TimeTickOrigin.Record:
                    timeAgentsToPersist = timeAgents;
                    break;
                case TimeTickOrigin.Simulation:
                    timeAgentsToPersist = simulationTimeAgents;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            foreach (var timeAgentToPersist in timeAgentsToPersist)
            {
                try
                {
                    currentPersistTickAgent = timeAgentToPersist;
                    // 2 - Now really persist time agents data
                    timeAgentToPersist.PersistTick(this, origin, deltaTime);
                }
                catch (Exception e)
                {
                    Debug.LogError("[TimeStone] " + name + " : Exception running PersistTick on <" +
                                   timeAgentToPersist.name + "> : " +
                                   e);
                }
            }
        }

        /// <summary>
        /// Persist data in the TimeStone's TimeLine using current persisted tick TimeAgent, TimeTickOrigin and DeltaTime.
        /// </summary>
        /// <param name="dataId">The id of the data to persist</param>
        /// <param name="dataValue">The value of the data to persist</param>
        /// <typeparam name="T"></typeparam>
        public void PersistData<T>(string dataId, T dataValue)
        {
            timeLine.PersistTimeDataAt(addTickCursor, currentPersistTickAgent.guid, dataId, currentPersistTickDeltaTime,
                dataValue, currentPersistTickOrigin,
                timeLineAddTickBehaviour);
        }

        /// <summary>
        /// Persist timeAgent's data in the TimeStone's TimeLine.
        /// </summary>
        /// <param name="timeAgent">The timeAgent to persist data from</param>
        /// <param name="origin">The origin of the data, record or simulation</param>
        /// <param name="stepTime">The step in the timeLine</param>
        /// <param name="dataId">The id of the data to persist</param>
        /// <param name="dataValue">The value of the data to persist</param>
        /// <typeparam name="T"></typeparam>
        public void PersistData<T>(TimeAgent timeAgent, TimeTickOrigin origin, float stepTime, string dataId,
            T dataValue)
        {
            timeLine.PersistTimeDataAt(addTickCursor, timeAgent.guid, dataId, stepTime, dataValue, origin,
                timeLineAddTickBehaviour);
        }

        /// <summary>
        /// Get TimeData at the current played tick
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="dataExists"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public TimeLine.TimeData<T> GetData<T>(string dataId, out bool dataExists)
        {
            return GetDataAt<T>(currentSetDataTickIndex, currentSetDataAgent, dataId, out dataExists);
        }

        /// <summary>
        /// Get TimeData at the given tick index
        /// </summary>
        /// <param name="tickIndex">The tick index search</param>
        /// <param name="timeAgent">The TimeAgent to get the data from</param>
        /// <param name="dataId">The id of the data searched</param>
        /// <param name="dataExists">Out parameter exposing if the asked data exist</param>
        /// <typeparam name="T">The type of the TimeData's value searched</typeparam>
        /// <returns>The TimeData object if the data exist, null otherwise</returns>
        public TimeLine.TimeData<T> GetDataAt<T>(int tickIndex, TimeAgent timeAgent, string dataId, out bool dataExists)
        {
            return timeLine.GetDataAt<T>(tickIndex, timeAgent.guid, dataId, out dataExists);
        }

        /// <summary>
        /// Get TimeData Value at the current played tick
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="dataExists"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetDataValue<T>(string dataId, out bool dataExists)
        {
            return GetDataValueAt<T>(currentSetDataTickIndex, currentSetDataAgent, dataId, out dataExists);
        }

        /// <summary>
        /// Get TimeData Value at the given tick index
        /// </summary>
        /// <param name="tickIndex">The tick index search</param>
        /// <param name="timeAgent">The TimeAgent to get the data from</param>
        /// <param name="dataId">The id of the data searched</param>
        /// <param name="dataExists">Out parameter exposing if the asked data exist</param>
        /// <typeparam name="T">The type of the data searched</typeparam>
        /// <returns>The TimeData value if the data exist, null otherwise</returns>
        public T GetDataValueAt<T>(int tickIndex, TimeAgent timeAgent, string dataId, out bool dataExists)
        {
            var timeData = GetDataAt<T>(tickIndex, timeAgent, dataId, out dataExists);
            return dataExists ? timeData.Value : default;
        }

        /// <summary>
        /// Get the TimeData at the given Time
        /// </summary>
        /// <param name="time">The time searched</param>
        /// <param name="timeAgent">The TimeAgent to get the data from</param>
        /// <param name="dataId">The id of the data searched</param>
        /// <typeparam name="T">The type of the data searched</typeparam>
        /// <returns>The TimeData searched</returns>
        public TimeLine.TimeData<T> GetDataAtTime<T>(float time, TimeAgent timeAgent, string dataId)
        {
            return timeLine.GetClosestDataAtTime<T>(time, timeAgent.guid, dataId);
        }

        /// <summary>
        /// Updates Time Agents with the TimeLine's data at tickIndex.
        /// </summary>
        /// <param name="tickIndex"></param>
        /// <param name="updateAddTickCursor"></param>
        public void SetPlaybackTick(int tickIndex, bool updateAddTickCursor = true)
        {
            tickIndex = Mathf.Clamp(tickIndex, 0, timeLine.GetTickCount() - 1);

            if (updateAddTickCursor)
            {
                addTickCursor = tickIndex;
            }

            playbackTickCursor = tickIndex;
            currentSetDataTickIndex = playbackTickCursor;

            foreach (var timeAgent in timeAgents)
            {
                try
                {
                    currentSetDataAgent = timeAgent;
                    timeAgent.SetDataTick(this, playbackTickCursor);
                }
                catch (Exception e)
                {
                    Debug.LogError("[TimeStone] " + name + " : Exception running SetDataTick on <" + timeAgent.name +
                                   "> : " + e);
                }
            }
        }

        /// <summary>
        /// Get playback's time, corresponding to current <see cref="playbackTickCursor"/> using <see cref="GetTimeAtTickIndex"/>
        /// </summary>
        /// <returns>The time for current <see cref="playbackTickCursor"/></returns>
        public float GetPlaybackTime()
        {
            return GetTimeAtTickIndex(playbackTickCursor);
        }

        /// <summary>
        /// Set playback time. Playback is based on <see cref="playbackTickCursor"/>, so the tick index value is
        /// deducted using <see cref="GetTickIndexAtTime"/>
        /// </summary>
        /// <param name="time"></param>
        /// <param name="updateAddTickCursor"></param>
        public void SetPlaybackTime(float time, bool updateAddTickCursor = true)
        {
            time = Mathf.Clamp(time, 0, GetTimeLineDuration());

            SetPlaybackTick(GetTickIndexAtTime(time), updateAddTickCursor);
        }


        /// <summary>
        /// Returns if the stone handles the timeAgent.
        /// This method does not check the timeAgent's state (active/inactive).
        /// <see cref="TimeAgent"/> are distinguished by the stone using their <see cref="TimeAgent.guid"/>
        /// </summary>
        /// <param name="timeAgent"></param>
        /// <returns>If the TimeAgent is in TimeStone's list</returns>
        public bool HasTimeAgent(TimeAgent timeAgent)
        {
            foreach (var agent in timeAgents)
            {
                if (agent.guid == timeAgent.guid)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Remove timeLine's data from the stone.
        /// Cursors <see cref="playbackTickCursor"/> and <see cref="addTickCursor"/> are updates in consequence.
        /// </summary>
        /// <param name="tickFrom">The start tick index</param>
        /// <param name="tickTo">The end tick index</param>
        public void RemoveData(int tickFrom, int tickTo)
        {
            // Debug.Log("Removing : " + tickFrom + " > " + tickTo);
            // Securities
            if (tickTo < tickFrom)
                tickTo = tickFrom;
            if (tickFrom < 0)
                tickFrom = 0;
            if (tickTo > GetTickCount())
                tickTo = GetTickCount();

            // Remove data
            timeLine.Remove(tickFrom, tickTo);

            // Updates cursors positions to reflect the data removed
            if (addTickCursor > tickTo)
            {
                addTickCursor = Mathf.Max(tickFrom, addTickCursor - (tickTo - tickFrom));
            }
            else if (addTickCursor > tickFrom)
                addTickCursor = tickFrom;

            if (playbackTickCursor > tickTo)
            {
                playbackTickCursor = Mathf.Max(tickFrom, playbackTickCursor - (tickTo - tickFrom));
            }
            else if (playbackTickCursor > tickFrom)
                playbackTickCursor = tickFrom;
        }

        /// <summary>
        /// Clear the <see cref="timeLine"/>'s data (not its time agents)
        /// </summary>
        public void Clear()
        {
            timeLine.Clear();
            recordTime = 0.0f;
            addTickCursor = 0;
            playbackTickCursor = 0;
            // If async simulation is running, clear must be handled with a new init
            _simulationDoInit = true;
            onTimeLineClear.Invoke();
        }

        /// <summary>
        /// Changes freeze state of <see cref="timeAgents"/> using <see cref="TimeUtils.RigidbodyFreezeType.PhysicsAutoSimulation"/> for the physics.
        /// </summary>
        /// <param name="state">The freeze state</param>
        public void FreezeTimeAgents(bool state)
        {
            FreezeTimeAgents(state, TimeUtils.RigidbodyFreezeType.PhysicsAutoSimulation);
        }

        /// <summary>
        ///  Changes freeze state of <see cref="timeAgents"/> using given physics freeze type
        /// </summary>
        /// <param name="state">The freeze state</param>
        /// <param name="rigidbodyFreezeType">The type of freeze for physics</param>
        public void FreezeTimeAgents(bool state, TimeUtils.RigidbodyFreezeType rigidbodyFreezeType)
        {
            if (logDebug)
                Debug.Log("[TimeStone] " + name + " : Freezing <" + state + "> " + timeAgents.Count + " time agents");
            TimeUtils.Freeze(timeAgents, state, rigidbodyFreezeType);
        }

        /// <summary>
        /// Get the tick index at given time (in seconds). The result is an approximation, the closest tick corresponding to this time is returned.
        /// Therefore this method always returns an index, even if time does not exist in the <see cref="timeLine"/>
        /// </summary>
        /// <param name="time">The time searched</param>
        /// <returns>The tick index at given time</returns>
        public int GetTickIndexAtTime(float time)
        {
            return timeLine.GetClosestTickIndexAtTime(time);
        }

        /// <summary>
        /// Get the time (in seconds) corresponding to the tick index.
        /// If tick index does not exist in the <see cref="timeLine"/> then returns 0
        /// </summary>
        /// <param name="index"></param>
        /// <returns>The time at tick index or zero if index does not exist</returns>
        public float GetTimeAtTickIndex(int index)
        {
            return timeLine.GetTimeAtTickIndex(index);
        }

        /// <summary>
        /// Get the number of ticks in the <see cref="timeLine"/>
        /// </summary>
        /// <returns>The number of ticks in the <see cref="timeLine"/></returns>
        public int GetTickCount()
        {
            return timeLine.GetTickCount();
        }

        /// <summary>
        /// Get the <see cref="timeLine"/> duration (in seconds)
        /// </summary>
        /// <returns>The <see cref="timeLine"/> duration (in seconds)</returns>
        public float GetTimeLineDuration()
        {
            return timeLine.GetTimeAtTickIndex(timeLine.GetTickCount() - 1);
        }

        [Serializable]
        public class OnInitTimeAgentsListEvent : UnityEvent
        {
        }

        [Serializable]
        public class OnSimulationSceneReadyEvent : UnityEvent
        {
        }

        [Serializable]
        public class OnTimeLineClearEvent : UnityEvent
        {
        }

        [Serializable]
        public class OnTimeLineChangeEvent : UnityEvent<TimeTickOrigin>
        {
        }

        [Serializable]
        public class OnSimulationStartEvent : UnityEvent
        {
        }

        [Serializable]
        public class OnSimulationCompleteEvent : UnityEvent
        {
        }

        [Serializable]
        public class OnRecordStartEvent : UnityEvent
        {
        }

        [Serializable]
        public class OnRecordStopEvent : UnityEvent
        {
        }
    }
}