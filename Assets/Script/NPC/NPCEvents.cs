using System;
using UnityEngine;

/// <summary>
/// Static event hub for NPC system events.
/// Subscribe to these events for loose coupling between systems.
/// </summary>
public static class NPCEvents
{
    #region Spawn & Lifecycle Events

    /// <summary>
    /// Fired when a new NPC spawns.
    /// Parameters: NPC instance
    /// </summary>
    public static event Action<NPC> OnNPCSpawned;

    /// <summary>
    /// Fired when an NPC is despawned/destroyed.
    /// Parameters: NPC instance
    /// </summary>
    public static event Action<NPC> OnNPCDespawned;

    #endregion

    #region State Events

    /// <summary>
    /// Fired when an NPC changes state (idle, move, interaction, etc.)
    /// Parameters: NPC instance, old state name, new state name
    /// </summary>
    public static event Action<NPC, string, string> OnNPCStateChanged;

    #endregion

    #region Interaction Events

    /// <summary>
    /// Fired when player starts interaction with an NPC.
    /// Parameters: NPC instance
    /// </summary>
    public static event Action<NPC> OnNPCInteractionStarted;

    /// <summary>
    /// Fired when player ends interaction with an NPC.
    /// Parameters: NPC instance
    /// </summary>
    public static event Action<NPC> OnNPCInteractionEnded;

    /// <summary>
    /// Fired when player enters NPC's interaction range.
    /// Parameters: NPC instance
    /// </summary>
    public static event Action<NPC> OnPlayerEnteredRange;

    /// <summary>
    /// Fired when player leaves NPC's interaction range.
    /// Parameters: NPC instance
    /// </summary>
    public static event Action<NPC> OnPlayerLeftRange;

    #endregion

    #region Movement Events

    /// <summary>
    /// Fired when NPC receives a move command.
    /// Parameters: NPC instance, destination
    /// </summary>
    public static event Action<NPC, Vector2> OnNPCMovementStarted;

    /// <summary>
    /// Fired when NPC reaches its destination.
    /// Parameters: NPC instance
    /// </summary>
    public static event Action<NPC> OnNPCReachedDestination;

    #endregion

    #region Schedule Events

    /// <summary>
    /// Fired when NPC receives a schedule command.
    /// Parameters: NPC instance, command type
    /// </summary>
    public static event Action<NPC, ScheduleCommandType> OnNPCScheduleCommandReceived;

    #endregion

    #region Internal Raise Methods

    public static void RaiseNPCSpawned(NPC npc)
    {
        OnNPCSpawned?.Invoke(npc);
    }

    public static void RaiseNPCDespawned(NPC npc)
    {
        OnNPCDespawned?.Invoke(npc);
    }

    public static void RaiseNPCStateChanged(NPC npc, string oldState, string newState)
    {
        OnNPCStateChanged?.Invoke(npc, oldState, newState);
    }

    public static void RaiseNPCInteractionStarted(NPC npc)
    {
        OnNPCInteractionStarted?.Invoke(npc);
    }

    public static void RaiseNPCInteractionEnded(NPC npc)
    {
        OnNPCInteractionEnded?.Invoke(npc);
    }

    public static void RaisePlayerEnteredRange(NPC npc)
    {
        OnPlayerEnteredRange?.Invoke(npc);
    }

    public static void RaisePlayerLeftRange(NPC npc)
    {
        OnPlayerLeftRange?.Invoke(npc);
    }

    public static void RaiseNPCMovementStarted(NPC npc, Vector2 destination)
    {
        OnNPCMovementStarted?.Invoke(npc, destination);
    }

    public static void RaiseNPCReachedDestination(NPC npc)
    {
        OnNPCReachedDestination?.Invoke(npc);
    }

    public static void RaiseNPCScheduleCommandReceived(NPC npc, ScheduleCommandType commandType)
    {
        OnNPCScheduleCommandReceived?.Invoke(npc, commandType);
    }

    #endregion

    #region Utility

    /// <summary>
    /// Clear all event subscribers. Call when reloading scenes.
    /// </summary>
    public static void ClearAllSubscribers()
    {
        OnNPCSpawned = null;
        OnNPCDespawned = null;
        OnNPCStateChanged = null;
        OnNPCInteractionStarted = null;
        OnNPCInteractionEnded = null;
        OnPlayerEnteredRange = null;
        OnPlayerLeftRange = null;
        OnNPCMovementStarted = null;
        OnNPCReachedDestination = null;
        OnNPCScheduleCommandReceived = null;
    }

    #endregion
}
