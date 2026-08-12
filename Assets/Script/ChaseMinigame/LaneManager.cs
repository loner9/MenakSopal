using UnityEngine;

namespace MenakSopal.ChaseMinigame
{
    public enum LaneMode
    {
        CalculatedSpacing,
        CustomYPositions,
        CustomTransforms
    }

    /// <summary>
    /// Manages Y-coordinates for fixed horizontal lanes.
    /// Supports calculated even spacing, custom float Y values array, or Transform anchors.
    /// </summary>
    public class LaneManager : MonoBehaviour
    {
        [Header("Lane Mode")]
        [SerializeField] private LaneMode mode = LaneMode.CalculatedSpacing;

        [Header("Calculated Spacing Setup")]
        [SerializeField] private int defaultLaneCount = 4;
        [SerializeField] private float topLaneY = 3.0f;
        [SerializeField] private float laneSpacing = 2.0f;

        [Header("Custom Positions (Inspector / Runtime)")]
        [Tooltip("Custom Y coordinate for each lane index")]
        [SerializeField] private float[] customLaneYPositions;

        [Tooltip("Transform anchors representing each lane position")]
        [SerializeField] private Transform[] laneTransforms;

        public LaneMode Mode => mode;
        public float LaneSpacing => laneSpacing;
        public float TopLaneY => topLaneY;

        /// <summary>
        /// Total number of available lanes.
        /// </summary>
        public int LaneCount
        {
            get
            {
                switch (mode)
                {
                    case LaneMode.CustomYPositions:
                        return (customLaneYPositions != null && customLaneYPositions.Length > 0)
                            ? customLaneYPositions.Length
                            : defaultLaneCount;
                    case LaneMode.CustomTransforms:
                        return (laneTransforms != null && laneTransforms.Length > 0)
                            ? laneTransforms.Length
                            : defaultLaneCount;
                    default:
                        return defaultLaneCount;
                }
            }
        }

        /// <summary>
        /// Gets the world Y coordinate for a given lane index (0 = Topmost / Lane 0).
        /// </summary>
        public float GetLaneY(int laneIndex)
        {
            int totalLanes = LaneCount;
            laneIndex = Mathf.Clamp(laneIndex, 0, totalLanes - 1);

            switch (mode)
            {
                case LaneMode.CustomYPositions:
                    if (customLaneYPositions != null && laneIndex < customLaneYPositions.Length)
                    {
                        return customLaneYPositions[laneIndex];
                    }
                    break;

                case LaneMode.CustomTransforms:
                    if (laneTransforms != null && laneIndex < laneTransforms.Length && laneTransforms[laneIndex] != null)
                    {
                        return laneTransforms[laneIndex].position.y;
                    }
                    break;
            }

            // Fallback to calculated spacing
            return topLaneY - (laneIndex * laneSpacing);
        }

        /// <summary>
        /// Set custom Y positions array at runtime.
        /// </summary>
        public void SetCustomYPositions(float[] yPositions)
        {
            customLaneYPositions = yPositions;
            mode = LaneMode.CustomYPositions;
        }

        /// <summary>
        /// Set custom lane transforms at runtime.
        /// </summary>
        public void SetLaneTransforms(Transform[] transforms)
        {
            laneTransforms = transforms;
            mode = LaneMode.CustomTransforms;
        }

        public bool IsValidLane(int laneIndex)
        {
            return laneIndex >= 0 && laneIndex < LaneCount;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            int total = LaneCount;
            for (int i = 0; i < total; i++)
            {
                float y = GetLaneY(i);
                Gizmos.DrawLine(new Vector3(-20f, y, 0f), new Vector3(20f, y, 0f));
            }
        }
    }
}
