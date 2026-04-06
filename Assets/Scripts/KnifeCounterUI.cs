using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KnifeCounterUI : MonoBehaviour
{
    public static KnifeCounterUI instance;
    public static int TotalSuccessfulKnives => totalSuccessfulKnives;

    static readonly List<KnifeCounterUI> instances = new List<KnifeCounterUI>();
    static KnifeCounterDisplayMode displayMode = KnifeCounterDisplayMode.Gameplay;
    static int totalSuccessfulKnives;

    [SerializeField] TextMeshProUGUI counterText;
    [SerializeField] bool isContinueCounter;

    void Awake()
    {
        AutoDetectCounterType();

        if (!instances.Contains(this))
            instances.Add(this);

        if (instance == null || (instance.isContinueCounter && !isContinueCounter))
            instance = this;

        UpdateUI();
        ApplyVisibility();
    }

    void OnDestroy()
    {
        instances.Remove(this);

        if (instance != this)
            return;

        instance = null;

        for (int i = 0; i < instances.Count; i++)
        {
            if (!instances[i].isContinueCounter)
            {
                instance = instances[i];
                return;
            }
        }

        if (instances.Count > 0)
            instance = instances[0];
    }

    void Start()
    {
        UpdateUI();
        ApplyVisibility();
    }

    public void AddKnife()
    {
        totalSuccessfulKnives++;
        RefreshAll();
    }

    public void ResetCounter()
    {
        ResetTotalCounter();
    }

    public static void ResetTotalCounter()
    {
        totalSuccessfulKnives = 0;
        RefreshAll();
    }

    public static void SetDisplayMode(KnifeCounterDisplayMode mode)
    {
        displayMode = mode;

        for (int i = 0; i < instances.Count; i++)
            instances[i].ApplyVisibility();
    }

    void UpdateUI()
    {
        if (counterText != null)
            counterText.text = totalSuccessfulKnives.ToString();
    }

    void ApplyVisibility()
    {
        bool shouldShow = displayMode switch
        {
            KnifeCounterDisplayMode.Gameplay => !isContinueCounter,
            KnifeCounterDisplayMode.Continue => isContinueCounter,
            _ => false
        };

        gameObject.SetActive(shouldShow);
    }

    void AutoDetectCounterType()
    {
        if (isContinueCounter)
            return;

        isContinueCounter = gameObject.name.ToLowerInvariant().Contains("continue");
    }

    static void RefreshAll()
    {
        for (int i = 0; i < instances.Count; i++)
            instances[i].UpdateUI();
    }
}

public enum KnifeCounterDisplayMode
{
    Gameplay,
    Continue,
    Hidden
}
