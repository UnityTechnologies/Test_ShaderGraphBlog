using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Recorder.Timeline
{
    class RecorderSelector
    {
        string[] m_RecorderNames;
        List<Type> m_RecorderTypes;
        
        bool m_SettingsAreAssets;

        Type m_SelectedRecorder;

        public event Action<Type> OnSelectionChanged;

        public void Init(RecorderSettings settings)
        {
            if (m_RecorderTypes == null)
            {
                var recorderList = RecordersInventory.builtInRecorderInfos.ToList();
                
                if (Options.showLegacyRecorders)
                    recorderList.AddRange(RecordersInventory.legacyRecorderInfos);
                
                recorderList.AddRange(RecordersInventory.customRecorderInfos);
                
                m_RecorderTypes = recorderList.Select(x => x.settingsType).ToList();
                m_RecorderNames = recorderList.Select(x => x.displayName).ToArray();
            }

            SelectRecorder(settings != null ? settings.GetType() : m_RecorderTypes.First());
        }

        int GetRecorderIndex(Type settingType)
        {
            return m_RecorderTypes.IndexOf(settingType);
        }
        
        Type GetRecorderFromIndex(int index)
        {
            return m_RecorderTypes.ElementAt(index);
        }

        public void OnGui()
        {
            // Recorder in group selection
            EditorGUILayout.BeginHorizontal();
            var oldIndex = GetRecorderIndex(m_SelectedRecorder);
            var newIndex = EditorGUILayout.Popup("Selected recorder:", oldIndex, m_RecorderNames);
            SelectRecorder(GetRecorderFromIndex(newIndex));

            EditorGUILayout.EndHorizontal();
        }

        void SelectRecorder(Type newSelection)
        {
            if (m_SelectedRecorder == newSelection)
                return;

            var recorderAttribs = newSelection.GetCustomAttributes(typeof(ObsoleteAttribute), false);
            if (recorderAttribs.Length > 0 )
                Debug.LogWarning( "Recorder " + ((ObsoleteAttribute)recorderAttribs[0]).Message);

            m_SelectedRecorder = newSelection;
            
            if (OnSelectionChanged != null)
                OnSelectionChanged.Invoke(m_SelectedRecorder);
        }
    }
}