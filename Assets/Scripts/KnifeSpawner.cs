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
            if (LevelManager.instance == null) return;
            if (!LevelManager.instance.CanThrowKnife()) return;

            Knife knife = currentKnife.GetComponent<Knife>();
            if (knife == null) return;

            canThrow = false;

            knife.Throw();

            KnifeUIManager.instance.UseKnife();
            LevelManager.instance.OnKnifeThrown();

            currentKnife = null;

            if (LevelManager.instance.ShouldSpawnNextKnife())
                Invoke(nameof(Spawn), 0.02f);
        }
    }

    void Spawn()
    {
        if (knifePrefab == null) return;

        currentKnife = Instantiate(knifePrefab, transform.position, Quaternion.identity);
        canThrow = true;
    }

    public void ResetSpawner()
    {
        CancelInvoke();

        if (currentKnife != null)
            Destroy(currentKnife);

        currentKnife = null;
        Spawn();
    }

    public void StopSpawning()
    {
        CancelInvoke();
        canThrow = false;

        if (currentKnife != null)
            Destroy(currentKnife);

        currentKnife = null;
    }
}
