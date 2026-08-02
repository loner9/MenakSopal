using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MenakSopal.Audio
{
    /// <summary>
    /// Plays an SFX when interacting with a UI element or Button.
    /// Supports playing on Pointer Down (press down), Pointer Up (release), Click (standard button click), or Pointer Enter (hover).
    /// </summary>
    public class PlaySoundOnClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
    {
        public enum TriggerType
        {
            [Tooltip("Plays immediately when the button/element is pressed down")]
            OnPointerDown,
            [Tooltip("Plays when the button press is released")]
            OnPointerUp,
            [Tooltip("Plays on standard Unity UI Button.onClick event")]
            OnClick,
            [Tooltip("Plays when the mouse/pointer hovers over the element")]
            OnPointerEnter
        }

        [Header("Audio Settings")]
        [SerializeField] private string soundName;

        [Header("Trigger Option")]
        [SerializeField] private TriggerType triggerType = TriggerType.OnPointerDown;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void Start()
        {
            if (triggerType == TriggerType.OnClick && button != null)
            {
                button.onClick.AddListener(PlaySound);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (triggerType == TriggerType.OnPointerDown)
            {
                if (button == null || button.interactable)
                {
                    PlaySound();
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (triggerType == TriggerType.OnPointerUp)
            {
                if (button == null || button.interactable)
                {
                    PlaySound();
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (triggerType == TriggerType.OnPointerEnter)
            {
                if (button == null || button.interactable)
                {
                    PlaySound();
                }
            }
        }

        public void PlaySound()
        {
            if (AudioSystem.Instance != null && !string.IsNullOrEmpty(soundName))
            {
                AudioSystem.Instance.PlaySFX(soundName);
            }
        }
    }
}
