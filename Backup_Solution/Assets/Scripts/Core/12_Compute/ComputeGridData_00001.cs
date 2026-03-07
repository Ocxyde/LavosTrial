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
// ComputeGridData.cs
// BINARY DATA HANDLER - Compute grid storage to disk
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT ARCHITECTURE:
// - Independent binary data handler
// - Stores compute grid to encrypted binary files
// - Located in Assets/StreamingAssets/ComputeGrid/
// - Fast I/O for byte-to-byte operations
// - XOR encryption with seed-derived keys
//
// FILE FORMAT:
// [Magic: 4 bytes][Version: 2 bytes][GridSize: 2 bytes][Seed: 4 bytes][Data: N bytes][Checksum: 4 bytes]
//
// USAGE:
//   ComputeGridData.SaveGrid(mazeId, gridBytes, seed)
//   ComputeGridData.LoadGrid(mazeId, seed)
//   ComputeGridData.DeleteGrid(mazeId)
//
// Location: Assets/Scripts/Core/12_Compute/

using System;
using System.IO;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// ComputeGridData - Binary data handler for compute grid storage.
    /// Stores grid data to encrypted binary files in StreamingAssets/ComputeGrid/.
    /// 
    /// FILE STRUCTURE:
    /// Offset  Size  Field
    /// 0       4     Magic Number (0x434F4D50 = "COMP")
    /// 4       2     Version (1)
    /// 6       2     Grid Size
    /// 8       4     Seed (for encryption)
    /// 12      N     Grid Data (1 byte per cell)
    /// 12+N    4     Checksum (CRC32)
    /// 
    /// PLUG-IN-OUT: Independent data handler, no dependencies.
    /// </summary>
    public static class ComputeGridData
    {
        // File constants
        private const uint MAGIC_NUMBER = 0x434F4D50; // "COMP" in hex
        private const ushort FILE_VERSION = 1;
        private const string FOLDER_NAME = "ComputeGrid";

        // Cache for fast access
        private static readonly System.Collections.Generic.Dictionary<string, byte[]> _cache = 
            new System.Collections.Generic.Dictionary<string, byte[]>();

        #region Save Operations

        /// <summary>
        /// Save compute grid to binary file.
        /// </summary>
        /// <param name="mazeId">Unique maze identifier</param>
        /// <param name="gridData">Grid byte array (1 byte per cell)</param>
        /// <param name="seed">Seed for encryption</param>
        /// <returns>True if saved successfully</returns>
        public static bool SaveGrid(string mazeId, byte[] gridData, int seed)
        {
            if (string.IsNullOrEmpty(mazeId))
            {
                Debug.LogError("[ComputeGridData] Maze ID is null or empty");
                return false;
            }

            if (gridData == null || gridData.Length == 0)
            {
                Debug.LogError("[ComputeGridData] Grid data is null or empty");
                return false;
            }

            Debug.Log($"[ComputeGridData] Saving grid: {mazeId} ({gridData.Length} bytes)...");

            try
            {
                // Build binary file
                using (var ms = new MemoryStream())
                {
                    // Write header
                    WriteUInt(ms, MAGIC_NUMBER);      // 4 bytes
                    WriteUShort(ms, FILE_VERSION);    // 2 bytes
                    WriteUShort(ms, (ushort)Mathf.Sqrt(gridData.Length)); // 2 bytes (grid size)
                    WriteInt(ms, seed);               // 4 bytes

                    // Encrypt and write grid data
                    byte[] encryptedData = EncryptData(gridData, seed);
                    ms.Write(encryptedData, 0, encryptedData.Length);

                    // Calculate and write checksum
                    uint checksum = CalculateChecksum(encryptedData);
                    WriteUInt(ms, checksum);          // 4 bytes

                    // Write to file
                    string filePath = GetFilePath(mazeId);
                    EnsureFolderExists();
                    File.WriteAllBytes(filePath, ms.ToArray());

                    // Update cache
                    _cache[mazeId] = gridData;

                    Debug.Log($"[ComputeGridData] Saved: {filePath} ({ms.Length} bytes)");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ComputeGridData] Save failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Save compute grid with metadata.
        /// </summary>
        public static bool SaveGridWithMetadata(string mazeId, byte[] gridData, byte[] metadata, int seed)
        {
            if (string.IsNullOrEmpty(mazeId))
            {
                Debug.LogError("[ComputeGridData] Maze ID is null or empty");
                return false;
            }

            Debug.Log($"[ComputeGridData] Saving grid with metadata: {mazeId}...");

            try
            {
                using (var ms = new MemoryStream())
                {
                    // Write header
                    WriteUInt(ms, MAGIC_NUMBER);
                    WriteUShort(ms, FILE_VERSION);
                    WriteUShort(ms, (ushort)Mathf.Sqrt(gridData.Length));
                    WriteInt(ms, seed);

                    // Write grid data length and data
                    WriteInt(ms, gridData.Length);
                    byte[] encryptedGrid = EncryptData(gridData, seed);
                    ms.Write(encryptedGrid, 0, encryptedGrid.Length);

                    // Write metadata length and data
                    if (metadata != null && metadata.Length > 0)
                    {
                        WriteInt(ms, metadata.Length);
                        byte[] encryptedMetadata = EncryptData(metadata, seed ^ 0xDEADBEEF); // Different XOR key
                        ms.Write(encryptedMetadata, 0, encryptedMetadata.Length);
                    }
                    else
                    {
                        WriteInt(ms, 0);
                    }

                    // Calculate and write checksum
                    uint checksum = CalculateChecksum(ms.ToArray());
                    WriteUInt(ms, checksum);

                    // Write to file
                    string filePath = GetFilePath(mazeId);
                    EnsureFolderExists();
                    File.WriteAllBytes(filePath, ms.ToArray());

                    // Update cache
                    _cache[mazeId] = gridData;

                    Debug.Log($"[ComputeGridData] Saved with metadata: {filePath} ({ms.Length} bytes)");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ComputeGridData] Save with metadata failed: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Load Operations

        /// <summary>
        /// Load compute grid from binary file.
        /// </summary>
        /// <param name="mazeId">Unique maze identifier</param>
        /// <param name="seed">Seed for decryption</param>
        /// <returns>Grid byte array, or null if not found</returns>
        public static byte[] LoadGrid(string mazeId, int seed)
        {
            // Check cache first
            if (_cache.TryGetValue(mazeId, out byte[] cached))
            {
                Debug.Log($"[ComputeGridData] Cache hit: {mazeId}");
                return cached;
            }

            string filePath = GetFilePath(mazeId);

            if (!File.Exists(filePath))
            {
                Debug.Log($"[ComputeGridData] File not found: {filePath}");
                return null;
            }

            Debug.Log($"[ComputeGridData] Loading grid: {filePath}...");

            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);

                using (var ms = new MemoryStream(fileData))
                {
                    // Read and verify header
                    uint magic = ReadUInt(ms);
                    if (magic != MAGIC_NUMBER)
                    {
                        Debug.LogError($"[ComputeGridData] Invalid magic number: {magic:X8}");
                        return null;
                    }

                    ushort version = ReadUShort(ms);
                    if (version != FILE_VERSION)
                    {
                        Debug.LogError($"[ComputeGridData] Unsupported version: {version}");
                        return null;
                    }

                    ushort gridSize = ReadUShort(ms);
                    int fileSeed = ReadInt(ms);

                    // Read grid data length
                    int dataLength = ReadInt(ms);
                    if (dataLength <= 0 || dataLength > fileData.Length)
                    {
                        Debug.LogError($"[ComputeGridData] Invalid data length: {dataLength}");
                        return null;
                    }

                    // Read and decrypt grid data
                    byte[] encryptedData = new byte[dataLength];
                    ms.Read(encryptedData, 0, dataLength);
                    byte[] decryptedData = DecryptData(encryptedData, seed);

                    // Read metadata length (if present)
                    int metadataLength = ReadInt(ms);
                    if (metadataLength > 0)
                    {
                        byte[] encryptedMetadata = new byte[metadataLength];
                        ms.Read(encryptedMetadata, 0, metadataLength);
                        byte[] decryptedMetadata = DecryptData(encryptedMetadata, seed ^ 0xDEADBEEF);
                        Debug.Log($"[ComputeGridData] Metadata loaded: {metadataLength} bytes");
                    }

                    // Verify checksum
                    uint storedChecksum = ReadUInt(ms);
                    uint calculatedChecksum = CalculateChecksum(encryptedData);
                    if (storedChecksum != calculatedChecksum)
                    {
                        Debug.LogError($"[ComputeGridData] Checksum mismatch: {storedChecksum:X8} vs {calculatedChecksum:X8}");
                        return null;
                    }

                    // Cache and return
                    _cache[mazeId] = decryptedData;

                    Debug.Log($"[ComputeGridData] Loaded: {decryptedData.Length} bytes");
                    return decryptedData;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ComputeGridData] Load failed: {e.Message}");
                return null;
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Delete compute grid file.
        /// </summary>
        public static bool DeleteGrid(string mazeId)
        {
            string filePath = GetFilePath(mazeId);

            if (!File.Exists(filePath))
            {
                Debug.Log($"[ComputeGridData] File not found: {filePath}");
                return false;
            }

            try
            {
                File.Delete(filePath);
                _cache.Remove(mazeId);
                Debug.Log($"[ComputeGridData] Deleted: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ComputeGridData] Delete failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clear all cached data.
        /// </summary>
        public static void ClearCache()
        {
            _cache.Clear();
            Debug.Log("[ComputeGridData] Cache cleared");
        }

        #endregion

        #region File Utilities

        /// <summary>
        /// Get file path for maze ID.
        /// </summary>
        private static string GetFilePath(string mazeId)
        {
            string folder = Path.Combine(Application.streamingAssetsPath, FOLDER_NAME);
            return Path.Combine(folder, $"{mazeId}.bin");
        }

        /// <summary>
        /// Ensure folder exists.
        /// </summary>
        private static void EnsureFolderExists()
        {
            string folder = Path.Combine(Application.streamingAssetsPath, FOLDER_NAME);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Debug.Log($"[ComputeGridData] Created folder: {folder}");
            }
        }

        /// <summary>
        /// Check if grid file exists.
        /// </summary>
        public static bool GridExists(string mazeId)
        {
            if (_cache.ContainsKey(mazeId)) return true;

            string filePath = GetFilePath(mazeId);
            return File.Exists(filePath);
        }

        #endregion

        #region Binary I/O Helpers

        private static void WriteUInt(Stream stream, uint value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void WriteUShort(Stream stream, ushort value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void WriteInt(Stream stream, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static uint ReadUInt(Stream stream)
        {
            int b1 = stream.ReadByte();
            int b2 = stream.ReadByte();
            int b3 = stream.ReadByte();
            int b4 = stream.ReadByte();
            return (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        private static ushort ReadUShort(Stream stream)
        {
            int b1 = stream.ReadByte();
            int b2 = stream.ReadByte();
            return (ushort)((b1 << 8) | b2);
        }

        private static int ReadInt(Stream stream)
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            return BitConverter.ToInt32(bytes, 0);
        }

        #endregion

        #region Encryption

        /// <summary>
        /// Simple XOR encryption with seed-derived key.
        /// </summary>
        private static byte[] EncryptData(byte[] data, int seed)
        {
            byte[] result = new byte[data.Length];
            uint key = (uint)seed;

            for (int i = 0; i < data.Length; i++)
            {
                // Generate pseudo-random key byte
                key = key * 1103515245 + 12345;
                result[i] = (byte)(data[i] ^ (byte)(key >> 16));
            }

            return result;
        }

        /// <summary>
        /// Decrypt data (XOR is symmetric).
        /// </summary>
        private static byte[] DecryptData(byte[] data, int seed)
        {
            return EncryptData(data, seed); // XOR is symmetric
        }

        #endregion

        #region Checksum

        /// <summary>
        /// Calculate CRC32 checksum.
        /// </summary>
        private static uint CalculateChecksum(byte[] data)
        {
            uint crc = 0xFFFFFFFF;

            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xEDB88320 : 0);
                }
            }

            return ~crc;
        }

        #endregion
    }
}
