# Flag System Flow Analysis

## Overview
This document maps the complete flag flow system in the MenakSopal Unity project, from NPC interactions through all related systems, and identifies which scripts need to be persistent across scenes.

## Flag Flow Architecture

### 1. Flag Creation & Storage
**Primary Source: NPCInteractionSystem.cs**
- **Location**: `Assets/Script/NPC/NPCInteractionSystem.cs:81`
- **Storage**: `private List<string> gameFlags = new List<string>();`
- **Methods**:
  - `AddGameFlag(string flag)` - Line 1055
  - `RemoveGameFlag(string flag)` - Line 1065  
  - `HasGameFlag(string flag)` - Line 1074
  - `GetGameFlags()` - Line 1084

### 2. Flag Triggers & Sources

#### A. Dialogue System Triggers
**Source: NPCInteractionSystem.cs**
- **DialogueEntry consequences**: Lines 1017-1038
  - `entry.flagsToAdd[]` → Calls `AddGameFlag()`
  - `entry.flagsToRemove[]` → Calls `RemoveGameFlag()`
- **Choice System consequences**: Lines 951-973
  - `choice.flagsToAdd[]` → Calls `AddGameFlag()`
  - `choice.flagsToRemove[]` → Calls `RemoveGameFlag()`

#### B. Quest System Integration
**Source: QuestTrigger.cs**
- **Conditional flag checking**: Lines 131-167
  - `requiredFlags[]` - Must be present to trigger
  - `blockingFlags[]` - Prevent triggering if present
- **Flag validation**: Queries `NPCInteractionSystem.GetGameFlags()`

### 3. Flag Monitoring & Reactions

#### A. Event-Driven System
**Source: FlagMonitorSystem.cs**
- **Event notifications**: Lines 21-23
  - `OnFlagAdded` - Triggered when flag added
  - `OnFlagRemoved` - Triggered when flag removed  
  - `OnFlagChanged` - Triggered on any flag change
- **Watcher registration**: Lines 67-135
  - `WatchFlag()` - General flag watching
  - `WatchFlagAdded()` - Only triggers on add
  - `WatchFlagRemoved()` - Only triggers on remove

#### B. Automatic System Integration
**NPCInteractionSystem → FlagMonitorSystem Integration**:
```csharp
// Line 1061: When flag added
FlagMonitorSystem.NotifyFlagAdded(flag);

// Line 1070: When flag removed  
FlagMonitorSystem.NotifyFlagRemoved(flag);
```

### 4. Flag Consumers & Systems

#### A. Quest System
**QuestManager.cs** - Uses flags for:
- Quest start conditions: `quest.CanStart(gameFlags)`
- Objective completion: FlagCondition objectives
- Quest progression gating

#### B. Dialogue System 
**DialogueData.cs** - Uses flags for:
- Dialogue availability: `GetAvailableDialogues(currentTime, gameFlags)`
- Choice availability: `GetAvailableChoices(entry, currentTime, gameFlags)`
- Conditional responses

#### C. Objective Auto-Completion
**ObjectiveAutoCompletion.cs** - Uses flags for:
- FlagCondition objectives: Lines monitoring flag changes
- Automatic quest progression based on flag states

### 5. Manual Flag Implementation Points

#### Flags You Need to Manually Set via QuestTrigger:
Based on quest data analysis, these flags typically need manual triggers:

1. **Story Progression Flags**:
   - `water_crisis_discovered`
   - `dam_construction_started` 
   - `spirit_vision_completed`
   - `white_elephant_found`
   - `sacrifice_completed`

2. **Location Discovery Flags**:
   - `arrived_at_krandon`
   - `found_padepokan`
   - `reached_river_source`

3. **Character Interaction Flags**:
   - `met_mbok_randa`
   - `talked_to_ki_ageng`
   - `received_guidance`

4. **Achievement Flags**:
   - `dam_completed_successfully`
   - `reconciliation_achieved`
   - `story_completion_witnessed`

