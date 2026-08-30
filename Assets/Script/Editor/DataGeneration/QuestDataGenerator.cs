using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR

/// <summary>
/// Generates QuestData ScriptableObjects for the main story progression
/// Run this from Tools -> Trenggalek Game -> Generate Quest Data
/// </summary>
public class QuestDataGenerator : EditorWindow
{
    [MenuItem("Tools/Trenggalek Game/Generate Quest Data")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(QuestDataGenerator));
    }

    /// <summary>
    /// Static method to generate all quests without opening the window
    /// </summary>
    public static void GenerateAllQuestsStatic()
    {
        var generator = CreateInstance<QuestDataGenerator>();
        generator.outputPath = "Assets/Resources/Quests/";
        generator.GenerateAllQuests();
        DestroyImmediate(generator);
    }

    private string outputPath = "Assets/Resources/Quests/";
    private bool generateMainQuests = true;
    private bool generateSideQuests = true;

    private void OnGUI()
    {
        titleContent = new GUIContent("Quest Data Generator");
        
        GUILayout.Label("Quest Data Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        generateMainQuests = EditorGUILayout.Toggle("Generate Main Story Quests", generateMainQuests);
        generateSideQuests = EditorGUILayout.Toggle("Generate Side Quests", generateSideQuests);
        
        GUILayout.Space(10);
        outputPath = EditorGUILayout.TextField("Output Path:", outputPath);
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Generate All Quests", GUILayout.Height(40)))
        {
            GenerateAllQuests();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate Main Story Quests Only"))
        {
            GenerateMainStoryQuests();
        }
        
        if (GUILayout.Button("Generate Side Quests Only"))
        {
            GenerateSideQuests();
        }
    }

    private void GenerateAllQuests()
    {
        Debug.Log("Starting Quest Data Generation...");
        
        // Ensure output directory exists
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        if (generateMainQuests)
        {
            GenerateMainStoryQuests();
        }
        
        if (generateSideQuests)
        {
            GenerateSideQuests();
        }
        
        AssetDatabase.Refresh();
        Debug.Log("✅ Quest Data Generation Complete!");
    }

    #region Main Story Quests
    
    private void GenerateMainStoryQuests()
    {
        // Phase 1: Discovery & Commitment
        GenerateWaterCrisisQuest();
        
        // Phase 2: Planning & Assembly
        GenerateSeekGuidanceQuest();
        GenerateGatherHelpersQuest();
        
        // Phase 3: Dam Building & Destruction
        GenerateDamConstructionQuest();
        GenerateInvestigateDamDestructionQuest();
        
        // Phase 4: Spiritual Encounter & Spirit Demand
        GenerateSpiritualVisionQuest();
        
        // Phase 5: Journey to Krandon & White Elephant
        GenerateFindWhiteElephantQuest();
        GenerateJourneyToKrandonQuest();
        GenerateNegotiateElephantQuest();
        
        // Phase 6: Ritual Sacrifice & Water Restoration
        GenerateCompleteSacrificeQuest();
        GenerateWitnessDamSuccessQuest();
        
        // Phase 7: Exposure, Flight & Spirit Rescue
        GenerateFaceMbokRandaAngerQuest();
        GenerateEscapePursuitQuest();
        GenerateRiverSpiritRescueQuest();
        
        // Phase 8: Truth, Reconciliation & Legacy
        GenerateReturnToPadepokanQuest();
        GenerateCompleteTruthTellingQuest();
        GenerateAchieveReconciliationQuest();
        GenerateLandNamingQuest();
        GenerateStoryCompletionQuest();
    }
    
    private void GenerateWaterCrisisQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "water_crisis_discovery";
        quest.questTitle = "Derita Sumur Tua";
        quest.questDescription = "Selidiki laporan tentang penderitaan warga desa di sumur tua. Warga desa sangat membutuhkan air, dan teriakan mereka meminta pertolongan tidak boleh diabaikan.";
        quest.questType = QuestType.Main;
        quest.questLevel = 1;
        
        // Prerequisites
        quest.requiredFlags = new string[] { "story_started" };
        quest.flagsOnStart = new string[] { "water_crisis_discovery_active", "pre_crisis_dialogue_active" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "water_crisis_discovered" };
        quest.flagsToRemoveOnComplete = new string[] { "water_crisis_discovery_active", "pre_crisis_dialogue_active" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        // Objectives
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "witness_crisis",
                description = "Pergi ke sumur desa, dan pahami konflik yang terjadi",
                type = ObjectiveType.Custom,
                targetNPC = "Anak Gembala",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "talk_to_villagers",
                description = "Berbincang dengan warga yang terdampak",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "warga_haus_1",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        // Rewards
        quest.rewards = new List<QuestReward>
        {
            new QuestReward
            {
                type = QuestRewardType.Flags,
                flagsToAdd = new string[] { "committed_to_help" },
                customRewardDescription = "Moral commitment to help the villagers"
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.2f, 0.6f, 1f); // Blue for main quests
        
        SaveQuest(quest, "01_WaterCrisisDiscovery");
    }
    
    private void GenerateSeekGuidanceQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "seek_guru_guidance";
        quest.questTitle = "Kebijksanaan dari guru";
        quest.questDescription = "Konsultasikan dengan Ki Ageng Sinawang mengenai krisis air di desa. Kebijaksanaannya akan membimbing mu ke jalan yang benar.";
        quest.questType = QuestType.Main;
        quest.questLevel = 1;
        
        quest.requiredFlags = new string[] { "water_crisis_discovered" };
        quest.flagsOnStart = new string[] { "seek_guru_guidance_active" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "guru_guidance_received" };
        quest.flagsToRemoveOnComplete = new string[] { "seek_guru_guidance_active" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "consult_ki_ageng",
                description = "Bicaralah dengan Ki Ageng Sinawang tentang krisis yang terjadi.",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "ki_ageng_sinawang",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "receive_permission",
                description = "Minta izin untuk membantu mengatasi masalah yang terjadi.",
                type = ObjectiveType.Custom,
                flagToSetOnComplete = "asked_permission_water_project",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.2f, 0.6f, 1f);
        
        SaveQuest(quest, "02_SeekGuidance");
    }

    private void GenerateGatherHelpersQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "gather_construction_helpers";
        quest.questTitle = "Mencari Bantuan";
        quest.questDescription = "Cari dan rekrutlah orang orang untuk membantu dalam proyek pembangunan bendungan.";
        quest.questType = QuestType.Main;
        quest.questLevel = 2;
        
        quest.requiredFlags = new string[] { "committed_to_help", "guru_guidance_received" };
        quest.flagsOnStart = new string[] { "gathering_helpers_active" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "construction_team_assembled", "helpers_recruited" };
        quest.flagsToRemoveOnComplete = new string[] { "gathering_helpers_active" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "recruit_andi",
                description = "Minta Andi untuk membantu dalam pembangunan.",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "murid_padepokan_1",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "recruit_budi", 
                description = "Yakinkan Budi untuk bergabung dengan tim.",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "murid_padepokan_2",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "recruit_candra",
                description = "Minta persetujuan Candra untuk membantu.",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "murid_padepokan_3",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "recruit_yf",
                description = "Minta bantuan dari petani muda, Bayu",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "young_farmer",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "recruit_joko",
                description = "Mintalah Joko untuk bergabung membangun dam",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "pemandu_jalan",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "recruit_wh",
                description = "Ajak Karto untuk membantu membangun dam",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "warga_haus_3",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.2f, 0.8f, 0.4f); // Green for teamwork
        
        SaveQuest(quest, "02_GatherHelpers");
    }
    
    private void GenerateDamConstructionQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "dam_construction_project";
        quest.questTitle = "Membangun Harapan";
        quest.questDescription = "Bangun bendungan untuk membawa air ke desa yang sedang menderita. Proyek ambisius ini akan membutuhkan kerja sama tim, sumber daya, dan tekad yang kuat.";
        quest.questType = QuestType.Main;
        quest.questLevel = 2;
        
        quest.requiredFlags = new string[] { "asked_permission_water_project", "construction_team_assembled" };
        quest.flagsOnStart = new string[] { "dam_construction_in_progress" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "dam_construction_started", "initial_dam_built" };
        quest.flagsToRemoveOnComplete = new string[] { "dam_construction_in_progress" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "collect_materials",
                description = "Kumpulkan bebatuan dan kayu",
                type = ObjectiveType.CollectItems,
                targetItem = "construction_materials",
                targetAmount = 10,
                flagToSetOnComplete = "materials_collected",
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "build_dam_structure",
                description = "Selesaikan Pembangunan Dam",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "murid_padepokan_2",
                requiredFlags = new string[] { "materials_collected", "materials_collected_del" },
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.rewards = new List<QuestReward>
        {
            new QuestReward
            {
                type = QuestRewardType.Flags,
                flagsToAdd = new string[] { "initial_dam_success" },
                customRewardDescription = "Temporary success and village gratitude"
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.2f, 0.6f, 1f);
        
        SaveQuest(quest, "03_DamConstruction");
    }
    
    private void GenerateInvestigateDamDestructionQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "investigate_dam_destruction";
        quest.questTitle = "Sabotasi Misterius";
        quest.questDescription = "Temukan alasan mengapa bendungan hancur terus menerus. Ada kekuatan tak kasat mata yang berperan, dan kebenaran harus diungkap.";
        quest.questType = QuestType.Main;
        quest.questLevel = 3;
        
        quest.requiredFlags = new string[] { "initial_dam_built" };
        quest.flagsOnStart = new string[] { "investigating_dam_active" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "dam_repeatedly_destroyed", "spiritual_interference_confirmed" };
        quest.flagsToRemoveOnComplete = new string[] { "investigating_dam_active" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "examine_destruction",
                description = "Selidiki lokasi bendungan yang hancur",
                type = ObjectiveType.VisitLocation,
                targetLocation = "DamSite",
                flagToSetOnComplete = "done_examine",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "talk_to_witnesses",
                description = "Diskusikan kepada murid padepokan tentang apa yang terjadi",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "murid_padepokan_2",
                requiredFlags = new string[] { "done_examine" },
                flagToSetOnComplete = "talked_with_witness",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.6f, 0.2f, 0.8f); // Purple for supernatural elements
        
        SaveQuest(quest, "04_InvestigateDestruction");
    }
    
    private void GenerateSpiritualVisionQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "spiritual_vision_encounter";
        quest.questTitle = "Investigasi Kerusakan Dam";
        quest.questDescription = "Investigasilah sekitar area dam untuk mencari tahu hal apa yang menyebabkan gagalnya dam dibangun";
        quest.questType = QuestType.Main;
        quest.questLevel = 4;
        
        quest.requiredFlags = new string[] { "spiritual_interference_confirmed" };
        quest.flagsOnStart = new string[] { "seeking_spiritual_vision" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "river_spirit_encountered", "tribute_demand_received" };
        quest.flagsToRemoveOnComplete = new string[] { "seeking_spiritual_vision" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "perform_ritual",
                description = "Bersemedilah di sekitar dam",
                type = ObjectiveType.VisitLocation,
                targetLocation = "DamSite",
                flagToSetOnComplete = "spiritual_vision_active",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "defeat_monster",
                description = "Kalahkan Rintangan!",
                type = ObjectiveType.DefeatEnemies,
                targetItem = "slime",
                targetAmount = 3,
                requiredFlags = new string[] { "spiritual_vision_active" },
                flagToSetOnComplete = "monsters_defeated",
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "confront_river_spirit",
                description = "Berbicaralah dengan penunggu sungai",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "buaya_putih",
                requiredFlags = new string[] { "monsters_defeated" },
                flagToSetOnComplete = "keberadaan_gajah_putih",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "locate_white_elephant",
                description = "Cari tahu keberadaan Gajah Putih",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "murid_padepokan_1",
                requiredFlags = new string[] { "monsters_defeated" },
                flagToSetOnComplete = "keberadaan_gajah",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.6f, 0.2f, 0.8f);
        
        SaveQuest(quest, "05_SpiritualVision");
    }

    private void GenerateJourneyToKrandonQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "journey_to_krandon";
        quest.questTitle = "Perjalanan ke Desa Krandon";
        quest.questDescription = "Pergilah ke desa Krandon dengan selamat untuk mendapatkan Gajah Putih";
        quest.questType = QuestType.Main;
        quest.questLevel = 5;
        
        quest.requiredFlags = new string[] { "tribute_demand_received" };
        quest.flagsOnStart = new string[] { "journeying_to_krandon" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "arrived_desa_krandon" };
        quest.flagsToRemoveOnComplete = new string[] { "journeying_to_krandon", "seeking_white_elephant" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "looking_for_way",
                description = "Cari tahu untuk ke desa Krandon",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "pemandu_jalan",
                flagToSetOnComplete = "path_obtained",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "forest_travel",
                description = "Telusuri hutan dengan aman",
                type = ObjectiveType.VisitLocation,
                targetLocation = "ForestPathFinal",
                requiredFlags = new string[] { "path_obtained" },
                flagToSetOnComplete = "finish_forest",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = false;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.6f, 0.4f, 0.2f); // Brown for travel
        
        SaveQuest(quest, "05_JourneyToKrandon");
    }
    
    private void GenerateFindWhiteElephantQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "find_white_elephant";
        quest.questTitle = "Sacred Beast of Legend";
        quest.questDescription = "Kumpulkan informasi untuk mencari keberadaan gajah putih.";
        quest.questType = QuestType.Main;
        quest.questLevel = 5;
        
        quest.requiredFlags = new string[] { "accepted_spirit_demand" };
        quest.flagsOnStart = new string[] { "seeking_white_elephant" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "seeking_white_elephant", "ready_for_journey" };
        quest.flagsToRemoveOnComplete = new string[] { };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "gather_information",
                description = "Pelajari dimana keberadaan gajah putih",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "pemandu_jalan",
                flagToSetOnComplete = "heard_white_elephant_legend",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "find_location",
                description = "Discover where the white elephant can be found",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "pemandu_jalan",
                flagToSetOnComplete = "krandon_location_discovered",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "hire_guide",
                description = "Secure guide to Desa Krandon",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "pemandu_jalan",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(1f, 0.8f, 0.2f); // Gold for legendary quest
        
        SaveQuest(quest, "06_FindWhiteElephant");
    }

    private void GenerateNegotiateElephantQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "negotiate_elephant_loan";
        quest.questTitle = "\"Meminjam\" Gajah Putih";
        quest.questDescription = "Berbicaralah dengan mbok Randa agar bisa meminjamkan gajah putihnya";
        quest.questType = QuestType.Main;
        quest.questLevel = 6;
        
        quest.requiredFlags = new string[] { "arrived_desa_krandon" };
        quest.flagsOnStart = new string[] { "negotiating_with_mbok_randa" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "white_elephant_borrowed", "mbok_randa_trusts_player" };
        quest.flagsToRemoveOnComplete = new string[] { "negotiating_with_mbok_randa" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "meet_mbok_randa",
                description = "Berbicaralah dengan mbok randa",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "mbok_randa",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = false; // Requires dialogue completion
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.8f, 0.6f, 0.8f); // Purple for negotiation
        
        SaveQuest(quest, "06_NegotiateElephant");
    }
    
    private void GenerateCompleteSacrificeQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "complete_spirit_sacrifice";
        quest.questTitle = "Persembahan Gajah Putih";
        quest.questDescription = "Penuhi permintaan dari penjaga sungai";
        quest.questType = QuestType.Main;
        quest.questLevel = 7;
        
        quest.requiredFlags = new string[] { "white_elephant_borrowed" };
        quest.flagsOnStart = new string[] { "performing_sacrifice_active" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "elephant_sacrifice_complete", "spirit_pact_complete", "white_elephant_taken" };
        quest.flagsToRemoveOnComplete = new string[] { "performing_sacrifice_active", "white_elephant_borrowed" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "talk_to_joko",
                description = "Paman Joko ingin berbicara",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "pemandu_jalan",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "perform_sacrifice",
                description = "Selesaikan proses pengorbanan",
                type = ObjectiveType.VisitLocation,
                targetLocation = "GajahRiver",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = false; // Requires manual completion due to moral weight
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.8f, 0.2f, 0.2f); // Red for moral crisis
        
        SaveQuest(quest, "07_CompleteSacrifice");
    }

    private void GenerateWitnessDamSuccessQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "witness_dam_success";
        quest.questTitle = "Waters of Life";
        quest.questDescription = "Lihat bagaimana efek dari pembangunan dam oleh masyarakat sekitar padepokan";
        quest.questType = QuestType.Main;
        quest.questLevel = 7;
        
        quest.requiredFlags = new string[] { "spirit_pact_complete" };
        quest.flagsOnStart = new string[] { "witnessing_dam_success_active" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "dam_construction_complete", "village_water_restored" };
        quest.flagsToRemoveOnComplete = new string[] { "witnessing_dam_success_active" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "check_village_well",
                description = "Pastikan air di sumur telah diisi oleh warga",
                type = ObjectiveType.VisitLocation,
                targetLocation = "VillageWell",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "speak_with_farmers",
                description = "Lihat bagaimana air membantu kebun warga sekitar",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "pak_tani",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.2f, 0.6f, 1f); // Blue for water success
        
        SaveQuest(quest, "07_WitnessDamSuccess");
    }
    
    private void GenerateFaceMbokRandaAngerQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "face_mbok_randa_anger";
        quest.questTitle = "Kebenaran";
        quest.questDescription = "Kebenaran yang terungkap...";
        quest.questType = QuestType.Main;
        quest.questLevel = 8;
        
        quest.requiredFlags = new string[] { "elephant_sacrifice_complete" };
        quest.flagsOnStart = new string[] { "penjaga_spawn", "facing_mbok_randa_active" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "mbok_randa_angry", "truth_exposed" };
        quest.flagsToRemoveOnComplete = new string[] { "facing_mbok_randa_active" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "return_to_krandon",
                description = "Kembalilah ke padepokan",
                type = ObjectiveType.VisitLocation,
                targetLocation = "Padepokan",
                flagToSetOnComplete = "mbr_cutscene",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "face_confrontation",
                description = "Jelaskan hal hal kepada Mbok Randa",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "mbok_randa_krandon",
                flagToSetOnComplete = "elephant_sacrifice_revealed",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.8f, 0.2f, 0.2f); // Red for anger/confrontation
        
        SaveQuest(quest, "08_FaceMbokRandaAnger");
    }
    
    private void GenerateEscapePursuitQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "escape_krandon_pursuit";
        quest.questTitle = "Flight from Justice";
        quest.questDescription = "Berlarilah dan hindari kejaran!";
        quest.questType = QuestType.Main;
        quest.questLevel = 9;
        
        quest.requiredFlags = new string[] { "mbok_randa_angry" };
        quest.flagsOnStart = new string[] { "chase_sequence_active" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "reached_river_escape" };
        quest.flagsToRemoveOnComplete = new string[] { "chase_sequence_active", "mbok_randa_angry" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "evade_pursuers",
                description = "Avoid capture by the angry villagers",
                type = ObjectiveType.Custom,
                flagToSetOnComplete = "chase_sequence_active",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "reach_river",
                description = "Make it to the river crossing",
                type = ObjectiveType.VisitLocation,
                targetLocation = "RiverCrossing",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.6f, 0.3f, 0.1f); // Dark brown for escape
        
        SaveQuest(quest, "09_EscapePursuit");
    }
    
    private void GenerateRiverSpiritRescueQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "river_spirit_rescue";
        quest.questTitle = "Salvation from the Depths";
        quest.questDescription = "Face drowning and receive unexpected rescue from the river spirit.";
        quest.questType = QuestType.Main;
        quest.questLevel = 10;
        
        quest.requiredFlags = new string[] { "reached_river_escape" };
        quest.flagsOnStart = new string[] { "river_spirit_rescuing" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "rescued_by_crocodile", "spirit_protection_granted" };
        quest.flagsToRemoveOnComplete = new string[] { "river_spirit_rescuing", "reached_river_escape" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "attempt_river_crossing",
                description = "Try to cross the dangerous river",
                type = ObjectiveType.Custom,
                flagToSetOnComplete = "drowning_in_river",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "receive_spirit_aid",
                description = "Be rescued by the white crocodile spirit",
                type = ObjectiveType.Custom,
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.4f, 0.8f, 0.9f); // Light blue for spirit rescue
        
        SaveQuest(quest, "10_RiverSpiritRescue");
    }
    
    private void GenerateReturnToPadepokanQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "return_to_padepokan";
        quest.questTitle = "Homecoming";
        quest.questDescription = "Return safely to the padepokan after the ordeal.";
        quest.questType = QuestType.Main;
        quest.questLevel = 11;
        
        quest.requiredFlags = new string[] { "rescued_by_crocodile" };
        quest.flagsOnStart = new string[] { "returning_to_padepokan" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "returned_home_safely", "story_events_reported" };
        quest.flagsToRemoveOnComplete = new string[] { "returning_to_padepokan", "rescued_by_crocodile" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "reach_padepokan",
                description = "Arrive safely at the padepokan",
                type = ObjectiveType.VisitLocation,
                targetLocation = "Padepokan",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "report_to_guru",
                description = "Tell Ki Ageng what happened",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "ki_ageng_sinawang",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "seek_mother_comfort",
                description = "Find solace with Raden Ayu Saraswati",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "raden_ayu_saraswati",
                targetAmount = 1,
                showProgress = true,
                isOptional = true
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.9f, 0.7f, 0.4f); // Warm yellow for homecoming
        
        SaveQuest(quest, "11_ReturnToPadepokan");
    }
    
    private void GenerateCompleteTruthTellingQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "complete_truth_telling";
        quest.questTitle = "The Whole Truth";
        quest.questDescription = "Reveal the complete story to achieve understanding.";
        quest.questType = QuestType.Main;
        quest.questLevel = 12;
        
        quest.requiredFlags = new string[] { "confronted_at_padepokan" };
        quest.flagsOnStart = new string[] { "telling_truth_active" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "complete_story_told", "remorse_expressed" };
        quest.flagsToRemoveOnComplete = new string[] { "telling_truth_active", "returned_home_safely" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "explain_water_crisis",
                description = "Describe the village's desperate situation",
                type = ObjectiveType.Custom,
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "explain_spirit_demands",
                description = "Reveal the river spirit's ultimatum",
                type = ObjectiveType.Custom,
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "express_remorse",
                description = "Show genuine regret for the deception",
                type = ObjectiveType.Custom,
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = false; // Requires careful dialogue choices
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.9f, 0.9f, 0.3f); // Yellow for truth/honesty
        
        SaveQuest(quest, "12_CompleteTruthTelling");
    }

    private void GenerateAchieveReconciliationQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "achieve_reconciliation";
        quest.questTitle = "Healing the Wounds";
        quest.questDescription = "Work toward mutual understanding and peace with Mbok Randa. Through truth and genuine remorse, healing is possible.";
        quest.questType = QuestType.Main;
        quest.questLevel = 9;
        
        quest.requiredFlags = new string[] { "sincere_apology_given" };
        quest.flagsOnStart = new string[] { "seeking_reconciliation_active" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "reconciliation_complete", "mutual_understanding_achieved" };
        quest.flagsToRemoveOnComplete = new string[] { "seeking_reconciliation_active", "sincere_apology_given" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "demonstrate_remorse",
                description = "Show continued commitment to making amends",
                type = ObjectiveType.Custom,
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "accept_consequences",
                description = "Accept responsibility for all actions",
                type = ObjectiveType.Custom,
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "find_mutual_understanding",
                description = "Reach peace with Mbok Randa",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "mbok_randa_krandon",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(0.2f, 0.8f, 0.4f); // Green for resolution
        
        SaveQuest(quest, "08_AchieveReconciliation");
    }
    
    private void GenerateLandNamingQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "land_naming_ceremony";
        quest.questTitle = "Teranging Galih";
        quest.questDescription = "Witness the naming of the land in honor of understanding.";
        quest.questType = QuestType.Main;
        quest.questLevel = 13;
        
        quest.requiredFlags = new string[] { "reconciliation_complete" };
        quest.flagsOnStart = new string[] { "naming_land_active" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "teranging_galih_named", "land_naming_complete" };
        quest.flagsToRemoveOnComplete = new string[] { "naming_land_active" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "attend_ceremony",
                description = "Participate in the land naming ceremony",
                type = ObjectiveType.VisitLocation,
                targetLocation = "VillageCenter",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "hear_mbok_randa_declaration",
                description = "Listen to Mbok Randa's pronouncement",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "mbok_randa_krandon",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(1f, 0.8f, 0.2f); // Gold for the naming/legacy
        
        SaveQuest(quest, "13_LandNaming");
    }

    private void GenerateStoryCompletionQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "story_completion";
        quest.questTitle = "Lessons Learned";
        quest.questDescription = "Reflect on the journey and its lessons. The story of Teranging Galih - the brightness of understanding - has come to its conclusion.";
        quest.questType = QuestType.Main;
        quest.questLevel = 10;
        
        quest.requiredFlags = new string[] { "land_naming_complete" };
        quest.flagsOnStart = new string[] { "completing_story_active" };
        quest.flagsToRemoveOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "story_completed", "wisdom_gained" };
        quest.flagsToRemoveOnComplete = new string[] { "completing_story_active", "land_naming_complete" };
        quest.flagsOnFail = new string[] { };
        quest.flagsToRemoveOnFail = new string[] { };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "final_guru_wisdom",
                description = "Receive final wisdom from Ki Ageng Sinawang",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "ki_ageng_sinawang",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "mother_pride",
                description = "Share the conclusion with Raden Ayu Saraswati",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "raden_ayu_saraswati",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            }
        };
        
        quest.rewards = new List<QuestReward>
        {
            new QuestReward
            {
                type = QuestRewardType.Custom,
                customRewardDescription = "Complete understanding of the folklore and moral growth"
            }
        };
        
        quest.autoComplete = true;
        quest.canAbandon = true;
        quest.showInJournal = true;
        quest.trackByDefault = true;
        quest.questColor = new Color(1f, 0.8f, 0.2f); // Gold for completion
        
        SaveQuest(quest, "09_StoryCompletion");
    }
    
    #endregion
    
    #region Side Quests
    
    private void GenerateSideQuests()
    {
        GenerateVillageHelpQuests();
        GenerateFarmingQuests();
        GenerateCulturalLearningQuests();
        GenerateRelationshipQuests();
    }
    
    private void GenerateVillageHelpQuests()
    {
        // Rice Harvest Quest
        var riceHarvestQuest = ScriptableObject.CreateInstance<QuestData>();
        riceHarvestQuest.questID = "village_rice_harvest";
        riceHarvestQuest.questTitle = "Hands of the Harvest";
        riceHarvestQuest.questDescription = "Help Pak Tani with the rice harvest. Your assistance will strengthen bonds with the farming community.";
        riceHarvestQuest.questType = QuestType.Side;
        riceHarvestQuest.questLevel = 2;
        
        riceHarvestQuest.requiredFlags = new string[] { "water_crisis_discovered" };
        riceHarvestQuest.flagsOnStart = new string[] { };
        riceHarvestQuest.flagsToRemoveOnStart = new string[] { };
        riceHarvestQuest.flagsOnComplete = new string[] { "village_rice_harvest_complete" };
        riceHarvestQuest.flagsToRemoveOnComplete = new string[] { };
        riceHarvestQuest.flagsOnFail = new string[] { };
        riceHarvestQuest.flagsToRemoveOnFail = new string[] { };
        
        riceHarvestQuest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "talk_to_pak_tani",
                description = "Speak with Pak Tani about helping with harvest",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "pak_tani",
                targetAmount = 1,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "harvest_rice_bundles",
                description = "Harvest rice in the fields",
                type = ObjectiveType.CollectItems,
                targetItem = "rice_bundles",
                targetAmount = 15,
                showProgress = true,
                isOptional = false
            }
        };
        
        riceHarvestQuest.rewards = new List<QuestReward>
        {
            new QuestReward
            {
                type = QuestRewardType.Flags,
                flagsToAdd = new string[] { "pak_tani_harvest_accepted", "established_village_reputation" },
                customRewardDescription = "Improved standing with village farmers"
            }
        };
        
        riceHarvestQuest.isRepeatable = true;
        riceHarvestQuest.canAbandon = true;
        riceHarvestQuest.autoComplete = true;
        riceHarvestQuest.showInJournal = true;
        riceHarvestQuest.trackByDefault = true;
        riceHarvestQuest.questColor = new Color(0.4f, 0.8f, 0.2f); // Green for village quests
        
        SaveQuest(riceHarvestQuest, "Side_RiceHarvest");
        
        // Additional village help quests...
        Debug.Log("Generated village help side quests");
    }
    
    private void GenerateFarmingQuests()
    {
        Debug.Log("Generating farming side quests...");
        // Implementation for farming-related side quests
    }
    
    private void GenerateCulturalLearningQuests()
    {
        Debug.Log("Generating cultural learning side quests...");
        // Implementation for cultural education side quests
    }
    
    private void GenerateRelationshipQuests()
    {
        Debug.Log("Generating relationship side quests...");
        // Implementation for NPC relationship building quests
    }
    
    #endregion
    
    #region Utility Methods
    
    private void SaveQuest(QuestData quest, string filename)
    {
        string path = Path.Combine(outputPath, $"{filename}.asset");
        AssetDatabase.CreateAsset(quest, path);
        Debug.Log($"✅ Created Quest: {path}");
    }
    
    #endregion
}

#endif