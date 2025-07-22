# NPC System Setup Guide
**Version: 07/20/25**  
**MenakSopal Unity Project**

## Overview
This guide explains how to properly set up NPCs using the tag-based object lookup system with NPCScheduleData and NPCManager.

---

## 🛠️ Complete NPC Setup Workflow

### Step 1: Prepare Scene Objects with Tags

**Tag your scene GameObjects for NPC interaction:**

```
Houses: Tag = "House"
- House1, House2, PlayerHouse, BlacksmithHouse, etc.

Work Stations: Tag = "WorkStation" 
- Blacksmith, Bakery, Farm, Workshop, etc.

Shops: Tag = "Shop"
- GeneralStore, Tavern, WeaponShop, etc.

Other locations: Tag = "NPCTarget"
- Well, Market, ChurchPews, TownSquare, etc.
```

**Why use tags?**
- Performance: Search only within tagged objects instead of entire scene
- Organization: Clear categorization of interactive objects
- Flexibility: Easy to reference objects by name within categories

### Step 2: Create NPCScheduleData Asset

**Right-click in Project → Create → NPC → Schedule Data**

**Example: Blacksmith Schedule**
```csharp
Basic Info:
- scheduleName = "Blacksmith Daily Routine"
- spawnHour = 6

Home Location:
- homeObjectTag = "House"
- homeObjectName = "BlacksmithHouse"
- homePosition = (fallback manual position if object not found)

Schedule Events:
Event 0: hour=6,  targetObjectTag="WorkStation", targetObjectName="Blacksmith", behavior=Work
Event 1: hour=12, targetObjectTag="NPCTarget",   targetObjectName="Well",      behavior=Walk  
Event 2: hour=13, targetObjectTag="WorkStation", targetObjectName="Blacksmith", behavior=Work
Event 3: hour=18, targetObjectTag="House",       targetObjectName="BlacksmithHouse", behavior=Sleep, shouldDespawn=true
```

**Example: Shopkeeper Schedule**
```csharp
Basic Info:
- scheduleName = "General Store Owner"
- spawnHour = 8

Home Location:
- homeObjectTag = "Shop"
- homeObjectName = "GeneralStore"

Schedule Events:
Event 0: hour=8,  targetObjectTag="Shop", targetObjectName="GeneralStore", behavior=Work
Event 1: hour=20, targetObjectTag="House", targetObjectName="ShopkeeperHouse", behavior=Sleep, shouldDespawn=true
```

### Step 3: Setup NPC Prefab

**Create NPC prefab with NPC.cs component:**
1. Create GameObject with NPC.cs script
2. Assign the NPCScheduleData asset to `scheduleData` field
3. Set up Animator, SpriteRenderer, Rigidbody2D
4. Configure interaction range, movement speeds
5. Save as prefab in Assets/Prefabs/NPCs/

### Step 4: Configure NPCManager

**Add to NPCManager's npcSpawnList:**

**Example configurations:**

**Blacksmith (Daytime Worker):**
```csharp
NPCSpawnData:
- npcPrefab = BlacksmithPrefab
- npcID = "blacksmith_001"
- spawnAtStart = false              // Wait for proper time
- spawnBasedOnTime = true           // Use schedule timing
- availableSpawnTimes = [Day, Sunset] // When NPC should exist
- scheduleData = BlacksmithScheduleData
```

**Shopkeeper (Always Available):**
```csharp
NPCSpawnData:
- npcPrefab = ShopkeeperPrefab
- npcID = "shopkeeper_001"
- spawnAtStart = true               // Available immediately
- spawnBasedOnTime = false          // Always present
- scheduleData = ShopkeeperScheduleData
```

**Night Guard:**
```csharp
NPCSpawnData:
- npcPrefab = GuardPrefab
- npcID = "guard_night_001"
- spawnAtStart = false
- spawnBasedOnTime = true
- availableSpawnTimes = [Night]     // Only at night
- scheduleData = NightGuardScheduleData
```

---

## 🎯 Common NPC Patterns

