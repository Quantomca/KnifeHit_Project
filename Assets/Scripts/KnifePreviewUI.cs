using UnityEngine;
using UnityEngine.UI;

public class KnifePreviewUI : MonoBehaviour
{
    public Image previewImage;

    public void ShowKnife(KnifeData data)
    {
        if (previewImage == null)
            return;

        previewImage.sprite = data != null ? data.GetPreviewSprite() : null;
        previewImage.preserveAspect = true;
    }
}
