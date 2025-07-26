# Flag-Driven Cutscenes & Custom Scenes Guide

## 🎬 **Overview**

This guide demonstrates how to extend the flag system to create dynamic cutscenes and custom scenes that respond to player choices and story progression. Transform your dialogue system into a complete narrative engine!

---

## 🎭 **Core Concept**

```
Player Dialogue Choices → Flags Added → Triggers Cutscenes/Scenes → Story Progression
```

The flag system becomes the **backbone of your entire narrative experience** - from simple dialogue choices to major story events and cutscenes.

---

## 🚀 **Benefits of Flag-Driven Scenes**

### **1. Dynamic Storytelling**
- Story adapts to player choices automatically
- Multiple possible outcomes based on player actions
- High replay value through different narrative paths

### **2. Seamless Integration**
- Uses existing flag system architecture
- No complex additional systems needed
- Works perfectly with current dialogue system

### **3. Easy Content Creation**
- Designers can create cutscene triggers in Unity Inspector
- No programming needed for new story events
- Visual setup through Unity editor

### **4. Flexible Implementation**
- Can trigger complete scene changes
- Can spawn/despawn NPCs dynamically
- Can modify environment and atmosphere
- Can start Unity Timeline cutscenes
- Can change music, lighting, and ambiance

---

## 🎬 **Implementation Approaches**

### **Approach 1: Scene-Based Cutscenes**

Complete scene transitions for major story moments.

```cs
public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneTrigger
    {
        public string cutsceneName;
        public string[] requiredFlags;
        public string[] conflictingFlags;  // Flags that prevent this cutscene
        public string sceneToLoad;
        public bool hasBeenTriggered = false;
    }
    
    public CutsceneTrigger[] cutsceneTriggers;
    private NPCInteractionSystem flagSystem;
    
    void Start()
    {
        flagSystem = FindObjectOfType<NPCInteractionSystem>();
    }
    
    void Update()
    {
        CheckForCutsceneTriggers();
    }
    
    void CheckForCutsceneTriggers()
    {
        foreach (var trigger in cutsceneTriggers)
        {
            if (!trigger.hasBeenTriggered && ShouldTriggerCutscene(trigger))
            {
                TriggerCutscene(trigger);
                trigger.hasBeenTriggered = true;
            }
        }
    }
    
    bool ShouldTriggerCutscene(CutsceneTrigger trigger)
    {
        // Check required flags exist
        foreach (string flag in trigger.requiredFlags)
        {
            if (!flagSystem.HasGameFlag(flag))
                return false;
        }
        
        // Check conflicting flags don't exist
        foreach (string flag in trigger.conflictingFlags)
        {
            if (flagSystem.HasGameFlag(flag))
                return false;
        }
        
        return true;
    }
    
    void TriggerCutscene(CutsceneTrigger trigger)
    {
        Debug.Log($"Triggering cutscene: {trigger.cutsceneName}");
        
        // Add transition flag before loading scene
        flagSystem.AddGameFlag($"CUTSCENE_{trigger.cutsceneName.ToUpper()}_PLAYED");
        
        // Load cutscene scene
        SceneManager.LoadScene(trigger.sceneToLoad);
    }
}
```

### **Approach 2: In-Scene Event Triggers**

Dynamic events within the current scene.

```cs
public class StoryEventTrigger : MonoBehaviour
{
    [Header("Trigger Conditions")]
    public string[] requiredFlags;
    public string[] flagsToAddAfterEvent;
    
    [Header("Event Configuration")]
    public GameObject[] npcsToSpawn;
    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;
    public Transform[] cameraPositions;
    public AudioClip eventMusic;
    
    [Header("Dialogue Integration")]
    public DialogueData eventDialogue;
    public NPC primaryNPC;
    
    [Header("Timeline Integration")]
    public PlayableDirector timelineDirector;
    
    private NPCInteractionSystem flagSystem;
    private bool hasTriggered = false;
    
    void Start()
    {
        flagSystem = FindObjectOfType<NPCInteractionSystem>();
        CheckTriggerConditions();
    }
    
    void CheckTriggerConditions()
    {
        if (hasTriggered) return;
        
        // Check if all required flags exist
        foreach (string flag in requiredFlags)
        {
            if (!flagSystem.HasGameFlag(flag))
                return; // Not ready yet
        }
        
        // All conditions met - trigger event!
        TriggerStoryEvent();
    }
    
    void TriggerStoryEvent()
    {
        hasTriggered = true;
        
        Debug.Log("Story event triggered!");
        
        // Spawn specific NPCs for this event
        foreach (var npc in npcsToSpawn)
        {
            npc.SetActive(true);
        }
        
        // Configure scene objects
        foreach (var obj in objectsToActivate)
            obj.SetActive(true);
        foreach (var obj in objectsToDeactivate)
            obj.SetActive(false);
        
        // Play Timeline if assigned
        if (timelineDirector != null)
        {
            timelineDirector.Play();
        }
        
        // Start event dialogue if assigned
        if (eventDialogue != null && primaryNPC != null)
        {
            primaryNPC.dialogueData = eventDialogue;
            // Auto-start dialogue or wait for player interaction
        }
        
        // Change music if specified
        if (eventMusic != null)
        {
            var audioSource = Camera.main.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.clip = eventMusic;
                audioSource.Play();
            }
        }
        
        // Add consequence flags
        foreach (string flag in flagsToAddAfterEvent)
        {
            flagSystem.AddGameFlag(flag);
        }
    }
}
```

