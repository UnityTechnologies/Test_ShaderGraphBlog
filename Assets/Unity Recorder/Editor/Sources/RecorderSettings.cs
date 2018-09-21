using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Recorder
{
    /// <summary>
    /// A list of possible image source that can be used by some image specific recorders  
    /// </summary>
    [Flags]
    public enum ImageSource
    {
        ActiveCamera = 1,
        MainCamera = 2,
        TaggedCamera = 4
    }

    /// <summary>
    /// Type of frame rate the recorder is using.
    /// </summary>
    public enum FrameRatePlayback
    {
        /// <summary>
        /// Will not vary during the recording, even if the actual frame rate is lower or higher
        /// </summary>
        Constant,
        
        /// <summary>
        /// May vary during the recording. Not supported by all recorders
        /// </summary>
        Variable,
    }

    /// <summary>
    /// Used to specify which time or frame interval to record.
    /// </summary>
    enum RecordMode
    {
        /// <summary>
        /// Records every frame between the moment the recording is started and stopped (either by pressing the button, or when calling the API methods)
        /// </summary>
        Manual,
        
        /// <summary>
        /// Records one single frame specified by it's number
        /// </summary>
        SingleFrame,
        
        /// <summary>
        /// Records all the frame between a Start Frame number and a Stop Frame number
        /// </summary>
        FrameInterval,
        
        /// <summary>
        /// Records all the frame between a Start Time and a Stop Time.
        /// </summary>
        TimeInterval
    }

    /// <summary>
    /// Main base class for a Recorder settings.
    /// Each recorder needs to have its corresponding settings properly configured.
    /// </summary>
    public abstract class RecorderSettings : ScriptableObject
    {
        /// <summary>
        /// The output path this recorder will use to generate it's output file.
        /// File extension is automatically added.
        /// It can be either an absolute or relative path.
        /// Wildcards like <c>DefaultWildcard.Time</c> are supported.
        /// <seealso cref="DefaultWildcard"/>
        /// </summary>
        public string outputFile
        {
            get { return fileNameGenerator.ToPath(); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Recorder output file must be a valid");
                
                fileNameGenerator.FromPath(value);
            }
        }
        
        /// <summary>
        /// Used only when starting the recording. If false, this recorder will be ignored.
        /// </summary>
        public bool enabled = true;
        
        /// <summary>
        /// Current Take number. Automatically incremented after each recording session.
        /// </summary>
        public int take = 1;
        
        /// <summary>
        /// The extension used by this recorder without the dot.
        /// </summary>
        public abstract string extension { get; }
        
        [SerializeField] internal int captureEveryNthFrame = 1;
        
        [SerializeField] internal FileNameGenerator fileNameGenerator;

        internal RecordMode recordMode { get; set; }

        internal FrameRatePlayback frameRatePlayback { get; set; }

        internal float frameRate { get; set; }

        internal int startFrame { get; set; }

        internal int endFrame { get; set; }

        internal float startTime { get; set; }

        internal float endTime { get; set; }

        internal bool capFrameRate { get; set; }
        
        protected RecorderSettings()
        {
            fileNameGenerator = new FileNameGenerator(this)
            {
                root = OutputPath.Root.Project,
                leaf = "Recordings"
            };
        }

        internal virtual bool ValidityCheck(List<string> errors)
        {
            var ok = true;

            if (inputsSettings != null)
            {
                var inputErrors = new List<string>();

                var valid = inputsSettings.All(x => x.ValidityCheck(inputErrors));
                
                if (!valid)
                {
                    errors.AddRange(inputErrors);
                    ok = false;
                }
            }
            
            if (string.IsNullOrEmpty(fileNameGenerator.fileName))
            {
                errors.Add("Missing file name");
                ok = false;
            }

            if (Math.Abs(frameRate) <= float.Epsilon)
            {
                ok = false;
                errors.Add("Invalid frame rate");
            }

            if (captureEveryNthFrame <= 0)
            {
                ok = false;
                errors.Add("Invalid frame skip value");
            }

            if (!isPlatformSupported)
            {
                errors.Add("Current platform is not supported");
                ok  = false;
            }

            return ok;
        }

        /// <summary>
        /// Virtual method that can be used to return false if current platform is not supported.
        /// </summary>
        public virtual bool isPlatformSupported
        {
            get { return true; }
        }

        /// <summary>
        /// List of Input settings required by this recorder.
        /// </summary>
        public abstract IEnumerable<RecorderInputSettings> inputsSettings { get; }

        /// <summary>
        /// Called each time a settings has changed in the RecorderWindow and before starting recording.
        /// </summary>
        public virtual void SelfAdjustSettings()
        {
        }

        /// <summary>
        /// Override this method if any post treatement need to be done after a this recorder is duplicated in the RecorderWindow
        /// </summary>
        public virtual void OnAfterDuplicate()
        {
        }

        internal virtual bool HasErrors()
        {
            return false;
        }

        internal virtual bool HasWarnings()
        {
            return !ValidityCheck(new List<string>());
        }
    }
}
