// FpsMazeTest.cs
// FPS Maze Test with wide corridors and wall-mounted torches
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Create empty GameObject "MazeTest"
//   2. Add this component
//   3. Press Play - auto-generates wide maze with torches
//
// CONTROLS:
//   [R] - Regenerate maze (new seed)
//   [G] - Regenerate (same seed)
//   [T] - Toggle torches
//   [Space] - Clear all
//   [WASD] - Move | [Shift] - Sprint | [Space] - Jump
//   [Mouse] - Look around (FPS view)
//
// Location: Assets/Scripts/Tests/

using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;  // For Path, Directory, File

namespace Code.Lavos.Core
{
    /// <summary>
    /// FpsMazeTest - FPS maze test with wide corridors and wall-mounted torches.
    /// Camera is at eye level with head bob while walking.
    /// </summary>
    public class FpsMazeTest : MonoBehaviour
    {
        #region Inspector Settings

        [Header("Generation Settings")]
        [Tooltip("Auto-generate on start")]
        [SerializeField] private bool autoGenerateOnStart = true;

        [Tooltip("Use random seed (false = same seed for reproducible tests)")]
        [SerializeField] private bool useRandomSeed = true;

        [Tooltip("Seed for reproducible generation")]
        [SerializeField] private string testSeed = "FpsMaze2026";

        [Header("Maze Settings - Wide Corridors")]
        [Tooltip("Maze width (odd number for proper corridors)")]
        [SerializeField] private int mazeWidth = 31;

        [Tooltip("Maze height (odd number for proper corridors)")]
        [SerializeField] private int mazeHeight = 31;

        [Tooltip("Cell size for maze renderer")]
        [SerializeField] private float cellSize = 6f;

        [Header("Torch Settings - More Torches on Walls")]
        [Tooltip("Enable torch placement")]
        [SerializeField] private bool enableTorches = true;

        [Tooltip("Number of torches (increased for wide corridors)")]
        [SerializeField] private int torchCount = 40;

        [Tooltip("Minimum distance between torches (reduced for better placement)")]
        [SerializeField] private float minTorchDistance = 3f;

        [Tooltip("Torch height ratio on wall (0.55 = 55% up the wall)")]
        [SerializeField] private float torchHeightRatio = 0.6f;

        [Tooltip("Torch inset from wall edge (0.35 = 35% into cell)")]
        [SerializeField] private float torchInset = 0.35f;

        [Header("FPS Player Settings")]
        [Tooltip("Spawn test player for FPS exploration")]
        [SerializeField] private bool spawnTestPlayer = true;

        [Tooltip("Player eye height (FPS view)")]
        [SerializeField] private float eyeHeight = 1.7f;

        // Note: mouseSensitivity handled by PlayerController
        [Tooltip("Enable head bob while walking")]
        [SerializeField] private bool enableHeadBob = true;

        [Tooltip("Head bob frequency (walking)")]
        [SerializeField] private float headBobFreqWalk = 8f;

        [Tooltip("Head bob frequency (sprinting)")]
        [SerializeField] private float headBobFreqSprint = 13f;

        [Tooltip("Head bob amplitude (vertical)")]
        [SerializeField] private float headBobAmpY = 0.05f;

        [Tooltip("Head bob amplitude (horizontal)")]
        [SerializeField] private float headBobAmpX = 0.025f;

        [Header("Ground Plane")]
        [Tooltip("Add ground plane under maze for testing")]
        [SerializeField] private bool spawnGroundPlane = true;

        [Tooltip("Ground plane size")]
        [SerializeField] private float groundSize = 200f;

        [Header("Debug")]
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private bool verboseLogging = false;

        #endregion

        #region Component References

        private MazeGenerator _mazeGenerator;
        private MazeRenderer _mazeRenderer;
        private TorchPool _torchPool;
        private SpatialPlacer _spatialPlacer;
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
        private Camera _fpsCamera;
        private float _bobTimer;
        private Vector3 _bobOffset;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            FindComponents();
            ValidateSetup();
        }

