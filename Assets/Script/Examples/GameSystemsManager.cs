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
        Debug.Log("[GameSystems] GameSystemsManager starting up...");
        
        // Try to find QuestManager if not assigned
        if (questManager == null)
        {
            questManager = QuestManager.Instance;
            Debug.Log($"[GameSystems] QuestManager found: {questManager != null}");
        }
        
        SetupFlagReactions();
        
        // Check if story_started flag exists and trigger water crisis discovery
        var interactionSystem = FindObjectOfType<NPCInteractionSystem>();
        if (interactionSystem != null)
        {
            var flags = interactionSystem.GetGameFlags();
            Debug.Log($"[GameSystems] Current flags: {string.Join(", ", flags)}");
            
            // If story_started flag exists but water_crisis_discovered doesn't, add it
            if (flags.Contains("story_started") && !flags.Contains("water_crisis_discovered"))
            {
                Debug.Log("[GameSystems] story_started flag found but water_crisis_discovered missing. Adding it now.");
                interactionSystem.AddGameFlag("water_crisis_discovered");
            }
        }
        else
        {
            Debug.LogWarning("[GameSystems] NPCInteractionSystem not found!");
        }
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

        FlagMonitorSystem.WatchFlagAdded("story_started", () =>
        {
            LogReaction("Story Started - Starting Chapter 1");
            
            // Auto-start related quest if QuestManager exists
            if (questManager != null)
            {
                bool started = questManager.StartQuest("water_crisis_discovery");
                Debug.Log($"[GameSystems] Quest start result: {started}");
            }
            else
            {
                Debug.LogWarning("[GameSystems] QuestManager is null - cannot start quest");
            }
        });

        FlagMonitorSystem.WatchFlagAdded("first_contact", () =>
        {
            LogReaction("Story Started - Starting Chapter 1");
            PlayMusic(urgentMusic);
            // ShowUrgentMessage("Ber-Interaksilah dan Cari tahu apa yang terjadi di desa!");
        });

        // Chapter 1: Water Crisis Discovery
        FlagMonitorSystem.WatchFlagAdded("water_crisis_discovered", () =>
        {
            LogReaction("Water crisis discovered - Starting Chapter 2");
            PlayMusic(urgentMusic);
            ShowUrgentMessage("Ber-Interaksilah dan Cari tahu apa yang terjadi di desa!");

            // Auto-start related quest if QuestManager exists
            if (questManager != null)
            {
                Debug.Log("[GameSystems] Attempting to start quest water_crisis_discovery");
                bool started = questManager.StartQuest("water_crisis_discovery");
                Debug.Log($"[GameSystems] Quest start result: {started}");
            }
            else
            {
                Debug.LogWarning("[GameSystems] QuestManager is null - cannot start quest");
            }
        });

        FlagMonitorSystem.WatchFlagAdded("committed_to_help", () =>
        {
            ShowUrgentMessage("Temuilah Ki Ageng dan mintalah petunjuk darinya!");
            if (questManager != null)
            {
                Debug.Log("[GameSystems] Attempting to start quest seek_guru_guidance");
                bool started = questManager.StartQuest("seek_guru_guidance");
                Debug.Log($"[GameSystems] Quest start result: {started}");
            }
            else
            {
                Debug.LogWarning("[GameSystems] QuestManager is null - cannot start quest");
            }
        });

        // Chapter 2: Permission and Planning
        FlagMonitorSystem.WatchFlagAdded("asked_permission_water_project", () =>
        {
            LogReaction("Permission requested - Starting construction phase");

            if (questManager != null)
            {
                questManager.StartQuest("gather_construction_helpers");
            }
        });

        // Chapter 3: Helper Recruitment
        FlagMonitorSystem.WatchFlagAdded("helpers_recruited", () =>
        {
            LogReaction("Students recruited - Construction can begin");
            ShowMessage("Murid murid Padepokan siap untuk membantumu!");

            if (questManager != null)
            {
                questManager.StartQuest("dam_construction_project");
            }
        });

        //trigger event dam rusak. setelah objective kelar dan "initial_dam_success" flag ini muncul
        //ada animasi fade yang nunjukin bangunan selesai. MC ngobrol sama murid2 padepokan, sehabis itu selesai, animasi fade
        //pindah tempat, dan cerita berikutnya dimulai, enggak lama dari fade ini akan ada suara ledakan
        //mc bergegas ke dam.
        FlagMonitorSystem.WatchFlagAdded("initial_dam_success", () =>
        {

        }); 

        // Chapter 4: Mystical Encounters
        FlagMonitorSystem.WatchFlagAdded("spiritual_vision_active", () =>
        {
            LogReaction("Spiritual vision activated - Mystical chapter begins");
            PlayMusic(mysticalMusic);

            if (questManager != null)
            {
                questManager.StartQuest("spiritual_vision_encounter");
            }
        });

        FlagMonitorSystem.WatchFlagAdded("spirit_pact_complete", () =>
        {
            LogReaction("Spirit pact completed - Sacred quest unlocked");
            ShowMessage("Penunggu sungai menerima pemberianmu!");

            if (questManager != null)
            {
                questManager.CompleteQuest("spiritual_vision_encounter");
                questManager.StartQuest("complete_spirit_sacrifice");
            }
        });

        // Chapter 5: Conflict and Resolution
        FlagMonitorSystem.WatchFlagAdded("mbok_randa_angry", () =>
        {
            LogReaction("Mbok Randa is angry - Conflict chapter begins");
            ShowUrgentMessage("Mbok Randa telah mengetahui apa yang terjadi pada Gajahnya!");

            if (questManager != null)
            {
                questManager.StartQuest("face_mbok_randa_anger");
            }
        });

        FlagMonitorSystem.WatchFlagAdded("reconciliation_complete", () =>
        {
            LogReaction("Reconciliation achieved - Story resolution begins");
            PlayMusic(victoryMusic);
            ShowMessage("Perdamain telah tercapai melalui pemahaman satu sama lain!");

            if (questManager != null)
            {
                questManager.CompleteQuest("achieve_reconciliation");
                questManager.StartQuest("story_completion");
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
                questManager.CompleteQuest("story_completion");
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
        ShowMessage("Terima kasih telah menyelesaikan perjalanan Menak Sopal!");
        
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