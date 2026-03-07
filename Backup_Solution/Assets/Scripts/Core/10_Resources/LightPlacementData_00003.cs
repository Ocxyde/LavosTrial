// LightPlacementData.cs
// Binary storage and encryption for light placement data
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT ARCHITECTURE:
// - Stores torch/light positions in encrypted binary format
// - Fast XOR cipher encryption with seed-derived key
// - Batch loading for performance (no runtime teleportation)
// - Supports save/load across sessions
//
// BINARY FORMAT:
//   Header (16 bytes):
//     - Magic: 0x544F5243 ("TORC")
//     - Version: 1
//     - Count: Number of lights
//     - Flags: Encryption/compression flags
//   
//   Light Data (32 bytes per light):
//     - Position: float3 (12 bytes)
//     - Rotation: Quaternion (16 bytes)
//     - Height: float (4 bytes)
//
// USAGE:
//   // Save torch positions
//   byte[] data = LightPlacementData.SaveTorches(torchRecords, seed);
//   LightPlacementData.SaveToFile(data, mazeId);
//   
//   // Load torch positions
//   byte[] data = LightPlacementData.LoadFromFile(mazeId);
//   List<TorchRecord> torches = LightPlacementData.LoadTorches(data, seed);
//
// Location: Assets/Scripts/Core/10_Resources/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// LightPlacementData - Binary storage and encryption for light placement data.
    /// Provides fast, secure storage for torch and light-emitting object positions.
    /// </summary>
    public static class LightPlacementData
    {
        // Binary format constants
        private const uint MAGIC_NUMBER = 0x544F5243; // "TORC" in ASCII
        private const int VERSION = 1;
        private const int HEADER_SIZE = 16;
        private const int LIGHT_DATA_SIZE = 32;
        
        // Flag bits
        private const int FLAG_ENCRYPTED = 1;
        private const int FLAG_COMPRESSED = 2;
        
        /// <summary>
        /// Save torch records to encrypted binary format.
        /// </summary>
        /// <param name="torches">List of torch records to save</param>
        /// <param name="seed">Seed for encryption key derivation</param>
        /// <returns>Encrypted binary data</returns>
        public static byte[] SaveTorches(List<WallPositionArchitect.TorchRecord> torches, int seed)
        {
            if (torches == null || torches.Count == 0)
            {
                Debug.LogWarning("[LightPlacementData] No torches to save");
                return new byte[0];
            }
            
            // Calculate buffer size
            int bufferSize = HEADER_SIZE + (torches.Count * LIGHT_DATA_SIZE);
            byte[] buffer = new byte[bufferSize];
            
            // Write header
            WriteHeader(buffer, torches.Count, FLAG_ENCRYPTED);
            
            // Write torch data
            int offset = HEADER_SIZE;
            foreach (var torch in torches)
            {
                WriteLightData(buffer, ref offset, torch.position, torch.rotation, torch.height);
            }
            
            // Encrypt using LightCipher (skip header for validation)
            LightCipher.EncryptInPlace(buffer, HEADER_SIZE, bufferSize - HEADER_SIZE, seed);
            
            Debug.Log($"[LightPlacementData] Saved {torches.Count} torches ({buffer.Length} bytes)");
            return buffer;
        }
        
        /// <summary>
        /// Load torch records from encrypted binary data.
        /// </summary>
        /// <param name="data">Encrypted binary data</param>
        /// <param name="seed">Seed for decryption key derivation</param>
        /// <returns>List of torch records</returns>
        public static List<WallPositionArchitect.TorchRecord> LoadTorches(byte[] data, int seed)
        {
            var torches = new List<WallPositionArchitect.TorchRecord>();
            
            if (data == null || data.Length < HEADER_SIZE)
            {
                Debug.LogError("[LightPlacementData] Invalid data format");
                return torches;
            }
            
            // Create working copy for decryption
            byte[] buffer = new byte[data.Length];
            Array.Copy(data, buffer, data.Length);
            
            // Decrypt using LightCipher (skip magic number for validation)
            LightCipher.DecryptInPlace(buffer, HEADER_SIZE, buffer.Length - HEADER_SIZE, seed);
            
            // Validate header
            uint magic = ReadUInt32(buffer, 0);
            if (magic != MAGIC_NUMBER)
            {
                Debug.LogError($"[LightPlacementData] Invalid magic number: {magic:X8} (expected {MAGIC_NUMBER:X8})");
                return torches;
            }
            
            int version = ReadInt32(buffer, 4);
            if (version != VERSION)
            {
                Debug.LogError($"[LightPlacementData] Unsupported version: {version} (expected {VERSION})");
                return torches;
            }
            
            int count = ReadInt32(buffer, 8);
            int flags = ReadInt32(buffer, 12);
            
            // Validate data size
            int expectedSize = HEADER_SIZE + (count * LIGHT_DATA_SIZE);
            if (buffer.Length < expectedSize)
            {
                Debug.LogError($"[LightPlacementData] Data too small: {buffer.Length} bytes (expected {expectedSize})");
                return torches;
            }
            
            // Read torch data
            int offset = HEADER_SIZE;
            for (int i = 0; i < count; i++)
            {
                ReadLightData(buffer, ref offset, out Vector3 position, out Quaternion rotation, out float height);
                
                torches.Add(new WallPositionArchitect.TorchRecord
                {
                    position = position,
                    rotation = rotation,
                    height = height,
                    inset = 0.5f, // Default inset
                    guid = $"torch_{i:D4}" // Generate GUID
                });
            }
            
            Debug.Log($"[LightPlacementData] Loaded {torches.Count} torches");
            return torches;
        }
        
        /// <summary>
        /// Save binary data to file in custom StreamingWorkFlow folder (relative path).
        /// </summary>
        /// <param name="data">Binary data to save</param>
        /// <param name="mazeId">Unique maze identifier</param>
        public static void SaveToFile(byte[] data, string mazeId)
        {
            if (data == null || data.Length == 0)
            {
                Debug.LogError("[LightPlacementData] No data to save");
                return;
            }

            string fileName = $"{mazeId}.bytes";
            string relativePath = "StreamingWorkFlow/MazeData/" + fileName;
            string filePath = Path.Combine(Application.dataPath, relativePath);

            // Ensure directory exists
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllBytes(filePath, data);
            Debug.Log($"[LightPlacementData] Saved to {relativePath}");
        }
        
        /// <summary>
        /// Load binary data from custom StreamingWorkFlow folder (relative path).
        /// </summary>
        /// <param name="mazeId">Unique maze identifier</param>
        /// <returns>Binary data or null if not found</returns>
        public static byte[] LoadFromFile(string mazeId)
        {
            string fileName = $"{mazeId}.bytes";
            string relativePath = "StreamingWorkFlow/MazeData/" + fileName;
            string filePath = Path.Combine(Application.dataPath, relativePath);

            Debug.Log($"[LightPlacementData] Trying to load from: {relativePath}");

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[LightPlacementData] File not found: {relativePath}");
                Debug.LogWarning($"[LightPlacementData] This is NORMAL on first maze generation - will save new file");
                return null;
            }

            byte[] data = File.ReadAllBytes(filePath);
            Debug.Log($"[LightPlacementData] Loaded from {relativePath} ({data.Length} bytes)");
            return data;
        }
        
        /// <summary>
        /// Check if a maze placement file exists.
        /// </summary>
        public static bool FileExists(string mazeId)
        {
            string fileName = $"{mazeId}.bytes";
            string relativePath = "StreamingWorkFlow/MazeData/" + fileName;
            string filePath = Path.Combine(Application.dataPath, relativePath);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Delete a maze placement file.
        /// </summary>
        public static void DeleteFile(string mazeId)
        {
            string fileName = $"{mazeId}.bytes";
            string relativePath = "StreamingWorkFlow/MazeData/" + fileName;
            string filePath = Path.Combine(Application.dataPath, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[LightPlacementData] Deleted {relativePath}");
            }
        }
        
        #region Binary I/O Helpers
        
        private static void WriteHeader(byte[] buffer, int count, int flags)
        {
            WriteUInt32(buffer, 0, MAGIC_NUMBER);
            WriteInt32(buffer, 4, VERSION);
            WriteInt32(buffer, 8, count);
            WriteInt32(buffer, 12, flags);
        }
        
        private static void WriteLightData(byte[] buffer, ref int offset, Vector3 position, Quaternion rotation, float height)
        {
            // Position (12 bytes)
            WriteFloat(buffer, offset, position.x);
            WriteFloat(buffer, offset + 4, position.y);
            WriteFloat(buffer, offset + 8, position.z);
            
            // Rotation (16 bytes)
            WriteFloat(buffer, offset + 12, rotation.x);
            WriteFloat(buffer, offset + 16, rotation.y);
            WriteFloat(buffer, offset + 20, rotation.z);
            WriteFloat(buffer, offset + 24, rotation.w);
            
            // Height (4 bytes)
            WriteFloat(buffer, offset + 28, height);
            
            offset += LIGHT_DATA_SIZE;
        }
        
        private static void ReadLightData(byte[] buffer, ref int offset, out Vector3 position, out Quaternion rotation, out float height)
        {
            // Position
            float x = ReadFloat(buffer, offset);
            float y = ReadFloat(buffer, offset + 4);
            float z = ReadFloat(buffer, offset + 8);
            position = new Vector3(x, y, z);
            
            // Rotation
            float rx = ReadFloat(buffer, offset + 12);
            float ry = ReadFloat(buffer, offset + 16);
            float rz = ReadFloat(buffer, offset + 20);
            float rw = ReadFloat(buffer, offset + 24);
            rotation = new Quaternion(rx, ry, rz, rw);
            
            // Height
            height = ReadFloat(buffer, offset + 28);
            
            offset += LIGHT_DATA_SIZE;
        }
        
        private static void WriteUInt32(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }
        
        private static uint ReadUInt32(byte[] buffer, int offset)
        {
            return (uint)(buffer[offset] | (buffer[offset + 1] << 8) | 
                         (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24));
        }
        
        private static void WriteInt32(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }
        
        private static int ReadInt32(byte[] buffer, int offset)
        {
            return buffer[offset] | (buffer[offset + 1] << 8) | 
                   (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);
        }
        
        private static void WriteFloat(byte[] buffer, int offset, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, buffer, offset, 4);
        }
        
        private static float ReadFloat(byte[] buffer, int offset)
        {
            return BitConverter.ToSingle(buffer, offset);
        }

        #endregion

        #region Generic Light Data (for other light-emitting objects)

        /// <summary>
        /// Save generic light positions (candles, lamps, etc.) to binary format.
        /// </summary>
        public static byte[] SaveLights(List<LightData> lights, int seed)
        {
            if (lights == null || lights.Count == 0)
            {
                Debug.LogWarning("[LightPlacementData] No lights to save");
                return new byte[0];
            }

            int bufferSize = HEADER_SIZE + (lights.Count * LIGHT_DATA_SIZE);
            byte[] buffer = new byte[bufferSize];

            WriteHeader(buffer, lights.Count, FLAG_ENCRYPTED);

            int offset = HEADER_SIZE;
            foreach (var light in lights)
            {
                WriteLightData(buffer, ref offset, light.position, light.rotation, 0f);
            }

            // Encrypt using LightCipher
            LightCipher.EncryptInPlace(buffer, HEADER_SIZE, bufferSize - HEADER_SIZE, seed);

            Debug.Log($"[LightPlacementData] Saved {lights.Count} lights ({buffer.Length} bytes)");
            return buffer;
        }
        
        /// <summary>
        /// Load generic light positions from binary data.
        /// </summary>
        public static List<LightData> LoadLights(byte[] data, int seed)
        {
            var lights = new List<LightData>();

            if (data == null || data.Length < HEADER_SIZE)
            {
                Debug.LogError("[LightPlacementData] Invalid data format");
                return lights;
            }

            byte[] buffer = new byte[data.Length];
            Array.Copy(data, buffer, data.Length);

            // Decrypt using LightCipher
            LightCipher.DecryptInPlace(buffer, HEADER_SIZE, buffer.Length - HEADER_SIZE, seed);

            uint magic = ReadUInt32(buffer, 0);
            if (magic != MAGIC_NUMBER)
            {
                Debug.LogError($"[LightPlacementData] Invalid magic number: {magic:X8}");
                return lights;
            }

            int count = ReadInt32(buffer, 8);
            int offset = HEADER_SIZE;

            for (int i = 0; i < count; i++)
            {
                ReadLightData(buffer, ref offset, out Vector3 position, out Quaternion rotation, out _);

                lights.Add(new LightData
                {
                    position = position,
                    rotation = rotation
                });
            }

            Debug.Log($"[LightPlacementData] Loaded {lights.Count} lights");
            return lights;
        }
        
        /// <summary>
        /// Generic light data structure for non-torch light sources.
        /// </summary>
        public struct LightData
        {
            public Vector3 position;
            public Quaternion rotation;
            public string lightType;
        }
        
        #endregion
    }
}
