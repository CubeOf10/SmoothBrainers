using System;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RedGirafeGames.Agamotto.Scripts.Runtime.Visualizers
{
    [AddComponentMenu("Agamotto/Path Visualizer")]
    [RequireComponent(typeof(LineRenderer))]
    public class PathVisualizer : AbstractTimeVisualizer
    {
        /// <summary>
        /// The LineRenderer displaying the path, each point correspond to a tick in the TimeStone's TimeLine
        /// </summary>
        [Header("Path Config")]
        [Tooltip("The LineRenderer displaying the path, each point correspond to a tick in the TimeStone's TimeLine. Automatically defined if not set")]
        public LineRenderer lineRenderer;

        /// <summary>
        /// Tolerance for <see cref="LineRenderer.Simplify"/> calls during PathVisualizer update
        /// 0 = same line, higher value = better perf and less precision.
        /// See LineRenderer documentation for more details
        /// </summary>
        [Space]
        [Tooltip("Tolerance used to simplify (using LineRenderer.Simplify) the path. Set 0 to disable simplification")]
        public float simplifyTolerance = 0.1f;

        /// <summary>
        /// Tolerance for <see cref="LineRenderer.Simplify"/> calls during PathVisualizer dynamic updates (timeLine change)
        /// This value should be lower than <see cref="simplifyTolerance"/> because the simplification will be made while
        /// the line is created, therefore the "breaking" point could be simplified while it shouldn't
        /// </summary>
        [Tooltip("Tolerance used to simplify the path during dynamic updates like recording.")]
        public float dynamicSimplifyTolerance = 0.01f;

        /// <summary>
        /// Offset applied to each point, useful to display the line above the ground if the TimeAgent origin is at ground level for example.
        /// </summary>
        [Space]
        [Tooltip("Offset applied to each point, useful to display the line above the ground if the TimeAgent origin is at ground level for example")]
        public Vector3 pointsOffset;

        /// <summary>
        /// Flag to know when we start a new simulation, so we can do a fresh update on its first data streamed (for async executions)
        /// </summary>
        private bool _newSimulationStart;

        /// <summary>
        /// The last tick event handled by the component. Useful to update only what is needed when
        /// TimeLineChange events occurs for Record and asynchronous Simulation
        /// </summary>
        private int _lastChangeEventTickIndex;

        private void Start()
        {
            InitLineRenderer();
        }

        private void InitLineRenderer()
        {
            if (lineRenderer != null) return;

            lineRenderer = GetComponent<LineRenderer>();

            if (logDebug)
                Debug.Log("[PathVisualizer] " + name + " : Auto assigning LineRenderer <" + lineRenderer.name +
                          ">");
        }

        public override void ClearVisualization(int fromIndex = 0)
        {
            base.ClearVisualization(fromIndex);

            _lastChangeEventTickIndex = fromIndex;

            if (logDebug)
                Debug.Log("[PathVisualizer] " + name + " : Clear visualization from index <" + fromIndex + ">");

            // Can be called too early when cloned in simulation cases
            if (lineRenderer == null)
                return;

            lineRenderer.positionCount = fromIndex;
        }

        public override void TryDisableClone()
        {
            base.TryDisableClone();
            if (lineRenderer != null)
                lineRenderer.enabled = visualizationEnabled;
        }

        public override void OnStoneInitTimeAgentsList(TimeStone eventStone)
        {
            base.OnStoneInitTimeAgentsList(eventStone);
            InitLineRenderer();
        }

        public override void OnSimulationStart()
        {
            base.OnSimulationStart();
            _newSimulationStart = true;
        }

        public override void OnSimulationComplete()
        {
            base.OnSimulationComplete();

            UpdateVisualization();
            SimplifyLine(simplifyTolerance);
        }

        public override void OnRecordStop()
        {
            base.OnRecordStop();

            UpdateVisualization();
            SimplifyLine(simplifyTolerance);
        }

        public override void OnTimeLineChange(TimeStone.TimeTickOrigin origin)
        {
            base.OnTimeLineChange(origin);

            if (!visualizationEnabled)
                return;

            switch (origin)
            {
                // Due to TimeStone.recordDuration option, updating only modified ticks can be very complex
                // So this generic visualizer is redrawing the whole timeline on every record
                case TimeStone.TimeTickOrigin.Record:
                    UpdateVisualization();
                    SimplifyLine(dynamicSimplifyTolerance);
                    break;
                case TimeStone.TimeTickOrigin.Simulation:
                    // If our last reference is not before the last point, it means it's a rewind and we need to clean
                    // by doing a fresh update (i.e : this is a new simulation here, need a fresh start)
                    if (_newSimulationStart)
                    {
                        UpdateVisualization();
                        _newSimulationStart = false;
                        SimplifyLine(simplifyTolerance);
                    }
                    else
                    {
                        AddPointsSinceLastChange();
                        SimplifyLine(dynamicSimplifyTolerance);
                    }


                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }
        }

        public override void OnTimeLineClear()
        {
            base.OnTimeLineClear();
            ClearVisualization();
        }

        public void AddPointsSinceLastChange()
        {
            if (!visualizationEnabled)
                return;

            var tickCount = stone.GetTickCount();

            var newPointsCount = stone.GetTickCount() - _lastChangeEventTickIndex;

            var lineRendererIndex = lineRenderer.positionCount;
            var timeLineIndex = _lastChangeEventTickIndex;
            lineRenderer.positionCount += newPointsCount;

            while (lineRendererIndex < lineRenderer.positionCount)
            {
                var position = stone.GetDataValueAt<Vector3>(timeLineIndex, timeAgent, TimeAgent.PositionDataId,
                    out var dataExists);
                if (!dataExists)
                {
                    if (logDebug)
                        Debug.LogWarning("[PathVisualizer] " + name + " : Data at <" + lineRendererIndex +
                                         "> does not exist. Skipped.");
                    lineRendererIndex++;
                    timeLineIndex++;
                    continue;
                }

                lineRenderer.SetPosition(lineRendererIndex, position + pointsOffset);
                lineRendererIndex++;
                timeLineIndex++;
            }

            _lastChangeEventTickIndex = tickCount;
        }

        public void UpdateVisualization()
        {
            if (!visualizationEnabled)
                return;

            var tickCount = stone.GetTickCount();
            if (tickCount == 0) return;
            if (logDebug)
                Debug.Log("[PathVisualizer] " + name + " : Update visualization from tick <" +
                          _lastChangeEventTickIndex +
                          "> to tick <" + tickCount + ">");

            lineRenderer.positionCount = tickCount;

            var updateIndex = 0;
            while (updateIndex < tickCount)
            {
                var position = stone.GetDataValueAt<Vector3>(updateIndex, timeAgent, TimeAgent.PositionDataId,
                    out var dataExists);
                if (!dataExists)
                {
                    if (logDebug)
                        Debug.LogWarning("[PathVisualizer] " + name + " : Data at <" + updateIndex +
                                         "> does not exist. Skipped.");
                    updateIndex++;
                    continue;
                }

                lineRenderer.SetPosition(updateIndex, position + pointsOffset);
                updateIndex++;
            }

            _lastChangeEventTickIndex = tickCount;
        }

        /// <summary>
        /// Simplify the lineRenderer to optimize its display.
        /// Delay counter avoid over-simplifying while the path is created, this would lead in big approximations
        /// </summary>
        private void SimplifyLine(float tolerance)
        {
            if (tolerance > 0 && lineRenderer.positionCount > 1)
                lineRenderer.Simplify(tolerance);
        }
    }
}