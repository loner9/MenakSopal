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
    /// Central Level Manager overseeing game state transitions, distance progress, and UI updates.
    /// </summary>
    public class ChaseLevelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerChaseController playerController;
        [SerializeField] private ObstacleSpawner obstacleSpawner;
        [SerializeField] private FinishLine finishLinePrefab;

        [Header("Level Progress Settings")]
        [SerializeField] private float targetDistance = 100.0f;
        [SerializeField] private Slider progressBarUI;

        [Header("Game State")]
        [SerializeField] private ChaseGameState currentState = ChaseGameState.Ready;

        private float currentDistance = 0f;
        private FinishLine activeFinishLine;

        public event Action<ChaseGameState> OnGameStateChanged;

        public ChaseGameState CurrentState => currentState;
        public float ProgressNormalized => Mathf.Clamp01(currentDistance / targetDistance);

        private void Start()
        {
            if (playerController == null) playerController = FindFirstObjectByType<PlayerChaseController>();
            if (obstacleSpawner == null) obstacleSpawner = FindFirstObjectByType<ObstacleSpawner>();

            if (playerController != null)
            {
                playerController.OnPlayerCaught += HandlePlayerCaught;
            }

            StartLevel();
        }

        private void OnDestroy()
        {
            if (playerController != null)
            {
                playerController.OnPlayerCaught -= HandlePlayerCaught;
            }

            if (activeFinishLine != null)
            {
                activeFinishLine.OnPlayerCrossedFinishLine -= HandleVictory;
            }
        }

        private void Update()
        {
            if (currentState != ChaseGameState.Playing) return;

            // Accumulate distance based on current spawner/run speed
            float runSpeed = (obstacleSpawner != null) ? obstacleSpawner.CurrentSpeed : 8.0f;
            currentDistance += runSpeed * Time.deltaTime;

            if (progressBarUI != null)
            {
                progressBarUI.value = ProgressNormalized;
            }

            // Check if distance goal reached
            if (currentDistance >= targetDistance && activeFinishLine == null)
            {
                OnTargetDistanceReached();
            }
        }

        public void StartLevel()
        {
            currentDistance = 0f;
            SetState(ChaseGameState.Playing);

            if (obstacleSpawner != null)
            {
                obstacleSpawner.ResetDifficulty();
                obstacleSpawner.StartSpawning();
            }

            if (playerController != null)
            {
                playerController.SetInputEnabled(true);
            }
        }

        private void OnTargetDistanceReached()
        {
            // Halt regular obstacle spawning
            if (obstacleSpawner != null)
            {
                obstacleSpawner.StopSpawning();
            }

            // Spawn finish line
            if (finishLinePrefab != null)
            {
                float speed = (obstacleSpawner != null) ? obstacleSpawner.CurrentSpeed : 8.0f;
                activeFinishLine = Instantiate(finishLinePrefab, new Vector3(12.0f, 0f, 0f), Quaternion.identity);
                activeFinishLine.OnPlayerCrossedFinishLine += HandleVictory;
                activeFinishLine.Initialize(speed);
            }
            else
            {
                // Instant victory fallback if no finish line prefab assigned
                HandleVictory();
            }
        }

        private void HandlePlayerCaught()
        {
            SetState(ChaseGameState.Defeat);

            if (obstacleSpawner != null)
            {
                obstacleSpawner.StopSpawning();
            }
        }

        private void HandleVictory()
        {
            SetState(ChaseGameState.Victory);

            if (playerController != null)
            {
                playerController.SetInputEnabled(false);
            }

            if (obstacleSpawner != null)
            {
                obstacleSpawner.StopSpawning();
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
