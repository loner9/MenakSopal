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
        // Chapter 1-2: Discovery Phase
        GenerateWaterCrisisQuest();
        GenerateSeekGuidanceQuest();
        
        // Chapter 3: Construction Phase
        GenerateDamConstructionQuest();
        GenerateGatherHelpersQuest();
        
        // Chapter 4: Opposition Phase
        GenerateInvestigateDamDestructionQuest();
        GenerateSpiritualVisionQuest();
        
        // Chapter 5-6: Quest Phase
        GenerateFindWhiteElephantQuest();
        GenerateJourneyToKrandonQuest();
        GenerateNegotiateElephantQuest();
        
        // Chapter 7: Sacrifice Phase
        GenerateCompleteSacrificeQuest();
        GenerateWitnessDamSuccessQuest();
        
        // Chapter 8: Reckoning Phase
        GenerateFaceMbokRandaAngerQuest();
        GenerateEscapePursuitQuest();
        GenerateRiverSpiritRescueQuest();
        
        // Chapter 9: Resolution Phase
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
        quest.questTitle = "Voices of Thirst";
        quest.questDescription = "Investigate reports of suffering villagers at the old well. The people are in desperate need of water, and their cries for help cannot be ignored.";
        quest.questType = QuestType.Main;
        quest.questLevel = 1;
        
        // Prerequisites
        quest.requiredFlags = new string[] { "story_started" };
        quest.flagsOnStart = new string[] { };
        quest.flagsOnComplete = new string[] { "water_crisis_discovered" };
        
        // Objectives
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "reach_village_well",
                description = "Travel to the village well",
                type = ObjectiveType.VisitLocation,
                targetLocation = "VillageWell",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "talk_to_villagers",
                description = "Speak with the suffering villagers",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "warga_haus_1",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "witness_crisis",
                description = "Understand the extent of the water shortage",
                type = ObjectiveType.Custom,
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
        quest.trackByDefault = true;
        quest.questColor = new Color(0.2f, 0.6f, 1f); // Blue for main quests
        
        SaveQuest(quest, "01_WaterCrisisDiscovery");
    }
    
    private void GenerateSeekGuidanceQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "seek_guru_guidance";
        quest.questTitle = "Wisdom of the Teacher";
        quest.questDescription = "Consult Ki Ageng Sinawang about the village's water crisis. Your spiritual teacher's wisdom will guide you on the right path.";
        quest.questType = QuestType.Main;
        quest.questLevel = 1;
        
        quest.requiredFlags = new string[] { "water_crisis_discovered" };
        quest.flagsOnComplete = new string[] { "guru_guidance_received" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "consult_ki_ageng",
                description = "Speak with Ki Ageng Sinawang about the crisis",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "ki_ageng_sinawang",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "receive_permission",
                description = "Ask for permission to help with the water project",
                type = ObjectiveType.Custom,
                flagToSetOnComplete = "asked_permission_water_project",
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.questColor = new Color(0.2f, 0.6f, 1f);
        
        SaveQuest(quest, "02_SeekGuidance");
    }
    
    private void GenerateDamConstructionQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "dam_construction_project";
        quest.questTitle = "Building Hope";
        quest.questDescription = "Construct a dam to bring water to the suffering village. This ambitious project will require teamwork, resources, and determination.";
        quest.questType = QuestType.Main;
        quest.questLevel = 2;
        
        quest.requiredFlags = new string[] { "asked_permission_water_project" };
        quest.flagsOnComplete = new string[] { "dam_construction_started", "initial_dam_built" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "gather_students",
                description = "Recruit padepokan students to help",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "murid_padepokan_1",
                flagToSetOnComplete = "students_recruited",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "collect_materials",
                description = "Gather stones and wood for construction",
                type = ObjectiveType.CollectItems,
                targetItem = "construction_materials",
                targetAmount = 10,
                showProgress = true,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "build_dam_structure",
                description = "Complete the dam construction",
                type = ObjectiveType.Custom,
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
        quest.questColor = new Color(0.2f, 0.6f, 1f);
        
        SaveQuest(quest, "03_DamConstruction");
    }
    
    private void GenerateInvestigateDamDestructionQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "investigate_dam_destruction";
        quest.questTitle = "Mysterious Sabotage";
        quest.questDescription = "Discover why the dam keeps being destroyed overnight. Something supernatural is at work, and the truth must be uncovered.";
        quest.questType = QuestType.Main;
        quest.questLevel = 3;
        
        quest.requiredFlags = new string[] { "initial_dam_built" };
        quest.flagsOnComplete = new string[] { "dam_repeatedly_destroyed", "spiritual_interference_confirmed" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "examine_destruction",
                description = "Investigate the destroyed dam site",
                type = ObjectiveType.VisitLocation,
                targetLocation = "DamSite",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "talk_to_witnesses",
                description = "Question students about what they saw",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "murid_padepokan_2",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "consult_shaman",
                description = "Seek spiritual guidance from village shaman",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "dukun_kampung",
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.questColor = new Color(0.6f, 0.2f, 0.8f); // Purple for supernatural elements
        
        SaveQuest(quest, "04_InvestigateDestruction");
    }
    
    private void GenerateSpiritualVisionQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "spiritual_vision_encounter";
        quest.questTitle = "Communion with the River Spirit";
        quest.questDescription = "Enter spiritual communion to understand the supernatural opposition. Face the ancient guardian of the waters.";
        quest.questType = QuestType.Main;
        quest.questLevel = 4;
        
        quest.requiredFlags = new string[] { "spiritual_interference_confirmed" };
        quest.flagsOnComplete = new string[] { "river_spirit_encountered", "tribute_demand_received" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "perform_ritual",
                description = "Complete the spiritual ritual with village shaman",
                type = ObjectiveType.Custom,
                flagToSetOnComplete = "spiritual_vision_active",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "confront_river_spirit",
                description = "Face the guardian of the river",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "buaya_putih_spirit",
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.questColor = new Color(0.6f, 0.2f, 0.8f);
        
        SaveQuest(quest, "05_SpiritualVision");
    }
    
    private void GenerateFindWhiteElephantQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "find_white_elephant";
        quest.questTitle = "Sacred Beast of Legend";
        quest.questDescription = "Locate the legendary white elephant required by the river spirit. This sacred creature holds the key to appeasing the supernatural forces.";
        quest.questType = QuestType.Main;
        quest.questLevel = 5;
        
        quest.requiredFlags = new string[] { "accepted_spirit_demand" };
        quest.flagsOnComplete = new string[] { "seeking_white_elephant", "ready_for_journey" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "gather_information",
                description = "Learn about white elephant legends",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "nenek_bijak",
                flagToSetOnComplete = "heard_white_elephant_legend",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "find_location",
                description = "Discover where the white elephant can be found",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "pemandu_jalan",
                flagToSetOnComplete = "krandon_location_discovered",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "hire_guide",
                description = "Secure guide to Desa Krandon",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "pemandu_jalan",
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.questColor = new Color(1f, 0.8f, 0.2f); // Gold for legendary quest
        
        SaveQuest(quest, "06_FindWhiteElephant");
    }
    
    private void GenerateCompleteSacrificeQuest()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "complete_spirit_sacrifice";
        quest.questTitle = "The Sacred Offering";
        quest.questDescription = "Complete the river spirit's demanded sacrifice. This terrible choice will weigh heavily on your conscience, but it may be the only way to save the village.";
        quest.questType = QuestType.Main;
        quest.questLevel = 7;
        
        quest.requiredFlags = new string[] { "white_elephant_borrowed" };
        quest.flagsOnComplete = new string[] { "elephant_sacrifice_complete", "spirit_pact_complete", "white_elephant_taken" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "bring_elephant_to_river",
                description = "Lead the white elephant to the river shrine",
                type = ObjectiveType.VisitLocation,
                targetLocation = "RiverShrine",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "perform_sacrifice",
                description = "Complete the spiritual ritual",
                type = ObjectiveType.Custom,
                isOptional = false
            }
        };
        
        quest.autoComplete = false; // Requires manual completion due to moral weight
        quest.questColor = new Color(0.8f, 0.2f, 0.2f); // Red for moral crisis
        
        SaveQuest(quest, "07_CompleteSacrifice");
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
        quest.flagsOnComplete = new string[] { "reconciliation_complete", "mutual_understanding_achieved" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "demonstrate_remorse",
                description = "Show continued commitment to making amends",
                type = ObjectiveType.Custom,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "accept_consequences",
                description = "Accept responsibility for all actions",
                type = ObjectiveType.Custom,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "find_mutual_understanding",
                description = "Reach peace with Mbok Randa",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "mbok_randa_krandon",
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.questColor = new Color(0.2f, 0.8f, 0.4f); // Green for resolution
        
        SaveQuest(quest, "08_AchieveReconciliation");
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
        quest.flagsOnComplete = new string[] { "story_completed", "wisdom_gained" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "final_guru_wisdom",
                description = "Receive final wisdom from Ki Ageng Sinawang",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "ki_ageng_sinawang",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "mother_pride",
                description = "Share the conclusion with Raden Ayu Saraswati",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "raden_ayu_saraswati",
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
        quest.questColor = new Color(1f, 0.8f, 0.2f); // Gold for completion
        
        SaveQuest(quest, "09_StoryCompletion");
    }
    
    // Additional quest generation methods...
    private void GenerateGatherHelpersQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "gather_construction_helpers";
        quest.questTitle = "Assembling the Team";
        quest.questDescription = "Recruit padepokan students to help with the dam construction project.";
        quest.questType = QuestType.Main;
        quest.questLevel = 2;
        
        quest.requiredFlags = new string[] { "students_permission_granted" };
        quest.flagsOnComplete = new string[] { "construction_team_assembled", "helpers_recruited" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "recruit_andi",
                description = "Ask Andi to help with construction",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "murid_padepokan_1",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "recruit_budi", 
                description = "Convince Budi to join the team",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "murid_padepokan_2",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "recruit_candra",
                description = "Get Candra's agreement to help",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "murid_padepokan_3",
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.questColor = new Color(0.2f, 0.8f, 0.4f); // Green for teamwork
        
        SaveQuest(quest, "02_GatherHelpers");
    }
    
    private void GenerateJourneyToKrandonQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "journey_to_krandon";
        quest.questTitle = "Path to the Sacred Beast";
        quest.questDescription = "Travel safely to Desa Krandon where the white elephant lives.";
        quest.questType = QuestType.Main;
        quest.questLevel = 5;
        
        quest.requiredFlags = new string[] { "ready_for_journey" };
        quest.flagsOnComplete = new string[] { "arrived_desa_krandon" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "forest_travel",
                description = "Navigate the forest path safely",
                type = ObjectiveType.VisitLocation,
                targetLocation = "ForestPath",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "arrive_krandon",
                description = "Reach Desa Krandon safely",
                type = ObjectiveType.VisitLocation,
                targetLocation = "DesaKrandon", 
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.questColor = new Color(0.6f, 0.4f, 0.2f); // Brown for travel
        
        SaveQuest(quest, "05_JourneyToKrandon");
    }
    
    private void GenerateNegotiateElephantQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "negotiate_elephant_loan";
        quest.questTitle = "Convincing the Owner";
        quest.questDescription = "Persuade Mbok Randa to lend her precious white elephant.";
        quest.questType = QuestType.Main;
        quest.questLevel = 6;
        
        quest.requiredFlags = new string[] { "arrived_desa_krandon" };
        quest.flagsOnComplete = new string[] { "white_elephant_borrowed", "mbok_randa_trusts_player" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "meet_mbok_randa",
                description = "Introduce yourself to the elephant's owner",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "mbok_randa_krandon",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "explain_situation",
                description = "Explain the water crisis situation",
                type = ObjectiveType.Custom,
                flagToSetOnComplete = "explained_water_crisis",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "secure_agreement",
                description = "Obtain permission to borrow the elephant",
                type = ObjectiveType.Custom,
                isOptional = false
            }
        };
        
        quest.autoComplete = false; // Requires dialogue completion
        quest.questColor = new Color(0.8f, 0.6f, 0.8f); // Purple for negotiation
        
        SaveQuest(quest, "06_NegotiateElephant");
    }
    
    private void GenerateWitnessDamSuccessQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "witness_dam_success";
        quest.questTitle = "Waters of Life";
        quest.questDescription = "See the positive results of the successful dam after the spirit sacrifice.";
        quest.questType = QuestType.Main;
        quest.questLevel = 7;
        
        quest.requiredFlags = new string[] { "spirit_pact_complete" };
        quest.flagsOnComplete = new string[] { "dam_construction_complete", "village_water_restored" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "check_village_well",
                description = "Confirm water has returned to the village",
                type = ObjectiveType.VisitLocation,
                targetLocation = "VillageWell",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "speak_with_farmers",
                description = "See how the water helps agriculture",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "pak_tani",
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.questColor = new Color(0.2f, 0.6f, 1f); // Blue for water success
        
        SaveQuest(quest, "07_WitnessDamSuccess");
    }
    
    private void GenerateFaceMbokRandaAngerQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "face_mbok_randa_anger";
        quest.questTitle = "The Price of Deception";
        quest.questDescription = "Confront Mbok Randa's fury over the white elephant's fate.";
        quest.questType = QuestType.Main;
        quest.questLevel = 8;
        
        quest.requiredFlags = new string[] { "elephant_sacrifice_complete" };
        quest.flagsOnComplete = new string[] { "mbok_randa_angry", "truth_exposed" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "return_to_krandon",
                description = "Return to face Mbok Randa's questions",
                type = ObjectiveType.VisitLocation,
                targetLocation = "DesaKrandon",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "face_confrontation",
                description = "Endure Mbok Randa's anger and accusations",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "mbok_randa_krandon",
                flagToSetOnComplete = "elephant_sacrifice_revealed",
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.questColor = new Color(0.8f, 0.2f, 0.2f); // Red for anger/confrontation
        
        SaveQuest(quest, "08_FaceMbokRandaAnger");
    }
    
    private void GenerateEscapePursuitQuest() 
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questID = "escape_krandon_pursuit";
        quest.questTitle = "Flight from Justice";
        quest.questDescription = "Escape the angry villagers of Krandon pursuing you.";
        quest.questType = QuestType.Main;
        quest.questLevel = 9;
        
        quest.requiredFlags = new string[] { "mbok_randa_angry" };
        quest.flagsOnComplete = new string[] { "reached_river_escape" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "evade_pursuers",
                description = "Avoid capture by the angry villagers",
                type = ObjectiveType.Custom,
                flagToSetOnComplete = "chase_sequence_active",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "reach_river",
                description = "Make it to the river crossing",
                type = ObjectiveType.VisitLocation,
                targetLocation = "RiverCrossing",
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
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
        quest.flagsOnComplete = new string[] { "rescued_by_crocodile", "spirit_protection_granted" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "attempt_river_crossing",
                description = "Try to cross the dangerous river",
                type = ObjectiveType.Custom,
                flagToSetOnComplete = "drowning_in_river",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "receive_spirit_aid",
                description = "Be rescued by the white crocodile spirit",
                type = ObjectiveType.Custom,
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
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
        quest.flagsOnComplete = new string[] { "returned_home_safely", "story_events_reported" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "reach_padepokan",
                description = "Arrive safely at the padepokan",
                type = ObjectiveType.VisitLocation,
                targetLocation = "Padepokan",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "report_to_guru",
                description = "Tell Ki Ageng what happened",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "ki_ageng_sinawang",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "seek_mother_comfort",
                description = "Find solace with Raden Ayu Saraswati",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "raden_ayu_saraswati",
                isOptional = true
            }
        };
        
        quest.autoComplete = true;
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
        quest.flagsOnComplete = new string[] { "complete_story_told", "remorse_expressed" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "explain_water_crisis",
                description = "Describe the village's desperate situation",
                type = ObjectiveType.Custom,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "explain_spirit_demands",
                description = "Reveal the river spirit's ultimatum",
                type = ObjectiveType.Custom,
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "express_remorse",
                description = "Show genuine regret for the deception",
                type = ObjectiveType.Custom,
                isOptional = false
            }
        };
        
        quest.autoComplete = false; // Requires careful dialogue choices
        quest.questColor = new Color(0.9f, 0.9f, 0.3f); // Yellow for truth/honesty
        
        SaveQuest(quest, "12_CompleteTruthTelling");
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
        quest.flagsOnComplete = new string[] { "teranging_galih_named", "land_naming_complete" };
        
        quest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "attend_ceremony",
                description = "Participate in the land naming ceremony",
                type = ObjectiveType.VisitLocation,
                targetLocation = "VillageCenter",
                isOptional = false
            },
            new QuestObjective
            {
                objectiveID = "hear_mbok_randa_declaration",
                description = "Listen to Mbok Randa's pronouncement",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "mbok_randa_krandon",
                isOptional = false
            }
        };
        
        quest.autoComplete = true;
        quest.questColor = new Color(1f, 0.8f, 0.2f); // Gold for the naming/legacy
        
        SaveQuest(quest, "13_LandNaming");
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
        riceHarvestQuest.flagsOnComplete = new string[] { "village_rice_harvest_complete" };
        
        riceHarvestQuest.objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                objectiveID = "talk_to_pak_tani",
                description = "Speak with Pak Tani about helping with harvest",
                type = ObjectiveType.TalkToNPC,
                targetNPC = "pak_tani",
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
        riceHarvestQuest.autoComplete = true;
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