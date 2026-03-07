// CompleteMazeBuilder.cs
// Unified maze generation - walls, doors (snapped), rooms, ground, ceiling, player spawn
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// FEATURES:
// - Auto-generates on Start (configurable)
// - Walls WITH TEXTURES
// - Ground floor WITH TEXTURES
// - Ceiling WITH TEXTURES
// - Doors snap perfectly to wall gaps
// - Rooms with exactly 1 entrance + 1 exit
// - Player spawn position FIXED (at entrance room)
// - Uses relative paths for all prefabs/materials/textures
// - NO hole traps - clean maze structure
//
// USAGE:
//   1. Add to GameObject in scene
//   2. Tools → Create Maze Prefabs (first time)
//   3. Press Play (auto-generates) OR click "Generate Complete Maze"

using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Code.Lavos.Core
{
    /// <summary>
    /// CompleteMazeBuilder - Auto-generates complete maze with walls, ground, ceiling, doors, rooms.
    /// All paths relative to Assets/. Textures applied to walls/ground/ceiling. Player spawns at entrance.
    /// </summary>
    public class CompleteMazeBuilder : MonoBehaviour
    {
        #region Inspector Settings

        [Header("🏗️ Maze Dimensions")]
        [Tooltip("Maze width in cells (odd number recommended)")]
        [SerializeField] private int mazeWidth = 21;
        [Tooltip("Maze height in cells (odd number recommended)")]
        [SerializeField] private int mazeHeight = 21;
        [Tooltip("Size of each cell in meters")]
        [SerializeField] private float cellSize = 6f;
        [Tooltip("Wall height in meters")]
        [SerializeField] private float wallHeight = 4f;
        [Tooltip("Wall thickness in meters")]
        [SerializeField] private float wallThickness = 0.5f;
        [Tooltip("Ceiling height in meters (above ground)")]
        [SerializeField] private float ceilingHeight = 5f;

        [Header("🚪 Door Settings")]
        [Tooltip("Chance for a passage to have a door (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float doorSpawnChance = 0.6f;
        [Tooltip("Chance for door to be locked (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float lockedDoorChance = 0.3f;
        [Tooltip("Chance for door to be secret (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float secretDoorChance = 0.1f;

        [Header("🏛️ Room Settings")]
        [Tooltip("Generate special rooms (entrance/exit/normal)")]
        [SerializeField] private bool generateRooms = true;
        [Tooltip("Minimum number of rooms")]
        [SerializeField] private int minRooms = 3;
        [Tooltip("Maximum number of rooms")]
        [SerializeField] private int maxRooms = 8;

        [Header("👤 Player Spawn")]
        [Tooltip("Player spawn offset from entrance room center")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1f, 3f);

        [Header("⚙️ Generation Options")]
        [Tooltip("Auto-generate maze on Start")]
        [SerializeField] private bool autoGenerateOnStart = true;
        [Tooltip("Use random seed (false = use manual seed)")]
        [SerializeField] private bool useRandomSeed = true;
        [Tooltip("Manual seed for reproducible generation")]
        [SerializeField] private string manualSeed = "MazeSeed2026";

        [Header("📁 Relative Paths (relative to Assets/)")]
        [SerializeField] private string wallPrefabPath = "Prefabs/WallPrefab.prefab";
        [SerializeField] private string doorPrefabPath = "Prefabs/DoorPrefab.prefab";
        [SerializeField] private string lockedDoorPrefabPath = "Prefabs/LockedDoorPrefab.prefab";
        [SerializeField] private string secretDoorPrefabPath = "Prefabs/SecretDoorPrefab.prefab";
        [SerializeField] private string entranceRoomPrefabPath = "Prefabs/EntranceRoomPrefab.prefab";
        [SerializeField] private string exitRoomPrefabPath = "Prefabs/ExitRoomPrefab.prefab";
        [SerializeField] private string normalRoomPrefabPath = "Prefabs/NormalRoomPrefab.prefab";
        [SerializeField] private string wallMaterialPath = "Materials/WallMaterial.mat";
        [SerializeField] private string doorMaterialPath = "Materials/Door_PïxelArt.mat";
        [SerializeField] private string floorMaterialPath = "Materials/Floor/Stone_Floor.mat";
        [SerializeField] private string groundTexturePath = "Textures/floor_texture.png";
        [SerializeField] private string wallTexturePath = "Textures/wall_texture.png";
        [SerializeField] private string ceilingTexturePath = "Textures/ceiling_texture.png";

        #endregion

        #region Private Fields

        private MazeGenerator mazeGenerator;
        private SpatialPlacer spatialPlacer;
        private LightPlacementEngine lightPlacementEngine;
        private LightEngine lightEngine;
        private TorchPool torchPool;

        private uint currentSeed;
        private System.Random rng;
        
        // Track door positions for proper snapping
        private List<DoorPosition> doorPositions = new List<DoorPosition>();
        
        // Track entrance room position for player spawn
        private Vector3 entranceRoomPosition;
        
        private struct DoorPosition
        {
            public Vector3 position;
            public Quaternion rotation;
            public int x, y;
            public string direction;
            public string type;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Get or create required components
            GetOrAddComponent<MazeGenerator>();
            GetOrAddComponent<SpatialPlacer>();
            GetOrAddComponent<LightPlacementEngine>();
            
            // Find or create LightEngine
            lightEngine = FindFirstObjectByType<LightEngine>();
            if (lightEngine == null)
            {
                var lightGO = new GameObject("LightEngine");
                lightEngine = lightGO.AddComponent<LightEngine>();
            }

            // Find or create TorchPool
            torchPool = FindFirstObjectByType<TorchPool>();
            if (torchPool == null)
            {
                var torchGO = new GameObject("TorchPool");
                torchPool = torchGO.AddComponent<TorchPool>();
            }

            // Initialize seed
            if (useRandomSeed)
            {
                currentSeed = (uint)System.DateTime.Now.Ticks;
            }
            else
            {
                currentSeed = ComputeSeed(manualSeed);
            }
            rng = new System.Random((int)currentSeed);

            Debug.Log("[CompleteMazeBuilder] 🏗️ Component initialized");
            Debug.Log($"[CompleteMazeBuilder] 📏 Maze: {mazeWidth}x{mazeHeight}, Cell: {cellSize}m");
            Debug.Log($"[CompleteMazeBuilder] 🎲 Seed: {currentSeed}");
        }

        private void Start()
        {
            if (autoGenerateOnStart)
            {
                GenerateCompleteMaze();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Generate complete maze with walls, ground, ceiling, doors, rooms.
        /// </summary>
        [ContextMenu("Generate Complete Maze")]
        public void GenerateCompleteMaze()
        {
            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Debug.Log("[CompleteMazeBuilder] 🏗️ Starting maze generation...");
            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");

            doorPositions.Clear();
            entranceRoomPosition = Vector3.zero;

            // Step 1: Generate maze layout
            GenerateMazeLayout();

            // Step 2: Spawn ground floor (with texture)
            SpawnGroundFloor();

            // Step 3: Spawn ceiling (with texture)
            SpawnCeiling();

            // Step 4: Spawn walls (with textures, gaps for doors)
            SpawnWalls();

            // Step 5: Spawn doors (snapped to wall gaps)
            SpawnDoors();

            // Step 6: Spawn rooms (1 entrance + 1 exit each)
            if (generateRooms)
            {
                SpawnRooms();
            }

            // Step 7: Place player at entrance
            PlacePlayerAtSpawn();

            // Step 8: Place torches, chests, enemies, items
            PlaceObjects();

            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Debug.Log("[CompleteMazeBuilder] ✅ Maze generation complete!");
            Debug.Log($"[CompleteMazeBuilder] 📏 Dimensions: {mazeWidth}x{mazeHeight} cells ({mazeWidth * cellSize}m x {mazeHeight * cellSize}m)");
            Debug.Log($"[CompleteMazeBuilder] 🧱 Walls: Generated for {mazeWidth}x{mazeHeight} grid");
            Debug.Log($"[CompleteMazeBuilder] 🚪 Doors: {doorPositions.Count} placed (snapped to wall gaps)");
            Debug.Log($"[CompleteMazeBuilder] 🏛️ Rooms: {minRooms}-{maxRooms} generated (each 1 entrance + 1 exit)");
            Debug.Log($"[CompleteMazeBuilder] 👤 Player spawn: {entranceRoomPosition + spawnOffset}");
            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");
        }

        /// <summary>
        /// Validate all prefab/material/texture paths.
        /// </summary>
        [ContextMenu("Validate Paths")]
        public bool ValidatePaths()
        {
            Debug.Log("[CompleteMazeBuilder] 🔍 Validating paths...");
            bool allValid = true;
            string basePath = Application.dataPath + "/";

            allValid &= ValidatePath(basePath + wallPrefabPath, "Wall Prefab");
            allValid &= ValidatePath(basePath + doorPrefabPath, "Door Prefab");
            allValid &= ValidatePath(basePath + lockedDoorPrefabPath, "Locked Door Prefab");
            allValid &= ValidatePath(basePath + secretDoorPrefabPath, "Secret Door Prefab");
            allValid &= ValidatePath(basePath + wallMaterialPath, "Wall Material");
            allValid &= ValidatePath(basePath + doorMaterialPath, "Door Material");
            allValid &= ValidatePath(basePath + floorMaterialPath, "Floor Material");
            allValid &= ValidatePath(basePath + groundTexturePath, "Ground Texture");
            allValid &= ValidatePath(basePath + wallTexturePath, "Wall Texture");
            allValid &= ValidatePath(basePath + ceilingTexturePath, "Ceiling Texture");

            if (!allValid)
            {
                Debug.LogWarning("[CompleteMazeBuilder] ⚠️ Some paths invalid!");
                Debug.LogWarning("[CompleteMazeBuilder] 💡 Run: Tools → Create Maze Prefabs");
            }
            else
            {
                Debug.Log("[CompleteMazeBuilder] ✅ All paths valid!");
            }

            return allValid;
        }

        #endregion

        #region Generation Methods

        private void GenerateMazeLayout()
        {
            mazeGenerator = GetComponent<MazeGenerator>();
            if (mazeGenerator == null)
            {
                Debug.LogError("[CompleteMazeBuilder] ❌ MazeGenerator component missing!");
                return;
            }

            // Configure maze size
            var widthField = mazeGenerator.GetType().GetField("width", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var heightField = mazeGenerator.GetType().GetField("height", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            widthField?.SetValue(mazeGenerator, mazeWidth);
            heightField?.SetValue(mazeGenerator, mazeHeight);

            // Generate maze
            mazeGenerator.Generate();
            Debug.Log("[CompleteMazeBuilder] ✅ Maze layout generated");
        }

        private void SpawnGroundFloor()
        {
            // Simple ground floor (like ceiling but at bottom)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "GroundFloor";

            // Position: Center the maze at origin (0,0,0)
            // Maze spans from (0,0,0) to (mazeWidth*cellSize, 0, mazeHeight*cellSize)
            float centerX = (mazeWidth * cellSize) / 2f;
            float centerZ = (mazeHeight * cellSize) / 2f;
            
            ground.transform.position = new Vector3(centerX, -0.1f, centerZ);
            ground.transform.localScale = new Vector3(mazeWidth * cellSize, 0.1f, mazeHeight * cellSize);

            // Apply floor material (uses existing Stone_Floor.mat from FloorMaterialFactory)
            ApplyMaterial(ground, floorMaterialPath);

            Debug.Log($"[CompleteMazeBuilder] 🌍 Spawned ground floor at ({centerX}, -0.1, {centerZ}), size: {ground.transform.localScale.x}m x {ground.transform.localScale.z}m");
        }

        private void SpawnCeiling()
        {
            // Create ceiling covering entire maze
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            
            float centerX = (mazeWidth * cellSize) / 2f;
            float centerZ = (mazeHeight * cellSize) / 2f;
            
            ceiling.transform.position = new Vector3(centerX, ceilingHeight, centerZ);
            ceiling.transform.localScale = new Vector3(mazeWidth * cellSize, 0.1f, mazeHeight * cellSize);

            // Apply ceiling material with texture
            ApplyMaterial(ceiling, floorMaterialPath);
            ApplyTexture(ceiling, ceilingTexturePath);

            Debug.Log($"[CompleteMazeBuilder] ☁️ Spawned ceiling at ({centerX}, {ceilingHeight}, {centerZ}), size: {ceiling.transform.localScale.x}m x {ceiling.transform.localScale.z}m");
        }

        private void SpawnWalls()
        {
            var gridField = mazeGenerator.GetType().GetProperty("Grid");
            var grid = gridField?.GetValue(mazeGenerator) as MazeGenerator.Wall[,];

            if (grid == null)
            {
                Debug.LogError("[CompleteMazeBuilder] ❌ Maze grid is null!");
                return;
            }

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            int wallsSpawned = 0;
            int outerWallsSpawned = 0;

            // Step 1: Spawn inner walls from maze generator data
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = grid[x, y];
                    // Cell center position
                    Vector3 cellPos = new Vector3(x * cellSize + cellSize / 2f, wallHeight / 2f, y * cellSize + cellSize / 2f);

                    // Check each wall direction and spawn wall segment
                    // North wall (positive Z)
                    if ((cell & MazeGenerator.Wall.North) != 0)
                    {
                        SpawnWall(cellPos + Vector3.forward * (cellSize / 2f), Quaternion.Euler(0f, 90f, 0f), x, y, "North");
                        wallsSpawned++;
                    }
                    // East wall (positive X)
                    if ((cell & MazeGenerator.Wall.East) != 0)
                    {
                        SpawnWall(cellPos + Vector3.right * (cellSize / 2f), Quaternion.Euler(0f, 0f, 0f), x, y, "East");
                        wallsSpawned++;
                    }
                    // South wall (negative Z)
                    if ((cell & MazeGenerator.Wall.South) != 0)
                    {
                        SpawnWall(cellPos - Vector3.forward * (cellSize / 2f), Quaternion.Euler(0f, 90f, 0f), x, y, "South");
                        wallsSpawned++;
                    }
                    // West wall (negative X)
                    if ((cell & MazeGenerator.Wall.West) != 0)
                    {
                        SpawnWall(cellPos - Vector3.right * (cellSize / 2f), Quaternion.Euler(0f, 0f, 0f), x, y, "West");
                        wallsSpawned++;
                    }
                }
            }

            // Step 2: Add outer perimeter walls (ensure maze is fully enclosed)
            // North boundary (Z = mazeHeight * cellSize)
            for (int x = 0; x < width; x++)
            {
                Vector3 wallPos = new Vector3(x * cellSize + cellSize / 2f, wallHeight / 2f, height * cellSize);
                SpawnWall(wallPos, Quaternion.Euler(0f, 90f, 0f), x, height - 1, "OuterNorth");
                outerWallsSpawned++;
            }
            // South boundary (Z = 0)
            for (int x = 0; x < width; x++)
            {
                Vector3 wallPos = new Vector3(x * cellSize + cellSize / 2f, wallHeight / 2f, 0f);
                SpawnWall(wallPos, Quaternion.Euler(0f, 90f, 0f), x, 0, "OuterSouth");
                outerWallsSpawned++;
            }
            // East boundary (X = mazeWidth * cellSize)
            for (int y = 0; y < height; y++)
            {
                Vector3 wallPos = new Vector3(width * cellSize, wallHeight / 2f, y * cellSize + cellSize / 2f);
                SpawnWall(wallPos, Quaternion.Euler(0f, 0f, 0f), width - 1, y, "OuterEast");
                outerWallsSpawned++;
            }
            // West boundary (X = 0)
            for (int y = 0; y < height; y++)
            {
                Vector3 wallPos = new Vector3(0f, wallHeight / 2f, y * cellSize + cellSize / 2f);
                SpawnWall(wallPos, Quaternion.Euler(0f, 0f, 0f), 0, y, "OuterWest");
                outerWallsSpawned++;
            }

            Debug.Log($"[CompleteMazeBuilder] 🧱 Spawned {wallsSpawned} inner wall segments + {outerWallsSpawned} outer perimeter walls (maze fully enclosed)");
        }

        private void SpawnWall(Vector3 position, Quaternion rotation, int x, int y, string direction)
        {
            // Try to load wall prefab
            GameObject wallPrefab = LoadPrefab(wallPrefabPath);

            GameObject wall;
            if (wallPrefab != null)
            {
                wall = Instantiate(wallPrefab, position, rotation);
            }
            else
            {
                // Fallback: create wall cube
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"Wall_{x}_{y}_{direction}";
                wall.transform.position = position;
                wall.transform.rotation = rotation;
                // Wall dimensions: length=cellSize, height=wallHeight, thickness=wallThickness
                wall.transform.localScale = new Vector3(cellSize, wallHeight, wallThickness);
            }

            wall.name = $"Wall_{x}_{y}_{direction}";

            // Apply material AND texture
            ApplyMaterial(wall, wallMaterialPath);
            ApplyTexture(wall, wallTexturePath);
        }

        private void SpawnDoors()
        {
            var gridField = mazeGenerator.GetType().GetProperty("Grid");
            var grid = gridField?.GetValue(mazeGenerator) as MazeGenerator.Wall[,];

            if (grid == null) return;

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            int doorsSpawned = 0;

            // Spawn doors at passage points (where walls are missing between cells)
            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    var cell = grid[x, y];
                    // Cell center position
                    Vector3 cellPos = new Vector3(x * cellSize + cellSize / 2f, 0f, y * cellSize + cellSize / 2f);

                    // Check if passage to east (no wall = door position)
                    if ((cell & MazeGenerator.Wall.East) == 0)
                    {
                        if (Random.value < doorSpawnChance)
                        {
                            // Door position: between this cell and the cell to the east
                            Vector3 doorPos = new Vector3(
                                (x + 1) * cellSize,  // Boundary between cells
                                wallHeight / 2f,      // Centered vertically on wall
                                y * cellSize + cellSize / 2f  // Same Z as cell center
                            );
                            Quaternion doorRot = Quaternion.Euler(0f, 0f, 0f);

                            // Determine door type
                            string doorType = DetermineDoorType();
                            string prefabPath = GetDoorPrefabPath(doorType);

                            SpawnDoor(doorPos, doorRot, x, y, "East", doorType, prefabPath);
                            doorsSpawned++;

                            // Track for snapping
                            doorPositions.Add(new DoorPosition {
                                position = doorPos,
                                rotation = doorRot,
                                x = x, y = y,
                                direction = "East",
                                type = doorType
                            });
                        }
                    }

                    // Check if passage to south (no wall = door position)
                    if ((cell & MazeGenerator.Wall.South) == 0)
                    {
                        if (Random.value < doorSpawnChance)
                        {
                            // Door position: between this cell and the cell to the south
                            Vector3 doorPos = new Vector3(
                                x * cellSize + cellSize / 2f,  // Same X as cell center
                                wallHeight / 2f,               // Centered vertically on wall
                                y * cellSize                   // Boundary between cells
                            );
                            Quaternion doorRot = Quaternion.Euler(0f, 90f, 0f);

                            // Determine door type
                            string doorType = DetermineDoorType();
                            string prefabPath = GetDoorPrefabPath(doorType);

                            SpawnDoor(doorPos, doorRot, x, y, "South", doorType, prefabPath);
                            doorsSpawned++;

                            // Track for snapping
                            doorPositions.Add(new DoorPosition {
                                position = doorPos,
                                rotation = doorRot,
                                x = x, y = y,
                                direction = "South",
                                type = doorType
                            });
                        }
                    }
                }
            }

            Debug.Log($"[CompleteMazeBuilder] 🚪 Spawned {doorsSpawned} doors (snapped to wall gaps)");
        }

        private string DetermineDoorType()
        {
            float roll = Random.value;
            
            if (roll < secretDoorChance)
                return "Secret";
            else if (roll < secretDoorChance + lockedDoorChance)
                return "Locked";
            else
                return "Normal";
        }

        private string GetDoorPrefabPath(string doorType)
        {
            switch (doorType)
            {
                case "Secret": return secretDoorPrefabPath;
                case "Locked": return lockedDoorPrefabPath;
                default: return doorPrefabPath;
            }
        }

        private void SpawnDoor(Vector3 position, Quaternion rotation, int x, int y, string direction, string doorType, string prefabPath)
        {
            // Try to load door prefab
            GameObject doorPrefab = LoadPrefab(prefabPath);
            
            GameObject door;
            if (doorPrefab != null)
            {
                door = Instantiate(doorPrefab, position, rotation);
            }
            else
            {
                // Fallback: create simple door cube
                door = GameObject.CreatePrimitive(PrimitiveType.Cube);
                door.transform.position = position;
                door.transform.rotation = rotation;
                door.transform.localScale = new Vector3(wallThickness, wallHeight, cellSize * 0.9f);
            }

            door.name = $"{doorType}Door_{x}_{y}_{direction}";

            // Apply door material
            ApplyMaterial(door, doorMaterialPath);

            // Add DoorsEngine component
            var doorsEngine = door.AddComponent<DoorsEngine>();

            Debug.Log($"[CompleteMazeBuilder] 🚪 {doorType} door at ({x}, {y}) {direction}");
        }

        private void SpawnRooms()
        {
            int numRooms = Random.Range(minRooms, maxRooms + 1);
            Debug.Log($"[CompleteMazeBuilder] 🏛️ Generating {numRooms} rooms (each with 1 entrance + 1 exit)...");

            var gridField = mazeGenerator.GetType().GetProperty("Grid");
            var grid = gridField?.GetValue(mazeGenerator) as MazeGenerator.Wall[,];

            if (grid == null) return;

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Spawn entrance room (start - top-left area) - PLAYER SPAWNS HERE
            Vector2Int entrancePos = FindValidRoomPosition(grid, 0, 0, width / 3, height / 2);
            if (entrancePos.x >= 0)
            {
                SpawnRoom(entrancePos, "Entrance", entranceRoomPrefabPath, hasEntrance: true, hasExit: true);
                // Set player spawn position (center of entrance room)
                entranceRoomPosition = new Vector3(
                    entrancePos.x * cellSize + cellSize * 1.5f,  // Center of 3x3 room
                    1f,
                    entrancePos.y * cellSize + cellSize * 1.5f
                );
            }

            // Spawn exit room (end - bottom-right area)
            Vector2Int exitPos = FindValidRoomPosition(grid, width * 2 / 3, height / 2, width, height);
            if (exitPos.x >= 0)
            {
                SpawnRoom(exitPos, "Exit", exitRoomPrefabPath, hasEntrance: true, hasExit: true);
            }

            // Spawn normal rooms (each with 1 entrance + 1 exit)
            int roomsSpawned = 2; // entrance + exit
            while (roomsSpawned < numRooms)
            {
                Vector2Int roomPos = FindValidRoomPosition(grid, 0, 0, width, height);
                if (roomPos.x >= 0)
                {
                    SpawnRoom(roomPos, "Normal", normalRoomPrefabPath, hasEntrance: true, hasExit: true);
                    roomsSpawned++;
                }
                else
                {
                    break; // No more valid positions
                }
            }

            Debug.Log($"[CompleteMazeBuilder] 🏛️ Spawned {roomsSpawned} rooms (each with 1 entrance + 1 exit)");
        }

        private Vector2Int FindValidRoomPosition(MazeGenerator.Wall[,] grid, int minX, int minY, int maxX, int maxY)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Try random positions
            for (int i = 0; i < 20; i++)
            {
                int x = Random.Range(minX, Mathf.Min(maxX, width - 3));
                int y = Random.Range(minY, Mathf.Min(maxY, height - 3));

                // Check if area is clear (no walls)
                bool clear = true;
                for (int dx = 0; dx < 3 && clear; dx++)
                {
                    for (int dy = 0; dy < 3 && clear; dy++)
                    {
                        if (grid[x + dx, y + dy] != MazeGenerator.Wall.None)
                        {
                            clear = false;
                        }
                    }
                }

                if (clear)
                {
                    return new Vector2Int(x, y);
                }
            }

            return new Vector2Int(-1, -1); // No valid position
        }

        private void SpawnRoom(Vector2Int position, string roomType, string prefabPath, bool hasEntrance, bool hasExit)
        {
            // Try to load room prefab
            GameObject roomPrefab = LoadPrefab(prefabPath);

            GameObject room;
            if (roomPrefab != null)
            {
                // Room position: center of 3x3 cell area
                Vector3 roomPos = new Vector3(
                    position.x * cellSize + cellSize * 1.5f,
                    0f,
                    position.y * cellSize + cellSize * 1.5f
                );
                room = Instantiate(roomPrefab, roomPos, Quaternion.identity);
            }
            else
            {
                // Fallback: create simple room with floor + ceiling + 4 walls (1 entrance + 1 exit)
                room = new GameObject($"{roomType}Room_{position.x}_{position.y}");
                // Room position: center of 3x3 cell area
                Vector3 roomPos = new Vector3(
                    position.x * cellSize + cellSize * 1.5f,
                    0f,
                    position.y * cellSize + cellSize * 1.5f
                );
                room.transform.position = roomPos;

                // Create floor (with texture)
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = "Floor";
                floor.transform.parent = room.transform;
                floor.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                floor.transform.localScale = new Vector3(cellSize * 3f, 0.1f, cellSize * 3f);
                ApplyMaterial(floor, floorMaterialPath);
                ApplyTexture(floor, groundTexturePath);

                // Create ceiling (with texture)
                GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ceiling.name = "Ceiling";
                ceiling.transform.parent = room.transform;
                ceiling.transform.localPosition = new Vector3(0f, ceilingHeight, 0f);
                ceiling.transform.localScale = new Vector3(cellSize * 3f, 0.1f, cellSize * 3f);
                ApplyMaterial(ceiling, floorMaterialPath);
                ApplyTexture(ceiling, ceilingTexturePath);

                // Create 4 walls with 1 entrance + 1 exit (opposite sides, with textures)
                CreateRoomWall(room.transform, new Vector3(0f, wallHeight / 2f, -cellSize * 1.5f), Quaternion.Euler(0f, 90f, 0f), "North");
                CreateRoomWall(room.transform, new Vector3(0f, wallHeight / 2f, cellSize * 1.5f), Quaternion.Euler(0f, 90f, 0f), "South");
                CreateRoomWallWithGap(room.transform, new Vector3(cellSize * 1.5f, wallHeight / 2f, 0f), Quaternion.Euler(0f, 0f, 0f), "East", hasEntrance);
                CreateRoomWallWithGap(room.transform, new Vector3(-cellSize * 1.5f, wallHeight / 2f, 0f), Quaternion.Euler(0f, 0f, 0f), "West", hasExit);
            }

            room.name = $"{roomType}Room_{position.x}_{position.y}";
            Debug.Log($"[CompleteMazeBuilder] 🏛️ {roomType} room at ({position.x}, {position.y}) - 1 entrance + 1 exit");
        }

        private void CreateRoomWall(Transform parent, Vector3 localPos, Quaternion localRot, string name)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"Wall_{name}";
            wall.transform.parent = parent;
            wall.transform.localPosition = localPos;
            wall.transform.localRotation = localRot;
            wall.transform.localScale = new Vector3(cellSize * 3f, wallHeight, wallThickness);
            ApplyMaterial(wall, wallMaterialPath);
            ApplyTexture(wall, wallTexturePath);
        }

        private void CreateRoomWallWithGap(Transform parent, Vector3 localPos, Quaternion localRot, string name, bool hasGap)
        {
            if (hasGap)
            {
                // Create two wall segments with gap in middle (entrance/exit)
                GameObject wall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall1.name = $"Wall_{name}_Left";
                wall1.transform.parent = parent;
                wall1.transform.localPosition = localPos + Vector3.right * (cellSize * 0.75f);
                wall1.transform.localRotation = localRot;
                wall1.transform.localScale = new Vector3(cellSize * 1.5f, wallHeight, wallThickness);
                ApplyMaterial(wall1, wallMaterialPath);
                ApplyTexture(wall1, wallTexturePath);

                GameObject wall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall2.name = $"Wall_{name}_Right";
                wall2.transform.parent = parent;
                wall2.transform.localPosition = localPos - Vector3.right * (cellSize * 0.75f);
                wall2.transform.localRotation = localRot;
                wall2.transform.localScale = new Vector3(cellSize * 1.5f, wallHeight, wallThickness);
                ApplyMaterial(wall2, wallMaterialPath);
                ApplyTexture(wall2, wallTexturePath);
            }
            else
            {
                CreateRoomWall(parent, localPos, localRot, name);
            }
        }

        private void PlacePlayerAtSpawn()
        {
            // Find player and teleport to entrance room
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null && entranceRoomPosition != Vector3.zero)
            {
                Vector3 spawnPos = entranceRoomPosition + spawnOffset;
                player.transform.position = spawnPos;
                Debug.Log($"[CompleteMazeBuilder] 👤 Player spawned at entrance: {spawnPos}");
            }
            else if (entranceRoomPosition == Vector3.zero)
            {
                Debug.LogWarning("[CompleteMazeBuilder] ⚠️ Entrance room not found, using default spawn");
            }
            else
            {
                Debug.LogWarning("[CompleteMazeBuilder] ⚠️ PlayerController not found");
            }
        }

        private void PlaceObjects()
        {
            // Step 8a: Place torches using LightPlacementEngine
            PlaceTorches();

            // Step 8b: Place other objects (chests, enemies, items) using SpatialPlacer
            spatialPlacer = GetComponent<SpatialPlacer>();
            if (spatialPlacer != null)
            {
                // Wire up TorchPool reference (required for torch placement)
                if (torchPool == null)
                {
                    torchPool = FindFirstObjectByType<TorchPool>();
                    if (torchPool == null)
                    {
                        var torchGO = new GameObject("TorchPool");
                        torchPool = torchGO.AddComponent<TorchPool>();
                    }
                }
                
                // Assign TorchPool to SpatialPlacer via reflection
                var torchPoolField = typeof(SpatialPlacer).GetField("torchPool", BindingFlags.NonPublic | BindingFlags.Instance);
                if (torchPoolField != null)
                {
                    torchPoolField.SetValue(spatialPlacer, torchPool);
                }
                
                // Assign LightPlacementEngine to SpatialPlacer via reflection
                var lightPlacementEngineField = typeof(SpatialPlacer).GetField("lightPlacementEngine", BindingFlags.NonPublic | BindingFlags.Instance);
                if (lightPlacementEngineField != null && lightPlacementEngine != null)
                {
                    lightPlacementEngineField.SetValue(spatialPlacer, lightPlacementEngine);
                }

                // Enable torch placement in SpatialPlacer (dual placement for redundancy)
                spatialPlacer.PlaceTorchesEnabled = true;
                spatialPlacer.PlaceAll();
                Debug.Log("[CompleteMazeBuilder] ✅ Objects placed (torches, chests, enemies, items)");
            }
            else
            {
                Debug.LogWarning("[CompleteMazeBuilder] ⚠️ SpatialPlacer not found");
            }
        }

        private void PlaceTorches()
        {
            // Get LightPlacementEngine
            lightPlacementEngine = GetComponent<LightPlacementEngine>();
            if (lightPlacementEngine == null)
            {
                Debug.LogWarning("[CompleteMazeBuilder] ⚠️ LightPlacementEngine not found, skipping torch placement");
                return;
            }

            // Get TorchPool
            torchPool = FindFirstObjectByType<TorchPool>();
            if (torchPool == null)
            {
                var torchGO = new GameObject("TorchPool");
                torchPool = torchGO.AddComponent<TorchPool>();
            }

            // Get maze ID for binary storage
            string mazeId = $"Maze_{mazeWidth}x{mazeHeight}_{currentSeed}";

            // Load and instantiate torches from binary storage
            bool success = lightPlacementEngine.LoadAndInstantiateTorches(mazeId, (int)currentSeed);
            
            if (success)
            {
                Debug.Log($"[CompleteMazeBuilder] 🎆 Torches placed from binary storage (maze: {mazeId})");
            }
            else
            {
                Debug.LogWarning($"[CompleteMazeBuilder] ⚠️ Torch placement failed, will use fallback");
            }
        }

        #endregion

        #region Helper Methods

        private T GetOrAddComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        private GameObject LoadPrefab(string relativePath)
        {
            string fullPath = "Assets/" + relativePath;
            
            // Try Resources folder
            string resourcesPath = relativePath.Replace("Resources/", "");
            if (relativePath.StartsWith("Resources/"))
            {
                GameObject prefab = Resources.Load<GameObject>(resourcesPath.Replace(".prefab", ""));
                if (prefab != null) return prefab;
            }

            // Load via AssetDatabase (editor only)
            #if UNITY_EDITOR
            UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fullPath);
            if (obj is GameObject)
            {
                return obj as GameObject;
            }
            #endif

            Debug.LogWarning($"[CompleteMazeBuilder] ⚠️ Prefab not found: {fullPath}");
            return null;
        }

        private void ApplyMaterial(GameObject obj, string materialPath)
        {
            string fullPath = "Assets/" + materialPath;
            Material mat = null;

            #if UNITY_EDITOR
            mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            #endif

            if (mat != null)
            {
                var renderer = obj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = mat;
                }
            }
        }

        private void ApplyTexture(GameObject obj, string texturePath)
        {
            string fullPath = "Assets/" + texturePath;
            Texture2D tex = null;

            #if UNITY_EDITOR
            tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            #endif

            if (tex != null)
            {
                var renderer = obj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material.mainTexture = tex;
                }
            }
        }

        private bool ValidatePath(string fullPath, string name)
        {
            if (System.IO.File.Exists(fullPath))
            {
                Debug.Log($"[CompleteMazeBuilder] ✅ {name}: {fullPath}");
                return true;
            }
            else
            {
                Debug.LogError($"[CompleteMazeBuilder] ❌ {name} NOT FOUND: {fullPath}");
                return false;
            }
        }

        private uint ComputeSeed(string seedString)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(seedString);
            uint hash = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                hash = hash * 31 + bytes[i];
            }
            return hash;
        }

        #endregion
    }
}
