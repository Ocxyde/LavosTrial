// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using UnityEngine;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Core
{
    // -------------------------------------------------------------------------
    // CompleteMazeBuilder8 - 8-axis maze orchestrator
    //
    // Attach to a single GameObject in the scene.
    // Plug-in-Out: FindFirstObjectByType everywhere, never AddComponent.
    //
    // Generation pipeline:
    //   1.  Config         -> Load GameConfig8 from scene or JSON
    //   2.  Assets         -> Resolve prefab references
    //   3.  Components     -> Find (never create)
    //   4.  Cleanup        -> Destroy previous maze objects
    //   5.  Ground         -> Spawn floor plane
    //   6.  Spawn Room     -> Guaranteed 5x5 cleared first
    //   7.  Corridors      -> 8-axis DFS + A* (difficulty-scaled)
    //   8.  Walls          -> Cardinal + diagonal wall prefabs
    //                         Pivot fixed at bottom-center of each wall edge
    //   9.  Doors          -> Placed ON the access wall, oriented to face corridor
    //  10.  Torches        -> Scaled torch placement
    //  11.  Save           -> Binary .lvm  ->  Runtimes/Mazes/
    //  12.  Player         -> Spawn LAST
    // -------------------------------------------------------------------------
    public sealed class CompleteMazeBuilder8 : BaseMazeBuilder
    {
        [Header("Diagonal Prefabs")]
        [SerializeField] private GameObject wallDiagPrefab;
        [SerializeField] private GameObject wallCornerPrefab;
        [SerializeField] private Material wallDiagMaterial;
        [SerializeField] private Material wallCornerMaterial;

        [Header("Generator Options")]
        [Tooltip("Use new GuaranteedPathMazeGenerator (Minotaur Maze)")]
        public bool UseGuaranteedPathGenerator = false;
        [Tooltip("Use PassageFirstMazeGenerator for passage-first generation")]
        public bool UsePassageFirstGenerator = false;

        // Runtime (specific to CompleteMazeBuilder8)
        private DungeonMazeData _mazeData;
        private DungeonMazeGenerator _generator;
        private GuaranteedPathMazeGenerator _guaranteedGenerator;

        // -------------------------------------------------------------------------
        // Public Accessors (for other systems to access maze data)
        // -------------------------------------------------------------------------
        public DungeonMazeData MazeData => _mazeData;

        // -------------------------------------------------------------------------
        // Unity lifecycle
        // -------------------------------------------------------------------------
        private void Start()
        {
            currentSeed = Random.Range(int.MinValue, int.MaxValue);
            GenerateMaze();
        }

        // -------------------------------------------------------------------------
        // PUBLIC API
        // -------------------------------------------------------------------------

        /// <summary>
        /// Set the level number and seed before calling GenerateMaze().
        /// </summary>
        public void SetLevelAndSeed(int level, int seed)
        {
            currentLevel = level;
            currentSeed = seed;
            Debug.Log($"[MazeBuilder8] Level and seed set: L{level} S{seed}");
        }

        [ContextMenu("Generate Maze (8-axis)")]
        public void GenerateMaze()
        {
            Debug.Log("[MazeBuilder8] === GENERATE MAZE STARTED ===");
            float t0 = Time.realtimeSinceStartup;

            // 1 - Config
            Debug.Log("[MazeBuilder8] Step 1: Loading config...");
            LoadConfig();
            Debug.Log($"[MazeBuilder8] Config loaded: CellSize={_config?.CellSize} WallHeight={_config?.WallHeight}");
            
            // CRITICAL: Validate config before proceeding
            if (_config == null)
            {
                Debug.LogError("[MazeBuilder8] CRITICAL: _config is NULL - cannot generate maze!");
                Debug.LogError("[MazeBuilder8] Ensure GameConfig component exists in scene or Resources folder");
                return;
            }

            // 2+3 - Assets + Components
            Debug.Log("[MazeBuilder8] Step 2+3: Validating assets...");
            ValidateAssets();
            Debug.Log($"[MazeBuilder8] Assets validated: wallPrefab={(wallPrefab!=null?"OK":"NULL")}");

            // 4 - Cleanup
            Debug.Log("[MazeBuilder8] Step 4: Cleaning up previous maze...");
            DestroyMazeObjects();

            // 5-7 - Data (cache first)
            // Try to load from cache using compat wrapper
            bool foundInCache = false;
            try
            {
                // Try compat wrapper first (with reflection fallback)
                if (MazeBinaryStorage8Compat.Exists(currentLevel, currentSeed))
                {
                    Debug.Log($"[MazeBuilder8] Cache hit  L{currentLevel}  S{currentSeed}");
                    _mazeData = MazeBinaryStorage8Compat.Load(currentLevel, currentSeed);
                    foundInCache = (_mazeData != null);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MazeBuilder8] Cache load failed, will regenerate: {ex.Message}");
            }

            if (!foundInCache)
            {
                Debug.Log($"[MazeBuilder8] Cache miss  L{currentLevel}  S{currentSeed} - generating new maze");
                
                // Convert GameConfig to DungeonMazeConfig
                var dungeonCfg = new DungeonMazeConfig
                {
                    BaseSize = _config.MazeCfg.BaseSize,
                    MinSize = _config.MazeCfg.MinSize,
                    MaxSize = _config.MazeCfg.MaxSize,
                    CellSize = _config.CellSize,
                    WallHeight = _config.WallHeight,
                    SpawnRoomSize = _config.MazeCfg.SpawnRoomSize,
                    ExitRoomSize = MazeConfig.ExitRoomSize,
                    TorchChance = _config.MazeCfg.TorchChance,
                    ChestDensity = _config.MazeCfg.ChestDensity,
                    EnemyDensity = _config.MazeCfg.EnemyDensity,
                    AllowDiagonalWalls = false, // DiagonalWalls removed 2026-03-09 - cardinal-only passages
                    Difficulty = new DifficultyScalerConfig
                    {
                        BaseFactor = 1.0f,
                        FactorPerLevel = 0.15f,
                        MaxFactor = _config.DifficultyCfg.MaxFactor,
                        SizeGrowthPerLevel = 2,
                    },
                };

                // Use PassageFirst generator if enabled, otherwise use DFS
                if (UseGuaranteedPathGenerator)
                {
                    // NEW: Minotaur Maze - Guaranteed path with classic labyrinth structure
                    Debug.Log("[MazeBuilder8] Using GuaranteedPathMazeGenerator (Minotaur Maze)");
                    _guaranteedGenerator ??= new GuaranteedPathMazeGenerator();

                    // Use same dungeonCfg we already created
                    _mazeData = _guaranteedGenerator.Generate(currentSeed, currentLevel, dungeonCfg);
                }
                else if (UsePassageFirstGenerator || dungeonCfg.UsePassageFirst)
                {
                    var passageGenerator = new PassageFirstMazeGenerator();
                    _mazeData = passageGenerator.Generate(currentSeed, currentLevel, dungeonCfg);
                }
                else
                {
                    // DEFAULT: Dungeon maze with rooms and corridors
                    Debug.Log("[MazeBuilder8] Using DungeonMazeGenerator (Rooms + Corridors)");
                    _generator ??= new DungeonMazeGenerator();
                    _mazeData = _generator.Generate(currentSeed, currentLevel, dungeonCfg);
                }
            }

            // Verify maze data
            if (_mazeData == null)
            {
                Debug.LogError("[MazeBuilder8] CRITICAL: _mazeData is NULL after generation/load!");
                return;
            }

            Debug.Log(
                $"[MazeBuilder8] Maze data loaded: {_mazeData.Width}x{_mazeData.Height} " +
                $"seed={_mazeData.Seed} level={_mazeData.Level}");

            currentDifficultyFactor = _mazeData.DifficultyFactor;

            // Verify config before proceeding
            if (_config == null)
            {
                Debug.LogError("[MazeBuilder8] CRITICAL: _config is NULL - cannot spawn maze objects!");
                return;
            }

            // 5 - Ground
            SpawnGround();

            // Verify ground spawned
            Debug.Log("[MazeBuilder8] Ground spawned, now spawning walls...");

            // 8 - Walls (cardinal + diagonal)
            SpawnAllWalls();

            // 9 - Doors (on access wall, oriented)
            SpawnDoors();

            // 10 - Torches
            SpawnTorches();

            // Objects (chests + enemies)
            SpawnObjects();

            // 11b - Visual Room Markers (spawn/exit indicators)
            SpawnRoomMarkers();

            // 11 - Binary save
            try
            {
                if (!MazeBinaryStorage8Compat.Exists(currentLevel, currentSeed))
                {
                    MazeBinaryStorage8Compat.Save(_mazeData);
                    Debug.Log($"[MazeBuilder8] Maze saved to cache L{currentLevel} S{currentSeed}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MazeBuilder8] Cache save failed: {ex.Message}");
            }

            // 12 - Player LAST
            SpawnPlayer();

            lastGenMs = (Time.realtimeSinceStartup - t0) * 1000f;
            Debug.Log(
                $"[MazeBuilder8] Done -- {lastGenMs:F2} ms  " +
                $"factor={currentDifficultyFactor:F3}");
        }

        [ContextMenu("Next Level (Harder)")]
        public void NextLevel()
        {
            currentLevel++;
            currentSeed = Random.Range(int.MinValue, int.MaxValue);
            if (_config != null)
            {
                Debug.Log(
                    $"[MazeBuilder8] Level {currentLevel}  " +
                    $"expected factor={_config.DifficultyCfg.Factor(currentLevel):F3}");
            }
            else
            {
                Debug.Log($"[MazeBuilder8] Level {currentLevel}");
            }
            GenerateMaze();
        }

        // -------------------------------------------------------------------------
        // 1 - Config (override to add specific logging)
        // -------------------------------------------------------------------------
        protected override void LoadConfig()
        {
            base.LoadConfig();
            if (_config != null)
            {
                Debug.Log($"[MazeBuilder8] Config loaded - BaseSize: {_config.MazeCfg.BaseSize}");
            }
        }

        // -------------------------------------------------------------------------
        // 2+3 - Asset validation (override to add diagonal prefabs)
        // -------------------------------------------------------------------------
        protected override void ValidateAssets()
        {
            base.ValidateAssets();

            // Load diagonal-specific prefabs
            wallDiagPrefab   ??= Resources.Load<GameObject>("Prefabs/WallDiagPrefab");
            wallCornerPrefab ??= Resources.Load<GameObject>("Prefabs/WallCornerPrefab");

            // Load materials - try GameConfig first, fallback to Resources
            Material loadedWallMaterial = null;

            if (Application.isPlaying)
            {
                var gameConfig = GameConfig.Instance;
                if (gameConfig != null)
                {
                    loadedWallMaterial = Resources.Load<Material>(gameConfig.WallMaterial);
                }
            }

            loadedWallMaterial ??= Resources.Load<Material>("Materials/WallMaterial");
            wallDiagMaterial   ??= loadedWallMaterial;
            wallCornerMaterial ??= loadedWallMaterial;

            if (wallDiagPrefab == null)
            {
                wallDiagPrefab = wallPrefab;
                Debug.LogWarning("[MazeBuilder8] wallDiagPrefab not set -- reusing wallPrefab at 45 deg.");
            }
        }

        // -------------------------------------------------------------------------
        // Get wall thickness from config (no hardcoded values)
        // -------------------------------------------------------------------------
        private float WallThickness => _config?.WallThickness ?? 0.2f;
        private float DiagonalWallThickness => _config?.defaultDiagonalWallThickness ?? 0.5f;

        // -------------------------------------------------------------------------
        // 4 - Cleanup (override for custom container names)
        // -------------------------------------------------------------------------
        protected override string GetWallsContainerName() => "MazeWalls8";
        protected override string GetObjectsContainerName() => "MazeObjects8";

        // -------------------------------------------------------------------------
        // 5 - Ground (override for 8-specific naming)
        // -------------------------------------------------------------------------
        protected override void SpawnGround()
        {
            if (floorPrefab == null || _mazeData == null || _config == null)
            {
                Debug.LogError("[MazeBuilder8] SpawnGround: _mazeData or _config is NULL!");
                return;
            }

            float sz = _mazeData.Width * _config.CellSize;
            var go = Instantiate(floorPrefab, new Vector3(sz * 0.5f, 0f, sz * 0.5f), Quaternion.identity);

            if (go == null)
            {
                Debug.LogError("[MazeBuilder8] Failed to instantiate floor prefab!");
                return;
            }

            go.name = "MazeFloor8";
            go.transform.localScale = new Vector3(sz, 1f, sz);
        }

        // -------------------------------------------------------------------------
        // Implementation of Abstract Methods from BaseMazeBuilder
        // -------------------------------------------------------------------------

        /// <summary>
        /// Generate maze data (implementation of abstract method).
        /// </summary>
        protected override void GenerateMazeData()
        {
            Debug.Log($"[MazeBuilder8] Generating new maze L{currentLevel} S{currentSeed}");

            // Create dungeon config
            var dungeonCfg = new DungeonMazeConfig
            {
                BaseSize = _config.MazeCfg.BaseSize,
                MinSize = _config.MazeCfg.MinSize,
                MaxSize = _config.MazeCfg.MaxSize,
                CellSize = _config.CellSize,
                WallHeight = _config.WallHeight,
                SpawnRoomSize = _config.MazeCfg.SpawnRoomSize,
                ExitRoomSize = MazeConfig.ExitRoomSize,
                TorchChance = _config.MazeCfg.TorchChance,
                ChestDensity = _config.MazeCfg.ChestDensity,
                EnemyDensity = _config.MazeCfg.EnemyDensity,
                AllowDiagonalWalls = false,
                Difficulty = new DifficultyScalerConfig
                {
                    BaseFactor = 1.0f,
                    FactorPerLevel = 0.15f,
                    MaxFactor = _config.DifficultyCfg.MaxFactor,
                    SizeGrowthPerLevel = 2,
                },
            };

            // Use appropriate generator based on settings
            if (UseGuaranteedPathGenerator)
            {
                Debug.Log("[MazeBuilder8] Using GuaranteedPathMazeGenerator (Minotaur Maze)");
                _guaranteedGenerator ??= new GuaranteedPathMazeGenerator();
                _mazeData = _guaranteedGenerator.Generate(currentSeed, currentLevel, dungeonCfg);
            }
            else if (UsePassageFirstGenerator || dungeonCfg.UsePassageFirst)
            {
                var passageGenerator = new PassageFirstMazeGenerator();
                _mazeData = passageGenerator.Generate(currentSeed, currentLevel, dungeonCfg);
            }
            else
            {
                Debug.Log("[MazeBuilder8] Using DungeonMazeGenerator (Rooms + Corridors)");
                _generator ??= new DungeonMazeGenerator();
                _mazeData = _generator.Generate(currentSeed, currentLevel, dungeonCfg);
            }

            // Verify maze data
            if (_mazeData == null)
            {
                Debug.LogError("[MazeBuilder8] CRITICAL: _mazeData is NULL after generation/load!");
                return;
            }

            Debug.Log($"[MazeBuilder8] Maze data loaded: {_mazeData.Width}x{_mazeData.Height} seed={_mazeData.Seed} level={_mazeData.Level}");
            currentDifficultyFactor = _mazeData.DifficultyFactor;

            // Verify config before proceeding
            if (_config == null)
            {
                Debug.LogError("[MazeBuilder8] CRITICAL: _config is NULL - cannot spawn maze objects!");
                return;
            }
        }

        /// <summary>
        /// Get maze size (implementation of abstract method).
        /// </summary>
        protected override float GetMazeSize() => _mazeData?.Width ?? 0f;

        /// <summary>
        /// Get player spawn position (implementation of abstract method).
        /// </summary>
        protected override Vector3 GetPlayerSpawnPosition()
        {
            if (_mazeData == null || _config == null)
                return Vector3.zero;

            int sx = _mazeData.SpawnCell.x, sz = _mazeData.SpawnCell.z;
            return new Vector3(
                (sx + 0.5f) * _config.CellSize,
                _config.PlayerEyeHeight,
                (sz + 0.5f) * _config.CellSize
            );
        }

        // -------------------------------------------------------------------------
        // SpawnAllWalls (override - uses MazeWallSpawner module)
        // -------------------------------------------------------------------------
        protected override void SpawnAllWalls()
        {
            if (wallPrefab == null)
            {
                Debug.LogError("[MazeBuilder8] wallPrefab is NULL - cannot spawn walls!");
                return;
            }

            if (_mazeData == null || _config == null)
            {
                Debug.LogError("[MazeBuilder8] SpawnAllWalls: _mazeData or _config is NULL!");
                return;
            }

            _wallsRoot = new GameObject("MazeWalls8").transform;

            float cs = _config.CellSize;
            float wh = _config.WallHeight;

            int cardinalCount = 0;

            Debug.Log(
                $"[MazeBuilder8] Spawning walls for {_mazeData.Width}x{_mazeData.Height} maze " +
                $"using modular MazeWallSpawner (cardinal-only passages)");

            // Spawn cardinal walls using modular spawner
            for (int z = 0; z < _mazeData.Height; z++)
            for (int x = 0; x < _mazeData.Width; x++)
            {
                var cell = _mazeData.GetCell(x, z);
                bool isWalkable = IsCellWalkable(cell);

                if (isWalkable)
                {
                    // Cardinal walls only (N, S, E, W)
                    if (ShouldSpawnWall(x, z, Direction8.N, out _))
                    {
                        MazeWallSpawner.SpawnCardinalWall(
                            x, z, Direction8.N,
                            wallPrefab, wallMaterial,
                            cs, wh, WallThickness,
                            wallPivotIsAtMeshCenter, _wallsRoot);
                        cardinalCount++;
                    }
                    if (ShouldSpawnWall(x, z, Direction8.E, out _))
                    {
                        MazeWallSpawner.SpawnCardinalWall(
                            x, z, Direction8.E,
                            wallPrefab, wallMaterial,
                            cs, wh, WallThickness,
                            wallPivotIsAtMeshCenter, _wallsRoot);
                        cardinalCount++;
                    }
                    if (ShouldSpawnWall(x, z, Direction8.S, out _))
                    {
                        MazeWallSpawner.SpawnCardinalWall(
                            x, z, Direction8.S,
                            wallPrefab, wallMaterial,
                            cs, wh, WallThickness,
                            wallPivotIsAtMeshCenter, _wallsRoot);
                        cardinalCount++;
                    }
                    if (ShouldSpawnWall(x, z, Direction8.W, out _))
                    {
                        MazeWallSpawner.SpawnCardinalWall(
                            x, z, Direction8.W,
                            wallPrefab, wallMaterial,
                            cs, wh, WallThickness,
                            wallPivotIsAtMeshCenter, _wallsRoot);
                        cardinalCount++;
                    }

                    // NOTE: Diagonal walls (NE, NW, SE, SW) removed 2026-03-09
                    // MazeWallSpawner.SpawnDiagonalWall() is available for future use
                }
            }

            Debug.Log(
                $"[MazeBuilder8] Wall spawn complete: " +
                $"{cardinalCount} cardinal walls (cardinal-only mode)");
        }

        /// <summary>
        /// Check if a cell is walkable (no wall flags set).
        /// A cell with Wall_All = 0 is walkable (corridor/floor space).
        /// </summary>
        private bool IsCellWalkable(uint cellFlags)
        {
            return (cellFlags & Advanced.CellFlags8.Wall_All) == 0;
        }

        /// <summary>
        /// Determine if a wall should be spawned in the given direction.
        /// 
        /// A wall is spawned if:
        /// 1. The neighbor is out of bounds (maze perimeter wall), OR
        /// 2. The neighbor is NOT walkable (has wall flags = blocked cell)
        ///
        /// Parameters:
        /// - cx, cz: Current cell coordinates
        /// - dir: Direction to check
        /// - isBoundary: Output true if wall is at maze perimeter edge
        ///
        /// Returns: true if wall should be spawned
        /// </summary>
        private bool ShouldSpawnWall(int cx, int cz, Direction8 dir, out bool isBoundary)
        {
            var (dx, dz) = Direction8Helper.ToOffset(dir);
            int nx = cx + dx;
            int nz = cz + dz;

            isBoundary = false;

            // Check if neighbor is out of bounds (maze perimeter wall)
            if (nx < 0 || nx >= _mazeData.Width || nz < 0 || nz >= _mazeData.Height)
            {
                isBoundary = true;
                return true; // Spawn wall at maze boundary
            }

            // Check if neighbor is NOT walkable (has walls = blocked)
            var neighborCell = _mazeData.GetCell(nx, nz);
            bool neighborIsWalkable = IsCellWalkable(neighborCell);

            // Spawn wall if neighbor is blocked (not walkable)
            return !neighborIsWalkable;
        }

        // -------------------------------------------------------------------------
        // 9 - Doors (uses MazeDoorSpawner module)
        // -------------------------------------------------------------------------
        private void SpawnDoors()
        {
            MazeDoorSpawner.SpawnDoors(
                _mazeData, doorPrefab,
                _config.CellSize, _config.WallHeight,
                WallThickness, wallPivotIsAtMeshCenter);
        }

        // -------------------------------------------------------------------------
        // 10 - Torches (uses MazeObjectSpawner module)
        // -------------------------------------------------------------------------
        private void SpawnTorches()
        {
            EnsureObjectsRoot();
            MazeObjectSpawner.SpawnTorches(
                _mazeData, torchPrefab, _config.CellSize, _objectsRoot);
        }

        // Chests + Enemies (uses MazeObjectSpawner module)
        private void SpawnObjects()
        {
            EnsureObjectsRoot();
            MazeObjectSpawner.SpawnObjects(
                _mazeData, chestPrefab, enemyPrefab, _config.CellSize, _objectsRoot);
        }

        // -------------------------------------------------------------------------
        // 11b - Visual Markers (uses MazeMarkerSpawner module)
        // -------------------------------------------------------------------------
        private void SpawnRoomMarkers()
        {
            MazeMarkerSpawner.SpawnRoomMarkers(
                _mazeData, _config.CellSize,
                markerHeight: 2f, markerScale: 0.3f, markerLightIntensity: 2.5f);
        }

        // 12 - Player  (ALWAYS LAST)
        // -------------------------------------------------------------------------
        private void SpawnPlayer()
        {
            var existing = FindFirstObjectByType<PlayerController>();
            if (existing != null)
            {
                // Player already exists - do NOT teleport (prevents splitting glitch)
                _playerInstance = existing.gameObject;
                Debug.Log("[MazeBuilder8] Player already exists - keeping current position (no teleport)");
                return;
            }

            if (playerPrefab == null)
            {
                Debug.LogWarning("[MazeBuilder8] playerPrefab not set -- skipping spawn.");
                return;
            }

            if (_mazeData == null || _config == null)
            {
                Debug.LogError("[MazeBuilder8] SpawnPlayer: _mazeData or _config is NULL!");
                return;
            }

            var pos = CellCenter(
                _mazeData.SpawnCell.x,
                _mazeData.SpawnCell.z,
                _config.PlayerEyeHeight);

            _playerInstance      = Instantiate(playerPrefab, pos, Quaternion.identity);
            if (_playerInstance == null)
            {
                Debug.LogError("[MazeBuilder8] Failed to instantiate player prefab!");
                return;
            }
            _playerInstance.name = "Player";

            Debug.Log($"[MazeBuilder8] Player spawned at {pos}");
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        // World-space center of cell (cx, cz) at height y
        private Vector3 CellCenter(int cx, int cz, float y = 0f)
        {
            float cs = _config?.CellSize ?? 1f;
            return new Vector3(cx * cs + cs * 0.5f, y, cz * cs + cs * 0.5f);
        }

        private void PlaceAtCell(
            int        cx,
            int        cz,
            GameObject prefab,
            string     objName,
            Transform  parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError($"[MazeBuilder8] PlaceAtCell: prefab is NULL for {objName}!");
                return;
            }

            var go  = Instantiate(prefab, CellCenter(cx, cz), Quaternion.identity, parent);
            if (go != null)
                go.name = objName;
        }

        private void EnsureObjectsRoot()
        {
            if (_objectsRoot == null)
                _objectsRoot = new GameObject("MazeObjects8").transform;
        }

        // Status
        public string StatusString()
            => $"Level {currentLevel} | {_mazeData?.Width}x{_mazeData?.Height} | " +
               $"Seed {currentSeed} | factor={currentDifficultyFactor:F3} | {lastGenMs:F2}ms";

        public DungeonMazeData CurrentMazeData   => _mazeData;
        public int             CurrentLevel      => currentLevel;
        public int             CurrentSeed       => currentSeed;
        public float           CurrentDifficultyFactor => currentDifficultyFactor;

        /// <summary>
        /// Current maze grid size (calculated from level and GameConfig).
        /// Uses DifficultyScaler.MazeSize() with config values.
        /// </summary>
        public int MazeSize
        {
            get
            {
                var cfg = _config ?? GameConfig.Instance;
                return cfg?.DifficultyCfg.MazeSize(
                    currentLevel,
                    cfg.MazeCfg.BaseSize,
                    cfg.MazeCfg.MinSize,
                    cfg.MazeCfg.MaxSize
                ) ?? cfg.MazeCfg.BaseSize;
            }
        }

        // -------------------------------------------------------------------------
        // 8-bit Pixel Art Marker Texture Generator
        // -------------------------------------------------------------------------
        private static Texture2D Create8BitMarkerTexture(Color color, bool isEntrance, int size = 32)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point; // Pixel-perfect, no smoothing
            tex.wrapMode = TextureWrapMode.Clamp;

            // 8-bit color palette (limited colors for retro style)
            byte r = (byte)(color.r * 255);
            byte g = (byte)(color.g * 255);
            byte b = (byte)(color.b * 255);

            // Pixel art pattern (checkerboard + border for 8-bit style)
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Border (2 pixels)
                    if (x < 2 || x >= size - 2 || y < 2 || y >= size - 2)
                    {
                        tex.SetPixel(x, y, new Color32((byte)(r * 0.5f), (byte)(g * 0.5f), (byte)(b * 0.5f), 255));
                    }
                    // Center pattern (checkerboard for pixel art look)
                    else if ((x + y) % 4 < 2)
                    {
                        tex.SetPixel(x, y, new Color32(r, g, b, 255));
                    }
                    else
                    {
                        tex.SetPixel(x, y, new Color32((byte)(r * 0.8f), (byte)(g * 0.8f), (byte)(b * 0.8f), 255));
                    }
                }
            }

            tex.Apply();
            return tex;
        }
    }
}
