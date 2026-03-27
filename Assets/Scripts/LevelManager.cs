using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public int currentLevel = 1;
    public int maxLevel = 5;

    public int knivesPerLevel = 5;
    public int knivesLeft;
    public bool canThrow = true;
    public bool isStageClearing = false; 

    public TargetRotation target;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartLevel();
    }

public void StartLevel()
{
    Debug.Log("Level: " + currentLevel);

    canThrow = true;

    knivesLeft = knivesPerLevel + currentLevel;

    target.speed = Random.Range(150f, 300f) * (Random.value > 0.5f ? 1 : -1);

    if (currentLevel == 5)
    {
        target.speed = Random.Range(300f, 500f);
    }

    KnifeUIManager.instance.totalKnives = knivesLeft;
    KnifeUIManager.instance.ResetUI();

    StageUIManager.instance.UpdateStageUI(currentLevel);
}
public void UseKnife()
{
    if (isStageClearing) return;

    knivesLeft--;

    if (knivesLeft <= 0)
    {
        canThrow = false;
        isStageClearing = true;
        WinLevel();
    }
}

    void WinLevel()
    {
        Debug.Log("WIN LEVEL " + currentLevel);

        canThrow = false;

        if (currentLevel >= maxLevel)
        {
            GameWin();
            return;
        }

        currentLevel++;
        Invoke(nameof(NextLevel), 1f);
    }

void NextLevel()
{
    isStageClearing = false;
    canThrow = true;

    ClearKnives();

    KnifeSpawner spawner = FindObjectOfType<KnifeSpawner>();
    if (spawner != null)
    {
        spawner.ResetSpawner();
    }

    StartLevel();
}

    public void GameOver()
    {
        currentLevel = 1;
        GameManager.instance.GameOver();
    }

    void GameWin()
    {
        GameManager.instance.GameWin();
    }

    void ClearKnives()
    {
        GameObject[] knives = GameObject.FindGameObjectsWithTag("Knife");

        foreach (var k in knives)
        {
            Destroy(k);
        }
    }
}