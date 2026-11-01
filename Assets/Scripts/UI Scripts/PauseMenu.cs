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
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

            if (optionsMenuPanel != null && optionsMenuPanel.activeSelf)
            {
                CloseOptions();
            }
            
            else if (isPaused)
            {
                Resume();
            }
            
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