using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

// Custom ReadOnly attribute for inspector
public class ReadOnlyAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
{
    public override float GetPropertyHeight(UnityEditor.SerializedProperty property,
                                            GUIContent label)
    {
        return UnityEditor.EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               UnityEditor.SerializedProperty property,
                               GUIContent label)
    {
        GUI.enabled = false;
        UnityEditor.EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif

[System.Serializable]
public class TimeSettings
{
    [Header("Cycle Duration")]
    [Range(30f, 1800f)] // 30 seconds to 30 minutes
    public float totalCycleDuration = 120f; // Total day/night cycle in real seconds (2 minutes default)
    
    [Range(0f, 24f)]
    public float startTime = 6f; // 6 AM
    
    [Header("Day Period Settings")]
    [Range(6f, 18f)]
    public float dayStartHour = 7f; // When day starts (after sunrise)
    [Range(12f, 20f)]
    public float dayEndHour = 17f; // When day ends (before sunset)
    
    [Header("Transition Settings")]
    [Range(0.05f, 0.25f)]
    public float transitionRatio = 0.1f; // Percentage of cycle for each sunrise/sunset
    
    [Header("Calculated Times (Read Only)")]
    [SerializeField, ReadOnly] public float calculatedSunriseStart;
    [SerializeField, ReadOnly] public float calculatedSunriseEnd;
    [SerializeField, ReadOnly] public float calculatedSunsetStart;
    [SerializeField, ReadOnly] public float calculatedSunsetEnd;
    [SerializeField, ReadOnly] public float calculatedDayStart;
    [SerializeField, ReadOnly] public float calculatedNightStart;
    
    [Header("Real Time Info (Read Only)")]
    [SerializeField, ReadOnly] public float realDayDuration;
    [SerializeField, ReadOnly] public float realNightDuration;
    [SerializeField, ReadOnly] public float realTransitionDuration;
}

[System.Serializable]
public class LightingSettings
{
    [Header("Ambient Light Colors")]
    public Color dayAmbientColor = new Color(1f, 0.95f, 0.8f);
    public Color sunriseAmbientColor = new Color(1f, 0.6f, 0.4f);
    public Color sunsetAmbientColor = new Color(0.8f, 0.4f, 0.6f);
    public Color nightAmbientColor = new Color(0.2f, 0.2f, 0.4f);
    
    [Header("Ambient Light Intensity")]
    [Range(0f, 2f)]
    public float dayAmbientIntensity = 1f;
    [Range(0f, 2f)]
    public float sunriseAmbientIntensity = 0.7f;
    [Range(0f, 2f)]
    public float sunsetAmbientIntensity = 0.6f;
    [Range(0f, 2f)]
    public float nightAmbientIntensity = 0.3f;
    
    [Header("Directional Light (Sun/Moon)")]
    public Light directionalLight;
    public Color dayLightColor = Color.white;
    public Color sunriseLightColor = new Color(1f, 0.7f, 0.4f);
    public Color sunsetLightColor = new Color(1f, 0.5f, 0.3f);
    public Color nightLightColor = new Color(0.5f, 0.7f, 1f);
    
    [Range(0f, 3f)]
    public float dayLightIntensity = 1f;
    [Range(0f, 3f)]
    public float sunriseLightIntensity = 0.8f;
    [Range(0f, 3f)]
    public float sunsetLightIntensity = 0.6f;
    [Range(0f, 3f)]
    public float nightLightIntensity = 0.4f;
}

public enum TimeOfDay
{
    Day,
    Sunrise,
    Sunset,
    Night
}

public class DayNightCycle : MonoBehaviour
{
    [Header("System Settings")]
    public TimeSettings timeSettings;
    public LightingSettings lightingSettings;
    
    [Header("Optional Components")]
    public Camera mainCamera;
    public Light2D globalLight2D; // For URP 2D
    public SpriteRenderer skyRenderer; // For sky color changes
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    [Header("Current Time Info (Read Only)")]
    [SerializeField, ReadOnly] private float displayCurrentTime;
    [SerializeField, ReadOnly] private string displayTimeFormatted;
    [SerializeField, ReadOnly] private string displayCurrentPeriod;
    [SerializeField, ReadOnly] private float displayTimePercentage;
    [SerializeField, ReadOnly] private string displayNextPeriod;
    [SerializeField, ReadOnly] private string displayTimeUntilNext;
    public static DayNightCycle Instance { get; private set; }
    
    // Events
    public System.Action<TimeOfDay> OnTimeOfDayChanged;
    public System.Action<float> OnTimeChanged; // 0-24 hours
    
    // Private variables
    private float currentTime;
    private TimeOfDay currentTimeOfDay;
    private Color originalCameraBackground;
    private Coroutine timeProgressionCoroutine;
    
    // Properties
    public float CurrentTime => currentTime;
    public TimeOfDay CurrentTimeOfDay => currentTimeOfDay;
    public float TimePercentage => currentTime / 24f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            throw new System.Exception("An instance of this singleton already exists.");
        }
        else
        {
            Instance = this;
        }
        // Calculate transition times based on day/night durations
        CalculateTransitionTimes();

        // Initialize time
        currentTime = timeSettings.startTime;

        // Cache original camera background
        if (mainCamera != null)
        {
            originalCameraBackground = mainCamera.backgroundColor;
        }

        // Initialize lighting
        UpdateTimeOfDay();
        ApplyGradualLighting();
    }
    
