// DoorHolePlacer.cs
// Reserves and creates holes in room walls for door placement
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Plug-in-and-Out Architecture:
// - Reserves wall space during room generation
// - Creates holes in walls for doors to fit
// - Doors snap into reserved holes

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// DoorHolePlacer - Reserves wall spaces for doors.
    /// Plug into RoomGenerator system.
    /// Each hole is a reserved space in a wall where a door will fit.
    /// </summary>
    public class DoorHolePlacer : MonoBehaviour
    {
        [Header("Hole Settings")]
        [SerializeField] private float holeWidth = 2.5f;      // Door width + frame
        [SerializeField] private float holeHeight = 3f;       // Door height + frame
        [SerializeField] private float holeDepth = 0.5f;      // Wall thickness
        [SerializeField] private float doorChancePerWall = 0.6f;

        [Header("Wall Integration")]
        [SerializeField] private bool carveHolesInWalls = true;
        [SerializeField] private bool showDebugGizmos = true;

        // Public accessors for editor script
        public float HoleWidth { get => holeWidth; set => holeWidth = value; }
        public float HoleHeight { get => holeHeight; set => holeHeight = value; }
        public float HoleDepth { get => holeDepth; set => holeDepth = value; }
        public float DoorChancePerWall { get => doorChancePerWall; set => doorChancePerWall = value; }
        public bool CarveHolesInWalls { get => carveHolesInWalls; set => carveHolesInWalls = value; }

        [Header("References")]
        [SerializeField] private RoomGenerator roomGenerator;
        [SerializeField] private MazeGenerator mazeGenerator;

        private List<DoorHoleData> _placedHoles = new();
        private Dictionary<Vector2Int, List<DoorHoleData>> _holesByCell = new();

        public List<DoorHoleData> PlacedHoles => _placedHoles;
        public int HoleCount => _placedHoles.Count;

        private void Awake()
        {
            if (roomGenerator == null)
                roomGenerator = GetComponent<RoomGenerator>();

            if (mazeGenerator == null)
                mazeGenerator = GetComponent<MazeGenerator>();
        }

        /// <summary>
        /// Place door holes in all room walls.
        /// Call after room generation, before wall geometry build.
        /// </summary>
        [ContextMenu("Place Door Holes")]
        public void PlaceAllHoles()
        {
            if (!carveHolesInWalls)
            {
                Debug.Log("[DoorHolePlacer] Hole carving disabled");
                return;
            }

            if (roomGenerator == null || roomGenerator.GeneratedRooms == null)
            {
                Debug.LogError("[DoorHolePlacer] RoomGenerator not initialized!");
                return;
            }

            ClearHoles();

            Debug.Log("[DoorHolePlacer] Starting hole placement in room walls...");

            foreach (var room in roomGenerator.GeneratedRooms)
            {
                PlaceHolesInRoom(room);
            }

            Debug.Log($"[DoorHolePlacer] Placed {_placedHoles.Count} door holes");
        }

        /// <summary>
        /// Place holes in a specific room's walls.
        /// </summary>
        private void PlaceHolesInRoom(RoomData room)
        {
            List<WallSegment> wallSegments = GetRoomWallSegments(room);

            foreach (var wall in wallSegments)
            {
                // Skip room entrances (they're already open)
                if (IsRoomEntrance(wall.Position, room))
                    continue;

                // Check if we should place a hole on this wall segment
                if (Random.value > doorChancePerWall)
                    continue;

                // Place hole
                DoorHoleData hole = CreateHoleAtWall(wall, room);
                if (hole != null)
                {
                    _placedHoles.Add(hole);

                    // Register in cell dictionary
                    if (!_holesByCell.ContainsKey(wall.Position))
                        _holesByCell[wall.Position] = new List<DoorHoleData>();
                    _holesByCell[wall.Position].Add(hole);
                }
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

            // North wall
            for (int x = startX; x < endX; x++)
            {
                walls.Add(new WallSegment
                {
                    Position = new Vector2Int(x, startY),
                    Direction = WallDirection.North,
                    Room = room,
                    CellSize = mazeGenerator != null ? mazeGenerator.Width : 1
                });
            }

            // South wall
            for (int x = startX; x < endX; x++)
            {
                walls.Add(new WallSegment
                {
                    Position = new Vector2Int(x, endY - 1),
                    Direction = WallDirection.South,
                    Room = room,
                    CellSize = mazeGenerator != null ? mazeGenerator.Width : 1
                });
            }

            // West wall
            for (int y = startY; y < endY; y++)
            {
                walls.Add(new WallSegment
                {
                    Position = new Vector2Int(startX, y),
                    Direction = WallDirection.West,
                    Room = room,
                    CellSize = mazeGenerator != null ? mazeGenerator.Width : 1
                });
            }

            // East wall
            for (int y = startY; y < endY; y++)
            {
                walls.Add(new WallSegment
                {
                    Position = new Vector2Int(endX - 1, y),
                    Direction = WallDirection.East,
                    Room = room,
                    CellSize = mazeGenerator != null ? mazeGenerator.Width : 1
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
        /// Create a door hole at a wall segment.
        /// </summary>
        private DoorHoleData CreateHoleAtWall(WallSegment wall, RoomData room)
        {
            // Calculate world position (centered on wall cell)
            Vector3 worldPos = GridToWorldPosition(wall.Position);

            // Adjust position based on wall direction (hole is in the wall face)
            Vector3 holePos = AdjustPositionForWallDirection(worldPos, wall.Direction);

            // Get rotation for door alignment
            Quaternion rotation = GetWallRotation(wall.Direction);

            return new DoorHoleData
            {
                Position = holePos,
                Rotation = rotation,
                Width = holeWidth,
                Height = holeHeight,
                Depth = holeDepth,
                WallDirection = wall.Direction,
                Room = room,
                GridPosition = wall.Position,
                IsCarved = false
            };
        }

        /// <summary>
        /// Adjust position based on wall direction.
        /// </summary>
        private Vector3 AdjustPositionForWallDirection(Vector3 pos, WallDirection direction)
        {
            float offset = cellSize / 2f - 0.15f; // Half cell, minus half wall thickness

            return direction switch
            {
                WallDirection.North => pos + Vector3.forward * offset,
                WallDirection.South => pos - Vector3.forward * offset,
                WallDirection.East => pos - Vector3.right * offset,
                WallDirection.West => pos + Vector3.right * offset,
                _ => pos
            };
        }

        /// <summary>
        /// Get rotation for wall direction.
        /// </summary>
        private Quaternion GetWallRotation(WallDirection direction)
        {
            return direction switch
            {
                WallDirection.North => Quaternion.Euler(0f, 180f, 0f),
                WallDirection.South => Quaternion.Euler(0f, 0f, 0f),
                WallDirection.East => Quaternion.Euler(0f, -90f, 0f),
                WallDirection.West => Quaternion.Euler(0f, 90f, 0f),
                _ => Quaternion.identity
            };
        }

        /// <summary>
        /// Check if a cell has a door hole.
        /// </summary>
        public bool HasHoleAtCell(Vector2Int cell)
        {
            return _holesByCell.ContainsKey(cell) && _holesByCell[cell].Count > 0;
        }

        /// <summary>
        /// Get hole at a specific cell.
        /// </summary>
        public DoorHoleData GetHoleAtCell(Vector2Int cell)
        {
            if (_holesByCell.TryGetValue(cell, out var holes) && holes.Count > 0)
            {
                return holes[0];
            }
            return null;
        }

        /// <summary>
        /// Mark a hole as carved (for wall geometry).
        /// </summary>
        public void MarkHoleAsCarved(DoorHoleData hole)
        {
            hole.IsCarved = true;
        }

        /// <summary>
        /// Clear all placed holes.
        /// </summary>
        public void ClearHoles()
        {
            _placedHoles.Clear();
            _holesByCell.Clear();
        }

        #region Utilities

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

        [SerializeField] private float cellSize = 4f;

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos || _placedHoles == null)
                return;

            // Draw holes
            Gizmos.color = Color.cyan;
            foreach (var hole in _placedHoles)
            {
                Gizmos.matrix = Matrix4x4.TRS(hole.Position, hole.Rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(holeWidth, holeHeight, holeDepth));
            }

            // Draw hole centers
            Gizmos.color = Color.yellow;
            foreach (var hole in _placedHoles)
            {
                Gizmos.DrawSphere(hole.Position, 0.2f);
            }
        }

        #endregion
    }

    /// <summary>
    /// Door hole data structure.
    /// </summary>
    [System.Serializable]
    public class DoorHoleData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float Width;
        public float Height;
        public float Depth;
        public WallDirection WallDirection;
        public RoomData Room;
        public Vector2Int GridPosition;
        public bool IsCarved;

        /// <summary>
        /// Get the bounds of this hole in world space.
        /// </summary>
        public Bounds GetWorldBounds()
        {
            Vector3 size = new Vector3(Width, Height, Depth);
            return new Bounds(Position, size);
        }

        /// <summary>
        /// Check if a door fits in this hole.
        /// </summary>
        public bool DoorFits(float doorWidth, float doorHeight, float doorDepth)
        {
            return doorWidth <= Width &&
                   doorHeight <= Height &&
                   doorDepth <= Depth;
        }
    }

    /// <summary>
    /// Wall segment data for hole placement.
    /// </summary>
    [System.Serializable]
    public class WallSegment
    {
        public Vector2Int Position;
        public WallDirection Direction;
        public RoomData Room;
        public int CellSize;
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
}
