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
using Code.Lavos.Status;

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
                ParticleSystem ps = ParticleGenerator.CreateParticleSystem(transform.position);
                ps.gameObject.SetActive(false);
                ps.transform.SetParent(_particleContainer);
                _particlePool.Add(ps);
            }
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            if (_eventHandler == null)
            {
                Debug.LogWarning("[SFXVFXEngine] EventHandler not found! Event integration disabled.");
                return;
            }

            // Combat events
            _eventHandler.OnDamageDealt += OnDamageDealt;
            _eventHandler.OnDamageTaken += OnDamageTaken;
            _eventHandler.OnKill += OnKill;
            _eventHandler.OnDeath += OnDeath;

            // Player events
            _eventHandler.OnPlayerHealthChanged += OnPlayerHealthChanged;
            _eventHandler.OnPlayerDamaged += OnPlayerDamaged;
            _eventHandler.OnPlayerHealed += OnPlayerHealed;
            _eventHandler.OnPlayerDied += OnPlayerDied;
            _eventHandler.OnPlayerManaChanged += OnPlayerManaChanged;
            _eventHandler.OnPlayerStaminaChanged += OnPlayerStaminaChanged;
            _eventHandler.OnLevelChanged += OnLevelChanged;
            _eventHandler.OnExperienceChanged += OnExperienceChanged;

            // Item events
            _eventHandler.OnItemPickedUp += OnItemPickedUp;
            _eventHandler.OnItemUsed += OnItemUsed;
            _eventHandler.OnItemDropped += OnItemDropped;

            // UI events
            _eventHandler.OnFloatingTextRequested += OnFloatingTextRequested;

            // Game events
            _eventHandler.OnAchievementUnlocked += OnAchievementUnlocked;
            _eventHandler.OnQuestCompleted += OnQuestCompleted;

            Debug.Log("[SFXVFXEngine] Subscribed to all EventHandler events");
        }

        private void UnsubscribeFromEvents()
        {
            if (_eventHandler == null) return;

            // Combat events
            _eventHandler.OnDamageDealt -= OnDamageDealt;
            _eventHandler.OnDamageTaken -= OnDamageTaken;
            _eventHandler.OnKill -= OnKill;
            _eventHandler.OnDeath -= OnDeath;

            // Player events
            _eventHandler.OnPlayerHealthChanged -= OnPlayerHealthChanged;
            _eventHandler.OnPlayerDamaged -= OnPlayerDamaged;
            _eventHandler.OnPlayerHealed -= OnPlayerHealed;
            _eventHandler.OnPlayerDied -= OnPlayerDied;
            _eventHandler.OnPlayerManaChanged -= OnPlayerManaChanged;
            _eventHandler.OnPlayerStaminaChanged -= OnPlayerStaminaChanged;
            _eventHandler.OnLevelChanged -= OnLevelChanged;
            _eventHandler.OnExperienceChanged -= OnExperienceChanged;

            // Item events
            _eventHandler.OnItemPickedUp -= OnItemPickedUp;
            _eventHandler.OnItemUsed -= OnItemUsed;
            _eventHandler.OnItemDropped -= OnItemDropped;

            // UI events
            _eventHandler.OnFloatingTextRequested -= OnFloatingTextRequested;

            // Game events
            _eventHandler.OnAchievementUnlocked -= OnAchievementUnlocked;
            _eventHandler.OnQuestCompleted -= OnQuestCompleted;
        }

        #endregion

        #region Event Handlers - Combat

        private void OnDamageDealt(DamageInfo damageInfo, float finalDamage)
        {
            if (!enableVFX || damageInfo.hitPosition == Vector3.zero) return;

            // Spawn VFX based on damage type
            switch (damageInfo.type)
            {
                case DamageType.Physical:
                    SpawnBloodSplatter(damageInfo.hitPosition);
                    PlaySFX("HitPhysical", damageInfo.hitPosition);
                    break;
                case DamageType.Fire:
                    SpawnFireHit(damageInfo.hitPosition);
                    PlaySFX("HitFire", damageInfo.hitPosition);
                    break;
                case DamageType.Ice:
                    SpawnIceHit(damageInfo.hitPosition);
                    PlaySFX("HitIce", damageInfo.hitPosition);
                    break;
                case DamageType.Lightning:
                    SpawnLightningHit(damageInfo.hitPosition);
                    PlaySFX("HitLightning", damageInfo.hitPosition);
                    break;
                case DamageType.Arcane:
                    SpawnArcaneHit(damageInfo.hitPosition);
                    PlaySFX("HitArcane", damageInfo.hitPosition);
                    break;
                case DamageType.Holy:
                    SpawnHolyHit(damageInfo.hitPosition);
                    PlaySFX("HitHoly", damageInfo.hitPosition);
                    break;
                case DamageType.Shadow:
                    SpawnShadowHit(damageInfo.hitPosition);
                    PlaySFX("HitShadow", damageInfo.hitPosition);
                    break;
                case DamageType.Poison:
                    SpawnPoisonHit(damageInfo.hitPosition);
                    PlaySFX("HitPoison", damageInfo.hitPosition);
                    break;
                default:
                    SpawnGenericHit(damageInfo.hitPosition);
                    PlaySFX("HitGeneric", damageInfo.hitPosition);
                    break;
            }

            // Spawn floating damage text
            if (enableVFX && DialogEngine.Instance != null)
            {
                DialogEngine.Instance.ShowFloatingText(
                    finalDamage.ToString("F0"),
                    damageInfo.isCritical ? Color.red : Color.white,
                    damageInfo.hitPosition,
                    duration: 1.5f
                );
            }
        }

        private void OnDamageTaken(DamageInfo damageInfo, float finalDamage)
        {
            // Player took damage - screen effects
            if (!enableVFX) return;

            // Screen flash red
            // TODO: Implement screen flash effect
            PlaySFX("PlayerHit", Vector3.zero, volume: 0.5f);
        }

        private void OnKill(GameObject killer, GameObject victim)
        {
            if (!enableVFX || victim == null) return;

            SpawnDeathEffect(victim.transform.position);
            PlaySFX("Kill", victim.transform.position);
        }

        private void OnDeath(GameObject victim)
        {
            if (!enableVFX || victim == null) return;

            SpawnDeathEffect(victim.transform.position);
            PlaySFX("Death", victim.transform.position);
        }

        #endregion

        #region Event Handlers - Player

        private void OnPlayerHealthChanged(float currentValue, float maxValue)
        {
            // Low health warning effect
            if (currentValue / maxValue < 0.2f)
            {
                PlaySFX("LowHealthWarning", Vector3.zero, volume: 0.3f);
                // TODO: Add pulsing red screen effect
            }
        }

        private void OnPlayerDamaged(float amount)
        {
            PlaySFX("PlayerHurt", Vector3.zero);
        }

        private void OnPlayerHealed(float amount)
        {
            if (!enableVFX) return;
            SpawnHealEffect(GetPlayerPosition());
            PlaySFX("Heal", GetPlayerPosition());
        }

        private void OnPlayerDied()
        {
            if (!enableVFX) return;
            SpawnDeathEffect(GetPlayerPosition());
            PlaySFX("PlayerDeath", Vector3.zero);
        }

        private void OnPlayerManaChanged(float currentValue, float maxValue)
        {
            // Mana spent effect
            if (currentValue < maxValue)
            {
                // TODO: Small blue particle burst from player
            }
        }

        private void OnPlayerStaminaChanged(float currentValue, float maxValue)
        {
            // Stamina spent effect
            if (currentValue < maxValue)
            {
                // TODO: Small white particle burst from player
            }
        }

        private void OnLevelChanged(int newLevel)
        {
            if (!enableVFX) return;

            Vector3 playerPos = GetPlayerPosition();
            
            // Spawn level up effect - golden explosion
            SpawnLevelUpEffect(playerPos);
            PlaySFX("LevelUp", playerPos);

            // Spawn tetrahedral celebration
            if (enableTetrahedralVFX)
            {
                SpawnTetrahedralMagic(playerPos, Color.gold, scale: 2f);
            }
        }

        private void OnExperienceChanged(int amount)
        {
            if (amount > 0 && enableVFX)
            {
                // Small XP gain effect
                SpawnPickupEffect(GetPlayerPosition());
            }
        }

        #endregion

        #region Event Handlers - Items

        private void OnItemPickedUp(ItemData item, int quantity)
        {
            if (!enableVFX) return;

            // Spawn at player position (item was just picked up)
            SpawnPickupEffect(GetPlayerPosition());
            PlaySFX("Pickup", GetPlayerPosition());
        }

        private void OnItemUsed(ItemData item, int quantity)
        {
            if (!enableVFX) return;

            SpawnItemUseEffect(GetPlayerPosition());
            PlaySFX("ItemUse", GetPlayerPosition());
        }

        private void OnItemDropped(ItemData item, int quantity)
        {
            if (!enableVFX) return;

            // TODO: Spawn at drop position when available
            PlaySFX("ItemDrop", GetPlayerPosition());
        }

        #endregion

        #region Event Handlers - UI & Game

        private void OnFloatingTextRequested(string text, Color color, float duration)
        {
            // Already handled by DialogEngine
        }

        private void OnAchievementUnlocked(string achievementName)
        {
            if (!enableVFX) return;

            SpawnPickupEffect(GetPlayerPosition());
            PlaySFX("Achievement", Vector3.zero);
        }

        private void OnQuestCompleted(string questName)
        {
            if (!enableVFX) return;

            SpawnLevelUpEffect(GetPlayerPosition());
            PlaySFX("QuestComplete", Vector3.zero);
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
            ParticleGenerator.ApplyConfig(ps, config);
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
            ParticleGenerator.ApplyConfig(ps, config);
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
            ParticleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);
        }

        /// <summary>
        /// Spawn arcane hit effect.
        /// </summary>
        public void SpawnArcaneHit(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreateArcaneHit();
            ParticleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);

            if (enableTetrahedralVFX)
            {
                SpawnTetrahedralMagic(position, Color.purple, scale: 1.2f);
            }
        }

        /// <summary>
        /// Spawn holy hit effect.
        /// </summary>
        public void SpawnHolyHit(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreateHolyHit();
            ParticleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);

            if (enableTetrahedralVFX)
            {
                SpawnTetrahedralMagic(position, Color.gold, scale: 1.2f);
            }
        }

        /// <summary>
        /// Spawn shadow hit effect.
        /// </summary>
        public void SpawnShadowHit(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreateShadowHit();
            ParticleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);
        }

        /// <summary>
        /// Spawn poison hit effect.
        /// </summary>
        public void SpawnPoisonHit(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreatePoisonHit();
            ParticleGenerator.ApplyConfig(ps, config);
            ps.Play();

            _activeParticles.Add(ps);
        }

        /// <summary>
        /// Spawn level up effect.
        /// </summary>
        public void SpawnLevelUpEffect(Vector3 position)
        {
            if (!enableVFX) return;

            ParticleSystem ps = GetPooledParticleSystem();
            if (ps == null) return;

            ps.transform.position = position;
            var config = ParticleConfig.CreateLevelUpEffect();
            ParticleGenerator.ApplyConfig(ps, config);
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
            ParticleGenerator.ApplyConfig(ps, config);
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
            ParticleGenerator.ApplyConfig(ps, config);
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
            ParticleGenerator.ApplyConfig(ps, config);
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
            ParticleGenerator.ApplyConfig(ps, config);
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
            ParticleGenerator.ApplyConfig(ps, config);
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
            ParticleGenerator.ApplyConfig(ps, config);
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
            ParticleGenerator.ApplyConfig(ps, config);
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
                ParticleSystem ps = ParticleGenerator.CreateParticleSystem(transform.position);
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
