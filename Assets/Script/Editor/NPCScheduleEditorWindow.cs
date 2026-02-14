using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class NPCScheduleEditorWindow : EditorWindow
{
    private Vector2 scrollPos;
    private Vector2 editorScrollPos;
    private List<NPCScheduleData> allSchedules = new List<NPCScheduleData>();
    private NPCScheduleData selectedSchedule;
    private string searchText = "";

    // UI Styling
    private GUIStyle headerStyle;
    private GUIStyle timeBlockStyle;
    private GUIStyle selectedBlockStyle;

    [MenuItem("Window/Story Tools/Schedule Designer")]
    public static void ShowWindow()
    {
        GetWindow<NPCScheduleEditorWindow>("Schedule Designer");
    }

    private void OnEnable()
    {
        LoadAllSchedules();
    }

    private void LoadAllSchedules()
    {
        allSchedules.Clear();
        string[] guids = AssetDatabase.FindAssets("t:NPCScheduleData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            NPCScheduleData data = AssetDatabase.LoadAssetAtPath<NPCScheduleData>(path);
            if (data != null) allSchedules.Add(data);
        }
    }

    private void OnGUI()
    {
        InitializeStyles();

        EditorGUILayout.BeginHorizontal();

        // Left Panel: Schedule List
        DrawScheduleList();

        // Right Panel: Editor
        DrawScheduleEditor();

        EditorGUILayout.EndHorizontal();
    }

    private void InitializeStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;

            timeBlockStyle = new GUIStyle(GUI.skin.box);
            selectedBlockStyle = new GUIStyle(GUI.skin.box);
            selectedBlockStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.5f, 0.8f, 0.5f));
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void DrawScheduleList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(250), GUILayout.ExpandHeight(true));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("NPC Schedules", headerStyle);

        // Search
        searchText = EditorGUILayout.TextField("Search", searchText);

        EditorGUILayout.Space(5);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, "box");

        var filteredList = allSchedules
            .Where(s => string.IsNullOrEmpty(searchText) || s.name.ToLower().Contains(searchText.ToLower()))
            .OrderBy(s => s.name)
            .ToList();

        foreach (var schedule in filteredList)
        {
            if (GUILayout.Toggle(selectedSchedule == schedule, schedule.name, "Button"))
            {
                if (selectedSchedule != schedule)
                {
                    selectedSchedule = schedule;
                    GUI.FocusControl(null);
                }
            }
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Refresh List")) LoadAllSchedules();

        EditorGUILayout.EndVertical();

        // Vertical Divider
        Rect dividerRect = new Rect(255, 0, 1, position.height);
        EditorGUI.DrawRect(dividerRect, Color.gray);
    }

    private void DrawScheduleEditor()
    {
        if (selectedSchedule == null)
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Select a schedule from the list to edit", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Editing: {selectedSchedule.name}", headerStyle);
        if (GUILayout.Button("Ping Asset", GUILayout.Width(100))) EditorGUIUtility.PingObject(selectedSchedule);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos);

        SerializedObject so = new SerializedObject(selectedSchedule);
        so.Update();

        // 1. Basic Info
        EditorGUILayout.PropertyField(so.FindProperty("scheduleName"));
        EditorGUILayout.PropertyField(so.FindProperty("scheduleDescription"));
        EditorGUILayout.PropertyField(so.FindProperty("spawnHour"));

        EditorGUILayout.Space(10);

        // 2. Timeline Visualization
        DrawTimeline(selectedSchedule);

        EditorGUILayout.Space(10);

        // 3. Home Settings
        EditorGUILayout.LabelField("Home Location", EditorStyles.boldLabel);
        DrawTargetSelector(so.FindProperty("homeObjectTag"), so.FindProperty("homeObjectName"), so.FindProperty("homePosition"));

        EditorGUILayout.Space(10);

        // 4. Schedule Events
        DrawEventsList(so);

        EditorGUILayout.Space(10);

        so.ApplyModifiedProperties();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private void DrawTimeline(NPCScheduleData schedule)
    {
        EditorGUILayout.LabelField("24-Hour Routine Visualizer", EditorStyles.boldLabel);
        Rect timelineRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(timelineRect, new Color(0.15f, 0.15f, 0.15f));

        // Draw Hour Markers
        float widthPerHour = timelineRect.width / 24f;

        for (int i = 0; i <= 24; i++)
        {
            float x = timelineRect.x + (i * widthPerHour);
            EditorGUI.DrawRect(new Rect(x, timelineRect.y, 1, timelineRect.height), new Color(0.3f, 0.3f, 0.3f));
            if (i % 6 == 0)
            {
                EditorGUI.LabelField(new Rect(x, timelineRect.y + 20, 20, 20), i.ToString(), EditorStyles.miniLabel);
            }
        }

        // Sort events by hour for visualization
        var events = schedule.scheduleEvents != null ? schedule.scheduleEvents.Where(e => e != null).OrderBy(e => e.hour).ToList() : new List<ScheduleEvent>();

        for (int i = 0; i < events.Count; i++)
        {
            var evt = events[i];
            float startX = timelineRect.x + (evt.hour * widthPerHour);
            float endX = (i + 1 < events.Count) ? timelineRect.x + (events[i + 1].hour * widthPerHour) : timelineRect.x + timelineRect.width;

            Rect blockRect = new Rect(startX, timelineRect.y + 5, endX - startX - 1, timelineRect.height - 25);

            Color blockColor = GetBehaviorColor(evt.behavior);
            EditorGUI.DrawRect(blockRect, blockColor);

            // Interaction: Clicking block selects the event in the list
            if (Event.current.type == EventType.MouseDown && blockRect.Contains(Event.current.mousePosition))
            {
                // This would be cool to auto-scroll to the event, but for now just show it's active
            }

            string label = evt.behavior.ToString();
            GUI.Label(blockRect, label, EditorStyles.miniLabel);
        }
    }

    private Color GetBehaviorColor(NPCBehavior behavior)
    {
        switch (behavior)
        {
            case NPCBehavior.Idle: return new Color(0.5f, 0.5f, 0.5f, 0.8f);
            case NPCBehavior.Walk: return new Color(0.2f, 0.7f, 0.2f, 0.8f);
            case NPCBehavior.Work: return new Color(0.8f, 0.5f, 0.2f, 0.8f);
            case NPCBehavior.Sleep: return new Color(0.2f, 0.4f, 0.8f, 0.8f);
            case NPCBehavior.Interact: return new Color(0.8f, 0.2f, 0.8f, 0.8f);
            case NPCBehavior.Flee: return new Color(0.8f, 0.2f, 0.2f, 0.8f);
            default: return Color.gray;
        }
    }

    private void DrawTargetSelector(SerializedProperty tagProp, SerializedProperty nameProp, SerializedProperty posProp)
    {
        EditorGUILayout.BeginVertical("helpbox");

        // Tag Selection
        string[] allTags = UnityEditorInternal.InternalEditorUtility.tags;
        int currentTagIndex = System.Array.IndexOf(allTags, tagProp.stringValue);
        if (currentTagIndex == -1) currentTagIndex = 0;

        int newTagIndex = EditorGUILayout.Popup("Target Tag", currentTagIndex, allTags);
        tagProp.stringValue = allTags[newTagIndex];

        // Name Selection (Smart Dropdown)
        string[] availableObjects = GetObjectNamesWithTag(tagProp.stringValue);

        EditorGUILayout.BeginHorizontal();

        if (availableObjects.Length > 0)
        {
            int currentNameIndex = System.Array.IndexOf(availableObjects, nameProp.stringValue);
            int newNameIndex = EditorGUILayout.Popup("Target Name", currentNameIndex, availableObjects);
            if (newNameIndex >= 0) nameProp.stringValue = availableObjects[newNameIndex];
        }
        else
        {
            EditorGUILayout.PropertyField(nameProp);
        }

        // Manual override button
        if (GUILayout.Button("Manual", GUILayout.Width(60)))
        {
            nameProp.stringValue = "";
        }

        EditorGUILayout.EndHorizontal();

        // Manual Position (visible only if name is empty)
        if (string.IsNullOrEmpty(nameProp.stringValue))
        {
            EditorGUILayout.PropertyField(posProp);
        }
        else
        {
            // Validation & Focus
            GameObject target = GameObject.Find(nameProp.stringValue);
            if (target != null)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField("✓ Object found in scene", EditorStyles.miniLabel);
                GUI.color = Color.white;

                if (GUILayout.Button("Focus In Scene", GUILayout.Width(120)))
                {
                    Selection.activeGameObject = target;
                    SceneView.FrameLastActiveSceneView();
                }
            }
            else
            {
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField("⚠ Object not found! (Manual position will be used as fallback)", EditorStyles.miniLabel);
                GUI.color = Color.white;
                EditorGUILayout.PropertyField(posProp);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private string[] GetObjectNamesWithTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return new string[0];
        return GameObject.FindGameObjectsWithTag(tag).Select(obj => obj.name).OrderBy(n => n).ToArray();
    }

    private void DrawEventsList(SerializedObject so)
    {
        SerializedProperty eventsProp = so.FindProperty("scheduleEvents");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Schedule Events", EditorStyles.boldLabel);
        if (GUILayout.Button("+ Add Event", GUILayout.Width(100)))
        {
            eventsProp.arraySize++;
            var newEntry = eventsProp.GetArrayElementAtIndex(eventsProp.arraySize - 1);
            newEntry.FindPropertyRelative("hour").intValue = 12;
            newEntry.FindPropertyRelative("targetObjectTag").stringValue = "NPCTarget";
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Group events by index to allow sorting visually in the UI
        List<int> sortedIndices = new List<int>();
        for (int i = 0; i < eventsProp.arraySize; i++) sortedIndices.Add(i);

        sortedIndices = sortedIndices.OrderBy(idx => eventsProp.GetArrayElementAtIndex(idx).FindPropertyRelative("hour").intValue).ToList();

        foreach (int idx in sortedIndices)
        {
            SerializedProperty element = eventsProp.GetArrayElementAtIndex(idx);

            EditorGUILayout.BeginVertical("box");
            EditorGUIUtility.labelWidth = 120;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(element.FindPropertyRelative("hour"), new GUIContent("Time (H)"));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                eventsProp.DeleteArrayElementAtIndex(idx);
                break; // Break loop since array modified
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(element.FindPropertyRelative("behavior"));

            // Targeted Location
            DrawTargetSelector(
                element.FindPropertyRelative("targetObjectTag"),
                element.FindPropertyRelative("targetObjectName"),
                element.FindPropertyRelative("targetPosition")
            );

            EditorGUILayout.PropertyField(element.FindPropertyRelative("shouldIdleWhenReached"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("shouldDespawn"));

            // Custom Dialogue
            SerializedProperty dialogs = element.FindPropertyRelative("customDialogue");
            EditorGUILayout.PropertyField(dialogs, true);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
    }
}
