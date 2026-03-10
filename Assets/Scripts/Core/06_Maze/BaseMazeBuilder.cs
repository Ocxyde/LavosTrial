// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// BaseMazeBuilder.cs - Common base class for maze builders to reduce duplication

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Base class for maze builders with common functionality.
    /// Extracts duplicate code from CompleteMazeBuilder8 and CompleteCorridorMazeBuilder.
    /// </summary>
    public abstract class BaseMazeBuilder : MonoBehaviour
    {
        #region Common Fields

        [Header("Cardinal Prefabs")]
        [SerializeField] protected GameObject wallPrefab;
        [SerializeField] protected GameObject doorPrefab;
        [SerializeField] protected Material wallMaterial;

        [Header("Object Prefabs")]
        [SerializeField] protected GameObject torchPrefab;
        [SerializeField] protected GameObject chestPrefab;
        [SerializeField] protected GameObject enemyPrefab;
        [SerializeField] protected GameObject floorPrefab;
        [SerializeField] protected GameObject playerPrefab;

        [Header("Config")]
        [SerializeField] protected string configResourcePath = "Config/GameConfig8-default";

        [Header("Wall Settings")]
        [Tooltip("If true, the WallPrefab pivot is at the mesh center (default Unity cube). If false, pivot is at bottom edge.")]
        [SerializeField] protected bool wallPivotIsAtMeshCenter = true;

        [Header("State (read-only)")]
        [SerializeField] protected int currentLevel;
        [SerializeField] protected int currentSeed;
        [SerializeField] protected float lastGenMs;
        [SerializeField] protected float currentDifficultyFactor;

        // Runtime
        protected GameConfig _config;
        protected Transform _wallsRoot;
        protected Transform _objectsRoot;
        protected GameObject _playerInstance;

        #endregion

        #region Public Accessors

        public GameConfig Config => _config;
        public int CurrentLevel => currentLevel;
        public float LastGenMs => lastGenMs;
        public float CurrentDifficultyFactor => currentDifficultyFactor;

        #endregion

        #region Abstract Methods (Must be implemented by derived classes)

        /// <summary>
        /// Generate the maze data. Called by GenerateMaze().
        /// Derived classes must implement their specific generation logic.
        /// </summary>
        protected abstract void GenerateMazeData();

        #endregion

        #region Common Maze Generation Pipeline

        /// <summary>
        /// Main maze generation pipeline. Calls abstract GenerateMazeData() for specific logic.
        /// </summary>
        [ContextMenu("Generate Maze")]
        public void GenerateMaze()
        {
            Debug.Log($"[{GetType().Name}] === GENERATE MAZE STARTED ===");
            float t0 = Time.realtimeSinceStartup;

            // Step 1: Load config
            LoadConfig();
            if (_config == null)
            {
                Debug.LogError($"[{GetType().Name}] Config is NULL!");
                return;
            }

            // Step 2: Validate assets
            ValidateAssets();

            // Step 3: Cleanup previous maze
            DestroyMazeObjects();

            // Step 4: Generate maze data (derived class implementation)
            GenerateMazeData();

            // Step 5: Spawn maze geometry and objects
            SpawnGround();
            SpawnAllWalls();
            SpawnTorches();
            SpawnObjects();
            SpawnPlayer();

            // Step 6: Log completion
            lastGenMs = (Time.realtimeSinceStartup - t0) * 1000f;
            Debug.Log($"[{GetType().Name}] Done -- {lastGenMs:F2}ms factor={currentDifficultyFactor:F3}");
        }

        #endregion

        #region Configuration Loading

        /// <summary>
        /// Load GameConfig from scene or Resources.
        /// </summary>
        protected virtual void LoadConfig()
        {
            var comp = FindFirstObjectByType<GameConfig>();
            if (comp != null)
            {
                _config = comp;
                return;
            }

            var json = Resources.Load<TextAsset>(configResourcePath);
            _config = json != null ? GameConfig.FromJson(json.text) : new GameConfig();

            if (json == null)
                Debug.LogWarning($"[{GetType().Name}] Config not found -- using defaults.");

            if (_config == null)
            {
                Debug.LogError($"[{GetType().Name}] CRITICAL: Failed to load or create GameConfig!");
                enabled = false;
                return;
            }
        }

        #endregion

        #region Asset Validation

        /// <summary>
        /// Validate and load required assets from Resources.
        /// </summary>
        protected virtual void ValidateAssets()
        {
            // Load prefabs from Resources
            wallPrefab   ??= Resources.Load<GameObject>("Prefabs/WallPrefab");
            doorPrefab   ??= Resources.Load<GameObject>("Prefabs/DoorPrefab");
            torchPrefab  ??= Resources.Load<GameObject>("Prefabs/TorchHandlePrefab");
            chestPrefab  ??= Resources.Load<GameObject>("Prefabs/ChestPrefab");
            enemyPrefab  ??= Resources.Load<GameObject>("Prefabs/EnemyPrefab");
            floorPrefab  ??= Resources.Load<GameObject>("Prefabs/FloorTilePrefab");
            playerPrefab ??= Resources.Load<GameObject>("Prefabs/Player");

            // Load materials - try GameConfig first, fallback to Resources
            Material loadedWallMaterial = null;

            if (Application.isPlaying)
            {
                var gameConfig = GameConfig.Instance;
                if (gameConfig != null)
                {
                    loadedWallMaterial = Resources.Load<Material>(gameConfig.wallMaterial);
                }
            }

            loadedWallMaterial ??= Resources.Load<Material>("Materials/WallMaterial");
            wallMaterial ??= loadedWallMaterial;

            // Log errors for missing critical assets
            if (wallPrefab == null)
                Debug.LogError($"[{GetType().Name}] wallPrefab missing! Cannot generate maze walls.");

            if (playerPrefab == null)
                Debug.LogWarning($"[{GetType().Name}] playerPrefab not assigned - player spawning disabled.");

            if (wallMaterial == null)
                Debug.LogWarning($"[{GetType().Name}] wallMaterial not assigned - walls will use default material.");
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Destroy all maze objects from previous generation.
        /// </summary>
        protected virtual void DestroyMazeObjects()
        {
            DestroyContainer(ref _wallsRoot, GetWallsContainerName());
            DestroyContainer(ref _objectsRoot, GetObjectsContainerName());

            if (_playerInstance != null)
            {
                if (Application.isPlaying)
                    Destroy(_playerInstance);
                else
                    DestroyImmediate(_playerInstance);
                _playerInstance = null;
            }
        }

        /// <summary>
        /// Destroy a container GameObject and nullify reference.
        /// </summary>
        protected void DestroyContainer(ref Transform t, string name)
        {
            if (t != null)
            {
                if (Application.isPlaying)
                    Destroy(t.gameObject);
                else
                    DestroyImmediate(t.gameObject);
                t = null;
                return;
            }

            var g = GameObject.Find(name);
            if (g != null)
            {
                if (Application.isPlaying)
                    Destroy(g);
                else
                    DestroyImmediate(g);
            }
        }

        /// <summary>
        /// Override to customize walls container name.
        /// </summary>
        protected virtual string GetWallsContainerName() => "MazeWalls";

        /// <summary>
        /// Override to customize objects container name.
        /// </summary>
        protected virtual string GetObjectsContainerName() => "MazeObjects";

        #endregion

        #region Common Spawning Methods

        /// <summary>
        /// Spawn the ground floor plane.
        /// Override in derived classes if custom ground spawning is needed.
        /// </summary>
        protected virtual void SpawnGround()
        {
            if (floorPrefab == null)
            {
                Debug.LogWarning($"[{GetType().Name}] floorPrefab is null - skipping ground spawn");
                return;
            }

            float size = GetMazeSize() * GetCellSize();
            var go = Instantiate(floorPrefab, new Vector3(size * 0.5f, 0f, size * 0.5f), Quaternion.identity);

            if (go == null)
            {
                Debug.LogError($"[{GetType().Name}] Failed to instantiate floor prefab!");
                return;
            }

            go.name = "MazeFloor";
            go.transform.localScale = new Vector3(size, 1f, size);
        }

        /// <summary>
        /// Spawn all walls. Override in derived classes for custom wall spawning.
        /// </summary>
        protected abstract void SpawnAllWalls();

        /// <summary>
        /// Spawn torches. Override in derived classes for custom torch placement.
        /// </summary>
        protected virtual void SpawnTorches()
        {
            // Default implementation - override in derived classes
        }

        /// <summary>
        /// Spawn objects (chests, enemies). Override in derived classes.
        /// </summary>
        protected virtual void SpawnObjects()
        {
            // Default implementation - override in derived classes
        }

        /// <summary>
        /// Spawn the player at the spawn position.
        /// </summary>
        protected virtual void SpawnPlayer()
        {
            if (playerPrefab == null)
            {
                Debug.LogError($"[{GetType().Name}] playerPrefab is NULL - cannot spawn player!");
                return;
            }

            Vector3 spawnPos = GetPlayerSpawnPosition();
            _playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

            if (_playerInstance == null)
            {
                Debug.LogError($"[{GetType().Name}] Failed to instantiate player prefab!");
                return;
            }

            _playerInstance.name = "Player";
            Debug.Log($"[{GetType().Name}] Player spawned at {spawnPos}");
        }

        #endregion

        #region Helper Methods (Override in derived classes)

        /// <summary>
        /// Get the maze size (width/height assumed square).
        /// </summary>
        protected abstract float GetMazeSize();

        /// <summary>
        /// Get the cell size from config.
        /// </summary>
        protected float GetCellSize() => _config?.CellSize ?? 1f;

        /// <summary>
        /// Get the player spawn position.
        /// </summary>
        protected abstract Vector3 GetPlayerSpawnPosition();

        /// <summary>
        /// Get wall thickness from config.
        /// </summary>
        protected virtual float GetWallThickness() => _config?.WallThickness ?? 0.2f;

        /// <summary>
        /// Get wall height from config.
        /// </summary>
        protected virtual float GetWallHeight() => _config?.WallHeight ?? 1f;

        #endregion
    }
}
