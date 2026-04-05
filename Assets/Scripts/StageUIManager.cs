using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageUIManager : MonoBehaviour
{
    public static StageUIManager instance;

    [Header("UI Setup")]
    public TextMeshProUGUI stageText;
    public Transform iconContainer;
    public GameObject levelIconPrefab;
    public GameObject bossIconPrefab;

    [Header("Colors")]
    public Color unlockedColor = new Color(1f, 0.85f, 0f);
    public Color lockedColor = Color.white;

    [Header("Level Config")]
    public int totalLevels = 5;
    public int bossEvery = 5;

    readonly List<Image> levelIcons = new List<Image>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        RebuildIcons();
        UpdateStageUI(1, 1);
    }

    public void RebuildIcons()
    {
        levelIcons.Clear();

        if (iconContainer == null)
            return;

        for (int index = iconContainer.childCount - 1; index >= 0; index--)
            Destroy(iconContainer.GetChild(index).gameObject);

        for (int level = 1; level <= totalLevels; level++)
        {
            GameObject prefab = level % Mathf.Max(1, bossEvery) == 0 ? bossIconPrefab : levelIconPrefab;
            if (prefab == null)
                continue;

            GameObject icon = Instantiate(prefab, iconContainer);
            Image image = icon.GetComponent<Image>();
            if (image == null)
                continue;

            image.color = lockedColor;
            image.transform.localScale = Vector3.one;
            levelIcons.Add(image);
        }
    }

    public void UpdateStageUI(int currentStage, int currentLevel)
    {
        if (stageText != null)
            stageText.text = "STAGE " + Mathf.Max(1, currentStage);

        int visibleProgress = Mathf.Clamp(currentLevel, 1, Mathf.Max(1, totalLevels));

        for (int index = 0; index < levelIcons.Count; index++)
        {
            Image image = levelIcons[index];
            bool isUnlocked = index < visibleProgress;

            if (isUnlocked)
            {
                if (image.color != unlockedColor)
                {
                    image.color = unlockedColor;
                    StartCoroutine(PopIcon(image.transform));
                }
            }
            else
            {
                image.color = lockedColor;
                image.transform.localScale = Vector3.one;
            }
        }
    }

    public void ResetForNewStage(int currentStage)
    {
        RebuildIcons();
        UpdateStageUI(currentStage, 1);
    }

    public void ResetStageUI()
    {
        RebuildIcons();
        UpdateStageUI(1, 1);
    }

    public void SetGameplayUIVisible(bool isVisible)
    {
        if (stageText != null)
            stageText.gameObject.SetActive(isVisible);

        if (iconContainer != null)
            iconContainer.gameObject.SetActive(isVisible);
    }

    IEnumerator PopIcon(Transform iconTransform)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * 1.3f;
        float elapsed = 0f;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            iconTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            iconTransform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        iconTransform.localScale = originalScale;
    }
}
