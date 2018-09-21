using System;
using UnityEngine;

namespace UnityEditor.Recorder
{
    class TextureFlipper : IDisposable
    {
        RenderTexture m_WorkTexture;

        internal void Flip(RenderTexture target)
        {
            if (m_WorkTexture == null || m_WorkTexture.width != target.width || m_WorkTexture.height != target.height)
            {
                UnityHelpers.Destroy(m_WorkTexture);
                m_WorkTexture = new RenderTexture(target);
            }

            var sRGBWrite = GL.sRGBWrite;
            GL.sRGBWrite = PlayerSettings.colorSpace == ColorSpace.Linear;
            
            Graphics.Blit(target, m_WorkTexture, new Vector2(1.0f, -1.0f), new Vector2(0.0f, 1.0f));
            Graphics.Blit(m_WorkTexture, target);
            
            

            GL.sRGBWrite = sRGBWrite;
        }

        public void Dispose()
        {
            UnityHelpers.Destroy(m_WorkTexture);
            m_WorkTexture = null;
        }

    }
}
