using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
    public bool unDisturbedTime = false;

    [SerializeField] GameObject ttsCanvas;
    [SerializeField] Button ttsButton;

    public static GameSystemsManager Instance { get; private set; }

    void Awake()
    {
         if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }       
    }

    void Start()
    {
        Debug.Log("[GameSystems] GameSystemsManager starting up...");
        
        // Try to find QuestManager if not assigned
        if (questManager == null)
        {
            questManager = QuestManager.Instance;
            Debug.Log($"[GameSystems] QuestManager found: {questManager != null}");
        }

        if (ttsButton != null)
        {
            Debug.Log("[GameSystems] Setting up TTS button...");
            ttsButton.onClick.AddListener(MovePlayerTo.Instance.movePlayerWithDelay);
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
            MovePlayerTo.Instance.stopPlayerMovement();
            ttsCanvas.SetActive(true);
        }
    }

    IEnumerator WaitForDialogueEndThenShowMonologue()
    {
        // Wait until player finishes the current dialogue naturally
        while (NPCInteractionSystem.Instance != null &&
               NPCInteractionSystem.Instance.IsInDialogue())
        {
            yield return null; // Wait one frame and check again
        }

        Debug.Log("[GameSystemsManager] Dialogue ended, waiting 0.5-1 second before showing monologue");

        // Add delay between dialogue end and monologue start (0.5-1 second)
        yield return new WaitForSeconds(2.35f);

        Debug.Log("[GameSystemsManager] Showing monologue after recruitment");

        // Subscribe to monologue end event BEFORE showing monologue
        System.Action<MonologueEntry> onMonologueEndHandler = null;
        onMonologueEndHandler = (entry) =>
        {
            Debug.Log("[GameSystemsManager] Monologue ended, executing post-monologue actions");

            // Unsubscribe from event to prevent memory leaks
            if (MonologueSystem.Instance != null)
            {
                MonologueSystem.Instance.OnMonologueEnded -= onMonologueEndHandler;
            }

            // Now execute the actions that should happen AFTER monologue ends
            OnMonologueCompletedAfterRecruitment();
        };

        // Subscribe to the event
        if (MonologueSystem.Instance != null)
        {
            MonologueSystem.Instance.OnMonologueEnded += onMonologueEndHandler;
        }

        // Pause time DURING the monologue
        DayNightCycle.Instance.PauseTime();

        // Show monologue with clean UI state
        MonologueSystem.Instance.ShowSimpleMonologue("Hmm, aku telah selesai meminta izin dan mengumpulkan bala bantuan untuk membangun dam ini. Saatnya untuk menuju ke sungai!", new string[] { "to_river", "npc_to_river"});
    }

    void OnMonologueCompletedAfterRecruitment()
    {
        Debug.Log("[GameSystemsManager] Executing post-monologue sequence: move to river and set time to day");

        // These happen AFTER the monologue ends (not immediately)
        // The move is triggered by "npc_to_river" flag which is added by the monologue
        // So we just need to set time to day and start quest here
        DayNightCycle.Instance.SetTimeOfDay(TimeOfDay.Day);

        if (questManager != null)
        {
            questManager.StartQuest("dam_construction_project");
        }
    }

    void monologueAfterRecruit()
    {
        // This method now just shows the monologue (no force-close needed)
        MonologueSystem.Instance.ShowSimpleMonologue("Hmm, aku telah selesai meminta izin dan mengumpulkan bala bantuan untuk membangun dam ini. Saatnya untuk menuju ke sungai!", new string[] { "to_river", "npc_to_river"});
    }

    void dialogueWithAndiAfterDam()
    {
        ForceDialogueTrigger.Instance.TriggerDialogue();
    }

    IEnumerator WaitForDialogueEndThenTriggerDamBuiltSequence()
    {
        // Wait until player finishes the current dialogue naturally
        while (NPCInteractionSystem.Instance != null &&
               NPCInteractionSystem.Instance.IsInDialogue())
        {
            yield return null; // Wait one frame and check again
        }

        Debug.Log("[GameSystemsManager] Dialogue ended, starting dam built sequence");

        // Pindah MC ke bantaran kali disebelah salah satu NPC, kemudian mereka ngobrol
        MovePlayerTo.Instance.movePlayerWithDestinationFade("TempatMCKali");

        // Add flag for Andi's comment
        NPCInteractionSystem.Instance.AddGameFlag("andi_comment_after_dam");

        // Wait for teleport to complete, then trigger dialogue
        yield return new WaitForSeconds(4.5f);
        dialogueWithAndiAfterDam();

        // Ketika ngobrol belum selesai (secara sistem diselesaiin), tiba tiba ada suara ledakan dan
        // nanti ada semacam screen shake. Ketika dialog selesai maka akan ditambahkan game flag baru untuk trigger next quest
    }

    void SetupFlagReactions()
    {
        // ===== STORY PROGRESSION REACTIONS =====

        FlagMonitorSystem.WatchFlagAdded("story_started", () =>
        {
            LogReaction("Story Started - Starting Chapter 1");
            
            // Show player reflection monologue using MonologueData asset
            // if (MonologueSystem.Instance != null)
            // {
            //     MonologueSystem.Instance.ShowMonologue("PlayerReflections");
            // }
            
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
            // ShowUrgentMessage("Ber-Interaksilah dan Cari tahu apa yang terjadi di desa!");

            // Show player monologue about discovering the crisis
            // if (MonologueSystem.Instance != null)
            // {
            //     MonologueSystem.Instance.ShowSimpleMonologue(
            //         "Ya Tuhan... rakyat di sini benar-benar menderita kekurangan air. Aku tidak bisa membiarkan hal ini terus terjadi. Harus ada yang kulakukan untuk membantu mereka.",
            //         new string[] { "water_crisis_witnessed" }
            //     );
            // }

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
                // Debug.Log("[GameSystems] Attempting to start quest seek_guru_guidance");
                bool started = questManager.StartQuest("seek_guru_guidance");
                // Debug.Log($"[GameSystems] Quest start result: {started}");
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
        // setelah ini harus e ada sequence pindah dari map biasa ke map bangun bendungan
        FlagMonitorSystem.WatchFlagAdded("helpers_recruited", () =>
        {
            LogReaction("Students recruited - Construction can begin");
            ShowMessage("Bala bantuan telah terkumpul, saatnya membangun dam!");

            if (questManager != null)
            {
                // Start the proper sequence:
                // 1. Wait for dialogue to end
                // 2. Wait 0.5-1 second
                // 3. Show monologue + pause time
                // 4. When monologue ends → move to river (via flag) + set time to day + start quest
                StartCoroutine(WaitForDialogueEndThenShowMonologue());
            }
        });

        // FlagMonitorSystem.WatchFlagAdded("to_river", () =>
        // {
        //     MovePlayerTo.Instance.movePlayerWithDestinationFade("BantaranKali");
        // });

        FlagMonitorSystem.WatchFlagAdded("npc_to_river", () =>
        {
            MovePlayerTo.Instance.movePlayerWithDestinationFade("BantaranKali");
            GameObject materialHolder = GameObject.FindGameObjectWithTag("MaterialsDam");

            if (materialHolder != null)
            {
                materialHolder.SetActive(true);
            }

            unDisturbedTime = true;

            // Despawn NPCs from their current locations, then respawn them at the river
            // This makes them instantly appear at the river based on their conditional schedules
            string[] npcIDsToMove = new string[]
            {
                "murid_padepokan_1",
                "murid_padepokan_2",
                "murid_padepokan_3",
                "young_farmer",
                "pemandu_jalan",
                "warga_haus_3"
            };

            foreach (string npcID in npcIDsToMove)
            {
                // Despawn if already spawned
                NPCManager.Instance.DespawnNPC(npcID);

                // Respawn at their current scheduled location (will use conditional schedule with npc_to_river flag)
                NPCManager.Instance.SpawnNPCAtCurrentScheduledLocation(npcID);
            }
        });

        FlagMonitorSystem.WatchFlagAdded("materials_collected", () =>
        {
            NPCInteractionSystem.Instance.AddGameFlag("materials_collected_del");
        });

        FlagMonitorSystem.WatchFlagAdded("materials_collected_del", () =>
        {
            //hapus flag npc_to_river
            NPCInteractionSystem.Instance.RemoveGameFlag("npc_to_river");
            NPCManager.Instance.UpdateNPCSchedules();
        });

        //harus bikin gameobjek yang bisa dibuat muncul sesuai kondisi adanya flag atau engga

        //trigger event dam rusak. setelah objective kelar dan "initial_dam_built" flag ini muncul
        //ada animasi fade yang nunjukin bangunan selesai. MC ngobrol sama murid2 padepokan, sehabis itu selesai, animasi fade
        //pindah tempat, dan cerita berikutnya dimulai, enggak lama dari fade ini akan ada suara ledakan
        //mc bergegas ke dam.
        FlagMonitorSystem.WatchFlagAdded("initial_dam_built", () =>
        {
            // Wait for any ongoing dialogue to finish before executing
            StartCoroutine(WaitForDialogueEndThenTriggerDamBuiltSequence());
        });

        FlagMonitorSystem.WatchFlagAdded("dam_broken", () =>
        {
            //trigger screen shake dan suara ledakan atau bangunan rubuh disini
            //disable movement player selama durasi ledakan
            //munculin monologue setelah ledakan
            MovePlayerTo.Instance.stopPlayerMovement();
            CameraShake.Instance.ShakeExplosion(() =>
            {
                MovePlayerTo.Instance.resumePlayerMovement();
                MonologueSystem.Instance.ShowSimpleMonologue("Astaga!, dentuman kali ini keras sekali. Sebaiknya aku memastikan tidak ada yang terluka disana!", new string[] { "", ""});
            });
            questManager.StartQuest("investigate_dam_destruction");
        });

        // Chapter 4: Mystical Encounters
        FlagMonitorSystem.WatchFlagAdded("spiritual_interference_confirmed", () =>
        {
            questManager.StartQuest("spiritual_vision_encounter");
        });

        //spiritual_vision_active <- iki active, pindah scene dimana
        //ada sequence gelut, dan kalau udah selesai gelut, buaya muncul


        FlagMonitorSystem.WatchFlagAdded("river_spirit_encountered", () =>
        {
            PlayMusic(mysticalMusic);

            if (questManager != null)
            {
                questManager.StartQuest("journey_to_krandon");
            }
        });

        // FlagMonitorSystem.WatchFlagAdded("spirit_pact_complete", () =>
        // {
        //     ShowMessage("Penunggu sungai menerima pemberianmu!");

        //     if (questManager != null)
        //     {
        //         questManager.CompleteQuest("spiritual_vision_encounter");
        //         questManager.StartQuest("complete_spirit_sacrifice");
        //     }
        // });

        FlagMonitorSystem.WatchFlagAdded("arrived_desa_krandon", () =>
        {
            if (questManager != null)
            {
                questManager.StartQuest("negotiate_elephant_loan");
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

            // Show reflective monologue about reconciliation
            // if (MonologueSystem.Instance != null)
            // {
            //     MonologueSystem.Instance.ShowSimpleMonologue(
            //         "Akhirnya kami menemukan jalan damai... Mbok Randa dan aku telah saling memahami. Terkadang konflik lahir dari ketidakpahaman, bukan dari kebencian. Inilah pelajaran berharga yang akan kuingat sepanjang hidup.",
            //         new string[] { "wisdom_gained", "story_reflection" },
            //         "reflect_on_journey", // objective to complete
            //         "story_completion"    // quest that contains the objective
            //     );
            // }

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
            ShowMessage("Petunjuk dan restu dari guru telah diterima!");
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