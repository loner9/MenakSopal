using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MenakSopal.UI
{
    /// <summary>
    /// Manages game pausing, pause menu UI, time scale, cursor visibility, and navigation to settings or main menu.
    /// Attach this script to your Pause Menu Manager GameObject in in-game scenes.
    /// </summary>
    public class PauseMenuManager : MonoBehaviour
    {
        public static PauseMenuManager Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("The main pause menu panel containing Resume, Settings, Exit, etc.")]
        public GameObject pauseMenuPanel;

        [Tooltip("Optional sub-panel for settings. Will be closed when resuming.")]
        public GameObject settingsPanel;

        [Header("Buttons")]
        public Button resumeButton;
        public Button settingsButton;
        public Button mainMenuButton;
        public Button exitButton;

        [Header("Settings")]
        [Tooltip("Key used to toggle pause menu in-game")]
        public KeyCode pauseKey = KeyCode.Escape;

        [Tooltip("Should cursor be unlocked and made visible when paused?")]
        public bool manageCursor = true;

        [Header("Scene Navigation")]
        public string mainMenuSceneName = "MainMenu";

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SetupButtons();
            ResumeGame(); // Ensure game starts unpaused
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                TogglePause();
            }
        }

        private void SetupButtons()
        {
            if (resumeButton) resumeButton.onClick.AddListener(ResumeGame);
            if (settingsButton) settingsButton.onClick.AddListener(OpenSettings);
            if (mainMenuButton) mainMenuButton.onClick.AddListener(GoToMainMenu);
            if (exitButton) exitButton.onClick.AddListener(ExitGame);
        }

        /// <summary>
        /// Toggles between paused and running states.
        /// </summary>
        public void TogglePause()
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        /// <summary>
        /// Pauses the game, stops time, and shows the pause menu.
        /// </summary>
        public void PauseGame()
        {
            IsPaused = true;
            Time.timeScale = 0f;

            if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
            if (settingsPanel) settingsPanel.SetActive(false);

            if (manageCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            Debug.Log("[PauseMenuManager] Game Paused");
        }

        /// <summary>
        /// Resumes the game and restores time scale.
        /// </summary>
        public void ResumeGame()
        {
            IsPaused = false;
            Time.timeScale = 1f;

            if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);

            if (manageCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            Debug.Log("[PauseMenuManager] Game Resumed");
        }

        /// <summary>
        /// Opens the settings sub-panel from the pause menu.
        /// </summary>
        public void OpenSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Closes the settings sub-panel.
        /// </summary>
        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Returns to the main menu scene, resetting time scale.
        /// </summary>
        public void GoToMainMenu()
        {
            Time.timeScale = 1f;

            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.LoadScene(mainMenuSceneName);
            }
            else
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        public void ExitGame()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
