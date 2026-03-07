// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using UnityEngine;

namespace Code.Lavos.Core
{
    // ─────────────────────────────────────────────────────────────
    //  CompleteMazeBuilder8  —  8-axis maze orchestrator
    //
    //  Attach to a single GameObject in the scene.
    //  Plug-in-Out: FindFirstObjectByType everywhere, never AddComponent.
    //
    //  Generation pipeline:
    //    1.  Config         → Load GameConfig8 from scene or JSON
    //    2.  Assets         → Resolve prefab references
    //    3.  Components     → Find (never create)
    //    4.  Cleanup        → Destroy previous maze objects
    //    5.  Ground         → Spawn floor plane
    //    6.  Spawn Room     → Guaranteed 5×5 cleared first
    //    7.  Corridors      → 8-axis DFS + A* (difficulty-scaled)
    //    8.  Walls          → Cardinal + diagonal wall prefabs
    //    9.  Doors          → Entry + exit markers
    //   10.  Torches        → Scaled torch placement
    //   11.  Save           → Binary .lvm  →  Runtimes/Mazes/
    //   12.  Player         → Spawn LAST
    // ─────────────────────────────────────────────────────────────
    public sealed class CompleteMazeBuilder8 : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────
        [Header("Cardinal Prefabs")]
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject doorPrefab;

        [Header("Diagonal Prefabs")]
        [SerializeField] private GameObject wallDiagPrefab;
        [SerializeField] private GameObject wallCornerPrefab;

        [Header("Object Prefabs")]
        [SerializeField] private GameObject torchPrefab;
        [SerializeField] private GameObject chestPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject playerPrefab;

        [Header("Config")]
        [SerializeField] private string configResourcePath = "Config/GameConfig8-default";

        [Header("State  (read-only)")]
        [SerializeField] private int   currentLevel;
        [SerializeField] private int   currentSeed;
        [SerializeField] private float lastGenMs;
        [SerializeField] private float currentDifficultyFactor;

        // ── Runtime ───────────────────────────────────────────────
        private MazeData8          _mazeData;
        private GameConfig8        _config;
        private GridMazeGenerator8 _generator;
        private Transform          _wallsRoot;
        private Transform          _objectsRoot;
        private GameObject         _playerInstance;

        // ─────────────────────────────────────────────────────────
        //  Unity lifecycle
        // ─────────────────────────────────────────────────────────
        private void Start()
        {
            currentSeed = Random.Range(int.MinValue, int.MaxValue);
            GenerateMaze();
        }

        // ─────────────────────────────────────────────────────────
        //  PUBLIC API
        // ─────────────────────────────────────────────────────────

        [ContextMenu("Generate Maze (8-axis)")]
        public void GenerateMaze()
        {
            float t0 = Time.realtimeSinceStartup;

            // 1 — Config
            LoadConfig();

            // 2+3 — Assets + Components
            ValidateAssets();

            // 4 — Cleanup
            DestroyMazeObjects();

            // 5-7 — Data (cache first)
            if (MazeBinaryStorage8.Exists(currentLevel, currentSeed))
            {
                Debug.Log($"[MazeBuilder8] Cache hit  L{currentLevel}  S{currentSeed}");
                _mazeData = MazeBinaryStorage8.Load(currentLevel, currentSeed);
            }
            else
            {
                _generator ??= new GridMazeGenerator8();
                _mazeData   = _generator.Generate(currentSeed, currentLevel, _config.MazeCfg);
            }

            currentDifficultyFactor = _mazeData.DifficultyFactor;

            // 5 — Ground
            SpawnGround();

            // 8 — Walls (cardinal + diagonal)
            SpawnAllWalls();

            // 9 — Doors
            SpawnDoors();

            // 10 — Torches
            SpawnTorches();

            // Objects (chests + enemies)
            SpawnObjects();

            // 11 — Binary save
            if (!MazeBinaryStorage8.Exists(currentLevel, currentSeed))
                MazeBinaryStorage8.Save(_mazeData);

            // 12 — Player LAST
            SpawnPlayer();

            lastGenMs = (Time.realtimeSinceStartup - t0) * 1000f;
            Debug.Log($"[MazeBuilder8] ✅ Done — {lastGenMs:F2} ms  " +
                      $"factor={currentDifficultyFactor:F3}");
        }