---

## 🎯 **Real-World Usage Examples**

### **Example 1: Romance Cutscene**

**Player Dialogue Journey:**
```
Multiple Conversations with Sarah:
├── Helped Sarah with quest → "HELPED_SARAH"
├── Was kind in conversations → "KIND_TO_SARAH" 
├── Gave Sarah a gift → "GAVE_SARAH_GIFT"
├── Talked to Sarah 5+ times → "SARAH_FRIENDSHIP_HIGH"
└── Made romantic dialogue choices → "SARAH_ROMANCE_INTEREST"

Cutscene Trigger Configuration:
├── Required Flags: ["HELPED_SARAH", "KIND_TO_SARAH", "GAVE_SARAH_GIFT", "SARAH_ROMANCE_INTEREST"]
├── Scene To Load: "RomanceCutscene"
├── Adds Flag After: "SARAH_ROMANCE_STARTED"
└── Conflicting Flags: ["SARAH_REJECTED_PLAYER", "DATING_SOMEONE_ELSE"]
```

**Result:** Romantic cutscene only triggers if player consistently made caring choices.

### **Example 2: Betrayal Revelation Scene**

**Player Choices Leading Up:**
```
Trusting Marcus Path:
├── Trusted Marcus in dialogue → "TRUSTS_MARCUS"
├── Gave Marcus secret information → "TOLD_MARCUS_SECRET"
├── Completed Marcus's suspicious tasks → "HELPED_MARCUS_SCHEMES"

BUT Player Also Discovers:
└── Found incriminating evidence → "FOUND_BETRAYAL_EVIDENCE"

Event Trigger:
├── Required Flags: ["TRUSTS_MARCUS", "TOLD_MARCUS_SECRET", "FOUND_BETRAYAL_EVIDENCE"]
├── Conflicting Flags: ["ALREADY_CONFRONTED_MARCUS", "MARCUS_IS_DEAD"]
├── Result: Dramatic betrayal revelation scene!
├── NPCs to Spawn: [Angry Marcus, Guards as witnesses]
├── Objects to Activate: [Evidence props, dramatic lighting]
└── Flags Added After: ["BETRAYED_BY_MARCUS", "STORY_CHAPTER_3_START"]
```

**Result:** Highly emotional scene because player trusted Marcus, making betrayal impactful.

### **Example 3: Town Festival Event**

**Community Building Choices:**
```
Helping the Community:
├── Helped the Blacksmith → "HELPED_BLACKSMITH"
├── Helped the Baker → "HELPED_BAKER" 
├── Helped the Farmer → "HELPED_FARMER"
├── Resolved town disputes → "PEACEMAKER"
└── General reputation → "TOWN_LIKES_PLAYER"

Festival Event Trigger:
├── Required Flags: All 5 flags above
├── Scene Changes:
│   ├── Spawn festival decorations
│   ├── Spawn celebration NPCs with unique dialogues
│   ├── Activate festival music and lighting
│   ├── Enable special festival activities
│   └── Festival-specific merchant stalls
├── Special Cutscene: "Town Celebration"
└── Adds Flag: "TOWN_FESTIVAL_HERO"
```

**Result:** Entire town transforms to celebrate the player's contributions.

---

## 🏗️ **Advanced Implementation Components**

### **1. Timeline Integration**

