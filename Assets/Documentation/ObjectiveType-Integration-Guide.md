# ObjectiveType Integration Guide

## Overview

This guide explains how each ObjectiveType works in your quest system and how to set them up for automatic completion.

## 🎯 **Supported ObjectiveTypes**

### **1. TalkToNPC** ✅ (Automatic)
**Automatically completes when player talks to the specified NPC.**

#### Setup:
```csharp
new QuestObjective
{
    objectiveID = "talk_to_elder",
    description = "Speak with the village elder",
    type = ObjectiveType.TalkToNPC,
    targetNPC = "village_elder",  // Must match NPC's npcID field
    isOptional = false
}
```

#### NPC Setup:
1. **Set NPC ID**: In NPC component, set `npcID = "village_elder"`
2. **Automatic**: When player talks to NPC, objective completes automatically
3. **Fallback**: Uses `npcName` if `npcID` is empty

#### How it Works:
- Player interacts with NPC → `StartDialogue()` called
- `CheckTalkToNPCObjectives()` finds matching objectives
- Objectives completed automatically

---

### **2. CollectItems** ✅ (QuestTrigger)
**Tracks item collection progress using QuestTrigger components.**

#### Setup:
```csharp
new QuestObjective
{
    objectiveID = "collect_materials",
    description = "Gather construction materials",
    type = ObjectiveType.CollectItems,
    targetItem = "construction_materials",
    targetAmount = 10,
    currentAmount = 0
}
```

#### Item Setup:
1. **Create collectible prefab** with QuestTrigger component
2. **Configure QuestTrigger:**
   ```
   objectiveToComplete: "collect_materials"
   questForObjective: "dam_construction_project"
   progressAmount: 1
   destroyAfterTrigger: true
   ```

#### How it Works:
- Player touches collectible → QuestTrigger fires
- `progressAmount` added to `currentAmount`
- When `currentAmount >= targetAmount` → Objective completes

---

### **3. VisitLocation** ✅ (LocationTrigger)
**Automatically completes when player visits specified location.**

#### Setup:
```csharp
new QuestObjective
{
    objectiveID = "reach_village_well",
    description = "Go to the village well",
    type = ObjectiveType.VisitLocation,
    targetLocation = "VillageWell"
}
```

#### Location Setup:
1. **Add LocationTrigger component** to location GameObject
2. **Configure LocationTrigger:**
   ```
   locationID: "VillageWell"  // Must match targetLocation
   locationName: "Village Well"
   requiredTag: "Player"
   showLocationMessage: true
   ```

#### How it Works:
- Player enters trigger area → `OnTriggerEnter2D()` called  
- `ObjectiveAutoCompletion.OnLocationVisited()` notified
- Matching VisitLocation objectives completed automatically

---

### **4. DefeatEnemies** ✅ (Manual Integration)
**Tracks enemy defeats through manual API calls.**

#### Setup:
```csharp
new QuestObjective
{
    objectiveID = "defeat_slimes",
    description = "Defeat 5 slimes",
    type = ObjectiveType.DefeatEnemies,
    targetItem = "slime",  // Enemy type identifier
    targetAmount = 5,
    currentAmount = 0
}
```

#### Enemy Integration:
```csharp
// In your enemy death/destruction code:
void OnEnemyDeath()
{
    ObjectiveAutoCompletion.Instance.OnEnemyDefeated("slime");
}

// Or in enemy script:
void Die()
{
    // ... existing death logic ...
    
    // Notify quest system
    if (ObjectiveAutoCompletion.Instance != null)
    {
        ObjectiveAutoCompletion.Instance.OnEnemyDefeated(enemyType);
    }
}
```

#### How it Works:
- Enemy defeated → `OnEnemyDefeated()` called manually
- System finds matching DefeatEnemies objectives
- `currentAmount` incremented, completes when target reached

---

### **5. FlagCondition** ✅ (Automatic)
**Automatically completes when specified story flag is set.**

#### Setup:
```csharp
new QuestObjective
{
    objectiveID = "complete_ritual",
    description = "Complete the spiritual ritual",
    type = ObjectiveType.FlagCondition,
    targetItem = "ritual_completed",  // Flag name to watch
    // OR use requiredFlags array:
    requiredFlags = new string[] { "ritual_completed" }
}
```

#### How it Works:
- Any flag gets added → `FlagMonitorSystem.OnFlagAdded` triggers
- `CheckFlagConditionObjectives()` checks all FlagCondition objectives
- Objectives complete when their required flag is set

---

### **6. TimeDelay** ✅ (Automatic)
**Automatically completes after specified time passes.**

#### Setup:
```csharp
new QuestObjective
{
    objectiveID = "wait_for_dawn",
    description = "Wait until morning",
    type = ObjectiveType.TimeDelay,
    timeDelay = 6f  // 6 AM in game time
}
```

#### How it Works:
- System checks every minute via coroutine
- Compares `DayNightCycle.CurrentTime` with `timeDelay`
- Completes when current time >= delay time

---

### **7. Custom** ✅ (Manual)
**Requires manual completion via QuestTrigger or code.**

#### Setup:
```csharp
new QuestObjective
{
    objectiveID = "build_dam",
    description = "Construct the dam",
    type = ObjectiveType.Custom
}
```

