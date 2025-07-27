using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR

/// <summary>
/// Comprehensive validation system for all generated game data
/// Run this from Tools -> Trenggalek Game -> Validate Game Data
/// </summary>
public class GameDataValidator : EditorWindow
{
    [MenuItem("Tools/Trenggalek Game/Validate Game Data")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(GameDataValidator));
    }

    /// <summary>
    /// Static method to validate all game data without opening the window
    /// </summary>
    public static void ValidateAllGameDataStatic()
    {
        var validator = CreateInstance<GameDataValidator>();
        validator.ValidateAllGameData();
        DestroyImmediate(validator);
    }

    private Vector2 scrollPosition;
    private List<ValidationResult> lastValidationResults = new List<ValidationResult>();

    private void OnGUI()
    {
        titleContent = new GUIContent("Game Data Validator");
        
        GUILayout.Label("Game Data Validation System", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Validate All Game Data", GUILayout.Height(40)))
        {
            ValidateAllGameData();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Validate Dialogue Data Only"))
        {
            ValidateDialogueData();
        }
        
        if (GUILayout.Button("Validate Quest Data Only"))
        {
            ValidateQuestData();
        }
        
        if (GUILayout.Button("Validate Schedule Data Only"))
        {
            ValidateScheduleData();
        }
        
        if (GUILayout.Button("Validate Cross-References"))
        {
            ValidateCrossReferences();
        }
        
        GUILayout.Space(20);
        
        if (lastValidationResults.Count > 0)
        {
            GUILayout.Label("Validation Results:", EditorStyles.boldLabel);
            
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            
            foreach (var result in lastValidationResults)
            {
                Color originalColor = GUI.color;
                GUI.color = GetResultColor(result.severity);
                
                GUILayout.Label($"{GetSeverityIcon(result.severity)} {result.category}: {result.message}");
                
                if (!string.IsNullOrEmpty(result.details))
                {
                    GUILayout.Label($"  Details: {result.details}", EditorStyles.miniLabel);
                }
                
                GUI.color = originalColor;
            }
            
            GUILayout.EndScrollView();
        }
    }

    #region Validation Data Structures
    
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
    
    public class ValidationResult
    {
        public ValidationSeverity severity;
        public string category;
        public string message;
        public string details;
        public string assetPath;
        
        public ValidationResult(ValidationSeverity sev, string cat, string msg, string det = "", string path = "")
        {
            severity = sev;
            category = cat;
            message = msg;
            details = det;
            assetPath = path;
        }
    }
    
    #endregion
    
    #region Main Validation Methods
    
    private void ValidateAllGameData()
    {
        lastValidationResults.Clear();
        
        Debug.Log("🔍 Starting Comprehensive Game Data Validation...");
        
        ValidateDialogueData();
        ValidateQuestData();
        ValidateScheduleData();
        ValidateCrossReferences();
        ValidateAssetIntegrity();
        
        DisplayValidationSummary();
    }
    
    private void ValidateDialogueData()
    {
        Debug.Log("📝 Validating Dialogue Data...");
        
        // Find all DialogueData assets
        string[] guids = AssetDatabase.FindAssets("t:DialogueData");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogueData dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            
            if (dialogue != null)
            {
                ValidateDialogueAsset(dialogue, path);
            }
        }
        
        lastValidationResults.Add(new ValidationResult(
            ValidationSeverity.Info, 
            "Dialogue", 
            $"Validated {guids.Length} dialogue assets"
        ));
    }
    
    private void ValidateDialogueAsset(DialogueData dialogue, string path)
    {
        // Check basic properties
        if (string.IsNullOrEmpty(dialogue.npcName))
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Error,
                "Dialogue",
                "Missing NPC name",
                $"Dialogue asset has no NPC name assigned",
                path
            ));
        }
        
        // Validate dialogue entries
        if (dialogue.dialogueEntries == null || dialogue.dialogueEntries.Length == 0)
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Warning,
                "Dialogue",
                "No dialogue entries",
                $"NPC '{dialogue.npcName}' has no dialogue entries",
                path
            ));
        }
        else
        {
            for (int i = 0; i < dialogue.dialogueEntries.Length; i++)
            {
                ValidateDialogueEntry(dialogue.dialogueEntries[i], dialogue.npcName, i, path);
            }
        }
        
        // Validate choices
        foreach (var entry in dialogue.dialogueEntries)
        {
            if (entry.hasChoices && (entry.choices == null || entry.choices.Length == 0))
            {
                lastValidationResults.Add(new ValidationResult(
                    ValidationSeverity.Error,
                    "Dialogue",
                    "Missing choices",
                    $"Dialogue entry marked as having choices but no choices defined",
                    path
                ));
            }
        }
    }
    
    private void ValidateDialogueEntry(DialogueEntry entry, string npcName, int index, string path)
    {
        // Check for empty dialogue text
        if (string.IsNullOrEmpty(entry.dialogueText))
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Error,
                "Dialogue",
                "Empty dialogue text",
                $"Entry {index} for {npcName} has no dialogue text",
                path
            ));
        }
        
        // Check speaker name consistency
        if (string.IsNullOrEmpty(entry.speakerName))
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Warning,
                "Dialogue",
                "Missing speaker name",
                $"Entry {index} for {npcName} has no speaker name",
                path
            ));
        }
        else if (entry.speakerName != npcName)
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Warning,
                "Dialogue",
                "Speaker name mismatch",
                $"Entry {index}: speaker '{entry.speakerName}' doesn't match NPC '{npcName}'",
                path
            ));
        }
        
        // Validate flag references
        if (entry.requiredFlags != null)
        {
            foreach (string flag in entry.requiredFlags)
            {
                if (!IsValidStoryFlag(flag))
                {
                    lastValidationResults.Add(new ValidationResult(
                        ValidationSeverity.Error,
                        "Dialogue",
                        "Invalid flag reference",
                        $"Entry {index} references unknown flag: {flag}",
                        path
                    ));
                }
            }
        }
        
        // Validate choices
        if (entry.hasChoices && entry.choices != null)
        {
            for (int j = 0; j < entry.choices.Length; j++)
            {
                ValidateDialogueChoice(entry.choices[j], npcName, index, j, path);
            }
        }
    }
    
    private void ValidateDialogueChoice(DialogueChoice choice, string npcName, int entryIndex, int choiceIndex, string path)
    {
        if (string.IsNullOrEmpty(choice.choiceText))
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Error,
                "Dialogue",
                "Empty choice text",
                $"{npcName} entry {entryIndex}, choice {choiceIndex} has no text",
                path
            ));
        }
        
        // Validate flag operations
        if (choice.flagsToAdd != null)
        {
            foreach (string flag in choice.flagsToAdd)
            {
                if (!IsValidStoryFlag(flag))
                {
                    lastValidationResults.Add(new ValidationResult(
                        ValidationSeverity.Error,
                        "Dialogue",
                        "Invalid flag operation",
                        $"Choice tries to add unknown flag: {flag}",
                        path
                    ));
                }
            }
        }
        
        // Validate quest references
        if (!string.IsNullOrEmpty(choice.questToStart))
        {
            if (!DoesQuestExist(choice.questToStart))
            {
                lastValidationResults.Add(new ValidationResult(
                    ValidationSeverity.Error,
                    "Dialogue",
                    "Invalid quest reference",
                    $"Choice references non-existent quest: {choice.questToStart}",
                    path
                ));
            }
        }
    }
    
    private void ValidateQuestData()
    {
        Debug.Log("🎯 Validating Quest Data...");
        
        string[] guids = AssetDatabase.FindAssets("t:QuestData");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            QuestData quest = AssetDatabase.LoadAssetAtPath<QuestData>(path);
            
            if (quest != null)
            {
                ValidateQuestAsset(quest, path);
            }
        }
        
        lastValidationResults.Add(new ValidationResult(
            ValidationSeverity.Info,
            "Quest",
            $"Validated {guids.Length} quest assets"
        ));
    }
    
    private void ValidateQuestAsset(QuestData quest, string path)
    {
        // Check basic properties
        if (string.IsNullOrEmpty(quest.questID))
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Error,
                "Quest",
                "Missing quest ID",
                $"Quest '{quest.questTitle}' has no ID",
                path
            ));
        }
        
        if (string.IsNullOrEmpty(quest.questTitle))
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Error,
                "Quest",
                "Missing quest title",
                $"Quest with ID '{quest.questID}' has no title",
                path
            ));
        }
        
        // Validate objectives
        if (quest.objectives == null || quest.objectives.Count == 0)
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Warning,
                "Quest",
                "No objectives",
                $"Quest '{quest.questTitle}' has no objectives",
                path
            ));
        }
        else
        {
            bool hasNonOptionalObjective = false;
            
            for (int i = 0; i < quest.objectives.Count; i++)
            {
                var objective = quest.objectives[i];
                ValidateQuestObjective(objective, quest.questTitle, i, path);
                
                if (!objective.isOptional)
                {
                    hasNonOptionalObjective = true;
                }
            }
            
            if (!hasNonOptionalObjective)
            {
                lastValidationResults.Add(new ValidationResult(
                    ValidationSeverity.Warning,
                    "Quest",
                    "All objectives optional",
                    $"Quest '{quest.questTitle}' has only optional objectives",
                    path
                ));
            }
        }
        
        // Validate flag references
        if (quest.requiredFlags != null)
        {
            foreach (string flag in quest.requiredFlags)
            {
                if (!IsValidStoryFlag(flag))
                {
                    lastValidationResults.Add(new ValidationResult(
                        ValidationSeverity.Error,
                        "Quest",
                        "Invalid required flag",
                        $"Quest '{quest.questTitle}' requires unknown flag: {flag}",
                        path
                    ));
                }
            }
        }
        
        // Check for duplicate quest IDs
        CheckForDuplicateQuestID(quest.questID, path);
    }
    
    private void ValidateQuestObjective(QuestObjective objective, string questTitle, int index, string path)
    {
        if (string.IsNullOrEmpty(objective.objectiveID))
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Warning,
                "Quest",
                "Missing objective ID",
                $"Quest '{questTitle}' objective {index} has no ID",
                path
            ));
        }
        
        if (string.IsNullOrEmpty(objective.description))
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Error,
                "Quest",
                "Missing objective description",
                $"Quest '{questTitle}' objective {index} has no description",
                path
            ));
        }
        
        // Validate objective type-specific properties
        switch (objective.type)
        {
            case ObjectiveType.TalkToNPC:
                if (string.IsNullOrEmpty(objective.targetNPC))
                {
                    lastValidationResults.Add(new ValidationResult(
                        ValidationSeverity.Error,
                        "Quest",
                        "Missing target NPC",
                        $"TalkToNPC objective '{objective.description}' has no target NPC",
                        path
                    ));
                }
                break;
                
            case ObjectiveType.CollectItems:
                if (string.IsNullOrEmpty(objective.targetItem))
                {
                    lastValidationResults.Add(new ValidationResult(
                        ValidationSeverity.Error,
                        "Quest",
                        "Missing target item",
                        $"CollectItems objective '{objective.description}' has no target item",
                        path
                    ));
                }
                if (objective.targetAmount <= 0)
                {
                    lastValidationResults.Add(new ValidationResult(
                        ValidationSeverity.Error,
                        "Quest",
                        "Invalid target amount",
                        $"CollectItems objective '{objective.description}' has invalid amount: {objective.targetAmount}",
                        path
                    ));
                }
                break;
                
            case ObjectiveType.VisitLocation:
                if (string.IsNullOrEmpty(objective.targetLocation))
                {
                    lastValidationResults.Add(new ValidationResult(
                        ValidationSeverity.Error,
                        "Quest",
                        "Missing target location",
                        $"VisitLocation objective '{objective.description}' has no target location",
                        path
                    ));
                }
                break;
        }
    }
    
    private void ValidateScheduleData()
    {
        Debug.Log("📅 Validating Schedule Data...");
        
        string[] guids = AssetDatabase.FindAssets("t:NPCScheduleData");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            NPCScheduleData schedule = AssetDatabase.LoadAssetAtPath<NPCScheduleData>(path);
            
            if (schedule != null)
            {
                ValidateScheduleAsset(schedule, path);
            }
        }
        
        lastValidationResults.Add(new ValidationResult(
            ValidationSeverity.Info,
            "Schedule",
            $"Validated {guids.Length} schedule assets"
        ));
    }
    
    private void ValidateScheduleAsset(NPCScheduleData schedule, string path)
    {
        if (string.IsNullOrEmpty(schedule.scheduleName))
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Warning,
                "Schedule",
                "Missing schedule name",
                "Schedule has no name assigned",
                path
            ));
        }
        
        // Validate schedule events
        if (schedule.scheduleEvents == null || schedule.scheduleEvents.Length == 0)
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Warning,
                "Schedule",
                "No schedule events",
                $"Schedule '{schedule.scheduleName}' has no events",
                path
            ));
        }
        else
        {
            var eventHours = new HashSet<int>();
            
            for (int i = 0; i < schedule.scheduleEvents.Length; i++)
            {
                var evt = schedule.scheduleEvents[i];
                
                // Check for duplicate hours
                if (eventHours.Contains(evt.hour))
                {
                    lastValidationResults.Add(new ValidationResult(
                        ValidationSeverity.Warning,
                        "Schedule",
                        "Duplicate event hour",
                        $"Schedule '{schedule.scheduleName}' has multiple events at hour {evt.hour}",
                        path
                    ));
                }
                else
                {
                    eventHours.Add(evt.hour);
                }
                
                // Validate object references
                if (!string.IsNullOrEmpty(evt.targetObjectName) && !string.IsNullOrEmpty(evt.targetObjectTag))
                {
                    // Note: We can't validate actual GameObject existence in editor without scene context
                    // This would be better done at runtime
                }
                
                // Check reasonable hour range
                if (evt.hour < 0 || evt.hour > 23)
                {
                    lastValidationResults.Add(new ValidationResult(
                        ValidationSeverity.Error,
                        "Schedule",
                        "Invalid hour",
                        $"Event {i} has invalid hour: {evt.hour}",
                        path
                    ));
                }
            }
        }
        
        // Validate home object reference
        if (!string.IsNullOrEmpty(schedule.homeObjectName) && string.IsNullOrEmpty(schedule.homeObjectTag))
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Warning,
                "Schedule",
                "Missing home object tag",
                $"Schedule '{schedule.scheduleName}' specifies home object name but no tag",
                path
            ));
        }
    }
    
    private void ValidateCrossReferences()
    {
        Debug.Log("🔗 Validating Cross-References...");
        
        // Validate quest-dialogue integration
        ValidateQuestDialogueIntegration();
        
        // Validate flag consistency across systems
        ValidateFlagConsistency();
        
        // Validate NPC references
        ValidateNPCReferences();
    }
    
    private void ValidateQuestDialogueIntegration()
    {
        var quests = LoadAllAssets<QuestData>();
        var dialogues = LoadAllAssets<DialogueData>();
        
        // Check if quests referenced in dialogues actually exist
        foreach (var dialogue in dialogues)
        {
            foreach (var entry in dialogue.dialogueEntries)
            {
                if (entry.hasChoices && entry.choices != null)
                {
                    foreach (var choice in entry.choices)
                    {
                        if (!string.IsNullOrEmpty(choice.questToStart))
                        {
                            bool questExists = quests.Any(q => q.questID == choice.questToStart);
                            if (!questExists)
                            {
                                lastValidationResults.Add(new ValidationResult(
                                    ValidationSeverity.Error,
                                    "Cross-Reference",
                                    "Missing quest reference",
                                    $"Dialogue '{dialogue.npcName}' references non-existent quest: {choice.questToStart}"
                                ));
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void ValidateFlagConsistency()
    {
        var allFlags = new HashSet<string>();
        var flagUsages = new Dictionary<string, List<string>>();
        
        // Collect all flag references from all systems
        CollectFlagsFromDialogues(allFlags, flagUsages);
        CollectFlagsFromQuests(allFlags, flagUsages);
        
        // Validate against known story flags
        foreach (var flag in allFlags)
        {
            if (!IsValidStoryFlag(flag))
            {
                var usageInfo = flagUsages.ContainsKey(flag) ? string.Join(", ", flagUsages[flag]) : "Unknown";
                lastValidationResults.Add(new ValidationResult(
                    ValidationSeverity.Error,
                    "Cross-Reference",
                    "Unknown flag reference",
                    $"Flag '{flag}' used in: {usageInfo}"
                ));
            }
        }
    }
    
    private void ValidateNPCReferences()
    {
        var schedules = LoadAllAssets<NPCScheduleData>();
        var dialogues = LoadAllAssets<DialogueData>();
        var quests = LoadAllAssets<QuestData>();
        
        var npcNames = new HashSet<string>();
        
        // Collect NPC names from schedules and dialogues
        foreach (var schedule in schedules)
        {
            if (!string.IsNullOrEmpty(schedule.scheduleName))
            {
                // Extract NPC name from schedule name (remove "Schedule" suffix)
                string npcName = schedule.scheduleName.Replace(" Schedule", "").Replace("_Schedule", "");
                npcNames.Add(npcName);
            }
        }
        
        foreach (var dialogue in dialogues)
        {
            if (!string.IsNullOrEmpty(dialogue.npcName))
            {
                npcNames.Add(dialogue.npcName);
            }
        }
        
        // Check if NPCs referenced in quests have corresponding data
        foreach (var quest in quests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.type == ObjectiveType.TalkToNPC && !string.IsNullOrEmpty(objective.targetNPC))
                {
                    // Check if the NPC exists in our data
                    bool npcExists = npcNames.Any(name => name.ToLower().Contains(objective.targetNPC.ToLower()) || 
                                                          objective.targetNPC.ToLower().Contains(name.ToLower()));
                    if (!npcExists)
                    {
                        lastValidationResults.Add(new ValidationResult(
                            ValidationSeverity.Warning,
                            "Cross-Reference",
                            "Missing NPC data",
                            $"Quest '{quest.questTitle}' references NPC '{objective.targetNPC}' but no matching dialogue/schedule found"
                        ));
                    }
                }
            }
        }
    }
    
    private void ValidateAssetIntegrity()
    {
        Debug.Log("🔍 Validating Asset Integrity...");
        
        // Check for missing assets in expected locations
        ValidateExpectedAssets();
        
        // Check for broken references
        ValidateBrokenReferences();
    }
    
    private void ValidateExpectedAssets()
    {
        // Define expected core NPCs
        string[] expectedNPCs = {
            "Ki Ageng Sinawang",
            "Raden Ayu Saraswati", 
            "Mbok Randa Krandon",
            "Buaya Putih"
        };
        
        var dialogues = LoadAllAssets<DialogueData>();
        var schedules = LoadAllAssets<NPCScheduleData>();
        
        foreach (string expectedNPC in expectedNPCs)
        {
            bool hasDialogue = dialogues.Any(d => d.npcName.Contains(expectedNPC));
            bool hasSchedule = schedules.Any(s => s.scheduleName.Contains(expectedNPC));
            
            if (!hasDialogue)
            {
                lastValidationResults.Add(new ValidationResult(
                    ValidationSeverity.Warning,
                    "Asset Integrity",
                    "Missing core NPC dialogue",
                    $"No dialogue found for core NPC: {expectedNPC}"
                ));
            }
            
            if (!hasSchedule)
            {
                lastValidationResults.Add(new ValidationResult(
                    ValidationSeverity.Warning,
                    "Asset Integrity",
                    "Missing core NPC schedule",
                    $"No schedule found for core NPC: {expectedNPC}"
                ));
            }
        }
    }
    
    private void ValidateBrokenReferences()
    {
        // This would check for broken Unity object references
        // Implementation depends on specific asset structure
    }
    
    #endregion
    
    #region Helper Methods
    
    private List<T> LoadAllAssets<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        var assets = new List<T>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        
        return assets;
    }
    
    private bool IsValidStoryFlag(string flagName)
    {
        // This should check against the story flag definitions
        // For now, we'll use a basic list of known flags
        string[] knownFlags = {
            "story_started", "water_crisis_discovered", "committed_to_help", "avoided_responsibility",
            "guru_guidance_received", "asked_permission_water_project", "dam_construction_started",
            "initial_dam_built", "dam_repeatedly_destroyed", "spiritual_interference_confirmed",
            "river_spirit_encountered", "accepted_spirit_demand", "white_elephant_borrowed",
            "elephant_sacrifice_complete", "spirit_pact_complete", "reconciliation_complete",
            "story_completed", "wisdom_gained", "teranging_galih_named"
        };
        
        return knownFlags.Contains(flagName);
    }
    
    private bool DoesQuestExist(string questID)
    {
        var quests = LoadAllAssets<QuestData>();
        return quests.Any(q => q.questID == questID);
    }
    
    private void CheckForDuplicateQuestID(string questID, string currentPath)
    {
        var quests = LoadAllAssets<QuestData>();
        var duplicates = quests.Where(q => q.questID == questID).ToList();
        
        if (duplicates.Count > 1)
        {
            lastValidationResults.Add(new ValidationResult(
                ValidationSeverity.Error,
                "Quest",
                "Duplicate quest ID",
                $"Quest ID '{questID}' is used by multiple quest assets",
                currentPath
            ));
        }
    }
    
    private void CollectFlagsFromDialogues(HashSet<string> allFlags, Dictionary<string, List<string>> flagUsages)
    {
        var dialogues = LoadAllAssets<DialogueData>();
        
        foreach (var dialogue in dialogues)
        {
            foreach (var entry in dialogue.dialogueEntries)
            {
                if (entry.requiredFlags != null)
                {
                    foreach (string flag in entry.requiredFlags)
                    {
                        allFlags.Add(flag);
                        if (!flagUsages.ContainsKey(flag))
                            flagUsages[flag] = new List<string>();
                        flagUsages[flag].Add($"Dialogue:{dialogue.npcName}");
                    }
                }
                
                if (entry.hasChoices && entry.choices != null)
                {
                    foreach (var choice in entry.choices)
                    {
                        if (choice.flagsToAdd != null)
                        {
                            foreach (string flag in choice.flagsToAdd)
                            {
                                allFlags.Add(flag);
                                if (!flagUsages.ContainsKey(flag))
                                    flagUsages[flag] = new List<string>();
                                flagUsages[flag].Add($"Dialogue:{dialogue.npcName}:Choice");
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void CollectFlagsFromQuests(HashSet<string> allFlags, Dictionary<string, List<string>> flagUsages)
    {
        var quests = LoadAllAssets<QuestData>();
        
        foreach (var quest in quests)
        {
            if (quest.requiredFlags != null)
            {
                foreach (string flag in quest.requiredFlags)
                {
                    allFlags.Add(flag);
                    if (!flagUsages.ContainsKey(flag))
                        flagUsages[flag] = new List<string>();
                    flagUsages[flag].Add($"Quest:{quest.questID}:Required");
                }
            }
            
            if (quest.flagsOnComplete != null)
            {
                foreach (string flag in quest.flagsOnComplete)
                {
                    allFlags.Add(flag);
                    if (!flagUsages.ContainsKey(flag))
                        flagUsages[flag] = new List<string>();
                    flagUsages[flag].Add($"Quest:{quest.questID}:Complete");
                }
            }
        }
    }
    
    private Color GetResultColor(ValidationSeverity severity)
    {
        switch (severity)
        {
            case ValidationSeverity.Info: return Color.white;
            case ValidationSeverity.Warning: return Color.yellow;
            case ValidationSeverity.Error: return Color.red;
            case ValidationSeverity.Critical: return Color.magenta;
            default: return Color.white;
        }
    }
    
    private string GetSeverityIcon(ValidationSeverity severity)
    {
        switch (severity)
        {
            case ValidationSeverity.Info: return "ℹ️";
            case ValidationSeverity.Warning: return "⚠️";
            case ValidationSeverity.Error: return "❌";
            case ValidationSeverity.Critical: return "🔥";
            default: return "?";
        }
    }
    
    private void DisplayValidationSummary()
    {
        int errors = lastValidationResults.Count(r => r.severity == ValidationSeverity.Error);
        int warnings = lastValidationResults.Count(r => r.severity == ValidationSeverity.Warning);
        int infos = lastValidationResults.Count(r => r.severity == ValidationSeverity.Info);
        
        string summary = $"Validation Complete: {errors} errors, {warnings} warnings, {infos} info";
        
        if (errors == 0 && warnings == 0)
        {
            Debug.Log($"✅ {summary}");
        }
        else if (errors == 0)
        {
            Debug.LogWarning($"⚠️ {summary}");
        }
        else
        {
            Debug.LogError($"❌ {summary}");
        }
        
        lastValidationResults.Add(new ValidationResult(
            errors > 0 ? ValidationSeverity.Error : (warnings > 0 ? ValidationSeverity.Warning : ValidationSeverity.Info),
            "Summary",
            summary
        ));
    }
    
    #endregion
}

#endif