For cinematic cutscenes using Unity Timeline.

```cs
public class FlagTriggeredTimeline : MonoBehaviour
{
    [Header("Timeline Configuration")]
    public PlayableDirector timeline;
    public string[] requiredFlags;
    public string[] flagsToAddAfterPlayback;
    
    [Header("Scene Setup")]
    public GameObject[] objectsToActivateForTimeline;
    public GameObject[] objectsToDeactivateForTimeline;
    
    private NPCInteractionSystem flagSystem;
    private bool hasPlayed = false;
    
    void Start()
    {
        flagSystem = FindObjectOfType<NPCInteractionSystem>();
    }
    
    void Update()
    {
        if (!hasPlayed && AllRequiredFlagsPresent())
        {
            PlayTimeline();
            hasPlayed = true;
        }
    }
    
    bool AllRequiredFlagsPresent()
    {
        foreach (string flag in requiredFlags)
        {
            if (!flagSystem.HasGameFlag(flag))
                return false;
        }
        return true;
    }
    
    void PlayTimeline()
    {
        // Setup scene for timeline
        foreach (var obj in objectsToActivateForTimeline)
            obj.SetActive(true);
        foreach (var obj in objectsToDeactivateForTimeline)
            obj.SetActive(false);
        
        // Play timeline
        timeline.Play();
        
        // Listen for timeline completion
        timeline.stopped += OnTimelineFinished;
    }
    
    void OnTimelineFinished(PlayableDirector director)
    {
        // Add flags after timeline completion
        foreach (string flag in flagsToAddAfterPlayback)
        {
            flagSystem.AddGameFlag(flag);
        }
        
        timeline.stopped -= OnTimelineFinished;
        Debug.Log("Timeline finished and flags added");
    }
}
```

### **2. Dynamic NPC Spawning System**

Spawn different NPCs based on story state.

```cs
public class ConditionalNPCSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ConditionalNPC
    {
        [Header("NPC Configuration")]
        public string npcName;
        public GameObject npcPrefab;
        public Transform spawnPoint;
        public DialogueData customDialogue;
        
        [Header("Spawn Conditions")]
        public string[] requiredFlags;
        public string[] excludingFlags;
        public TimeOfDay[] allowedTimes;
        
        [Header("State")]
        public bool hasSpawned = false;
    }
    
    public ConditionalNPC[] conditionalNPCs;
    private NPCInteractionSystem flagSystem;
    private DayNightCycle dayNightCycle;
    
    void Start()
    {
        flagSystem = FindObjectOfType<NPCInteractionSystem>();
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        
        SpawnConditionalNPCs();
    }
    
    void SpawnConditionalNPCs()
    {
        TimeOfDay currentTime = dayNightCycle != null ? dayNightCycle.CurrentTimeOfDay : TimeOfDay.Day;
        
        foreach (var conditionalNPC in conditionalNPCs)
        {
            if (conditionalNPC.hasSpawned) continue;
            
            if (ShouldSpawnNPC(conditionalNPC, currentTime))
            {
                SpawnNPC(conditionalNPC);
            }
        }
    }
    
    bool ShouldSpawnNPC(ConditionalNPC npcConfig, TimeOfDay currentTime)
    {
        // Check time requirements
        if (npcConfig.allowedTimes != null && npcConfig.allowedTimes.Length > 0)
        {
            bool timeMatches = false;
            foreach (var time in npcConfig.allowedTimes)
            {
                if (currentTime == time)
                {
                    timeMatches = true;
                    break;
                }
            }
            if (!timeMatches) return false;
        }
        
        // Check required flags
        foreach (string flag in npcConfig.requiredFlags)
        {
            if (!flagSystem.HasGameFlag(flag))
                return false;
        }
        
        // Check excluding flags
        foreach (string flag in npcConfig.excludingFlags)
        {
            if (flagSystem.HasGameFlag(flag))
                return false;
        }
        
        return true;
    }
    
    void SpawnNPC(ConditionalNPC npcConfig)
    {
        GameObject spawnedNPC = Instantiate(npcConfig.npcPrefab, 
            npcConfig.spawnPoint.position, npcConfig.spawnPoint.rotation);
        
        // Configure NPC with custom dialogue if provided
        NPC npcComponent = spawnedNPC.GetComponent<NPC>();
        if (npcComponent != null && npcConfig.customDialogue != null)
        {
            npcComponent.dialogueData = npcConfig.customDialogue;
        }
        
        npcConfig.hasSpawned = true;
        Debug.Log($"Spawned conditional NPC: {npcConfig.npcName}");
    }
}
```

