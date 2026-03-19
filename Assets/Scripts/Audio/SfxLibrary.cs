using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivorSeries.Audio
{
    [CreateAssetMenu(menuName = "SurvivorSeries/Sfx Library", fileName = "SO_SfxLibrary")]
    public class SfxLibrary : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public SfxId Id;
            public AudioClip[] Clips;
            [Range(0f, 1f)] public float Volume = 1f;
            [Range(0f, 0.5f)] public float PitchVariance = 0.05f;
            [Range(0f, 0.5f)] public float MinInterval = 0.05f;
        }

        [SerializeField] private Entry[] _entries;

        private Dictionary<SfxId, Entry> _map;

        public AudioClip Pick(SfxId id, out float volume, out float pitch, out float minInterval)
        {
            volume = 1f;
            pitch = 1f;
            minInterval = 0.05f;
            if (_entries == null || _entries.Length == 0) return null;
            EnsureMap();
            if (!_map.TryGetValue(id, out var entry) || entry == null) return null;
            if (entry.Clips == null || entry.Clips.Length == 0) return null;

            var clip = entry.Clips[UnityEngine.Random.Range(0, entry.Clips.Length)];
            volume = entry.Volume;
            minInterval = entry.MinInterval;
            float v = entry.PitchVariance;
            pitch = 1f + UnityEngine.Random.Range(-v, v);
            return clip;
        }

        private void EnsureMap()
        {
            if (_map != null) return;
            _map = new Dictionary<SfxId, Entry>();
            foreach (var e in _entries) if (e != null) _map[e.Id] = e;
        }

        public void InvalidateCache() => _map = null;
    }
}
