using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR

/// <summary>
/// Debug tool for testing quest system and managing quest states
/// Allows activating quests, completing objectives, and managing quest progress
/// Access via Tools -> Trenggalek Game -> Quest Debugger
/// </summary>
public class QuestDebugger : EditorWindow
{
    [MenuItem("Tools/Trenggalek Game/Quest Debugger")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow<QuestDebugger>();
        window.titleContent = new GUIContent("Quest Debugger");
        window.minSize = new Vector2(700, 500);
    }

    private Vector2 scrollPosition;
    private Vector2 objectivesScrollPosition;
    private QuestManager questManager;
    private NPCInteractionSystem interactionSystem;

    // Quest filtering
    private string searchText = "";
    private QuestType filterByType = (QuestType)(-1); // -1 means "All"
    private bool showOnlyActive = false;
    private bool showOnlyAvailable = false;

    // Selected quest for detailed view
    private QuestData selectedQuest;
    private bool showQuestDetails = false;

    // UI State
    private bool showCompletedQuests = false;
    private bool showFailedQuests = false;
    private bool autoRefresh = true;
    private float lastRefreshTime = 0f;
    private const float REFRESH_INTERVAL = 1f;

    private void OnEnable()
    {
        RefreshSystemReferences();
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            RefreshSystemReferences();
        }
    }

    private void OnGUI()
    {
        // Auto-refresh in play mode
        if (autoRefresh && Application.isPlaying && Time.realtimeSinceStartup - lastRefreshTime > REFRESH_INTERVAL)
        {
            RefreshSystemReferences();
            lastRefreshTime = Time.realtimeSinceStartup;
            Repaint();
        }

        titleContent = new GUIContent("Quest Debugger");
        EditorGUILayout.Space(5);

        DrawHeader();
        EditorGUILayout.Space(10);

        DrawControls();
        EditorGUILayout.Space(10);

        DrawQuestOverview();
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        // Left panel - Quest list
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.45f));
        DrawQuestList();
        EditorGUILayout.EndVertical();

        // Right panel - Quest details
        EditorGUILayout.BeginVertical();
        if (showQuestDetails && selectedQuest != null)
        {
            DrawQuestDetails();
        }
        else
        {
            EditorGUILayout.HelpBox("Select a quest from the list to view details and controls", MessageType.Info);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    #region Header and Controls

    private void DrawHeader()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Quest Debugger", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();

        autoRefresh = GUILayout.Toggle(autoRefresh, "Auto-Refresh", EditorStyles.toolbarButton);

        if (GUILayout.Button("Refresh Systems", EditorStyles.toolbarButton))
        {
            RefreshSystemReferences();
        }

        if (GUILayout.Button("Reset All Quests", EditorStyles.toolbarButton))
        {
            if (EditorUtility.DisplayDialog("Reset All Quests",
                "This will reset all quest progress and states. Continue?",
                "Yes", "Cancel"))
            {
                ResetAllQuests();
            }
        }

        GUILayout.EndHorizontal();

        // System status
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Quest Debugger requires Play Mode to function. Please enter Play Mode to debug quests.",
                MessageType.Warning
            );
        }
        else if (questManager == null || interactionSystem == null)
        {
            EditorGUILayout.HelpBox(
                $"Systems Status: QuestManager={questManager != null}, NPCInteractionSystem={interactionSystem != null}\n" +
                "Please ensure these systems exist in the scene.",
                MessageType.Error
            );
        }
    }

    private void DrawControls()
    {
        GUILayout.BeginHorizontal();

        // Search
        GUILayout.Label("Search:", GUILayout.Width(50));
        searchText = EditorGUILayout.TextField(searchText, GUILayout.Width(150));

        GUILayout.Space(20);

        // Type filter
        GUILayout.Label("Type:", GUILayout.Width(40));
        filterByType = (QuestType)EditorGUILayout.EnumPopup(filterByType, GUILayout.Width(100));

        if (GUILayout.Button("All Types", GUILayout.Width(70)))
        {
            filterByType = (QuestType)(-1);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        showOnlyActive = EditorGUILayout.Toggle("Show Only Active", showOnlyActive);
        showOnlyAvailable = EditorGUILayout.Toggle("Show Only Available", showOnlyAvailable);
        showCompletedQuests = EditorGUILayout.Toggle("Show Completed", showCompletedQuests);
        showFailedQuests = EditorGUILayout.Toggle("Show Failed", showFailedQuests);
        GUILayout.EndHorizontal();
    }

    #endregion

    #region Quest Overview

    private void DrawQuestOverview()
    {
        if (!Application.isPlaying || questManager == null)
            return;

        EditorGUILayout.LabelField("Quest Overview", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        // Active Quests
        DrawOverviewBox("Active Quests", questManager.ActiveQuestCount.ToString(), new Color(0.3f, 0.8f, 0.3f));

        // Completed Quests
        DrawOverviewBox("Completed", questManager.CompletedQuests.Count.ToString(), new Color(0.3f, 0.5f, 0.8f));

        // Failed Quests
        DrawOverviewBox("Failed", questManager.FailedQuests.Count.ToString(), new Color(0.8f, 0.3f, 0.3f));

        // Available Quests
        int availableCount = questManager.GetAvailableQuests().Count;
        DrawOverviewBox("Available", availableCount.ToString(), new Color(0.8f, 0.8f, 0.3f));

        GUILayout.EndHorizontal();
    }

    private void DrawOverviewBox(string label, string value, Color color)
    {
        var originalColor = GUI.backgroundColor;
        GUI.backgroundColor = color;

        GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(150));
        GUILayout.Label(label, EditorStyles.miniLabel);
        GUILayout.Label(value, EditorStyles.boldLabel);
        GUILayout.EndVertical();

        GUI.backgroundColor = originalColor;
    }

    #endregion

    #region Quest List

    private void DrawQuestList()
    {
        EditorGUILayout.LabelField("Quest List", EditorStyles.boldLabel);

        if (!Application.isPlaying || questManager == null)
        {
            EditorGUILayout.HelpBox("Quest list unavailable - enter Play Mode", MessageType.Info);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        var quests = GetFilteredQuests();

        if (quests.Count == 0)
        {
            EditorGUILayout.HelpBox("No quests match the current filters", MessageType.Info);
        }
        else
        {
            foreach (var quest in quests)
            {
                DrawQuestListItem(quest);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawQuestListItem(QuestData quest)
    {
        var isSelected = (selectedQuest == quest);
        var backgroundColor = GUI.backgroundColor;

        if (isSelected)
        {
            GUI.backgroundColor = new Color(0.5f, 0.7f, 1f, 0.5f);
        }

        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();

        // Status indicator
        var statusColor = GetQuestStatusColor(quest.status);
        var originalColor = GUI.color;
        GUI.color = statusColor;
        GUILayout.Label("●", GUILayout.Width(20));
        GUI.color = originalColor;

        // Quest title and type
        GUILayout.BeginVertical();
        if (GUILayout.Button(quest.questTitle, EditorStyles.boldLabel, GUILayout.Height(20)))
        {
            selectedQuest = quest;
            showQuestDetails = true;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label($"[{quest.questType}] {quest.status}", EditorStyles.miniLabel);

        if (quest.status == QuestStatus.Active)
        {
            int completed = quest.GetCompletedObjectiveCount();
            int total = quest.GetTotalObjectiveCount();
            GUILayout.Label($"({completed}/{total} objectives)", EditorStyles.miniLabel);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        // Quick actions
        DrawQuickActionButtons(quest);

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUI.backgroundColor = backgroundColor;
    }

    private void DrawQuickActionButtons(QuestData quest)
    {
        GUILayout.BeginVertical(GUILayout.Width(80));

        switch (quest.status)
        {
            case QuestStatus.NotStarted:
                if (GUILayout.Button("Start", EditorStyles.miniButtonMid))
                {
                    questManager.StartQuest(quest);
                }
                break;

            case QuestStatus.Active:
                if (GUILayout.Button("Complete", EditorStyles.miniButtonMid))
                {
                    questManager.CompleteQuest(quest);
                }
                if (GUILayout.Button("Fail", EditorStyles.miniButtonMid))
                {
                    questManager.FailQuest(quest);
                }
                break;

            case QuestStatus.Completed:
            case QuestStatus.Failed:
                if (GUILayout.Button("Reset", EditorStyles.miniButtonMid))
                {
                    ResetQuest(quest);
                }
                break;
        }

        GUILayout.EndVertical();
    }

    #endregion

    #region Quest Details

    private void DrawQuestDetails()
    {
        EditorGUILayout.LabelField("Quest Details", EditorStyles.boldLabel);

        objectivesScrollPosition = EditorGUILayout.BeginScrollView(objectivesScrollPosition);

        GUILayout.BeginVertical(EditorStyles.helpBox);

        // Quest header
        GUILayout.BeginHorizontal();
        var statusColor = GetQuestStatusColor(selectedQuest.status);
        var originalColor = GUI.color;
        GUI.color = statusColor;
        GUILayout.Label("●", EditorStyles.largeLabel, GUILayout.Width(25));
        GUI.color = originalColor;

        GUILayout.BeginVertical();
        EditorGUILayout.LabelField(selectedQuest.questTitle, EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"{selectedQuest.questType} Quest | Level {selectedQuest.questLevel}", EditorStyles.miniLabel);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Description
        EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(selectedQuest.questDescription, EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space(5);

        // Quest Info
        DrawQuestInfo();

        EditorGUILayout.Space(10);

        // Objectives
        DrawObjectivesList();

        EditorGUILayout.Space(10);

        // Actions
        DrawQuestActions();

        GUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }

    private void DrawQuestInfo()
    {
        EditorGUILayout.LabelField("Quest Information:", EditorStyles.boldLabel);

        GUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField($"Quest ID: {selectedQuest.questID}");
        EditorGUILayout.LabelField($"Status: {selectedQuest.status}");
        EditorGUILayout.LabelField($"Auto-Complete: {selectedQuest.autoComplete}");
        EditorGUILayout.LabelField($"Can Abandon: {selectedQuest.canAbandon}");
        EditorGUILayout.LabelField($"Repeatable: {selectedQuest.isRepeatable}");

        // Progress bar
        if (selectedQuest.status == QuestStatus.Active)
        {
            float progress = selectedQuest.GetProgressPercentage();
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, $"Progress: {Mathf.RoundToInt(progress * 100)}%");
        }

        // Flags info
        if (selectedQuest.requiredFlags != null && selectedQuest.requiredFlags.Length > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Required Flags:", EditorStyles.miniLabel);
            foreach (var flag in selectedQuest.requiredFlags)
            {
                bool hasFlag = interactionSystem != null && interactionSystem.GetGameFlags().Contains(flag);
                GUI.color = hasFlag ? Color.green : Color.red;
                EditorGUILayout.LabelField($"  • {flag} {(hasFlag ? "✓" : "✗")}", EditorStyles.miniLabel);
                GUI.color = Color.white;
            }
        }

        GUILayout.EndVertical();
    }

    private void DrawObjectivesList()
    {
        EditorGUILayout.LabelField("Objectives:", EditorStyles.boldLabel);

        if (selectedQuest.objectives == null || selectedQuest.objectives.Count == 0)
        {
            EditorGUILayout.HelpBox("No objectives defined for this quest", MessageType.Info);
            return;
        }

        foreach (var objective in selectedQuest.objectives)
        {
            DrawObjectiveItem(objective);
        }
    }

    private void DrawObjectiveItem(QuestObjective objective)
    {
        var bgColor = GUI.backgroundColor;
        GUI.backgroundColor = objective.isCompleted ? new Color(0.3f, 0.8f, 0.3f, 0.3f) : Color.white;

        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();

        // Checkbox
        bool newCompleted = EditorGUILayout.Toggle(objective.isCompleted, GUILayout.Width(20));
        if (newCompleted != objective.isCompleted)
        {
            if (newCompleted)
            {
                CompleteObjective(objective);
            }
            else
            {
                objective.isCompleted = false;
                objective.currentAmount = 0;
            }
        }

        // Objective info
        GUILayout.BeginVertical();

        var labelStyle = objective.isCompleted ? EditorStyles.boldLabel : EditorStyles.label;
        EditorGUILayout.LabelField(objective.description, labelStyle);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"[{objective.type}]", EditorStyles.miniLabel, GUILayout.Width(100));
        if (objective.isOptional)
        {
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("Optional", EditorStyles.miniLabel, GUILayout.Width(60));
            GUI.color = Color.white;
        }
        GUILayout.EndHorizontal();

        // Progress control for collection/defeat objectives
        if (objective.type == ObjectiveType.CollectItems || objective.type == ObjectiveType.DefeatEnemies)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Progress:", GUILayout.Width(60));

            int newAmount = EditorGUILayout.IntSlider(objective.currentAmount, 0, objective.targetAmount);
            if (newAmount != objective.currentAmount)
            {
                objective.currentAmount = newAmount;
                if (objective.currentAmount >= objective.targetAmount && !objective.isCompleted)
                {
                    CompleteObjective(objective);
                }
            }

            EditorGUILayout.LabelField($"{objective.currentAmount}/{objective.targetAmount}", GUILayout.Width(50));
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        // Actions
        GUILayout.BeginVertical(GUILayout.Width(80));
        if (!objective.isCompleted && selectedQuest.status == QuestStatus.Active)
        {
            if (GUILayout.Button("Complete", EditorStyles.miniButton))
            {
                CompleteObjective(objective);
            }
        }

        if (objective.isCompleted)
        {
            if (GUILayout.Button("Reset", EditorStyles.miniButton))
            {
                objective.isCompleted = false;
                objective.currentAmount = 0;
            }
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUI.backgroundColor = bgColor;
    }

    private void DrawQuestActions()
    {
        EditorGUILayout.LabelField("Quest Actions:", EditorStyles.boldLabel);

        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();

        switch (selectedQuest.status)
        {
            case QuestStatus.NotStarted:
                if (GUILayout.Button("Start Quest", GUILayout.Height(30)))
                {
                    questManager.StartQuest(selectedQuest);
                }
                break;

            case QuestStatus.Active:
                if (GUILayout.Button("Complete All Objectives", GUILayout.Height(30)))
                {
                    CompleteAllObjectives();
                }

                if (GUILayout.Button("Complete Quest", GUILayout.Height(30)))
                {
                    questManager.CompleteQuest(selectedQuest);
                }

                if (GUILayout.Button("Fail Quest", GUILayout.Height(30)))
                {
                    questManager.FailQuest(selectedQuest);
                }

                if (selectedQuest.canAbandon && GUILayout.Button("Abandon Quest", GUILayout.Height(30)))
                {
                    questManager.AbandonQuest(selectedQuest.questID);
                }
                break;

            case QuestStatus.Completed:
            case QuestStatus.Failed:
            case QuestStatus.Abandoned:
                if (GUILayout.Button("Reset Quest", GUILayout.Height(30)))
                {
                    ResetQuest(selectedQuest);
                }

                if (selectedQuest.isRepeatable && GUILayout.Button("Restart Quest", GUILayout.Height(30)))
                {
                    ResetQuest(selectedQuest);
                    questManager.StartQuest(selectedQuest);
                }
                break;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    #endregion

    #region Helper Methods

    private void RefreshSystemReferences()
    {
        if (Application.isPlaying)
        {
            questManager = QuestManager.Instance;
            interactionSystem = FindObjectOfType<NPCInteractionSystem>();
        }
        else
        {
            questManager = null;
            interactionSystem = null;
        }
    }

    private List<QuestData> GetFilteredQuests()
    {
        if (questManager == null)
            return new List<QuestData>();

        var allQuests = new List<QuestData>();

        // Gather quests based on filter settings
        allQuests.AddRange(questManager.ActiveQuests);

        if (!showOnlyActive)
        {
            // Add available quests (not started)
            foreach (var quest in questManager.GetAvailableQuests())
            {
                if (!allQuests.Contains(quest))
                    allQuests.Add(quest);
            }
        }

        if (showCompletedQuests)
        {
            allQuests.AddRange(questManager.CompletedQuests);
        }

        if (showFailedQuests)
        {
            allQuests.AddRange(questManager.FailedQuests);
        }

        var filtered = allQuests.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrEmpty(searchText))
        {
            filtered = filtered.Where(q =>
                q.questTitle.ToLower().Contains(searchText.ToLower()) ||
                q.questDescription.ToLower().Contains(searchText.ToLower()) ||
                q.questID.ToLower().Contains(searchText.ToLower())
            );
        }

        // Apply type filter
        if ((int)filterByType >= 0)
        {
            filtered = filtered.Where(q => q.questType == filterByType);
        }

        // Apply availability filter
        if (showOnlyAvailable && interactionSystem != null)
        {
            var gameFlags = interactionSystem.GetGameFlags();
            filtered = filtered.Where(q => q.CanStart(gameFlags));
        }

        return filtered.Distinct().ToList();
    }

    private Color GetQuestStatusColor(QuestStatus status)
    {
        switch (status)
        {
            case QuestStatus.NotStarted: return Color.gray;
            case QuestStatus.Active: return Color.yellow;
            case QuestStatus.Completed: return Color.green;
            case QuestStatus.Failed: return Color.red;
            case QuestStatus.Abandoned: return new Color(0.7f, 0.5f, 0.2f);
            default: return Color.white;
        }
    }

    private void CompleteObjective(QuestObjective objective)
    {
        if (questManager == null || selectedQuest == null)
            return;

        objective.currentAmount = objective.targetAmount;
        questManager.CompleteObjective(selectedQuest.questID, objective.objectiveID);
    }

    private void CompleteAllObjectives()
    {
        if (selectedQuest == null || selectedQuest.objectives == null)
            return;

        foreach (var objective in selectedQuest.objectives)
        {
            if (!objective.isCompleted)
            {
                CompleteObjective(objective);
            }
        }
    }

    private void ResetQuest(QuestData quest)
    {
        if (questManager == null)
            return;

        // Remove from all lists
        questManager.ActiveQuests.Remove(quest);
        questManager.CompletedQuests.Remove(quest);
        questManager.FailedQuests.Remove(quest);

        // Reset quest state
        quest.status = QuestStatus.NotStarted;
        quest.startTime = 0f;

        // Reset all objectives
        if (quest.objectives != null)
        {
            foreach (var objective in quest.objectives)
            {
                objective.isCompleted = false;
                objective.currentAmount = 0;
            }
        }

        Debug.Log($"Reset quest: {quest.questTitle}");
    }

    private void ResetAllQuests()
    {
        if (questManager != null)
        {
            questManager.ResetAllQuests();
            selectedQuest = null;
            showQuestDetails = false;
        }
    }

    #endregion
}

#endif
