using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public void Retry()
    {
        Time.timeScale = 1f;
        
        KnifeCounterUI.instance.ResetCounter();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Menu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    public void Restart()
{
    Time.timeScale = 1f;
    LevelManager.instance.currentLevel = 1;
    LevelManager.instance.StartLevel();
    gameObject.SetActive(false);
}
}