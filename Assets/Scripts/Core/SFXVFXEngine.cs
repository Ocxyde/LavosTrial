// SFXVFXEngine.cs - Simplified Version
// Centralized Sound and Visual Effects Engine
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System;
using System.Collections.Generic;
using UnityEngine;
using Code.Lavos.Status;

#pragma warning disable CS0414 // Disable warnings for unused serialized fields (reserved for future features)

namespace Code.Lavos.Core
{
    /// <summary>
    /// Centralized engine for all sound and visual effects.
    /// Singleton pattern - access via SFXVFXEngine.Instance
    /// </summary>
    public class SFXVFXEngine : MonoBehaviour
    {
        #region Singleton

        private static SFXVFXEngine _instance;
        public static SFXVFXEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SFXVFXEngine>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SFXVFXEngine");
                        _instance = go.AddComponent<SFXVFXEngine>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Inspector Fields

        [Header("Audio Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float sfxVolume = 0.8f;
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.5f;

        [Header("VFX Settings")]
        [SerializeField] private bool enableVFX = true;
        [SerializeField] private int maxParticleSystems = 50;  // Reserved for future particle system limit
        
        #endregion

        #region Private Fields

        private AudioSource _sfxSource;
        private AudioSource _musicSource;
        private List<AudioSource> _pooledAudioSources = new();
        private EventHandler _eventHandler;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            // Initialize audio sources
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = sfxVolume;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.volume = musicVolume;
            _musicSource.loop = true;

            // Create pooled audio sources
            for (int i = 0; i < 10; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = sfxVolume;
                _pooledAudioSources.Add(source);
            }

            // Get event handler reference
            _eventHandler = FindFirstObjectByType<EventHandler>();

            Debug.Log("[SFXVFXEngine] Initialized");
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            if (_eventHandler == null) return;

            // Combat events
            _eventHandler.OnDamageDealt += OnDamageDealt;
            _eventHandler.OnDeath += OnDeath;

            // Player events
            _eventHandler.OnPlayerHealed += OnPlayerHealed;
            _eventHandler.OnLevelChanged += OnLevelChanged;

            // Item events
            _eventHandler.OnItemPickedUp += OnItemPickedUp;
            _eventHandler.OnItemUsed += OnItemUsed;

            Debug.Log("[SFXVFXEngine] Subscribed to events");
        }

        private void UnsubscribeFromEvents()
        {
            if (_eventHandler == null) return;

            _eventHandler.OnDamageDealt -= OnDamageDealt;
            _eventHandler.OnDeath -= OnDeath;
            _eventHandler.OnPlayerHealed -= OnPlayerHealed;
            _eventHandler.OnLevelChanged -= OnLevelChanged;
            _eventHandler.OnItemPickedUp -= OnItemPickedUp;
            _eventHandler.OnItemUsed -= OnItemUsed;
        }

        #endregion

        #region Event Handlers

        private void OnDamageDealt(DamageInfo damageInfo, float finalDamage)
        {
            if (!enableVFX) return;

            // Play sound based on damage type
            string sfxName = damageInfo.type switch
            {
                DamageType.Fire => "HitFire",
                DamageType.Ice => "HitIce",
                DamageType.Lightning => "HitLightning",
                DamageType.Poison => "HitPoison",
                DamageType.Holy => "HitHoly",
                DamageType.Shadow => "HitShadow",
                DamageType.Magic => "HitMagic",
                _ => "HitPhysical"
            };

            PlaySFX(sfxName);
        }

        private void OnDeath(GameObject victim)
        {
            if (!enableVFX || victim == null) return;
            PlaySFX("Death");
        }

        private void OnPlayerHealed(float amount)
        {
            if (!enableVFX) return;
            PlaySFX("Heal");
        }

        private void OnLevelChanged(int newLevel)
        {
            if (!enableVFX) return;
            PlaySFX("LevelUp");
        }

        private void OnItemPickedUp(ItemData item, int quantity)
        {
            if (!enableVFX) return;
            PlaySFX("Pickup");
        }

        private void OnItemUsed(ItemData item, int quantity)
        {
            if (!enableVFX) return;
            PlaySFX("ItemUse");
        }

        #endregion

        #region Sound Effects

        /// <summary>
        /// Play a sound effect by name.
        /// Loads from Resources/Audio/SFX/ folder.
        /// </summary>
        public void PlaySFX(string sfxName, float? volume = null)
        {
            AudioSource source = GetPooledAudioSource();
            if (source == null) return;

            // Try to load AudioClip from Resources
            AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{sfxName}");
            if (clip == null)
            {
                // Placeholder - just log for now
                Debug.Log($"[SFXVFXEngine] Playing SFX: {sfxName} (no clip loaded)");
                return;
            }

            source.clip = clip;
            source.volume = volume ?? sfxVolume;
            source.Play();
        }

        /// <summary>
        /// Play music track.
        /// </summary>
        public void PlayMusic(AudioClip clip, float fadeDuration = 1f)
        {
            if (_musicSource == null || clip == null) return;
            _musicSource.clip = clip;
            _musicSource.Play();
        }

        /// <summary>
        /// Stop music.
        /// </summary>
        public void StopMusic()
        {
            _musicSource?.Stop();
        }

        private AudioSource GetPooledAudioSource()
        {
            foreach (var source in _pooledAudioSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // Pool exhausted - create new if under limit
            if (_pooledAudioSources.Count < 20)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = sfxVolume;
                _pooledAudioSources.Add(source);
                return source;
            }

            Debug.LogWarning("[SFXVFXEngine] Audio source pool exhausted!");
            return null;
        }

        #endregion

        #region Volume Control

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            if (_sfxSource != null) _sfxSource.volume = sfxVolume * masterVolume;
            if (_musicSource != null) _musicSource.volume = musicVolume * masterVolume;

            foreach (var source in _pooledAudioSources)
            {
                source.volume = sfxVolume * masterVolume;
            }
        }

        #endregion

        #region VFX Toggle

        public void SetVFXEnabled(bool enabled)
        {
            enableVFX = enabled;
        }

        #endregion
    }
}