    private void Start()
    {
        // Start the time cycle
        timeProgressionCoroutine = StartCoroutine(TimeProgressionCoroutine());
    }
    
    private void Update()
    {
        // Update inspector display values
        UpdateInspectorDisplay();
        
        // Optional debug ray (safe to use)
        if (showDebugInfo)
        {
            Debug.DrawRay(transform.position, Vector3.up, Color.yellow);
        }
    }
    
    private IEnumerator TimeProgressionCoroutine()
    {
        while (true)
        {
            // Calculate time increment based on total cycle duration
            float timeIncrement = (24f / timeSettings.totalCycleDuration) * Time.deltaTime;
            currentTime += timeIncrement;
            
            // Wrap around 24-hour cycle
            if (currentTime >= 24f)
            {
                currentTime = 0f;
            }
            
            // Update time of day
            TimeOfDay previousTimeOfDay = currentTimeOfDay;
            UpdateTimeOfDay();
            
            // Apply smooth lighting continuously
            ApplyGradualLighting();
            
            // Check for time of day changes for events
            if (previousTimeOfDay != currentTimeOfDay)
            {
                OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
            }
            
            OnTimeChanged?.Invoke(currentTime);
            
            yield return null;
        }
    }
    
    private void CalculateTransitionTimes()
    {
        // Calculate transition duration in game hours
        float transitionDuration = timeSettings.totalCycleDuration * timeSettings.transitionRatio;
        float transitionHours = (transitionDuration / timeSettings.totalCycleDuration) * 24f;
        float halfTransition = transitionHours * 0.5f;
        
        // Use direct hour settings for day period
        timeSettings.calculatedDayStart = timeSettings.dayStartHour;
        float dayEnd = timeSettings.dayEndHour;
        
        // Calculate transitions centered on day start/end times
        timeSettings.calculatedSunriseStart = timeSettings.dayStartHour - halfTransition;
        timeSettings.calculatedSunriseEnd = timeSettings.dayStartHour + halfTransition;
        timeSettings.calculatedSunsetStart = dayEnd - halfTransition; 
        timeSettings.calculatedSunsetEnd = dayEnd + halfTransition;
        timeSettings.calculatedNightStart = timeSettings.calculatedSunsetEnd;
        
        // Calculate actual durations for display
        float dayDurationHours = dayEnd - timeSettings.dayStartHour;
        float nightDurationHours = 24f - dayDurationHours - (transitionHours * 2f);
        
        timeSettings.realDayDuration = (dayDurationHours / 24f) * timeSettings.totalCycleDuration;
        timeSettings.realNightDuration = (nightDurationHours / 24f) * timeSettings.totalCycleDuration;
        timeSettings.realTransitionDuration = transitionDuration;
        
        // Handle wrap-around
        if (timeSettings.calculatedSunriseStart < 0f)
        {
            timeSettings.calculatedSunriseStart += 24f;
        }
        if (timeSettings.calculatedSunriseEnd > 24f)
        {
            timeSettings.calculatedSunriseEnd -= 24f;
        }
        
        Debug.Log($"Day: {timeSettings.dayStartHour:F1}-{dayEnd:F1}, " +
                 $"Sunrise: {timeSettings.calculatedSunriseStart:F1}-{timeSettings.calculatedSunriseEnd:F1}, " +
                 $"Sunset: {timeSettings.calculatedSunsetStart:F1}-{timeSettings.calculatedSunsetEnd:F1}");
    }
    
