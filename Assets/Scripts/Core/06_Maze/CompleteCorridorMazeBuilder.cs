﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// CompleteCorridorMazeBuilder.cs - Pure corridor maze builder

using UnityEngine;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Core
{
    public sealed class CompleteCorridorMazeBuilder : MonoBehaviour
    {
        [Header("Cardinal Prefabs")]
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject doorPrefab;
        [SerializeField] private Material wallMaterial;

        [Header("Object Prefabs")]
        [SerializeField] private GameObject torchPrefab;
        [SerializeField] private GameObject chestPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject playerPrefab;

        [Header("Config")]
        [SerializeField] private string configResourcePath = "Config/GameConfig8-default";

        [Header("Spawn Options")]
        [Tooltip("Spawn enemies in maze")]
        [SerializeField] private bool spawnEnemies = true;
        [Tooltip("Spawn chests in maze")]
        [SerializeField] private bool spawnChests = true;

        [Header("Marker Settings")]
        [Tooltip("Height of entrance/exit markers")]
        [SerializeField] private float markerHeight = 2f;
        [Tooltip("Scale of entrance/exit markers")]
        [SerializeField] private float markerScale = 0.3f;
        [Tooltip("Light intensity for markers")]
        [SerializeField] private float markerLightIntensity = 2f;
        [Tooltip("Particle emission rate")]
        [SerializeField] private float particleEmissionRate = 10f;
        [Tooltip("Particle size")]
        [SerializeField] private float particleSize = 0.15f;

        [Header("Wall Settings")]
        [SerializeField] private bool wallPivotIsAtMeshCenter = true;

        [Header("State (read-only)")]
        [SerializeField] private int _currentLevel;
        [SerializeField] private int _currentSeed;
        [SerializeField] private float _lastGenMs;
        [SerializeField] private float _currentDifficultyFactor;

        private MazeData8 _mazeData;
        private GameConfig _config;
        private GridMazeGenerator _generator;
        private Transform _wallsRoot;
        private Transform _objectsRoot;
        private GameObject _playerInstance;

        public MazeData8 MazeData => _mazeData;
        public GameConfig Config => _config;
        public int CurrentLevel => _currentLevel;
        public float LastGenMs => _lastGenMs;
        public float CurrentDifficultyFactor => _currentDifficultyFactor;

        private void Start()
        {
            _currentSeed = Random.Range(int.MinValue, int.MaxValue);
            GenerateMaze();
        }

        public void SetLevelAndSeed(int level, int seed)
        {
            _currentLevel = level;
            _currentSeed = seed;
        }

        [ContextMenu("Generate Maze (Pure Corridors)")]
        public void GenerateMaze()
        {
            Debug.Log("[CorridorMazeBuilder] === GENERATE MAZE STARTED ===");
            float t0 = Time.realtimeSinceStartup;

            LoadConfig();
            if (_config == null) { Debug.LogError("[CorridorMazeBuilder] Config is NULL!"); return; }

            ValidateAssets();
            DestroyMazeObjects();

            // Always generate fresh for corridor maze (cache uses different data type)
            Debug.Log($"[CorridorMazeBuilder] Generating new maze L{_currentLevel} S{_currentSeed}");
            var mazeCfg = new MazeConfig
            {
                BaseSize = _config.MazeCfg.BaseSize,
                MinSize = _config.MazeCfg.MinSize,
                MaxSize = _config.MazeCfg.MaxSize,
                SpawnRoomSize = _config.MazeCfg.SpawnRoomSize,
                TorchChance = _config.MazeCfg.TorchChance,
                ChestDensity = _config.MazeCfg.ChestDensity,
                EnemyDensity = _config.MazeCfg.EnemyDensity,
                DeadEndDensity = _config.MazeCfg.DeadEndDensity,
            };
            _generator ??= new GridMazeGenerator();
            _mazeData = _generator.Generate(currentSeed, currentLevel, mazeCfg);

            if (_mazeData == null) { Debug.LogError("[CorridorMazeBuilder] MazeData is NULL!"); return; }

            Debug.Log($"[CorridorMazeBuilder] Maze: {_mazeData.Width}x{_mazeData.Height} seed={_mazeData.Seed}");
            _currentDifficultyFactor = _mazeData.DifficultyFactor;

            SpawnGround();
            SpawnAllWalls();
            SpawnTorches();
            SpawnObjects();
            SpawnEntranceExitMarkers();

            SpawnPlayer();
            _lastGenMs = (Time.realtimeSinceStartup - t0) * 1000f;
            Debug.Log($"[CorridorMazeBuilder] Done -- {_lastGenMs:F2}ms factor={_currentDifficultyFactor:F3}");
        }

        private void LoadConfig()
        {
            var comp = FindFirstObjectByType<GameConfig>();
            if (comp != null) { _config = comp; return; }
            var json = Resources.Load<TextAsset>(configResourcePath);
            _config = json != null ? GameConfig.FromJson(json.text) : new GameConfig();
            if (_config == null) { Debug.LogError("[CorridorMazeBuilder] CRITICAL: Failed to load GameConfig!"); enabled = false; }
        }

        private void ValidateAssets()
        {
            wallPrefab ??= Resources.Load<GameObject>("Prefabs/WallPrefab");
            doorPrefab ??= Resources.Load<GameObject>("Prefabs/DoorPrefab");
            torchPrefab ??= Resources.Load<GameObject>("Prefabs/TorchHandlePrefab");
            chestPrefab ??= Resources.Load<GameObject>("Prefabs/ChestPrefab");
            enemyPrefab ??= Resources.Load<GameObject>("Prefabs/EnemyPrefab");
            floorPrefab ??= Resources.Load<GameObject>("Prefabs/FloorTilePrefab");
            playerPrefab ??= Resources.Load<GameObject>("Prefabs/PlayerPrefab");
            if (wallPrefab == null) Debug.LogError("[CorridorMazeBuilder] wallPrefab missing!");
        }

        private void DestroyMazeObjects()
        {
            DestroyContainer(ref _wallsRoot, "MazeWalls");
            DestroyContainer(ref _objectsRoot, "MazeObjects");
            if (_playerInstance != null) { if (Application.isPlaying) Destroy(_playerInstance); else DestroyImmediate(_playerInstance); _playerInstance = null; }
        }

        private void DestroyContainer(ref Transform t, string name)
        {
            if (t != null) { if (Application.isPlaying) Destroy(t.gameObject); else DestroyImmediate(t.gameObject); t = null; return; }
            var g = GameObject.Find(name);
            if (g != null) { if (Application.isPlaying) Destroy(g); else DestroyImmediate(g); }
        }

        private void SpawnGround()
        {
            if (floorPrefab == null || _mazeData == null || _config == null) return;
            float sz = _mazeData.Width * _config.CellSize;
            var go = Instantiate(floorPrefab, new Vector3(sz * 0.5f, 0f, sz * 0.5f), Quaternion.identity);
            go.name = "MazeFloor";
            go.transform.localScale = new Vector3(sz, 1f, sz);
        }

        private void SpawnAllWalls()
        {
            if (wallPrefab == null || _mazeData == null || _config == null) return;
            _wallsRoot = new GameObject("MazeWalls").transform;
            float cs = _config.CellSize, wh = _config.WallHeight;
            int count = 0;
            for (int z = 0; z < _mazeData.Height; z++)
            for (int x = 0; x < _mazeData.Width; x++)
            {
                var cell = _mazeData.GetCell(x, z);
                if ((cell & CellFlags8.AllWalls) == CellFlags8.None)
                {
                    if (ShouldSpawnWall(x, z, Direction8.N)) { SpawnCardinalWall(x, z, Direction8.N, cs, wh); count++; }
                    if (ShouldSpawnWall(x, z, Direction8.E)) { SpawnCardinalWall(x, z, Direction8.E, cs, wh); count++; }
                    if (ShouldSpawnWall(x, z, Direction8.S)) { SpawnCardinalWall(x, z, Direction8.S, cs, wh); count++; }
                    if (ShouldSpawnWall(x, z, Direction8.W)) { SpawnCardinalWall(x, z, Direction8.W, cs, wh); count++; }
                }
            }
            Debug.Log($"[CorridorMazeBuilder] Spawned {count} walls");
        }

        private bool ShouldSpawnWall(int cx, int cz, Direction8 dir)
        {
            var (dx, dz) = Direction8Helper.ToOffset(dir);
            int nx = cx + dx, nz = cz + dz;
            if (nx < 0 || nx >= _mazeData.Width || nz < 0 || nz >= _mazeData.Height) return true;
            var neighbor = _mazeData.GetCell(nx, nz);
            return (neighbor & CellFlags8.AllWalls) != CellFlags8.None;
        }

        private void SpawnCardinalWall(int cx, int cz, Direction8 dir, float cs, float wh)
        {
            var (dx, dz) = Direction8Helper.ToOffset(dir);
            var pos = new Vector3((cx + 0.5f + dx * 0.5f) * cs, 0f, (cz + 0.5f + dz * 0.5f) * cs);
            Quaternion rot = (dir == Direction8.E || dir == Direction8.W) ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
            var go = Instantiate(wallPrefab, pos, rot);
            go.name = $"Wall_{cx}_{cz}_{dir}";
            go.transform.SetParent(_wallsRoot);
            float thickness = _config.WallThickness;
            float yOffset = wallPivotIsAtMeshCenter ? wh * 0.5f : 0f;
            go.transform.localScale = new Vector3(cs, wh, thickness);
            go.transform.localPosition += Vector3.up * yOffset;
            if (wallMaterial != null) { var r = go.GetComponent<Renderer>(); if (r != null) r.material = wallMaterial; }
        }

        private void SpawnTorches()
        {
            if (torchPrefab == null || _mazeData == null) return;
            int count = 0;
            for (int x = 0; x < _mazeData.Width; x++)
            for (int z = 0; z < _mazeData.Height; z++)
            {
                if ((_mazeData.GetCell(x, z) & CellFlags8.HasTorch) != 0)
                {
                    var pos = new Vector3((x + 0.5f) * _config.CellSize, 0f, (z + 0.5f) * _config.CellSize);
                    var go = Instantiate(torchPrefab, pos, Quaternion.identity);
                    go.name = $"Torch_{x}_{z}";
                    if (_objectsRoot == null) _objectsRoot = new GameObject("MazeObjects").transform;
                    go.transform.SetParent(_objectsRoot);
                    count++;
                }
            }
            Debug.Log($"[CorridorMazeBuilder] Spawned {count} torches");
        }

        private void SpawnObjects()
        {
            if (_mazeData == null) return;
            int chests = 0, enemies = 0;
            for (int x = 0; x < _mazeData.Width; x++)
            for (int z = 0; z < _mazeData.Height; z++)
            {
                var cell = _mazeData.GetCell(x, z);
                // Center of cell (matching player spawn formula)
                var pos = new Vector3((x + 0.5f) * _config.CellSize, 0f, (z + 0.5f) * _config.CellSize);
                
                if (spawnChests && (cell & CellFlags8.HasChest) != 0 && chestPrefab != null)
                {
                    var go = Instantiate(chestPrefab, pos, Quaternion.identity);
                    go.name = $"Chest_{x}_{z}";
                    if (_objectsRoot == null) _objectsRoot = new GameObject("MazeObjects").transform;
                    go.transform.SetParent(_objectsRoot);
                    Debug.Log($"[CorridorMazeBuilder] Chest at ({pos.x:F2}, {pos.y:F2}, {pos.z:F2}) - Grid({x},{z})");
                    chests++;
                }
                if (spawnEnemies && (cell & CellFlags8.HasEnemy) != 0 && enemyPrefab != null)
                {
                    var go = Instantiate(enemyPrefab, pos, Quaternion.identity);
                    go.name = $"Enemy_{x}_{z}";
                    if (_objectsRoot == null) _objectsRoot = new GameObject("MazeObjects").transform;
                    go.transform.SetParent(_objectsRoot);
                    enemies++;
                }
            }
            Debug.Log($"[CorridorMazeBuilder] Spawned {chests} chests, {enemies} enemies");
        }

        private void SpawnPlayer()
        {
            if (playerPrefab == null || _mazeData == null || _config == null) return;
            int sx = _mazeData.SpawnCell.x, sz = _mazeData.SpawnCell.z;
            var pos = new Vector3((sx + 0.5f) * _config.CellSize, _config.PlayerEyeHeight, (sz + 0.5f) * _config.CellSize);
            _playerInstance = Instantiate(playerPrefab, pos, Quaternion.identity);
            _playerInstance.name = "Player";
            Debug.Log($"[CorridorMazeBuilder] Player spawned at ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
        }

        private void SpawnEntranceExitMarkers()
        {
            if (_mazeData == null || _config == null) return;

            // Entrance marker (GREEN - spawn point)
            SpawnPixelArtMarker(
                _mazeData.SpawnCell.x, 
                _mazeData.SpawnCell.z, 
                Color.green, 
                "EntranceMarker", 
                isEntrance: true
            );

            // Exit marker (RED - exit point)
            SpawnPixelArtMarker(
                _mazeData.ExitCell.x, 
                _mazeData.ExitCell.z, 
                Color.red, 
                "ExitMarker", 
                isEntrance: false
            );

            Debug.Log($"[CorridorMazeBuilder] Entrance/Exit markers spawned");
        }

        private void SpawnPixelArtMarker(int cx, int cz, Color color, string name, bool isEntrance)
        {
            if (_config == null) return;

            float cs = _config.CellSize;
            var pos = new Vector3((cx + 0.5f) * cs, 0.5f, (cz + 0.5f) * cs);

            // Create 8-bit pixel art style marker (cylinder with emissive material)
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = name;
            marker.transform.position = pos;
            marker.transform.localScale = new Vector3(cs * markerScale, markerHeight, cs * markerScale);

            // Create emissive material
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (mat == null) mat = new Material(Shader.Find("Unlit/Color"));
            
            mat.color = new Color(color.r, color.g, color.b, 1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * markerLightIntensity);

            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = mat;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            // Add particle effect
            SpawnPixelParticles(pos, color, isEntrance);

            // Add point light for glow effect
            Light glowLight = marker.AddComponent<Light>();
            if (glowLight != null)
            {
                glowLight.color = color;
                glowLight.intensity = markerLightIntensity;
                glowLight.range = cs * 1.5f;
                glowLight.shadows = LightShadows.None;
            }

            if (_objectsRoot != null)
                marker.transform.SetParent(_objectsRoot);

            Debug.Log($"[CorridorMazeBuilder] {name} spawned at grid ({cx},{cz})");
        }

        private void SpawnPixelParticles(Vector3 position, Color color, bool isEntrance)
        {
            // Create particle system for 8-bit pixel effect
            GameObject particleObj = new GameObject(isEntrance ? "EntranceParticles" : "ExitParticles");
            particleObj.transform.position = position + Vector3.up * 1f;

            var particleSystem = particleObj.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            var emission = particleSystem.emission;
            var shape = particleSystem.shape;
            var colorOverLifetime = particleSystem.colorOverLifetime;

            // 8-bit pixel style settings
            main.duration = 1f;
            main.loop = true;
            main.startSize = particleSize;
            main.startSpeed = 0.5f;
            main.startLifetime = 2f;
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 20;

            // Emission rate
            emission.rateOverTime = particleEmissionRate;

            // Shape: emit from top
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            // Color gradient (fading pixel)
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color * 0.5f, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;

            // Rotate for dynamic effect
            var rotationOverLifetime = particleSystem.rotationOverLifetime;
            rotationOverLifetime.zMultiplier = 90f;

            if (_objectsRoot != null)
                particleObj.transform.SetParent(_objectsRoot);
        }
    }
}
