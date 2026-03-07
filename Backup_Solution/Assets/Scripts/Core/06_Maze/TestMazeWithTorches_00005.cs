// TestMazeWithTorches.cs
// Comprehensive test harness for maze generation with torch placement
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Add this component to an empty GameObject in a test scene
//   2. Required components on same GameObject:
//      - MazeGenerator
//      - MazeRenderer
//      - TorchPool
//      - SpatialPlacer
//      - MazeIntegration
//   3. Press Play - maze auto-generates
//   4. Use keyboard shortcuts or context menu to test
//
// CONTROLS:
//   [R] - Regenerate maze with new random seed
//   [T] - Toggle torch placement on/off
//   [G] - Generate new maze (same seed)
//
// Location: Assets/Scripts/Core/06_Maze/

using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Lavos.Core
{
    /// <summary>
    /// TestMazeWithTorches - Complete test harness for maze + torch system.
    ///
    /// Features:
    /// - Automatic maze generation on start
    /// - Seed-based procedural generation (reproducible)
    /// - Real-time torch placement and removal
    /// - Performance stats and diagnostics
    /// - Runtime and editor testing support
    /// - Context menu actions for quick testing
    ///
    /// Requirements:
    /// - New Input System package
    /// - All required components on same GameObject
    /// - DrawingPool in scene (for torch textures)
    /// - SeedManager in scene (optional, for seed control)
    /// </summary>
    public class TestMazeWithTorches : MonoBehaviour
    {
        #region Inspector Settings

        [Header("Generation Settings")]
        [Tooltip("Auto-generate maze when scene starts")]
        [SerializeField] private bool autoGenerateOnStart = true;

        [Tooltip("Use random seed each generation (false = same seed for reproducible tests)")]
        [SerializeField] private bool useRandomSeed = true;

        [Tooltip("Seed string for procedural generation (leave empty for auto)")]
        [SerializeField] private string testSeed = "MazeTest2026";

        [Header("Maze Dimensions")]
        [Tooltip("Maze width (odd numbers work best for perfect mazes)")]
        [SerializeField] private int mazeWidth = 21;

        [Tooltip("Maze height (odd numbers work best for perfect mazes)")]
        [SerializeField] private int mazeHeight = 21;

        [Header("Torch Settings")]
        [Tooltip("Enable torch placement after maze generation")]
        [SerializeField] private bool placeTorches = true;

        [Tooltip("Number of torches to place (actual count may vary based on wall availability)")]
        [SerializeField] private int torchCount = 15;

        [Tooltip("Minimum distance between torches")]
        [SerializeField] private float minTorchDistance = 6f;

        [Header("Component References")]
        [Tooltip("MazeIntegration component (auto-finds if not assigned)")]
        [SerializeField] private MazeIntegration mazeIntegration;

        [Tooltip("SpatialPlacer component (auto-finds if not assigned)")]
        [SerializeField] private SpatialPlacer spatialPlacer;

        [Tooltip("MazeGenerator component (auto-finds if not assigned)")]
        [SerializeField] private MazeGenerator mazeGenerator;

        [Tooltip("MazeRenderer component (auto-finds if not assigned)")]
        [SerializeField] private MazeRenderer mazeRenderer;

        [Header("Debug Options")]
        [Tooltip("Show debug UI overlay")]
        [SerializeField] private bool showDebugUI = true;

        [Tooltip("Log detailed generation info to console")]
        [SerializeField] private bool verboseLogging = false;

        [Header("Keyboard Controls")]
        [SerializeField] private Key regenerateKey = Key.R;
        [SerializeField] private Key toggleTorchesKey = Key.T;
        [SerializeField] private Key generateSameSeedKey = Key.G;

        #endregion

        #region Private State

        private bool _mazeGenerated = false;
        private bool _torchesActive = false;
        private float _lastGenerationTime;
        private int _generationCount = 0;
        private string _currentSeed = "";

        // Timing stats
        private float _mazeGenTime = 0f;
        private float _torchPlaceTime = 0f;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            FindComponents();
            ValidateSetup();
        }

        void Start()
        {
            if (autoGenerateOnStart)
            {
                GenerateTestMaze();
            }
        }

        void Update()
        {
            HandleInput();
        }

        #endregion

        #region Component Setup

        /// <summary>
        /// Auto-find all required components on same GameObject.
        /// </summary>
        private void FindComponents()
        {
            if (mazeIntegration == null)
                mazeIntegration = GetComponent<MazeIntegration>();

            if (spatialPlacer == null)
                spatialPlacer = GetComponent<SpatialPlacer>();

            if (mazeGenerator == null)
                mazeGenerator = GetComponent<MazeGenerator>();

            if (mazeRenderer == null)
                mazeRenderer = GetComponent<MazeRenderer>();

            // Initialize seed from inspector
            if (string.IsNullOrEmpty(_currentSeed))
                _currentSeed = testSeed;
        }

        /// <summary>
        /// Validate that all required components are present.
        /// </summary>
        private void ValidateSetup()
        {
            bool hasErrors = false;

            if (mazeIntegration == null)
            {
                Debug.LogError("[TestMazeWithTorches] MISSING: MazeIntegration component!");
                hasErrors = true;
            }

            if (mazeGenerator == null)
            {
                Debug.LogError("[TestMazeWithTorches] MISSING: MazeGenerator component!");
                hasErrors = true;
            }

            if (mazeRenderer == null)
            {
                Debug.LogError("[TestMazeWithTorches] MISSING: MazeRenderer component!");
                hasErrors = true;
            }

            if (spatialPlacer == null)
            {
                Debug.LogError("[TestMazeWithTorches] MISSING: SpatialPlacer component!");
                hasErrors = true;
            }

            // Check TorchPool via SpatialPlacer or direct component
            var torchPool = GetComponent<TorchPool>();
            if (torchPool == null)
            {
                Debug.LogError("[TestMazeWithTorches] MISSING: TorchPool component!");
                Debug.LogError("[TestMazeWithTorches] All components must be on the same GameObject!");
                hasErrors = true;
            }

            if (hasErrors)
            {
                Debug.LogError("[TestMazeWithTorches] Use menu: Tools → Maze Test → Quick Setup to auto-configure!");
                enabled = false;
                return;
            }

            if (verboseLogging)
            {
                Debug.Log("[TestMazeWithTorches] All required components found. Ready to test.");
            }
        }

        #endregion

        #region Main Test Methods

        /// <summary>
        /// Generate a complete test maze with torches.
        /// Call this via context menu or keyboard shortcut.
        /// </summary>
        [ContextMenu("Generate Test Maze")]
        public void GenerateTestMaze()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            Log("=== Test Maze Generation Started ===");

            // Validate components
            if (mazeIntegration == null)
            {
                Debug.LogError("[TestMazeWithTorches] Cannot generate: MazeIntegration missing!");
                return;
            }

            // Configure seed
            ConfigureSeed();

            // Configure maze dimensions
            if (mazeGenerator != null)
            {
                mazeGenerator.width = mazeWidth;
                mazeGenerator.height = mazeHeight;
            }

            // Configure torch settings on SpatialPlacer
            ConfigureTorchSettings();

            // Generate maze
            _lastGenerationTime = Time.time;
            _generationCount++;

            var mazeStopwatch = System.Diagnostics.Stopwatch.StartNew();
            mazeIntegration.GenerateMaze();
            mazeStopwatch.Stop();
            _mazeGenTime = mazeStopwatch.ElapsedMilliseconds / 1000f;

            _mazeGenerated = true;

            // Place torches if enabled
            if (placeTorches && spatialPlacer != null)
            {
                var torchStopwatch = System.Diagnostics.Stopwatch.StartNew();
                spatialPlacer.PlaceTorches();
                torchStopwatch.Stop();
                _torchPlaceTime = torchStopwatch.ElapsedMilliseconds / 1000f;
                _torchesActive = true;
            }

            stopwatch.Stop();
            float totalTime = stopwatch.ElapsedMilliseconds / 1000f;

            // Log results
            Log($"=== Generation Complete ===");
            Log($"Seed: {_currentSeed}");
            Log($"Dimensions: {mazeWidth}x{mazeHeight}");
            Log($"Torches: {(_torchesActive ? "Enabled" : "Disabled")}");
            Log($"Timing: Maze={_mazeGenTime:F3}s, Torches={_torchPlaceTime:F3}s, Total={totalTime:F3}s");
            Log($"Generation #{_generationCount}");
        }

        /// <summary>
        /// Regenerate maze with a new random seed.
        /// </summary>
        [ContextMenu("Regenerate (New Random Seed)")]
        public void RegenerateMaze()
        {
            useRandomSeed = true;
            GenerateTestMaze();
        }

        /// <summary>
        /// Regenerate maze with the same seed (reproducible test).
        /// </summary>
        [ContextMenu("Regenerate (Same Seed)")]
        public void RegenerateSameSeed()
        {
            useRandomSeed = false;
            GenerateTestMaze();
        }

        /// <summary>
        /// Toggle torch placement on/off.
        /// </summary>
        [ContextMenu("Toggle Torches")]
        public void ToggleTorches()
        {
            if (spatialPlacer == null)
            {
                Debug.LogWarning("[TestMazeWithTorches] SpatialPlacer not found - cannot toggle torches!");
                return;
            }

            if (_torchesActive)
            {
                RemoveTorches();
            }
            else
            {
                PlaceTorches();
            }
        }

        /// <summary>
        /// Place torches on existing maze.
        /// </summary>
        [ContextMenu("Place Torches")]
        public void PlaceTorches()
        {
            if (spatialPlacer == null)
            {
                Debug.LogWarning("[TestMazeWithTorches] SpatialPlacer not found!");
                return;
            }

            if (!_mazeGenerated)
            {
                Debug.LogWarning("[TestMazeWithTorches] Generate maze first!");
                return;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            spatialPlacer.PlaceTorches();
            stopwatch.Stop();
            _torchPlaceTime = stopwatch.ElapsedMilliseconds / 1000f;
            _torchesActive = true;

            Log($"[TestMazeWithTorches] Torches placed in {_torchPlaceTime:F3}s");
        }

        /// <summary>
        /// Remove all placed torches.
        /// </summary>
        [ContextMenu("Remove Torches")]
        public void RemoveTorches()
        {
            if (spatialPlacer == null)
            {
                Debug.LogWarning("[TestMazeWithTorches] SpatialPlacer not found!");
                return;
            }

            spatialPlacer.RemoveTorches();
            _torchesActive = false;

            Log("[TestMazeWithTorches] Torches removed");
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Configure seed for procedural generation.
        /// </summary>
        private void ConfigureSeed()
        {
            if (useRandomSeed)
            {
                _currentSeed = System.Guid.NewGuid().ToString();
            }
            else if (string.IsNullOrEmpty(_currentSeed))
            {
                _currentSeed = testSeed;
            }

            // Set seed via SeedManager if available
            var seedManager = FindFirstObjectByType<SeedManager>();
            if (seedManager != null)
            {
                var seedField = seedManager.GetType().GetField("currentSeed",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                if (seedField != null)
                {
                    seedField.SetValue(seedManager, _currentSeed);
                }
            }

            testSeed = _currentSeed;
        }

        /// <summary>
        /// Configure torch placement settings on SpatialPlacer.
        /// </summary>
        private void ConfigureTorchSettings()
        {
            if (spatialPlacer == null) return;

            // Set torch count via reflection
            var torchCountField = spatialPlacer.GetType().GetField("torchCount",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (torchCountField != null)
            {
                torchCountField.SetValue(spatialPlacer, torchCount);
            }

            // Set min distance via reflection
            var minDistField = spatialPlacer.GetType().GetField("minDistanceBetweenTorches",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (minDistField != null)
            {
                minDistField.SetValue(spatialPlacer, minTorchDistance);
            }

            // Enable/disable via public property
            spatialPlacer.PlaceTorchesEnabled = placeTorches;
        }

        #endregion

        #region Input Handling

        /// <summary>
        /// Handle keyboard input for testing controls.
        /// </summary>
        private void HandleInput()
        {
            if (Keyboard.current == null) return;

            var keyboard = Keyboard.current;

            // Regenerate with new seed
            if (keyboard[regenerateKey].wasPressedThisFrame)
            {
                RegenerateMaze();
            }

            // Toggle torches
            if (keyboard[toggleTorchesKey].wasPressedThisFrame)
            {
                ToggleTorches();
            }

            // Regenerate with same seed
            if (keyboard[generateSameSeedKey].wasPressedThisFrame)
            {
                RegenerateSameSeed();
            }
        }

        #endregion

        #region Debug UI

        void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 400, 500));
            GUILayout.BeginVertical("box");

            GUILayout.Label("═══ Maze + Torches Test ═══", GUILayout.Height(25));
            GUILayout.Space(5);

            // Status
            GUILayout.Label($"Maze: {(_mazeGenerated ? "✅ Generated" : "⏳ Not Generated")}");
            GUILayout.Label($"Torches: {(_torchesActive ? "✅ Active" : "⏸ Inactive")}");
            GUILayout.Space(5);

            // Configuration
            GUILayout.Label($"Seed: {_currentSeed.Substring(0, Mathf.Min(8, _currentSeed.Length))}...");
            GUILayout.Label($"Size: {mazeWidth}x{mazeHeight}");
            GUILayout.Label($"Target Torches: {torchCount}");
            GUILayout.Space(5);

            // Stats
            GUILayout.Label($"Generations: {_generationCount}");
            GUILayout.Label($"Maze Time: {_mazeGenTime:F3}s");
            GUILayout.Label($"Torch Time: {_torchPlaceTime:F3}s");
            GUILayout.Label($"Total Time: {(_mazeGenTime + _torchPlaceTime):F3}s");
            GUILayout.Space(5);

            // Controls
            GUILayout.Label("═══ Controls ═══");
            GUILayout.Label("[R] Regenerate (New Seed)");
            GUILayout.Label("[G] Regenerate (Same Seed)");
            GUILayout.Label("[T] Toggle Torches");
            GUILayout.Space(10);

            // Buttons
            if (GUILayout.Button("Generate Maze", GUILayout.Height(25)))
                GenerateTestMaze();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Place Torches"))
                PlaceTorches();
            if (GUILayout.Button("Remove Torches"))
                RemoveTorches();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion

        #region Utilities

        private void Log(string message)
        {
            if (verboseLogging || !Application.isPlaying)
            {
                Debug.Log(message);
            }
        }

        #endregion

        #region Public Accessors

        public bool IsMazeGenerated => _mazeGenerated;
        public bool AreTorchesActive => _torchesActive;
        public string CurrentSeed => _currentSeed;
        public int GenerationCount => _generationCount;
        public float LastMazeGenTime => _mazeGenTime;
        public float LastTorchPlaceTime => _torchPlaceTime;

        #endregion
    }
}
