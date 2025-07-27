using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR

/// <summary>
/// Generates and manages story flags data for the Trenggalek folklore game
/// Run this from Tools -> Trenggalek Game -> Story Flag Manager
/// </summary>
public class StoryFlagManager : EditorWindow
{
    [MenuItem("Tools/Trenggalek Game/Story Flag Manager")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(StoryFlagManager));
    }

    /// <summary>
    /// Static method to generate flag definitions without opening the window
    /// </summary>
    public static void GenerateFlagDefinitionsStatic()
    {
        var generator = CreateInstance<StoryFlagManager>();
        generator.flagDefinitionPath = "Assets/Resources/Data/StoryFlags.json";
        generator.GenerateFlagDefinitionFile();
        DestroyImmediate(generator);
    }

    private string flagDefinitionPath = "Assets/Resources/Data/StoryFlags.json";
    private Vector2 scrollPosition;

    private void OnGUI()
    {
        titleContent = new GUIContent("Story Flag Manager");
        
        GUILayout.Label("Story Flag Management System", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        flagDefinitionPath = EditorGUILayout.TextField("Flag Definition Path:", flagDefinitionPath);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate Flag Definition File", GUILayout.Height(30)))
        {
            GenerateFlagDefinitionFile();
        }
        
        if (GUILayout.Button("Validate All Story Flags", GUILayout.Height(30)))
        {
            ValidateAllStoryFlags();
        }
        
        if (GUILayout.Button("Export Flag Documentation", GUILayout.Height(30)))
        {
            ExportFlagDocumentation();
        }
        
        GUILayout.Space(20);
        
        GUILayout.Label("Flag System Status", EditorStyles.boldLabel);
        
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        
        // Display flag statistics and validation results
        DisplayFlagSystemStatus();
        
        GUILayout.EndScrollView();
    }

    #region Flag Definition Generation
    
    [System.Serializable]
    public class StoryFlagDefinition
    {
        public string flagName;
        public string category;
        public string description;
        public string setBy;
        public string[] dependencies;
        public string[] unlocks;
        public bool isPlayerChoice;
        public bool isPermanent;
        public string[] mutuallyExclusive;
        public string phase;
    }
    
    [System.Serializable]
    public class StoryFlagCollection
    {
        public StoryFlagDefinition[] flags;
        public string version;
        public string lastUpdated;
    }
    
    private void GenerateFlagDefinitionFile()
    {
        Debug.Log("Generating Story Flag Definition File...");
        
        var flagCollection = new StoryFlagCollection
        {
            version = "1.0",
            lastUpdated = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            flags = CreateAllStoryFlags()
        };
        
        // Ensure output directory exists
        string directory = Path.GetDirectoryName(flagDefinitionPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        string json = JsonUtility.ToJson(flagCollection, true);
        File.WriteAllText(flagDefinitionPath, json);
        
        AssetDatabase.Refresh();
        Debug.Log($"✅ Story Flag Definition generated: {flagDefinitionPath}");
    }
    
    private StoryFlagDefinition[] CreateAllStoryFlags()
    {
        var flags = new List<StoryFlagDefinition>();
        
        // Phase 1: Discovery & Commitment
        flags.Add(new StoryFlagDefinition
        {
            flagName = "story_started",
            category = "Core Progression",
            description = "Player has begun the main story",
            setBy = "Game initialization",
            dependencies = new string[] { },
            unlocks = new string[] { "Initial NPC interactions", "world exploration" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 1: Discovery"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "water_crisis_discovered",
            category = "Core Progression",
            description = "Player becomes aware of the village's water shortage",
            setBy = "Dialogue with Warga Haus 1 (villager at well)",
            dependencies = new string[] { "story_started" },
            unlocks = new string[] { "Crisis-related dialogue options", "seek_guru_guidance quest" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 1: Discovery"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "committed_to_help",
            category = "Player Choice",
            description = "Player makes moral commitment to assistance",
            setBy = "Choosing 'I want to help solve this water problem'",
            dependencies = new string[] { "water_crisis_discovered" },
            unlocks = new string[] { "Positive reputation with villagers" },
            isPlayerChoice = true,
            isPermanent = true,
            mutuallyExclusive = new string[] { "avoided_responsibility" },
            phase = "Phase 1: Discovery"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "avoided_responsibility",
            category = "Player Choice",
            description = "Player initially avoids involvement",
            setBy = "Choosing 'This isn't my responsibility'",
            dependencies = new string[] { "water_crisis_discovered" },
            unlocks = new string[] { "Modified NPC reactions" },
            isPlayerChoice = true,
            isPermanent = true,
            mutuallyExclusive = new string[] { "committed_to_help" },
            phase = "Phase 1: Discovery"
        });
        
        // Phase 2: Planning & Construction
        flags.Add(new StoryFlagDefinition
        {
            flagName = "guru_guidance_received",
            category = "Story Milestone",
            description = "Player has consulted their spiritual teacher",
            setBy = "Completion of dialogue with Ki Ageng Sinawang about crisis",
            dependencies = new string[] { "water_crisis_discovered" },
            unlocks = new string[] { "Access to padepokan resources and students" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 2: Planning"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "asked_permission_water_project",
            category = "Story Progression",
            description = "Formal authorization to proceed with dam construction",
            setBy = "Requesting Ki Ageng's permission to help with water project",
            dependencies = new string[] { "guru_guidance_received" },
            unlocks = new string[] { "dam_construction_project quest" },
            isPlayerChoice = true,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 2: Planning"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "dam_construction_started",
            category = "Core Progression",
            description = "Major world state change - construction is underway",
            setBy = "Beginning dam building work",
            dependencies = new string[] { "materials_gathered" },
            unlocks = new string[] { "Modified NPC schedules", "Environmental changes" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 2: Construction"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "initial_dam_built",
            category = "Story Milestone",
            description = "Dam structure is complete and functional",
            setBy = "Completion of first dam construction",
            dependencies = new string[] { "dam_construction_started" },
            unlocks = new string[] { "Temporary success period", "village celebration" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 2: Construction"
        });
        
        // Phase 3: Supernatural Opposition
        flags.Add(new StoryFlagDefinition
        {
            flagName = "dam_repeatedly_destroyed",
            category = "Core Progression",
            description = "Supernatural opposition becomes clear",
            setBy = "Pattern of mysterious dam destructions",
            dependencies = new string[] { "initial_dam_built" },
            unlocks = new string[] { "investigate_dam_destruction quest", "Supernatural dialogue" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 3: Opposition"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "spiritual_interference_confirmed",
            category = "Story Revelation",
            description = "Player understands the nature of the opposition",
            setBy = "Investigation revealing supernatural cause",
            dependencies = new string[] { "dam_repeatedly_destroyed" },
            unlocks = new string[] { "spiritual_vision_encounter quest" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 3: Opposition"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "river_spirit_encountered",
            category = "Story Milestone",
            description = "Direct contact with the supernatural antagonist",
            setBy = "First dialogue with Buaya Putih",
            dependencies = new string[] { "spiritual_vision_active" },
            unlocks = new string[] { "Understanding of spirit's demands" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 3: Opposition"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "accepted_spirit_demand",
            category = "Player Choice",
            description = "Player commits to the sacrifice path",
            setBy = "Agreeing to find the white elephant",
            dependencies = new string[] { "tribute_demand_received" },
            unlocks = new string[] { "find_white_elephant quest" },
            isPlayerChoice = true,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 4: Quest"
        });
        
        // Phase 4: The Sacred Quest
        flags.Add(new StoryFlagDefinition
        {
            flagName = "arrived_desa_krandon",
            category = "Location Progress",
            description = "Player is in position to find the white elephant",
            setBy = "Successfully reaching Desa Krandon",
            dependencies = new string[] { "guide_hired" },
            unlocks = new string[] { "Interaction with Mbok Randa Krandon" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 4: Quest"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "promised_safe_return",
            category = "Player Commitment (Deceptive)",
            description = "Gaining Mbok Randa's trust through false promise",
            setBy = "Promising to return the elephant safely",
            dependencies = new string[] { "explained_water_crisis" },
            unlocks = new string[] { "Access to the white elephant" },
            isPlayerChoice = true,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 4: Quest"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "white_elephant_borrowed",
            category = "Resource Obtained",
            description = "Player has the required sacrifice",
            setBy = "Successfully convincing Mbok Randa to lend elephant",
            dependencies = new string[] { "promised_safe_return" },
            unlocks = new string[] { "Return journey and sacrifice quest" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 4: Quest"
        });
        
        // Phase 5: The Sacrifice
        flags.Add(new StoryFlagDefinition
        {
            flagName = "elephant_sacrifice_complete",
            category = "Core Progression (Moral Crisis)",
            description = "The required tribute has been paid",
            setBy = "Completing the ritual sacrifice of white elephant",
            dependencies = new string[] { "white_elephant_borrowed" },
            unlocks = new string[] { "Spirit cooperation", "dam functionality" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 5: Sacrifice"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "spirit_pact_complete",
            category = "Supernatural Agreement",
            description = "River spirit will no longer destroy the dam",
            setBy = "Buaya Putih accepting the sacrifice",
            dependencies = new string[] { "elephant_sacrifice_complete" },
            unlocks = new string[] { "Permanent dam functionality" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 5: Sacrifice"
        });
        
        // Phase 6: Discovery & Pursuit
        flags.Add(new StoryFlagDefinition
        {
            flagName = "elephant_sacrifice_revealed",
            category = "Truth Exposure",
            description = "The deception is exposed",
            setBy = "Mbok Randa discovering what happened to her elephant",
            dependencies = new string[] { "white_elephant_taken" },
            unlocks = new string[] { "Anger", "pursuit", "confrontation" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 6: Reckoning"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "rescued_by_crocodile",
            category = "Supernatural Intervention",
            description = "Spirit honors the pact by protecting the player",
            setBy = "Buaya Putih saving Menak Sopal from drowning",
            dependencies = new string[] { "drowning_in_river" },
            unlocks = new string[] { "Safe return", "spiritual protection confirmed" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 6: Reckoning"
        });
        
        // Phase 7: Truth & Reconciliation
        flags.Add(new StoryFlagDefinition
        {
            flagName = "sincere_apology_given",
            category = "Moral Choice",
            description = "Player takes responsibility and shows growth",
            setBy = "Offering genuine remorse and apology",
            dependencies = new string[] { "full_truth_explained" },
            unlocks = new string[] { "Path to forgiveness and reconciliation" },
            isPlayerChoice = true,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 7: Truth"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "reconciliation_complete",
            category = "Core Resolution",
            description = "Main conflict resolved peacefully",
            setBy = "Achieving mutual understanding and forgiveness",
            dependencies = new string[] { "remorse_expressed" },
            unlocks = new string[] { "Final ceremony and naming event" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 7: Resolution"
        });
        
        // Phase 8: Resolution & Legacy
        flags.Add(new StoryFlagDefinition
        {
            flagName = "teranging_galih_named",
            category = "Cultural Legacy",
            description = "Story becomes part of local legend",
            setBy = "Mbok Randa's declaration of the land name",
            dependencies = new string[] { "reconciliation_complete" },
            unlocks = new string[] { "Final story completion" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 8: Legacy"
        });
        
        flags.Add(new StoryFlagDefinition
        {
            flagName = "story_completed",
            category = "Main Story Complete",
            description = "Player has experienced complete narrative arc",
            setBy = "All major story beats concluded",
            dependencies = new string[] { "land_naming_complete" },
            unlocks = new string[] { "Post-story content", "Achievement recognition" },
            isPlayerChoice = false,
            isPermanent = true,
            mutuallyExclusive = new string[] { },
            phase = "Phase 8: Legacy"
        });
        
        return flags.ToArray();
    }
    
    #endregion
    
    #region Flag Validation
    
    private void ValidateAllStoryFlags()
    {
        Debug.Log("🔍 Validating Story Flag System...");
        
        var flags = CreateAllStoryFlags();
        var flagNames = new HashSet<string>();
        var issues = new List<string>();
        
        // Check for duplicate flag names
        foreach (var flag in flags)
        {
            if (flagNames.Contains(flag.flagName))
            {
                issues.Add($"❌ Duplicate flag name: {flag.flagName}");
            }
            else
            {
                flagNames.Add(flag.flagName);
            }
        }
        
        // Check flag dependencies
        foreach (var flag in flags)
        {
            if (flag.dependencies != null)
            {
                foreach (var dependency in flag.dependencies)
                {
                    if (!flagNames.Contains(dependency))
                    {
                        issues.Add($"❌ Flag '{flag.flagName}' depends on non-existent flag '{dependency}'");
                    }
                }
            }
            
            // Check mutual exclusivity
            if (flag.mutuallyExclusive != null)
            {
                foreach (var exclusive in flag.mutuallyExclusive)
                {
                    if (!flagNames.Contains(exclusive))
                    {
                        issues.Add($"❌ Flag '{flag.flagName}' mutually exclusive with non-existent flag '{exclusive}'");
                    }
                }
            }
        }
        
        // Check for circular dependencies
        foreach (var flag in flags)
        {
            if (HasCircularDependency(flag, flags, new HashSet<string>()))
            {
                issues.Add($"❌ Circular dependency detected involving flag: {flag.flagName}");
            }
        }
        
        // Display results
        if (issues.Count == 0)
        {
            Debug.Log($"✅ Story Flag System Validation Complete! {flags.Length} flags validated successfully.");
        }
        else
        {
            Debug.LogError($"❌ Story Flag System Validation Found {issues.Count} Issues:");
            foreach (var issue in issues)
            {
                Debug.LogError(issue);
            }
        }
    }
    
    private bool HasCircularDependency(StoryFlagDefinition flag, StoryFlagDefinition[] allFlags, HashSet<string> visited)
    {
        if (visited.Contains(flag.flagName))
        {
            return true; // Circular dependency found
        }
        
        visited.Add(flag.flagName);
        
        if (flag.dependencies != null)
        {
            foreach (var dependency in flag.dependencies)
            {
                var dependentFlag = System.Array.Find(allFlags, f => f.flagName == dependency);
                if (dependentFlag != null && HasCircularDependency(dependentFlag, allFlags, new HashSet<string>(visited)))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    #endregion
    
    #region Documentation Export
    
    private void ExportFlagDocumentation()
    {
        var flags = CreateAllStoryFlags();
        var markdown = GenerateFlagMarkdownDocumentation(flags);
        
        string exportPath = "Assets/Documentation/Generated_Story_Flags_Reference.md";
        string directory = Path.GetDirectoryName(exportPath);
        
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        File.WriteAllText(exportPath, markdown);
        AssetDatabase.Refresh();
        
        Debug.Log($"✅ Flag documentation exported to: {exportPath}");
    }
    
    private string GenerateFlagMarkdownDocumentation(StoryFlagDefinition[] flags)
    {
        var markdown = new System.Text.StringBuilder();
        
        markdown.AppendLine("# Story Flags Reference (Generated)");
        markdown.AppendLine();
        markdown.AppendLine($"Generated on: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        markdown.AppendLine($"Total Flags: {flags.Length}");
        markdown.AppendLine();
        
        // Group flags by phase
        var phases = new Dictionary<string, List<StoryFlagDefinition>>();
        foreach (var flag in flags)
        {
            if (!phases.ContainsKey(flag.phase))
            {
                phases[flag.phase] = new List<StoryFlagDefinition>();
            }
            phases[flag.phase].Add(flag);
        }
        
        foreach (var phase in phases)
        {
            markdown.AppendLine($"## {phase.Key}");
            markdown.AppendLine();
            
            foreach (var flag in phase.Value)
            {
                markdown.AppendLine($"### `{flag.flagName}`");
                markdown.AppendLine($"**Category:** {flag.category}");
                markdown.AppendLine($"**Description:** {flag.description}");
                markdown.AppendLine($"**Set By:** {flag.setBy}");
                
                if (flag.dependencies != null && flag.dependencies.Length > 0)
                {
                    markdown.AppendLine($"**Dependencies:** {string.Join(", ", flag.dependencies)}");
                }
                
                if (flag.unlocks != null && flag.unlocks.Length > 0)
                {
                    markdown.AppendLine($"**Unlocks:** {string.Join(", ", flag.unlocks)}");
                }
                
                if (flag.mutuallyExclusive != null && flag.mutuallyExclusive.Length > 0)
                {
                    markdown.AppendLine($"**Mutually Exclusive:** {string.Join(", ", flag.mutuallyExclusive)}");
                }
                
                markdown.AppendLine($"**Player Choice:** {(flag.isPlayerChoice ? "Yes" : "No")}");
                markdown.AppendLine($"**Permanent:** {(flag.isPermanent ? "Yes" : "No")}");
                markdown.AppendLine();
            }
        }
        
        return markdown.ToString();
    }
    
    #endregion
    
    #region Status Display
    
    private void DisplayFlagSystemStatus()
    {
        var flags = CreateAllStoryFlags();
        
        GUILayout.Label($"Total Story Flags: {flags.Length}", EditorStyles.label);
        
        // Count by category
        var categories = new Dictionary<string, int>();
        var phases = new Dictionary<string, int>();
        int playerChoices = 0;
        int permanentFlags = 0;
        
        foreach (var flag in flags)
        {
            // Count categories
            if (!categories.ContainsKey(flag.category))
                categories[flag.category] = 0;
            categories[flag.category]++;
            
            // Count phases
            if (!phases.ContainsKey(flag.phase))
                phases[flag.phase] = 0;
            phases[flag.phase]++;
            
            // Count special types
            if (flag.isPlayerChoice) playerChoices++;
            if (flag.isPermanent) permanentFlags++;
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Categories:", EditorStyles.boldLabel);
        foreach (var category in categories)
        {
            GUILayout.Label($"  {category.Key}: {category.Value}");
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Phases:", EditorStyles.boldLabel);
        foreach (var phase in phases)
        {
            GUILayout.Label($"  {phase.Key}: {phase.Value}");
        }
        
        GUILayout.Space(10);
        GUILayout.Label($"Player Choice Flags: {playerChoices}");
        GUILayout.Label($"Permanent Flags: {permanentFlags}");
    }
    
    #endregion
}

#endif