using UnityEngine;

namespace MenakSopal.ChaseMinigame
{
    /// <summary>
    /// Chaser entity that advances forward along the X-axis track and smoothly follows the player's lane.
    /// </summary>
    public class ChaserController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerChaseController playerController;
        [SerializeField] private LaneManager laneManager;

        [Header("Forward Movement")]
        [SerializeField] private float baseRunSpeed = 8.0f;
        [SerializeField] private float followSpeed = 4.5f;

        private float currentTrackX = 0f;
        private float currentRunSpeed;
        private bool isRunning = false;

        public float TrackX => currentTrackX;
        public float CurrentRunSpeed => currentRunSpeed;

        private void Start()
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerChaseController>();
            }

            if (laneManager == null)
            {
                laneManager = FindFirstObjectByType<LaneManager>();
            }

            currentRunSpeed = baseRunSpeed;
            currentTrackX = transform.position.x;
        }

        private void Update()
        {
            if (!isRunning || playerController == null || laneManager == null) return;

            // Advance X along the track
            currentTrackX += currentRunSpeed * Time.deltaTime;

            // Smoothly follow Player's lane on Y-axis
            float targetY = laneManager.GetLaneY(playerController.CurrentLane);
            Vector3 targetPos = new Vector3(currentTrackX, targetY, transform.position.z);

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
        }

        public void StartRunning()
        {
            isRunning = true;
        }

        public void StopRunning()
        {
            isRunning = false;
        }

        public void SetRunSpeed(float speed)
        {
            currentRunSpeed = speed;
        }

        public void ResetPosition(float startX)
        {
            currentTrackX = startX;
            currentRunSpeed = baseRunSpeed;
            isRunning = false;

            if (playerController != null && laneManager != null)
            {
                float initialY = laneManager.GetLaneY(playerController.CurrentLane);
                transform.position = new Vector3(currentTrackX, initialY, transform.position.z);
            }
        }
    }
}
