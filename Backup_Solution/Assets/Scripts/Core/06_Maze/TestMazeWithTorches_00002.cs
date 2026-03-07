// TestMazeWithTorches.cs
// Test script for maze generation with torch placement
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Usage: Attach to a GameObject in a test scene
//        Or run via Editor command menu
//
// Location: Assets/Scripts/Core/06_Maze/

using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Lavos.Core
{
    /// <summary>
    /// TestMazeWithTorches - Test harness for maze + torch system.
    /// 
    /// Features:
    /// - Generates maze with configurable seed
    /// - Places torches via SpatialPlacer
    /// - Displays stats and diagnostics
    /// - Editor and runtime support
    /// </summary>
    public class TestMazeWithTorches : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool autoGenerateOnStart = true;
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private string testSeed = "TEST123";

        [Header("Maze Settings")]
        [SerializeField] private int mazeWidth = 21;
        [SerializeField] private int mazeHeight = 21;

        [Header("Torch Settings")]
        [SerializeField] private bool placeTorches = true;
        [SerializeField] private int torchCount = 15;
        [SerializeField] private float torchProbability = 0.5f;

        [Header("Components")]
        [SerializeField] private MazeIntegration mazeIntegration;
        [SerializeField] private SpatialPlacer spatialPlacer;
        [SerializeField] private MazeGenerator mazeGenerator;
        [SerializeField] private MazeRenderer mazeRenderer;

        [Header("Debug")]
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private Key regenerateKey = Key.R;
        [SerializeField] private Key toggleTorchesKey = Key.T;

        // State
        private bool _mazeGenerated = false;
        private bool _torchesActive = false;
        private float _lastGenerationTime;
        private int _generationCount;

        void Awake()
        {
            // Auto-find components if not assigned
            FindComponents();
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
            // Keyboard shortcuts using New Input System
            // Note: Requires Game View to have focus in Editor
            if (Keyboard.current != null)
            {
                if (Keyboard.current[regenerateKey].wasPressedThisFrame)
                {
                    RegenerateMaze();
                }

                if (Keyboard.current[toggleTorchesKey].wasPressedThisFrame)
                {
                    ToggleTorches();
                }
            }
        }

        #region Component Setup

        private void FindComponents()
        {
            // Auto-find all components on the same GameObject
            if (mazeIntegration == null)
                mazeIntegration = GetComponent<MazeIntegration>();

            if (spatialPlacer == null)
                spatialPlacer = GetComponent<SpatialPlacer>();

            if (mazeGenerator == null)
                mazeGenerator = GetComponent<MazeGenerator>();

            if (mazeRenderer == null)
                mazeRenderer = GetComponent<MazeRenderer>();

            // Auto-find SeedManager and DrawingPool if not in scene
            var seedManager = FindFirstObjectByType<SeedManager>();
            var drawingPool = FindFirstObjectByType<DrawingPool>();

            // Validate required components - but be more lenient for testing
            if (mazeIntegration == null)
            {
                Debug.LogWarning("[TestMazeWithTorches] MazeIntegration not found on this GameObject. Add it or disable auto-generate.");
                // Don't disable - allow manual testing
                return;
            }

            if (spatialPlacer == null)
            {
                Debug.LogWarning("[TestMazeWithTorches] SpatialPlacer not found on this GameObject. Add it for torch placement.");
                // Don't disable - allow maze-only testing
            }
        }

        #endregion

        #region Test Methods

        /// <summary>
        /// Generate test maze with torches.
        /// </summary>
        [ContextMenu("Generate Test Maze")]
        public void GenerateTestMaze()
        {
            Debug.Log("=== Test Maze Generation Started ===");

            // Check if components are available
            if (mazeIntegration == null)
            {
                Debug.LogError("[TestMazeWithTorches] Cannot generate: MazeIntegration is missing!");
                Debug.LogError("[TestMazeWithTorches] Add MazeIntegration component to this GameObject or create a new test scene.");
                return;
            }

            // Configure seed
            if (useRandomSeed)
            {
                testSeed = System.Guid.NewGuid().ToString();
            }

            // Set seed via SeedManager if available
            var seedManager = FindFirstObjectByType<SeedManager>();
            if (seedManager != null)
            {
                // Force seed
                var seedField = seedManager.GetType().GetField("currentSeed",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                if (seedField != null)
                {
                    seedField.SetValue(seedManager, testSeed);
                }
            }

            // Configure maze dimensions
            if (mazeGenerator != null)
            {
                mazeGenerator.width = mazeWidth;
                mazeGenerator.height = mazeHeight;
            }

            // Configure torch settings
            if (spatialPlacer != null)
            {
                // Use reflection to set torch count
                var torchCountField = spatialPlacer.GetType().GetField("torchCount",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                if (torchCountField != null)
                {
                    torchCountField.SetValue(spatialPlacer, torchCount);
                }

                // Enable torch placement using public property
                spatialPlacer.PlaceTorchesEnabled = placeTorches;
            }
            else if (placeTorches)
            {
                Debug.LogWarning("[TestMazeWithTorches] Torch placement enabled but SpatialPlacer not found!");
            }

            // Generate maze
            _lastGenerationTime = Time.time;
            _generationCount++;

            mazeIntegration.GenerateMaze();

            _mazeGenerated = true;

            Debug.Log($"=== Test Maze Generation Complete ===");
            Debug.Log($"Seed: {testSeed}");
            Debug.Log($"Dimensions: {mazeWidth}x{mazeHeight}");
            Debug.Log($"Torches Enabled: {placeTorches && spatialPlacer != null}");
            Debug.Log($"Generation #{_generationCount}");
        }

        /// <summary>
        /// Regenerate maze with new random seed.
        /// </summary>
        [ContextMenu("Regenerate (New Seed)")]
        public void RegenerateMaze()
        {
            useRandomSeed = true;
            GenerateTestMaze();
        }

        /// <summary>
        /// Regenerate maze with same seed.
        /// </summary>
        [ContextMenu("Regenerate (Same Seed)")]
        public void RegenerateSameSeed()
        {
            useRandomSeed = false;
            GenerateTestMaze();
        }

        /// <summary>
        /// Toggle torch placement.
        /// </summary>
        [ContextMenu("Toggle Torches")]
        public void ToggleTorches()
        {
            if (spatialPlacer == null)
            {
                Debug.LogWarning("[TestMazeWithTorches] SpatialPlacer not found!");
                return;
            }

            if (_torchesActive)
            {
                spatialPlacer.RemoveTorches();
                _torchesActive = false;
                Debug.Log("[TestMazeWithTorches] Torches removed");
            }
            else
            {
                spatialPlacer.PlaceTorches();
                _torchesActive = true;
                Debug.Log("[TestMazeWithTorches] Torches placed");
            }
        }

        /// <summary>
        /// Place torches only.
        /// </summary>
        [ContextMenu("Place Torches")]
        public void PlaceTorchesOnly()
        {
            if (spatialPlacer != null)
            {
                spatialPlacer.PlaceTorches();
                _torchesActive = true;
            }
        }

        /// <summary>
        /// Remove torches only.
        /// </summary>
        [ContextMenu("Remove Torches")]
        public void RemoveTorchesOnly()
        {
            if (spatialPlacer != null)
            {
                spatialPlacer.RemoveTorches();
                _torchesActive = false;
            }
        }

        #endregion

        #region Debug UI

        void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 350, 400));
            GUILayout.BeginVertical("box");

            GUILayout.Label("=== Test Maze with Torches ===");
            GUILayout.Space(10);

            // Status
            GUILayout.Label($"Maze Generated: {_mazeGenerated}");
            GUILayout.Label($"Torches Active: {_torchesActive}");
            GUILayout.Label($"Seed: {testSeed}");
            GUILayout.Label($"Dimensions: {mazeWidth}x{mazeHeight}");

            GUILayout.Space(10);

            // Stats
            GUILayout.Label($"Generations: {_generationCount}");
            GUILayout.Label($"Last Gen: {(_lastGenerationTime > 0 ? (Time.time - _lastGenerationTime).ToString("F2") : "N/A")}s ago");

            GUILayout.Space(10);

            // Controls
            GUILayout.Label("--- Controls ---");
            GUILayout.Label("Keyboard (click Game View first):");
            GUILayout.Label($"  [R] Regenerate (New Seed)");
            GUILayout.Label($"  [T] Toggle Torches");

            GUILayout.Space(10);

            // Buttons (work without keyboard focus)
            if (GUILayout.Button("Generate Maze"))
            {
                GenerateTestMaze();
            }

            if (GUILayout.Button("Regenerate (New Seed)"))
            {
                RegenerateMaze();
            }

            if (GUILayout.Button("Regenerate (Same Seed)"))
            {
                RegenerateSameSeed();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Place Torches"))
            {
                PlaceTorchesOnly();
            }
            if (GUILayout.Button("Remove Torches"))
            {
                RemoveTorchesOnly();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion

        #region Public Accessors

        public bool IsMazeGenerated => _mazeGenerated;
        public bool AreTorchesActive => _torchesActive;
        public string CurrentSeed => testSeed;
        public int GenerationCount => _generationCount;

        #endregion
    }
}
