// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// CompleteCorridorMazeBuilder.cs - Pure corridor maze builder

using UnityEngine;
using Code.Lavos.Core.Advanced;
using System.Collections.Generic;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    public sealed class CompleteCorridorMazeBuilder : MonoBehaviour
    {
        private static readonly Dictionary<string, GameObject> _prefabCache = new();

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
            _mazeData = _generator.Generate(_currentSeed, _currentLevel, mazeCfg);

            if (_mazeData == null) { Debug.LogError("[CorridorMazeBuilder] MazeData is NULL!"); return; }

            Debug.Log($"[CorridorMazeBuilder] Maze: {_mazeData.Width}x{_mazeData.Height} seed={_mazeData.Seed}");
            _currentDifficultyFactor = _mazeData.DifficultyFactor;

            SpawnGround();
            SpawnAllWalls();
            SpawnTorches();
            SpawnObjects();
            SpawnEntranceExitMarkers();

            Debug.Log("[CorridorMazeBuilder] Calling SpawnPlayer()...");
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
            // FIX: Prefab is named "Player" not "PlayerPrefab"
            playerPrefab ??= Resources.Load<GameObject>("Prefabs/Player");
            
            // Log prefab loading status
            Debug.Log($"[CorridorMazeBuilder] Prefab loading status:");
            Debug.Log($"  - wallPrefab: {(wallPrefab != null ? "LOADED" : "NULL")}");
            Debug.Log($"  - doorPrefab: {(doorPrefab != null ? "LOADED" : "NULL")}");
            Debug.Log($"  - torchPrefab: {(torchPrefab != null ? "LOADED" : "NULL")}");
            Debug.Log($"  - chestPrefab: {(chestPrefab != null ? "LOADED" : "NULL")}");
            Debug.Log($"  - enemyPrefab: {(enemyPrefab != null ? "LOADED" : "NULL")}");
            Debug.Log($"  - floorPrefab: {(floorPrefab != null ? "LOADED" : "NULL")}");
            Debug.Log($"  - playerPrefab: {(playerPrefab != null ? "LOADED" : "NULL")} (looking for 'Prefabs/Player')");
            
            if (wallPrefab == null) Debug.LogError("[CorridorMazeBuilder] wallPrefab missing!");
            if (playerPrefab == null) Debug.LogError("[CorridorMazeBuilder] playerPrefab missing! Check Assets/Resources/Prefabs/Player.prefab exists");
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
            if (go == null)
            {
                Debug.LogError("[CorridorMazeBuilder] Failed to instantiate floor prefab!");
                return;
            }
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
            if (go == null)
            {
                Debug.LogError($"[CorridorMazeBuilder] Failed to instantiate wall at ({cx},{cz})!");
                return;
            }
            go.name = $"Wall_{cx}_{cz}_{dir}";
            go.transform.SetParent(_wallsRoot);
            float thickness = _config.WallThickness;
            float yOffset = wallPivotIsAtMeshCenter ? wh * 0.5f : 0f;
            go.transform.localScale = new Vector3(cs, wh, thickness);
            go.transform.localPosition += Vector3.up * yOffset;
            
            // Ensure wall has a collider for collision with player, enemies, and items
            BoxCollider collider = go.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = go.AddComponent<BoxCollider>();
                Debug.Log($"[CorridorMazeBuilder] Added BoxCollider to wall {go.name}");
            }
            collider.enabled = true;
            collider.isTrigger = false; // Ensure it's a solid collider, not a trigger
            
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
                    if (go != null)
                    {
                        go.name = $"Torch_{x}_{z}";
                        if (_objectsRoot == null) _objectsRoot = new GameObject("MazeObjects").transform;
                        go.transform.SetParent(_objectsRoot);
                        count++;
                    }
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
                    if (go != null)
                    {
                        go.name = $"Chest_{x}_{z}";
                        if (_objectsRoot == null) _objectsRoot = new GameObject("MazeObjects").transform;
                        go.transform.SetParent(_objectsRoot);
                        Debug.Log($"[CorridorMazeBuilder] Chest at ({pos.x:F2}, {pos.y:F2}, {pos.z:F2}) - Grid({x},{z})");
                        chests++;
                    }
                }
                if (spawnEnemies && (cell & CellFlags8.HasEnemy) != 0 && enemyPrefab != null)
                {
                    var go = Instantiate(enemyPrefab, pos, Quaternion.identity);
                    if (go != null)
                    {
                        go.name = $"Enemy_{x}_{z}";
                        if (_objectsRoot == null) _objectsRoot = new GameObject("MazeObjects").transform;
                        go.transform.SetParent(_objectsRoot);
                        enemies++;
                    }
                }
            }
            Debug.Log($"[CorridorMazeBuilder] Spawned {chests} chests, {enemies} enemies");
        }

        private void SpawnPlayer()
        {
            // Debug logging to identify spawn issue
            if (playerPrefab == null)
            {
                Debug.LogError("[CorridorMazeBuilder] SpawnPlayer FAILED: playerPrefab is NULL!");
                return;
            }
            if (_mazeData == null)
            {
                Debug.LogError("[CorridorMazeBuilder] SpawnPlayer FAILED: _mazeData is NULL!");
                return;
            }
            if (_config == null)
            {
                Debug.LogError("[CorridorMazeBuilder] SpawnPlayer FAILED: _config is NULL!");
                return;
            }

            int sx = _mazeData.SpawnCell.x, sz = _mazeData.SpawnCell.z;
            var pos = new Vector3((sx + 0.5f) * _config.CellSize, _config.PlayerEyeHeight, (sz + 0.5f) * _config.CellSize);
            _playerInstance = Instantiate(playerPrefab, pos, Quaternion.identity);
            if (_playerInstance == null)
            {
                Debug.LogError("[CorridorMazeBuilder] Failed to instantiate player prefab!");
                return;
            }
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

            // Create 8-bit pixel art style marker (cylinder with pixelated texture)
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = name;
            marker.transform.position = pos;
            marker.transform.localScale = new Vector3(cs * markerScale, markerHeight, cs * markerScale);

            // Create 8-bit pixel art texture for marker
            Texture2D pixelTex = Create8BitMarkerTexture(color, isEntrance);
            
            // Create emissive material with pixel art texture
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (mat == null) mat = new Material(Shader.Find("Unlit/Color"));

            mat.mainTexture = pixelTex;
            mat.color = new Color(color.r, color.g, color.b, 1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * markerLightIntensity * 1.5f);

            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = mat;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            // Add particle effect (ascending pixels)
            SpawnPixelParticles(pos, color, isEntrance);

            // Add point light for glow effect
            Light glowLight = marker.AddComponent<Light>();
            if (glowLight != null)
            {
                glowLight.color = color;
                glowLight.intensity = markerLightIntensity * 1.2f;
                glowLight.range = cs * 2f;
                glowLight.shadows = LightShadows.None;
                glowLight.bounceIntensity = 1.5f;
            }

            // Add floating ring effect around marker
            SpawnFloatingRing(pos, color, isEntrance);

            if (_objectsRoot != null)
                marker.transform.SetParent(_objectsRoot);

            Debug.Log($"[CorridorMazeBuilder] {name} spawned at grid ({cx},{cz})");
        }

        private void SpawnFloatingRing(Vector3 position, Color color, bool isEntrance)
        {
            // Create rotating ring around marker for extra visual flair
            GameObject ringObj = new GameObject(isEntrance ? "EntranceRing" : "ExitRing");
            ringObj.transform.position = position + Vector3.up * 1f;
            ringObj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            // Create ring mesh (torus approximation)
            Mesh ringMesh = CreateRingMesh(0.6f, 0.05f, 32);
            MeshFilter meshFilter = ringObj.AddComponent<MeshFilter>();
            meshFilter.mesh = ringMesh;

            // Add renderer BEFORE accessing material
            MeshRenderer renderer = ringObj.AddComponent<MeshRenderer>();

            // Emissive material for ring
            Material ringMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (ringMat != null)
            {
                ringMat.color = color;
                ringMat.EnableKeyword("_EMISSION");
                ringMat.SetColor("_EmissionColor", color * 2f);
                renderer.material = ringMat;
            }

            // Add rotation animation component
            RingRotator rotator = ringObj.AddComponent<RingRotator>();
            rotator.rotationSpeed = isEntrance ? 30f : -20f; // Entrance spins faster, opposite direction

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

        private void SpawnPixelParticles(Vector3 position, Color color, bool isEntrance)
        {
            // Create particle system for 8-bit pixel effect
            GameObject particleObj = new GameObject(isEntrance ? "EntranceParticles" : "ExitParticles");
            particleObj.transform.position = position + Vector3.up * 1f;

            var particleSystem = particleObj.AddComponent<ParticleSystem>();
            
            // Configure all settings BEFORE accessing modules
            var main = particleSystem.main;
            
            // 8-bit pixel style settings
            main.startSize = particleSize;
            main.startSpeed = 0.5f;
            main.startLifetime = 2f;
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 20;
            main.loop = true;
            main.playOnAwake = true;

            // Emission
            var emission = particleSystem.emission;
            emission.rateOverTime = particleEmissionRate;

            // Shape: emit from top
            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            // Color over lifetime (fading pixel)
            var colorOverLifetime = particleSystem.colorOverLifetime;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color * 0.5f, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;

            // Rotation for dynamic effect
            var rotationOverLifetime = particleSystem.rotationOverLifetime;
            rotationOverLifetime.zMultiplier = 90f;

            if (_objectsRoot != null)
                particleObj.transform.SetParent(_objectsRoot);
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
