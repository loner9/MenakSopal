# Quest System Guide

## Overview

The Quest System provides a comprehensive framework for creating, managing, and tracking quests in your Unity game. It seamlessly integrates with the existing dialogue and flag systems while providing a professional quest journal interface styled with the Adventure Book theme.

## Table of Contents

1. [Core Components](#core-components)
2. [Quest Creation](#quest-creation)
3. [Dialogue Integration](#dialogue-integration)
4. [Quest Journal UI](#quest-journal-ui)
5. [Quest Triggers](#quest-triggers)
6. [Objective Types](#objective-types)
7. [Save/Load System](#saveload-system)
8. [Examples](#examples)
9. [Best Practices](#best-practices)
10. [Troubleshooting](#troubleshooting)

## Core Components

### QuestManager
- **Location**: `Assets/Script/Quest/Core/QuestManager.cs`
- **Purpose**: Singleton that manages all quest operations
- **Key Features**:
  - Quest state management (Active, Completed, Failed)
  - Objective progress tracking
  - Event system for UI updates
  - Save/load functionality
  - Integration with flag system

### QuestData (ScriptableObject)
- **Location**: `Assets/Script/Quest/Core/QuestData.cs`
- **Purpose**: Defines quest structure and properties
- **Creation**: Right-click → Create → Quest System → Quest Data

### QuestJournalUI
- **Location**: `Assets/Script/Quest/UI/QuestJournalUI.cs`
- **Purpose**: Adventure Book themed quest journal interface
- **Features**:
  - Tabbed interface (Active/Completed/Failed)
  - Quest details panel
  - Objective tracking
  - Book opening/closing animations

## Quest Creation

### 1. Creating a Basic Quest

```csharp
// Create quest asset
QuestData quest = ScriptableObject.CreateInstance<QuestData>();

// Basic information
quest.questID = "meet_village_elder";
quest.questTitle = "Meet the Village Elder";
quest.questDescription = "The village elder wishes to speak with you about important matters.";
quest.questType = QuestType.Main;
quest.questLevel = 1;

// Prerequisites and consequences
quest.requiredFlags = new string[] { "arrived_in_village" };
quest.flagsOnStart = new string[] { "elder_quest_started" };
quest.flagsOnComplete = new string[] { "elder_quest_completed", "gained_village_trust" };
```

### 2. Adding Objectives

```csharp
// Talk to NPC objective
var talkObjective = new QuestObjective
{
    objectiveID = "talk_to_elder",
    description = "Find and speak with Elder Marcus",
    type = ObjectiveType.TalkToNPC,
    targetNPC = "Elder_Marcus",
    flagToSetOnComplete = "talked_to_elder"
};

// Collection objective
var collectObjective = new QuestObjective
{
    objectiveID = "collect_herbs",
    description = "Collect healing herbs",
    type = ObjectiveType.CollectItems,
    targetItem = "healing_herb",
    targetAmount = 5,
    showProgress = true,
    flagToSetOnComplete = "collected_herbs"
};

quest.objectives.Add(talkObjective);
quest.objectives.Add(collectObjective);
```

### 3. Quest Rewards

```csharp
// Flag reward
quest.rewards.Add(new QuestReward
{
    type = QuestRewardType.Flags,
    flagsToAdd = new string[] { "village_hero", "merchant_discount" }
});

// Item reward (for future inventory system)
quest.rewards.Add(new QuestReward
{
    type = QuestRewardType.Item,
    itemID = "magic_sword",
    amount = 1
});
```

## Dialogue Integration

### 1. Quest Start Choices

Add quest triggers to DialogueChoice in your DialogueData assets:

```csharp
// In your DialogueData asset
var choice = new DialogueChoice
{
    choiceText = "I'd like to help the village.",
    questToStart = "meet_village_elder",
    flagsToAdd = new string[] { "volunteered_to_help" },
    isImportantChoice = true,
    response = new DialogueResponse
    {
        speakerName = "Elder Marcus",
        responseText = "Excellent! Your willingness to help is exactly what we need.",
        continueToNext = false
    }
};
```

### 2. Objective Completion

```csharp
var completionChoice = new DialogueChoice
{
    choiceText = "I've completed the task you gave me.",
    questForObjective = "meet_village_elder",
    objectiveToComplete = "talk_to_elder",
    requiredFlags = new string[] { "task_completed" },
    response = new DialogueResponse
    {
        speakerName = "Elder Marcus",
        responseText = "Well done! You have proven yourself trustworthy.",
        continueToNext = false
    }
};
```

### 3. Quest Completion

```csharp
var questCompleteChoice = new DialogueChoice
{
    choiceText = "I've finished everything you asked of me.",
    questToComplete = "meet_village_elder",
    requiredFlags = new string[] { "all_tasks_done" },
    isImportantChoice = true
};
```

## Quest Journal UI

### 1. Setup Requirements

1. **QuestJournalUI Component**: Add to a UI Canvas
2. **UI References**: Assign all required UI elements
3. **Adventure Book Sprites**: Use sprites from your Adventure Book assets
4. **Prefabs**: Create QuestEntry and ObjectiveEntry prefabs

### 2. Key Bindings

- **Default**: Press `J` to open/close quest journal
- **Navigation**: Tab between Active/Completed/Failed quests
- **Selection**: Click quest entries to view details

### 3. Customization

```csharp
// Customize colors and appearance
public Color selectedTabColor = Color.yellow;
public Color completedColor = Color.green;
public Color failedColor = Color.red;

// Audio feedback
public AudioClip bookOpenSound;
public AudioClip questCompleteSound;
```

## Quest Triggers

### 1. QuestTrigger Component

Use for location-based or collision-based quest events:

```csharp
// Add to any GameObject with a Collider2D
var trigger = gameObject.AddComponent<QuestTrigger>();

trigger.questToStart = "forest_exploration";
trigger.triggerType = TriggerType.OnTriggerEnter;
trigger.requiredTag = "Player";
trigger.requiredFlags = new string[] { "has_map" };
```

### 2. Manual Triggers

For script-based quest progression:

```csharp
// Get trigger reference
QuestTrigger trigger = GetComponent<QuestTrigger>();

// Trigger manually
trigger.ManualTrigger();

// Or with specific object
trigger.ManualTrigger(playerObject);
```

## Objective Types

### 1. TalkToNPC
Complete when talking to a specific NPC:
```csharp
var objective = new QuestObjective
{
    type = ObjectiveType.TalkToNPC,
    targetNPC = "Blacksmith_John",
    description = "Speak with the village blacksmith"
};
```

### 2. CollectItems
Track collection progress:
```csharp
var objective = new QuestObjective
{
    type = ObjectiveType.CollectItems,
    targetItem = "iron_ore",
    targetAmount = 10,
    showProgress = true,
    description = "Collect iron ore from the mines"
};
```

### 3. DefeatEnemies
Count enemy defeats:
```csharp
var objective = new QuestObjective
{
    type = ObjectiveType.DefeatEnemies,
    targetAmount = 5,
    showProgress = true,
    description = "Defeat slimes in the forest"
};
```

### 4. VisitLocation
Complete when reaching a location:
```csharp
var objective = new QuestObjective
{
    type = ObjectiveType.VisitLocation,
    targetLocation = "ancient_ruins",
    description = "Explore the ancient ruins"
};
```

### 5. TimeDelay
Wait for a specific time:
```csharp
var objective = new QuestObjective
{
    type = ObjectiveType.TimeDelay,
    timeDelay = 24f, // 24 game hours
    description = "Wait until tomorrow"
};
```

### 6. FlagCondition
Complete when a flag is set:
```csharp
var objective = new QuestObjective
{
    type = ObjectiveType.FlagCondition,
    flagToSetOnComplete = "ritual_completed",
    description = "Complete the ancient ritual"
};
```

## Save/Load System

### 1. Automatic Integration

The QuestManager automatically integrates with save systems:

```csharp
// Get save data
var questSaveData = QuestManager.Instance.GetSaveData();

// Save to your save system
saveFile.questData = questSaveData;

// Load from save system
QuestManager.Instance.LoadSaveData(saveFile.questData);
```

### 2. Save Data Structure

```csharp
[System.Serializable]
public class QuestManagerSaveData
{
    public List<string> activeQuestIDs;
    public List<string> completedQuestIDs;
    public List<string> failedQuestIDs;
    public List<QuestSaveData> questSaveData;
}
```

## Examples

### 1. Simple Fetch Quest

```csharp
public static QuestData CreateFetchQuest()
{
    var quest = ScriptableObject.CreateInstance<QuestData>();
    
    quest.questID = "fetch_medicine";
    quest.questTitle = "Urgent Medicine";
    quest.questDescription = "The healer needs rare herbs for a critically ill patient.";
    quest.questType = QuestType.Side;
    
    // Objectives
    quest.objectives.Add(new QuestObjective
    {
        objectiveID = "get_herbs",
        description = "Collect 3 Moonleaf herbs from the forest",
        type = ObjectiveType.CollectItems,
        targetItem = "moonleaf",
        targetAmount = 3
    });
    
    quest.objectives.Add(new QuestObjective
    {
        objectiveID = "return_herbs",
        description = "Return the herbs to the healer",
        type = ObjectiveType.TalkToNPC,
        targetNPC = "Healer_Sarah",
        requiredFlags = new string[] { "collected_moonleaf" }
    });
    
    return quest;
}
```

### 2. Multi-Stage Quest Chain

```csharp
public static QuestData CreateInvestigationQuest()
{
    var quest = ScriptableObject.CreateInstance<QuestData>();
    
    quest.questID = "mystery_investigation";
    quest.questTitle = "The Missing Merchant";
    quest.questDescription = "A traveling merchant has gone missing. Investigate his disappearance.";
    quest.questType = QuestType.Main;
    
    // Stage 1: Gather information
    quest.objectives.Add(new QuestObjective
    {
        objectiveID = "talk_to_witnesses",
        description = "Question the townspeople about the merchant",
        type = ObjectiveType.Custom,
        flagToSetOnComplete = "questioned_witnesses"
    });
    
    // Stage 2: Search location
    quest.objectives.Add(new QuestObjective
    {
        objectiveID = "search_forest",
        description = "Search the Dark Forest for clues",
        type = ObjectiveType.VisitLocation,
        targetLocation = "dark_forest_clearing",
        requiredFlags = new string[] { "questioned_witnesses" }
    });
    
    // Stage 3: Confront villain
    quest.objectives.Add(new QuestObjective
    {
        objectiveID = "defeat_bandits",
        description = "Defeat the bandit leader",
        type = ObjectiveType.DefeatEnemies,
        targetAmount = 1,
        requiredFlags = new string[] { "found_bandit_camp" }
    });
    
    return quest;
}
```

## Best Practices

### 1. Quest Design
- **Clear Objectives**: Make quest goals obvious and achievable
- **Logical Progression**: Use flags to create meaningful quest chains
- **Player Choice**: Offer optional objectives and multiple solutions
- **Balanced Rewards**: Match rewards to quest difficulty and importance

### 2. Flag Management
- **Consistent Naming**: Use descriptive, consistent flag names
- **Namespacing**: Prefix flags with quest IDs to avoid conflicts
- **Documentation**: Comment complex flag interactions

### 3. UI Integration
- **Feedback**: Provide clear progress indicators
- **Accessibility**: Support keyboard navigation
- **Visual Hierarchy**: Use colors and typography effectively

### 4. Performance
- **Asset Organization**: Keep quest assets in organized folders
- **Memory Management**: Unload unused quest data when appropriate
- **Event Cleanup**: Unsubscribe from events properly

## Troubleshooting

### Common Issues

1. **Quest Not Starting**
   - Check required flags are set
   - Verify QuestManager is present in scene
   - Confirm quest ID matches exactly

2. **Objectives Not Completing**
   - Ensure objective IDs are unique
   - Check flag conditions are met
   - Verify trigger methods are called correctly

3. **UI Not Updating**
   - Check event subscriptions
   - Verify UI references are assigned
   - Ensure QuestJournalUI is active

4. **Save/Load Issues**
   - Confirm save data structure matches
   - Check for null references
   - Verify quest IDs exist in available quests

### Debug Tools

```csharp
// Enable debug logging
QuestManager.Instance.showDebugLogs = true;

// Manual quest testing
QuestManager.Instance.StartQuest("test_quest");
QuestManager.Instance.CompleteObjective("test_quest", "test_objective");

// Reset all quests for testing
QuestManager.Instance.ResetAllQuests();
```

---

**Created**: 2025-01-26  
**Version**: 1.0  
**Compatibility**: Unity 6000.0.36f1 with existing MenakSopal systems

This quest system provides a solid foundation for quest-based gameplay while maintaining compatibility with your existing dialogue and flag systems.