using UnityEngine;

namespace MenakSopal.ChaseMinigame
{
    /// <summary>
    /// Manages Y-coordinates for fixed horizontal lanes (0 to 3, top to bottom).
    /// </summary>
    public class LaneManager : MonoBehaviour
    {
        [Header("Lane Setup")]
        [SerializeField] private int laneCount = 4;
        [SerializeField] private float topLaneY = 3.0f;
        [SerializeField] private float laneSpacing = 2.0f;

        public int LaneCount => laneCount;
        public float LaneSpacing => laneSpacing;
        public float TopLaneY => topLaneY;

        /// <summary>
        /// Gets the world Y coordinate for a given lane index (0 = Topmost, 3 = Bottommost).
        /// </summary>
        public float GetLaneY(int laneIndex)
        {
            laneIndex = Mathf.Clamp(laneIndex, 0, laneCount - 1);
            return topLaneY - (laneIndex * laneSpacing);
        }

        public bool IsValidLane(int laneIndex)
        {
            return laneIndex >= 0 && laneIndex < laneCount;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < laneCount; i++)
            {
                float y = GetLaneY(i);
                Gizmos.DrawLine(new Vector3(-20f, y, 0f), new Vector3(20f, y, 0f));
            }
        }
    }
}
