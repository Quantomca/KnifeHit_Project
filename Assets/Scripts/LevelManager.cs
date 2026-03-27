using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Level Data")]
    public List<LevelData> levels;

    public GameObject knifePrefab;
    public static LevelManager instance;

    public int currentLevel = 1;
    public int maxLevel = 5;

    public int knivesLeft;
    public bool canThrow = true;
    public bool isStageClearing = false;

    public TargetRotation target;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        ClearKnives();
        isStageClearing = false;
        canThrow = true;
            KnifeSpawner spawner = FindObjectOfType<KnifeSpawner>();
    if (spawner != null)
        spawner.ResetSpawner();

        LevelData data = levels[currentLevel - 1];

        // 🎯 knives
        knivesLeft = data.knivesCount;

        // 🎯 rotation
        float speed = data.rotationSpeed;

        if (data.randomDirection)
            speed *= (Random.value > 0.5f ? 1 : -1);

        if (data.isBoss)
            speed *= 1.3f;

        target.speed = speed;

        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        sr.sprite = data.targetSprite;
        sr.color = Color.white;

        float baseRadius = sr.sprite.bounds.extents.y;
        float targetRadius = data.targetRadius;

        float scale = (targetRadius / baseRadius) * 1.2f;

        target.transform.localScale = Vector3.one * scale;

        // 🎯 UI
        KnifeUIManager.instance.totalKnives = knivesLeft;
        KnifeUIManager.instance.ResetUI();
        StageUIManager.instance.UpdateStageUI(currentLevel);

        // 🎯 spawn dao cắm sẵn
        SpawnPreStuckKnives(data.stuckKnifeAngles);
    }

    void SpawnPreStuckKnives(List<float> angles)
    {
        if (angles == null || angles.Count == 0) return;

        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        float radius = sr.bounds.extents.y; 
            Vector2 center = target.transform.position;

            foreach (float angle in angles)
            {
                GameObject knife = Instantiate(knifePrefab);

                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;

                Vector2 hitPoint = center + dir * radius;

                knife.transform.up = -dir;

                SpriteRenderer knifeSR = knife.GetComponent<SpriteRenderer>();

                float knifeLength = knifeSR.sprite.bounds.size.y * knife.transform.localScale.y;
                float embed = knifeLength * 0.25f;

                // 🎯 FIX QUAN TRỌNG: đúng chiều
                knife.transform.position = hitPoint - dir * embed;

                knife.transform.SetParent(target.transform);

                Rigidbody2D rb = knife.GetComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.simulated = true;

                knife.layer = LayerMask.NameToLayer("KnifeStuck");
                knife.tag = "Knife";
                Collider2D col = knife.GetComponent<Collider2D>();
                col.isTrigger = false;
            }
    }

    public void UseKnife()
    {
        if (isStageClearing) return;

        knivesLeft--;

        if (knivesLeft <= 0)
        {
            canThrow = false;
            isStageClearing = true;
            WinLevel();
        }
    }

    void WinLevel()
    {
        canThrow = false;

        if (currentLevel >= maxLevel)
        {
            GameWin();
            return;
        }

        currentLevel++;
        Invoke(nameof(NextLevel), 1f);
    }

    void NextLevel()
    {
        isStageClearing = false;
        canThrow = true;

        ClearKnives();

        KnifeSpawner spawner = FindObjectOfType<KnifeSpawner>();
        if (spawner != null)
            spawner.ResetSpawner();

        StartLevel();
    }

    public void GameOver()
    {
        currentLevel = 1;
        GameManager.instance.GameOver();
    }

    void GameWin()
    {
        GameManager.instance.GameWin();
    }

    void ClearKnives()
    {
        GameObject[] knives = GameObject.FindGameObjectsWithTag("Knife");

        foreach (var k in knives)
            Destroy(k);
    }
}