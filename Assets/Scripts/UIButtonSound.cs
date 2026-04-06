using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        Bind();
    }

    void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        Bind();
    }

    void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClicked);
    }

    void Bind()
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(HandleClicked);
        button.onClick.AddListener(HandleClicked);
    }

    void HandleClicked()
    {
        GameAudio.PlayButtonClick();
    }
}
