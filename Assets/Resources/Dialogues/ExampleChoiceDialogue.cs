using UnityEngine;

// This is an example script showing how to create choice-based dialogues programmatically
// In practice, you'll create DialogueData assets in the Unity editor
public class ExampleChoiceDialogue : MonoBehaviour
{
    [Header("Example: How to Create Choice-Based Dialogues")]
    [TextArea(5, 10)]
    public string instructions = @"
To create choice-based dialogues:

1. Create a DialogueData asset (Right-click → Create → NPC → Dialogue Data)
2. Add dialogue entries as normal
3. For choice dialogues:
   - Check 'Has Choices' on the DialogueEntry
   - Add choices in the 'Choices' array
   - Set choice text, consequences (flags), and navigation

Choice Navigation Options:
- targetDialogueIndex: Jump to specific dialogue entry (-1 to end)
- response: Show NPC response to choice before continuing
- flagsToAdd/Remove: Modify game state based on choice

Example Flow:
Entry 0: 'Hello there!' (hasChoices = true)
  Choice 1: 'Tell me about the town' → targetDialogueIndex = 1
  Choice 2: 'I need help' → targetDialogueIndex = 2
  Choice 3: 'Goodbye' → targetDialogueIndex = -1 (ends dialogue)

Entry 1: 'This town has a rich history...'
Entry 2: 'What kind of help do you need?'

The system automatically handles:
- Flag-based choice availability
- Time-based choice restrictions  
- Audio feedback and visual styling
- Backward compatibility with existing dialogues
";

    // Example of creating a choice dialogue programmatically
    public DialogueData CreateExampleDialogue()
    {
        var dialogue = ScriptableObject.CreateInstance<DialogueData>();
        dialogue.npcName = "Village Elder";
        
        // Main greeting with choices
        var greeting = new DialogueEntry
        {
            speakerName = "Village Elder",
            dialogueText = "Greetings, traveler! How may I assist you today?",
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Tell me about this village",
                    targetDialogueIndex = 1, // Go to village info dialogue
                    isRepeatable = true
                },
                new DialogueChoice
                {
                    choiceText = "I'm looking for work",
                    targetDialogueIndex = 2, // Go to work dialogue  
                    flagsToAdd = new string[] { "ASKED_ABOUT_WORK" },
                    isRepeatable = false
                },
                new DialogueChoice
                {
                    choiceText = "Farewell",
                    targetDialogueIndex = -1, // End dialogue
                    isRepeatable = true
                }
            }
        };
        
        // Village information dialogue
        var villageInfo = new DialogueEntry
        {
            speakerName = "Village Elder",
            dialogueText = "This village has stood for over 200 years. We're known for our skilled craftsmen and peaceful nature.",
            hasChoices = false
        };
        
        // Work dialogue
        var workInfo = new DialogueEntry
        {
            speakerName = "Village Elder", 
            dialogueText = "There's always work to be done! Check with the blacksmith or visit the merchant quarter.",
            hasChoices = false
        };
        
        dialogue.dialogueEntries = new DialogueEntry[] { greeting, villageInfo, workInfo };
        
        return dialogue;
    }
}