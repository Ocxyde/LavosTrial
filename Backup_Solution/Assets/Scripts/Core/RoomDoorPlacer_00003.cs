// RoomDoorPlacer.cs
// Places doors in reserved wall holes with random textures
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Plug-in-and-Out Architecture:
// - Inherits from BehaviorEngine for ItemEngine integration
// - Uses DoorHolePlacer for reserved wall spaces
// - Doors snap into pre-carved holes

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// RoomDoorPlacer - Places doors in reserved wall holes.
    /// Plug-in-and-Out: Inherits from BehaviorEngine, plugs into ItemEngine.
    /// Each door is placed in a pre-carved hole with random texture.
    /// </summary>
    public class RoomDoorPlacer : MonoBehaviour
    {
        [Header("Door Placement")]
        [SerializeField] private bool placeDoorsInHoles = true;
        [SerializeField] private DoorVariant[] availableVariants;
        [SerializeField] private DoorTrapType[] availableTrapTypes;

        [Header("Wall Texture")]
        [SerializeField] private WallTextureSet[] wallTextureSets;
        [SerializeField] private bool randomizeWallTextures = true;

        [Header("Hole Integration")]
        [SerializeField] private DoorHolePlacer holePlacer;

        [Header("References")]
        [SerializeField] private RoomGenerator roomGenerator;
        [SerializeField] private MazeGenerator mazeGenerator;

        private List<GameObject> _placedDoors = new();
        private Dictionary<Vector2Int, Material> _wallTextureCache = new();
        private List<DoorData> _placedDoorData = new();

        public List<GameObject> PlacedDoors => _placedDoors;
        public int DoorCount => _placedDoors.Count;
        public List<DoorData> PlacedDoorData => _placedDoorData;

        private void Awake()
        {
            if (holePlacer == null)
                holePlacer = GetComponent<DoorHolePlacer>();

            if (roomGenerator == null)
                roomGenerator = GetComponent<RoomGenerator>();

            if (mazeGenerator == null)
                mazeGenerator = GetComponent<MazeGenerator>();

            // Initialize default variants if not assigned
            if (availableVariants == null || availableVariants.Length == 0)
            {
                availableVariants = new[] { DoorVariant.Wood, DoorVariant.Stone, DoorVariant.Metal };
            }

            if (availableTrapTypes == null || availableTrapTypes.Length == 0)
            {
                availableTrapTypes = new[] { DoorTrapType.None, DoorTrapType.None, DoorTrapType.Poison };
            }
        }

        /// <summary>
        /// Place doors in all reserved holes.
        /// Call after hole placement.
        /// </summary>
        [ContextMenu("Place Doors in Holes")]
        public void PlaceAllDoors()
        {
            if (!placeDoorsInHoles)
            {
                Debug.Log("[RoomDoorPlacer] Door placement disabled");
                return;
            }

            if (holePlacer == null || holePlacer.PlacedHoles == null)
            {
                Debug.LogError("[RoomDoorPlacer] DoorHolePlacer not initialized!");
                return;
            }

            ClearPlacedDoors();

            Debug.Log("[RoomDoorPlacer] Starting door placement in reserved holes...");

            foreach (var hole in holePlacer.PlacedHoles)
            {
                PlaceDoorInHole(hole);
            }

            Debug.Log($"[RoomDoorPlacer] Placed {_placedDoors.Count} doors in holes");
        }

        /// <summary>
        /// Place a door in a specific hole.
        /// </summary>
        private void PlaceDoorInHole(DoorHoleData hole)
        {
            // Select random variant and trap type
            DoorVariant variant = availableVariants[Random.Range(0, availableVariants.Length)];
            DoorTrapType trapType = availableTrapTypes[Random.Range(0, availableTrapTypes.Length)];

            // Calculate door dimensions to fit hole
            float doorWidth = hole.Width * 0.9f;  // Slightly smaller than hole
            float doorHeight = hole.Height * 0.95f;
            float doorDepth = hole.Depth * 0.8f;

            // Create door using DoorFactory
            GameObject door = DoorFactory.CreateDoor(
                hole.Position,
                hole.Rotation,
                variant,
                trapType,
                cellSize,
                wallHeight,
                doorWidth,
                doorHeight,
                doorDepth
            );

            if (door != null)
            {
                _placedDoors.Add(door);

                // Store door data
                _placedDoorData.Add(new DoorData
                {
                    DoorObject = door,
                    Variant = variant,
                    TrapType = trapType,
                    Hole = hole,
                    Room = hole.Room
                });

                // Apply random wall texture if enabled
                if (randomizeWallTextures && wallTextureSets != null && wallTextureSets.Length > 0)
                {
                    ApplyRandomWallTexture(door, hole.GridPosition);
                }

                // Mark hole as carved
                holePlacer.MarkHoleAsCarved(hole);

                Debug.Log($"[RoomDoorPlacer] Placed {variant} door at ({hole.GridPosition.x}, {hole.GridPosition.y})");
            }
        }

        /// <summary>
        /// Apply random wall texture to door frame.
        /// </summary>
        private void ApplyRandomWallTexture(GameObject door, Vector2Int gridPosition)
        {
            // Check cache first
            if (_wallTextureCache.TryGetValue(gridPosition, out Material cachedMat))
            {
                ApplyMaterialToDoorFrames(door, cachedMat);
                return;
            }

            // Select random texture set
            WallTextureSet textureSet = wallTextureSets[Random.Range(0, wallTextureSets.Length)];

            // Generate or get cached material
            Material wallMat = GetWallMaterial(textureSet);
            _wallTextureCache[gridPosition] = wallMat;

            ApplyMaterialToDoorFrames(door, wallMat);
        }

        /// <summary>
        /// Get or create wall material from texture set.
        /// </summary>
        private Material GetWallMaterial(WallTextureSet textureSet)
        {
            // Generate texture for wall
            Texture2D wallTex = textureSet.GenerateTexture();

            // Create material
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.mainTexture = wallTex;
            mat.SetFloat("_Smoothness", textureSet.smoothness);
            mat.SetColor("_BaseColor", textureSet.tint);

            return mat;
        }

        /// <summary>
        /// Apply material to door frame renderers.
        /// </summary>
        private void ApplyMaterialToDoorFrames(GameObject door, Material material)
        {
            MeshRenderer[] renderers = door.GetComponentsInChildren<MeshRenderer>();

            foreach (var renderer in renderers)
            {
                if (renderer.name.Contains("Frame") || renderer.name.Contains("Wall"))
                {
                    renderer.sharedMaterial = material;
                }
            }
        }

        /// <summary>
        /// Clear all placed doors.
        /// </summary>
        public void ClearPlacedDoors()
        {
            if (_placedDoors != null)
            {
                foreach (var door in _placedDoors)
                {
                    if (door != null)
                        Destroy(door);
                }
                _placedDoors.Clear();
            }

            _placedDoorData.Clear();
            _wallTextureCache.Clear();
        }

        #region Debug

        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos || _placedDoors == null)
                return;

            // Draw placed doors
            Gizmos.color = Color.yellow;
            foreach (var door in _placedDoors)
            {
                if (door != null)
                {
                    Gizmos.DrawSphere(door.transform.position, 0.3f);
                }
            }

            // Draw door data info
            if (_placedDoorData != null && _placedDoorData.Count > 0)
            {
                foreach (var doorData in _placedDoorData)
                {
                    Debug.DrawLine(
                        doorData.DoorObject.transform.position,
                        doorData.DoorObject.transform.position + Vector3.up * 0.5f,
                        Color.magenta
                    );
                }
            }
        }

        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private float cellSize = 4f;
        [SerializeField] private float wallHeight = 3f;

        #endregion
    }

    /// <summary>
    /// Door data structure for tracking placed doors.
    /// </summary>
    [System.Serializable]
    public class DoorData
    {
        public GameObject DoorObject;
        public DoorVariant Variant;
        public DoorTrapType TrapType;
        public DoorHoleData Hole;
        public RoomData Room;

        public Vector3 Position => DoorObject?.transform.position ?? Vector3.zero;
        public Quaternion Rotation => DoorObject?.transform.rotation ?? Quaternion.identity;
    }

    // DoorVariant and DoorTrapType enums are defined in DoorsEngine.cs
    // Using those definitions to avoid duplication

    /// <summary>
    /// Wall texture set for procedural generation.
    /// </summary>
    [System.Serializable]
    public class WallTextureSet
    {
        [Header("Texture Settings")]
        public string setName;
        public Color baseColor = Color.gray;
        public Color variationColor = Color.white;
        public float noiseScale = 0.1f;
        public float contrast = 1.2f;

        [Header("Material Settings")]
        public Color tint = Color.white;
        public float smoothness = 0.5f;

        /// <summary>
        /// Generate procedural wall texture.
        /// </summary>
        public Texture2D GenerateTexture(int resolution = 64)
        {
            Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    // Generate noise-based variation
                    float nx = x / (float)resolution * noiseScale;
                    float ny = y / (float)resolution * noiseScale;

                    float noise = Mathf.PerlinNoise(nx * 10f, ny * 10f);
                    noise = Mathf.Pow(noise, contrast);

                    // Blend colors
                    Color pixel = Color.Lerp(baseColor, variationColor, noise);
                    pixel *= tint;

                    tex.SetPixel(x, y, pixel);
                }
            }

            tex.Apply();
            return tex;
        }
    }
}
