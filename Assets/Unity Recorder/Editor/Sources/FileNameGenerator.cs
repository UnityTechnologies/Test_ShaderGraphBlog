using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.Recorder
{
    class Wildcard
    {
        readonly string m_Pattern;
        readonly string m_Label;
        readonly Func<RecordingSession, string> m_Resolver;

        public string pattern { get { return m_Pattern; } }
        public string label { get { return m_Label; } }

        internal Wildcard(string pattern, Func<RecordingSession, string> resolver, string info = null)
        {
            m_Pattern = pattern;
            m_Label = m_Pattern;
                
            if (info != null)
                m_Label += " " + info;
                
            m_Resolver = resolver;
        }

        internal string Resolve(RecordingSession session)
        {
            return m_Resolver == null ? string.Empty : m_Resolver(session);
        }
    }
    
    /// <summary>
    /// Helper class for default wildcards that can be used when constructing the output file of a recorder.
    /// <see cref="RecorderSettings.outputFile"/>
    /// </summary>
    public static class DefaultWildcard
    {
        public static readonly string Recorder = GeneratePattern("Recorder");
        public static readonly string Time = GeneratePattern("Time");
        public static readonly string Take = GeneratePattern("Take");
        public static readonly string Date = GeneratePattern("Date");
        public static readonly string Project = GeneratePattern("Project");
        public static readonly string Product = GeneratePattern("Product");
        public static readonly string Scene = GeneratePattern("Scene");
        public static readonly string Resolution = GeneratePattern("Resolution");
        public static readonly string Frame = GeneratePattern("Frame");
        public static readonly string Extension = GeneratePattern("Extension");
        
        public static string GeneratePattern(string tag)
        {
            return "<" + tag + ">";
        }
    }
    
    [Serializable]
    class FileNameGenerator
    {
        static string s_ProjectName;

        [SerializeField] OutputPath m_Path = new OutputPath();
        [SerializeField] string m_FileName = DefaultWildcard.Recorder;
        
        readonly List<Wildcard> m_Wildcards;
        
        public IEnumerable<Wildcard> wildcards
        {
            get { return m_Wildcards; }
        }

        internal void FromPath(string str)
        {
            str = SanitizePath(str);

            var i = str.LastIndexOf('/');
            if (i != -1 && i < str.Length - 1)
            {
                m_FileName = str.Substring(i + 1);

                if (i == 0)
                {
                    m_Path.root = OutputPath.Root.Absolute;
                    m_Path.leaf = "/";
                }
                else
                {
                    str = str.Substring(0, i);
                    m_Path = OutputPath.FromPath(str);
                }
            }
            else
            {
                m_FileName = str;
                m_Path.root = OutputPath.Root.Absolute;
                m_Path.leaf = string.Empty;
            }
        }
        
        internal string ToPath()
        {            
            var path = m_Path.GetFullPath();
            
            if (!string.IsNullOrEmpty(path))
                path += "/";
            
            return SanitizePath(path + SanitizeFilename(m_FileName));
        }
        
        internal string fileName {
            get { return m_FileName; }
            set { m_FileName = value; }
        }

        internal OutputPath.Root root
        {
            get { return m_Path.root; }
            set { m_Path.root = value; }
        }
        
        internal string leaf
        {
            get { return m_Path.leaf; }
            set { m_Path.leaf = value; }
        }

        internal bool forceAssetsFolder
        {
            get { return m_Path.forceAssetsFolder; }
            set { m_Path.forceAssetsFolder = value; }
        }

        readonly RecorderSettings m_RecorderSettings;
        
        internal FileNameGenerator(RecorderSettings recorderSettings)
        {
            m_RecorderSettings = recorderSettings;
            
            m_Wildcards = new List<Wildcard>
            {
                new Wildcard(DefaultWildcard.Recorder, RecorderResolver),
                new Wildcard(DefaultWildcard.Time, TimeResolver),
                new Wildcard(DefaultWildcard.Take, TakeResolver),
                new Wildcard(DefaultWildcard.Date, DateResolver),
                new Wildcard(DefaultWildcard.Project, ProjectNameResolver),
                new Wildcard(DefaultWildcard.Product, ProductNameResolver),
                new Wildcard(DefaultWildcard.Scene, SceneResolver),
                new Wildcard(DefaultWildcard.Resolution, ResolutionResolver),
                new Wildcard(DefaultWildcard.Frame, FrameResolver),
                new Wildcard(DefaultWildcard.Extension, ExtensionResolver)
            };
        }

        internal void AddWildcard(string tag, Func<RecordingSession, string> resolver)
        {
            m_Wildcards.Add(new Wildcard(tag, resolver));
        }

        string RecorderResolver(RecordingSession session)
        {
            return m_RecorderSettings.name;
        }
        
        static string TimeResolver(RecordingSession session)
        {
            var date = session != null ? session.sessionStartTS : DateTime.Now;
            return string.Format("{0:HH}h{1:mm}m", date, date);
        }
        
        string TakeResolver(RecordingSession session)
        {
            return m_RecorderSettings.take.ToString("000");
        }

        static string DateResolver(RecordingSession session)
        {
            var date = session != null ? session.sessionStartTS : DateTime.Now;
            return date.ToString(CultureInfo.InvariantCulture).Replace('/', '-');
        }

        string ExtensionResolver(RecordingSession session)
        {
            return m_RecorderSettings.extension;
        }

        string ResolutionResolver(RecordingSession session)
        {
            var input = m_RecorderSettings.inputsSettings.FirstOrDefault() as ImageInputSettings;
            if (input == null)
                return "NA";
            
            return input.outputWidth + "x" + input.outputHeight;
        }

        static string SceneResolver(RecordingSession session)
        {
            return SceneManager.GetActiveScene().name;
        }

        static string FrameResolver(RecordingSession session)
        {
            var i = session != null ? session.frameIndex : 0;
            return i.ToString("0000");
        }

        static string ProjectNameResolver(RecordingSession session)
        {
            if (string.IsNullOrEmpty(s_ProjectName))
            {
                var parts = Application.dataPath.Split('/');
                s_ProjectName = parts[parts.Length - 2];
            }
            
            return s_ProjectName;
        }

        static string ProductNameResolver(RecordingSession session)
        {
            return PlayerSettings.productName;
        }

        internal string BuildAbsolutePath(RecordingSession session)
        {
            var fullPath = ApplyWildcards(ToPath(), session) + "." + ExtensionResolver(session);            
            
            string drive = null;
            
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (fullPath.Length > 2 && char.IsLetter(fullPath[0]) && fullPath[1] == ':' && fullPath[2] == '/')
                {
                    drive = fullPath.Substring(0, 2);
                    fullPath = fullPath.Substring(3);
                }
            }

            fullPath = string.Join(Path.DirectorySeparatorChar.ToString(), fullPath.Split('/').Select(s =>
                Path.GetInvalidFileNameChars().Aggregate(s, (current, c) => current.Replace(c.ToString(), string.Empty))).ToArray());

            if (!string.IsNullOrEmpty(drive))
                fullPath = drive.ToUpper() + Path.DirectorySeparatorChar + fullPath;

            return fullPath;
        }

        internal void CreateDirectory(RecordingSession session)
        {
            var path = ApplyWildcards(m_Path.GetFullPath(), session);
            if(!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        internal static string SanitizeFilename(string filename)
        {
            filename = filename.Replace("\\", "");
            filename = Regex.Replace(filename, "/", "");
            return filename;
        }

        internal static string SanitizePath(string fullPath)
        {
            fullPath = fullPath.Replace("\\", "/");
            fullPath = Regex.Replace(fullPath, "/+", "/");
            return fullPath;
        }

        string ApplyWildcards(string str, RecordingSession session)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            
            foreach (var w in wildcards)
                str = str.Replace(w.pattern, w.Resolve(session));
            
            return str;
        }

    }
}
