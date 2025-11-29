using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor window for testing CameraShake in the Unity Editor.
/// Access via: Window → Camera Shake Tester
/// </summary>
public class CameraShakeEditorWindow : EditorWindow
{
    private float customDuration = 0.5f;
    private float customIntensity = 2.0f;
    private float customFrequency = 1.0f;

    private CameraShake cameraShake;
    private Vector2 scrollPosition;

    [MenuItem("Window/Camera Shake Tester")]
    public static void ShowWindow()
    {
        CameraShakeEditorWindow window = GetWindow<CameraShakeEditorWindow>("Camera Shake Tester");
        window.minSize = new Vector2(300, 450);
    }

    private void OnEnable()
    {
        FindCameraShake();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Title
        EditorGUILayout.Space(10);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Camera Shake Tester", titleStyle);
        EditorGUILayout.Space(10);

        // Check for CameraShake instance
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test Camera Shake", MessageType.Info);
            EditorGUILayout.Space(10);
        }
        else if (cameraShake == null)
        {
            EditorGUILayout.HelpBox("CameraShake instance not found!\n\nMake sure you have a CameraShake component in your scene.", MessageType.Warning);

            if (GUILayout.Button("Refresh", GUILayout.Height(30)))
            {
                FindCameraShake();
            }

            EditorGUILayout.EndScrollView();
            return;
        }

        // Status
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);

        if (Application.isPlaying && cameraShake != null)
        {
            EditorGUILayout.LabelField("CameraShake:", "Ready");
            EditorGUILayout.LabelField("Virtual Camera:", cameraShake.virtualCamera != null ? cameraShake.virtualCamera.name : "Not Assigned");
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);

        // Preset Shakes
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Preset Shakes", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        GUI.enabled = Application.isPlaying && cameraShake != null;

        // Light Shake
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Light Shake", GUILayout.Height(35)))
        {
            cameraShake.ShakeLight();
            Debug.Log("[CameraShake Tester] Testing Light Shake");
        }
        EditorGUILayout.LabelField($"({cameraShake?.lightShake.duration}s, {cameraShake?.lightShake.intensity})", GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(3);

        // Medium Shake
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Medium Shake", GUILayout.Height(35)))
        {
            cameraShake.ShakeMedium();
            Debug.Log("[CameraShake Tester] Testing Medium Shake");
        }
        EditorGUILayout.LabelField($"({cameraShake?.mediumShake.duration}s, {cameraShake?.mediumShake.intensity})", GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(3);

        // Heavy Shake
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Heavy Shake", GUILayout.Height(35)))
        {
            cameraShake.ShakeHeavy();
            Debug.Log("[CameraShake Tester] Testing Heavy Shake");
        }
        EditorGUILayout.LabelField($"({cameraShake?.heavyShake.duration}s, {cameraShake?.heavyShake.intensity})", GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(3);

        // Explosion Shake
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("💥 Explosion Shake", GUILayout.Height(40)))
        {
            cameraShake.ShakeExplosion();
            Debug.Log("[CameraShake Tester] Testing Explosion Shake");
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.LabelField($"({cameraShake?.explosionShake.duration}s, {cameraShake?.explosionShake.intensity})", GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);

        // Custom Shake
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Custom Shake", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        customDuration = EditorGUILayout.Slider("Duration (s)", customDuration, 0.1f, 2f);
        customIntensity = EditorGUILayout.Slider("Intensity", customIntensity, 0f, 10f);
        customFrequency = EditorGUILayout.Slider("Frequency", customFrequency, 0f, 5f);

        EditorGUILayout.Space(5);

        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
        if (GUILayout.Button("Test Custom Shake", GUILayout.Height(35)))
        {
            cameraShake.Shake(customDuration, customIntensity, customFrequency);
            Debug.Log($"[CameraShake Tester] Testing Custom Shake - Duration: {customDuration}s, Intensity: {customIntensity}, Frequency: {customFrequency}");
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);

        // Stop Button
        GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
        if (GUILayout.Button("⏹ Stop Shake", GUILayout.Height(40)))
        {
            cameraShake.StopShake();
            Debug.Log("[CameraShake Tester] Shake Stopped");
        }
        GUI.backgroundColor = Color.white;

        GUI.enabled = true;

        EditorGUILayout.Space(10);

        // Instructions
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Quick Tips", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("• Light: Small impacts, footsteps", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField("• Medium: Player damage, hits", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField("• Heavy: Large impacts, boss attacks", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField("• Explosion: Building collapse, big events", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }

    private void FindCameraShake()
    {
        if (Application.isPlaying)
        {
            cameraShake = CameraShake.Instance;

            if (cameraShake == null)
            {
                cameraShake = FindObjectOfType<CameraShake>();
            }
        }
    }

    // Update window when entering/exiting play mode
    private void OnInspectorUpdate()
    {
        if (Application.isPlaying && cameraShake == null)
        {
            FindCameraShake();
            Repaint();
        }
    }
}
