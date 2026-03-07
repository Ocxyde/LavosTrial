// MazeRenderer.cs
// Renders maze from MazeGenerator data
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Ressources system - maze visualization
// Clean, Unity6-compatible MazeRenderer: final consolidated class
// Unity6-era compatibility note: this file follows Unity6 patterns and uses the New Input System placeholder guidance.

using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0414 // Disable warnings for unused serialized fields (reserved for future features)

namespace Code.Lavos.Core
{
    [RequireComponent(typeof(MazeGenerator))]
    [RequireComponent(typeof(TorchPool))]
    public class MazeRenderer : MonoBehaviour, IMazeRenderer
    {
    [Header("Dimensions")]
    [SerializeField] private float cellSize = 6f;  // Increased from 4f to 6f for wider corridors (fits 2 bodies)
    [SerializeField] private float wallHeight = 3.5f;  // Increased from 3f for better proportions

    [Header("Torches")]
    [SerializeField] private float torchProbability = 0.5f;  // 1 in 2 walls (50% chance) - INCREASED for more light
    [SerializeField] private float torchHeightRatio = 0.55f;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float playerSpawnOffsetY = 1.5f;

    [Header("Ambiance")]
    [SerializeField] private Color ambientColor = new Color(0.25f, 0.2f, 0.15f);  // Brighter ambient light
    [SerializeField] private float ambientIntensity = 0.6f;  // Increased from 0.3 for better visibility

    [Header("Lighting")]
    [SerializeField] private bool useBakedLighting = false;  // Disable realtime GI
    [SerializeField] private float torchLightRange = 8f;
    [SerializeField] private Color torchLightColor = new Color(1f, 0.9f, 0.7f);

    [Header("Exit Door")]
    [SerializeField] private bool spawnExitDoor = false;  // DISABLED - No exit door

    private MazeGenerator _gen;
    private TorchPool _torchPool;
    private RoomGenerator _roomGenerator;
    private DoorHolePlacer _doorHolePlacer;
    private RoomDoorPlacer _roomDoorPlacer;

    private Material _wallMat;
    private Material _floorMat;
    private Material _ceilMat;
    private Material _flameMat;
    private Material _handleMat;

    private Transform _mazeRoot;
    private Transform _torchRoot;
    private GameObject _exitDoor;

    private static Shader _litShader;
    private static Shader _unlitShader;

    private bool _isInitialized = false;

    void Awake()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _gen = GetComponent<MazeGenerator>();
        _torchPool = GetComponent<TorchPool>();
        _roomGenerator = GetComponent<RoomGenerator>();
        _doorHolePlacer = GetComponent<DoorHolePlacer>();
        _roomDoorPlacer = GetComponent<RoomDoorPlacer>();
        EnsureDrawingPoolExists();
        EnsureRuntimeStatusUI();
    }

    private void EnsureDrawingPoolExists()
    {
        if (DrawingPool.Instance == null)
        {
            var go = new GameObject("DrawingPool");
            var comp = go.AddComponent<DrawingPool>();
            if (comp == null) Debug.LogWarning("DrawingPool component could not be added");
        }
    }

    void Start()
    {
        // Only auto-build if MazeIntegration is not present
        // MazeIntegration controls the generation order
        var mazeIntegration = GetComponent<MazeIntegration>();
        if (mazeIntegration == null)
        {
            Debug.Log("[MazeRenderer] No MazeIntegration found - auto-building maze");
            BuildMaze();
        }
        else
        {
            Debug.Log("[MazeRenderer] MazeIntegration found - waiting for generation command");
        }
    }

