using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Quest Designer Window - A visual editor for managing all quests in one place.
/// Provides table view, quick editing, and chain visualization.
/// </summary>
public class QuestDesignerWindow : EditorWindow
{
    // Quest data
    private List<QuestData> allQuests = new List<QuestData>();
    private QuestData selectedQuest;
    private int selectedQuestIndex = -1;

    // UI State
    private Vector2 questListScroll;
    private Vector2 detailsScroll;
    private Vector2 objectivesScroll;
    private string searchFilter = "";
    private QuestType? filterType = null;
    private bool showCompleted = true;
    private bool showMainQuests = true;
    private bool showSideQuests = true;

    // Cached data for dropdowns
    private string[] allQuestIDs;
    private string[] allNPCNames;
    private string[] allFlagNames;
    private string[] allItemIDs;

    // Styles
    private GUIStyle headerStyle;
    private GUIStyle selectedStyle;
    private bool stylesInitialized = false;

    // Editing state
    private bool isDirty = false;
    private int editingObjectiveIndex = -1;

    [MenuItem("Window/Story Tools/Quest Designer")]
    public static void ShowWindow()
    {
        var window = GetWindow<QuestDesignerWindow>("Quest Designer");
        window.minSize = new Vector2(800, 500);
        window.Show();
    }

    private void OnEnable()
    {
        RefreshQuestList();
        RefreshCachedData();
    }

    private void OnFocus()
    {
        RefreshQuestList();
        RefreshCachedData();
    }

    private void InitStyles()
    {
        if (stylesInitialized) return;

        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            margin = new RectOffset(5, 5, 10, 5)
        };

        selectedStyle = new GUIStyle(EditorStyles.helpBox)
        {
            normal = { background = MakeTex(2, 2, new Color(0.3f, 0.5f, 0.8f, 0.3f)) }
        };

        stylesInitialized = true;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void OnGUI()
    {
        InitStyles();

        // Toolbar
        DrawToolbar();

        EditorGUILayout.BeginHorizontal();

        // Left panel - Quest list
        EditorGUILayout.BeginVertical(GUILayout.Width(280));
        DrawQuestListPanel();
        EditorGUILayout.EndVertical();

        // Divider
        EditorGUILayout.BeginVertical(GUILayout.Width(2));
        GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(2));
        EditorGUILayout.EndVertical();

