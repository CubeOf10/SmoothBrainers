using System.Collections.Generic;
using System.Linq;
using RedGirafeGames.Agamotto.Demo;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace RedGirafeGames.Agamotto.Scripts.Runtime.Utils
{
    /// <summary>
    /// Utility class to find <see cref="TimeStone"/> agents
    /// </summary>
    public static class TimeStoneUtils
    {
        /// <summary>
        /// Equivalent to <c>GameObject.Find</c> but with a scope limited to stone's simulation agents
        /// </summary>
        /// <param name="stone">The stone to search in</param>
        /// <param name="name">The name of the GameObject searched</param>
        /// <returns>The first active GameObject found in TimeStone's simulation agents with that name</returns>
        public static GameObject FindInSimulationAgents(TimeStone stone, string name)
        {
            foreach (var simulationTimeAgent in stone.simulationTimeAgents)
            {
                if (simulationTimeAgent != null &&
                    simulationTimeAgent.gameObject.activeSelf &&
                    simulationTimeAgent.gameObject.name.Equals(name))
                {
                    return simulationTimeAgent.gameObject;
                }
            }

            return null;
        }

        /// <summary>
        /// Equivalent to <c>GameObject.FindObjectOfType</c> but with a scope limited to stone's simulation agents
        /// </summary>
        /// <param name="stone">The stone to search in</param>
        /// <typeparam name="T">The type of object searched</typeparam>
        /// <returns>The first object of this type in TimeStone's simulation agents</returns>
        public static T FindObjectOfTypeInSimulationAgents<T>(TimeStone stone)
        {
            foreach (var simulationTimeAgent in stone.simulationTimeAgents)
            {
                if (simulationTimeAgent != null && simulationTimeAgent.TryGetComponent<T>(out var c))
                {
                    return c;
                }
            }

            return default;
        }

        /// <summary>
        /// Equivalent to <c>GameObject.FindObjectsOfType</c> but with a scope limited to stone's simulation agents
        /// </summary>
        /// <param name="stone">The stone to search in</param>
        /// <typeparam name="T">The type of object searched</typeparam>
        /// <returns>All objects of this type in TimeStone's simulation agents</returns>
        public static T[] FindObjectsOfTypeInSimulationAgents<T>(TimeStone stone)
        {
            var result = new List<T>();

            foreach (var simulationTimeAgent in stone.simulationTimeAgents)
            {
                if (simulationTimeAgent != null && simulationTimeAgent.TryGetComponent<T>(out var c))
                {
                    result.Add(c);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Equivalent to <c>GameObject.FindGameObjectWithTag</c> but with a scope limited to stone's simulation agents
        /// </summary>
        /// <param name="stone">The stone to search in</param>
        /// <param name="tag">The searched tag</param>
        /// <returns>The first object with this tag in TimeStone's simulation agents</returns>
        public static GameObject FindGameObjectWithTagInSimulationAgents(TimeStone stone, string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return null;

            foreach (var simulationTimeAgent in stone.simulationTimeAgents)
            {
                if (simulationTimeAgent != null && simulationTimeAgent.CompareTag(tag))
                {
                    return simulationTimeAgent.gameObject;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Equivalent to <c>GameObject.FindGameObjectWithTag</c> but with a scope limited to stone's simulation scene
        /// </summary>
        /// <param name="stone">The stone to search in</param>
        /// <param name="tag">The searched tag</param>
        /// <returns>The first object with this tag in TimeStone's simulation scene</returns>
        public static GameObject FindGameObjectWithTagInSimulationScene(TimeStone stone, string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return null;

            var simulationScene = stone.simulationScene;
            
            var foundObjects = GameObject.FindGameObjectsWithTag(tag);
            return foundObjects.FirstOrDefault(obj =>
                 obj.scene.name.Equals(simulationScene.name));
            
        }

        /// <summary>
        /// Equivalent to <c>GameObject.FindGameObjectsWithTag</c> but with a scope limited to stone's simulation agents
        /// </summary>
        /// <param name="stone">The stone to search in</param>
        /// <param name="tag">The searched tag</param>
        /// <returns>All the objects with this tag in TimeStone's simulation agents</returns>
        public static GameObject[] FindGameObjectsWithTagInSimulationAgents(TimeStone stone, string tag)
        {
            var result = new List<GameObject>();

            if (string.IsNullOrEmpty(tag))
                return null;

            foreach (var simulationTimeAgent in stone.simulationTimeAgents)
            {
                if (simulationTimeAgent != null && simulationTimeAgent.CompareTag(tag))
                {
                    result.Add(simulationTimeAgent.gameObject);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Equivalent to <c>GameObject.FindGameObjectsWithTag</c> but with a scope limited to stone's simulation scene
        /// </summary>
        /// <param name="stone">The stone to search in</param>
        /// <param name="tag">The searched tag</param>
        /// <returns>All the objects with this tag in TimeStone's simulation scene</returns>
        public static GameObject[] FindGameObjectsWithTagInSimulationScene(TimeStone stone, string tag)
        {
            var result = new List<GameObject>();

            if (string.IsNullOrEmpty(tag))
                return null;
            
            var simulationScene = stone.simulationScene;
            
            var foundObjects = GameObject.FindGameObjectsWithTag(tag);
            return foundObjects.Where(obj =>
                obj.scene.name.Equals(simulationScene.name)).ToArray();
        }
        
        /// <summary>
        /// Equivalent to <c>Object.FindObjectOfType</c> but automatically excluding any cloned simulation agents
        /// </summary>
        /// <typeparam name="T">The object type searched</typeparam>
        /// <returns>The first object of this type that is not a cloned simulation agent</returns>
        public static T FindObjectOfTypeWithoutSimulationAgents<T>() where T : Object
        {
            var foundObjects = Object.FindObjectsOfType<T>();
            return foundObjects.FirstOrDefault(obj =>
                (obj is GameObject gameObject
                 && (!gameObject.TryGetComponent<TimeAgent>(out var timeAgent) || !timeAgent.IsClone))
                || (obj is Component component
                    && (!component.TryGetComponent<TimeAgent>(out timeAgent) || !timeAgent.IsClone))
                || (!(obj is GameObject) && !(obj is Component)));
        }

        /// <summary>
        /// Equivalent to <c>Object.FindObjectsOfType</c> but automatically excluding any cloned simulation agents
        /// </summary>
        /// <typeparam name="T">The object type searched</typeparam>
        /// <returns>All objects of this type that is not a cloned simulation agent</returns>
        public static T[] FindObjectsOfTypeWithoutSimulationAgents<T>() where T : Object
        {
            var foundObjects = Object.FindObjectsOfType<T>();
            return foundObjects.Where(obj =>
                (obj is GameObject gameObject
                 && (!gameObject.TryGetComponent<TimeAgent>(out var timeAgent) || !timeAgent.IsClone))
                || (!(obj is GameObject) && !(obj is Component))).ToArray();
        }

        /// <summary>
        /// Equivalent to <c>GameObject.FindGameObjectWithTag</c> but automatically excluding any cloned simulation agents
        /// </summary>
        /// <param name="tag">The searched tag</param>
        /// <returns>The first object with this tag in TimeStone's simulation agents</returns>
        public static GameObject FindGameObjectWithTagWithoutSimulationAgents(string tag)
        {
            var foundGameObjects = GameObject.FindGameObjectsWithTag(tag);
            return foundGameObjects.FirstOrDefault(obj =>
                !obj.TryGetComponent<TimeAgent>(out var timeAgent) || !timeAgent.IsClone);
        }

        /// <summary>
        /// Equivalent to <c>GameObject.FindGameObjectsWithTag</c> but automatically excluding any cloned simulation agents
        /// </summary>
        /// <param name="tag">The searched tag</param>
        /// <returns>The first object with this tag in TimeStone's simulation agents</returns>
        public static GameObject[] FindGameObjectsWithTagWithoutSimulationAgents(string tag)
        {
            var foundGameObjects = GameObject.FindGameObjectsWithTag(tag);
            return foundGameObjects.Where(obj =>
                !obj.TryGetComponent<TimeAgent>(out var timeAgent) || !timeAgent.IsClone).ToArray();
        }


        /// <summary>
        /// Search for all time agents (even inactive) in current scene, excluding simulation agents 
        /// </summary>
        /// <returns></returns>
        public static List<TimeAgent> GetAllTimeAgentsWithoutSimulationAgents()
        {
            List<TimeAgent> agentsInScene = new List<TimeAgent>();

            foreach (TimeAgent timeAgent in Resources.FindObjectsOfTypeAll<TimeAgent>())
            {
#if UNITY_EDITOR
                if (EditorUtility.IsPersistent(timeAgent.gameObject.transform.root.gameObject))
                    continue;
#endif
                if (!(timeAgent.gameObject.hideFlags == HideFlags.NotEditable ||
                      timeAgent.hideFlags == HideFlags.HideAndDontSave) && !timeAgent.IsClone)
                    agentsInScene.Add(timeAgent);
            }

            return agentsInScene;
        }
    }
}