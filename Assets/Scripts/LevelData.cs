using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public int knivesCount;
    public float rotationSpeed;
    public bool isBoss;
    public List<float> stuckKnifeAngles;
    public bool randomDirection;
    public Sprite targetSprite;
    public float targetRadius = 1.5f;
    [HideInInspector]
    public Vector2 targetScale = Vector2.one;
    public GameObject[] breakPieces;
}
