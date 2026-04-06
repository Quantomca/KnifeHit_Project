using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdsButton : MonoBehaviour
{
    enum RewardTarget
    {
        Auto,
        ContinueGameplay,
        AddApples
    }

    const int DefaultAppleReward = 50;

    public AdsManager adsManager;

    [SerializeField] RewardTarget rewardTarget = RewardTarget.Auto;
    [SerializeField] int appleRewardAmount = DefaultAppleReward;

    public void WatchAd()
    {
        if (adsManager == null)
        {
            Debug.LogWarning($"{nameof(AdsButton)} on {name} is missing an AdsManager reference.");
            return;
        }

        adsManager.ShowRewardedAd(GrantReward);
    }

    void GrantReward()
    {
        switch (ResolveRewardTarget())
        {
            case RewardTarget.ContinueGameplay:
                if (LevelManager.instance != null)
                    LevelManager.instance.ContinueAfterRewardedAd();
                break;

            case RewardTarget.AddApples:
                AppleWallet.AddApples(Mathf.Max(1, appleRewardAmount));
                break;
        }
    }

    RewardTarget ResolveRewardTarget()
    {
        if (rewardTarget != RewardTarget.Auto)
            return rewardTarget;

        string activeSceneName = SceneManager.GetActiveScene().name;

        if (string.Equals(activeSceneName, KnifeSelectManager.SceneName, StringComparison.Ordinal))
            return RewardTarget.AddApples;

        if (string.Equals(name, "WatchAdsButton", StringComparison.Ordinal))
            return RewardTarget.ContinueGameplay;

        return RewardTarget.AddApples;
    }
}