    private void UpdateTimeOfDay()
    {
        // Use calculated transition times instead of hardcoded values
        float time = currentTime;
        
        // Handle wrap-around for sunrise calculations
        if (timeSettings.calculatedSunriseStart > timeSettings.calculatedSunriseEnd)
        {
            // Sunrise spans midnight (e.g., 23:00 to 06:00)
            if (time >= timeSettings.calculatedSunriseStart || time <= timeSettings.calculatedSunriseEnd)
            {
                currentTimeOfDay = TimeOfDay.Sunrise;
                return;
            }
        }
        else
        {
            // Normal sunrise (doesn't span midnight)
            if (time >= timeSettings.calculatedSunriseStart && time < timeSettings.calculatedSunriseEnd)
            {
                currentTimeOfDay = TimeOfDay.Sunrise;
                return;
            }
        }
        
        // Check for day period
        if (time >= timeSettings.calculatedDayStart && time < timeSettings.calculatedSunsetStart)
        {
            currentTimeOfDay = TimeOfDay.Day;
            return;
        }
        
        // Check for sunset period
        if (time >= timeSettings.calculatedSunsetStart && time < timeSettings.calculatedSunsetEnd)
        {
            currentTimeOfDay = TimeOfDay.Sunset;
            return;
        }
        
        // Everything else is night
        currentTimeOfDay = TimeOfDay.Night;
    }
    
    private void ApplyGradualLighting()
    {
        // Get current lighting values based on continuous time interpolation
        Color targetAmbientColor;
        float targetAmbientIntensity;
        Color targetLightColor;
        float targetLightIntensity;
        
        GetInterpolatedLightingValues(out targetAmbientColor, out targetAmbientIntensity, 
                                    out targetLightColor, out targetLightIntensity);
        
        // Apply ambient lighting
        RenderSettings.ambientLight = targetAmbientColor;
        RenderSettings.ambientIntensity = targetAmbientIntensity;
        
        // Apply directional light
        if (lightingSettings.directionalLight != null)
        {
            lightingSettings.directionalLight.color = targetLightColor;
            lightingSettings.directionalLight.intensity = targetLightIntensity;
        }
        
        // Apply 2D global light (URP)
        if (globalLight2D != null)
        {
            globalLight2D.color = targetLightColor;
            globalLight2D.intensity = targetLightIntensity;
        }
        
        // Apply camera background color
        if (mainCamera != null)
        {
            Color bgColor = Color.Lerp(targetAmbientColor, Color.black, 0.3f);
            mainCamera.backgroundColor = bgColor;
        }
        
        // Apply sky color if using a sky renderer
        if (skyRenderer != null)
        {
            skyRenderer.color = targetAmbientColor;
        }
    }
    