    public void BuildMaze()
    {
        if (_gen == null)
        {
            Debug.LogError("[MazeRenderer] MazeGenerator component not found!");
            return;
        }
        CleanupOld();
        SetupEnvironment();
        _gen.Generate();

        // Generate rooms after maze generation
        if (_roomGenerator != null)
        {
            _roomGenerator.GenerateRooms();
        }

        // Place door holes in room walls
        if (_doorHolePlacer != null)
        {
            _doorHolePlacer.PlaceAllHoles();
        }

        // Place doors in reserved holes
        if (_roomDoorPlacer != null)
        {
            _roomDoorPlacer.PlaceAllDoors();
        }

        CreateMaterials();
        CreateContainers();
        BuildGeometry();
        SpawnPlayer();
        // SpawnExitDoor(); // DISABLED - No exit door
    }

    public void Regenerate() => BuildMaze();

    private void CleanupOld()
    {
        if (_mazeRoot) Destroy(_mazeRoot.gameObject);
        if (_torchRoot) Destroy(_torchRoot.gameObject);
        if (_exitDoor) Destroy(_exitDoor);
        
        // Clean up door holes and placed doors
        if (_doorHolePlacer != null)
        {
            _doorHolePlacer.ClearHoles();
        }
        if (_roomDoorPlacer != null)
        {
            _roomDoorPlacer.ClearPlacedDoors();
        }
        
        _torchPool?.ReleaseAll();
        DestroyMaterial(ref _wallMat);
        DestroyMaterial(ref _floorMat);
        DestroyMaterial(ref _ceilMat);
        DestroyMaterial(ref _flameMat);
        DestroyMaterial(ref _handleMat);
        if (DrawingPool.Instance != null) DrawingPool.Instance.Clear();
    }

    private void DestroyMaterial(ref Material mat)
    {
        if (mat) { Destroy(mat); mat = null; }
    }

    private void CreateMaterials()
    {
        if (DrawingPool.Instance == null)
        {
            Debug.LogError("[MazeRenderer] DrawingPool.Instance is null! Cannot create materials.");
            return;
        }
        uint seed = _gen?.CurrentSeed ?? 0u;
        if (!DrawingPool.Instance.IsInitialized || DrawingPool.Instance.CurrentSeed != seed)
        {
            DrawingPool.Instance.Initialize(seed);
        }
        InitShaders();
        var wallTex = DrawingPool.Instance.GetWall(0);
        _wallMat = MakeMaterial(wallTex, cellSize / 2f, wallHeight / 2f);
        var floorTex = DrawingPool.Instance.GetFloor(0);
        _floorMat = MakeMaterial(floorTex, 1f, 1f);
        var ceilTex = DrawingPool.Instance.GetCeiling(0);
        _ceilMat = MakeMaterial(ceilTex, 1f, 1f);
        _handleMat = MakeMaterial(DrawingPool.Instance.GetTorch(0), 1f, 1f);
        var flameFrames = DrawingPool.Instance.GetFlameTextures();
        if (flameFrames != null && flameFrames.Length > 0)
            _flameMat = MakeTransparentMaterial(flameFrames[0]);
        // else skip flame mat; warnings may be emitted by caller
    }

    private static void InitShaders()
    {
        _litShader ??= Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("URP/Lit") ?? Shader.Find("Standard");
        _unlitShader ??= Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("URP/Unlit") ?? Shader.Find("Unlit/Transparent");
        if (_unlitShader == null) _unlitShader = Shader.Find("Sprites/Default");
        if (_litShader == null) _litShader = Shader.Find("Standard");
    }

