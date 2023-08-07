using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Scripts.Runtime.Agents
{
    /// <summary>
    /// TimeAgent implementation adding a generic way to persist custom properties.
    ///
    /// <para>This component is made to make things simple, but it should not be used in production if performances are critical.
    /// Because Reflection is used to get custom properties values and it will always be way more slower than direct access.</para>
    ///
    /// <para>To get the same result with no performance impact, use TimeAgent UnityEvent or extend TimeAgent with your own class</para>
    /// 
    /// </summary>
    [RequireComponent(typeof(Transform))]
    [AddComponentMenu("Agamotto/Generic Time Agent")]
    public class GenericTimeAgent : TimeAgent
    {
        /// <summary>
        /// The separator used to differentiate ComponentName and MemberName in DataId
        /// </summary>
        private const string PersistedDataIdMemberSeparator = "#";

        /// <summary>
        /// The list of the ComponentNames persisted, must be synchronized with <see cref="autoPersistedDataIdMemberNames"/>
        /// </summary>
        public List<string> autoPersistedDataIdComponentTypes = new List<string>();
        /// <summary>
        /// The list of the MemberNames persisted, must be synchronized with <see cref="autoPersistedDataIdComponentTypes"/>
        /// </summary>
        public List<string> autoPersistedDataIdMemberNames = new List<string>();

        /// <summary>
        /// Cached listing of the GameObject's components by their type
        /// If you add components dynamically, call <see cref="CacheComponentsByType"/> to update this list
        /// </summary>
        private readonly Dictionary<string, Component> _componentsByType = new Dictionary<string, Component>();

        /// <summary>
        /// Flag if the component is initialized
        /// </summary>
        private bool _initialized;

        private void Start()
        {
            InitGenericTimeAgent();
        }

        /// <summary>
        /// Initializes the GenericTimeAgent
        /// </summary>
        /// <param name="forceInit"></param>
        private void InitGenericTimeAgent(bool forceInit = false)
        {
            if (_initialized && !forceInit)
                return;

            // Initialization of the  
            CacheComponentsByType();

            _initialized = true;
        }

        /// <summary>
        /// SimulationSceneReady callback
        /// </summary>
        /// <param name="stone"></param>
        public override void SimulationSceneReady(TimeStone stone)
        {
            base.SimulationSceneReady(stone);
            
            if (logDebug) Debug.Log("[GenericTimeAgent] " + name + " : Simulation scene ready");

            InitGenericTimeAgent();
        }

        /// <summary>
        /// Add a custom data to be persisted to the component's list.
        /// No security is made to check if the component and its member really exist
        /// </summary>
        /// <param name="componentType">The type of the component</param>
        /// <param name="memberName">The name of the component's member</param>
        /// <returns></returns>
        public bool AddCustomDataId(string componentType, string memberName)
        {
            // Check for duplicate
            for (int i = 0; i < autoPersistedDataIdComponentTypes.Count; i++)
            {
                var compName = autoPersistedDataIdComponentTypes[i];
                if (compName == componentType && autoPersistedDataIdMemberNames[i] == memberName)
                    return false;
            }

            autoPersistedDataIdComponentTypes.Add(componentType);
            autoPersistedDataIdMemberNames.Add(memberName);
            return true;
        }

        /// <summary>
        /// Remove a custom data from automatic persistence.
        /// </summary>
        /// <param name="componentType">The component's type</param>
        /// <param name="memberName">The name of the component's member</param>
        /// <returns>If the data removed</returns>
        public bool RemoveCustomDataId(string componentType, string memberName)
        {
            // Check existence
            for (var i = 0; i < autoPersistedDataIdComponentTypes.Count; i++)
            {
                var compName = autoPersistedDataIdComponentTypes[i];
                if (compName == componentType && autoPersistedDataIdMemberNames[i] == memberName)
                {
                    autoPersistedDataIdComponentTypes.Remove(componentType);
                    autoPersistedDataIdMemberNames.Remove(memberName);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Clear automatically persisted components and members
        /// </summary>
        public void Clear()
        {
            autoPersistedDataIdComponentTypes.Clear();
            autoPersistedDataIdMemberNames.Clear();
        }

        /// <summary>
        /// Override of <see cref="TimeAgent.PersistTick"/> to add the persistence of custom generic data using Reflection
        /// </summary>
        /// <param name="stone">The stone calling</param>
        /// <param name="origin">The origin of the call, Record or Simulation</param>
        /// <param name="deltaTime">The deltaTime</param>
        public override void PersistTick(TimeStone stone, TimeStone.TimeTickOrigin origin, float deltaTime)
        {
            base.PersistTick(stone, origin, deltaTime);

            for (var i = 0; i < autoPersistedDataIdComponentTypes.Count; i++)
            {
                var componentTypeName = autoPersistedDataIdComponentTypes[i];
                var propertyOrField = autoPersistedDataIdMemberNames[i];
                var dataId = AutoPersistedDataIdToString(componentTypeName, propertyOrField);

                Component component = GetComponentByType(componentTypeName);
                Type componentType = component.GetType();
                PropertyInfo property = componentType.GetProperty(propertyOrField);
                if (property != null)
                {
                    stone.PersistData(this, origin, deltaTime, dataId,
                        property.GetValue(component));
                    continue;
                }

                FieldInfo field = componentType.GetField(propertyOrField);
                if (field != null)
                {
                    stone.PersistData(this, origin, deltaTime, dataId,
                        field.GetValue(component));
                    continue;
                }
            }
        }

        /// <summary>
        /// Override of <see cref="TimeAgent.SetDataTick"/> to apply custom generic properties using Reflection
        /// </summary>
        /// <param name="stone"></param>
        /// <param name="tick"></param>
        public override void SetDataTick(TimeStone stone, int tick)
        {
            base.SetDataTick(stone, tick);

            for (var i = 0; i < autoPersistedDataIdComponentTypes.Count; i++)
            {
                var componentTypeName = autoPersistedDataIdComponentTypes[i];
                var propertyOrField = autoPersistedDataIdMemberNames[i];
                var dataId = AutoPersistedDataIdToString(componentTypeName, propertyOrField);
                
                var value = stone.GetDataValueAt<object>(tick, this, dataId,
                    out var dataExists);
                if (!dataExists)
                    continue;

                Component component = GetComponent(componentTypeName);
                Type componentType = component.GetType();

                PropertyInfo property = componentType.GetProperty(propertyOrField);
                if (property != null)
                {
                    property.SetValue(component, value);
                    continue;
                }

                FieldInfo field = componentType.GetField(propertyOrField);
                if (field != null)
                {
                    field.SetValue(component, value);
                    continue;
                }

                Debug.LogError("[GenericTimeAgent] " + name + " : ERROR during SetTime <" + dataId +
                               ">, field or property not found on <" +
                               componentTypeName + ">");
            }
        }

        /// <summary>
        /// Cache GameObject's components by type
        /// </summary>
        public void CacheComponentsByType()
        {
            _componentsByType.Clear();
            foreach (var component in GetComponents<Component>())
            {
                Type componentType = component.GetType();
                _componentsByType[componentType.Name] = component;
            }
        }

        /// <summary>
        /// Get GameObject's component by its type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Component GetComponentByType(string type)
        {
            if (_componentsByType.ContainsKey(type))
            {
                return _componentsByType[type];
            }

            Debug.LogError("[GenericTimeAgent] " + name + " : No component with name <" + type +
                           "> available. If it was dynamically added, be sure to call CacheComponentsByName to update cache.");
            return default;
        }

        /// <summary>
        /// Concatenate a componentType and its memberName to generate a dataId
        /// </summary>
        /// <param name="componentType">The component's type</param>
        /// <param name="memberName">The name of the component's member</param>
        /// <returns></returns>
        private static string AutoPersistedDataIdToString(string componentType, string memberName)
        {
            return componentType + PersistedDataIdMemberSeparator + memberName;
        }
    }
}