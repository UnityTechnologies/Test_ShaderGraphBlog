using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Recorder
{
    class ImageHeightSelector
    {
        string[] m_ToName;
        int[] m_ToEnumValue;
        readonly Dictionary<ImageHeight, int> m_ToIndex = new Dictionary<ImageHeight, int>();

        int m_Max;
        readonly bool m_AllowCustom;
        readonly bool m_AllowWindow;
        
        static readonly Dictionary<ImageHeight, string> s_HeightToName = new Dictionary<ImageHeight, string>
        {
            { ImageHeight.x4320p_8K, "8K - 4320p" },
            { ImageHeight.x2880p_5K, "5K - 2880p" },
            { ImageHeight.x2160p_4K, "4K - 2160p" },
            { ImageHeight.x1440p_QHD, "QHD - 1440p" },
            { ImageHeight.x1080p_FHD, "FHD - 1080p" },
            { ImageHeight.x720p_HD, "HD - 720p" },
            { ImageHeight.x480p, "SD - 480p" },
            { ImageHeight.x240p, "240p" },
            { ImageHeight.Window, "Match Window Size" },
            { ImageHeight.Custom, "Custom" }
        };
        
        public ImageHeightSelector(int max, bool allowCustom = true, bool allowWindow = true)
        {
            m_AllowCustom = allowCustom;
            m_AllowWindow = allowWindow;
            BuildPopup(max);
        }
        
        void BuildPopup(int max)
        {
            var values = (ImageHeight[]) Enum.GetValues(typeof(ImageHeight));

            var ordered = new List<ImageHeight>();
            
            if (m_AllowWindow)
                ordered.Add(ImageHeight.Window);

            ordered.AddRange(values.Where(value => value != ImageHeight.Window && value != ImageHeight.Custom && (int)value <= max));

            if (m_AllowCustom)
                ordered.Add(ImageHeight.Custom);

            var count = ordered.Count;
            m_ToName = new string[count];
            m_ToEnumValue = new int[count];
            
            for (int i = 0; i < count; ++i)
            {
                var e = ordered[i];
                m_ToName[i] = s_HeightToName[e];
                m_ToEnumValue[i] = (int)e;
                m_ToIndex[e] = i;
            }

            m_Max = max;
        }

        public int Popup(GUIContent label, int value, int max)
        {   
            if (m_Max != max)
                BuildPopup(max);
            
            int index;
            
            if (!m_ToIndex.TryGetValue((ImageHeight) value, out index))
                index = 0;
            
            index = EditorGUILayout.Popup(label, index, m_ToName);

            return m_ToEnumValue[index];
        }
    }
}