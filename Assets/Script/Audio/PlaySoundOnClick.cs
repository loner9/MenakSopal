using UnityEngine;
using UnityEngine.UI;

namespace MenakSopal.Audio
{
    [RequireComponent(typeof(Button))]
    public class PlaySoundOnClick : MonoBehaviour
    {
        [SerializeField] private string soundName;

        private void Start()
        {
            Button btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(PlaySound);
            }
        }

        private void PlaySound()
        {
            if (AudioSystem.Instance != null && !string.IsNullOrEmpty(soundName))
            {
                AudioSystem.Instance.PlaySFX(soundName);
            }
        }
    }
}
