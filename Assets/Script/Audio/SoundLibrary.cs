using System.Collections.Generic;
using UnityEngine;

namespace MenakSopal.Audio
{
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
    public class SoundLibrary : ScriptableObject
    {
        [System.Serializable]
        public class SoundGroup
        {
            public string groupName;
            public List<AudioClip> clips = new List<AudioClip>();

            [Range(0f, 1f)] public float volume = 1f;
            [Range(0.1f, 3f)] public float pitch = 1f;

            [Header("Variation")]
            [Range(0f, 0.5f)] public float volumeVariance = 0.1f;
            [Range(0f, 0.5f)] public float pitchVariance = 0.1f;

            public AudioClip GetRandomClip()
            {
                if (clips == null || clips.Count == 0) return null;
                return clips[Random.Range(0, clips.Count)];
            }
        }

        public List<SoundGroup> soundGroups = new List<SoundGroup>();

        public SoundGroup GetSoundGroup(string name)
        {
            return soundGroups.Find(g => g.groupName == name);
        }
    }
}
