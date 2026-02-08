using System.Collections.Generic;
using MenakSopal.Cutscenes;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor window for creating and editing cutscenes.
/// Provides a visual interface for building cutscene sequences.
/// </summary>
public class CutsceneEditorWindow : EditorWindow
{
    // Data
    private List<CutsceneData> allCutscenes = new List<CutsceneData>();
    private CutsceneData selectedCutscene;
    private int selectedStepIndex = -1;

    // UI State
    private Vector2 listScroll;
    private Vector2 stepsScroll;
    private Vector2 detailsScroll;
    private string searchFilter = "";
    private bool isDirty = false;

    // Styles
    private GUIStyle headerStyle;
    private GUIStyle stepStyle;
    private GUIStyle selectedStepStyle;
    private bool stylesInitialized = false;

    // Step type colors for visual distinction
    private static readonly Dictionary<string, Color> stepTypeColors = new Dictionary<string, Color>
    {
        { "Dialogue", new Color(0.3f, 0.6f, 0.9f, 0.3f) },     // Blue
        { "Player", new Color(0.3f, 0.9f, 0.3f, 0.3f) },       // Green
        { "NPC", new Color(0.9f, 0.6f, 0.3f, 0.3f) },          // Orange
        { "Camera", new Color(0.9f, 0.3f, 0.6f, 0.3f) },       // Pink
        { "GameState", new Color(0.6f, 0.3f, 0.9f, 0.3f) },    // Purple
        { "Time", new Color(0.9f, 0.9f, 0.3f, 0.3f) },         // Yellow
        { "Audio", new Color(0.3f, 0.9f, 0.9f, 0.3f) },        // Cyan
        { "Scene", new Color(0.6f, 0.6f, 0.6f, 0.3f) },        // Gray
        { "Flow", new Color(0.9f, 0.9f, 0.9f, 0.3f) },         // White
        { "Default", new Color(0.5f, 0.5f, 0.5f, 0.3f) }       // Default
    };

    [MenuItem("Window/Story Tools/Cutscene Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<CutsceneEditorWindow>("Cutscene Editor");
        window.minSize = new Vector2(1000, 600);
        window.Show();
    }

    private void OnEnable()
    {
        RefreshCutsceneList();
    }

    private void OnFocus()
    {
        RefreshCutsceneList();
    }

    private void InitStyles()
    {
        if (stylesInitialized) return;

        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            margin = new RectOffset(5, 5, 10, 5)
        };

        stepStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(8, 8, 6, 6),
            margin = new RectOffset(0, 0, 2, 2)
        };

        selectedStepStyle = new GUIStyle(stepStyle)
        {
            normal = { background = MakeTex(2, 2, new Color(0.3f, 0.5f, 0.8f, 0.4f)) }
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

        // Left panel - Cutscene list
        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        DrawCutsceneListPanel();
        EditorGUILayout.EndVertical();

        // Divider
        GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(2));

        // Middle panel - Steps list
        EditorGUILayout.BeginVertical(GUILayout.Width(350));
        DrawStepsPanel();
        EditorGUILayout.EndVertical();

