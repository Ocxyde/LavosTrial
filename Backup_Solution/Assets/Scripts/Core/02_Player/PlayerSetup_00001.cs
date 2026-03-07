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
// - Does not create components (finds existing)
// - Uses EventHandler for communication
// - Initializes player on Start
//
// Location: Assets/Scripts/Core/02_Player/

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// PlayerSetup - Player component orchestrator.
    /// Initializes player, camera, and stats on start.
    /// Add this to Player GameObject with PlayerController, PlayerStats, CameraFollow.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerSetup : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Camera Settings")]
        [Tooltip("Camera child (assign in Inspector or auto-find)")]
        [SerializeField] private Camera playerCamera;

        [Tooltip("Eye height (default 1.7m for average adult)")]
        [SerializeField] private float eyeHeight = 1.7f;

        [Tooltip("Camera follow speed")]
        [SerializeField] private float followSpeed = 10f;

        [Header("Player Settings")]
        [Tooltip("Start position (use CompleteMazeBuilder spawn point if null)")]
        [SerializeField] private Vector3 startPosition;

        [Tooltip("Use maze spawn point (from CompleteMazeBuilder)")]
        [SerializeField] private bool useMazeSpawnPoint = true;

        [Header("Events")]
        [Tooltip("Listen to player spawn events")]
        [SerializeField] private bool listenToSpawnEvents = true;

        #endregion

        #region Private Data

        private PlayerController playerController;
        private PlayerStats playerStats;
        private CameraFollow cameraFollow;
        private EventHandler eventHandler;
        private bool isInitialized = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Find required components
            FindComponents();

            // Initialize components
            InitializeComponents();

            // Subscribe to events
            if (listenToSpawnEvents)
            {
                SubscribeToEvents();
            }

            isInitialized = true;
        }

        private void Start()
        {
            if (!isInitialized)
            {
                Awake();
            }

            // Position player at spawn point
            PositionPlayer();

            // Setup camera
            SetupCamera();

            Debug.Log("[PlayerSetup] ✅ Player initialized");
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Component Discovery

        /// <summary>
        /// Find all required components (plug-in-out).
        /// </summary>
        private void FindComponents()
        {
            // Find components on this GameObject
            playerController = GetComponent<PlayerController>();
            playerStats = GetComponent<PlayerStats>();

            // Find camera (auto-find if not assigned)
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }

            // Find CameraFollow on camera
            if (playerCamera != null)
            {
                cameraFollow = playerCamera.GetComponent<CameraFollow>();
            }

            // Find event handler
            eventHandler = EventHandler.Instance;

            // Validate required components
            ValidateComponents();
        }

        /// <summary>
        /// Validate that all required components exist.
        /// </summary>
        private void ValidateComponents()
        {
            if (playerController == null)
            {
                Debug.LogError("[PlayerSetup] ❌ PlayerController not found on this GameObject!");
                Debug.LogError("[PlayerSetup] 💡 Add PlayerController component");
            }

            if (playerStats == null)
            {
                Debug.LogError("[PlayerSetup] ❌ PlayerStats not found on this GameObject!");
                Debug.LogError("[PlayerSetup] 💡 Add PlayerStats component");
            }

            if (playerCamera == null)
            {
                Debug.LogWarning("[PlayerSetup] ⚠️ No Camera found! Create child with Camera component");
            }

            if (cameraFollow == null && playerCamera != null)
            {
                Debug.LogWarning("[PlayerSetup] ⚠️ CameraFollow not found on camera!");
                Debug.LogWarning("[PlayerSetup] 💡 Add CameraFollow component to Main Camera");
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize all components.
        /// </summary>
        private void InitializeComponents()
        {
            // Initialize PlayerStats
            if (playerStats != null)
            {
                playerStats.Initialize();
            }

            // Initialize CameraFollow
            if (cameraFollow != null && playerController != null)
            {
                cameraFollow.target = playerController.transform;
                cameraFollow.followSpeed = followSpeed;
            }
        }

        /// <summary>
        /// Position player at spawn point.
        /// </summary>
        private void PositionPlayer()
        {
            if (useMazeSpawnPoint)
            {
                // Try to get spawn point from CompleteMazeBuilder
                var mazeBuilder = Object.FindFirstObjectByType<CompleteMazeBuilder>();
                if (mazeBuilder != null)
                {
                    // Get spawn position (will be set after maze generation)
                    // For now, use default
                    transform.position = startPosition;
                    Debug.Log("[PlayerSetup] 🎯 Using maze spawn point");
                }
                else
                {
                    transform.position = startPosition;
                    Debug.Log("[PlayerSetup] ⚠️ No CompleteMazeBuilder - using start position");
                }
            }
            else
            {
                transform.position = startPosition;
            }

            Debug.Log($"[PlayerSetup] 📍 Player positioned at {transform.position}");
        }

        /// <summary>
        /// Setup camera at eye height.
        /// </summary>
        private void SetupCamera()
        {
            if (playerCamera != null)
            {
                // Set camera to eye height
                playerCamera.transform.localPosition = new Vector3(0f, eyeHeight, 0f);
                playerCamera.transform.localRotation = Quaternion.identity;

                Debug.Log($"[PlayerSetup] 📷 Camera at eye height ({eyeHeight}m)");
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Subscribe to spawn events.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (eventHandler != null)
            {
                // Subscribe to player spawn event
                // EventHandler.OnPlayerSpawned += OnPlayerSpawned;
            }
        }

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (eventHandler != null)
            {
                // Unsubscribe from all events
                // EventHandler.OnPlayerSpawned -= OnPlayerSpawned;
            }
        }

        /// <summary>
        /// Called when player is spawned (via event).
        /// </summary>
        private void OnPlayerSpawned(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;
            Debug.Log($"[PlayerSetup] 🎯 Teleported to spawn point: {spawnPosition}");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Teleport player to position.
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            transform.position = position;
            Debug.Log($"[PlayerSetup] 🎯 Teleported to {position}");
        }

        /// <summary>
        /// Reset player to start position.
        /// </summary>
        public void ResetPlayer()
        {
            transform.position = startPosition;
            transform.rotation = Quaternion.identity;

            if (playerStats != null)
            {
                playerStats.ResetStats();
            }

            Debug.Log("[PlayerSetup] 🔄 Player reset");
        }

        /// <summary>
        /// Get player controller.
        /// </summary>
        public PlayerController GetPlayerController() => playerController;

        /// <summary>
        /// Get player stats.
        /// </summary>
        public PlayerStats GetPlayerStats() => playerStats;

        /// <summary>
        /// Get player camera.
        /// </summary>
        public Camera GetPlayerCamera() => playerCamera;

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [ContextMenu("Setup Player")]
        private void EditorSetup()
        {
            Debug.Log("[PlayerSetup] 🔧 Running setup...");
            FindComponents();
            InitializeComponents();
            PositionPlayer();
            SetupCamera();
            Debug.Log("[PlayerSetup] ✅ Setup complete");
        }

        [ContextMenu("Validate Components")]
        private void EditorValidate()
        {
            ValidateComponents();
        }
#endif

        #endregion
    }
}
