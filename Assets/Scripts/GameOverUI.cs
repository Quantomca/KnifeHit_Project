using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public void Retry()
    {
        Time.timeScale = 1f;
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

        if (LevelManager.instance != null)
        {
            LevelManager.instance.currentStage = 1;
            LevelManager.instance.currentLevel = 1;
            LevelManager.instance.StartLevel();
        }

        if (StageUIManager.instance != null)
            StageUIManager.instance.ResetStageUI();

        if (GameManager.instance != null)
            GameManager.instance.PrepareGameplayUI();
    }
}
