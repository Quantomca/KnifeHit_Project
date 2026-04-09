using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameAudio : MonoBehaviour
{
    const string MutedKey = "GameAudio.Muted";

    static GameAudio instance;
    static bool isMuted;

    AudioSource audioSource;
    GameAudioSettings settings;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CreateInstance()
    {
        LoadMuteState();
        ApplyMuteState();

        if (instance != null)
            return;

        GameObject audioObject = new GameObject(nameof(GameAudio));
        DontDestroyOnLoad(audioObject);
        instance = audioObject.AddComponent<GameAudio>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.mute = isMuted;

        settings = Resources.Load<GameAudioSettings>("GameAudioSettings");

        AppleWallet.ApplesAdded -= HandleApplesAdded;
        AppleWallet.ApplesAdded += HandleApplesAdded;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;

        ApplyMuteState();
    }

    void OnDestroy()
    {
        AppleWallet.ApplesAdded -= HandleApplesAdded;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    public static void PlayButtonClick()
    {
        instance?.PlayClip(instance.settings != null ? instance.settings.buttonClickClip : null,
            instance != null && instance.settings != null ? instance.settings.buttonClickVolume : 1f);
    }

    public static bool IsMuted => isMuted;

    public static void SetMuted(bool muted)
    {
        if (isMuted == muted)
            return;

        isMuted = muted;
        PlayerPrefs.SetInt(MutedKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMuteState();
    }

    public static void PlayKnifeThrow()
    {
        instance?.PlayClip(instance.settings != null ? instance.settings.knifeThrowClip : null,
            instance != null && instance.settings != null ? instance.settings.knifeThrowVolume : 1f);
    }

    public static void PlayKnifeHitWood()
    {
        instance?.PlayClip(instance.settings != null ? instance.settings.knifeHitWoodClip : null,
            instance != null && instance.settings != null ? instance.settings.knifeHitWoodVolume : 1f);
    }

    public static void PlayKnifeHitKnife()
    {
        instance?.PlayClip(instance.settings != null ? instance.settings.knifeHitKnifeClip : null,
            instance != null && instance.settings != null ? instance.settings.knifeHitKnifeVolume : 1f);
    }

    public static void PlayKnifeUnlock()
    {
        instance?.PlayClip(instance.settings != null ? instance.settings.knifeUnlockClip : null,
            instance != null && instance.settings != null ? instance.settings.knifeUnlockVolume : 1f);
    }

    public static void PlayGameLose()
    {
        instance?.PlayClip(instance.settings != null ? instance.settings.gameLoseClip : null,
            instance != null && instance.settings != null ? instance.settings.gameLoseVolume : 1f);
    }

    public static void PlayContinueReward()
    {
        instance?.PlayClip(instance.settings != null ? instance.settings.continueRewardClip : null,
            instance != null && instance.settings != null ? instance.settings.continueRewardVolume : 1f);
    }

    public static void PlayRewardedAppleBonus()
    {
        instance?.PlayClip(instance.settings != null ? instance.settings.rewardedAppleClip : null,
            instance != null && instance.settings != null ? instance.settings.rewardedAppleVolume : 1f);
    }

    void HandleApplesAdded(int amount)
    {
        if (amount <= 0 || settings == null)
            return;

        PlayClip(settings.appleGainClip, settings.appleGainVolume);
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InstallButtonSounds();
    }

    void InstallButtonSounds()
    {
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++)
        {
            UIButtonSound buttonSound = buttons[i].GetComponent<UIButtonSound>();

            if (ShouldSkipButtonSound(buttons[i]))
            {
                if (buttonSound != null)
                    Destroy(buttonSound);

                continue;
            }

            if (buttonSound == null)
                buttons[i].gameObject.AddComponent<UIButtonSound>();
        }
    }

    void PlayClip(AudioClip clip, float volume)
    {
        if (isMuted || audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    static void LoadMuteState()
    {
        isMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
    }

    static void ApplyMuteState()
    {
        AudioListener.volume = isMuted ? 0f : 1f;

        if (instance != null && instance.audioSource != null)
            instance.audioSource.mute = isMuted;
    }

    static bool ShouldSkipButtonSound(Button button)
    {
        if (button == null)
            return false;

        Transform parent = button.transform.parent;
        return parent != null
            && parent.name == "SOUNDSRow"
            && button.name == "ToggleButton";
    }
}
