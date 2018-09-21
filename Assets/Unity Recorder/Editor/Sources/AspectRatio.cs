using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [Serializable]
    class AspectRatio
    {
        [SerializeField] float m_CustomAspectX = 1.0f;
        [SerializeField] float m_CustomAspectY = 1.0f;

        [SerializeField] ImageAspect m_ImageAspect = ImageAspect.x16_9;

        internal static readonly Dictionary<ImageAspect, float> s_AspectToValue = new Dictionary<ImageAspect, float>
        {
            { ImageAspect.x16_9, 16.0f / 9.0f },
            { ImageAspect.x16_10, 16.0f / 10.0f },
            { ImageAspect.x19_10, 19.0f / 10.0f },
            { ImageAspect.x5_4, 5.0f / 4.0f },
            { ImageAspect.x4_3, 4.0f / 3.0f },
            { ImageAspect.x3_2, 3.0f / 2.0f },
            { ImageAspect.x1_1, 1.0f }
        };

        public float GetAspect()
        {
            return m_ImageAspect == ImageAspect.Custom ? m_CustomAspectX / m_CustomAspectY : s_AspectToValue[m_ImageAspect];
        }
    }
}