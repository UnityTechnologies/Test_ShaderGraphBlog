
namespace UnityEditor.Recorder
{
    enum FrameRateType
    {
        FR_23, // 24 * 1000 / 1001
        FR_24,
        FR_25,
        FR_29, // 30 * 1000 / 1001,
        FR_30,
        FR_50,
        FR_59, // 60 * 1000 / 1001,
        FR_60,
        FR_CUSTOM,
    }
    
    enum ImageAspect
    {
        x19_10,
        x16_9,
        x16_10,
        x3_2,
        x4_3,
        x5_4,
        x1_1,
        Custom
    }
    
    enum ImageHeight
    {
        Window = 0,
        x4320p_8K = 4320,
        x2880p_5K = 2880,
        x2160p_4K = 2160,
        x1440p_QHD = 1440,
        x1080p_FHD = 1080,
        x720p_HD = 720,
        x480p = 480,
        x240p = 240,
        Custom = int.MaxValue
    }
}
