using System;
using UnityEngine.Timeline;

namespace UnityEditor.Recorder.Timeline
{
    [Serializable]
    [TrackClipType(typeof(RecorderClip))]
    [TrackColor(0f, 0.53f, 0.08f)]
    public class RecorderTrack : TrackAsset
    {
    }
}
