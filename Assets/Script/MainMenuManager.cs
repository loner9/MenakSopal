using System.Collections.Generic;
using System.Linq;
using MenakSopal.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main menu manager that handles start/continue button visibility based on save files.
/// Manages new game creation, continue game loading, and story completion flow.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button startNewGameButton;
    public Button continueGameButton;
    public Button settingsButton;
    public Button exitGameButton;


    [Header("Continue Game Info")]
    public GameObject continueInfoPanel;
    public Text continueSceneText;
    public Text continueTimeText;
    public Text continueProgressText;


    [Header("New Game Confirmation")]
    public GameObject newGameConfirmPanel;
    public Button confirmNewGameButton;
    public Button cancelNewGameButton;
    public Text confirmationText;


    [Header("Story Completion")]
    public GameObject storyCompletePanel;
    public Button newGamePlusButton;
    public Button backToMenuButton;


    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Button closeSettingsButton;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider masterVolumeSlider;


    [Header("Game Start Settings")]
    public string firstGameScene = "SceneAwal";
    public Vector3 startPosition = Vector3.zero;
    public AudioClip menuMusic;


    [Header("Debug")]
    public bool enableDebugLogs = true;


    private SaveFileInfo latestSave;
    private bool hasExistingSaves;


    void Start()
    {
        InitializeMenu();
        SetupButtons();
        CheckForExistingSaves();
        SetupStoryCompletionHandler();

        if (AudioSystem.Instance != null && menuMusic != null)
        {
            AudioSystem.Instance.PlayMusic(menuMusic);
        }
    }


    void InitializeMenu()
    {
        // Hide panels initially
        if (continueInfoPanel) continueInfoPanel.SetActive(false);
        if (newGameConfirmPanel) newGameConfirmPanel.SetActive(false);
        if (storyCompletePanel) storyCompletePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);


        LogDebug("Main menu initialized");
    }

    void SetupButtons()
    {
        // Debug button assignments
        Debug.Log($"[MainMenu] Setting up buttons:");
        Debug.Log($"  - Start New Game Button: {(startNewGameButton != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"  - Continue Game Button: {(continueGameButton != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"  - Settings Button: {(settingsButton != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"  - Exit Button: {(exitGameButton != null ? "ASSIGNED" : "NULL")}");

        // Setup button events

        if (startNewGameButton)

        {
            startNewGameButton.onClick.AddListener(OnStartNewGameClicked);
            Debug.Log("Start New Game button listener added");
        }
        else
        {
            Debug.LogError("startNewGameButton is NULL! Please assign it in the inspector.");
        }


        if (continueGameButton)

        {
            continueGameButton.onClick.AddListener(OnContinueGameClicked);
            Debug.Log("Continue Game button listener added");
        }
        else
        {
            Debug.LogWarning("continueGameButton is NULL - assign it if you want continue functionality");
        }


        if (settingsButton) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (closeSettingsButton) closeSettingsButton.onClick.AddListener(OnCloseSettingsClicked);
        if (exitGameButton) exitGameButton.onClick.AddListener(OnExitGameClicked);

        // New game confirmation
        if (confirmNewGameButton) confirmNewGameButton.onClick.AddListener(OnConfirmNewGame);
        if (cancelNewGameButton) cancelNewGameButton.onClick.AddListener(OnCancelNewGame);

        // Story completion buttons
        if (newGamePlusButton) newGamePlusButton.onClick.AddListener(OnNewGamePlusClicked);
        if (backToMenuButton) backToMenuButton.onClick.AddListener(OnBackToMenuClicked);


        Debug.Log("[MainMenu] Button setup complete");
    }


    void SetupStoryCompletionHandler()
    {
        // Listen for story completion to return to main menu
        FlagMonitorSystem.WatchFlagAdded("story_completed", OnStoryCompleted);
        FlagMonitorSystem.WatchFlagAdded("game_ending_reached", OnStoryCompleted);
    }

    #region Save File Detection


    void CheckForExistingSaves()
    {
        List<SaveFileInfo> saveFiles = GameSaveManager.Instance.GetSaveFiles();

        // Filter out auto-saves and get actual player saves

        var playerSaves = saveFiles.Where(s => !s.slotName.StartsWith("Auto_")).ToList();
        hasExistingSaves = playerSaves.Count > 0;


        if (hasExistingSaves)
        {
            // Get the most recent save
            latestSave = playerSaves.OrderByDescending(s => s.saveTime).First();
            UpdateContinueButton(true);
            UpdateContinueInfo();
        }
        else
        {
            UpdateContinueButton(false);
        }


        LogDebug($"Save check complete. Has saves: {hasExistingSaves}");
    }


    void UpdateContinueButton(bool hasSaves)
    {
        if (continueGameButton)
        {
            continueGameButton.interactable = hasSaves;

            // Update button text or visual state

            Text buttonText = continueGameButton.GetComponentInChildren<Text>();
            if (buttonText)
            {
                buttonText.color = hasSaves ? Color.white : Color.gray;
            }
        }
    }


    void UpdateContinueInfo()
    {
        if (latestSave == null) return;


        if (continueSceneText)
        {
            string sceneName = GetFriendlySceneName(latestSave.currentScene);
            continueSceneText.text = $"Location: {sceneName}";
        }


        if (continueTimeText)
        {
            continueTimeText.text = $"Saved: {latestSave.saveTime:MM/dd/yyyy HH:mm}";
        }


        if (continueProgressText)
        {
            continueProgressText.text = $"Progress: {latestSave.totalFlags} story events";
        }
    }


    string GetFriendlySceneName(string sceneName)
    {
        switch (sceneName)
        {
            case "SceneAwal": return "Village";
            case "SceneHutan": return "Forest";
            case "SceneDesaKrandon": return "Desa Krandon";
            default: return sceneName;
        }
    }

    #endregion

    #region Button Handlers


    void OnStartNewGameClicked()
    {
        Debug.Log("[MainMenu] Start New Game button clicked!");
        LogDebug("Start New Game button clicked!");


        if (hasExistingSaves)
        {
            // Show confirmation dialog
            ShowNewGameConfirmation();
        }
        else
        {
            // No existing saves, start immediately
            StartNewGame();
        }
    }


    void OnContinueGameClicked()
    {
        Debug.Log("[MainMenu] Continue Game button clicked!");
        LogDebug("Continue Game button clicked!");


        if (latestSave != null)
        {
            if (LoadingManager.Instance != null) LoadingManager.Instance.RegisterSystemBusy("SaveData");
            ContinueGame(latestSave.slotName);
        }
        else
        {
            LogDebug("No save file found to continue");
        }
    }


    void OnSettingsClicked()
    {
        LogDebug("Settings button clicked - toggling settings panel");
        ToggleSettingsPanel();
    }


    void OnCloseSettingsClicked()
    {
        HideSettingsPanel();
    }


    void OnExitGameClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    #endregion

    #region New Game Confirmation


    void ShowNewGameConfirmation()
    {
        if (newGameConfirmPanel)
        {
            newGameConfirmPanel.SetActive(true);


            if (confirmationText)
            {
                confirmationText.text = "Starting a new game will erase your current progress. Are you sure?";
            }
        }
    }


    void OnConfirmNewGame()
    {
        if (newGameConfirmPanel) newGameConfirmPanel.SetActive(false);
        StartNewGame();
    }


    void OnCancelNewGame()
    {
        if (newGameConfirmPanel) newGameConfirmPanel.SetActive(false);
    }

    #endregion

    #region Game Flow


    void StartNewGame()
    {
        LogDebug("Starting new game");

        // Clear all existing save data

        ClearAllSaveData();

        // Reset all persistent systems

        ResetGameSystems();

        // Load first game scene

        LoadFirstScene();
    }


    void ContinueGame(string saveSlotName)
    {
        LogDebug($"Continuing game from save: {saveSlotName}");


        bool success = GameSaveManager.Instance.LoadGame(saveSlotName);
        if (!success)
        {
            LogDebug("Failed to load save file");
            // Show error message to player
            return;
        }

        // Save system will handle scene loading automatically
    }


    void ClearAllSaveData()
    {
        // Get all save files and delete them
        List<SaveFileInfo> allSaves = GameSaveManager.Instance.GetSaveFiles();


        foreach (var save in allSaves)
        {
            GameSaveManager.Instance.DeleteSave(save.slotName);
        }


        LogDebug($"Cleared {allSaves.Count} save files");
    }


    void ResetGameSystems()
    {
        // Reset quest manager
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ResetAllQuests();
        }

        // Reset dialogue flags

        var npcInteraction = FindObjectOfType<NPCInteractionSystem>();
        if (npcInteraction != null)
        {
            npcInteraction.SetGameFlags(new List<string>());
        }

        // Reset day/night cycle

        if (DayNightCycle.Instance != null)
        {
            DayNightCycle.Instance.ResetToDay();
        }


        LogDebug("Game systems reset to initial state");
    }


    void LoadFirstScene()
    {
        // Set initial game flag
        var npcInteraction = FindObjectOfType<NPCInteractionSystem>();
        if (npcInteraction != null)
        {
            npcInteraction.AddGameFlag("game_started");
        }

        // Load first scene

        if (LoadingManager.Instance != null)
            LoadingManager.Instance.LoadScene(firstGameScene);
        else
            SceneManager.LoadScene(firstGameScene);


        LogDebug($"Loading first scene: {firstGameScene}");
    }

    #endregion

    #region Story Completion


    void OnStoryCompleted()
    {
        LogDebug("Story completed - returning to main menu");

        // Wait a moment for final story events to process

        Invoke(nameof(ReturnToMainMenuAfterStory), 3f);
    }


    void ReturnToMainMenuAfterStory()
    {
        // Show story completion options
        if (storyCompletePanel)
        {
            storyCompletePanel.SetActive(true);
        }
        else
        {
            // If no completion panel, just return to menu
            ReturnToMainMenu();
        }
    }


    void OnNewGamePlusClicked()
    {
        LogDebug("New Game Plus selected");

        // For New Game+, you might want to keep some progress
        // but reset story flags. Implement based on your needs.


        StartNewGame(); // For now, same as new game
    }


    void OnBackToMenuClicked()
    {
        ReturnToMainMenu();
    }


    void ReturnToMainMenu()
    {
        // Clear save data after story completion
        ClearAllSaveData();

        // Reset systems

        ResetGameSystems();

        // Load main menu scene

        if (LoadingManager.Instance != null)
            LoadingManager.Instance.LoadScene("MainMenu");
        else
            SceneManager.LoadScene("MainMenu");


        LogDebug("Returned to main menu after story completion");
    }

    #endregion

    #region UI Updates


    public void RefreshSaveInfo()
    {
        CheckForExistingSaves();
    }


    public void ShowContinueInfo()
    {
        if (continueInfoPanel && latestSave != null)
        {
            continueInfoPanel.SetActive(true);
        }
    }

    public void HideContinueInfo()
    {
        if (continueInfoPanel)
        {
            continueInfoPanel.SetActive(false);
        }
    }


    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            bool isActive = !settingsPanel.activeSelf;
            settingsPanel.SetActive(isActive);
            if (isActive)
            {
                SyncSettingsSliders();
            }
            LogDebug($"Settings panel toggled: {isActive}");
        }
        else
        {
            Debug.LogWarning("settingsPanel is NULL! Please assign it in the Unity Inspector.");
        }
    }


    public void ShowSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            SyncSettingsSliders();
        }
    }


    public void HideSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }


    private void SyncSettingsSliders()
    {
        if (AudioSystem.Instance == null) return;

        BindSlider(masterVolumeSlider, AudioSystem.Instance.masterVolume, (val) => AudioSystem.Instance.SetMasterVolume(val));
        BindSlider(musicVolumeSlider, AudioSystem.Instance.musicVolume, (val) => AudioSystem.Instance.SetMusicVolume(val));
        BindSlider(sfxVolumeSlider, AudioSystem.Instance.sfxVolume, (val) => AudioSystem.Instance.SetSFXVolume(val));
    }


    private void BindSlider(Slider slider, float volume01, System.Action<float> onVolumeChanged)
    {
        if (slider == null) return;

        slider.onValueChanged.RemoveAllListeners();

        // Map 0..1 volume to slider's min..max range
        float mappedValue = Mathf.Lerp(slider.minValue, slider.maxValue, Mathf.Clamp01(volume01));
        slider.value = mappedValue;

        slider.onValueChanged.AddListener((val) =>
        {
            float range = slider.maxValue - slider.minValue;
            float normalizedVol = range > 0.0001f ? (val - slider.minValue) / range : val;
            onVolumeChanged?.Invoke(normalizedVol);
        });
    }


    #endregion

    #region Utility

    void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[MainMenu] {message}");
        }
    }

    void OnDestroy()
    {
        // Clean up any listeners if needed
    }

    #endregion

    #region Public API


    /// <summary>
    /// Force refresh of menu state (call after external save operations)
    /// </summary>
    public void RefreshMenuState()
    {
        CheckForExistingSaves();
    }


    /// <summary>
    /// Show story completion screen externally
    /// </summary>
    public void ShowStoryCompletion()
    {
        if (storyCompletePanel)
        {
            storyCompletePanel.SetActive(true);
        }
    }


    #endregion
}