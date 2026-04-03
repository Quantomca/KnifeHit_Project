using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class TargetRotation : MonoBehaviour
{
    public float speed = 200f;

    [Header("Hit Feedback")]
    [SerializeField] float hitScaleMultiplier = 0.965f;
    [SerializeField] float hitOvershootMultiplier = 1.01f;
    [SerializeField] float hitShrinkDuration = 0.03f;
    [SerializeField] float hitOvershootDuration = 0.045f;
    [SerializeField] float hitRecoverDuration = 0.06f;
    [SerializeField] float hitWhiteBlend = 0.22f;

    public float Radius { get; private set; } = 1.5f;

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private Vector3 baseScale = Vector3.one;
    private Color baseColor = Color.white;
    private Coroutine hitFeedbackRoutine;

    void Awake()
    {
        CacheComponents();
    }

    void Update()
    {
        transform.Rotate(0f, 0f, speed * Time.deltaTime);
    }

    public void Configure(Sprite sprite, float radius)
    {
        CacheComponents();

        if (sprite != null)
            spriteRenderer.sprite = sprite;

        baseColor = spriteRenderer.color;
        SetRadius(radius);
    }

    public void SetRadius(float radius)
    {
        CacheComponents();

        Radius = Mathf.Max(0.1f, radius);

        float spriteRadius = GetSpriteRadius();
        float scale = Radius / spriteRadius;

        baseScale = Vector3.one * scale;
        transform.localScale = baseScale;
        circleCollider.radius = spriteRadius;
    }

    public Vector2 GetSurfacePoint(Vector2 outwardDirection)
    {
        Vector2 dir = outwardDirection.sqrMagnitude > 0f ? outwardDirection.normalized : Vector2.up;
        return (Vector2)transform.position + dir * Radius;
    }

    public Vector2 GetOutwardDirection(Vector2 worldPoint)
    {
        Vector2 dir = worldPoint - (Vector2)transform.position;
        return dir.sqrMagnitude > 0f ? dir.normalized : Vector2.up;
    }

    public void PlayHitFeedback()
    {
        if (hitFeedbackRoutine != null)
            StopCoroutine(hitFeedbackRoutine);

        transform.localScale = baseScale;
        spriteRenderer.color = baseColor;
        hitFeedbackRoutine = StartCoroutine(HitFeedbackRoutine());
    }

    void CacheComponents()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (circleCollider == null)
            circleCollider = GetComponent<CircleCollider2D>();
    }

    float GetSpriteRadius()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return 1f;

        Bounds bounds = spriteRenderer.sprite.bounds;
        float radius = Mathf.Max(bounds.extents.x, bounds.extents.y);
        return radius > 0f ? radius : 1f;
    }

    System.Collections.IEnumerator HitFeedbackRoutine()
    {
        Vector3 shrunkenScale = baseScale * hitScaleMultiplier;
        Vector3 overshootScale = baseScale * hitOvershootMultiplier;
        Color whiteTint = Color.Lerp(baseColor, Color.white, hitWhiteBlend);
        whiteTint.a = baseColor.a;

        float elapsed = 0f;
        while (elapsed < hitShrinkDuration)
        {
            float t = hitShrinkDuration <= 0f ? 1f : elapsed / hitShrinkDuration;
            transform.localScale = Vector3.Lerp(baseScale, shrunkenScale, t);
            spriteRenderer.color = Color.Lerp(baseColor, whiteTint, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < hitOvershootDuration)
        {
            float t = hitOvershootDuration <= 0f ? 1f : elapsed / hitOvershootDuration;
            transform.localScale = Vector3.Lerp(shrunkenScale, overshootScale, t);
            spriteRenderer.color = Color.Lerp(whiteTint, baseColor, t);
 
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < hitRecoverDuration)
        {
            float t = hitRecoverDuration <= 0f ? 1f : elapsed / hitRecoverDuration;
            transform.localScale = Vector3.Lerp(overshootScale, baseScale, t);
            spriteRenderer.color = baseColor;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = baseScale;
        spriteRenderer.color = baseColor;
        hitFeedbackRoutine = null;
    }
}
