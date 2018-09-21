using System.IO;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomPropertyDrawer(typeof(OutputPath))]
    class OutputPathDrawer : TargetedPropertyDrawer<OutputPath>
    {
        SerializedProperty m_RootProperty;
        SerializedProperty m_LeafProperty;
        SerializedProperty m_ForceAssetFolder;

        protected override void Initialize(SerializedProperty property)
        {
            base.Initialize(property);
            
            if (m_RootProperty == null)
                m_RootProperty = property.FindPropertyRelative("m_Root");
            
            if (m_LeafProperty == null)
                m_LeafProperty = property.FindPropertyRelative("m_Leaf");
            
            if (m_ForceAssetFolder == null)
                m_ForceAssetFolder = property.FindPropertyRelative("m_ForceAssetFolder");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            const float rootWidth = 110.0f;
            const float btnWidth = 30.0f;
            
            var leafWidth = target.forceAssetsFolder ? position.width - rootWidth : position.width - rootWidth - btnWidth - 10;
            var rootRect = new Rect(position.x, position.y, rootWidth, position.height);
            var leafRect = new Rect(position.x + rootWidth + 5, position.y, leafWidth, position.height);
            var btnRect = new Rect(position.x + rootWidth  + leafWidth + 10, position.y, btnWidth, position.height);

            if (target.forceAssetsFolder)
            {
                var root = (OutputPath.Root) m_RootProperty.intValue;
                GUI.Label(rootRect, root + " " + Path.DirectorySeparatorChar);
            }
            else
            {
                EditorGUI.PropertyField(rootRect, m_RootProperty, GUIContent.none);
            }

            EditorGUI.PropertyField(leafRect, m_LeafProperty, GUIContent.none);

            var fullPath = OutputPath.GetFullPath((OutputPath.Root)m_RootProperty.intValue, m_LeafProperty.stringValue);

            if (!target.forceAssetsFolder)
            {
                if (GUI.Button(btnRect, new GUIContent("...", fullPath)))
                {
                    var newPath = EditorUtility.OpenFolderPanel("Select output location", fullPath, "");
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        var newValue = OutputPath.FromPath(newPath);
                        m_RootProperty.intValue = (int) newValue.root;
                        m_LeafProperty.stringValue = newValue.leaf;
                    }
                }
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}
