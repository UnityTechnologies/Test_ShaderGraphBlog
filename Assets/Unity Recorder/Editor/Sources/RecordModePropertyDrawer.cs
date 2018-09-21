namespace UnityEditor.Recorder
{
    [CustomPropertyDrawer(typeof(RecordMode))]
    class RecordModePropertyDrawer : EnumProperyDrawer<RecordMode>
    {
        protected override string ToLabel(RecordMode value)
        {
            switch (value)
            {
                case RecordMode.Manual:
                    return "Manual";
                
                case RecordMode.SingleFrame:
                    return "Single Frame";
                
                case RecordMode.FrameInterval:
                    return "Frame Interval";
                
                case RecordMode.TimeInterval:
                    return "Time Interval (sec)";
                
                default:
                    return "unknown";
            }
        }
    }
}