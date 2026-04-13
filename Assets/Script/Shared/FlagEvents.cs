using System;

/// <summary>
/// Static event hub for flag system events.
/// Provides a centralized location for subscribing to flag changes.
/// </summary>
public static class FlagEvents
{
    /// <summary>
    /// Fired when a flag is added to the system.
    /// Parameter: flag name that was added.
    /// </summary>
    public static event Action<string> OnFlagAdded;

    /// <summary>
    /// Fired when a flag is removed from the system.
    /// Parameter: flag name that was removed.
    /// </summary>
    public static event Action<string> OnFlagRemoved;

    /// <summary>
    /// Fired when flags are loaded from a save file.
    /// Parameter: list of all loaded flags.
    /// </summary>
    public static event Action<System.Collections.Generic.List<string>> OnFlagsLoaded;

    /// <summary>
    /// Fired when all flags are cleared.
    /// </summary>
    public static event Action OnFlagsCleared;

    // Public invocation methods
    public static void RaiseFlagAdded(string flag) => OnFlagAdded?.Invoke(flag);
    public static void RaiseFlagRemoved(string flag) => OnFlagRemoved?.Invoke(flag);
    public static void RaiseFlagsLoaded(System.Collections.Generic.List<string> flags) => OnFlagsLoaded?.Invoke(flags);
    public static void RaiseFlagsCleared() => OnFlagsCleared?.Invoke();

    /// <summary>
    /// Clear all event subscribers. Call when reloading scenes or resetting the game.
    /// </summary>
    public static void ClearAllSubscribers()
    {
        OnFlagAdded = null;
        OnFlagRemoved = null;
        OnFlagsLoaded = null;
        OnFlagsCleared = null;
    }
}
