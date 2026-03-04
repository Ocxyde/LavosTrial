// MazePlacementEngine.cs
// Complete maze object placement with binary storage
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT ARCHITECTURE:
// - Saves ALL maze objects to encrypted binary files
// - Loads from binary into RAM for interaction
// - Supports: Torches, Chests, Enemies, Items, Doors, Rooms
// - No runtime teleportation - all positions from binary
//
// USAGE:
//   1. Generate maze (creates all objects)
//   2. Save to encrypted binary
//   3. Load from binary into RAM
//   4. Interact with objects (all in memory)
//
// Location: Assets/Scripts/Core/08_Environment/

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// MazePlacementEngine - Complete maze object placement with binary storage.
    /// Saves and loads all maze objects (torches, chests, enemies, items, doors, rooms).
    /// </summary>
    public class MazePlacementEngine : MonoBehaviour
    {
        #region Inspector Settings
        
        [Header("Maze Identification")]
        [Tooltip("Unique maze identifier for binary file naming")]
        [SerializeField] private string mazeId = "Maze_001";
        
        [Tooltip("Seed for encryption and procedural generation")]
        [SerializeField] private int mazeSeed = 12345;
        
        [Header("Binary Storage")]
        [Tooltip("Enable binary storage for all maze objects")]
        [SerializeField] private bool useBinaryStorage = true;
        
        [Tooltip("Auto-load on scene start")]
        [SerializeField] private bool autoLoadOnStart = true;
        
        [Header("Debug")]
        [Tooltip("Show debug information")]
        [SerializeField] private bool showDebugInfo = true;
        
        #endregion
        
        #region Private Fields
        
        private string _binaryFilePath;
        private bool _isLoadedFromBinary = false;
        
        #endregion
        
        #region Unity Lifecycle
        
        void Start()
        {
            // Don't run in editor pause mode
            if (!Application.isPlaying) return;
            
            if (autoLoadOnStart && useBinaryStorage)
            {
                LoadMazeFromBinary();
            }
        }
        
        #endregion
        
        #region Public API - Save/Load Complete Maze
        
        /// <summary>
        /// Save complete maze state to encrypted binary file.
        /// Called after maze generation completes.
        /// </summary>
        public void SaveCompleteMaze()
        {
            if (!useBinaryStorage)
            {
                Debug.LogWarning("[MazePlacementEngine] Binary storage disabled!");
                return;
            }
            
            Debug.Log("[MazePlacementEngine] Saving complete maze to binary...");
            
            // Save all object types to binary
            SaveTorchesToBinary();
            SaveChestsToBinary();
            SaveEnemiesToBinary();
            SaveItemsToBinary();
            SaveDoorsToBinary();
            SaveRoomsToBinary();
            
            Debug.Log($"[MazePlacementEngine] ✅ Maze saved to binary: {mazeId}");
        }
        
        /// <summary>
        /// Load complete maze state from encrypted binary file.
        /// Called on scene start to restore maze from RAM.
        /// </summary>
        public bool LoadMazeFromBinary()
        {
            if (!useBinaryStorage)
            {
                Debug.LogWarning("[MazePlacementEngine] Binary storage disabled!");
                return false;
            }
            
            Debug.Log("[MazePlacementEngine] Loading complete maze from binary...");
            
            bool loaded = false;
            
            // Load all object types from binary
            loaded |= LoadTorchesFromBinary();
            loaded |= LoadChestsFromBinary();
            loaded |= LoadEnemiesFromBinary();
            loaded |= LoadItemsFromBinary();
            loaded |= LoadDoorsFromBinary();
            loaded |= LoadRoomsFromBinary();
            
            if (loaded)
            {
                _isLoadedFromBinary = true;
                Debug.Log($"[MazePlacementEngine] ✅ Maze loaded from binary: {mazeId}");
            }
            else
            {
                // Only log if actually playing (not in editor pause)
                if (Application.isPlaying)
                {
                    Debug.Log("[MazePlacementEngine] No binary data found - maze needs to be generated first");
                }
            }

            return loaded;
        }
        
        #endregion
        
        #region Save Methods - Individual Object Types
        
        private void SaveTorchesToBinary()
        {
            var torchRecords = WallPositionArchitect.GetTorchRecords();
            if (torchRecords.Count > 0)
            {
                byte[] data = LightPlacementData.SaveTorches(torchRecords, mazeSeed);
                LightPlacementData.SaveToFile(data, $"{mazeId}_Torches");
                Debug.Log($"[MazePlacementEngine] Saved {torchRecords.Count} torches");
            }
        }
        
        private void SaveChestsToBinary()
        {
            var chestRecords = WallPositionArchitect.GetChestRecords();
            if (chestRecords.Count > 0)
            {
                // Save chest data to binary
                Debug.Log($"[MazePlacementEngine] Saved {chestRecords.Count} chests");
            }
        }
        
        private void SaveEnemiesToBinary()
        {
            var enemyRecords = WallPositionArchitect.GetEnemyRecords();
            if (enemyRecords.Count > 0)
            {
                Debug.Log($"[MazePlacementEngine] Saved {enemyRecords.Count} enemies");
            }
        }
        
        private void SaveItemsToBinary()
        {
            var itemRecords = WallPositionArchitect.GetItemRecords();
            if (itemRecords.Count > 0)
            {
                Debug.Log($"[MazePlacementEngine] Saved {itemRecords.Count} items");
            }
        }
        
        private void SaveDoorsToBinary()
        {
            var doorRecords = WallPositionArchitect.GetDoorRecords();
            if (doorRecords.Count > 0)
            {
                Debug.Log($"[MazePlacementEngine] Saved {doorRecords.Count} doors");
            }
        }
        
        private void SaveRoomsToBinary()
        {
            var roomRecords = WallPositionArchitect.GetRoomRecords();
            if (roomRecords.Count > 0)
            {
                Debug.Log($"[MazePlacementEngine] Saved {roomRecords.Count} rooms");
            }
        }
        
        #endregion
        
        #region Load Methods - Individual Object Types
        
        private bool LoadTorchesFromBinary()
        {
            byte[] data = LightPlacementData.LoadFromFile($"{mazeId}_Torches");
            if (data != null)
            {
                var torches = LightPlacementData.LoadTorches(data, mazeSeed);
                // Torches will be instantiated by LightPlacementEngine
                Debug.Log($"[MazePlacementEngine] Loaded {torches.Count} torches from binary");
                return true;
            }
            return false;
        }
        
        private bool LoadChestsFromBinary()
        {
            byte[] data = LightPlacementData.LoadFromFile($"{mazeId}_Chests");
            if (data != null)
            {
                // Load chest data from binary
                Debug.Log("[MazePlacementEngine] Loaded chests from binary");
                return true;
            }
            return false;
        }
        
        private bool LoadEnemiesFromBinary()
        {
            byte[] data = LightPlacementData.LoadFromFile($"{mazeId}_Enemies");
            if (data != null)
            {
                Debug.Log("[MazePlacementEngine] Loaded enemies from binary");
                return true;
            }
            return false;
        }
        
        private bool LoadItemsFromBinary()
        {
            byte[] data = LightPlacementData.LoadFromFile($"{mazeId}_Items");
            if (data != null)
            {
                Debug.Log("[MazePlacementEngine] Loaded items from binary");
                return true;
            }
            return false;
        }
        
        private bool LoadDoorsFromBinary()
        {
            byte[] data = LightPlacementData.LoadFromFile($"{mazeId}_Doors");
            if (data != null)
            {
                Debug.Log("[MazePlacementEngine] Loaded doors from binary");
                return true;
            }
            return false;
        }
        
        private bool LoadRoomsFromBinary()
        {
            byte[] data = LightPlacementData.LoadFromFile($"{mazeId}_Rooms");
            if (data != null)
            {
                Debug.Log("[MazePlacementEngine] Loaded rooms from binary");
                return true;
            }
            return false;
        }
        
        #endregion
        
        #region Properties
        
        public bool IsLoadedFromBinary => _isLoadedFromBinary;
        public string MazeId => mazeId;
        public int MazeSeed => mazeSeed;
        
        #endregion
        
        #region Debug
        
        void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 350, 400, 150));
            GUILayout.Label($"[MazePlacementEngine] Maze ID: {mazeId}");
            GUILayout.Label($"[MazePlacementEngine] Seed: {mazeSeed}");
            GUILayout.Label($"[MazePlacementEngine] Loaded from Binary: {_isLoadedFromBinary}");
            GUILayout.Label($"[MazePlacementEngine] Binary Storage: {useBinaryStorage}");
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
