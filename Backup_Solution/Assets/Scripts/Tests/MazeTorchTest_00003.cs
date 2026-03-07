// MazeTorchTest.cs
// ⚠️ DEPRECATED - TO BE DELETED ⚠️
// This test file uses legacy MazeIntegration system.
// Migrate to CompleteMazeBuilder when ready, then delete this file.
//
// Simplified test harness for maze generation with torch placement
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Create empty GameObject
//   2. Add this component
//   3. Press Play - auto-generates maze with torches
//
// CONTROLS:
//   [R] - Regenerate maze (new seed)
//   [G] - Regenerate (same seed)
//   [T] - Toggle torches
//   [Space] - Clear all
//
// Location: Assets/Scripts/Core/06_Maze/

using UnityEngine;
using UnityEngine.InputSystem;

// Suppress obsolete warnings - this file uses legacy system (to be deleted)
#pragma warning disable CS0618

namespace Code.Lavos.Core
{
    /// <summary>
    /// ⚠️ DEPRECATED - TO BE DELETED ⚠️
    /// 
    /// MazeTorchTest - Legacy maze + torch test harness.
    /// Uses deprecated MazeIntegration system.
    /// Replace with CompleteMazeBuilder-based test when ready.
    /// </summary>
    [System.Obsolete("This test uses legacy system. Create new test with CompleteMazeBuilder.")]
    public class MazeTorchTest : MonoBehaviour
    {
        #region Inspector Settings

        [Header("Generation Settings")]
        [Tooltip("Auto-generate on start")]
        [SerializeField] private bool autoGenerateOnStart = true;

        [Tooltip("Use random seed (false = same seed for reproducible tests)")]
        [SerializeField] private bool useRandomSeed = true;

        [Tooltip("Seed for reproducible generation")]
        [SerializeField] private string testSeed = "MazeTest2026";

        [Header("Maze Settings")]
        [SerializeField] private int mazeWidth = 21;
        [SerializeField] private int mazeHeight = 21;

        [Header("Torch Settings")]
        [SerializeField] private bool enableTorches = true;
        [SerializeField] private int torchCount = 15;
        [SerializeField] private float minTorchDistance = 6f;

        [Header("Test Player")]
        [Tooltip("Spawn a test player with camera for maze exploration")]
        [SerializeField] private bool spawnTestPlayer = true;

        [Tooltip("Camera distance from player (eye level view)")]
        [SerializeField] private float cameraDistance = 3.5f;

        [Tooltip("Camera height offset")]
        [SerializeField] private float cameraHeight = 1.7f;

        [Tooltip("Add a ground plane under the maze for testing")]
        [SerializeField] private bool spawnGroundPlane = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private bool verboseLogging = false;

        #endregion

        #region Component References (auto-found)

        private MazeGenerator _mazeGenerator;
        private MazeRenderer _mazeRenderer;
        private TorchPool _torchPool;
        private SpatialPlacer _spatialPlacer;  // Universal placement engine (PLUG-IN)
        private MazeIntegration _mazeIntegration;

        #endregion

        #region State

        private bool _isGenerated = false;
        private bool _torchesActive = false;
        private int _generationCount = 0;
        private string _currentSeed = "";
        private float _lastGenTime = 0f;
        private float _lastTorchTime = 0f;
        private GameObject _testPlayer;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            FindComponents();
            ValidateSetup();
        }

        void Start()
        {
            // Spawn ground plane first (for player to stand on)
            if (spawnGroundPlane)
            {
                SpawnGroundPlane();
            }

            // Generate maze FIRST so player spawns inside it
            if (autoGenerateOnStart)
            {
                GenerateMaze();
            }

            // Spawn player AFTER maze is generated
            if (spawnTestPlayer)
            {
                SpawnTestPlayer();
            }
        }

        /// <summary>
        /// Spawn a ground plane under the maze for testing.
        /// </summary>
        private void SpawnGroundPlane()
        {
            // Create or find ground plane
            var ground = GameObject.Find("GroundPlane");
            if (ground == null)
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "GroundPlane";
                ground.transform.position = new Vector3(0f, 0f, 0f);
                
                // Scale to cover entire maze (21 cells * 6m = 126m, use 200m to be safe)
                ground.transform.localScale = new Vector3(20f, 1f, 20f);

                // Add a simple material
                var renderer = ground.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                }

                // Ensure ground has a collider (for CharacterController ground detection)
                var collider = ground.GetComponent<Collider>();
                if (collider == null)
                {
                    var meshCollider = ground.AddComponent<MeshCollider>();
                    meshCollider.convex = false;
                }

