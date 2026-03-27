using UnityEngine;

public class KnifeSpawner : MonoBehaviour
{
    public GameObject knifePrefab;
    private GameObject currentKnife;

    private bool canThrow = true;

    void Start()
    {
        Spawn();
    }

    void Update()
    {
        if (GameManager.instance != null && Time.timeScale == 0f)
            return;

        if (!canThrow) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (currentKnife == null) return;

            Knife knife = currentKnife.GetComponent<Knife>();
            if (knife == null) return;

            canThrow = false;

            knife.Throw();

            KnifeUIManager.instance.UseKnife();
            LevelManager.instance.UseKnife();

            currentKnife = null;

            Invoke(nameof(Spawn), 0.05f);
        }
    }

    void Spawn()
    {
        currentKnife = Instantiate(knifePrefab, transform.position, Quaternion.identity);
        canThrow = true;
    }
    // 🔥 FIX KHI QUA LEVEL
    public void ResetSpawner()
    {
        CancelInvoke();
        currentKnife = null;
        Spawn();
    }
}