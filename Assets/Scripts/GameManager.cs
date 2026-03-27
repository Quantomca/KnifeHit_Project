using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject gameOverUI;
    public GameObject gameWinUI;

    void Awake()
    {
        instance = this;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        gameOverUI.SetActive(true);
    }

    public void GameWin()
    {
        Time.timeScale = 0f;
        gameWinUI.SetActive(true);
    }
}