// AudioManager.cs
// Professional audio management system with pooling and mixing
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// FEATURES:
// - Background music looping with crossfade
// - Sound effect pooling (zero GC allocations)
// - Volume control (master, music, SFX)
// - Audio mixing via Unity Audio Mixer
// - Pre-warmed sound effect pool
//
// SETUP:
// 1. Create Audio folder structure (see TODO.md)
// 2. Add audio clips to appropriate folders
// 3. Create AudioMixer (Assets > Create > Audio Mixer)
// 4. Attach this script to "AudioManager" GameObject

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Code.Lavos.Core
{
    /// <summary>
    /// AUDIOMANAGER - Professional audio management with pooling.
    /// Singleton pattern (DontDestroyOnLoad) for persistent audio across scenes.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Singleton Pattern

        private static AudioManager _instance;
        private static bool _applicationIsQuitting = false;

        public static AudioManager Instance
        {
            get
            {
                if (_instance == null && !_applicationIsQuitting)
                {
                    _instance = FindFirstObjectByType<AudioManager>();

                    if (_instance == null)
                    {
                        var go = new GameObject("AudioManager");
                        _instance = go.AddComponent<AudioManager>();
                        Debug.Log("[AudioManager] Created new AudioManager instance");
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Inspector Settings

        [Header("Audio Mixer")]
        [Tooltip("Master Audio Mixer (create via Assets > Create > Audio Mixer)")]
        [SerializeField] private AudioMixer masterMixer;

        [Tooltip("Music volume exponent (controls mixer volume curve)")]
        [SerializeField] private float musicVolumeExponent = 2f;

        [Tooltip("SFX volume exponent (controls mixer volume curve)")]
        [SerializeField] private float sfxVolumeExponent = 2f;

        [Header("Background Music")]
        [Tooltip("Playlist for background music (looped)")]
        [SerializeField] private List<AudioClip> musicPlaylist = new List<AudioClip>();

        [Tooltip("Crossfade duration between tracks (seconds)")]
        [SerializeField] private float crossfadeDuration = 1f;

        [Tooltip("Enable music crossfading")]
        [SerializeField] private bool enableCrossfade = true;

        [Header("Sound Effect Pool")]
        [Tooltip("Pre-warm sound effect pool at start (zero runtime GC)")]
        [SerializeField] private bool prewarmSFXPool = true;

        [Tooltip("Initial pool size per sound type")]
        [SerializeField] private int initialPoolSizePerType = 5;

        [Tooltip("Maximum pool size per sound type")]
        [SerializeField] private int maxPoolSizePerType = 20;

        [Tooltip("Expand pool if needed (or fail if false)")]
        [SerializeField] private bool canExpandSFXPool = true;

        [Header("Debug")]
        [Tooltip("Show audio events in console")]
        [SerializeField] private bool debugAudio = false;

        #endregion

        #region Audio Sources

        // Music sources (2 for crossfading)
        private AudioSource _musicSourceA;
        private AudioSource _musicSourceB;
        private AudioSource _activeMusicSource;
        private AudioSource _inactiveMusicSource;

        // SFX pool
        private readonly Queue<AudioSource> _sfxPool = new Queue<AudioSource>();
        private readonly List<AudioSource> _activeSFX = new List<AudioSource>();

        #endregion

        #region Volume Settings

        private float _masterVolume = 1f;
        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;

        #endregion

        #region Music State

        private int _currentMusicIndex = 0;
        private bool _isMusicPlaying = false;
        private bool _isCrossfading = false;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            // Singleton setup
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSystem();
        }

        void Start()
        {
            if (prewarmSFXPool)
            {
                PrewarmSFXPool(initialPoolSizePerType * 5); // Pool for 5 sound types
                Debug.Log($"[AudioManager] ✅ Pre-warmed SFX pool ({_sfxPool.Count} sources)");
            }

            // Start background music
            if (musicPlaylist.Count > 0)
            {
                PlayBackgroundMusic();
            }
        }

        void OnDestroy()
        {
            _applicationIsQuitting = true;

            // Clean up audio sources
            foreach (var source in _sfxPool)
            {
                if (source != null)
                    Destroy(source.gameObject);
            }
            _sfxPool.Clear();
            _activeSFX.Clear();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize audio system (mixer, sources, volumes).
        /// </summary>
        private void InitializeAudioSystem()
        {
            // Create music sources
            CreateMusicSources();

            // Set initial volumes
            SetMasterVolume(_masterVolume);
            SetMusicVolume(_musicVolume);
            SetSFXVolume(_sfxVolume);

            Debug.Log("[AudioManager] ✅ Audio system initialized");
        }

        private void CreateMusicSources()
        {
            // Music Source A
            var goA = new GameObject("MusicSource_A");
            goA.transform.SetParent(transform);
            _musicSourceA = goA.AddComponent<AudioSource>();
            _musicSourceA.playOnAwake = false;
            _musicSourceA.loop = true;
            _musicSourceA.spatialBlend = 0f; // 2D sound
            _musicSourceA.priority = 0; // Highest priority

            // Music Source B (for crossfade)
            var goB = new GameObject("MusicSource_B");
            goB.transform.SetParent(transform);
            _musicSourceB = goB.AddComponent<AudioSource>();
            _musicSourceB.playOnAwake = false;
            _musicSourceB.loop = true;
            _musicSourceB.spatialBlend = 0f;
            _musicSourceB.priority = 0;

            _activeMusicSource = _musicSourceA;
            _inactiveMusicSource = _musicSourceB;
        }

        #endregion

        #region Pool Management

        /// <summary>
        /// Pre-warm sound effect pool (zero runtime GC allocations).
        /// </summary>
        public void PrewarmSFXPool(int count)
        {
            Debug.Log($"[AudioManager] Pre-warming {count} SFX sources...");

            for (int i = 0; i < count; i++)
            {
                var source = CreateSFXSource();
                _sfxPool.Enqueue(source);
            }

            Debug.Log($"[AudioManager] ✅ SFX pool pre-warmed: {count} sources");
        }

        private AudioSource CreateSFXSource()
        {
            var go = new GameObject("SFX_Pooled");
            go.transform.SetParent(transform);

            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f; // 2D by default (can be changed per SFX)
            source.priority = 128; // Normal priority

            return source;
        }

        private AudioSource GetSFXSource()
        {
            AudioSource source;

            // Try to get from pool
            if (_sfxPool.Count > 0)
            {
                source = _sfxPool.Dequeue();
                if (debugAudio)
                    Debug.Log($"[AudioManager] ♻️ SFX reused from pool (remaining: {_sfxPool.Count})");
            }
            else if (canExpandSFXPool)
            {
                source = CreateSFXSource();
                if (debugAudio)
                    Debug.Log($"[AudioManager] 🆕 SFX source created (pool was empty)");
            }
            else
            {
                Debug.LogWarning("[AudioManager] ⚠️ SFX pool exhausted!");
                return null;
            }

            return source;
        }

        private void ReturnSFXSource(AudioSource source)
        {
            if (source == null) return;

            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);

            _activeSFX.Remove(source);

            // Return to pool if not too large
            if (_sfxPool.Count < maxPoolSizePerType * 10)
            {
                _sfxPool.Enqueue(source);
                if (debugAudio)
                    Debug.Log($"[AudioManager] ♻️ SFX returned to pool (size: {_sfxPool.Count})");
            }
            else
            {
                Destroy(source.gameObject);
                if (debugAudio)
                    Debug.Log($"[AudioManager] 🗑️ SFX destroyed (pool full)");
            }
        }

        #endregion

        #region Background Music

        /// <summary>
        /// Play background music from playlist (looping with optional crossfade).
        /// </summary>
        public void PlayBackgroundMusic(int startIndex = 0, float fadeDuration = 2f)
        {
            if (musicPlaylist.Count == 0)
            {
                Debug.LogWarning("[AudioManager] No music in playlist!");
                return;
            }

            _currentMusicIndex = Mathf.Clamp(startIndex, 0, musicPlaylist.Count - 1);
            var clip = musicPlaylist[_currentMusicIndex];

            if (debugAudio)
                Debug.Log($"[AudioManager] 🎵 Playing music: {clip.name}");

            _activeMusicSource.clip = clip;
            _activeMusicSource.Play();
            _isMusicPlaying = true;

            // Fade in
            if (enableCrossfade && fadeDuration > 0)
            {
                StartCoroutine(FadeVolume(_activeMusicSource, 0f, 1f, fadeDuration));
            }
        }

        /// <summary>
        /// Play next track in playlist with crossfade.
        /// </summary>
        public void PlayNextTrack()
        {
            if (musicPlaylist.Count == 0) return;

            _currentMusicIndex = (_currentMusicIndex + 1) % musicPlaylist.Count;
            CrossfadeToTrack(musicPlaylist[_currentMusicIndex]);
        }

        /// <summary>
        /// Crossfade to specific track.
        /// </summary>
        private void CrossfadeToTrack(AudioClip newTrack)
        {
            if (_isCrossfading) return;

            _isCrossfading = true;

            if (debugAudio)
                Debug.Log($"[AudioManager] 🎵 Crossfading to: {newTrack.name}");

            // Set up inactive source
            _inactiveMusicSource.clip = newTrack;
            _inactiveMusicSource.Play();

            // Crossfade
            StartCoroutine(CrossfadeSources(crossfadeDuration));
        }

        private System.Collections.IEnumerator CrossfadeSources(float duration)
        {
            float elapsed = 0f;
            float startVolumeA = _activeMusicSource.volume;
            float startVolumeB = 0f;

            _inactiveMusicSource.volume = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                _activeMusicSource.volume = Mathf.Lerp(startVolumeA, 0f, t);
                _inactiveMusicSource.volume = Mathf.Lerp(startVolumeB, 1f, t);

                yield return null;
            }

            _activeMusicSource.Stop();

            // Swap sources
            var temp = _activeMusicSource;
            _activeMusicSource = _inactiveMusicSource;
            _inactiveMusicSource = temp;

            _isCrossfading = false;
        }

        /// <summary>
        /// Stop background music.
        /// </summary>
        public void StopMusic(float fadeDuration = 1f)
        {
            if (!_isMusicPlaying) return;

            if (enableCrossfade && fadeDuration > 0)
            {
                StartCoroutine(FadeVolume(_activeMusicSource, _activeMusicSource.volume, 0f, fadeDuration, () =>
                {
                    _activeMusicSource.Stop();
                    _isMusicPlaying = false;
                }));
            }
            else
            {
                _activeMusicSource.Stop();
                _isMusicPlaying = false;
            }

            if (debugAudio)
                Debug.Log("[AudioManager] ⏹️ Music stopped");
        }

        /// <summary>
        /// Pause/resume background music.
        /// </summary>
        public void PauseMusic(bool pause)
        {
            if (pause)
                _activeMusicSource.Pause();
            else
                _activeMusicSource.UnPause();
            
            if (debugAudio)
                Debug.Log(pause ? "[AudioManager] ⏸️ Music paused" : "[AudioManager] ▶️ Music resumed");
        }

        private System.Collections.IEnumerator FadeVolume(AudioSource source, float from, float to, float duration, System.Action onComplete = null)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                source.volume = Mathf.Lerp(from, to, t);
                yield return null;
            }

            source.volume = to;
            onComplete?.Invoke();
        }

        #endregion

        #region Sound Effects

        /// <summary>
        /// Play sound effect (pooled, zero GC).
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitch = 1f, bool loop = false)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] ⚠️ Null SFX clip!");
                return;
            }

            var source = GetSFXSource();
            if (source == null) return;

            source.gameObject.SetActive(true);
            source.clip = clip;
            source.volume = _sfxVolume * volumeScale;
            source.pitch = pitch;
            source.loop = loop;
            source.Play();

            _activeSFX.Add(source);

            if (!loop)
            {
                // Auto-return to pool when done
                StartCoroutine(ReturnSFXAfterPlay(source, clip.length));
            }

            if (debugAudio)
                Debug.Log($"[AudioManager] 🔊 Playing SFX: {clip.name}");
        }

        /// <summary>
        /// Play sound effect at position (3D spatial audio).
        /// </summary>
        public void PlaySFX3D(AudioClip clip, Vector3 position, float volumeScale = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            var source = GetSFXSource();
            if (source == null) return;

            source.gameObject.SetActive(true);
            source.transform.position = position;
            source.clip = clip;
            source.volume = _sfxVolume * volumeScale;
            source.pitch = pitch;
            source.spatialBlend = 1f; // 3D sound
            source.Play();

            _activeSFX.Add(source);
            StartCoroutine(ReturnSFXAfterPlay(source, clip.length));
        }

        /// <summary>
        /// Play one-shot sound effect (no pooling, for very short sounds).
        /// </summary>
        public void PlayOneShot(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;

            // Create temporary source for one-shot
            var go = new GameObject("OneShot_SFX");
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();

            source.PlayOneShot(clip, volumeScale);
            Destroy(go, clip.length + 0.1f);

            if (debugAudio)
                Debug.Log($"[AudioManager] 🔊 One-shot SFX: {clip.name}");
        }

        /// <summary>
        /// Stop all sound effects.
        /// </summary>
        public void StopAllSFX()
        {
            foreach (var source in _activeSFX)
            {
                if (source != null)
                {
                    source.Stop();
                    ReturnSFXSource(source);
                }
            }

            if (debugAudio)
                Debug.Log("[AudioManager] ⏹️ All SFX stopped");
        }

        /// <summary>
        /// Stop sound effects by clip name.
        /// </summary>
        public void StopSFX(string clipName)
        {
            foreach (var source in _activeSFX)
            {
                if (source != null && source.clip != null && source.clip.name == clipName)
                {
                    source.Stop();
                    ReturnSFXSource(source);
                }
            }
        }

        private System.Collections.IEnumerator ReturnSFXAfterPlay(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (source != null && source.isPlaying == false)
            {
                ReturnSFXSource(source);
            }
        }

        #endregion

        #region Volume Control

        /// <summary>
        /// Set master volume (0-1).
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);

            if (masterMixer != null)
            {
                // Convert to decibels (0dB = max, -80dB = mute)
                float db = VolumeToDB(_masterVolume);
                masterMixer.SetFloat("MasterVolume", db);
            }

            if (debugAudio)
                Debug.Log($"[AudioManager] 🔊 Master Volume: {_masterVolume:P0}");
        }

        /// <summary>
        /// Set music volume (0-1).
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);

            if (masterMixer != null)
            {
                float db = VolumeToDB(_musicVolume, musicVolumeExponent);
                masterMixer.SetFloat("MusicVolume", db);
            }

            _musicSourceA.volume = _musicVolume;
            _musicSourceB.volume = _musicVolume;

            if (debugAudio)
                Debug.Log($"[AudioManager] 🎵 Music Volume: {_musicVolume:P0}");
        }

        /// <summary>
        /// Set SFX volume (0-1).
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);

            if (masterMixer != null)
            {
                float db = VolumeToDB(_sfxVolume, sfxVolumeExponent);
                masterMixer.SetFloat("SFXVolume", db);
            }

            // Update all active SFX
            foreach (var source in _activeSFX)
            {
                if (source != null)
                    source.volume = _sfxVolume;
            }

            if (debugAudio)
                Debug.Log($"[AudioManager] 🔊 SFX Volume: {_sfxVolume:P0}");
        }

        /// <summary>
        /// Mute/unmute all audio.
        /// </summary>
        public void SetMute(bool mute)
        {
            AudioListener.volume = mute ? 0f : 1f;
            if (debugAudio)
                Debug.Log(mute ? "[AudioManager] 🔇 Muted" : "[AudioManager] 🔊 Unmuted");
        }

        /// <summary>
        /// Get current volume levels.
        /// </summary>
        public (float master, float music, float sfx) GetVolumes()
        {
            return (_masterVolume, _musicVolume, _sfxVolume);
        }

        private float VolumeToDB(float volume, float exponent = 1f)
        {
            if (volume <= 0.001f) return -80f; // Mute

            // Apply exponent for volume curve
            volume = Mathf.Pow(volume, exponent);

            // Convert to decibels
            return Mathf.Log10(volume) * 20f;
        }

        #endregion

        #region Stats & Debug

        /// <summary>
        /// Get audio system statistics.
        /// </summary>
        public string GetStats()
        {
            return $"[AudioManager] SFX Pool: {_sfxPool.Count} | Active SFX: {_activeSFX.Count} | Music: {(_isMusicPlaying ? "Playing" : "Stopped")}";
        }

        /// <summary>
        /// Debug: Show pool stats in console.
        /// </summary>
        public void DebugPoolStats()
        {
            Debug.Log(GetStats());
            Debug.Log($"[AudioManager] Volumes - Master: {_masterVolume:P0}, Music: {_musicVolume:P0}, SFX: {_sfxVolume:P0}");
        }

        #endregion

        #region Playlist Management

        /// <summary>
        /// Add clip to music playlist.
        /// </summary>
        public void AddToPlaylist(AudioClip clip)
        {
            if (clip != null && !musicPlaylist.Contains(clip))
            {
                musicPlaylist.Add(clip);
                if (debugAudio)
                    Debug.Log($"[AudioManager] ➕ Added to playlist: {clip.name}");
            }
        }

        /// <summary>
        /// Remove clip from playlist.
        /// </summary>
        public void RemoveFromPlaylist(AudioClip clip)
        {
            if (musicPlaylist.Remove(clip))
            {
                if (debugAudio)
                    Debug.Log($"[AudioManager] ➖ Removed from playlist: {clip.name}");
            }
        }

        /// <summary>
        /// Clear playlist.
        /// </summary>
        public void ClearPlaylist()
        {
            musicPlaylist.Clear();
            StopMusic();
            if (debugAudio)
                Debug.Log("[AudioManager] 🗑️ Playlist cleared");
        }

        #endregion
    }
}
