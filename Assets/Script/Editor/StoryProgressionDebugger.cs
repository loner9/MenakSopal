using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR

/// <summary>
/// Debug tool for testing story progression and flag states in the Trenggalek folklore game
/// Allows setting flags by chapter/phase to verify story flow and delivery
/// Access via Tools -> Trenggalek Game -> Story Progression Debugger
/// </summary>
public class StoryProgressionDebugger : EditorWindow
{
    [MenuItem("Tools/Trenggalek Game/Story Progression Debugger")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow<StoryProgressionDebugger>();
        window.titleContent = new GUIContent("Story Progression Debugger");
        window.minSize = new Vector2(600, 400);
    }

    private Vector2 scrollPosition;
    private string selectedPhase = "All Phases";
    private Dictionary<string, bool> flagStates = new Dictionary<string, bool>();
    private StoryFlagManager.StoryFlagDefinition[] allFlags;
    private NPCInteractionSystem interactionSystem;
    private QuestManager questManager;
    
    // Phase organization
    private Dictionary<string, List<StoryFlagManager.StoryFlagDefinition>> flagsByPhase;
    private string[] phaseNames;
    
    // Search and filter
    private string searchText = "";
    private bool showOnlyActiveFlags = false;
    private bool showQuestStatus = true;
    
    private void OnEnable()
    {
        LoadStoryFlags();
        RefreshSystemReferences();
    }

    private void OnGUI()
    {
        titleContent = new GUIContent("Story Progression Debugger");
        EditorGUILayout.Space(5);
        
        DrawHeader();
        EditorGUILayout.Space(10);
        
        DrawControls();
        EditorGUILayout.Space(10);
        
        DrawQuickActions();
        EditorGUILayout.Space(10);
        
        if (showQuestStatus)
        {
            DrawQuestStatus();
            EditorGUILayout.Space(10);
        }
        
        DrawFlagsList();
    }

    #region Header and Controls
    
