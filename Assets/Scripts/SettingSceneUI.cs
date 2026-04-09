using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingSceneUI : MonoBehaviour
{
    static readonly Color ToggleOnColor = new Color(0.11f, 0.87f, 0.24f, 1f);
    static readonly Color ToggleOffColor = Color.white;
    static readonly Color ToggleOnTextColor = Color.white;
    static readonly Color ToggleOffTextColor = new Color(0.62f, 0.62f, 0.62f, 1f);

    [Header("Toggle Sprites")]
    [SerializeField] Sprite toggleOffSprite;
    [SerializeField] Sprite toggleOnSprite;

    [Header("Optional Direct References")]
    [SerializeField] Button soundsToggleButton;
    [SerializeField] Image soundsTrackImage;
    [SerializeField] TMP_Text soundsStateText;
    [SerializeField] RectTransform soundsThumbRect;

    void Awake()
    {
        ResolveReferences();
        BindSoundsToggle();
        RefreshSoundsToggle();
    }

    void OnEnable()
    {
        RefreshSoundsToggle();
    }

    void ResolveReferences()
    {
        if (soundsToggleButton == null)
        {
            Transform soundsRow = FindChildRecursive(transform, "SOUNDSRow");
            if (soundsRow != null)
            {
                Transform toggleButton = FindChildRecursive(soundsRow, "ToggleButton");
                if (toggleButton != null)
                    soundsToggleButton = toggleButton.GetComponent<Button>();
            }
        }

        if (soundsTrackImage == null && soundsToggleButton != null)
            soundsTrackImage = soundsToggleButton.GetComponent<Image>();

        if (soundsStateText == null && soundsToggleButton != null)
        {
            Transform stateText = FindChildRecursive(soundsToggleButton.transform, "StateText");
            if (stateText != null)
                soundsStateText = stateText.GetComponent<TMP_Text>();
        }

        if (soundsThumbRect == null && soundsToggleButton != null)
        {
            Transform thumb = FindChildRecursive(soundsToggleButton.transform, "Thumb");
            if (thumb != null)
                soundsThumbRect = thumb as RectTransform;
        }
    }

    void BindSoundsToggle()
    {
        if (soundsToggleButton == null)
            return;

        UIButtonSound buttonSound = soundsToggleButton.GetComponent<UIButtonSound>();
        if (buttonSound != null)
            Destroy(buttonSound);

        soundsToggleButton.onClick.RemoveListener(HandleSoundsToggleClicked);
        soundsToggleButton.onClick.AddListener(HandleSoundsToggleClicked);
    }

    void HandleSoundsToggleClicked()
    {
        GameAudio.SetMuted(!GameAudio.IsMuted);
        RefreshSoundsToggle();
    }

    void RefreshSoundsToggle()
    {
        bool soundEnabled = !GameAudio.IsMuted;

        if (soundsTrackImage != null)
        {
            if (toggleOnSprite != null && toggleOffSprite != null)
                soundsTrackImage.sprite = soundEnabled ? toggleOnSprite : toggleOffSprite;

            soundsTrackImage.color = soundEnabled ? ToggleOnColor : ToggleOffColor;
        }

        if (soundsStateText != null)
        {
            RectTransform stateRect = soundsStateText.rectTransform;
            float horizontalOffset = stateRect.sizeDelta.x * 0.22f;

            soundsStateText.text = soundEnabled ? "ON" : "OFF";
            soundsStateText.color = soundEnabled ? ToggleOnTextColor : ToggleOffTextColor;
            stateRect.anchoredPosition = new Vector2(soundEnabled ? -horizontalOffset : horizontalOffset, stateRect.anchoredPosition.y);
        }

        if (soundsThumbRect != null && soundsToggleButton != null)
        {
            RectTransform buttonRect = soundsToggleButton.transform as RectTransform;
            float width = buttonRect != null ? buttonRect.rect.width : 0f;
            float thumbWidth = soundsThumbRect.rect.width;
            float edgePadding = (buttonRect != null ? buttonRect.rect.height : thumbWidth) * 0.12f;
            float travel = Mathf.Max(0f, (width * 0.5f) - (thumbWidth * 0.5f) - edgePadding);

            soundsThumbRect.anchoredPosition = new Vector2(soundEnabled ? travel : -travel, soundsThumbRect.anchoredPosition.y);
        }
    }

    Transform FindChildRecursive(Transform parent, string targetName)
    {
        if (parent == null)
            return null;

        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == targetName)
                return children[i];
        }

        return null;
    }
}
