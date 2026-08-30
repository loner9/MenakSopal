using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Testing tool for CameraShake system.
/// Provides UI buttons and keyboard shortcuts to test different shake intensities.
/// Attach to a GameObject in your scene for runtime testing.
/// </summary>
public class CameraShakeTester : MonoBehaviour
{
    [Header("UI References (Optional)")]
    [Tooltip("If you want UI buttons, assign a Canvas here")]
    public Canvas testCanvas;

    [Header("Keyboard Testing")]
    [Tooltip("Enable keyboard shortcuts for quick testing")]
    public bool enableKeyboardShortcuts = true;

    [Header("Custom Test Settings")]
    public float customDuration = 0.5f;
    [Range(0f, 10f)]
    public float customIntensity = 2.0f;
    [Range(0f, 5f)]
    public float customFrequency = 1.0f;

    [Header("Debug")]
    public bool showInstructions = true;

    private void Start()
    {
        if (showInstructions)
        {
            PrintInstructions();
        }

        // Auto-create test UI if canvas is assigned
        if (testCanvas != null)
        {
            CreateTestUI();
        }
    }

    private void Update()
    {
        if (!enableKeyboardShortcuts) return;

        // Keyboard shortcuts for testing
        if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.Alpha1) || ControlFreak2.CF2Input.GetKeyDown(KeyCode.Keypad1))
        {
            TestLight();
        }
        else if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.Alpha2) || ControlFreak2.CF2Input.GetKeyDown(KeyCode.Keypad2))
        {
            TestMedium();
        }
        else if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.Alpha3) || ControlFreak2.CF2Input.GetKeyDown(KeyCode.Keypad3))
        {
            TestHeavy();
        }
        else if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.Alpha4) || ControlFreak2.CF2Input.GetKeyDown(KeyCode.Keypad4))
        {
            TestExplosion();
        }
        else if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.Alpha5) || ControlFreak2.CF2Input.GetKeyDown(KeyCode.Keypad5))
        {
            TestCustom();
        }
        else if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.Escape) || ControlFreak2.CF2Input.GetKeyDown(KeyCode.Alpha0))
        {
            StopShake();
        }
    }

    #region Test Methods

    [ContextMenu("Test - Light Shake")]
    public void TestLight()
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeLight();
            Debug.Log("[CameraShakeTester] Testing Light Shake");
        }
        else
        {
            Debug.LogError("[CameraShakeTester] CameraShake.Instance not found!");
        }
    }

    [ContextMenu("Test - Medium Shake")]
    public void TestMedium()
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeMedium();
            Debug.Log("[CameraShakeTester] Testing Medium Shake");
        }
        else
        {
            Debug.LogError("[CameraShakeTester] CameraShake.Instance not found!");
        }
    }

    [ContextMenu("Test - Heavy Shake")]
    public void TestHeavy()
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeHeavy();
            Debug.Log("[CameraShakeTester] Testing Heavy Shake");
        }
        else
        {
            Debug.LogError("[CameraShakeTester] CameraShake.Instance not found!");
        }
    }

    [ContextMenu("Test - Explosion Shake")]
    public void TestExplosion()
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeExplosion();
            Debug.Log("[CameraShakeTester] Testing Explosion Shake");
        }
        else
        {
            Debug.LogError("[CameraShakeTester] CameraShake.Instance not found!");
        }
    }

    [ContextMenu("Test - Custom Shake")]
    public void TestCustom()
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(customDuration, customIntensity, customFrequency);
            Debug.Log($"[CameraShakeTester] Testing Custom Shake - Duration: {customDuration}s, Intensity: {customIntensity}, Frequency: {customFrequency}");
        }
        else
        {
            Debug.LogError("[CameraShakeTester] CameraShake.Instance not found!");
        }
    }

    [ContextMenu("Stop Shake")]
    public void StopShake()
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.StopShake();
            Debug.Log("[CameraShakeTester] Stopping Shake");
        }
    }

    #endregion

    #region UI Creation

    private void CreateTestUI()
    {
        // Create a panel for buttons
        GameObject panel = new GameObject("CameraShake Test Panel");
        panel.transform.SetParent(testCanvas.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(10, -10);
        panelRect.sizeDelta = new Vector2(200, 250);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);

        // Create title
        CreateLabel(panel.transform, "Camera Shake Test", new Vector2(100, -15));

        // Create buttons
        float yPos = -45;
        float spacing = 35;

        CreateButton(panel.transform, "1. Light", new Vector2(100, yPos), TestLight);
        yPos -= spacing;

        CreateButton(panel.transform, "2. Medium", new Vector2(100, yPos), TestMedium);
        yPos -= spacing;

        CreateButton(panel.transform, "3. Heavy", new Vector2(100, yPos), TestHeavy);
        yPos -= spacing;

        CreateButton(panel.transform, "4. Explosion", new Vector2(100, yPos), TestExplosion);
        yPos -= spacing;

        CreateButton(panel.transform, "5. Custom", new Vector2(100, yPos), TestCustom);
        yPos -= spacing;

        CreateButton(panel.transform, "0. Stop", new Vector2(100, yPos), StopShake);

        Debug.Log("[CameraShakeTester] Test UI created successfully");
    }

    private void CreateButton(Transform parent, string text, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonGO = new GameObject($"Button_{text}");
        buttonGO.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1);
        rectTransform.anchorMax = new Vector2(0.5f, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(180, 30);

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Button button = buttonGO.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        // Button text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        Text textComponent = textGO.AddComponent<Text>();
        textComponent.text = text;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.fontSize = 14;

        // Try to find a font
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private void CreateLabel(Transform parent, string text, Vector2 position)
    {
        GameObject labelGO = new GameObject($"Label_{text}");
        labelGO.transform.SetParent(parent, false);

        RectTransform rectTransform = labelGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1);
        rectTransform.anchorMax = new Vector2(0.5f, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(180, 25);

        Text textComponent = labelGO.AddComponent<Text>();
        textComponent.text = text;
        textComponent.color = Color.yellow;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.fontSize = 16;
        textComponent.fontStyle = FontStyle.Bold;

        // Try to find a font
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    #endregion

    private void PrintInstructions()
    {
        Debug.Log("=== CAMERA SHAKE TESTER ===\n" +
                  "Keyboard Shortcuts:\n" +
                  "  1 - Light Shake\n" +
                  "  2 - Medium Shake\n" +
                  "  3 - Heavy Shake\n" +
                  "  4 - Explosion Shake\n" +
                  "  5 - Custom Shake (adjust in Inspector)\n" +
                  "  0 or ESC - Stop Shake\n\n" +
                  "You can also right-click this component in Inspector and use Context Menu options.\n" +
                  "=========================");
    }

    private void OnGUI()
    {
        if (!showInstructions) return;

        // Show keyboard shortcuts on screen
        GUI.color = Color.yellow;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 12;

        string instructions =
            "Camera Shake Test:\n" +
            "1-Light  2-Medium  3-Heavy  4-Explosion  5-Custom  0-Stop";

        GUI.Label(new Rect(10, Screen.height - 50, 500, 40), instructions, style);
    }
}
