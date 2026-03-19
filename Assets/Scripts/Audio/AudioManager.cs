using System.Collections.Generic;
using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private SfxLibrary _library;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _musicSource;

        [Header("Music Clips")]
        [SerializeField] private AudioClip _menuMusic;
        [SerializeField] private AudioClip _gameplayMusic;
        [SerializeField] private AudioClip _bossMusic;

        [Header("Mix")]
        [Range(0f, 1f)][SerializeField] private float _sfxVolume = 1f;
        [Range(0f, 1f)][SerializeField] private float _musicVolume = 0.55f;

        private readonly Dictionary<SfxId, float> _lastPlayTimes = new();
        private MusicMood _currentMood = MusicMood.None;

        private void Awake()
        {
            ServiceLocator.Register<AudioManager>(this);
            if (_sfxSource == null)
            {
                var go = new GameObject("SfxSource");
                go.transform.SetParent(transform, false);
                _sfxSource = go.AddComponent<AudioSource>();
                _sfxSource.playOnAwake = false;
            }
            if (_musicSource == null)
            {
                var go = new GameObject("MusicSource");
                go.transform.SetParent(transform, false);
                _musicSource = go.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
            }
            _musicSource.volume = _musicVolume;
        }

        private void OnDestroy() => ServiceLocator.Unregister<AudioManager>();

        public void PlaySfx(SfxId id)
        {
            if (_library == null || _sfxSource == null) return;
            var clip = _library.Pick(id, out var vol, out var pitch, out var minInterval);
            if (clip == null) return;

            float now = Time.unscaledTime;
            if (_lastPlayTimes.TryGetValue(id, out var last) && now - last < minInterval) return;
            _lastPlayTimes[id] = now;

            _sfxSource.pitch = pitch;
            _sfxSource.PlayOneShot(clip, vol * _sfxVolume);
        }

        public void PlayMusic(MusicMood mood)
        {
            if (_currentMood == mood) return;
            _currentMood = mood;
            var clip = ResolveMusic(mood);
            if (_musicSource == null) return;
            if (clip == null) { _musicSource.Stop(); return; }
            _musicSource.clip = clip;
            _musicSource.volume = _musicVolume;
            _musicSource.Play();
        }

        private AudioClip ResolveMusic(MusicMood mood) => mood switch
        {
            MusicMood.Menu => _menuMusic,
            MusicMood.Gameplay => _gameplayMusic,
            MusicMood.Boss => _bossMusic,
            _ => null
        };

        public static void Play(SfxId id)
        {
            if (ServiceLocator.TryGet<AudioManager>(out var am)) am.PlaySfx(id);
        }

        public static void Music(MusicMood mood)
        {
            if (ServiceLocator.TryGet<AudioManager>(out var am)) am.PlayMusic(mood);
        }
    }
}
