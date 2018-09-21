using UnityEngine;

namespace UnityEditor.Recorder
{
    static class StatusBarHelper
    {
        static Texture2D s_ErrorIcon;
        static Texture2D s_WarningIcon;
        static Texture2D s_InfoIcon;

        static GUIStyle s_ErrorStyle;
        static GUIStyle s_WarningStyle;
        static GUIStyle s_InfoStyle;

        public static Texture2D errorIcon
        {
            get
            {
                if (s_ErrorIcon == null)
                    s_ErrorIcon = EditorGUIUtility.Load("Icons/console.erroricon.sml.png") as Texture2D;

                return s_ErrorIcon;
            }
        }
        
        public static Texture2D warningIcon
        {
            get
            {
                if (s_WarningIcon == null)
                    s_WarningIcon = EditorGUIUtility.Load("Icons/console.warnicon.sml.png") as Texture2D;

                return s_WarningIcon;
            }
        }

        public static Texture2D infoIcon
        {
            get
            {
                if (s_InfoIcon == null)
                    s_InfoIcon = EditorGUIUtility.Load("Icons/console.infoicon.sml.png") as Texture2D;

                return s_InfoIcon;
            }
        }
            
        public static GUIStyle errorStyle
        {
            get
            {
                return s_ErrorStyle ?? (s_ErrorStyle = new GUIStyle("CN StatusError"));
            }
        }
        
        public static GUIStyle warningStyle
        {
            get
            {
                return s_WarningStyle ?? (s_WarningStyle = new GUIStyle("CN StatusWarn"));
            }
        }

        public static GUIStyle infoStyle
        {
            get
            {
                return s_InfoStyle ?? (s_InfoStyle = new GUIStyle("CN StatusInfo"));
            }
        }


    }
}