### **3. Scene State Manager**

Transform entire scenes based on story progression.

```cs
public class SceneStateManager : MonoBehaviour
{
    [System.Serializable] 
    public class SceneState
    {
        [Header("State Configuration")]
        public string stateName;
        public string[] requiredFlags;
        public int priority = 0; // Higher priority states override lower ones
        
        [Header("Scene Modifications")]
        public GameObject[] objectsToEnable;
        public GameObject[] objectsToDisable;
        public Material skyboxMaterial;
        public AudioClip backgroundMusic;
        public Color ambientLightColor = Color.white;
        public float fogDensity = 0.01f;
        public Color fogColor = Color.gray;
        
        [Header("Lighting")]
        public bool overrideLighting = false;
        public float lightIntensity = 1f;
        public Color lightColor = Color.white;
    }
    
    public SceneState[] possibleStates;
    private SceneState currentState;
    private NPCInteractionSystem flagSystem;
    
    void Start()
    {
        flagSystem = FindObjectOfType<NPCInteractionSystem>();
        DetermineAndApplySceneState();
    }
    
    void DetermineAndApplySceneState()
    {
        SceneState bestState = null;
        int bestPriority = -1;
        
        // Find the highest priority state that meets requirements
        foreach (var state in possibleStates)
        {
            if (state.priority <= bestPriority) continue;
            
            bool allFlagsPresent = true;
            foreach (string flag in state.requiredFlags)
            {
                if (!flagSystem.HasGameFlag(flag))
                {
                    allFlagsPresent = false;
                    break;
                }
            }
            
            if (allFlagsPresent)
            {
                bestState = state;
                bestPriority = state.priority;
            }
        }
        
        if (bestState != null)
        {
            ApplySceneState(bestState);
            currentState = bestState;
        }
    }
    
    void ApplySceneState(SceneState state)
    {
        Debug.Log($"Applying scene state: {state.stateName}");
        
        // Enable/disable objects
        foreach (var obj in state.objectsToEnable)
            if (obj != null) obj.SetActive(true);
        foreach (var obj in state.objectsToDisable)
            if (obj != null) obj.SetActive(false);
        
        // Apply environmental settings
        if (state.skyboxMaterial != null)
            RenderSettings.skybox = state.skyboxMaterial;
        
        RenderSettings.ambientLight = state.ambientLightColor;
        RenderSettings.fogDensity = state.fogDensity;
        RenderSettings.fogColor = state.fogColor;
        
        // Apply lighting changes
        if (state.overrideLighting)
        {
            Light mainLight = GameObject.FindObjectOfType<Light>();
            if (mainLight != null)
            {
                mainLight.intensity = state.lightIntensity;
                mainLight.color = state.lightColor;
            }
        }
        
        // Change background music
        AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
        if (audioSource != null && state.backgroundMusic != null)
        {
            audioSource.clip = state.backgroundMusic;
            audioSource.Play();
        }
    }
    
    // Call this when flags change to check for state updates
    public void RefreshSceneState()
    {
        DetermineAndApplySceneState();
    }
}
```

---

## 🎬 **Complete Setup Example**

### **Unity Inspector Configuration:**

#### **CutsceneManager Setup:**
```
CutsceneManager Component:
├── Cutscene Triggers (Array Size: 3):
    ├── [0] Romance Scene:
    │   ├── Cutscene Name: "Sarah Romance"
    │   ├── Required Flags: ["HELPED_SARAH", "KIND_TO_SARAH", "GAVE_GIFT", "SARAH_LIKES_PLAYER"]
    │   ├── Conflicting Flags: ["SARAH_REJECTED", "DATING_OTHERS"]
    │   ├── Scene To Load: "RomanceCutscene"
    │   └── Has Been Triggered: ✗
    ├── [1] Betrayal Scene:
    │   ├── Cutscene Name: "Marcus Betrayal"
    │   ├── Required Flags: ["TRUSTS_MARCUS", "FOUND_EVIDENCE", "CHAPTER_2_COMPLETE"]
    │   ├── Conflicting Flags: ["MARCUS_DEAD", "ALREADY_CONFRONTED"]
    │   ├── Scene To Load: "BetrayalCutscene"
    │   └── Has Been Triggered: ✗
    └── [2] Festival Scene:
        ├── Cutscene Name: "Town Festival"
        ├── Required Flags: ["HELPED_BLACKSMITH", "HELPED_BAKER", "HELPED_FARMER", "TOWN_HERO"]
        ├── Conflicting Flags: ["TOWN_ENEMY", "EXILED_FROM_TOWN"]
        ├── Scene To Load: "FestivalScene"
        └── Has Been Triggered: ✗
```

