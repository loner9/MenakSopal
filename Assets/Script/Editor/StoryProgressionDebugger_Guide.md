# Story Progression Debugger - Usage Guide

## Overview
The Story Progression Debugger is a Unity Editor tool designed to help test and verify the story flow in the MenakSopal game. It allows developers to set story flags for different chapters/phases and monitor quest progression.

## How to Access
1. In Unity Editor, go to **Tools → Trenggalek Game → Story Progression Debugger**
2. The debugger window will open showing all story flags organized by phases

## Key Features

### Phase-Based Testing
- **Quick Phase Setups**: Buttons to instantly set the game to specific story phases
- **Phase 1**: Discovery phase (water crisis discovered, commitment made)
- **Phase 2**: Planning and construction phase 
- **Phase 3**: Supernatural opposition phase
- **Phase 4**: Sacred quest phase (journey to find white elephant)
- **Phase 5**: Sacrifice phase
- **Phase 6**: Reckoning phase (truth exposed, pursuit)
- **Final Phase**: Truth, reconciliation, and legacy

### Flag Management
- **Individual Flag Control**: Toggle any story flag on/off
- **Search and Filter**: Find specific flags by name, description, or category
- **Dependency Tracking**: Flags show their dependencies and what they unlock
- **Category Color-Coding**: Visual organization by flag categories:
  - Green: Core Progression
  - Blue: Player Choice  
  - Orange: Story Milestone
  - Purple: Story Revelation
  - etc.

### Quest Status Monitoring
- **Active Quests**: See currently running quests
- **Completed Quests**: Track finished quests
- **Failed Quests**: Monitor any failed quest attempts
- **Real-time Updates**: Quest status updates as flags change

## Best Practices for Testing

### 1. Story Flow Verification
```
1. Start with "Reset All Flags"
2. Use "Set to Phase X" buttons to jump to different story points
3. Verify that NPCs react appropriately
4. Check that quests become available/complete as expected
5. Test dialogue options match the current story state
```

### 2. Dependency Testing
```
1. Enable a flag that has dependencies
2. Check if dependent content becomes available
3. Disable prerequisite flags to test error handling
4. Ensure story doesn't break with missing dependencies
```

### 3. Choice Consequence Testing
```
1. Set flags for different player choices
2. Test both paths (e.g., "committed_to_help" vs "avoided_responsibility")
3. Verify exclusive choices work correctly
4. Check long-term consequences of choices
```

### 4. Phase Transition Testing
```
1. Set up a phase ending (e.g., Phase 2 completion)
2. Trigger the transition to next phase
3. Verify all flags transfer correctly
4. Check that previous phase content becomes unavailable if appropriate
```

## Usage in Different Scenarios

### Scenario 1: Testing New Dialogue
```
1. Set flags to the appropriate story phase
2. Enable any specific choice flags needed
3. Test the dialogue in play mode
4. Verify flag changes after dialogue completion
```

### Scenario 2: Quest Implementation Testing
```
1. Set prerequisite flags for the quest
2. Verify quest becomes available
3. Test quest progression and completion
4. Check that completion flags are set correctly
```

### Scenario 3: Story Branching Verification
```
1. Test both paths of a major choice
2. Use debugger to set up each scenario
3. Play through consequences
4. Reset and test the alternative path
```

## Important Notes

### System Requirements
- **Play Mode**: Full functionality requires entering Play Mode
- **Scene Systems**: NPCInteractionSystem and QuestManager must exist in scene
- **Flag Persistence**: Flags are reset when exiting Play Mode

### Limitations
- Changes made in debugger don't persist between sessions
- Some quest states may require manual triggering beyond flag setting
- Complex quest chains may need step-by-step progression

### Troubleshooting
- **"Systems not found"**: Enter Play Mode and ensure scene has required components
- **Flags not updating**: Click "Refresh Systems" button
- **Quest status not showing**: Verify QuestManager exists and has quest data loaded

## Integration with Game Systems

The debugger integrates with:
- **NPCInteractionSystem**: For flag management and dialogue state
- **QuestManager**: For quest status and progression
- **StoryFlagManager**: For story flag definitions and validation

This tool is essential for QA testing, story verification, and ensuring the narrative experience is delivered correctly at all story progression points.