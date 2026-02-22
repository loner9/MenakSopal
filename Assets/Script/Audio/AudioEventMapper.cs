using System.Collections.Generic;
using UnityEngine;

namespace MenakSopal.Audio
{
    public class AudioEventMapper : MonoBehaviour
    {
        public enum EventSourceType
        {
            Quest,
            Flag,
            NPC,
            Enemy
        }

        [System.Serializable]
        public class EventMapping
        {
            public EventSourceType sourceType;
            public string eventKey; // e.g., Quest ID or Flag Name
            public string soundName; // Sound Group from Library
            public bool playAsMusic = false;
            public float musicFadeDuration = 1.0f;
        }

        [SerializeField] private List<EventMapping> mappings = new List<EventMapping>();

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            QuestEvents.OnQuestStarted += HandleQuestStarted;
            QuestEvents.OnQuestCompleted += HandleQuestCompleted;
            FlagEvents.OnFlagAdded += HandleFlagAdded;
            NPCEvents.OnNPCSpawned += HandleNPCSpawned;
            NPCEvents.OnNPCDespawned += HandleNPCDespawned;
            EnemyEvents.OnEnemyDied += HandleEnemyDied;
        }

        private void UnsubscribeFromEvents()
        {
            QuestEvents.OnQuestStarted -= HandleQuestStarted;
            QuestEvents.OnQuestCompleted -= HandleQuestCompleted;
            FlagEvents.OnFlagAdded -= HandleFlagAdded;
            NPCEvents.OnNPCSpawned -= HandleNPCSpawned;
            NPCEvents.OnNPCDespawned -= HandleNPCDespawned;
            EnemyEvents.OnEnemyDied -= HandleEnemyDied;
        }

        private void HandleQuestStarted(QuestData quest)
        {
            TryPlaySound(EventSourceType.Quest, quest.questID);
        }

        private void HandleQuestCompleted(QuestData quest)
        {
            TryPlaySound(EventSourceType.Quest, quest.questID + "_completed");
        }

        private void HandleFlagAdded(string flag)
        {
            TryPlaySound(EventSourceType.Flag, flag);
        }

        private void HandleNPCSpawned(NPC npc)
        {
            TryPlaySound(EventSourceType.NPC, npc.npcName + "_spawned");
        }

        private void HandleNPCDespawned(NPC npc)
        {
            TryPlaySound(EventSourceType.NPC, npc.npcName + "_despawned");
        }

        private void HandleEnemyDied(Enemy enemy, string enemyType)
        {
            TryPlaySound(EventSourceType.Enemy, enemy.gameObject.name + "_died");
        }

        private void TryPlaySound(EventSourceType type, string key)
        {
            foreach (var mapping in mappings)
            {
                if (mapping.sourceType == type && mapping.eventKey == key)
                {
                    if (AudioSystem.Instance != null)
                    {
                        if (mapping.playAsMusic)
                        {
                            AudioSystem.Instance.PlayMusic(mapping.soundName, mapping.musicFadeDuration);
                        }
                        else
                        {
                            AudioSystem.Instance.PlaySFX(mapping.soundName);
                        }
                    }
                }
            }
        }
    }
}
