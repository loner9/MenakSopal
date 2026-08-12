using UnityEngine;

namespace MenakSopal.ChaseMinigame
{
    /// <summary>
    /// Spawns dynamic obstacles ahead of the advancing Chaser/Player on the track.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LaneManager laneManager;
        [SerializeField] private ObstaclePool obstaclePool;
        [SerializeField] private ChaserController chaserController;

        [Header("Spawn Settings")]
        [Tooltip("Distance ahead of Chaser where obstacles spawn")]
        [SerializeField] private float spawnAheadDistance = 18.0f;
        [SerializeField] private float relativeMoveSpeed = 2.0f; // Extra leftward speed relative to track

        [Header("Difficulty Scaling")]
        [SerializeField] private float initialSpawnInterval = 2.0f;
        [SerializeField] private float minSpawnInterval = 0.75f;
        [SerializeField] private float intervalDecreaseRate = 0.05f;

        [SerializeField] private float initialRunSpeed = 8.0f;
        [SerializeField] private float maxRunSpeed = 16.0f;
        [SerializeField] private float speedIncreaseRate = 0.25f;

        private bool isSpawning = false;
        private float spawnTimer = 0f;
        private float currentInterval;
        private float currentSpeed;

        public float CurrentSpeed => currentSpeed;

        private void Start()
        {
            if (laneManager == null) laneManager = FindFirstObjectByType<LaneManager>();
            if (obstaclePool == null) obstaclePool = FindFirstObjectByType<ObstaclePool>();
            if (chaserController == null) chaserController = FindFirstObjectByType<ChaserController>();

            ResetDifficulty();
        }

        private void Update()
        {
            if (!isSpawning || laneManager == null || obstaclePool == null || chaserController == null) return;

            // Difficulty escalation over time
            currentInterval = Mathf.Max(minSpawnInterval, currentInterval - (intervalDecreaseRate * Time.deltaTime));
            currentSpeed = Mathf.Min(maxRunSpeed, currentSpeed + (speedIncreaseRate * Time.deltaTime));

            // Update Chaser run speed
            chaserController.SetRunSpeed(currentSpeed);

            // Timer for spawning obstacles ahead of Chaser
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= currentInterval)
            {
                spawnTimer = 0f;
                SpawnObstacleAhead();
            }
        }

        private void SpawnObstacleAhead()
        {
            int randomLane = Random.Range(0, laneManager.LaneCount);
            float spawnY = laneManager.GetLaneY(randomLane);
            float spawnX = chaserController.TrackX + spawnAheadDistance;

            ObstacleItem obstacle = obstaclePool.GetObstacle();
            if (obstacle != null)
            {
                obstacle.transform.position = new Vector3(spawnX, spawnY, 0f);
                obstacle.Initialize(relativeMoveSpeed, obstaclePool, chaserController);
            }
        }

        public void StartSpawning()
        {
            isSpawning = true;
            spawnTimer = 0f;
        }

        public void StopSpawning()
        {
            isSpawning = false;
        }

        /// <summary>
        /// Stops spawning and immediately despawns all active obstacles currently on the track.
        /// </summary>
        public void ClearAllObstacles()
        {
            StopSpawning();

            ObstacleItem[] activeObstacles = FindObjectsByType<ObstacleItem>(FindObjectsSortMode.None);
            foreach (var obstacle in activeObstacles)
            {
                if (obstacle != null && obstacle.gameObject.activeSelf)
                {
                    obstacle.Despawn();
                }
            }
        }

        public void ResetDifficulty()
        {
            currentInterval = initialSpawnInterval;
            currentSpeed = initialRunSpeed;
            spawnTimer = 0f;

            if (chaserController != null)
            {
                chaserController.SetRunSpeed(currentSpeed);
            }
        }
    }
}