        [ContextMenu("Next Level (Harder)")]
        public void NextLevel()
        {
            currentLevel++;
            currentSeed = Random.Range(int.MinValue, int.MaxValue);
            Debug.Log($"[MazeBuilder8] → Level {currentLevel}  " +
                      $"expected factor={_config?.MazeCfg.Difficulty.Factor(currentLevel):F3}");
            GenerateMaze();
        }

        // ─────────────────────────────────────────────────────────
        //  1 — Config
        // ─────────────────────────────────────────────────────────
        private void LoadConfig()
        {
            var comp = FindFirstObjectByType<GameConfig8>();
            if (comp != null) { _config = comp; return; }

            var json = Resources.Load<TextAsset>(configResourcePath);
            _config = json != null ? GameConfig8.FromJson(json.text) : new GameConfig8();

            if (json == null)
                Debug.LogWarning("[MazeBuilder8] Config not found — using defaults.");
        }

        // ─────────────────────────────────────────────────────────
        //  2+3 — Asset validation
        // ─────────────────────────────────────────────────────────
        private void ValidateAssets()
        {
            wallPrefab       ??= Resources.Load<GameObject>("Prefabs/WallPrefab");
            wallDiagPrefab   ??= Resources.Load<GameObject>("Prefabs/WallDiagPrefab");
            wallCornerPrefab ??= Resources.Load<GameObject>("Prefabs/WallCornerPrefab");
            doorPrefab       ??= Resources.Load<GameObject>("Prefabs/DoorPrefab");
            torchPrefab      ??= Resources.Load<GameObject>("Prefabs/TorchHandlePrefab");
            chestPrefab      ??= Resources.Load<GameObject>("Prefabs/ChestPrefab");
            enemyPrefab      ??= Resources.Load<GameObject>("Prefabs/EnemyPrefab");
            floorPrefab      ??= Resources.Load<GameObject>("Prefabs/FloorPrefab");
            playerPrefab     ??= Resources.Load<GameObject>("Prefabs/PlayerPrefab");

            if (wallPrefab   == null) Debug.LogError("[MazeBuilder8] wallPrefab missing!");
            if (playerPrefab == null) Debug.LogError("[MazeBuilder8] playerPrefab missing!");

            if (wallDiagPrefab == null)
            {
                wallDiagPrefab = wallPrefab;
                Debug.LogWarning("[MazeBuilder8] wallDiagPrefab not set — reusing wallPrefab at 45°.");
            }
        }

        // ─────────────────────────────────────────────────────────
        //  4 — Cleanup
        // ─────────────────────────────────────────────────────────
        private void DestroyMazeObjects()
        {
            DestroyContainer(ref _wallsRoot,   "MazeWalls8");
            DestroyContainer(ref _objectsRoot, "MazeObjects8");
            if (_playerInstance != null) { Destroy(_playerInstance); _playerInstance = null; }
        }

        private static void DestroyContainer(ref Transform t, string name)
        {
            if (t != null) { Destroy(t.gameObject); t = null; return; }
            var g = GameObject.Find(name);
            if (g != null) Destroy(g);
        }

        // ─────────────────────────────────────────────────────────
        //  5 — Ground
        // ─────────────────────────────────────────────────────────
        private void SpawnGround()
        {
            if (floorPrefab == null) return;
            float sz = _mazeData.Width * _config.CellSize;
            var   go = Instantiate(floorPrefab,
                                   new Vector3(sz * 0.5f, 0f, sz * 0.5f),
                                   Quaternion.identity);
            go.name               = "MazeFloor8";
            go.transform.localScale = new Vector3(sz, 1f, sz);
        }

