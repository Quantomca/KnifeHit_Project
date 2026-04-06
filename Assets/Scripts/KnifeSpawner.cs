using UnityEngine;
using UnityEngine.InputSystem;

public class KnifeSpawner : MonoBehaviour
{
    public GameObject knifePrefab;
    [SerializeField] float nextKnifeSpawnDelay = 0.01f;

    private GameObject currentKnife;
    private bool canThrow = true;

    void Start()
    {
        if (currentKnife == null)
            Spawn();
    }

    void Update()
    {
        if (GameManager.instance != null && Time.timeScale == 0f)
            return;

        if (!canThrow) return;

        if (WasThrowPressedThisFrame())
        {
            if (currentKnife == null) return;
            if (LevelManager.instance == null) return;
            if (!LevelManager.instance.CanThrowKnife()) return;

            Knife knife = currentKnife.GetComponent<Knife>();
            if (knife == null) return;

            canThrow = false;

            knife.Throw();
            GameAudio.PlayKnifeThrow();

            currentKnife = null;
        }
    }

    public void QueueNextKnife()
    {
        CancelInvoke(nameof(Spawn));

        if (knifePrefab == null)
            return;

        Invoke(nameof(Spawn), nextKnifeSpawnDelay);
    }

    void Spawn()
    {
        if (knifePrefab == null) return;

        currentKnife = Instantiate(knifePrefab, transform.position, Quaternion.identity);
        ParentToGameplayRoot(currentKnife.transform);
        ApplySelectedKnifeAppearance(currentKnife);
        canThrow = true;
    }

    public void ResetSpawner()
    {
        CancelInvoke(nameof(Spawn));

        if (currentKnife != null)
            Destroy(currentKnife);

        currentKnife = null;
        Spawn();
    }

    public void StopSpawning()
    {
        CancelInvoke(nameof(Spawn));
        canThrow = false;

        if (currentKnife != null)
            Destroy(currentKnife);

        currentKnife = null;
    }

    bool WasThrowPressedThisFrame()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        return false;
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

    void ParentToGameplayRoot(Transform knifeTransform)
    {
        if (knifeTransform == null || LevelManager.instance == null)
            return;

        Transform gameplayParent = LevelManager.instance.GetGameplayParent();
        if (gameplayParent != null)
        {
            knifeTransform.SetParent(gameplayParent, true);
            knifeTransform.SetAsLastSibling();
        }
    }
}