        // Right panel - Quest details
        EditorGUILayout.BeginVertical();
        DrawQuestDetailsPanel();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    #region Toolbar

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        // Search
        GUILayout.Label("Search:", GUILayout.Width(50));
        searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(150));

        GUILayout.FlexibleSpace();

        // Filter buttons
        showMainQuests = GUILayout.Toggle(showMainQuests, "Main", EditorStyles.toolbarButton, GUILayout.Width(50));
        showSideQuests = GUILayout.Toggle(showSideQuests, "Side", EditorStyles.toolbarButton, GUILayout.Width(50));

        GUILayout.Space(10);

        // Actions
        if (GUILayout.Button("+ New Quest", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            CreateNewQuest();
        }

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            RefreshQuestList();
            RefreshCachedData();
        }

        GUI.enabled = isDirty;
        if (GUILayout.Button("Save All", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            SaveAllChanges();
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    #endregion

    #region Quest List Panel

    private void DrawQuestListPanel()
    {
        EditorGUILayout.LabelField("Quests", headerStyle);

        questListScroll = EditorGUILayout.BeginScrollView(questListScroll);

        var filteredQuests = GetFilteredQuests();

        // Group by type
        if (showMainQuests)
        {
            DrawQuestGroup("Main Quests", filteredQuests.Where(q => q.questType == QuestType.Main).ToList());
        }

        if (showSideQuests)
        {
            DrawQuestGroup("Side Quests", filteredQuests.Where(q => q.questType == QuestType.Side).ToList());
            DrawQuestGroup("Daily Quests", filteredQuests.Where(q => q.questType == QuestType.Daily).ToList());
            DrawQuestGroup("Collection", filteredQuests.Where(q => q.questType == QuestType.Collection).ToList());
            DrawQuestGroup("Delivery", filteredQuests.Where(q => q.questType == QuestType.Delivery).ToList());
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawQuestGroup(string groupName, List<QuestData> quests)
    {
        if (quests.Count == 0) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(groupName, EditorStyles.boldLabel);

        foreach (var quest in quests)
        {
            bool isSelected = selectedQuest == quest;

            EditorGUILayout.BeginHorizontal(isSelected ? selectedStyle : GUIStyle.none);

            // Chain indicator
            if (quest.isChainQuest)
            {
                GUILayout.Label("⛓", GUILayout.Width(20));
            }
            else
            {
                GUILayout.Space(20);
            }

            // Quest button
            if (GUILayout.Button(quest.questTitle, EditorStyles.label))
            {
                SelectQuest(quest);
            }

            // Status indicator
            string statusIcon = GetQuestStatusIcon(quest);
            GUILayout.Label(statusIcon, GUILayout.Width(20));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private string GetQuestStatusIcon(QuestData quest)
    {
        if (quest.objectives == null || quest.objectives.Count == 0) return "⚠";
        if (string.IsNullOrEmpty(quest.questID)) return "⚠";
        return "✓";
    }

    private List<QuestData> GetFilteredQuests()
    {
        var filtered = allQuests.AsEnumerable();

        if (!string.IsNullOrEmpty(searchFilter))
        {
            string filter = searchFilter.ToLower();
            filtered = filtered.Where(q =>
                q.questTitle.ToLower().Contains(filter) ||
                q.questID.ToLower().Contains(filter) ||
                q.questDescription.ToLower().Contains(filter));
        }

        return filtered.ToList();
    }

    #endregion

    #region Quest Details Panel

    private void DrawQuestDetailsPanel()
    {
        if (selectedQuest == null)
        {
            EditorGUILayout.HelpBox("Select a quest to edit", MessageType.Info);
            return;
        }

        detailsScroll = EditorGUILayout.BeginScrollView(detailsScroll);

        EditorGUI.BeginChangeCheck();

        // Basic Info
        EditorGUILayout.LabelField("Quest Details", headerStyle);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        selectedQuest.questID = EditorGUILayout.TextField("Quest ID", selectedQuest.questID);
        selectedQuest.questTitle = EditorGUILayout.TextField("Title", selectedQuest.questTitle);

        EditorGUILayout.LabelField("Description");
        selectedQuest.questDescription = EditorGUILayout.TextArea(selectedQuest.questDescription, GUILayout.Height(60));

        selectedQuest.questType = (QuestType)EditorGUILayout.EnumPopup("Type", selectedQuest.questType);
        selectedQuest.questLevel = EditorGUILayout.IntField("Level", selectedQuest.questLevel);
        selectedQuest.questIcon = (Sprite)EditorGUILayout.ObjectField("Icon", selectedQuest.questIcon, typeof(Sprite), false);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // Quest Chain
        DrawQuestChainSection();

        EditorGUILayout.Space(10);

        // Flags
        DrawFlagsSection();

        EditorGUILayout.Space(10);

        // Objectives
        DrawObjectivesSection();

        EditorGUILayout.Space(10);

        // Settings
        DrawSettingsSection();

        if (EditorGUI.EndChangeCheck())
        {
            isDirty = true;
            EditorUtility.SetDirty(selectedQuest);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawQuestChainSection()
    {
        EditorGUILayout.LabelField("Quest Chain", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        selectedQuest.isChainQuest = EditorGUILayout.Toggle("Is Chain Quest", selectedQuest.isChainQuest);

        // Prerequisite quests
        EditorGUILayout.LabelField("Prerequisites (must complete first):");
        DrawQuestIDArrayField(ref selectedQuest.prerequisiteQuestIDs);

        // Unlocks quests
        EditorGUILayout.LabelField("Unlocks (available after completion):");
        DrawQuestIDArrayField(ref selectedQuest.unlocksQuestIDs);

        EditorGUILayout.EndVertical();
    }

    private void DrawQuestIDArrayField(ref string[] questIDs)
    {
        if (questIDs == null) questIDs = new string[0];

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginVertical();

        for (int i = 0; i < questIDs.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // Dropdown for quest selection
            int currentIndex = System.Array.IndexOf(allQuestIDs, questIDs[i]);
            int newIndex = EditorGUILayout.Popup(currentIndex, allQuestIDs);
            if (newIndex >= 0 && newIndex < allQuestIDs.Length)
            {
                questIDs[i] = allQuestIDs[newIndex];
            }

            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                var list = questIDs.ToList();
                list.RemoveAt(i);
                questIDs = list.ToArray();
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Add Prerequisite", GUILayout.Width(150)))
        {
            var list = questIDs.ToList();
            list.Add("");
            questIDs = list.ToArray();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawFlagsSection()
    {
        EditorGUILayout.LabelField("Flags", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        DrawFlagArrayField("Required Flags", ref selectedQuest.requiredFlags);
        DrawFlagArrayField("Flags on Start", ref selectedQuest.flagsOnStart);
        DrawFlagArrayField("Flags on Complete", ref selectedQuest.flagsOnComplete);
        DrawFlagArrayField("Flags on Fail", ref selectedQuest.flagsOnFail);

        EditorGUILayout.EndVertical();
    }

    private void DrawFlagArrayField(string label, ref string[] flags)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(120));

        if (flags == null) flags = new string[0];

        string flagsText = string.Join(", ", flags);
        string newFlagsText = EditorGUILayout.TextField(flagsText);

        if (newFlagsText != flagsText)
        {
            flags = newFlagsText.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawObjectivesSection()
    {
        EditorGUILayout.LabelField("Objectives", EditorStyles.boldLabel);

        if (selectedQuest.objectives == null)
            selectedQuest.objectives = new List<QuestObjective>();

        objectivesScroll = EditorGUILayout.BeginScrollView(objectivesScroll, GUILayout.Height(200));

        for (int i = 0; i < selectedQuest.objectives.Count; i++)
        {
            DrawObjectiveEntry(i);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Objective"))
        {
            selectedQuest.objectives.Add(new QuestObjective
            {
                objectiveID = $"obj_{selectedQuest.objectives.Count}",
                description = "New Objective",
                type = ObjectiveType.Custom,
                targetAmount = 1
            });
            isDirty = true;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawObjectiveEntry(int index)
    {
        var obj = selectedQuest.objectives[index];

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Objective {index + 1}", EditorStyles.boldLabel);

        if (GUILayout.Button("↑", GUILayout.Width(25)) && index > 0)
        {
            var temp = selectedQuest.objectives[index - 1];
            selectedQuest.objectives[index - 1] = obj;
            selectedQuest.objectives[index] = temp;
        }
        if (GUILayout.Button("↓", GUILayout.Width(25)) && index < selectedQuest.objectives.Count - 1)
        {
            var temp = selectedQuest.objectives[index + 1];
            selectedQuest.objectives[index + 1] = obj;
            selectedQuest.objectives[index] = temp;
        }
        if (GUILayout.Button("×", GUILayout.Width(25)))
        {
            selectedQuest.objectives.RemoveAt(index);
            return;
        }
        EditorGUILayout.EndHorizontal();

        obj.objectiveID = EditorGUILayout.TextField("ID", obj.objectiveID);
        obj.description = EditorGUILayout.TextField("Description", obj.description);
        obj.type = (ObjectiveType)EditorGUILayout.EnumPopup("Type", obj.type);

        // Type-specific fields
        switch (obj.type)
        {
            case ObjectiveType.TalkToNPC:
                DrawNPCSelector("Target NPC", ref obj.targetNPC);
                break;
            case ObjectiveType.CollectItems:
                obj.targetItem = EditorGUILayout.TextField("Item ID", obj.targetItem);
                obj.targetAmount = EditorGUILayout.IntField("Amount", obj.targetAmount);
                break;
            case ObjectiveType.DefeatEnemies:
                obj.targetItem = EditorGUILayout.TextField("Enemy Type", obj.targetItem);
                obj.targetAmount = EditorGUILayout.IntField("Amount", obj.targetAmount);
                break;
            case ObjectiveType.VisitLocation:
                obj.targetLocation = EditorGUILayout.TextField("Location", obj.targetLocation);
                break;
        }

        obj.isOptional = EditorGUILayout.Toggle("Optional", obj.isOptional);

        EditorGUILayout.EndVertical();
    }

    private void DrawNPCSelector(string label, ref string npcName)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(100));

        if (allNPCNames != null && allNPCNames.Length > 0)
        {
            int currentIndex = System.Array.IndexOf(allNPCNames, npcName);
            if (currentIndex < 0) currentIndex = 0;
            int newIndex = EditorGUILayout.Popup(currentIndex, allNPCNames);
            if (newIndex >= 0 && newIndex < allNPCNames.Length)
            {
                npcName = allNPCNames[newIndex];
            }
        }
        else
        {
            npcName = EditorGUILayout.TextField(npcName);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawSettingsSection()
    {
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        selectedQuest.isRepeatable = EditorGUILayout.Toggle("Repeatable", selectedQuest.isRepeatable);
        selectedQuest.canAbandon = EditorGUILayout.Toggle("Can Abandon", selectedQuest.canAbandon);
        selectedQuest.autoComplete = EditorGUILayout.Toggle("Auto Complete", selectedQuest.autoComplete);
        selectedQuest.timeLimit = EditorGUILayout.FloatField("Time Limit (0=none)", selectedQuest.timeLimit);
        selectedQuest.showInJournal = EditorGUILayout.Toggle("Show in Journal", selectedQuest.showInJournal);
        selectedQuest.trackByDefault = EditorGUILayout.Toggle("Track by Default", selectedQuest.trackByDefault);

        EditorGUILayout.EndVertical();
    }

    #endregion

    #region Actions

    private void SelectQuest(QuestData quest)
    {
        selectedQuest = quest;
        selectedQuestIndex = allQuests.IndexOf(quest);
        editingObjectiveIndex = -1;
        GUI.FocusControl(null);
    }

    private void CreateNewQuest()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New Quest",
            "NewQuest",
            "asset",
            "Choose a location for the new quest",
            "Assets/Resources/Quests"
        );

        if (string.IsNullOrEmpty(path)) return;

        QuestData newQuest = CreateInstance<QuestData>();
        newQuest.questID = System.IO.Path.GetFileNameWithoutExtension(path);
        newQuest.questTitle = "New Quest";
        newQuest.questDescription = "Enter description...";
        newQuest.questType = QuestType.Side;

        AssetDatabase.CreateAsset(newQuest, path);
        AssetDatabase.SaveAssets();

        RefreshQuestList();
        SelectQuest(newQuest);
    }

    private void SaveAllChanges()
    {
        foreach (var quest in allQuests)
        {
            EditorUtility.SetDirty(quest);
        }
        AssetDatabase.SaveAssets();
        isDirty = false;
        Debug.Log("Quest Designer: All changes saved");
    }

    private void RefreshQuestList()
    {
        allQuests.Clear();

        // Load from Resources/Quests
        QuestData[] questAssets = Resources.LoadAll<QuestData>("Quests");
        allQuests.AddRange(questAssets);

        // Also check for any in Assets folder
        string[] guids = AssetDatabase.FindAssets("t:QuestData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            QuestData quest = AssetDatabase.LoadAssetAtPath<QuestData>(path);
            if (quest != null && !allQuests.Contains(quest))
            {
                allQuests.Add(quest);
            }
        }

        allQuests = allQuests.OrderBy(q => q.questType).ThenBy(q => q.questTitle).ToList();

        // Update quest IDs cache
        allQuestIDs = allQuests.Select(q => q.questID).Where(id => !string.IsNullOrEmpty(id)).ToArray();
    }

    private void RefreshCachedData()
    {
        // Get all NPC names from dialogue data
        var dialogues = Resources.LoadAll<DialogueData>("Dialogues");
        allNPCNames = dialogues.Select(d => d.npcName).Distinct().OrderBy(n => n).ToArray();

        // Get all flags from various sources
        var allFlags = new HashSet<string>();

        foreach (var quest in allQuests)
        {
            if (quest.requiredFlags != null) allFlags.UnionWith(quest.requiredFlags);
            if (quest.flagsOnStart != null) allFlags.UnionWith(quest.flagsOnStart);
            if (quest.flagsOnComplete != null) allFlags.UnionWith(quest.flagsOnComplete);
            if (quest.flagsOnFail != null) allFlags.UnionWith(quest.flagsOnFail);
        }

        allFlagNames = allFlags.OrderBy(f => f).ToArray();
    }

    #endregion
}
