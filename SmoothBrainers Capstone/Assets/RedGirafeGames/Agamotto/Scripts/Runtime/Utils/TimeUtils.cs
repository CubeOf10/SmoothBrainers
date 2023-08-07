using System;
using System.Collections.Generic;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Scripts.Runtime.Utils
{
    /// <summary>
    /// Utility class to manipulate Time
    /// </summary>
    public static class TimeUtils
    {
        /// <summary>
        /// The different ways the time can be frozen for rigidbodys
        /// </summary>
        public enum RigidbodyFreezeType
        {
            /// <summary>
            /// Sets the TimeScale to 0.0. This type has a lot of side effects.
            /// </summary>
            TimeScale,
            /// <summary>
            /// Default type. Sets <see cref="Physics.autoSimulation"/> to false. Simple and effective but it affects all objects with no distinction
            /// </summary>
            PhysicsAutoSimulation,
            /// <summary>
            /// Sets <see cref="Rigidbody.isKinematic"/> to true
            /// </summary>
            IsKinematic,
            /// <summary>
            /// Calls <see cref="Rigidbody.Sleep"/> on object
            /// </summary>
            Sleep,
            /// <summary>
            /// Nothing is done... maybe not the best way to freeze objects...
            /// </summary>
            None
        }

        /// <summary>
        /// Freeze or UnFreeze the given timeAgents list
        /// </summary>
        /// <remarks>Can also be used directly from <see cref="TimeStone.FreezeTimeAgents(bool)"/></remarks>
        /// <param name="timeAgents">The list of <see cref="TimeAgent"/> to freeze</param>
        /// <param name="state">The freeze state</param>
        /// <param name="rigidbodyFreezeType">The type of freeze applied to rigidbodys</param>
        public static void Freeze(List<TimeAgent> timeAgents, bool state, RigidbodyFreezeType rigidbodyFreezeType)
        {
            foreach (var timeAgent in timeAgents)
            {
                Freeze(timeAgent, state, rigidbodyFreezeType);
            }
        }

        /// <summary>
        /// Freeze or Unfreeze the given <see cref="TimeAgent"/>
        /// </summary>
        /// <param name="timeAgent">The list of <see cref="TimeAgent"/> to freeze</param>
        /// <param name="state">The freeze state</param>
        /// <param name="rigidbodyFreezeType">The type of freeze applied to rigidbodys</param>
        public static void Freeze(TimeAgent timeAgent, bool state, RigidbodyFreezeType rigidbodyFreezeType)
        {
            if (timeAgent == null)
                return;
            
            if (timeAgent.agentRigidbody != null)
            {
                FreezeRigidbody(timeAgent.agentRigidbody, rigidbodyFreezeType, state);
            }

            if (timeAgent.persistAnimator)
            {
                FreezeAnimator(timeAgent.agentAnimator, state);
            }

            if (timeAgent.persistParticles)
            {
                foreach (var ps in timeAgent.persistParticlesList)
                {
                    FreezeParticleSystem(ps, state);
                }
            }
        }

        /// <summary>
        /// Freeze or Unfreeze given list of GameObjects
        /// </summary>
        /// <param name="gameObjects">The list of GameObjects</param>
        /// <param name="state">The freeze state</param>
        /// <param name="rigidbodyFreezeType">The type of freeze applied to rigidbodys</param>
        /// <param name="freezeAnimator">Freeze Animator components</param>
        /// <param name="freezeParticles">Freeze GameObject's ParticleSystems</param>
        public static void Freeze(List<GameObject> gameObjects, bool state, RigidbodyFreezeType rigidbodyFreezeType,
            bool freezeAnimator,
            bool freezeParticles)
        {
            foreach (var go in gameObjects)
            {
                Freeze(go, state, rigidbodyFreezeType, freezeAnimator, freezeParticles);
            }
        }

        /// <summary>
        /// Freeze or Unfreeze given GameObject
        /// </summary>
        /// <param name="go">The GameObjects</param>
        /// <param name="state">The freeze state</param>
        /// <param name="rigidbodyFreezeType">The type of freeze applied to rigidbodys</param>
        /// <param name="freezeAnimator">Freeze Animator components</param>
        /// <param name="freezeParticles">Freeze GameObject's ParticleSystems</param>
        public static void Freeze(GameObject go, bool state, RigidbodyFreezeType rigidbodyFreezeType,
            bool freezeAnimator, bool freezeParticles)
        {
            if (go == null)
                return;
            
            go.TryGetComponent<Rigidbody>(out var rigidbody);
            if (rigidbody != null)
            {
                FreezeRigidbody(rigidbody, rigidbodyFreezeType, state);
            }

            if (freezeAnimator)
            {
                go.TryGetComponent<Animator>(out var animator);
                if (animator != null)
                {
                    FreezeAnimator(animator, state);
                }
            }

            if (freezeParticles)
            {
                var particleSystems = go.GetComponents<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    FreezeParticleSystem(ps, state);
                }
            }
        }

        /// <summary>
        /// Freeze a RigidBody component
        /// </summary>
        /// <param name="rigidbody">The Rigidbody</param>
        /// <param name="freezeType">The type of freeze</param>
        /// <param name="state">The state of freeze</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void FreezeRigidbody(Rigidbody rigidbody, RigidbodyFreezeType freezeType, bool state)
        {
            switch (freezeType)
            {
                case RigidbodyFreezeType.PhysicsAutoSimulation:
                    Physics.autoSimulation = !state;
                    break;
                case RigidbodyFreezeType.IsKinematic:
                    rigidbody.isKinematic = state;
                    break;
                case RigidbodyFreezeType.Sleep:
                    if (state) 
                        rigidbody.Sleep();
                    else 
                        rigidbody.WakeUp();
                    break;
                case RigidbodyFreezeType.None:
                    break;
                case RigidbodyFreezeType.TimeScale:
                    Time.timeScale = state ? 0.0f : 1.0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(freezeType), freezeType,
                        "What the fuck have you done to generate that O_o");
            }
        }

        /// <summary>
        /// Freeze an Animator component
        /// </summary>
        /// <param name="animator">The Animator</param>
        /// <param name="state">The state of freeze</param>
        public static void FreezeAnimator(Animator animator, bool state)
        {
            animator.speed = state ? 0.0f : 1.0f;
        }

        /// <summary>
        /// Freeze a ParticleSystem
        /// </summary>
        /// <param name="ps">The particleSystem</param>
        /// <param name="state">The state of freeze</param>
        public static void FreezeParticleSystem(ParticleSystem ps, bool state)
        {
            if (state)
            {
                ps.Pause();
            }
            else
            {
                ps.Play();
            }
        }
    }
}