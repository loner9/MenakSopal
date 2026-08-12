using UnityEngine;

namespace MenakSopal.ChaseMinigame
{
    /// <summary>
    /// Individual obstacle instance on the track.
    /// Supports both static track obstacles and moving obstacles.
    /// </summary>
    public class ObstacleItem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float despawnBehindChaserDistance = 5.0f;

        private float moveSpeed = 0f; // Additional movement relative to track
        private ObstaclePool poolOwner;
        private ChaserController chaserRef;

        public void Initialize(float speed, ObstaclePool pool, ChaserController chaser)
        {
            moveSpeed = speed;
            poolOwner = pool;
            chaserRef = chaser;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            // Additional move speed (if dynamic moving obstacle)
            if (moveSpeed != 0f)
            {
                transform.Translate(Vector3.left * (moveSpeed * Time.deltaTime));
            }

            // Despawn check relative to Chaser's advancing track position
            if (chaserRef != null)
            {
                if (transform.position.x <= chaserRef.TrackX - despawnBehindChaserDistance)
                {
                    Despawn();
                }
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
    }
}
