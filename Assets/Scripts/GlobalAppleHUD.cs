using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalAppleHUD : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] RectTransform counterRoot;
    [SerializeField] Image appleIconImage;
    [SerializeField] TMP_Text appleCountTMPText;
    [SerializeField] Text appleCountText;

    void Reset()
    {
        ResolveReferences();
    }

    void Awake()
    {
        ResolveReferences();
        ApplyPresentation();
        RefreshNow();
    }

    void OnEnable()
    {
        ResolveReferences();
        AppleWallet.AppleCountChanged += HandleAppleCountChanged;

        ApplyPresentation();
        RefreshNow();
    }

    void OnDisable()
    {
        AppleWallet.AppleCountChanged -= HandleAppleCountChanged;
    }

    void ResolveReferences()
    {
        if (counterRoot == null)
            counterRoot = FindChildComponent<RectTransform>("AppleCounterUI");

        if (appleIconImage == null)
            appleIconImage = FindChildComponent<Image>("AppleIcon");

        if (appleCountTMPText == null)
            appleCountTMPText = FindChildComponent<TMP_Text>("AppleCountText");

        if (appleCountText == null)
            appleCountText = FindChildComponent<Text>("AppleCountText");
    }

    void ApplyPresentation()
    {
        if (appleCountTMPText != null)
            appleCountTMPText.raycastTarget = false;

        if (appleCountText != null)
            appleCountText.raycastTarget = false;

        if (appleIconImage != null)
        {
            if (appleIconImage.sprite == null)
                appleIconImage.sprite = AppleConfigLoader.AppleSprite;

            appleIconImage.preserveAspect = true;
            appleIconImage.raycastTarget = false;
        }
    }

    public void RefreshNow()
    {
        RefreshVisibility();
        RefreshAppleCount(AppleWallet.GetAppleCount());
    }

    void RefreshVisibility()
    {
        if (counterRoot == null)
            return;

        counterRoot.gameObject.SetActive(!IsIntroScene());
    }

    bool IsIntroScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName.ToLowerInvariant().Contains("intro");
    }

    void HandleAppleCountChanged(int appleCount)
    {
        RefreshAppleCount(appleCount);
    }

    void RefreshAppleCount(int appleCount)
    {
        string countText = appleCount.ToString();

        if (appleCountTMPText != null)
            appleCountTMPText.text = countText;

        if (appleCountText != null)
            appleCountText.text = countText;
    }

    T FindChildComponent<T>(string objectName) where T : Component
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name != objectName)
                continue;

            T component = children[i].GetComponent<T>();
            if (component != null)
                return component;
        }

        return null;
    }
}