#### **StoryEventTrigger Setup:**
```
StoryEventTrigger Component:
├── Trigger Conditions:
│   ├── Required Flags: ["QUEST_COMPLETED", "KNOWS_SECRET", "TRUSTED_BY_KING"]
│   └── Flags To Add After Event: ["ROYAL_AUDIENCE_GRANTED", "STORY_ACT_2"]
├── Event Configuration:
│   ├── NPCs To Spawn: [Royal Guard Prefab, Court Wizard Prefab]
│   ├── Objects To Activate: [Throne Room Lighting, Royal Banners]
│   ├── Objects To Deactivate: [Regular Hall Setup, Commoner NPCs]
│   └── Event Music: RoyalAudienceTheme.wav
└── Dialogue Integration:
    ├── Event Dialogue: RoyalAudienceDialogue.asset
    └── Primary NPC: King GameObject
```

---

## 🚀 **Implementation Workflow**

### **Step 1: Plan Your Narrative Branches**
```
Map out story paths:
├── What player choices lead to which scenes?
├── What flags represent major story beats?
├── Which scenes are mutually exclusive?
└── How do scenes connect to overall narrative?
```

### **Step 2: Create Flag Requirements**
```
Define trigger conditions:
├── Required flags for each scene
├── Conflicting flags that prevent scenes
├── Priority system for overlapping conditions
└── Consequence flags added after scenes
```

### **Step 3: Build Scene Assets**
```
Create the actual content:
├── Cutscene scenes or Timeline assets
├── Conditional NPC prefabs with unique dialogues
├── Environmental props and lighting setups
└── Audio assets (music, sound effects)
```

### **Step 4: Configure Trigger Systems**
```
Set up managers in Unity:
├── Add CutsceneManager to main scene
├── Configure StoryEventTrigger components
├── Set up ConditionalNPCSpawner systems
└── Configure SceneStateManager for environmental changes
```

### **Step 5: Test and Iterate**
```
Validate the system:
├── Test different flag combinations
├── Ensure scenes trigger at appropriate times
├── Verify no conflicts between triggers
└── Test save/load compatibility
```

---

## 🎯 **Best Practices**

### **Flag Management**
- Use descriptive flag names that indicate story beats
- Group related flags with consistent prefixes (e.g., "SARAH_", "QUEST_MAIN_")
- Document flag dependencies for complex narrative branches
- Test edge cases where multiple conditions might conflict

### **Performance Considerations**
- Use Update() sparingly - consider event-driven triggers instead
- Cache frequently accessed components (flagSystem, etc.)
- Disable unused GameObjects rather than destroying them for performance
- Use object pooling for frequently spawned/despawned NPCs

### **Content Organization**
- Keep cutscene assets organized in dedicated folders
- Use consistent naming conventions for scenes and assets
- Document which flags trigger which content for team members
- Create fallback states for when no conditions are met

### **Player Experience**
- Ensure cutscenes feel earned through player choices
- Provide clear visual/audio feedback when major story events trigger
- Allow players to understand the consequences of their choices
- Test narrative pacing to avoid overwhelming the player

---

## 🌟 **The Power of Flag-Driven Narratives**

This system transforms your game from a linear experience into a dynamic, reactive world where:

- **Every dialogue choice matters** and can lead to unique story moments
- **The world remembers** and responds to player actions
- **Multiple playthroughs** reveal different content and story paths
- **Players feel agency** in shaping their narrative experience
- **Content creators** can easily add new story branches without programming

Your flag system becomes the foundation for rich, branching narratives that rival the complexity of major RPGs, all built on the simple concept of remembering what the player has done! 🎭✨

---

## 🔗 **Integration with Existing Systems**

This cutscene system works seamlessly with your existing:
- ✅ **Choice-based dialogue system**
- ✅ **Flag management system** 
- ✅ **NPC interaction system**
- ✅ **Save/load functionality**
- ✅ **Day/night cycle system**

Everything is designed to work together as a cohesive narrative engine!