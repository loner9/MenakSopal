# Save System Integration Guide

## Overview

The GameSaveManager provides a comprehensive save/load system for MenakSopal that automatically saves story progression and allows manual save management.

## Features Implemented

### ✅ **Complete Save System**
- **GameSaveManager.cs** - Core save/load functionality
- **SaveLoadUI.cs** - UI interface for manual saves
- **SceneTransitionWithSave.cs** - Auto-save on scene changes

### ✅ **Auto-Save Integration**
- Story progression auto-saves
- Quest completion auto-saves  
- Scene transition auto-saves
- Timed auto-saves (every 5 minutes)

### ✅ **Data Saved**
- Quest progress (active, completed, failed)
- Story flags (all dialogue choices)
- NPC states and schedules
- Day/night cycle time
- Player position and health
- Current scene

## Quick Start

### 1. **Automatic Setup**
The system initializes automatically when first accessed. No manual setup required.

### 2. **Auto-Save Points Already Configured**
```csharp
// These trigger automatically when flags are set:
- water_crisis_discovered → "Chapter2_WaterCrisis"
- asked_permission_water_project → "Chapter2_PermissionGranted"  
- student_helpers_recruited → "Chapter2_HelpersRecruited"
- spirit_pact_complete → "Chapter4_SpiritPact"
- white_elephant_taken → "Chapter5_ElephantTaken"
- reconciliation_complete → "Chapter6_Reconciliation"
- story_completed → "StoryComplete"
```

### 3. **Quest Auto-Saves**
```csharp
// Triggers when any quest completes:
GameSaveManager.Instance.AutoSave($"Quest_{questID}_Complete");
```

## Manual Save/Load API

### **Save Game**
```csharp
// Save to specific slot
GameSaveManager.Instance.SaveGame("MyGame");

// Auto-save with description
GameSaveManager.Instance.AutoSave("ChapterStart");

// Quick save to current slot
GameSaveManager.Instance.QuickSave();
```

### **Load Game**
```csharp
// Load from specific slot
GameSaveManager.Instance.LoadGame("MyGame");

// Quick load latest save
GameSaveManager.Instance.TriggerQuickLoad();
```

### **Save Management**
```csharp
// Check if save exists
bool exists = GameSaveManager.Instance.SaveExists("MyGame");

// Get save file info
SaveFileInfo info = GameSaveManager.Instance.GetSaveInfo("MyGame");

// Get all saves
List<SaveFileInfo> saves = GameSaveManager.Instance.GetSaveFiles();

// Delete save
GameSaveManager.Instance.DeleteSave("MyGame");
```

## UI Integration

### **Add to Scene**
1. Create UI GameObject
2. Add `SaveLoadUI` component
3. Assign UI references in inspector
4. Create save slot prefabs

### **Call from Buttons**
```csharp
// Open save/load menu
FindObjectOfType<SaveLoadUI>().OpenSaveLoadPanel();

// Quick save button
FindObjectOfType<SaveLoadUI>().QuickSave();

// Quick load button  
FindObjectOfType<SaveLoadUI>().QuickLoad();
```

## Scene Transition Integration

### **Auto-Save Scene Changes**
```csharp
// Use SceneTransitionWithSave instead of SceneManager
SceneTransitionWithSave.Instance.LoadScene("SceneHutan");

// Set up flag-based transitions
SceneTransitionWithSave.Instance.LoadSceneOnFlag("SceneDesaKrandon", "journey_to_krandon");
```

### **Quest-Based Scene Changes**
```csharp
// In QuestTrigger component, add:
SceneTransitionWithSave.Instance.TransitionForQuest("05_JourneyToKrandon", "SceneDesaKrandon");
```

## Save File Locations

### **Save Directory**
```
Windows: %USERPROFILE%/AppData/LocalLow/[CompanyName]/[GameName]/Saves/
Mac: ~/Library/Application Support/[CompanyName]/[GameName]/Saves/
Linux: ~/.config/unity3d/[CompanyName]/[GameName]/Saves/
```

### **File Format**
- **Format:** JSON (human-readable)
- **Extension:** .json
- **Naming:** SlotName.json
- **Backup:** SlotName_backup.json

## Save File Structure

```json
{
  "saveVersion": "1.0",
  "saveTime": 638000000000000000,
  "gameVersion": "1.0.0",
  "currentScene": "SceneAwal",
  "playerPosition": {"x": 10.0, "y": 5.0, "z": 0.0},
  "playerHealth": 100.0,
  "playTime": 1800.0,
  "questData": {
    "activeQuestIDs": ["01_WaterCrisisDiscovery"],
    "completedQuestIDs": [],
    "failedQuestIDs": []
  },
  "dialogueData": {
    "gameFlags": ["water_crisis_discovered", "asked_permission_water_project"]
  },
  "npcData": {
    "npcStates": [...]
  },
  "timeData": {
    "currentTime": 14.5,
    "currentDay": 1
  },
  "totalFlags": 15
}
```

