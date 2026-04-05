using UnityEngine;

public static class AppleConfigLoader
{
    const string ResourcePath = "AppleConfig";

    static AppleConfig cachedConfig;

    public static AppleConfig Config
    {
        get
        {
            if (cachedConfig == null)
                cachedConfig = Resources.Load<AppleConfig>(ResourcePath);

            return cachedConfig;
        }
    }

    public static Sprite AppleSprite => Config != null ? Config.appleSprite : null;
    public static float WorldScale => Config != null ? Config.worldScale : 0.55f;
    public static float SurfaceOffset => Config != null ? Config.surfaceOffset : 0.08f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState()
    {
        cachedConfig = null;
    }
}
