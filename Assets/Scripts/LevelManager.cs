using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    const int GameplayBackgroundSortingOrder = -1000;

    [Header("Level Data")]
    public List<LevelData> levels;

    [Header("Prefabs")]
    public GameObject targetPrefab;

    [FormerlySerializedAs("knifePrefab")]
    public GameObject stuckKnifePrefab;

    [Header("Apples")]
    [SerializeField] float appleSurfaceOffset = 0.08f;

    [Header("Runtime")]
    public int currentLevel = 1;
    public int currentStage = 1;
    public int maxLevel = 5;

    public int knivesLeft;
    public bool canThrow = true;
    public bool isStageClearing = false;

    [Header("Apple Runtime")]
    public int applesLeft;

    [SerializeField] Vector3 targetSpawnPosition = new Vector3(0f, 1.2f, 0f);

    public static LevelManager instance;
    public TargetRotation target;

    Transform gameplayParent;
    SpriteRenderer gameplayBackgroundRenderer;
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
        ClearApples();
        isStageClearing = false;
        canThrow = true;

        if (GameManager.instance != null)
            GameManager.instance.PrepareGameplayUI();

        ResetSpawner();
        DestroyCurrentTarget();
        ResolveGameplayParent();

        LevelData data = GetCurrentLevelData();
        if (data == null || targetPrefab == null)
            return;

        knivesLeft = Mathf.Max(0, data.knivesCount);
        knivesRemainingToStick = knivesLeft;

        GameObject newTarget = Instantiate(targetPrefab, targetSpawnPosition, Quaternion.identity);
        ParentToGameplayRoot(newTarget.transform);
        target = newTarget.GetComponent<TargetRotation>();

        if (target == null)
            return;

        target.speed = ResolveRotationSpeed(data);
        target.Configure(data.targetSprite, data.targetRadius);

        ConfigureBreakPieces(newTarget, data);

        if (KnifeUIManager.instance != null)
        {
            KnifeUIManager.instance.totalKnives = knivesLeft;
            KnifeUIManager.instance.ResetUI();
        }

        if (KnifeCounterUI.instance != null)
            KnifeCounterUI.instance.ResetCounter();

        if (StageUIManager.instance != null)
            StageUIManager.instance.UpdateStageUI(currentStage, currentLevel);

        SpawnPreStuckKnives(data.stuckKnifeAngles);
        SpawnApples(data);
    }

    public void OnKnifeThrown()
    {
        if (isStageClearing || !canThrow)
            return;

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
        thrownKnife.PlaceAsStuck(target, outwardDirection, true);

        target.PlayHitFeedback();
        GameAudio.PlayKnifeHitWood();
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
        GameAudio.PlayKnifeHitKnife();

        KnifeSpawner spawner = FindFirstObjectByType<KnifeSpawner>();
        if (spawner != null)
            spawner.StopSpawning();

        if (thrownKnife != null)
            thrownKnife.DropOnKnifeCollision();

        ShowContinueUI();
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
        ShowFinalGameOverUI();
    }

    public void HandleContinueDeclined()
    {
        canThrow = false;
        ShowFinalGameOverUI();
    }

    public void ContinueAfterRewardedAd()
    {
        if (isStageClearing)
            return;

        if (knivesLeft <= 0)
        {
            knivesLeft = 1;

            if (KnifeUIManager.instance != null)
                KnifeUIManager.instance.AddReserveKnives(1);
        }

        canThrow = true;
        CancelInvoke();
        GameAudio.PlayContinueReward();

        if (GameManager.instance != null)
            GameManager.instance.PrepareGameplayUI();

        KnifeSpawner spawner = FindFirstObjectByType<KnifeSpawner>();
        if (spawner != null)
            spawner.ResetSpawner();
    }

    public void NotifyAppleCollected()
    {
        applesLeft = Mathf.Max(0, applesLeft - 1);
    }

    public int GetCurrentStage()
    {
        return Mathf.Max(1, currentStage);
    }

    public int GetCompletedLevelsInCurrentStage()
    {
        return Mathf.Clamp(currentLevel - 1, 0, GetConfiguredLevelCount() - 1);
    }

    public Transform GetGameplayParent()
    {
        ResolveGameplayParent();
        return gameplayParent;
    }

    public void SetGameplayPaused(bool isPaused)
    {
        if (target != null)
            target.SetRotationPaused(isPaused);
    }

    public void SetGameplayWorldVisible(bool isVisible)
    {
        ResolveGameplayParent();

        if (gameplayParent != null)
            gameplayParent.gameObject.SetActive(isVisible);

        if (gameplayBackgroundRenderer != null)
            gameplayBackgroundRenderer.enabled = isVisible;
    }

    void SpawnPreStuckKnives(List<float> angles)
    {
        if (target == null || angles == null || angles.Count == 0)
            return;

        foreach (float angle in angles)
        {
            Vector2 outwardDirection = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
            SpawnStuckKnife(outwardDirection, false, false);
        }
    }

    void SpawnApples(LevelData data)
    {
        if (target == null)
            return;

        List<float> appleAngles = ResolveAppleAngles(data);
        applesLeft = 0;

        foreach (float angle in appleAngles)
        {
            Vector2 outwardDirection = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
            AppleCollectible apple = AppleCollectible.CreateRuntimeApple(target, outwardDirection, appleSurfaceOffset);

            if (apple != null)
                applesLeft++;
        }
    }

    Knife SpawnStuckKnife(Vector2 outwardDirection, bool playImpactFeedback, bool useSelectedAppearance)
    {
        if (target == null || stuckKnifePrefab == null)
            return null;

        GameObject knifeObject = Instantiate(stuckKnifePrefab);
        if (useSelectedAppearance)
            ApplySelectedKnifeAppearance(knifeObject);

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
        if (isStageClearing)
            return;

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

    List<float> ResolveAppleAngles(LevelData data)
    {
        List<float> appleAngles = new List<float>();

        if (data == null)
            return appleAngles;

        if (data.appleAngles != null)
        {
            foreach (float angle in data.appleAngles)
                appleAngles.Add(angle);
        }

        if (appleAngles.Count > 0)
            return appleAngles;

        int automaticAppleCount = Mathf.Max(0, data.appleCount);
        if (automaticAppleCount <= 0)
            return appleAngles;

        float startAngle = Random.Range(0f, 360f);
        float angleStep = 360f / automaticAppleCount;

        for (int index = 0; index < automaticAppleCount; index++)
            appleAngles.Add(startAngle + (angleStep * index));

        return appleAngles;
    }

    LevelData GetCurrentLevelData()
    {
        if (levels == null || levels.Count == 0)
            return null;

        currentLevel = Mathf.Clamp(currentLevel, 1, GetConfiguredLevelCount());
        return levels[currentLevel - 1];
    }

    int GetConfiguredLevelCount()
    {
        if (levels == null || levels.Count == 0)
            return 1;

        int configuredMaxLevel = maxLevel > 0 ? maxLevel : levels.Count;
        return Mathf.Max(1, Mathf.Min(configuredMaxLevel, levels.Count));
    }

    void AfterBreak()
    {
        if (levels == null || levels.Count == 0)
        {
            currentStage++;
            currentLevel = 1;
            StartLevel();
            return;
        }

        bool reachedBossLevel = currentLevel >= GetConfiguredLevelCount();

        if (reachedBossLevel)
        {
            currentStage++;
            currentLevel = 1;

            if (StageUIManager.instance != null)
                StageUIManager.instance.ResetForNewStage(currentStage);
        }
        else
        {
            currentLevel++;
        }

        StartLevel();
    }

    void ShowContinueUI()
    {
        if (GameManager.instance != null)
            GameManager.instance.ShowContinueUI();
    }

    void ShowFinalGameOverUI()
    {
        if (GameManager.instance != null)
            GameManager.instance.ShowFinalGameOverUI();
    }

    void ResetSpawner()
    {
        KnifeSpawner spawner = FindFirstObjectByType<KnifeSpawner>();
        if (spawner != null)
            spawner.ResetSpawner();
    }

    void DestroyCurrentTarget()
    {
        if (target == null)
            return;

        Destroy(target.gameObject);
        target = null;
    }

    void ClearKnives()
    {
        Knife[] knives = FindObjectsByType<Knife>(FindObjectsSortMode.None);

        foreach (Knife knife in knives)
            Destroy(knife.gameObject);
    }

    void ClearApples()
    {
        AppleCollectible[] apples = FindObjectsByType<AppleCollectible>(FindObjectsSortMode.None);

        foreach (AppleCollectible apple in apples)
            Destroy(apple.gameObject);
    }

    void ApplySelectedKnifeAppearance(GameObject knifeObject)
    {
        if (knifeObject == null)
            return;

        KnifeData selectedKnife = KnifeDatabase.GetSelectedKnife();
        if (selectedKnife == null || selectedKnife.gameplaySprite == null)
            return;

        Knife knife = knifeObject.GetComponent<Knife>();
        if (knife != null)
        {
            knife.SetAppearance(selectedKnife.gameplaySprite);
            return;
        }

        SpriteRenderer spriteRenderer = knifeObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.sprite = selectedKnife.gameplaySprite;
    }

    void ResolveGameplayParent()
    {
        if (gameplayParent == null)
        {
            GameObject gameplayRootObject = GameObject.Find("GameplayWorldRoot");
            if (gameplayRootObject == null)
                gameplayRootObject = new GameObject("GameplayWorldRoot");

            gameplayParent = gameplayRootObject.transform;
        }

        EnsureGameplayBackground();
    }

    void ParentToGameplayRoot(Transform targetTransform)
    {
        if (targetTransform == null)
            return;

        ResolveGameplayParent();

        if (gameplayParent != null)
        {
            targetTransform.SetParent(gameplayParent, true);
            targetTransform.SetAsLastSibling();
        }
    }

    void EnsureGameplayBackground()
    {
        Image backgroundImage = FindCanvasBackgroundImage();
        if (backgroundImage == null || backgroundImage.sprite == null)
            return;

        backgroundImage.raycastTarget = false;
        backgroundImage.enabled = false;

        if (gameplayBackgroundRenderer == null)
        {
            GameObject backgroundObject = GameObject.Find("GameplayBackground");
            if (backgroundObject == null)
                backgroundObject = new GameObject("GameplayBackground", typeof(SpriteRenderer));

            gameplayBackgroundRenderer = backgroundObject.GetComponent<SpriteRenderer>();
        }

        gameplayBackgroundRenderer.sprite = backgroundImage.sprite;
        gameplayBackgroundRenderer.color = backgroundImage.color;
        gameplayBackgroundRenderer.sortingLayerID = 0;
        gameplayBackgroundRenderer.sortingOrder = GameplayBackgroundSortingOrder;

        FitBackgroundToCamera(gameplayBackgroundRenderer);
    }

    Image FindCanvasBackgroundImage()
    {
        GameObject canvasUiObject = GameObject.Find("CanvasUI");
        if (canvasUiObject == null)
            return null;

        for (int i = 0; i < canvasUiObject.transform.childCount; i++)
        {
            Transform child = canvasUiObject.transform.GetChild(i);
            Image image = child.GetComponent<Image>();
            RectTransform rectTransform = child as RectTransform;

            if (image == null || rectTransform == null)
                continue;

            if (rectTransform.anchorMin != Vector2.zero || rectTransform.anchorMax != Vector2.one)
                continue;

            return image;
        }

        return null;
    }

    void FitBackgroundToCamera(SpriteRenderer backgroundRenderer)
    {
        if (backgroundRenderer == null || backgroundRenderer.sprite == null)
            return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        Transform backgroundTransform = backgroundRenderer.transform;
        backgroundTransform.SetParent(null, true);

        Vector3 cameraPosition = mainCamera.transform.position;
        backgroundTransform.position = new Vector3(cameraPosition.x, cameraPosition.y, 0f);

        Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            return;

        if (mainCamera.orthographic)
        {
            float worldHeight = mainCamera.orthographicSize * 2f;
            float worldWidth = worldHeight * mainCamera.aspect;
            float scale = Mathf.Max(worldWidth / spriteSize.x, worldHeight / spriteSize.y);
            backgroundTransform.localScale = new Vector3(scale, scale, 1f);
            return;
        }

        backgroundTransform.localScale = Vector3.one;
    }
}