    private Material MakeMaterial(Texture2D tex, float tilingX, float tilingY, bool transparent = false)
    {
        if (!_litShader) return null;
        if (!tex) return null;
        var mat = new Material(_litShader) { mainTexture = tex, mainTextureScale = new Vector2(tilingX, tilingY) };
        mat.SetFloat("_Smoothness", 0.1f);  // Reduced from 0f to minimize texture artifacts
        mat.SetFloat("_Metallic", 0f);
        
        if (transparent)
        {
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        else
        {
            mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        
        return mat;
    }

    private Material MakeTransparentMaterial(Texture2D tex)
    {
        if (!_unlitShader) return null;
        if (!tex) return null;
        var mat = new Material(_unlitShader) { mainTexture = tex, renderQueue = 3000 };
        mat.SetFloat("_SurfaceType", 1f);
        mat.SetFloat("_BlendMode", 0f);
        return mat;
    }

    private void CreateContainers()
    {
        _mazeRoot = new GameObject("MazeGeometry").transform;
        _torchRoot = new GameObject("Torches").transform;
    }

    private List<(Vector3, Quaternion)> _cachedWallFaces;

    private void BuildGeometry()
    {
        _cachedWallFaces = new List<(Vector3, Quaternion)>();
        for (int x = 0; x < _gen.Width; x++)
            for (int y = 0; y < _gen.Height; y++)
            {
                float cx = (x + 0.5f) * cellSize;
                float cz = (y + 0.5f) * cellSize;
                float wx = x * cellSize;
                float wz = y * cellSize;

                // Floor - quad facing up (with material check)
                if (_floorMat != null)
                {
                    CreateQuad(new Vector3(cx, 0f, cz), Quaternion.Euler(90f, 0f, 0f), cellSize, cellSize, _floorMat);
                }
                else
                {
                    Debug.LogWarning($"[MazeRenderer] Floor material is null at ({x},{y})! Using fallback.");
                    CreateQuad(new Vector3(cx, 0f, cz), Quaternion.Euler(90f, 0f, 0f), cellSize, cellSize, GetDefaultWhiteMaterial());
                }

                // Ceiling - use solid cube instead of quad to prevent light bleeding
                if (_ceilMat != null)
                {
                    CreateCeiling(new Vector3(cx, wallHeight, cz), cellSize, _ceilMat);
                }
                else
                {
                    CreateCeiling(new Vector3(cx, wallHeight, cz), cellSize, GetDefaultWhiteMaterial());
                }

                // Walls
                if (_gen.HasWall(x, y, MazeGenerator.Wall.North)) { var p = new Vector3(cx, wallHeight * .5f, wz + cellSize); var r = Quaternion.Euler(0f, 180f, 0f); CreateWall(p, r); _cachedWallFaces.Add((p, r)); }
                if (_gen.HasWall(x, y, MazeGenerator.Wall.South)) { var p = new Vector3(cx, wallHeight * .5f, wz); var r = Quaternion.identity; CreateWall(p, r); _cachedWallFaces.Add((p, r)); }
                if (_gen.HasWall(x, y, MazeGenerator.Wall.East)) { var p = new Vector3(wx + cellSize, wallHeight * .5f, cz); var r = Quaternion.Euler(0f, -90f, 0f); CreateWall(p, r); _cachedWallFaces.Add((p, r)); }
                if (_gen.HasWall(x, y, MazeGenerator.Wall.West)) { var p = new Vector3(wx, wallHeight * .5f, cz); var r = Quaternion.Euler(0f, 90f, 0f); CreateWall(p, r); _cachedWallFaces.Add((p, r)); }
            }
        // Torches are placed by external plug-in (MazeTorchPlacer.cs) when called
    }

    private void CreateCeiling(Vector3 pos, float size, Material mat)
    {
        // Use a thin cube instead of quad - blocks light properly
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size, 0.2f, size);  // Thin but solid
        
        // Use material if available, otherwise use fallback
        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = (mat != null) ? mat : GetDefaultWhiteMaterial();
        }
        
        go.isStatic = true;
    }

    private void CreateWall(Vector3 pos, Quaternion rot)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetPositionAndRotation(pos, rot);
        go.transform.localScale = new Vector3(cellSize, wallHeight, 0.3f);
        
