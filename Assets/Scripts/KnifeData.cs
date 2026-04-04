using UnityEngine;

[System.Serializable]
public class KnifeData
{
    public string id;
    public string displayName;
    public Sprite gameplaySprite;
    public Sprite icon;
    public bool unlocked = true;

    public Sprite GetPreviewSprite()
    {
        return icon != null ? icon : gameplaySprite;
    }
}
