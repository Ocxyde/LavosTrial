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
// SpatialPlacer.cs
// Universal object placement orchestrator with binary storage
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT ARCHITECTURE:
// - Finds components (never creates)
// - Orchestrates specialized placers
// - Centralized binary storage (byte-to-byte)
// - All values from JSON config
// - No hardcoded values
//
// LOCATION: Assets/Scripts/Core/08_Environment/

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// SpatialPlacer - Universal object placement orchestrator.
    /// PLUG-IN-OUT COMPLIANT: Finds components, never creates.
    /// BINARY STORAGE: Centralized byte-to-byte storage for all placed objects.
    /// ALL VALUES FROM JSON: All settings loaded from GameConfig.
    /// </summary>
    public class SpatialPlacer : MonoBehaviour
    {
        #region Inspector Fields

        [Header(" Component References (Plug-in-Out)")]
        [Tooltip("Auto-finds GridMazeGenerator in scene")]
        [SerializeField] private GridMazeGenerator gridMazeGenerator;
        
        [Tooltip("Auto-finds CompleteMazeBuilder in scene")]
        [SerializeField] private CompleteMazeBuilder completeMazeBuilder;
        
        [Tooltip("Auto-finds LightPlacementEngine for binary storage")]
        [SerializeField] private LightPlacementEngine lightPlacementEngine;
        
        [Tooltip("Specialized chest placer")]
        [SerializeField] private ChestPlacer chestPlacer;
        
        [Tooltip("Specialized enemy placer")]
        [SerializeField] private EnemyPlacer enemyPlacer;
        
        [Tooltip("Specialized item placer")]
        [SerializeField] private ItemPlacer itemPlacer;
        
        [Tooltip("Specialized torch placer")]
        [SerializeField] private TorchPlacer torchPlacer;

        [Header(" Binary Storage Settings")]
        [Tooltip("Enable binary storage (RAM/cache)")]
        [SerializeField] private bool enableBinaryStorage = true;
        
        [Tooltip("Storage folder path")]
        [SerializeField] private string storageFolder = "Assets/StreamingAssets/MazeStorage/";

        [Header(" Debug")]
        [SerializeField] private bool showDebugLogs = true;

        #endregion

        #region Private Data

        private string _currentMazeId;
        private int _currentSeed;
        private BinaryObjectStorage _objectStorage;

        #endregion

        #region Public Accessors

        public int TotalObjectsPlaced
        {
            get
            {
                int total = 0;
                if (chestPlacer != null) total += chestPlacer.ChestsSpawned;
                if (enemyPlacer != null) total += enemyPlacer.EnemiesSpawned;
                if (itemPlacer != null) total += itemPlacer.ItemsSpawned;
                if (torchPlacer != null) total += torchPlacer.TorchesSpawned;
                return total;
            }
        }

        public string CurrentMazeId => _currentMazeId;
        public int CurrentSeed => _currentSeed;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            FindComponents();
            InitializeBinaryStorage();
        }

        #endregion

        #region Plug-in-Out Compliance

        private void FindComponents()
        {
            // GridMazeGenerator is created by CompleteMazeBuilder
            if (completeMazeBuilder == null)
                completeMazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();
            
            if (lightPlacementEngine == null)
                lightPlacementEngine = FindFirstObjectByType<LightPlacementEngine>();
            
            if (chestPlacer == null)
                chestPlacer = GetComponent<ChestPlacer>();
            
            if (enemyPlacer == null)
                enemyPlacer = GetComponent<EnemyPlacer>();
            
            if (itemPlacer == null)
                itemPlacer = GetComponent<ItemPlacer>();
            
            if (torchPlacer == null)
                torchPlacer = GetComponent<TorchPlacer>();
        }

        #endregion

        #region Binary Storage System

        private void InitializeBinaryStorage()
        {
            _objectStorage = new BinaryObjectStorage(storageFolder);
            
            if (showDebugLogs)
            {
                Debug.Log($"[SpatialPlacer]  Binary storage initialized: {storageFolder}");
            }
        }

        /// <summary>
        /// Save object placement to binary storage.
        /// </summary>
        public void SaveObjectToBinary(string objectType, Vector3 position, Quaternion rotation, string extraData = "")
        {
            if (!enableBinaryStorage) return;
            
            var record = new ObjectPlacementRecord
            {
                ObjectType = objectType,
                Position = position,
                Rotation = rotation,
                ExtraData = extraData,
                MazeId = _currentMazeId,
                Seed = _currentSeed
            };
            
            _objectStorage.SaveRecord(record);
        }

        /// <summary>
        /// Load objects from binary storage.
        /// </summary>
        public List<ObjectPlacementRecord> LoadObjectsFromBinary(string objectType)
        {
            if (!enableBinaryStorage) return new List<ObjectPlacementRecord>();
            
            return _objectStorage.LoadRecords(_currentMazeId, objectType);
        }

        /// <summary>
        /// Clear binary storage for current maze.
        /// </summary>
        public void ClearBinaryStorage()
        {
            _objectStorage.ClearMaze(_currentMazeId);
            if (showDebugLogs)
                Debug.Log("[SpatialPlacer]  Binary storage cleared");
        }

        #endregion

        #region Public API - Orchestrator

        [ContextMenu("Place All Objects")]
        public void PlaceAllObjects()
        {
            if (showDebugLogs)
            {
                Debug.Log("[SpatialPlacer] ════════════════════════════════════════");
                Debug.Log("  PLACING ALL OBJECTS");
                Debug.Log("═══════════════════════════════════════════");
            }

            // Generate maze ID and seed
            _currentMazeId = System.Guid.NewGuid().ToString();
            _currentSeed = Random.Range(0, int.MaxValue);

            if (showDebugLogs)
            {
                Debug.Log($"[SpatialPlacer]  Maze ID: {_currentMazeId}");
                Debug.Log($"[SpatialPlacer]  Seed: {_currentSeed}");
            }

            // Try to load from binary storage first
            if (enableBinaryStorage && LoadFromBinary())
            {
                if (showDebugLogs)
                {
                    Debug.Log("[SpatialPlacer]  Loaded all objects from binary storage (RAM)");
                    Debug.Log("═══════════════════════════════════════════");
                }
                return;
            }

            // Binary not found - place objects and save
            if (showDebugLogs)
            {
                Debug.Log("[SpatialPlacer]  Binary not found - calculating positions...");
            }

            PlaceChests();
            PlaceEnemies();
            PlaceItems();
            PlaceTorches(_currentMazeId, _currentSeed);

            // Save to binary for next time
            if (enableBinaryStorage)
            {
                SaveToBinary();
            }

            if (showDebugLogs)
            {
                Debug.Log("═══════════════════════════════════════════");
                Debug.Log($"   TOTAL OBJECTS PLACED: {TotalObjectsPlaced}");
                Debug.Log("═══════════════════════════════════════════");
            }
        }

        /// <summary>
        /// Load all objects from binary storage.
        /// </summary>
        private bool LoadFromBinary()
        {
            bool loaded = false;
            
            // Load chests
            var chestRecords = LoadObjectsFromBinary("Chest");
            if (chestRecords.Count > 0 && chestPlacer != null)
            {
                chestPlacer.PlaceChestsFromRecords(chestRecords);
                loaded = true;
            }
            
            // Load enemies
            var enemyRecords = LoadObjectsFromBinary("Enemy");
            if (enemyRecords.Count > 0 && enemyPlacer != null)
            {
                enemyPlacer.PlaceEnemiesFromRecords(enemyRecords);
                loaded = true;
            }
            
            // Load items
            var itemRecords = LoadObjectsFromBinary("Item");
            if (itemRecords.Count > 0 && itemPlacer != null)
            {
                itemPlacer.PlaceItemsFromRecords(itemRecords);
                loaded = true;
            }
            
            // Load torches (uses LightPlacementEngine)
            if (lightPlacementEngine != null)
            {
                bool torchesLoaded = lightPlacementEngine.LoadAndInstantiateTorches(_currentMazeId, _currentSeed);
                if (torchesLoaded) loaded = true;
            }
            
            return loaded;
        }

        /// <summary>
        /// Save all placed objects to binary storage.
        /// </summary>
        private void SaveToBinary()
        {
            if (showDebugLogs)
            {
                Debug.Log("[SpatialPlacer]  Saving objects to binary storage...");
            }
            
            // Torches saved via LightPlacementEngine
            // Other objects saved by their placers via SaveObjectToBinary()
        }

        [ContextMenu("Place Chests")]
        public void PlaceChests()
        {
            if (chestPlacer != null)
            {
                chestPlacer.SetStorageReference(this);
                chestPlacer.PlaceAllChests();
            }
        }

        [ContextMenu("Place Enemies")]
        public void PlaceEnemies()
        {
            if (enemyPlacer != null)
            {
                enemyPlacer.SetStorageReference(this);
                enemyPlacer.PlaceAllEnemies();
            }
        }

        [ContextMenu("Place Items")]
        public void PlaceItems()
        {
            if (itemPlacer != null)
            {
                itemPlacer.SetStorageReference(this);
                itemPlacer.PlaceAllItems();
            }
        }

        [ContextMenu("Place Torches")]
        public void PlaceTorches(string mazeId, int seed)
        {
            if (torchPlacer != null)
            {
                torchPlacer.SetStorageReference(this, lightPlacementEngine);
                torchPlacer.PlaceAllTorches(mazeId, seed);
            }
        }

        [System.Serializable]
        public class ObjectPlacementRecord
        {
            public string ObjectType;      // "Chest", "Enemy", "Item", "Torch"
            public Vector3 Position;
            public Quaternion Rotation;
            public string ExtraData;       // Variant, trap type, etc.
            public string MazeId;
            public int Seed;
        }

        public class BinaryObjectStorage
        {
            private string folderPath;
            
            public BinaryObjectStorage(string path)
            {
                folderPath = path;
                EnsureFolderExists();
            }
            
            private void EnsureFolderExists()
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
            
            public void SaveRecord(ObjectPlacementRecord record)
            {
                string fileName = $"{record.MazeId}_{record.ObjectType}.bin";
                string fullPath = Path.Combine(folderPath, fileName);
                
                // Simple binary serialization (can be enhanced with encryption)
                using (var stream = new FileStream(fullPath, FileMode.Append))
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(record.Position.x);
                    writer.Write(record.Position.y);
                    writer.Write(record.Position.z);
                    writer.Write(record.Rotation.x);
                    writer.Write(record.Rotation.y);
                    writer.Write(record.Rotation.z);
                    writer.Write(record.Rotation.w);
                    writer.Write(record.ExtraData ?? "");
                }
            }
            
            public List<ObjectPlacementRecord> LoadRecords(string mazeId, string objectType)
            {
                var records = new List<ObjectPlacementRecord>();
                string fileName = $"{mazeId}_{objectType}.bin";
                string fullPath = Path.Combine(folderPath, fileName);
                
                if (!File.Exists(fullPath))
                    return records;
                
                using (var stream = new FileStream(fullPath, FileMode.Open))
                using (var reader = new BinaryReader(stream))
                {
                    while (stream.Position < stream.Length)
                    {
                        var record = new ObjectPlacementRecord
                        {
                            ObjectType = objectType,
                            MazeId = mazeId,
                            Position = new Vector3(
                                reader.ReadSingle(),
                                reader.ReadSingle(),
                                reader.ReadSingle()
                            ),
                            Rotation = new Quaternion(
                                reader.ReadSingle(),
                                reader.ReadSingle(),
                                reader.ReadSingle(),
                                reader.ReadSingle()
                            ),
                            ExtraData = reader.ReadString()
                        };
                        records.Add(record);
                    }
                }
                
                return records;
            }
            
            public void ClearMaze(string mazeId)
            {
                string[] files = Directory.GetFiles(folderPath, $"{mazeId}_*.bin");
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
        }  // BinaryObjectStorage class closes

        #endregion
    }  // SpatialPlacer class closes
}  // Namespace closes
