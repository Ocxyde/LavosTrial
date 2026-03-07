// SeedManager.cs
// Centralized seed management with plug-in-out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Modular system for seed generation, progression, and persistence

using System;
using System.Text;
using UnityEngine;

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

        /// <summary>
        /// [OBSOLETE] Use EventHandler events instead of direct singleton access.
        /// Deprecated: 2026-03-03
        /// </summary>
        public static SeedManager Instance
        {
            get
            {
                if (_instance == null && !_instanceChecked)
                {
                    _instance = FindFirstObjectByType<SeedManager>();
                    _instanceChecked = true;
                    
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SeedManager");
                        _instance = go.AddComponent<SeedManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

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

        #endregion

        #region Properties

        /// <summary>
        /// Current level number (0-based).
        /// </summary>
        public int CurrentLevel => _currentLevel;

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
            DontDestroyOnLoad(gameObject);
            
            Initialize();
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
        /// </summary>
        public void SetCustomSeed(string seed)
        {
            customSeed = seed;
            if (seedMode == SeedMode.Custom)
            {
                GenerateSeed();
            }
            Debug.Log($"[SeedManager] Custom seed set: {seed}");
        }

        #endregion

        #region Debug GUI

        private void OnGUI()
        {
            // Debug UI disabled for production
            return;
            
            // Original code below (disabled)
            /*
            if (!Application.isEditor) return;

            GUILayout.BeginArea(new Rect(10, 10, 350, 300));
            GUILayout.Label($"[SeedManager DEBUG]");
            GUILayout.Label($"Mode: {seedMode}");
            GUILayout.Label($"Level: {_currentLevel}");
            GUILayout.Label($"Seed: {_currentSeed}");
            GUILayout.Label($"Seed Length: {_currentSeed.Length}");
            GUILayout.Label($"Complexity: {GetCurrentComplexity()}");

            GUILayout.Space(10);

            if (GUILayout.Button("Next Level"))
            {
                NextLevel();
            }

            if (GUILayout.Button("Reset Progress"))
            {
                ResetProgress();
            }

            if (GUILayout.Button("Clear Save"))
            {
                ClearSavedProgress();
            }

            GUILayout.Space(10);
            GUILayout.Label("Change Mode:");

            if (GUILayout.Button("Progressive"))
            {
                SetSeedMode(SeedMode.Progressive);
            }

            if (GUILayout.Button("Fixed"))
            {
                SetSeedMode(SeedMode.Fixed);
            }

            if (GUILayout.Button("Random"))
            {
                SetSeedMode(SeedMode.Random);
            }

            if (GUILayout.Button("Daily"))
            {
                SetSeedMode(SeedMode.Daily);
            }

            GUILayout.EndArea();
            */
        }

        #endregion
    }
}
