using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    const int OverlayCanvasSortingOrder = 1500;

    public static GameManager instance;

    public GameObject gameOverUI;
    public GameObject gameWinUI;

    GameObject gameContinueUI;
    GameObject levelCounterUI;
    Canvas gameContinueCanvas;
    Canvas gameOverCanvas;
    Image gameContinueBackdrop;
    TMP_Text levelCounterText;
    TMP_Text gameOverStageText;
    TMP_Text gameOverKnifeCounterText;
    Button noThanksButton;
    Button knifeSelectButton;
    GameObject canvasUiRoot;
    GameObject globalAppleHudObject;
    bool hasPlayedLoseSound;

    void Awake()
    {
        instance = this;
        ResolveSceneReferences();
        BindButtons();
        PrepareGameplayUI();
    }

    void Start()
    {
        PrepareGameplayUI();
    }

    public void GameOver(bool freezeTime = true)
    {
        if (freezeTime)
            Time.timeScale = 0f;

        ShowFinalGameOverUI();
    }

    public void GameWin()
    {
        Time.timeScale = 0f;

        if (gameWinUI != null)
            gameWinUI.SetActive(true);
    }

    public void ShowContinueUI()
    {
        ResolveSceneReferences();
        Time.timeScale = 1f;

        ConfigureOverlayCanvas(gameContinueUI, gameContinueCanvas);

        if (gameContinueUI != null)
            gameContinueUI.SetActive(true);

        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        if (levelCounterUI != null)
            levelCounterUI.SetActive(false);

        ConfigureContinueBackdrop();
        BindButtons();
        SetCanvasUiChildrenVisibleForGameplay();
        SetGameplayWorldState(isVisible: true, isPaused: true);
        KnifeCounterUI.SetDisplayMode(KnifeCounterDisplayMode.Continue);
        SetGameplayHudVisible(true);
        RefreshAppleHud();
    }

    public void ShowFinalGameOverUI()
    {
        ResolveSceneReferences();

        if (!hasPlayedLoseSound)
        {
            GameAudio.PlayGameLose();
            hasPlayedLoseSound = true;
        }

        ConfigureOverlayCanvas(gameOverUI, gameOverCanvas);

        if (gameContinueUI != null)
            gameContinueUI.SetActive(false);

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        if (levelCounterUI != null)
            levelCounterUI.SetActive(true);

        RefreshGiftButtons(gameOverUI);

        SetCanvasUiChildrenVisibleForGameOver();
        SetGameplayWorldState(isVisible: false, isPaused: true);
        KnifeCounterUI.SetDisplayMode(KnifeCounterDisplayMode.Hidden);
        SetGameplayHudVisible(false);
        UpdateGameOverStats();
        RefreshAppleHud();
    }

    public void PrepareGameplayUI()
    {
        ResolveSceneReferences();
        Time.timeScale = 1f;
        hasPlayedLoseSound = false;

        if (gameContinueUI != null)
            gameContinueUI.SetActive(false);

        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        if (gameWinUI != null)
            gameWinUI.SetActive(false);

        if (levelCounterUI != null)
            levelCounterUI.SetActive(false);

        RefreshGiftButtons(gameOverUI);
        SetCanvasUiChildrenVisibleForGameplay();
        SetGameplayWorldState(isVisible: true, isPaused: false);
        KnifeCounterUI.SetDisplayMode(KnifeCounterDisplayMode.Gameplay);
        SetGameplayHudVisible(true);
        RefreshAppleHud();
    }

    public void NoThanks()
    {
        Time.timeScale = 0f;
        StopGameplayImmediately();
        ShowFinalGameOverUI();
    }

    void ResolveSceneReferences()
    {
        if (gameContinueUI == null)
        {
            GameObject continueObject = FindSceneObject("GameContinueUI");
            if (continueObject != null)
                gameContinueUI = continueObject;
        }

        if (gameOverUI == null)
        {
            GameObject gameOverObject = FindSceneObject("GameOverUI");
            if (gameOverObject != null)
                gameOverUI = gameOverObject;
        }

        if (gameContinueCanvas == null && gameContinueUI != null)
            gameContinueCanvas = gameContinueUI.GetComponent<Canvas>();

        if (gameOverCanvas == null && gameOverUI != null)
            gameOverCanvas = gameOverUI.GetComponent<Canvas>();

        if (gameContinueBackdrop == null && gameContinueUI != null)
        {
            Transform backdropTransform = FindDirectChild(gameContinueUI.transform, "Image");
            if (backdropTransform != null)
                gameContinueBackdrop = backdropTransform.GetComponent<Image>();
        }

        if (levelCounterUI == null && gameOverUI != null)
        {
            Transform levelCounterTransform = FindChildRecursive(gameOverUI.transform, "LevelCounter");
            if (levelCounterTransform != null)
                levelCounterUI = levelCounterTransform.gameObject;
        }

        if (levelCounterText == null && levelCounterUI != null)
        {
            Transform levelTextTransform = FindChildRecursive(levelCounterUI.transform, "LevelCounterText");
            if (levelTextTransform != null)
                levelCounterText = levelTextTransform.GetComponent<TMP_Text>();
        }

        if (gameOverStageText == null && levelCounterUI != null)
        {
            Transform stageTextTransform = FindChildRecursive(levelCounterUI.transform, "StageText");
            if (stageTextTransform != null)
                gameOverStageText = stageTextTransform.GetComponent<TMP_Text>();
        }

        if (gameOverKnifeCounterText == null && gameOverUI != null)
            ResolveGameOverKnifeCounterText();

        if (noThanksButton == null && gameContinueUI != null)
        {
            Transform noThanksTransform = FindChildRecursive(gameContinueUI.transform, "ButtonNothanks");
            if (noThanksTransform != null)
                noThanksButton = noThanksTransform.GetComponent<Button>();
        }

        if (knifeSelectButton == null && gameOverUI != null)
        {
            Transform knifeButtonTransform = FindChildRecursive(gameOverUI.transform, "ButtonKnife");
            if (knifeButtonTransform != null)
                knifeSelectButton = knifeButtonTransform.GetComponent<Button>();
        }

        if (canvasUiRoot == null)
        {
            GameObject canvasObject = FindSceneObject("CanvasUI");
            if (canvasObject != null)
                canvasUiRoot = canvasObject;
        }

        if (globalAppleHudObject == null && canvasUiRoot != null)
        {
            Transform globalAppleHudTransform = FindChildRecursive(canvasUiRoot.transform, "GlobalAppleHUD");
            if (globalAppleHudTransform != null)
                globalAppleHudObject = globalAppleHudTransform.gameObject;
        }
    }

    void BindButtons()
    {
        if (noThanksButton != null)
        {
            noThanksButton.enabled = true;
            noThanksButton.interactable = true;

            if (noThanksButton.targetGraphic != null)
                noThanksButton.targetGraphic.raycastTarget = true;

            Graphic[] graphics = noThanksButton.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] == noThanksButton.targetGraphic)
                    continue;

                graphics[i].raycastTarget = false;
            }

            noThanksButton.transform.SetAsLastSibling();
            noThanksButton.onClick.RemoveListener(NoThanks);

            if (noThanksButton.onClick.GetPersistentEventCount() == 0)
                noThanksButton.onClick.AddListener(NoThanks);
        }

        if (knifeSelectButton != null)
        {
            knifeSelectButton.onClick.RemoveListener(OpenKnifeSelectScene);
            knifeSelectButton.onClick.AddListener(OpenKnifeSelectScene);
        }
    }

    void OpenKnifeSelectScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(KnifeSelectManager.SceneName);
    }

    void UpdateGameOverStats()
    {
        if (LevelManager.instance != null && levelCounterText != null)
            levelCounterText.text = LevelManager.instance.GetCompletedLevelsInCurrentStage().ToString();

        if (LevelManager.instance != null && gameOverStageText != null)
            gameOverStageText.text = "STAGE " + LevelManager.instance.GetCurrentStage().ToString();

        if (gameOverKnifeCounterText != null)
            gameOverKnifeCounterText.text = "" + KnifeCounterUI.TotalSuccessfulKnives.ToString();
    }

    void SetGameplayHudVisible(bool isVisible)
    {
        if (StageUIManager.instance != null)
            StageUIManager.instance.SetGameplayUIVisible(isVisible);
    }

    void RefreshAppleHud()
    {
        GlobalAppleHUD[] huds = Object.FindObjectsByType<GlobalAppleHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < huds.Length; i++)
            huds[i].RefreshNow();
    }

    void StopGameplayImmediately()
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.CancelInvoke();
            LevelManager.instance.canThrow = false;
            LevelManager.instance.isStageClearing = false;
        }

        KnifeSpawner spawner = Object.FindFirstObjectByType<KnifeSpawner>();
        if (spawner != null)
            spawner.StopSpawning();
    }

    void ConfigureContinueBackdrop()
    {
        if (gameContinueBackdrop == null)
            return;

        gameContinueBackdrop.enabled = true;
        gameContinueBackdrop.raycastTarget = true;
    }

    void ConfigureOverlayCanvas(GameObject overlayObject, Canvas overlayCanvas)
    {
        if (overlayObject == null)
            return;

        if (overlayCanvas == null)
            overlayCanvas = overlayObject.GetComponent<Canvas>();

        if (overlayCanvas != null)
        {
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = OverlayCanvasSortingOrder;
        }

        GraphicRaycaster raycaster = overlayObject.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
            raycaster.enabled = true;

        CanvasGroup canvasGroup = overlayObject.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    void SetGameplayWorldState(bool isVisible, bool isPaused)
    {
        if (LevelManager.instance == null)
            return;

        LevelManager.instance.SetGameplayPaused(isPaused);
        LevelManager.instance.SetGameplayWorldVisible(isVisible);
    }

    void SetCanvasUiChildrenVisibleForGameplay()
    {
        if (canvasUiRoot == null)
            return;

        for (int i = 0; i < canvasUiRoot.transform.childCount; i++)
            canvasUiRoot.transform.GetChild(i).gameObject.SetActive(true);
    }

    void SetCanvasUiChildrenVisibleForGameOver()
    {
        if (canvasUiRoot == null)
            return;

        for (int i = 0; i < canvasUiRoot.transform.childCount; i++)
        {
            GameObject childObject = canvasUiRoot.transform.GetChild(i).gameObject;
            bool shouldRemainVisible = childObject == globalAppleHudObject;
            childObject.SetActive(shouldRemainVisible);
        }
    }

    void RefreshGiftButtons(GameObject rootObject)
    {
        if (rootObject == null)
            return;

        GiftButtonController[] giftButtons = rootObject.GetComponentsInChildren<GiftButtonController>(true);
        for (int i = 0; i < giftButtons.Length; i++)
            giftButtons[i].RefreshNow();
    }

    void ResolveGameOverKnifeCounterText()
    {
        if (gameOverUI == null)
            return;

        Transform knifeCounterTransform = FindChildRecursive(gameOverUI.transform, "KnifeCounterText");
        if (knifeCounterTransform == null)
            knifeCounterTransform = FindChildRecursive(gameOverUI.transform, "KnifeCountText");

        if (knifeCounterTransform != null)
        {
            gameOverKnifeCounterText = knifeCounterTransform.GetComponent<TMP_Text>();
            if (gameOverKnifeCounterText != null)
                return;
        }

        CreateGameOverKnifeCounterText();
    }

    void CreateGameOverKnifeCounterText()
    {
        Transform parentTransform = levelCounterUI != null ? levelCounterUI.transform : gameOverUI.transform;
        if (parentTransform == null)
            return;

        GameObject knifeCounterObject = new GameObject("KnifeCounterText", typeof(RectTransform));
        RectTransform knifeCounterRect = knifeCounterObject.GetComponent<RectTransform>();
        knifeCounterRect.SetParent(parentTransform, false);
        knifeCounterRect.anchorMin = new Vector2(0.5f, 0.5f);
        knifeCounterRect.anchorMax = new Vector2(0.5f, 0.5f);
        knifeCounterRect.pivot = new Vector2(0.5f, 0.5f);
        knifeCounterRect.anchoredPosition = new Vector2(-18f, -210f);
        knifeCounterRect.sizeDelta = new Vector2(520f, 84f);

        TextMeshProUGUI knifeCounterTmp = knifeCounterObject.AddComponent<TextMeshProUGUI>();
        TMP_Text styleSource = gameOverStageText != null ? gameOverStageText : levelCounterText;
        if (styleSource != null)
        {
            knifeCounterTmp.font = styleSource.font;
            knifeCounterTmp.fontSharedMaterial = styleSource.fontSharedMaterial;
        }

        knifeCounterTmp.text = "0";
        knifeCounterTmp.fontSize = 72f;
        knifeCounterTmp.fontStyle = FontStyles.Bold;
        knifeCounterTmp.color = new Color(0.8039216f, 0.63529414f, 0f, 1f);
        knifeCounterTmp.alignment = TextAlignmentOptions.Center;
        knifeCounterTmp.enableWordWrapping = false;
        knifeCounterTmp.raycastTarget = false;

        gameOverKnifeCounterText = knifeCounterTmp;
    }

    GameObject FindSceneObject(string objectName)
    {
        Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == objectName)
                return transforms[i].gameObject;
        }

        return null;
    }

    Transform FindChildRecursive(Transform root, string objectName)
    {
        if (root == null)
            return null;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == objectName)
                return child;

            Transform nestedChild = FindChildRecursive(child, objectName);
            if (nestedChild != null)
                return nestedChild;
        }

        return null;
    }

    Transform FindDirectChild(Transform root, string objectName)
    {
        if (root == null)
            return null;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == objectName)
                return child;
        }

        return null;
    }
}
