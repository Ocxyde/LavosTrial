// SFXVFXEngine.cs
// Centralized Sound and Visual Effects Engine
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Features:
// - Particle system presets (fire, smoke, sparks, magic, etc.)
// - Sound effect management (SFX)
// - Tetrahedral VFX integration
// - Object pooling for performance
// - Event-driven architecture

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Centralized engine for all sound and visual effects.
    /// Singleton pattern - access via SFXVFXEngine.Instance
    /// Integrates with EventHandler for event-driven effects.
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
        [SerializeField] private float ambientVolume = 0.6f;
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.5f;

        [Header("VFX Settings")]
        [SerializeField] private bool enableVFX = true;
        [SerializeField] private bool enableTetrahedralVFX = true;
        [SerializeField] private int maxParticleSystems = 50;
        [SerializeField] private float particleCullDistance = 30f;

        [Header("Object Pooling")]
        [SerializeField] private int initialPoolSize = 20;
        [SerializeField] private int maxPoolSize = 100;

        [Header("References")]
        [SerializeField] private ParticleGenerator particleGenerator;
        [SerializeField] private Transform vfxContainer;

        #endregion

        #region Private Fields

        // Audio sources
        private AudioSource _sfxSource;
        private AudioSource _ambientSource;
        private AudioSource _musicSource;
        private List<AudioSource> _pooledAudioSources = new();

        // Particle systems pool
        private List<ParticleSystem> _particlePool = new();
        private List<ParticleSystem> _activeParticles = new();
        private Transform _particleContainer;

        // Tetrahedral VFX
        private List<GameObject> _tetraVFX = new();
        private List<GameObject> _activeTetraVFX = new();

        // Event integration
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
            // Subscribe to events
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            Cleanup();
        }

        private void Update()
        {
            // Cull distant particle systems
            if (enableVFX)
            {
                CullDistantParticles();
            }
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            // Create containers
            if (vfxContainer == null)
            {
                GameObject container = new GameObject("VFX_Container");
                container.transform.SetParent(transform);
                vfxContainer = container.transform;
            }

            _particleContainer = new GameObject("ParticleSystems").transform;
            _particleContainer.SetParent(vfxContainer);

            // Initialize audio sources
            CreateAudioSources();

            // Initialize particle pool
            if (enableVFX)
            {
                InitializeParticlePool();
            }

            // Get references
            _eventHandler = FindFirstObjectByType<EventHandler>();
            if (particleGenerator == null)
            {
                particleGenerator = GetComponent<ParticleGenerator>();
                if (particleGenerator == null)
                {
                    particleGenerator = gameObject.AddComponent<ParticleGenerator>();
                }
            }

            Debug.Log("[SFXVFXEngine] Initialized");
        }

        private void CreateAudioSources()
        {
            // Main SFX source
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = sfxVolume;

            // Ambient source
            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.playOnAwake = false;
            _ambientSource.volume = ambientVolume;
            _ambientSource.loop = true;

            // Music source
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.volume = musicVolume;
            _musicSource.loop = true;

            // Create pooled audio sources
            for (int i = 0; i < initialPoolSize; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = sfxVolume;
                _pooledAudioSources.Add(source);
            }
        }

        private void InitializeParticlePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                ParticleSystem ps = particleGenerator.CreateParticleSystem(transform.position);
                ps.gameObject.SetActive(false);
                ps.transform.SetParent(_particleContainer);
                _particlePool.Add(ps);
            }
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            if (_eventHandler == null) return;

            // Combat events
            _eventHandler.OnDamageDealt += OnDamageDealt;
            _eventHandler.OnHealReceived += OnHealReceived;
            _eventHandler.OnDeath += OnDeath;

            // Item events
            _eventHandler.OnItemPickedUp += OnItemPickedUp;
            _eventHandler.OnItemUsed += OnItemUsed;

            // Player events
            _eventHandler.OnPlayerJump += OnPlayerJump;
            _eventHandler.OnPlayerLand += OnPlayerLand;
        }

        private void UnsubscribeFromEvents()
        {
            if (_eventHandler == null) return;

            _eventHandler.OnDamageDealt -= OnDamageDealt;
            _eventHandler.OnHealReceived -= OnHealReceived;
            _eventHandler.OnDeath -= OnDeath;
            _eventHandler.OnItemPickedUp -= OnItemPickedUp;
            _eventHandler.OnItemUsed -= OnItemUsed;
            _eventHandler.OnPlayerJump -= OnPlayerJump;
            _eventHandler.OnPlayerLand -= OnPlayerLand;
        }

        #endregion

        #region Event Handlers

        private void OnDamageDealt(DamageInfo damageInfo, float finalDamage)
        {
            // Play hit VFX based on damage type
            switch (damageInfo.type)
            {
                case DamageType.Physical:
                    SpawnBloodSplatter(damageInfo.hitPosition);
                    PlaySFX("HitPhysical");
                    break;
                case DamageType.Fire:
                    SpawnFireHit(damageInfo.hitPosition);
                    PlaySFX("HitFire");
                    break;
                case DamageType.Ice:
                    SpawnIceHit(damageInfo.hitPosition);
                    PlaySFX("HitIce");
                    break;
                case DamageType.Lightning:
                    SpawnLightningHit(damageInfo.hitPosition);
                    PlaySFX("HitLightning");
                    break;
                default:
                    SpawnGenericHit(damageInfo.hitPosition);
                    PlaySFX("HitGeneric");
                    break;
            }

            // Spawn floating damage text (via DialogEngine)
            if (enableVFX)
            {
                DialogEngine.Instance?.ShowFloatingText(
                    finalDamage.ToString("F0"),
                    damageInfo.isCritical ? Color.red : Color.white,
                    damageInfo.hitPosition
                );
            }
        }

        private void OnHealReceived(float healAmount, Vector3 position)
        {
            if (!enableVFX) return;

            SpawnHealEffect(position);
            PlaySFX("Heal");
        }

        private void OnDeath(GameObject victim)
        {
            if (!enableVFX || victim == null) return;

            SpawnDeathEffect(victim.transform.position);
            PlaySFX("Death");
        }

        private void OnItemPickedUp(GameObject item)
        {
            if (!enableVFX || item == null) return;

            SpawnPickupEffect(item.transform.position);
            PlaySFX("Pickup");
        }

        private void OnItemUsed(GameObject item)
        {
            if (!enableVFX || item == null) return;

            SpawnItemUseEffect(item.transform.position);
            PlaySFX("ItemUse");
        }

        private void OnPlayerJump()
        {
            if (!enableVFX) return;

            SpawnJumpEffect(GetPlayerPosition());
            PlaySFX("Jump");
        }

        private void OnPlayerLand()
        {
            if (!enableVFX) return;

            SpawnLandEffect(GetPlayerPosition());
            PlaySFX("Land");
        }

        private Vector3 GetPlayerPosition()
        {
            var player = FindFirstObjectByType<PlayerController>();
            return player != null ? player.transform.position : Vector3.zero;
        }

        #endregion

        #region Sound Effects

        /// <summary>
        /// Play a sound effect by name.
        /// </summary>
        public void PlaySFX(string sfxName, Vector3? position = null, float? volume = null)
        {
            AudioSource source = GetPooledAudioSource();
            if (source == null) return;

            // TODO: Load AudioClip from Resources or Addressables
            // For now, this is a placeholder
            Debug.Log($"[SFXVFXEngine] Playing SFX: {sfxName}");

            source.Play();
        }

        /// <summary>
        /// Play ambient sound.
        /// </summary>
        public void PlayAmbient(AudioClip clip)
        {
            if (_ambientSource == null || clip == null) return;

            _ambientSource.clip = clip;
            _ambientSource.Play();
        }

        /// <summary>
        /// Stop ambient sound.
        /// </summary>
        public void StopAmbient()
        {
            _ambientSource?.Stop();
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
        public void StopMusic(float fadeDuration = 1f)
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
            if (_pooledAudioSources.Count < maxPoolSize)
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

        #region Visual Effects - Combat

        /// <summary>
        /// Spawn blood splatter effect.
        /// </summary>
        public void SpawnBloodSplatter(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreateBloodSplatter();
            particleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);
        }

        /// <summary>
        /// Spawn fire hit effect.
        /// </summary>
        public void SpawnFireHit(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreateFireHit();
            particleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);

            // Also spawn tetrahedral fire if enabled
            if (enableTetrahedralVFX)
            {
                SpawnTetrahedralFire(position);
            }
        }

        /// <summary>
        /// Spawn ice hit effect.
        /// </summary>
        public void SpawnIceHit(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreateIceHit();
            particleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);
        }

        /// <summary>
        /// Spawn lightning hit effect.
        /// </summary>
        public void SpawnLightningHit(Vector3 position)
        {
            if (!enableVFX) return;

            // TODO: Implement lightning bolt VFX
            Debug.Log($"[SFXVFXEngine] Lightning hit at {position}");
        }

        /// <summary>
        /// Spawn generic hit effect.
        /// </summary>
        public void SpawnGenericHit(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreateSparks();
            particleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);
        }

        /// <summary>
        /// Spawn heal effect.
        /// </summary>
        public void SpawnHealEffect(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreateHealEffect();
            particleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);
        }

        /// <summary>
        /// Spawn death effect.
        /// </summary>
        public void SpawnDeathEffect(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreateDeathEffect();
            particleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);
        }

        #endregion

        #region Visual Effects - Environment

        /// <summary>
        /// Spawn pickup effect.
        /// </summary>
        public void SpawnPickupEffect(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreatePickupEffect();
            particleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);
        }

        /// <summary>
        /// Spawn item use effect.
        /// </summary>
        public void SpawnItemUseEffect(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreateItemUseEffect();
            particleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);
        }

        /// <summary>
        /// Spawn jump effect.
        /// </summary>
        public void SpawnJumpEffect(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreateJumpEffect();
            particleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);
        }

        /// <summary>
        /// Spawn land effect.
        /// </summary>
        public void SpawnLandEffect(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreateLandEffect();
            particleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);
        }

        #endregion

        #region Tetrahedral VFX

        /// <summary>
        /// Spawn tetrahedral fire effect.
        /// </summary>
        public void SpawnTetrahedralFire(Vector3 position, float scale = 1f)
        {
            if (!enableVFX || !enableTetrahedralVFX) return;

            // Generate tetrahedrons for fire effect
            int tetraCount = 8;
            for (int i = 0; i < tetraCount; i++)
            {
                int variant = UnityEngine.Random.Range(0, TetrahedronEngine.VariantCount);
                GameObject tetra = TetrahedronEngine.CreateTetrahedron(
                    position + UnityEngine.Random.insideUnitSphere * 0.3f * scale,
                    UnityEngine.Random.rotation,
                    variant,
                    GetFireMaterial()
                );

                tetra.transform.SetParent(vfxContainer);
                _activeTetraVFX.Add(tetra);

                // Animate upward
                StartCoroutine(AnimateTetrahedronUpward(tetra, 1f));
            }
        }

        /// <summary>
        /// Spawn tetrahedral magic effect.
        /// </summary>
        public void SpawnTetrahedralMagic(Vector3 position, Color magicColor, float scale = 1f)
        {
            if (!enableVFX || !enableTetrahedralVFX) return;

            int tetraCount = 12;
            for (int i = 0; i < tetraCount; i++)
            {
                int variant = UnityEngine.Random.Range(0, TetrahedronEngine.VariantCount);
                Material mat = GetMagicMaterial(magicColor);

                GameObject tetra = TetrahedronEngine.CreateTetrahedron(
                    position + UnityEngine.Random.insideUnitSphere * 0.5f * scale,
                    UnityEngine.Random.rotation,
                    variant,
                    mat
                );

                tetra.transform.SetParent(vfxContainer);
                _activeTetraVFX.Add(tetra);

                // Animate outward
                StartCoroutine(AnimateTetrahedronOutward(tetra, 0.8f));
            }
        }

        private System.Collections.IEnumerator AnimateTetrahedronUpward(GameObject tetra, float duration)
        {
            float elapsed = 0f;
            Vector3 startPos = tetra.transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                tetra.transform.position = startPos + Vector3.up * Mathf.Lerp(0f, 1f, t);
                tetra.transform.Rotate(0, 180 * Time.deltaTime, 0);
                yield return null;
            }

            Destroy(tetra);
            _activeTetraVFX.Remove(tetra);
        }

        private System.Collections.IEnumerator AnimateTetrahedronOutward(GameObject tetra, float duration)
        {
            float elapsed = 0f;
            Vector3 startPos = tetra.transform.position;
            Vector3 direction = (tetra.transform.position - transform.position).normalized;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                tetra.transform.position = startPos + direction * Mathf.Lerp(0f, 2f, t);
                tetra.transform.Rotate(90 * Time.deltaTime, 180 * Time.deltaTime, 0);
                yield return null;
            }

            Destroy(tetra);
            _activeTetraVFX.Remove(tetra);
        }

        private Material GetFireMaterial()
        {
            // TODO: Load from Resources or create dynamically
            Material mat = new Material(Shader.Find("Unlit/Color"));
            if (mat != null)
            {
                mat.color = new Color(1f, 0.6f, 0.2f);
            }
            return mat;
        }

        private Material GetMagicMaterial(Color color)
        {
            Material mat = new Material(Shader.Find("Unlit/Color"));
            if (mat != null)
            {
                mat.color = color;
            }
            return mat;
        }

        #endregion

        #region Particle System Pooling

        private ParticleSystem GetPooledParticleSystem()
        {
            foreach (var ps in _particlePool)
            {
                if (!ps.gameObject.activeSelf)
                {
                    ps.gameObject.SetActive(true);
                    return ps;
                }
            }

            // Pool exhausted - create new if reasonable
            if (_particlePool.Count < maxPoolSize)
            {
                ParticleSystem ps = particleGenerator.CreateParticleSystem(transform.position);
                ps.transform.SetParent(_particleContainer);
                _particlePool.Add(ps);
                return ps;
            }

            Debug.LogWarning("[SFXVFXEngine] Particle system pool exhausted!");
            return null;
        }

        private void CullDistantParticles()
        {
            Vector3 playerPos = GetPlayerPosition();

            foreach (var ps in _activeParticles)
            {
                if (ps == null) continue;

                float distance = Vector3.Distance(ps.transform.position, playerPos);
                if (distance > particleCullDistance)
                {
                    ps.Stop();
                    ps.gameObject.SetActive(false);
                }
            }
        }

        #endregion

        #region Cleanup

        private void Cleanup()
        {
            // Stop all particle systems
            foreach (var ps in _particlePool)
            {
                if (ps != null)
                {
                    ps.Stop();
                    Destroy(ps.gameObject);
                }
            }
            _particlePool.Clear();
            _activeParticles.Clear();

            // Destroy tetrahedral VFX
            foreach (var tetra in _activeTetraVFX)
            {
                if (tetra != null)
                {
                    Destroy(tetra);
                }
            }
            _activeTetraVFX.Clear();

            // Clean up audio sources
            foreach (var source in _pooledAudioSources)
            {
                if (source != null)
                {
                    Destroy(source);
                }
            }
            _pooledAudioSources.Clear();
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Set master volume.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        /// <summary>
        /// Set SFX volume.
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        /// <summary>
        /// Set music volume.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            if (_sfxSource != null) _sfxSource.volume = sfxVolume * masterVolume;
            if (_ambientSource != null) _ambientSource.volume = ambientVolume * masterVolume;
            if (_musicSource != null) _musicSource.volume = musicVolume * masterVolume;

            foreach (var source in _pooledAudioSources)
            {
                source.volume = sfxVolume * masterVolume;
            }
        }

        /// <summary>
        /// Enable or disable VFX.
        /// </summary>
        public void SetVFXEnabled(bool enabled)
        {
            enableVFX = enabled;
            if (!enabled)
            {
                // Stop all active particles
                foreach (var ps in _activeParticles)
                {
                    ps?.Stop();
                }
            }
        }

        #endregion
    }
}
