// SeedProgression.cs
// Manages alpha-digital seed progression across levels/scenes
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Starts with 5-character seed, adds 1 character per level/scene

using System.Text;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Singleton that manages seed progression across game sessions.
    /// Seed starts at 5 characters, grows by 1 each level/scene.
    /// </summary>
    public class SeedProgression : MonoBehaviour
    {
        #region Singleton

        private static SeedProgression _instance;
        public static SeedProgression Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SeedProgression>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SeedProgression");
                        _instance = go.AddComponent<SeedProgression>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Inspector Fields

        [Header("Seed Settings")]
        [SerializeField] private int startingSeedLength = 5;
        [SerializeField] private int maxSeedLength = 16;
        [SerializeField] private string baseSeedPrefix = "LAVOS";
        
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
                    GenerateSeedForLevel(_currentLevel);
                }
                return _currentSeed;
            }
        }

        /// <summary>
        /// Current seed length (starts at 5, increases per level).
        /// </summary>
        public int CurrentSeedLength => Mathf.Min(startingSeedLength + _currentLevel, maxSeedLength);

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
        /// Initialize seed progression system.
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

            _isInitialized = true;
            Debug.Log($"[SeedProgression] Initialized - Level: {_currentLevel}, Seed: {_currentSeed}");
        }

        #endregion

        #region Seed Generation

        /// <summary>
        /// Generate seed for current level.
        /// Seed length = startingSeedLength + currentLevel (capped at maxSeedLength)
        /// </summary>
        private void GenerateSeedForLevel(int level)
        {
            int seedLength = Mathf.Min(startingSeedLength + level, maxSeedLength);
            
            StringBuilder sb = new StringBuilder(baseSeedPrefix);
            
            // Add progressive characters
            while (sb.Length < seedLength)
            {
                // Generate deterministic character based on level
                char c = GetCharacterForLevel(level, sb.Length);
                sb.Append(c);
            }

            _currentSeed = sb.ToString();
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
        /// Generate a completely random seed (for testing).
        /// </summary>
        public string GenerateRandomSeed(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            StringBuilder sb = new StringBuilder();
            
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[Random.Range(0, chars.Length)]);
            }
            
            return sb.ToString();
        }

        #endregion

        #region Progression

        /// <summary>
        /// Advance to next level/scene.
        /// Seed length increases by 1.
        /// </summary>
        public void NextLevel()
        {
            _currentLevel++;
            GenerateSeedForLevel(_currentLevel);
            
            Debug.Log($"[SeedProgression] Advanced to Level {_currentLevel} - Seed: {_currentSeed} (Length: {_currentSeed.Length})");
            
            if (saveBetweenSessions)
            {
                SaveProgress();
            }
        }

        /// <summary>
        /// Reset progress to beginning.
        /// </summary>
        public void ResetProgress()
        {
            _currentLevel = 0;
            GenerateSeedForLevel(_currentLevel);
            
            Debug.Log($"[SeedProgression] Reset to Level 0 - Seed: {_currentSeed}");
            
            if (saveBetweenSessions)
            {
                SaveProgress();
            }
        }

        /// <summary>
        /// Set specific level (for testing/debugging).
        /// </summary>
        public void SetLevel(int level)
        {
            _currentLevel = Mathf.Max(0, level);
            GenerateSeedForLevel(_currentLevel);
            
            Debug.Log($"[SeedProgression] Set to Level {_currentLevel} - Seed: {_currentSeed}");
        }

        #endregion

        #region Persistence

        /// <summary>
        /// Save progress to PlayerPrefs.
        /// </summary>
        private void SaveProgress()
        {
            PlayerPrefs.SetInt(saveKey, _currentLevel);
            PlayerPrefs.Save();
            Debug.Log($"[SeedProgression] Progress saved - Level: {_currentLevel}");
        }

        /// <summary>
        /// Load progress from PlayerPrefs.
        /// </summary>
        private void LoadProgress()
        {
            if (PlayerPrefs.HasKey(saveKey))
            {
                _currentLevel = PlayerPrefs.GetInt(saveKey);
                GenerateSeedForLevel(_currentLevel);
                Debug.Log($"[SeedProgression] Progress loaded - Level: {_currentLevel}, Seed: {_currentSeed}");
            }
            else
            {
                // First time - start fresh
                _currentLevel = 0;
                GenerateSeedForLevel(_currentLevel);
                Debug.Log($"[SeedProgression] No save found - starting fresh at Level 0");
            }
        }

        /// <summary>
        /// Clear saved progress.
        /// </summary>
        public void ClearSavedProgress()
        {
            if (PlayerPrefs.HasKey(saveKey))
            {
                PlayerPrefs.DeleteKey(saveKey);
                Debug.Log("[SeedProgression] Saved progress cleared");
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get seed for a specific level (without changing current level).
        /// </summary>
        public string GetSeedForLevel(int level)
        {
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
        /// </summary>
        public int GetCurrentComplexity()
        {
            return CurrentSeedLength * 10 + _currentLevel;
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!Application.isEditor) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"[SeedProgression DEBUG]");
            GUILayout.Label($"Level: {_currentLevel}");
            GUILayout.Label($"Seed: {_currentSeed}");
            GUILayout.Label($"Seed Length: {_currentSeed.Length}");
            GUILayout.Label($"Complexity: {GetCurrentComplexity()}");

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

            GUILayout.EndArea();
        }

        #endregion
    }
}
