using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class AppleCollectible : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    CircleCollider2D circleCollider;
    bool collected;

    void Awake()
    {
        CacheComponents();
    }

    public static AppleCollectible CreateRuntimeApple(TargetRotation target, Vector2 outwardDirection, float extraOffset)
    {
        if (target == null)
            return null;

        Sprite appleSprite = AppleConfigLoader.AppleSprite;
        if (appleSprite == null)
            return null;

        GameObject appleObject = new GameObject("Apple", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(AppleCollectible));
        AppleCollectible apple = appleObject.GetComponent<AppleCollectible>();
        apple.Initialize(
            target,
            outwardDirection,
            appleSprite,
            AppleConfigLoader.WorldScale,
            extraOffset + AppleConfigLoader.SurfaceOffset);

        return apple;
    }

    public void Initialize(TargetRotation target, Vector2 outwardDirection, Sprite sprite, float worldScale, float extraOffset)
    {
        CacheComponents();

        spriteRenderer.sprite = sprite;
        transform.localScale = Vector3.one * Mathf.Max(0.1f, worldScale);

        circleCollider.isTrigger = true;
        circleCollider.radius = 0.35f;

        Vector2 direction = outwardDirection.sqrMagnitude > 0f ? outwardDirection.normalized : Vector2.up;
        Vector2 worldPosition = target.GetSurfacePoint(direction) + direction * Mathf.Max(0f, extraOffset);

        transform.position = worldPosition;
        transform.rotation = Quaternion.identity;
        transform.SetParent(target.transform, true);

        SpriteRenderer targetRenderer = target.GetComponent<SpriteRenderer>();
        if (targetRenderer != null)
        {
            spriteRenderer.sortingLayerID = targetRenderer.sortingLayerID;
            spriteRenderer.sortingOrder = targetRenderer.sortingOrder + 1;
        }
    }

    public bool Collect()
    {
        if (collected)
            return false;

        collected = true;

        if (circleCollider != null)
            circleCollider.enabled = false;

        AppleWallet.AddApples(1);

        if (LevelManager.instance != null)
            LevelManager.instance.NotifyAppleCollected();

        Destroy(gameObject);
        return true;
    }

    void CacheComponents()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (circleCollider == null)
            circleCollider = GetComponent<CircleCollider2D>();
    }

    float GetSpriteRadiusWorld()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return 0.15f;

        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        float localRadius = Mathf.Max(spriteBounds.extents.x, spriteBounds.extents.y);
        float scale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
        return localRadius * scale;
    }
}
