using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Dialogue Editor Window - A visual editor for managing all NPC dialogues in one place.
/// Provides NPC list, dialogue preview, and inline editing.
/// </summary>
public class DialogueEditorWindow : EditorWindow
{
    // Dialogue data
    private List<DialogueData> allDialogues = new List<DialogueData>();
    private DialogueData selectedDialogue;

    // UI State
    private Vector2 npcListScroll;
    private Vector2 dialogueScroll;
    private Vector2 entryScroll;
    private string searchFilter = "";
    private int selectedEntryIndex = -1;

    // Cached data
    private string[] allNPCNames;
    private string[] allFlagNames;

    // Styles
    private GUIStyle headerStyle;
    private GUIStyle entryStyle;
    private GUIStyle selectedEntryStyle;
    private bool stylesInitialized = false;

    // State
    private bool isDirty = false;

    [MenuItem("Window/Story Tools/Dialogue Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<DialogueEditorWindow>("Dialogue Editor");
        window.minSize = new Vector2(900, 550);
        window.Show();
    }

    private void OnEnable()
    {
        RefreshDialogueList();
        RefreshCachedData();
    }

    private void OnFocus()
    {
        RefreshDialogueList();
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

        entryStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 10, 8, 8),
            margin = new RectOffset(0, 0, 2, 2)
        };

        selectedEntryStyle = new GUIStyle(entryStyle)
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

        // Left panel - NPC list
        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        DrawNPCListPanel();
        EditorGUILayout.EndVertical();

