using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Example implementation showing how different game systems can react to flag changes.
/// This demonstrates the FlagMonitorSystem integration with various game systems.
/// 
/// Attach this to a GameObject in your scene to see automatic reactions to story flags.
/// </summary>
public class GameSystemsManager : MonoBehaviour
{
    [Header("Quest Integration")]
    public QuestManager questManager;

    [Header("Audio Integration")]
    public AudioSource musicSource;
    public AudioClip peacefulMusic;
    public AudioClip urgentMusic;
    public AudioClip mysticalMusic;
    public AudioClip victoryMusic;

    [Header("UI Integration")]
    public GameObject urgentMessagePanel;
    public TextMeshProUGUI messageText;

    [Header("Debug")]
    public bool enableReactionLogs = true;

    [SerializeField] GameObject ttsCanvas;

    void Start()
    {
        SetupFlagReactions();
    }

    void LateUpdate()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "SceneAwal") return;
        if (DayNightCycle.Instance.CurrentTime >= 22f && DayNightCycle.Instance.CurrentTime <= 22.4f)
        {
            DayNightCycle.Instance.SetTime(22.5f);
            DayNightCycle.Instance.PauseTime();
            ttsCanvas.SetActive(true);
        }
    }


    void SetupFlagReactions()
    {
        // ===== STORY PROGRESSION REACTIONS =====

        // Chapter 1: Water Crisis Discovery
        FlagMonitorSystem.WatchFlagAdded("water_crisis_discovered", () =>
        {
            LogReaction("Water crisis discovered - Starting Chapter 2");
            PlayMusic(urgentMusic);
            ShowUrgentMessage("Desa membutuhkan bantuanmu untuk mengatasi krisis air!");

            // Auto-start related quest if QuestManager exists
            if (questManager != null)
            {
                questManager.StartQuest("01_WaterCrisisDiscovery");
            }
        });

        // Chapter 2: Permission and Planning
        FlagMonitorSystem.WatchFlagAdded("asked_permission_water_project", () =>
        {
            LogReaction("Permission requested - Starting construction phase");

            if (questManager != null)
            {
                questManager.StartQuest("03_DamConstruction");
            }
        });

        // Chapter 3: Helper Recruitment
        FlagMonitorSystem.WatchFlagAdded("student_helpers_recruited", () =>
        {
            LogReaction("Students recruited - Construction can begin");
            ShowMessage("Murid murid Padepokan siap untuk membantumu!");

            if (questManager != null)
            {
                questManager.CompleteObjective("03_DamConstruction", "gather_helpers");
            }
        });

        // Chapter 4: Mystical Encounters
        FlagMonitorSystem.WatchFlagAdded("spiritual_vision_active", () =>
        {
            LogReaction("Spiritual vision activated - Mystical chapter begins");
            PlayMusic(mysticalMusic);

            if (questManager != null)
            {
                questManager.StartQuest("05_SpiritualVision");
            }
        });

        FlagMonitorSystem.WatchFlagAdded("spirit_pact_complete", () =>
        {
            LogReaction("Spirit pact completed - Sacred quest unlocked");
            ShowMessage("Penunggu sungai menerima pemberianmu!");

            if (questManager != null)
            {
                questManager.CompleteQuest("05_SpiritualVision");
                questManager.StartQuest("07_CompleteSacrifice");
            }
        });

        // Chapter 5: Conflict and Resolution
        FlagMonitorSystem.WatchFlagAdded("mbok_randa_angry", () =>
        {
            LogReaction("Mbok Randa is angry - Conflict chapter begins");
            ShowUrgentMessage("Mbok Randa telah mengetahui apa yang terjadi pada Gajahnya!");

            if (questManager != null)
            {
                questManager.StartQuest("08_FaceMbokRandaAnger");
            }
        });

        FlagMonitorSystem.WatchFlagAdded("reconciliation_complete", () =>
        {
            LogReaction("Reconciliation achieved - Story resolution begins");
            PlayMusic(victoryMusic);
            ShowMessage("Perdamain telah tercapai melalui pemahaman satu sama lain!");

            if (questManager != null)
            {
                questManager.CompleteQuest("08_AchieveReconciliation");
                questManager.StartQuest("09_StoryCompletion");
            }
        });

        // Final Chapter: Story Completion
        FlagMonitorSystem.WatchFlagAdded("story_completed", () =>
        {
            LogReaction("Story completed - Returning to peaceful state");
            PlayMusic(peacefulMusic);
            ShowMessage("Petualanganmu telah mengajarkanmu kearifan dan kebijakan!");

            if (questManager != null)
            {
                questManager.CompleteQuest("09_StoryCompletion");
            }

            // Trigger story completion sequence
            StartCoroutine(HandleStoryCompletion());
        });

        // Land naming completion (final story event)
        FlagMonitorSystem.WatchFlagAdded("teranging_galih_named", () =>
        {
            LogReaction("Land named Teranging Galih - Legacy established, story ending");

            // This is the true end of the story
            StartCoroutine(HandleStoryCompletion());
        });

        // ===== SPECIAL EVENT REACTIONS =====

        // Dam construction milestones
        FlagMonitorSystem.WatchFlagAdded("dam_construction_complete", () =>
        {
            LogReaction("Dam construction completed - Water flows again!");
            ShowMessage("Sukses!, kini air bagi desa dapat terakses lagi!");
        });

        // Character interactions
        FlagMonitorSystem.WatchFlagAdded("guru_guidance_received", () =>
        {
            LogReaction("Guru guidance received - Wisdom gained");
            ShowMessage("Pengetahuan Ki Ageng Sinawang membimbingmu!");
        });

        // Mystical events
        FlagMonitorSystem.WatchFlagAdded("rescued_by_crocodile", () =>
        {
            LogReaction("Rescued by white crocodile - Divine intervention");
            ShowMessage("Buaya putih telah menyelamatkanmu!");
        });

        // Land naming ceremony
        FlagMonitorSystem.WatchFlagAdded("teranging_galih_named", () =>
        {
            LogReaction("Land named Teranging Galih - Legacy established");
            ShowMessage("Tanah ini kelak akan dinamakan Teranging Galih (Trenggalek)!");
        });
    }

    #region Reaction Helper Methods

    void LogReaction(string message)
    {
        if (enableReactionLogs)
        {
            Debug.Log($"[GameSystems] {message}");
        }
    }

    void PlayMusic(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
            LogReaction($"Playing music: {clip.name}");
        }
    }

    void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            LogReaction($"Showing message: {message}");
        }
    }

    void ShowUrgentMessage(string message)
    {
        ShowMessage(message);

        if (urgentMessagePanel != null)
        {
            urgentMessagePanel.SetActive(true);
            // Hide after 5 seconds
            Invoke(nameof(HideUrgentMessage), 5f);
        }
    }

    void HideUrgentMessage()
    {
        if (urgentMessagePanel != null)
        {
            urgentMessagePanel.SetActive(false);
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Show Flag Statistics")]
    void ShowFlagStatistics()
    {
        Debug.Log($"Total Flag Watchers: {FlagMonitorSystem.GetTotalWatcherCount()}");
        Debug.Log($"Watched Flags: {string.Join(", ", FlagMonitorSystem.GetWatchedFlags())}");
    }

    [ContextMenu("Test Flag Trigger")]
    void TestFlagTrigger()
    {
        // For testing - manually trigger a flag
        var interactionSystem = FindObjectOfType<NPCInteractionSystem>();
        if (interactionSystem != null)
        {
            interactionSystem.AddGameFlag("water_crisis_discovered");
        }
    }

    #endregion
    private System.Collections.IEnumerator HandleStoryCompletion()
    {
        LogReaction("Initiating story completion sequence");
        
        // Wait for final story elements to complete
        yield return new WaitForSeconds(5f);
        
        // Show completion message
        ShowMessage("Terima kasih telah menyelesaikan perjalanan Sinawang!");
        
        // Wait a bit more for player to read
        yield return new WaitForSeconds(3f);
        
        // Auto-save the completion
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.AutoSave("StoryComplete_Final");
        }
        
        // Wait before returning to menu
        yield return new WaitForSeconds(2f);
        
        // Return to main menu
        SceneManager.LoadScene("MainMenu");
        
        LogReaction("Story completion sequence finished - returned to main menu");
    }
}