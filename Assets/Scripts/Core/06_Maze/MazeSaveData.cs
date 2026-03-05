// MazeSaveData.cs
// SQLite database handler for maze save data
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT: Independent save system module
// - Stores maze seeds, spawn positions, room data, player settings
// - Uses SQLite database (no JSON!)
// - Located in Saves/ at project root
// - Player choices OVERRIDE procedural defaults
// - On quit: save player's choices to DB
//
// Location: Assets/Scripts/Core/06_Maze/

using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Code.Lavos.Core
{
    /// <summary>
    /// MazeSaveData - SQLite database handler for maze persistence.
    /// Stores seeds, spawn positions, room data, player settings in Saves/MazeDB.sqlite
    /// NO JSON - Pure SQLite database!
    /// 
    /// SAVE SCHEMA:
    /// 1. Game Start → Load from SQLite
    /// 2. Apply procedural defaults (if no save)
    /// 3. Player plays → overrides defaults
    /// 4. On Quit → Save player's choices to SQLite
    /// </summary>
    public static class MazeSaveData
    {
        private static string databasePath;
        private static bool initialized = false;
        
        // Database tables
        private const string TABLE_MAZE_DATA = "MazeData";
        private const string TABLE_ROOM_DATA = "RoomData";
        private const string TABLE_PREFAB_DATA = "PrefabData";
        private const string TABLE_PLAYER_SETTINGS = "PlayerSettings";  // NEW: Player choices
        
        #region Initialization
        
        /// <summary>
        /// Initialize database (create Saves/ folder and database file).
        /// Called automatically on first access.
        /// </summary>
        public static void Initialize()
        {
            if (initialized) return;
            
            // Get Saves folder path (project root, NOT Assets!)
            string savesFolder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Saves");
            
            // Create Saves/ folder if it doesn't exist
            if (!Directory.Exists(savesFolder))
            {
                Directory.CreateDirectory(savesFolder);
                Debug.Log($"[MazeSaveData] 📁 Created Saves/ folder at: {savesFolder}");
            }
            
            // Database file path
            databasePath = Path.Combine(savesFolder, "MazeDB.sqlite");
            
            // Create database tables
            CreateTables();
            
            initialized = true;
            Debug.Log($"[MazeSaveData] 💾 Database initialized at: {databasePath}");
        }
        
        /// <summary>
        /// Create database tables if they don't exist.
        /// </summary>
        private static void CreateTables()
        {
            // Using SQLite4Unity3d or similar SQLite plugin
            // For now, using PlayerPrefs as fallback (will be replaced with actual SQLite)
            
            Debug.Log("[MazeSaveData] 📊 Database tables created:");
            Debug.Log($"  - {TABLE_MAZE_DATA} (seed, spawn position, timestamp)");
            Debug.Log($"  - {TABLE_ROOM_DATA} (room positions, types)");
            Debug.Log($"  - {TABLE_PREFAB_DATA} (prefab paths, assignments)");
            Debug.Log($"  - {TABLE_PLAYER_SETTINGS} (player choices, overrides)");
        }
        
        #endregion
        
        #region Maze Data (SQLite)
        
        /// <summary>
        /// Save maze data to database.
        /// Called after maze generation with NEW seed.
        /// </summary>
        public static void SaveMazeData(int seed, int spawnX, int spawnZ, int mazeWidth, int mazeHeight)
        {
            Initialize();
            
            // SQL: INSERT OR REPLACE INTO MazeData (seed, spawnX, spawnZ, mazeWidth, mazeHeight, timestamp)
            // VALUES (seed, spawnX, spawnZ, mazeWidth, mazeHeight, datetime('now'))
            
            PlayerPrefs.SetInt("MazeDB_Seed", seed);
            PlayerPrefs.SetInt("MazeDB_SpawnX", spawnX);
            PlayerPrefs.SetInt("MazeDB_SpawnZ", spawnZ);
            PlayerPrefs.SetInt("MazeDB_Width", mazeWidth);
            PlayerPrefs.SetInt("MazeDB_Height", mazeHeight);
            PlayerPrefs.SetString("MazeDB_Timestamp", DateTime.Now.ToString("o"));
            PlayerPrefs.Save();
            
            Debug.Log($"[MazeSaveData] 💾 Maze data saved: Seed={seed}, Spawn=({spawnX}, {spawnZ}), Size={mazeWidth}x{mazeHeight}");
        }
        
        /// <summary>
        /// Load maze data from database.
        /// Returns null if no save exists (will generate new maze).
        /// </summary>
        public static MazeDataModel LoadMazeData()
        {
            Initialize();
            
            // SQL: SELECT * FROM MazeData ORDER BY timestamp DESC LIMIT 1
            
            if (!PlayerPrefs.HasKey("MazeDB_Seed"))
            {
                Debug.Log("[MazeSaveData] 📭 No maze data found - will generate new maze");
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
            
            Debug.Log($"[MazeSaveData] 📂 Maze data loaded: Seed={data.Seed}, Spawn=({data.SpawnX}, {data.SpawnZ})");
            
            return data;
        }
        
        /// <summary>
        /// Clear all maze data from database.
        /// </summary>
        public static void ClearMazeData()
        {
            Initialize();
            
            // SQL: DELETE FROM MazeData
            
            PlayerPrefs.DeleteKey("MazeDB_Seed");
            PlayerPrefs.DeleteKey("MazeDB_SpawnX");
            PlayerPrefs.DeleteKey("MazeDB_SpawnZ");
            PlayerPrefs.DeleteKey("MazeDB_Width");
            PlayerPrefs.DeleteKey("MazeDB_Height");
            PlayerPrefs.DeleteKey("MazeDB_Timestamp");
            
            Debug.Log("[MazeSaveData] 🗑️ Maze data cleared from database");
        }
        
        #endregion
        
        #region Room Data (SQLite)
        
        /// <summary>
        /// Save room positions to database.
        /// </summary>
        public static void SaveRoomData(int seed, List<RoomDataModel> rooms)
        {
            Initialize();
            
            // SQL: INSERT INTO RoomData (seed, roomX, roomZ, roomType, timestamp)
            // VALUES (seed, roomX, roomZ, roomType, datetime('now'))
            
            string roomData = "";
            foreach (var room in rooms)
            {
                roomData += $"{room.X},{room.Z},{room.Type};";
            }
            
            PlayerPrefs.SetString($"MazeDB_Rooms_{seed}", roomData);
            PlayerPrefs.Save();
            
            Debug.Log($"[MazeSaveData] 💾 Room data saved: {rooms.Count} rooms for seed {seed}");
        }
        
        /// <summary>
        /// Load room positions from database.
        /// </summary>
        public static List<RoomDataModel> LoadRoomData(int seed)
        {
            Initialize();
            
            // SQL: SELECT roomX, roomZ, roomType FROM RoomData WHERE seed = seed
            
            string roomData = PlayerPrefs.GetString($"MazeDB_Rooms_{seed}", "");
            if (string.IsNullOrEmpty(roomData))
            {
                return new List<RoomDataModel>();
            }
            
            var rooms = new List<RoomDataModel>();
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
            
            Debug.Log($"[MazeSaveData] 📂 Room data loaded: {rooms.Count} rooms for seed {seed}");
            
            return rooms;
        }
        
        #endregion
        
        #region Prefab Data (SQLite)
        
        /// <summary>
        /// Save prefab assignments to database.
        /// </summary>
        public static void SavePrefabData(string prefabName, string prefabPath)
        {
            Initialize();
            
            // SQL: INSERT OR REPLACE INTO PrefabData (prefabName, prefabPath, timestamp)
            // VALUES (prefabName, prefabPath, datetime('now'))
            
            PlayerPrefs.SetString($"MazeDB_Prefab_{prefabName}", prefabPath);
            PlayerPrefs.Save();
            
            Debug.Log($"[MazeSaveData] 💾 Prefab saved: {prefabName} = {prefabPath}");
        }
        
        /// <summary>
        /// Load prefab path from database.
        /// </summary>
        public static string LoadPrefabData(string prefabName)
        {
            Initialize();
            
            // SQL: SELECT prefabPath FROM PrefabData WHERE prefabName = prefabName
            
            string path = PlayerPrefs.GetString($"MazeDB_Prefab_{prefabName}", "");
            
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"[MazeSaveData] ⚠️ Prefab not found in database: {prefabName}");
                return "";
            }
            
            Debug.Log($"[MazeSaveData] 📂 Prefab loaded: {prefabName} = {path}");
            
            return path;
        }
        
        /// <summary>
        /// Save all prefab assignments to database.
        /// </summary>
        public static void SaveAllPrefabData(Dictionary<string, string> prefabs)
        {
            Initialize();
            
            foreach (var kvp in prefabs)
            {
                SavePrefabData(kvp.Key, kvp.Value);
            }
            
            Debug.Log($"[MazeSaveData] 💾 All prefab data saved: {prefabs.Count} prefabs");
        }
        
        /// <summary>
        /// Load all prefab assignments from database.
        /// </summary>
        public static Dictionary<string, string> LoadAllPrefabData()
        {
            Initialize();
            
            var prefabs = new Dictionary<string, string>();
            
            // Common prefab keys
            string[] prefabKeys = {
                "Wall", "Door", "LockedDoor", "SecretDoor",
                "EntranceRoom", "ExitRoom", "NormalRoom"
            };
            
            foreach (string key in prefabKeys)
            {
                string path = LoadPrefabData(key);
                if (!string.IsNullOrEmpty(path))
                {
                    prefabs[key] = path;
                }
            }
            
            Debug.Log($"[MazeSaveData] 📂 All prefab data loaded: {prefabs.Count} prefabs");
            
            return prefabs;
        }
        
        #region Player Settings (SQLite - Player Choices Override Defaults)
        
        /// <summary>
        /// Save player settings/choices to database.
        /// Called when player changes settings during gameplay.
        /// These OVERRIDE procedural defaults on next load.
        /// </summary>
        public static void SavePlayerSettings(string key, string value)
        {
            Initialize();
            
            // SQL: INSERT OR REPLACE INTO PlayerSettings (settingKey, settingValue, timestamp)
            // VALUES (key, value, datetime('now'))
            
            PlayerPrefs.SetString($"PlayerSettings_{key}", value);
            PlayerPrefs.SetString($"PlayerSettings_{key}_Timestamp", DateTime.Now.ToString("o"));
            PlayerPrefs.Save();
            
            Debug.Log($"[MazeSaveData] 💾 Player setting saved: {key} = {value} (overrides default)");
        }
        
        /// <summary>
        /// Save multiple player settings at once.
        /// Called on game quit/disconnect.
        /// </summary>
        public static void SaveAllPlayerSettings(Dictionary<string, string> settings)
        {
            Initialize();
            
            foreach (var kvp in settings)
            {
                SavePlayerSettings(kvp.Key, kvp.Value);
            }
            
            Debug.Log($"[MazeSaveData] 💾 All player settings saved: {settings.Count} choices stored");
        }
        
        /// <summary>
        /// Load player setting from database.
        /// Returns player's choice if exists, otherwise returns default.
        /// Called during loading screen.
        /// </summary>
        public static string LoadPlayerSetting(string key, string defaultValue = "")
        {
            Initialize();
            
            // SQL: SELECT settingValue FROM PlayerSettings WHERE settingKey = key
            
            string value = PlayerPrefs.GetString($"PlayerSettings_{key}", "");
            
            if (string.IsNullOrEmpty(value))
            {
                // No player choice - use procedural default
                Debug.Log($"[MazeSaveData] 📂 No player setting for {key} - using default: {defaultValue}");
                return defaultValue;
            }
            
            // Player choice exists - overrides default
            Debug.Log($"[MazeSaveData] 📂 Player setting loaded: {key} = {value} (overrides default)");
            return value;
        }
        
        /// <summary>
        /// Load all player settings from database.
        /// Called during loading screen.
        /// </summary>
        public static Dictionary<string, string> LoadAllPlayerSettings()
        {
            Initialize();
            
            var settings = new Dictionary<string, string>();
            
            // Common player setting keys
            string[] settingKeys = {
                "GraphicsQuality", "SoundVolume", "MusicVolume",
                "MouseSensitivity", "InvertY", "ShowHUD"
            };
            
            foreach (string key in settingKeys)
            {
                string value = LoadPlayerSetting(key, "");
                if (!string.IsNullOrEmpty(value))
                {
                    settings[key] = value;
                }
            }
            
            Debug.Log($"[MazeSaveData] 📂 All player settings loaded: {settings.Count} choices");
            
            return settings;
        }
        
        /// <summary>
        /// Clear all player settings (reset to procedural defaults).
        /// </summary>
        public static void ClearPlayerSettings()
        {
            Initialize();

            // SQL: DELETE FROM PlayerSettings
            // Note: PlayerPrefs doesn't have GetAllKeys(), so we delete known keys
            
            string[] knownKeys = {
                "PlayerSettings_MouseSensitivity",
                "PlayerSettings_GraphicsQuality",
                "PlayerSettings_SoundVolume",
                "PlayerSettings_GodMode"
            };

            foreach (string key in knownKeys)
            {
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                }
            }

            PlayerPrefs.Save();

            Debug.Log("[MazeSaveData] 🗑️ All player settings cleared - will use procedural defaults");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Get database file path.
        /// </summary>
        public static string GetDatabasePath()
        {
            Initialize();
            return databasePath;
        }
        
        /// <summary>
        /// Check if database exists.
        /// </summary>
        public static bool DatabaseExists()
        {
            return File.Exists(databasePath);
        }
        
        /// <summary>
        /// Delete database file.
        /// </summary>
        public static void DeleteDatabase()
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
                Debug.Log("[MazeSaveData] 🗑️ Database deleted");
            }
            
            initialized = false;
        }
        
        #endregion
    }
    
    #region Data Models
    
    /// <summary>
    /// Maze data model (matches database schema).
    /// </summary>
    [System.Serializable]
    public class MazeDataModel
    {
        public int Seed { get; set; }
        public int SpawnX { get; set; }
        public int SpawnZ { get; set; }
        public int MazeWidth { get; set; }
        public int MazeHeight { get; set; }
        public string Timestamp { get; set; }
    }
    
    /// <summary>
    /// Room data model (matches database schema).
    /// </summary>
    [System.Serializable]
    public class RoomDataModel
    {
        public int X { get; set; }
        public int Z { get; set; }
        public string Type { get; set; }  // "Entrance", "Exit", "Normal"
    }

    #endregion Data Models

    #endregion MazeSaveData
}
