using System;
using UnityEngine;

namespace MenakSopal.ChaseMinigame
{
    /// <summary>
    /// Goal object spawned at the end of a finite distance runner level.
    /// </summary>
    public class FinishLine : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 8f;
        private bool isActive = false;

        public event Action OnPlayerCrossedFinishLine;

        public void Initialize(float speed)
        {
            moveSpeed = speed;
            isActive = true;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!isActive) return;

            transform.Translate(Vector3.left * (moveSpeed * Time.deltaTime));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerChaseController>(out _))
            {
                TriggerFinish();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerChaseController>(out _))
            {
                TriggerFinish();
            }
        }

        private void TriggerFinish()
        {
            if (!isActive) return;
            isActive = false;
            OnPlayerCrossedFinishLine?.Invoke();
        }
    }
}
