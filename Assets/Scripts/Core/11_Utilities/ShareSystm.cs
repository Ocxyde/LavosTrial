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
//
// ShareSystm.cs
// Maze sharing system via shareable codes - tactical extensible pattern
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   Export: ShareSystm.ExportMaze()
//   Import: ShareSystm.ImportMaze(code)
//   Share: ShareSystm.ShareMaze()
//   Custom: ShareSystm.RegisterExporter/Importer()
//
// LOCATION: Assets/Scripts/Core/11_Utilities/

using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Lavos.Core
{
    /// <summary>
    /// ShareSystm - Tactical maze sharing system with extensible pattern.
    /// Plug-in-out compliant: Finds components, never creates.
    /// Communicates via EventHandler for decoupled architecture.
    /// </summary>
    public static class ShareSystm
    {
        // Code format: LAVOS-{seed}-{level}-{checksum}
        private const string PREFIX = "LAVOS";
        private const char SEPARATOR = '-';

        // Default salt (can be overridden via GameConfig)
        private const string DEFAULT_SALT = "LAVOS_SECRET_SALT_2026";

        // Custom exporters/importers registry
        private static Func<uint, int, string> _customExporter;
        private static Func<string, bool> _customImporter;

        /// <summary>
        /// Export current maze to shareable code.
        /// Plug-in-out: Finds CompleteMazeBuilder8, never creates.
        /// </summary>
        /// <returns>Shareable maze code, or empty string if not found.</returns>
        public static string ExportMaze()
        {
            // Plug-in-out: Find CompleteMazeBuilder8, never create
            var builder = Object.FindFirstObjectByType<CompleteMazeBuilder8>();
            if (builder == null)
            {
                Debug.LogWarning("[ShareSystm] CompleteMazeBuilder8 not found in scene");
                return string.Empty;
            }

            uint seed = (uint)builder.CurrentSeed;
            int level = builder.CurrentLevel;

            string code = ExportCode(seed, level);
            Debug.Log($"[ShareSystm] Exported maze code: {code}");

            // Publish via EventHandler for subscribers
            EventHandler.Instance?.InvokeMazeCodeExported(code);

            return code;
        }

        /// <summary>
        /// Export maze to shareable code with explicit seed and level.
        /// Uses custom exporter if registered.
        /// </summary>
        /// <param name="seed">Maze seed (uint).</param>
        /// <param name="level">Maze level (int).</param>
        /// <returns>Shareable code in format: LAVOS-seed-level-checksum</returns>
        public static string ExportCode(uint seed, int level)
        {
            // Use custom exporter if registered
            if (_customExporter != null)
            {
                return _customExporter(seed, level);
            }

            int checksum = CreateChecksum(seed, level);
            string code = $"{PREFIX}{SEPARATOR}{seed}{SEPARATOR}{level}{SEPARATOR}{checksum}";
            return code;
        }

        /// <summary>
        /// Import maze from shareable code and regenerate.
        /// Plug-in-out: Finds CompleteMazeBuilder, never creates.
        /// Uses custom importer if registered.
        /// </summary>
        /// <param name="code">Shareable maze code.</param>
        /// <returns>True if valid and ready to generate, false otherwise.</returns>
        public static bool ImportMaze(string code)
        {
            // Use custom importer if registered
            if (_customImporter != null)
            {
                return _customImporter(code);
            }

            if (ImportCode(code, out uint seed, out int level))
            {
                // Publish via EventHandler for subscribers
                EventHandler.Instance?.InvokeMazeCodeImported(code, seed, level);

                // Plug-in-out: Find CompleteMazeBuilder8, never create
                var builder = Object.FindFirstObjectByType<CompleteMazeBuilder8>();
                if (builder != null)
                {
                    Debug.Log($"[ShareSystm] Ready to generate: Seed={seed}, Level={level}");
                    return true;
                }
                else
                {
                    Debug.LogWarning("[ShareSystm] CompleteMazeBuilder8 not found - cannot regenerate");
                }
            }

            return false;
        }

        /// <summary>
        /// Import maze from shareable code.
        /// Validates format and checksum.
        /// </summary>
        /// <param name="code">Shareable maze code.</param>
        /// <param name="seed">Output: parsed seed.</param>
        /// <param name="level">Output: parsed level.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool ImportCode(string code, out uint seed, out int level)
        {
            seed = 0;
            level = 0;

            if (string.IsNullOrWhiteSpace(code))
            {
                Debug.LogWarning("[ShareSystm] Import failed: code is empty");
                return false;
            }

            // Parse: LAVOS-seed-level-checksum
            string[] parts = code.Trim().Split(SEPARATOR);

            if (parts.Length != 4)
            {
                Debug.LogWarning($"[ShareSystm] Import failed: invalid format (expected 4 parts, got {parts.Length})");
                return false;
            }

            // Validate prefix
            if (parts[0] != PREFIX)
            {
                Debug.LogWarning($"[ShareSystm] Import failed: invalid prefix '{parts[0]}' (expected '{PREFIX}')");
                return false;
            }

            // Parse seed
            if (!uint.TryParse(parts[1], out seed))
            {
                Debug.LogWarning($"[ShareSystm] Import failed: invalid seed '{parts[1]}'");
                return false;
            }

            // Parse level
            if (!int.TryParse(parts[2], out level))
            {
                Debug.LogWarning($"[ShareSystm] Import failed: invalid level '{parts[2]}'");
                return false;
            }

            // Validate checksum
            int expectedChecksum = CreateChecksum(seed, level);
            if (!int.TryParse(parts[3], out int actualChecksum) || actualChecksum != expectedChecksum)
            {
                Debug.LogWarning($"[ShareSystm] Import failed: checksum mismatch (expected {expectedChecksum}, got {actualChecksum})");
                return false;
            }

            Debug.Log($"[ShareSystm] Imported code: Seed={seed}, Level={level}");
            return true;
        }

        /// <summary>
        /// Create checksum for validation.
        /// Uses MD5 hash with configurable salt.
        /// </summary>
        /// <param name="seed">Maze seed.</param>
        /// <param name="level">Maze level.</param>
        /// <returns>Checksum integer for validation.</returns>
        private static int CreateChecksum(uint seed, int level)
        {
            // Get salt from GameConfig if available, otherwise use default
            string salt = DEFAULT_SALT;

            // Try to get salt from GameConfig (plug-in-out: find, never create)
            var config = Object.FindFirstObjectByType<GameConfig>();
            if (config != null && !string.IsNullOrEmpty(config.shareSalt))
            {
                salt = config.shareSalt;
            }

            string data = $"{seed}:{level}:{salt}";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(bytes);
                return BitConverter.ToInt32(hash, 0);
            }
        }

        /// <summary>
        /// Copy maze code to clipboard.
        /// Runtime-compatible (no Editor dependencies).
        /// </summary>
        /// <param name="code">Maze code to copy.</param>
        public static void CopyToClipboard(string code)
        {
            if (string.IsNullOrEmpty(code)) return;

            // Runtime-compatible clipboard using GUIUtility
            // Note: Works in Unity runtime, not standalone builds
            try
            {
                GUIUtility.systemCopyBuffer = code;
                Debug.Log($"[ShareSystm] Copied to clipboard: {code}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ShareSystm] Clipboard copy failed: {ex.Message}");
                Debug.Log($"[ShareSystm] Manual copy: {code}");
            }
        }

        /// <summary>
        /// Generate QR code data URL for external QR generation.
        /// Uses qrserver.com API for simple QR code display.
        /// </summary>
        /// <param name="code">Maze code to encode.</param>
        /// <returns>QR code image URL.</returns>
        public static string GenerateQRDataUrl(string code)
        {
            string encoded = Uri.EscapeDataString(code);
            return $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={encoded}";
        }

        /// <summary>
        /// Validate code format without full parsing.
        /// Quick validation for UI input fields.
        /// </summary>
        /// <param name="code">Maze code to validate.</param>
        /// <returns>True if format is valid, false otherwise.</returns>
        public static bool IsValidCodeFormat(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;

            string[] parts = code.Trim().Split(SEPARATOR);
            return parts.Length == 4 && parts[0] == PREFIX;
        }

        /// <summary>
        /// Register custom exporter function.
        /// Extensible: Override default export behavior.
        /// </summary>
        /// <param name="exporter">Function that takes (seed, level) and returns code string.</param>
        public static void RegisterExporter(Func<uint, int, string> exporter)
        {
            _customExporter = exporter;
            Debug.Log("[ShareSystm] Custom exporter registered");
        }

        /// <summary>
        /// Register custom importer function.
        /// Extensible: Override default import behavior.
        /// </summary>
        /// <param name="importer">Function that takes code string and returns true if valid.</param>
        public static void RegisterImporter(Func<string, bool> importer)
        {
            _customImporter = importer;
            Debug.Log("[ShareSystm] Custom importer registered");
        }

        /// <summary>
        /// Clear custom exporter (revert to default).
        /// </summary>
        public static void ClearExporter()
        {
            _customExporter = null;
            Debug.Log("[ShareSystm] Custom exporter cleared");
        }

        /// <summary>
        /// Clear custom importer (revert to default).
        /// </summary>
        public static void ClearImporter()
        {
            _customImporter = null;
            Debug.Log("[ShareSystm] Custom importer cleared");
        }

        /// <summary>
        /// Check if custom exporter is registered.
        /// </summary>
        /// <returns>True if custom exporter exists.</returns>
        public static bool HasCustomExporter()
        {
            return _customExporter != null;
        }

        /// <summary>
        /// Check if custom importer is registered.
        /// </summary>
        /// <returns>True if custom importer exists.</returns>
        public static bool HasCustomImporter()
        {
            return _customImporter != null;
        }
    }

    /// <summary>
    /// Extension methods for CompleteMazeBuilder sharing features.
    /// Plug-in-out compliant extensions.
    /// </summary>
    public static class CompleteMazeBuilder8SharingExtensions
    {
        /// <summary>
        /// Export current maze to shareable code.
        /// </summary>
        /// <param name="builder">CompleteMazeBuilder8 instance.</param>
        /// <returns>Shareable maze code.</returns>
        public static string ExportMazeCode(this CompleteMazeBuilder8 builder)
        {
            return ShareSystm.ExportMaze();
        }

        /// <summary>
        /// Import maze from code and regenerate.
        /// </summary>
        /// <param name="builder">CompleteMazeBuilder8 instance.</param>
        /// <param name="code">Shareable maze code.</param>
        /// <returns>True if valid and regenerated, false otherwise.</returns>
        public static bool ImportMazeCode(this CompleteMazeBuilder8 builder, string code)
        {
            return ShareSystm.ImportMaze(code);
        }

        /// <summary>
        /// Copy current maze code to clipboard.
        /// </summary>
        /// <param name="builder">CompleteMazeBuilder8 instance.</param>
        public static void CopyMazeCodeToClipboard(this CompleteMazeBuilder8 builder)
        {
            string code = ShareSystm.ExportMaze();
            if (!string.IsNullOrEmpty(code))
            {
                ShareSystm.CopyToClipboard(code);
            }
        }
    }
}