        // Divider
        GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(2));

        // Right panel - Step details
        EditorGUILayout.BeginVertical();
        DrawStepDetailsPanel();
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
        GUILayout.Label($"Cutscenes: {allCutscenes.Count}", GUILayout.Width(100));

        if (GUILayout.Button("+ New Cutscene", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            CreateNewCutscene();
        }

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            RefreshCutsceneList();
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

    #region Cutscene List Panel

    private void DrawCutsceneListPanel()
    {
        EditorGUILayout.LabelField("Cutscenes", headerStyle);

        listScroll = EditorGUILayout.BeginScrollView(listScroll);

        foreach (var cutscene in allCutscenes)
        {
            if (!string.IsNullOrEmpty(searchFilter) &&
                !cutscene.cutsceneID.ToLower().Contains(searchFilter.ToLower()))
                continue;

            bool isSelected = selectedCutscene == cutscene;

            EditorGUILayout.BeginHorizontal(isSelected ? selectedStepStyle : GUIStyle.none);

            int stepCount = cutscene.steps?.Count ?? 0;
            string label = $"{cutscene.cutsceneID} ({stepCount})";

            if (GUILayout.Button(label, EditorStyles.label))
            {
                SelectCutscene(cutscene);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region Steps Panel

    private void DrawStepsPanel()
    {
        if (selectedCutscene == null)
        {
            EditorGUILayout.HelpBox("Select a cutscene to view steps", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField($"{selectedCutscene.cutsceneID} - Steps", headerStyle);

        EditorGUI.BeginChangeCheck();

        // Cutscene settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        selectedCutscene.cutsceneID = EditorGUILayout.TextField("ID", selectedCutscene.cutsceneID);
        selectedCutscene.description = EditorGUILayout.TextField("Description", selectedCutscene.description);
        selectedCutscene.triggerFlag = EditorGUILayout.TextField("Trigger Flag", selectedCutscene.triggerFlag);

        EditorGUILayout.BeginHorizontal();
        selectedCutscene.canSkip = EditorGUILayout.Toggle("Can Skip", selectedCutscene.canSkip);
        selectedCutscene.pauseGameTime = EditorGUILayout.Toggle("Pause Time", selectedCutscene.pauseGameTime);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // Steps list
        EditorGUILayout.LabelField("Steps:", EditorStyles.boldLabel);

        stepsScroll = EditorGUILayout.BeginScrollView(stepsScroll);

        int stepToMove = -1;
        int moveDirection = 0;
        int stepToDelete = -1;

        if (selectedCutscene.steps != null)
        {
            for (int i = 0; i < selectedCutscene.steps.Count; i++)
            {
                int action = DrawStepPreview(i);
                if (action == 1) { stepToMove = i; moveDirection = -1; }
                else if (action == 2) { stepToMove = i; moveDirection = 1; }
                else if (action == 3) { stepToDelete = i; }
            }
        }

        EditorGUILayout.EndScrollView();

        // Process deferred actions after the GUI loop to avoid Layout errors
        if (stepToMove != -1) MoveStep(stepToMove, moveDirection);
        if (stepToDelete != -1) DeleteStep(stepToDelete);

        // Add step buttons
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("+ Add Step"))
        {
            AddNewStep();
        }

        if (GUILayout.Button("+ Quick Add"))
        {
            ShowQuickAddMenu();
        }

        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            isDirty = true;
            EditorUtility.SetDirty(selectedCutscene);
        }
    }

    private int DrawStepPreview(int index)
    {
        var step = selectedCutscene.steps[index];
        bool isSelected = selectedStepIndex == index;
        int actionResult = 0; // 0: none, 1: up, 2: down, 3: delete

        var style = isSelected ? selectedStepStyle : stepStyle;

        EditorGUILayout.BeginVertical(style);

        EditorGUILayout.BeginHorizontal();

        // Step number and type
        GUILayout.Label($"{index + 1}.", GUILayout.Width(25));
        GUILayout.Label(GetStepTypeIcon(step.type), GUILayout.Width(25));
        GUILayout.Label(step.type.ToString(), EditorStyles.boldLabel, GUILayout.Width(120));

        GUILayout.FlexibleSpace();

        // Move buttons
        GUI.enabled = index > 0;
        if (GUILayout.Button("↑", GUILayout.Width(25))) actionResult = 1;

        GUI.enabled = index < selectedCutscene.steps.Count - 1;
        if (GUILayout.Button("↓", GUILayout.Width(25))) actionResult = 2;
        GUI.enabled = true;

        // Select button
        if (GUILayout.Button("Edit", GUILayout.Width(40)))
        {
            selectedStepIndex = index;
        }

        // Delete button
        if (GUILayout.Button("×", GUILayout.Width(25))) actionResult = 3;

        EditorGUILayout.EndHorizontal();

        // Preview info
        string preview = GetStepPreviewText(step);
        if (!string.IsNullOrEmpty(preview))
        {
            EditorGUILayout.LabelField(preview, EditorStyles.miniLabel);
        }

        EditorGUILayout.EndVertical();
        return actionResult;
    }

    private string GetStepTypeIcon(CutsceneStep.StepType type)
    {
        switch (type)
        {
            case CutsceneStep.StepType.ShowDialogue:
            case CutsceneStep.StepType.ShowMonologue:
            case CutsceneStep.StepType.ShowMessage:
                return "💬";
            case CutsceneStep.StepType.DisablePlayerMovement:
            case CutsceneStep.StepType.EnablePlayerMovement:
            case CutsceneStep.StepType.TeleportPlayer:
            case CutsceneStep.StepType.MovePlayerTo:
            case CutsceneStep.StepType.MovePlayerWalk:
                return "🚶";
            case CutsceneStep.StepType.SpawnNPC:
            case CutsceneStep.StepType.EnterSubArea:
            case CutsceneStep.StepType.ExitSubArea:
                return "🚪";
            case CutsceneStep.StepType.CameraShake:
            case CutsceneStep.StepType.CameraFocusOn:
            case CutsceneStep.StepType.CameraFollowPlayer:
                return "🎥";
            case CutsceneStep.StepType.TriggerEvent:
            case CutsceneStep.StepType.SetFlag:
            case CutsceneStep.StepType.RemoveFlag:
            case CutsceneStep.StepType.StartQuest:
            case CutsceneStep.StepType.CompleteQuest:
                return "📜";
            case CutsceneStep.StepType.SetTimeOfDay:
            case CutsceneStep.StepType.PauseGameTime:
                return "🕐";
            case CutsceneStep.StepType.PlaySound:
            case CutsceneStep.StepType.PlayMusic:
                return "🔊";
            case CutsceneStep.StepType.FadeToBlack:
            case CutsceneStep.StepType.FadeFromBlack:
                return "🌑";
            case CutsceneStep.StepType.WaitSeconds:
            case CutsceneStep.StepType.WaitForDialogueEnd:
                return "⏳";
            default:
                return "▪";
        }
    }

    private Color GetStepTypeColor(CutsceneStep.StepType type)
    {
        string category = GetStepCategory(type);
        if (stepTypeColors.TryGetValue(category, out Color color))
            return color;
        return stepTypeColors["Default"];
    }

    private string GetStepCategory(CutsceneStep.StepType type)
    {
        switch (type)
        {
            case CutsceneStep.StepType.ShowDialogue:
            case CutsceneStep.StepType.ShowMonologue:
            case CutsceneStep.StepType.ShowMessage:
                return "Dialogue";
            case CutsceneStep.StepType.DisablePlayerMovement:
            case CutsceneStep.StepType.EnablePlayerMovement:
            case CutsceneStep.StepType.TeleportPlayer:
            case CutsceneStep.StepType.MovePlayerTo:
            case CutsceneStep.StepType.MovePlayerWalk:
                return "Player";
            case CutsceneStep.StepType.SpawnNPC:
            case CutsceneStep.StepType.DespawnNPC:
            case CutsceneStep.StepType.MoveNPCTo:
            case CutsceneStep.StepType.FaceNPCTowards:
                return "NPC";
            case CutsceneStep.StepType.CameraShake:
            case CutsceneStep.StepType.CameraFocusOn:
            case CutsceneStep.StepType.CameraFollowPlayer:
                return "Camera";
            case CutsceneStep.StepType.SetFlag:
            case CutsceneStep.StepType.RemoveFlag:
            case CutsceneStep.StepType.StartQuest:
            case CutsceneStep.StepType.CompleteQuest:
            case CutsceneStep.StepType.CompleteObjective:
                return "GameState";
            case CutsceneStep.StepType.SetTimeOfDay:
            case CutsceneStep.StepType.PauseGameTime:
            case CutsceneStep.StepType.ResumeGameTime:
                return "Time";
            case CutsceneStep.StepType.PlaySound:
            case CutsceneStep.StepType.PlayMusic:
            case CutsceneStep.StepType.StopMusic:
                return "Audio";
            case CutsceneStep.StepType.EnterSubArea:
            case CutsceneStep.StepType.ExitSubArea:
            case CutsceneStep.StepType.FadeToBlack:
            case CutsceneStep.StepType.FadeFromBlack:
                return "Scene";
            case CutsceneStep.StepType.WaitSeconds:
            case CutsceneStep.StepType.WaitForDialogueEnd:
            case CutsceneStep.StepType.WaitForInput:
                return "Flow";
            default:
                return "Default";
        }
    }

    private string GetStepPreviewText(CutsceneStep step)
    {
        switch (step.type)
        {
            case CutsceneStep.StepType.ShowDialogue:
            case CutsceneStep.StepType.SpawnNPC:
            case CutsceneStep.StepType.DespawnNPC:
            case CutsceneStep.StepType.TeleportPlayer:
            case CutsceneStep.StepType.StartQuest:
            case CutsceneStep.StepType.CompleteQuest:
            case CutsceneStep.StepType.SetFlag:
            case CutsceneStep.StepType.RemoveFlag:
            case CutsceneStep.StepType.EnterSubArea:
            case CutsceneStep.StepType.MovePlayerWalk:
                return !string.IsNullOrEmpty(step.targetID) ? $"→ {step.targetID}" : "";
            case CutsceneStep.StepType.MoveNPCTo:
                return $"{step.targetID} → {step.secondaryTargetID}";
            case CutsceneStep.StepType.ShowMonologue:
            case CutsceneStep.StepType.ShowMessage:
                return !string.IsNullOrEmpty(step.textContent)
                    ? (step.textContent.Length > 40 ? step.textContent.Substring(0, 40) + "..." : step.textContent)
                    : "";
            case CutsceneStep.StepType.WaitSeconds:
                return $"{step.duration}s";
            case CutsceneStep.StepType.SetTimeOfDay:
                return step.timeOfDay.ToString();
            default:
                return "";
        }
    }

    #endregion

    #region Step Details Panel

    private void DrawStepDetailsPanel()
    {
        if (selectedCutscene == null || selectedStepIndex < 0 ||
            selectedCutscene.steps == null || selectedStepIndex >= selectedCutscene.steps.Count)
        {
            EditorGUILayout.HelpBox("Select a step to edit details", MessageType.Info);
            DrawQuickReferencePanel();
            return;
        }

        var step = selectedCutscene.steps[selectedStepIndex];

        EditorGUILayout.LabelField($"Step #{selectedStepIndex + 1}", headerStyle);

        EditorGUI.BeginChangeCheck();

        detailsScroll = EditorGUILayout.BeginScrollView(detailsScroll);

        // Basic info
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Step Type", EditorStyles.boldLabel);
        step.type = (CutsceneStep.StepType)EditorGUILayout.EnumPopup("Type", step.type);
        step.stepName = EditorGUILayout.TextField("Name (Optional)", step.stepName);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // Type-specific fields
        DrawTypeSpecificFields(step);

        EditorGUILayout.Space(5);

        // Timing
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
        step.delayBefore = EditorGUILayout.FloatField("Delay Before", step.delayBefore);
        step.duration = EditorGUILayout.FloatField("Duration", step.duration);
        step.waitForCompletion = EditorGUILayout.Toggle("Wait for Completion", step.waitForCompletion);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // Flags
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Flags", EditorStyles.boldLabel);
        DrawStringArrayField("Set Flags", ref step.flagsToSet);
        DrawStringArrayField("Remove Flags", ref step.flagsToRemove);
        DrawStringArrayField("Required Flags", ref step.requiredFlags);
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();

        if (EditorGUI.EndChangeCheck())
        {
            isDirty = true;
            EditorUtility.SetDirty(selectedCutscene);
        }
    }

    private void DrawTypeSpecificFields(CutsceneStep step)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Step Parameters", EditorStyles.boldLabel);

        switch (step.type)
        {
            case CutsceneStep.StepType.ShowDialogue:
                step.targetID = EditorGUILayout.TextField("NPC ID", step.targetID);
                step.dialogueOverride = (DialogueData)EditorGUILayout.ObjectField("Dialogue Override", step.dialogueOverride, typeof(DialogueData), false);
                break;

            case CutsceneStep.StepType.ShowMonologue:
            case CutsceneStep.StepType.ShowMessage:
                EditorGUILayout.LabelField("Text Content:");
                step.textContent = EditorGUILayout.TextArea(step.textContent, GUILayout.Height(60));
                break;

            case CutsceneStep.StepType.TeleportPlayer:
            case CutsceneStep.StepType.MovePlayerTo:
            case CutsceneStep.StepType.MovePlayerWalk:
                step.targetID = EditorGUILayout.TextField("Destination Name", step.targetID);
                EditorGUILayout.HelpBox("Use the name of a destination GameObject in the scene", MessageType.Info);
                break;

            case CutsceneStep.StepType.SpawnNPC:
            case CutsceneStep.StepType.DespawnNPC:
                step.targetID = EditorGUILayout.TextField("NPC ID", step.targetID);
                break;

            case CutsceneStep.StepType.MoveNPCTo:
                step.targetID = EditorGUILayout.TextField("NPC ID", step.targetID);
                step.secondaryTargetID = EditorGUILayout.TextField("Destination ID", step.secondaryTargetID);
                break;

            case CutsceneStep.StepType.CameraShake:
                step.shakeIntensity = EditorGUILayout.Slider("Intensity", step.shakeIntensity, 0.1f, 3f);
                EditorGUILayout.HelpBox("0-0.5 = Light, 0.5-1.5 = Medium, 1.5+ = Explosion", MessageType.Info);
                break;

            case CutsceneStep.StepType.CameraFocusOn:
                step.targetID = EditorGUILayout.TextField("Focus Target ID", step.targetID);
                step.duration = EditorGUILayout.FloatField("Pan Duration", step.duration);
                break;

            case CutsceneStep.StepType.CameraFollowPlayer:
                step.duration = EditorGUILayout.FloatField("Pan back Duration", step.duration);
                break;

            case CutsceneStep.StepType.SetFlag:
            case CutsceneStep.StepType.RemoveFlag:
                step.targetID = EditorGUILayout.TextField("Flag Name", step.targetID);
                break;

            case CutsceneStep.StepType.StartQuest:
            case CutsceneStep.StepType.CompleteQuest:
                step.targetID = EditorGUILayout.TextField("Quest ID", step.targetID);
                break;

            case CutsceneStep.StepType.CompleteObjective:
                step.targetID = EditorGUILayout.TextField("Quest ID", step.targetID);
                step.secondaryTargetID = EditorGUILayout.TextField("Objective ID", step.secondaryTargetID);
                break;

            case CutsceneStep.StepType.SetTimeOfDay:
                step.timeOfDay = (TimeOfDay)EditorGUILayout.EnumPopup("Time of Day", step.timeOfDay);
                break;

            case CutsceneStep.StepType.PlaySound:
            case CutsceneStep.StepType.PlayMusic:
                step.audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", step.audioClip, typeof(AudioClip), false);
                break;

            case CutsceneStep.StepType.EnterSubArea:
                step.targetID = EditorGUILayout.TextField("Scene Name", step.targetID);
                break;

            case CutsceneStep.StepType.WaitSeconds:
                step.duration = EditorGUILayout.FloatField("Wait Duration", step.duration);
                break;

            case CutsceneStep.StepType.EnableGameObject:
            case CutsceneStep.StepType.DisableGameObject:
                step.targetID = EditorGUILayout.TextField("GameObject Tag", step.targetID);
                break;

            case CutsceneStep.StepType.TriggerEvent:
                step.targetID = EditorGUILayout.TextField("Event Name", step.targetID);
                break;

            default:
                EditorGUILayout.LabelField("No additional parameters needed");
                break;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawStringArrayField(string label, ref string[] array)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(100));

        if (array == null) array = new string[0];

        string text = string.Join(", ", array);
        string newText = EditorGUILayout.TextField(text);

        if (newText != text)
        {
            array = newText.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawQuickReferencePanel()
    {
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Quick Reference", headerStyle);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("Step Categories:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("💬 Dialogue - ShowDialogue, ShowMonologue, ShowMessage");
        EditorGUILayout.LabelField("🚶 Player - Disable/Enable Movement, Teleport, Move, Walk");
        EditorGUILayout.LabelField("👤 NPC - Spawn, Despawn, Move, Face Direction");
        EditorGUILayout.LabelField("📷 Camera - Shake, Focus, Follow");
        EditorGUILayout.LabelField("🏳 Flags - SetFlag, RemoveFlag");
        EditorGUILayout.LabelField("📜 Quests - StartQuest, CompleteQuest, CompleteObjective");
        EditorGUILayout.LabelField("🕐 Time - SetTimeOfDay, Pause/Resume");
        EditorGUILayout.LabelField("🔊 Audio - PlaySound, PlayMusic, StopMusic");
        EditorGUILayout.LabelField("🌑 Scene - Fade, EnterSubArea, ExitSubArea");
        EditorGUILayout.LabelField("⏳ Flow - WaitSeconds, WaitForDialogue, WaitForInput");

        EditorGUILayout.EndVertical();
    }

    #endregion

    #region Actions

    private void SelectCutscene(CutsceneData cutscene)
    {
        selectedCutscene = cutscene;
        selectedStepIndex = -1;
        GUI.FocusControl(null);
    }

    private void CreateNewCutscene()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New Cutscene",
            "NewCutscene",
            "asset",
            "Choose a location for the new cutscene",
            "Assets/Resources/Cutscenes"
        );

        if (string.IsNullOrEmpty(path)) return;

        CutsceneData newCutscene = CreateInstance<CutsceneData>();
        newCutscene.cutsceneID = System.IO.Path.GetFileNameWithoutExtension(path);
        newCutscene.steps = new List<CutsceneStep>();

        AssetDatabase.CreateAsset(newCutscene, path);
        AssetDatabase.SaveAssets();

        RefreshCutsceneList();
        SelectCutscene(newCutscene);
    }

    private void AddNewStep()
    {
        if (selectedCutscene == null) return;

        if (selectedCutscene.steps == null)
            selectedCutscene.steps = new List<CutsceneStep>();

        selectedCutscene.steps.Add(new CutsceneStep
        {
            type = CutsceneStep.StepType.WaitSeconds,
            duration = 1f,
            waitForCompletion = true
        });

        selectedStepIndex = selectedCutscene.steps.Count - 1;
        isDirty = true;
        EditorUtility.SetDirty(selectedCutscene);
    }

    private void ShowQuickAddMenu()
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Dialogue/Show Dialogue"), false, () => QuickAddStep(CutsceneStep.StepType.ShowDialogue));
        menu.AddItem(new GUIContent("Dialogue/Show Monologue"), false, () => QuickAddStep(CutsceneStep.StepType.ShowMonologue));
        menu.AddItem(new GUIContent("Dialogue/Show Message"), false, () => QuickAddStep(CutsceneStep.StepType.ShowMessage));

        menu.AddSeparator("");

        menu.AddItem(new GUIContent("Player/Disable Movement"), false, () => QuickAddStep(CutsceneStep.StepType.DisablePlayerMovement));
        menu.AddItem(new GUIContent("Player/Enable Movement"), false, () => QuickAddStep(CutsceneStep.StepType.EnablePlayerMovement));
        menu.AddItem(new GUIContent("Player/Teleport"), false, () => QuickAddStep(CutsceneStep.StepType.TeleportPlayer));
        menu.AddItem(new GUIContent("Player/Walk to Location"), false, () => QuickAddStep(CutsceneStep.StepType.MovePlayerWalk));

        menu.AddSeparator("");

        menu.AddItem(new GUIContent("NPC/Spawn"), false, () => QuickAddStep(CutsceneStep.StepType.SpawnNPC));
        menu.AddItem(new GUIContent("NPC/Despawn"), false, () => QuickAddStep(CutsceneStep.StepType.DespawnNPC));
        menu.AddItem(new GUIContent("NPC/Walk to Location"), false, () => QuickAddStep(CutsceneStep.StepType.MoveNPCTo));

        menu.AddSeparator("");

        menu.AddItem(new GUIContent("Camera/Shake"), false, () => QuickAddStep(CutsceneStep.StepType.CameraShake));
        menu.AddItem(new GUIContent("Camera/Focus on Target"), false, () => QuickAddStep(CutsceneStep.StepType.CameraFocusOn));
        menu.AddItem(new GUIContent("Camera/Focus on Player"), false, () => QuickAddStep(CutsceneStep.StepType.CameraFollowPlayer));

        menu.AddSeparator("");

        menu.AddItem(new GUIContent("Game State/Set Flag"), false, () => QuickAddStep(CutsceneStep.StepType.SetFlag));
        menu.AddItem(new GUIContent("Game State/Start Quest"), false, () => QuickAddStep(CutsceneStep.StepType.StartQuest));
        menu.AddItem(new GUIContent("Game State/Complete Quest"), false, () => QuickAddStep(CutsceneStep.StepType.CompleteQuest));

        menu.AddSeparator("");

        menu.AddItem(new GUIContent("Time/Set Time of Day"), false, () => QuickAddStep(CutsceneStep.StepType.SetTimeOfDay));
        menu.AddItem(new GUIContent("Time/Pause Time"), false, () => QuickAddStep(CutsceneStep.StepType.PauseGameTime));

        menu.AddSeparator("");

        menu.AddItem(new GUIContent("Flow/Wait Seconds"), false, () => QuickAddStep(CutsceneStep.StepType.WaitSeconds));
        menu.AddItem(new GUIContent("Flow/Wait for Dialogue"), false, () => QuickAddStep(CutsceneStep.StepType.WaitForDialogueEnd));
        menu.AddItem(new GUIContent("Flow/Fade to Black"), false, () => QuickAddStep(CutsceneStep.StepType.FadeToBlack));
        menu.AddItem(new GUIContent("Flow/Fade from Black"), false, () => QuickAddStep(CutsceneStep.StepType.FadeFromBlack));

        menu.ShowAsContext();
    }

    private void QuickAddStep(CutsceneStep.StepType type)
    {
        if (selectedCutscene == null) return;

        if (selectedCutscene.steps == null)
            selectedCutscene.steps = new List<CutsceneStep>();

        selectedCutscene.steps.Add(new CutsceneStep
        {
            type = type,
            duration = 1f,
            waitForCompletion = true
        });

        selectedStepIndex = selectedCutscene.steps.Count - 1;
        isDirty = true;
        EditorUtility.SetDirty(selectedCutscene);
    }

    private void DeleteStep(int index)
    {
        if (selectedCutscene == null || selectedCutscene.steps == null) return;

        selectedCutscene.steps.RemoveAt(index);

        if (selectedStepIndex >= selectedCutscene.steps.Count)
            selectedStepIndex = selectedCutscene.steps.Count - 1;

        isDirty = true;
        EditorUtility.SetDirty(selectedCutscene);
    }

    private void MoveStep(int index, int direction)
    {
        if (selectedCutscene == null || selectedCutscene.steps == null) return;

        int newIndex = index + direction;
        if (newIndex < 0 || newIndex >= selectedCutscene.steps.Count) return;

        var temp = selectedCutscene.steps[index];
        selectedCutscene.steps[index] = selectedCutscene.steps[newIndex];
        selectedCutscene.steps[newIndex] = temp;

        selectedStepIndex = newIndex;
        isDirty = true;
        EditorUtility.SetDirty(selectedCutscene);
    }

    private void SaveAllChanges()
    {
        foreach (var cutscene in allCutscenes)
        {
            EditorUtility.SetDirty(cutscene);
        }
        AssetDatabase.SaveAssets();
        isDirty = false;
        Debug.Log("Cutscene Editor: All changes saved");
    }

    private void RefreshCutsceneList()
    {
        allCutscenes.Clear();

        // Load from Resources
        CutsceneData[] assets = Resources.LoadAll<CutsceneData>("Cutscenes");
        allCutscenes.AddRange(assets);

        // Also find any outside Resources
        string[] guids = AssetDatabase.FindAssets("t:CutsceneData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CutsceneData cutscene = AssetDatabase.LoadAssetAtPath<CutsceneData>(path);
            if (cutscene != null && !allCutscenes.Contains(cutscene))
                allCutscenes.Add(cutscene);
        }
    }

    #endregion
}