        // Use wall material if available, otherwise use fallback
        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = (_wallMat != null) ? _wallMat : GetDefaultWhiteMaterial();
        }
        
        go.isStatic = true;
    }

    private void CreateQuad(Vector3 pos, Quaternion rot, float w, float h, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.transform.SetPositionAndRotation(pos, rot);
        go.transform.localScale = new Vector3(w, h, 1f);
        
        // Use material if available, otherwise use default white
        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = (mat != null) ? mat : GetDefaultWhiteMaterial();
        }
        
        go.isStatic = true;
    }

    private static Material _defaultWhiteMat;
    private Material GetDefaultWhiteMaterial()
    {
        if (_defaultWhiteMat == null)
        {
            _defaultWhiteMat = new Material(Shader.Find("Standard"));
            _defaultWhiteMat.color = Color.white;
            _defaultWhiteMat.SetFloat("_Smoothness", 0.5f);
        }
        return _defaultWhiteMat;
    }

    /// <summary>
    /// Place torches on maze walls (plug-in system).
    /// Call this independently after BuildMaze() to add torches.
    /// Torches are NOT placed automatically during maze generation.
    /// </summary>
    [ContextMenu("Place Torches")]
    public void PlaceTorches()
    {
        if (_cachedWallFaces == null || _cachedWallFaces.Count == 0)
        {
            Debug.LogWarning("[MazeRenderer] No wall faces cached. Call BuildMaze() first.");
            return;
        }

        PlaceTorchesInternal(_cachedWallFaces);
    }

    private void PlaceTorchesInternal(List<(Vector3, Quaternion)> wallFaces)
    {
        if (DrawingPool.Instance == null) return;

        // Shuffle wall faces for random distribution
        for (int i = wallFaces.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (wallFaces[i], wallFaces[j]) = (wallFaces[j], wallFaces[i]);
        }

        var placed = new List<Vector3>();
        float minDist = cellSize * 1.0f;  // Reduced spacing for more torches
        var frames = DrawingPool.Instance.GetFlameTextures();
        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning("[MazeRenderer] No flame textures available for torches!");
            return;
        }

        int torchesPlaced = 0;
        foreach (var (wallPos, wallRot) in wallFaces)
        {
            // 33% chance for torch on each wall segment
            if (Random.value > torchProbability) continue;

            // Ensure minimum spacing between torches
            if (placed.Exists(p => Vector3.Distance(wallPos, p) < minDist)) continue;

            Vector3 inward = wallRot * Vector3.forward;
            float torchY = torchHeightRatio * wallHeight;
            Vector3 torchPos = new Vector3(wallPos.x, torchY, wallPos.z) + inward * 0.35f;

            _torchPool.Get(torchPos, wallRot, _torchRoot, frames, _flameMat, _handleMat);
            placed.Add(wallPos);
            torchesPlaced++;
        }

        Debug.Log($"[MazeRenderer] Placed {torchesPlaced} torches on {wallFaces.Count} wall faces ({torchProbability * 100f:F0}% probability)");
    }

    private void SetupEnvironment()
    {
        // Disable realtime global illumination to prevent light artifacts
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor * ambientIntensity;
        
        // Fog for atmosphere and to hide draw distance
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.08f, 0.05f, 0.03f);
        RenderSettings.fogStartDistance = cellSize * 2f;
        RenderSettings.fogEndDistance = cellSize * 15f;
        
        // Disable realtime reflections
        RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
        RenderSettings.reflectionBounces = 1;
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPos = new Vector3((_gen.StartCell.x + 0.5f) * cellSize, playerSpawnOffsetY, (_gen.StartCell.y + 0.5f) * cellSize);
        if (playerPrefab) Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        else
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player) player.transform.position = spawnPos;
        }
    }

    private void SpawnExitDoor()
    {
        // DISABLED - Doors are now handled by DoorFactory and DoorsEngine
        // Legacy door spawning removed
        Debug.Log("[MazeRenderer] Exit door spawning disabled - use DoorFactory instead");
    }

    private void EnsureRuntimeStatusUI()
    {
        // RuntimeStatusUI type not required for Unity6 baseline
    }
}
}
