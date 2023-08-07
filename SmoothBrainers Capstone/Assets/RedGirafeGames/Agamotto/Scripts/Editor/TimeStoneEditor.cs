using RedGirafeGames.Agamotto.Scripts.Runtime;
using UnityEditor;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Scripts.Editor
{
    [CustomEditor(typeof(TimeStone))]
    [CanEditMultipleObjects]
    public class TimeStoneEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Id to save the property in EditorPrefs (so the user does not have to continuously reopen this section)
        /// </summary>
        private const string ControllerSectionId = "Agamotto.TimeStoneEditor.controllerSection";

        /*
         * GUI fields
         */
        private bool controllerSection = false;
        private bool timelineConfigSection = false;
        private bool eventsSection = false;
        private bool debugSection = false;

        /*
         * Serialized elements
         */
        //Controller
        private SerializedProperty playback;
        private SerializedProperty playbackTickCursor;
        private SerializedProperty playbackSpeed;
        private SerializedProperty playbackTimeScale;
        private SerializedProperty playUpdateAddTickCursor;

        // Record
        private SerializedProperty recordOnStart;
        private SerializedProperty recordDuration;
        private SerializedProperty recordingTimeScale;
        private SerializedProperty recordingStep;
        private SerializedProperty updatePlayTickCursorOnRecord;

        // Simulation
        private SerializedProperty simulateUpdate;
        private SerializedProperty simulatePhysics;
        private SerializedProperty simulationUpdateStep;
        private SerializedProperty simulationUseUpdateDeltaTime;
        private SerializedProperty simulationPhysicsStep;
        private SerializedProperty simulationUseUnityFixedDeltaTime;
        private SerializedProperty simulationHasDuration;
        private SerializedProperty simulationDuration;
        private SerializedProperty simulationTimeScale;
        private SerializedProperty simulationSpeed;
        private SerializedProperty simulationGroupFrameTicks;
        private SerializedProperty simulationMaxMsByFrame;
        private SerializedProperty resetAddTickCursorOnSimulationComplete;
        private SerializedProperty simulateOnStart;
        private SerializedProperty keepSimulationScene;

        // Agents
        private SerializedProperty timeAgentsListInitMode;
        private SerializedProperty timeAgentsManualInitList;
        private SerializedProperty timeAgentsListInitTag;
        private SerializedProperty timeAgentsListInitLayer;
        private SerializedProperty timeAgents;

        // Config
        private SerializedProperty timeLineAddTickBehaviour;

        // Events
        private SerializedProperty onInitTimeAgentsList;
        private SerializedProperty onTimeLineChange;
        private SerializedProperty onTimeLineClear;
        private SerializedProperty onSimulationStart;
        private SerializedProperty onSimulationComplete;
        private SerializedProperty onRecordStop;

        // Debug
        private SerializedProperty addTickCursor;
        private SerializedProperty playTickCursor;
        private SerializedProperty simulationTime;
        private SerializedProperty simulationTimeAgents;
        private SerializedProperty frameTimeStack;
        private SerializedProperty logDebug;
        private SerializedProperty logPerfs;

        // Private fields
        private TimeStone stone;

        void OnEnable()
        {
            SharedGui.InitTextures();

            // Controller
            playback = serializedObject.FindProperty("playback");
            playbackTickCursor = serializedObject.FindProperty("playbackTickCursor");
            playbackSpeed = serializedObject.FindProperty("playbackSpeed");
            playbackTimeScale = serializedObject.FindProperty("playbackTimeScale");
            playUpdateAddTickCursor = serializedObject.FindProperty("playbackUpdateAddTickCursor");

            // Record
            recordOnStart = serializedObject.FindProperty("recordOnStart");
            recordDuration = serializedObject.FindProperty("recordDuration");
            recordingTimeScale = serializedObject.FindProperty("recordingTimeScale");
            recordingStep = serializedObject.FindProperty("recordingStep");
            updatePlayTickCursorOnRecord = serializedObject.FindProperty("updatePlayTickCursorOnRecord");

            // Simulation
            simulateUpdate = serializedObject.FindProperty("simulateUpdate");
            simulatePhysics = serializedObject.FindProperty("simulatePhysics");
            simulationUpdateStep = serializedObject.FindProperty("simulationUpdateStep");
            simulationUseUnityFixedDeltaTime = serializedObject.FindProperty("simulationUseUnityFixedDeltaTime");
            simulationPhysicsStep = serializedObject.FindProperty("simulationPhysicsStep");
            simulationDuration = serializedObject.FindProperty("simulationDuration");
            simulationUseUpdateDeltaTime = serializedObject.FindProperty("simulationUseUpdateDeltaTime");
            simulationHasDuration = serializedObject.FindProperty("simulationHasDuration");
            simulationTimeScale = serializedObject.FindProperty("simulationTimeScale");
            simulationSpeed = serializedObject.FindProperty("simulationSpeed");
            simulationGroupFrameTicks = serializedObject.FindProperty("simulationGroupFrameTicks");
            simulationMaxMsByFrame = serializedObject.FindProperty("simulationMaxMsByFrame");
            resetAddTickCursorOnSimulationComplete =
                serializedObject.FindProperty("resetAddTickCursorOnSimulationComplete");
            simulateOnStart = serializedObject.FindProperty("simulateOnStart");
            keepSimulationScene = serializedObject.FindProperty("keepSimulationScene");

            // Agents
            timeAgentsListInitMode = serializedObject.FindProperty("timeAgentsListInitMode");
            timeAgentsManualInitList = serializedObject.FindProperty("timeAgentsManualInitList");
            timeAgentsListInitTag = serializedObject.FindProperty("timeAgentsListInitTag");
            timeAgentsListInitLayer = serializedObject.FindProperty("timeAgentsListInitLayer");
            timeAgents = serializedObject.FindProperty("timeAgents");

            // Config
            timeLineAddTickBehaviour = serializedObject.FindProperty("timeLineAddTickBehaviour");

            // Events
            onInitTimeAgentsList = serializedObject.FindProperty("_onInitTimeAgentsList");
            onTimeLineClear = serializedObject.FindProperty("_onTimeLineClear");
            onTimeLineChange = serializedObject.FindProperty("_onTimeLineChange");
            onSimulationStart = serializedObject.FindProperty("_onSimulationStart");
            onSimulationComplete = serializedObject.FindProperty("_onSimulationComplete");
            onRecordStop = serializedObject.FindProperty("_onRecordStop");

            // Debug
            simulationTimeAgents = serializedObject.FindProperty("simulationTimeAgents");
            playTickCursor = serializedObject.FindProperty("playbackTickCursor");
            addTickCursor = serializedObject.FindProperty("addTickCursor");
            simulationTime = serializedObject.FindProperty("simulationTime");
            frameTimeStack = serializedObject.FindProperty("frameTimeStack");
            logDebug = serializedObject.FindProperty("logDebug");
            logPerfs = serializedObject.FindProperty("logPerfs");

            stone = ((TimeStone) serializedObject.targetObject);
        }

        public override void OnInspectorGUI()
        {
            SharedGui.InitStyles();

            serializedObject.Update();

            LoadEditorPrefs();

            EditorGUI.BeginChangeCheck();

            CreateControllerSection();
            EditorGUILayout.Separator();
            CreateAgentsSection();
            EditorGUILayout.Separator();
            CreateRecordSection();
            EditorGUILayout.Separator();
            CreateSimulationSection();
            EditorGUILayout.Separator();
            CreateTimelineConfigSection();
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
            if (EditorPrefs.HasKey(ControllerSectionId))
                controllerSection = EditorPrefs.GetBool(ControllerSectionId);
        }

        private void SaveEditorPrefs()
        {
            EditorPrefs.SetBool(ControllerSectionId, controllerSection);
        }

        private void CreateRecordSection()
        {
            EditorGUILayout.LabelField("RECORD", SharedGui.headerGuiStyle);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(recordOnStart,
                new GUIContent("Record on start", "Stone activates recording on game start"));
            EditorGUILayout.PropertyField(recordDuration,
                new GUIContent("Duration",
                    "The maximum recording duration. When reached, oldest timeline values are deleted when new values are added"));
            EditorGUILayout.PropertyField(recordingTimeScale,
                new GUIContent("Time mode", "Scaled for time dependent and unscaled for time free mode"));
            EditorGUILayout.PropertyField(recordingStep,
                new GUIContent("Record step (seconds)",
                    "Minimum delta time between record data, 0 or less will record on each Update call"));
            if (recordingStep.floatValue == 0)
            {
                EditorGUILayout.HelpBox(
                    "Be careful, 0 recording step can harm performances in critical way. Be sure that you need that much precision before using it.",
                    MessageType.Warning);
            }

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(updatePlayTickCursorOnRecord,
                new GUIContent("Synchronize playback tick",
                    "The playback tick will be updated when recording. For example : If you use a Time Slider to visualize the time line, the slider value will be synchronous with the time line recording"));
            EditorGUI.indentLevel--;
        }

        private void CreateSimulationSection()
        {
            EditorGUILayout.LabelField("SIMULATION", SharedGui.headerGuiStyle);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(simulateOnStart,
                new GUIContent("Do simulation on start", "Run a simulation on game start"));

            EditorGUILayout.Separator();
            
            EditorGUILayout.PropertyField(simulationHasDuration,
                new GUIContent("Limit duration",
                    "Defines if the simulation should use maximum duration. If unchecked, simulation will run till StopSimulation is called (be sure to call it somewhere...)"));
            GUI.enabled = simulationHasDuration.boolValue;
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(simulationDuration,
                new GUIContent("Duration (seconds)",
                    "The maximum duration of the simulation. You can stop it before by calling TimeStone.StopSimulation()"));
            EditorGUI.indentLevel--;
            GUI.enabled = true;

            EditorGUILayout.Separator();
            
            EditorGUILayout.PropertyField(simulationTimeScale,
                new GUIContent("Time mode", "The Time scale"));

            EditorGUILayout.PropertyField(simulationSpeed,
                new GUIContent("Simulation speed",
                    "With a value of 1, simulation is ran at real time. higher values speed up the process but has more impact on performance."));

            EditorGUILayout.Separator();
            
            EditorGUILayout.PropertyField(simulationUseUpdateDeltaTime,
                new GUIContent("Use Time.deltaTime",
                    "The simulation will use the Time.deltaTime or Time.unscaledDeltaTime value. Using this option, the simulation will be " +
                    "more similar to the real execution, but you will also get the real frame spikes (for example on game start)"));
            GUI.enabled = !simulationUseUpdateDeltaTime.boolValue;
            EditorGUILayout.PropertyField(simulationUpdateStep,
                new GUIContent("Frame Delta time (ms)",
                    "The delta time between each simulation frame, equivalent to 'Time.deltaTime' when called from a MonoBehaviour's Update method"));
            GUI.enabled = true;
            
            EditorGUILayout.Separator();
            
            EditorGUILayout.PropertyField(simulateUpdate,
                new GUIContent("Simulate Code",
                    "Time agents life cycle callbacks (SimulationStart, SimulationUpdate, etc.) will be executed every tick to simulate code execution"));

            EditorGUILayout.PropertyField(simulatePhysics,
                new GUIContent("Simulate Physics", "Physics simulation will be ran on time agents"));

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(simulationUseUnityFixedDeltaTime,
                new GUIContent("Use Time.fixedDeltaTime",
                    "Physics simulation will use Time.fixedDeltaTime as delta time between each simulation tick"));
            GUI.enabled = !simulationUseUnityFixedDeltaTime.boolValue;
            EditorGUILayout.PropertyField(simulationPhysicsStep,
                new GUIContent("Physics Delta time (ms)",
                    "Delta time between each physics simulation will be forced to this value"));
            GUI.enabled = true;

            EditorGUI.indentLevel--;
            
            EditorGUILayout.Separator();
            
            EditorGUILayout.PropertyField(simulationMaxMsByFrame,
                new GUIContent("Max ms by frame",
                    "The maximum milliseconds the simulation can use each frame"));
            EditorGUILayout.PropertyField(simulationGroupFrameTicks, new GUIContent("Group frame ticks",
                "If true, all ticks that should happen on the same frame will be grouped into one tick. " +
                "Execution is therefore much faster but at the cost of precision as there will be less ticks simulated. Has no effect if Simulation speed = 1"));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(keepSimulationScene,
                new GUIContent("Keep simulation scene",
                    "Scene created to run the simulation won't be destroyed when simulation end."));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(resetAddTickCursorOnSimulationComplete,
                new GUIContent("Reset add cursor on completion",
                    "This will replace the addTickCursor at the simulation start tick index on completion."));

            EditorGUI.indentLevel--;
        }


        private void CreateAgentsSection()
        {
            EditorGUILayout.LabelField("TIME AGENTS", SharedGui.headerGuiStyle);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(timeAgentsListInitMode,
                new GUIContent("Initialization mode",
                    "Manual : Agents list is provided. Tag : All game objects with a TimeAgent component and this tag are included. Layer : All game objects on this layer with a TimeAgent component are included."));
            EditorGUI.indentLevel++;
            if (timeAgentsListInitMode.enumValueIndex == (int) TimeStone.TimeAgentsListInitMode.Manual)
            {
                EditorGUILayout.PropertyField(timeAgentsManualInitList, new GUIContent("Init list"));
            }
            else if (timeAgentsListInitMode.enumValueIndex == (int) TimeStone.TimeAgentsListInitMode.Layer)
            {
                EditorGUILayout.PropertyField(timeAgentsListInitLayer, new GUIContent("Layer name"));
            }
            else if (timeAgentsListInitMode.enumValueIndex == (int) TimeStone.TimeAgentsListInitMode.Tag)
            {
                EditorGUILayout.PropertyField(timeAgentsListInitTag, new GUIContent("Tag"));
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Separator();
            
            EditorGUI.indentLevel--;
        }

        private void CreateControllerSection()
        {
            controllerSection =
                EditorGUILayout.BeginFoldoutHeaderGroup(controllerSection,
                    new GUIContent("Real Time Controllers"));
            if (controllerSection)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("PLAYBACK");

                GUI.enabled = false;
                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField("Agents : " + stone.timeAgents.Count, SharedGui.infosLabelGuiStyle);
                EditorGUILayout.TextField("Play cursor : " + playTickCursor.intValue.ToString(),
                    SharedGui.infosLabelGuiStyle);
                EditorGUILayout.TextField("Add cursor : " + addTickCursor.intValue.ToString(),
                    SharedGui.infosLabelGuiStyle);
                EditorGUILayout.TextField("Ticks : " + stone.GetTickCount(),
                    SharedGui.infosLabelGuiStyle);
                GUILayout.EndHorizontal();
                GUI.enabled = true;

                GUILayout.BeginHorizontal();

                GUI.enabled = playbackTickCursor.intValue != 0 && EditorApplication.isPlaying && !stone.Simulating && !stone.Recording;
                if (GUILayout.Button(SharedGui.toStartIcon))
                {
                    stone.StopPlayback();
                    stone.SetPlaybackTick(0);
                }

                GUI.enabled = playback.boolValue && EditorApplication.isPlaying && !stone.Simulating && !stone.Recording;
                if (GUILayout.Button(SharedGui.stopIcon))
                {
                    stone.StopPlayback();
                }

                GUI.enabled = !playback.boolValue && EditorApplication.isPlaying && !stone.Simulating && !stone.Recording;
                if (GUILayout.Button(SharedGui.playIcon))
                {
                    stone.StartPlayback();
                }

                GUI.enabled = EditorApplication.isPlaying && !stone.Simulating && !stone.Recording;
                if (GUILayout.Button(SharedGui.toEndIcon))
                {
                    stone.StopPlayback();
                    stone.SetPlaybackTick(GetMaxTick());
                }

                GUILayout.EndHorizontal();

                GUI.enabled = EditorApplication.isPlaying && !stone.Simulating && !stone.Recording;

                float playSliderValue = playbackTickCursor.intValue;
                EditorGUI.BeginChangeCheck();
                playSliderValue = EditorGUILayout.Slider(playSliderValue, 0, GetMaxTick());
                if (EditorGUI.EndChangeCheck())
                {
                    stone.SetPlaybackTick((int) playSliderValue, playUpdateAddTickCursor.boolValue);
                }

                GUI.enabled = true;
                EditorGUILayout.PropertyField(playbackSpeed);
                EditorGUILayout.PropertyField(playbackTimeScale);
                EditorGUILayout.PropertyField(playUpdateAddTickCursor, new GUIContent("Update addTickCursor",
                    "This will update the addTickCursor." +
                    " So that starting a simulation or recording will be synchronized with time agents state."));

                EditorGUILayout.EndVertical();

                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label("FREEZE");
                if (GUILayout.Button(new GUIContent("Freeze", "Freezes time agents using default options")))
                {
                    stone.FreezeTimeAgents(true);
                }
                if (GUILayout.Button(new GUIContent("UnFreeze", "UnFreezes time agents using default options")))
                {
                    stone.FreezeTimeAgents(false);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Separator();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("SIMULATE");

                GUI.enabled = false;
                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField("Sim Agents  : " + stone.simulationTimeAgents.Count,
                    SharedGui.infosLabelGuiStyle);
                EditorGUILayout.TextField("Simulating  : " + stone.Simulating, SharedGui.infosLabelGuiStyle);
                GUILayout.EndHorizontal();
                GUI.enabled = true;

                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                GUI.enabled = EditorApplication.isPlaying && stone.Simulating;
                if (GUILayout.Button(new GUIContent("Stop Simulation", "Stops running simulation")))
                {
                    stone.StopSimulation();
                }

                GUI.enabled = EditorApplication.isPlaying && !stone.Simulating && !stone.Recording;
                if (GUILayout.Button(new GUIContent("StartSimulation", "Run a simulation")))
                {
                    stone.StartSimulation(true);
                }

                EditorGUILayout.EndHorizontal();

                GUI.enabled = true;

                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Separator();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("RECORD");

                GUI.enabled = false;
                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField("Recording  : " + stone.Recording, SharedGui.infosLabelGuiStyle);
                EditorGUILayout.TextField("Record time  : " + stone.recordTime, SharedGui.infosLabelGuiStyle);
                GUILayout.EndHorizontal();
                GUI.enabled = true;

                GUI.enabled = EditorApplication.isPlaying && stone.Recording;

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Stop", "Stop recording")))
                {
                    stone.StopRecording();
                }

                GUI.enabled = EditorApplication.isPlaying && !stone.Recording && !stone.Simulating;
                if (GUILayout.Button(new GUIContent("Start",
                    "Prepares pancakes and hot chocolate... hum... what?... ok... ok... it just Starts recording")))
                {
                    stone.StartRecording();
                }

                EditorGUILayout.EndHorizontal();

                GUI.enabled = true;

                EditorGUILayout.EndVertical();

                GUI.enabled = EditorApplication.isPlaying && !stone.Recording && !stone.Simulating;

                if (GUILayout.Button(new GUIContent("Clear", "Clear current TimeLine data (Record and Simulation)")))
                {
                    stone.Clear();
                }

                GUI.enabled = true;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void CreateTimelineConfigSection()
        {
            timelineConfigSection =
                EditorGUILayout.BeginFoldoutHeaderGroup(timelineConfigSection,
                    new GUIContent("TimeLine configuration"));
            if (timelineConfigSection)
            {
                EditorGUILayout.PropertyField(timeLineAddTickBehaviour,
                    new GUIContent("Tick data save behaviour",
                        "The timeline behaviour when adding saving a tick data. Should it clear, insert or replace existing data."));
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void CreateEventsSection()
        {
            eventsSection = EditorGUILayout.BeginFoldoutHeaderGroup(eventsSection, new GUIContent("Events"));
            if (eventsSection)
            {
                EditorGUILayout.PropertyField(onInitTimeAgentsList);
                EditorGUILayout.PropertyField(onTimeLineClear);
                EditorGUILayout.PropertyField(onTimeLineChange);
                EditorGUILayout.PropertyField(onSimulationStart);
                EditorGUILayout.PropertyField(onSimulationComplete);
                EditorGUILayout.PropertyField(onRecordStop);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void CreateDebugSection()
        {
            debugSection = EditorGUILayout.BeginFoldoutHeaderGroup(debugSection, new GUIContent("Debug options"));
            if (debugSection)
            {
                EditorGUILayout.LabelField("Data", SharedGui.subheaderGuiStyle);
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(timeAgents);
                EditorGUILayout.PropertyField(addTickCursor);
                EditorGUILayout.PropertyField(playTickCursor);
                EditorGUILayout.PropertyField(simulationTime);
                EditorGUILayout.PropertyField(frameTimeStack);
                EditorGUILayout.PropertyField(simulationTimeAgents);
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;

                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("Logs", SharedGui.subheaderGuiStyle);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(logDebug,
                    new GUIContent("Verbose", "Activate all logging messages, except performances"));
                EditorGUILayout.PropertyField(logPerfs,
                    new GUIContent("Performances", "Activate all performances logs (simulation time, etc.)"));

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private int GetMaxTick()
        {
            return Mathf.Max(stone.GetTickCount() - 1, 0);
        }
    }
}