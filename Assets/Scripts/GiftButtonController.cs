using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GiftButtonController : MonoBehaviour
{
    const string ReadyAtKey = "GIFT_REWARD_READY_AT_UTC";
    const int CountdownSeconds = 10;
    const int RewardApples = 50;

    [SerializeField] Button button;
    [SerializeField] TMP_Text timeCounterText;

    void Reset()
    {
        ResolveReferences();
    }

    void Awake()
    {
        ResolveReferences();
        EnsureReadyTimestampInitialized();
        BindButton();
        RefreshUI();
    }

    void OnEnable()
    {
        ResolveReferences();
        BindButton();
        RefreshUI();
    }

    void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }

    void Update()
    {
        RefreshUI();
    }

    void ResolveReferences()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (timeCounterText == null)
            timeCounterText = GetComponentInChildren<TMP_Text>(true);
    }

    void BindButton()
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(HandleClick);
        button.onClick.AddListener(HandleClick);
    }

    void HandleClick()
    {
        if (!IsReady())
            return;

        AppleWallet.AddApples(RewardApples);
        SetReadyAtUtc(DateTime.UtcNow.AddSeconds(CountdownSeconds));
        RefreshUI();
    }

    void RefreshUI()
    {
        if (timeCounterText == null)
            return;

        if (IsReady())
        {
            timeCounterText.text = "READY!";
            return;
        }

        TimeSpan remaining = GetRemainingTime();
        int totalSeconds = Mathf.Max(0, Mathf.CeilToInt((float)remaining.TotalSeconds));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timeCounterText.text = $"{minutes:00}:{seconds:00}";
    }

    bool IsReady()
    {
        return DateTime.UtcNow >= GetReadyAtUtc();
    }

    TimeSpan GetRemainingTime()
    {
        DateTime readyAtUtc = GetReadyAtUtc();
        if (DateTime.UtcNow >= readyAtUtc)
            return TimeSpan.Zero;

        return readyAtUtc - DateTime.UtcNow;
    }

    void EnsureReadyTimestampInitialized()
    {
        if (PlayerPrefs.HasKey(ReadyAtKey))
            return;

        SetReadyAtUtc(DateTime.UtcNow.AddSeconds(CountdownSeconds));
    }

    DateTime GetReadyAtUtc()
    {
        EnsureReadyTimestampInitialized();
        string rawValue = PlayerPrefs.GetString(ReadyAtKey, string.Empty);
        rawValue = rawValue.Trim().TrimEnd('\0');

        if (long.TryParse(rawValue, out long ticks))
            return new DateTime(ticks, DateTimeKind.Utc);

        DateTime fallback = DateTime.UtcNow.AddSeconds(CountdownSeconds);
        SetReadyAtUtc(fallback);
        return fallback;
    }

    void SetReadyAtUtc(DateTime utcTime)
    {
        PlayerPrefs.SetString(ReadyAtKey, utcTime.Ticks.ToString());
        PlayerPrefs.Save();
    }
}

public static class GiftButtonBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InstallGiftButtonsInCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid())
            return;

        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
            InstallGiftButtonsRecursive(rootObjects[i].transform);
    }

    static void InstallGiftButtonsRecursive(Transform root)
    {
        if (root == null)
            return;

        if (root.name == "ButtonGift" && root.GetComponent<Button>() != null && root.GetComponent<GiftButtonController>() == null)
            root.gameObject.AddComponent<GiftButtonController>();

        for (int i = 0; i < root.childCount; i++)
            InstallGiftButtonsRecursive(root.GetChild(i));
    }
}
