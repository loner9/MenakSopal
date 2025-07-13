using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SpotLightSchedule
{
    [Header("Light Settings")]
    public Light2D spotLight;
    public string lightName = "Street Light";
    
    [Header("Activation Time")]
    [Range(0f, 24f)]
    public float turnOnHour = 18f; // 6 PM
    [Range(0f, 24f)]
    public float turnOffHour = 6f;  // 6 AM
    
    [Header("Light Properties")]
    public Color lightColor = Color.white;
    [Range(0f, 3f)]
    public float lightIntensity = 1f;
    [Range(0f, 10f)]
    public float innerRadius = 1f;
    [Range(0f, 20f)]
    public float outerRadius = 5f;
    [Range(0f, 360f)]
    public float spotAngle = 90f;
    
    [Header("Transition Settings")]
    public bool useSmoothTransition = true;
    [Range(0.1f, 2f)]
    public float transitionDuration = 0.5f; // In game hours
    
    [Header("Advanced Options")]
    public bool flickerEffect = false;
    [Range(0f, 1f)]
    public float flickerChance = 0.1f;
    [Range(0.1f, 1f)]
    public float flickerDuration = 0.2f;
    
    [Header("Debug Info (Read Only)")]
    [SerializeField, ReadOnly] public bool isCurrentlyOn;
    [SerializeField, ReadOnly] public float currentIntensity;
    [SerializeField, ReadOnly] public string nextAction;
    [SerializeField, ReadOnly] public float timeUntilNextAction;
    
    // Internal variables
    [System.NonSerialized] public float targetIntensity;
    [System.NonSerialized] public float originalIntensity;
    [System.NonSerialized] public bool isTransitioning;
    [System.NonSerialized] public float transitionStartTime;
    [System.NonSerialized] public float transitionTargetTime;
    [System.NonSerialized] public float flickerTimer;
    [System.NonSerialized] public bool isFlickering;
}

public class SpotLight2DManager : MonoBehaviour
{
    [Header("System References")]
    public DayNightCycle dayNightCycle;
    
    [Header("Light Schedules")]
    public List<SpotLightSchedule> lightSchedules = new List<SpotLightSchedule>();
    
    [Header("Auto Gather Settings")]
    public bool autoGatherOnStart = true;
    public string lightTag = "Lights";
    [SerializeField] private bool useDefaultScheduleForGathered = true;
    
    [Header("Default Schedule for Auto-Gathered Lights")]
    [Range(0f, 24f)]
    public float defaultTurnOnHour = 18f;
    [Range(0f, 24f)]
    public float defaultTurnOffHour = 6f;
    public bool defaultUseSmoothTransition = true;
    [Range(0.1f, 2f)]
    public float defaultTransitionDuration = 0.5f;
    public bool defaultFlickerEffect = false;
    
    [Header("Global Settings")]
    public bool enableAllLights = true;
    [Range(0f, 2f)]
    public float globalIntensityMultiplier = 1f;
    
    [Header("Performance Settings")]
    [Range(0.1f, 1f)]
    public float updateInterval = 0.2f; // How often to update lights (in real seconds)
    public bool enableFlickerEffects = true;
    
    [Header("Editor Tools")]
    [Space(10)]
    [SerializeField] private bool showEditorButtons = true;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private float lastUpdateTime;
    private bool systemInitialized = false;
    
    void Start()
    {
        InitializeSystem();
    }
    
    void Update()
    {
        if (!systemInitialized || dayNightCycle == null) return;
        
        // Update at specified interval for performance
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateAllLights();
            lastUpdateTime = Time.time;
        }
        
