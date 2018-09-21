using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityEditor.Recorder
{    
    [Serializable]
    class OutputPath
    {
        public enum Root
        {
            Project,
            AssetsFolder,
            StreamingAssets,
            PersistentData,
            TemporaryCache,
            Absolute
        }

        [SerializeField] Root m_Root;
        [SerializeField] string m_Leaf;
        
        [SerializeField] bool m_ForceAssetFolder;

        public Root root
        {
            get { return m_Root; }
            set { m_Root = value; }
        }
        
        public string leaf
        {
            get { return m_Leaf; }
            set { m_Leaf = value; }
        }

        public bool forceAssetsFolder
        {
            get { return m_ForceAssetFolder;}
            set
            {
                m_ForceAssetFolder = value;
                
                if (m_ForceAssetFolder)
                    m_Root = Root.AssetsFolder;
            }
        }

        public static OutputPath FromPath(string path)
        {
            var result = new OutputPath();
            
            if (path.Contains(Application.streamingAssetsPath))
            {
                result.m_Root = Root.StreamingAssets;
                result.m_Leaf = path.Replace(Application.streamingAssetsPath, string.Empty);
            }
            else if (path.Contains(Application.dataPath))
            {
                result.m_Root = Root.AssetsFolder;
                result.m_Leaf = path.Replace(Application.dataPath, string.Empty);
            }
            else if (path.Contains(Application.persistentDataPath))
            {
                result.m_Root = Root.PersistentData;
                result.m_Leaf = path.Replace(Application.persistentDataPath, string.Empty);
            }
            else if (path.Contains(Application.temporaryCachePath))
            {
                result.m_Root = Root.TemporaryCache;
                result.m_Leaf = path.Replace(Application.temporaryCachePath, string.Empty);
            }
            else if (path.Contains(ProjectPath()))
            {
                result.m_Root = Root.Project;
                result.m_Leaf = path.Replace(ProjectPath(), string.Empty);
            }
            else
            {
                result.m_Root = Root.Absolute;
                result.m_Leaf = path;
            }

            return result;
        }

        public static string GetFullPath(Root root, string leaf)
        {          
            var ret = string.Empty;
            switch (root)
            {
                case Root.PersistentData:
                    ret = Application.persistentDataPath;
                    break;
                case Root.StreamingAssets:
                    ret = Application.streamingAssetsPath;
                    break;
                case Root.TemporaryCache:
                    ret = Application.temporaryCachePath;
                    break;
                case Root.AssetsFolder:
                    ret = Application.dataPath;
                    break;
                case Root.Project:
                    ret = ProjectPath();
                    break;
            }

            if (root != Root.Absolute && !leaf.StartsWith("/"))
            {
                ret += "/";
            }
            ret += leaf;
            return ret;            
        }

        public string GetFullPath()
        {
            return GetFullPath(m_Root, m_Leaf);
        }

        static string ProjectPath()
        {
            return Regex.Replace(Application.dataPath, "/Assets$", string.Empty);
        }
    }
}