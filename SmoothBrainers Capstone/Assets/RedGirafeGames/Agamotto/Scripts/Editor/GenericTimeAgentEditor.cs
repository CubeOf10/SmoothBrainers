using System;
using System.Collections.Generic;
using RedGirafeGames.Agamotto.Scripts.Runtime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using UnityEditor;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Scripts.Editor
{
    [CustomEditor(typeof(GenericTimeAgent))]
    public class GenericTimeAgentEditor : TimeAgentEditor
    {
        /*
         * Serialized elements
         */
        private SerializedProperty autoPersistedDataIdComponentNames;
        private SerializedProperty autoPersistedDataIdMemberNames;

        /*
         * Private fields
         */
        private GenericTimeAgent _genericTimeAgent;

        private bool addDataIdRawMode = false;
        private string addDataIdComponentName = "";
        private string addDataIdMemberName = "";
        private Component addDataIdComponent;
        private int addDataIdMemberIndex;
        private string addDataIdMember;

        protected override void OnEnable()
        {
            base.OnEnable();
            _genericTimeAgent = ((GenericTimeAgent) serializedObject.targetObject);
            autoPersistedDataIdComponentNames = serializedObject.FindProperty("autoPersistedDataIdComponentNames");
            autoPersistedDataIdMemberNames = serializedObject.FindProperty("autoPersistedDataIdMemberNames");
        }

        public override void OnInspectorGUI()
        {
            SharedGui.InitStyles();

            serializedObject.Update();

            LoadEditorPrefs();
            EditorGUI.BeginChangeCheck();

            CreateDataIdSection();
            EditorGUILayout.Separator();

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                SaveEditorPrefs();
            }

            base.OnInspectorGUI();
        }

        private void LoadEditorPrefs()
        {
        }

        private void SaveEditorPrefs()
        {
        }

        private void CreateDataIdSection()
        {
            EditorGUILayout.LabelField("Custom Persisted Data", SharedGui.headerGuiStyle);
            EditorGUILayout.Separator();

            // Creates a Table with a header to display existing data ids
            EditorGUILayout.BeginHorizontal(SharedGui.tableHeaderGuiStyle);
            EditorGUILayout.LabelField("Component");
            EditorGUILayout.LabelField("Member");
            EditorGUILayout.EndHorizontal();
            if (_genericTimeAgent.autoPersistedDataIdComponentTypes.Count == 0)
            {
                EditorGUILayout.BeginHorizontal(SharedGui.tableLineGuiStyle);
                EditorGUILayout.LabelField("- List is empty -");
                EditorGUILayout.EndHorizontal();
            }
            for (var i = 0; i < _genericTimeAgent.autoPersistedDataIdComponentTypes.Count; i++)
            {
                var compName = _genericTimeAgent.autoPersistedDataIdComponentTypes[i];
                var memberName = _genericTimeAgent.autoPersistedDataIdMemberNames[i];
                EditorGUILayout.BeginHorizontal(SharedGui.tableLineGuiStyle);
                EditorGUILayout.LabelField(compName);
                EditorGUILayout.LabelField(memberName);
                if (GUILayout.Button("Delete"))
                {
                    _genericTimeAgent.RemoveCustomDataId(compName, memberName);
                }

                EditorGUILayout.EndHorizontal();
            }
            // End data table

            EditorGUILayout.Separator();

            // Add new data id panel, 2 options raw or assisted to switch
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(addDataIdRawMode ? "Assisted mode" : "Raw mode", SharedGui.secondaryButtonGuiStyle, GUILayout.MaxWidth(100)))
            {
                addDataIdRawMode = !addDataIdRawMode;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (addDataIdRawMode)
            {
                addDataIdComponentName = EditorGUILayout.TextField("Component", addDataIdComponentName);
                addDataIdMemberName = EditorGUILayout.TextField("Member", addDataIdMemberName);
            }
            else
            {
                var oldDataIdComponent = addDataIdComponent;

                EditorGUILayout.BeginHorizontal();
                addDataIdComponent =
                    (Component) EditorGUILayout.ObjectField(addDataIdComponent, typeof(Component), true);

                if (oldDataIdComponent != addDataIdComponent || addDataIdComponent == null)
                {
                    // Reset index if selection changed because old index could be out of bounds
                    addDataIdMemberIndex = -1;
                }

                GUI.enabled = addDataIdComponent != null;
                string[] members = new string[0];

                if (addDataIdComponent == null)
                {
                    addDataIdComponentName = "";
                }
                else
                {
                    Type componentType = addDataIdComponent.GetType();
                    addDataIdComponentName = componentType.Name;

                    var props = componentType.GetProperties();

                    var fields = componentType.GetFields();
                    
                    members = new string[props.Length + fields.Length];
                    for (var i = 0; i < props.Length; i++)
                    {
                        var propertyInfo = props[i];
                        members[i] = propertyInfo.Name;
                    }
                    for (var i = 0; i < fields.Length; i++)
                    {
                        var fieldInfo = fields[i];
                        members[props.Length + i] = fieldInfo.Name;
                    }
                }

                addDataIdMemberIndex = EditorGUILayout.Popup(new GUIContent("", "Select component's member to persist"),
                    addDataIdMemberIndex, members);
                if (addDataIdMemberIndex >= 0)
                {
                    addDataIdMemberName = members[addDataIdMemberIndex];
                }
                else
                {
                    addDataIdMemberName = "";
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("Drag and drop the component with the data to persist then select the member",
                    MessageType.None);

                GUI.enabled = true;
            }

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = _genericTimeAgent.autoPersistedDataIdComponentTypes.Count > 0;
            if (GUILayout.Button("Clear"))
            {
                Undo.RecordObject(target, "Clear Data");
                _genericTimeAgent.Clear();
            }
            
            GUI.enabled = addDataIdComponentName != "" && addDataIdMemberName != "";
            if (GUILayout.Button("Add Data"))
            {
                // Undo/redo support for changes made to behaviour fields.
                Undo.RecordObject(target, "Add Data");
                var addResult = _genericTimeAgent.AddCustomDataId(addDataIdComponentName,
                    addDataIdMemberName);
                if (!addResult)
                {
                    EditorUtility.DisplayDialog("Add data id impossible",
                        "The dataId <" + addDataIdComponentName + "> <" + addDataIdMemberName +
                        "> could not be added. Duplicate maybe?", "OK");
                }
            }
            // End add data id panel 

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}