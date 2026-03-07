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
// ComputeGridEngine.cs
// COMPUTE GRID ENGINE - Byte-to-byte wall storage in RAM/Storage
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT ARCHITECTURE:
// - Independent process for compute grid management
// - Stores wall data byte-to-byte in RAM
// - Binary storage for persistence
// - Direct memory mapping for performance
// - Event-driven via EventHandler
//
// USAGE:
//   ComputeGridEngine.Instance.SetWall(x, z, cellType)
//   ComputeGridEngine.Instance.GetWall(x, z)
//   ComputeGridEngine.Instance.SaveToBinary()
//   ComputeGridEngine.Instance.LoadFromBinary()
//
// Location: Assets/Scripts/Core/12_Compute/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// ComputeGridEngine - Byte-to-byte wall storage in RAM/Storage.
    /// Independent process for compute grid management.
    /// 
    /// MEMORY LAYOUT:
    /// - Grid stored as byte array (1 byte per cell)
    /// - Direct memory mapping for instant access
    /// - Binary storage for persistence across sessions
    /// 
    /// PLUG-IN-OUT: Finds components, never creates.
    /// Must be added to scene manually.
    /// </summary>
    public class ComputeGridEngine : MonoBehaviour
    {
        #region Singleton

        private static ComputeGridEngine _instance;
        public static ComputeGridEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ComputeGridEngine>();
                    if (_instance == null)
                    {
                        Debug.LogWarning("[ComputeGridEngine] Not found in scene - auto-creating (add manually!)");
                        var go = new GameObject("ComputeGridEngine");
                        _instance = go.AddComponent<ComputeGridEngine>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Grid Cell Types (Byte Values)

        /// <summary>
        /// Grid cell types - matches GridMazeCell for compatibility.
        /// Each cell stored as 1 byte in RAM.
        /// </summary>
        [Serializable]
        public enum GridCell : byte
        {
            Floor = 0,      // Empty floor
            Room = 1,       // Room cell
            Corridor = 2,   // Corridor cell
            Wall = 3,       // Wall cell
            SpawnPoint = 4, // Player spawn
            Door = 5,       // Door cell
            Torch = 6,      // Torch mount point
            Chest = 7,      // Chest position
            Enemy = 8,      // Enemy spawn
            Item = 9        // Item drop
        }

        #endregion

        #region Inspector Settings

        [Header("Grid Configuration")]
        [Tooltip("Grid size (e.g., 21 for 21x21 maze)")]
        [SerializeField] private int gridSize = 21;

        [Tooltip("Enable byte-to-byte RAM storage")]
        [SerializeField] private bool useRamStorage = true;

        [Tooltip("Enable binary file persistence")]
        [SerializeField] private bool useBinaryPersistence = true;

        [Header("Maze Identification")]
        [Tooltip("Unique maze identifier")]
        [SerializeField] private string mazeId = "Maze_001";

        [Tooltip("Seed for procedural generation")]
        [SerializeField] private int mazeSeed = 12345;

        [Header("Debug")]
        [Tooltip("Show debug information")]
        [SerializeField] private bool showDebugInfo = true;

        #endregion

        #region RAM Storage - Byte Array

        /// <summary>
        /// Primary RAM storage - byte array (1 byte per cell).
        /// Direct memory mapping for instant access.
        /// </summary>
        private byte[,] _gridRam;

        /// <summary>
        /// Wall metadata stored in parallel RAM structure.
        /// Contains orientation, material, state info.
        /// </summary>
        private WallMetadata[,] _wallMetadata;

        /// <summary>
        /// Wall metadata structure (8 bytes per wall).
        /// </summary>
        [Serializable]
        public struct WallMetadata
        {
            public byte orientation;    // 0=North, 1=South, 2=East, 3=West
            public byte material;       // Material type index
            public byte state;          // 0=Intact, 1=Damaged, 2=Destroyed
            public byte flags;          // Bit flags for special properties
            public uint reserved1;      // Reserved for future use
        }

        #endregion

        #region Private Fields

        private bool _isInitialized = false;
        private string _binaryFilePath;
        private Dictionary<string, Action<int, int, GridCell>> _onCellChangedCallbacks;

        #endregion

        #region Public Properties

        public int GridSize => gridSize;
        public bool IsInitialized => _isInitialized;
        public string MazeId => mazeId;
        public int MazeSeed => mazeSeed;

        /// <summary>
        /// Get raw byte array for direct memory access.
        /// </summary>
        public byte[,] GridRam => _gridRam;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize compute grid engine.
        /// Creates byte array in RAM for direct memory mapping.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.Log("[ComputeGridEngine] Already initialized");
                return;
            }

            Debug.Log($"[ComputeGridEngine] Initializing {gridSize}x{gridSize} compute grid...");

            // Allocate RAM - byte array (1 byte per cell)
            _gridRam = new byte[gridSize, gridSize];
            _wallMetadata = new WallMetadata[gridSize, gridSize];

            // Initialize callbacks
            _onCellChangedCallbacks = new Dictionary<string, Action<int, int, GridCell>>();

            // Clear memory
            ClearGrid();

            _isInitialized = true;

            Debug.Log($"[ComputeGridEngine] Initialized - RAM: {GetMemoryUsageKB()}KB");
        }

        /// <summary>
        /// Set grid size and reinitialize.
        /// </summary>
        public void SetGridSize(int size)
        {
            gridSize = Mathf.Clamp(size, 5, 101);
            _isInitialized = false;
            Initialize();
            Debug.Log($"[ComputeGridEngine] Grid size set to {gridSize}x{gridSize}");
        }

        /// <summary>
        /// Set maze identification.
        /// </summary>
        public void SetMazeId(string id)
        {
            mazeId = id;
        }

        /// <summary>
        /// Set maze seed.
        /// </summary>
        public void SetMazeSeed(int seed)
        {
            mazeSeed = seed;
        }

        #endregion

        #region Byte-to-Byte RAM Operations

        /// <summary>
        /// Set cell value at position (byte-to-byte write).
        /// </summary>
        public void SetCell(int x, int z, GridCell cellType)
        {
            if (!IsWithinBounds(x, z))
            {
                Debug.LogWarning($"[ComputeGridEngine] Position ({x}, {z}) out of bounds");
                return;
            }

            // Direct byte write to RAM
            _gridRam[x, z] = (byte)cellType;

            // Notify callbacks
            NotifyCellChanged(x, z, cellType);
        }

        /// <summary>
        /// Get cell value at position (byte-to-byte read).
        /// </summary>
        public GridCell GetCell(int x, int z)
        {
            if (!IsWithinBounds(x, z))
            {
                return GridCell.Floor;
            }

            // Direct byte read from RAM
            return (GridCell)_gridRam[x, z];
        }

        /// <summary>
        /// Set wall metadata at position.
        /// </summary>
        public void SetWallMetadata(int x, int z, WallMetadata metadata)
        {
            if (!IsWithinBounds(x, z)) return;

            _wallMetadata[x, z] = metadata;
        }

        /// <summary>
        /// Get wall metadata at position.
        /// </summary>
        public WallMetadata GetWallMetadata(int x, int z)
        {
            if (!IsWithinBounds(x, z))
            {
                return new WallMetadata();
            }

            return _wallMetadata[x, z];
        }

        /// <summary>
        /// Check if cell is a wall.
        /// </summary>
        public bool IsWall(int x, int z)
        {
            return GetCell(x, z) == GridCell.Wall;
        }

        /// <summary>
        /// Check if cell is walkable.
        /// </summary>
        public bool IsWalkable(int x, int z)
        {
            var cell = GetCell(x, z);
            return cell == GridCell.Floor || 
                   cell == GridCell.Room || 
                   cell == GridCell.Corridor ||
                   cell == GridCell.Door;
        }

        /// <summary>
        /// Clear entire grid (set all to Floor).
        /// </summary>
        public void ClearGrid()
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    _gridRam[x, z] = (byte)GridCell.Floor;
                    _wallMetadata[x, z] = new WallMetadata();
                }
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Set entire row of cells (bulk operation).
        /// </summary>
        public void SetRow(int z, GridCell[] cells)
        {
            int width = Mathf.Min(cells.Length, gridSize);
            for (int x = 0; x < width; x++)
            {
                _gridRam[x, z] = (byte)cells[x];
            }
        }

        /// <summary>
        /// Set entire column of cells (bulk operation).
        /// </summary>
        public void SetColumn(int x, GridCell[] cells)
        {
            int height = Mathf.Min(cells.Length, gridSize);
            for (int z = 0; z < height; z++)
            {
                _gridRam[x, z] = (byte)cells[z];
            }
        }

        /// <summary>
        /// Get entire grid as byte array (for serialization).
        /// </summary>
        public byte[] GetGridBytes()
        {
            byte[] data = new byte[gridSize * gridSize];
            int index = 0;
            for (int z = 0; z < gridSize; z++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    data[index++] = _gridRam[x, z];
                }
            }
            return data;
        }

        /// <summary>
        /// Set entire grid from byte array (for deserialization).
        /// </summary>
        public void SetGridBytes(byte[] data)
        {
            if (data.Length != gridSize * gridSize)
            {
                Debug.LogError($"[ComputeGridEngine] Data size mismatch: expected {gridSize * gridSize}, got {data.Length}");
                return;
            }

            int index = 0;
            for (int z = 0; z < gridSize; z++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    _gridRam[x, z] = data[index++];
                }
            }

            Debug.Log($"[ComputeGridEngine] Grid loaded from {data.Length} bytes");
        }

        #endregion

        #region Binary Persistence

        /// <summary>
        /// Save compute grid to binary file.
        /// </summary>
        public bool SaveToBinary()
        {
            if (!useBinaryPersistence)
            {
                Debug.LogWarning("[ComputeGridEngine] Binary persistence disabled");
                return false;
            }

            if (!_isInitialized)
            {
                Debug.LogError("[ComputeGridEngine] Not initialized");
                return false;
            }

            Debug.Log("[ComputeGridEngine] Saving to binary...");

            // Use ComputeGridData for binary I/O
            bool saved = ComputeGridData.SaveGrid(mazeId, GetGridBytes(), mazeSeed);

            if (saved)
            {
                Debug.Log($"[ComputeGridEngine] Saved to binary: {mazeId} ({GetMemoryUsageKB()}KB)");
            }

            return saved;
        }

        /// <summary>
        /// Load compute grid from binary file.
        /// </summary>
        public bool LoadFromBinary()
        {
            if (!useBinaryPersistence)
            {
                Debug.LogWarning("[ComputeGridEngine] Binary persistence disabled");
                return false;
            }

            if (!_isInitialized)
            {
                Debug.LogError("[ComputeGridEngine] Not initialized");
                return false;
            }

            Debug.Log($"[ComputeGridEngine] Loading from binary: {mazeId}...");

            // Use ComputeGridData for binary I/O
            byte[] data = ComputeGridData.LoadGrid(mazeId, mazeSeed);

            if (data != null)
            {
                SetGridBytes(data);
                Debug.Log($"[ComputeGridEngine] Loaded from binary: {mazeId} ({data.Length} bytes)");
                return true;
            }

            Debug.LogWarning($"[ComputeGridEngine] No binary data found for {mazeId}");
            return false;
        }

        /// <summary>
        /// Clear binary storage.
        /// </summary>
        public void ClearBinary()
        {
            ComputeGridData.DeleteGrid(mazeId);
            Debug.Log($"[ComputeGridEngine] Binary cleared for {mazeId}");
        }

        #endregion

        #region Event System (Plug-in-Out)

        /// <summary>
        /// Register callback for cell changes.
        /// </summary>
        public void RegisterCellChangedCallback(string key, Action<int, int, GridCell> callback)
        {
            if (!_onCellChangedCallbacks.ContainsKey(key))
            {
                _onCellChangedCallbacks[key] = callback;
                Debug.Log($"[ComputeGridEngine] Callback registered: {key}");
            }
        }

        /// <summary>
        /// Unregister callback for cell changes.
        /// </summary>
        public void UnregisterCellChangedCallback(string key)
        {
            if (_onCellChangedCallbacks.ContainsKey(key))
            {
                _onCellChangedCallbacks.Remove(key);
                Debug.Log($"[ComputeGridEngine] Callback unregistered: {key}");
            }
        }

        /// <summary>
        /// Notify all callbacks of cell change.
        /// </summary>
        private void NotifyCellChanged(int x, int z, GridCell cellType)
        {
            foreach (var kvp in _onCellChangedCallbacks)
            {
                try
                {
                    kvp.Value?.Invoke(x, z, cellType);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ComputeGridEngine] Callback error ({kvp.Key}): {e.Message}");
                }
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Check if position is within grid bounds.
        /// </summary>
        private bool IsWithinBounds(int x, int z)
        {
            return x >= 0 && x < gridSize && z >= 0 && z < gridSize;
        }

        /// <summary>
        /// Get memory usage estimate.
        /// </summary>
        public int GetMemoryUsageKB()
        {
            // Grid RAM: gridSize * gridSize bytes
            // Metadata: gridSize * gridSize * 12 bytes (WallMetadata struct)
            int gridBytes = gridSize * gridSize;
            int metadataBytes = gridSize * gridSize * 12;
            return (gridBytes + metadataBytes) / 1024;
        }

        /// <summary>
        /// Print grid statistics.
        /// </summary>
        public void PrintStatistics()
        {
            int[] cellCounts = new int[Enum.GetNames(typeof(GridCell)).Length];

            for (int z = 0; z < gridSize; z++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    cellCounts[(int)GetCell(x, z)]++;
                }
            }

            Debug.Log("═══════════════════════════════════════════════");
            Debug.Log("  COMPUTE GRID STATISTICS");
            Debug.Log("═══════════════════════════════════════════════");
            Debug.Log($"Grid Size: {gridSize}x{gridSize}");
            Debug.Log($"Total Cells: {gridSize * gridSize}");
            Debug.Log($"Memory Usage: {GetMemoryUsageKB()}KB");
            Debug.Log("───────────────────────────────────────────────");
            Debug.Log($"Floor: {cellCounts[(int)GridCell.Floor]}");
            Debug.Log($"Room: {cellCounts[(int)GridCell.Room]}");
            Debug.Log($"Corridor: {cellCounts[(int)GridCell.Corridor]}");
            Debug.Log($"Wall: {cellCounts[(int)GridCell.Wall]}");
            Debug.Log($"SpawnPoint: {cellCounts[(int)GridCell.SpawnPoint]}");
            Debug.Log($"Door: {cellCounts[(int)GridCell.Door]}");
            Debug.Log($"Torch: {cellCounts[(int)GridCell.Torch]}");
            Debug.Log($"Chest: {cellCounts[(int)GridCell.Chest]}");
            Debug.Log($"Enemy: {cellCounts[(int)GridCell.Enemy]}");
            Debug.Log($"Item: {cellCounts[(int)GridCell.Item]}");
            Debug.Log("═══════════════════════════════════════════════");
        }

        #endregion

        #region Debug

        void OnGUI()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 10, 350, 200));
            GUILayout.Label($"[ComputeGridEngine] Grid: {gridSize}x{gridSize}");
            GUILayout.Label($"[ComputeGridEngine] Initialized: {_isInitialized}");
            GUILayout.Label($"[ComputeGridEngine] RAM Storage: {useRamStorage}");
            GUILayout.Label($"[ComputeGridEngine] Binary Persistence: {useBinaryPersistence}");
            GUILayout.Label($"[ComputeGridEngine] Memory Usage: {GetMemoryUsageKB()}KB");
            GUILayout.Label($"[ComputeGridEngine] Maze ID: {mazeId}");
            GUILayout.Label($"[ComputeGridEngine] Seed: {mazeSeed}");
            GUILayout.EndArea();
        }

        #endregion
    }
}