#### Manual Completion:
```csharp
// Via QuestTrigger component
questTrigger.objectiveToComplete = "build_dam";

// Via code
QuestManager.Instance.CompleteObjective("dam_construction", "build_dam");

// Via flag monitoring
FlagMonitorSystem.WatchFlagAdded("dam_built", () => {
    QuestManager.Instance.CompleteObjective("dam_construction", "build_dam");
});
```

---

## 🛠️ **Implementation Checklist**

### **For TalkToNPC Objectives:**
- ✅ Set `npcID` field on NPC components
- ✅ Match `targetNPC` in objectives with NPC IDs
- ✅ System automatically integrated with NPCInteractionSystem

### **For CollectItems Objectives:**
- ✅ Create collectible prefabs with QuestTrigger
- ✅ Set `objectiveToComplete` and `progressAmount`
- ✅ Works with existing QuestTrigger system

### **For VisitLocation Objectives:**
- ✅ Add LocationTrigger components to location GameObjects
- ✅ Set `locationID` to match `targetLocation` in objectives
- ✅ Configure trigger areas and feedback

### **For DefeatEnemies Objectives:**
- ❌ **TODO**: Add `OnEnemyDefeated()` calls to enemy scripts
- ❌ **TODO**: Set enemy type identifiers
- ✅ System ready to receive notifications

### **For FlagCondition Objectives:**
- ✅ Use existing flag system
- ✅ Set `targetItem` or `requiredFlags` to flag names
- ✅ Automatically integrated with FlagMonitorSystem

### **For TimeDelay Objectives:**
- ✅ Set `timeDelay` to target game time
- ✅ Automatically integrated with DayNightCycle
- ✅ Checks every minute automatically

---

## 🎮 **Usage Examples from Your Game**

### **Water Crisis Quest:**
```csharp
// TalkToNPC objective
{
    objectiveID = "talk_to_villagers",
    type = ObjectiveType.TalkToNPC,
    targetNPC = "warga_haus_1"  // ✅ Set npcID = "warga_haus_1" on NPC
}

// VisitLocation objective  
{
    objectiveID = "reach_village_well",
    type = ObjectiveType.VisitLocation,
    targetLocation = "VillageWell"  // ✅ Add LocationTrigger with locationID = "VillageWell"
}
```

### **Dam Construction Quest:**
```csharp
// CollectItems objective
{
    objectiveID = "collect_materials",
    type = ObjectiveType.CollectItems,
    targetItem = "construction_materials",
    targetAmount = 10  // ✅ Create 10 collectibles with QuestTrigger progressAmount = 1
}

// Custom objective (manual completion)
{
    objectiveID = "build_initial_dam",
    type = ObjectiveType.Custom  // ✅ Complete via QuestTrigger or dialogue flags
}
```

### **Spiritual Vision Quest:**
```csharp
// FlagCondition objective
{
    objectiveID = "complete_offering",
    type = ObjectiveType.FlagCondition,
    targetItem = "spiritual_offering_complete"  // ✅ Auto-completes when flag set
}

// TimeDelay objective
{
    objectiveID = "meditate_until_dawn",
    type = ObjectiveType.TimeDelay,
    timeDelay = 6f  // ✅ Auto-completes at 6 AM
}
```

---

## 🔧 **Setup Instructions**

### **1. Scene Setup:**
1. **Add ObjectiveAutoCompletion** to scene (auto-creates if missing)
2. **Add LocationTrigger** components to important locations
3. **Set npcID** fields on all quest-relevant NPCs
4. **Create collectible prefabs** with QuestTrigger components

### **2. Enemy Integration:**
```csharp
// Add to your enemy death code:
void OnDestroy()
{
    if (ObjectiveAutoCompletion.Instance != null)
    {
        ObjectiveAutoCompletion.Instance.OnEnemyDefeated(enemyType);
    }
}
```

### **3. NPC ID Setup:**
```csharp
// Set these IDs on your NPCs to match quest objectives:
Ki Ageng Sinawang: npcID = "ki_ageng_sinawang"
Mbok Randa: npcID = "mbok_randa_krandon"  
Village Elder: npcID = "village_elder"
Pak Darmo: npcID = "pak_darmo"
// etc.
```

---

## 🎯 **Benefits**

### **Automatic Completion:**
- ✅ **TalkToNPC**: Talk to NPC → Objective completes
- ✅ **VisitLocation**: Enter area → Objective completes  
- ✅ **FlagCondition**: Flag set → Objective completes
- ✅ **TimeDelay**: Time passes → Objective completes

### **Manual Completion (when needed):**
- ✅ **CollectItems**: Via QuestTrigger progressAmount
- ✅ **DefeatEnemies**: Via OnEnemyDefeated() calls
- ✅ **Custom**: Via QuestTrigger or direct API calls

### **Developer Experience:**
- 📝 **Less code** - Most objectives work automatically
- 🐛 **Easier debugging** - Clear completion logs
- 🔄 **Consistent behavior** - All objectives work the same way
- 🎮 **Better UX** - Players see immediate objective progress

Your quest system now supports **all ObjectiveTypes** with mostly automatic completion! 🎉