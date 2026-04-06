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
        KnifeCounterUI.ResetTotalCounter();

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
    
    public void Vip()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("VipScene");
    }
    public void Shop()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("ShopScene");
    }
    public void Setting()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SettingScene");
    }
}
