// Copyright (C) 2026 Ocxyde
//
// This file is part of PeuImporte.
//
// PeuImporte is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeuImporte is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeuImporte.  If not, see <https://www.gnu.org/licenses/>.
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
    /// Performance: Cached singleton access for minimal overhead.
    /// </summary>
    public class EventHandler : MonoBehaviour
    {
        // Cached singleton reference (performance optimization)
        private static EventHandler _instance;
        private static bool _instanceChecked = false;
        
        public static EventHandler Instance
        {
            get
            {
                if (_instance == null && !_instanceChecked)
                {
                    _instance = FindFirstObjectByType<EventHandler>();
                    _instanceChecked = true;

                    if (_instance == null)
                    {
                        // Only warn if actually playing (not in editor pause)
                        if (Application.isPlaying)
                        {
                            Debug.LogWarning("[EventHandler] No instance found in scene!");
                        }
                    }
                }
                return _instance;
            }
        }

        [Header("Settings")]
        [SerializeField] private bool dontDestroyOnLoad = true;
        public bool debugEvents = false;

        #region Player Events

        // Health
        public event Action<float, float> OnPlayerHealthChanged;
        public event Action<float> OnPlayerDamaged;
        public event Action<float> OnPlayerHealed;
        public event Action OnPlayerDied;
        public event Action OnPlayerRespawned;

        // Mana
        public event Action<float, float> OnPlayerManaChanged;
        public event Action<float> OnPlayerManaUsed;
        public event Action<float> OnPlayerManaRestored;

        // Stamina
        public event Action<float, float> OnPlayerStaminaChanged;
        public event Action<float> OnPlayerStaminaUsed;
        public event Action<float> OnPlayerStaminaRestored;

        // Stats
        public event Action<string, float> OnStatChanged;
        public event Action<int> OnLevelChanged;
        public event Action<int> OnExperienceChanged;

        #endregion

        #region Combat Events

        public event Action<Status.DamageInfo, float> OnDamageDealt;
        public event Action<Status.DamageInfo, float> OnDamageTaken;
        public event Action<GameObject, GameObject> OnKill;
        public event Action<GameObject> OnDeath;

        #endregion

        #region Item Events

        public event Action<ItemData, int> OnItemPickedUp;
        public event Action<ItemData, int> OnItemUsed;
        public event Action<ItemData, int> OnItemDropped;
        public event Action<ItemData, int> OnItemStacked;

        #endregion

        #region Door Events

        public event Action<Vector3, DoorVariant> OnDoorOpened;
        public event Action<Vector3, DoorVariant> OnDoorClosed;
        public event Action<Vector3> OnDoorLocked;
        public event Action<Vector3, DoorTrapType> OnDoorTrapTriggered;

        #endregion

        #region Chest Events

        public event Action<Vector3, int> OnChestOpened;
        public event Action<Vector3> OnChestClosed;
        public event Action<Vector3, int, GameObject> OnChestLootGenerated;
        public event Action<Vector3, LootTable> OnChestItemSpawned;

        #endregion

        #region Game Events

        public event Action<int> OnScoreChanged;
        public event Action<string> OnQuestUpdated;
        public event Action<string> OnQuestCompleted;
        public event Action<string> OnAchievementUnlocked;
        
        // Game State Management
        public event Action<GameManager.GameState> OnGameStateChanged;
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        public event Action OnGameOver;
        public event Action OnGameVictory;
        
        #endregion

        #region Maze Events

        public event Action<int> OnMazeLevelChanged;
        public event Action OnMazeGenerated;  // Published when complete maze generation finishes

        #endregion
        
        #region Material Events
        
        public event Action<ProceduralCompute.MaterialType, ProceduralCompute.TextureType> OnMaterialRequested;
        public event Action<ProceduralCompute.TextureType, ProceduralCompute.MaterialType> OnTextureRequested;
        
        #endregion

        #region UI Events

        public event Action OnUIBarsInitialized;
        public event Action<string, Color, float> OnFloatingTextRequested;
        public event Action<string, string> OnDialogRequested;
        public event Action<string> OnNotificationRequested;
        public event Action<string> OnWindowRequested;

        #endregion

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (debugEvents)
            {
                Debug.Log("[EventHandler] Initialized - All systems ready");
            }
        }

        void Start()
        {
            // Delay subscription to ensure all systems are initialized
            Invoke(nameof(DelayedPlayerStatsSubscription), 0.2f);
            Invoke(nameof(RetryPlayerStatsSubscription), 1.0f); // Retry after 1s if first fails
        }

        private void DelayedPlayerStatsSubscription()
        {
            var playerStats = FindFirstObjectByType<Component>() as IPlayerStats;
            if (playerStats != null)
            {
                SubscribeToPlayerStats(playerStats);
                Debug.Log("[EventHandler] Successfully subscribed to PlayerStats events");
            }
            else
            {
                Debug.LogWarning("[EventHandler] PlayerStats not found for subscription - will retry");
            }
        }

        private void RetryPlayerStatsSubscription()
        {
            // Check if already subscribed via OnPlayerHealthChangedStatic subscribers count
            var playerStats = FindFirstObjectByType<Component>() as IPlayerStats;
            if (playerStats != null)
            {
                SubscribeToPlayerStats(playerStats);
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

        public void InvokeDamageDealt(Status.DamageInfo info, float finalDamage)
        {
            OnDamageDealt?.Invoke(info, finalDamage);
            if (debugEvents) Debug.Log($"[EventHandler] DamageDealt: {finalDamage} ({info.type})");
        }

        public void InvokeDamageTaken(Status.DamageInfo info, float finalDamage)
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
        
        public void InvokeMazeLevelChanged(int newLevel)
        {
            OnMazeLevelChanged?.Invoke(newLevel);
            if (debugEvents) Debug.Log($"[EventHandler] MazeLevelChanged: {newLevel}");
        }

        public void InvokeMazeGenerated()
        {
            OnMazeGenerated?.Invoke();
            if (debugEvents) Debug.Log($"[EventHandler] MazeGenerated: Complete maze generation finished");
        }

        public void RequestMaterial(ProceduralCompute.MaterialType type, ProceduralCompute.TextureType textureType)
        {
            // PLUG-IN-AND-OUT: Invoke event, don't call ProceduralCompute directly!
            OnMaterialRequested?.Invoke(type, textureType);
            if (debugEvents) Debug.Log($"[EventHandler] MaterialRequested: {type}_{textureType}");
            
            // Let subscribers respond to the event
            // ProceduralCompute (or any other system) can listen and provide material
        }
        
        public void RequestTexture(ProceduralCompute.TextureType type, ProceduralCompute.MaterialType material)
        {
            // PLUG-IN-AND-OUT: Invoke event, don't call ProceduralCompute directly!
            OnTextureRequested?.Invoke(type, material);
            if (debugEvents) Debug.Log($"[EventHandler] TextureRequested: {type}_{material}");
            
            // Let subscribers respond to the event
        }

        #endregion

        #region Game State Invokers

        public void InvokeGameStateChanged(GameManager.GameState newState)
        {
            OnGameStateChanged?.Invoke(newState);
            if (debugEvents) Debug.Log($"[EventHandler] GameStateChanged: {newState}");
        }
        
        public void InvokeGamePaused()
        {
            OnGamePaused?.Invoke();
            if (debugEvents) Debug.Log("[EventHandler] GamePaused");
        }
        
        public void InvokeGameResumed()
        {
            OnGameResumed?.Invoke();
            if (debugEvents) Debug.Log("[EventHandler] GameResumed");
        }
        
        public void InvokeGameOver()
        {
            OnGameOver?.Invoke();
            if (debugEvents) Debug.Log("[EventHandler] GameOver");
        }
        
        public void InvokeGameVictory()
        {
            OnGameVictory?.Invoke();
            if (debugEvents) Debug.Log("[EventHandler] GameVictory");
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

        #region Door Event Invokers

        public void InvokeDoorOpened(Vector3 position, DoorVariant variant)
        {
            OnDoorOpened?.Invoke(position, variant);
            if (debugEvents) Debug.Log($"[EventHandler] DoorOpened: {variant} at {position}");
        }

        public void InvokeDoorClosed(Vector3 position, DoorVariant variant)
        {
            OnDoorClosed?.Invoke(position, variant);
            if (debugEvents) Debug.Log($"[EventHandler] DoorClosed: {variant} at {position}");
        }

        public void InvokeDoorLocked(Vector3 position)
        {
            OnDoorLocked?.Invoke(position);
            if (debugEvents) Debug.Log($"[EventHandler] DoorLocked at {position}");
        }

        public void InvokeDoorTrapTriggered(Vector3 position, DoorTrapType trap)
        {
            OnDoorTrapTriggered?.Invoke(position, trap);
            if (debugEvents) Debug.Log($"[EventHandler] DoorTrapTriggered: {trap} at {position}");
        }

        #endregion

        #region Chest Event Invokers

        public void InvokeChestOpened(Vector3 position, int goldAmount)
        {
            OnChestOpened?.Invoke(position, goldAmount);
            if (debugEvents) Debug.Log($"[EventHandler] ChestOpened: {goldAmount} gold at {position}");
        }

        public void InvokeChestClosed(Vector3 position)
        {
            OnChestClosed?.Invoke(position);
            if (debugEvents) Debug.Log($"[EventHandler] ChestClosed at {position}");
        }

        public void InvokeChestLootGenerated(Vector3 position, int goldAmount, GameObject looter)
        {
            OnChestLootGenerated?.Invoke(position, goldAmount, looter);
            if (debugEvents) Debug.Log($"[EventHandler] ChestLootGenerated: {goldAmount} gold at {position}");
        }

        public void InvokeChestItemSpawned(Vector3 position, LootTable lootTable)
        {
            OnChestItemSpawned?.Invoke(position, lootTable);
            if (debugEvents) Debug.Log($"[EventHandler] ChestItemSpawned at {position}");
        }

        #endregion

        #region Utility

        /// <summary>
        /// Subscribe to all player stat events from PlayerStats.
        /// </summary>
        public void SubscribeToPlayerStats(IPlayerStats stats)
        {
            if (stats == null)
            {
                Debug.LogWarning("[EventHandler] Cannot subscribe - PlayerStats is null");
                return;
            }

            // Subscribe to instance events from IPlayerStats
            // Note: IPlayerStats only has OnManaChanged and OnStaminaChanged
            // Health/Death events are handled by PlayerStats directly (not in interface)
            stats.OnManaChanged += InvokePlayerManaChanged;
            stats.OnStaminaChanged += InvokePlayerStaminaChanged;

            if (debugEvents)
            {
                Debug.Log("[EventHandler] Subscribed to IPlayerStats events (Mana, Stamina)");
            }
        }

        #endregion

        private void OnDestroy()
        {
            // Instance assignment removed (Instance property removed)
            
            // Destroy the auto-created singleton GameObject when exiting play mode
            if (Application.isPlaying && gameObject.scene.name == null)
            {
                Destroy(gameObject);
            }
        }
    }
}