        // Handle flicker effects (updated every frame for smoothness)
        if (enableFlickerEffects)
        {
            UpdateFlickerEffects();
        }
    }
    
    void InitializeSystem()
    {
        // Find DayNightCycle if not assigned
        if (dayNightCycle == null)
        {
            dayNightCycle = FindFirstObjectByType<DayNightCycle>();
            if (dayNightCycle == null)
            {
                Debug.LogError("SpotLight2DManager: No DayNightCycle found in scene!");
                return;
            }
        }
        
        // Auto-gather lights with tag if enabled
        if (autoGatherOnStart)
        {
            AutoGatherLightsWithTag();
        }
        
        // Initialize each light schedule
        foreach (var schedule in lightSchedules)
        {
            InitializeLightSchedule(schedule);
        }
        
        // Subscribe to day/night cycle events if available
        if (dayNightCycle.OnTimeOfDayChanged != null)
        {
            dayNightCycle.OnTimeOfDayChanged += OnTimeOfDayChanged;
        }
        
        systemInitialized = true;
        
        // Initial update
        UpdateAllLights();
        
        Debug.Log($"SpotLight2DManager initialized with {lightSchedules.Count} light schedules");
    }
    
    void InitializeLightSchedule(SpotLightSchedule schedule)
    {
        if (schedule.spotLight == null)
        {
            Debug.LogWarning($"SpotLight2DManager: Light '{schedule.lightName}' has no Light2D assigned!");
            return;
        }
        
        // Store original intensity
        schedule.originalIntensity = schedule.lightIntensity;
        
        // Apply initial light properties
        schedule.spotLight.color = schedule.lightColor;
        schedule.spotLight.pointLightInnerRadius = schedule.innerRadius;
        schedule.spotLight.pointLightOuterRadius = schedule.outerRadius;
        
        // Set light specific properties based on type
        if (schedule.spotLight.lightType == Light2D.LightType.Point)
        {
            schedule.spotLight.pointLightInnerAngle = schedule.spotAngle * 0.8f; // Inner angle slightly smaller
            schedule.spotLight.pointLightOuterAngle = schedule.spotAngle;
        }
        else if (schedule.spotLight.lightType == Light2D.LightType.Freeform)
        {
            // Freeform lights use shape, not angle properties
            // The shape is defined by the sprite
        }
        else if (schedule.spotLight.lightType == Light2D.LightType.Parametric)
        {
            // Parametric lights can have custom shapes
            schedule.spotLight.pointLightInnerAngle = schedule.spotAngle * 0.8f;
            schedule.spotLight.pointLightOuterAngle = schedule.spotAngle;
        }
        
        // Initialize state
        schedule.isCurrentlyOn = false;
        schedule.targetIntensity = 0f;
        schedule.currentIntensity = 0f;
        schedule.isTransitioning = false;
        schedule.flickerTimer = 0f;
        schedule.isFlickering = false;
    }
    
    void UpdateAllLights()
    {
        if (!enableAllLights) return;
        
        float currentTime = dayNightCycle.CurrentTime;
        
        foreach (var schedule in lightSchedules)
        {
            UpdateLightSchedule(schedule, currentTime);
        }
    }
    
    void UpdateLightSchedule(SpotLightSchedule schedule, float currentTime)
    {
        if (schedule.spotLight == null) return;
        
        // Determine if light should be on
        bool shouldBeOn = ShouldLightBeOn(schedule, currentTime);
        
        // Handle state changes
        if (shouldBeOn != schedule.isCurrentlyOn)
        {
            if (schedule.useSmoothTransition)
            {
                StartTransition(schedule, shouldBeOn, currentTime);
            }
            else
            {
                SetLightState(schedule, shouldBeOn);
            }
            
            schedule.isCurrentlyOn = shouldBeOn;
        }
        
        // Update transitions
        if (schedule.isTransitioning)
        {
            UpdateTransition(schedule, currentTime);
        }
        
        // Apply current intensity
        ApplyLightIntensity(schedule);
        
        // Update debug info
        UpdateDebugInfo(schedule, currentTime);
    }
    
    bool ShouldLightBeOn(SpotLightSchedule schedule, float currentTime)
    {
        float turnOn = schedule.turnOnHour;
        float turnOff = schedule.turnOffHour;
        
        // Handle case where light period spans midnight
        if (turnOn > turnOff)
        {
            // Light is on from turnOn to 24:00 and from 0:00 to turnOff
            return currentTime >= turnOn || currentTime < turnOff;
        }
        else
        {
            // Normal case: light is on between turnOn and turnOff
            return currentTime >= turnOn && currentTime < turnOff;
        }
    }
    
    void StartTransition(SpotLightSchedule schedule, bool turningOn, float currentTime)
    {
        schedule.isTransitioning = true;
        schedule.transitionStartTime = currentTime;
        schedule.transitionTargetTime = currentTime + schedule.transitionDuration;
        
        if (turningOn)
        {
            schedule.targetIntensity = schedule.originalIntensity * globalIntensityMultiplier;
        }
        else
        {
            schedule.targetIntensity = 0f;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Light '{schedule.lightName}' starting transition to {(turningOn ? "ON" : "OFF")}");
        }
    }
    
    void UpdateTransition(SpotLightSchedule schedule, float currentTime)
    {
        float transitionProgress = (currentTime - schedule.transitionStartTime) / schedule.transitionDuration;
        transitionProgress = Mathf.Clamp01(transitionProgress);
        
        // Use smooth step for natural transition
        float smoothProgress = Mathf.SmoothStep(0f, 1f, transitionProgress);
        
        float startIntensity = schedule.currentIntensity;
        schedule.currentIntensity = Mathf.Lerp(startIntensity, schedule.targetIntensity, smoothProgress);
        
        // Check if transition is complete
        if (transitionProgress >= 1f)
        {
            schedule.isTransitioning = false;
            schedule.currentIntensity = schedule.targetIntensity;
            
            if (showDebugInfo)
            {
                Debug.Log($"Light '{schedule.lightName}' transition completed");
            }
        }
    }
    
    void SetLightState(SpotLightSchedule schedule, bool isOn)
    {
        if (isOn)
        {
            schedule.currentIntensity = schedule.originalIntensity * globalIntensityMultiplier;
            schedule.targetIntensity = schedule.currentIntensity;
        }
        else
        {
            schedule.currentIntensity = 0f;
            schedule.targetIntensity = 0f;
        }
        
        schedule.isTransitioning = false;
    }
    
    void UpdateFlickerEffects()
    {
        foreach (var schedule in lightSchedules)
        {
            if (!schedule.flickerEffect || !schedule.isCurrentlyOn) continue;
            
            schedule.flickerTimer -= Time.deltaTime;
            
            if (!schedule.isFlickering && schedule.flickerTimer <= 0f)
            {
                // Check if we should start flickering
                if (Random.Range(0f, 1f) < schedule.flickerChance)
                {
                    schedule.isFlickering = true;
                    schedule.flickerTimer = schedule.flickerDuration;
                }
                else
                {
                    schedule.flickerTimer = Random.Range(0.5f, 3f); // Next flicker check
                }
            }
            
            if (schedule.isFlickering && schedule.flickerTimer <= 0f)
            {
                schedule.isFlickering = false;
                schedule.flickerTimer = Random.Range(1f, 5f); // Time until next possible flicker
            }
        }
    }
    
    void ApplyLightIntensity(SpotLightSchedule schedule)
    {
        if (schedule.spotLight == null) return;
        
        float finalIntensity = schedule.currentIntensity;
        
        // Apply flicker effect
        if (schedule.isFlickering && schedule.flickerEffect)
        {
            float flickerMultiplier = Random.Range(0.3f, 1f);
            finalIntensity *= flickerMultiplier;
        }
        
        // Apply global multiplier
        finalIntensity *= globalIntensityMultiplier;
        
        schedule.spotLight.intensity = finalIntensity;
    }
    
    void UpdateDebugInfo(SpotLightSchedule schedule, float currentTime)
    {
        schedule.currentIntensity = schedule.spotLight.intensity;
        
        // Calculate next action and time
        bool shouldBeOn = ShouldLightBeOn(schedule, currentTime);
        
        if (shouldBeOn)
        {
            schedule.nextAction = "Turn OFF";
            // Calculate time until turn off
            if (schedule.turnOffHour > currentTime)
            {
                schedule.timeUntilNextAction = schedule.turnOffHour - currentTime;
            }
            else
            {
                schedule.timeUntilNextAction = (24f - currentTime) + schedule.turnOffHour;
            }
        }
        else
        {
            schedule.nextAction = "Turn ON";
            // Calculate time until turn on
            if (schedule.turnOnHour > currentTime)
            {
                schedule.timeUntilNextAction = schedule.turnOnHour - currentTime;
            }
            else
            {
                schedule.timeUntilNextAction = (24f - currentTime) + schedule.turnOnHour;
            }
        }
    }
    
    // Event handler for day/night cycle changes
    void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        if (showDebugInfo)
        {
            Debug.Log($"SpotLight2DManager: Time of day changed to {newTimeOfDay}");
        }
        
        // Force immediate update when time of day changes
        UpdateAllLights();
    }
    
    // Public methods for external control
    public void SetGlobalIntensity(float multiplier)
    {
        globalIntensityMultiplier = Mathf.Clamp(multiplier, 0f, 2f);
    }
    
    public void ToggleAllLights()
    {
        enableAllLights = !enableAllLights;
        
        if (!enableAllLights)
        {
            // Turn off all lights immediately
            foreach (var schedule in lightSchedules)
            {
                if (schedule.spotLight != null)
                {
                    schedule.spotLight.intensity = 0f;
                }
            }
        }
    }
    
    public void ForceUpdateLights()
    {
        UpdateAllLights();
    }
    
    public void AutoGatherLightsWithTag()
    {
        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag(lightTag);
        int newLightsAdded = 0;
        
        foreach (GameObject lightObj in lightObjects)
        {
            Light2D light2D = lightObj.GetComponent<Light2D>();
            if (light2D == null)
            {
                // Try to find Light2D in children
                light2D = lightObj.GetComponentInChildren<Light2D>();
            }
            
            if (light2D != null)
            {
                // Check if this light is already in our schedules
                bool alreadyExists = lightSchedules.Any(schedule => schedule.spotLight == light2D);
                
                if (!alreadyExists)
                {
                    AddLightScheduleFromAutoGather(light2D, lightObj.name);
                    newLightsAdded++;
                }
            }
            else
            {
                Debug.LogWarning($"GameObject '{lightObj.name}' has tag '{lightTag}' but no Light2D component found!");
            }
        }
        
        Debug.Log($"Auto-gathered {newLightsAdded} new lights with tag '{lightTag}'. Total lights: {lightSchedules.Count}");
    }
    
    private void AddLightScheduleFromAutoGather(Light2D light2D, string objectName)
    {
        var newSchedule = new SpotLightSchedule
        {
            spotLight = light2D,
            lightName = objectName,
            turnOnHour = defaultTurnOnHour,
            turnOffHour = defaultTurnOffHour,
            lightColor = light2D.color,
            lightIntensity = light2D.intensity,
            innerRadius = light2D.pointLightInnerRadius,
            outerRadius = light2D.pointLightOuterRadius,
            spotAngle = light2D.pointLightOuterAngle,
            useSmoothTransition = defaultUseSmoothTransition,
            transitionDuration = defaultTransitionDuration,
            flickerEffect = defaultFlickerEffect
        };
        
        lightSchedules.Add(newSchedule);
        
        if (systemInitialized)
        {
            InitializeLightSchedule(newSchedule);
        }
    }
    
    public void ClearAllLightSchedules()
    {
        lightSchedules.Clear();
        Debug.Log("All light schedules cleared");
    }
    
    public void RefreshAutoGatheredLights()
    {
        // Store manually configured lights (those not using default settings)
        var manualLights = lightSchedules.Where(schedule => 
            !IsUsingDefaultSettings(schedule)).ToList();
        
        // Clear all schedules
        lightSchedules.Clear();
        
        // Re-add manual lights
        lightSchedules.AddRange(manualLights);
        
        // Auto-gather again
        AutoGatherLightsWithTag();
        
        Debug.Log($"Refreshed auto-gathered lights. Manual lights preserved: {manualLights.Count}");
    }
    
    private bool IsUsingDefaultSettings(SpotLightSchedule schedule)
    {
        return Mathf.Approximately(schedule.turnOnHour, defaultTurnOnHour) &&
               Mathf.Approximately(schedule.turnOffHour, defaultTurnOffHour) &&
               schedule.useSmoothTransition == defaultUseSmoothTransition &&
               Mathf.Approximately(schedule.transitionDuration, defaultTransitionDuration) &&
               schedule.flickerEffect == defaultFlickerEffect;
    }
    
    public int GetLightCount()
    {
        return lightSchedules.Count;
    }
    
    public int GetActiveLightCount()
    {
        return lightSchedules.Count(schedule => schedule.isCurrentlyOn);
    }
    
    public void AddLightSchedule(Light2D light, float turnOnHour, float turnOffHour, string name = "")
    {
        var newSchedule = new SpotLightSchedule
        {
            spotLight = light,
            lightName = string.IsNullOrEmpty(name) ? $"Light_{lightSchedules.Count}" : name,
            turnOnHour = turnOnHour,
            turnOffHour = turnOffHour,
            lightColor = light.color,
            lightIntensity = light.intensity
        };
        
        lightSchedules.Add(newSchedule);
        InitializeLightSchedule(newSchedule);
    }
    
    public void RemoveLightSchedule(Light2D light)
    {
        for (int i = lightSchedules.Count - 1; i >= 0; i--)
        {
            if (lightSchedules[i].spotLight == light)
            {
                lightSchedules.RemoveAt(i);
                break;
            }
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (dayNightCycle != null && dayNightCycle.OnTimeOfDayChanged != null)
        {
            dayNightCycle.OnTimeOfDayChanged -= OnTimeOfDayChanged;
        }
    }
    
    // Validation in editor
    void OnValidate()
    {
        // Clamp values to valid ranges
        globalIntensityMultiplier = Mathf.Clamp(globalIntensityMultiplier, 0f, 2f);
        updateInterval = Mathf.Clamp(updateInterval, 0.1f, 1f);
        
        // Validate each schedule
        foreach (var schedule in lightSchedules)
        {
            schedule.turnOnHour = Mathf.Clamp(schedule.turnOnHour, 0f, 24f);
            schedule.turnOffHour = Mathf.Clamp(schedule.turnOffHour, 0f, 24f);
            schedule.lightIntensity = Mathf.Clamp(schedule.lightIntensity, 0f, 3f);
            schedule.transitionDuration = Mathf.Clamp(schedule.transitionDuration, 0.1f, 2f);
            schedule.flickerChance = Mathf.Clamp01(schedule.flickerChance);
            schedule.flickerDuration = Mathf.Clamp(schedule.flickerDuration, 0.1f, 1f);
        }
    }
    
    // Gizmos for debugging
    void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;
        
        foreach (var schedule in lightSchedules)
        {
            if (schedule.spotLight == null) continue;
            
            // Draw light radius
            Gizmos.color = schedule.isCurrentlyOn ? Color.yellow : Color.gray;
            Gizmos.DrawWireSphere(schedule.spotLight.transform.position, schedule.outerRadius);
            
            // Draw inner radius
            Gizmos.color = schedule.isCurrentlyOn ? Color.white : Color.gray;
            Gizmos.DrawWireSphere(schedule.spotLight.transform.position, schedule.innerRadius);
        }
    }
}