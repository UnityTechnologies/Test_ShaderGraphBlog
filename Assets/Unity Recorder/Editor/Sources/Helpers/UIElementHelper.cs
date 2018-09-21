using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Recorder
{
    static class UIElementHelper
    {
        internal static void SetFlex(VisualElement element, float value)
        {
            #if UNITY_2018_3_OR_NEWER
                element.style.flex = new Flex(value);
            #else
                element.style.flex = value;
            #endif
        }

        internal static bool GetToggleValue(Toggle toggle)
        {
            #if UNITY_2018_2_OR_NEWER
                return toggle.value;
            #else
                return toggle.on;
            #endif
        }

        internal static void SetToggleValue(Toggle toggle, bool value)
        {
            #if UNITY_2018_2_OR_NEWER
                toggle.value = value;
            #else
                toggle.on = value;
            #endif
        }
        
        internal static bool MultiIntField(GUIContent label, GUIContent[] subLabels, int[] values)
        {
            var r = EditorGUILayout.GetControlRect();

            var rLabel = r;
            rLabel.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(rLabel, label);

            var rContent = r;
            rContent.xMin = rLabel.xMax;
            
            var width = subLabels.Select(l => GUI.skin.label.CalcSize(l).x).Max();
            
            EditorGUI.BeginChangeCheck();
            MultiIntField(rContent, subLabels, values, width);
            return EditorGUI.EndChangeCheck();
        }
        
        internal static bool MultiFloatField(GUIContent label, GUIContent[] subLabels, float[] values)
        {
            var r = EditorGUILayout.GetControlRect();

            var rLabel = r;
            rLabel.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(rLabel, label);

            var rContent = r;
            rContent.xMin = rLabel.xMax;
            
            var width = subLabels.Select(l => GUI.skin.label.CalcSize(l).x).Max();
            
            EditorGUI.BeginChangeCheck();
            MultiFloatField(rContent, subLabels, values, width);
            return EditorGUI.EndChangeCheck();
        }
        
        static void MultiIntField(Rect position, IList<GUIContent> subLabels, IList<int> values, float labelWidth)
        {
            var length = values.Count;
            var num = (position.width - (float) (length - 1) * 2f) / (float) length;
            var position1 = new Rect(position)
            {
                width = num
            };
            var labelWidth1 = EditorGUIUtility.labelWidth;
            var indentLevel = EditorGUI.indentLevel;
            
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = 0;
            for (int index = 0; index < values.Count; ++index)
            {
                values[index] = EditorGUI.IntField(position1, subLabels[index], values[index]);
                position1.x += num + 2f;
            }
            EditorGUIUtility.labelWidth = labelWidth1;
            EditorGUI.indentLevel = indentLevel;
        }
        
        static void MultiFloatField(Rect position, IList<GUIContent> subLabels, IList<float> values, float labelWidth)
        {
            var length = values.Count;
            var num = (position.width - (float) (length - 1) * 2f) / (float) length;
            var position1 = new Rect(position)
            {
                width = num
            };
            var labelWidth1 = EditorGUIUtility.labelWidth;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = 0;
            for (int index = 0; index < values.Count; ++index)
            {
                values[index] = EditorGUI.FloatField(position1, subLabels[index], values[index]);
                position1.x += num + 2f;
            }
            EditorGUIUtility.labelWidth = labelWidth1;
            EditorGUI.indentLevel = indentLevel;
        }
    }
}