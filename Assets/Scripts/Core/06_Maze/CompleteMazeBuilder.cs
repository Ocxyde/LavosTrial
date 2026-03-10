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
        public bool useGuaranteedPathGenerator = false;
        [Tooltip("Use PassageFirstMazeGenerator for passage-first generation")]
        public bool usePassageFirstGenerator = false;

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
                if (useGuaranteedPathGenerator)
                {
                    // NEW: Minotaur Maze - Guaranteed path with classic labyrinth structure
                    Debug.Log("[MazeBuilder8] Using GuaranteedPathMazeGenerator (Minotaur Maze)");
                    _guaranteedGenerator ??= new GuaranteedPathMazeGenerator();

                    // Use same dungeonCfg we already created
                    _mazeData = _guaranteedGenerator.Generate(currentSeed, currentLevel, dungeonCfg);
                }
                else if (usePassageFirstGenerator || dungeonCfg.UsePassageFirst)
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
                    loadedWallMaterial = Resources.Load<Material>(gameConfig.wallMaterial);
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
        // SpawnAllWalls
        //
        // BOUNDARY-BASED WALL SPAWNING (NO OVERLAPPING WALLS):
        // 
        // PROBLEM SOLVED: Previous approach spawned walls from EVERY cell,
        // causing duplicate walls at shared edges (overlapping geometry).
        //
        // SOLUTION: Only spawn walls on BOUNDARIES:
        // 1. For each WALKABLE cell (corridor/floor)
        // 2. Check each neighbor direction
        // 3. Spawn wall ONLY if neighbor is NOT walkable OR is maze edge
        // 4. Result: Clean walls with no overlaps!
        //
        // Cardinal wall position = center of the edge shared by cell (cx,cz)
        //   in direction dir, at y = 0.
        //   Formula: x = (cx + 0.5 + dx * 0.5) * cellSize
        //            z = (cz + 0.5 + dz * 0.5) * cellSize
        //
        // Diagonal wall position = corner shared by cell (cx,cz) in direction dir,
        //   at y = 0.  Scale X = cellSize * sqrt(2).
        // -------------------------------------------------------------------------
        private void SpawnAllWalls()
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
            int diagonalCount = 0;
            int totalCells = _mazeData.Width * _mazeData.Height;
            int boundaryWalls = 0;
            int internalWalls = 0;

            Debug.Log(
                $"[MazeBuilder8] Spawning walls for {_mazeData.Width}x{_mazeData.Height} maze " +
                $"({totalCells} cells) using BOUNDARY method (no overlaps)");

            // Spawn walls ONLY on boundaries between walkable and non-walkable cells
            for (int z = 0; z < _mazeData.Height; z++)
            for (int x = 0; x < _mazeData.Width; x++)
            {
                var cell = _mazeData.GetCell(x, z);
                bool isWalkable = IsCellWalkable(cell);

                // Only process walkable cells (corridors, spawn, exit)
                if (isWalkable)
                {
                    // Check each direction: spawn wall if neighbor is NOT walkable or is boundary
                    if (ShouldSpawnWall(x, z, Direction8.N, out bool isBoundary))
                    {
                        SpawnCardinalWall(x, z, Direction8.N, cs, wh);
                        cardinalCount++;
                        if (isBoundary) boundaryWalls++; else internalWalls++;
                    }
                    if (ShouldSpawnWall(x, z, Direction8.E, out isBoundary))
                    {
                        SpawnCardinalWall(x, z, Direction8.E, cs, wh);
                        cardinalCount++;
                        if (isBoundary) boundaryWalls++; else internalWalls++;
                    }
                    if (ShouldSpawnWall(x, z, Direction8.S, out isBoundary))
                    {
                        SpawnCardinalWall(x, z, Direction8.S, cs, wh);
                        cardinalCount++;
                        if (isBoundary) boundaryWalls++; else internalWalls++;
                    }
                    if (ShouldSpawnWall(x, z, Direction8.W, out isBoundary))
                    {
                        SpawnCardinalWall(x, z, Direction8.W, cs, wh);
                        cardinalCount++;
                        if (isBoundary) boundaryWalls++; else internalWalls++;
                    }

                    // Diagonal walls removed 2026-03-09 - cardinal-only passages
                    // diagonalCount remains 0
                }
            }

            Debug.Log(
                $"[MazeBuilder8] Wall spawn complete: " +
                $"{cardinalCount} cardinal + {diagonalCount} diagonal = " +
                $"{cardinalCount + diagonalCount} total walls " +
                $"(perimeter: {boundaryWalls}, interior: {internalWalls})");
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
        // SpawnCardinalWall
        //
        // Position: bottom-center of the shared edge, y = 0.
        //   edgeCenter.x = (cx + 0.5 + dx * 0.5) * cellSize
        //   edgeCenter.z = (cz + 0.5 + dz * 0.5) * cellSize
        //   edgeCenter.y = 0
        //
        // If pivot is at mesh center, shift up by wh * 0.5 so the wall
        // stands on the floor instead of sinking into it.
        // Scale: X = length along wall, Y = height, Z = thickness (from config)
        // -------------------------------------------------------------------------
        private void SpawnCardinalWall(int cx, int cz, Direction8 dir, float cs, float wh)
        {
            var (dx, dz) = Direction8Helper.ToOffset(dir);

            // Bottom-center of the wall edge at floor level
            var edgePos = new Vector3(
                (cx + 0.5f + dx * 0.5f) * cs,
                0f,
                (cz + 0.5f + dz * 0.5f) * cs
            );

            Quaternion rot = (dir == Direction8.E || dir == Direction8.W)
                ? Quaternion.Euler(0f, 90f, 0f)
                : Quaternion.identity;

            var wall = Instantiate(wallPrefab, edgePos, rot, _wallsRoot);
            if (wall == null)
            {
                Debug.LogError($"[MazeBuilder8] Failed to instantiate cardinal wall at {edgePos}");
                return;
            }

            // Scale: X = length along wall, Y = height, Z = thickness (from config)
            wall.transform.localScale = new Vector3(cs, wh, WallThickness);

            // Apply material if assigned
            if (wallMaterial != null)
            {
                var renderer = wall.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = wallMaterial;
                }
            }

            // If the prefab pivot is at mesh center, offset Y so wall sits on floor
            if (wallPivotIsAtMeshCenter)
            {
                var p = wall.transform.position;
                wall.transform.position = new Vector3(p.x, wh * 0.5f, p.z);
            }

            wall.name = $"Wall_{cx}_{cz}_{dir}";
        }

        // -------------------------------------------------------------------------
        // SpawnDiagonalWall
        //
        // Position: corner shared by cell (cx,cz) in direction dir, y = 0.
        //   NE -> (+h, 0, +h)  rot +45 deg Y
        //   NW -> (-h, 0, +h)  rot -45 deg Y
        //   SE -> (+h, 0, -h)  rot -45 deg Y
        //   SW -> (-h, 0, -h)  rot +45 deg Y
        // Scale: X = cellSize * sqrt(2), Y = height, Z = thickness (from config)
        // -------------------------------------------------------------------------
        private void SpawnDiagonalWall(int cx, int cz, Direction8 dir, float cs, float wh)
        {
            float h      = cs * 0.5f;

            Vector3 offset;
            float   rotY;

            switch (dir)
            {
                case Direction8.NE: offset = new Vector3( h, 0f,  h); rotY =  45f; break;
                case Direction8.NW: offset = new Vector3(-h, 0f,  h); rotY = -45f; break;
                case Direction8.SE: offset = new Vector3( h, 0f, -h); rotY = -45f; break;
                case Direction8.SW: offset = new Vector3(-h, 0f, -h); rotY =  45f; break;
                default: return;
            }

            // Corner position in world space, at y = 0
            var cornerPos = new Vector3(
                (cx + 0.5f) * cs + offset.x,
                0f,
                (cz + 0.5f) * cs + offset.z
            );

            var wall = Instantiate(
                wallDiagPrefab,
                cornerPos,
                Quaternion.Euler(0f, rotY, 0f),
                _wallsRoot);

            if (wall == null)
            {
                Debug.LogError($"[MazeBuilder8] Failed to instantiate diagonal wall at {cornerPos}");
                return;
            }

            // Scale: X = cellSize * sqrt(2), Y = height, Z = thickness (from config)
            float diagLength = cs * Mathf.Sqrt(2f);
            wall.transform.localScale = new Vector3(diagLength, wh, DiagonalWallThickness);

            // Apply diagonal material if assigned
            if (wallDiagMaterial != null)
            {
                var renderer = wall.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = wallDiagMaterial;
                }
            }

            if (wallPivotIsAtMeshCenter)
            {
                var p = wall.transform.position;
                wall.transform.position = new Vector3(p.x, wh * 0.5f, p.z);
            }

            wall.name = $"WallDiag_{cx}_{cz}_{dir}";
        }

        // -------------------------------------------------------------------------
        // 9 - Doors
        //
        // Doors are NOT placed at the cell center.
        // For each cell (spawn / exit), we find the first open cardinal direction
        // and place the door prefab ON that wall edge, oriented to face the corridor.
        //
        // Position = center of the open wall edge (same formula as SpawnCardinalWall)
        // Rotation = perpendicular to the wall normal so the door faces the passage
        // -------------------------------------------------------------------------
        private void SpawnDoors()
        {
            if (doorPrefab == null) return;
            if (_mazeData == null || _config == null)
            {
                Debug.LogError("[MazeBuilder8] SpawnDoors: _mazeData or _config is NULL!");
                return;
            }

            SpawnDoorOnAccessWall(_mazeData.SpawnCell.x, _mazeData.SpawnCell.z, "Door_Entry");
            SpawnDoorOnAccessWall(_mazeData.ExitCell.x,  _mazeData.ExitCell.z,  "Door_Exit");
        }

        private void SpawnDoorOnAccessWall(int cx, int cz, string doorName)
        {
            if (doorPrefab == null) return;
            if (_config == null)
            {
                Debug.LogError("[MazeBuilder8] SpawnDoorOnAccessWall: _config is NULL!");
                return;
            }

            float cs = _config.CellSize;
            float wh = _config.WallHeight;

            // Scan cardinal directions only - doors never on diagonals
            Advanced.Direction8[] cardinals = { Advanced.Direction8.N, Advanced.Direction8.S, Advanced.Direction8.E, Advanced.Direction8.W };

            foreach (var dir in cardinals)
            {
                // Skip if wall is present (door requires an open passage)
                if (_mazeData.HasWall(cx, cz, dir)) continue;

                var (dx, dz) = Advanced.Direction8Helper.ToOffset(dir);
                int nx = cx + dx;
                int nz = cz + dz;
                if (!_mazeData.InBounds(nx, nz)) continue;

                // Bottom-center of the open wall edge, y = 0
                var wallEdgePos = new Vector3(
                    (cx + 0.5f + dx * 0.5f) * cs,
                    0f,
                    (cz + 0.5f + dz * 0.5f) * cs
                );

                // Door faces the corridor: N/S doors rotate 0 deg, E/W doors rotate 90 deg
                float rotY = (dir == Advanced.Direction8.E || dir == Advanced.Direction8.W) ? 90f : 0f;

                var door = Instantiate(
                    doorPrefab,
                    wallEdgePos,
                    Quaternion.Euler(0f, rotY, 0f));

                if (door == null)
                {
                    Debug.LogError($"[MazeBuilder8] Failed to instantiate door at {wallEdgePos}");
                    continue;
                }

                door.name                 = doorName;
                door.transform.localScale = new Vector3(cs, wh, WallThickness);

                // Same pivot fix as walls
                if (wallPivotIsAtMeshCenter)
                {
                    var p = door.transform.position;
                    door.transform.position = new Vector3(p.x, wh * 0.5f, p.z);
                }

                // One door per cell - stop after the first open direction
                break;
            }
        }

        // -------------------------------------------------------------------------
        // 10 - Torches
        // -------------------------------------------------------------------------
        private void SpawnTorches()
        {
            if (torchPrefab == null) return;
            if (_mazeData == null)
            {
                Debug.LogError("[MazeBuilder8] SpawnTorches: _mazeData is NULL!");
                return;
            }

            EnsureObjectsRoot();

            for (int z = 0; z < _mazeData.Height; z++)
            for (int x = 0; x < _mazeData.Width;  x++)
                if ((_mazeData.GetCell(x, z) & Advanced.CellFlags8.HasTorch) != 0)
                    PlaceAtCell(x, z, torchPrefab, $"Torch_{x}_{z}", _objectsRoot);
        }

        // Chests + Enemies
        private void SpawnObjects()
        {
            if (_mazeData == null)
            {
                Debug.LogError("[MazeBuilder8] SpawnObjects: _mazeData is NULL!");
                return;
            }

            EnsureObjectsRoot();

            for (int z = 0; z < _mazeData.Height; z++)
            for (int x = 0; x < _mazeData.Width;  x++)
            {
                var cell = _mazeData.GetCell(x, z);

                // Skip spawn cell and exit cell for enemy/chest spawning
                bool isSpawnCell = (x == _mazeData.SpawnCell.x && z == _mazeData.SpawnCell.z);
                bool isExitCell = (x == _mazeData.ExitCell.x && z == _mazeData.ExitCell.z);

                // Chests - skip spawn/exit rooms (reserve for player safety)
                if ((cell & Advanced.CellFlags8.HasChest) != 0 && chestPrefab != null)
                {
                    if (!isSpawnCell && !isExitCell)
                        PlaceAtCell(x, z, chestPrefab, $"Chest_{x}_{z}", _objectsRoot);
                }

                // Enemies - NEVER spawn at player spawn or exit (critical bug fix)
                if ((cell & Advanced.CellFlags8.HasEnemy) != 0 && enemyPrefab != null)
                {
                    if (!isSpawnCell && !isExitCell)
                        PlaceAtCell(x, z, enemyPrefab, $"Enemy_{x}_{z}", _objectsRoot);
                    else
                        Debug.LogWarning($"[MazeBuilder8] Enemy flag at spawn/exit cell ({x},{z}) - skipped to prevent player damage");
                }
            }
        }

        // -------------------------------------------------------------------------
        // 11b - Visual Markers (spawn/exit room indicators)
        // -------------------------------------------------------------------------
        private void SpawnRoomMarkers()
        {
            if (_mazeData == null || _config == null)
            {
                Debug.LogError("[MazeBuilder8] SpawnRoomMarkers: _mazeData or _config is NULL!");
                return;
            }

            EnsureObjectsRoot();

            // Spawn room marker - Enhanced green cylinder with particles and ring
            Vector3 spawnPos = CellCenter(_mazeData.SpawnCell.x, _mazeData.SpawnCell.z, 0.5f);
            SpawnEnhancedMarker(spawnPos, Color.green, "Marker_SpawnRoom", isEntrance: true);

            // Exit room marker - Enhanced red cube with particles and ring
            Vector3 exitPos = CellCenter(_mazeData.ExitCell.x, _mazeData.ExitCell.z, 0.5f);
            SpawnEnhancedMarker(exitPos, Color.red, "Marker_ExitRoom", isEntrance: false);

            Debug.Log($"[MazeBuilder8] Room markers spawned: Spawn@{_mazeData.SpawnCell} Exit@{_mazeData.ExitCell}");
        }

        private void SpawnEnhancedMarker(Vector3 position, Color color, string name, bool isEntrance)
        {
            float cs = _config.CellSize;

            // Create 8-bit pixel art style marker (cylinder with pixelated texture)
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = name;
            marker.transform.position = position;
            marker.transform.localScale = new Vector3(cs * 0.3f, 2f, cs * 0.3f);

            // Create 8-bit pixel art texture for marker
            Texture2D pixelTex = Create8BitMarkerTexture(color, isEntrance);
            
            // Create emissive material with pixel art texture
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (mat == null) mat = new Material(Shader.Find("Unlit/Color"));

            mat.mainTexture = pixelTex;
            mat.color = new Color(color.r, color.g, color.b, 1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 3f);

            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = mat;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            // Add point light for glow effect
            Light glowLight = marker.AddComponent<Light>();
            if (glowLight != null)
            {
                glowLight.color = color;
                glowLight.intensity = 2.5f;
                glowLight.range = cs * 2f;
                glowLight.shadows = LightShadows.None;
                glowLight.bounceIntensity = 1.5f;
            }

            // Add floating ring effect around marker
            SpawnFloatingRing(position, color, isEntrance);

            if (_objectsRoot != null)
                marker.transform.SetParent(_objectsRoot);
        }

        private void SpawnFloatingRing(Vector3 position, Color color, bool isEntrance)
        {
            // Create rotating ring around marker for extra visual flair
            GameObject ringObj = new GameObject(isEntrance ? "EntranceRing" : "ExitRing");
            ringObj.transform.position = position + Vector3.up * 1f;
            ringObj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            // Create ring mesh (torus approximation)
            Mesh ringMesh = CreateRingMesh(0.6f, 0.05f, 32);
            ringObj.AddComponent<MeshFilter>().mesh = ringMesh;

            // Emissive material for ring
            Material ringMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (ringMat != null)
            {
                ringMat.color = color;
                ringMat.EnableKeyword("_EMISSION");
                ringMat.SetColor("_EmissionColor", color * 2f);
                
                var ringRenderer = ringObj.GetComponent<Renderer>();
                if (ringRenderer != null)
                    ringRenderer.material = ringMat;
            }

            // Add rotation animation component
            RingRotator rotator = ringObj.AddComponent<RingRotator>();
            rotator.rotationSpeed = isEntrance ? 30f : -20f;

            if (_objectsRoot != null)
                ringObj.transform.SetParent(_objectsRoot);
        }

        private Mesh CreateRingMesh(float radius, float thickness, int segments)
        {
            Mesh mesh = new Mesh();
            mesh.name = "RingMesh";

            Vector3[] vertices = new Vector3[segments * 4];
            int[] triangles = new int[segments * 6];
            Vector3[] normals = new Vector3[segments * 4];

            float angleStep = 360f / segments;
            float halfThickness = thickness * 0.5f;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 p1 = new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
                Vector3 p2 = new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);

                int vIdx = i * 4;
                vertices[vIdx] = p1 + Vector3.up * halfThickness;
                vertices[vIdx + 1] = p1 + Vector3.down * halfThickness;
                vertices[vIdx + 2] = p2 + Vector3.up * halfThickness;
                vertices[vIdx + 3] = p2 + Vector3.down * halfThickness;

                normals[vIdx] = Vector3.up;
                normals[vIdx + 1] = Vector3.up;
                normals[vIdx + 2] = Vector3.up;
                normals[vIdx + 3] = Vector3.up;

                int tIdx = i * 6;
                triangles[tIdx] = vIdx;
                triangles[tIdx + 1] = vIdx + 2;
                triangles[tIdx + 2] = vIdx + 1;
                triangles[tIdx + 3] = vIdx + 2;
                triangles[tIdx + 4] = vIdx + 3;
                triangles[tIdx + 5] = vIdx + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.RecalculateBounds();

            return mesh;
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
