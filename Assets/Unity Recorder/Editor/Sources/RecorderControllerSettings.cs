using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder
{
    public class RecorderControllerSettings : ScriptableObject
    {
        [SerializeField] RecordMode m_RecordMode = RecordMode.Manual;
        [SerializeField] FrameRatePlayback m_FrameRatePlayback = FrameRatePlayback.Constant;
        [SerializeField] FrameRateType m_FrameRateType = FrameRateType.FR_30;
        [SerializeField] [Range(1.0f, 120.0f)] float m_CustomFrameRateValue = 30.0f;
        
        [SerializeField] int m_StartFrame;
        [SerializeField] int m_EndFrame;
        
        [SerializeField] float m_StartTime;
        [SerializeField] float m_EndTime;
        
        [SerializeField] bool m_CapFrameRate = true;

        static readonly Dictionary<FrameRateType, float> s_FPSToValue = new Dictionary<FrameRateType, float>()
        {
            { FrameRateType.FR_23, 24 * 1000 / 1001f },
            { FrameRateType.FR_24, 24 },
            { FrameRateType.FR_25, 25 },
            { FrameRateType.FR_29, 30 * 1000 / 1001f },
            { FrameRateType.FR_30, 30 },
            { FrameRateType.FR_50, 50 },
            { FrameRateType.FR_59, 60 * 1000 / 1001f },
            { FrameRateType.FR_60, 60 }
        };

        public FrameRatePlayback frameRatePlayback
        {
            get { return m_FrameRatePlayback; }
            set { m_FrameRatePlayback = value; }
        }

        public float frameRate
        {
            get
            {
                return m_FrameRateType == FrameRateType.FR_CUSTOM ? m_CustomFrameRateValue : s_FPSToValue[m_FrameRateType];
            }
            
            set
            {
                m_FrameRateType = FrameRateType.FR_CUSTOM;
                m_CustomFrameRateValue = value;
            }
        }

        public void SetRecordModeToManual()
        {
            m_RecordMode = RecordMode.Manual;
        }
        
        public void SetRecordModeToSingleFrame(int frameNumber)
        {
            m_RecordMode = RecordMode.SingleFrame;
            m_StartFrame = m_EndFrame = frameNumber;
        }
        
        public void SetRecordModeToFrameInterval(int startFrame, int endFrame)
        {
            m_RecordMode = RecordMode.FrameInterval;
            m_StartFrame = startFrame;
            m_EndFrame = endFrame;
        }
        
        public void SetRecordModeToTimeInterval(float startTime, float endTime)
        {
            m_RecordMode = RecordMode.TimeInterval;
            m_StartTime = startTime;
            m_EndTime = endTime;
        }

        public bool capFrameRate
        {
            get { return m_CapFrameRate; }
            set { m_CapFrameRate = value; }
        }
        
        [SerializeField] List<RecorderSettings> m_RecorderSettings = new List<RecorderSettings>();

        string m_Path;
        
        public static RecorderControllerSettings LoadOrCreate(string path)
        {
            RecorderControllerSettings prefs;
            try
            {
                var objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                prefs = objs.FirstOrDefault(p => p is RecorderControllerSettings) as RecorderControllerSettings;
            }
            catch (Exception e)
            {
                Debug.LogError("Unhandled exception while loading Recorder preferences: " + e);
                prefs = null;
            }

            if (prefs == null)
            {
                prefs = CreateInstance<RecorderControllerSettings>();
                prefs.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                prefs.name = "Global Settings";
                prefs.Save();
            }
            
            prefs.m_Path = path;
            
            return prefs;
        }

        internal void ReleaseRecorderSettings()
        {
            foreach (var recorder in m_RecorderSettings)
            {
                DestroyImmediate(recorder);
            }

            ClearRecorderSettings();
        }
        
        internal void ClearRecorderSettings()
        {           
            m_RecorderSettings.Clear();
        }
        
        public IEnumerable<RecorderSettings> recorderSettings
        {
            get { return m_RecorderSettings; }
        }
     
        public void AddRecorderSettings(RecorderSettings recorder)
        {
            if (!m_RecorderSettings.Contains(recorder))
            {
                AddRecorderInternal(recorder);
                Save();
            }
        }

        public void RemoveRecorder(RecorderSettings recorder)
        {
            if (m_RecorderSettings.Contains(recorder))
            {
                m_RecorderSettings.Remove(recorder);
                Save();
            }
        }
        
        public void Save()
        {
            if (string.IsNullOrEmpty(m_Path))
                return;
            
            try
            {
                var directory = Path.GetDirectoryName(m_Path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var recordersCopy = recorderSettings.ToArray();

                var objs = new UnityObject[recordersCopy.Length + 1];
                objs[0] = this;

                for (int i = 0; i < recordersCopy.Length; ++i)
                    objs[i + 1] = recordersCopy[i];

                InternalEditorUtility.SaveToSerializedFileAndForget(objs, m_Path, true);
            }
            catch (Exception e)
            {
                Debug.LogError("Unhandled exception while saving Recorder settings: " + e);
            }
        }

        internal void ApplyGlobalSetting(RecorderSettings recorder)
        {
            recorder.recordMode = m_RecordMode;
            recorder.frameRatePlayback = m_FrameRatePlayback;
            recorder.frameRate = frameRate;
            recorder.startFrame = m_StartFrame;
            recorder.endFrame = m_EndFrame;
            recorder.startTime = m_StartTime;
            recorder.endTime = m_EndTime;
            recorder.capFrameRate = m_CapFrameRate;
            recorder.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            
            recorder.SelfAdjustSettings();
        }
        
        internal void ApplyGlobalSettingToAllRecorders()
        {
            foreach (var recorder in recorderSettings)
                ApplyGlobalSetting(recorder);
        }

        void AddRecorderInternal(RecorderSettings recorder)
        {
            ApplyGlobalSetting(recorder);
            m_RecorderSettings.Add(recorder);
        }
    }
}
