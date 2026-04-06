using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Knife : MonoBehaviour
{
    const string KnifeTag = "Knife";
    const string StuckKnifeLayer = "KnifeStuck";
    static readonly Vector2 FixedColliderSize = new Vector2(0.28f, 2.56f);
    const int KnifeSortingOrder = 110;

    [Header("Movement")]
    public float speed = 40f;

    [Header("Stuck Pose")]
    [FormerlySerializedAs("stuckEmbedPercent")]
    [SerializeField] float stuckPenetrationPercent = 0.3f;
    [SerializeField] float tipInsetPercent = 0.06f;

    [Header("Shake Effect")]
    [SerializeField] float shakeDuration = 0.06f;
    [SerializeField] float shakeStrength = 1.5f;
    [SerializeField] int shakeVibrato = 40;

    [Header("Fail Drop")]
    [SerializeField] float failDropGravity = 2.8f;
    [SerializeField] float failDropVerticalSpeed = -2.5f;
    [SerializeField] float failDropHorizontalSpeed = 1.25f;
    [SerializeField] float failDropAngularSpeed = 540f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D knifeCollider;
    private float defaultGravityScale;
    private int defaultLayer;

    private bool isThrown;
    private bool isStuck;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        knifeCollider = GetComponent<Collider2D>();
        defaultGravityScale = rb.gravityScale;
        defaultLayer = gameObject.layer;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        gameObject.tag = KnifeTag;

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = KnifeSortingOrder;

        RefreshColliderToSprite();
    }

    public void Throw()
    {
        if (isThrown) return;
        if (LevelManager.instance == null || !LevelManager.instance.canThrow) return;

        isThrown = true;
        isStuck = false;

        if (knifeCollider != null)
            knifeCollider.enabled = true;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = defaultGravityScale;
        rb.linearVelocity = Vector2.up * speed;
        rb.angularVelocity = 0f;
        gameObject.layer = defaultLayer;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        HandleTrigger(col);
    }

    void OnTriggerStay2D(Collider2D col)
    {
        HandleTrigger(col);
    }

    public void PlaceAsStuck(TargetRotation target, Vector2 outwardDirection, bool playImpactFeedback)
    {
        if (target == null) return;

        Vector2 dir = outwardDirection.sqrMagnitude > 0f ? outwardDirection.normalized : Vector2.up;
        Vector2 surfacePoint = target.GetSurfacePoint(dir);

        isThrown = false;
        isStuck = true;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (knifeCollider != null)
            knifeCollider.enabled = true;

        transform.up = -dir;

        float knifeLength = GetKnifeLengthWorld();
        float penetrationDepth = knifeLength * stuckPenetrationPercent;

        Vector2 desiredTipPosition = surfacePoint - (dir * penetrationDepth);
        Vector2 worldTipOffset = transform.TransformVector(GetLocalTipOffset());
        transform.position = desiredTipPosition - worldTipOffset;
        transform.SetParent(target.transform, true);
        transform.SetAsFirstSibling();

        SpriteRenderer targetRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && targetRenderer != null)
        {
            spriteRenderer.sortingLayerID = targetRenderer.sortingLayerID;
            spriteRenderer.sortingOrder = targetRenderer.sortingOrder - 1;
        }

        int stuckLayer = LayerMask.NameToLayer(StuckKnifeLayer);
        if (stuckLayer >= 0)
            gameObject.layer = stuckLayer;

        gameObject.tag = KnifeTag;

        if (playImpactFeedback)
            PlayImpactFeedback();
    }

    public void DisableBeforeDestroy()
    {
        isThrown = false;

        if (knifeCollider != null)
            knifeCollider.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void DropOnKnifeCollision()
    {
        isThrown = false;
        isStuck = false;

        Transform gameplayParent = LevelManager.instance != null ? LevelManager.instance.GetGameplayParent() : null;
        transform.SetParent(gameplayParent, true);
        transform.SetAsLastSibling();

        if (knifeCollider != null)
            knifeCollider.enabled = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = failDropGravity;

        float horizontalDirection = transform.position.x >= 0f ? 1f : -1f;
        rb.linearVelocity = new Vector2(horizontalDirection * failDropHorizontalSpeed, failDropVerticalSpeed);
        rb.angularVelocity = -horizontalDirection * failDropAngularSpeed;

        gameObject.layer = defaultLayer;

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = KnifeSortingOrder;
    }

    public bool HasReachedTargetSurface(TargetRotation target)
    {
        if (target == null || spriteRenderer == null)
            return false;

        Vector2 offset = GetTipPosition() - (Vector2)target.transform.position;
        return offset.sqrMagnitude <= target.Radius * target.Radius;
    }

    public Vector2 GetTipPosition()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return transform.position;

        return transform.TransformPoint(GetLocalTipOffset());
    }

    public void SetAppearance(Sprite sprite)
    {
        if (spriteRenderer == null || sprite == null)
            return;

        spriteRenderer.sprite = sprite;
        RefreshColliderToSprite();
    }

    void PlayImpactFeedback()
    {
        StartCoroutine(ShakeKnife());
    }

    void HandleTrigger(Collider2D col)
    {
        if (col == null) return;
        if (isStuck) return;
        if (!isThrown) return;
        if (LevelManager.instance == null || !LevelManager.instance.canThrow) return;

        AppleCollectible apple = col.GetComponent<AppleCollectible>();
        if (apple != null)
        {
            apple.Collect();
            return;
        }

        if (col.CompareTag("Target"))
        {
            TargetRotation target = col.GetComponent<TargetRotation>();
            if (target == null || !HasReachedTargetSurface(target))
                return;

            LevelManager.instance.HandleThrownKnifeHit(this, col);
        }
        else if (col.CompareTag(KnifeTag))
        {
            LevelManager.instance.FailKnifeCollision(this);
        }
    }

    IEnumerator ShakeKnife()
    {
        float elapsed = 0f;
        Quaternion originalRot = transform.localRotation;

        while (elapsed < shakeDuration)
        {
            float progress = elapsed / shakeDuration;
            float damper = 1f - progress;
            float angle = Mathf.Sin(elapsed * shakeVibrato) * shakeStrength * damper;

            transform.localRotation = originalRot * Quaternion.Euler(0f, 0f, angle);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = originalRot;
    }

    float GetKnifeLengthWorld()
    {
        return spriteRenderer != null ? spriteRenderer.bounds.size.y : 1f;
    }

    Vector2 GetLocalTipOffset()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return Vector2.up * 0.5f;

        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        float tipY = spriteBounds.center.y + (spriteBounds.extents.y * (1f - tipInsetPercent));
        return new Vector2(spriteBounds.center.x, tipY);
    }

    void RefreshColliderToSprite()
    {
        BoxCollider2D boxCollider = knifeCollider as BoxCollider2D;
        if (boxCollider == null)
            return;

        boxCollider.offset = Vector2.zero;
        boxCollider.size = FixedColliderSize;
    }
}
