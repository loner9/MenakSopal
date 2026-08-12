using System;
using UnityEngine;
using UnityEngine.UI;

namespace MenakSopal.ChaseMinigame
{
    public enum ChaseGameState
    {
        Ready,
        Playing,
        Victory,
        Defeat
    }

    /// <summary>
    /// Central Level Manager managing progress along the X-axis track, game states, and victory/defeat.
    /// </summary>
    public class ChaseLevelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerChaseController playerController;
        [SerializeField] private ChaserController chaserController;
        [SerializeField] private ObstacleSpawner obstacleSpawner;
        [SerializeField] private FinishLine finishLineInstance;

        [Header("Level Track Setup")]
        [Tooltip("Target X distance for the Finish Line at the end of the track")]
        [SerializeField] private float finishLineX = 200.0f;
        [SerializeField] private Slider progressBarUI;

        [Header("State Settings")]
        [SerializeField] private bool autoStartOnLoad = false;
        [SerializeField] private ChaseGameState currentState = ChaseGameState.Ready;

        public event Action<ChaseGameState> OnGameStateChanged;

        public ChaseGameState CurrentState => currentState;
        public float ProgressNormalized => (chaserController != null && finishLineX > 0f)
            ? Mathf.Clamp01(chaserController.TrackX / finishLineX)
            : 0f;

        private void Start()
        {
            if (playerController == null) playerController = FindFirstObjectByType<PlayerChaseController>();
            if (chaserController == null) chaserController = FindFirstObjectByType<ChaserController>();
            if (obstacleSpawner == null) obstacleSpawner = FindFirstObjectByType<ObstacleSpawner>();
            if (finishLineInstance == null) finishLineInstance = FindFirstObjectByType<FinishLine>();

            if (playerController != null)
            {
                playerController.OnPlayerCaught += HandlePlayerCaught;
            }

            if (finishLineInstance != null)
            {
                // Respect the position where the FinishLine object is placed in Scene View
                finishLineX = finishLineInstance.transform.position.x;
                finishLineInstance.OnPlayerCrossedFinishLine += HandleVictory;
            }

            if (autoStartOnLoad)
            {
                StartLevel();
            }
            else
            {
                SetState(ChaseGameState.Ready);
            }
        }

        private void OnDestroy()
        {
            if (playerController != null) playerController.OnPlayerCaught -= HandlePlayerCaught;
            if (finishLineInstance != null) finishLineInstance.OnPlayerCrossedFinishLine -= HandleVictory;
        }

        private void Update()
        {
            if (currentState != ChaseGameState.Playing) return;

            if (progressBarUI != null)
            {
                progressBarUI.value = ProgressNormalized;
            }

            // Fallback: If player X reaches or exceeds finishLineX coordinate
            if (playerController != null && playerController.transform.position.x >= finishLineX)
            {
                HandleVictory();
            }
        }

        public void StartLevel()
        {
            SetState(ChaseGameState.Playing);

            if (chaserController != null)
            {
                chaserController.ResetPosition(0f);
                chaserController.StartRunning();
            }

            if (playerController != null)
            {
                playerController.ResetState();
            }

            if (obstacleSpawner != null)
            {
                obstacleSpawner.ResetDifficulty();
                obstacleSpawner.StartSpawning();
            }

            if (finishLineInstance != null)
            {
                finishLineInstance.ResetTrigger();
            }
        }

        private void HandlePlayerCaught()
        {
            SetState(ChaseGameState.Defeat);

            if (chaserController != null) chaserController.StopRunning();
            if (obstacleSpawner != null)
            {
                obstacleSpawner.ClearAllObstacles();
            }
        }

        private void HandleVictory()
        {
            SetState(ChaseGameState.Victory);

            if (chaserController != null) chaserController.StopRunning();
            if (playerController != null) playerController.SetInputEnabled(false);
            if (obstacleSpawner != null)
            {
                obstacleSpawner.ClearAllObstacles();
            }
        }

        private void SetState(ChaseGameState newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(currentState);
            Debug.Log($"[ChaseLevelManager] Game State changed to: {newState}");
        }
    }
}
