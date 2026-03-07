// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details

using System.Collections;
using UnityEngine;

namespace LavosTrial.Core.Maze
{
    // ─────────────────────────────────────────────────────────────
    //  CompleteMazeBuilder  —  Main game orchestrator
    //
    //  Attach to a single GameObject in the scene.
    //  Calls GenerateMaze() on Start or via Editor Tools.
    //
    //  Generation order (Plug-in-Out — FIND, never AddComponent):
    //    1.  Config         → Load GameConfig JSON
    //    2.  Assets         → Resolve prefab references
    //    3.  Components     → FindFirstObjectByType (never create)
    //    4.  Cleanup        → Destroy previous maze objects
    //    5.  Ground         → Spawn floor plane
    //    6.  Spawn Room     → Guaranteed 5×5 cleared first
    //    7.  Corridors      → A* + DFS carved in MazeData
    //    8.  Walls          → Instantiate oriented wall prefabs
    //    9.  Doors          → Entry + exit
    //   10.  Torches        → 30% wall-adjacent cells
    //   11.  Save           → Binary storage (.lvm)
    //   12.  Player         → Spawn LAST (FPS camera)
    // ─────────────────────────────────────────────────────────────
    public sealed class CompleteMazeBuilder : MonoBehaviour
    {
        // ── Inspector References (assign in Editor) ───────────────
        [Header("Prefabs")]
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject doorPrefab;
        [SerializeField] private GameObject torchPrefab;
        [SerializeField] private GameObject chestPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject playerPrefab;

        [Header("Config")]
        [SerializeField] private string configPath = "Config/GameConfig-default";

        [Header("State (read-only in inspector)")]
        [SerializeField] private int currentLevel;
        [SerializeField] private int currentSeed;
        [SerializeField] private float lastGenMs;

        // ── Runtime data ──────────────────────────────────────────
        private MazeData         _mazeData;
        private GameConfig        _config;
        private MazeConfig        _mazeCfg;
        private GridMazeGenerator _generator;

        // Parent containers for scene organisation
        private Transform _wallsRoot;
        private Transform _objectsRoot;

        // ── Cached player ─────────────────────────────────────────
        private GameObject _playerInstance;

        // ─────────────────────────────────────────────────────────
        //  Unity lifecycle
        // ─────────────────────────────────────────────────────────
        private void Start()
        {
            // Seed: new random seed on every scene load
            currentSeed = Random.Range(int.MinValue, int.MaxValue);
            GenerateMaze();
        }

        // ─────────────────────────────────────────────────────────
        //  PUBLIC API
        // ─────────────────────────────────────────────────────────

        /// <summary>Full maze generation pipeline (synchronous).</summary>
        [ContextMenu("Generate Maze")]
        public void GenerateMaze()
        {
            float t0 = Time.realtimeSinceStartup;

            // ── 1. Config ────────────────────────────────────────
            LoadConfig();

            // ── 2 & 3. Assets + Components ───────────────────────
            ValidateAssets();

            // ── 4. Cleanup ───────────────────────────────────────
            DestroyMazeObjects();

            // ── 5–7. Data generation (DFS + A*) ──────────────────
            //   Check cache first — skip regen if .lvm exists
            if (MazeBinaryStorage.Exists(currentLevel, currentSeed))
            {
                Debug.Log($"[MazeBuilder] Cache hit  L{currentLevel} S{currentSeed} — loading binary.");
                _mazeData = MazeBinaryStorage.Load(currentLevel, currentSeed);
            }
            else
            {
                Debug.Log($"[MazeBuilder] Generating LEVEL {currentLevel} " +
                          $"seed={currentSeed}  " +
                          $"size={(12 + currentLevel)}×{(12 + currentLevel)}");
                _generator ??= new GridMazeGenerator();
                _mazeData = _generator.Generate(currentSeed, currentLevel, _mazeCfg);
            }

            // ── 8. Walls ─────────────────────────────────────────
            SpawnWalls();

            // ── 9. Doors ─────────────────────────────────────────
            SpawnDoors();

            // ── 10. Torches ───────────────────────────────────────
            SpawnTorches();

            // ── 5. Ground ─────────────────────────────────────────
            SpawnGround();

            // ── 6. Chests + Enemies ───────────────────────────────
            SpawnObjects();

            // ── 11. Binary save ───────────────────────────────────
            if (!MazeBinaryStorage.Exists(currentLevel, currentSeed))
                MazeBinaryStorage.Save(_mazeData);

            // ── 12. Player (LAST) ─────────────────────────────────
            SpawnPlayer();

            lastGenMs = (Time.realtimeSinceStartup - t0) * 1000f;
            Debug.Log($"[MazeBuilder] ✅ Done — {lastGenMs:F2} ms  " +
                      $"({_config.CellSize}u cells, {_mazeData.Width}×{_mazeData.Height})");
        }

        /// <summary>Advance to the next, harder level.</summary>
        [ContextMenu("Next Level (Harder)")]
        public void NextLevel()
        {
            currentLevel++;
            currentSeed = Random.Range(int.MinValue, int.MaxValue);
            Debug.Log($"[MazeBuilder] → Level {currentLevel}");
            GenerateMaze();
        }

