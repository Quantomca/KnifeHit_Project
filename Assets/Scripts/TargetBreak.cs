using UnityEngine;

public class TargetBreak : MonoBehaviour
{
    [Header("Pieces")]
    public GameObject[] piecePrefabs;

    [Header("Force")]
    public float minForce = 3f;
    public float maxForce = 7f;
    public float torque = 400f;

    [Header("Effect")]
    public GameObject hitEffect;
    public AudioSource breakSound;

    public float pieceLifeTime = 2f;

    private bool isBroken;

    public void Break()
    {
        if (isBroken) return;
        isBroken = true;

        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        if (breakSound != null)
            breakSound.Play();

        if (piecePrefabs != null)
        {
            foreach (GameObject prefab in piecePrefabs)
            {
                if (prefab == null) continue;

                GameObject piece = Instantiate(prefab, transform.position, Quaternion.identity);

                Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = Random.insideUnitCircle.normalized;
                    float force = Random.Range(minForce, maxForce);

                    rb.linearVelocity = dir * force;
                    rb.angularVelocity = Random.Range(-torque, torque);
                }

                Destroy(piece, pieceLifeTime);
            }
        }

        Collider2D targetCollider = GetComponent<Collider2D>();
        if (targetCollider != null)
            targetCollider.enabled = false;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        Destroy(gameObject, 0.1f);
    }
}
