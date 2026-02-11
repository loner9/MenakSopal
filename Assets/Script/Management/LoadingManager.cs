using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject loadingCanvas;
    public CanvasGroup canvasGroup;
    public Slider progressBar;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI progressPercentageText;

    [Header("Settings")]
    public float fadeDuration = 0.5f;
    public float minLoadingTime = 1.0f; // Minimum time to show loading screen
    public float postLoadDelay = 0.5f; // Extra time after everything is ready

    private bool isTransitioning = false;
    private List<string> pendingSystems = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadingCanvas != null)
            loadingCanvas.SetActive(false);

        if (canvasGroup != null)
            canvasGroup.alpha = 0;
    }

    /// <summary>
    /// Call this to transition to a new scene with a loading screen.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(LoadSequence(sceneName));
    }

    /// <summary>
    /// Systems like NPCManager or DayNightCycle can register themselves as "Busy"
    /// to prevent the loading screen from closing too early.
    /// </summary>
    public void RegisterSystemBusy(string systemName)
    {
        if (!pendingSystems.Contains(systemName))
        {
            pendingSystems.Add(systemName);
            Debug.Log($"[LoadingManager] System '{systemName}' is now BUSY.");
        }
    }

    public void ReportSystemReady(string systemName)
    {
        if (pendingSystems.Contains(systemName))
        {
            pendingSystems.Remove(systemName);
            Debug.Log($"[LoadingManager] System '{systemName}' is now READY.");
        }
    }

    private IEnumerator LoadSequence(string sceneName)
    {
        isTransitioning = true;
        pendingSystems.Clear();

        // 1. Show Loading UI
        if (loadingCanvas != null) loadingCanvas.SetActive(true);
        yield return StartCoroutine(Fade(1.0f));

        float startTime = Time.time;

        // 2. Start Async Loading
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            float progress = MathUtils.Map(operation.progress, 0, 0.9f, 0, 1.0f);
            UpdateUI(progress, "Loading Assets...");
            yield return null;
        }

        // 3. Activate Scene
        UpdateUI(0.9f, "Initializing Scene...");
        operation.allowSceneActivation = true;

        // Wait for scene to actually change
        while (!operation.isDone)
        {
            yield return null;
        }

        // 4. Wait for Internal Systems
        // We wait at least one frame for Awake/Start methods of the new scene to run
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);

        UpdateUI(0.95f, "Finalizing World...");

        // Wait until all systems that registered themselves are ready
        while (pendingSystems.Count > 0)
        {
            string busyList = string.Join(", ", pendingSystems);
            UpdateUI(0.95f, $"Waiting for: {busyList}");
            yield return null;
        }

        // 5. Enforce Minimum Loading Time
        float elapsedSinceStart = Time.time - startTime;
        if (elapsedSinceStart < minLoadingTime)
        {
            yield return new WaitForSeconds(minLoadingTime - elapsedSinceStart);
        }

        // 6. Post-Load Delay (0.5s as requested)
        UpdateUI(1.0f, "Ready!");
        yield return new WaitForSeconds(postLoadDelay);

        // 7. Hide Loading UI
        yield return StartCoroutine(Fade(0.0f));
        if (loadingCanvas != null) loadingCanvas.SetActive(false);

        isTransitioning = false;
    }

    private void UpdateUI(float progress, string status)
    {
        if (progressBar != null) progressBar.value = progress;
        if (statusText != null) statusText.text = status;
        if (progressPercentageText != null) progressPercentageText.text = $"{(progress * 100):F0}%";
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (canvasGroup == null) yield break;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}

public static class MathUtils
{
    public static float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
    {
        return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }
}