        void Start()
        {
            // Delete old binary files to ensure fresh torch positions
            DeleteOldBinaryFiles();
            
            // Spawn ground plane first (creates ground + ceiling)
            if (spawnGroundPlane)
            {
                SpawnGroundPlane();
            }

            // Generate maze FIRST
            if (autoGenerateOnStart)
            {
                GenerateMaze();
            }

            // Spawn FPS player AFTER maze is generated
            if (spawnTestPlayer)
            {
                SpawnFpsPlayer();
            }

            // Lock cursor for FPS
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        /// <summary>
        /// Delete old binary files if they exist to ensure fresh torch positions each run.
        /// </summary>
        private void DeleteOldBinaryFiles()
        {
            string binaryPath = Path.Combine(Application.dataPath, "StreamingWorkFlow/MazeData");
            
            if (!Directory.Exists(binaryPath))
            {
                return;  // Directory doesn't exist, nothing to delete
            }
            
            string[] files = Directory.GetFiles(binaryPath, "*.bytes");
            
            if (files.Length == 0)
            {
                return;  // No binary files to delete
            }
            
            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                        Debug.Log($"[FpsMazeTest] ✅ Deleted old binary: {Path.GetFileName(file)}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[FpsMazeTest] ⚠️ Could not delete {file}: {e.Message}");
                    }
                }
            }
            
            Debug.Log($"[FpsMazeTest] 🧹 Cleaned up {files.Length} old binary file(s)");
        }

        void Update()
        {
            HandleInput();
            UpdateHeadBob();
        }

        void OnDestroy()
        {
            if (_testPlayer != null)
            {
                Destroy(_testPlayer);
            }
        }

        #endregion

        #region Component Setup

        private void FindComponents()
        {
            _mazeGenerator = GetComponent<MazeGenerator>();
            _mazeRenderer = GetComponent<MazeRenderer>();
            _torchPool = GetComponent<TorchPool>();
            _spatialPlacer = GetComponent<SpatialPlacer>();
            _mazeIntegration = GetComponent<MazeIntegration>();
        }

        private void SpawnGroundPlane()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  SPAWNING GROUND & CEILING");
            Debug.Log("═══════════════════════════════════════════");
            
            // AGGRESSIVE CLEANUP - Find and delete ALL ground/ceiling objects
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int deletedCount = 0;
            foreach (var obj in allObjects)
            {
                string objName = obj.name.ToLower();
                if (objName.Contains("ground") || objName.Contains("ceiling") || 
                    objName.Contains("plane") || objName.Contains("quad"))
                {
                    // Destroy material first (clear cache)
                    var renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null && renderer.sharedMaterial != null)
                    {
                        Debug.Log($"[FpsMazeTest] 🗑️ Destroying material: {renderer.sharedMaterial.name}");
                        DestroyImmediate(renderer.sharedMaterial);
                    }
                    
                    Debug.Log($"[FpsMazeTest] 🗑️ Deleting: {obj.name}");
                    DestroyImmediate(obj);
                    deletedCount++;
                }
            }
            
            if (deletedCount > 0)
            {
                Debug.Log($"[FpsMazeTest] 🧹 Cleaned up {deletedCount} old ground/ceiling objects");
            }
            
            // FORCE UNITY TO CLEAR MATERIAL CACHE
            Debug.Log("[FpsMazeTest] 🧽 Clearing Unity material cache...");
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            
            // CREATE FRESH GROUND
            Debug.Log("[FpsMazeTest] Creating NEW ground cube...");
            var ground = GroundPlaneGenerator.CreateGroundCube(groundSize, 32);
            ground.layer = LayerMask.NameToLayer("Default");
            Debug.Log($"[FpsMazeTest] ✅ Ground: {ground.name}");
            Debug.Log($"[FpsMazeTest] ✅ Position: {ground.transform.position}");
            Debug.Log($"[FpsMazeTest] ✅ Scale: {ground.transform.localScale}");
            Debug.Log($"[FpsMazeTest] ✅ Layer: {LayerMask.LayerToName(ground.layer)}");
            
            // CREATE FRESH CEILING
            Debug.Log("[FpsMazeTest] Creating NEW ceiling cube...");
            var ceiling = CeilingGenerator.CreateCeilingCube(groundSize, 32);
            ceiling.layer = LayerMask.NameToLayer("Default");
            Debug.Log($"[FpsMazeTest] ✅ Ceiling: {ceiling.name}");
            Debug.Log($"[FpsMazeTest] ✅ Position: {ceiling.transform.position}");
            Debug.Log($"[FpsMazeTest] ✅ Scale: {ceiling.transform.localScale}");
            Debug.Log($"[FpsMazeTest] ✅ Layer: {LayerMask.LayerToName(ceiling.layer)}");
            
            Debug.Log("═══════════════════════════════════════════");
        }

        private void SpawnFpsPlayer()
        {
            var existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                Log("[FpsMazeTest] Player already exists, skipping spawn");
                _testPlayer = existingPlayer;
                _fpsCamera = Camera.main;
                return;
            }

            Log("[FpsMazeTest] Spawning FPS player...");

            _testPlayer = new GameObject("Player");
            _testPlayer.tag = "Player";

            Vector3 spawnPos = FindValidSpawnPosition();
            _testPlayer.transform.position = spawnPos;

            // CharacterController
            var controller = _testPlayer.AddComponent<CharacterController>();
            controller.radius = 0.4f;
            controller.height = 1.8f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.skinWidth = 0.08f;

            // PlayerStats
            _testPlayer.AddComponent<PlayerStats>();

            // PlayerController (FPS configuration)
            var playerController = _testPlayer.AddComponent<PlayerController>();

            // Setup FPS camera as child of player (at eye height)
            SetupFpsCamera();

            Log($"[FpsMazeTest] FPS player spawned at {spawnPos}");
            Log("[FpsMazeTest] WASD = Move | Shift = Sprint | Space = Jump | Mouse = Look");
        }

        private void SetupFpsCamera()
        {
            // Disable ALL existing cameras first to prevent conflicts
            Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var cam in allCameras)
            {
                if (cam.gameObject.name != "FPSCamera")
                {
                    cam.enabled = false;
                    Debug.Log($"[FpsMazeTest] Disabled camera: {cam.gameObject.name}");
                }
            }

            // Create camera directly on player for FPS view
            var cameraGO = new GameObject("FPSCamera");
            cameraGO.transform.SetParent(_testPlayer.transform);
            cameraGO.transform.localPosition = new Vector3(0f, eyeHeight, 0f);
            cameraGO.transform.localRotation = Quaternion.identity;

            _fpsCamera = cameraGO.AddComponent<Camera>();
            _fpsCamera.tag = "MainCamera";

            // Configure camera for FPS
            _fpsCamera.fieldOfView = 75f;
            _fpsCamera.nearClipPlane = 0.1f;
            _fpsCamera.farClipPlane = 500f;
            _fpsCamera.enabled = true;

            Log($"[FpsMazeTest] FPS camera positioned at eye height ({eyeHeight}m)");
            Log($"[FpsMazeTest] FPS camera enabled: {_fpsCamera.enabled}");
        }

        private void UpdateHeadBob()
        {
            if (!enableHeadBob || _fpsCamera == null || _testPlayer == null) return;

            // Get player movement
            var playerController = _testPlayer.GetComponent<PlayerController>();
            if (playerController == null) return;

            // Simple head bob based on movement (New Input System)
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;

            bool isMoving = keyboard[UnityEngine.InputSystem.Key.W].isPressed ||
                           keyboard[UnityEngine.InputSystem.Key.A].isPressed ||
                           keyboard[UnityEngine.InputSystem.Key.S].isPressed ||
                           keyboard[UnityEngine.InputSystem.Key.D].isPressed;
            bool isSprinting = keyboard[UnityEngine.InputSystem.Key.LeftShift].isPressed;

            if (isMoving)
            {
                float freq = isSprinting ? headBobFreqSprint : headBobFreqWalk;
                _bobTimer += Time.deltaTime * freq;
                _bobOffset.y = Mathf.Sin(_bobTimer * 2 * Mathf.PI) * headBobAmpY;
                _bobOffset.x = Mathf.Cos(_bobTimer * 2 * Mathf.PI) * headBobAmpX;
            }
            else
            {
                _bobTimer = 0f;
                _bobOffset = Vector3.Lerp(_bobOffset, Vector3.zero, Time.deltaTime * 10f);
            }

            // Apply bob to camera
            var targetPos = new Vector3(0f, eyeHeight, 0f) + _bobOffset;
            _fpsCamera.transform.localPosition = Vector3.Lerp(
                _fpsCamera.transform.localPosition,
                targetPos,
                Time.deltaTime * 15f
            );
        }

        private void ValidateSetup()
        {
            bool hasErrors = false;

            if (_mazeGenerator == null)
            {
                Debug.LogError("[FpsMazeTest] MISSING: MazeGenerator component!");
                hasErrors = true;
            }

            if (_mazeRenderer == null)
            {
                Debug.LogError("[FpsMazeTest] MISSING: MazeRenderer component!");
                hasErrors = true;
            }

            if (_torchPool == null)
            {
                Debug.LogError("[FpsMazeTest] MISSING: TorchPool component!");
                hasErrors = true;
            }

            if (_spatialPlacer == null)
            {
                Debug.LogError("[FpsMazeTest] MISSING: SpatialPlacer component!");
                hasErrors = true;
            }

            if (_mazeIntegration == null)
            {
                Debug.LogError("[FpsMazeTest] MISSING: MazeIntegration component!");
                hasErrors = true;
            }

            if (hasErrors)
            {
                Debug.LogError("[FpsMazeTest] All components must be on the SAME GameObject!");
                enabled = false;
                return;
            }

            Log("[FpsMazeTest] All components found. Ready to test.");
        }

        #endregion

        #region Main Test Methods

        [ContextMenu("Generate Maze")]
        public void GenerateMaze()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            Log("=== Maze Generation Started ===");

            // Clear existing torches and lights first
            if (_isGenerated)
            {
                RemoveTorches();
                LightEngine.Instance?.UnregisterAllLights();
            }

            ConfigureSeed();

            // Configure maze dimensions
            _mazeGenerator.width = mazeWidth;
            _mazeGenerator.height = mazeHeight;

            // Configure renderer cell size
            if (_mazeRenderer != null)
            {
                var cellSizeField = _mazeRenderer.GetType().GetField("cellSize",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                cellSizeField?.SetValue(_mazeRenderer, cellSize);
            }

            // Configure torch settings for wide corridors
            ConfigureTorchSettings();

            // Generate maze
            _mazeIntegration.GenerateMaze();
            _isGenerated = true;
            _generationCount++;

            _lastGenTime = stopwatch.ElapsedMilliseconds / 1000f;

            // Torches are already placed by MazeIntegration.GenerateMaze() via SpatialPlacer.PlaceAllObjects()
            // Just mark as active
            _torchesActive = enableTorches;

            float totalTime = _lastGenTime + _lastTorchTime;

            Log($"=== Generation Complete ===");
            Log($"Seed: {_currentSeed.Substring(0, 8)}...");
            Log($"Size: {mazeWidth}x{mazeHeight}");
            Log($"Cell Size: {cellSize}m");
            Log($"Torches: {(_torchesActive ? $"✅ {torchCount} placed" : "❌ OFF")}");
            Log($"Timing: Maze={_lastGenTime:F3}s, Torches={_lastTorchTime:F3}s, Total={totalTime:F3}s");
        }

        [ContextMenu("Regenerate (New Seed)")]
        public void RegenerateMaze()
        {
            useRandomSeed = true;
            GenerateMaze();
        }

        [ContextMenu("Regenerate (Same Seed)")]
        public void RegenerateSameSeed()
        {
            useRandomSeed = false;
            GenerateMaze();
        }

        [ContextMenu("Toggle Torches")]
        public void ToggleTorches()
        {
            if (_torchesActive)
                RemoveTorches();
            else
                PlaceTorches();
        }

        [ContextMenu("Place Torches")]
        public void PlaceTorches()
        {
            if (!_isGenerated)
            {
                Debug.LogWarning("[FpsMazeTest] Generate maze first!");
                return;
            }

            if (_spatialPlacer == null)
            {
                Debug.LogError("[FpsMazeTest] SpatialPlacer not found!");
                return;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _spatialPlacer.PlaceTorches();
            _lastTorchTime = stopwatch.ElapsedMilliseconds / 1000f;
            _torchesActive = true;

            Log($"[FpsMazeTest] {torchCount} torches placed on walls in {_lastTorchTime:F3}s");
        }

        [ContextMenu("Remove Torches")]
        public void RemoveTorches()
        {
            if (_torchPool == null)
            {
                Debug.LogError("[FpsMazeTest] TorchPool not found!");
                return;
            }

            _torchPool.ReleaseAll();
            _torchesActive = false;

            Log("[FpsMazeTest] Torches removed");
        }

        [ContextMenu("Clear All")]
        public void ClearAll()
        {
            RemoveTorches();
            _isGenerated = false;
            Log("[FpsMazeTest] All cleared");
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

            var seedManager = FindFirstObjectByType<SeedManager>();
            if (seedManager != null)
            {
                var seedField = seedManager.GetType().GetField("currentSeed",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                seedField?.SetValue(seedManager, _currentSeed);
            }

            testSeed = _currentSeed;
        }

        private void ConfigureTorchSettings()
        {
            if (_spatialPlacer == null) return;

            var torchCountField = _spatialPlacer.GetType().GetField("torchCount",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            torchCountField?.SetValue(_spatialPlacer, torchCount);

            var minDistField = _spatialPlacer.GetType().GetField("minDistanceBetweenTorches",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            minDistField?.SetValue(_spatialPlacer, minTorchDistance);

            var heightRatioField = _spatialPlacer.GetType().GetField("torchHeightRatio",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            heightRatioField?.SetValue(_spatialPlacer, torchHeightRatio);

            var insetField = _spatialPlacer.GetType().GetField("torchInset",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            insetField?.SetValue(_spatialPlacer, torchInset);

            _spatialPlacer.PlaceTorchesEnabled = enableTorches;
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            if (Keyboard.current == null) return;

            var keyboard = Keyboard.current;

            if (keyboard[Key.R].wasPressedThisFrame)
                RegenerateMaze();

            if (keyboard[Key.G].wasPressedThisFrame)
                RegenerateSameSeed();

            if (keyboard[Key.T].wasPressedThisFrame)
                ToggleTorches();

            if (keyboard[Key.Space].wasPressedThisFrame)
                ClearAll();
        }

        #endregion

        #region Utilities

        private Vector3 FindValidSpawnPosition()
        {
            float size = cellSize;
            int middleCell = mazeWidth / 2;

            int checkX = middleCell;
            int checkY = middleCell;

            float spawnX = (checkX + 0.5f) * size;
            float spawnZ = (checkY + 0.5f) * size;
            float spawnY = 1f;

            return new Vector3(spawnX, spawnY, spawnZ);
        }

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

        #region Debug UI

        void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 420, 500));
            GUILayout.BeginVertical("box");

            GUILayout.Label("═══ FPS Maze Test ═══", GUILayout.Height(25));
            GUILayout.Space(5);

            GUILayout.Label($"Maze: {(_isGenerated ? "✅ Generated" : "⏳ Not Generated")}");
            GUILayout.Label($"Torches: {(_torchesActive ? $"✅ {torchCount} Active" : "⏸ Inactive")}");
            GUILayout.Label($"Player: {(_testPlayer != null ? "✅ FPS Spawned" : "⏸ Not Spawned")}");
            GUILayout.Space(5);

            GUILayout.Label($"Seed: {_currentSeed.Substring(0, Mathf.Min(8, _currentSeed.Length))}...");
            GUILayout.Label($"Size: {mazeWidth}x{mazeHeight}");
            GUILayout.Label($"Cell Size: {cellSize}m");
            GUILayout.Label($"Target Torches: {torchCount}");
            GUILayout.Space(5);

            GUILayout.Label($"Generations: {_generationCount}");
            GUILayout.Label($"Maze Time: {_lastGenTime:F3}s");
            GUILayout.Label($"Torch Time: {_lastTorchTime:F3}s");
            GUILayout.Label($"Total: {(_lastGenTime + _lastTorchTime):F3}s");
            GUILayout.Space(5);

            GUILayout.Label("═══ Controls ═══");
            GUILayout.Label("[R] Regenerate (New Seed)");
            GUILayout.Label("[G] Regenerate (Same Seed)");
            GUILayout.Label("[T] Toggle Torches");
            GUILayout.Label("[Space] Clear All");
            GUILayout.Label("[WASD] Move | [Shift] Sprint | [Space] Jump");
            GUILayout.Label("[Mouse] Look (FPS View)");
            GUILayout.Space(10);

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
    }
}