    private void GetInterpolatedLightingValues(out Color ambientColor, out float ambientIntensity, 
                                             out Color lightColor, out float lightIntensity)
    {
        float time = currentTime;
        
        // Calculate peak times (middle of transitions)
        float sunrisePeak = timeSettings.calculatedDayStart; // Peak sunrise at day start time
        float sunsetPeak = (timeSettings.calculatedSunsetStart + timeSettings.calculatedSunsetEnd) * 0.5f; // Peak sunset at middle
        
        var points = new List<(float time, Color ambient, float ambientInt, Color light, float lightInt)>();
        
        // Night period
        points.Add((0f, lightingSettings.nightAmbientColor, lightingSettings.nightAmbientIntensity, lightingSettings.nightLightColor, lightingSettings.nightLightIntensity));
        
        // Sunrise transition - starts, peaks at designated time, then transitions to day
        points.Add((timeSettings.calculatedSunriseStart, lightingSettings.nightAmbientColor, lightingSettings.nightAmbientIntensity, lightingSettings.nightLightColor, lightingSettings.nightLightIntensity));
        
        // Peak sunrise at the designated sunrise time
        points.Add((sunrisePeak, lightingSettings.sunriseAmbientColor, lightingSettings.sunriseAmbientIntensity, lightingSettings.sunriseLightColor, lightingSettings.sunriseLightIntensity));
        
        points.Add((timeSettings.calculatedSunriseEnd, lightingSettings.dayAmbientColor, lightingSettings.dayAmbientIntensity, lightingSettings.dayLightColor, lightingSettings.dayLightIntensity));
        
        // Day period
        points.Add((timeSettings.calculatedDayStart, lightingSettings.dayAmbientColor, lightingSettings.dayAmbientIntensity, lightingSettings.dayLightColor, lightingSettings.dayLightIntensity));
        points.Add((12f, lightingSettings.dayAmbientColor, lightingSettings.dayAmbientIntensity, lightingSettings.dayLightColor, lightingSettings.dayLightIntensity));
        points.Add((timeSettings.calculatedSunsetStart, lightingSettings.dayAmbientColor, lightingSettings.dayAmbientIntensity, lightingSettings.dayLightColor, lightingSettings.dayLightIntensity));
        
        // Sunset transition - starts, peaks at designated time, then transitions to night
        // Peak sunset at the designated sunset time
        points.Add((sunsetPeak, lightingSettings.sunsetAmbientColor, lightingSettings.sunsetAmbientIntensity, lightingSettings.sunsetLightColor, lightingSettings.sunsetLightIntensity));
        
        points.Add((timeSettings.calculatedSunsetEnd, lightingSettings.nightAmbientColor, lightingSettings.nightAmbientIntensity, lightingSettings.nightLightColor, lightingSettings.nightLightIntensity));
        
        // Night period
        points.Add((timeSettings.calculatedNightStart, lightingSettings.nightAmbientColor, lightingSettings.nightAmbientIntensity, lightingSettings.nightLightColor, lightingSettings.nightLightIntensity));
        points.Add((24f, lightingSettings.nightAmbientColor, lightingSettings.nightAmbientIntensity, lightingSettings.nightLightColor, lightingSettings.nightLightIntensity));
        
        // Find interpolation segment
        for (int i = 0; i < points.Count - 1; i++)
        {
            var current = points[i];
            var next = points[i + 1];
            
            if (time >= current.time && time <= next.time)
            {
                float timeDiff = next.time - current.time;
                float t = timeDiff > 0 ? (time - current.time) / timeDiff : 0f;
                
                // Use SmoothStep for natural transitions
                t = Mathf.SmoothStep(0f, 1f, t);
                
                ambientColor = Color.Lerp(current.ambient, next.ambient, t);
                ambientIntensity = Mathf.Lerp(current.ambientInt, next.ambientInt, t);
                lightColor = Color.Lerp(current.light, next.light, t);
                lightIntensity = Mathf.Lerp(current.lightInt, next.lightInt, t);
                return;
            }
        }
        
        // Fallback
        ambientColor = lightingSettings.nightAmbientColor;
        ambientIntensity = lightingSettings.nightAmbientIntensity;
        lightColor = lightingSettings.nightLightColor;
        lightIntensity = lightingSettings.nightLightIntensity;
    }
    
