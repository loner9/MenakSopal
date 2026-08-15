using System.Collections.Generic;
using UnityEngine;

namespace MenakSopal.Cutscenes
{
    /// <summary>
    /// Defines a single step in a cutscene sequence.
    /// Each step performs one action (show dialogue, move player, etc.)
    /// </summary>
    [System.Serializable]
    public class CutsceneStep
    {
        public enum StepType
        {
            // Dialogue & Text
            ShowDialogue,           // Show NPC dialogue (uses DialogueData)
            ShowMonologue,          // Show player inner thought
            ShowMessage,            // Show UI message (toast notification)

            // Player Control
            DisablePlayerMovement,  // Lock player in place
            EnablePlayerMovement,   // Unlock player
            TeleportPlayer,         // Instantly move player to location
            MovePlayerTo,           // Smoothly walk player to location (with fade)
            MovePlayerWalk,         // Literally walking player to location (animated)

            // NPC Control
            SpawnNPC,               // Make NPC appear
            DespawnNPC,             // Make NPC disappear
            MoveNPCTo,              // Move NPC to location
            FaceNPCTowards,         // Make NPC face direction/target

            // Camera
            CameraShake,            // Screen shake effect
            CameraFocusOn,          // Move camera to focus on target
            CameraFollowPlayer,     // Return camera to follow player

            // Game State
            SetFlag,                // Add a game flag
            RemoveFlag,             // Remove a game flag
            StartQuest,             // Start a quest
            CompleteQuest,          // Complete a quest
            CompleteObjective,      // Complete quest objective

            // Time & Environment
            SetTimeOfDay,           // Change time (Day, Night, etc.)
            PauseGameTime,          // Stop day/night cycle
            ResumeGameTime,         // Resume day/night cycle

            // Audio
            PlaySound,              // Play sound effect
            PlayMusic,              // Change background music
            StopMusic,              // Stop current music

            // Scene & Area
            EnterSubArea,           // Load sub-area (like SpiritualPlane)
            ExitSubArea,            // Return from sub-area
            FadeToBlack,            // Fade screen to black
            FadeFromBlack,          // Fade screen back in

            // Flow Control
            WaitSeconds,            // Wait for duration
            WaitForDialogueEnd,     // Wait until dialogue finishes
            WaitForInput,           // Wait for player to press button

            // Game Objects
            EnableGameObject,       // Activate a GameObject by tag/name
            DisableGameObject,      // Deactivate a GameObject

            // Custom
            TriggerEvent            // Fire a custom event (for special cases)
        }

        [Header("Step Configuration")]
        public StepType type;

        [Tooltip("Optional name for this step (for debugging)")]
        public string stepName;

        [Header("Target")]
        [Tooltip("Target ID (NPC ID, location name, quest ID, etc.)")]
        public string targetID;

        [Tooltip("Secondary target (e.g., objective ID for quest)")]
        public string secondaryTargetID;

        [Header("Text Content")]
        [TextArea(2, 4)]
        [Tooltip("Text content for monologue/message")]
        public string textContent;

        [Header("Timing")]
        [Tooltip("Duration in seconds (for waits, fades, shakes)")]
        public float duration = 1f;

        [Tooltip("Delay before executing this step")]
        public float delayBefore = 0f;

        [Header("Flags")]
        [Tooltip("Flags to set when this step completes")]
        public string[] flagsToSet;

        [Tooltip("Flags to remove when this step completes")]
        public string[] flagsToRemove;

        [Header("Conditions")]
        [Tooltip("Required flags for this step to execute (skip if not met)")]
        public string[] requiredFlags;

        [Header("Audio")]
        public AudioClip audioClip;

        [Header("Advanced")]
        [Tooltip("Wait for this step to complete before moving to next")]
        public bool waitForCompletion = true;

        [Tooltip("Time of day for SetTimeOfDay step")]
        public TimeOfDay timeOfDay = TimeOfDay.Day;

        [Tooltip("Shake intensity for camera shake")]
        public float shakeIntensity = 1f;

        [Header("Overrides")]
        [Tooltip("Optional: override the default dialogue for this NPC")]
        public DialogueData dialogueOverride;
    }

    public enum CutsceneSaveTiming
    {
        None,
        AtStart,
        AtEnd
    }

    /// <summary>
    /// ScriptableObject that defines a complete cutscene.
    /// Create via: Create > Cutscene System > Cutscene Data
    /// </summary>
    [CreateAssetMenu(fileName = "New Cutscene", menuName = "Cutscene System/Cutscene Data")]
    public class CutsceneData : ScriptableObject
    {
        [Header("Cutscene Info")]
        public string cutsceneID;

        [TextArea(2, 3)]
        public string description;

        [Header("Trigger Conditions")]
        [Tooltip("Flag that triggers this cutscene automatically")]
        public string triggerFlag;

        [Tooltip("Required flags that must exist for cutscene to play")]
        public string[] requiredFlags;

        [Header("Cutscene Steps")]
        public List<CutsceneStep> steps = new List<CutsceneStep>();

        [Header("Settings")]
        [Tooltip("Can this cutscene be skipped?")]
        public bool canSkip = false;

        [Tooltip("Pause game time during cutscene?")]
        public bool pauseGameTime = true;

        [Tooltip("Disable player input during cutscene?")]
        public bool disablePlayerInput = true;

        [Tooltip("Flags to set when cutscene starts")]
        public string[] flagsOnStart;

        [Tooltip("Flags to set when cutscene completes")]
        public string[] flagsOnComplete;

        [Header("Auto-Save Settings")]
        [Tooltip("When should the game automatically save during this cutscene?")]
        public CutsceneSaveTiming autoSaveTiming = CutsceneSaveTiming.None;

        /// <summary>
        /// Check if this cutscene can be triggered based on current game flags
        /// </summary>
        public bool CanTrigger(List<string> currentFlags)
        {
            if (requiredFlags == null || requiredFlags.Length == 0)
                return true;

            foreach (string flag in requiredFlags)
            {
                if (!currentFlags.Contains(flag))
                    return false;
            }
            return true;
        }
    }
}
