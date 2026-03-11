// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using System.Linq;
using UnityEngine;
using Code.Lavos.Core.Advanced;
using Code.Lavos.Core.Environment;

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
    //   8.  Walls          -> Cardinal wall prefabs
    //                         Pivot fixed at bottom-center of each wall edge
    //   9.  Doors          -> Placed ON the access wall, oriented to face corridor
    //  10.  Torches        -> Scaled torch placement
    //  11.  Save           -> Binary .lvm  ->  Runtimes/Mazes/
    //  12.  Player         -> Spawn LAST
    // -------------------------------------------------------------------------
    public sealed class CompleteMazeBuilder8 : BaseMazeBuilder
    {
        [Header("Generator Options")]
        [Tooltip("Use new GuaranteedPathMazeGenerator (Minotaur Maze)")]
        public bool UseGuaranteedPathGenerator = false;
        [Tooltip("Use PassageFirstMazeGenerator for passage-first generation")]
        public bool UsePassageFirstGenerator = false;

        [Header("Random Seed (Scene)")]
        [Tooltip("Enable to use a fixed seed from Inspector")]
        [SerializeField] private bool useFixedSeed = false;
        [Tooltip("Seed value for reproducible maze generation")]
        [SerializeField] private int fixedSeed = 12345;
        [Tooltip("Click to generate a new random seed")]
        [SerializeField] private bool randomizeSeed = false;

        [Header("Object Spawning")]
        [Tooltip("Enable enemy spawning in the maze")]
        [SerializeField] private bool spawnEnemies = true;
        [Tooltip("Enemy density (0-1) - percentage of rooms with enemies")]
        [Range(0f, 1f)]
        [SerializeField] private float enemyDensity = 0.08f;
        [Tooltip("Enable chest spawning in the maze")]
        [SerializeField] private bool spawnChests = true;
        [Tooltip("Chest density (0-1) - percentage of rooms with chests")]
        [Range(0f, 1f)]
        [SerializeField] private float chestDensity = 0.05f;
        [Tooltip("Minimum enemies at low levels (ensures combat at lvl 0-3)")]
        [Range(0f, 1f)]
        [SerializeField] private float minEnemyDensity = 0.04f;

        [Header("Door Prefabs")]
        [Tooltip("Locked door prefab (requires key)")]
        [SerializeField] protected GameObject lockedDoorPrefab = null;
        [Tooltip("Secret door prefab (hidden passages)")]
        [SerializeField] protected GameObject secretDoorPrefab = null;
        [Tooltip("Exit door prefab (interactable)")]
        [SerializeField] protected GameObject exitDoorPrefab = null;
        // NOTE: doorPrefab is inherited from BaseMazeBuilder (do not redeclare!)

        // Runtime (specific to CompleteMazeBuilder8)
        private MazeData8 _mazeData;
        private GridMazeGenerator _generator;
        private GuaranteedPathMazeGenerator _guaranteedGenerator;
        private bool _lastRandomizeSeedState = false;

        // -------------------------------------------------------------------------
        // Public Accessors (for other systems to access maze data)
        // -------------------------------------------------------------------------
        public MazeData8 MazeData => _mazeData;

        // -------------------------------------------------------------------------
        // Unity lifecycle
        // -------------------------------------------------------------------------
        private void Start()
        {
            InitializeSeed();
            GenerateMaze();
        }

        private void InitializeSeed()
        {
            if (randomizeSeed && !_lastRandomizeSeedState)
            {
                fixedSeed = Random.Range(int.MinValue, int.MaxValue);
                Debug.Log($"[MazeBuilder8] Random seed generated: {fixedSeed}");
                randomizeSeed = false;
            }

            currentSeed = useFixedSeed ? fixedSeed : Random.Range(int.MinValue, int.MaxValue);
            _lastRandomizeSeedState = randomizeSeed;

            Debug.Log($"[MazeBuilder8] Seed initialized: {currentSeed} (fixed={useFixedSeed})");
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
            useFixedSeed = true;
            fixedSeed = seed;
            Debug.Log($"[MazeBuilder8] Level and seed set: L{level} S{seed}");
        }

        /// <summary>
        /// Generate a new random seed and regenerate the maze.
        /// </summary>
        [ContextMenu("Randomize Seed & Regenerate")]
        public void RandomizeSeedAndRegenerate()
        {
            fixedSeed = Random.Range(int.MinValue, int.MaxValue);
            useFixedSeed = true;
            currentSeed = fixedSeed;
            Debug.Log($"[MazeBuilder8] Seed randomized: {fixedSeed}");
            GenerateMaze();
        }

        /// <summary>
        /// Set a specific seed for reproducible maze generation.
        /// </summary>
        public void SetSeed(int seed)
        {
            useFixedSeed = true;
            fixedSeed = seed;
            currentSeed = seed;
            Debug.Log($"[MazeBuilder8] Seed set: {seed}");
        }

        /// <summary>
        /// Get the current seed value.
        /// </summary>
        public int GetSeed() => currentSeed;

        /// <summary>
        /// Toggle between fixed and random seed mode.
        /// </summary>
        public void SetFixedSeedMode(bool fixedMode)
        {
            useFixedSeed = fixedMode;
            Debug.Log($"[MazeBuilder8] Fixed seed mode: {fixedMode}");
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
                    var dungeonData = MazeBinaryStorage8Compat.Load(currentLevel, currentSeed);
                    _mazeData = ConvertDungeonToMazeData(dungeonData);
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

                // Use PassageFirst generator if enabled
                if (UseGuaranteedPathGenerator)
                {
                    // NEW: Minotaur Maze - Guaranteed path with classic labyrinth structure
                    Debug.Log("[MazeBuilder8] Using GuaranteedPathMazeGenerator (Minotaur Maze)");
                    _guaranteedGenerator ??= new GuaranteedPathMazeGenerator();

                    // Create dungeon config for guaranteed path generator
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
                        ChestDensity = spawnChests ? chestDensity : 0f,
                        EnemyDensity = spawnEnemies ? Mathf.Max(minEnemyDensity, enemyDensity) : 0f,
                        DeadEndDensity = _config.MazeCfg.DeadEndDensity,
                        AllowDiagonalWalls = false,
                        Difficulty = new DifficultyScalerConfig
                        {
                            BaseFactor = 1.0f,
                            FactorPerLevel = 0.15f,
                            MaxFactor = _config.DifficultyCfg.MaxFactor,
                            SizeGrowthPerLevel = 2,
                        },
                    };

                    var dungeonData = _guaranteedGenerator.Generate(currentSeed, currentLevel, dungeonCfg);
                    _mazeData = ConvertDungeonToMazeData(dungeonData);
                }
                else if (UsePassageFirstGenerator)
                {
                    var passageGenerator = new PassageFirstMazeGenerator();
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
                        ChestDensity = spawnChests ? chestDensity : 0f,
                        EnemyDensity = spawnEnemies ? Mathf.Max(minEnemyDensity, enemyDensity) : 0f,
                        DeadEndDensity = _config.MazeCfg.DeadEndDensity,
                        AllowDiagonalWalls = false,
                        Difficulty = new DifficultyScalerConfig
                        {
                            BaseFactor = 1.0f,
                            FactorPerLevel = 0.15f,
                            MaxFactor = _config.DifficultyCfg.MaxFactor,
                            SizeGrowthPerLevel = 2,
                        },
                    };
                    var dungeonData = passageGenerator.Generate(currentSeed, currentLevel, dungeonCfg);
                    _mazeData = ConvertDungeonToMazeData(dungeonData);
                }
                else
                {
                    // DEFAULT: Grid maze with dead-end corridors and fill
                    Debug.Log("[MazeBuilder8] Using GridMazeGenerator (DFS + Dead-Ends + Fill)");
                    _generator ??= new GridMazeGenerator();
                    _mazeData = _generator.Generate(currentSeed, currentLevel, _config.MazeCfg, _config.DifficultyCfg);
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

            // 8 - Walls (cardinal only)
            SpawnAllWalls();

            // 9 - Doors (NOW INTEGRATED WITH WALLS - WallWithDoorHandler spawns both)
            // SpawnDoors() - Disabled 2026-03-11, doors spawned with walls

            // 10 - Torches
            SpawnTorches();

            // Objects (chests + enemies)
            SpawnObjects();

            // Exit Door (NOW INTEGRATED - spawned by WallWithDoorHandler)
            // SpawnExitDoor() - Disabled 2026-03-11

            // 11b - Visual Room Markers (spawn/exit indicators)
            SpawnRoomMarkers();

            // 11 - Binary save
            try
            {
                if (!MazeBinaryStorage8Compat.Exists(currentLevel, currentSeed))
                {
                    var dungeonData = ConvertMazeToDungeonData(_mazeData);
                    MazeBinaryStorage8Compat.Save(dungeonData);
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
        // 2+3 - Asset validation
        // -------------------------------------------------------------------------
        protected override void ValidateAssets()
        {
            base.ValidateAssets();
        }

        // -------------------------------------------------------------------------
        // Get wall thickness from config (no hardcoded values)
        // -------------------------------------------------------------------------
        private float WallThickness => _config?.WallThickness ?? 0.2f;

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

            // Use appropriate generator based on settings
            if (UseGuaranteedPathGenerator)
            {
                Debug.Log("[MazeBuilder8] Using GuaranteedPathMazeGenerator (Minotaur Maze)");
                _guaranteedGenerator ??= new GuaranteedPathMazeGenerator();

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
                    ChestDensity = spawnChests ? chestDensity : 0f,
                    EnemyDensity = spawnEnemies ? Mathf.Max(minEnemyDensity, enemyDensity) : 0f,
                    DeadEndDensity = _config.MazeCfg.DeadEndDensity,
                    AllowDiagonalWalls = false,
                    Difficulty = new DifficultyScalerConfig
                    {
                        BaseFactor = 1.0f,
                        FactorPerLevel = 0.15f,
                        MaxFactor = _config.DifficultyCfg.MaxFactor,
                        SizeGrowthPerLevel = 2,
                    },
                };

                var dungeonData = _guaranteedGenerator.Generate(currentSeed, currentLevel, dungeonCfg);
                _mazeData = ConvertDungeonToMazeData(dungeonData);
            }
            else if (UsePassageFirstGenerator)
            {
                var passageGenerator = new PassageFirstMazeGenerator();
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
                    ChestDensity = spawnChests ? chestDensity : 0f,
                    EnemyDensity = spawnEnemies ? Mathf.Max(minEnemyDensity, enemyDensity) : 0f,
                    DeadEndDensity = _config.MazeCfg.DeadEndDensity,
                    AllowDiagonalWalls = false,
                    Difficulty = new DifficultyScalerConfig
                    {
                        BaseFactor = 1.0f,
                        FactorPerLevel = 0.15f,
                        MaxFactor = _config.DifficultyCfg.MaxFactor,
                        SizeGrowthPerLevel = 2,
                    },
                };
                var dungeonData = passageGenerator.Generate(currentSeed, currentLevel, dungeonCfg);
                _mazeData = ConvertDungeonToMazeData(dungeonData);
            }
            else
            {
                Debug.Log("[MazeBuilder8] Using GridMazeGenerator (DFS + Dead-Ends + Fill)");
                _generator ??= new GridMazeGenerator();
                _mazeData = _generator.Generate(currentSeed, currentLevel, _config.MazeCfg, _config.DifficultyCfg);
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
        /// Convert DungeonMazeData to MazeData8 for compatibility
        /// </summary>
        private MazeData8 ConvertDungeonToMazeData(DungeonMazeData dungeonData)
        {
            if (dungeonData == null) return null;

            var mazeData = new MazeData8(dungeonData.Width, dungeonData.Height, dungeonData.Seed, dungeonData.Level);
            mazeData.DifficultyFactor = dungeonData.DifficultyFactor;

            for (int x = 0; x < dungeonData.Width; x++)
            {
                for (int z = 0; z < dungeonData.Height; z++)
                {
                    var dungeonCell = dungeonData.GetCell(x, z);
                    mazeData.SetCell(x, z, (CellFlags8)dungeonCell);
                }
            }

            return mazeData;
        }

        /// <summary>
        /// Convert MazeData8 to DungeonMazeData for storage compatibility
        /// </summary>
        private DungeonMazeData ConvertMazeToDungeonData(MazeData8 mazeData)
        {
            if (mazeData == null) return null;

            var dungeonData = new DungeonMazeData(mazeData.Width, mazeData.Height, mazeData.Seed, mazeData.Level);
            dungeonData.DifficultyFactor = mazeData.DifficultyFactor;

            for (int x = 0; x < mazeData.Width; x++)
            {
                for (int z = 0; z < mazeData.Height; z++)
                {
                    var mazeCell = mazeData.GetCell(x, z);
                    dungeonData.SetCell(x, z, (uint)mazeCell);
                }
            }

            // Copy spawn and exit positions
            dungeonData.SetSpawn(mazeData.SpawnCell.x, mazeData.SpawnCell.z);
            dungeonData.SetExit(mazeData.ExitCell.x, mazeData.ExitCell.z);

            return dungeonData;
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
        // SpawnAllWalls (UPDATED 2026-03-11 - uses WallWithDoorHandler)
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

            // Plug-in-Out: Find or create root transforms
            _wallsRoot = FindFirstObjectByType<Transform>();
            if (_wallsRoot == null || _wallsRoot.name != "MazeWalls8")
            {
                _wallsRoot = FindObjectsOfType<Transform>().FirstOrDefault(t => t.name == "MazeWalls8");
            }
            if (_wallsRoot == null)
            {
                Debug.LogWarning("[MazeBuilder8] MazeWalls8 not found! Creating new (assign in scene for Plug-in-Out compliance).");
                _wallsRoot = new GameObject("MazeWalls8").transform;
            }
            
            Transform doorsRoot = FindFirstObjectByType<Transform>();
            if (doorsRoot == null || doorsRoot.name != "MazeDoors")
            {
                doorsRoot = FindObjectsOfType<Transform>().FirstOrDefault(t => t.name == "MazeDoors");
            }
            if (doorsRoot == null)
            {
                Debug.LogWarning("[MazeBuilder8] MazeDoors not found! Creating new (assign in scene for Plug-in-Out compliance).");
                doorsRoot = new GameObject("MazeDoors").transform;
            }
            
            doorsRoot.SetParent(_wallsRoot, false);

            float cs = _config.CellSize;
            float wh = _config.WallHeight;

            Debug.Log(
                $"[MazeBuilder8] Spawning walls with integrated doors using WallWithDoorHandler");

            // Prepare door prefabs array
            GameObject[] doorPrefabs = new GameObject[4];
            doorPrefabs[0] = doorPrefab;          // Normal
            doorPrefabs[1] = lockedDoorPrefab;    // Locked
            doorPrefabs[2] = secretDoorPrefab;    // Secret
            doorPrefabs[3] = exitDoorPrefab;      // Exit

            // Use WallWithDoorHandler to spawn walls with integrated doors
            WallWithDoorHandler.SpawnWallsWithDoors(
                _mazeData,
                wallPrefab,
                doorPrefabs,
                wallMaterial,
                _wallsRoot,
                doorsRoot,
                cs, wh, WallThickness,
                wallPivotIsAtMeshCenter);

            Debug.Log($"[MazeBuilder8] Wall+Door spawning complete");
        }

        /// <summary>
        /// Check if a cell is walkable (no wall flags set).
        /// A cell with Wall_All = 0 is walkable (corridor/floor space).
        /// </summary>
        private bool IsCellWalkable(CellFlags8 cellFlags)
        {
            return ((uint)cellFlags & (uint)Advanced.CellFlags8.Wall_All) == 0;
        }

        /// <summary>
        /// Determine if a wall should be spawned in the given direction.
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
                return true;
            }

            // Check if neighbor is NOT walkable
            var neighborCell = _mazeData.GetCell(nx, nz);
            bool neighborIsWalkable = IsCellWalkable(neighborCell);

            return !neighborIsWalkable;
        }

        // -------------------------------------------------------------------------
        // 9 - Doors (DISABLED 2026-03-11: Now integrated with walls)
        // -------------------------------------------------------------------------
        // NOTE: Door spawning is now handled by WallWithDoorHandler.SpawnWallsWithDoors()
        // Old SpawnDoors(), SpawnExitDoor(), and DirectionToRotation() removed.

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

        // Exit Door (NOW INTEGRATED - spawned by WallWithDoorHandler)
        // NOTE: Exit door is now spawned as part of WallWithDoorHandler.SpawnWallsWithDoors()

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

            var pos = GetValidPlayerSpawnPosition();

            _playerInstance      = Instantiate(playerPrefab, pos, Quaternion.identity);
            if (_playerInstance == null)
            {
                Debug.LogError("[MazeBuilder8] Failed to instantiate player prefab!");
                return;
            }
            _playerInstance.name = "Player";

            Debug.Log($"[MazeBuilder8] Player spawned at {pos}");
        }

        /// <summary>
        /// Get valid player spawn position with collision validation.
        /// Falls back to finding alternative spawn if primary is blocked.
        /// </summary>
        private Vector3 GetValidPlayerSpawnPosition()
        {
            if (_mazeData == null || _config == null)
                return Vector3.zero;

            int sx = _mazeData.SpawnCell.x, sz = _mazeData.SpawnCell.z;
            Vector3 pos = CellCenter(sx, sz, _config.PlayerEyeHeight);

            // Validate spawn position using raycast
            if (Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out var hit, 3f))
            {
                if (hit.collider != null && hit.collider.CompareTag("Wall"))
                {
                    Debug.LogWarning($"[MazeBuilder8] Primary spawn blocked by wall at {pos}, finding alternative...");
                    pos = FindAlternativeSpawnPosition();
                }
            }

            return pos;
        }

        /// <summary>
        /// Find alternative spawn position by searching nearby walkable cells.
        /// </summary>
        private Vector3 FindAlternativeSpawnPosition()
        {
            if (_mazeData == null || _config == null)
                return Vector3.zero;

            int startX = _mazeData.SpawnCell.x;
            int startZ = _mazeData.SpawnCell.z;

            // Search in expanding spiral pattern
            for (int radius = 1; radius < 5; radius++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    for (int z = -radius; z <= radius; z++)
                    {
                        int checkX = startX + x;
                        int checkZ = startZ + z;

                        if (_mazeData.InBounds(checkX, checkZ))
                        {
                            var cell = _mazeData.GetCell(checkX, checkZ);
                            uint cellFlags = (uint)cell;
                            bool isWalkable = (cellFlags & (uint)CellFlags8.AllWalls) == 0;
                            bool isSpawnRoom = (cellFlags & (uint)CellFlags8.SpawnRoom) != 0;

                            if (isWalkable || isSpawnRoom)
                            {
                                Vector3 altPos = CellCenter(checkX, checkZ, _config.PlayerEyeHeight);
                                Debug.Log($"[MazeBuilder8] Alternative spawn found at {altPos} (cell {checkX},{checkZ})");
                                return altPos;
                            }
                        }
                    }
                }
            }

            // Fallback to original position if no alternative found
            Debug.LogError("[MazeBuilder8] No valid spawn position found! Using original spawn.");
            return CellCenter(startX, startZ, _config.PlayerEyeHeight);
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

        public MazeData8 CurrentMazeData   => _mazeData;
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
