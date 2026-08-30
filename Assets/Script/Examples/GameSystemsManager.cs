using System.Collections;
using System.Collections.Generic;
using MenakSopal.Audio;
using MenakSopal.Cutscenes;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Example implementation showing how different game systems can react to flag changes.
/// This demonstrates the FlagMonitorSystem integration with various game systems.
/// 
/// Attach this to a GameObject in your scene to see automatic reactions to story flags.
/// </summary>
public class GameSystemsManager : MonoBehaviour
{
    [System.Serializable]
    public class RespawnCheckpoint
    {
        public string flagName;
        public string spawnPointName;
        [Tooltip("Optional: message to show when respawning")]
        public string respawnMessage;
    }

    [Header("Checkpoint System")]
    public List<RespawnCheckpoint> respawnCheckpoints = new List<RespawnCheckpoint>();
    public float respawnDelay = 2.0f;

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
    InvisibleObjectHolder invisibleObjectHolder;

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

        invisibleObjectHolder = GetComponent<InvisibleObjectHolder>();

        if (ttsButton != null)
        {
            Debug.Log("[GameSystems] Setting up TTS button...");
            ttsButton.onClick.AddListener(MovePlayerTo.Instance.movePlayerWithDelay);
        }

        SetupFlagReactions();
        SetupSubAreaEventHandlers();
        SetupDeathDetection();

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

    // void LateUpdate()
    // {
    //     string sceneName = SceneManager.GetActiveScene().name;
    //     if (sceneName != "SceneAwal") return;
    //     if (DayNightCycle.Instance.CurrentTime >= 22f && DayNightCycle.Instance.CurrentTime <= 22.4f)
    //     {
    //         DayNightCycle.Instance.SetTime(22.5f);
    //         DayNightCycle.Instance.PauseTime();
    //         MovePlayerTo.Instance.stopPlayerMovement();
    //         ttsCanvas.SetActive(true);
    //     }
    // }

