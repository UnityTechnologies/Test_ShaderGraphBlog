using System;
using System.Collections;
using UnityEngine;

namespace UnityEditor.Recorder.Timeline
{
    [ExecuteInEditMode]
    class WaitForEndOfFrameComponent : MonoBehaviour
    {
        [NonSerialized]
        public RecorderPlayableBehaviour m_playable;

        public IEnumerator WaitForEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            if(m_playable != null)
                m_playable.FrameEnded();
        }

        public void LateUpdate()
        {
            StartCoroutine(WaitForEndOfFrame());
        }
    }
}
