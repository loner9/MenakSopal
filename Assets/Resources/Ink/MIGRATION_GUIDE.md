# DialogueData to Ink Migration Guide

## Overview

This guide documents how to convert MenakSopal's custom `DialogueData` ScriptableObjects to Ink format.

## File Locations

- **Original Assets**: `Assets/Resources/Dialogues/`
- **Ink Files**: `Assets/Resources/Ink/Story/`

## Conversion Mapping

### DialogueData Fields → Ink Equivalents

| DialogueData Field | Ink Equivalent |
|-------------------|----------------|
| `npcName` | Comment at top of file |
| `speakerName` | `# speaker: Name` tag |
| `dialogueText` | Plain text line |
| `hasChoices` | `+` choice syntax |
| `choices[].choiceText` | `+ [Choice text]` |
| `choices[].response` | Lines after choice |
| `requiredFlags` | Ink conditional `{flag:}` |
| `flagsToAdd[]` | `~ addFlag("name")` |
| `flagsToRemove[]` | `~ removeFlag("name")` |
| `targetDialogueIndex` | `-> knot_name` |
| `availableTimesOfDay` | `{time_of_day == X:}` |
| `isImportantDialogue` | `# important` tag |
| `pauseAfterDialogue` | `# pause: X` tag |
| `questToStart` | `~ startQuest("id")` |
| `objectiveToComplete` | `~ completeObjective("questId", "objId")` |
| `greetings[]` | `=== greetings ===` knot |
| `farewells[]` | `=== farewell ===` knot |

## Ink Structure Template

```ink
// NPC Name - Description
// Migrated from: OriginalFile.asset

// ============================================
// VARIABLES (synced with game flag system)
// ============================================
VAR flag_name = false
VAR time_of_day = 0

// External functions for game integration
EXTERNAL startQuest(questId)
EXTERNAL completeObjective(questId, objectiveId)
EXTERNAL addFlag(flagName)
EXTERNAL removeFlag(flagName)

// ============================================
// MAIN ENTRY POINT
// ============================================
=== main ===
-> greetings

=== greetings ===
{time_of_day:
    - 0: # speaker: NPC Name
        Morning greeting
    - 1: # speaker: NPC Name
        Day greeting
    - else: ...
}
-> story_dialogue

=== story_dialogue ===
// Priority-based conditions for story progression
{story_completed:
    -> ending_dialogue
}
{quest_active:
    -> quest_dialogue  
}
-> generic_dialogue

=== quest_dialogue ===
# speaker: NPC Name
Quest-related dialogue text

+ [Choice 1]
    # speaker: Player
    Player response
    ~ addFlag("chose_option_1")
    -> next_knot

+ [Choice 2]
    -> other_knot

=== generic_dialogue ===
# speaker: NPC Name
Default dialogue when no special conditions
-> DONE
```

## Dialogue Tags

Custom tags parsed by `InkDialogueRenderer`:

| Tag | Purpose | Example |
|-----|---------|---------|
| `# speaker: Name` | Set current speaker | `# speaker: Ki Ageng` |
| `# important` | Important dialogue flag | `# important` |
| `# pause: X` | Pause X seconds | `# pause: 1.5` |
| `# bubble: type` | Show bubble sprite | `# bubble: happy` |
| `# audio: clip` | Play audio clip | `# audio: greeting_sfx` |

## Migrated Files

| Original Asset | Ink File | Status |
|---------------|----------|--------|
| KiAgengSinawang_ID.asset | KiAgengSinawang_ID.ink | ✅ Done |
| AndiStudent_ID.asset | AndiStudent_ID.ink | ✅ Done |
| Jono.asset | Jono_ID.ink | ✅ Done |
| New Dialogue.asset | TestNPC_ID.ink | ✅ Done |
| ... | ... | Pending |

## Next Steps

1. Install `ink-unity-integration` package
2. Create `InkStoryManager.cs` to load stories
3. Create `InkFlagBridge.cs` to sync flags
4. Update `NPCInteractionSystem.cs` to use Ink
5. Convert remaining dialogue assets
