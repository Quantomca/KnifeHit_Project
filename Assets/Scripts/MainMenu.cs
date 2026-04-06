using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Optional References")]
    [SerializeField] Button knifeSelectButton;
    [SerializeField] Button shopButton;
    [SerializeField] Image selectedKnifePreviewImage;
    [SerializeField] SpriteRenderer selectedKnifePreviewRenderer;

    void Start()
    {
        BindKnifeButton();
        ResolvePreviewTargets();
        RefreshSelectedKnifePreview();
    }

    void OnEnable()
    {
        RefreshSelectedKnifePreview();
    }

    public void Play()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OpenKnifeSelect()
    {
        SceneManager.LoadScene(KnifeSelectManager.SceneName);
    }


    public void Quit()
    {
        Application.Quit();
    }

    void BindKnifeButton()
    {
        if (knifeSelectButton == null)
        {
            GameObject buttonObject = GameObject.Find("ButtonKnife");
            if (buttonObject != null)
                knifeSelectButton = buttonObject.GetComponent<Button>();
        }

        if (knifeSelectButton == null)
            return;

        knifeSelectButton.onClick.RemoveListener(OpenKnifeSelect);
        knifeSelectButton.onClick.AddListener(OpenKnifeSelect);
    }


    void ResolvePreviewTargets()
    {
        if (selectedKnifePreviewImage == null)
        {
            GameObject imageObject = GameObject.Find("SelectedKnifePreview");
            if (imageObject != null)
                selectedKnifePreviewImage = imageObject.GetComponent<Image>();
        }

        if (selectedKnifePreviewRenderer == null)
        {
            GameObject rendererObject = GameObject.Find("SelectedKnifePreview");
            if (rendererObject != null)
                selectedKnifePreviewRenderer = rendererObject.GetComponent<SpriteRenderer>();
        }
    }

    void RefreshSelectedKnifePreview()
    {
        KnifeData selectedKnife = KnifeDatabase.GetSelectedKnife();
        Sprite previewSprite = selectedKnife != null ? selectedKnife.GetPreviewSprite() : null;

        if (selectedKnifePreviewImage != null)
        {
            selectedKnifePreviewImage.sprite = previewSprite;
            selectedKnifePreviewImage.preserveAspect = true;
            selectedKnifePreviewImage.enabled = previewSprite != null;
        }

        if (selectedKnifePreviewRenderer != null)
            selectedKnifePreviewRenderer.sprite = previewSprite;
    }
}
