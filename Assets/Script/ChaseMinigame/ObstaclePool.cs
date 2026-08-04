using System.Collections.Generic;
using UnityEngine;

namespace MenakSopal.ChaseMinigame
{
    /// <summary>
    /// Object Pool manager for recycling ObstacleItem instances.
    /// </summary>
    public class ObstaclePool : MonoBehaviour
    {
        [Header("Pool Setup")]
        [SerializeField] private ObstacleItem obstaclePrefab;
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private Transform poolParent;

        private readonly Queue<ObstacleItem> poolQueue = new Queue<ObstacleItem>();

        private void Start()
        {
            if (poolParent == null)
            {
                poolParent = transform;
            }

            PrewarmPool();
        }

        private void PrewarmPool()
        {
            if (obstaclePrefab == null) return;

            for (int i = 0; i < initialPoolSize; i++)
            {
                ObstacleItem newItem = Instantiate(obstaclePrefab, poolParent);
                newItem.gameObject.SetActive(false);
                poolQueue.Enqueue(newItem);
            }
        }

        /// <summary>
        /// Gets an obstacle instance from the pool or creates a new one if empty.
        /// </summary>
        public ObstacleItem GetObstacle()
        {
            ObstacleItem item;

            if (poolQueue.Count > 0)
            {
                item = poolQueue.Dequeue();
            }
            else
            {
                item = Instantiate(obstaclePrefab, poolParent);
            }

            return item;
        }

        /// <summary>
        /// Returns an obstacle to the pool for reuse.
        /// </summary>
        public void ReturnObstacle(ObstacleItem item)
        {
            item.gameObject.SetActive(false);
            poolQueue.Enqueue(item);
        }
    }
}
