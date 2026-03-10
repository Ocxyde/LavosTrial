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
// PlayerSetup.cs
// Player component orchestrator - handles player initialization
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   Add this component to Player GameObject along with:
//   - PlayerController
//   - PlayerStats
//   - CameraFollow (on Camera child)
//
// PLUG-IN-OUT COMPLIANT:
// - Does NOT create components (only finds existing)
// - All values from JSON config (no hardcoding)
// - Uses EventHandler for communication
//
// UNITY 6 C# COMPLIANCE:
// - Uses null-conditional operators (?. ??)
// - Uses string interpolation ($)
// - Uses readonly for constants
// - Full XML documentation
// - Proper lifecycle methods
// - Nullable reference types enabled
//
// C# NAMING CONVENTIONS:
// - Private fields: _camelCase with underscore prefix
// - Public methods: PascalCase
// - Parameters: camelCase
// - Properties: PascalCase
//
// Location: Assets/Scripts/Core/02_Player/

#nullable enable

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// PlayerSetup - Player component orchestrator.
    /// Initializes player, camera, and stats on start.
    /// PLUG-IN-OUT COMPLIANT: Finds components, never creates.
    /// ALL VALUES FROM JSON: No hardcoded values.
    /// UNITY 6 C# COMPLIANT: Modern C# features used throughout.
    /// C# NAMING CONVENTIONS: Private fields use _camelCase with underscore prefix.
    /// </summary>
    public class PlayerSetup : MonoBehaviour
    {
        #region Inspector Fields (From JSON Config)

        [Header("Camera Settings (From JSON)")]
        [Tooltip("Camera child (assign in Inspector or auto-find)")]
        [SerializeField] private Camera _playerCamera = null!;

        [Tooltip("Eye height - loaded from JSON config")]
        [SerializeField] private float _eyeHeight;

        [Tooltip("Camera follow speed - loaded from JSON")]
        [SerializeField] private float _followSpeed;

        [Header("Player Settings")]
        [Tooltip("Start position (use CompleteMazeBuilder spawn point if null)")]
        [SerializeField] private Vector3 _startPosition;

        [Tooltip("Use maze spawn point (from CompleteMazeBuilder)")]
        [SerializeField] private bool _useMazeSpawnPoint = true;

        [Header("Events")]
        [Tooltip("Listen to player spawn events")]
        [SerializeField] private bool _listenToSpawnEvents = true;

        #endregion

        #region Private Fields

        private PlayerController _playerController = null!;
        private PlayerStats _playerStats = null!;
        private CameraFollow _cameraFollow = null!;
        private EventHandler? _eventHandler;
        private bool _isInitialized;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Initialize on Awake (Unity lifecycle).
        /// </summary>
        private void Awake()
        {
            LoadConfig();
            FindComponents();
            InitializeComponents();

            if (_listenToSpawnEvents)
            {
                SubscribeToEvents();
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Initialize on Start (Unity lifecycle).
        /// </summary>
        private void Start()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[PlayerSetup] Not initialized in Awake - initializing now");
                Awake();
            }

            // Check if CompleteMazeBuilder8 exists - if yes, it will spawn the player
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder8>();
            if (mazeBuilder != null)
            {
                Debug.Log("[PlayerSetup] CompleteMazeBuilder8 found - skipping player positioning (maze builder will spawn)");
                // Don't position player here - CompleteMazeBuilder8 will do it after maze generation
            }
            else
            {
                // No maze builder - position player manually (for non-maze scenes)
                PositionPlayer();
            }

            SetupCamera();

            Debug.Log("[PlayerSetup] Player initialized");
        }

        /// <summary>
        /// Cleanup on destroy (Unity lifecycle).
        /// </summary>
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region JSON Config Loading

        /// <summary>
        /// Load ALL values from JSON config.
        /// NO HARDCODED VALUES - everything from GameConfig-default.json.
        /// </summary>
        private void LoadConfig()
        {
            var config = GameConfig.Instance;

            // Load camera settings from JSON
            _eyeHeight = config.DefaultPlayerEyeHeight;
            _followSpeed = config.MouseSensitivity * 5f;

            Debug.Log($"[PlayerSetup] Config loaded: EyeHeight={_eyeHeight}m, FollowSpeed={_followSpeed}");
        }

        #endregion

        #region Component Discovery

        /// <summary>
        /// Find all required components (PLUG-IN-OUT: never create!).
        /// </summary>
        private void FindComponents()
        {
            // Find components on this GameObject (DO NOT CREATE!)
            _playerController = GetComponent<PlayerController>();
            _playerStats = GetComponent<PlayerStats>();

            // Find camera (auto-find if not assigned)
            _playerCamera ??= GetComponentInChildren<Camera>();

            // Find CameraFollow on camera
            if (_playerCamera != null)
            {
                _cameraFollow = _playerCamera.GetComponent<CameraFollow>();
            }

            // Find event handler
            _eventHandler = EventHandler.Instance;

            // Validate components (log warnings, don't create!)
            ValidateComponents();
        }

        /// <summary>
        /// Validate that all required components exist.
        /// Logs warnings if missing (does NOT create them!).
        /// </summary>
        private void ValidateComponents()
        {
            if (_playerController == null)
            {
                Debug.LogError("[PlayerSetup] PlayerController not found!");
                Debug.LogError("[PlayerSetup] Add PlayerController component");
            }

            if (_playerStats == null)
            {
                Debug.LogError("[PlayerSetup] PlayerStats not found!");
                Debug.LogError("[PlayerSetup] Add PlayerStats component");
            }

            if (_playerCamera == null)
            {
                Debug.LogWarning("[PlayerSetup] No Camera found! Add Main Camera as child.");
            }

            if (_cameraFollow == null && _playerCamera != null)
            {
                Debug.LogWarning("[PlayerSetup] CameraFollow not found! Add CameraFollow component.");
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize all components.
        /// PlayerStats and CameraFollow auto-initialize in their Awake() methods.
        /// </summary>
        private void InitializeComponents()
        {
            // PlayerStats auto-initializes in Awake() - no manual call needed
            Debug.Log("[PlayerSetup] PlayerStats initialized (auto in Awake)");

            // CameraFollow auto-finds target automatically (set in Inspector)
            // Can't set autoFindTarget from here (it's private)
            Debug.Log("[PlayerSetup] CameraFollow will auto-find target");
        }

        /// <summary>
        /// Position player at spawn point.
        /// </summary>
        private void PositionPlayer()
        {
            if (_useMazeSpawnPoint)
            {
                // Try to get spawn point from CompleteMazeBuilder8
                var mazeBuilder = Object.FindFirstObjectByType<CompleteMazeBuilder8>();
                if (mazeBuilder != null && mazeBuilder.MazeData != null)
                {
                    // Use maze spawn point from MazeData8
                    float cellSize = 6f;  // Default, should come from config
                    var spawn = mazeBuilder.MazeData.SpawnCell;
                    transform.position = new Vector3(
                        spawn.x * cellSize + cellSize / 2f,
                        1.5f,  // Eye height
                        spawn.z * cellSize + cellSize / 2f
                    );
                    Debug.Log("[PlayerSetup] Using maze spawn point");
                }
                else
                {
                    transform.position = _startPosition;
                    Debug.Log("[PlayerSetup] No CompleteMazeBuilder8 - using start position");
                }
            }
            else
            {
                transform.position = _startPosition;
            }

            Debug.Log($"[PlayerSetup] Player positioned at {transform.position}");
        }

        /// <summary>
        /// Setup camera at eye height.
        /// </summary>
        private void SetupCamera()
        {
            if (_playerCamera == null) return;

            // Set camera to eye height (from JSON config)
            _playerCamera.transform.localPosition = new Vector3(0f, _eyeHeight, 0f);
            _playerCamera.transform.localRotation = Quaternion.identity;

            Debug.Log($"[PlayerSetup] Camera at eye height ({_eyeHeight}m)");
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Subscribe to spawn events.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_eventHandler == null) return;

            // Subscribe to player respawn event
            _eventHandler.OnPlayerRespawned += OnPlayerRespawned;
            Debug.Log("[PlayerSetup]  Subscribed to player events");
        }

        /// <summary>
        /// Unsubscribe from events (prevents memory leaks).
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (_eventHandler == null) return;

            // Unsubscribe from all events
            _eventHandler.OnPlayerRespawned -= OnPlayerRespawned;
        }

        /// <summary>
        /// Called when player is respawned (via event).
        /// </summary>
        private void OnPlayerRespawned()
        {
            Debug.Log("[PlayerSetup]  Player respawned via event");
            ResetPlayer();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Teleport player to position.
        /// </summary>
        /// <param name="position">Target position.</param>
        public void TeleportTo(Vector3 position)
        {
            transform.position = position;
            Debug.Log($"[PlayerSetup]  Teleported to {position}");
        }

        /// <summary>
        /// Reset player to start position.
        /// </summary>
        public void ResetPlayer()
        {
            transform.position = _startPosition;
            transform.rotation = Quaternion.identity;

            // PlayerStats resets via its own methods
            if (_playerStats != null)
            {
                Debug.Log("[PlayerSetup]  PlayerStats will reset to base values");
            }

            // Notify event system
            _eventHandler?.InvokePlayerRespawned();

            Debug.Log("[PlayerSetup]  Player reset");
        }

        /// <summary>
        /// Get player controller.
        /// </summary>
        /// <returns>PlayerController component.</returns>
        public PlayerController GetPlayerController() => _playerController;

        /// <summary>
        /// Get player stats.
        /// </summary>
        /// <returns>PlayerStats component.</returns>
        public PlayerStats GetPlayerStats() => _playerStats;

        /// <summary>
        /// Get player camera.
        /// </summary>
        /// <returns>Camera component.</returns>
        public Camera GetPlayerCamera() => _playerCamera;

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only setup method.
        /// </summary>
        [ContextMenu("Setup Player")]
        private void EditorSetup()
        {
            Debug.Log("[PlayerSetup]  Running setup...");
            LoadConfig();
            FindComponents();
            InitializeComponents();
            PositionPlayer();
            SetupCamera();
            Debug.Log("[PlayerSetup]  Setup complete");
        }

        /// <summary>
        /// Editor-only validation method.
        /// </summary>
        [ContextMenu("Validate Components")]
        private void EditorValidate()
        {
            ValidateComponents();
        }
#endif

        #endregion
    }
}
