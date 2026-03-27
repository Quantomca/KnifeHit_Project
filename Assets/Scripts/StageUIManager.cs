using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class StageUIManager : MonoBehaviour
{
    public static StageUIManager instance;

    [Header("UI Setup")]
    public TextMeshProUGUI stageText;       // Text hiển thị Stage
    public Transform iconContainer;          // Container cho icon level
    public GameObject levelIconPrefab;       // Prefab level thường
    public GameObject bossIconPrefab;        // Prefab boss

    [Header("Colors")]
    public Color unlockedColor = new Color(1f, 0.85f, 0f); // vàng
    public Color lockedColor = Color.white;                 // trắng

    [Header("Level Config")]
    public int totalLevels = 20;
    public int bossEvery = 5;     // mỗi 5 level là boss

    private List<Image> levelIcons = new List<Image>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        GenerateLevelIcons();
        UpdateStageUI(1);
    }

    /// <summary>
    /// Tạo icon cho tất cả level
    /// </summary>
    public void GenerateLevelIcons()
    {
        levelIcons.Clear();

        for (int i = 1; i <= totalLevels; i++)
        {
            GameObject prefab = (i % bossEvery == 0) ? bossIconPrefab : levelIconPrefab;
            GameObject icon = Instantiate(prefab, iconContainer);
            Image img = icon.GetComponent<Image>();

            img.color = lockedColor;
            img.transform.localScale = Vector3.one;

            levelIcons.Add(img);
        }
    }

    /// <summary>
    /// Cập nhật stage hiện tại
    /// </summary>
public void UpdateStageUI(int currentLevel)
{
    stageText.text = "STAGE " + currentLevel;

    for (int i = 0; i < levelIcons.Count; i++)
    {
        Image img = levelIcons[i];

        if (i < currentLevel)
        {
            if (img.color != unlockedColor)  // chỉ pop lần đầu
            {
                img.color = unlockedColor;
                StartCoroutine(PopIcon(img.transform));
            }
        }
        else
        {
            img.color = lockedColor;
        }
    }
}

    private IEnumerator PopIcon(Transform t)
    {
        Vector3 original = t.localScale;
        Vector3 target = original * 1.3f;

        float elapsed = 0f;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            t.localScale = Vector3.Lerp(original, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            t.localScale = Vector3.Lerp(target, original, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        t.localScale = original;
    }

    /// <summary>
    /// Reset stage (chơi lại game)
    /// </summary>
    public void ResetStageUI()
    {
        foreach (var img in levelIcons)
        {
            img.color = lockedColor;
            img.transform.localScale = Vector3.one;
        }
        stageText.text = "STAGE 1";
    }
}