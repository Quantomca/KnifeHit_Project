using UnityEngine;
using UnityEngine.UI;

public class KnifeItemUI : MonoBehaviour
{
    [SerializeField] Image background;
    [SerializeField] Image icon;
    [SerializeField] GameObject selectedBorder;
    [SerializeField] GameObject lockIcon;

    string knifeID;
    KnifeSelectManager manager;
    bool unlocked;
    Button button;

    static Sprite whiteSprite;

    static Sprite WhiteSprite
    {
        get
        {
            if (whiteSprite == null)
            {
                whiteSprite = Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f));
            }

            return whiteSprite;
        }
    }

    public string KnifeID => knifeID;

    void Awake()
    {
        button = GetComponent<Button>();
        if (background == null)
            background = GetComponent<Image>();
    }

    public void Setup(KnifeData data, KnifeSelectManager manager)
    {
        knifeID = data.id;
        this.manager = manager;
        unlocked = data.unlocked;

        if (icon != null)
        {
            icon.sprite = data.GetPreviewSprite();
            icon.preserveAspect = true;
        }

        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
            button.interactable = unlocked;
        }
    }

    void OnClick()
    {
        if (!unlocked)
            return;

        manager.SelectKnife(knifeID);
    }

    public void SetSelected(bool isSelected)
    {
        if (selectedBorder != null)
            selectedBorder.SetActive(false);

        if (background != null)
        {
            background.color = isSelected
                ? new Color(0.12f, 0.45f, 0.46f, 1f)
                : new Color(0.04f, 0.08f, 0.12f, 0.98f);
        }
    }

    public static KnifeItemUI CreateRuntimeItem(Transform parent)
    {
        GameObject root = new GameObject("KnifeItem", typeof(RectTransform), typeof(Image), typeof(Button), typeof(KnifeItemUI));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(parent, false);
        rootRect.sizeDelta = new Vector2(160f, 160f);

        Image rootImage = root.GetComponent<Image>();
        rootImage.sprite = WhiteSprite;
        rootImage.color = new Color(0.04f, 0.08f, 0.12f, 0.98f);

        Button rootButton = root.GetComponent<Button>();
        rootButton.targetGraphic = rootImage;

        ColorBlock colors = rootButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.selectedColor = Color.white;
        rootButton.colors = colors;

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.SetParent(rootRect, false);
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(20f, 20f);
        iconRect.offsetMax = new Vector2(-20f, -20f);

        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.raycastTarget = false;
        iconImage.preserveAspect = true;

        GameObject lockObject = new GameObject("LockOverlay", typeof(RectTransform), typeof(Image));
        RectTransform lockRect = lockObject.GetComponent<RectTransform>();
        lockRect.SetParent(rootRect, false);
        lockRect.anchorMin = Vector2.zero;
        lockRect.anchorMax = Vector2.one;
        lockRect.offsetMin = Vector2.zero;
        lockRect.offsetMax = Vector2.zero;

        Image lockImage = lockObject.GetComponent<Image>();
        lockImage.sprite = WhiteSprite;
        lockImage.color = new Color(0f, 0f, 0f, 0.55f);
        lockImage.raycastTarget = false;
        lockObject.SetActive(false);

        KnifeItemUI item = root.GetComponent<KnifeItemUI>();
        item.background = rootImage;
        item.icon = iconImage;
        item.selectedBorder = null;
        item.lockIcon = lockObject;
        item.button = rootButton;

        return item;
    }
}
