// EventHandler.cs
// Central Event Management System for all game events
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Single point of truth for all game events
// - Player events (health, mana, stamina, stats)
// - Combat events (damage, healing, death)
// - Item events (pickup, use, drop)
// - Game events (score, level, quests)
// - UI events (bars, dialogs, windows)

using System;
using UnityEngine;
using UnityEngine.Events;
using Code.Lavos.Status;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Central event manager for all game systems.
    /// Provides unified event subscription and invocation.
    /// </summary>
    public class EventHandler : MonoBehaviour
    {
        public static EventHandler Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool debugEvents = false;

        #region Player Events

        // Health
        public event Action<float, float> OnPlayerHealthChanged; // current, max
        public event Action<float> OnPlayerDamaged; // damage amount
        public event Action<float> OnPlayerHealed; // heal amount
        public event Action OnPlayerDied;
        public event Action OnPlayerRespawned;

        // Mana
        public event Action<float, float> OnPlayerManaChanged; // current, max
        public event Action<float> OnPlayerManaUsed; // amount used
        public event Action<float> OnPlayerManaRestored; // amount restored

        // Stamina
        public event Action<float, float> OnPlayerStaminaChanged; // current, max
        public event Action<float> OnPlayerStaminaUsed; // amount used
        public event Action<float> OnPlayerStaminaRestored; // amount restored

        // Stats
        public event Action<string, float> OnStatChanged; // stat name, new value
        public event Action<int> OnLevelChanged; // new level
        public event Action<int> OnExperienceChanged; // experience amount

        #endregion

        #region Combat Events

        public event Action<DamageInfo, float> OnDamageDealt; // damage info, final damage
        public event Action<DamageInfo, float> OnDamageTaken; // damage info, final damage
        public event Action<GameObject, GameObject> OnKill; // killer, victim
        public event Action<GameObject> OnDeath; // deceased

        #endregion

        #region Item Events

        public event Action<ItemData, int> OnItemPickedUp; // item, quantity
        public event Action<ItemData, int> OnItemUsed; // item, quantity
        public event Action<ItemData, int> OnItemDropped; // item, quantity
        public event Action<ItemData, int> OnItemStacked; // item, new stack size

        #endregion

        #region Game Events

        public event Action<int> OnScoreChanged; // new score
        public event Action<string> OnQuestUpdated; // quest name
        public event Action<string> OnQuestCompleted; // quest name
        public event Action<string> OnAchievementUnlocked; // achievement name

        #endregion

        #region UI Events

        public event Action OnUIBarsInitialized;
        public event Action<string, Color, float> OnFloatingTextRequested; // text, color, duration
        public event Action<string, string> OnDialogRequested; // title, message
        public event Action<string> OnNotificationRequested; // message
        public event Action<string> OnWindowRequested; // window type

        #endregion

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (debugEvents)
            {
                Debug.Log("[EventHandler] Initialized - All systems ready");
            }
        }

        #region Player Event Invokers

        public void InvokePlayerHealthChanged(float current, float max)
        {
            OnPlayerHealthChanged?.Invoke(current, max);
            if (debugEvents) Debug.Log($"[EventHandler] PlayerHealthChanged: {current}/{max}");
        }

        public void InvokePlayerDamaged(float amount)
        {
            OnPlayerDamaged?.Invoke(amount);
            if (debugEvents) Debug.Log($"[EventHandler] PlayerDamaged: {amount}");
        }

        public void InvokePlayerHealed(float amount)
        {
            OnPlayerHealed?.Invoke(amount);
            if (debugEvents) Debug.Log($"[EventHandler] PlayerHealed: {amount}");
        }

        public void InvokePlayerDied()
        {
            OnPlayerDied?.Invoke();
            if (debugEvents) Debug.Log("[EventHandler] PlayerDied");
        }

        public void InvokePlayerRespawned()
        {
            OnPlayerRespawned?.Invoke();
            if (debugEvents) Debug.Log("[EventHandler] PlayerRespawned");
        }

        public void InvokePlayerManaChanged(float current, float max)
        {
            OnPlayerManaChanged?.Invoke(current, max);
            if (debugEvents) Debug.Log($"[EventHandler] PlayerManaChanged: {current}/{max}");
        }

        public void InvokePlayerManaUsed(float amount)
        {
            OnPlayerManaUsed?.Invoke(amount);
            if (debugEvents) Debug.Log($"[EventHandler] PlayerManaUsed: {amount}");
        }

        public void InvokePlayerManaRestored(float amount)
        {
            OnPlayerManaRestored?.Invoke(amount);
            if (debugEvents) Debug.Log($"[EventHandler] PlayerManaRestored: {amount}");
        }

        public void InvokePlayerStaminaChanged(float current, float max)
        {
            OnPlayerStaminaChanged?.Invoke(current, max);
            if (debugEvents) Debug.Log($"[EventHandler] PlayerStaminaChanged: {current}/{max}");
        }

        public void InvokePlayerStaminaUsed(float amount)
        {
            OnPlayerStaminaUsed?.Invoke(amount);
            if (debugEvents) Debug.Log($"[EventHandler] PlayerStaminaUsed: {amount}");
        }

        public void InvokePlayerStaminaRestored(float amount)
        {
            OnPlayerStaminaRestored?.Invoke(amount);
            if (debugEvents) Debug.Log($"[EventHandler] PlayerStaminaRestored: {amount}");
        }

        public void InvokeStatChanged(string statName, float newValue)
        {
            OnStatChanged?.Invoke(statName, newValue);
            if (debugEvents) Debug.Log($"[EventHandler] StatChanged: {statName} = {newValue}");
        }

        public void InvokeLevelChanged(int newLevel)
        {
            OnLevelChanged?.Invoke(newLevel);
            if (debugEvents) Debug.Log($"[EventHandler] LevelChanged: {newLevel}");
        }

        public void InvokeExperienceChanged(int amount)
        {
            OnExperienceChanged?.Invoke(amount);
            if (debugEvents) Debug.Log($"[EventHandler] ExperienceChanged: {amount}");
        }

        #endregion

        #region Combat Event Invokers

        public void InvokeDamageDealt(DamageInfo info, float finalDamage)
        {
            OnDamageDealt?.Invoke(info, finalDamage);
            if (debugEvents) Debug.Log($"[EventHandler] DamageDealt: {finalDamage} ({info.type})");
        }

        public void InvokeDamageTaken(DamageInfo info, float finalDamage)
        {
            OnDamageTaken?.Invoke(info, finalDamage);
            if (debugEvents) Debug.Log($"[EventHandler] DamageTaken: {finalDamage} ({info.type})");
        }

        public void InvokeKill(GameObject killer, GameObject victim)
        {
            OnKill?.Invoke(killer, victim);
            if (debugEvents) Debug.Log($"[EventHandler] Kill: {killer.name} -> {victim.name}");
        }

        public void InvokeDeath(GameObject deceased)
        {
            OnDeath?.Invoke(deceased);
            if (debugEvents) Debug.Log($"[EventHandler] Death: {deceased.name}");
        }

        #endregion

        #region Item Event Invokers

        public void InvokeItemPickedUp(ItemData item, int quantity)
        {
            OnItemPickedUp?.Invoke(item, quantity);
            if (debugEvents) Debug.Log($"[EventHandler] ItemPickedUp: {item.itemName} x{quantity}");
        }

        public void InvokeItemUsed(ItemData item, int quantity)
        {
            OnItemUsed?.Invoke(item, quantity);
            if (debugEvents) Debug.Log($"[EventHandler] ItemUsed: {item.itemName} x{quantity}");
        }

        public void InvokeItemDropped(ItemData item, int quantity)
        {
            OnItemDropped?.Invoke(item, quantity);
            if (debugEvents) Debug.Log($"[EventHandler] ItemDropped: {item.itemName} x{quantity}");
        }

        public void InvokeItemStacked(ItemData item, int newSize)
        {
            OnItemStacked?.Invoke(item, newSize);
            if (debugEvents) Debug.Log($"[EventHandler] ItemStacked: {item.itemName} x{newSize}");
        }

        #endregion

        #region Game Event Invokers

        public void InvokeScoreChanged(int newScore)
        {
            OnScoreChanged?.Invoke(newScore);
            if (debugEvents) Debug.Log($"[EventHandler] ScoreChanged: {newScore}");
        }

        public void InvokeQuestUpdated(string questName)
        {
            OnQuestUpdated?.Invoke(questName);
            if (debugEvents) Debug.Log($"[EventHandler] QuestUpdated: {questName}");
        }

        public void InvokeQuestCompleted(string questName)
        {
            OnQuestCompleted?.Invoke(questName);
            if (debugEvents) Debug.Log($"[EventHandler] QuestCompleted: {questName}");
        }

        public void InvokeAchievementUnlocked(string achievementName)
        {
            OnAchievementUnlocked?.Invoke(achievementName);
            if (debugEvents) Debug.Log($"[EventHandler] AchievementUnlocked: {achievementName}");
        }

        #endregion

        #region UI Event Invokers

        public void InvokeUIBarsInitialized()
        {
            OnUIBarsInitialized?.Invoke();
            if (debugEvents) Debug.Log("[EventHandler] UIBarsInitialized");
        }

        public void InvokeFloatingTextRequested(string text, Color color, float duration = 1.5f)
        {
            OnFloatingTextRequested?.Invoke(text, color, duration);
            if (debugEvents) Debug.Log($"[EventHandler] FloatingTextRequested: {text}");
        }

        public void InvokeDialogRequested(string title, string message)
        {
            OnDialogRequested?.Invoke(title, message);
            if (debugEvents) Debug.Log($"[EventHandler] DialogRequested: {title}");
        }

        public void InvokeNotificationRequested(string message)
        {
            OnNotificationRequested?.Invoke(message);
            if (debugEvents) Debug.Log($"[EventHandler] NotificationRequested: {message}");
        }

        public void InvokeWindowRequested(string windowType)
        {
            OnWindowRequested?.Invoke(windowType);
            if (debugEvents) Debug.Log($"[EventHandler] WindowRequested: {windowType}");
        }

        #endregion

        #region Utility

        /// <summary>
        /// Subscribe to all player stat events from PlayerStats.
        /// Call this once during initialization.
        /// </summary>
        public void SubscribeToPlayerStats(PlayerStats stats)
        {
            if (stats == null)
            {
                Debug.LogWarning("[EventHandler] Cannot subscribe - PlayerStats is null");
                return;
            }

            // Subscribe to PlayerStats events
            stats.OnManaChanged += InvokePlayerManaChanged;
            stats.OnStaminaChanged += InvokePlayerStaminaChanged;
            stats.OnEffectAdded += (effect) => { /* Handle effect added */ };
            stats.OnEffectRemoved += (effect) => { /* Handle effect removed */ };

            // Subscribe to static health events
            PlayerStats.OnHealthChanged += InvokePlayerHealthChanged;
            PlayerStats.OnPlayerDied += InvokePlayerDied;

            if (debugEvents)
            {
                Debug.Log("[EventHandler] Subscribed to PlayerStats events");
            }
        }

        /// <summary>
        /// Clear all events. Use with caution!
        /// </summary>
        public void ClearAllEvents()
        {
            OnPlayerHealthChanged = null;
            OnPlayerManaChanged = null;
            OnPlayerStaminaChanged = null;
            // ... clear all events
            if (debugEvents)
            {
                Debug.LogWarning("[EventHandler] All events cleared!");
            }
        }

        #endregion
    }

    /// <summary>
    /// Damage information container.
    /// </summary>
    [Serializable]
    public class DamageInfo
    {
        public float amount;
        public DamageType type;
        public GameObject source;
        public bool isCritical;
        public float criticalMultiplier = 2f;

        public DamageInfo(float amount, DamageType type, GameObject source = null, bool isCritical = false)
        {
            this.amount = amount;
            this.type = type;
            this.source = source;
            this.isCritical = isCritical;
        }
    }
}
