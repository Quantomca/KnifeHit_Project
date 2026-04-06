using System;
using System.Collections;
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

    Coroutine refreshRoutine;

    void Reset()
    {
        ResolveReferences();
    }

    void Awake()
    {
        ResolveReferences();
        BindButton();
        RefreshUI();
    }

    void OnEnable()
    {
        ResolveReferences();
        BindButton();
        RestartRefreshLoop();
        RefreshUI();
    }

    void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);

        if (refreshRoutine != null)
        {
            StopCoroutine(refreshRoutine);
            refreshRoutine = null;
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            RefreshUI();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
            RefreshUI();
    }

    public void RefreshNow()
    {
        ResolveReferences();
        BindButton();
        RefreshUI();
    }

    void ResolveReferences()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (timeCounterText == null)
        {
            Transform timeCounterTransform = FindChildRecursive(transform, "TimeCounter");
            if (timeCounterTransform != null)
                timeCounterText = timeCounterTransform.GetComponent<TMP_Text>();
        }

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

    void RestartRefreshLoop()
    {
        if (refreshRoutine != null)
            StopCoroutine(refreshRoutine);

        refreshRoutine = StartCoroutine(RefreshLoop());
    }

    IEnumerator RefreshLoop()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.25f);

        while (enabled)
        {
            RefreshUI();
            yield return wait;
        }
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
        bool isReady = IsReady();

        if (isReady)
            ClearExpiredCooldown();

        if (button != null)
            button.interactable = isReady;

        if (timeCounterText == null)
            return;

        if (isReady)
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

    DateTime GetReadyAtUtc()
    {
        if (!PlayerPrefs.HasKey(ReadyAtKey))
            return DateTime.MinValue;

        string rawValue = PlayerPrefs.GetString(ReadyAtKey, string.Empty);
        rawValue = rawValue.Trim().TrimEnd('\0');

        if (TryParseReadyAtUtc(rawValue, out DateTime readyAtUtc))
            return readyAtUtc;

        ClearReadyAtUtc();
        return DateTime.MinValue;
    }

    void SetReadyAtUtc(DateTime utcTime)
    {
        PlayerPrefs.SetString(ReadyAtKey, utcTime.Ticks.ToString());
        PlayerPrefs.Save();
    }

    void ClearReadyAtUtc()
    {
        if (!PlayerPrefs.HasKey(ReadyAtKey))
            return;

        PlayerPrefs.DeleteKey(ReadyAtKey);
        PlayerPrefs.Save();
    }

    void ClearExpiredCooldown()
    {
        if (!PlayerPrefs.HasKey(ReadyAtKey))
            return;

        if (DateTime.UtcNow >= GetReadyAtUtc())
            ClearReadyAtUtc();
    }

    bool TryParseReadyAtUtc(string rawValue, out DateTime readyAtUtc)
    {
        readyAtUtc = default;

        if (!long.TryParse(rawValue, out long rawNumber))
            return false;

        try
        {
            if (rawValue.Length >= 17)
            {
                readyAtUtc = new DateTime(rawNumber, DateTimeKind.Utc);
                return !IsClearlyInvalidTimestamp(readyAtUtc);
            }

            readyAtUtc = DateTimeOffset.FromUnixTimeSeconds(rawNumber).UtcDateTime;
            return !IsClearlyInvalidTimestamp(readyAtUtc);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    bool IsClearlyInvalidTimestamp(DateTime readyAtUtc)
    {
        DateTime nowUtc = DateTime.UtcNow;
        return readyAtUtc < nowUtc.AddYears(-1) || readyAtUtc > nowUtc.AddYears(10);
    }

    Transform FindChildRecursive(Transform root, string objectName)
    {
        if (root == null)
            return null;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == objectName)
                return child;

            Transform nested = FindChildRecursive(child, objectName);
            if (nested != null)
                return nested;
        }

        return null;
    }
}

public static class GiftButtonBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        InstallGiftButtonsInScene(SceneManager.GetActiveScene());
    }

    static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InstallGiftButtonsInScene(scene);
    }

    static void InstallGiftButtonsInScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        GameObject[] rootObjects = scene.GetRootGameObjects();
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
