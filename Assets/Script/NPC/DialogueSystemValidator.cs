using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Validation script to ensure choice-based dialogue system maintains backward compatibility
/// </summary>
public class DialogueSystemValidator : MonoBehaviour
{
    [Header("Validation Tests")]
    [SerializeField] private bool runTestsOnStart = false;
    
    private void Start()
    {
        if (runTestsOnStart)
        {
            RunAllValidationTests();
        }
    }
    
    [ContextMenu("Run All Validation Tests")]
    public void RunAllValidationTests()
    {
        Debug.Log("=== Dialogue System Validation Tests ===");
        
        TestBasicDialogueCompatibility();
        TestChoiceDialogueCreation();
        TestFlagSystem();
        TestChoiceAvailability();
        TestNavigation();
        
        Debug.Log("=== Validation Tests Complete ===");
    }
    
    private void TestBasicDialogueCompatibility()
    {
        Debug.Log("Testing backward compatibility with existing dialogues...");
        
        // Create a traditional dialogue (no choices)
        var dialogue = ScriptableObject.CreateInstance<DialogueData>();
        dialogue.dialogueEntries = new DialogueEntry[]
        {
            new DialogueEntry
            {
                speakerName = "Test NPC",
                dialogueText = "Hello!",
                hasChoices = false // Traditional dialogue
            }
        };
        
        var entry = dialogue.dialogueEntries[0];
        
        // Test that traditional dialogues still work
        Assert(entry.hasChoices == false, "Traditional dialogue should not have choices");
        Assert(entry.choices == null || entry.choices.Length == 0, "Traditional dialogue should have no choices array");
        
        Debug.Log("✓ Backward compatibility test passed");
    }
    
    private void TestChoiceDialogueCreation()
    {
        Debug.Log("Testing choice dialogue creation...");
        
        var dialogue = ScriptableObject.CreateInstance<DialogueData>();
        var choiceEntry = new DialogueEntry
        {
            speakerName = "Choice NPC",
            dialogueText = "What would you like?",
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Option 1",
                    targetDialogueIndex = 1,
                    flagsToAdd = new string[] { "TEST_FLAG" }
                }
            }
        };
        
        dialogue.dialogueEntries = new DialogueEntry[] { choiceEntry };
        
        Assert(choiceEntry.hasChoices == true, "Choice dialogue should have choices enabled");
        Assert(choiceEntry.choices.Length > 0, "Choice dialogue should have choices array");
        
        var gameFlags = new List<string>();
        var choices = dialogue.GetAvailableChoices(choiceEntry, TimeOfDay.Day, gameFlags);
        Assert(choices.Length > 0, "Should return available choices");
        
        Debug.Log("✓ Choice dialogue creation test passed");
    }
    
    private void TestFlagSystem()
    {
        Debug.Log("Testing flag-based choice availability...");
        
        var choice = new DialogueChoice
        {
            choiceText = "Restricted Choice",
            requiredFlags = new string[] { "REQUIRED_FLAG" }
        };
        
        var dialogue = ScriptableObject.CreateInstance<DialogueData>();
        
        // Test without required flag
        var noFlags = new List<string>();
        bool availableWithoutFlag = dialogue.IsChoiceAvailable(choice, TimeOfDay.Day, noFlags);
        
        // Test with required flag
        var withFlags = new List<string> { "REQUIRED_FLAG" };
        bool availableWithFlag = dialogue.IsChoiceAvailable(choice, TimeOfDay.Day, withFlags);
        
        Assert(availableWithoutFlag == false, "Choice should not be available without required flag");
        Assert(availableWithFlag == true, "Choice should be available with required flag");
        
        Debug.Log("✓ Flag system test passed");
    }
    
    private void TestChoiceAvailability()
    {
        Debug.Log("Testing time-based choice availability...");
        
        var dayChoice = new DialogueChoice
        {
            choiceText = "Day Only Choice",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day }
        };
        
        var dialogue = ScriptableObject.CreateInstance<DialogueData>();
        var gameFlags = new List<string>();
        
        bool availableDuringDay = dialogue.IsChoiceAvailable(dayChoice, TimeOfDay.Day, gameFlags);
        bool availableDuringNight = dialogue.IsChoiceAvailable(dayChoice, TimeOfDay.Night, gameFlags);
        
        Assert(availableDuringDay == true, "Day choice should be available during day");
        Assert(availableDuringNight == false, "Day choice should not be available during night");
        
        Debug.Log("✓ Time-based availability test passed");
    }
    
    private void TestNavigation()
    {
        Debug.Log("Testing dialogue navigation...");
        
        var dialogue = ScriptableObject.CreateInstance<DialogueData>();
        dialogue.dialogueEntries = new DialogueEntry[]
        {
            new DialogueEntry { speakerName = "Test", dialogueText = "Entry 0" },
            new DialogueEntry { speakerName = "Test", dialogueText = "Entry 1" },
            new DialogueEntry { speakerName = "Test", dialogueText = "Entry 2" }
        };
        
        // Test getting dialogue by index
        var entry1 = dialogue.GetDialogueEntry(1);
        var invalidEntry = dialogue.GetDialogueEntry(99);
        
        Assert(entry1 != null && entry1.dialogueText == "Entry 1", "Should retrieve correct entry by index");
        Assert(invalidEntry == null, "Should return null for invalid index");
        
        // Test finding entry index
        int index = dialogue.GetDialogueEntryIndex(entry1);
        Assert(index == 1, "Should return correct index for entry");
        
        Debug.Log("✓ Navigation test passed");
    }
    
    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError($"ASSERTION FAILED: {message}");
        }
    }
}