    IEnumerator WaitForDialogueEndThenShowMonologue()
    {
        // Wait until player finishes the current dialogue naturally
        while (NPCInteractionSystem.Instance != null &&
               NPCInteractionSystem.Instance.IsInDialogue())
        {
            yield return null; // Wait one frame and check again
        }

        // If a cutscene is already playing (e.g. transisiKali), it handles the full
        // post-recruitment sequence (monologue → to_river → teleport) itself.
        // Don't interfere — bail out early.
        var cutsceneController = FindObjectOfType<MenakSopal.Cutscenes.CutsceneController>();
        if (cutsceneController != null && cutsceneController.IsPlaying)
        {
            Debug.Log("[GameSystemsManager] Cutscene is active — skipping post-recruitment monologue (cutscene handles it).");
            yield break;
        }

        Debug.Log("[GameSystemsManager] Dialogue ended, waiting 0.5-1 second before showing monologue");

        // Add delay between dialogue end and monologue start (0.5-1 second)
        yield return new WaitForSeconds(2.35f);

        // Check again after delay — cutscene may have started during the wait
        if (cutsceneController != null && cutsceneController.IsPlaying)
        {
            Debug.Log("[GameSystemsManager] Cutscene started during wait — skipping post-recruitment monologue.");
            yield break;
        }

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
        MonologueSystem.Instance.ShowSimpleMonologue("aku telah selesai meminta izin dan mengumpulkan bala bantuan untuk membangun dam ini. Saatnya untuk menuju ke sungai!", new string[] { "to_river", "npc_to_river" });
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
        MonologueSystem.Instance.ShowSimpleMonologue("Hmm, aku telah selesai meminta izin dan mengumpulkan bala bantuan untuk membangun dam ini. Saatnya untuk menuju ke sungai!", new string[] { "to_river", "npc_to_river" });
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
        MovePlayerTo.Instance.movePlayerWithDestinationFade("TempatMCKali", () =>
        {
            // Called after teleport completes
            Debug.Log("[GameSystemsManager] Teleport to TempatMCKali completed");

            // Add flag for Andi's comment
            NPCInteractionSystem.Instance.AddGameFlag("andi_comment_after_dam");

            // Wait a bit more before triggering dialogue (adjusted from 4.5s since teleport already waited 2s)
            Invoke("dialogueWithAndiAfterDam", 2.3f);

            // Ketika ngobrol belum selesai (secara sistem diselesaiin), tiba tiba ada suara ledakan dan
            // nanti ada semacam screen shake. Ketika dialog selesai maka akan ditambahkan game flag baru untuk trigger next quest
        });
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
                FlagManager.Instance.AddFlag("perkenalan");
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
                FlagManager.Instance.RemoveFlag("perkenalan");
            }
        });

        // Chapter 3: Helper Recruitment
        // setelah ini harus e ada sequence pindah dari map biasa ke map bangun bendungan


        // FlagMonitorSystem.WatchFlagAdded("to_river", () =>
        // {
        //     MovePlayerTo.Instance.movePlayerWithDestinationFade("BantaranKali");
        // });


        FlagMonitorSystem.WatchFlagAdded("npc_to_river", () =>
        {
            Debug.Log("npc_to_river flag added");
            // MovePlayerTo.Instance.movePlayerWithDestinationFade("BantaranKali");
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

        FlagMonitorSystem.WatchFlagRemoved("materials_collected_del", () =>
        {
            NPCManager.Instance.UpdateNPCSchedules();
            Debug.Log("materials_collected_del flag removed");

            // Despawn NPCs from their current locations, then respawn them at the river
            // This makes them instantly appear at the river based on their conditional schedules
            string[] npcIDsToMove = new string[]
            {
                "murid_padepokan_1",
                "murid_padepokan_2",
                "murid_padepokan_3",
                "young_farmer",
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

        //harus bikin gameobjek yang bisa dibuat muncul sesuai kondisi adanya flag atau engga

        //trigger event dam rusak. setelah objective kelar dan "initial_dam_built" flag ini muncul
        //ada animasi fade yang nunjukin bangunan selesai. MC ngobrol sama murid2 padepokan, sehabis itu selesai, animasi fade
        //pindah tempat, dan cerita berikutnya dimulai, enggak lama dari fade ini akan ada suara ledakan
        //mc bergegas ke dam.
        // FlagMonitorSystem.WatchFlagAdded("initial_dam_built", () =>
        // {
        //     // Wait for any ongoing dialogue to finish before executing
        //     StartCoroutine(WaitForDialogueEndThenTriggerDamBuiltSequence());
        // });

        // FlagMonitorSystem.WatchFlagAdded("dam_broken", () =>
        // {
        //     //trigger screen shake dan suara ledakan atau bangunan rubuh disini
        //     //disable movement player selama durasi ledakan
        //     //munculin monologue setelah ledakan
        //     MovePlayerTo.Instance.stopPlayerMovement();
        //     CameraShake.Instance.ShakeExplosion(() =>
        //     {
        //         MovePlayerTo.Instance.resumePlayerMovement();
        //         MonologueSystem.Instance.ShowSimpleMonologue("Astaga!, dentuman kali ini keras sekali. Sebaiknya aku memastikan tidak ada yang terluka disana!", new string[] { "", "" });
        //     });
        //     questManager.StartQuest("investigate_dam_destruction");
        // });

        // Chapter 4: Mystical Encounters
        FlagMonitorSystem.WatchFlagAdded("spiritual_interference_confirmed", () =>
        {
            questManager.StartQuest("spiritual_vision_encounter");
        });

        //spiritual_vision_active <- iki active, pindah scene dimana
        //ada sequence gelut, dan kalau udah selesai gelut, buaya muncul

        FlagMonitorSystem.WatchFlagAdded("fog_active", () =>
        {
            //todo: pindah ke scene spiritual plane -> done
            GameObject fogLayer = GameObject.FindGameObjectWithTag("Fog");
            if (fogLayer != null)
            {
                SpriteRenderer spriteRenderer = fogLayer.GetComponent<SpriteRenderer>();
                spriteRenderer.enabled = true;
            }
        });

        FlagMonitorSystem.WatchFlagRemoved("fog_active", () =>
        {
            //todo: pindah ke scene spiritual plane -> done
            GameObject fogLayer = GameObject.FindGameObjectWithTag("Fog");
            if (fogLayer != null)
            {
                SpriteRenderer spriteRenderer = fogLayer.GetComponent<SpriteRenderer>();
                spriteRenderer.enabled = false;
            }
        });

        // FlagMonitorSystem.WatchFlagAdded("mc_done_talking", () =>
        // {
        //     ShowMessage("Kalahkan rintangan yang ada!");
        //     //todo : enable enemies container
        //     GameObject enemiesContainer = GameObject.FindGameObjectWithTag("EnemiesContainer");
        //     if (enemiesContainer != null)
        //     {
        //         enemiesContainer.SetActive(true);
        //     }
        //     else
        //     {
        //         Debug.Log("Engga ada");
        //     }
        // });

        // FlagMonitorSystem.WatchFlagAdded("monsters_defeated", () =>
        // {
        //     // ShowMessage("Selamat! Anda berhasil mengalahkan semua rintangan!");
        //     CameraShake.Instance.ShakeMedium(() =>
        //     {
        //         // spawn buaya putih

        //         // NPCManager.Instance.DespawnNPC("buaya_putih_spirit");
        //         NPCManager.Instance.SpawnNPCAtCurrentScheduledLocation("buaya_putih_spirit");
        //     });
        // });

        FlagMonitorSystem.WatchFlagAdded("accepted_spirit_demand", () =>
        {


        });

        FlagMonitorSystem.WatchFlagAdded("tribute_demand_received", () =>
        {
            // PlayMusic(mysticalMusic);

            if (questManager != null)
            {
                questManager.StartQuest("journey_to_krandon");
            }
        });


        FlagMonitorSystem.WatchFlagAdded("arrived_desa_krandon", () =>
        {
            if (questManager != null)
            {
                questManager.StartQuest("negotiate_elephant_loan");
            }
        });

        FlagMonitorSystem.WatchFlagAdded("white_elephant_borrowed", () =>
        {
            // ShowMessage("Penunggu sungai menerima pemberianmu!");

            if (questManager != null)
            {
                // questManager.CompleteQuest("spiritual_vision_encounter");
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

        FlagMonitorSystem.WatchFlagAdded("in_forest", () =>
        {
            LogReaction("Player entered forest area");
            UpdateCameraBound(setCameraConfiner("HutanConfiner"));
        });

        FlagMonitorSystem.WatchFlagAdded("finish_forest", () =>
        {
            LogReaction("Player exited forest area");
            UpdateCameraBound(setCameraConfiner("KrandonConfiner"));
        });
    }

    #region Death and Respawn System

    private void SetupDeathDetection()
    {
        // Find player and subscribe to death event
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnDeath.AddListener(HandlePlayerDeath);
                Debug.Log("[GameSystems] Subscribed to PlayerHealth.OnDeath");
            }
        }
    }

    private void HandlePlayerDeath()
    {
        Debug.Log("[GameSystems] Player death detected. Initiating respawn sequence...");
        StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {
        // Wait for potential death animation to play
        yield return new WaitForSeconds(respawnDelay);

        // Find the latest checkpoint flag
        var interactionSystem = NPCInteractionSystem.Instance;
        if (interactionSystem == null) yield break;

        var currentFlags = interactionSystem.GetGameFlags();
        RespawnCheckpoint latestCheckpoint = null;
        int latestFlagIndex = -1;

        // Iterate through registered checkpoints to find which one matches the latest acquired flag
        foreach (var checkpoint in respawnCheckpoints)
        {
            int index = currentFlags.IndexOf(checkpoint.flagName);
            if (index != -1 && index > latestFlagIndex)
            {
                latestFlagIndex = index;
                latestCheckpoint = checkpoint;
            }
        }

        if (latestCheckpoint != null)
        {
            Debug.Log($"[GameSystems] Respawning at checkpoint: {latestCheckpoint.flagName} -> {latestCheckpoint.spawnPointName}");

            // 1. Fade to black
            if (CutsceneController.Instance != null)
            {
                yield return StartCoroutine(CutsceneController.Instance.FadeScreen(true, 1.0f));
            }

            // 2. Rollback flags to the checkpoint
            interactionSystem.RollbackFlags(latestCheckpoint.flagName);

            // 3. Move player and Revive
            if (MovePlayerTo.Instance != null)
            {
                bool moveComplete = false;
                MovePlayerTo.Instance.movePlayerWithDestinationFade(latestCheckpoint.spawnPointName, () => moveComplete = true);

                // Wait for teleport (including internal delays in movePlayerWithDestinationFade)
                while (!moveComplete) yield return null;

                // Revive player
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.Revive();
                    }
                }
            }

            // 4. Fade from black
            if (CutsceneController.Instance != null)
            {
                yield return StartCoroutine(CutsceneController.Instance.FadeScreen(false, 1.0f));
            }

            // 5. Show respawn message
            if (!string.IsNullOrEmpty(latestCheckpoint.respawnMessage))
            {
                ShowMessage(latestCheckpoint.respawnMessage);
            }
        }
        else
        {
            Debug.LogWarning("[GameSystems] No checkpoint flag found! Player cannot be respawned automatically.");
            // Optional: Fallback to a default location or reload scene
        }
    }

    #endregion

    #region Sub-Area Event Handlers

    void SetupSubAreaEventHandlers()
    {
        OnSubAreaLoaded += HandleSubAreaLoaded;
        OnSubAreaUnloaded += HandleSubAreaUnloaded;
        OnSubAreaRestarted += HandleSubAreaRestarted;


        Debug.Log("[GameSystems] Sub-area event handlers registered");
    }

    void HandleSubAreaLoaded(string sceneName)
    {
        Debug.Log($"[GameSystems] Sub-area loaded: {sceneName}");

        // Handle specific scenes

        switch (sceneName)
        {
            case "SpiritualPlane":
                StartCoroutine(InitializeSpiritualPlane());
                break;

                // Add more cases for other sub-areas as needed
                // case "MiniGame":
                //     StartCoroutine(InitializeMiniGame());
                //     break;

        }
    }

    void HandleSubAreaUnloaded(string sceneName)
    {
        Debug.Log($"[GameSystems] Sub-area unloaded: {sceneName}");

        // Cleanup logic if needed

        switch (sceneName)
        {
            case "SpiritualPlane":
                NPCInteractionSystem.Instance.RemoveGameFlag("mc_done_talking");
                break;
        }
    }

    void HandleSubAreaRestarted(string sceneName)
    {
        Debug.Log($"[GameSystems] Sub-area restarted: {sceneName}");
        switch (sceneName)
        {
            case "SpiritualPlane":
                NPCInteractionSystem.Instance.RemoveGameFlag("mc_done_talking");
                StartCoroutine(SpiritualPlaneRestarted());
                break;
        }
    }

    IEnumerator SpiritualPlaneRestarted()
    {
        // Wait a moment for scene to fully initialize
        yield return new WaitForSeconds(0.5f);


        Debug.Log("[GameSystems] Spiritual Plane restarted...");
        MonologueSystem.Instance.ShowSimpleMonologue("Sepertinya aku harus menyelesaikan semua rintangan agar bisa keluar!", new string[] { "mc_done_talking" });
    }

    IEnumerator InitializeSpiritualPlane()
    {
        // Wait a moment for scene to fully initialize
        yield return new WaitForSeconds(0.5f);

        GameObject light2d = GameObject.FindGameObjectWithTag("LightSpiritual");
        // light2d.SetActive(true);
        DayNightCycle.Instance.SetTime(20f);
        DayNightCycle.Instance.PauseTime();
        Debug.Log("[GameSystems] Initializing Spiritual Plane encounter...");
        MonologueSystem.Instance.ShowSimpleMonologue("Tempat ini, sepertinya ini ruang spiritual. Aku harus berhati hati disini!", new string[] { "mc_done_talking" });
    }

    IEnumerator HandleExitSubArea()
    {
        yield return new WaitForSeconds(0.7f);


        DayNightCycle.Instance.SetTimeOfDay(TimeOfDay.Day);
        DayNightCycle.Instance.PauseTime();

        ExitSubArea();
    }

    #endregion

    #region Sub-Area Scene Management

    // Store the current sub-area scene name for unloading

    private string currentSubAreaScene = "";

    // ===== EVENTS =====
    /// <summary>Event fired when a sub-area scene finishes loading.</summary>
    public event System.Action<string> OnSubAreaLoaded;

    /// <summary>Event fired when a sub-area scene finishes unloading.</summary>
    public event System.Action<string> OnSubAreaUnloaded;

    /// <summary>Event fired when a sub-area scene finishes restarting.</summary>
    public event System.Action<string> OnSubAreaRestarted;

    // ===== ENTER SUB-AREA =====

    /// <summary>Enter a sub-area scene (no callback).</summary>
    public void EnterSubArea(string subAreaScene)
    {
        StartCoroutine(LoadSubArea(subAreaScene, null));
    }

    /// <summary>Enter a sub-area scene with callback when loading completes.</summary>
    public void EnterSubArea(string subAreaScene, System.Action onComplete)
    {
        StartCoroutine(LoadSubArea(subAreaScene, onComplete));
    }

    IEnumerator LoadSubArea(string subAreaScene, System.Action onComplete)
    {
        // 1. Load the sub-area additively
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(subAreaScene, LoadSceneMode.Additive);
        yield return loadOp;

        // 2. HIDE main scene objects AND disable Light2D components (but keep in memory!)
        GameObject[] sceneRoots = GameObject.FindGameObjectsWithTag("SceneRoot");
        foreach (GameObject root in sceneRoots)
        {
            // Disable all Light2D components to prevent global light conflicts
            var lights = root.GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>(true);
            foreach (var light in lights)
            {
                if (light != null)
                {
                    light.enabled = false;
                    Debug.Log($"[GameSystems] Disabled Light2D: {light.gameObject.name} (Type: {light.lightType})");
                }
            }

            root.SetActive(false);
        }

        // 3. Set sub-area as active scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(subAreaScene));

        // 4. Track the current sub-area
        currentSubAreaScene = subAreaScene;

        Debug.Log($"[GameSystems] Entered sub-area: {subAreaScene}");

        // 5. Fire event and callback
        OnSubAreaLoaded?.Invoke(subAreaScene);
        onComplete?.Invoke();
    }

    // ===== EXIT SUB-AREA =====

    /// <summary>Exit the current sub-area (no callback).</summary>
    public void ExitSubArea()
    {
        ExitSubArea((System.Action)null);
    }

    /// <summary>Exit the current sub-area with callback when unloading completes.</summary>
    public void ExitSubArea(System.Action onComplete)
    {
        if (!string.IsNullOrEmpty(currentSubAreaScene))
        {
            StartCoroutine(UnloadSubArea(currentSubAreaScene, onComplete));
        }
        else
        {
            Debug.LogWarning("[GameSystems] No sub-area scene is currently loaded!");
            onComplete?.Invoke(); // Still call callback even if nothing to unload
        }
    }

    /// <summary>Exit a specific sub-area by name (no callback).</summary>
    public void ExitSubArea(string subAreaScene)
    {
        StartCoroutine(UnloadSubArea(subAreaScene, (System.Action)null));
    }

    /// <summary>Exit a specific sub-area by name with callback.</summary>
    public void ExitSubArea(string subAreaScene, System.Action onComplete)
    {
        StartCoroutine(UnloadSubArea(subAreaScene, onComplete));
    }

    IEnumerator UnloadSubArea(string subAreaScene, System.Action onComplete)
    {
        // 1. Restore visibility of main scene objects AND re-enable Light2D components
        GameObject[] sceneRoots = GameObject.FindGameObjectsWithTag("SceneRoot");
        foreach (GameObject root in sceneRoots)
        {
            root.SetActive(true);

            // Re-enable all Light2D components

            var lights = root.GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>(true);
            foreach (var light in lights)
            {
                if (light != null)
                {
                    light.enabled = true;
                    Debug.Log($"[GameSystems] Re-enabled Light2D: {light.gameObject.name} (Type: {light.lightType})");
                }
            }
        }

        // 2. Set main scene as active again
        Scene mainScene = SceneManager.GetSceneByName("SceneAwal");
        if (mainScene.IsValid())
        {
            SceneManager.SetActiveScene(mainScene);
        }

        // 3. Unload the sub-area scene
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(subAreaScene);
        yield return unloadOp;

        // 4. Clear the current sub-area reference
        currentSubAreaScene = "";

        Debug.Log($"[GameSystems] Exited sub-area: {subAreaScene}");

        // 5. Fire event and callback
        OnSubAreaUnloaded?.Invoke(subAreaScene);
        onComplete?.Invoke();
    }

    // ===== RESTART SUB-AREA =====

    /// <summary>Restart the current sub-area (no callback).</summary>
    public void RestartSubArea()
    {
        RestartSubArea(null);
    }

    /// <summary>Restart the current sub-area with callback when restart completes.</summary>
    public void RestartSubArea(System.Action onComplete)
    {
        if (!string.IsNullOrEmpty(currentSubAreaScene))
        {
            StartCoroutine(ReloadSubArea(currentSubAreaScene, onComplete));
        }
        else
        {
            Debug.LogWarning("[GameSystems] No sub-area scene to restart!");
            onComplete?.Invoke();
        }
    }

    IEnumerator ReloadSubArea(string subAreaScene, System.Action onComplete)
    {
        // 1. Unload current sub-area
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(subAreaScene);
        yield return unloadOp;

        // 2. Load it fresh
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(subAreaScene, LoadSceneMode.Additive);
        yield return loadOp;

        // 3. Set as active scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(subAreaScene));

        Debug.Log($"[GameSystems] Restarted sub-area: {subAreaScene}");

        // 4. Fire event and callback
        OnSubAreaRestarted?.Invoke(subAreaScene);
        onComplete?.Invoke();
    }

    /// <summary>Get the currently loaded sub-area scene name (empty if none).</summary>
    public string GetCurrentSubArea() => currentSubAreaScene;

    /// <summary>Check if a sub-area is currently loaded.</summary>
    public bool IsInSubArea() => !string.IsNullOrEmpty(currentSubAreaScene);

    #endregion

    private IEnumerator HandleSpiritualVisianEncounter()
    {
        MovePlayerTo.Instance.stopPlayerMovement();

        yield return new WaitForSeconds(0.8f);

        MovePlayerTo.Instance.movePlayerWithDestinationFade("SpiritualPlane", () =>
        {
            MovePlayerTo.Instance.resumePlayerMovement();
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
        if (AudioSystem.Instance != null && clip != null)
        {
            AudioSystem.Instance.PlayMusic(clip);
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

    private Collider2D setCameraConfiner(string confinerName)
    {
        GameObject[] cameraConfiner = GameObject.FindGameObjectsWithTag("Confiner");
        Collider2D confiner = null;
        foreach (GameObject camCon in cameraConfiner)
        {
            Debug.Log("Confiner :" + camCon.name);
            if (camCon.name == confinerName)
            {
                confiner = camCon.GetComponent<Collider2D>();
            }
        }

        return confiner;
    }

    private void UpdateCameraBound(Collider2D confiner)
    {
        if (confiner == null)
        {
            Debug.LogWarning("[GameSystemsManager] Cannot update camera bounds: confiner collider is null");
            return;
        }

        // Try to get the active virtual camera from CameraShake
        CinemachineCamera activeVcam = null;
        if (CameraShake.Instance != null)
        {
            activeVcam = CameraShake.Instance.virtualCamera;
        }

        CinemachineConfiner2D conf = null;
        if (activeVcam != null)
        {
            conf = activeVcam.GetComponent<CinemachineConfiner2D>();
        }

        // Fallback search if not found through CameraShake
        if (conf == null)
        {
            conf = FindObjectOfType<CinemachineConfiner2D>();
            if (conf == null)
            {
                CinemachineCamera virtualCamera = FindObjectOfType<CinemachineCamera>();
                if (virtualCamera != null)
                {
                    conf = virtualCamera.GetComponent<CinemachineConfiner2D>();
                }
            }
        }

        if (conf != null)
        {
            conf.BoundingShape2D = confiner;
            conf.InvalidateBoundingShapeCache();

            // Find the virtual camera to force position
            CinemachineCamera vcam = conf.GetComponent<CinemachineCamera>();
            if (vcam == null)
            {
                vcam = conf.GetComponentInParent<CinemachineCamera>();
            }

            // Snap the camera directly to the player's position to avoid getting stuck outside the new boundary
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null && vcam != null)
            {
                Vector3 targetPos = playerObj.transform.position;
                targetPos.z = vcam.transform.position.z; // Keep camera Z depth

                // Set transform position directly
                vcam.transform.position = targetPos;

                // Force Cinemachine to update its position instantly
                vcam.ForceCameraPosition(targetPos, vcam.transform.rotation);

                Debug.Log($"[GameSystemsManager] Snapped virtual camera '{vcam.name}' to player position: {targetPos}");
            }

            Debug.Log($"[GameSystemsManager] Camera boundary successfully updated to: {confiner.name}");
        }
        else
        {
            Debug.LogError("[GameSystemsManager] CinemachineConfiner2D component not found in the scene! Bounding shape could not be updated.");
        }
    }
}