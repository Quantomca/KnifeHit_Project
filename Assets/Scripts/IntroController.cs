using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    void Start()
    {
        Invoke("NextScene", 2f);
    }

    void NextScene()
    {
        SceneManager.LoadScene("MainMenu");
    }
}