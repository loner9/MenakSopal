using UnityEngine;

namespace MenakSopal.ChaseMinigame
{
    /// <summary>
    /// Chaser entity that stays fixed at left X-position and smoothly follows the player's current lane.
    /// </summary>
    public class ChaserController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerChaseController playerController;
        [SerializeField] private LaneManager laneManager;

        [Header("Position & Tracking")]
        [SerializeField] private float fixedXPosition = -6.0f;
        [SerializeField] private float followSpeed = 4.5f;

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

            // Sync initial position
            if (playerController != null && laneManager != null)
            {
                float initialY = laneManager.GetLaneY(playerController.CurrentLane);
                transform.position = new Vector3(fixedXPosition, initialY, transform.position.z);
            }
        }

        private void Update()
        {
            if (playerController == null || laneManager == null) return;

            // Target Y is based on Player's current lane index
            float targetY = laneManager.GetLaneY(playerController.CurrentLane);
            Vector3 targetPos = new Vector3(fixedXPosition, targetY, transform.position.z);

            // Smoothly move towards the target Y lane position
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
        }

        public void SetFixedXPosition(float xPos)
        {
            fixedXPosition = xPos;
        }
    }
}
