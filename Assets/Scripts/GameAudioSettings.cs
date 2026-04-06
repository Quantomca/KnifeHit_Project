using UnityEngine;

[CreateAssetMenu(menuName = "Knife Hit/Game Audio Settings", fileName = "GameAudioSettings")]
public class GameAudioSettings : ScriptableObject
{
    [Header("UI")]
    public AudioClip buttonClickClip;
    public AudioClip appleGainClip;
    public AudioClip knifeUnlockClip;

    [Header("Gameplay")]
    public AudioClip knifeThrowClip;
    public AudioClip knifeHitWoodClip;
    public AudioClip knifeHitKnifeClip;
    public AudioClip gameLoseClip;

    [Header("Rewarded Ads")]
    public AudioClip continueRewardClip;
    public AudioClip rewardedAppleClip;

    [Header("Volumes")]
    [Range(0f, 1f)] public float buttonClickVolume = 0.8f;
    [Range(0f, 1f)] public float appleGainVolume = 0.9f;
    [Range(0f, 1f)] public float knifeUnlockVolume = 1f;
    [Range(0f, 1f)] public float knifeThrowVolume = 0.9f;
    [Range(0f, 1f)] public float knifeHitWoodVolume = 1f;
    [Range(0f, 1f)] public float knifeHitKnifeVolume = 1f;
    [Range(0f, 1f)] public float gameLoseVolume = 1f;
    [Range(0f, 1f)] public float continueRewardVolume = 1f;
    [Range(0f, 1f)] public float rewardedAppleVolume = 1f;
}
