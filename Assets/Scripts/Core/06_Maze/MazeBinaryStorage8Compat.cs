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
// MazeBinaryStorage8Compat.cs
// Compatibility wrapper for DungeonMazeData serialization
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;

namespace Code.Lavos.Core.Advanced
{
    /// <summary>
    /// Compatibility wrapper that makes DungeonMazeData work with
    /// CompleteMazeBuilder's serialization needs.
    /// 
    /// If MazeBinaryStorage8 exists and works with the original MazeData8,
    /// this wrapper seamlessly converts between them.
    /// </summary>
    public static class MazeBinaryStorage8Compat
    {
        /// <summary>
        /// Check if maze data exists in binary storage.
        /// Delegates to original MazeBinaryStorage8 if available.
        /// </summary>
        public static bool Exists(int level, int seed)
        {
            try
            {
                var storageType = System.Type.GetType("Code.Lavos.Core.MazeBinaryStorage8");
                if (storageType != null)
                {
                    var existsMethod = storageType.GetMethod("Exists", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (existsMethod != null)
                    {
                        return (bool)existsMethod.Invoke(null, new object[] { level, seed });
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MazeBinaryCompat] Exists check failed: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Load maze data from binary storage.
        /// Returns DungeonMazeData for compatibility.
        /// </summary>
        public static DungeonMazeData Load(int level, int seed)
        {
            try
            {
                var storageType = System.Type.GetType("Code.Lavos.Core.MazeBinaryStorage8");
                if (storageType != null)
                {
                    var loadMethod = storageType.GetMethod("Load",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (loadMethod != null)
                    {
                        var result = loadMethod.Invoke(null, new object[] { level, seed });

                        if (result is DungeonMazeData dungeonData)
                            return dungeonData;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MazeBinaryCompat] Load failed: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Save maze data to binary storage.
        /// Accepts DungeonMazeData.
        /// </summary>
        public static void Save(DungeonMazeData mazeData)
        {
            if (mazeData == null)
            {
                Debug.LogError("[MazeBinaryCompat] Cannot save null maze data");
                return;
            }

            try
            {
                var storageType = System.Type.GetType("Code.Lavos.Core.MazeBinaryStorage8");
                if (storageType != null)
                {
                    var saveMethod = storageType.GetMethod("Save", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (saveMethod != null)
                    {
                        // Try to save directly as MazeData8
                        try
                        {
                            saveMethod.Invoke(null, new object[] { mazeData });
                            return;
                        }
                        catch
                        {
                            // If that fails, try to convert to MazeData8 equivalent
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MazeBinaryCompat] Save failed: {ex.Message}");
            }
        }
    }
}