### Pattern 1: Always Available NPCs
**Use Case:** Essential shopkeepers, quest givers, permanent residents
```
spawnAtStart = true
spawnBasedOnTime = false
```

### Pattern 2: Daytime Workers
**Use Case:** Blacksmiths, farmers, market vendors
```
spawnAtStart = false  
spawnBasedOnTime = true
availableSpawnTimes = [Day, Sunset]
```

### Pattern 3: Night-Only NPCs
**Use Case:** Guards, tavern patrons, suspicious characters
```
spawnAtStart = false
spawnBasedOnTime = true  
availableSpawnTimes = [Night]
```

### Pattern 4: Quest/Event NPCs
**Use Case:** Story-specific characters, temporary NPCs
```
spawnAtStart = false
spawnBasedOnTime = false
// Spawn manually via code: npcManager.SpawnNPC(questNPCData)
```

### Pattern 5: Seasonal/Conditional NPCs
**Use Case:** Festival NPCs, weather-dependent characters
```
spawnAtStart = false
spawnBasedOnTime = true
availableSpawnTimes = [specific times]
requiredFlags = ["festival_active"] // Quest system integration
```

---

## 🔧 Technical Details

### Tag-Based Object Lookup
The system uses `NPCScheduleData.FindObjectByTagAndName(tag, name)` for performance:
- Searches only within tagged objects (e.g., 10 houses vs 1000 scene objects)
- Includes caching for frequently accessed objects
- Automatic fallback to manual positions if objects not found

### Performance Features
- Static cache per tag category
- Automatic cache refresh on object lookup failures
- `ClearAllCaches()` and `RefreshTagCache(tag)` for cache management

### Common Tags Reference
```csharp
// From NPCScheduleData.CommonTags
House = "House"
WorkStation = "WorkStation"
Shop = "Shop"
Bed = "Bed"
NPCTarget = "NPCTarget"
Market = "Market"
Farm = "Farm"
Well = "Well"
Church = "Church"
Tavern = "Tavern"
```

---

## 🐛 Troubleshooting

### NPC Not Spawning
1. Check `spawnAtStart` and `spawnBasedOnTime` flags
2. Verify `availableSpawnTimes` includes current time of day
3. Ensure NPCScheduleData is assigned
4. Check `spawnHour` in schedule data

### NPC Spawning at Wrong Location
1. Verify tagged objects exist in scene
2. Check `homeObjectTag` and `homeObjectName` match scene objects
3. Use `scheduleData.GetMissingObjectNames()` for validation
4. Fallback to manual `homePosition` if needed

### Performance Issues
1. Use tags consistently to reduce search scope
2. Call `NPCScheduleData.RefreshTagCache(tag)` when scene objects change
3. Enable `enablePerformanceOptimization` in NPCManager for large NPC counts

### Schedule Events Not Working
1. Ensure target objects are properly tagged
2. Check `targetObjectTag` and `targetObjectName` in schedule events
3. Verify hour values are between 0-23
4. Use scene gizmos to visualize schedule paths

---

## 📝 Best Practices

1. **Consistent Naming:** Use descriptive, consistent names for tagged objects
2. **Tag Organization:** Group related objects under same tags
3. **Fallback Positions:** Always provide manual positions as backup
4. **Testing:** Use gizmos visualization to verify NPC paths and schedules
5. **Performance:** Use `enablePerformanceOptimization` for 20+ NPCs
6. **Validation:** Check `GetMissingObjectNames()` during development

---

## 🔄 System Integration

### With Day/Night Cycle
- NPCs automatically spawn/despawn based on `availableSpawnTimes`
- Schedule events trigger on hour changes
- `spawnHour` determines when NPC becomes available

### With Dialogue System
- NPCs automatically enter interaction state when player is in range
- Schedule data can include dialogue arrays
- Interaction bubbles show current behavior

### With Save/Load System
- NPCs restore to their schedule-defined home positions
- No manual position saving needed
- NPC states preserved through save data

---

**Last Updated:** July 20, 2025  
**System Version:** Tag-Based NPCScheduleData with NPCManager  
**Unity Version:** 6000.0.36f1