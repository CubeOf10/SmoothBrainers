using System;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Scripts.Runtime.Utils
{
    public static class GameObjectUtils
    {
        /// <summary>
        /// The ways a GameObject can be hidden
        /// </summary>
        public enum HidePolicy
        {
            NoHide,

            /// <summary>
            /// The simulation agent's Renderer (if exists) is disabled.
            /// </summary>
            DisableRenderer
        }

        /// <summary>
        /// The property to modify material's color
        /// </summary>
        private static readonly int MaterialColorProperty = Shader.PropertyToID("_Color");

        /// <summary>
        /// Hides a GameObject using given policy
        /// </summary>
        /// <param name="gameObject">The gameObject to hide</param>
        /// <param name="hidePolicy">The hiding policy</param>
        /// <param name="applyOnChildren">Applies on children</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void Hide(GameObject gameObject, HidePolicy hidePolicy, bool applyOnChildren)
        {
            switch (hidePolicy)
            {
                case HidePolicy.NoHide:
                    break;
                case HidePolicy.DisableRenderer:
                    gameObject.TryGetComponent<Renderer>(out var cloneRend);
                    if (cloneRend != null)
                        cloneRend.enabled = false;
                    if (applyOnChildren)
                    {
                        foreach (var rend in gameObject.GetComponentsInChildren<Renderer>())
                        {
                            rend.enabled = false;
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// Shows a GameObject using given policy
        /// </summary>
        /// <param name="gameObject">The gameObject to hide</param>
        /// <param name="hidePolicy">The hiding policy</param>
        /// <param name="applyOnChildren">Applies on children</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void Show(GameObject gameObject, HidePolicy hidePolicy, bool applyOnChildren)
        {
            switch (hidePolicy)
            {
                case HidePolicy.NoHide:
                    break;
                case HidePolicy.DisableRenderer:
                    gameObject.TryGetComponent<Renderer>(out var cloneRend);
                    if (cloneRend != null)
                        cloneRend.enabled = true;
                    if (applyOnChildren)
                    {
                        foreach (var rend in gameObject.GetComponentsInChildren<Renderer>())
                        {
                            rend.enabled = true;
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Apply material and color on a GameObject's renderers
        /// </summary>
        /// <param name="gameObject">The GameObject to modify</param>
        /// <param name="applyOnChildren">Applies on children</param>
        /// <param name="material">The material to apply, null if no material has to be applied</param>
        /// <param name="applyColor">If the color must be applied</param>
        /// <param name="color">The color to apply</param>
        public static void ApplyColorAndMaterial(GameObject gameObject, bool applyOnChildren, Material material,
            bool applyColor, Color color)
        {
            var applyMaterial = material != null;
            if (applyColor || applyMaterial)
            {
                gameObject.TryGetComponent<Renderer>(out var cloneRend);
                if (applyMaterial)
                    ApplyMaterialOnRenderer(cloneRend, material);
                if (applyColor)
                    ApplyColorOnRenderer(cloneRend, color);

                if (applyOnChildren)
                {
                    foreach (var rend in gameObject.GetComponentsInChildren<Renderer>())
                    {
                        if (applyMaterial)
                            ApplyMaterialOnRenderer(rend, material);
                        if (applyColor)
                            ApplyColorOnRenderer(rend, color);
                    }
                }
            }
        }

        /// <summary>
        /// Apply material to all materials of a Renderer
        /// </summary>
        /// <param name="rend">The Renderer modified</param>
        /// <param name="material">The material to apply</param>
        public static void ApplyMaterialOnRenderer(Renderer rend, Material material)
        {
            if (rend == null || rend.materials.Length <= 0) return;

            var tempMaterials = rend.materials;
            for (var i = 0; i < rend.materials.Length; i++)
            {
                tempMaterials[i] = material;
            }

            rend.materials = tempMaterials;
        }

        /// <summary>
        /// Apply Color to all materials of a Renderer
        /// </summary>
        /// <param name="rend">The Renderer modified</param>
        /// <param name="color">The color to apply</param>
        public static void ApplyColorOnRenderer(Renderer rend, Color color)
        {
            if (rend == null || rend.materials.Length <= 0) return;

            var tempMaterials = rend.materials;
            for (var i = 0; i < rend.materials.Length; i++)
            {
                tempMaterials[i].SetColor(MaterialColorProperty, color);
            }

            rend.materials = tempMaterials;
        }
    }
}