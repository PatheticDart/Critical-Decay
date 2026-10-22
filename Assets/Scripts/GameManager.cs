using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameWinPanel;

    private bool isGameOver = false;
    private bool isGameWin = false;

    public bool IsGameOver => isGameOver;
    public bool IsGameWin => isGameWin;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void GameWin()
    {
        if (isGameWin) return;
        isGameWin = true;

        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartNormalMode()
    {
        PlayerPrefs.SetInt("IsEndlessMode", 0);
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Scene");
    }

    public void StartEndlessMode()
    {
        PlayerPrefs.SetInt("IsEndlessMode", 1);
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Scene");
    }
}