## Events System

### **Subscribe to Save Events**
```csharp
void Start()
{
    GameSaveManager.OnGameSaved += OnGameSaved;
    GameSaveManager.OnGameLoaded += OnGameLoaded;
    GameSaveManager.OnSaveError += OnSaveError;
}

void OnGameSaved(string slotName)
{
    ShowMessage($"Game saved: {slotName}");
}

void OnGameLoaded(string slotName)
{
    ShowMessage($"Game loaded: {slotName}");
}

void OnSaveError(string error)
{
    ShowMessage($"Save error: {error}");
}
```

## Configuration Options

### **GameSaveManager Inspector**
```
Save Settings:
├─ Enable Auto Save: ✓
├─ Max Save Slots: 5
├─ Create Backups: ✓
└─ Auto Save Interval: 300s

Debug:
├─ Enable Debug Logs: ✓
└─ Show Save Notifications: ✓
```

### **Disable Auto-Save**
```csharp
// Disable all auto-saves
GameSaveManager.Instance.enableAutoSave = false;

// Disable specific auto-save points
// (Remove FlagMonitorSystem.WatchFlagAdded calls)
```

## Best Practices

### **When to Save**
✅ **Good Save Points:**
- Major story progression
- Quest completions
- Scene transitions
- Before difficult sections
- Player-initiated saves

❌ **Avoid Saving:**
- Every dialogue line
- Minor flag changes
- Combat start/end
- Frequent movement

### **Save Slot Management**
- **Auto-saves:** Descriptive names with timestamps
- **Manual saves:** Player-chosen names
- **Quick saves:** Overwrite same slot
- **Cleanup:** Remove old auto-saves periodically

### **Error Handling**
```csharp
// Always check save success
bool success = GameSaveManager.Instance.SaveGame("MyGame");
if (!success)
{
    // Handle save failure
    ShowErrorMessage("Failed to save game");
}
```

## Integration with Your Existing Systems

### **QuestTrigger Integration**
Add to your QuestTrigger component:
```csharp
[Header("Save Integration")]
public bool saveOnTrigger = false;
public string saveDescription = "";

// In trigger method:
if (saveOnTrigger)
{
    GameSaveManager.Instance.AutoSave(saveDescription);
}
```

### **FlagMonitorSystem Integration**
```csharp
// Add new auto-save points:
FlagMonitorSystem.WatchFlagAdded("new_story_flag", () => {
    GameSaveManager.Instance.AutoSave("NewStoryBeat");
});
```

### **UI Menu Integration**
```csharp
// Add to main menu:
public void OnSaveMenuClicked()
{
    FindObjectOfType<SaveLoadUI>().OpenSaveLoadPanel();
}

// Add to pause menu:
public void OnQuickSaveClicked()
{
    GameSaveManager.Instance.TriggerManualSave();
}
```

## Debugging

### **Debug Commands**
```csharp
// Export current save data
GameSaveManager.Instance.ExportSaveData();

// Show save statistics
var saves = GameSaveManager.Instance.GetSaveFiles();
Debug.Log($"Total saves: {saves.Count}");

// Cleanup old auto-saves
GameSaveManager.Instance.CleanupAutoSaves();
```

### **Console Commands (Inspector)**
- **Manual Save** - Trigger manual save
- **Quick Load** - Load latest save
- **Export Save Data** - Export for debugging

## Troubleshooting

### **Save Not Working**
1. Check console for error messages
2. Verify save directory permissions
3. Check available disk space
4. Enable debug logs in GameSaveManager

### **Load Fails**
1. Check if save file exists
2. Verify save file format (JSON)
3. Check for corrupted save data
4. Try loading backup save

### **Performance Issues**
1. Disable frequent auto-saves
2. Increase auto-save interval
3. Disable save backups
4. Clean up old saves

---

## Summary

The save system is now fully integrated with your game:

✅ **Auto-saves on story progression**  
✅ **Auto-saves on quest completion**  
✅ **Auto-saves on scene transitions**  
✅ **Manual save/load UI**  
✅ **Backup and error handling**  
✅ **Integration with all existing systems**  

**Your players can now:**
- Have their progress automatically saved at key moments
- Manually save/load at any time
- Manage multiple save slots
- Continue their story seamlessly

The system works with your existing dialogue, quest, NPC, and flag systems without requiring changes to your current assets or workflows!