        // ─────────────────────────────────────────────────────────
        //  8 — Walls (cardinal + diagonal)
        //
        //  Cardinal : placed at cell edge centre, axis-aligned
        //  Diagonal : placed at cell corner, rotated ±45°
        //             NE/SW → +45°  |  NW/SE → -45°
        //             scale X = cellSize × √2
        // ─────────────────────────────────────────────────────────
        private void SpawnAllWalls()
        {
            if (wallPrefab == null) return;
            _wallsRoot = new GameObject("MazeWalls8").transform;

            float cs = _config.CellSize;
            float wh = _config.WallHeight;

            for (int z = 0; z < _mazeData.Height; z++)
            for (int x = 0; x < _mazeData.Width;  x++)
            {
                var cell = _mazeData.GetCell(x, z);

                // Cardinal — North + East always; South/West only on border
                if ((cell & CellFlags8.WallN) != 0)
                    SpawnCardinalWall(x, z, Direction8.N, cs, wh);
                if ((cell & CellFlags8.WallE) != 0)
                    SpawnCardinalWall(x, z, Direction8.E, cs, wh);
                if (z == 0 && (cell & CellFlags8.WallS) != 0)
                    SpawnCardinalWall(x, z, Direction8.S, cs, wh);
                if (x == 0 && (cell & CellFlags8.WallW) != 0)
                    SpawnCardinalWall(x, z, Direction8.W, cs, wh);

                // Diagonal (toggled by config)
                if (_config.MazeCfg.DiagonalWalls)
                {
                    if ((cell & CellFlags8.WallNE) != 0)
                        SpawnDiagonalWall(x, z, Direction8.NE, cs, wh);
                    if ((cell & CellFlags8.WallNW) != 0)
                        SpawnDiagonalWall(x, z, Direction8.NW, cs, wh);
                    if (z == 0 && (cell & CellFlags8.WallSE) != 0)
                        SpawnDiagonalWall(x, z, Direction8.SE, cs, wh);
                    if (x == 0 && (cell & CellFlags8.WallSW) != 0)
                        SpawnDiagonalWall(x, z, Direction8.SW, cs, wh);
                }
            }
        }

        private void SpawnCardinalWall(int cx, int cz, Direction8 dir, float cs, float wh)
        {
            Vector3    center = CellCenter(cx, cz, wh * 0.5f);
            Vector3    offset;
            Quaternion rot;

            switch (dir)
            {
                case Direction8.N: offset = new Vector3(0,         0,  cs * 0.5f); rot = Quaternion.identity;        break;
                case Direction8.S: offset = new Vector3(0,         0, -cs * 0.5f); rot = Quaternion.identity;        break;
                case Direction8.E: offset = new Vector3( cs * 0.5f, 0, 0);         rot = Quaternion.Euler(0, 90f, 0); break;
                case Direction8.W: offset = new Vector3(-cs * 0.5f, 0, 0);         rot = Quaternion.Euler(0, 90f, 0); break;
                default: return;
            }

            var wall = Instantiate(wallPrefab, center + offset, rot, _wallsRoot);
            wall.transform.localScale = new Vector3(cs, wh, 0.2f);
            wall.name = $"Wall_{cx}_{cz}_{dir}";
        }

        private void SpawnDiagonalWall(int cx, int cz, Direction8 dir, float cs, float wh)
        {
            Vector3 center = CellCenter(cx, cz, wh * 0.5f);
            float   h      = cs * 0.5f;
            Vector3 offset;
            float   rotY;

            switch (dir)
            {
                case Direction8.NE: offset = new Vector3( h, 0,  h); rotY =  45f; break;
                case Direction8.NW: offset = new Vector3(-h, 0,  h); rotY = -45f; break;
                case Direction8.SE: offset = new Vector3( h, 0, -h); rotY = -45f; break;
                case Direction8.SW: offset = new Vector3(-h, 0, -h); rotY =  45f; break;
                default: return;
            }

            var wall = Instantiate(wallDiagPrefab,
                                   center + offset,
                                   Quaternion.Euler(0, rotY, 0),
                                   _wallsRoot);
            wall.transform.localScale = new Vector3(cs * 1.414f, wh, 0.2f);
            wall.name = $"WallDiag_{cx}_{cz}_{dir}";
        }