        // ─────────────────────────────────────────────────────────
        //  Step 1 — Config
        // ─────────────────────────────────────────────────────────
        private void LoadConfig()
        {
            // Plug-in-Out: find GameConfig component in scene first
            var cfgComponent = FindFirstObjectByType<GameConfig>();
            if (cfgComponent != null)
            {
                _config  = cfgComponent;
                _mazeCfg = _config.MazeCfg;
                return;
            }

            // Fallback: load from Resources JSON
            var json = Resources.Load<TextAsset>(configPath);
            if (json != null)
            {
                _config  = JsonUtility.FromJson<GameConfig>(json.text);
                _mazeCfg = _config?.MazeCfg ?? new MazeConfig();
                Debug.Log("[MazeBuilder] Config loaded from JSON.");
            }
            else
            {
                Debug.LogWarning("[MazeBuilder] No config found — using defaults.");
                _config  = new GameConfig();
                _mazeCfg = new MazeConfig();
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 2+3 — Asset + Component validation
        // ─────────────────────────────────────────────────────────
        private void ValidateAssets()
        {
            // Try finding prefabs in Resources if not set in Inspector
            wallPrefab   ??= Resources.Load<GameObject>("Prefabs/WallPrefab");
            doorPrefab   ??= Resources.Load<GameObject>("Prefabs/DoorPrefab");
            torchPrefab  ??= Resources.Load<GameObject>("Prefabs/TorchHandlePrefab");
            chestPrefab  ??= Resources.Load<GameObject>("Prefabs/ChestPrefab");
            enemyPrefab  ??= Resources.Load<GameObject>("Prefabs/EnemyPrefab");
            floorPrefab  ??= Resources.Load<GameObject>("Prefabs/FloorPrefab");
            playerPrefab ??= Resources.Load<GameObject>("Prefabs/PlayerPrefab");

            if (wallPrefab  == null) Debug.LogError("[MazeBuilder] wallPrefab missing!");
            if (playerPrefab == null) Debug.LogError("[MazeBuilder] playerPrefab missing!");
        }

        // ─────────────────────────────────────────────────────────
        //  Step 4 — Cleanup
        // ─────────────────────────────────────────────────────────
        private void DestroyMazeObjects()
        {
            // Destroy organised containers
            DestroyContainer(ref _wallsRoot,   "MazeWalls");
            DestroyContainer(ref _objectsRoot, "MazeObjects");

            // Destroy previous player
            if (_playerInstance != null)
            {
                Destroy(_playerInstance);
                _playerInstance = null;
            }
        }

        private static void DestroyContainer(ref Transform container, string name)
        {
            if (container != null) { Destroy(container.gameObject); container = null; return; }
            var existing = GameObject.Find(name);
            if (existing != null) Destroy(existing);
        }

        // ─────────────────────────────────────────────────────────
        //  Step 5 — Ground
        // ─────────────────────────────────────────────────────────
        private void SpawnGround()
        {
            if (floorPrefab == null) return;
            float size   = _mazeData.Width * _config.CellSize;
            float halfSz = size * 0.5f;

            var floor = Instantiate(floorPrefab,
                            new Vector3(halfSz, 0f, halfSz),
                            Quaternion.identity);
            floor.name = "MazeFloor";
            floor.transform.localScale = new Vector3(size, 1f, size);
        }

        // ─────────────────────────────────────────────────────────
        //  Step 8 — Walls
        // ─────────────────────────────────────────────────────────
        private void SpawnWalls()
        {
            if (wallPrefab == null) return;

            _wallsRoot = new GameObject("MazeWalls").transform;
            float cs   = _config.CellSize;
            float wh   = _config.WallHeight;

            for (int z = 0; z < _mazeData.Height; z++)
            for (int x = 0; x < _mazeData.Width;  x++)
            {
                var cell = _mazeData.GetCell(x, z);

                // North wall edge
                if ((cell & CellFlags.WallN) != 0)
                    SpawnWall(x, z, Direction.North, cs, wh);

                // East wall edge
                if ((cell & CellFlags.WallE) != 0)
                    SpawnWall(x, z, Direction.East, cs, wh);

                // Only spawn South/West on border to avoid doubles
                if (z == 0 && (cell & CellFlags.WallS) != 0)
                    SpawnWall(x, z, Direction.South, cs, wh);

                if (x == 0 && (cell & CellFlags.WallW) != 0)
                    SpawnWall(x, z, Direction.West, cs, wh);
            }
        }

        private void SpawnWall(int cx, int cz, Direction dir, float cs, float wh)
        {
            // Compute world position at the edge centre of the cell
            Vector3 basePos = new Vector3(cx * cs + cs * 0.5f, wh * 0.5f, cz * cs + cs * 0.5f);
            Quaternion rot  = Quaternion.identity;
            Vector3 offset  = Vector3.zero;

            switch (dir)
            {
                case Direction.North:
                    offset = new Vector3(0f, 0f,  cs * 0.5f);
                    rot    = Quaternion.identity;
                    break;
                case Direction.South:
                    offset = new Vector3(0f, 0f, -cs * 0.5f);
                    rot    = Quaternion.identity;
                    break;
                case Direction.East:
                    offset = new Vector3( cs * 0.5f, 0f, 0f);
                    rot    = Quaternion.Euler(0f, 90f, 0f);
                    break;
                case Direction.West:
                    offset = new Vector3(-cs * 0.5f, 0f, 0f);
                    rot    = Quaternion.Euler(0f, 90f, 0f);
                    break;
            }

            var wall = Instantiate(wallPrefab, basePos + offset, rot, _wallsRoot);
            wall.transform.localScale = new Vector3(cs, wh, 0.2f);
            wall.name = $"Wall_{cx}_{cz}_{dir}";
        }

        // ─────────────────────────────────────────────────────────
        //  Step 9 — Doors (entry + exit markers)
        // ─────────────────────────────────────────────────────────
        private void SpawnDoors()
        {
            if (doorPrefab == null) return;
            float cs = _config.CellSize;

            SpawnAtCell(_mazeData.SpawnCell.x, _mazeData.SpawnCell.z,
                        doorPrefab, "Door_Entry");
            SpawnAtCell(_mazeData.ExitCell.x, _mazeData.ExitCell.z,
                        doorPrefab, "Door_Exit");
        }

        // ─────────────────────────────────────────────────────────
        //  Step 10 — Torches
        // ─────────────────────────────────────────────────────────
        private void SpawnTorches()
        {
            if (torchPrefab == null) return;
            EnsureObjectsRoot();

            for (int z = 0; z < _mazeData.Height; z++)
            for (int x = 0; x < _mazeData.Width;  x++)
                if ((_mazeData.GetCell(x, z) & CellFlags.HasTorch) != 0)
                    SpawnAtCell(x, z, torchPrefab, $"Torch_{x}_{z}", _objectsRoot);
        }

        // ─────────────────────────────────────────────────────────
        //  Step 6-ish — Chests + Enemies
        // ─────────────────────────────────────────────────────────
        private void SpawnObjects()
        {
            EnsureObjectsRoot();

            for (int z = 0; z < _mazeData.Height; z++)
            for (int x = 0; x < _mazeData.Width;  x++)
            {
                var cell = _mazeData.GetCell(x, z);

                if ((cell & CellFlags.HasChest) != 0 && chestPrefab != null)
                    SpawnAtCell(x, z, chestPrefab, $"Chest_{x}_{z}", _objectsRoot);

                if ((cell & CellFlags.HasEnemy) != 0 && enemyPrefab != null)
                    SpawnAtCell(x, z, enemyPrefab, $"Enemy_{x}_{z}", _objectsRoot);
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 12 — Player  (ALWAYS LAST)
        // ─────────────────────────────────────────────────────────
        private void SpawnPlayer()
        {
            // Plug-in-Out: find existing player first
            var existing = FindFirstObjectByType<PlayerController>();
            if (existing != null)
            {
                _playerInstance = existing.gameObject;
                TeleportPlayer(_playerInstance);
                return;
            }

            if (playerPrefab == null)
            {
                Debug.LogWarning("[MazeBuilder] No playerPrefab — skipping player spawn.");
                return;
            }

            Vector3 spawnPos = CellToWorld(
                _mazeData.SpawnCell.x, _mazeData.SpawnCell.z,
                _config.PlayerEyeHeight);

            _playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            _playerInstance.name = "Player";

            Debug.Log($"[MazeBuilder] Player spawned INSIDE maze at {spawnPos}");
        }

        private void TeleportPlayer(GameObject player)
        {
            player.transform.position = CellToWorld(
                _mazeData.SpawnCell.x, _mazeData.SpawnCell.z,
                _config.PlayerEyeHeight);
        }

        // ─────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────
        private Vector3 CellToWorld(int x, int z, float yOffset = 0f)
        {
            float cs = _config.CellSize;
            return new Vector3(x * cs + cs * 0.5f, yOffset, z * cs + cs * 0.5f);
        }

        private void SpawnAtCell(int x, int z, GameObject prefab,
                                  string objName, Transform parent = null)
        {
            float cs  = _config.CellSize;
            var pos   = new Vector3(x * cs + cs * 0.5f, 0f, z * cs + cs * 0.5f);
            var obj   = Instantiate(prefab, pos, Quaternion.identity, parent);
            obj.name  = objName;
        }

        private void EnsureObjectsRoot()
        {
            if (_objectsRoot == null)
                _objectsRoot = new GameObject("MazeObjects").transform;
        }

        // ─────────────────────────────────────────────────────────
        //  Console command access (called by MazeConsoleCommands)
        // ─────────────────────────────────────────────────────────
        public string StatusString()
            => $"Level {currentLevel} | Size {_mazeData?.Width}×{_mazeData?.Height} " +
               $"| Seed {currentSeed} | Gen {lastGenMs:F2}ms";

        public MazeData CurrentMazeData => _mazeData;
        public int CurrentLevel         => currentLevel;
        public int CurrentSeed          => currentSeed;
    }
}
