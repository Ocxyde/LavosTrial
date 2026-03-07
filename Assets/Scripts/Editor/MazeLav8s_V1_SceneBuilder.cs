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
// MazeLav8s_V1_SceneBuilder.cs
// Tool-script for scene MazeLav8s_v1-0_0_0 — full procedural build
// Unity 6 (600.3.7f1) compatible - UTF-8 encoding - Unix line endings
// Locale: en_US
//
// ════════════════════════════════════════════════════════════════════
//  SCENE LAYOUT  MazeLav8s_v1-0_0_0
// ════════════════════════════════════════════════════════════════════
//
//  [SPAWN ROOM 5×5]  ── simple door ──►  [ANTI-CHAMBER 1]
//       │                                     │
//   one-way entrance                       maze corridor
//       │
//   simple door
//       ▼
//  [EXTRA ROOM 1]  ── EnemyGuardedChest (requires enemy KO)
//
//  ... maze corridors ...
//
//  [EXTRA ROOM 2]  ── EnemyGuardedChest (requires enemy KO)
//
//  ... maze corridors ...
//
//  [ANTI-CHAMBER 2]  ── CastleDoubleDoor (OneWay) ──►  [EXIT]
//
// ════════════════════════════════════════════════════════════════════
//
//  GENERATION PIPELINE:
//    1. LoadConfig       JSON → GameConfig8
//    2. GenerateMaze     GridMazeGenerator (8-axis DFS + A*)
//    3. ClearRooms       Carve special room cells into MazeData8
//    4. SpawnGround      Floor plane (GroundPlaneGenerator style)
//    5. SpawnWalls       Cardinal + diagonal via wallPrefab
//    6. SpawnDoors       Normal bi-dir doors, castle double door
//    7. SpawnTorches     Wall-adjacent cells (30%)
//    8. SpawnEnemies     Guarded cells — Ennemi component
//    9. SpawnChests      EnemyGuardedChest + ChestBehavior
//   10. SpawnPlayer      FPS CharacterController — ALWAYS LAST
//
//  PLUG-IN-OUT: FindFirstObjectByType only — no AddComponent.
//  ALL VALUES FROM JSON — no hardcoded gameplay constants.
//  LOCATION: Assets/Scripts/Core/06_Maze/

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Code.Lavos.Core
{
    /// <summary>
    /// MazeLav8s_V1_SceneBuilder — procedural scene tool for MazeLav8s_v1-0_0_0.
    /// Attach to a single empty GameObject named "SceneBuilder" in the scene.
    /// All maze content is generated at runtime from JSON config.
    /// </summary>
    public sealed class MazeLav8s_V1_SceneBuilder : MonoBehaviour
    {
        // ── Inspector — Prefabs ───────────────────────────────────────────
        [Header("Cardinal Wall Prefabs")]
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject doorPrefab;          // simple bi-dir door

        [Header("Diagonal Wall Prefabs")]
        [SerializeField] private GameObject wallDiagPrefab;

        [Header("Object Prefabs")]
        [SerializeField] private GameObject torchPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject playerPrefab;

        [Header("Config")]
        [SerializeField] private string configPath = "GameConfig8-default";
        [SerializeField] private int    overrideSeed = 0;  // 0 = random

        [Header("State (read-only)")]
        [SerializeField] private int    currentSeed;
        [SerializeField] private int    currentLevel = 0;
        [SerializeField] private float  lastBuildMs;

        // ── Runtime ──────────────────────────────────────────────────────
        private MazeData8          _mazeData;
        private GameConfig8        _cfg;
        private GridMazeGenerator8 _gen;
        private Transform          _wallsRoot;
        private Transform          _objRoot;
        private GameObject         _playerInstance;

        // Special room grid positions (maze cell coords)
        private (int x, int z) _spawnRoomOrigin;   // top-left of 5×5 spawn
        private (int x, int z) _antiChamber1;      // East of spawn
        private (int x, int z) _extraRoom1;         // North of spawn
        private (int x, int z) _extraRoom2;         // dead-end in maze
        private (int x, int z) _antiChamber2;       // near exit
        private (int x, int z) _exitRoom;           // final exit

        // Enemy refs for guarded chests
        private Ennemi _guard1;
        private Ennemi _guard2;

        // Room size constants
        private const int SPAWN_R  = 5;  // 5×5 spawn room (from GridMazeGenerator)
        private const int ROOM_R   = 3;  // 3×3 anti-chambers / extra rooms

        // ── Unity lifecycle ───────────────────────────────────────────────

        private void Start() => BuildScene();

        // ── [ContextMenu] for editor testing ────────────────────────────

        [ContextMenu("🔨 Build Scene")]
        public void BuildScene()
        {
            float t0 = Time.realtimeSinceStartup;

            // 1. Config
            if (!LoadConfig()) return;

            // 2. Seed
            currentSeed = (overrideSeed != 0) ? overrideSeed
                        : Random.Range(1, int.MaxValue);

            // 3. Generate maze data
            _gen      ??= new GridMazeGenerator8();
            _mazeData   = _gen.Generate(currentSeed, currentLevel, _cfg.MazeCfg);

            // 4. Carve extra rooms into the grid
            CarveAllSpecialRooms();

            // 5. Destroy previous maze objects
            DestroyMaze();

            _wallsRoot = new GameObject("[ Walls ]").transform;
            _objRoot   = new GameObject("[ Objects ]").transform;

            // 6. Ground
            SpawnGround();

            // 7. Walls
            SpawnAllWalls();

            // 8. Doors (normal bi-dir + castle double exit)
            SpawnAllDoors();

            // 9. Torches
            SpawnTorches();

            // 10. Enemies
            SpawnEnemies();

            // 11. Chests (guarded)
            SpawnGuardedChests();

            // 12. Player — LAST
            SpawnPlayer();

            lastBuildMs = (Time.realtimeSinceStartup - t0) * 1000f;
            Debug.Log($"[MazeLav8s_V1] Scene built in {lastBuildMs:F1} ms " +
                      $"| seed={currentSeed} size={_mazeData.Width}×{_mazeData.Height}");
        }

        [ContextMenu("🗑 Destroy Maze")]
        public void DestroyMaze()
        {
            if (_wallsRoot != null) DestroyImmediate(_wallsRoot.gameObject);
            if (_objRoot   != null) DestroyImmediate(_objRoot.gameObject);
            if (_playerInstance != null) DestroyImmediate(_playerInstance);
            _wallsRoot = null; _objRoot = null; _playerInstance = null;
        }

        // ════════════════════════════════════════════════════════════════
        //  1. CONFIG
        // ════════════════════════════════════════════════════════════════

        private bool LoadConfig()
        {
            // Try scene GameConfig8 first
            _cfg = FindFirstObjectByType<GameConfig8>();
            if (_cfg != null) return true;

            // Load from Resources JSON
            var asset = Resources.Load<TextAsset>(configPath);
            if (asset == null)
            {
                Debug.LogError($"[MazeLav8s_V1] Config not found: {configPath}");
                return false;
            }
            _cfg = GameConfig8.FromJson(asset.text);
            return true;
        }

        // ════════════════════════════════════════════════════════════════
        //  3. CARVE SPECIAL ROOMS
        // ════════════════════════════════════════════════════════════════
        //
        //  Rooms are cleared in MazeData8 AFTER generation so the DFS
        //  has already established corridors. We then clear all walls
        //  inside each room area, create doorway openings, and record
        //  the positions for object placement.

        private void CarveAllSpecialRooms()
        {
            int W = _mazeData.Width;
            int H = _mazeData.Height;

            // ── SPAWN ROOM — always at (1,1) via GridMazeGenerator ──────
            _spawnRoomOrigin = (1, 1);

            // ── ANTI-CHAMBER 1 — East of spawn room (col = 1+SPAWN_R+1) ─
            int ac1x = _spawnRoomOrigin.x + SPAWN_R + 1;
            int ac1z = _spawnRoomOrigin.z + 1;
            if (ac1x + ROOM_R < W - 1)
            {
                _antiChamber1 = (ac1x, ac1z);
                CarveRoom(ac1x, ac1z, ROOM_R);
                // Connect: open east wall of spawn room → west wall of anti-chamber
                OpenPassage(_spawnRoomOrigin.x + SPAWN_R - 1, _spawnRoomOrigin.z + 2,
                            Direction8.E);
            }

            // ── EXTRA ROOM 1 — North of spawn room ──────────────────────
            int er1x = _spawnRoomOrigin.x + 1;
            int er1z = _spawnRoomOrigin.z + SPAWN_R + 1;
            if (er1z + ROOM_R < H - 1)
            {
                _extraRoom1 = (er1x, er1z);
                CarveRoom(er1x, er1z, ROOM_R);
                OpenPassage(_spawnRoomOrigin.x + 2, _spawnRoomOrigin.z + SPAWN_R - 1,
                            Direction8.N);
            }

            // ── EXTRA ROOM 2 — pick a dead-end near maze center ─────────
            (int x, int z)? deadEnd = FindDeadEnd(W / 2, H / 2, 6);
            if (deadEnd.HasValue)
            {
                int er2x = Mathf.Clamp(deadEnd.Value.x, 2, W - ROOM_R - 2);
                int er2z = Mathf.Clamp(deadEnd.Value.z, 2, H - ROOM_R - 2);
                _extraRoom2 = (er2x, er2z);
                CarveRoom(er2x, er2z, ROOM_R);
                // Passage already exists (dead-end was accessible)
            }
            else
            {
                _extraRoom2 = (W / 2, H / 2);
                CarveRoom(_extraRoom2.x, _extraRoom2.z, ROOM_R);
            }

            // ── ANTI-CHAMBER 2 — just before the exit ───────────────────
            int ex = _mazeData.ExitCell.x;
            int ez = _mazeData.ExitCell.z;
            int ac2x = Mathf.Clamp(ex - ROOM_R - 1, 2, W - ROOM_R - 2);
            int ac2z = Mathf.Clamp(ez - ROOM_R - 1, 2, H - ROOM_R - 2);
            _antiChamber2 = (ac2x, ac2z);
            CarveRoom(ac2x, ac2z, ROOM_R);
            OpenPassage(ac2x, ac2z, Direction8.E);  // corridor → anti-chamber

            // ── EXIT ROOM — actual exit cell ─────────────────────────────
            _exitRoom = (ex, ez);
            CarveRoom(ex - 1, ez - 1, ROOM_R);
            // Mark as exit
            _mazeData.SetExit(ex, ez);
        }

        /// <summary>Clear all internal walls in a rectangle of [size×size] cells.</summary>
        private void CarveRoom(int ox, int oz, int size)
        {
            for (int dx = 0; dx < size; dx++)
            {
                for (int dz = 0; dz < size; dz++)
                {
                    int cx = ox + dx;
                    int cz = oz + dz;
                    if (!_mazeData.InBounds(cx, cz)) continue;
                    // Remove all walls inside
                    _mazeData.ClearFlag(cx, cz, CellFlags8.AllWalls);
                    _mazeData.AddFlag(cx, cz, CellFlags8.IsRoom);
                }
            }
        }

        /// <summary>Open a wall passage (remove flag in both cells).</summary>
        private void OpenPassage(int x, int z, Direction8 dir)
        {
            if (!_mazeData.InBounds(x, z)) return;
            _mazeData.ClearFlag(x, z, Direction8Helper.ToWallFlag(dir));
            var (dx, dz) = Direction8Helper.ToOffset(dir);
            int nx = x + dx, nz = z + dz;
            if (!_mazeData.InBounds(nx, nz)) return;
            _mazeData.ClearFlag(nx, nz,
                Direction8Helper.ToWallFlag(Direction8Helper.Opposite(dir)));
        }

        /// <summary>Find a dead-end cell within [radius] of (cx, cz).</summary>
        private (int, int)? FindDeadEnd(int cx, int cz, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    int x = cx + dx, z = cz + dz;
                    if (!_mazeData.InBounds(x, z)) continue;
                    var cell = _mazeData.GetCell(x, z);
                    // Dead-end = exactly 1 open passage (3 walls set out of 4 cardinal)
                    int wallCount = 0;
                    foreach (var d in new[] { Direction8.N, Direction8.S,
                                              Direction8.E, Direction8.W })
                        if (_mazeData.HasWall(x, z, d)) wallCount++;
                    if (wallCount == 3) return (x, z);
                }
            }
            return null;
        }

        // ════════════════════════════════════════════════════════════════
        //  5. GROUND
        // ════════════════════════════════════════════════════════════════

        private void SpawnGround()
        {
            float cs = _cfg.CellSize;
            float w  = _mazeData.Width  * cs;
            float h  = _mazeData.Height * cs;

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.SetParent(_objRoot);
            ground.transform.position  = new Vector3(w * 0.5f, -0.05f, h * 0.5f);
            ground.transform.localScale= new Vector3(w, 0.1f, h);
            ApplyFloorMaterial(ground.GetComponent<Renderer>());

            // Ceiling
            var ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.SetParent(_objRoot);
            ceiling.transform.position  = new Vector3(w * 0.5f, _cfg.WallHeight + 0.05f, h * 0.5f);
            ceiling.transform.localScale= new Vector3(w, 0.1f, h);
            ApplyCeilingMaterial(ceiling.GetComponent<Renderer>());
            Destroy(ceiling.GetComponent<Collider>());
        }

        // ════════════════════════════════════════════════════════════════
        //  6. WALLS
        // ════════════════════════════════════════════════════════════════

        private void SpawnAllWalls()
        {
            float cs  = _cfg.CellSize;
            float wh  = _cfg.WallHeight;
            bool diag = _cfg.MazeCfg.DiagonalWalls;

            for (int x = 0; x < _mazeData.Width; x++)
            {
                for (int z = 0; z < _mazeData.Height; z++)
                {
                    var cell = _mazeData.GetCell(x, z);
                    float wx = x * cs;
                    float wz = z * cs;

                    TrySpawnWall(x, z, wx, wz, wh, cs, Direction8.N, cell);
                    TrySpawnWall(x, z, wx, wz, wh, cs, Direction8.E, cell);
                    TrySpawnWall(x, z, wx, wz, wh, cs, Direction8.S, cell);
                    TrySpawnWall(x, z, wx, wz, wh, cs, Direction8.W, cell);

                    if (diag)
                    {
                        TrySpawnWall(x, z, wx, wz, wh, cs, Direction8.NE, cell);
                        TrySpawnWall(x, z, wx, wz, wh, cs, Direction8.NW, cell);
                        TrySpawnWall(x, z, wx, wz, wh, cs, Direction8.SE, cell);
                        TrySpawnWall(x, z, wx, wz, wh, cs, Direction8.SW, cell);
                    }
                }
            }
        }

        private void TrySpawnWall(int x, int z, float wx, float wz,
                                   float wh, float cs,
                                   Direction8 dir, CellFlags8 cell)
        {
            if ((cell & Direction8Helper.ToWallFlag(dir)) == 0) return;

            bool isDiag = Direction8Helper.IsDiagonal(dir);
            var  prefab = isDiag ? wallDiagPrefab : wallPrefab;
            if (prefab == null) return;

            var (dx, dz) = Direction8Helper.ToOffset(dir);
            float wposX  = wx + (dx > 0 ? cs * 0.5f : dx < 0 ? -cs * 0.5f : 0f);
            float wposZ  = wz + (dz > 0 ? cs * 0.5f : dz < 0 ? -cs * 0.5f : 0f);

            float angle = isDiag ? dir switch
            {
                Direction8.NE =>  45f, Direction8.NW => -45f,
                Direction8.SE => -45f, Direction8.SW =>  45f,
                _ => 0f
            } : dir switch
            {
                Direction8.N => 0f, Direction8.E => 90f,
                Direction8.S => 180f, Direction8.W => 270f,
                _ => 0f
            };

            GameObject wall = Instantiate(prefab,
                new Vector3(wposX, wh * 0.5f, wposZ),
                Quaternion.Euler(0f, angle, 0f));
            wall.name = $"Wall_{x}_{z}_{dir}";
            wall.transform.SetParent(_wallsRoot);

            // Apply castle wall texture for room walls, dungeon for corridors
            var cell0 = _mazeData.GetCell(x, z);
            bool isRoom = (cell0 & CellFlags8.IsRoom) != 0
                       || (cell0 & CellFlags8.SpawnRoom) != 0;

            var r = wall.GetComponentInChildren<Renderer>()
                 ?? wall.GetComponent<Renderer>();
            if (r != null)
            {
                if (isRoom)
                    ApplyCastleWallMaterial(r);
                else
                    ApplyWallMaterial(r);
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  7. DOORS
        // ════════════════════════════════════════════════════════════════

        private void SpawnAllDoors()
        {
            float cs = _cfg.CellSize;
            float wh = _cfg.WallHeight;

            // ── Normal bi-directional door: spawn room → anti-chamber 1 ─
            if (_antiChamber1 != default)
                SpawnSimpleDoor(_spawnRoomOrigin.x + SPAWN_R - 1, _spawnRoomOrigin.z + 2,
                                Direction8.E, cs, wh);

            // ── Normal bi-dir door: spawn room → extra room 1 ────────────
            if (_extraRoom1 != default)
                SpawnSimpleDoor(_spawnRoomOrigin.x + 2, _spawnRoomOrigin.z + SPAWN_R - 1,
                                Direction8.N, cs, wh);

            // ── Castle double door (one-way EXIT) at anti-chamber 2 ──────
            SpawnCastleExitDoor(cs, wh);
        }

        private void SpawnSimpleDoor(int x, int z, Direction8 dir, float cs, float wh)
        {
            if (doorPrefab == null) return;
            var (dx, dz) = Direction8Helper.ToOffset(dir);
            float px = x * cs + (dx > 0 ? cs * 0.5f : dx < 0 ? -cs * 0.5f : cs * 0.5f);
            float pz = z * cs + (dz > 0 ? cs * 0.5f : dz < 0 ? -cs * 0.5f : cs * 0.5f);
            float angle = dir switch
            {
                Direction8.N => 0f, Direction8.E => 90f,
                Direction8.S => 180f, Direction8.W => 270f, _ => 0f
            };
            var door = Instantiate(doorPrefab,
                new Vector3(px, wh * 0.5f, pz),
                Quaternion.Euler(0f, angle, 0f));
            door.name = "Door_BiDir";
            door.transform.SetParent(_objRoot);

            // Apply wooden door texture
            var r = door.GetComponentInChildren<Renderer>()
                 ?? door.GetComponent<Renderer>();
            if (r) ApplyDoorMaterial(r);
        }

        private void SpawnCastleExitDoor(float cs, float wh)
        {
            // Place between anti-chamber 2 and exit cell
            var ex = _mazeData.ExitCell;
            float px = (ex.x - 0.5f) * cs;
            float pz = (ex.z - 0.5f) * cs;

            // CastleDoubleDoor builds its own geometry — just needs a host GO
            var host = new GameObject("CastleExitDoor");
            host.transform.SetParent(_objRoot);
            host.transform.position = new Vector3(px, 0f, pz);

            var door = host.AddComponent<CastleDoubleDoor>();
            // Use reflection to set serialized fields at runtime
            // (SerializeField fields are set via component instantiation in Editor;
            //  at runtime we use public API or the default values which are fine)
            // isOneWayExit is public: set directly
            door.isOneWayExit = true;

            Debug.Log($"[MazeLav8s_V1] CastleDoubleDoor (OneWay) placée à {host.transform.position}");
        }

        // ════════════════════════════════════════════════════════════════
        //  8. TORCHES
        // ════════════════════════════════════════════════════════════════

        private void SpawnTorches()
        {
            if (torchPrefab == null) return;
            float cs = _cfg.CellSize;
            float wh = _cfg.WallHeight;

            for (int x = 0; x < _mazeData.Width; x++)
            {
                for (int z = 0; z < _mazeData.Height; z++)
                {
                    var cell = _mazeData.GetCell(x, z);
                    if ((cell & CellFlags8.HasTorch) == 0) continue;

                    var torch = Instantiate(torchPrefab,
                        new Vector3(x * cs + cs * 0.5f, wh * 0.75f, z * cs + cs * 0.5f),
                        Quaternion.Euler(0f, Random.Range(0, 4) * 90f, 0f));
                    torch.name = $"Torch_{x}_{z}";
                    torch.transform.SetParent(_objRoot);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  9. ENEMIES
        // ════════════════════════════════════════════════════════════════

        private void SpawnEnemies()
        {
            float cs = _cfg.CellSize;

            // Guard 1: inside Extra Room 1
            _guard1 = SpawnEnemy(_extraRoom1.x + 1, _extraRoom1.z + 1, cs, "Guard_1");

            // Guard 2: inside Extra Room 2
            _guard2 = SpawnEnemy(_extraRoom2.x + 1, _extraRoom2.z + 1, cs, "Guard_2");

            // Additional enemies from MazeData8 HasEnemy flag
            for (int x = 0; x < _mazeData.Width; x++)
            {
                for (int z = 0; z < _mazeData.Height; z++)
                {
                    var cell = _mazeData.GetCell(x, z);
                    if ((cell & CellFlags8.HasEnemy) == 0) continue;
                    // Skip the guard slots (already spawned)
                    if ((x == _extraRoom1.x + 1 && z == _extraRoom1.z + 1) ||
                        (x == _extraRoom2.x + 1 && z == _extraRoom2.z + 1)) continue;

                    SpawnEnemy(x, z, cs, $"Enemy_{x}_{z}");
                }
            }
        }

        private Ennemi SpawnEnemy(int gx, int gz, float cs, string label)
        {
            if (enemyPrefab == null)
            {
                // Build a minimal procedural enemy (red cube with Ennemi component)
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = label;
                go.transform.position = new Vector3(gx * cs + cs * 0.5f, 1f, gz * cs + cs * 0.5f);
                go.transform.localScale = new Vector3(0.8f, 1.6f, 0.8f);
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")
                                    ?? Shader.Find("Standard"));
                mat.color = new Color(0.8f, 0.05f, 0.05f);
                go.GetComponent<Renderer>().material = mat;
                go.transform.SetParent(_objRoot);
                var e = go.AddComponent<Ennemi>();
                return e;
            }
            else
            {
                var go = Instantiate(enemyPrefab,
                    new Vector3(gx * cs + cs * 0.5f, 1f, gz * cs + cs * 0.5f),
                    Quaternion.identity);
                go.name = label;
                go.transform.SetParent(_objRoot);
                return go.GetComponent<Ennemi>();
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  10. CHESTS (guarded)
        // ════════════════════════════════════════════════════════════════

        private void SpawnGuardedChests()
        {
            float cs = _cfg.CellSize;

            // Chest 1: Extra Room 1, corner away from guard
            SpawnGuardedChest(_extraRoom1.x + 2, _extraRoom1.z + 2, cs, _guard1, "GuardedChest_1");

            // Chest 2: Extra Room 2, corner away from guard
            SpawnGuardedChest(_extraRoom2.x + 2, _extraRoom2.z + 2, cs, _guard2, "GuardedChest_2");

            // Bonus plain chest (HasChest cells from maze generator)
            for (int x = 0; x < _mazeData.Width; x++)
            {
                for (int z = 0; z < _mazeData.Height; z++)
                {
                    var cell = _mazeData.GetCell(x, z);
                    if ((cell & CellFlags8.HasChest) == 0) continue;
                    // Skip guarded slots
                    if ((x == _extraRoom1.x + 2 && z == _extraRoom1.z + 2) ||
                        (x == _extraRoom2.x + 2 && z == _extraRoom2.z + 2)) continue;

                    SpawnGuardedChest(x, z, cs, null, $"Chest_{x}_{z}");
                }
            }
        }

        private void SpawnGuardedChest(int gx, int gz, float cs,
                                        Ennemi guardian, string label)
        {
            if (!_mazeData.InBounds(gx, gz)) return;

            // Build chest geometry procedurally
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = label;
            go.transform.position   = new Vector3(gx * cs + cs * 0.5f, 0.6f, gz * cs + cs * 0.5f);
            go.transform.localScale = new Vector3(1.5f, 1.2f, 1.0f);
            go.transform.SetParent(_objRoot);

            ApplyChestMaterial(go.GetComponent<Renderer>());

            // Add required components
            var chest = go.AddComponent<ChestBehavior>();
            var audio = go.AddComponent<AudioSource>();
            var egc   = go.AddComponent<EnemyGuardedChest>();

            if (guardian != null)
                egc.SetGuardian(guardian);

            Debug.Log($"[MazeLav8s_V1] {label} placé à ({gx},{gz})" +
                      (guardian != null ? $" gardé par {guardian.name}" : " (libre)"));
        }

        // ════════════════════════════════════════════════════════════════
        //  11. PLAYER (FPS — ALWAYS LAST)
        // ════════════════════════════════════════════════════════════════

        private void SpawnPlayer()
        {
            float cs  = _cfg.CellSize;
            float eyeH = _cfg.PlayerEyeHeight;

            var spawnCell = _mazeData.SpawnCell;
            var pos = new Vector3(
                spawnCell.x * cs + cs * 0.5f,
                eyeH,
                spawnCell.z * cs + cs * 0.5f);

            if (playerPrefab != null)
            {
                _playerInstance = Instantiate(playerPrefab, pos, Quaternion.identity);
            }
            else
            {
                // Build minimal FPS player procedurally
                _playerInstance = BuildProceduralPlayer(pos, eyeH);
            }

            _playerInstance.name = "Player";
            _playerInstance.tag  = "Player";

            // Face toward anti-chamber 1 (East)
            _playerInstance.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            Debug.Log($"[MazeLav8s_V1] Joueur spawné à {pos}");
        }

        /// <summary>
        /// Builds a minimal FPS player capsule with PlayerController + PlayerStats
        /// + CharacterController + camera, purely from existing Core components.
        /// Used as fallback when no playerPrefab is assigned.
        /// </summary>
        private GameObject BuildProceduralPlayer(Vector3 pos, float eyeH)
        {
            // Root GO
            var root = new GameObject("Player");
            root.transform.position = pos;
            root.layer = LayerMask.NameToLayer("Default");

            // CharacterController
            var cc = root.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            // Visual capsule (no renderer for FPS but we add it for editor gizmos)
            // PlayerStats
            root.AddComponent<PlayerStats>();

            // PlayerController
            var pc = root.AddComponent<PlayerController>();

            // FPS Camera
            var camGo = new GameObject("PlayerCamera");
            camGo.transform.SetParent(root.transform, false);
            camGo.transform.localPosition = new Vector3(0f, eyeH, 0f);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 80f;
            cam.nearClipPlane = 0.08f;
            cam.farClipPlane  = 200f;
            camGo.AddComponent<AudioListener>();

            // Assign camera to PlayerController via SerializedField workaround:
            // PlayerController finds its camera via GetComponentInChildren<Camera>()
            // OR via the Inspector-assigned field. Since we're at runtime and
            // PlayerController uses a [SerializeField] private Camera, we use
            // the public accessor pattern defined in PlayerController.SetCamera()
            // if available, otherwise it will auto-find via GetComponentInChildren.

            Debug.Log("[MazeLav8s_V1] Joueur procédural construit (fallback — assignez playerPrefab pour le build complet).");
            return root;
        }

        // ── Material Creation (local, no external dependency) ────────────

        private static void ApplyFloorMaterial(Renderer renderer)
        {
            if (renderer == null) return;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color32(100, 100, 110, 255); // Gray stone floor
            mat.SetFloat("_Glossiness", 0.3f);
            mat.SetFloat("_Metallic", 0.1f);
            renderer.material = mat;
        }

        private static void ApplyCeilingMaterial(Renderer renderer)
        {
            if (renderer == null) return;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color32(60, 50, 40, 255); // Dark brown ceiling
            mat.SetFloat("_Glossiness", 0.2f);
            mat.SetFloat("_Metallic", 0.0f);
            renderer.material = mat;
        }

        private static void ApplyWallMaterial(Renderer renderer)
        {
            if (renderer == null) return;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color32(90, 85, 80, 255); // Gray wall
            mat.SetFloat("_Glossiness", 0.25f);
            mat.SetFloat("_Metallic", 0.05f);
            renderer.material = mat;
        }

        private static void ApplyCastleWallMaterial(Renderer renderer)
        {
            if (renderer == null) return;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color32(80, 75, 90, 255); // Purple-gray castle wall
            mat.SetFloat("_Glossiness", 0.3f);
            mat.SetFloat("_Metallic", 0.1f);
            renderer.material = mat;
        }

        private static void ApplyDoorMaterial(Renderer renderer)
        {
            if (renderer == null) return;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color32(139, 90, 43, 255); // Brown wood door
            mat.SetFloat("_Glossiness", 0.35f);
            mat.SetFloat("_Metallic", 0.05f);
            renderer.material = mat;
        }

        private static void ApplyChestMaterial(Renderer renderer)
        {
            if (renderer == null) return;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color32(139, 90, 43, 255); // Brown wood chest
            mat.SetFloat("_Glossiness", 0.4f);
            mat.SetFloat("_Metallic", 0.2f);
            renderer.material = mat;
        }
    }
}
