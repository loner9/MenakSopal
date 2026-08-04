using UnityEngine;

namespace MenakSopal.ChaseMinigame
{
    /// <summary>
    /// Handles periodic obstacle spawning across the 4 lanes with difficulty scaling over time.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LaneManager laneManager;
        [SerializeField] private ObstaclePool obstaclePool;

        [Header("Spawn Positions")]
        [SerializeField] private float spawnX = 12.0f;
        [SerializeField] private float despawnX = -9.0f;

        [Header("Difficulty Curves")]
        [SerializeField] private float initialSpawnInterval = 2.0f;
        [SerializeField] private float minSpawnInterval = 0.75f;
        [SerializeField] private float intervalDecreaseRate = 0.05f; // per second

        [SerializeField] private float initialObstacleSpeed = 7.0f;
        [SerializeField] private float maxObstacleSpeed = 15.0f;
        [SerializeField] private float speedIncreaseRate = 0.2f; // per second

        private bool isSpawning = false;
        private float spawnTimer = 0f;
        private float currentInterval;
        private float currentSpeed;

        private void Start()
        {
            if (laneManager == null) laneManager = FindFirstObjectByType<LaneManager>();
            if (obstaclePool == null) obstaclePool = FindFirstObjectByType<ObstaclePool>();

            ResetDifficulty();
        }

        private void Update()
        {
            if (!isSpawning || laneManager == null || obstaclePool == null) return;

            // Ramp up difficulty over time
            currentInterval = Mathf.Max(minSpawnInterval, currentInterval - (intervalDecreaseRate * Time.deltaTime));
            currentSpeed = Mathf.Min(maxObstacleSpeed, currentSpeed + (speedIncreaseRate * Time.deltaTime));

            // Timer
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= currentInterval)
            {
                spawnTimer = 0f;
                SpawnObstacle();
            }
        }

        private void SpawnObstacle()
        {
            int randomLane = Random.Range(0, laneManager.LaneCount);
            float spawnY = laneManager.GetLaneY(randomLane);

            ObstacleItem obstacle = obstaclePool.GetObstacle();
            if (obstacle != null)
            {
                obstacle.transform.position = new Vector3(spawnX, spawnY, 0f);
                obstacle.SetDespawnX(despawnX);
                obstacle.Initialize(currentSpeed, obstaclePool);
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

        public void ResetDifficulty()
        {
            currentInterval = initialSpawnInterval;
            currentSpeed = initialObstacleSpeed;
            spawnTimer = 0f;
        }

        public float CurrentSpeed => currentSpeed;
    }
}
