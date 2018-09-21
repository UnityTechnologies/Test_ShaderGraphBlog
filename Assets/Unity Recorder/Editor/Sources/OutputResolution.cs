using System;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [Serializable]
    class OutputResolution
    {
        [SerializeField] int m_CustomWidth = 1024;
        [SerializeField] int m_CustomHeight = 1024;

        [SerializeField] internal ImageHeight imageHeight = ImageHeight.x720p_HD;
        [SerializeField] internal ImageHeight maxSupportedHeight = ImageHeight.x4320p_8K;
        
        [SerializeField] AspectRatio m_AspectRatio = new AspectRatio();
        
        public int GetWidth()
        {
            if (imageHeight == ImageHeight.Custom)
                return m_CustomWidth;
            
            if (imageHeight == ImageHeight.Window)
            {
                int w, h;
                GameViewSize.GetGameRenderSize(out w, out h);
                return w;
            }
            
            var aspect = m_AspectRatio.GetAspect();
            return (int) (aspect * (int)imageHeight);
        }
        
        public int GetHeight()
        {
            if (imageHeight == ImageHeight.Custom)
                return m_CustomHeight;

            if (imageHeight == ImageHeight.Window)
            {
                int w, h;
                GameViewSize.GetGameRenderSize(out w, out h);
                return h;
            }

            return (int)imageHeight;
        }
        
        public void SetWidth(int w)
        {
            imageHeight = ImageHeight.Custom;
            m_CustomWidth = w;
        }
        
        public void SetHeight(int h)
        {
            imageHeight = ImageHeight.Custom;
            m_CustomHeight = h;
        }
    }
}