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
// CompleteMazeBuilder.cs
// 8-axis maze orchestrator - Plug-in-Out compliant
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;

namespace Code.Lavos.Core
{
    // ─────────────────────────────────────────────────────────────
    //  CompleteMazeBuilder  —  8-axis maze orchestrator
    //
    //  Attach to a single GameObject in the scene.
    //
    //  Generation pipeline (Plug-in-Out — FIND, never AddComponent):
    //    1.  Config         → Load GameConfig JSON
    //    2.  Assets         → Resolve prefab references
    //    3.  Components     → FindFirstObjectByType (never create)
    //    4.  Cleanup        → Destroy previous maze objects
    //    5.  Ground         → Spawn floor plane
    //    6.  Spawn Room     → Guaranteed 5×5 cleared first
    //    7.  Corridors      → 8-axis DFS + A* in MazeData8
    //    8.  Walls          → Cardinal + diagonal wall prefabs
    //    9.  Doors          → Entry + exit markers
    //   10.  Torches        → 30% wall-adjacent cells
    //   11.  Save           → Binary storage (.lvm)
    //   12.  Player         → Spawn LAST (FPS camera)
    // ─────────────────────────────────────────────────────────────
    public sealed class CompleteMazeBuilder : MonoBehaviour
    {
        // ── Inspector references ──────────────────────────────────
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

        [Header("State (read-only)")]
        [SerializeField] private int currentLevel;
        [SerializeField] private int currentSeed;
        [SerializeField] private float lastGenMs;

        // ── Runtime ───────────────────────────────────────────────
        private MazeData8          _mazeData;
        private GameConfig         _config;
        private GridMazeGenerator  _generator;
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

            // 1. Config
            LoadConfig();

            // 2+3. Assets + Components
            ValidateAssets();

            // 4. Cleanup
            DestroyMazeObjects();

            // 5-7. Data — cache-first
            if (MazeBinaryStorage8.Exists(currentLevel, currentSeed))
            {
                Debug.Log($"[MazeBuilder] Cache hit L{currentLevel} S{currentSeed}");
                _mazeData = MazeBinaryStorage8.Load(currentLevel, currentSeed);
            }
            else
            {
                int sz = Mathf.Clamp(_config.MazeCfg.BaseSize + currentLevel,
                                     _config.MazeCfg.MinSize,
                                     _config.MazeCfg.MaxSize);
                Debug.Log($"[MazeBuilder] Generating LEVEL {currentLevel} " +
                          $"seed={currentSeed}  size={sz}×{sz}  (8-axis)");

                _generator ??= new GridMazeGenerator();
                _mazeData   = _generator.Generate(currentSeed, currentLevel, _config.MazeCfg);
            }

            // 5. Ground
            SpawnGround();

            // 8. Walls (cardinal + diagonal)
            SpawnAllWalls();

            // 9. Doors
            SpawnDoors();

            // 10. Torches
            SpawnTorches();

            // Chests + Enemies
            SpawnObjects();

            // 11. Binary save
            if (!MazeBinaryStorage8.Exists(currentLevel, currentSeed))
                MazeBinaryStorage8.Save(_mazeData);

            // 12. Player — ALWAYS LAST
            SpawnPlayer();

            lastGenMs = (Time.realtimeSinceStartup - t0) * 1000f;
            Debug.Log($"[MazeBuilder] Done — {lastGenMs:F2} ms");
        }

        [ContextMenu("Next Level (Harder)")]
        public void NextLevel()
        {
            currentLevel++;
            currentSeed = Random.Range(int.MinValue, int.MaxValue);
            Debug.Log($"[MazeBuilder] Level {currentLevel}");
            GenerateMaze();
        }

