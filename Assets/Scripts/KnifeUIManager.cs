using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class KnifeUIManager : MonoBehaviour
{
    public static KnifeUIManager instance;

    [Header("UI Setup")]
    public GameObject knifeIconPrefab;
    public Transform container;

    [Header("Runtime")]
    public int totalKnives = 5;

    private List<Image> knifeIcons = new List<Image>();
    private int currentIndex = 0;

    private Color activeColor = Color.white;
    private Color usedColor = new Color(0, 0, 0, 0.3f);

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
    }


void GenerateUI()
{
    knifeIcons.Clear();

    for (int i = 0; i < totalKnives; i++)
    {
        GameObject icon = Instantiate(knifeIconPrefab, container);

        Image img = icon.GetComponent<Image>();
        img.color = activeColor;

        knifeIcons.Add(img);
        Canvas.ForceUpdateCanvases();
    }

    // 🔥 ÉP cập nhật layout
    LayoutRebuilder.ForceRebuildLayoutImmediate(container.GetComponent<RectTransform>());
}

    // 🔥 Gọi mỗi khi dùng dao
        public void UseKnife()
        {
            if (currentIndex >= knifeIcons.Count) return;

            int index = knifeIcons.Count - 1 - currentIndex;

            knifeIcons[index].color = usedColor;

            currentIndex++;
        }

    // 🔥 Reset UI khi sang level
    public void ResetUI()
    {
        // Xoá sạch UI cũ NGAY LẬP TỨC
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(container.GetChild(i).gameObject);
        }

        knifeIcons.Clear();
        currentIndex = 0;

        GenerateUI();
    }
}