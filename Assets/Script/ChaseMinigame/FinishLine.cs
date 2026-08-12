using System;
using UnityEngine;

namespace MenakSopal.ChaseMinigame
{
    /// <summary>
    /// Static or spawned finish line object positioned at the end of the track.
    /// </summary>
    public class FinishLine : MonoBehaviour
    {
        private bool isActive = true;

        public event Action OnPlayerCrossedFinishLine;

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("Name :" + other.name);
            if (other.TryGetComponent<PlayerChaseController>(out _) || other.name.Contains("Player"))
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

        public void ResetTrigger()
        {
            isActive = true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(transform.position.x, -10f, 0f), new Vector3(transform.position.x, 10f, 0f));
            Gizmos.DrawCube(transform.position, new Vector3(0.5f, 8f, 1f));
        }
    }
}
