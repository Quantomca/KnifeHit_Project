using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KnifeSelectManager : MonoBehaviour
{
    public const string SceneName = "KnifeSelectScene";

    [Header("Scene References")]
    [SerializeField] Transform contentParent;
    [SerializeField] KnifePreviewUI previewUI;

    [Header("Grid")]
    [SerializeField] int maxGridItems = 16;
    [SerializeField] Vector2 gridCellSize = new Vector2(155f, 155f);
    [SerializeField] Vector2 gridSpacing = new Vector2(12f, 12f);

    [Header("Unlock Random")]
    [SerializeField] Button unlockRandomButton;
    [SerializeField] Text unlockRandomText;
    [SerializeField] TMP_Text unlockRandomTMPText;
    [SerializeField] int unlockAppleCost = 1;

    readonly List<KnifeItemUI> items = new List<KnifeItemUI>();

    string currentSelectedID;

    public static void EnsureRuntimeSceneExists()
    {
        if (FindFirstObjectByType<KnifeSelectManager>() != null)
            return;

        GameObject managerObject = GameObject.Find(nameof(KnifeSelectManager));
        if (managerObject == null)
            managerObject = new GameObject(nameof(KnifeSelectManager));

        managerObject.AddComponent<KnifeSelectManager>();
    }

    void Awake()
    {
        EnsureEventSystem();
        ResolveSceneReferences();
        ConfigureExistingGrid();
    }

    void OnEnable()
    {
        AppleWallet.AppleCountChanged += HandleAppleCountChanged;
    }

    void OnDisable()
    {
        AppleWallet.AppleCountChanged -= HandleAppleCountChanged;
    }

    void Start()
    {
        LoadSelection();
        GenerateUI();
        ShowSelectedPreview();
        RefreshUnlockButton();
    }

    void LoadSelection()
    {
        currentSelectedID = KnifeDatabase.GetSelectedKnifeId();
    }

    void GenerateUI()
    {
        if (contentParent == null)
        {
            Debug.LogError("KnifeSelectManager requires an existing Grid/Content container in the scene.");
            return;
        }

        items.Clear();

        IReadOnlyList<KnifeData> knives = KnifeDatabase.Knives;
        int knifeCount = Mathf.Min(maxGridItems, knives.Count);
        List<KnifeItemUI> existingItems = GetExistingItems();

        for (int index = 0; index < knifeCount; index++)
        {
            KnifeData knife = knives[index];
            KnifeItemUI item = index < existingItems.Count
                ? existingItems[index]
                : KnifeItemUI.CreateRuntimeItem(contentParent);

            item.Setup(knife, this);
            item.SetSelected(knife.id == currentSelectedID);
            item.gameObject.SetActive(true);
            items.Add(item);
        }

        for (int index = knifeCount; index < existingItems.Count; index++)
            existingItems[index].gameObject.SetActive(false);
    }

    public void SelectKnife(string id)
    {
        if (!KnifeDatabase.IsKnifeUnlocked(id))
            return;

        currentSelectedID = id;
        KnifeDatabase.SelectKnife(id);

        UpdateSelectionUI();
        ShowSelectedPreview();
    }

    void UpdateSelectionUI()
    {
        foreach (KnifeItemUI item in items)
            item.SetSelected(item.KnifeID == currentSelectedID);
    }

    void ShowSelectedPreview()
    {
        if (previewUI == null)
            return;

        KnifeData knife = KnifeDatabase.GetKnifeByID(currentSelectedID);
        previewUI.ShowKnife(knife);
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    void ResolveSceneReferences()
    {
        if (contentParent == null)
        {
            GameObject gridObject = GameObject.Find("Grid");
            if (gridObject != null)
                contentParent = gridObject.transform;
        }

        if (contentParent == null)
        {
            ScrollRect scrollRect = FindFirstObjectByType<ScrollRect>();
            if (scrollRect != null && scrollRect.content != null)
                contentParent = scrollRect.content;
        }

        if (contentParent == null)
        {
            GameObject contentObject = GameObject.Find("Content");
            if (contentObject != null)
                contentParent = contentObject.transform;
        }

        if (previewUI == null)
            previewUI = FindFirstObjectByType<KnifePreviewUI>();

        if (previewUI == null)
        {
            Image previewImage = FindImage("PreviewImage");
            if (previewImage == null)
                previewImage = FindImage("SelectedKnifePreview");

            if (previewImage != null)
            {
                previewUI = previewImage.GetComponent<KnifePreviewUI>();
                if (previewUI == null)
                    previewUI = previewImage.gameObject.AddComponent<KnifePreviewUI>();

                previewUI.previewImage = previewImage;
            }
        }

        ResolveUnlockButton();
    }

    void ConfigureExistingGrid()
    {
        if (contentParent == null)
            return;

        GridLayoutGroup gridLayout = contentParent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            return;

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 4;
        gridLayout.cellSize = gridCellSize;
        gridLayout.spacing = gridSpacing;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.childAlignment = TextAnchor.UpperLeft;

        RectTransform contentRect = contentParent as RectTransform;
        if (contentRect != null)
        {
            float width = (gridCellSize.x * 4f) + (gridSpacing.x * 3f);
            float height = (gridCellSize.y * 4f) + (gridSpacing.y * 3f);
            contentRect.sizeDelta = new Vector2(width, height);
        }

        ScrollRect scrollRect = contentParent.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = false;

            if (scrollRect.horizontalScrollbar != null)
                scrollRect.horizontalScrollbar.gameObject.SetActive(false);

            if (scrollRect.verticalScrollbar != null)
                scrollRect.verticalScrollbar.gameObject.SetActive(false);
        }
    }

    List<KnifeItemUI> GetExistingItems()
    {
        List<KnifeItemUI> existingItems = new List<KnifeItemUI>();

        for (int i = 0; i < contentParent.childCount; i++)
        {
            KnifeItemUI item = contentParent.GetChild(i).GetComponent<KnifeItemUI>();
            if (item != null)
                existingItems.Add(item);
        }

        return existingItems;
    }

    void ResolveUnlockButton()
    {
        if (unlockRandomButton == null)
        {
            GameObject buttonObject = GameObject.Find("UnlockRandomButton");
            if (buttonObject != null)
                unlockRandomButton = buttonObject.GetComponent<Button>();
        }

        if (unlockRandomButton == null)
        {
            Debug.LogWarning("KnifeSelectManager requires an UnlockRandomButton in the scene.");
            return;
        }

        if (unlockRandomText == null)
            unlockRandomText = unlockRandomButton.GetComponentInChildren<Text>(true);

        if (unlockRandomTMPText == null)
            unlockRandomTMPText = unlockRandomButton.GetComponentInChildren<TMP_Text>(true);

        unlockRandomButton.onClick.RemoveListener(UnlockRandomKnife);
        unlockRandomButton.onClick.AddListener(UnlockRandomKnife);
    }

    public void UnlockRandomKnife()
    {
        if (!KnifeDatabase.HasLockedKnives())
        {
            RefreshAppleUI();
            return;
        }

        if (!AppleWallet.TrySpendApples(unlockAppleCost))
        {
            RefreshAppleUI();
            return;
        }

        RefreshAppleUI();

        string unlockedKnifeId = KnifeDatabase.UnlockRandomLockedKnife();
        if (string.IsNullOrWhiteSpace(unlockedKnifeId))
        {
            AppleWallet.AddApples(unlockAppleCost);
            RefreshAppleUI();
            return;
        }

        currentSelectedID = unlockedKnifeId;
        KnifeDatabase.SelectKnife(unlockedKnifeId);
        GameAudio.PlayKnifeUnlock();

        GenerateUI();
        UpdateSelectionUI();
        ShowSelectedPreview();
        RefreshAppleUI();
    }

    void RefreshUnlockButton()
    {
        if (unlockRandomButton == null)
            return;

        int lockedKnives = KnifeDatabase.CountLockedKnives();
        bool canUnlock = lockedKnives > 0 && AppleWallet.GetAppleCount() >= unlockAppleCost;
        string buttonLabel = lockedKnives <= 0
            ? "ALL KNIVES UNLOCKED"
            : $"UNLOCK RANDOM\nCOST: {unlockAppleCost} APPLE";

        unlockRandomButton.interactable = canUnlock;

        Image background = unlockRandomButton.GetComponent<Image>();
        if (background != null)
        {
            background.color = lockedKnives <= 0
                ? new Color(0.26f, 0.31f, 0.35f, 1f)
                : canUnlock
                    ? new Color(0.11f, 0.82f, 0.25f, 1f)
                    : new Color(0.19f, 0.49f, 0.25f, 1f);
        }

        SetUnlockButtonText(buttonLabel);
    }

    void HandleAppleCountChanged(int appleCount)
    {
        RefreshUnlockButton();
    }

    Image FindImage(string objectName)
    {
        GameObject imageObject = GameObject.Find(objectName);
        return imageObject != null ? imageObject.GetComponent<Image>() : null;
    }

    void SetUnlockButtonText(string value)
    {
        if (unlockRandomText != null)
            unlockRandomText.text = value;

        if (unlockRandomTMPText != null)
            unlockRandomTMPText.text = value;
    }

    void RefreshAppleUI()
    {
        RefreshUnlockButton();

        GlobalAppleHUD[] huds = Object.FindObjectsByType<GlobalAppleHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < huds.Length; i++)
            huds[i].RefreshNow();
    }
}
