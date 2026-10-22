using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsMenuPanel;

    private bool isPaused = false;

    public bool IsPaused => isPaused;

    private void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

            // 1. If Options Sub-Menu is active, close it and return to Pause Menu
            if (optionsMenuPanel != null && optionsMenuPanel.activeSelf)
            {
                CloseOptions();
            }
            // 2. If main Pause Menu is active, resume the game
            else if (isPaused)
            {
                Resume();
            }
            // 3. Otherwise, open the Pause Menu
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        isPaused = true;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        if (optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(false);
        }

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    public void OpenOptions()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(true);
        }
    }

    public void CloseOptions()
    {
        if (optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(false);
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }
}