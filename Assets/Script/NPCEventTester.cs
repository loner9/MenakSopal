using UnityEngine;

public class NPCEventTester : MonoBehaviour
{
    void OnEnable()
    {
        NPCEvents.OnNPCSpawned += OnSpawned;
        NPCEvents.OnNPCDespawned += OnDespawned;
        NPCEvents.OnNPCStateChanged += OnStateChanged;
        NPCEvents.OnNPCInteractionStarted += OnInteraction;
        NPCEvents.OnPlayerEnteredRange += OnPlayerEntered;
        NPCEvents.OnNPCReachedDestination += OnReached;
    }
    
    void OnDisable()
    {
        NPCEvents.OnNPCSpawned -= OnSpawned;
        NPCEvents.OnNPCDespawned -= OnDespawned;
        NPCEvents.OnNPCStateChanged -= OnStateChanged;
        NPCEvents.OnNPCInteractionStarted -= OnInteraction;
        NPCEvents.OnPlayerEnteredRange -= OnPlayerEntered;
        NPCEvents.OnNPCReachedDestination -= OnReached;
    }
    
    void OnSpawned(NPC n) => Debug.Log($"[NPC EVENT] Spawned: {n.npcName}");
    void OnDespawned(NPC n) => Debug.Log($"[NPC EVENT] Despawned: {n.npcName}");
    void OnStateChanged(NPC n, string old, string nw) => Debug.Log($"[NPC EVENT] State: {n.npcName} {old}->{nw}");
    void OnInteraction(NPC n) => Debug.Log($"[NPC EVENT] Interaction: {n.npcName}");
    void OnPlayerEntered(NPC n) => Debug.Log($"[NPC EVENT] Player near: {n.npcName}");
    void OnReached(NPC n) => Debug.Log($"[NPC EVENT] Reached dest: {n.npcName}");
}
