using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using UnityEditor;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Scripts.Editor
{
    [CustomEditor(typeof(TimeAgent))]
    [CanEditMultipleObjects]
    public class TimeAgentEditor : UnityEditor.Editor
    {
        /*
         * GUI fields
         */
        private bool eventsSection;
        private bool debugSection;

        /*
         * Serialized elements
         */

        // Config
        private SerializedProperty parseChildren;
        private SerializedProperty persistActive;
        private SerializedProperty persistPosition;
        private SerializedProperty useLocalPosition;
        private SerializedProperty persistRotation;
        private SerializedProperty useLocalRotation;
        private SerializedProperty persistScale;
        private SerializedProperty persistVelocity;
        private SerializedProperty persistAnimator;
        private SerializedProperty agentAnimator;
        private SerializedProperty persistParticles;
        private SerializedProperty persistParticlesList;
        private SerializedProperty searchParticleChildren;
        private SerializedProperty simulationCallbackPolicy;
        private SerializedProperty hidePolicy;
        private SerializedProperty cloneMaterial;
        private SerializedProperty applyColorToClone;
        private SerializedProperty cloneColor;
        private SerializedProperty applySimulationConfigOnChildren;

        // Events
        private SerializedProperty onInitTimeAgentsList;
        private SerializedProperty onTimeLineChange;
        private SerializedProperty onSimulationStart;
        private SerializedProperty onSimulationUpdate;
        private SerializedProperty onSimulationFixedUpdate;
        private SerializedProperty onSimulationLateUpdate;
        private SerializedProperty onSimulationComplete;
        private SerializedProperty onPersistTick;
        private SerializedProperty onSetDataTick;

        // Private fields
        private TimeAgent _timeAgent;

        // Debug
        private SerializedProperty logDebug;

        protected virtual void OnEnable()
        {
            // Config
            _timeAgent = ((TimeAgent) serializedObject.targetObject);
            parseChildren = serializedObject.FindProperty("parseChildren");
            persistActive = serializedObject.FindProperty("persistActive");
            persistPosition = serializedObject.FindProperty("persistPosition");
            useLocalPosition = serializedObject.FindProperty("useLocalPosition");
            persistRotation = serializedObject.FindProperty("persistRotation");
            useLocalRotation = serializedObject.FindProperty("useLocalRotation");
            persistScale = serializedObject.FindProperty("persistScale");
            persistVelocity = serializedObject.FindProperty("persistVelocity");
            persistAnimator = serializedObject.FindProperty("persistAnimator");
            agentAnimator = serializedObject.FindProperty("agentAnimator");
            persistParticles = serializedObject.FindProperty("persistParticles");
            persistParticlesList = serializedObject.FindProperty("persistParticlesList");
            searchParticleChildren = serializedObject.FindProperty("searchParticleChildren");
            simulationCallbackPolicy = serializedObject.FindProperty("simulationCallbackPolicy");
            hidePolicy = serializedObject.FindProperty("hidePolicy");
            cloneMaterial = serializedObject.FindProperty("cloneMaterial");
            applyColorToClone = serializedObject.FindProperty("applyColorToClone");
            cloneColor = serializedObject.FindProperty("cloneColor");
            applySimulationConfigOnChildren = serializedObject.FindProperty("applySimulationConfigOnChildren");

            // Events
            onInitTimeAgentsList = serializedObject.FindProperty("_onInitTimeAgentsList");
            onTimeLineChange = serializedObject.FindProperty("_onTimeLineChange");
            onSimulationStart = serializedObject.FindProperty("_onSimulationStart");
            onSimulationUpdate = serializedObject.FindProperty("_onSimulationUpdate");
            onSimulationFixedUpdate = serializedObject.FindProperty("_onSimulationFixedUpdate");
            onSimulationLateUpdate = serializedObject.FindProperty("_onSimulationLateUpdate");
            onSimulationComplete = serializedObject.FindProperty("_onSimulationComplete");
            onPersistTick = serializedObject.FindProperty("_onPersistTick");
            onSetDataTick = serializedObject.FindProperty("_onSetDataTick");

            // Debug
            logDebug = serializedObject.FindProperty("logDebug");
        }

        public override void OnInspectorGUI()
        {
            SharedGui.InitStyles();

            serializedObject.Update();

            LoadEditorPrefs();

            EditorGUI.BeginChangeCheck();

            CreateConfigSection();
            EditorGUILayout.Separator();
            CreateEventsSection();
            EditorGUILayout.Separator();
            CreateDebugSection();

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                SaveEditorPrefs();
            }

            // base.OnInspectorGUI();
        }

        private void LoadEditorPrefs()
        {
        }

        private void SaveEditorPrefs()
        {
        }

        private void CreateConfigSection()
        {
            if (_timeAgent.IsClone)
                EditorGUILayout.LabelField("SIMULATION CLONE", SharedGui.cloneLabelGuiStyle);

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(parseChildren,
                new GUIContent(
                    "Parse children",
                    "Will automatically search for TimeAgents in children during TimeStone initialization. Be careful with duplicates when using this option (parent TimeAgent with already added children, etc.)"));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(persistActive,
                new GUIContent("Active state",
                    "Persist GameObject.activeSelf"));

            EditorGUILayout.PropertyField(persistPosition,
                new GUIContent("Position",
                    "Persist Transform.position"));

            EditorGUI.indentLevel++;
            GUI.enabled = persistPosition.boolValue;
            EditorGUILayout.PropertyField(useLocalPosition,
                new GUIContent("Use localPosition",
                    "Persist local position"));
            GUI.enabled = true;
            EditorGUI.indentLevel--;

            
            EditorGUILayout.PropertyField(persistRotation,
                new GUIContent("Rotation",
                    "Persist Transform.rotation"));
            
            EditorGUI.indentLevel++;
            GUI.enabled = persistRotation.boolValue;
            EditorGUILayout.PropertyField(useLocalRotation,
                new GUIContent("Use localRotation",
                    "Persist local rotation"));
            GUI.enabled = true;
            EditorGUI.indentLevel--;
            
            EditorGUILayout.PropertyField(persistScale,
                new GUIContent("Scale",
                    "Persist Transform.localScale"));
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(persistVelocity,
                new GUIContent("Velocity",
                    "Persist Rigidbody.velocity and Rigidbody.angularVelocity"));
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(persistAnimator,
                new GUIContent("Enabled",
                    "Persist Animator's state"));
            GUI.enabled = persistAnimator.boolValue;
            EditorGUILayout.PropertyField(agentAnimator,
                new GUIContent("Animator", "Let undefined to automatically assign GameObject's Animator component"));
            GUI.enabled = true;
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(persistParticles,
                new GUIContent("Enabled",
                    "Persist Particles state"));
            GUI.enabled = persistParticles.boolValue;
            EditorGUILayout.PropertyField(persistParticlesList,
                new GUIContent("Particle Systems",
                    "Let undefined to automatically assign GameObject's Particle Systems"));
            EditorGUILayout.PropertyField(searchParticleChildren,
                new GUIContent("Search children", "Search for other Particle Systems in object's children"));
            GUI.enabled = true;

            EditorGUILayout.PropertyField(hidePolicy,
                new GUIContent("Hide policy", "How the simulation clone is hidden"));
            EditorGUILayout.PropertyField(cloneMaterial,
                new GUIContent("Apply Material", "Apply a specific material when cloned for simulation"));
            EditorGUILayout.PropertyField(applyColorToClone,
                new GUIContent("Apply Color", "Apply a specific color when cloned for simulation"));
            if (applyColorToClone.boolValue)
            {
                EditorGUILayout.PropertyField(cloneColor, new GUIContent(""));
            }

            EditorGUILayout.PropertyField(applySimulationConfigOnChildren,
                new GUIContent("Apply on children", "Apply the hide policy, material and color to clone's children"));

            EditorGUILayout.PropertyField(simulationCallbackPolicy,
                new GUIContent("Callback policy",
                    "Life cycle callbacks (SimulationUpdate, etc.) receive policy. Depending on your choice, you may need to use TimeAgent.IsClone to differentiate who the callback is called on."));
            EditorGUI.indentLevel--;
        }

        private void CreateEventsSection()
        {
            eventsSection = EditorGUILayout.BeginFoldoutHeaderGroup(eventsSection, new GUIContent("Events"));
            if (eventsSection)
            {
                EditorGUILayout.PropertyField(onInitTimeAgentsList);
                EditorGUILayout.PropertyField(onTimeLineChange);
                EditorGUILayout.PropertyField(onSimulationStart);
                EditorGUILayout.PropertyField(onSimulationUpdate);
                EditorGUILayout.PropertyField(onSimulationFixedUpdate);
                EditorGUILayout.PropertyField(onSimulationLateUpdate);
                EditorGUILayout.PropertyField(onSimulationComplete);
                EditorGUILayout.PropertyField(onPersistTick);
                EditorGUILayout.PropertyField(onSetDataTick);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void CreateDebugSection()
        {
            debugSection = EditorGUILayout.BeginFoldoutHeaderGroup(debugSection, new GUIContent("Debug options"));
            if (debugSection)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(logDebug,
                    new GUIContent("Verbose", "Activate all logging messages"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}