using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KnifeSelectManager : MonoBehaviour
{
    public const string SceneName = "KnifeSelectScene";

    [Header("Scene References")]
    [SerializeField] Canvas sceneCanvas;
    [SerializeField] Transform contentParent;
    [SerializeField] KnifePreviewUI previewUI;

    [Header("Grid")]
    [SerializeField] int maxGridItems = 16;
    [SerializeField] Vector2 gridCellSize = new Vector2(155f, 155f);
    [SerializeField] Vector2 gridSpacing = new Vector2(12f, 12f);

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

    void Start()
    {
        LoadSelection();
        GenerateUI();
        ShowSelectedPreview();
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
        if (sceneCanvas == null)
            sceneCanvas = ResolveCanvas();

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
    }

    Canvas ResolveCanvas()
    {
        GameObject preferredCanvasObject = GameObject.Find("Canvas (1)");
        if (preferredCanvasObject == null)
            preferredCanvasObject = GameObject.Find("Canvas");
        if (preferredCanvasObject == null)
            preferredCanvasObject = GameObject.Find("Canvas ");

        if (preferredCanvasObject != null)
            return preferredCanvasObject.GetComponent<Canvas>();

        return FindFirstObjectByType<Canvas>();
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

    Image FindImage(string objectName)
    {
        GameObject imageObject = GameObject.Find(objectName);
        return imageObject != null ? imageObject.GetComponent<Image>() : null;
    }
}
