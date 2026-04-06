using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KnifeUIManager : MonoBehaviour
{
    public static KnifeUIManager instance;

    [Header("UI Setup")]
    public GameObject knifeIconPrefab;
    public Transform container;

    [Header("Runtime")]
    public int totalKnives = 5;

    readonly List<Image> knifeIcons = new List<Image>();
    int currentIndex;

    readonly Color activeColor = Color.white;
    readonly Color usedColor = new Color(0f, 0f, 0f, 0.3f);

    void Awake()
    {
        instance = this;
    }

    void GenerateUI()
    {
        EnsureIconCount(totalKnives);
        currentIndex = 0;

        for (int i = 0; i < knifeIcons.Count; i++)
        {
            Image image = knifeIcons[i];
            if (image == null)
                continue;

            image.gameObject.SetActive(i < totalKnives);
            image.color = activeColor;
        }

        RebuildLayout();
    }

    public void UseKnife()
    {
        if (currentIndex >= knifeIcons.Count)
            return;

        int index = knifeIcons.Count - 1 - currentIndex;
        if (knifeIcons[index] != null)
            knifeIcons[index].color = usedColor;

        currentIndex++;
    }

    public void ResetUI()
    {
        if (container == null)
            return;

        GenerateUI();
    }

    public void AddReserveKnives(int amount)
    {
        if (amount <= 0 || knifeIconPrefab == null || container == null)
            return;

        totalKnives += amount;

        EnsureIconCount(totalKnives);
        RebuildLayout();
    }

    void EnsureIconCount(int requiredCount)
    {
        while (knifeIcons.Count < requiredCount)
            AddKnifeIcon();
    }

    void AddKnifeIcon()
    {
        if (knifeIconPrefab == null || container == null)
            return;

        GameObject icon = Instantiate(knifeIconPrefab, container);
        Image image = icon.GetComponent<Image>();
        if (image != null)
            image.color = activeColor;

        knifeIcons.Add(image);
    }

    void RebuildLayout()
    {
        if (container is RectTransform rectTransform)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}
