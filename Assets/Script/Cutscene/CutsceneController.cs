using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MenakSopal.Cutscenes
{
    /// <summary>
    /// Main controller for playing cutscenes.
    /// Handles step execution, timing, and integration with game systems.
    /// </summary>
    public class CutsceneController : MonoBehaviour
    {
        public static CutsceneController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private Image fadeImage;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Settings")]
        [SerializeField] private float defaultFadeDuration = 0.5f;
        [SerializeField] private KeyCode skipKey = KeyCode.Escape;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        // State
        private CutsceneData currentCutscene;
        private int currentStepIndex;
        private bool isPlaying;
        private bool isWaitingForInput;
        private Coroutine playbackCoroutine;
        private Coroutine fadeCoroutine;   // tracked so cleanup can kill orphaned fades
        private Queue<CutsceneData> cutsceneQueue = new Queue<CutsceneData>();

        // Cached references
        private NPCInteractionSystem interactionSystem;
        private MovePlayerTo movePlayer;
        private MonologueSystem monologueSystem;
        private DayNightCycle dayNightCycle;
        private CinematicUI cinematicUI;
        private QuestManager questManager;
        private GameSystemsManager gameSystemsManager;

        // Properties
        public bool IsPlaying => isPlaying;
        public CutsceneData CurrentCutscene => currentCutscene;
        public int CurrentStepIndex => currentStepIndex;
        /// <summary>Current alpha of the fade canvas (0=transparent, 1=black).</summary>
        public float FadeCanvasAlpha => fadeCanvasGroup != null ? fadeCanvasGroup.alpha : 0f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            CacheReferences();
        }

        void Start()
        {
            // Ensure fade starts transparent
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0;
                fadeCanvasGroup.blocksRaycasts = false;
            }
        }

        private float _lastKnownAlpha = -1f;
        private bool _isFading = false;  // suppress monitor during our own fades

        void Update()
        {
            // ── Alpha change monitor ────────────────────────────────────────
            if (fadeCanvasGroup != null && !_isFading)
            {
                float cur = fadeCanvasGroup.alpha;
                if (_lastKnownAlpha >= 0 && Mathf.Abs(cur - _lastKnownAlpha) > 0.01f)
                {
                    Debug.LogWarning($"[Cutscene:AlphaMonitor] FadeCanvas alpha changed externally: " +
                                     $"{_lastKnownAlpha:F2} → {cur:F2} | isPlaying={isPlaying} | " +
                                     $"currentCutscene={currentCutscene?.cutsceneID ?? "none"}");
                }
                _lastKnownAlpha = cur;
            }
            // ───────────────────────────────────────────────────────────────

            // Handle skip input
            if (isPlaying && currentCutscene != null && currentCutscene.canSkip)
            {
                if (Input.GetKeyDown(skipKey))
                {
                    SkipCutscene();
                }
            }

            // Handle wait for input
            if (isWaitingForInput && Input.anyKeyDown)
            {
                isWaitingForInput = false;
            }
        }

        private void OnEnable()
        {
            FlagEvents.OnFlagAdded += HandleFlagAdded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            FlagEvents.OnFlagAdded -= HandleFlagAdded;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Refresh references whenever a new scene is loaded
            CacheReferences();
        }

        private void HandleFlagAdded(string flagName)
        {
            // We still allow triggering if playing, because they will be queued now.
            // But we should probably prevent the EXACT same cutscene from being queued multiple times.


            // Search for cutscenes that trigger on this flag in Resources/Cutscenes
            CutsceneData[] allCutscenes = Resources.LoadAll<CutsceneData>("Cutscenes");
            
            // Get current flags for requirement checking
            List<string> currentFlags = interactionSystem != null ? interactionSystem.GetGameFlags() : new List<string>();

            foreach (var cs in allCutscenes)
            {
                if (cs.triggerFlag == flagName)
                {
                    // Check if other requirements are met
                    if (cs.CanTrigger(currentFlags))
                    {
                        if (showDebugLogs) Debug.Log($"[CutsceneController] Triggering cutscene '{cs.cutsceneID}' via flag '{flagName}'");
                        PlayCutscene(cs);
                        break;
                    }
                }
            }
        }

        void CacheReferences()
        {
            interactionSystem = Object.FindFirstObjectByType<NPCInteractionSystem>();

            movePlayer = MovePlayerTo.Instance;
            if (movePlayer == null)
                movePlayer = Object.FindFirstObjectByType<MovePlayerTo>();

            monologueSystem = Object.FindFirstObjectByType<MonologueSystem>();

            dayNightCycle = DayNightCycle.Instance;
            if (dayNightCycle == null)
                dayNightCycle = Object.FindFirstObjectByType<DayNightCycle>();

            questManager = QuestManager.Instance;
            if (questManager == null)
                questManager = Object.FindFirstObjectByType<QuestManager>();

            gameSystemsManager = GameSystemsManager.Instance;
            if (gameSystemsManager == null)
                gameSystemsManager = Object.FindFirstObjectByType<GameSystemsManager>();

            cinematicUI = Object.FindFirstObjectByType<CinematicUI>();

            // Re-acquire fadeCanvasGroup if the serialized reference was lost
            TryAcquireFadeCanvasGroup();
        }

        /// <summary>
        /// Attempts to find and assign the fadeCanvasGroup if it's null.
        /// Logs clearly whether it succeeded or failed so you can diagnose in the Unity console.
        /// </summary>
        private void TryAcquireFadeCanvasGroup()
        {
            if (fadeCanvasGroup != null)
            {
                Debug.Log($"[Cutscene:Fade] fadeCanvasGroup already assigned → '{fadeCanvasGroup.gameObject.name}' (alpha={fadeCanvasGroup.alpha:F2})");
                return;
            }

            // Try to find by tag (same tag used by MovePlayerTo and NPCManager)
            GameObject fadeObj = GameObject.FindGameObjectWithTag("FadeImage");
            if (fadeObj != null)
            {
                fadeCanvasGroup = fadeObj.GetComponent<CanvasGroup>();
                if (fadeCanvasGroup == null)
                {
                    Debug.Log($"[Cutscene:Fade] FadeImage-tagged object '{fadeObj.name}' has no CanvasGroup — adding one automatically.");
                    fadeCanvasGroup = fadeObj.AddComponent<CanvasGroup>();
                }

                fadeImage = fadeObj.GetComponent<Image>();
                Animator anim = fadeObj.GetComponent<Animator>();
                Debug.Log($"[Cutscene:Fade] Re-acquired fadeCanvasGroup from '{fadeObj.name}'. " +
                          $"Has Image={fadeImage != null} | Has Animator={anim != null} (enabled={anim?.enabled})");
            }
            else
            {
                Debug.LogError("[Cutscene:Fade] No GameObject with tag 'FadeImage' found in scene! " +
                               "Assign the Fade Canvas Group directly on the CutsceneController in the Inspector.");
            }
        }

        #region Public API

        /// <summary>
        /// Play a cutscene by its ID (loaded from Resources/Cutscenes)
        /// </summary>
        public void PlayCutscene(string cutsceneID)
        {
            CutsceneData cutscene = Resources.Load<CutsceneData>($"Cutscenes/{cutsceneID}");
            if (cutscene == null)
            {
                Debug.LogError($"[Cutscene] Cutscene not found: {cutsceneID}");
                return;
            }
            PlayCutscene(cutscene);
        }

        /// <summary>
        /// Play a cutscene from a CutsceneData asset. 
        /// If a dialogue is active, it will be queued until the dialogue ends.
        /// </summary>
        public void PlayCutscene(CutsceneData cutscene)
        {
            if (cutscene == null)
            {
                return;
            }

            // Safety check: ensure we have current scene references
            CacheReferences();

            // Check if cutscene is already in queue or playing to avoid duplicates
            if (currentCutscene == cutscene || cutsceneQueue.Contains(cutscene))
            {
                if (showDebugLogs) Debug.Log($"[Cutscene] Cutscene '{cutscene.cutsceneID}' is already playing or queued.");
                return;
            }

            // Check if cutscene can trigger
            var flags = GetCurrentFlags();
            if (!cutscene.CanTrigger(flags))
            {
                Log($"Cutscene '{cutscene.cutsceneID}' cannot trigger - required flags not met");
                return;
            }

            // Add to queue
            cutsceneQueue.Enqueue(cutscene);
            Log($"Queued cutscene: {cutscene.cutsceneID} (Queue size: {cutsceneQueue.Count})");

            // Start processing if not already playing
            if (!isPlaying)
            {
                playbackCoroutine = StartCoroutine(ProcessCutsceneQueue());
            }
        }

        /// <summary>
        /// Skip the current cutscene (if skippable)
        /// </summary>
        public void SkipCutscene()
        {
            if (!isPlaying || currentCutscene == null)
                return;

            if (!currentCutscene.canSkip)
            {
                Log("This cutscene cannot be skipped");
                return;
            }

            Log($"Skipping cutscene: {currentCutscene.cutsceneID}");

            if (playbackCoroutine != null)
                StopCoroutine(playbackCoroutine);

            // Apply skip - set completion flags but skip the sequence
            ApplyFlagsOnComplete(currentCutscene);

            // Cleanup
            CleanupAfterCutscene();

            CutsceneEvents.InvokeCutsceneSkipped(currentCutscene);

            currentCutscene = null;
            isPlaying = false;
        }

        /// <summary>
        /// Check if a specific cutscene is currently playing
        /// </summary>
        public bool IsCutscenePlaying(string cutsceneID)
        {
            return isPlaying && currentCutscene != null && currentCutscene.cutsceneID == cutsceneID;
        }

        #endregion

        #region Cutscene Playback

        IEnumerator ProcessCutsceneQueue()
        {
            isPlaying = true;
            Debug.Log($"[Cutscene:Queue] Processing started. Queue has {cutsceneQueue.Count} cutscene(s).");

            while (cutsceneQueue.Count > 0)
            {
                CutsceneData nextCutscene = cutsceneQueue.Dequeue();
                Debug.Log($"[Cutscene:Queue] Dequeued '{nextCutscene.cutsceneID}'. Remaining in queue: {cutsceneQueue.Count}");
                yield return StartCoroutine(PlayCutsceneSequence(nextCutscene));
                Debug.Log($"[Cutscene:Queue] Finished '{nextCutscene.cutsceneID}'. Remaining in queue: {cutsceneQueue.Count}");
            }

            isPlaying = false;
            playbackCoroutine = null;
            Debug.Log("[Cutscene:Queue] All cutscenes finished. isPlaying = false.");
        }

        IEnumerator PlayCutsceneSequence(CutsceneData cutscene)
        {
            currentCutscene = cutscene;
            currentStepIndex = 0;

            Log($"Preparing cutscene: {cutscene.cutsceneID}");

            // Wait for any active dialogue/monologue to end instead of force-closing
            if (IsDialogueActive())
            {
                Debug.Log($"[Cutscene:Wait] Dialogue active — waiting. Alpha={fadeCanvasGroup?.alpha:F2}");
                Log("Waiting for active dialogue/monologue to finish before starting cutscene...");

                yield return new WaitForSeconds(0.2f);

                while (IsDialogueActive())
                {
                    yield return null;
                }

                Debug.Log($"[Cutscene:Wait] Dialogue ENDED. Alpha={fadeCanvasGroup?.alpha:F2}  — waiting 0.5s grace...");
                yield return new WaitForSeconds(0.5f);
                Debug.Log($"[Cutscene:Wait] Grace period over. Alpha={fadeCanvasGroup?.alpha:F2}");
            }

            Debug.Log($"[Cutscene:Wait] Starting step execution. Alpha={fadeCanvasGroup?.alpha:F2}");
            Log($"Starting cutscene sequence: {cutscene.cutsceneID}");

            // Show cinematic bars and hide HUD
            if (cinematicUI != null)
                cinematicUI.ShowCinematicMode(true, cutscene.canSkip);
            Debug.Log($"[Cutscene:Wait] After ShowCinematicMode. Alpha={fadeCanvasGroup?.alpha:F2}");

            // Apply start settings
            if (cutscene.pauseGameTime && dayNightCycle != null)
                dayNightCycle.PauseTime();

            if (cutscene.disablePlayerInput && movePlayer != null)
                movePlayer.stopPlayerMovement();

            // Set start flags
            if (cutscene.flagsOnStart != null)
            {
                foreach (string flag in cutscene.flagsOnStart)
                {
                    if (!string.IsNullOrEmpty(flag))
                    {
                        Debug.Log($"[Cutscene:Wait] Setting start flag '{flag}'. Alpha before={fadeCanvasGroup?.alpha:F2}");
                        AddFlag(flag);
                        Debug.Log($"[Cutscene:Wait] After flag '{flag}'. Alpha={fadeCanvasGroup?.alpha:F2}");
                    }
                }
            }

            CutsceneEvents.InvokeCutsceneStarted(cutscene);

            // Execute each step
            for (int i = 0; i < cutscene.steps.Count; i++)
            {
                currentStepIndex = i;
                CutsceneStep step = cutscene.steps[i];

                // Check step conditions
                if (!CheckStepConditions(step))
                {
                    Log($"Skipping step {i} ({step.type}) - conditions not met");
                    continue;
                }

                // Delay before step
                if (step.delayBefore > 0)
                    yield return new WaitForSeconds(step.delayBefore);

                Debug.Log($"[Cutscene:Step] Step {i} ({step.type}) | Alpha={fadeCanvasGroup?.alpha:F2}");
                Log($"Executing step {i}: {step.type}" +
                    (string.IsNullOrEmpty(step.stepName) ? "" : $" ({step.stepName})"));

                CutsceneEvents.InvokeStepStarted(cutscene, step, i);

                // Execute the step
                if (step.waitForCompletion)
                {
                    yield return StartCoroutine(ExecuteStep(step));
                }
                else
                {
                    StartCoroutine(ExecuteStep(step));
                }

                // Apply step flags
                ApplyStepFlags(step);

                CutsceneEvents.InvokeStepCompleted(cutscene, step, i);
            }

            // Cutscene complete
            Log($"Cutscene complete: {cutscene.cutsceneID}");

            ApplyFlagsOnComplete(cutscene);
            CleanupAfterCutscene();

            CutsceneEvents.InvokeCutsceneCompleted(cutscene);

            currentCutscene = null;
        }

        /// <summary>
        /// Checks if any dialogue system is currently active (NPC dialogue, Ink story, or Monologue)
        /// </summary>
        public bool IsDialogueActive()
        {
            // Check NPC Interaction System
            if (interactionSystem != null && interactionSystem.IsInDialogue())
                return true;

            // Check Ink Story Manager
            if (InkStoryManager.Instance != null && InkStoryManager.Instance.IsDialogueActive)
                return true;

            // Check Monologue System
            if (monologueSystem != null && monologueSystem.IsInMonologue)
                return true;

            return false;
        }

        IEnumerator ExecuteStep(CutsceneStep step)
        {
            switch (step.type)
            {
                // ===== DIALOGUE & TEXT =====
                case CutsceneStep.StepType.ShowDialogue:
                    yield return StartCoroutine(StepShowDialogue(step));
                    break;

                case CutsceneStep.StepType.ShowMonologue:
                    yield return StartCoroutine(StepShowMonologue(step));
                    break;

                case CutsceneStep.StepType.ShowMessage:
                    ShowMessage(step.textContent);
                    yield return new WaitForSeconds(step.duration);
                    break;

                // ===== PLAYER CONTROL =====
                case CutsceneStep.StepType.DisablePlayerMovement:
                    if (movePlayer != null) movePlayer.stopPlayerMovement();
                    break;

                case CutsceneStep.StepType.EnablePlayerMovement:
                    if (movePlayer != null) movePlayer.resumePlayerMovement();
                    break;

                case CutsceneStep.StepType.TeleportPlayer:
                    yield return StartCoroutine(StepTeleportPlayer(step));
                    break;

                case CutsceneStep.StepType.MovePlayerTo:
                    yield return StartCoroutine(StepMovePlayerTo(step));
                    break;

                case CutsceneStep.StepType.MovePlayerWalk:
                    yield return StartCoroutine(StepMovePlayerWalk(step));
                    break;

                // ===== NPC CONTROL =====
                case CutsceneStep.StepType.SpawnNPC:
                    if (NPCManager.Instance != null)
                        NPCManager.Instance.SpawnNPCAtCurrentScheduledLocation(step.targetID);
                    break;

                case CutsceneStep.StepType.DespawnNPC:
                    if (NPCManager.Instance != null)
                        NPCManager.Instance.DespawnNPC(step.targetID);
                    break;

                case CutsceneStep.StepType.MoveNPCTo:
                    yield return StartCoroutine(StepMoveNPCTo(step));
                    break;

                // ===== CAMERA =====
                case CutsceneStep.StepType.CameraShake:
                    yield return StartCoroutine(StepCameraShake(step));
                    break;

                case CutsceneStep.StepType.CameraFocusOn:
                    yield return StartCoroutine(StepCameraFocusOn(step));
                    break;

                case CutsceneStep.StepType.CameraFollowPlayer:
                    yield return StartCoroutine(StepCameraFollowPlayer(step));
                    break;

                // ===== GAME STATE =====
                case CutsceneStep.StepType.SetFlag:
                    AddFlag(step.targetID);
                    break;

                case CutsceneStep.StepType.RemoveFlag:
                    RemoveFlag(step.targetID);
                    break;

                case CutsceneStep.StepType.StartQuest:
                    if (questManager != null)
                        questManager.StartQuest(step.targetID);
                    break;

                case CutsceneStep.StepType.CompleteQuest:
                    if (questManager != null)
                        questManager.CompleteQuest(step.targetID);
                    break;

                case CutsceneStep.StepType.CompleteObjective:
                    if (questManager != null)
                        questManager.CompleteObjective(step.targetID, step.secondaryTargetID);
                    break;

                // ===== TIME & ENVIRONMENT =====
                case CutsceneStep.StepType.SetTimeOfDay:
                    if (dayNightCycle != null)
                    {
                        if (step.duration > 0)
                            yield return StartCoroutine(StepTransitionTime(step.timeOfDay, step.duration));
                        else
                            dayNightCycle.SetTimeOfDay(step.timeOfDay);
                    }
                    break;

                case CutsceneStep.StepType.PauseGameTime:
                    if (dayNightCycle != null)
                        dayNightCycle.PauseTime();
                    break;

                case CutsceneStep.StepType.ResumeGameTime:
                    if (dayNightCycle != null)
                        dayNightCycle.ResumeTime();
                    break;

                // ===== AUDIO =====
                case CutsceneStep.StepType.PlaySound:
                    if (sfxSource != null && step.audioClip != null)
                        sfxSource.PlayOneShot(step.audioClip);
                    break;

                case CutsceneStep.StepType.PlayMusic:
                    if (musicSource != null && step.audioClip != null)
                    {
                        musicSource.clip = step.audioClip;
                        musicSource.Play();
                    }
                    break;

                case CutsceneStep.StepType.StopMusic:
                    if (musicSource != null)
                        musicSource.Stop();
                    break;

                // ===== SCENE & AREA =====
                case CutsceneStep.StepType.EnterSubArea:
                case CutsceneStep.StepType.ExitSubArea:
                    // Currently using teleport for sub-areas to avoid scene loading issues
                    yield return StartCoroutine(StepTeleportPlayer(step));
                    break;

                case CutsceneStep.StepType.FadeToBlack:
                    // Use tracked fade so cleanup can stop it if waitForCompletion=false
                    yield return StartTrackedFade(true, step.duration);
                    fadeCoroutine = null;
                    break;

                case CutsceneStep.StepType.FadeFromBlack:
                    yield return StartTrackedFade(false, step.duration);
                    fadeCoroutine = null;
                    break;

                // ===== FLOW CONTROL =====
                case CutsceneStep.StepType.WaitSeconds:
                    yield return new WaitForSeconds(step.duration);
                    break;

                case CutsceneStep.StepType.WaitForDialogueEnd:
                    yield return StartCoroutine(WaitForDialogueEnd());
                    break;

                case CutsceneStep.StepType.WaitForInput:
                    isWaitingForInput = true;
                    while (isWaitingForInput)
                        yield return null;
                    break;

                // ===== GAME OBJECTS =====
                case CutsceneStep.StepType.EnableGameObject:
                    EnableGameObjectByTag(step.targetID, true);
                    break;

                case CutsceneStep.StepType.DisableGameObject:
                    EnableGameObjectByTag(step.targetID, false);
                    break;

                // ===== CUSTOM =====
                case CutsceneStep.StepType.TriggerEvent:
                    // Custom events can be handled by subscribers to CutsceneEvents
                    Log($"Custom event triggered: {step.targetID}");
                    break;

                default:
                    Log($"Unknown step type: {step.type}");
                    break;
            }
        }

        #endregion

        #region Step Implementations

        IEnumerator StepShowDialogue(CutsceneStep step)
        {
            // Trigger dialogue with NPC
            if (interactionSystem != null && !string.IsNullOrEmpty(step.targetID))
            {
                // Find NPC and start dialogue
                NPC npc = FindNPCByID(step.targetID);

                // Start dialogue with override if provided
                if (step.dialogueOverride != null)
                {
                    interactionSystem.StartDialogue(npc, step.dialogueOverride);
                }
                else if (npc != null)
                {
                    interactionSystem.StartDialogue(npc);
                }
                else
                {
                    Debug.LogWarning($"[Cutscene] NPC/Player not found for dialogue: {step.targetID}");
                    yield break;
                }

                yield return StartCoroutine(WaitForDialogueEnd());
            }
        }

        IEnumerator StepShowMonologue(CutsceneStep step)
        {
            if (monologueSystem == null) yield break;

            if (showDebugLogs) Debug.Log($"[CutsceneController] Step: Show Monologue - \"{step.textContent}\"");

            // Use the standard monologue system
            monologueSystem.ShowSimpleMonologue(step.textContent, step.flagsToSet);

            // Wait for it to finish if requested
            if (step.waitForCompletion)
            {
                // Wait a small amount of time to ensure either IsInMonologue or IsInDialogue has updated to true
                yield return new WaitForSeconds(0.1f);

                while ((monologueSystem != null && monologueSystem.IsInMonologue) || 
                       (interactionSystem != null && interactionSystem.IsInDialogue()))
                {
                    yield return null;
                }
            }
        }

        IEnumerator StepTeleportPlayer(CutsceneStep step)
        {
            if (movePlayer != null && !string.IsNullOrEmpty(step.targetID))
            {
                bool teleportComplete = false;
                // Use the cutscene-specific move: no internal fade.
                // Fading is controlled by explicit FadeToBlack / FadeFromBlack steps.
                movePlayer.MovePlayerForCutscene(step.targetID, () => teleportComplete = true);

                while (!teleportComplete)
                    yield return null;
            }
        }

        IEnumerator StepMovePlayerTo(CutsceneStep step)
        {
            // For now, reuse teleport with fade
            yield return StartCoroutine(StepTeleportPlayer(step));
        }

        IEnumerator StepMovePlayerWalk(CutsceneStep step)
        {
            if (movePlayer == null || string.IsNullOrEmpty(step.targetID)) yield break;

            Transform targetTransform = FindTargetByID(step.targetID);
            if (targetTransform == null)
            {
                Debug.LogWarning($"[Cutscene] MovePlayerWalk: Target '{step.targetID}' not found!");
                yield break;
            }

            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO == null) yield break;

            PlayerAnimation anim = playerGO.GetComponent<PlayerAnimation>();
            movePlayer.stopPlayerMovement();

            float walkSpeed = 3f; // Default walk speed
            float arrivalThreshold = 0.1f;

            while (Vector2.Distance(playerGO.transform.position, targetTransform.position) > arrivalThreshold)
            {
                Vector2 direction = ((Vector2)targetTransform.position - (Vector2)playerGO.transform.position).normalized;

                // Move player
                playerGO.transform.position = Vector2.MoveTowards(playerGO.transform.position, targetTransform.position, walkSpeed * Time.deltaTime);

                // Update animations
                if (anim != null)
                {
                    anim.setMovingAnimation(true);
                    anim.setMovingAnimation(direction.x, direction.y);
                }

                yield return null;
            }

            playerGO.transform.position = targetTransform.position;
            if (anim != null) anim.setMovingAnimation(false);
        }

        IEnumerator StepMoveNPCTo(CutsceneStep step)
        {
            if (string.IsNullOrEmpty(step.targetID) || string.IsNullOrEmpty(step.secondaryTargetID)) yield break;

            NPC npc = FindNPCByID(step.targetID);
            Transform targetTransform = FindTargetByID(step.secondaryTargetID);

            if (npc == null || targetTransform == null)
            {
                Debug.LogWarning($"[Cutscene] MoveNPCTo: NPC '{step.targetID}' or Target '{step.secondaryTargetID}' not found!");
                yield break;
            }

            // Disable NPC AI/Schedule temporarily
            npc.StopScheduleExecution();

            float walkSpeed = 2.5f;
            float arrivalThreshold = 0.1f;

            // Trigger NPC walk animation if they have one
            // (Assuming NPC has similar setMovingAnimation as Player)

            while (Vector2.Distance(npc.transform.position, targetTransform.position) > arrivalThreshold)
            {
                npc.transform.position = Vector2.MoveTowards(npc.transform.position, targetTransform.position, walkSpeed * Time.deltaTime);
                yield return null;
            }

            npc.transform.position = targetTransform.position;
            npc.ResumeScheduleExecution();
        }

        IEnumerator StepTransitionTime(TimeOfDay targetTime, float duration)
        {
            if (dayNightCycle == null) yield break;

            float startHour = dayNightCycle.CurrentTime;
            float targetHour = 12f; // Default Day

            // Get the target hour representation from the DayNightCycle system
            // (We'll use a temporary snap to get the hour value and then reset it)
            float originalTime = dayNightCycle.CurrentTime;
            dayNightCycle.SetTimeOfDay(targetTime);
            targetHour = dayNightCycle.CurrentTime;
            dayNightCycle.SetTime(originalTime);

            // Handle wrap-around (if it's closer to go backwards or through midnight)
            if (Mathf.Abs(targetHour - startHour) > 12f)
            {
                if (targetHour > startHour) startHour += 24f;
                else targetHour += 24f;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float currentHour = Mathf.Lerp(startHour, targetHour, elapsed / duration);

                // Keep it in 0-24 range
                if (currentHour >= 24f) currentHour -= 24f;

                dayNightCycle.SetTime(currentHour);
                yield return null;
            }

            dayNightCycle.SetTimeOfDay(targetTime);
        }

        private Transform FindTargetByID(string targetID)
        {
            // First check NPCTarget tags (convention in MovePlayerTo)
            GameObject[] targets = GameObject.FindGameObjectsWithTag("NPCTarget");
            foreach (var t in targets)
            {
                if (t.name == targetID) return t.transform;
            }

            // Check NPC IDs
            NPC npc = FindNPCByID(targetID);
            if (npc != null) return npc.transform;

            // Fallback to finding by name anywhere in scene
            GameObject go = GameObject.Find(targetID);
            if (go != null) return go.transform;

            return null;
        }

        IEnumerator StepCameraShake(CutsceneStep step)
        {
            if (CameraShake.Instance != null)
            {
                bool shakeComplete = false;

                if (step.shakeIntensity > 1.5f)
                    CameraShake.Instance.ShakeExplosion(() => shakeComplete = true);
                else if (step.shakeIntensity > 0.5f)
                    CameraShake.Instance.ShakeMedium(() => shakeComplete = true);
                else
                    CameraShake.Instance.ShakeLight(() => shakeComplete = true);

                while (!shakeComplete)
                    yield return null;
            }
            else
            {
                yield return new WaitForSeconds(step.duration);
            }
        }

        IEnumerator StepCameraFocusOn(CutsceneStep step)
        {
            Transform target = FindTargetByID(step.targetID);
            if (target == null) yield break;

            if (CameraShake.Instance != null && CameraShake.Instance.virtualCamera != null)
            {
                var cam = CameraShake.Instance.virtualCamera;
                cam.Follow = target;
                // cam.LookAt = target; // Often not needed in 2D

                // Wait for camera to arrive (approximation)
                yield return new WaitForSeconds(step.duration);
            }
        }

        IEnumerator StepCameraFollowPlayer(CutsceneStep step)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO == null) yield break;

            if (CameraShake.Instance != null && CameraShake.Instance.virtualCamera != null)
            {
                var cam = CameraShake.Instance.virtualCamera;
                cam.Follow = playerGO.transform;
                // cam.LookAt = playerGO.transform;

                yield return new WaitForSeconds(step.duration);
            }
        }

        IEnumerator WaitForDialogueEnd()
        {
            // Wait for dialogue to start (if not already)
            yield return new WaitForSeconds(0.1f);

            // Wait for dialogue to end
            while (interactionSystem != null && interactionSystem.IsInDialogue())
            {
                yield return null;
            }
        }

        public IEnumerator FadeScreen(bool toBlack, float duration)
        {
            string direction = toBlack ? "TO BLACK" : "FROM BLACK";

            // Re-acquire reference if lost (e.g. after scene reload)
            if (fadeCanvasGroup == null)
            {
                Debug.LogWarning($"[Cutscene:Fade] fadeCanvasGroup is NULL before {direction} — trying to re-acquire...");
                TryAcquireFadeCanvasGroup();
            }

            if (fadeCanvasGroup == null)
            {
                Debug.LogError($"[Cutscene:Fade] fadeCanvasGroup still NULL after re-acquire attempt! {direction} fade SKIPPED. " +
                               "Make sure a CanvasGroup is assigned to 'Fade Canvas Group' on the CutsceneController, " +
                               "or that a GameObject tagged 'FadeImage' exists in the scene.");
                yield return new WaitForSeconds(duration);
                yield break;
            }

            // Disable any Animator on the same GameObject to prevent it from
            // overriding the alpha we set via the CanvasGroup
            Animator fadeAnim = fadeCanvasGroup.GetComponent<Animator>();
            bool animWasEnabled = fadeAnim != null && fadeAnim.enabled;
            if (animWasEnabled)
            {
                Debug.Log($"[Cutscene:Fade] Disabling Animator on '{fadeCanvasGroup.gameObject.name}' to prevent alpha override.");
                fadeAnim.enabled = false;
            }

            float endAlpha   = toBlack ? 1f : 0f;
            float startAlpha = fadeCanvasGroup.alpha;
            float elapsed    = 0f;

            Debug.Log($"[Cutscene:Fade] {direction} | duration={duration}s | " +
                      $"startAlpha={startAlpha:F2} → endAlpha={endAlpha:F2} | " +
                      $"GameObject='{fadeCanvasGroup.gameObject.name}' | " +
                      $"active={fadeCanvasGroup.gameObject.activeInHierarchy} | " +
                      $"Animator present={fadeAnim != null} (was enabled={animWasEnabled})");

            fadeCanvasGroup.blocksRaycasts = toBlack;
            fadeCanvasGroup.gameObject.SetActive(true);

            if (duration <= 0f)
            {
                fadeCanvasGroup.alpha = endAlpha;
                _isFading = false;
                _lastKnownAlpha = endAlpha;
                Debug.Log($"[Cutscene:Fade] {direction} instant (duration=0). Final alpha={endAlpha:F2}");
                yield break;
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }

            fadeCanvasGroup.alpha = endAlpha;
            _isFading = false;
            _lastKnownAlpha = endAlpha;
            Debug.Log($"[Cutscene:Fade] {direction} COMPLETE. Final alpha={fadeCanvasGroup.alpha:F2}");
        }

        /// <summary>
        /// Starts a tracked fade coroutine, cancelling any previous one first.
        /// Use this instead of StartCoroutine(FadeScreen(...)) to prevent
        /// orphaned fire-and-forget fades from writing stale alpha values.
        /// </summary>
        /// <summary>
        /// Public tracked fade — use this from external scripts (e.g. MovePlayerTo)
        /// so the coroutine is registered and can be killed by CleanupAfterCutscene.
        /// </summary>
        public Coroutine StartTrackedFade(bool toBlack, float duration)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                Debug.Log($"[Cutscene:Fade] Killed previous fade coroutine before starting {(toBlack ? "TO BLACK" : "FROM BLACK")}.");
            }
            _isFading = true;
            fadeCoroutine = StartCoroutine(FadeScreen(toBlack, duration));
            return fadeCoroutine;
        }

        #endregion

        #region Helper Methods

        bool CheckStepConditions(CutsceneStep step)
        {
            if (step.requiredFlags == null || step.requiredFlags.Length == 0)
                return true;

            var flags = GetCurrentFlags();
            foreach (string flag in step.requiredFlags)
            {
                if (!string.IsNullOrEmpty(flag) && !flags.Contains(flag))
                    return false;
            }
            return true;
        }

        void ApplyStepFlags(CutsceneStep step)
        {
            if (step.flagsToSet != null)
            {
                foreach (string flag in step.flagsToSet)
                {
                    if (!string.IsNullOrEmpty(flag))
                        AddFlag(flag);
                }
            }

            if (step.flagsToRemove != null)
            {
                foreach (string flag in step.flagsToRemove)
                {
                    if (!string.IsNullOrEmpty(flag))
                        RemoveFlag(flag);
                }
            }
        }

        void ApplyFlagsOnComplete(CutsceneData cutscene)
        {
            if (cutscene.flagsOnComplete != null)
            {
                foreach (string flag in cutscene.flagsOnComplete)
                {
                    if (!string.IsNullOrEmpty(flag))
                        AddFlag(flag);
                }
            }
        }

        void CleanupAfterCutscene()
        {
            Debug.Log($"[Cutscene:Cleanup] Running cleanup for '{currentCutscene?.cutsceneID}'. " +
                      $"fadeCanvasGroup null={fadeCanvasGroup == null} | " +
                      $"current alpha={(fadeCanvasGroup != null ? fadeCanvasGroup.alpha.ToString("F2") : "N/A")}");

            // Hide cinematic bars and restore HUD
            if (cinematicUI != null)
                cinematicUI.ShowCinematicMode(false);

            // Resume time if it was paused
            if (currentCutscene != null && currentCutscene.pauseGameTime && dayNightCycle != null)
            {
                dayNightCycle.ResumeTime();
            }

            // Re-enable player input if it was disabled
            if (currentCutscene != null && currentCutscene.disablePlayerInput && movePlayer != null)
            {
                movePlayer.resumePlayerMovement();
            }

            // Kill any orphaned fire-and-forget fade coroutine BEFORE resetting alpha.
            // Without this, a fade started with waitForCompletion=false will finish
            // AFTER cleanup and silently set alpha back to 1, breaking all future fades.
            if (fadeCoroutine != null)
            {
                Debug.Log("[Cutscene:Cleanup] Stopping orphaned fade coroutine to prevent stale alpha write.");
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            // Ensure fade is cleared
            if (fadeCanvasGroup != null)
            {
                // Disable any Animator that might override our alpha reset
                Animator fadeAnim = fadeCanvasGroup.GetComponent<Animator>();
                if (fadeAnim != null && fadeAnim.enabled)
                {
                    fadeAnim.enabled = false;
                }

                fadeCanvasGroup.alpha = 0;
                fadeCanvasGroup.blocksRaycasts = false;
            }
        }

        List<string> GetCurrentFlags()
        {
            if (interactionSystem != null)
                return interactionSystem.GetGameFlags();
            return new List<string>();
        }

        void AddFlag(string flag)
        {
            if (interactionSystem != null)
                interactionSystem.AddGameFlag(flag);
        }

        void RemoveFlag(string flag)
        {
            if (interactionSystem != null)
                interactionSystem.RemoveGameFlag(flag);
        }

        NPC FindNPCByID(string npcID)
        {
            if (NPCManager.Instance != null)
            {
                return NPCManager.Instance.GetNPCByID(npcID);
            }
            return null;
        }

        void EnableGameObjectByTag(string tag, bool enable)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(tag);
            if (obj != null)
                obj.SetActive(enable);
            else
                Debug.LogWarning($"[Cutscene] GameObject with tag '{tag}' not found");
        }

        void ShowMessage(string message)
        {
            if (gameSystemsManager != null)
            {
                Debug.Log($"[Cutscene Message] {message}");
            }
        }

        void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[Cutscene] {message}");
        }

        #endregion
    }
}
