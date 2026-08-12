using UnityEngine;

namespace MenakSopal.ChaseMinigame
{
    /// <summary>
    /// Controls camera tracking along the X-axis track during the chase minigame.
    /// </summary>
    public class ChaseCameraController : MonoBehaviour
    {
        [Header("Target References")]
        [SerializeField] private ChaserController chaserController;
        [SerializeField] private PlayerChaseController playerController;

        [Header("Camera Offsets & Damping")]
        [Tooltip("Horizontal offset from target midpoint")]
        [SerializeField] private float offsetX = 3.0f;
        [SerializeField] private float cameraSmoothSpeed = 5.0f;

        [Header("Y Position Options")]
        [SerializeField] private bool lockCameraY = true;
        [SerializeField] private float fixedCameraY = 0f;

        private void Start()
        {
            if (chaserController == null) chaserController = FindFirstObjectByType<ChaserController>();
            if (playerController == null) playerController = FindFirstObjectByType<PlayerChaseController>();

            if (lockCameraY)
            {
                fixedCameraY = transform.position.y;
            }

            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (chaserController == null) return;

            float targetX;
            if (playerController != null)
            {
                // Follow midpoint between Chaser and Player for ideal framing
                targetX = (chaserController.TrackX + playerController.transform.position.x) * 0.5f + offsetX;
            }
            else
            {
                targetX = chaserController.TrackX + offsetX;
            }

            float targetY = lockCameraY ? fixedCameraY : (chaserController.transform.position.y);
            Vector3 desiredPos = new Vector3(targetX, targetY, transform.position.z);

            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * cameraSmoothSpeed);
        }

        public void SnapToTarget()
        {
            if (chaserController == null) return;

            float targetX = chaserController.TrackX + offsetX;
            float targetY = lockCameraY ? fixedCameraY : transform.position.y;
            transform.position = new Vector3(targetX, targetY, transform.position.z);
        }
    }
}
