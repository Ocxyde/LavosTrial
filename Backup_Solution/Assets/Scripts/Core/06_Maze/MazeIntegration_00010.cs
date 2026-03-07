// Copyright (C) 2026 Ocxyde
//
// This file is part of PeuImporte.
//
// PeuImporte is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeuImporte is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeuImporte.  If not, see <https://www.gnu.org/licenses/>.
// MazeIntegration.cs
// ️ DEPRECATED - LEGACY MAZE SYSTEM ️
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
//  DO NOT USE FOR NEW DEVELOPMENT 
// This class is part of the LEGACY maze generation system.
//
// USE CompleteMazeBuilder.cs FOR ALL NEW MAZE GENERATION:
// - CompleteMazeBuilder is the authoritative maze generator
// - CompleteMazeBuilder is plug-in-out compliant
// - CompleteMazeBuilder loads from JSON config
//
// This file is kept for:
// - Backward compatibility with existing tests
// - Legacy scene support
//
// Migration: Replace MazeIntegration with CompleteMazeBuilder in all scenes.

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// ️ DEPRECATED - Use CompleteMazeBuilder instead.
    /// 
    /// Legacy maze orchestrator. Coordinates deprecated components:
    /// - MazeGenerator (legacy DFS algorithm)
    /// - RoomGenerator (legacy room placement)
    /// - DoorHolePlacer (legacy door holes)
    /// - RoomDoorPlacer (legacy door placement)
    /// - MazeRenderer (deprecated geometry builder)
    /// </summary>
    [System.Obsolete("Use CompleteMazeBuilder instead. This class is deprecated.")]
    public class MazeIntegration : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private bool autoGenerateOnStart = true;
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private string manualSeed = "MyCustomSeed123";

        [Header("Maze Dimensions")]
        [SerializeField] private int mazeWidth = 21;  //  Reduced from 31 for better performance
        [SerializeField] private int mazeHeight = 21;  //  Reduced from 31 for better performance

        [Header("Room Settings")]
        [SerializeField] private bool generateRooms = true;
        [SerializeField] private int minRooms = 3;
        [SerializeField] private int maxRooms = 8;

        [Header("Door Settings")]
        [SerializeField] private bool generateDoors = true;
        [SerializeField] private float doorChance = 0.6f;

        [Header("Torch Settings")]
        [SerializeField] private bool placeTorches = true;  //  Torches ARE placed by default
        
        [Header("Level Settings")]
        [Tooltip("Current maze level (affects maze size)")]
        [SerializeField] private int currentLevel = 1;
        
        [Tooltip("Enable level progression (maze grows with level)")]
        [SerializeField] private bool useLevelProgression = true;

        // Public accessors for editor script
        public bool AutoGenerateOnStart { get => autoGenerateOnStart; set => autoGenerateOnStart = value; }
        public bool UseRandomSeed { get => useRandomSeed; set => useRandomSeed = value; }
        public string ManualSeed { get => manualSeed; set => manualSeed = value; }
        public int MazeWidth { get => mazeWidth; set => mazeWidth = value; }
        public int MazeHeight { get => mazeHeight; set => mazeHeight = value; }
        public int CurrentLevel { get => currentLevel; set => currentLevel = value; }
        public bool GenerateRooms { get => generateRooms; set => generateRooms = value; }
        public bool GenerateDoors { get => generateDoors; set => generateDoors = value; }
        public float DoorChance { get => doorChance; set => doorChance = value; }
        public bool PlaceTorches { get => placeTorches; set => placeTorches = value; }

        [Header("Components")]
        [SerializeField] private MazeGenerator mazeGenerator;
        [SerializeField] private RoomGenerator roomGenerator;
        [SerializeField] private DoorHolePlacer doorHolePlacer;
        [SerializeField] private RoomDoorPlacer roomDoorPlacer;
        [SerializeField] private IMazeRenderer mazeRenderer;
        [SerializeField] private SeedManager seedManager;
        [SerializeField] private SpatialPlacer spatialPlacer;  // Universal placement system (includes torches)

        // State
        private bool _isGenerated = false;

        public bool IsGenerated => _isGenerated;
        public uint CurrentSeed { get; private set; }

        private void Awake()
        {
            // Auto-find components if not assigned
            FindComponents();
        }

        private void Start()
        {
            if (autoGenerateOnStart && !_isGenerated)
            {
                GenerateMaze();
            }
        }

        /// <summary>
        /// Find all required components.
        /// </summary>
        private void FindComponents()
        {
            if (mazeGenerator == null)
                mazeGenerator = GetComponent<MazeGenerator>();

            if (roomGenerator == null)
                roomGenerator = GetComponent<RoomGenerator>();

            if (doorHolePlacer == null)
                doorHolePlacer = GetComponent<DoorHolePlacer>();

            if (roomDoorPlacer == null)
                roomDoorPlacer = GetComponent<RoomDoorPlacer>();

            if (mazeRenderer == null)
                mazeRenderer = GetComponent<IMazeRenderer>();

            if (seedManager == null)
                seedManager = FindFirstObjectByType<SeedManager>();

            if (spatialPlacer == null)
                spatialPlacer = GetComponent<SpatialPlacer>();  // Auto-find if not assigned
        }

        /// <summary>
        /// Generate complete maze with rooms and doors.
        /// </summary>
        [ContextMenu("Generate Maze")]
        public void GenerateMaze()
        {
            Debug.Log("=== Starting Maze Generation ===");

            // Set seed
            if (useRandomSeed)
            {
                manualSeed = System.Guid.NewGuid().ToString();
            }

            // Set seed via SeedManager if available
            if (seedManager != null)
            {
                // SeedManager uses CurrentSeed property - access it to trigger generation
                string seed = seedManager.CurrentSeed;
                Debug.Log($"[MazeIntegration] Using SeedManager seed: {seed}");
            }

            CurrentSeed = ComputeSeed(manualSeed);

            // Set maze level for dynamic sizing (plug-in-and-out: each component listens to level events)
            if (mazeGenerator != null && useLevelProgression)
            {
                mazeGenerator.SetMazeLevel(currentLevel);
            }
            else if (mazeGenerator != null)
            {
                Debug.Log($"[MazeIntegration]  Level {currentLevel}: {mazeWidth}x{mazeHeight} (fixed size)");
            }
            
            // PLUG-IN-AND-OUT: Invoke level changed event via EventHandler
            // Other components (SpatialPlacer, Lighting, etc.) listen to this event
            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.InvokeMazeLevelChanged(currentLevel);
                Debug.Log($"[MazeIntegration]  Invoked MazeLevelChanged event (Level {currentLevel})");
            }

            // Configure maze dimensions
            if (mazeGenerator != null)
            {
                mazeGenerator.width = mazeWidth;
                mazeGenerator.height = mazeHeight;
            }

            // Configure rooms
            if (roomGenerator != null)
            {
                // Set room settings via public fields if available
                Debug.Log($"[MazeIntegration] Room count: {minRooms}-{maxRooms}");
            }

            // Configure doors
            if (doorHolePlacer != null)
            {
                // Door chance is set in inspector
            }

            // Generate in order:
            // 1. Maze (creates grid)
            // 2. Rooms (carves rooms into grid)
            // 3. Door Holes (reserves wall space)
            // 4. Doors (places doors in holes)
            // 5. Render (builds geometry)

            Debug.Log("[MazeIntegration] Step 1: Generating maze...");
            if (mazeGenerator != null)
            {
                mazeGenerator.Generate();
            }

            Debug.Log("[MazeIntegration] Step 2: Generating rooms...");
            if (roomGenerator != null && generateRooms)
            {
                roomGenerator.GenerateRooms();
            }

            Debug.Log("[MazeIntegration] Step 3: Placing door holes...");
            if (doorHolePlacer != null && generateDoors)
            {
                doorHolePlacer.PlaceAllHoles();
            }

            Debug.Log("[MazeIntegration] Step 4: Placing doors...");
            if (roomDoorPlacer != null && generateDoors)
            {
                roomDoorPlacer.PlaceAllDoors();
            }

            Debug.Log("[MazeIntegration] Step 5: Building geometry...");
            if (mazeRenderer != null)
            {
                mazeRenderer.BuildMaze();
            }

            // Step 6: Place ALL objects (torches, chests, enemies, items)
            // SpatialPlacer handles all object placement in one call
            if (spatialPlacer != null)
            {
                Debug.Log("[MazeIntegration] Step 6: Placing ALL objects via SpatialPlacer...");
                spatialPlacer.PlaceAllObjects();  //  Places torches, chests, enemies, items
            }
            else
            {
                Debug.LogWarning("[MazeIntegration] SpatialPlacer component not found!");
            }

            _isGenerated = true;

            Debug.Log("=== Maze Generation Complete ===");
            Debug.Log($"Seed: {manualSeed}");
            Debug.Log($"Maze Size: {mazeWidth}x{mazeHeight}");

            if (roomGenerator != null)
                Debug.Log($"Rooms Generated: {roomGenerator.RoomCount}");

            if (doorHolePlacer != null)
                Debug.Log($"Door Holes: {doorHolePlacer.HoleCount}");

            if (roomDoorPlacer != null)
                Debug.Log($"Doors Placed: {roomDoorPlacer.DoorCount}");

            if (spatialPlacer != null && placeTorches)
                Debug.Log($"Torches Placed: Check SpatialPlacer stats");
        }

        /// <summary>
        /// Regenerate maze with same seed.
        /// </summary>
        [ContextMenu("Regenerate (Same Seed)")]
        public void Regenerate()
        {
            _isGenerated = false;
            GenerateMaze();
        }

        /// <summary>
        /// Regenerate maze with new random seed.
        /// </summary>
        [ContextMenu("Regenerate (New Seed)")]
        public void RegenerateWithNewSeed()
        {
            useRandomSeed = true;
            _isGenerated = false;
            GenerateMaze();
        }

        /// <summary>
        /// Compute numeric seed from string.
        /// </summary>
        private uint ComputeSeed(string seedString)
        {
            if (string.IsNullOrEmpty(seedString))
                return 12345;

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(seedString);
            uint hash = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                hash = hash * 31 + bytes[i];
            }

            return hash;
        }

        #region Debug

        // Debug GUI disabled for production
        // Uncomment to enable debug UI in editor
        /*
        private void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginVertical("box");

            GUILayout.Label($"Seed: {manualSeed}");
            GUILayout.Label($"Maze: {mazeWidth}x{mazeHeight}");
            GUILayout.Label($"Generated: {_isGenerated}");

            if (roomGenerator != null)
                GUILayout.Label($"Rooms: {roomGenerator.RoomCount}");

            if (doorHolePlacer != null)
                GUILayout.Label($"Holes: {doorHolePlacer.HoleCount}");

            if (roomDoorPlacer != null)
                GUILayout.Label($"Doors: {roomDoorPlacer.DoorCount}");

            GUILayout.Space(10);

            if (GUILayout.Button("Generate"))
            {
                GenerateMaze();
            }

            if (GUILayout.Button("Regenerate"))
            {
                Regenerate();
            }

            if (GUILayout.Button("New Seed"))
            {
                RegenerateWithNewSeed();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        [SerializeField] private bool showDebugUI = true;
        */

        #endregion
    }
}
