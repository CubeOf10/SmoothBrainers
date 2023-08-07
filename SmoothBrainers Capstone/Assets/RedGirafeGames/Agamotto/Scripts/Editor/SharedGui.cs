using UnityEditor;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Scripts.Editor
{
    /// <summary>
    /// Shared resources for Editor GUI creation
    /// </summary>
    public static class SharedGui
    {
        private const string StopIconPath = "Assets/RedGirafeGames/Agamotto/Art/Images/Stop_icon.png";
        private const string PlayIconPath = "Assets/RedGirafeGames/Agamotto/Art/Images/Play_icon.png";
        private const string ToStartIconPath = "Assets/RedGirafeGames/Agamotto/Art/Images/ToStart_icon.png";
        private const string ToEndIconPath = "Assets/RedGirafeGames/Agamotto/Art/Images/ToEnd_icon.png";
        
        /*
         * Icons
         */
        public static Texture2D playIcon;
        public static Texture2D stopIcon;
        public static Texture2D toStartIcon;
        public static Texture2D toEndIcon;
        
        /*
         * Styles
         */
        public static GUIStyle headerGuiStyle;
        public static GUIStyle subheaderGuiStyle;
        public static GUIStyle infosLabelGuiStyle;
        public static GUIStyle cloneLabelGuiStyle;
        public static GUIStyle secondaryButtonGuiStyle;
        public static GUIStyle tableHeaderGuiStyle;
        public static GUIStyle tableLineGuiStyle;

        public static void InitStyles()
        {
            headerGuiStyle = "CN CountBadge";
            infosLabelGuiStyle = "PR PrefabLabel";
            subheaderGuiStyle = EditorStyles.boldLabel;
            cloneLabelGuiStyle = "OL Ping";
            secondaryButtonGuiStyle = "Tab middle";
            tableHeaderGuiStyle = "TimeAreaToolbar";
            tableLineGuiStyle = "Tooltip";
        }

        public static void InitTextures()
        {
            playIcon = (Texture2D) AssetDatabase.LoadAssetAtPath(PlayIconPath, typeof(Texture2D));
            stopIcon = (Texture2D) AssetDatabase.LoadAssetAtPath(StopIconPath, typeof(Texture2D));
            toStartIcon = (Texture2D) AssetDatabase.LoadAssetAtPath(ToStartIconPath, typeof(Texture2D));
            toEndIcon = (Texture2D) AssetDatabase.LoadAssetAtPath(ToEndIconPath, typeof(Texture2D));
        }
    }
}