                Log("[MazeTorchTest] Ground plane created at y=0 (200x200m) with Collider");
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
            _mazeGenerator = GetComponent<MazeGenerator>();
            _mazeRenderer = GetComponent<MazeRenderer>();
            _torchPool = GetComponent<TorchPool>();
            _spatialPlacer = GetComponent<SpatialPlacer>();  // Universal placement PLUG-IN
            _mazeIntegration = GetComponent<MazeIntegration>();
        }

        /// <summary>
        /// Spawn a test player with camera for maze exploration.
        /// </summary>
        private void SpawnTestPlayer()
        {
            // Check if player already exists
            var existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                Log("[MazeTorchTest] Player already exists, skipping spawn");
                return;
            }

            Log("[MazeTorchTest] Spawning test player...");

            // Create player GameObject
            _testPlayer = new GameObject("Player");
            _testPlayer.tag = "Player";
            
            // Find a valid spawn position (corridor, not wall)
            Vector3 spawnPos = FindValidSpawnPosition();
            _testPlayer.transform.position = spawnPos;

            Log($"[MazeTorchTest] Spawn position: {spawnPos}");

            // Add CharacterController
            var characterController = _testPlayer.AddComponent<CharacterController>();
            characterController.radius = 0.4f;
            characterController.height = 1.8f;
            characterController.center = new Vector3(0f, 0.9f, 0f);
            characterController.skinWidth = 0.08f;
            characterController.minMoveDistance = 0.001f;
            
            // Add a small sphere collider to ensure ground detection works
            var sphereCollider = _testPlayer.AddComponent<SphereCollider>();
            sphereCollider.radius = 0.3f;
            sphereCollider.center = new Vector3(0f, 0.5f, 0f);
            sphereCollider.isTrigger = true;

            // Add PlayerStats
            _testPlayer.AddComponent<PlayerStats>();

            // Add PlayerController
            var playerController = _testPlayer.AddComponent<PlayerController>();
            
            // Configure PlayerController's camera reference
            var pcField = playerController.GetType().GetField("playerCamera",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (pcField != null)
            {
                pcField.SetValue(playerController, Camera.main);
            }

            // Create camera pivot
            var cameraPivot = new GameObject("CameraPivot");
            cameraPivot.transform.SetParent(_testPlayer.transform);
            cameraPivot.transform.localPosition = Vector3.zero;

            // Add CameraFollow
            var cameraFollow = cameraPivot.AddComponent<CameraFollow>();
            cameraFollow.SetTarget(_testPlayer.transform);

            // Set camera distance to middle of eye view (3.5f = comfortable third-person)
            cameraFollow.SetDistance(cameraDistance);

            Log($"[MazeTorchTest] Player spawned at {spawnPos} (VALID corridor position)");
            Log($"[MazeTorchTest] Camera distance set to {cameraDistance}m (eye level view)");
            Log("[MazeTorchTest] Press WASD to move, Mouse to look, Shift to sprint, Space to jump");
            Log("[MazeTorchTest] Check Console for [PlayerController] messages");
        }

        /// <summary>
        /// Find a valid spawn position in the maze (corridor, not inside wall).
        /// </summary>
        private Vector3 FindValidSpawnPosition()
        {
            float cellSize = 6f;
            int middleCell = mazeWidth / 2;
            
            // For now, just spawn at center cell (10,10) for 21x21 maze
            // This is guaranteed to be a corridor in a properly generated maze
            int checkX = middleCell;
            int checkY = middleCell;
            
            // Center of cell (not wall edge)
            float spawnX = (checkX + 0.5f) * cellSize;
            float spawnZ = (checkY + 0.5f) * cellSize;
            float spawnY = 1f; // On ground (CharacterController height is 1.8f, center at 0.9f)
            
            Log($"[MazeTorchTest] Spawning at cell ({checkX},{checkY}) = ({spawnX}, {spawnY}, {spawnZ})");
            Log($"[MazeTorchTest] Player feet at y={spawnY - 0.9f:F2}, eyes at y={spawnY:F2}");
            
            return new Vector3(spawnX, spawnY, spawnZ);
        }

        /// <summary>
        /// Cleanup test player on destroy.
        /// </summary>
        void OnDestroy()
        {
            if (_testPlayer != null)
            {
                Destroy(_testPlayer);
            }
        }

        /// <summary>
        /// Validate all required components are present.
        /// </summary>
        private void ValidateSetup()
        {
            bool hasErrors = false;

            if (_mazeGenerator == null)
            {
                Debug.LogError("[MazeTorchTest] MISSING: MazeGenerator component!");
                hasErrors = true;
            }

            if (_mazeRenderer == null)
            {
                Debug.LogError("[MazeTorchTest] MISSING: MazeRenderer component!");
                hasErrors = true;
            }

            if (_torchPool == null)
            {
                Debug.LogError("[MazeTorchTest] MISSING: TorchPool component!");
                hasErrors = true;
            }

            if (_spatialPlacer == null)
            {
                Debug.LogError("[MazeTorchTest] MISSING: SpatialPlacer component (Universal PLUG-IN)!");
                hasErrors = true;
            }

            if (_mazeIntegration == null)
            {
                Debug.LogError("[MazeTorchTest] MISSING: MazeIntegration component!");
                hasErrors = true;
            }

            if (hasErrors)
            {
                Debug.LogError("[MazeTorchTest] All components must be on the SAME GameObject!");
                Debug.LogError("[MazeTorchTest] Use: Tools → Maze Test → Quick Setup (Current Scene)");
                enabled = false;
                return;
            }

            Log("[MazeTorchTest] All components found. Ready to test.");
        }

        #endregion

        #region Main Test Methods

        /// <summary>
        /// Generate complete maze with torches.
        /// </summary>
        [ContextMenu("Generate Maze")]
        public void GenerateMaze()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            Log("=== Maze Generation Started ===");

            // Configure seed
            ConfigureSeed();

            // Configure maze dimensions
            _mazeGenerator.width = mazeWidth;
            _mazeGenerator.height = mazeHeight;

            // Configure torch plug-in settings
            ConfigureTorchSettings();

            // Generate maze (maze engine does NOT place torches)
            _mazeIntegration.GenerateMaze();
            _isGenerated = true;
            _generationCount++;

            _lastGenTime = stopwatch.ElapsedMilliseconds / 1000f;

            // PLUG-IN: Place torches via SpatialPlacer (universal placement engine)
            if (enableTorches && _spatialPlacer != null)
            {
                stopwatch.Restart();
                _spatialPlacer.PlaceTorches();  // CALL PLUG-IN
                _lastTorchTime = stopwatch.ElapsedMilliseconds / 1000f;
                _torchesActive = true;
            }

            float totalTime = _lastGenTime + _lastTorchTime;

            Log($"=== Generation Complete ===");
            Log($"Seed: {_currentSeed.Substring(0, 8)}...");
            Log($"Size: {mazeWidth}x{mazeHeight}");
            Log($"Torches: {(_torchesActive ? "ON (via SpatialPlacer PLUG-IN)" : "OFF")}");
            Log($"Timing: Maze={_lastGenTime:F3}s, Torches={_lastTorchTime:F3}s, Total={totalTime:F3}s");
            Log($"Generation #{_generationCount}");
        }

        /// <summary>
        /// Regenerate with new random seed.
        /// </summary>
        [ContextMenu("Regenerate (New Seed)")]
        public void RegenerateMaze()
        {
            useRandomSeed = true;
            GenerateMaze();
        }

        /// <summary>
        /// Regenerate with same seed.
        /// </summary>
        [ContextMenu("Regenerate (Same Seed)")]
        public void RegenerateSameSeed()
        {
            useRandomSeed = false;
            GenerateMaze();
        }

        /// <summary>
        /// Toggle torch placement.
        /// </summary>
        [ContextMenu("Toggle Torches")]
        public void ToggleTorches()
        {
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
        /// Place torches on existing maze (via SpatialPlacer PLUG-IN).
        /// </summary>
        [ContextMenu("Place Torches")]
        public void PlaceTorches()
        {
            if (!_isGenerated)
            {
                Debug.LogWarning("[MazeTorchTest] Generate maze first!");
                return;
            }

            if (_spatialPlacer == null)
            {
                Debug.LogError("[MazeTorchTest] SpatialPlacer not found!");
                return;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _spatialPlacer.PlaceTorches();  // CALL PLUG-IN
            _lastTorchTime = stopwatch.ElapsedMilliseconds / 1000f;
            _torchesActive = true;

            Log($"[MazeTorchTest] Torches placed in {_lastTorchTime:F3}s (via SpatialPlacer)");
        }

        /// <summary>
        /// Remove all torches (via TorchPool).
        /// </summary>
        [ContextMenu("Remove Torches")]
        public void RemoveTorches()
        {
            if (_torchPool == null)
            {
                Debug.LogError("[MazeTorchTest] TorchPool not found!");
                return;
            }

            _torchPool.ReleaseAll();
            _torchesActive = false;

            Log("[MazeTorchTest] Torches removed");
        }

        /// <summary>
        /// Clear everything.
        /// </summary>
        [ContextMenu("Clear All")]
        public void ClearAll()
        {
            RemoveTorches();
            _isGenerated = false;
            Log("[MazeTorchTest] All cleared");
        }

        #endregion

        #region Configuration

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
                seedField?.SetValue(seedManager, _currentSeed);
            }

            testSeed = _currentSeed;
        }

        private void ConfigureTorchSettings()
        {
            if (_spatialPlacer == null) return;

            // Set torch count via reflection
            var torchCountField = _spatialPlacer.GetType().GetField("torchCount",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            torchCountField?.SetValue(_spatialPlacer, torchCount);

            // Set min distance via reflection
            var minDistField = _spatialPlacer.GetType().GetField("minDistanceBetweenTorches",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            minDistField?.SetValue(_spatialPlacer, minTorchDistance);

            // Enable via public property
            _spatialPlacer.PlaceTorchesEnabled = enableTorches;
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            if (Keyboard.current == null) return;

            var keyboard = Keyboard.current;

            // Regenerate with new seed
            if (keyboard[Key.R].wasPressedThisFrame)
            {
                RegenerateMaze();
            }

            // Regenerate with same seed
            if (keyboard[Key.G].wasPressedThisFrame)
            {
                RegenerateSameSeed();
            }

            // Toggle torches
            if (keyboard[Key.T].wasPressedThisFrame)
            {
                ToggleTorches();
            }

            // Clear all
            if (keyboard[Key.Space].wasPressedThisFrame)
            {
                ClearAll();
            }

            // Camera zoom with mouse wheel
            if (spawnTestPlayer && _testPlayer != null)
            {
                var cameraPivot = _testPlayer.transform.Find("CameraPivot");
                if (cameraPivot != null)
                {
                    var cameraFollow = cameraPivot.GetComponent<CameraFollow>();
                    if (cameraFollow != null)
                    {
                        // Use New Input System for mouse scroll
                        var mouse = UnityEngine.InputSystem.Mouse.current;
                        if (mouse != null)
                        {
                            float scroll = mouse.scroll.ReadValue().y;
                            if (scroll != 0)
                            {
                                cameraFollow.Zoom(scroll * 0.5f);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Debug UI

        void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 400, 550));
            GUILayout.BeginVertical("box");

            GUILayout.Label("═══ Maze + Torches Test ═══", GUILayout.Height(25));
            GUILayout.Space(5);

            // Status
            GUILayout.Label($"Maze: {(_isGenerated ? "✅ Generated" : "⏳ Not Generated")}");
            GUILayout.Label($"Torches: {(_torchesActive ? "✅ Active" : "⏸ Inactive")}");
            GUILayout.Label($"Player: {(_testPlayer != null ? "✅ Spawned" : "⏸ Not Spawned")}");
            GUILayout.Space(5);

            // Configuration
            GUILayout.Label($"Seed: {_currentSeed.Substring(0, Mathf.Min(8, _currentSeed.Length))}...");
            GUILayout.Label($"Size: {mazeWidth}x{mazeHeight}");
            GUILayout.Label($"Target Torches: {torchCount}");
            GUILayout.Space(5);

            // Camera settings
            if (_testPlayer != null)
            {
                var cameraPivot = _testPlayer.transform.Find("CameraPivot");
                if (cameraPivot != null)
                {
                    var cameraFollow = cameraPivot.GetComponent<CameraFollow>();
                    if (cameraFollow != null)
                    {
                        GUILayout.Label($"Camera Distance: {cameraDistance:F1}m");
                        GUILayout.Label($"Camera Height: {cameraHeight:F1}m");
                    }
                }
            }
            GUILayout.Space(5);

            // Stats
            GUILayout.Label($"Generations: {_generationCount}");
            GUILayout.Label($"Maze Time: {_lastGenTime:F3}s");
            GUILayout.Label($"Torch Time: {_lastTorchTime:F3}s");
            GUILayout.Label($"Total: {(_lastGenTime + _lastTorchTime):F3}s");
            GUILayout.Space(5);

            // Controls
            GUILayout.Label("═══ Controls ═══");
            GUILayout.Label("[R] Regenerate (New Seed)");
            GUILayout.Label("[G] Regenerate (Same Seed)");
            GUILayout.Label("[T] Toggle Torches");
            GUILayout.Label("[Space] Clear All");
            GUILayout.Label("[Mouse Wheel] Camera Zoom");
            GUILayout.Label("[WASD] Move | [Shift] Sprint | [Space] Jump");
            GUILayout.Label("[Mouse] Look Around");
            GUILayout.Space(10);

            // Buttons
            if (GUILayout.Button("Generate Maze", GUILayout.Height(25)))
                GenerateMaze();

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
            if (verboseLogging)
            {
                Debug.Log(message);
            }
        }

        #endregion

        #region Public Accessors

        public bool IsGenerated => _isGenerated;
        public bool AreTorchesActive => _torchesActive;
        public string CurrentSeed => _currentSeed;
        public int GenerationCount => _generationCount;
        public float LastGenTime => _lastGenTime;
        public float LastTorchTime => _lastTorchTime;

        #endregion
    }
}
