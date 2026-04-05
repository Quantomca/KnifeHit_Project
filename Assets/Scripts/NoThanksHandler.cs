using UnityEngine;

public class NoThanksHandler : MonoBehaviour
{
    public void OnNoThanksClick()
    {
        if (GameManager.instance != null)
            GameManager.instance.NoThanks();
    }
}