#### QuestTrigger Setup Examples:
```csharp
// For location-based flags
QuestTrigger locationTrigger = GetComponent<QuestTrigger>();
locationTrigger.requiredFlags = new string[] { "water_crisis_discovered" };
locationTrigger.objectiveToComplete = "visit_krandon";
locationTrigger.questForObjective = "05_JourneyToKrandon";
```

## Scene Persistence Analysis

### Scripts That MUST Be Persistent (DontDestroyOnLoad)

#### Core System Scripts:
1. **QuestManager.cs** ✅ Already persistent (Line 50)
2. **FlagMonitorSystem.cs** ✅ Already persistent (Line 54)  
3. **GameSaveManager.cs** ✅ Already persistent
4. **ObjectiveAutoCompletion.cs** ✅ Already persistent

#### Scripts That SHOULD Be Persistent:
1. **NPCInteractionSystem.cs** ❌ NOT persistent
   - **Issue**: Stores all game flags but resets on scene change
   - **Solution**: Add DontDestroyOnLoad or integrate with save system

2. **DayNightCycle.cs** - Depends on game design
   - Currently NOT persistent
   - Should be persistent if time continues across scenes

### Scene Transition Issues

#### Current Problems:
1. **Flag Loss**: NPCInteractionSystem flags reset between scenes
2. **State Desync**: Quest system may lose dialogue flag references  
3. **Progress Loss**: Objective completion states not preserved

#### Recommended Solutions:

##### Option 1: Make NPCInteractionSystem Persistent
```csharp
// In NPCInteractionSystem.Awake()
if (Instance == null)
{
    Instance = this;
    DontDestroyOnLoad(gameObject);
}
else
{
    // Transfer flags to existing instance
    Instance.gameFlags.AddRange(this.gameFlags.Except(Instance.gameFlags));
    Destroy(gameObject);
}
```

##### Option 2: Integrate with Save System  
```csharp
// Save flags to GameSaveManager
GameSaveManager.Instance.SaveFlags(gameFlags);

// Load flags on scene start
gameFlags = GameSaveManager.Instance.LoadFlags();
```

## Flag Implementation Workflow

### For New Story Elements:

1. **Identify Flag Points**: Where story events occur
2. **Create QuestTriggers**: At location/interaction points
3. **Set Conditions**: Required flags for trigger activation
4. **Configure Actions**: Which flags to add/remove
5. **Test Flow**: Verify flag propagation through systems

### Example Implementation:
```csharp
// Story location trigger
GameObject triggerGO = new GameObject("WaterCrisisDiscoveryTrigger");
QuestTrigger trigger = triggerGO.AddComponent<QuestTrigger>();
trigger.triggerType = TriggerType.OnTriggerEnter;
trigger.requiredTag = "Player";
trigger.questToStart = "01_WaterCrisisDiscovery";
trigger.requiredFlags = new string[] { }; // No requirements
trigger.destroyAfterTrigger = true;

// Position at discovery location
triggerGO.transform.position = waterCrisisLocation;
```

## Integration Points Summary

### Flag Sources:
- Dialogue system (choices & consequences)
- Quest system (completion triggers)  
- Manual QuestTriggers (location/event based)

### Flag Consumers:
- Quest availability & progression
- Dialogue branching & availability
- Objective auto-completion
- Story progression gates

### Critical Scripts for Scene Persistence:
1. **Must Persist**: QuestManager, FlagMonitorSystem, GameSaveManager
2. **Should Persist**: NPCInteractionSystem (contains flags)
3. **Scene-Specific**: QuestTriggers, LocationTriggers, NPCs

### Main Menu → Game Flow:
From MainMenu scene to gameplay:
1. QuestManager persists across scenes
2. FlagMonitorSystem persists and maintains watchers  
3. NPCInteractionSystem needs to be handled (currently resets)
4. GameSaveManager handles save/load of flag states
5. ObjectiveAutoCompletion maintains quest monitoring

This architecture provides a robust flag system for story-driven gameplay with proper persistence and automatic quest progression.