        // Divider
        GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(2));

        // Middle panel - Dialogue entries
        EditorGUILayout.BeginVertical(GUILayout.Width(350));
        DrawDialogueEntriesPanel();
        EditorGUILayout.EndVertical();

        // Divider
        GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(2));

        // Right panel - Entry details
        EditorGUILayout.BeginVertical();
        DrawEntryDetailsPanel();
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

        // Stats
        GUILayout.Label($"NPCs: {allDialogues.Count}", GUILayout.Width(80));

        if (GUILayout.Button("+ New Dialogue", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            CreateNewDialogue();
        }

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            RefreshDialogueList();
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

    #region NPC List Panel

    private void DrawNPCListPanel()
    {
        EditorGUILayout.LabelField("NPCs", headerStyle);

        npcListScroll = EditorGUILayout.BeginScrollView(npcListScroll);

        var filteredDialogues = GetFilteredDialogues();

        foreach (var dialogue in filteredDialogues)
        {
            bool isSelected = selectedDialogue == dialogue;

            EditorGUILayout.BeginHorizontal(isSelected ? selectedEntryStyle : GUIStyle.none);

            // Entry count indicator
            int entryCount = dialogue.dialogueEntries?.Length ?? 0;
            string countBadge = entryCount > 0 ? $"({entryCount})" : "⚠";

            if (GUILayout.Button($"{dialogue.npcName} {countBadge}", EditorStyles.label))
            {
                SelectDialogue(dialogue);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private List<DialogueData> GetFilteredDialogues()
    {
        var filtered = allDialogues.AsEnumerable();

        if (!string.IsNullOrEmpty(searchFilter))
        {
            string filter = searchFilter.ToLower();
            filtered = filtered.Where(d =>
                d.npcName.ToLower().Contains(filter) ||
                (d.dialogueDescription != null && d.dialogueDescription.ToLower().Contains(filter)));
        }

        return filtered.OrderBy(d => d.npcName).ToList();
    }

    #endregion

    #region Dialogue Entries Panel

    private void DrawDialogueEntriesPanel()
    {
        if (selectedDialogue == null)
        {
            EditorGUILayout.HelpBox("Select an NPC to view dialogues", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField($"{selectedDialogue.npcName} - Entries", headerStyle);

        EditorGUI.BeginChangeCheck();

        // Basic info
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        selectedDialogue.npcName = EditorGUILayout.TextField("NPC Name", selectedDialogue.npcName);
        selectedDialogue.dialogueDescription = EditorGUILayout.TextField("Description", selectedDialogue.dialogueDescription);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // Entries list
        dialogueScroll = EditorGUILayout.BeginScrollView(dialogueScroll);

        if (selectedDialogue.dialogueEntries != null)
        {
            for (int i = 0; i < selectedDialogue.dialogueEntries.Length; i++)
            {
                DrawDialogueEntryPreview(i);
            }
        }

        EditorGUILayout.EndScrollView();

        // Add entry button
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Entry"))
        {
            AddNewEntry();
        }
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            isDirty = true;
            EditorUtility.SetDirty(selectedDialogue);
        }
    }

    private void DrawDialogueEntryPreview(int index)
    {
        var entry = selectedDialogue.dialogueEntries[index];
        bool isSelected = selectedEntryIndex == index;

        EditorGUILayout.BeginVertical(isSelected ? selectedEntryStyle : entryStyle);

        EditorGUILayout.BeginHorizontal();

        // Entry number
        GUILayout.Label($"#{index + 1}", EditorStyles.boldLabel, GUILayout.Width(30));

        // Speaker
        GUILayout.Label(entry.speakerName, GUILayout.Width(100));

        // Time indicator
        string timeIcon = GetTimeIcon(entry.availableTimesOfDay);
        GUILayout.Label(timeIcon, GUILayout.Width(40));

        // Flags indicator
        if (entry.requiredFlags != null && entry.requiredFlags.Length > 0)
        {
            GUILayout.Label("🏳", GUILayout.Width(20));
        }

        // Choices indicator
        if (entry.hasChoices)
        {
            GUILayout.Label("💬", GUILayout.Width(20));
        }

        GUILayout.FlexibleSpace();

        // Select button
        if (GUILayout.Button("Edit", GUILayout.Width(40)))
        {
            selectedEntryIndex = index;
        }

        // Delete button
        if (GUILayout.Button("×", GUILayout.Width(25)))
        {
            DeleteEntry(index);
            return;
        }

        EditorGUILayout.EndHorizontal();

        // Preview text (truncated)
        string preview = entry.dialogueText;
        if (preview.Length > 60) preview = preview.Substring(0, 60) + "...";
        EditorGUILayout.LabelField(preview, EditorStyles.wordWrappedMiniLabel);

        EditorGUILayout.EndVertical();
    }

    private string GetTimeIcon(TimeOfDay[] times)
    {
        if (times == null || times.Length == 0) return "🕐";

        bool hasSunrise = times.Contains(TimeOfDay.Sunrise);
        bool hasDay = times.Contains(TimeOfDay.Day);
        bool hasSunset = times.Contains(TimeOfDay.Sunset);
        bool hasNight = times.Contains(TimeOfDay.Night);

        if (hasDay && hasNight) return "☀🌙";
        if (hasDay) return "☀";
        if (hasNight) return "🌙";
        if (hasSunrise) return "🌅";
        if (hasSunset) return "🌇";
        return "🕐";
    }

    #endregion

    #region Entry Details Panel

    private void DrawEntryDetailsPanel()
    {
        if (selectedDialogue == null || selectedEntryIndex < 0 ||
            selectedDialogue.dialogueEntries == null ||
            selectedEntryIndex >= selectedDialogue.dialogueEntries.Length)
        {
            EditorGUILayout.HelpBox("Select an entry to edit", MessageType.Info);
            return;
        }

        var entry = selectedDialogue.dialogueEntries[selectedEntryIndex];

        EditorGUILayout.LabelField($"Entry #{selectedEntryIndex + 1}", headerStyle);

        EditorGUI.BeginChangeCheck();

        entryScroll = EditorGUILayout.BeginScrollView(entryScroll);

        // Basic
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Basic", EditorStyles.boldLabel);

        entry.speakerName = EditorGUILayout.TextField("Speaker", entry.speakerName);

        EditorGUILayout.LabelField("Dialogue Text");
        entry.dialogueText = EditorGUILayout.TextArea(entry.dialogueText, GUILayout.Height(80));

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // Time of Day
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Available Times", EditorStyles.boldLabel);
        DrawTimeOfDaySelector(ref entry.availableTimesOfDay);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // Flags
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Flags", EditorStyles.boldLabel);

        DrawFlagArrayInline("Required", ref entry.requiredFlags);
        DrawFlagArrayInline("Add", ref entry.flagsToAdd);
        DrawFlagArrayInline("Remove", ref entry.flagsToRemove);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        entry.isRepeatable = EditorGUILayout.Toggle("Repeatable", entry.isRepeatable);
        entry.isImportantDialogue = EditorGUILayout.Toggle("Important", entry.isImportantDialogue);
        entry.pauseAfterDialogue = EditorGUILayout.FloatField("Pause After", entry.pauseAfterDialogue);
        entry.conversationBubbleSprite = (Sprite)EditorGUILayout.ObjectField("Bubble", entry.conversationBubbleSprite, typeof(Sprite), false);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // Choices
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        entry.hasChoices = EditorGUILayout.Toggle("Has Choices", entry.hasChoices);

        if (entry.hasChoices)
        {
            EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);
            DrawChoicesEditor(entry);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();

        if (EditorGUI.EndChangeCheck())
        {
            isDirty = true;
            EditorUtility.SetDirty(selectedDialogue);
        }
    }

    private void DrawTimeOfDaySelector(ref TimeOfDay[] times)
    {
        if (times == null) times = new TimeOfDay[0];

        EditorGUILayout.BeginHorizontal();

        bool hasSunrise = times.Contains(TimeOfDay.Sunrise);
        bool hasDay = times.Contains(TimeOfDay.Day);
        bool hasSunset = times.Contains(TimeOfDay.Sunset);
        bool hasNight = times.Contains(TimeOfDay.Night);

        bool newSunrise = GUILayout.Toggle(hasSunrise, "Sunrise", EditorStyles.miniButtonLeft);
        bool newDay = GUILayout.Toggle(hasDay, "Day", EditorStyles.miniButtonMid);
        bool newSunset = GUILayout.Toggle(hasSunset, "Sunset", EditorStyles.miniButtonMid);
        bool newNight = GUILayout.Toggle(hasNight, "Night", EditorStyles.miniButtonRight);

        // Rebuild array if changed
        if (newSunrise != hasSunrise || newDay != hasDay || newSunset != hasSunset || newNight != hasNight)
        {
            var list = new List<TimeOfDay>();
            if (newSunrise) list.Add(TimeOfDay.Sunrise);
            if (newDay) list.Add(TimeOfDay.Day);
            if (newSunset) list.Add(TimeOfDay.Sunset);
            if (newNight) list.Add(TimeOfDay.Night);
            times = list.ToArray();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawFlagArrayInline(string label, ref string[] flags)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(60));

        if (flags == null) flags = new string[0];

        string flagsText = string.Join(", ", flags);
        string newFlagsText = EditorGUILayout.TextField(flagsText);

        if (newFlagsText != flagsText)
        {
            flags = newFlagsText.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawChoicesEditor(DialogueEntry entry)
    {
        if (entry.choices == null)
            entry.choices = new DialogueChoice[0];

        for (int i = 0; i < entry.choices.Length; i++)
        {
            var choice = entry.choices[i];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Choice {i + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                var list = entry.choices.ToList();
                list.RemoveAt(i);
                entry.choices = list.ToArray();
                break;
            }
            EditorGUILayout.EndHorizontal();

            choice.choiceText = EditorGUILayout.TextField("Text", choice.choiceText);

            if (choice.response != null)
            {
                choice.response.responseText = EditorGUILayout.TextField("Response", choice.response.responseText);
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("+ Add Choice"))
        {
            var list = entry.choices.ToList();
            list.Add(new DialogueChoice
            {
                choiceText = "New Choice",
                response = new DialogueResponse { responseText = "" }
            });
            entry.choices = list.ToArray();
        }
    }

    #endregion

    #region Actions

    private void SelectDialogue(DialogueData dialogue)
    {
        selectedDialogue = dialogue;
        selectedEntryIndex = -1;
        GUI.FocusControl(null);
    }

    private void CreateNewDialogue()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New Dialogue",
            "NewDialogue",
            "asset",
            "Choose a location for the new dialogue",
            "Assets/Resources/Dialogues/Story"
        );

        if (string.IsNullOrEmpty(path)) return;

        DialogueData newDialogue = CreateInstance<DialogueData>();
        newDialogue.npcName = "New NPC";
        newDialogue.dialogueDescription = "";
        newDialogue.dialogueEntries = new DialogueEntry[0];

        AssetDatabase.CreateAsset(newDialogue, path);
        AssetDatabase.SaveAssets();

        RefreshDialogueList();
        SelectDialogue(newDialogue);
    }

    private void AddNewEntry()
    {
        if (selectedDialogue == null) return;

        var list = selectedDialogue.dialogueEntries?.ToList() ?? new List<DialogueEntry>();
        list.Add(new DialogueEntry
        {
            speakerName = selectedDialogue.npcName,
            dialogueText = "New dialogue...",
            availableTimesOfDay = new[] { TimeOfDay.Day },
            isRepeatable = true
        });
        selectedDialogue.dialogueEntries = list.ToArray();

        selectedEntryIndex = list.Count - 1;
        isDirty = true;
        EditorUtility.SetDirty(selectedDialogue);
    }

    private void DeleteEntry(int index)
    {
        if (selectedDialogue == null || selectedDialogue.dialogueEntries == null) return;

        var list = selectedDialogue.dialogueEntries.ToList();
        list.RemoveAt(index);
        selectedDialogue.dialogueEntries = list.ToArray();

        if (selectedEntryIndex >= list.Count)
            selectedEntryIndex = list.Count - 1;

        isDirty = true;
        EditorUtility.SetDirty(selectedDialogue);
    }

    private void SaveAllChanges()
    {
        foreach (var dialogue in allDialogues)
        {
            EditorUtility.SetDirty(dialogue);
        }
        AssetDatabase.SaveAssets();
        isDirty = false;
        Debug.Log("Dialogue Editor: All changes saved");
    }

    private void RefreshDialogueList()
    {
        allDialogues.Clear();

        // Load from Resources
        DialogueData[] dialogueAssets = Resources.LoadAll<DialogueData>("Dialogues");
        allDialogues.AddRange(dialogueAssets);

        // Also find any outside Resources
        string[] guids = AssetDatabase.FindAssets("t:DialogueData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogueData dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            if (dialogue != null && !allDialogues.Contains(dialogue))
            {
                allDialogues.Add(dialogue);
            }
        }
    }

    private void RefreshCachedData()
    {
        // Get all NPC names
        allNPCNames = allDialogues.Select(d => d.npcName).Distinct().OrderBy(n => n).ToArray();

        // Get all flags
        var allFlags = new HashSet<string>();

        foreach (var dialogue in allDialogues)
        {
            if (dialogue.dialogueEntries == null) continue;

            foreach (var entry in dialogue.dialogueEntries)
            {
                if (entry.requiredFlags != null) allFlags.UnionWith(entry.requiredFlags);
                if (entry.flagsToAdd != null) allFlags.UnionWith(entry.flagsToAdd);
                if (entry.flagsToRemove != null) allFlags.UnionWith(entry.flagsToRemove);
            }
        }

        allFlagNames = allFlags.OrderBy(f => f).ToArray();
    }

    #endregion
}
