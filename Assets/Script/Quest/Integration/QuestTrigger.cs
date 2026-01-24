using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    [Header("Quest Actions")]
    [Tooltip("Quest to start when triggered")]
    public string questToStart;
    [Tooltip("Quest to complete when triggered")]
    public string questToComplete;
    [Tooltip("Quest to fail when triggered")]
    public string questToFail;

    [Header("Objective Actions")]
    [Tooltip("Objective to complete when triggered")]
    public string objectiveToComplete;
    [Tooltip("Quest containing the objective to complete")]
    public string questForObjective;
    [Tooltip("Amount to add to objective progress")]
    public int progressAmount = 1;

    [Header("Trigger Settings")]
    [Tooltip("How this trigger is activated")]
    public TriggerType triggerType = TriggerType.OnTriggerEnter;
    [Tooltip("Tag required for triggering (empty = any object can trigger)")]
    public string requiredTag = "Player";
    [Tooltip("Can this trigger be activated multiple times?")]
    public bool isRepeatable = false;
    [Tooltip("Destroy this trigger after activation?")]
    public bool destroyAfterTrigger = false;

    [Header("Conditional Triggering")]
    [Tooltip("Flags required for this trigger to activate")]
    public string[] requiredFlags;
    [Tooltip("Flags that prevent this trigger from activating")]
    public string[] blockingFlags;

    [Header("Feedback")]
    [Tooltip("Show debug messages when triggered")]
    public bool showDebugMessages = true;

    // Events
    public System.Action OnQuestTriggered;

    private bool hasTriggered = false;
    private QuestManager questManager;

    public enum TriggerType
    {
        OnTriggerEnter,
        OnTriggerStay,
        OnCollisionEnter,
        Manual
    }

    private void Start()
    {
        questManager = QuestManager.Instance;

        if (questManager == null && showDebugMessages)
        {
            Debug.LogWarning($"QuestTrigger on {gameObject.name}: QuestManager not found!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerType == TriggerType.OnTriggerEnter)
        {
            CheckAndExecuteTrigger(other.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (triggerType == TriggerType.OnTriggerStay)
        {
            CheckAndExecuteTrigger(other.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (triggerType == TriggerType.OnCollisionEnter)
        {
            CheckAndExecuteTrigger(collision.gameObject);
        }
    }

    public void ManualTrigger()
    {
        if (triggerType == TriggerType.Manual)
        {
            CheckAndExecuteTrigger(null);
        }
    }

    public void ManualTrigger(GameObject triggeringObject)
    {
        CheckAndExecuteTrigger(triggeringObject);
    }

    private void CheckAndExecuteTrigger(GameObject triggeringObject)
    {
        // Check if already triggered and not repeatable
        if (hasTriggered && !isRepeatable)
            return;

        // Check required tag
        if (!string.IsNullOrEmpty(requiredTag) && triggeringObject != null)
        {
            if (!triggeringObject.CompareTag(requiredTag))
                return;
        }

        // Check quest manager availability
        if (questManager == null)
        {
            questManager = QuestManager.Instance;
            if (questManager == null)
                return;
        }

        // Check conditional flags
        if (!CheckTriggerConditions())
            return;

        // Execute the trigger
        ExecuteTrigger();
    }

    private bool CheckTriggerConditions()
    {
        var interactionSystem = FindObjectOfType<NPCInteractionSystem>();
        if (interactionSystem == null) return true;

        var gameFlags = interactionSystem.GetGameFlags();

        // Check required flags
        if (requiredFlags != null && requiredFlags.Length > 0)
        {
            foreach (string flag in requiredFlags)
            {
                if (!gameFlags.Contains(flag))
                {
                    if (showDebugMessages)
                        Debug.Log($"QuestTrigger: Missing required flag '{flag}'");
                    return false;
                }
            }
        }

        // Check blocking flags
        if (blockingFlags != null && blockingFlags.Length > 0)
        {
            foreach (string flag in blockingFlags)
            {
                if (gameFlags.Contains(flag))
                {
                    if (showDebugMessages)
                        Debug.Log($"QuestTrigger: Blocked by flag '{flag}'");
                    return false;
                }
            }
        }

        return true;
    }

    private void ExecuteTrigger()
    {
        hasTriggered = true;
        bool actionPerformed = false;

        // Start quest
        if (!string.IsNullOrEmpty(questToStart))
        {
            bool started = questManager.StartQuest(questToStart);
            if (started)
            {
                actionPerformed = true;
                if (showDebugMessages)
                    Debug.Log($"QuestTrigger: Started quest '{questToStart}'");
            }
        }

        // Complete quest
        if (!string.IsNullOrEmpty(questToComplete))
        {
            bool completed = questManager.CompleteQuest(questToComplete);
            if (completed)
            {
                actionPerformed = true;
                if (showDebugMessages)
                    Debug.Log($"QuestTrigger: Completed quest '{questToComplete}'");
            }
        }

        // Fail quest
        if (!string.IsNullOrEmpty(questToFail))
        {
            bool failed = questManager.FailQuest(questToFail);
            if (failed)
            {
                actionPerformed = true;
                if (showDebugMessages)
                    Debug.Log($"QuestTrigger: Failed quest '{questToFail}'");
            }
        }

        // Complete objective or update progress
        if (!string.IsNullOrEmpty(objectiveToComplete))
        {
            string questID = !string.IsNullOrEmpty(questForObjective) ?
                questForObjective : questToStart;

            if (!string.IsNullOrEmpty(questID))
            {
                // Always use UpdateObjectiveProgress - it handles both incrementing and auto-completing
                bool updated = questManager.UpdateObjectiveProgress(questID, objectiveToComplete, progressAmount);
                if (updated)
                {
                    actionPerformed = true;
                    if (showDebugMessages)
                        Debug.Log($"QuestTrigger: Updated objective '{objectiveToComplete}' progress by {progressAmount} in quest '{questID}'");
                }
            }
        }

        // Invoke event
        if (actionPerformed)
        {
            OnQuestTriggered?.Invoke();
        }

        // Destroy trigger if specified
        if (destroyAfterTrigger)
        {
            Destroy(gameObject);
        }

        // Reset triggered state if repeatable
        if (isRepeatable)
        {
            hasTriggered = false;
        }
    }

    #region Public Methods

    public void SetQuestToStart(string questID)
    {
        questToStart = questID;
    }

    public void SetQuestToComplete(string questID)
    {
        questToComplete = questID;
    }

    public void SetObjectiveToComplete(string questID, string objectiveID)
    {
        questForObjective = questID;
        objectiveToComplete = objectiveID;
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    public bool HasTriggered()
    {
        return hasTriggered;
    }

    #endregion

    #region Editor Helpers

    private void OnDrawGizmosSelected()
    {
        // Draw trigger area for colliders
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.yellow;
            if (col is BoxCollider2D box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.offset, box.size);
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(circle.offset, circle.radius);
            }
        }

        // Draw quest info
#if UNITY_EDITOR
        Vector3 labelPos = transform.position + Vector3.up * 1f;
        string label = "Quest Trigger";

        if (!string.IsNullOrEmpty(questToStart))
            label += $"\nStart: {questToStart}";
        if (!string.IsNullOrEmpty(questToComplete))
            label += $"\nComplete: {questToComplete}";
        if (!string.IsNullOrEmpty(objectiveToComplete))
            label += $"\nObjective: {objectiveToComplete}";

        UnityEditor.Handles.Label(labelPos, label);
#endif
    }

    #endregion
}