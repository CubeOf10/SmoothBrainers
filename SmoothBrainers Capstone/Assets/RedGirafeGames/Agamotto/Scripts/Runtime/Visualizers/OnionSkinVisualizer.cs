using System;
using System.Collections.Generic;
using System.Linq;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using RedGirafeGames.Agamotto.Scripts.Runtime.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RedGirafeGames.Agamotto.Scripts.Runtime.Visualizers
{
    /// <summary>
    /// Visualizer displaying the object's state before and/or after the current tick.
    /// Similar to the Onion Skin concept in animation.
    ///
    /// You can choose how many ticks to displat and the timeing between ticks.
    /// 
    /// Beware : This visualizer instantiates a duplication of the GameObject for each tick visualized (for example if you display 2 ticks before
    /// and 3 ticks after, your object will be duplicated 5 times in the scene), this could harm performances if it's used on complex objects or with a lot ticks visualized.
    /// In that case, you should consider extending this visualizer and use an other system creating sprite or anything less expensive in performance instead of Game Object duplication.
    /// </summary>
    [AddComponentMenu("Agamotto/Onion Skin Visualizer")]
    public class OnionSkinVisualizer : AbstractTimeVisualizer
    {
        /// <summary>
        /// The number of ticks visualized before
        /// </summary>
        [Header("Onion Skin Tick Settings")] [Tooltip("The number of ticks visualized before")] [Range(0, 50)]
        public int beforeCount = 4;

        /// <summary>
        /// The number of ticks visualized after
        /// </summary>
        [Tooltip("The number of ticks visualized after")] [Range(0, 50)]
        public int afterCount = 4;

        /// <summary>
        /// The number of tick between each visualized before
        /// </summary>
        [Tooltip("The timing between each tick visualized before")] [Range(0, 500)]
        public int beforeTiming = 20;

        /// <summary>
        /// The number of tick between each visualized after
        /// </summary>
        [Tooltip("The timing between each tick visualized after")] [Range(0, 500)]
        public int afterTiming = 20;

        /// <summary>
        /// If defined, the material applied to the duplicates
        /// </summary>
        [Header("Onion Skin Display")] [Tooltip("If defined, the material applied to the duplicates")]
        public Material duplicateMaterial;

        /// <summary>
        /// If defined, the duplicates will be created from this prefab instead of a duplication of the GameObject.
        /// </summary>
        [Tooltip(
            "If defined, the duplicates will be created from this prefab instead of a duplication of the GameObject.")]
        public GameObject duplicatePrefab;

        /// <summary>
        /// Applies <see cref="duplicateColor"/> to the duplicate material
        /// </summary>
        [Tooltip("If true, apply the selected color to the duplicates material")]
        public bool applyDuplicateColor;

        /// <summary>
        /// The color applied to the duplicate if <see cref="applyDuplicateColor"/> is true
        /// </summary>
        [Tooltip("Color applied to the duplicates material if applyDuplicateColor is selected")]
        public Color duplicateColor = new Color(1, 1, 1, 0.2f);

        /// <summary>
        /// If true, duplicates with same transform data as the original object won't be displayed.
        /// This prevents flickering from overlapping objects.
        /// Should be set to false if you're object is a skinned mesh and keeps the same position
        /// </summary>
        [Tooltip(
            "This prevents flickering from overlapping objects. Should be set to false if you're object is a skinned mesh and keeps the same position")]
        public bool optimizeSameTransform = true;

        /// <summary>
        /// The duplicated versions of the GameObject that will be displayed as onion skin
        /// </summary>
        private List<GameObject> duplicates;

        /// <summary>
        /// Cache list of the duplicates TimeAgent components to avoid recurrent GetComponent calls
        /// </summary>
        private List<TimeAgent> duplicatesTimeAgent;

        /// <summary>
        /// Flag to know if the visualizer is initialized
        /// </summary>
        private bool duplicatesInitialized;

        private void Start()
        {
            InitDuplicates();
        }

        private void InitDuplicates()
        {
            if (timeAgent == null || timeAgent.IsClone)
                return;
            if (logDebug)
                Debug.Log("[OnionSkinVisualizer] " + name + " : Init Duplicates");

            if (duplicates != null && duplicates.Count > 0)
            {
                foreach (var duplicate in duplicates)
                {
                    Destroy(duplicate);
                }
            }

            duplicates = new List<GameObject>();
            duplicatesTimeAgent = new List<TimeAgent>();

            var totalDuplicates = beforeCount + afterCount;

            if (logDebug)
                Debug.Log("[OnionSkinVisualizer] " + name + " : Creating " + totalDuplicates + " duplicates");

            for (var i = 0; i < totalDuplicates; i++)
            {
                var d = Instantiate(duplicatePrefab != null ? duplicatePrefab : gameObject);
                d.name = name + "_OnionSkin";
                // Setting default layer in case stone is on later detection mode, as we don't want duplicates to be detected
                d.layer = 0;
                // Same for the tag
                d.tag = "Untagged";
                // Just avoiding an infinite loop... it has some bad side effects... I heard...
                Destroy(d.GetComponent<OnionSkinVisualizer>());
                // Remove the duplicate from the physics simulation
                if (d.TryGetComponent<Collider>(out var selfCollider))
                {
                    selfCollider.enabled = false;
                }

                var colliders = d.GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                {
                    col.enabled = false;
                }

                GameObjectUtils.Hide(d, GameObjectUtils.HidePolicy.DisableRenderer, true);
                GameObjectUtils.ApplyColorAndMaterial(d, true, duplicateMaterial, applyDuplicateColor, duplicateColor);

                duplicates.Add(d);

                var ta = d.GetComponent<TimeAgent>();
                // Use the same guid so we can get its data in the timeStone. It has no side effect because it's not referenced by the stone
                ta.guid = timeAgent.guid;
                ta.Original = timeAgent;
                duplicatesTimeAgent.Add(ta);

                if (d.TryGetComponent<Animator>(out var animator))
                {
                    animator.speed = 0.0f;
                }
            }

            duplicatesInitialized = true;
        }

        public override void OnTimeAgentSetDataTick(TimeStone eventStone, int tick)
        {
            base.OnTimeAgentSetDataTick(eventStone, tick);

            UpdateVisualization(tick);
        }

        public override void ClearVisualization(int fromIndex = 0)
        {
            base.ClearVisualization(fromIndex);

            if (!duplicatesInitialized)
                return;
            
            if (logDebug)
                Debug.Log("[OnionSkinVisualizer] " + name + " : Clear visualization from index <" + fromIndex + ">");
            
            foreach (var duplicate in duplicates)
            {
                GameObjectUtils.Hide(duplicate, GameObjectUtils.HidePolicy.DisableRenderer, true);
            }
        }

        public override void OnStoneInitTimeAgentsList(TimeStone eventStone)
        {
            base.OnStoneInitTimeAgentsList(eventStone);
            InitDuplicates();
        }

        public override void OnSimulationStart()
        {
            base.OnSimulationStart();
            ClearVisualization();
        }
        
        public override void OnSimulationComplete()
        {
            base.OnSimulationComplete();

            UpdateVisualization(stone.playbackTickCursor);
        }

        public override void OnRecordStop()
        {
            base.OnRecordStop();

            UpdateVisualization(stone.playbackTickCursor);
        }

        public override void OnTimeLineChange(TimeStone.TimeTickOrigin origin)
        {
            base.OnTimeLineChange(origin);

            
            
            UpdateVisualization(stone.playbackTickCursor);
        }

        public override void OnTimeLineClear()
        {
            base.OnTimeLineClear();
            ClearVisualization();
        }

        private void UpdateVisualization(int tick)
        {
            if (!visualizationEnabled || !duplicatesInitialized)
                return;

            var tickCount = stone.GetTickCount();
            if (tickCount == 0) return;
            if (logDebug)
                Debug.Log("[OnionSkinVisualizer] " + name + " : Update visualization for tick <" + tick + ">");


            var duplicateTick = tick - beforeTiming;
            var beforeTicksIndex = 0;
            while (beforeTicksIndex < beforeCount)
            {
                ConfigureDuplicate(beforeTicksIndex, duplicateTick);

                duplicateTick -= beforeTiming;
                beforeTicksIndex++;
            }
            
            duplicateTick = tick + afterTiming;
            var afterTicksIndex = beforeCount;
            while (afterTicksIndex < afterCount + beforeCount)
            {
                ConfigureDuplicate(afterTicksIndex, duplicateTick);

                duplicateTick += afterTiming;
                afterTicksIndex++;
            }
        }

        private void ConfigureDuplicate(int duplicateIndex, int tickIndex)
        {
            var duplicate = duplicates[duplicateIndex];
            var duplicateTimeAgent = duplicatesTimeAgent[duplicateIndex];

            if (tickIndex < 0 || tickIndex >= stone.GetTickCount())
            {
                GameObjectUtils.Hide(duplicate, GameObjectUtils.HidePolicy.DisableRenderer, true);
            }
            else
            {
                duplicateTimeAgent.SetDataTick(stone, tickIndex);

                if (optimizeSameTransform && duplicate.transform.position == transform.position &&
                    duplicate.transform.rotation == transform.rotation &&
                    duplicate.transform.localScale == transform.localScale)
                {
                    GameObjectUtils.Hide(duplicate, GameObjectUtils.HidePolicy.DisableRenderer, true);
                }
                else
                {
                    GameObjectUtils.Show(duplicate, GameObjectUtils.HidePolicy.DisableRenderer, true);
                }
            }
        }

        private bool editorUpdate;

        private void OnValidate()
        {
            if (Application.isPlaying)
                editorUpdate = true;
        }

        private void LateUpdate()
        {
            if (!editorUpdate) return;
            InitDuplicates();
            UpdateVisualization(stone.playbackTickCursor);
            editorUpdate = false;
        }
    }
}