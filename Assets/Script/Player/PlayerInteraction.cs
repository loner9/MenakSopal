using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionRange = 2.5f;
    public LayerMask npcLayerMask = -1;
    
    [Header("UI References")]
    public GameObject interactionPrompt;
    public TMPro.TextMeshProUGUI promptText;
    
    private NPCInteractionSystem dialogueSystem;
    private NPC nearbyNPC;
    private Camera mainCamera;
    
    void Start()
    {
        // Find required components
        dialogueSystem = FindObjectOfType<NPCInteractionSystem>();
        mainCamera = Camera.main;
        
        if (dialogueSystem == null)
        {
            Debug.LogError("AdventurePlayerInteraction: No AdventureDialogueSystem found in scene!");
        }
        
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
    
    void Update()
    {
        if (dialogueSystem != null && !dialogueSystem.IsInDialogue())
        {
            CheckForNearbyNPCs();
            
            // Handle interaction input
            if (Input.GetKeyDown(interactKey) && nearbyNPC != null)
            {
                dialogueSystem.StartDialogue(nearbyNPC);
            }
        }
        
        // Update prompt position to follow NPC
        UpdatePromptPosition();
    }
    
    private void CheckForNearbyNPCs()
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, npcLayerMask);
        NPC closestNPC = null;
        float closestDistance = float.MaxValue;
        
        foreach (var collider in nearbyColliders)
        {
            NPC npc = collider.GetComponent<NPC>();
            if (npc != null && npc.canInteract)
            {
                float distance = Vector2.Distance(transform.position, npc.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNPC = npc;
                }
            }
        }
        
        if (closestNPC != nearbyNPC)
        {
            nearbyNPC = closestNPC;
            UpdateInteractionPrompt();
        }
    }
    
    private void UpdateInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            if (nearbyNPC != null)
            {
                interactionPrompt.SetActive(true);
                
                if (promptText != null)
                {
                    promptText.text = $"Press {interactKey} to talk to {nearbyNPC.npcName}";
                }
            }
            else
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    private void UpdatePromptPosition()
    {
        if (nearbyNPC != null && interactionPrompt != null && interactionPrompt.activeInHierarchy)
        {
            // Convert NPC world position to screen position
            Vector3 npcWorldPos = nearbyNPC.transform.position + Vector3.up * 1.5f; // Offset above NPC
            Vector3 screenPos = mainCamera.WorldToScreenPoint(npcWorldPos);
            
            // Update prompt position
            interactionPrompt.transform.position = screenPos;
        }
    }
    
    // Visual debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}