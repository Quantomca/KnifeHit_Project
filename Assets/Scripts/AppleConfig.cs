using UnityEngine;

[CreateAssetMenu(menuName = "Knife Hit/Apple Config", fileName = "AppleConfig")]
public class AppleConfig : ScriptableObject
{
    public Sprite appleSprite;
    public float worldScale = 0.55f;
    public float surfaceOffset = 0.08f;
}
