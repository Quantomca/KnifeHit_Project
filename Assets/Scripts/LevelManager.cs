using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Level Data")]
    public List<LevelData> levels;

    [Header("Prefabs")]
    public GameObject targetPrefab;

    [FormerlySerializedAs("knifePrefab")]
    public GameObject stuckKnifePrefab;

    [Header("Runtime")]
    public int currentLevel = 1;
    public int maxLevel = 5;

    public int knivesLeft;
    public bool canThrow = true;
    public bool isStageClearing = false;

    [SerializeField] Vector3 targetSpawnPosition = new Vector3(0f, 1.2f, 0f);

    public static LevelManager instance;
    public TargetRotation target;

    int knivesRemainingToStick;

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
        CancelInvoke();
        ClearKnives();

        isStageClearing = false;
        canThrow = true;

        ResetSpawner();
        DestroyCurrentTarget();

        LevelData data = GetCurrentLevelData();
        if (data == null || targetPrefab == null)
            return;

        knivesLeft = Mathf.Max(0, data.knivesCount);
        knivesRemainingToStick = knivesLeft;

        GameObject newTarget = Instantiate(targetPrefab, targetSpawnPosition, Quaternion.identity);
        target = newTarget.GetComponent<TargetRotation>();

        if (target == null)
            return;

        target.speed = ResolveRotationSpeed(data);
        target.Configure(data.targetSprite, data.targetRadius);

        ConfigureBreakPieces(newTarget, data);

        KnifeUIManager.instance.totalKnives = knivesLeft;
        KnifeUIManager.instance.ResetUI();
        StageUIManager.instance.UpdateStageUI(currentLevel);

        SpawnPreStuckKnives(data.stuckKnifeAngles);
    }

    public void OnKnifeThrown()
    {
        if (isStageClearing || !canThrow) return;

        knivesLeft = Mathf.Max(0, knivesLeft - 1);
    }

    public void HandleThrownKnifeHit(Knife thrownKnife, Collider2D targetCollider)
    {
        if (thrownKnife == null || target == null || isStageClearing)
            return;

        if (!thrownKnife.HasReachedTargetSurface(target))
            return;

        Vector2 impactPoint = thrownKnife.GetTipPosition();

        Vector2 outwardDirection = target.GetOutwardDirection(impactPoint);

        Knife stuckKnife = SpawnStuckKnife(outwardDirection, true);

        if (stuckKnife != null)
        {
            thrownKnife.DisableBeforeDestroy();
            Destroy(thrownKnife.gameObject);
        }
        else
        {
            thrownKnife.PlaceAsStuck(target, outwardDirection, true);
        }

        target.PlayHitFeedback();
        OnKnifeStuck();
    }

    public bool CanThrowKnife()
    {
        return canThrow && !isStageClearing && knivesLeft > 0;
    }

    public bool ShouldSpawnNextKnife()
    {
        return canThrow && !isStageClearing && knivesLeft > 0;
    }

    public void FailKnifeCollision(Knife thrownKnife)
    {
        if (!canThrow || isStageClearing)
            return;

        canThrow = false;

        KnifeSpawner spawner = FindFirstObjectByType<KnifeSpawner>();
        if (spawner != null)
            spawner.StopSpawning();

        if (thrownKnife != null)
            thrownKnife.DropOnKnifeCollision();

        Invoke(nameof(ShowGameOverUIWithoutFreeze), 0.3f);
    }

    public void WinLevel()
    {
        canThrow = false;

        if (target != null)
        {
            TargetBreak targetBreak = target.GetComponent<TargetBreak>();
            if (targetBreak != null)
                targetBreak.Break();
        }

        target = null;
        Invoke(nameof(AfterBreak), 2f);
    }

    public void GameOver()
    {
        canThrow = false;

        if (target != null)
        {
            TargetBreak targetBreak = target.GetComponent<TargetBreak>();
            if (targetBreak != null)
                targetBreak.Break();
        }

        target = null;
        Invoke(nameof(RestartGame), 0.6f);
    }

    void SpawnPreStuckKnives(List<float> angles)
    {
        if (target == null || angles == null || angles.Count == 0)
            return;

        foreach (float angle in angles)
        {
            Vector2 outwardDirection = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
            SpawnStuckKnife(outwardDirection, false);
        }
    }

    Knife SpawnStuckKnife(Vector2 outwardDirection, bool playImpactFeedback)
    {
        if (target == null || stuckKnifePrefab == null)
            return null;

        GameObject knifeObject = Instantiate(stuckKnifePrefab);
        Knife knife = knifeObject.GetComponent<Knife>();

        if (knife == null)
        {
            Destroy(knifeObject);
            return null;
        }

        knife.PlaceAsStuck(target, outwardDirection, playImpactFeedback);
        return knife;
    }

    void OnKnifeStuck()
    {
        if (isStageClearing) return;

        knivesRemainingToStick = Mathf.Max(0, knivesRemainingToStick - 1);

        if (knivesRemainingToStick <= 0)
        {
            isStageClearing = true;
            canThrow = false;
            Invoke(nameof(WinLevel), 0.12f);
        }
    }

    void ConfigureBreakPieces(GameObject targetObject, LevelData data)
    {
        TargetBreak targetBreak = targetObject.GetComponent<TargetBreak>();
        if (targetBreak == null || data == null)
            return;

        if (data.breakPieces != null && data.breakPieces.Length > 0)
            targetBreak.piecePrefabs = data.breakPieces;
    }

    float ResolveRotationSpeed(LevelData data)
    {
        float speed = data.rotationSpeed;

        if (data.randomDirection)
            speed *= Random.value > 0.5f ? 1f : -1f;

        if (data.isBoss)
            speed *= 1.3f;

        return speed;
    }

    LevelData GetCurrentLevelData()
    {
        if (levels == null || levels.Count == 0)
            return null;

        int configuredMaxLevel = maxLevel > 0 ? maxLevel : levels.Count;
        int highestLevel = Mathf.Max(1, Mathf.Min(configuredMaxLevel, levels.Count));
        currentLevel = Mathf.Clamp(currentLevel, 1, highestLevel);

        return levels[currentLevel - 1];
    }

    void AfterBreak()
    {
        if (levels == null || levels.Count == 0)
        {
            GameWin();
            return;
        }

        int configuredMaxLevel = maxLevel > 0 ? maxLevel : levels.Count;
        int highestLevel = Mathf.Max(1, Mathf.Min(configuredMaxLevel, levels.Count));

        if (currentLevel >= highestLevel)
        {
            GameWin();
            return;
        }

        currentLevel++;
        StartLevel();
    }

    void RestartGame()
    {
        currentLevel = 1;
        GameManager.instance.GameOver();
    }

    void ShowGameOverUIWithoutFreeze()
    {
        if (GameManager.instance != null)
            GameManager.instance.GameOver(false);
    }

    void GameWin()
    {
        GameManager.instance.GameWin();
    }

    void ResetSpawner()
    {
        KnifeSpawner spawner = FindFirstObjectByType<KnifeSpawner>();
        if (spawner != null)
            spawner.ResetSpawner();
    }

    void DestroyCurrentTarget()
    {
        if (target == null) return;

        Destroy(target.gameObject);
        target = null;
    }

    void ClearKnives()
    {
        Knife[] knives = FindObjectsByType<Knife>(FindObjectsSortMode.None);

        foreach (Knife knife in knives)
            Destroy(knife.gameObject);
    }
}
