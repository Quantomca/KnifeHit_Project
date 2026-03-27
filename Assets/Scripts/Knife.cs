using UnityEngine;
using System.Collections;

public class Knife : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 30f;

    [Header("Shake Effect")]
    [SerializeField] float shakeDuration = 0.15f;
    [SerializeField] float shakeStrength = 5f; // độ lệch góc (độ)
    [SerializeField] int shakeVibrato = 25;    // tần số rung

    private Rigidbody2D rb;

    private bool isThrown = false;
    private bool isStuck = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.simulated = false;
    }

    public void Throw()
    {
        if (isThrown) return;
        if (!LevelManager.instance.canThrow) return;
        isThrown = true;

        rb.simulated = true;
        rb.linearVelocity = Vector2.up * speed;
        KnifeCounterUI.instance.AddKnife();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Target"))
        {
            if (isStuck) return;
            isStuck = true;

            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;

            Transform target = col.transform;

            ContactPoint2D contact = col.contacts[0];
            Vector2 hitPoint = contact.point;

            Vector2 dir = (target.position - (Vector3)hitPoint).normalized;

            transform.up = dir;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            float knifeLength = sr.bounds.size.y;

            float embedPercent = -0.4f;
            float backOffset = knifeLength * embedPercent;

            transform.position = hitPoint - dir * backOffset;

            transform.SetParent(target);
            if (!GameManager.instance.gameOverUI.activeSelf)
            {
                CameraShake.instance.Shake(0.1f, 0.15f);
            }

        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            t.gameObject.layer = LayerMask.NameToLayer("KnifeStuck");
        }
            StartCoroutine(ShakeKnife());
        }

        else if (col.gameObject.CompareTag("Knife"))
        {
            if (!isThrown) return;

            if (col.transform.parent != null)
            {
                GameManager.instance.GameOver();
            }
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

            transform.localRotation = originalRot * Quaternion.Euler(0, 0, angle);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = originalRot;
    }
}