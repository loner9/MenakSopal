using UnityEngine;

namespace MenakSopal.ChaseMinigame
{
    /// <summary>
    /// Individual obstacle moving left across lanes.
    /// Handles collision with player and despawning behind the chaser.
    /// </summary>
    public class ObstacleItem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float despawnX = -9.0f;

        private float moveSpeed = 8.0f;
        private ObstaclePool poolOwner;

        public void Initialize(float speed, ObstaclePool pool)
        {
            moveSpeed = speed;
            poolOwner = pool;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            // Move left
            transform.Translate(Vector3.left * (moveSpeed * Time.deltaTime));

            // Despawn check when passing left boundary
            if (transform.position.x <= despawnX)
            {
                Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerChaseController>(out var player))
            {
                player.TakeHit();
                Despawn();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerChaseController>(out var player))
            {
                player.TakeHit();
                Despawn();
            }
        }

        public void Despawn()
        {
            gameObject.SetActive(false);
            if (poolOwner != null)
            {
                poolOwner.ReturnObstacle(this);
            }
        }

        public void SetDespawnX(float x)
        {
            despawnX = x;
        }
    }
}
