// WallPositionArchitect.cs
// Saves wall and torch positions like an architect's map plan
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// WallPositionArchitect - Records and saves all wall/torch positions.
    /// Like an architect's blueprint for the maze.
    /// </summary>
    public static class WallPositionArchitect
    {
        private static List<WallRecord> _wallRecords = new List<WallRecord>();
        private static List<TorchRecord> _torchRecords = new List<TorchRecord>();

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
        /// Record a wall position.
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
        /// Record a torch position.
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
        /// Save all records to file (like an architect's blueprint).
        /// </summary>
        public static void SaveBlueprint(string filename = "MazeBlueprint.txt")
        {
            string path = Path.Combine(Application.dataPath, "..", "Blueprints", filename);
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("═══════════════════════════════════════════════");
                writer.WriteLine("  MAZE BLUEPRINT - Architect's Plan");
                writer.WriteLine("═══════════════════════════════════════════════");
                writer.WriteLine($"Generated: {System.DateTime.Now}");
                writer.WriteLine($"Total Walls: {_wallRecords.Count}");
                writer.WriteLine($"Total Torches: {_torchRecords.Count}");
                writer.WriteLine();

                writer.WriteLine("═══════════════════════════════════════════════");
                writer.WriteLine("  WALL POSITIONS");
                writer.WriteLine("═══════════════════════════════════════════════");
                for (int i = 0; i < _wallRecords.Count; i++)
                {
                    var wall = _wallRecords[i];
                    writer.WriteLine($"[{i + 1:000}] Pos: {wall.position,15} | Rot: {wall.rotation.eulerAngles,12} | Type: {wall.wallType}");
                }

                writer.WriteLine();
                writer.WriteLine("═══════════════════════════════════════════════");
                writer.WriteLine("  TORCH POSITIONS");
                writer.WriteLine("═══════════════════════════════════════════════");
                for (int i = 0; i < _torchRecords.Count; i++)
                {
                    var torch = _torchRecords[i];
                    writer.WriteLine($"[{i + 1:000}] Pos: {torch.position,15} | Rot: {torch.rotation.eulerAngles,12} | Height: {torch.height:F2} | Inset: {torch.inset:F2}");
                }
            }

            Debug.Log($"[WallPositionArchitect] Blueprint saved to: {path}");
        }

        /// <summary>
        /// Clear all records.
        /// </summary>
        public static void Clear()
        {
            _wallRecords.Clear();
            _torchRecords.Clear();
        }

        /// <summary>
        /// Get all wall records.
        /// </summary>
        public static List<WallRecord> GetWallRecords() => _wallRecords;

        /// <summary>
        /// Get all torch records.
        /// </summary>
        public static List<TorchRecord> GetTorchRecords() => _torchRecords;
    }
}
