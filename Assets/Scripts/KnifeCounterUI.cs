using UnityEngine;
using TMPro; // nếu dùng TextMeshPro

public class KnifeCounterUI : MonoBehaviour
{
    public static KnifeCounterUI instance;

    [SerializeField] private TextMeshProUGUI counterText;

    private int totalThrown = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddKnife()
    {
        totalThrown++;
        UpdateUI();
    }

    void UpdateUI()
    {
        counterText.text = totalThrown.ToString();
    }

    public void ResetCounter()
    {
        totalThrown = 0;
        UpdateUI();
    }
}