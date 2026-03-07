// WallPositionArchitect.cs
// Stores ALL maze element positions in RAM (like an architect's complete blueprint)
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// WallPositionArchitect - Stores ALL maze element positions in RAM.
    /// Like an architect's complete blueprint kept in memory.
    /// Supports: Walls, Torches, Chests, Enemies, Items, Doors, Special Rooms
    /// </summary>
    public static class WallPositionArchitect
    {
        // Stored in RAM - 512MB can hold millions of positions
        private static readonly List<WallRecord> _wallRecords = new List<WallRecord>();
        private static readonly List<TorchRecord> _torchRecords = new List<TorchRecord>();
        private static readonly List<ChestRecord> _chestRecords = new List<ChestRecord>();
        private static readonly List<EnemyRecord> _enemyRecords = new List<EnemyRecord>();
        private static readonly List<ItemRecord> _itemRecords = new List<ItemRecord>();
        private static readonly List<DoorRecord> _doorRecords = new List<DoorRecord>();
        private static readonly List<RoomRecord> _roomRecords = new List<RoomRecord>();

        public struct WallRecord
        {
            public Vector3 position;
            public Quaternion rotation;
            public string wallType;
        }

        public struct TorchRecord
        {
            public Vector3 position;
            public Quaternion rotation;
            public float height;
            public float inset;
            public string guid;
        }

        public struct ChestRecord
        {
            public Vector3 position;
            public Quaternion rotation;
            public string chestType;
            public bool isOpen;
        }

        public struct EnemyRecord
        {
            public Vector3 position;
            public Quaternion rotation;
            public string enemyType;
            public float health;
        }

        public struct ItemRecord
        {
            public Vector3 position;
            public Quaternion rotation;
            public string itemType;
            public int quantity;
        }

        public struct DoorRecord
        {
            public Vector3 position;
            public Quaternion rotation;
            public string doorType;
            public bool isOpen;
            public bool isLocked;
        }

        public struct RoomRecord
        {
            public Vector3 center;
            public Vector2 size;
            public string roomType;
        }

        /// <summary>Record a wall position in RAM.</summary>
        public static void RecordWall(Vector3 pos, Quaternion rot, string type = "MazeWall")
        {
            _wallRecords.Add(new WallRecord { position = pos, rotation = rot, wallType = type });
        }

        /// <summary>Record a torch position in RAM.</summary>
        public static void RecordTorch(Vector3 pos, Quaternion rot, float height, float inset)
        {
            _torchRecords.Add(new TorchRecord { position = pos, rotation = rot, height = height, inset = inset, guid = "N/A" });
        }

        /// <summary>Record a torch position in RAM with unique GUID.</summary>
        public static void RecordTorchWithGUID(Vector3 pos, Quaternion rot, float height, float inset, string guid)
        {
            _torchRecords.Add(new TorchRecord { position = pos, rotation = rot, height = height, inset = inset, guid = guid });
        }

        /// <summary>Record a chest position in RAM.</summary>
        public static void RecordChest(Vector3 pos, Quaternion rot, string type = "Normal", bool isOpen = false)
        {
            _chestRecords.Add(new ChestRecord { position = pos, rotation = rot, chestType = type, isOpen = isOpen });
        }

        /// <summary>Record an enemy position in RAM.</summary>
        public static void RecordEnemy(Vector3 pos, Quaternion rot, string type = "Basic", float health = 100f)
        {
            _enemyRecords.Add(new EnemyRecord { position = pos, rotation = rot, enemyType = type, health = health });
        }

        /// <summary>Record an item position in RAM.</summary>
        public static void RecordItem(Vector3 pos, Quaternion rot, string type = "Generic", int quantity = 1)
        {
            _itemRecords.Add(new ItemRecord { position = pos, rotation = rot, itemType = type, quantity = quantity });
        }

        /// <summary>Record a door position in RAM.</summary>
        public static void RecordDoor(Vector3 pos, Quaternion rot, string type = "Normal", bool isOpen = false, bool isLocked = false)
        {
            _doorRecords.Add(new DoorRecord { position = pos, rotation = rot, doorType = type, isOpen = isOpen, isLocked = isLocked });
        }

        /// <summary>Record a room position in RAM.</summary>
        public static void RecordRoom(Vector3 center, Vector2 size, string type = "Normal")
        {
            _roomRecords.Add(new RoomRecord { center = center, size = size, roomType = type });
        }

        /// <summary>Get all records from RAM.</summary>
        public static List<WallRecord> GetWallRecords() => _wallRecords;
        public static List<TorchRecord> GetTorchRecords() => _torchRecords;
        public static List<ChestRecord> GetChestRecords() => _chestRecords;
        public static List<EnemyRecord> GetEnemyRecords() => _enemyRecords;
        public static List<ItemRecord> GetItemRecords() => _itemRecords;
        public static List<DoorRecord> GetDoorRecords() => _doorRecords;
        public static List<RoomRecord> GetRoomRecords() => _roomRecords;

        /// <summary>Clear RAM (call when maze regenerated).</summary>
        public static void Clear()
        {
            _wallRecords.Clear();
            _torchRecords.Clear();
            _chestRecords.Clear();
            _enemyRecords.Clear();
            _itemRecords.Clear();
            _doorRecords.Clear();
            _roomRecords.Clear();
        }

        /// <summary>Get memory usage estimate.</summary>
        public static string GetMemoryUsage()
        {
            int total = _wallRecords.Count + _torchRecords.Count + _chestRecords.Count +
                       _enemyRecords.Count + _itemRecords.Count + _doorRecords.Count + _roomRecords.Count;
            int totalKB = (total * 32) / 1024;
            return $"Total Elements: {total} | RAM Usage: ~{totalKB}KB (of 512MB available)";
        }

        /// <summary>Print complete blueprint to console.</summary>
        public static void PrintBlueprint()
        {
            Debug.Log("═══════════════════════════════════════════════");
            Debug.Log("  ARCHITECT'S BLUEPRINT (In RAM)");
            Debug.Log("═══════════════════════════════════════════════");
            Debug.Log($"Walls: {_wallRecords.Count}");
            Debug.Log($"Torches: {_torchRecords.Count}");
            Debug.Log($"Chests: {_chestRecords.Count}");
            Debug.Log($"Enemies: {_enemyRecords.Count}");
            Debug.Log($"Items: {_itemRecords.Count}");
            Debug.Log($"Doors: {_doorRecords.Count}");
            Debug.Log($"Rooms: {_roomRecords.Count}");
            Debug.Log($"Memory: {GetMemoryUsage()}");
            
            // Show first 5 torches with GUIDs
            if (_torchRecords.Count > 0)
            {
                Debug.Log("─── First 5 Torches ───");
                for (int i = 0; i < Mathf.Min(5, _torchRecords.Count); i++)
                {
                    var torch = _torchRecords[i];
                    Debug.Log($"  [{i+1}] GUID: {torch.guid.Substring(0, 8)}... | Pos: {torch.position}");
                }
            }
            
            Debug.Log("═══════════════════════════════════════════════");
        }
    }
}
