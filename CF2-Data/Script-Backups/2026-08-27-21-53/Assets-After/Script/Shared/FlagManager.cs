using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized flag management system with events for loose coupling.
/// Replaces scattered flag management across NPCInteractionSystem and QuestManager.
/// </summary>
public class FlagManager : MonoBehaviour
{
    public static FlagManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool logFlagChanges = true;

    // Core flag storage
    private HashSet<string> flags = new HashSet<string>();

    // Events for external subscription
    public static event Action<string> OnFlagAdded;
    public static event Action<string> OnFlagRemoved;
    public static event Action<List<string>> OnFlagsLoaded;
    public static event Action OnFlagsCleared;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.F1))
            FlagManager.Instance?.AddFlag("DEBUG_FLAG_1");
        if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.F2))
            Debug.Log($"Has flag: {FlagManager.Instance?.HasFlag("DEBUG_FLAG_1")}");
        if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.F3))
            FlagManager.Instance?.RemoveFlag("DEBUG_FLAG_1");
    }

    #region Public API

    /// <summary>
    /// Add a flag to the system. Does nothing if flag already exists.
    /// </summary>
    /// <param name="flag">The flag identifier to add.</param>
    /// <returns>True if flag was added, false if it already existed.</returns>
    public bool AddFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return false;

        if (flags.Add(flag))
        {
            if (logFlagChanges)
            {
                Debug.Log($"[FlagManager] Flag added: {flag}");
            }
            OnFlagAdded?.Invoke(flag);
            FlagEvents.RaiseFlagAdded(flag);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Add multiple flags at once.
    /// </summary>
    /// <param name="flagsToAdd">Collection of flags to add.</param>
    public void AddFlags(IEnumerable<string> flagsToAdd)
    {
        if (flagsToAdd == null) return;

        foreach (string flag in flagsToAdd)
        {
            AddFlag(flag);
        }
    }

    /// <summary>
    /// Remove a flag from the system.
    /// </summary>
    /// <param name="flag">The flag identifier to remove.</param>
    /// <returns>True if flag was removed, false if it didn't exist.</returns>
    public bool RemoveFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return false;

        if (flags.Remove(flag))
        {
            if (logFlagChanges)
            {
                Debug.Log($"[FlagManager] Flag removed: {flag}");
            }
            OnFlagRemoved?.Invoke(flag);
            FlagEvents.RaiseFlagRemoved(flag);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Remove multiple flags at once.
    /// </summary>
    /// <param name="flagsToRemove">Collection of flags to remove.</param>
    public void RemoveFlags(IEnumerable<string> flagsToRemove)
    {
        if (flagsToRemove == null) return;

        foreach (string flag in flagsToRemove)
        {
            RemoveFlag(flag);
        }
    }

    /// <summary>
    /// Check if a flag exists in the system.
    /// </summary>
    /// <param name="flag">The flag identifier to check.</param>
    /// <returns>True if flag exists, false otherwise.</returns>
    public bool HasFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return false;
        return flags.Contains(flag);
    }

    /// <summary>
    /// Check if all specified flags exist.
    /// </summary>
    /// <param name="requiredFlags">Collection of flags that must all exist.</param>
    /// <returns>True if all flags exist, false otherwise.</returns>
    public bool HasAllFlags(IEnumerable<string> requiredFlags)
    {
        if (requiredFlags == null) return true;

        foreach (string flag in requiredFlags)
        {
            if (!HasFlag(flag)) return false;
        }
        return true;
    }

    /// <summary>
    /// Check if any of the specified flags exist.
    /// </summary>
    /// <param name="flagsToCheck">Collection of flags to check.</param>
    /// <returns>True if at least one flag exists, false otherwise.</returns>
    public bool HasAnyFlag(IEnumerable<string> flagsToCheck)
    {
        if (flagsToCheck == null) return false;

        foreach (string flag in flagsToCheck)
        {
            if (HasFlag(flag)) return true;
        }
        return false;
    }

    /// <summary>
    /// Check if none of the specified flags exist.
    /// </summary>
    /// <param name="blockedFlags">Collection of flags that must not exist.</param>
    /// <returns>True if none of the flags exist, false otherwise.</returns>
    public bool HasNoFlags(IEnumerable<string> blockedFlags)
    {
        return !HasAnyFlag(blockedFlags);
    }

    /// <summary>
    /// Get all current flags as a list.
    /// </summary>
    /// <returns>A new list containing all current flags.</returns>
    public List<string> GetAllFlags()
    {
        return new List<string>(flags);
    }

    /// <summary>
    /// Get the total count of flags.
    /// </summary>
    public int FlagCount => flags.Count;

    /// <summary>
    /// Clear all flags from the system.
    /// </summary>
    public void ClearAllFlags()
    {
        flags.Clear();
        if (logFlagChanges)
        {
            Debug.Log("[FlagManager] All flags cleared");
        }
        OnFlagsCleared?.Invoke();
        FlagEvents.RaiseFlagsCleared();
    }

    /// <summary>
    /// Get flags that match a prefix (e.g., "QUEST_" to get all quest flags).
    /// </summary>
    /// <param name="prefix">The prefix to search for.</param>
    /// <returns>List of flags matching the prefix.</returns>
    public List<string> GetFlagsWithPrefix(string prefix)
    {
        List<string> result = new List<string>();
        foreach (string flag in flags)
        {
            if (flag.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(flag);
            }
        }
        return result;
    }

    #endregion

    #region Save/Load

    /// <summary>
    /// Serializable data class for saving/loading flags.
    /// </summary>
    [Serializable]
    public class FlagSaveData
    {
        public List<string> flags = new List<string>();
    }

    /// <summary>
    /// Get save data for the flag system.
    /// </summary>
    /// <returns>Serializable save data.</returns>
    public FlagSaveData GetSaveData()
    {
        return new FlagSaveData
        {
            flags = new List<string>(flags)
        };
    }

    /// <summary>
    /// Load flags from save data.
    /// </summary>
    /// <param name="data">The save data to load from.</param>
    public void LoadSaveData(FlagSaveData data)
    {
        if (data == null || data.flags == null) return;

        flags.Clear();
        foreach (string flag in data.flags)
        {
            flags.Add(flag);
        }

        if (logFlagChanges)
        {
            Debug.Log($"[FlagManager] Loaded {flags.Count} flags");
        }

        OnFlagsLoaded?.Invoke(new List<string>(flags));
        FlagEvents.RaiseFlagsLoaded(new List<string>(flags));
    }

    /// <summary>
    /// Merge flags from another source (e.g., legacy save data).
    /// Adds flags without removing existing ones.
    /// </summary>
    /// <param name="legacyFlags">Legacy flag list to merge.</param>
    public void MergeFlags(List<string> legacyFlags)
    {
        if (legacyFlags == null) return;

        foreach (string flag in legacyFlags)
        {
            flags.Add(flag);
        }

        if (logFlagChanges)
        {
            Debug.Log($"[FlagManager] Merged {legacyFlags.Count} legacy flags");
        }
    }

    #endregion

    #region Compatibility Layer

    /// <summary>
    /// Static method for compatibility with existing code using NPCInteractionSystem pattern.
    /// </summary>
    public static bool HasGameFlag(string flag)
    {
        return Instance != null && Instance.HasFlag(flag);
    }

    /// <summary>
    /// Static method for compatibility with existing code.
    /// </summary>
    public static void AddGameFlag(string flag)
    {
        if (Instance != null)
        {
            Instance.AddFlag(flag);
        }
        else
        {
            Debug.LogWarning($"[FlagManager] Cannot add flag '{flag}' - no instance exists");
        }
    }

    /// <summary>
    /// Static method for compatibility with existing code.
    /// </summary>
    public static void RemoveGameFlag(string flag)
    {
        if (Instance != null)
        {
            Instance.RemoveFlag(flag);
        }
    }

    /// <summary>
    /// Static method to get all flags for compatibility.
    /// </summary>
    public static List<string> GetGameFlags()
    {
        return Instance != null ? Instance.GetAllFlags() : new List<string>();
    }

    #endregion
}
