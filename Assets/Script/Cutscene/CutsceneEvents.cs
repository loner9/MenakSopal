using System;

/// <summary>
/// Static event hub for the Cutscene system.
/// Subscribe to these events to react to cutscene state changes.
/// </summary>
public static class CutsceneEvents
{
    /// <summary>Fired when a cutscene starts playing</summary>
    public static event Action<CutsceneData> OnCutsceneStarted;

    /// <summary>Fired when a cutscene completes normally</summary>
    public static event Action<CutsceneData> OnCutsceneCompleted;

    /// <summary>Fired when a cutscene is skipped by player</summary>
    public static event Action<CutsceneData> OnCutsceneSkipped;

    /// <summary>Fired when a cutscene step begins</summary>
    public static event Action<CutsceneData, CutsceneStep, int> OnStepStarted;

    /// <summary>Fired when a cutscene step completes</summary>
    public static event Action<CutsceneData, CutsceneStep, int> OnStepCompleted;

    // Invoke methods
    public static void InvokeCutsceneStarted(CutsceneData cutscene)
        => OnCutsceneStarted?.Invoke(cutscene);

    public static void InvokeCutsceneCompleted(CutsceneData cutscene)
        => OnCutsceneCompleted?.Invoke(cutscene);

    public static void InvokeCutsceneSkipped(CutsceneData cutscene)
        => OnCutsceneSkipped?.Invoke(cutscene);

    public static void InvokeStepStarted(CutsceneData cutscene, CutsceneStep step, int index)
        => OnStepStarted?.Invoke(cutscene, step, index);

    public static void InvokeStepCompleted(CutsceneData cutscene, CutsceneStep step, int index)
        => OnStepCompleted?.Invoke(cutscene, step, index);

    public static void ClearAllListeners()
    {
        OnCutsceneStarted = null;
        OnCutsceneCompleted = null;
        OnCutsceneSkipped = null;
        OnStepStarted = null;
        OnStepCompleted = null;
    }
}
