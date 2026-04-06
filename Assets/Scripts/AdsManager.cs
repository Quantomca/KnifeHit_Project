using System;
using Unity.Services.LevelPlay;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
#if UNITY_ANDROID
    const string AppKey = "85460dcd";
    const string RewardedAdUnitId = "76yy3nay3ceui2a3";
#elif UNITY_IOS
    const string AppKey = "8545d445";
    const string RewardedAdUnitId = "qwouvdrkuwivay5q";
#else
    const string AppKey = "";
    const string RewardedAdUnitId = "";
#endif

    bool isInitialized;
    bool isInitializing;
    bool showWhenReady;
    bool rewardGrantedForCurrentAd;
    Action pendingRewardAction;
    LevelPlayRewardedAd rewardedAd;

    void Awake()
    {
        InitializeAds();
    }

    void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= HandleInitSuccess;
        LevelPlay.OnInitFailed -= HandleInitFailed;

        if (rewardedAd == null)
            return;

        rewardedAd.OnAdLoaded -= HandleAdLoaded;
        rewardedAd.OnAdLoadFailed -= HandleAdLoadFailed;
        rewardedAd.OnAdDisplayFailed -= HandleAdDisplayFailed;
        rewardedAd.OnAdRewarded -= HandleAdRewarded;
        rewardedAd.OnAdClosed -= HandleAdClosed;
        rewardedAd.DestroyAd();
        rewardedAd = null;
    }

    public void ShowAd()
    {
        ShowRewardedAd(null);
    }

    public void ShowRewardedAd(Action onRewardCompleted)
    {
        if (Application.isEditor)
        {
            onRewardCompleted?.Invoke();
            return;
        }

        if (string.IsNullOrWhiteSpace(AppKey) || string.IsNullOrWhiteSpace(RewardedAdUnitId))
        {
            Debug.LogWarning("Rewarded ads are not configured for this platform.");
            return;
        }

        pendingRewardAction = onRewardCompleted;
        showWhenReady = true;

        if (!isInitialized)
        {
            InitializeAds();
            return;
        }

        TryShowOrLoadRewardedAd();
    }

    void InitializeAds()
    {
        if (Application.isEditor || isInitialized || isInitializing || string.IsNullOrWhiteSpace(AppKey))
            return;

        isInitializing = true;
        LevelPlay.OnInitSuccess -= HandleInitSuccess;
        LevelPlay.OnInitFailed -= HandleInitFailed;
        LevelPlay.OnInitSuccess += HandleInitSuccess;
        LevelPlay.OnInitFailed += HandleInitFailed;
        LevelPlay.Init(AppKey);
    }

    void HandleInitSuccess(LevelPlayConfiguration configuration)
    {
        isInitializing = false;
        isInitialized = true;

        if (rewardedAd == null)
        {
            rewardedAd = new LevelPlayRewardedAd(RewardedAdUnitId);
            rewardedAd.OnAdLoaded += HandleAdLoaded;
            rewardedAd.OnAdLoadFailed += HandleAdLoadFailed;
            rewardedAd.OnAdDisplayFailed += HandleAdDisplayFailed;
            rewardedAd.OnAdRewarded += HandleAdRewarded;
            rewardedAd.OnAdClosed += HandleAdClosed;
        }

        rewardedAd.LoadAd();

        if (showWhenReady)
            TryShowOrLoadRewardedAd();
    }

    void HandleInitFailed(LevelPlayInitError error)
    {
        isInitializing = false;
        showWhenReady = false;
        pendingRewardAction = null;
        Debug.LogWarning($"LevelPlay init failed: {error}");
    }

    void TryShowOrLoadRewardedAd()
    {
        if (rewardedAd == null)
            return;

        if (rewardedAd.IsAdReady())
        {
            showWhenReady = false;
            rewardGrantedForCurrentAd = false;
            rewardedAd.ShowAd();
            return;
        }

        rewardedAd.LoadAd();
    }

    void HandleAdLoaded(LevelPlayAdInfo adInfo)
    {
        if (showWhenReady)
            TryShowOrLoadRewardedAd();
    }

    void HandleAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogWarning($"Rewarded ad failed to load: {error}");
    }

    void HandleAdDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        showWhenReady = false;
        rewardGrantedForCurrentAd = false;
        pendingRewardAction = null;
        Debug.LogWarning($"Rewarded ad failed to display: {error}");

        if (rewardedAd != null)
            rewardedAd.LoadAd();
    }

    void HandleAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        if (rewardGrantedForCurrentAd)
            return;

        rewardGrantedForCurrentAd = true;

        Action rewardAction = pendingRewardAction;
        pendingRewardAction = null;
        rewardAction?.Invoke();
    }

    void HandleAdClosed(LevelPlayAdInfo adInfo)
    {
        rewardGrantedForCurrentAd = false;
        showWhenReady = false;

        if (rewardedAd != null)
            rewardedAd.LoadAd();
    }
}
