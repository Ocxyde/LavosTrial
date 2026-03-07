// RoomDoorPlacer.cs
// Places doors on room walls with random textures
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Plug-in-and-Out: Integrates with RoomGenerator and DoorFactory

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Places procedural doors on room walls.
    /// Each door gets a random wall texture variant.
    /// Integrates with RoomGenerator and DoorFactory.
    /// </summary>
    public class RoomDoorPlacer : MonoBehaviour
    {
        [Header("Door Placement")]
        [SerializeField] private bool placeDoorsOnRoomWalls = true;
        [SerializeField] private float doorChancePerWall = 0.6f;
        [SerializeField] private DoorVariant[] availableVariants;
        [SerializeField] private DoorTrapType[] availableTrapTypes;

        [Header("Wall Texture")]
        [SerializeField] private WallTextureSet[] wallTextureSets;
        [SerializeField] private bool randomizeWallTextures = true;

        [Header("References")]
        [SerializeField] private RoomGenerator roomGenerator;
        [SerializeField] private MazeGenerator mazeGenerator;

        private List<GameObject> _placedDoors = new();
        private Dictionary<Vector2Int, Material> _wallTextureCache = new();

        public List<GameObject> PlacedDoors => _placedDoors;
        public int DoorCount => _placedDoors.Count;

        private void Awake()
        {
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
        /// Place doors on all room walls.
        /// Call after room generation.
        /// </summary>
        [ContextMenu("Place Doors on Room Walls")]
        public void PlaceAllDoors()
        {
            if (!placeDoorsOnRoomWalls)
            {
                Debug.Log("[RoomDoorPlacer] Door placement disabled");
                return;
            }

            if (roomGenerator == null || roomGenerator.GeneratedRooms == null)
            {
                Debug.LogError("[RoomDoorPlacer] RoomGenerator not initialized!");
                return;
            }

            ClearPlacedDoors();

            Debug.Log("[RoomDoorPlacer] Starting door placement on room walls...");

            foreach (var room in roomGenerator.GeneratedRooms)
            {
                PlaceDoorsOnRoom(room);
            }

            Debug.Log($"[RoomDoorPlacer] Placed {_placedDoors.Count} doors on room walls");
        }

        /// <summary>
        /// Place doors on a specific room's walls.
        /// </summary>
        private void PlaceDoorsOnRoom(RoomData room)
        {
            // Get wall segments for this room
            List<WallSegment> wallSegments = GetRoomWallSegments(room);

            foreach (var wall in wallSegments)
            {
                // Check if we should place a door on this wall
                if (Random.value > doorChancePerWall)
                    continue;

                // Don't place door on room entrances (keep them open)
                if (IsRoomEntrance(wall.Position, room))
                    continue;

                // Place door
                PlaceDoorAtWall(wall, room);
            }
        }

        /// <summary>
        /// Get all wall segments for a room.
        /// </summary>
        private List<WallSegment> GetRoomWallSegments(RoomData room)
        {
            List<WallSegment> walls = new();

            int startX = room.Position.x;
            int startY = room.Position.y;
            int endX = startX + room.Width;
            int endY = startY + room.Height;

            // North wall (bottom in grid coordinates)
            for (int x = startX; x < endX; x++)
            {
                walls.Add(new WallSegment
                {
                    Position = new Vector2Int(x, startY),
                    Direction = WallDirection.North,
                    Room = room
                });
            }

            // South wall (top in grid coordinates)
            for (int x = startX; x < endX; x++)
            {
                walls.Add(new WallSegment
                {
                    Position = new Vector2Int(x, endY - 1),
                    Direction = WallDirection.South,
                    Room = room
                });
            }

            // West wall (left)
            for (int y = startY; y < endY; y++)
            {
                walls.Add(new WallSegment
                {
                    Position = new Vector2Int(startX, y),
                    Direction = WallDirection.West,
                    Room = room
                });
            }

            // East wall (right)
            for (int y = startY; y < endY; y++)
            {
                walls.Add(new WallSegment
                {
                    Position = new Vector2Int(endX - 1, y),
                    Direction = WallDirection.East,
                    Room = room
                });
            }

            return walls;
        }

        /// <summary>
        /// Check if a wall position is a room entrance.
        /// </summary>
        private bool IsRoomEntrance(Vector2Int position, RoomData room)
        {
            return room.Entrances.Contains(position);
        }

        /// <summary>
        /// Place a door at a specific wall segment.
        /// </summary>
        private void PlaceDoorAtWall(WallSegment wall, RoomData room)
        {
            // Calculate world position
            Vector3 worldPos = GridToWorldPosition(wall.Position);

            // Determine rotation based on wall direction
            Quaternion rotation = GetWallRotation(wall.Direction);

            // Select random variant and trap type
            DoorVariant variant = availableVariants[Random.Range(0, availableVariants.Length)];
            DoorTrapType trapType = availableTrapTypes[Random.Range(0, availableTrapTypes.Length)];

            // Create door using DoorFactory
            GameObject door = DoorFactory.CreateDoor(
                worldPos,
                rotation,
                variant,
                trapType,
                cellSize,
                wallHeight
            );

            if (door != null)
            {
                _placedDoors.Add(door);

                // Apply random wall texture if enabled
                if (randomizeWallTextures && wallTextureSets != null && wallTextureSets.Length > 0)
                {
                    ApplyRandomWallTexture(door, wall.Position);
                }

                Debug.Log($"[RoomDoorPlacer] Placed {variant} door at ({wall.Position.x}, {wall.Position.y})");
            }
        }

        /// <summary>
        /// Apply random wall texture to door.
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
        /// Convert grid position to world position.
        /// </summary>
        private Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            return new Vector3(
                gridPos.x * cellSize + cellSize / 2f,
                0f,
                gridPos.y * cellSize + cellSize / 2f
            );
        }

        /// <summary>
        /// Get rotation for wall direction.
        /// </summary>
        private Quaternion GetWallRotation(WallDirection direction)
        {
            switch (direction)
            {
                case WallDirection.North:
                    return Quaternion.Euler(0f, 0f, 0f);
                case WallDirection.South:
                    return Quaternion.Euler(0f, 180f, 0f);
                case WallDirection.West:
                    return Quaternion.Euler(0f, 90f, 0f);
                case WallDirection.East:
                    return Quaternion.Euler(0f, -90f, 0f);
                default:
                    return Quaternion.identity;
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

            _wallTextureCache.Clear();
        }

        #region Debug

        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos || roomGenerator == null || roomGenerator.GeneratedRooms == null)
                return;

            // Draw room bounds
            Gizmos.color = Color.blue;
            foreach (var room in roomGenerator.GeneratedRooms)
            {
                Vector3 center = GridToWorldPosition(room.Position) +
                                new Vector3(room.Width * cellSize / 2f, 0f, room.Height * cellSize / 2f);

                Vector3 size = new Vector3(room.Width * cellSize, wallHeight, room.Height * cellSize);

                Gizmos.DrawWireCube(center, size);
            }

            // Draw placed doors
            Gizmos.color = Color.yellow;
            if (_placedDoors != null)
            {
                foreach (var door in _placedDoors)
                {
                    if (door != null)
                    {
                        Gizmos.DrawSphere(door.transform.position, 0.3f);
                    }
                }
            }
        }

        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private float cellSize = 4f;
        [SerializeField] private float wallHeight = 3f;

        #endregion
    }

    /// <summary>
    /// Wall segment data for door placement.
    /// </summary>
    [System.Serializable]
    public class WallSegment
    {
        public Vector2Int Position;
        public WallDirection Direction;
        public RoomData Room;
    }

    /// <summary>
    /// Wall direction enum.
    /// </summary>
    public enum WallDirection
    {
        North,
        South,
        East,
        West
    }

    /// <summary>
    /// Door variant types.
    /// </summary>
    public enum DoorVariant
    {
        Wood,
        Stone,
        Metal,
        Magic,
        Iron
    }

    /// <summary>
    /// Door trap types.
    /// </summary>
    public enum DoorTrapType
    {
        None,
        Poison,
        Fire,
        Lightning,
        Freeze
    }

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
