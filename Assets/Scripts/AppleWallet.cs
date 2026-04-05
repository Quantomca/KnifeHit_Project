using System;
using UnityEngine;

public static class AppleWallet
{
    const string AppleCountKey = "APPLE_CURRENCY";

    static bool hasLoaded;
    static int cachedAppleCount;

    public static event Action<int> AppleCountChanged;

    public static int GetAppleCount()
    {
        EnsureLoaded();
        return cachedAppleCount;
    }

    public static void AddApples(int amount)
    {
        if (amount <= 0)
            return;

        SetAppleCount(GetAppleCount() + amount);
    }

    public static bool TrySpendApples(int amount)
    {
        if (amount <= 0)
            return true;

        int currentAppleCount = GetAppleCount();
        if (currentAppleCount < amount)
            return false;

        SetAppleCount(currentAppleCount - amount);
        return true;
    }

    static void EnsureLoaded()
    {
        if (hasLoaded)
            return;

        cachedAppleCount = Mathf.Max(0, PlayerPrefs.GetInt(AppleCountKey, 0));
        hasLoaded = true;
    }

    static void SetAppleCount(int amount)
    {
        EnsureLoaded();

        int nextAmount = Mathf.Max(0, amount);
        if (cachedAppleCount == nextAmount)
            return;

        cachedAppleCount = nextAmount;
        PlayerPrefs.SetInt(AppleCountKey, cachedAppleCount);
        PlayerPrefs.Save();
        AppleCountChanged?.Invoke(cachedAppleCount);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState()
    {
        hasLoaded = false;
        cachedAppleCount = 0;
        AppleCountChanged = null;
    }
}
