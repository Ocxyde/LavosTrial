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
// MazeSaveData.cs
// Maze data persistence layer (PlayerPrefs + Binary)
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT ARCHITECTURE:
// - Independent data persistence layer
// - Stores maze data to PlayerPrefs
// - Binary storage for grid data
// - No seed storage - procedural generation only
//
// USAGE:
//   MazeSaveData.SaveGridMaze(gridBytes, spawnX, spawnZ)
//   MazeSaveData.LoadGridMaze()
//   MazeSaveData.ClearGridMazeData()
//
// Location: Assets/Scripts/Core/06_Maze/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// MazeSaveData - Maze data persistence layer.
    /// Stores maze data to PlayerPrefs and binary files.
    /// No seed storage - each load generates a new procedural maze.
    /// </summary>
    public static class MazeSaveData
    {
        #region Initialization

        private static bool _initialized = false;

        private static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            Debug.Log("[MazeSaveData] Initialized");
        }

        #endregion

        #region Maze Data (PlayerPrefs)

        /// <summary>
        /// Save maze data to database.
        /// Called after maze generation.
        /// </summary>
        public static void SaveMazeData(int seed, int spawnX, int spawnZ, int mazeWidth, int mazeHeight)
        {
            Initialize();

            PlayerPrefs.SetInt("MazeDB_Seed", seed);
            PlayerPrefs.SetInt("MazeDB_SpawnX", spawnX);
            PlayerPrefs.SetInt("MazeDB_SpawnZ", spawnZ);
            PlayerPrefs.SetInt("MazeDB_Width", mazeWidth);
            PlayerPrefs.SetInt("MazeDB_Height", mazeHeight);
            PlayerPrefs.SetString("MazeDB_Timestamp", DateTime.Now.ToString("o"));
            PlayerPrefs.Save();

            Debug.Log($"[MazeSaveData]  Maze data saved: Seed={seed}, Spawn=({spawnX}, {spawnZ}), Size={mazeWidth}x{mazeHeight}");
        }

        /// <summary>
        /// Load maze data from database.
        /// Returns null if no save exists (will generate new maze).
        /// </summary>
        public static MazeDataModel LoadMazeData()
        {
            Initialize();

            if (!PlayerPrefs.HasKey("MazeDB_Seed"))
            {
                Debug.Log("[MazeSaveData]  No maze data found - will generate new maze");
                return null;
            }

            var data = new MazeDataModel
            {
                Seed = PlayerPrefs.GetInt("MazeDB_Seed", 0),
                SpawnX = PlayerPrefs.GetInt("MazeDB_SpawnX", 2),
                SpawnZ = PlayerPrefs.GetInt("MazeDB_SpawnZ", 2),
                MazeWidth = PlayerPrefs.GetInt("MazeDB_Width", 21),
                MazeHeight = PlayerPrefs.GetInt("MazeDB_Height", 21),
                Timestamp = PlayerPrefs.GetString("MazeDB_Timestamp", "")
            };

            Debug.Log($"[MazeSaveData]  Maze data loaded: Seed={data.Seed}, Spawn=({data.SpawnX}, {data.SpawnZ})");

            return data;
        }

        /// <summary>
        /// Clear all maze data from database.
        /// </summary>
        public static void ClearMazeData()
        {
            Initialize();

            PlayerPrefs.DeleteKey("MazeDB_Seed");
            PlayerPrefs.DeleteKey("MazeDB_SpawnX");
            PlayerPrefs.DeleteKey("MazeDB_SpawnZ");
            PlayerPrefs.DeleteKey("MazeDB_Width");
            PlayerPrefs.DeleteKey("MazeDB_Height");
            PlayerPrefs.DeleteKey("MazeDB_Timestamp");

            Debug.Log("[MazeSaveData]  Maze data cleared from database");
        }

        #endregion

        #region Room Data (PlayerPrefs)

        /// <summary>
        /// Save room positions to database.
        /// </summary>
        public static void SaveRoomData(int seed, List<RoomDataModel> rooms)
        {
            Initialize();

            string roomData = "";
            foreach (var room in rooms)
            {
                roomData += $"{room.X},{room.Z},{room.Type};";
            }

            PlayerPrefs.SetString($"MazeDB_Rooms_{seed}", roomData);
            PlayerPrefs.Save();

            Debug.Log($"[MazeSaveData]  Room data saved: {rooms.Count} rooms for seed {seed}");
        }

        /// <summary>
        /// Load room positions from database.
        /// </summary>
        public static List<RoomDataModel> LoadRoomData(int seed)
        {
            Initialize();

            if (!PlayerPrefs.HasKey($"MazeDB_Rooms_{seed}"))
            {
                Debug.Log($"[MazeSaveData]  No room data found for seed {seed}");
                return null;
            }

            string roomData = PlayerPrefs.GetString($"MazeDB_Rooms_{seed}");
            List<RoomDataModel> rooms = new List<RoomDataModel>();

            string[] roomEntries = roomData.Split(';');
            foreach (string entry in roomEntries)
            {
                if (string.IsNullOrEmpty(entry)) continue;

                string[] parts = entry.Split(',');
                if (parts.Length == 3)
                {
                    rooms.Add(new RoomDataModel
                    {
                        X = int.Parse(parts[0]),
                        Z = int.Parse(parts[1]),
                        Type = parts[2]
                    });
                }
            }

            Debug.Log($"[MazeSaveData]  Room data loaded: {rooms.Count} rooms for seed {seed}");
            return rooms;
        }

        #endregion

        #region Prefab Data (PlayerPrefs)

        /// <summary>
        /// Save prefab placements to database.
        /// </summary>
        public static void SavePrefabData(int seed, List<PrefabDataModel> prefabs)
        {
            Initialize();

            string prefabData = "";
            foreach (var prefab in prefabs)
            {
                prefabData += $"{prefab.X},{prefab.Z},{prefab.Rotation},{prefab.PrefabName};";
            }

            PlayerPrefs.SetString($"MazeDB_Prefabs_{seed}", prefabData);
            PlayerPrefs.Save();

            Debug.Log($"[MazeSaveData]  Prefab data saved: {prefabs.Count} prefabs for seed {seed}");
        }

        /// <summary>
        /// Load prefab placements from database.
        /// </summary>
        public static List<PrefabDataModel> LoadPrefabData(int seed)
        {
            Initialize();

            if (!PlayerPrefs.HasKey($"MazeDB_Prefabs_{seed}"))
            {
                Debug.Log($"[MazeSaveData]  No prefab data found for seed {seed}");
                return null;
            }

            string prefabData = PlayerPrefs.GetString($"MazeDB_Prefabs_{seed}");
            List<PrefabDataModel> prefabs = new List<PrefabDataModel>();

            string[] prefabEntries = prefabData.Split(';');
            foreach (string entry in prefabEntries)
            {
                if (string.IsNullOrEmpty(entry)) continue;

                string[] parts = entry.Split(',');
                if (parts.Length == 4)
                {
                    prefabs.Add(new PrefabDataModel
                    {
                        X = int.Parse(parts[0]),
                        Z = int.Parse(parts[1]),
                        Rotation = int.Parse(parts[2]),
                        PrefabName = parts[3]
                    });
                }
            }

            Debug.Log($"[MazeSaveData]  Prefab data loaded: {prefabs.Count} prefabs for seed {seed}");
            return prefabs;
        }

        #endregion

        #region Player Settings (PlayerPrefs)

        /// <summary>
        /// Save player setting override to database.
        /// </summary>
        public static void SavePlayerSetting(string key, string value)
        {
            Initialize();
            PlayerPrefs.SetString($"PlayerSetting_{key}", value);
            PlayerPrefs.Save();
            Debug.Log($"[MazeSaveData]  Player setting saved: {key} = {value}");
        }

        /// <summary>
        /// Load player setting override from database.
        /// </summary>
        public static string LoadPlayerSetting(string key, string defaultValue = "")
        {
            Initialize();

            if (PlayerPrefs.HasKey($"PlayerSetting_{key}"))
            {
                string value = PlayerPrefs.GetString($"PlayerSetting_{key}");
                Debug.Log($"[MazeSaveData]  Player setting loaded: {key} = {value}");
                return value;
            }

            Debug.Log($"[MazeSaveData]  Player setting not found: {key} (using default: {defaultValue})");
            return defaultValue;
        }

        #endregion

        #region Grid Maze Data (Binary Storage)

        /// <summary>
        /// Save grid maze data (binary format - 1 byte per cell).
        /// No seed storage - procedural generation only.
        /// </summary>
        public static void SaveGridMaze(byte[] gridData, int spawnX, int spawnZ)
        {
            Initialize();

            // Convert byte array to base64 for PlayerPrefs storage
            string base64 = System.Convert.ToBase64String(gridData);

            PlayerPrefs.SetString("GridDB_Data", base64);
            PlayerPrefs.SetInt("GridDB_SpawnX", spawnX);
            PlayerPrefs.SetInt("GridDB_SpawnZ", spawnZ);
            PlayerPrefs.Save();

            Debug.Log($"[MazeSaveData]  Grid maze saved: {gridData.Length} bytes, Spawn=({spawnX},{spawnZ})");
        }

        /// <summary>
        /// Load grid maze data (binary format).
        /// Returns null if no save exists.
        /// </summary>
        public static byte[] LoadGridMaze()
        {
            Initialize();

            if (!PlayerPrefs.HasKey("GridDB_Data"))
            {
                Debug.Log("[MazeSaveData]  No grid maze found");
                return null;
            }

            string base64 = PlayerPrefs.GetString("GridDB_Data");
            byte[] gridData = System.Convert.FromBase64String(base64);

            Debug.Log($"[MazeSaveData]  Loaded grid maze: {gridData.Length} bytes");
            return gridData;
        }

        /// <summary>
        /// Clear grid maze data.
        /// </summary>
        public static void ClearGridMazeData()
        {
            Initialize();
            PlayerPrefs.DeleteKey("GridDB_Data");
            PlayerPrefs.DeleteKey("GridDB_SpawnX");
            PlayerPrefs.DeleteKey("GridDB_SpawnZ");
            PlayerPrefs.Save();
            Debug.Log("[MazeSaveData]  Grid maze data cleared");
        }

        #endregion Grid Maze Data
    }

    #region Data Models

    /// <summary>
    /// Maze data model (matches database schema).
    /// </summary>
    [System.Serializable]
    public class MazeDataModel
    {
        public int Seed;
        public int SpawnX;
        public int SpawnZ;
        public int MazeWidth;
        public int MazeHeight;
        public string Timestamp;
    }

    /// <summary>
    /// Room data model (matches database schema).
    /// </summary>
    [System.Serializable]
    public class RoomDataModel
    {
        public int X;
        public int Z;
        public string Type;
    }

    /// <summary>
    /// Prefab data model (matches database schema).
    /// </summary>
    [System.Serializable]
    public class PrefabDataModel
    {
        public int X;
        public int Z;
        public int Rotation;
        public string PrefabName;
    }

    #endregion
}
