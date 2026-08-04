using System;
using UnityEngine;

namespace MenakSopal.ChaseMinigame
{
    /// <summary>
    /// Controls player lane switching (Up/Down) and discrete safety steps away from Chaser.
    /// </summary>
    public class PlayerChaseController : MonoBehaviour
    {
        [Header("Lane & Movement")]
        [SerializeField] private LaneManager laneManager;
        [SerializeField] private int currentLane = 1; // 0..3
        [SerializeField] private float laneChangeSpeed = 12f;

        [Header("Safety Steps (X Position)")]
        [SerializeField] private float baseChaserX = -6f;
        [SerializeField] private float stepWidthX = 2.5f;
        [SerializeField] private int maxSafetySteps = 3;
        [SerializeField] private int currentSafetyStep = 3;
        [SerializeField] private float stepMoveSpeed = 8f;

        [Header("Input Settings")]
        [SerializeField] private bool enableInput = true;

        public event Action<int> OnSafetyStepChanged;
        public event Action OnPlayerCaught; // Game Over trigger

        public int CurrentLane => currentLane;
        public int CurrentSafetyStep => currentSafetyStep;
        public int MaxSafetySteps => maxSafetySteps;

        private Vector3 targetPosition;

        private void Start()
        {
            if (laneManager == null)
            {
                laneManager = FindFirstObjectByType<LaneManager>();
            }

            currentSafetyStep = maxSafetySteps;
            UpdateTargetPositionImmediate();
        }

        private void Update()
        {
            if (enableInput)
            {
                HandleInput();
            }

            SmoothMoveToTarget();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveLane(-1); // Move Up (towards lane 0)
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveLane(1); // Move Down (towards lane 3)
            }
        }

        public void MoveLane(int laneDirection)
        {
            if (laneManager == null) return;

            int nextLane = Mathf.Clamp(currentLane + laneDirection, 0, laneManager.LaneCount - 1);
            if (nextLane != currentLane)
            {
                currentLane = nextLane;
            }
        }

        /// <summary>
        /// Called when obstacle hits player. Decrements safety step buffer.
        /// </summary>
        public void TakeHit()
        {
            if (currentSafetyStep <= 0) return;

            currentSafetyStep--;
            OnSafetyStepChanged?.Invoke(currentSafetyStep);

            if (currentSafetyStep <= 0)
            {
                enableInput = false;
                OnPlayerCaught?.Invoke();
            }
        }

        /// <summary>
        /// Recovers 1 safety step if pickups/buffs are added.
        /// </summary>
        public void RecoverStep()
        {
            if (currentSafetyStep < maxSafetySteps)
            {
                currentSafetyStep++;
                OnSafetyStepChanged?.Invoke(currentSafetyStep);
            }
        }

        private void SmoothMoveToTarget()
        {
            if (laneManager == null) return;

            float targetY = laneManager.GetLaneY(currentLane);
            float targetX = baseChaserX + (currentSafetyStep * stepWidthX);

            targetPosition = new Vector3(targetX, targetY, transform.position.z);

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * Mathf.Max(laneChangeSpeed, stepMoveSpeed)
            );
        }

        private void UpdateTargetPositionImmediate()
        {
            if (laneManager == null) return;

            float targetY = laneManager.GetLaneY(currentLane);
            float targetX = baseChaserX + (currentSafetyStep * stepWidthX);
            transform.position = new Vector3(targetX, targetY, transform.position.z);
        }

        public void SetInputEnabled(bool enabled)
        {
            enableInput = enabled;
        }
    }
}
