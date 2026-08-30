using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Story Dashboard - Overview of all story content with validation and stats.
/// </summary>
public class StoryDashboardWindow : EditorWindow
{
    // Data
    private List<QuestData> allQuests = new List<QuestData>();
    private List<DialogueData> allDialogues = new List<DialogueData>();

    // Analysis results
    private Dictionary<string, int> flagUsage = new Dictionary<string, int>();
    private List<string> orphanedQuests = new List<string>();
    private List<string> missingReferences = new List<string>();
    private List<string> warnings = new List<string>();

    // UI
    private Vector2 scrollPosition;
    private int selectedTab = 0;
    private string[] tabNames = { "Overview", "Flags", "Validation", "Export" };

    // Styles
    private GUIStyle headerStyle;
    private GUIStyle statBoxStyle;
    private bool stylesInitialized = false;

    [MenuItem("Window/Story Tools/Story Dashboard")]
    public static void ShowWindow()
    {
        var window = GetWindow<StoryDashboardWindow>("Story Dashboard");
        window.minSize = new Vector2(600, 400);
        window.Show();
    }

    private void OnEnable()
    {
        RefreshAllData();
        AnalyzeContent();
    }

    private void OnFocus()
    {
        RefreshAllData();
        AnalyzeContent();
    }

    private void InitStyles()
    {
        if (stylesInitialized) return;

        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            margin = new RectOffset(5, 5, 10, 10)
        };

        statBoxStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(15, 15, 10, 10),
            fontSize = 12
        };

        stylesInitialized = true;
    }

    private void OnGUI()
    {
        InitStyles();

        // Header
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Story Dashboard", headerStyle);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
        {
            RefreshAllData();
            AnalyzeContent();
        }
        EditorGUILayout.EndHorizontal();

        // Tabs
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

        EditorGUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        switch (selectedTab)
        {
            case 0: DrawOverviewTab(); break;
            case 1: DrawFlagsTab(); break;
            case 2: DrawValidationTab(); break;
            case 3: DrawExportTab(); break;
        }

        EditorGUILayout.EndScrollView();
    }

    #region Overview Tab

    private void DrawOverviewTab()
    {
        EditorGUILayout.LabelField("Content Statistics", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        // Quests stats
        EditorGUILayout.BeginVertical(statBoxStyle, GUILayout.Width(180));
        EditorGUILayout.LabelField("Quests", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Total: {allQuests.Count}");
        EditorGUILayout.LabelField($"Main: {allQuests.Count(q => q.questType == QuestType.Main)}");
        EditorGUILayout.LabelField($"Side: {allQuests.Count(q => q.questType == QuestType.Side)}");
        EditorGUILayout.LabelField($"Chain: {allQuests.Count(q => q.isChainQuest)}");

        int totalObjectives = allQuests.Sum(q => q.objectives?.Count ?? 0);
        EditorGUILayout.LabelField($"Objectives: {totalObjectives}");
        EditorGUILayout.EndVertical();

        // Dialogue stats
        EditorGUILayout.BeginVertical(statBoxStyle, GUILayout.Width(180));
        EditorGUILayout.LabelField("Dialogues", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"NPCs: {allDialogues.Count}");

        int totalEntries = allDialogues.Sum(d => d.dialogueEntries?.Length ?? 0);
        EditorGUILayout.LabelField($"Entries: {totalEntries}");

        int entriesWithChoices = allDialogues.Sum(d =>
            d.dialogueEntries?.Count(e => e.hasChoices) ?? 0);
        EditorGUILayout.LabelField($"With Choices: {entriesWithChoices}");

        int totalChoices = allDialogues.Sum(d =>
            d.dialogueEntries?.Sum(e => e.choices?.Length ?? 0) ?? 0);
        EditorGUILayout.LabelField($"Total Choices: {totalChoices}");
        EditorGUILayout.EndVertical();

        // Flags stats
        EditorGUILayout.BeginVertical(statBoxStyle, GUILayout.Width(180));
        EditorGUILayout.LabelField("Flags", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Unique Flags: {flagUsage.Count}");

        var mostUsed = flagUsage.OrderByDescending(kv => kv.Value).Take(3);
        EditorGUILayout.LabelField("Most Used:");
        foreach (var flag in mostUsed)
        {
            EditorGUILayout.LabelField($"  {flag.Key} ({flag.Value})", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(20);

        // Quest chains visualization
        EditorGUILayout.LabelField("Quest Chains", EditorStyles.boldLabel);
        DrawQuestChainOverview();
    }

    private void DrawQuestChainOverview()
    {
        var chainQuests = allQuests.Where(q => q.isChainQuest ||
            (q.prerequisiteQuestIDs != null && q.prerequisiteQuestIDs.Length > 0) ||
            (q.unlocksQuestIDs != null && q.unlocksQuestIDs.Length > 0)).ToList();

        if (chainQuests.Count == 0)
        {
            EditorGUILayout.HelpBox("No quest chains defined yet", MessageType.Info);
            return;
        }

        foreach (var quest in chainQuests)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Prerequisites
            if (quest.prerequisiteQuestIDs != null && quest.prerequisiteQuestIDs.Length > 0)
            {
                string prereqs = string.Join(", ", quest.prerequisiteQuestIDs);
                EditorGUILayout.LabelField($"← Requires: {prereqs}", EditorStyles.miniLabel);
            }

            // Quest itself
            EditorGUILayout.LabelField($"⬤ {quest.questTitle} ({quest.questID})", EditorStyles.boldLabel);

            // Unlocks
            if (quest.unlocksQuestIDs != null && quest.unlocksQuestIDs.Length > 0)
            {
                string unlocks = string.Join(", ", quest.unlocksQuestIDs);
                EditorGUILayout.LabelField($"→ Unlocks: {unlocks}", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
        }
    }

    #endregion

    #region Flags Tab

    private void DrawFlagsTab()
    {
        EditorGUILayout.LabelField("Flag Usage Map", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Shows all flags and where they are used", MessageType.Info);

        EditorGUILayout.Space(10);

        foreach (var flag in flagUsage.OrderBy(kv => kv.Key))
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(flag.Key, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Used {flag.Value} times", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            // Show where it's used
            var usages = GetFlagUsages(flag.Key);
            foreach (var usage in usages.Take(5))
            {
                EditorGUILayout.LabelField($"  • {usage}", EditorStyles.miniLabel);
            }
            if (usages.Count > 5)
            {
                EditorGUILayout.LabelField($"  ... and {usages.Count - 5} more", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
        }
    }

    private List<string> GetFlagUsages(string flagName)
    {
        var usages = new List<string>();

        // Check quests
        foreach (var quest in allQuests)
        {
            if (quest.requiredFlags?.Contains(flagName) == true)
                usages.Add($"Quest '{quest.questTitle}' requires");
            if (quest.flagsOnStart?.Contains(flagName) == true)
                usages.Add($"Quest '{quest.questTitle}' sets on start");
            if (quest.flagsOnComplete?.Contains(flagName) == true)
                usages.Add($"Quest '{quest.questTitle}' sets on complete");
        }

        // Check dialogues
        foreach (var dialogue in allDialogues)
        {
            if (dialogue.dialogueEntries == null) continue;

            foreach (var entry in dialogue.dialogueEntries)
            {
                if (entry.requiredFlags?.Contains(flagName) == true)
                    usages.Add($"Dialogue '{dialogue.npcName}' requires");
                if (entry.flagsToAdd?.Contains(flagName) == true)
                    usages.Add($"Dialogue '{dialogue.npcName}' adds");
            }
        }

        return usages;
    }

    #endregion

    #region Validation Tab

    private void DrawValidationTab()
    {
        EditorGUILayout.LabelField("Content Validation", EditorStyles.boldLabel);

        if (warnings.Count == 0 && missingReferences.Count == 0)
        {
            EditorGUILayout.HelpBox("✓ No issues found!", MessageType.Info);
        }
        else
        {
            if (missingReferences.Count > 0)
            {
                EditorGUILayout.LabelField($"Missing References ({missingReferences.Count})", EditorStyles.boldLabel);
                foreach (var issue in missingReferences)
                {
                    EditorGUILayout.HelpBox(issue, MessageType.Error);
                }
            }

            EditorGUILayout.Space(10);

            if (warnings.Count > 0)
            {
                EditorGUILayout.LabelField($"Warnings ({warnings.Count})", EditorStyles.boldLabel);
                foreach (var warning in warnings)
                {
                    EditorGUILayout.HelpBox(warning, MessageType.Warning);
                }
            }
        }

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Run Full Validation", GUILayout.Height(30)))
        {
            AnalyzeContent();
            Repaint();
        }
    }

    #endregion

    #region Export Tab

    private void DrawExportTab()
    {
        EditorGUILayout.LabelField("Export Story Documentation", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Generate markdown documentation of your story content", MessageType.Info);

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Export Quest List (Markdown)", GUILayout.Height(30)))
        {
            ExportQuestList();
        }

        if (GUILayout.Button("Export Dialogue Summary (Markdown)", GUILayout.Height(30)))
        {
            ExportDialogueSummary();
        }

        if (GUILayout.Button("Export Flag Reference (Markdown)", GUILayout.Height(30)))
        {
            ExportFlagReference();
        }
    }

    private void ExportQuestList()
    {
        string path = EditorUtility.SaveFilePanel("Export Quest List", "", "quests.md", "md");
        if (string.IsNullOrEmpty(path)) return;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Quest List\n");

        foreach (var questType in System.Enum.GetValues(typeof(QuestType)))
        {
            var quests = allQuests.Where(q => q.questType == (QuestType)questType).ToList();
            if (quests.Count == 0) continue;

            sb.AppendLine($"## {questType} Quests\n");

            foreach (var quest in quests)
            {
                sb.AppendLine($"### {quest.questTitle}");
                sb.AppendLine($"- **ID**: `{quest.questID}`");
                sb.AppendLine($"- **Description**: {quest.questDescription}");

                if (quest.objectives?.Count > 0)
                {
                    sb.AppendLine("- **Objectives**:");
                    foreach (var obj in quest.objectives)
                    {
                        sb.AppendLine($"  - {obj.description}");
                    }
                }
                sb.AppendLine();
            }
        }

        System.IO.File.WriteAllText(path, sb.ToString());
        Debug.Log($"Exported quest list to: {path}");
        EditorUtility.RevealInFinder(path);
    }

    private void ExportDialogueSummary()
    {
        string path = EditorUtility.SaveFilePanel("Export Dialogue Summary", "", "dialogues.md", "md");
        if (string.IsNullOrEmpty(path)) return;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Dialogue Summary\n");

        foreach (var dialogue in allDialogues.OrderBy(d => d.npcName))
        {
            sb.AppendLine($"## {dialogue.npcName}");
            sb.AppendLine($"*{dialogue.dialogueDescription}*\n");

            if (dialogue.dialogueEntries != null)
            {
                foreach (var entry in dialogue.dialogueEntries)
                {
                    sb.AppendLine($"> {entry.dialogueText}");
                    sb.AppendLine();
                }
            }
        }

        System.IO.File.WriteAllText(path, sb.ToString());
        Debug.Log($"Exported dialogue summary to: {path}");
        EditorUtility.RevealInFinder(path);
    }

    private void ExportFlagReference()
    {
        string path = EditorUtility.SaveFilePanel("Export Flag Reference", "", "flags.md", "md");
        if (string.IsNullOrEmpty(path)) return;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Flag Reference\n");

        foreach (var flag in flagUsage.OrderBy(kv => kv.Key))
        {
            sb.AppendLine($"## `{flag.Key}`");
            sb.AppendLine($"Used {flag.Value} times\n");

            var usages = GetFlagUsages(flag.Key);
            foreach (var usage in usages)
            {
                sb.AppendLine($"- {usage}");
            }
            sb.AppendLine();
        }

        System.IO.File.WriteAllText(path, sb.ToString());
        Debug.Log($"Exported flag reference to: {path}");
        EditorUtility.RevealInFinder(path);
    }

    #endregion

    #region Data Loading

    private void RefreshAllData()
    {
        // Load quests
        allQuests.Clear();
        QuestData[] questAssets = Resources.LoadAll<QuestData>("Quests");
        allQuests.AddRange(questAssets);

        string[] questGuids = AssetDatabase.FindAssets("t:QuestData");
        foreach (string guid in questGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            QuestData quest = AssetDatabase.LoadAssetAtPath<QuestData>(path);
            if (quest != null && !allQuests.Contains(quest))
                allQuests.Add(quest);
        }

        // Load dialogues
        allDialogues.Clear();
        DialogueData[] dialogueAssets = Resources.LoadAll<DialogueData>("Dialogues");
        allDialogues.AddRange(dialogueAssets);

        string[] dialogueGuids = AssetDatabase.FindAssets("t:DialogueData");
        foreach (string guid in dialogueGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogueData dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            if (dialogue != null && !allDialogues.Contains(dialogue))
                allDialogues.Add(dialogue);
        }
    }

    private void AnalyzeContent()
    {
        flagUsage.Clear();
        warnings.Clear();
        missingReferences.Clear();

        // Analyze quests
        foreach (var quest in allQuests)
        {
            // Check for missing ID
            if (string.IsNullOrEmpty(quest.questID))
                warnings.Add($"Quest '{quest.questTitle}' has no ID");

            // Check for no objectives
            if (quest.objectives == null || quest.objectives.Count == 0)
                warnings.Add($"Quest '{quest.questTitle}' has no objectives");

            // Track flags
            TrackFlags(quest.requiredFlags);
            TrackFlags(quest.flagsOnStart);
            TrackFlags(quest.flagsOnComplete);
            TrackFlags(quest.flagsOnFail);

            // Check prerequisite references
            if (quest.prerequisiteQuestIDs != null)
            {
                foreach (var prereqId in quest.prerequisiteQuestIDs)
                {
                    if (!string.IsNullOrEmpty(prereqId) && !allQuests.Any(q => q.questID == prereqId))
                    {
                        missingReferences.Add($"Quest '{quest.questTitle}' references non-existent prerequisite: {prereqId}");
                    }
                }
            }
        }

        // Analyze dialogues
        foreach (var dialogue in allDialogues)
        {
            if (dialogue.dialogueEntries == null) continue;

            foreach (var entry in dialogue.dialogueEntries)
            {
                TrackFlags(entry.requiredFlags);
                TrackFlags(entry.flagsToAdd);
                TrackFlags(entry.flagsToRemove);
            }
        }
    }

    private void TrackFlags(string[] flags)
    {
        if (flags == null) return;

        foreach (var flag in flags)
        {
            if (string.IsNullOrEmpty(flag)) continue;

            if (flagUsage.ContainsKey(flag))
                flagUsage[flag]++;
            else
                flagUsage[flag] = 1;
        }
    }

    #endregion
}
