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
// - AES-GCM authenticated encryption with seed-derived keys
//
// FILE FORMAT (v2):
// [Magic: 4 bytes][Version: 2 bytes][GridSize: 2 bytes][Seed: 4 bytes]
// [IV: 12 bytes][GridDataLen: 4 bytes][EncryptedGrid: N bytes][GridTag: 16 bytes]
// [MetadataLen: 4 bytes][EncryptedMeta: M bytes][MetaTag: 16 bytes]
//
// USAGE:
//   ComputeGridData.SaveGrid(mazeId, gridBytes, seed)
//   ComputeGridData.LoadGrid(mazeId, seed)
//   ComputeGridData.DeleteGrid(mazeId)
//
// Location: Assets/Scripts/Core/12_Compute/

using System;
using System.IO;
using System.Security.Cryptography;
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
        private const ushort FILE_VERSION = 2; // v2: AES-GCM with auth tags
        private const string FOLDER_NAME = "ComputeGrid";

        // AES-GCM constants
        private const int IV_SIZE = 12; // 96 bits
        private const int TAG_SIZE = 16; // 128 bits
        private const int KEY_SIZE = 32; // 256 bits

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

                    // Generate IV and derive key
                    byte[] iv = GenerateSecureIV();
                    byte[] key = DeriveKey(seed);

                    // Encrypt grid data with AES-GCM
                    byte[] encryptedData = EncryptAesGcm(gridData, key, iv);

                    // Write IV, data length, encrypted data, and auth tag
                    ms.Write(iv, 0, IV_SIZE);
                    WriteInt(ms, encryptedData.Length - TAG_SIZE); // Store ciphertext length (without tag)
                    ms.Write(encryptedData, 0, encryptedData.Length - TAG_SIZE);
                    ms.Write(encryptedData, encryptedData.Length - TAG_SIZE, TAG_SIZE);

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

                    // Generate IV and derive key
                    byte[] iv = GenerateSecureIV();
                    byte[] key = DeriveKey(seed);

                    // Encrypt grid data with AES-GCM
                    byte[] encryptedGrid = EncryptAesGcm(gridData, key, iv);

                    // Write IV, grid data length, encrypted data, and auth tag
                    ms.Write(iv, 0, IV_SIZE);
                    WriteInt(ms, encryptedGrid.Length - TAG_SIZE);
                    ms.Write(encryptedGrid, 0, encryptedGrid.Length - TAG_SIZE);
                    ms.Write(encryptedGrid, encryptedGrid.Length - TAG_SIZE, TAG_SIZE);

                    // Write metadata length and data
                    if (metadata != null && metadata.Length > 0)
                    {
                        // Derive separate key for metadata using different domain separator
                        byte[] metadataKey = DeriveKey(seed, 0x4D455441); // "META"
                        byte[] metadataIv = GenerateSecureIV();

                        byte[] encryptedMetadata = EncryptAesGcm(metadata, metadataKey, metadataIv);

                        // Write metadata IV, length, encrypted data, and auth tag
                        ms.Write(metadataIv, 0, IV_SIZE);
                        WriteInt(ms, encryptedMetadata.Length - TAG_SIZE);
                        ms.Write(encryptedMetadata, 0, encryptedMetadata.Length - TAG_SIZE);
                        ms.Write(encryptedMetadata, encryptedMetadata.Length - TAG_SIZE, TAG_SIZE);
                    }
                    else
                    {
                        WriteInt(ms, 0);
                    }

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

                    // Derive key from seed
                    byte[] key = DeriveKey(seed);

                    // Read IV
                    byte[] iv = new byte[IV_SIZE];
                    int bytesRead = ms.Read(iv, 0, IV_SIZE);
                    if (bytesRead != IV_SIZE)
                    {
                        Debug.LogError($"[ComputeGridData] Failed to read IV");
                        return null;
                    }

                    // Read grid data length
                    int dataLength = ReadInt(ms);
                    if (dataLength <= 0 || dataLength > fileData.Length)
                    {
                        Debug.LogError($"[ComputeGridData] Invalid data length: {dataLength}");
                        return null;
                    }

                    // Read encrypted grid data and auth tag
                    byte[] encryptedData = new byte[dataLength + TAG_SIZE];
                    ms.Read(encryptedData, 0, dataLength);
                    ms.Read(encryptedData, dataLength, TAG_SIZE);

                    // Decrypt and verify integrity
                    byte[] decryptedData = DecryptAesGcm(encryptedData, key, iv);
                    if (decryptedData == null)
                    {
                        Debug.LogError("[ComputeGridData] Grid data authentication failed - file may be tampered");
                        return null;
                    }

                    // Read metadata length (if present)
                    int metadataLength = ReadInt(ms);
                    if (metadataLength > 0)
                    {
                        // Read metadata IV
                        byte[] metadataIv = new byte[IV_SIZE];
                        ms.Read(metadataIv, 0, IV_SIZE);

                        // Read encrypted metadata and auth tag
                        byte[] encryptedMetadata = new byte[metadataLength + TAG_SIZE];
                        ms.Read(encryptedMetadata, 0, metadataLength);
                        ms.Read(encryptedMetadata, metadataLength, TAG_SIZE);

                        // Derive separate key for metadata
                        byte[] metadataKey = DeriveKey(seed, 0x4D455441); // "META"

                        // Decrypt and verify metadata integrity
                        byte[] decryptedMetadata = DecryptAesGcm(encryptedMetadata, metadataKey, metadataIv);
                        if (decryptedMetadata == null)
                        {
                            Debug.LogError("[ComputeGridData] Metadata authentication failed - file may be tampered");
                            return null;
                        }
                        Debug.Log($"[ComputeGridData] Metadata loaded and verified: {metadataLength} bytes");
                    }

                    // Cache and return
                    _cache[mazeId] = decryptedData;

                    Debug.Log($"[ComputeGridData] Loaded and verified: {decryptedData.Length} bytes");
                    return decryptedData;
                }
            }
            catch (CryptographicException e)
            {
                Debug.LogError($"[ComputeGridData] Decryption failed: {e.Message}");
                return null;
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
            stream.Write(bytes, 0, (int)bytes.Length);
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
        /// Derive a 256-bit AES key from a seed using SHA256.
        /// </summary>
        private static byte[] DeriveKey(int seed, int domainSeparator = 0)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] seedBytes = BitConverter.GetBytes(seed);
                byte[] domainBytes = BitConverter.GetBytes(domainSeparator);

                // Combine seed and domain separator for key derivation
                byte[] input = new byte[seedBytes.Length + domainBytes.Length];
                Buffer.BlockCopy(seedBytes, 0, input, 0, seedBytes.Length);
                Buffer.BlockCopy(domainBytes, 0, input, seedBytes.Length, domainBytes.Length);

                return sha256.ComputeHash(input);
            }
        }

        /// <summary>
        /// Generate a cryptographically secure 12-byte IV.
        /// </summary>
        private static byte[] GenerateSecureIV()
        {
            byte[] iv = new byte[IV_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }
            return iv;
        }

        /// <summary>
        /// Encrypt data using AES-256-GCM with authenticated encryption.
        /// Returns ciphertext + 16-byte auth tag.
        /// </summary>
        private static byte[] EncryptAesGcm(byte[] plaintext, byte[] key, byte[] iv)
        {
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] tag = new byte[TAG_SIZE];

            using (var aesGcm = new AesGcm(key, TAG_SIZE))
            {
                aesGcm.Encrypt(iv, plaintext, ciphertext, tag);
            }

            // Combine ciphertext and tag
            byte[] result = new byte[ciphertext.Length + tag.Length];
            Buffer.BlockCopy(ciphertext, 0, result, 0, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, ciphertext.Length, tag.Length);
            return result;
        }

        /// <summary>
        /// Decrypt data using AES-256-GCM with authentication.
        /// Returns null if authentication fails (tampered data).
        /// </summary>
        private static byte[] DecryptAesGcm(byte[] ciphertextWithTag, byte[] key, byte[] iv)
        {
            if (ciphertextWithTag.Length < TAG_SIZE)
            {
                throw new CryptographicException("Ciphertext too short");
            }

            int ciphertextLength = ciphertextWithTag.Length - TAG_SIZE;
            byte[] ciphertext = new byte[ciphertextLength];
            byte[] tag = new byte[TAG_SIZE];

            Buffer.BlockCopy(ciphertextWithTag, 0, ciphertext, 0, ciphertextLength);
            Buffer.BlockCopy(ciphertextWithTag, ciphertextLength, tag, 0, TAG_SIZE);

            byte[] plaintext = new byte[ciphertextLength];

            using (var aesGcm = new AesGcm(key, TAG_SIZE))
            {
                aesGcm.Decrypt(iv, ciphertext, tag, plaintext);
            }

            return plaintext;
        }

        #endregion
    }
}
