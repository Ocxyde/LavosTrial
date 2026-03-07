// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Code.Lavos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Code.Lavos.  If not, see <https://www.gnu.org/licenses/>.
// SeedManager.cs
// Centralized seed management with plug-in-out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// COMPUTE SEED SYSTEM:
// - Seed DESTROYED after each use
// - Immediately RESEEDED for next scene
// - New seed on every scene load/reload
// - New seed on game restart
//
// Modular system for seed generation, progression, and persistence

using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Seed types for different progression modes.
    /// </summary>
    public enum SeedMode
    {
        Progressive,    // Seed grows each level (default)
        Fixed,          // Same seed for all levels
        Random,         // New random seed each level
        Daily,          // Same seed for all players on same day
        Custom          // Manually set seed
    }

    /// <summary>
    /// Centralized seed manager with plug-in-out architecture.
    /// Access via SeedManager.Instance from anywhere.
    /// </summary>
    public class SeedManager : MonoBehaviour
    {
        #region Singleton

        // [REMOVED 2026-03-03] Use EventHandler events instead

        #endregion

        #region Inspector Fields

        [Header("Seed Mode")]
        [SerializeField] private SeedMode seedMode = SeedMode.Progressive;
        
        [Header("Progressive Settings")]
        [SerializeField] private int startingSeedLength = 5;
        [SerializeField] private int maxSeedLength = 16;
        [SerializeField] private string baseSeedPrefix = "LAVOS";
        
        [Header("Custom Seed")]
        [SerializeField] private string customSeed = "";
        
        [Header("Persistence")]
        [SerializeField] private bool saveBetweenSessions = true;
        [SerializeField] private string saveKey = "GameSeedProgress";

        #endregion

        #region Private Fields

        private int _currentLevel = 0;
        private string _currentSeed = "";
        private bool _isInitialized = false;

        // Compute seed - DESTROYED after each use, RESEEDED immediately
        private uint _currentComputeSeed = 0;
        private bool _computeSeedInitialized = false;
        private static SeedManager _instance;

        #endregion

        #region Properties

        /// <summary>
        /// Get singleton instance.
        /// </summary>
        public static SeedManager Instance => _instance;

        /// <summary>
        /// Current level number (0-based).
        /// </summary>
        public int CurrentLevel => _currentLevel;

        /// <summary>
        /// Get compute seed (auto-generates if not initialized).
        /// Destroyed after each use, reseeded immediately.
        /// </summary>
        public uint ComputeSeed
        {
            get
            {
                if (!_computeSeedInitialized)
                {
                    _currentComputeSeed = GenerateComputeSeed();
                    _computeSeedInitialized = true;
                    Debug.Log($"[SeedManager] Compute seed generated: {_currentComputeSeed}");
                }
                return _currentComputeSeed;
            }
        }

        /// <summary>
        /// Current seed string (alpha-digital).
        /// </summary>
        public string CurrentSeed
        {
            get
            {
                if (string.IsNullOrEmpty(_currentSeed))
                {
                    GenerateSeed();
                }
                return _currentSeed;
            }
        }

        /// <summary>
        /// Current seed mode.
        /// </summary>
        public SeedMode Mode => seedMode;

        /// <summary>
        /// Current seed length.
        /// </summary>
        public int CurrentSeedLength
        {
            get
            {
                if (seedMode == SeedMode.Progressive)
                {
                    return Mathf.Min(startingSeedLength + _currentLevel, maxSeedLength);
                }
                return startingSeedLength;
            }
        }

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

            // Do NOT use DontDestroyOnLoad - we want fresh seed each scene
            Initialize();

            // Generate compute seed for this scene
            uint seed = ComputeSeed;
            Debug.Log($"[SeedManager] Scene initialized with compute seed: {seed}");

            // Publish seed via EventHandler
            PublishComputeSeed(seed);

            // Destroy and reseed immediately for next scene
            DestroyAndReseed();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnApplicationQuit()
        {
            DestroySeed();
            _instance = null;
            Debug.Log("[SeedManager] Application quit - compute seed destroyed");
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize seed manager.
        /// Plug-in-out: Call this once at game start.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            if (saveBetweenSessions)
            {
                LoadProgress();
            }
            else
            {
                ResetProgress();
            }

            GenerateSeed();
            _isInitialized = true;

            Debug.Log($"[SeedManager] Initialized - Mode: {seedMode}, Level: {_currentLevel}, Seed: {_currentSeed}");
        }

        #endregion

        #region Compute Seed Management

        /// <summary>
        /// Destroy compute seed (clears for immediate reseed).
        /// </summary>
        private void DestroySeed()
        {
            if (_computeSeedInitialized)
            {
                uint oldSeed = _currentComputeSeed;
                _currentComputeSeed = 0;
                _computeSeedInitialized = false;
                Debug.Log($"[SeedManager] Compute seed destroyed: {oldSeed}");
            }
        }

        /// <summary>
        /// Destroy and immediately reseed.
        /// Called after seed is used to ensure fresh seed for next scene.
        /// Execution time: ~0.05ms (negligible)
        /// </summary>
        private void DestroyAndReseed()
        {
            DestroySeed();
            uint newSeed = ComputeSeed;
            Debug.Log($"[SeedManager] Destroyed and reseeded: {newSeed}");
        }

        /// <summary>
        /// Generate truly random compute seed from system entropy.
        /// Combines multiple entropy sources for maximum randomness.
        /// Execution time: ~0.03ms (SHA256 hash)
        /// </summary>
        private uint GenerateComputeSeed()
        {
            // Combine multiple entropy sources
            int tickCount = Environment.TickCount;
            int guidHash = Guid.NewGuid().GetHashCode();
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            float random = UnityEngine.Random.value;

            // Convert to uint and XOR for maximum entropy
            uint tick = (uint)tickCount;
            uint guid = (uint)guidHash;
            uint time = (uint)(timestamp & 0xFFFFFFFF);
            uint rand = (uint)(random * uint.MaxValue);

            // XOR all sources
            uint seed = tick ^ guid ^ time ^ rand;

            // Hash for better distribution (SHA256 -> first 4 bytes)
            byte[] seedBytes = BitConverter.GetBytes(seed);
            byte[] hash;
            using (var sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(seedBytes);
            }

            // Convert back to uint (ensure positive)
            uint finalSeed = (uint)BitConverter.ToInt32(hash, 0) & 0x7FFFFFFF;

            return finalSeed;
        }

        /// <summary>
        /// Publish compute seed via EventHandler.
        /// </summary>
        private void PublishComputeSeed(uint seed)
        {
            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.InvokeComputeSeedChanged(seed);
                Debug.Log($"[SeedManager] Published compute seed via EventHandler: {seed}");
            }
            else
            {
                Debug.LogWarning("[SeedManager] EventHandler not found - seed not published");
            }
        }

        #endregion

        #region Scene Callbacks

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[SeedManager] Scene loaded: {scene.name}");

            // Generate new compute seed for this scene
            uint seed = ComputeSeed;

            // Publish to subscribers
            PublishComputeSeed(seed);

            // Destroy and reseed immediately for next scene
            DestroyAndReseed();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            Debug.Log($"[SeedManager] Scene unloaded: {scene.name}");
            // Seed already destroyed in OnSceneLoaded
        }

        #endregion

        #region Seed Generation

        /// <summary>
        /// Generate seed based on current mode.
        /// </summary>
        private void GenerateSeed()
        {
            switch (seedMode)
            {
                case SeedMode.Progressive:
                    GenerateProgressiveSeed();
                    break;
                case SeedMode.Fixed:
                    GenerateFixedSeed();
                    break;
                case SeedMode.Random:
                    GenerateRandomSeed();
                    break;
                case SeedMode.Daily:
                    GenerateDailySeed();
                    break;
                case SeedMode.Custom:
                    GenerateCustomSeed();
                    break;
            }
        }

        /// <summary>
        /// Generate progressive seed (grows each level).
        /// </summary>
        private void GenerateProgressiveSeed()
        {
            int seedLength = Mathf.Min(startingSeedLength + _currentLevel, maxSeedLength);
            
            StringBuilder sb = new StringBuilder(baseSeedPrefix);
            
            while (sb.Length < seedLength)
            {
                char c = GetCharacterForLevel(_currentLevel, sb.Length);
                sb.Append(c);
            }

            _currentSeed = sb.ToString();
        }

        /// <summary>
        /// Generate fixed seed (same for all levels).
        /// </summary>
        private void GenerateFixedSeed()
        {
            _currentSeed = baseSeedPrefix;
        }

        /// <summary>
        /// Generate random seed (new each level).
        /// </summary>
        private void GenerateRandomSeed()
        {
            _currentSeed = GenerateRandomSeedString(startingSeedLength);
        }

        /// <summary>
        /// Generate daily seed (same for all players on same day).
        /// </summary>
        private void GenerateDailySeed()
        {
            string dateStr = System.DateTime.Now.ToString("yyyy-MM-dd");
            _currentSeed = $"DAILY-{dateStr}";
        }

        /// <summary>
        /// Generate custom seed (manually set).
        /// </summary>
        private void GenerateCustomSeed()
        {
            _currentSeed = string.IsNullOrEmpty(customSeed) ? 
                GenerateRandomSeedString(startingSeedLength) : customSeed;
        }

        /// <summary>
        /// Get deterministic character for specific level and position.
        /// </summary>
        private char GetCharacterForLevel(int level, int position)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // No I, O, 0, 1 (confusing)
            
            // Deterministic based on level + position
            int index = (level * 7 + position * 13) % chars.Length;
            return chars[index];
        }

        /// <summary>
        /// Generate random seed string.
        /// </summary>
        private string GenerateRandomSeedString(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            StringBuilder sb = new StringBuilder();
            
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
            }
            
            return sb.ToString();
        }

        #endregion

        #region Progression (Plug-in-Out)

        /// <summary>
        /// Advance to next level/scene.
        /// Plug-in-out: Call when loading new level.
        /// </summary>
        public void NextLevel()
        {
            _currentLevel++;
            GenerateSeed();
            
            Debug.Log($"[SeedManager] Advanced to Level {_currentLevel} - Seed: {_currentSeed} (Length: {_currentSeed.Length})");
            
            if (saveBetweenSessions)
            {
                SaveProgress();
            }
        }

        /// <summary>
        /// Reset progress to beginning.
        /// Plug-in-out: Call on new game.
        /// </summary>
        public void ResetProgress()
        {
            _currentLevel = 0;
            GenerateSeed();
            
            Debug.Log($"[SeedManager] Reset to Level 0 - Seed: {_currentSeed}");
            
            if (saveBetweenSessions)
            {
                SaveProgress();
            }
        }

        /// <summary>
        /// Set specific level (for testing/debugging).
        /// Plug-in-out: Use with caution.
        /// </summary>
        public void SetLevel(int level)
        {
            _currentLevel = Mathf.Max(0, level);
            GenerateSeed();
            
            Debug.Log($"[SeedManager] Set to Level {_currentLevel} - Seed: {_currentSeed}");
        }

        #endregion

        #region Persistence

        /// <summary>
        /// Save progress to PlayerPrefs.
        /// </summary>
        private void SaveProgress()
        {
            PlayerPrefs.SetInt(saveKey, _currentLevel);
            PlayerPrefs.SetString($"{saveKey}_Mode", seedMode.ToString());
            PlayerPrefs.Save();
            Debug.Log($"[SeedManager] Progress saved - Level: {_currentLevel}");
        }

        /// <summary>
        /// Load progress from PlayerPrefs.
        /// </summary>
        private void LoadProgress()
        {
            if (PlayerPrefs.HasKey(saveKey))
            {
                _currentLevel = PlayerPrefs.GetInt(saveKey);
                
                if (PlayerPrefs.HasKey($"{saveKey}_Mode"))
                {
                    Enum.TryParse(PlayerPrefs.GetString($"{saveKey}_Mode"), out seedMode);
                }
                
                GenerateSeed();
                Debug.Log($"[SeedManager] Progress loaded - Level: {_currentLevel}, Seed: {_currentSeed}");
            }
            else
            {
                // First time - start fresh
                _currentLevel = 0;
                GenerateSeed();
                Debug.Log($"[SeedManager] No save found - starting fresh at Level 0");
            }
        }

        /// <summary>
        /// Clear saved progress.
        /// Plug-in-out: Call on "New Game" or reset.
        /// </summary>
        public void ClearSavedProgress()
        {
            if (PlayerPrefs.HasKey(saveKey))
            {
                PlayerPrefs.DeleteKey(saveKey);
                PlayerPrefs.DeleteKey($"{saveKey}_Mode");
                Debug.Log("[SeedManager] Saved progress cleared");
            }
        }

        #endregion

        #region Utilities (Plug-in-Out API)

        /// <summary>
        /// Get seed for a specific level (without changing current level).
        /// Plug-in-out: Use for preview or planning.
        /// </summary>
        public string GetSeedForLevel(int level)
        {
            if (seedMode != SeedMode.Progressive)
            {
                return CurrentSeed;
            }
            
            int seedLength = Mathf.Min(startingSeedLength + level, maxSeedLength);
            
            StringBuilder sb = new StringBuilder(baseSeedPrefix);
            
            while (sb.Length < seedLength)
            {
                sb.Append(GetCharacterForLevel(level, sb.Length));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get complexity for current level.
        /// Plug-in-out: Use for difficulty scaling.
        /// </summary>
        public int GetCurrentComplexity()
        {
            return CurrentSeedLength * 10 + _currentLevel;
        }

        /// <summary>
        /// Set seed mode at runtime.
        /// Plug-in-out: Use for game options.
        /// </summary>
        public void SetSeedMode(SeedMode newMode)
        {
            seedMode = newMode;
            GenerateSeed();
            Debug.Log($"[SeedManager] Mode changed to {newMode} - Seed: {_currentSeed}");
        }

        /// <summary>
        /// Set custom seed at runtime.
        /// Plug-in-out: Use for cheat codes or level passwords.
        /// Also called by ShareSystm when importing maze codes.
        /// </summary>
        /// <param name="seed">Custom seed string or maze code.</param>
        public void SetCustomSeed(string seed)
        {
            if (string.IsNullOrEmpty(seed))
            {
                Debug.LogWarning("[SeedManager] SetCustomSeed called with empty seed");
                return;
            }

            customSeed = seed;
            
            // If seed looks like a maze code (LAVOS-xxx), extract the numeric seed
            if (seed.StartsWith("LAVOS-"))
            {
                string[] parts = seed.Split('-');
                if (parts.Length >= 2 && uint.TryParse(parts[1], out uint numericSeed))
                {
                    _currentComputeSeed = numericSeed;
                    _computeSeedInitialized = true;
                    Debug.Log($"[SeedManager] Compute seed set from maze code: {numericSeed}");
                }
            }
            
            if (seedMode == SeedMode.Custom)
            {
                GenerateSeed();
            }
            Debug.Log($"[SeedManager] Custom seed set: {seed}");
        }

        #endregion
    }
}