    private void UpdateInspectorDisplay()
    {
        displayCurrentTime = currentTime;
        
        // Format time as HH:MM
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime % 1) * 60);
        displayTimeFormatted = $"{hours:D2}:{minutes:D2}";
        
        // Display current period
        displayCurrentPeriod = currentTimeOfDay.ToString();
        
        // Display time percentage (0-100%)
        displayTimePercentage = (currentTime / 24f) * 100f;
        
        // Calculate next period and time until it
        CalculateNextPeriodInfo();
    }
    
    private void CalculateNextPeriodInfo()
    {
        float timeUntilNext = 0f;
        string nextPeriod = "";
        
        // Use calculated transition times instead of hardcoded values
        if (currentTime >= 0f && currentTime < timeSettings.calculatedSunriseStart)
        {
            nextPeriod = "Sunrise";
            timeUntilNext = timeSettings.calculatedSunriseStart - currentTime;
        }
        else if (currentTime >= timeSettings.calculatedSunriseStart && currentTime < timeSettings.calculatedDayStart)
        {
            nextPeriod = "Day";
            timeUntilNext = timeSettings.calculatedDayStart - currentTime;
        }
        else if (currentTime >= timeSettings.calculatedDayStart && currentTime < timeSettings.calculatedSunsetStart)
        {
            nextPeriod = "Sunset";
            timeUntilNext = timeSettings.calculatedSunsetStart - currentTime;
        }
        else if (currentTime >= timeSettings.calculatedSunsetStart && currentTime < timeSettings.calculatedNightStart)
        {
            nextPeriod = "Night";
            timeUntilNext = timeSettings.calculatedNightStart - currentTime;
        }
        else // Night time
        {
            nextPeriod = "Sunrise";
            // Calculate time until next sunrise (may wrap around midnight)
            if (timeSettings.calculatedSunriseStart > currentTime)
            {
                timeUntilNext = timeSettings.calculatedSunriseStart - currentTime;
            }
            else
            {
                timeUntilNext = (24f - currentTime) + timeSettings.calculatedSunriseStart;
            }
        }
        
        displayNextPeriod = nextPeriod;
        
        // Format time until next period
        if (timeUntilNext < 1f)
        {
            int minutesUntil = Mathf.FloorToInt(timeUntilNext * 60);
            displayTimeUntilNext = $"{minutesUntil} min";
        }
        else
        {
            int hoursUntil = Mathf.FloorToInt(timeUntilNext);
            int minutesUntil = Mathf.FloorToInt((timeUntilNext % 1) * 60);
            displayTimeUntilNext = $"{hoursUntil}h {minutesUntil}m";
        }
    }
    
    // Public methods for external control
    public void SetTime(float hour)
    {
        currentTime = Mathf.Clamp(hour, 0f, 24f);
        UpdateTimeOfDay();
        ApplyGradualLighting();
    }
    
    public void SetTimeOfDay(TimeOfDay targetTimeOfDay)
    {
        // Use calculated transition times instead of hardcoded values
        switch (targetTimeOfDay)
        {
            case TimeOfDay.Day:
                SetTime(12f); // Always set to noon for day
                break;
            case TimeOfDay.Sunrise:
                SetTime((timeSettings.calculatedSunriseStart + timeSettings.calculatedSunriseEnd) * 0.5f); // Middle of sunrise
                break;
            case TimeOfDay.Sunset:
                SetTime((timeSettings.calculatedSunsetStart + timeSettings.calculatedSunsetEnd) * 0.5f); // Middle of sunset
                break;
            case TimeOfDay.Night:
                // Set to middle of night period
                float nightMiddle;
                if (timeSettings.calculatedNightStart < timeSettings.calculatedSunriseStart)
                {
                    // Normal night (doesn't wrap around midnight)
                    nightMiddle = (timeSettings.calculatedNightStart + timeSettings.calculatedSunriseStart) * 0.5f;
                }
                else
                {
                    // Night wraps around midnight
                    float nightDuration = (24f - timeSettings.calculatedNightStart) + timeSettings.calculatedSunriseStart;
                    nightMiddle = timeSettings.calculatedNightStart + (nightDuration * 0.5f);
                    if (nightMiddle >= 24f) nightMiddle -= 24f; // Wrap around
                }
                SetTime(nightMiddle);
                break;
        }
    }
    
    public void PauseTime()
    {
        if (timeProgressionCoroutine != null)
        {
            StopCoroutine(timeProgressionCoroutine);
            timeProgressionCoroutine = null;
        }
    }
    
    public void ResumeTime()
    {
        if (timeProgressionCoroutine == null && !GameSystemsManager.Instance.unDisturbedTime)
        {
            timeProgressionCoroutine = StartCoroutine(TimeProgressionCoroutine());
        }
    }
    
    public void SetCycleDuration(float seconds)
    {
        timeSettings.totalCycleDuration = Mathf.Clamp(seconds, 30f, 1800f);
        CalculateTransitionTimes();
    }
    
    // Add validation method to recalculate when settings change
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            CalculateTransitionTimes();
        }
    }
    
    // Optional: Save/Load system
    [System.Serializable]
    public class DayNightSaveData
    {
        public float currentTime;
        public TimeOfDay currentTimeOfDay;
    }
    
    public DayNightSaveData GetSaveData()
    {
        return new DayNightSaveData
        {
            currentTime = this.currentTime,
            currentTimeOfDay = this.currentTimeOfDay
        };
    }
    
    public void LoadSaveData(DayNightSaveData data)
    {
        currentTime = data.currentTime;
        currentTimeOfDay = data.currentTimeOfDay;
        ApplyGradualLighting();
    }
    
    /// <summary>
    /// Reset time to morning of day 1 (for new game)
    /// </summary>
    public void ResetToDay()
    {
        currentTime = 4f; 
        currentTimeOfDay = TimeOfDay.Day;
        ResumeTime();
        
        ApplyGradualLighting();
        
        Debug.Log("[DayNightCycle] Reset to morning of new day");
    }
}