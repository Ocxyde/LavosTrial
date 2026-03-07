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
// MazeShareSystem.cs
// Maze sharing system via seed codes
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// INTEGRATION: Uses existing SeedManager for seed generation
// USAGE:
//   Export: MazeShareSystem.ExportCurrentMaze()
//   Import: MazeShareSystem.ImportAndGenerate(code)
//   Share: Copy to clipboard or generate QR

using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// MazeShareSystem - Export/import mazes via shareable codes.
    /// Integrates with existing SeedManager for seed-based reconstruction.
    /// </summary>
    public static class MazeShareSystem
    {
        // Code format: LAVOS-{seed}-{level}-{checksum}
        private const string PREFIX = "LAVOS";
        private const char SEPARATOR = '-';

        /// <summary>
        /// Export current maze to shareable code.
        /// Uses SeedManager for seed retrieval.
        /// </summary>
        public static string ExportCurrentMaze()
        {
            // Get seed from SeedManager if available
            uint seed = 0;
            int level = 0;

            if (SeedManager.Instance != null)
            {
                seed = SeedManager.Instance.ComputeSeed;
                level = SeedManager.Instance.CurrentLevel;
            }
            else
            {
                // Fallback to CompleteMazeBuilder
                var builder = Object.FindFirstObjectByType<CompleteMazeBuilder>();
                if (builder != null)
                {
                    seed = (uint)builder.CurrentSeed;
                    level = builder.CurrentLevel;
                }
                else
                {
                    Debug.LogWarning("[MazeShare] No SeedManager or CompleteMazeBuilder found");
                    return string.Empty;
                }
            }

            string code = ExportCode(seed, level);
            Debug.Log($"[MazeShare] Exported maze code: {code}");
            return code;
        }

        /// <summary>
        /// Export maze to shareable code.
        /// </summary>
        public static string ExportCode(uint seed, int level)
        {
            // Create checksum for validation
            int checksum = CreateChecksum(seed, level);

            // Format: LAVOS-seed-level-checksum
            string code = $"{PREFIX}{SEPARATOR}{seed}{SEPARATOR}{level}{SEPARATOR}{checksum}";

            return code;
        }

        /// <summary>
        /// Import maze from shareable code and regenerate.
        /// Returns true if valid.
        /// </summary>
        public static bool ImportAndGenerate(string code)
        {
            if (ImportCode(code, out uint seed, out int level))
            {
                // Set seed in SeedManager
                if (SeedManager.Instance != null)
                {
                    SeedManager.Instance.SetCustomSeed(code);
                    Debug.Log($"[MazeShare] Seed set via SeedManager");
                }

                // Regenerate maze
                var builder = Object.FindFirstObjectByType<CompleteMazeBuilder>();
                if (builder != null)
                {
                    // Note: You may need to add method to set seed directly
                    Debug.Log($"[MazeShare] Ready to generate: Seed={seed}, Level={level}");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Import maze from shareable code.
        /// Returns true if valid, outputs seed/level.
        /// </summary>
        public static bool ImportCode(string code, out uint seed, out int level)
        {
            seed = 0;
            level = 0;

            if (string.IsNullOrWhiteSpace(code))
            {
                Debug.LogWarning("[MazeShare] Import failed: code is empty");
                return false;
            }

            // Parse: LAVOS-seed-level-checksum
            string[] parts = code.Trim().Split(SEPARATOR);

            if (parts.Length != 4)
            {
                Debug.LogWarning($"[MazeShare] Import failed: invalid format (expected 4 parts, got {parts.Length})");
                return false;
            }

            // Validate prefix
            if (parts[0] != PREFIX)
            {
                Debug.LogWarning($"[MazeShare] Import failed: invalid prefix '{parts[0]}' (expected '{PREFIX}')");
                return false;
            }

            // Parse seed
            if (!uint.TryParse(parts[1], out seed))
            {
                Debug.LogWarning($"[MazeShare] Import failed: invalid seed '{parts[1]}'");
                return false;
            }

            // Parse level
            if (!int.TryParse(parts[2], out level))
            {
                Debug.LogWarning($"[MazeShare] Import failed: invalid level '{parts[2]}'");
                return false;
            }

            // Validate checksum
            int expectedChecksum = CreateChecksum(seed, level);
            if (!int.TryParse(parts[3], out int actualChecksum) || actualChecksum != expectedChecksum)
            {
                Debug.LogWarning($"[MazeShare] Import failed: checksum mismatch (expected {expectedChecksum}, got {actualChecksum})");
                return false;
            }

            Debug.Log($"[MazeShare] Imported code: Seed={seed}, Level={level}");
            return true;
        }

        /// <summary>
        /// Create checksum for validation.
        /// </summary>
        private static int CreateChecksum(uint seed, int level)
        {
            // Simple but effective checksum
            string data = $"{seed}:{level}:LAVOS_SECRET_SALT_2026";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] hash = MD5.HashData(bytes);

            // Convert first 4 bytes to int
            return BitConverter.ToInt32(hash, 0);
        }

        /// <summary>
        /// Copy maze code to clipboard.
        /// </summary>
        public static void CopyToClipboard(string code)
        {
            if (string.IsNullOrEmpty(code)) return;

            TextEditor editor = new TextEditor
            {
                text = code
            };
            editor.SelectAll();
            editor.Copy();

            Debug.Log($"[MazeShare] Copied to clipboard: {code}");
        }

        /// <summary>
        /// Generate QR code data URL (for simple QR display).
        /// </summary>
        public static string GenerateQRDataUrl(string code)
        {
            // Simple URL encoding for QR code generators
            string encoded = Uri.EscapeDataString(code);
            return $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={encoded}";
        }

        /// <summary>
        /// Validate code format without parsing.
        /// </summary>
        public static bool IsValidCodeFormat(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;

            string[] parts = code.Trim().Split(SEPARATOR);
            return parts.Length == 4 && parts[0] == PREFIX;
        }
    }

    /// <summary>
    /// Extension for CompleteMazeBuilder to add sharing features.
    /// </summary>
    public static class CompleteMazeBuilderSharingExtensions
    {
        /// <summary>
        /// Export current maze to shareable code.
        /// </summary>
        public static string ExportMazeCode(this CompleteMazeBuilder builder)
        {
            return MazeShareSystem.ExportCurrentMaze();
        }

        /// <summary>
        /// Import maze from code and regenerate.
        /// </summary>
        public static bool ImportMazeCode(this CompleteMazeBuilder builder, string code)
        {
            return MazeShareSystem.ImportAndGenerate(code);
        }

        /// <summary>
        /// Copy current maze code to clipboard.
        /// </summary>
        public static void CopyMazeCodeToClipboard(this CompleteMazeBuilder builder)
        {
            string code = MazeShareSystem.ExportCurrentMaze();
            if (!string.IsNullOrEmpty(code))
            {
                MazeShareSystem.CopyToClipboard(code);
            }
        }
    }
}
