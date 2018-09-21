using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Recorder
{
    /// <summary>
    /// Main class to use the Recorder framework via script.
    /// Controls recording states like start and stop.
    /// </summary>
    public class RecorderController
    {
        readonly SceneHook m_SceneHook;
        
        List<RecordingSession> m_RecordingSessions;
        readonly RecorderControllerSettings m_Settings;
        
        /// <summary>
        /// Current settings used by this RecorderControler.
        /// </summary>
        public RecorderControllerSettings settings
        {
            get { return m_Settings; }
        }

        /// <summary>
        /// A RecorderController requires a RecorderControllerSettings.
        /// </summary>
        /// <param name="settings">The RecorderControllerSettings to be used by this RecorderController.</param>
        /// <see cref="RecorderControllerSettings"/>
        public RecorderController(RecorderControllerSettings settings)
        {          
            m_Settings = settings;   
            m_SceneHook = new SceneHook(Guid.NewGuid().ToString());
        }
        
        /// <summary>
        /// Start recording. Works only in Playmode. 
        /// </summary>
        /// <returns>false if an error occured. The console will usually contains logs about the errors.</returns>
        /// <exception cref="Exception">If not in Playmode.</exception>
        /// <exception cref="NullReferenceException">If settings is null.</exception>
        public bool StartRecording()
        {          
            if (!Application.isPlaying)
                throw new Exception("Start Recording can only be called in Playmode.");

            if (m_Settings == null)
                throw new NullReferenceException("Can start recording without prefs");
            
            if (IsRecording())
            {
                if (Options.verboseMode)
                    Debug.Log("Recording was already started.");
                
                return false;
            }

            if (Options.verboseMode)
                Debug.Log("Start Recording.");
            
            SceneHook.PrepareSessionRoot();
            
            m_RecordingSessions = new List<RecordingSession>();
            
            foreach (var recorderSetting in m_Settings.recorderSettings)
            {
                if (recorderSetting == null)
                {
                    if (Options.verboseMode)
                        Debug.Log("Ignoring unknown recorder.");

                    continue;
                }

                m_Settings.ApplyGlobalSetting(recorderSetting);

                if (recorderSetting.HasErrors())
                {
                    if (Options.verboseMode)
                        Debug.Log("Ignoring invalid recorder '" + recorderSetting.name + "'");

                    continue;
                }

                var errors = new List<string>();

                if (recorderSetting.ValidityCheck(errors))
                {
                    foreach (var error in errors)
                        Debug.LogWarning(recorderSetting.name + ": " + error);
                }

                if (errors.Count > 0)
                {
                    if (Options.verboseMode)
                        Debug.LogWarning("Recorder '" + recorderSetting.name +
                                         "' has warnings and may not record properly.");
                }

                if (!recorderSetting.enabled)
                {
                    if (Options.verboseMode)
                        Debug.Log("Ignoring disabled recorder '" + recorderSetting.name + "'");

                    continue;
                }

                var session = m_SceneHook.CreateRecorderSessionWithRecorderComponent(recorderSetting);

                m_RecordingSessions.Add(session);
            }
            
            var success = m_RecordingSessions.Any() && m_RecordingSessions.All(r => r.SessionCreated() && r.BeginRecording());

            return success;
        }

        /// <summary>
        ///  Use this method to know if all recorders are done recording.
        /// A recording stops:
        /// 1. The settings is set to a time (or frame) interval and the end time (or last frame) was reached.
        /// 2. Calling the StopRecording method.
        /// 3. Exiting Playmode.
        /// </summary>
        /// <returns>True if at least one recording is still active. false otherwise.</returns>
        /// <seealso cref="RecorderControllerSettings.SetRecordModeToSingleFrame"/>
        /// <seealso cref="RecorderControllerSettings.SetRecordModeToFrameInterval"/>
        /// <seealso cref="RecorderControllerSettings.SetRecordModeToTimeInterval"/>
        public bool IsRecording()
        {
            return m_RecordingSessions != null && m_RecordingSessions.Any(r => r.isRecording);
        }

        /// <summary>
        /// Stops all active recordings.
        /// Most recordings will create the recorded file once stopped.
        /// If the settings is using Manual recording mode. then the only way to stop recording is by calling this method or by exiting Playmode.
        /// </summary>
        /// <seealso cref="RecorderControllerSettings.SetRecordModeToManual"/>
        public void StopRecording()
        {           
            if (Options.verboseMode)
                Debug.Log("Stop Recording.");

            if (m_RecordingSessions != null)
            {
                foreach (var session in m_RecordingSessions)
                {
                    session.EndRecording();
                    
                    if (session.recorderComponent != null)
                        UnityEngine.Object.Destroy(session.recorderComponent);
                }

                m_RecordingSessions = null;
            }
        }
        
        internal IEnumerable<RecordingSession> GetRecordingSessions()
        {
            return m_SceneHook.GetRecordingSessions();
        }
    }
}