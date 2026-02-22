using UnityEngine;

namespace MenakSopal.Audio
{
    [RequireComponent(typeof(Collider2D))]
    public class MusicZone : MonoBehaviour
    {
        [SerializeField] private string bmgName;
        [SerializeField] private float fadeDuration = 1.0f;
        [SerializeField] private bool playOnlyOnce = false;

        private bool hasPlayed = false;

        private void Start()
        {
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (playOnlyOnce && hasPlayed) return;

                if (AudioSystem.Instance != null && !string.IsNullOrEmpty(bmgName))
                {
                    AudioSystem.Instance.PlayMusic(bmgName, fadeDuration);
                    hasPlayed = true;
                }
            }
        }
    }
}
