using System;
using System.Collections.Generic;
using RedGirafeGames.Agamotto.Scripts.Runtime.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace RedGirafeGames.Agamotto.Scripts.Runtime.Agents
{
    /// <summary>
    /// <para>TimeAgent are the objects managed by the TimeStone.</para>
    /// 
    /// <para>This class natively handles <see cref="Transform"/> data, <see cref="Rigidbody"/> velocity, <see cref="Animator"/> data and <see cref="ParticleSystem"/>
    /// </para>
    ///
    /// <para>To save custom data in the TimeStone, you can either use the <see cref="onPersistTick"/> and <see cref="onSetDataTick"/> UnityEvent
    ///
    /// <example>For example
    /// <code>
    /// 
    /// var myTimeAgent = GetComponent(typeOf(TimeAgent));
    /// timeAgent.onPersistTick.AddListener(PersistTick);
    /// timeAgent.onSetDataTick.AddListener(SetDataTick);
    ///
    /// [...]
    /// 
    /// private void PersistTick(TimeStone stone, TimeStone.TimeTickOrigin origin, float deltaTime)
    /// {
    ///     stone.PersistData(minionsDeadDataId, minionsDead);
    ///     [...]
    /// }
    /// </code>
    /// </example>
    /// 
    /// Or extend the class and override <see cref="PersistTick"/> and <see cref="SetDataTick"/>. see <see cref="GenericTimeAgent"/> for an example.
    /// </para>
    /// 
    /// <para>The <see cref="TimeStone"/> manage its TimeAgents by calling <see cref="PersistTick"/> when data must be
    /// persisted (during record or simulation) and <see cref="SetDataTick"/> when data must be updated (during a playback). 
    /// </para>
    /// 
    /// <para>ATTENTION : To Execute a simulation, the <see cref="TimeStone"/> clones the <see cref="TimeAgent"/>, so you must handle that
    /// there are 2 versions of your TimeAgent during a simulation, the Original and the Clone. They can be differentiated using <see cref="IsClone"/> and
    /// you can control which will receive callbacks using <see cref="simulationCallbackPolicy"/></para>
    /// 
    /// <para>During a simulation, the <see cref="TimeStone"/> calls the lifeCycle callbacks on its TimeAgents : <see cref="SimulationStart"/>, <see cref="SimulationUpdate"/>, etc.</para>
    /// <para>To add simulation code execution, you can either use the <see cref="onSimulationUpdate"/>, <see cref="onSimulationStart"/>, etc. UnityEvents or extend the class and override
    /// <see cref="SimulationUpdate"/>, <see cref="SimulationStart"/>, etc.</para>
    /// 
    /// </summary>
    /// <remarks>
    /// <para>Add this component to any object you want to be managed by a <see cref="TimeStone"/></para>
    /// </remarks>
    [AddComponentMenu("Agamotto/Time Agent")]
    public class TimeAgent : MonoBehaviour
    {
        /// <summary>
        /// The token used in dataIds to be replaced by an unique id when data is not unique
        /// </summary>
        private const string DataIdUidToken = "#";

        /// <summary>
        /// <see cref="GameObject.activeSelf"/> id to persist in the <see cref="TimeStone"/>
        /// </summary>
        public const string ActiveDataId = "GameObjectActiveSelf";

        /// <summary>
        /// <see cref="Transform.position"/> id to persist in the <see cref="TimeStone"/>
        /// </summary>
        public const string PositionDataId = "TransformPosition";

        /// <summary>
        /// <see cref="Transform.localPosition"/> id to persist in the <see cref="TimeStone"/>
        /// </summary>
        public const string PositionLocalDataId = "TransformLocalPosition";

        /// <summary>
        /// <see cref="Transform.rotation"/> id to persist in the <see cref="TimeStone"/>
        /// </summary>
        public const string RotationDataId = "TransformRotation";

        /// <summary>
        /// <see cref="Transform.localRotation"/> id to persist in the <see cref="TimeStone"/>
        /// </summary>
        public const string RotationLocalDataId = "TransformLocalRotation";

        /// <summary>
        /// <see cref="Transform.localScale"/> id to persist in the <see cref="TimeStone"/>
        /// </summary>
        public const string ScaleDataId = "TransformScale";

        /// <summary>
        /// <see cref="Rigidbody.velocity"/> id to persist in the <see cref="TimeStone"/>
        /// </summary>
        public const string VelocityDataId = "RigidbodyVelocity";

        /// <summary>
        /// <see cref="Rigidbody.angularVelocity"/> id to persist in the <see cref="TimeStone"/>
        /// </summary>
        public const string AngularVelocityDataId = "RigidbodyAngularVelocity";

        /// <summary>
        /// <see cref="Animator"/>'s state hash where <see cref="DataIdUidToken"/> is replaced with a unique identifier using <see cref="GetUniqueDataId"/>
        /// </summary>
        public const string AnimatorStateHashDataId = "Animator" + DataIdUidToken + "StateHash";

        /// <summary>
        /// <see cref="Animator"/>'s state transition status where <see cref="DataIdUidToken"/> is replaced with a unique identifier using <see cref="GetUniqueDataId"/>
        /// </summary>
        public const string AnimatorStateInTransitionDataId = "Animator" + DataIdUidToken + "InTransition";

        /// <summary>
        /// <see cref="Animator"/>'s state layer weight where <see cref="DataIdUidToken"/> is replaced with a unique identifier using <see cref="GetUniqueDataId"/>
        /// </summary>
        public const string AnimatorStateLayerWeightDataId = "Animator" + DataIdUidToken + "LayerWeight";

        /// <summary>
        /// Animator state normalized time where <see cref="DataIdUidToken"/> is replaced with the layer num using <c>GetAnimatorDataIdWithLayer</c>
        /// </summary>
        public const string AnimatorStateNormalizedTimeDataId = "Animator" + DataIdUidToken + "StateNormalizedTime";

        /// <summary>
        /// <see cref="Animator"/>'s transition duration where <see cref="DataIdUidToken"/> is replaced with a unique identifier using <see cref="GetUniqueDataId"/>
        /// </summary>
        public const string AnimatorTransitionDurationDataId = "Animator" + DataIdUidToken + "TransitionDuration";

        /// <summary>
        /// <see cref="Animator"/>'s transition normalized time where <see cref="DataIdUidToken"/> is replaced with a unique identifier using <see cref="GetUniqueDataId"/>
        /// </summary>
        public const string AnimatorTransitionNormalizedTimeDataId =
            "Animator" + DataIdUidToken + "TransitionNormalizedTime";

        /// <summary>
        /// <see cref="Animator"/>'s transition next hash where <see cref="DataIdUidToken"/> is replaced with a unique identifier using <see cref="GetUniqueDataId"/>
        /// </summary>
        public const string AnimatorTransitionNextHashDataId = "Animator" + DataIdUidToken + "TransitionNextHash";

        /// <summary>
        /// <see cref="Animator"/>'s transition next normalized time where <see cref="DataIdUidToken"/> is replaced with a unique identifier using <see cref="GetUniqueDataId"/>
        /// </summary>
        public const string AnimatorTransitionNextNormalizedTimeDataId =
            "Animator" + DataIdUidToken + "TransitionNextNormalizedTime";

        /// <summary>
        /// ParticleSystem trails where <see cref="DataIdUidToken"/> is replaced with a unique identifier using <c>GetParticleSystemUniqueDataId</c>
        /// </summary>
        public const string ParticleSystemTrailsDataId = "ParticleSystem" + DataIdUidToken + "Trails";

        /// <summary>
        /// ParticleSystem particles where <see cref="DataIdUidToken"/> is replaced with a unique identifier using <c>GetParticleSystemUniqueDataId</c>
        /// </summary>
        public const string ParticleSystemParticlesDataId = "ParticleSystem" + DataIdUidToken + "Particles";

        /// <summary>
        /// ParticleSystem playback state where <see cref="DataIdUidToken"/> is replaced with a unique identifier using <c>GetParticleSystemUniqueDataId</c>
        /// </summary>
        public const string ParticleSystemPlaybackStateDataId = "ParticleSystem" + DataIdUidToken + "PlaybackState";


        /// <summary>
        /// The Simulation callback policies. The simulation callbacks are called on the <see cref="TimeAgent"/> by
        /// the <see cref="TimeStone"/> during a simulation execution.
        /// </summary>
        public enum SimulationCallbackPolicy
        {
            /// <summary>
            /// The callbacks are called on both the original TimeAgent and the simulation clone
            /// </summary>
            All,

            /// <summary>
            /// Only the original TimeAgent will receive the callbacks (to be honest, this policy does not really make
            /// any sense but... anyway
            /// </summary>
            OriginalOnly,

            /// <summary>
            /// Default policy : the callbacks are only called on the Cloned TimeAgent used by simulations.
            /// This policy makes code easier because you don't have to check for <see cref="TimeAgent.IsClone"/> in your
            /// callbacks if the original TimeAgent does not receive the callbacks
            /// </summary>
            CloneOnly
        }

        /// <summary>
        /// If this instance is the original TimeAgent, SimulationClone is a reference to the clone used in simulations by the stone
        /// </summary>
        public TimeAgent SimulationClone { get; set; }

        /// <summary>
        /// If this instance is the cloned TimeAgent for simulation, Original is a reference to the real object
        /// </summary>
        // public TimeAgent Original { get; set; }
        private TimeAgent _original;

        public TimeAgent Original
        {
            get => _original;
            set
            {
                _original = value;
                IsClone = value != null;
            }
        }

        /// <summary>
        /// Is this instance a clone used for simulation
        /// </summary>
        public bool IsClone { get; private set; }

        /// <summary>
        /// Unique ID shared by the original TimeAgent and the cloned TimeAgent used by simulations
        /// </summary>
        public Guid guid = Guid.NewGuid();

        /// <summary>
        /// TimeAgents will be searched in children on TimeStone initialization
        /// </summary>
        public bool parseChildren;

        /// <summary>
        /// Persist <see cref="GameObject.activeSelf"/> in <see cref="TimeStone"/>
        /// </summary>
        public bool persistActive = true;

        /// <summary>
        /// Persist <see cref="Transform.position"/> in <see cref="TimeStone"/>
        /// </summary>
        [Header("Persist Transform")] public bool persistPosition = true;

        public bool useLocalPosition;

        /// <summary>
        /// Persist <see cref="Transform.rotation"/> in <see cref="TimeStone"/>
        /// </summary>
        public bool persistRotation = true;

        public bool useLocalRotation;

        /// <summary>
        /// Persist <see cref="Transform.localScale"/> in <see cref="TimeStone"/>
        /// </summary>
        public bool persistScale;

        /// <summary>
        /// Persist <see cref="Rigidbody.velocity"/> and <see cref="Rigidbody.angularVelocity"/> in <see cref="TimeStone"/>
        /// </summary>
        [Header("Persist Physics")] public bool persistVelocity;

        /// <summary>
        /// Persist <see cref="Animator"/>'s state in <see cref="TimeStone"/>
        /// </summary>
        [Header("Persist Animator")] public bool persistAnimator;

        /// <summary>
        /// Persist <see cref="Animator"/>'s transitions between states.
        /// </summary>
        public bool persistAnimatorTransitions = true;

        /// <summary>
        /// The <see cref="Animator"/> component to persist. If not defined, automatically get gameObject's <see cref="Animator"/>
        /// </summary>
        public Animator agentAnimator;

        /// <summary>
        /// Persist <see cref="TimeAgent"/>'s particle systems in <see cref="TimeStone"/>
        /// </summary>
        [Header("Persist Particles")] public bool persistParticles;

        /// <summary>
        /// The list of <see cref="ParticleSystem"/> to persist. If empty, automatically get gameObject's <see cref="ParticleSystem"/>
        /// </summary>
        public List<ParticleSystem> persistParticlesList = new List<ParticleSystem>();

        /// <summary>
        /// Search for <see cref="ParticleSystem"/> in children
        /// </summary>
        public bool searchParticleChildren;

        /// <summary>
        /// How the simulation clone is hidden (or not)
        /// </summary>
        [Header("Clone Rendering")]
        public GameObjectUtils.HidePolicy hidePolicy = GameObjectUtils.HidePolicy.DisableRenderer;

        /// <summary>
        /// The material applied to the clone.
        /// If null, no material is applied.
        /// </summary>
        public Material cloneMaterial;

        /// <summary>
        /// Applies <see cref="cloneColor"/> to the clone material
        /// </summary>
        public bool applyColorToClone;

        /// <summary>
        /// The color applied to the clone if <see cref="applyColorToClone"/> is true
        /// </summary>
        public Color cloneColor = Color.green;


        /// <summary>
        /// If true, <see cref="cloneColor"/> and <see cref="cloneMaterial"/> are applied on clone's children recursively
        /// </summary>
        public bool applySimulationConfigOnChildren = true;

        /// <summary>
        /// The simulation events receive policy.
        /// In most of the cases, original time agents won't have any use of these events, so setting the policy
        /// to <see cref="SimulationCallbackPolicy.CloneOnly"/> there is no need in the code to check for <see cref="IsClone"/>
        /// </summary>
        [Header("Simulation Config")]
        public SimulationCallbackPolicy simulationCallbackPolicy = SimulationCallbackPolicy.CloneOnly;

        /// <summary>
        /// Cached value of Rigidbody
        /// </summary>
        [HideInInspector] public Rigidbody agentRigidbody;

        /// <summary>
        /// Log all messages
        /// </summary>
        [Header("Logs")] public bool logDebug;

        [FormerlySerializedAs("onInitTimeAgentsList")] [SerializeField]
        private OnInitTimeAgentsListEvent _onInitTimeAgentsList = new OnInitTimeAgentsListEvent();

        public OnInitTimeAgentsListEvent onInitTimeAgentsList
        {
            get => _onInitTimeAgentsList;
            set => _onInitTimeAgentsList = value;
        }

        [FormerlySerializedAs("onTimeLineChange")] [SerializeField]
        private OnTimeLineChangeEvent _onTimeLineChange = new OnTimeLineChangeEvent();

        public OnTimeLineChangeEvent onTimeLineChange
        {
            get => _onTimeLineChange;
            set => _onTimeLineChange = value;
        }


        [FormerlySerializedAs("onSimulationStart")] [SerializeField]
        private OnSimulationStartEvent _onSimulationStart = new OnSimulationStartEvent();

        public OnSimulationStartEvent onSimulationStart
        {
            get => _onSimulationStart;
            set => _onSimulationStart = value;
        }

        [FormerlySerializedAs("onSimulationUpdate")] [SerializeField]
        private OnSimulationUpdateEvent _onSimulationUpdate = new OnSimulationUpdateEvent();

        public OnSimulationUpdateEvent onSimulationUpdate
        {
            get => _onSimulationUpdate;
            set => _onSimulationUpdate = value;
        }

        [FormerlySerializedAs("onSimulationFixedUpdate")] [SerializeField]
        private OnSimulationFixedUpdateEvent _onSimulationFixedUpdate = new OnSimulationFixedUpdateEvent();

        public OnSimulationFixedUpdateEvent onSimulationFixedUpdate
        {
            get => _onSimulationFixedUpdate;
            set => _onSimulationFixedUpdate = value;
        }

        [FormerlySerializedAs("onSimulationLateUpdate")] [SerializeField]
        private OnSimulationLateUpdateEvent _onSimulationLateUpdate = new OnSimulationLateUpdateEvent();

        public OnSimulationLateUpdateEvent onSimulationLateUpdate
        {
            get => _onSimulationLateUpdate;
            set => _onSimulationLateUpdate = value;
        }

        [FormerlySerializedAs("onSimulationComplete")] [SerializeField]
        private OnSimulationCompleteEvent _onSimulationComplete = new OnSimulationCompleteEvent();

        public OnSimulationCompleteEvent onSimulationComplete
        {
            get => _onSimulationComplete;
            set => _onSimulationComplete = value;
        }

        [FormerlySerializedAs("onPersistTick")] [SerializeField]
        private OnPersistTickEvent _onPersistTick = new OnPersistTickEvent();

        public OnPersistTickEvent onPersistTick
        {
            get => _onPersistTick;
            set => _onPersistTick = value;
        }

        [FormerlySerializedAs("onSetDataTick")] [SerializeField]
        private OnSetDataTickEvent _onSetDataTick = new OnSetDataTickEvent();

        public OnSetDataTickEvent onSetDataTick
        {
            get => _onSetDataTick;
            set => _onSetDataTick = value;
        }

        /// <summary>
        /// Flag to know if gameObject has a <see cref="Rigidbody"/> component 
        /// </summary>
        private bool _hasRigidbody;

        /// <summary>
        /// Flag to know if gameObject has <see cref="Animator"/> component
        /// </summary>
        private bool _hasAnimator;

        /// <summary>
        /// Cached <see cref="GameObject.activeSelf"/> value of the original <see cref="TimeAgent"/> when starting the simulation
        /// </summary>
        private bool _initActive;

        /// <summary>
        /// Cached <see cref="Rigidbody.velocity"/> value of the original <see cref="TimeAgent"/> when starting a simulation
        /// </summary>
        private Vector3 _initRigidbodyVelocity;

        /// <summary>
        /// Cached <see cref="Rigidbody.angularVelocity"/> value of the original <see cref="TimeAgent"/> when starting a simulation
        /// </summary>
        private Vector3 _initRigidbodyAngularVelocity;

        /// <summary>
        /// Cached <see cref="Animator"/>'s state value of the original <see cref="TimeAgent"/> when starting a simulation
        /// Index of the list correspond to the layer number
        /// </summary>
        private readonly List<(int, float)> _initAnimatorState = new List<(int hash, float normalizedTime)>();

        /// <summary>
        /// Cached <see cref="Animator"/>'s transition value of the original <see cref="TimeAgent"/> when starting a simulation
        /// Index of the list correspond to the layer number
        /// </summary>
        private readonly List<(bool, float, float, int, float)> _initAnimatorTransition =
            new List<(bool inTransition, float transitionDuration, float transitionNormalizedTime, int nextHash, float
                nextNormalizedTime)>();

        /// <summary>
        /// Cached particle systems state of the original <see cref="TimeAgent"/> when starting a simulation
        /// </summary>
        private readonly List<(ParticleSystem.PlaybackState, ParticleSystem.Trails, ParticleSystem.Particle[])>
            _initParticlesState =
                new List<(ParticleSystem.PlaybackState, ParticleSystem.Trails, ParticleSystem.Particle[])>();


        public virtual void Awake()
        {
            // try get components, not using persistVelocity or persistAnimator fields here
            // to let the user the freedom of activating them later if needed
            _hasRigidbody = TryGetComponent(out agentRigidbody);
            _hasAnimator = agentAnimator != null || TryGetComponent(out agentAnimator);

            if (persistParticles && persistParticlesList.Count == 0)
            {
                // If particle systems are not explicitly defined, we get them automatically from component 
                var componentParticleSystems = GetComponents<ParticleSystem>();
                if (componentParticleSystems.Length > 0)
                {
                    persistParticlesList.AddRange(componentParticleSystems);
                }

                if (searchParticleChildren)
                {
                    var childrenParticles = GetComponentsInChildren<ParticleSystem>();
                    persistParticlesList.AddRange(childrenParticles);
                }
            }
        }

        /// <summary>
        /// Cache initialization data from the Original object to apply them on the cloned Simulation object when Simulation starts
        /// </summary>
        protected void CacheInitDataFromOriginal()
        {
            if (persistActive)
                _initActive = gameObject.activeSelf;

            if (_hasRigidbody)
            {
                _initRigidbodyVelocity = Original.agentRigidbody.velocity;
                _initRigidbodyAngularVelocity = Original.agentRigidbody.angularVelocity;
            }

            if (_hasAnimator)
            {
                for (var i = 0; i < agentAnimator.layerCount; i++)
                {
                    var layerNum = i;
                    var stateInfo = Original.agentAnimator.GetCurrentAnimatorStateInfo(layerNum);
                    var stateHash = stateInfo.fullPathHash;
                    var stateNormalizedTime = stateInfo.normalizedTime;

                    _initAnimatorState.Add((stateHash, stateNormalizedTime));

                    if (persistAnimatorTransitions)
                    {
                        var stateInTransition = Original.agentAnimator.IsInTransition(layerNum);
                        if (!stateInTransition)
                        {
                            _initAnimatorTransition.Add((false, 0, 0, 0, 0));
                        }
                        else
                        {
                            AnimatorTransitionInfo transInfo =
                                Original.agentAnimator.GetAnimatorTransitionInfo(layerNum);
                            AnimatorStateInfo nextInfo = Original.agentAnimator.GetNextAnimatorStateInfo(layerNum);

                            var transitionDuration = transInfo.duration;
                            var transitionNormalizedTime = transInfo.normalizedTime;
                            var nextHash = nextInfo.fullPathHash;
                            var nextNormalizedTime = nextInfo.normalizedTime;
                            _initAnimatorTransition.Add((true, transitionDuration, transitionNormalizedTime, nextHash,
                                nextNormalizedTime));
                        }
                    }
                }
            }

            if (persistParticlesList.Count > 0)
            {
                for (var i = 0; i < persistParticlesList.Count; i++)
                {
                    var psOriginal = Original.persistParticlesList[i];

                    var playbackState = psOriginal.GetPlaybackState();
                    var psOriginalParticles = new ParticleSystem.Particle[psOriginal.particleCount];
                    psOriginal.GetParticles(psOriginalParticles);
                    var psTrails = psOriginal.GetTrails();

                    _initParticlesState.Add((playbackState, psTrails, psOriginalParticles));
                }
            }
        }

        /// <summary>
        /// Applies the data cached on simulation initialization
        /// </summary>
        protected void ApplyCachedInitDataOnClone()
        {
            if (!IsClone) return;

            if (persistActive)
                gameObject.SetActive(_initActive);

            // Rigidbody velocity
            if (_hasRigidbody)
            {
                agentRigidbody.velocity = _initRigidbodyVelocity;
                agentRigidbody.angularVelocity = _initRigidbodyAngularVelocity;
            }

            // Animator state
            if (_hasAnimator)
            {
                for (var i = 0; i < _initAnimatorState.Count; i++)
                {
                    agentAnimator.Play(_initAnimatorState[i].Item1, i, _initAnimatorState[i].Item2);

                    var (inTransition, transitionDuration, transitionNormalizedTime, nextHash, nextHashNormalizedTime) =
                        _initAnimatorTransition[i];
                    if (!inTransition) continue;

                    agentAnimator.Update(0.0f);
                    agentAnimator.CrossFade(nextHash, transitionDuration, i,
                        nextHashNormalizedTime,
                        transitionNormalizedTime);
                }

                // Disabling the animator to control it manually during simulation
                agentAnimator.speed = 0.0f;
            }

            if (persistParticlesList.Count > 0)
            {
                for (var i = 0; i < persistParticlesList.Count; i++)
                {
                    var ps = persistParticlesList[i];
                    ps.SetPlaybackState(_initParticlesState[i].Item1);
                    ps.SetTrails(_initParticlesState[i].Item2);
                    ps.SetParticles(_initParticlesState[i].Item3);

                    ps.Play();
                }
            }
        }

        /// <summary>
        /// Cloning method called by the TimeStone when creating a simulation scene.
        /// You can override it to modify the way the object is cloned. 
        /// </summary>
        /// <returns></returns>
        public virtual GameObject GetClone()
        {
            var t = transform;
            return Instantiate(gameObject, t.position, t.rotation);
        }

        /// <summary>
        /// InitTimeAgentsList callback called by timeStone
        /// </summary>
        /// <remarks>Use UnityEvent or override to use. UnityEvent can't be received with simulation clones, they do not exist when stone inits his time agents</remarks>
        /// <remarks><see cref="TimeStone"/> is given as a parameter in case you have multiple <see cref="TimeStone"/> using the same <see cref="TimeAgent"/></remarks>
        /// <param name="stone">The <see cref="TimeStone"/> that called this method</param>
        public virtual void InitTimeAgentsList(TimeStone stone)
        {
            _onInitTimeAgentsList.Invoke(stone);
        }

        /// <summary>
        /// SimulationSceneReady callback called by timeStone
        /// </summary>
        /// <remarks>Override to use. UnityEvent can't be used here with clones because object has just been cloned when it's called and can't receive event at that moment.</remarks>
        /// <remarks><see cref="TimeStone"/> is given as a parameter in case you have multiple <see cref="TimeStone"/> using the same <see cref="TimeAgent"/></remarks>
        /// <param name="stone">The <see cref="TimeStone"/> that called this method</param>
        public virtual void SimulationSceneReady(TimeStone stone)
        {
            if (IsClone)
            {
                // Initialization data are stored because Cloning objects with <c>Instantiate</c> does not copy private fields
                // So we need to get current data from the real TimeAgent manually to apply them on the simulation TimeAgent
                // We need to store them on <c>StoneSimulationSceneReady</c> because they have to be applied on <c>SimulationStart</c> and
                // original TimeAgents could have been modified between <c>StoneSimulationSceneReady</c> and <c>SimulationStart</c>
                CacheInitDataFromOriginal();
            }
        }

        /// <summary>
        /// ClonedForSimulation callback called by timeStone
        /// </summary>
        /// <remarks>Override to use. UnityEvent can't be used here because object has just been cloned when it's called and can't receive event at that moment.</remarks>
        /// <remarks><see cref="TimeStone"/> is given as a parameter in case you have multiple <see cref="TimeStone"/> using the same <see cref="TimeAgent"/></remarks>
        /// <param name="stone">The <see cref="TimeStone"/> that called this method</param>
        public virtual void ClonedForSimulation(TimeStone stone)
        {
            if (IsClone)
            {
                GameObjectUtils.Hide(gameObject, hidePolicy, applySimulationConfigOnChildren);
                GameObjectUtils.ApplyColorAndMaterial(gameObject, applySimulationConfigOnChildren, cloneMaterial,
                    applyColorToClone, cloneColor);
            }
        }

        /// <summary>
        /// TimeLineChange callback called by timeStone
        /// </summary>
        /// <remarks>Use UnityEvent or override to use</remarks>
        /// <remarks><see cref="TimeStone"/> is given as a parameter in case you have multiple <see cref="TimeStone"/> using the same <see cref="TimeAgent"/></remarks>
        /// <param name="stone">The <see cref="TimeStone"/> that called this method</param>
        /// <param name="origin">The origin of the change, Record or Simulation</param>
        public virtual void TimeLineChange(TimeStone stone, TimeStone.TimeTickOrigin origin)
        {
            if (!isActiveAndEnabled) return;
            _onTimeLineChange.Invoke(stone, origin);
        }

        /// <summary>
        /// SimulationStart callback called by timeStone
        /// </summary>
        /// <remarks>Use UnityEvent or override to add code during simulation execution</remarks>
        /// <remarks><see cref="TimeStone"/> is given as a parameter in case you have multiple <see cref="TimeStone"/> using the same <see cref="TimeAgent"/></remarks>
        /// <param name="stone">The <see cref="TimeStone"/> that called this method></param>
        public virtual void SimulationStart(TimeStone stone)
        {
            ApplyCachedInitDataOnClone();
            if (!isActiveAndEnabled) return;
            _onSimulationStart.Invoke(stone);
        }

        /// <summary>
        /// SimulationUpdate callback called by timeStone.
        /// </summary>
        /// <remarks>Use UnityEvent or override to add code during simulation execution</remarks>
        /// <remarks><see cref="TimeStone"/> is given as a parameter in case you have multiple <see cref="TimeStone"/> using the same <see cref="TimeAgent"/></remarks>
        /// <param name="step">The time step (deltaTime) since last update call</param>
        /// <param name="stone">The <see cref="TimeStone"/> that called this method></param>
        public virtual void SimulationUpdate(float step, TimeStone stone)
        {
            if (!isActiveAndEnabled) return;

            if (IsClone)
            {
                if (persistAnimator)
                {
                    // Activates the Animator just to manually update it states then stops it
                    agentAnimator.speed = 1.0f;
                    agentAnimator.Update(step);
                    agentAnimator.speed = 0.0f;
                }

                if (persistParticlesList.Count > 0)
                {
                    foreach (var ps in persistParticlesList)
                    {
                        // Manually simulating particle systems
                        // withChildren is false as we are explicitly acting on all particle systems
                        ps.Simulate(step, false, false);
                    }
                }
            }

            _onSimulationUpdate.Invoke(stone, step);
        }

        /// <summary>
        /// SimulationUpdate callback called by timeStone
        /// </summary>
        /// <remarks>Use UnityEvent or override to add code during simulation execution</remarks>
        /// <remarks><see cref="TimeStone"/> is given as a parameter in case you have multiple <see cref="TimeStone"/> using the same <see cref="TimeAgent"/></remarks>
        /// <param name="fixedStep">The time fixedStep (fixedDeltaTime) since last fixedUpdate call</param>
        /// <param name="stone">The <see cref="TimeStone"/> that called this method></param>
        public virtual void SimulationFixedUpdate(float fixedStep, TimeStone stone)
        {
            if (!isActiveAndEnabled) return;
            _onSimulationFixedUpdate.Invoke(stone, fixedStep);
        }

        /// <summary>
        /// SimulationLateUpdate callback called by timeStone
        /// </summary>
        /// <remarks>Use UnityEvent or override to use</remarks>
        /// <remarks><see cref="TimeStone"/> is given as a parameter in case you have multiple <see cref="TimeStone"/> using the same <see cref="TimeAgent"/></remarks>
        /// <param name="step">The time step (deltaTime) since last lateUpdate call</param>
        /// <param name="stone">The <see cref="TimeStone"/> that called this method></param>
        public virtual void SimulationLateUpdate(float step, TimeStone stone)
        {
            if (!isActiveAndEnabled) return;
            _onSimulationLateUpdate.Invoke(stone, step);
        }

        /// <summary>
        /// SimulationComplete callback called by timeStone
        /// </summary>
        /// <remarks>Use UnityEvent or override to use</remarks>
        /// <remarks><see cref="TimeStone"/> is given as a parameter in case you have multiple <see cref="TimeStone"/> using the same <see cref="TimeAgent"/></remarks>
        /// <param name="stone">The <see cref="TimeStone"/> that called this method></param>
        public virtual void SimulationComplete(TimeStone stone)
        {
            if (!isActiveAndEnabled) return;
            _onSimulationComplete.Invoke(stone);
        }

        /// <summary>
        /// PersistTick callback called by the stone when it persist TimeAgents state for a tick.
        /// This is the callback where you store custom data of your gameObject. 
        /// 
        /// </summary>
        /// <remarks>Use UnityEvent or override to use</remarks>
        /// <remarks>You can use <see cref="GenericTimeAgent"/> to use it in a simple way, but be careful with performances as it relies on Reflection</remarks>
        /// <remarks><see cref="TimeStone"/> is given as a parameter in case you have multiple <see cref="TimeStone"/> using the same <see cref="TimeAgent"/></remarks>
        /// <param name="stone">The <see cref="TimeStone"/> that called this method></param>
        /// <param name="origin">The origin of the call, Record or Simulation</param>
        /// <param name="deltaTime">The time since the last tick performed, equivalent to using <see cref="Time.deltaTime"/> when coding MonoBehaviour.Update callback</param>
        public virtual void PersistTick(TimeStone stone, TimeStone.TimeTickOrigin origin, float deltaTime)
        {
            // Persist GameObject's state regardless of... its state, or we would never persist inactive state
            PersistGameObjectState(stone, origin, deltaTime);

            if (!isActiveAndEnabled) return;

            PersistTransformData(stone, origin, deltaTime);
            PersistRigidbodyData(stone, origin, deltaTime);
            PersistAnimatorData(stone, origin, deltaTime);
            PersistParticlesData(stone, origin, deltaTime);

            _onPersistTick.Invoke(stone, origin, deltaTime);
        }

        /// <summary>
        /// SetDataTick callback called by the stone when it applies a tick's state to TimeAgents
        /// This is the callback where you apply your custom data of your gameObject. 
        /// </summary>
        /// <remarks>Use UnityEvent or override to use</remarks>
        /// <remarks>You can use <see cref="GenericTimeAgent"/> to use it in a simple way, but be careful with performances as it relies on Reflection</remarks>
        /// <remarks><see cref="TimeStone"/> is given as a parameter in case you have multiple <see cref="TimeStone"/> using the same <see cref="TimeAgent"/></remarks>
        /// <param name="stone">The <see cref="TimeStone"/> that called this method></param>
        /// <param name="tick">The tick index of the TimeStone's timeLine that is applied to the GameObject</param>
        public virtual void SetDataTick(TimeStone stone, int tick)
        {
            SetGameObjectState(stone, tick);

            if (!isActiveAndEnabled) return;

            SetTransformData(stone, tick);
            SetRigidbodyData(stone, tick);
            SetAnimatorData(stone, tick);
            SetParticlesData(stone, tick);

            _onSetDataTick.Invoke(stone, tick);
        }

        /// <summary>
        /// Perists GameObject's data
        /// </summary>
        /// <param name="stone">The calling <see cref="TimeStone"/></param>
        /// <param name="origin">The origin of the call, Record or Simulation</param>
        /// <param name="deltaTime">The delta time</param>
        protected void PersistGameObjectState(TimeStone stone, TimeStone.TimeTickOrigin origin, float deltaTime)
        {
            if (persistActive)
                stone.PersistData(this, origin, deltaTime, ActiveDataId, gameObject.activeSelf);
        }

        /// <summary>
        /// Persists Transform's data
        /// </summary>
        /// <param name="stone">The calling <see cref="TimeStone"/></param>
        /// <param name="origin">The origin of the call, Record or Simulation</param>
        /// <param name="deltaTime">The delta time</param>
        protected void PersistTransformData(TimeStone stone, TimeStone.TimeTickOrigin origin, float deltaTime)
        {
            if (persistPosition)
            {
                stone.PersistData(this, origin, deltaTime, PositionLocalDataId, transform.localPosition);
                stone.PersistData(this, origin, deltaTime, PositionDataId, transform.position);
            }

            if (persistRotation)
            {
                stone.PersistData(this, origin, deltaTime, RotationLocalDataId, transform.localRotation);
                stone.PersistData(this, origin, deltaTime, RotationDataId, transform.rotation);
            }

            if (persistScale)
                stone.PersistData(this, origin, deltaTime, ScaleDataId, transform.localScale);
        }

        /// <summary>
        /// Persists Rigidbody's data
        /// </summary>
        /// <param name="stone">The calling <see cref="TimeStone"/></param>
        /// <param name="origin">The origin of the call, Record or Simulation</param>
        /// <param name="deltaTime">The delta time</param>
        protected void PersistRigidbodyData(TimeStone stone, TimeStone.TimeTickOrigin origin, float deltaTime)
        {
            if (!persistVelocity || !_hasRigidbody) return;

            stone.PersistData(this, origin, deltaTime, VelocityDataId, agentRigidbody.velocity);
            stone.PersistData(this, origin, deltaTime, AngularVelocityDataId, agentRigidbody.angularVelocity);
        }

        /// <summary>
        /// Persists Animator's data
        /// </summary>
        /// <param name="stone">The calling <see cref="TimeStone"/></param>
        /// <param name="origin">The origin of the call, Record or Simulation</param>
        /// <param name="deltaTime">The delta time</param>
        private void PersistAnimatorData(TimeStone stone, TimeStone.TimeTickOrigin origin, float deltaTime)
        {
            if (!persistAnimator || !_hasAnimator) return;

            for (var i = 0; i < agentAnimator.layerCount; i++)
            {
                var layerNum = i;
                var stateInfo = agentAnimator.GetCurrentAnimatorStateInfo(layerNum);
                var stateHash = stateInfo.fullPathHash;
                var layerWeight = agentAnimator.GetLayerWeight(layerNum);
                var stateNormalizedTime = stateInfo.normalizedTime;
                var stateInTransition = agentAnimator.IsInTransition(layerNum);
                stone.PersistData(this, origin, deltaTime, GetUniqueDataId(AnimatorStateLayerWeightDataId, layerNum),
                    layerWeight);
                stone.PersistData(this, origin, deltaTime, GetUniqueDataId(AnimatorStateHashDataId, layerNum),
                    stateHash);
                stone.PersistData(this, origin, deltaTime,
                    GetUniqueDataId(AnimatorStateNormalizedTimeDataId, layerNum),
                    stateNormalizedTime);
                stone.PersistData(this, origin, deltaTime, GetUniqueDataId(AnimatorStateInTransitionDataId, layerNum),
                    stateInTransition);

                if (stateInTransition && persistAnimatorTransitions)
                {
                    AnimatorTransitionInfo transInfo = agentAnimator.GetAnimatorTransitionInfo(layerNum);
                    AnimatorStateInfo nextInfo = agentAnimator.GetNextAnimatorStateInfo(layerNum);

                    var transitionDuration = transInfo.duration;
                    var transitionNormalizedTime = transInfo.normalizedTime;
                    var nextHash = nextInfo.fullPathHash;
                    var nextNormalizedTime = nextInfo.normalizedTime;
                    stone.PersistData(this, origin, deltaTime,
                        GetUniqueDataId(AnimatorTransitionDurationDataId, layerNum),
                        transitionDuration);
                    stone.PersistData(this, origin, deltaTime,
                        GetUniqueDataId(AnimatorTransitionNormalizedTimeDataId, layerNum),
                        transitionNormalizedTime);
                    stone.PersistData(this, origin, deltaTime,
                        GetUniqueDataId(AnimatorTransitionNextHashDataId, layerNum),
                        nextHash);
                    stone.PersistData(this, origin, deltaTime,
                        GetUniqueDataId(AnimatorTransitionNextNormalizedTimeDataId, layerNum),
                        nextNormalizedTime);
                }
            }
        }

        /// <summary>
        /// Persists ParticleSystems data
        /// </summary>
        /// <param name="stone">The calling <see cref="TimeStone"/></param>
        /// <param name="origin">The origin of the call, Record or Simulation</param>
        /// <param name="deltaTime">The delta time</param>
        private void PersistParticlesData(TimeStone stone, TimeStone.TimeTickOrigin origin, float deltaTime)
        {
            if (!persistParticles || persistParticlesList.Count <= 0) return;

            for (var i = 0; i < persistParticlesList.Count; i++)
            {
                var ps = persistParticlesList[i];
                stone.PersistData(this, origin, deltaTime, GetUniqueDataId(ParticleSystemPlaybackStateDataId, i),
                    ps.GetPlaybackState());
                ParticleSystem.Particle[] psParticles = new ParticleSystem.Particle[ps.particleCount];
                ps.GetParticles(psParticles);
                stone.PersistData(this, origin, deltaTime,
                    GetUniqueDataId(ParticleSystemParticlesDataId, i),
                    psParticles);
                stone.PersistData(this, origin, deltaTime, GetUniqueDataId(ParticleSystemTrailsDataId, i),
                    ps.GetTrails());
            }
        }

        /// <summary>
        /// Applies GameObject's data
        /// </summary>
        /// <param name="stone">The calling <see cref="TimeStone"/></param>
        /// <param name="tick">The tick index of the TimeStone's timeLine that is applied to the GameObject</param>
        private void SetGameObjectState(TimeStone stone, int tick)
        {
            if (persistActive)
            {
                var oldActiveSelf = gameObject.activeSelf;
                var activeSelf = stone.GetDataValueAt<bool>(tick, this, ActiveDataId, out var dataExist);
                if (dataExist && activeSelf != oldActiveSelf)
                    gameObject.SetActive(activeSelf);
            }
        }

        /// <summary>
        /// Applies Transform's data
        /// </summary>
        /// <param name="stone">The calling <see cref="TimeStone"/></param>
        /// <param name="tick">The tick index of the TimeStone's timeLine that is applied to the GameObject</param>
        private void SetTransformData(TimeStone stone, int tick)
        {
            bool dataExist;
            if (persistPosition)
            {
                Vector3 position;
                if (useLocalPosition)
                    position = stone.GetDataValueAt<Vector3>(tick, this, PositionLocalDataId, out dataExist);
                else
                    position = stone.GetDataValueAt<Vector3>(tick, this, PositionDataId, out dataExist);
                if (dataExist)
                {
                    if (useLocalPosition)
                        transform.localPosition = position;
                    else
                        transform.position = position;
                }
            }

            if (persistRotation)
            {
                Quaternion rotation;
                if (useLocalRotation)
                    rotation = stone.GetDataValueAt<Quaternion>(tick, this, RotationLocalDataId, out dataExist);
                else
                    rotation = stone.GetDataValueAt<Quaternion>(tick, this, RotationDataId, out dataExist);
                if (dataExist)
                {
                    if (useLocalRotation)
                        transform.localRotation = rotation;
                    else
                        transform.rotation = rotation;
                }
            }

            if (persistScale)
            {
                var scale = stone.GetDataValueAt<Vector3>(tick, this, ScaleDataId, out dataExist);
                if (dataExist)
                    transform.localScale = scale;
            }
        }

        /// <summary>
        /// Applies ParticleSystems data
        /// </summary>
        /// <param name="stone">The calling <see cref="TimeStone"/></param>
        /// <param name="tick">The tick index of the TimeStone's timeLine that is applied to the GameObject</param>
        private void SetParticlesData(TimeStone stone, int tick)
        {
            if (!persistParticles || persistParticlesList.Count <= 0) return;

            for (var i = 0; i < persistParticlesList.Count; i++)
            {
                var ps = persistParticlesList[i];
                var playbackState = stone.GetDataValueAt<ParticleSystem.PlaybackState>(tick, this,
                    GetUniqueDataId(ParticleSystemPlaybackStateDataId, i), out var dataExist);
                if (dataExist)
                    ps.SetPlaybackState(playbackState);

                var psParticles = stone.GetDataValueAt<ParticleSystem.Particle[]>(tick, this,
                    GetUniqueDataId(ParticleSystemParticlesDataId, i), out dataExist);
                if (dataExist)
                    ps.SetParticles(psParticles);

                var psTrails = stone.GetDataValueAt<ParticleSystem.Trails>(tick, this,
                    GetUniqueDataId(ParticleSystemTrailsDataId, i), out dataExist);
                if (dataExist)
                    ps.SetTrails(psTrails);
            }
        }

        /// <summary>
        /// Applies Animator's data
        /// </summary>
        /// <param name="stone">The calling <see cref="TimeStone"/></param>
        /// <param name="tick">The tick index of the TimeStone's timeLine that is applied to the GameObject</param>
        private void SetAnimatorData(TimeStone stone, int tick)
        {
            if (!persistAnimator || !_hasAnimator) return;
            
            for (var i = 0; i < agentAnimator.layerCount; i++)
            {
                bool allAnimatorDataExist;
                var layerNum = i;

                var stateHash = stone.GetDataValueAt<int>(tick, this,
                    GetUniqueDataId(AnimatorStateHashDataId, layerNum), out var dataExist);
                allAnimatorDataExist = dataExist;

                var stateNormalizedTime = stone.GetDataValueAt<float>(tick, this,
                    GetUniqueDataId(AnimatorStateNormalizedTimeDataId, layerNum), out dataExist);
                allAnimatorDataExist = allAnimatorDataExist && dataExist;

                var layerWeight = stone.GetDataValueAt<float>(tick, this,
                    GetUniqueDataId(AnimatorStateLayerWeightDataId, layerNum), out dataExist);
                allAnimatorDataExist = allAnimatorDataExist && dataExist;

                if (allAnimatorDataExist)
                {
                    agentAnimator.SetLayerWeight(layerNum, layerWeight);
                    agentAnimator.Play(stateHash, layerNum, stateNormalizedTime);
                }

                if (!persistAnimatorTransitions) continue;

                var inTransition = stone.GetDataValueAt<bool>(tick, this,
                    GetUniqueDataId(AnimatorStateInTransitionDataId, layerNum), out dataExist);

                if (!dataExist || !inTransition) continue;

                bool allTransitionDataExist;
                var transitionDuration = stone.GetDataValueAt<float>(tick, this,
                    GetUniqueDataId(AnimatorTransitionDurationDataId, layerNum), out dataExist);
                allTransitionDataExist = dataExist;
                var transitionNormalizedTime = stone.GetDataValueAt<float>(tick, this,
                    GetUniqueDataId(AnimatorTransitionNormalizedTimeDataId, layerNum), out dataExist);
                allTransitionDataExist = allTransitionDataExist && dataExist;
                var nextHash = stone.GetDataValueAt<int>(tick, this,
                    GetUniqueDataId(AnimatorTransitionNextHashDataId, layerNum), out dataExist);
                allTransitionDataExist = allTransitionDataExist && dataExist;
                var nextNormalizedTime = stone.GetDataValueAt<float>(tick, this,
                    GetUniqueDataId(AnimatorTransitionNextNormalizedTimeDataId, layerNum), out dataExist);
                allTransitionDataExist = allTransitionDataExist && dataExist;
                    
                if (!allTransitionDataExist) continue;
                    
                // Debug.Log("Applying transition : " + nextHash + " / " + transitionDuration + " / " + layerNum +
                //           " / " + nextNormalizedTime + " / " + transitionNormalizedTime);
                agentAnimator.Update(0.0f);
                agentAnimator.CrossFade(nextHash, transitionDuration, layerNum, nextNormalizedTime,
                    transitionNormalizedTime);
            }
        }

        /// <summary>
        /// Applies Rigidbody's data
        /// </summary>
        /// <param name="stone">The calling <see cref="TimeStone"/></param>
        /// <param name="tick">The tick index of the TimeStone's timeLine that is applied to the GameObject</param>
        private void SetRigidbodyData(TimeStone stone, int tick)
        {
            if (persistVelocity && _hasRigidbody)
            {
                var velocity = stone.GetDataValueAt<Vector3>(tick, this, VelocityDataId, out var dataExist);
                if (dataExist)
                    agentRigidbody.velocity = velocity;
                var angularVelocity = stone.GetDataValueAt<Vector3>(tick, this, AngularVelocityDataId, out dataExist);
                if (dataExist)
                    agentRigidbody.angularVelocity = angularVelocity;
            }
        }

        /// <summary>
        /// Set data from the stone but using time
        /// </summary>
        /// <param name="stone">The calling <see cref="TimeStone"/></param>
        /// <param name="time">The time to apply</param>
        public virtual void SetDataTime(TimeStone stone, float time)
        {
            var tickIndex = stone.GetTickIndexAtTime(time);
            SetDataTick(stone, tickIndex);
        }

        /// <summary>
        /// Replaces the <see cref="DataIdUidToken"/> with the given id in the dataId
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetUniqueDataId(string dataId, int id)
        {
            return dataId.Replace(DataIdUidToken, id.ToString());
        }

        [Serializable]
        public class OnInitTimeAgentsListEvent : UnityEvent<TimeStone>
        {
        }

        [Serializable]
        public class OnTimeLineChangeEvent : UnityEvent<TimeStone, TimeStone.TimeTickOrigin>
        {
        }

        [Serializable]
        public class OnSimulationStartEvent : UnityEvent<TimeStone>
        {
        }

        [Serializable]
        public class OnSimulationFixedUpdateEvent : UnityEvent<TimeStone, float>
        {
        }

        [Serializable]
        public class OnSimulationUpdateEvent : UnityEvent<TimeStone, float>
        {
        }

        [Serializable]
        public class OnSimulationLateUpdateEvent : UnityEvent<TimeStone, float>
        {
        }

        [Serializable]
        public class OnSimulationCompleteEvent : UnityEvent<TimeStone>
        {
        }

        [Serializable]
        public class OnPersistTickEvent : UnityEvent<TimeStone, TimeStone.TimeTickOrigin, float>
        {
        }

        [Serializable]
        public class OnSetDataTickEvent : UnityEvent<TimeStone, int>
        {
        }
    }
}