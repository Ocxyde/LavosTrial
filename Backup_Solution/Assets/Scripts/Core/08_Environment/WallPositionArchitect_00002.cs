// WallPositionArchitect.cs
// Stores wall and torch positions in RAM (like an architect's blueprint in memory)
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// WallPositionArchitect - Stores all wall/torch positions in RAM.
    /// Like an architect's blueprint kept in memory for quick access.
    /// </summary>
    public static class WallPositionArchitect
    {
        // Stored in RAM - ~512MB can hold millions of positions
        private static readonly List<WallRecord> _wallRecords = new List<WallRecord>();
        private static readonly List<TorchRecord> _torchRecords = new List<TorchRecord>();

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
        }

        /// <summary>
        /// Record a wall position in RAM.
        /// </summary>
        public static void RecordWall(Vector3 pos, Quaternion rot, string type = "MazeWall")
        {
            _wallRecords.Add(new WallRecord
            {
                position = pos,
                rotation = rot,
                wallType = type
            });
        }

        /// <summary>
        /// Record a torch position in RAM.
        /// </summary>
        public static void RecordTorch(Vector3 pos, Quaternion rot, float height, float inset)
        {
            _torchRecords.Add(new TorchRecord
            {
                position = pos,
                rotation = rot,
                height = height,
                inset = inset
            });
        }

        /// <summary>
        /// Get all wall records from RAM.
        /// </summary>
        public static List<WallRecord> GetWallRecords() => _wallRecords;

        /// <summary>
        /// Get all torch records from RAM.
        /// </summary>
        public static List<TorchRecord> GetTorchRecords() => _torchRecords;

        /// <summary>
        /// Clear RAM (call when maze regenerated).
        /// </summary>
        public static void Clear()
        {
            _wallRecords.Clear();
            _torchRecords.Clear();
        }

        /// <summary>
        /// Get memory usage estimate.
        /// </summary>
        public static string GetMemoryUsage()
        {
            int wallBytes = _wallRecords.Count * 32; // Vector3(12) + Quaternion(16) + string(4)
            int torchBytes = _torchRecords.Count * 24; // Vector3(12) + Quaternion(16) + 2 floats(8)
            int totalKB = (wallBytes + torchBytes) / 1024;
            return $"Walls: {_wallRecords.Count} | Torches: {_torchRecords.Count} | RAM: ~{totalKB}KB";
        }
    }
}
