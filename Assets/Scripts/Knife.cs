using UnityEngine;
using System.Collections;

public class Knife : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 60f;

    [Header("Shake Effect")]
    [SerializeField] float shakeDuration = 0.15f;
    [SerializeField] float shakeStrength = 5f;
    [SerializeField] int shakeVibrato = 25;

    private Rigidbody2D rb;

    private bool isThrown = false;
    private bool isStuck = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // ⚠️ luôn bật simulated để trigger hoạt động
        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Throw()
    {
        if (isThrown) return;
        if (!LevelManager.instance.canThrow) return;

        isThrown = true;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.up * speed;

        KnifeCounterUI.instance.AddKnife();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
    if (!isThrown && !isStuck) return;

        // 💀 Va vào target = chạm
        if (col.CompareTag("Target")){
            StickToTarget(col.transform, col);
        }

        // 💀 Va vào dao khác = thua
        else if (col.CompareTag("Knife"))
        {
            if (!isThrown) return;
            if (!LevelManager.instance.canThrow) return;

            GameManager.instance.GameOver();
        }
    }

    void StickToTarget(Transform target, Collider2D col)
    {
        if (isStuck) return;
        isStuck = true;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // 🎯 điểm chạm
        Vector2 hitPoint = col.ClosestPoint(transform.position);

        // 🎯 hướng về tâm
        Vector2 dir = (target.position - (Vector3)hitPoint).normalized;

        // 🎯 xoay dao
        transform.up = dir;

        // 🎯 độ cắm
        float knifeLength = GetComponent<SpriteRenderer>().bounds.size.y;
        float backOffset = knifeLength * -0.4f;

        transform.position = hitPoint - dir * backOffset;

        // 🎯 gắn vào target
        transform.SetParent(target);

        // 🎯 chuyển layer thành dao đã cắm
        gameObject.layer = LayerMask.NameToLayer("KnifeStuck");

        // 🎯 hiệu ứng
        if (!GameManager.instance.gameOverUI.activeSelf)
        {
            CameraShake.instance.Shake(0.1f, 0.15f);
        }

        StartCoroutine(ShakeKnife());
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

            transform.localRotation = originalRot * Quaternion.Euler(0, 0, angle);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = originalRot;
    }
}