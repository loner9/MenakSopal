using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Scene transition system with automatic save functionality.
/// Integrates with GameSaveManager to auto-save before scene changes.
/// </summary>
public class SceneTransitionWithSave : MonoBehaviour
{
    [Header("Transition Settings")]
    public bool autoSaveBeforeTransition = true;
    public bool showTransitionUI = true;
    public string transitionSceneName = ""; // Optional loading scene
    
    [Header("Save Settings")]
    public bool saveOnlyIfProgressed = true; // Only save if story flags changed
    public string[] importantFlags; // Flags that indicate story progression
    
    private static SceneTransitionWithSave instance;
    public static SceneTransitionWithSave Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SceneTransitionWithSave>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SceneTransitionWithSave");
                    instance = go.AddComponent<SceneTransitionWithSave>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Load scene with automatic save
    /// </summary>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithSave(sceneName));
    }
    
    /// <summary>
    /// Load scene by index with automatic save
    /// </summary>
    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneWithSave(sceneIndex));
    }
    
    /// <summary>
    /// Load scene triggered by flag change
    /// </summary>
    public void LoadSceneOnFlag(string sceneName, string triggerFlag)
    {
        FlagMonitorSystem.WatchFlagAdded(triggerFlag, () => {
            LoadScene(sceneName);
        });
    }
    
    IEnumerator LoadSceneWithSave(string sceneName)
    {
        yield return StartCoroutine(PrepareSceneTransition(sceneName));
        
        // Load the scene
        if (!string.IsNullOrEmpty(transitionSceneName))
        {
            // Load through transition scene
            SceneManager.LoadScene(transitionSceneName);
            yield return null;
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            // Direct scene load
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            yield return asyncLoad;
        }
    }
    
    IEnumerator LoadSceneWithSave(int sceneIndex)
    {
        string sceneName = GetSceneNameFromIndex(sceneIndex);
        yield return StartCoroutine(PrepareSceneTransition(sceneName));
        
        // Load the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        yield return asyncLoad;
    }
    
    IEnumerator PrepareSceneTransition(string targetScene)
    {
        Debug.Log($"[SceneTransition] Preparing transition to: {targetScene}");
        
        // Auto-save before transition
        if (autoSaveBeforeTransition && ShouldSaveForTransition())
        {
            bool saveSuccess = GameSaveManager.Instance.AutoSave($"BeforeScene_{targetScene}");
            if (saveSuccess)
            {
                Debug.Log($"[SceneTransition] Auto-saved before moving to {targetScene}");
            }
            
            // Wait a frame to ensure save completes
            yield return null;
        }
        
        // Show transition UI if enabled
        if (showTransitionUI)
        {
            // You can add fade/loading UI here
            yield return StartCoroutine(ShowTransitionEffect());
        }
    }
    
    bool ShouldSaveForTransition()
    {
        if (!saveOnlyIfProgressed) return true;
        
        // Check if any important story flags are set
        var npcInteractionSystem = FindObjectOfType<NPCInteractionSystem>();
        if (npcInteractionSystem == null) return true;
        
        foreach (string flag in importantFlags)
        {
            if (npcInteractionSystem.HasGameFlag(flag))
            {
                return true; // Story has progressed, save is warranted
            }
        }
        
        return false; // No story progression, skip save
    }
    
    IEnumerator ShowTransitionEffect()
    {
        // Simple fade or loading effect
        // You can integrate this with your existing transition system
        yield return new WaitForSeconds(0.5f);
    }
    
    string GetSceneNameFromIndex(int index)
    {
        if (index >= 0 && index < SceneManager.sceneCountInBuildSettings)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(index);
            return System.IO.Path.GetFileNameWithoutExtension(scenePath);
        }
        return $"Scene_{index}";
    }
    
    #region Integration Methods
    
    /// <summary>
    /// Set up common scene transitions for your game
    /// </summary>
    public void SetupGameSceneTransitions()
    {
        // Village to Forest
        FlagMonitorSystem.WatchFlagAdded("journey_to_forest", () => {
            LoadScene("SceneHutan");
        });
        
        // Forest to Desa Krandon
        FlagMonitorSystem.WatchFlagAdded("journey_to_krandon", () => {
            LoadScene("SceneDesaKrandon");
        });
        
        // Return to village
        FlagMonitorSystem.WatchFlagAdded("return_to_village", () => {
            LoadScene("SceneAwal");
        });
        
        Debug.Log("[SceneTransition] Game scene transitions configured");
    }
    
    /// <summary>
    /// Quick transition for quest triggers
    /// </summary>
    public void TransitionForQuest(string questID, string targetScene)
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.AutoSave($"Quest_{questID}_SceneChange");
        }
        LoadScene(targetScene);
    }
    
    #endregion
}

/// <summary>
/// Component to add to QuestTrigger for scene transitions
/// </summary>
[System.Serializable]
public class SceneTransitionData
{
    public string targetScene;
    public bool saveBeforeTransition = true;
    public string saveDescription = "";
    public float transitionDelay = 0f;
}