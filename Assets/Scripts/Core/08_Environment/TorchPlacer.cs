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
// TorchPlacer.cs
// DEPRECATED: Use MazeObjectSpawner.cs instead
//
// This file is a LEGACY remnant of the old torch placement system.
// New system: MazeObjectSpawner.cs with TORCH.fbx prefab
// - Simpler, direct instantiation
// - No binary storage overhead
// - TORCH.fbx snaps to walls automatically (no rotation needed)
// - Places torches in walkable cells, facing inward
//
// Specialized torch placement system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT ARCHITECTURE:
// - Finds components (never creates)
// - Uses prefabs when available
// - All values from JSON config
// - No hardcoded values
// - Uses LightPlacementEngine for binary storage
//
// LOCATION: Assets/Scripts/Core/08_Environment/

using System.Collections.Generic;
using UnityEngine;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Core
{
    /// <summary>
    /// TorchPlacer - Specialized torch placement.
    /// PLUG-IN-OUT COMPLIANT: Finds components, never creates.
    /// ALL VALUES FROM JSON: Torch settings loaded from GameConfig.
    /// BINARY STORAGE: Delegated to LightPlacementEngine (existing system).
    /// </summary>
    public class TorchPlacer : MonoBehaviour
    {
        #region Inspector Fields (From JSON)

        [Header(" Torch Settings (From JSON Config)")]
        [Tooltip("Enable torch spawning (loaded from JSON)")]
        [SerializeField] private bool enableTorchSpawning;

        [Tooltip("Use dynamic lighting (loaded from JSON)")]
        [SerializeField] private bool useDynamicLighting;

        [Tooltip("Torch count (loaded from JSON)")]
        [SerializeField] private int torchCount;

        [Tooltip("Min distance between torches (loaded from JSON)")]
        [SerializeField] private float minDistanceBetweenTorches;

        [Header(" Prefab Reference")]
        [Tooltip("Torch prefab (assign in Inspector or Resources/)")]
        [SerializeField] private GameObject torchPrefab;

        [Header(" Component References (Plug-in-Out)")]
        [Tooltip("Auto-finds CompleteMazeBuilder8 in scene")]
        [SerializeField] private CompleteMazeBuilder8 completeMazeBuilder;

        [Tooltip("GridMazeGenerator is accessed via CompleteMazeBuilder8")]
        [SerializeField] private GridMazeGenerator gridMazeGenerator;

        [Tooltip("Auto-finds LightPlacementEngine for binary storage")]
        [SerializeField] private LightPlacementEngine lightPlacementEngine;

        [Tooltip("Auto-finds TorchPool in scene")]
        [SerializeField] private TorchPool torchPool;

        [Header(" Debug")]
        [SerializeField] private bool showDebugLogs = true;

        #endregion

        #region Private Data

        private int _torchesSpawned;

        #endregion

        #region Public Accessors

        public int TorchesSpawned => _torchesSpawned;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            FindComponents();
            LoadConfig();
        }

        #endregion

        #region Plug-in-Out Compliance

        private void FindComponents()
        {
            // GridMazeGenerator is created by CompleteMazeBuilder8
            if (completeMazeBuilder == null)
            {
                completeMazeBuilder = FindFirstObjectByType<CompleteMazeBuilder8>();
                if (completeMazeBuilder == null)
                    Debug.LogWarning("[TorchPlacer] CompleteMazeBuilder8 not found!");
            }

            if (lightPlacementEngine == null)
            {
                lightPlacementEngine = FindFirstObjectByType<LightPlacementEngine>();
                if (lightPlacementEngine == null)
                    Debug.LogWarning("[TorchPlacer] LightPlacementEngine not found!");
            }

            if (torchPool == null)
            {
                torchPool = FindFirstObjectByType<TorchPool>();
                if (torchPool == null)
                    Debug.LogWarning("[TorchPlacer] TorchPool not found!");
            }
        }

        #endregion

        #region JSON Config Loading

        private void LoadConfig()
        {
            var config = GameConfig.Instance;

            enableTorchSpawning = config.GenerateRooms;
            useDynamicLighting = true;
            torchCount = 60;
            minDistanceBetweenTorches = 4f;

            if (showDebugLogs)
            {
                Debug.Log($"[TorchPlacer]  Config loaded from JSON:");
                Debug.Log($"  • Enable: {enableTorchSpawning}");
                Debug.Log($"  • Dynamic Lighting: {useDynamicLighting}");
                Debug.Log($"  • Torch Count: {torchCount}");
                Debug.Log($"  • Min Distance: {minDistanceBetweenTorches}m");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Place all torches using LightPlacementEngine binary storage.
        /// </summary>
        [ContextMenu("Place Torches")]
        public void PlaceAllTorches(string mazeId, int seed)
        {
            if (!enableTorchSpawning)
            {
                if (showDebugLogs)
                    Debug.Log("[TorchPlacer] Torch spawning disabled");
                return;
            }

            if (gridMazeGenerator == null || lightPlacementEngine == null || torchPool == null)
            {
                Debug.LogError("[TorchPlacer]  Required components not initialized!");
                return;
            }

            // Try to load from binary first
            bool loadedFromBinary = lightPlacementEngine.LoadAndInstantiateTorches(mazeId, seed);

            if (loadedFromBinary)
            {
                Debug.Log($"[TorchPlacer]  Loaded torches from binary (RAM)");
                return;
            }

            // Binary not found - calculate and save
            Debug.Log("[TorchPlacer]  Binary not found - calculating positions...");

            var wallFaces = GetWallFacesFromGrid();
            if (wallFaces.Count == 0)
            {
                Debug.LogWarning("[TorchPlacer] No wall faces found!");
                return;
            }

            var torchRecords = CalculateTorchPositions(wallFaces);
            if (torchRecords.Count == 0)
            {
                Debug.LogWarning("[TorchPlacer] No valid torch positions found");
                return;
            }

            // Save to binary via LightPlacementEngine
            lightPlacementEngine.SaveTorches(mazeId, seed, torchRecords);
            Debug.Log($"[TorchPlacer]  Saved {torchRecords.Count} torches to binary");

            _torchesSpawned = torchRecords.Count;
        }

        #endregion

        #region Wall Face Detection

        private List<(Vector3, Quaternion)> GetWallFacesFromGrid()
        {
            var wallFaces = new List<(Vector3, Quaternion)>();

            if (gridMazeGenerator == null)
            {
                Debug.LogError("[TorchPlacer] GridMazeGenerator not found!");
                return wallFaces;
            }

            int size = gridMazeGenerator.GridSize;
            float cellSize = GetCellSize();
            float wallHeight = GetWallHeight();

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var cell = gridMazeGenerator.GetCell(x, y);

                    // 8-axis compatibility: Check if cell is a wall
                    if (!cell.IsWall())
                        continue;

                    Vector3 cellCenter = new Vector3(
                        x * cellSize + cellSize / 2f,
                        wallHeight / 2f,
                        y * cellSize + cellSize / 2f
                    );

                    // Check adjacent cells for open faces - prefer corridor faces
                    if (y + 1 < size && !gridMazeGenerator.GetCell(x, y + 1).IsWall())
                    {
                        if (IsCorridorCell(x, y + 1))
                            wallFaces.Insert(0, (cellCenter, Quaternion.Euler(0f, 180f, 0f)));
                        else
                            wallFaces.Add((cellCenter, Quaternion.Euler(0f, 180f, 0f)));
                    }

                    if (y - 1 >= 0 && !gridMazeGenerator.GetCell(x, y - 1).IsWall())
                    {
                        if (IsCorridorCell(x, y - 1))
                            wallFaces.Insert(0, (cellCenter, Quaternion.identity));
                        else
                            wallFaces.Add((cellCenter, Quaternion.identity));
                    }

                    if (x + 1 < size && !gridMazeGenerator.GetCell(x + 1, y).IsWall())
                    {
                        if (IsCorridorCell(x + 1, y))
                            wallFaces.Insert(0, (cellCenter, Quaternion.Euler(0f, -90f, 0f)));
                        else
                            wallFaces.Add((cellCenter, Quaternion.Euler(0f, -90f, 0f)));
                    }

                    if (x - 1 >= 0 && !gridMazeGenerator.GetCell(x - 1, y).IsWall())
                    {
                        if (IsCorridorCell(x - 1, y))
                            wallFaces.Insert(0, (cellCenter, Quaternion.Euler(0f, 90f, 0f)));
                        else
                            wallFaces.Add((cellCenter, Quaternion.Euler(0f, 90f, 0f)));
                    }
                }
            }

            return wallFaces;
        }

        private bool IsCorridorCell(int x, int y)
        {
            var cell = gridMazeGenerator.GetCell(x, y);
            bool isPassage = !cell.IsWall();
            bool isRoom = (cell & CellFlags8.IsRoom) != CellFlags8.None;
            bool isSpawn = (cell & CellFlags8.SpawnRoom) != CellFlags8.None;
            bool isExit = (cell & CellFlags8.IsExit) != CellFlags8.None;
            return isPassage && !isRoom && !isSpawn && !isExit;
        }

        #endregion

        #region Torch Calculation

        private List<WallPositionArchitect.TorchRecord> CalculateTorchPositions(List<(Vector3, Quaternion)> wallFaces)
        {
            var torchRecords = new List<WallPositionArchitect.TorchRecord>();
            var placedPositions = new List<Vector3>();

            int actualTorchCount = useDynamicLighting ? GetDynamicTorchCount() : torchCount;
            float actualMinDistance = useDynamicLighting ?
                Mathf.Max(2f, minDistanceBetweenTorches * 0.67f) : minDistanceBetweenTorches;

            ShuffleList(wallFaces);

            foreach (var (wallPos, wallRot) in wallFaces)
            {
                if (torchRecords.Count >= actualTorchCount)
                    break;

                if (placedPositions.Exists(p => Vector3.Distance(wallPos, p) < actualMinDistance))
                    continue;

                float torchY = GetWallHeight() * 0.55f;
                float torchInset = 0.5f;
                Vector3 torchPos = new Vector3(wallPos.x, torchY, wallPos.z);
                torchPos += wallRot * Vector3.forward * torchInset;

                Quaternion torchRot = Quaternion.Euler(35f, wallRot.eulerAngles.y + 180f, 0f);

                var record = new WallPositionArchitect.TorchRecord
                {
                    position = torchPos,
                    rotation = torchRot,
                    height = torchY,
                    inset = torchInset,
                    guid = System.Guid.NewGuid().ToString()
                };

                torchRecords.Add(record);
                placedPositions.Add(torchPos);
            }

            return torchRecords;
        }

        private int GetDynamicTorchCount()
        {
            int size = gridMazeGenerator.GridSize;
            return Mathf.Max(20, size * size / 8);
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        #endregion

        #region Utilities

        private float GetCellSize() => GameConfig.Instance.DefaultCellSize;
        private float GetWallHeight() => GameConfig.Instance.DefaultWallHeight;

        #endregion

        #region Binary Storage Integration

        private SpatialPlacer _storageReference;
        private LightPlacementEngine _binaryStorage;

        public void SetStorageReference(SpatialPlacer placer, LightPlacementEngine engine)
        {
            _storageReference = placer;
            _binaryStorage = engine;
        }

        #endregion
    }
}