    private void DrawHeader()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Story Progression Debugger", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Refresh Systems", EditorStyles.toolbarButton))
        {
            RefreshSystemReferences();
        }
        
        if (GUILayout.Button("Reload Flags", EditorStyles.toolbarButton))
        {
            LoadStoryFlags();
        }
        GUILayout.EndHorizontal();
        
        // System status
        if (interactionSystem == null || questManager == null)
        {
            EditorGUILayout.HelpBox(
                $"Systems Status: NPCInteractionSystem={interactionSystem != null}, QuestManager={questManager != null}\n" +
                "Please ensure you're in Play Mode or these systems exist in the scene for full functionality.",
                MessageType.Warning
            );
        }
    }
    
    private void DrawControls()
    {
        GUILayout.BeginHorizontal();
        
        // Phase selection
        GUILayout.Label("Phase Filter:", GUILayout.Width(80));
        int selectedIndex = System.Array.IndexOf(phaseNames, selectedPhase);
        if (selectedIndex == -1) selectedIndex = 0;
        
        selectedIndex = EditorGUILayout.Popup(selectedIndex, phaseNames, GUILayout.Width(200));
        selectedPhase = phaseNames[selectedIndex];
        
        GUILayout.Space(20);
        
        // Search
        GUILayout.Label("Search:", GUILayout.Width(50));
        searchText = EditorGUILayout.TextField(searchText, GUILayout.Width(150));
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        showOnlyActiveFlags = EditorGUILayout.Toggle("Show Only Active Flags", showOnlyActiveFlags);
        showQuestStatus = EditorGUILayout.Toggle("Show Quest Status", showQuestStatus);
        GUILayout.EndHorizontal();
    }
    
    #endregion
    
    #region Quick Actions
    
    private void DrawQuickActions()
    {
        EditorGUILayout.LabelField("Quick Phase Setups", EditorStyles.boldLabel);
        
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Reset All Flags"))
        {
            if (EditorUtility.DisplayDialog("Reset All Flags", 
                "This will clear all story flags and reset quest progress. Continue?", 
                "Yes", "Cancel"))
            {
                ResetAllFlags();
            }
        }
        
        if (GUILayout.Button("Set to Phase 1"))
        {
            SetToPhase("Phase 1: Discovery");
        }
        
        if (GUILayout.Button("Set to Phase 2"))
        {
            SetToPhase("Phase 2");
        }
        
        if (GUILayout.Button("Set to Phase 3"))
        {
            SetToPhase("Phase 3: Opposition");
        }
        
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Set to Phase 4"))
        {
            SetToPhase("Phase 4: Quest");
        }
        
        if (GUILayout.Button("Set to Phase 5"))
        {
            SetToPhase("Phase 5: Sacrifice");
        }
        
        if (GUILayout.Button("Set to Phase 6"))
        {
            SetToPhase("Phase 6: Reckoning");
        }
        
        if (GUILayout.Button("Set to Final Phase"))
        {
            SetToPhase("Phase 8: Legacy");
        }
        
        GUILayout.EndHorizontal();
    }
    
    #endregion
    
    #region Quest Status Display
    
    private void DrawQuestStatus()
    {
        EditorGUILayout.LabelField("Quest Status", EditorStyles.boldLabel);
        
        if (questManager == null)
        {
            EditorGUILayout.HelpBox("QuestManager not found. Enter Play Mode to see quest status.", MessageType.Info);
            return;
        }
        
        GUILayout.BeginHorizontal();
        
        // Active Quests
        GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(180));
        EditorGUILayout.LabelField($"Active Quests ({questManager.ActiveQuestCount})", EditorStyles.boldLabel);
        foreach (var quest in questManager.ActiveQuests)
        {
            EditorGUILayout.LabelField($"• {quest.questTitle}", EditorStyles.miniLabel);
        }
        GUILayout.EndVertical();
        
        // Completed Quests
        GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(180));
        EditorGUILayout.LabelField($"Completed Quests ({questManager.CompletedQuests.Count})", EditorStyles.boldLabel);
        foreach (var quest in questManager.CompletedQuests.Take(5))
        {
            EditorGUILayout.LabelField($"✓ {quest.questTitle}", EditorStyles.miniLabel);
        }
        if (questManager.CompletedQuests.Count > 5)
        {
            EditorGUILayout.LabelField($"... and {questManager.CompletedQuests.Count - 5} more", EditorStyles.miniLabel);
        }
        GUILayout.EndVertical();
        
        // Failed Quests
        GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(180));
        EditorGUILayout.LabelField($"Failed Quests ({questManager.FailedQuests.Count})", EditorStyles.boldLabel);
        foreach (var quest in questManager.FailedQuests)
        {
            EditorGUILayout.LabelField($"✗ {quest.questTitle}", EditorStyles.miniLabel);
        }
        GUILayout.EndVertical();
        
        GUILayout.EndHorizontal();
    }
    
    #endregion
    
    #region Flags Display
    
    private void DrawFlagsList()
    {
        EditorGUILayout.LabelField("Story Flags", EditorStyles.boldLabel);
        
        if (allFlags == null || allFlags.Length == 0)
        {
            EditorGUILayout.HelpBox("No story flags loaded. Click 'Reload Flags' to load them.", MessageType.Info);
            return;
        }
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        var flagsToShow = GetFilteredFlags();
        
        foreach (var phase in flagsByPhase.Keys)
        {
            if (selectedPhase != "All Phases" && selectedPhase != phase) continue;
            
            var phaseFlags = flagsByPhase[phase].Where(f => flagsToShow.Contains(f)).ToList();
            if (phaseFlags.Count == 0) continue;
            
            // Phase header
            EditorGUILayout.Space(5);
            var headerStyle = new GUIStyle(EditorStyles.foldoutHeader);
            headerStyle.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField(phase, headerStyle);
            
            EditorGUI.indentLevel++;
            
            foreach (var flag in phaseFlags)
            {
                DrawFlagControl(flag);
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawFlagControl(StoryFlagManager.StoryFlagDefinition flag)
    {
        bool currentState = GetFlagState(flag.flagName);
        
        GUILayout.BeginHorizontal();
        
        // Flag toggle
        bool newState = EditorGUILayout.Toggle(currentState, GUILayout.Width(20));
        if (newState != currentState)
        {
            SetFlagState(flag.flagName, newState);
        }
        
        // Flag name and info
        var labelStyle = currentState ? EditorStyles.boldLabel : EditorStyles.label;
        var color = currentState ? Color.green : Color.gray;
        
        var originalColor = GUI.color;
        GUI.color = color;
        
        GUILayout.BeginVertical();
        EditorGUILayout.LabelField(flag.flagName, labelStyle);
        EditorGUILayout.LabelField(flag.description, EditorStyles.miniLabel);
        
        // Dependencies info
        if (flag.dependencies != null && flag.dependencies.Length > 0)
        {
            EditorGUILayout.LabelField($"Requires: {string.Join(", ", flag.dependencies)}", EditorStyles.miniLabel);
        }
        
        GUILayout.EndVertical();
        
        GUI.color = originalColor;
        
        // Category badge
        var categoryColor = GetCategoryColor(flag.category);
        var originalBgColor = GUI.backgroundColor;
        GUI.backgroundColor = categoryColor;
        GUILayout.Label(flag.category, EditorStyles.miniButton, GUILayout.Width(120));
        GUI.backgroundColor = originalBgColor;
        
        GUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
    }
    
    #endregion
    
    #region Utility Methods
    
    private void LoadStoryFlags()
    {
        // Create temporary StoryFlagManager to get flag definitions
        var tempManager = CreateInstance<StoryFlagManager>();
        allFlags = tempManager.CreateAllStoryFlags();
        DestroyImmediate(tempManager);
        
        // Organize flags by phase
        flagsByPhase = new Dictionary<string, List<StoryFlagManager.StoryFlagDefinition>>();
        foreach (var flag in allFlags)
        {
            if (!flagsByPhase.ContainsKey(flag.phase))
            {
                flagsByPhase[flag.phase] = new List<StoryFlagManager.StoryFlagDefinition>();
            }
            flagsByPhase[flag.phase].Add(flag);
        }
        
        // Create phase names array for dropdown
        var phases = new List<string> { "All Phases" };
        phases.AddRange(flagsByPhase.Keys.OrderBy(p => p));
        phaseNames = phases.ToArray();
        
        Debug.Log($"Loaded {allFlags.Length} story flags across {flagsByPhase.Keys.Count} phases");
    }
    
    private void RefreshSystemReferences()
    {
        if (Application.isPlaying)
        {
            interactionSystem = FindObjectOfType<NPCInteractionSystem>();
            questManager = QuestManager.Instance;
        }
        else
        {
            interactionSystem = null;
            questManager = null;
        }
    }
    
    private List<StoryFlagManager.StoryFlagDefinition> GetFilteredFlags()
    {
        var filtered = allFlags.AsEnumerable();
        
        // Apply search filter
        if (!string.IsNullOrEmpty(searchText))
        {
            filtered = filtered.Where(f => 
                f.flagName.ToLower().Contains(searchText.ToLower()) ||
                f.description.ToLower().Contains(searchText.ToLower()) ||
                f.category.ToLower().Contains(searchText.ToLower())
            );
        }
        
        // Apply active flags filter
        if (showOnlyActiveFlags)
        {
            filtered = filtered.Where(f => GetFlagState(f.flagName));
        }
        
        return filtered.ToList();
    }
    
    private bool GetFlagState(string flagName)
    {
        if (interactionSystem != null)
        {
            return interactionSystem.GetGameFlags().Contains(flagName);
        }
        
        // Fallback to local cache if system not available
        return flagStates.ContainsKey(flagName) && flagStates[flagName];
    }
    
    private void SetFlagState(string flagName, bool state)
    {
        if (interactionSystem != null)
        {
            if (state)
            {
                interactionSystem.AddGameFlag(flagName);
            }
            else
            {
                interactionSystem.RemoveGameFlag(flagName);
            }
        }
        else
        {
            // Update local cache if system not available
            flagStates[flagName] = state;
        }
        
        Debug.Log($"Flag '{flagName}' set to: {state}");
    }
    
    private void ResetAllFlags()
    {
        if (interactionSystem != null)
        {
            // Clear all flags by getting current flags and removing them one by one
            var currentFlags = new List<string>(interactionSystem.GetGameFlags());
            foreach (var flag in currentFlags)
            {
                interactionSystem.RemoveGameFlag(flag);
            }
        }
        
        if (questManager != null)
        {
            questManager.ResetAllQuests();
        }
        
        flagStates.Clear();
        
        Debug.Log("All flags and quests have been reset");
    }
    
    private void SetToPhase(string targetPhase)
    {
        ResetAllFlags();
        
        if (allFlags == null) return;
        
        // Find all flags that should be active at this phase
        var flagsToSet = new List<string>();
        
        foreach (var flag in allFlags)
        {
            // Set flags from current phase and all previous phases
            if (ShouldFlagBeActiveAtPhase(flag, targetPhase))
            {
                flagsToSet.Add(flag.flagName);
            }
        }
        
        // Apply flags in dependency order
        var sortedFlags = SortFlagsByDependencies(flagsToSet);
        
        foreach (var flagName in sortedFlags)
        {
            SetFlagState(flagName, true);
        }
        
        Debug.Log($"Set story progression to {targetPhase}. Activated {flagsToSet.Count} flags.");
    }
    
    private bool ShouldFlagBeActiveAtPhase(StoryFlagManager.StoryFlagDefinition flag, string targetPhase)
    {
        // Define phase progression order
        var phaseOrder = new Dictionary<string, int>
        {
            { "Phase 1: Discovery", 1 },
            { "Phase 2: Planning", 2 },
            { "Phase 2: Construction", 2 },
            { "Phase 2", 2 },
            { "Phase 3: Opposition", 3 },
            { "Phase 4: Quest", 4 },
            { "Phase 5: Sacrifice", 5 },
            { "Phase 6: Reckoning", 6 },
            { "Phase 7: Truth", 7 },
            { "Phase 7: Resolution", 7 },
            { "Phase 8: Legacy", 8 }
        };
        
        int targetPhaseLevel = phaseOrder.ContainsKey(targetPhase) ? phaseOrder[targetPhase] : 0;
        int flagPhaseLevel = phaseOrder.ContainsKey(flag.phase) ? phaseOrder[flag.phase] : 0;
        
        // Include core progression flags and story milestones from current and previous phases
        if (flagPhaseLevel <= targetPhaseLevel)
        {
            return flag.category.Contains("Core Progression") ||
                   flag.category.Contains("Story Milestone") ||
                   flag.category.Contains("Story Revelation");
        }
        
        return false;
    }
    
    private List<string> SortFlagsByDependencies(List<string> flagNames)
    {
        var result = new List<string>();
        var remaining = new HashSet<string>(flagNames);
        var flagsDict = allFlags.ToDictionary(f => f.flagName, f => f);
        
        while (remaining.Count > 0)
        {
            var added = false;
            var toRemove = new List<string>();
            
            foreach (var flagName in remaining)
            {
                if (flagsDict.ContainsKey(flagName))
                {
                    var flag = flagsDict[flagName];
                    bool canAdd = true;
                    
                    if (flag.dependencies != null)
                    {
                        foreach (var dependency in flag.dependencies)
                        {
                            if (flagNames.Contains(dependency) && !result.Contains(dependency))
                            {
                                canAdd = false;
                                break;
                            }
                        }
                    }
                    
                    if (canAdd)
                    {
                        result.Add(flagName);
                        toRemove.Add(flagName);
                        added = true;
                    }
                }
                else
                {
                    result.Add(flagName);
                    toRemove.Add(flagName);
                    added = true;
                }
            }
            
            foreach (var flagName in toRemove)
            {
                remaining.Remove(flagName);
            }
            
            if (!added && remaining.Count > 0)
            {
                // Add remaining flags to avoid infinite loop
                result.AddRange(remaining);
                break;
            }
        }
        
        return result;
    }
    
    private Color GetCategoryColor(string category)
    {
        switch (category)
        {
            case "Core Progression": return new Color(0.2f, 0.8f, 0.2f, 0.3f);
            case "Player Choice": return new Color(0.2f, 0.5f, 0.8f, 0.3f);
            case "Story Milestone": return new Color(0.8f, 0.6f, 0.2f, 0.3f);
            case "Story Revelation": return new Color(0.8f, 0.2f, 0.8f, 0.3f);
            case "Supernatural Agreement": return new Color(0.5f, 0.2f, 0.8f, 0.3f);
            case "Cultural Legacy": return new Color(0.8f, 0.8f, 0.2f, 0.3f);
            default: return new Color(0.7f, 0.7f, 0.7f, 0.3f);
        }
    }
    
    #endregion
}

#endif