        // ─────────────────────────────────────────────────────────
        //  Step 1 — Config
        // ─────────────────────────────────────────────────────────
        private void LoadConfig()
        {
            // Plug-in-Out: scene component first
            var comp = FindFirstObjectByType<GameConfig>();
            if (comp != null) { _config = comp; return; }

            // Fallback: load from JSON
            string jsonPath = configResourcePath;
            TextAsset jsonAsset = Resources.Load<TextAsset>(jsonPath.Replace("Assets/Resources/", "").Replace(".json", ""));
            
            if (jsonAsset != null)
            {
                _config = GameConfig.FromJson(jsonAsset.text);
                Debug.Log($"[MazeBuilder] Config loaded from {jsonPath}");
            }
            else
            {
                _config = new GameConfig();
                Debug.LogWarning("[MazeBuilder] Config not found, using defaults");
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 2 — Validate assets
        // ─────────────────────────────────────────────────────────
        private void ValidateAssets()
        {
            if (wallPrefab == null)
                Debug.LogError("[MazeBuilder] CRITICAL: wallPrefab not assigned!");
            if (torchPrefab == null)
                Debug.LogWarning("[MazeBuilder] torchPrefab not assigned - no torches");
        }

        // ─────────────────────────────────────────────────────────
        //  Step 4 — Destroy old maze
        // ─────────────────────────────────────────────────────────
        private void DestroyMazeObjects()
        {
            var existingWalls = GameObject.Find("MazeWalls");
            if (existingWalls != null)
            {
                if (Application.isPlaying)
                    Destroy(existingWalls);
                else
                    DestroyImmediate(existingWalls);
            }

            var existingObjects = GameObject.Find("MazeObjects");
            if (existingObjects != null)
            {
                if (Application.isPlaying)
                    Destroy(existingObjects);
                else
                    DestroyImmediate(existingObjects);
            }

            if (_playerInstance != null)
            {
                if (Application.isPlaying)
                    Destroy(_playerInstance);
                else
                    DestroyImmediate(_playerInstance);
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 5 — Ground
        // ─────────────────────────────────────────────────────────
        private void SpawnGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ground.name = "GroundFloor";

            float size = _mazeData.Width * _config.CellSize;
            ground.transform.position = new Vector3(size / 2f, 0f, size / 2f);
            ground.transform.localScale = new Vector3(size, 1f, size);

            var renderer = ground.GetComponent<MeshRenderer>();
            if (renderer != null && floorPrefab != null)
            {
                var floorMat = floorPrefab.GetComponent<MeshRenderer>()?.sharedMaterial;
                if (floorMat != null)
                    renderer.sharedMaterial = floorMat;
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 8 — All walls (cardinal + diagonal)
        // ─────────────────────────────────────────────────────────
        private void SpawnAllWalls()
        {
            _wallsRoot = new GameObject("MazeWalls").transform;

            for (int x = 0; x < _mazeData.Width; x++)
            {
                for (int z = 0; z < _mazeData.Height; z++)
                {
                    var cell = _mazeData.GetCell(x, z);

                    // Skip if all walls (boundary)
                    if ((cell & CellFlags8.AllWalls) == CellFlags8.AllWalls)
                        continue;

                    float wx = x * _config.CellSize;
                    float wz = z * _config.CellSize;

                    // Check each of 8 directions for walls
                    SpawnWallIfPresent(x, z, wx, wz, Direction8.N, cell);
                    SpawnWallIfPresent(x, z, wx, wz, Direction8.E, cell);
                    SpawnWallIfPresent(x, z, wx, wz, Direction8.S, cell);
                    SpawnWallIfPresent(x, z, wx, wz, Direction8.W, cell);

                    // Diagonal walls
                    if (_config.MazeCfg.DiagonalWalls)
                    {
                        SpawnWallIfPresent(x, z, wx, wz, Direction8.NE, cell);
                        SpawnWallIfPresent(x, z, wx, wz, Direction8.NW, cell);
                        SpawnWallIfPresent(x, z, wx, wz, Direction8.SE, cell);
                        SpawnWallIfPresent(x, z, wx, wz, Direction8.SW, cell);
                    }
                }
            }
        }

        private void SpawnWallIfPresent(int x, int z, float wx, float wz, Direction8 dir, CellFlags8 cell)
        {
            if ((cell & Direction8Helper.ToWallFlag(dir)) == 0)
                return;

            var (dx, dz) = Direction8Helper.ToOffset(dir);
            bool isDiagonal = Direction8Helper.IsDiagonal(dir);

            GameObject prefab = isDiagonal ? wallDiagPrefab : wallPrefab;
            if (prefab == null) return;

            // Position: center of wall edge
            float wallX = wx + (dx > 0 ? _config.CellSize / 2 : (dx < 0 ? -_config.CellSize / 2 : 0));
            float wallZ = wz + (dz > 0 ? _config.CellSize / 2 : (dz < 0 ? -_config.CellSize / 2 : 0));

            // Rotation for diagonal walls
            Quaternion rotation = Quaternion.identity;
            if (isDiagonal)
            {
                float angle = dir switch
                {
                    Direction8.NE => 45f,
                    Direction8.NW => -45f,
                    Direction8.SE => -45f,
                    Direction8.SW => 45f,
                    _ => 0f
                };
                rotation = Quaternion.Euler(0f, angle, 0f);
            }
            else
            {
                // Cardinal walls
                float angle = dir switch
                {
                    Direction8.N => 0f,
                    Direction8.E => 90f,
                    Direction8.S => 180f,
                    Direction8.W => 270f,
                    _ => 0f
                };
                rotation = Quaternion.Euler(0f, angle, 0f);
            }

            GameObject wall = Instantiate(prefab, new Vector3(wallX, _config.WallHeight / 2, wallZ), rotation);
            wall.transform.SetParent(_wallsRoot);
        }

        // ─────────────────────────────────────────────────────────
        //  Step 9 — Doors
        // ─────────────────────────────────────────────────────────
        private void SpawnDoors()
        {
            if (doorPrefab == null) return;

            // Exit door at exit cell
            var exit = _mazeData.ExitCell;
            float exitX = exit.x * _config.CellSize + _config.CellSize / 2;
            float exitZ = exit.z * _config.CellSize + _config.CellSize / 2;

            GameObject door = Instantiate(doorPrefab, new Vector3(exitX, _config.WallHeight / 2, exitZ), Quaternion.identity);
            door.name = "ExitDoor";
            door.transform.SetParent(_wallsRoot);
        }

        // ─────────────────────────────────────────────────────────
        //  Step 10 — Torches
        // ─────────────────────────────────────────────────────────
        private void SpawnTorches()
        {
            if (torchPrefab == null) return;

            _objectsRoot ??= new GameObject("MazeObjects").transform;

            for (int x = 0; x < _mazeData.Width; x++)
            {
                for (int z = 0; z < _mazeData.Height; z++)
                {
                    var cell = _mazeData.GetCell(x, z);
                    if ((cell & CellFlags8.HasTorch) == 0) continue;

                    float tx = x * _config.CellSize + _config.CellSize / 2;
                    float tz = z * _config.CellSize + _config.CellSize / 2;

                    GameObject torch = Instantiate(torchPrefab, new Vector3(tx, _config.WallHeight * 0.75f, tz), Quaternion.identity);
                    torch.name = $"Torch_{x}_{z}";
                    torch.transform.SetParent(_objectsRoot);
                }
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Objects — Chests + Enemies
        // ─────────────────────────────────────────────────────────
        private void SpawnObjects()
        {
            _objectsRoot ??= new GameObject("MazeObjects").transform;

            for (int x = 0; x < _mazeData.Width; x++)
            {
                for (int z = 0; z < _mazeData.Height; z++)
                {
                    var cell = _mazeData.GetCell(x, z);
                    float ox = x * _config.CellSize + _config.CellSize / 2;
                    float oz = z * _config.CellSize + _config.CellSize / 2;

                    if ((cell & CellFlags8.HasChest) != 0 && chestPrefab != null)
                    {
                        GameObject chest = Instantiate(chestPrefab, new Vector3(ox, 0.5f, oz), Quaternion.identity);
                        chest.name = $"Chest_{x}_{z}";
                        chest.transform.SetParent(_objectsRoot);
                    }

                    if ((cell & CellFlags8.HasEnemy) != 0 && enemyPrefab != null)
                    {
                        GameObject enemy = Instantiate(enemyPrefab, new Vector3(ox, 1f, oz), Quaternion.identity);
                        enemy.name = $"Enemy_{x}_{z}";
                        enemy.transform.SetParent(_objectsRoot);
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 12 — Player
        // ─────────────────────────────────────────────────────────
        private void SpawnPlayer()
        {
            if (playerPrefab == null)
            {
                Debug.LogWarning("[MazeBuilder] playerPrefab not assigned - creating default");
                // Create simple player capsule
                GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Player";
                player.transform.position = new Vector3(
                    _mazeData.SpawnCell.x * _config.CellSize + _config.CellSize / 2,
                    1f,
                    _mazeData.SpawnCell.z * _config.CellSize + _config.CellSize / 2
                );
                _playerInstance = player;
            }
            else
            {
                _playerInstance = Instantiate(playerPrefab, new Vector3(
                    _mazeData.SpawnCell.x * _config.CellSize + _config.CellSize / 2,
                    0f,
                    _mazeData.SpawnCell.z * _config.CellSize + _config.CellSize / 2
                ), Quaternion.identity);
            }

            Debug.Log($"[MazeBuilder] Player spawned at {_mazeData.SpawnCell}");
        }
    }
}
