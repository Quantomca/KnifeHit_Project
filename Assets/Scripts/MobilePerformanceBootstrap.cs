using UnityEngine;

public static class MobilePerformanceBootstrap
{
    const string LowQualityName = "Low";
    const int MobileTargetFrameRate = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyMobileDefaults()
    {
#if UNITY_ANDROID || UNITY_IOS
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = MobileTargetFrameRate;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        int lowQualityIndex = FindQualityLevel(LowQualityName);
        if (lowQualityIndex >= 0 && QualitySettings.GetQualityLevel() > lowQualityIndex)
            QualitySettings.SetQualityLevel(lowQualityIndex, true);
#endif
    }

    static int FindQualityLevel(string qualityName)
    {
        string[] qualityNames = QualitySettings.names;
        for (int index = 0; index < qualityNames.Length; index++)
        {
            if (qualityNames[index] == qualityName)
                return index;
        }

        return -1;
    }
}