        // ─────────────────────────────────────────────────────────
        //  9 — Doors
        // ─────────────────────────────────────────────────────────
        private void SpawnDoors()
        {
            if (doorPrefab == null) return;
            PlaceAtCell(_mazeData.SpawnCell.x, _mazeData.SpawnCell.z, doorPrefab, "Door_Entry");
            PlaceAtCell(_mazeData.ExitCell.x,  _mazeData.ExitCell.z,  doorPrefab, "Door_Exit");
        }

        // ─────────────────────────────────────────────────────────
        //  10 — Torches
        // ─────────────────────────────────────────────────────────
        private void SpawnTorches()
        {
            if (torchPrefab == null) return;
            EnsureObjectsRoot();
            for (int z = 0; z < _mazeData.Height; z++)
            for (int x = 0; x < _mazeData.Width;  x++)
                if ((_mazeData.GetCell(x, z) & CellFlags8.HasTorch) != 0)
                    PlaceAtCell(x, z, torchPrefab, $"Torch_{x}_{z}", _objectsRoot);
        }

        // ── Chests + Enemies ──────────────────────────────────────
        private void SpawnObjects()
        {
            EnsureObjectsRoot();
            for (int z = 0; z < _mazeData.Height; z++)
            for (int x = 0; x < _mazeData.Width;  x++)
            {
                var cell = _mazeData.GetCell(x, z);
                if ((cell & CellFlags8.HasChest) != 0 && chestPrefab != null)
                    PlaceAtCell(x, z, chestPrefab, $"Chest_{x}_{z}", _objectsRoot);
                if ((cell & CellFlags8.HasEnemy) != 0 && enemyPrefab != null)
                    PlaceAtCell(x, z, enemyPrefab, $"Enemy_{x}_{z}", _objectsRoot);
            }
        }

        // ─────────────────────────────────────────────────────────
        //  12 — Player  (ALWAYS LAST)
        // ─────────────────────────────────────────────────────────
        private void SpawnPlayer()
        {
            var existing = FindFirstObjectByType<PlayerController>();
            if (existing != null)
            {
                _playerInstance = existing.gameObject;
                _playerInstance.transform.position = CellCenter(
                    _mazeData.SpawnCell.x, _mazeData.SpawnCell.z,
                    _config.PlayerEyeHeight);
                return;
            }

            if (playerPrefab == null)
            {
                Debug.LogWarning("[MazeBuilder8] playerPrefab not set — skipping spawn.");
                return;
            }

            var pos = CellCenter(_mazeData.SpawnCell.x, _mazeData.SpawnCell.z,
                                  _config.PlayerEyeHeight);
            _playerInstance      = Instantiate(playerPrefab, pos, Quaternion.identity);
            _playerInstance.name = "Player";
            Debug.Log($"[MazeBuilder8] Player spawned at {pos}");
        }

        // ─────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────
        private Vector3 CellCenter(int x, int z, float y = 0f)
        {
            float cs = _config.CellSize;
            return new Vector3(x * cs + cs * 0.5f, y, z * cs + cs * 0.5f);
        }

        private void PlaceAtCell(int x, int z, GameObject prefab,
                                  string objName, Transform parent = null)
        {
            var go  = Instantiate(prefab, CellCenter(x, z), Quaternion.identity, parent);
            go.name = objName;
        }

        private void EnsureObjectsRoot()
        {
            if (_objectsRoot == null)
                _objectsRoot = new GameObject("MazeObjects8").transform;
        }

        // ── Status ────────────────────────────────────────────────
        public string StatusString()
            => $"Level {currentLevel} | {_mazeData?.Width}×{_mazeData?.Height} | " +
               $"Seed {currentSeed} | factor={currentDifficultyFactor:F3} | {lastGenMs:F2}ms";

        public MazeData8 CurrentMazeData        => _mazeData;
        public int       CurrentLevel           => currentLevel;
        public int       CurrentSeed            => currentSeed;
        public float     CurrentDifficultyFactor=> currentDifficultyFactor;
    }
}
