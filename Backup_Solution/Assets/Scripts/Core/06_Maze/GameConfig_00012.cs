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
// GameConfig.cs
// JSON configuration file for game defaults and modding support
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT: Independent config module
// - Stores default values (prefabs, materials, game balance)
// - JSON format (easy to edit/mod)
// - Located in Config/ at project root
// - Supports modding (god-slayer mode, damage scale, etc.)
//
// Location: Assets/Scripts/Core/06_Maze/

using UnityEngine;
using System;
using System.IO;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GameConfig - JSON configuration for game defaults and modding.
    /// Located in Config/GameConfig.json at project root.
    /// Easy to edit for modders (god-slayer mode, damage scale, etc.)
    /// </summary>
    [System.Serializable]
    public class GameConfig
    {
        #region Prefab Paths

        [Header(" Prefab Paths")]
        public string wallPrefab = "Prefabs/WallPrefab.prefab";
        public string doorPrefab = "Prefabs/DoorPrefab.prefab";
        public string lockedDoorPrefab = "Prefabs/LockedDoorPrefab.prefab";
        public string secretDoorPrefab = "Prefabs/SecretDoorPrefab.prefab";
        public string entranceRoomPrefab = "Prefabs/EntranceRoomPrefab.prefab";
        public string exitRoomPrefab = "Prefabs/ExitRoomPrefab.prefab";
        public string normalRoomPrefab = "Prefabs/NormalRoomPrefab.prefab";
        public string torchPrefab = "Prefabs/TorchHandlePrefab.prefab";

        #endregion

        #region Material/Texture Paths

        [Header(" Material/Texture Paths")]
        public string wallMaterial = "Materials/WallMaterial.mat";
        public string doorMaterial = "Materials/Door_PixelArt.mat";
        public string floorMaterial = "Materials/Floor/Stone_Floor.mat";
        public string groundTexture = "Textures/floor_texture.png";
        public string wallTexture = "Textures/wall_texture.png";
        public string ceilingTexture = "Textures/ceiling_texture.png";

        #endregion

        #region Maze Generation

        [Header("️ Maze Generation")]
        public int defaultMazeWidth = 21;
        public int defaultMazeHeight = 21;
        public float defaultCellSize = 6f;
        public float defaultWallHeight = 4f;
        public float defaultWallThickness = 0.5f;
        public float defaultCeilingHeight = 5f;
        
        [Header("️ Room & Corridor Settings")]
        public int defaultRoomSize = 5;        // 5x5 rooms
        public int defaultCorridorWidth = 2;   // 2 cells wide
        public int defaultGridSize = 21;       // Base grid size

        [Header(" Door Settings")]
        public float defaultDoorSpawnChance = 0.6f;
        public float defaultLockedDoorChance = 0.3f;
        public float defaultSecretDoorChance = 0.1f;
        
        [Header(" Door Dimensions")]
        public float defaultDoorWidth = 2.5f;        // Door width + frame
        public float defaultDoorHeight = 3f;        // Door height + frame
        public float defaultDoorDepth = 0.5f;       // Wall thickness
        public float defaultDoorHoleDepth = 0.5f;   // Hole depth in wall
        
        [Header(" Door Variants")]
        public bool enableTrappedDoors = true;
        public float defaultTrapChance = 0.2f;
        public bool enableLockedDoors = true;
        public bool enableSecretDoors = true;
        
        [Header(" Door Visual")]
        public bool randomizeWallTextures = true;
        public bool showDebugGizmos = false;

        [Header(" Player Settings")]
        public float defaultPlayerEyeHeight = 1.7f;    // FPS camera height (average adult eyes)
        public float defaultPlayerSpawnOffset = 0.5f;  // Random offset to prevent wall clipping
        public float defaultPlayerHeight = 2.0f;       // Player collider height

        [Header("️ Room Settings")]
        public bool generateRooms = true;
        public int minRooms = 3;
        public int maxRooms = 8;

        [Header("️ Difficulty Scaling")]
        [Tooltip("Base maze size (before difficulty bonus)")]
        public int baseMazeSize = 12;
        [Tooltip("Minimum maze size")]
        public int minMazeSize = 12;
        [Tooltip("Maximum maze size")]
        public int maxMazeSize = 51;
        [Tooltip("Maximum size bonus from difficulty (0-10)")]
        public int maxDifficultySizeBonus = 10;
        [Tooltip("Maximum room bonus from difficulty (0-5)")]
        public int maxDifficultyRoomBonus = 5;
        [Tooltip("Base rooms random range (1-4 = 1 to 3 rooms)")]
        public int baseRoomMin = 1;
        public int baseRoomMax = 4;
        [Tooltip("Spawn room size (3x3)")]
        public int spawnRoomSize = 3;
        [Tooltip("Margin from grid edge for spawn room")]
        public int spawnRoomMargin = 2;
        [Tooltip("Spawn point position in room (1,1 = center of 3x3)")]
        public int spawnPointInRoomX = 1;
        public int spawnPointInRoomY = 1;
        [Tooltip("Which wall is open (0=left, 1=right, 2=top, 3=bottom)")]
        public int spawnRoomOpenWall = 0;

        [Header("️ Generation Options")]
        public bool useRandomSeed = true;
        public string manualSeed = "MazeSeed2026";
        public bool spawnInsideRoom = true;

        #endregion

        #region Game Balance (Moddable!)

        [Header("️ Game Balance (Moddable!)")]
        [Tooltip("Damage multiplier (1.0 = normal, 10.0 = god-slayer!)")]
        public float damageScale = 1.0f;

        [Tooltip("Player health multiplier")]
        public float healthScale = 1.0f;

        [Tooltip("Enemy health multiplier")]
        public float enemyHealthScale = 1.0f;

        [Tooltip("Movement speed multiplier")]
        public float speedScale = 1.0f;

        [Tooltip("Stamina drain multiplier")]
        public float staminaDrainScale = 1.0f;

        [Tooltip("God mode enabled (invincible)")]
        public bool godMode = false;

        [Tooltip("One-hit kill mode (god-slayer!)")]
        public bool oneHitKill = false;

        [Tooltip("Infinite stamina")]
        public bool infiniteStamina = false;

        [Tooltip("No clip mode (walk through walls)")]
        public bool noClip = false;

        #endregion

        #region Graphics/Audio

        [Header(" Graphics/Audio")]
        public string graphicsQuality = "Medium";
        public float soundVolume = 0.8f;
        public float musicVolume = 0.6f;
        public float mouseSensitivity = 1.0f;
        public bool invertY = false;
        public bool showHUD = true;

        [Header(" Console Verbosity")]
        [Tooltip("Console output level: full, short, mute (default: short)")]
        public string consoleVerbosity = "short";

        #endregion
        
        #region Singleton
        
        private static GameConfig _instance;
        private static string configPath;
        private static bool initialized = false;
        
        /// <summary>
        /// Get config instance (singleton).
        /// Loads from JSON file on first access.
        /// </summary>
        public static GameConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Load/Save
        
        /// <summary>
        /// Load config from JSON file (Config/GameConfig.json).
        /// Creates default config if file doesn't exist.
        /// </summary>
        public static GameConfig Load()
        {
            if (initialized && _instance != null)
            {
                return _instance;
            }
            
            // Get config folder path (project root/Config/)
            string configFolder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Config");
            
            // Create Config/ folder if it doesn't exist
            if (!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
                Debug.Log($"[GameConfig]  Created Config/ folder at: {configFolder}");
            }
            
            // Config file path
            configPath = Path.Combine(configFolder, "GameConfig-default.json");
            
            // Load or create config
            if (File.Exists(configPath))
            {
                // Load existing config
                try
                {
                    string json = File.ReadAllText(configPath);
                    
                    // Strip comments (// lines) for modder-friendly JSON
                    json = StripJSONComments(json);
                    
                    _instance = JsonUtility.FromJson<GameConfig>(json);
                    Debug.Log($"[GameConfig]  Loaded config from: {configPath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[GameConfig]  Failed to load config: {e.Message}");
                    Debug.Log("[GameConfig]  Creating default config...");
                    _instance = CreateDefault();
                    Save();
                }
            }
            else
            {
                // Create default config
                Debug.Log("[GameConfig]  No config found - creating default");
                _instance = CreateDefault();
                Save();
            }
            
            initialized = true;
            return _instance;
        }
        
        /// <summary>
        /// Save config to JSON file.
        /// </summary>
        public static void Save()
        {
            if (_instance == null)
            {
                Debug.LogWarning("[GameConfig] ️ No config instance to save!");
                return;
            }
            
            try
            {
                // Pretty-print JSON
                string json = JsonUtility.ToJson(_instance, true);
                File.WriteAllText(configPath, json);
                Debug.Log($"[GameConfig]  Config saved to: {configPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameConfig]  Failed to save config: {e.Message}");
            }
        }
        
        /// <summary>
        /// Strip comment lines from JSON (lines starting with //).
        /// Makes JSON modder-friendly!
        /// </summary>
        private static string StripJSONComments(string json)
        {
            var lines = json.Split('\n');
            var result = new System.Text.StringBuilder();
            
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (!trimmed.StartsWith("//") && !string.IsNullOrWhiteSpace(trimmed))
                {
                    result.AppendLine(line);
                }
            }
            
            return result.ToString();
        }
        
        /// <summary>
        /// Create default config values.
        /// </summary>
        private static GameConfig CreateDefault()
        {
            return new GameConfig
            {
                // Prefabs
                wallPrefab = "Prefabs/WallPrefab.prefab",
                doorPrefab = "Prefabs/DoorPrefab.prefab",
                lockedDoorPrefab = "Prefabs/LockedDoorPrefab.prefab",
                secretDoorPrefab = "Prefabs/SecretDoorPrefab.prefab",
                entranceRoomPrefab = "Prefabs/EntranceRoomPrefab.prefab",
                exitRoomPrefab = "Prefabs/ExitRoomPrefab.prefab",
                normalRoomPrefab = "Prefabs/NormalRoomPrefab.prefab",

                // Materials/Textures
                wallMaterial = "Materials/WallMaterial.mat",
                doorMaterial = "Materials/Door_PixelArt.mat",
                floorMaterial = "Materials/Floor/Stone_Floor.mat",
                groundTexture = "Textures/floor_texture.png",
                wallTexture = "Textures/wall_texture.png",
                ceilingTexture = "Textures/ceiling_texture.png",

                // Maze Generation
                defaultMazeWidth = 21,
                defaultMazeHeight = 21,
                defaultCellSize = 6f,
                defaultWallHeight = 4f,
                defaultWallThickness = 0.5f,
                defaultCeilingHeight = 5f,
                
                // Door Settings
                defaultDoorSpawnChance = 0.6f,
                defaultLockedDoorChance = 0.3f,
                defaultSecretDoorChance = 0.1f,
                
                // Room Settings
                generateRooms = true,
                minRooms = 3,
                maxRooms = 8,
                
                // Generation Options
                useRandomSeed = true,
                manualSeed = "MazeSeed2026",
                spawnInsideRoom = true,

                // Game Balance (NORMAL MODE)
                damageScale = 1.0f,
                healthScale = 1.0f,
                enemyHealthScale = 1.0f,
                speedScale = 1.0f,
                staminaDrainScale = 1.0f,
                godMode = false,
                oneHitKill = false,
                infiniteStamina = false,
                noClip = false,

                // Graphics/Audio
                graphicsQuality = "Medium",
                soundVolume = 0.8f,
                musicVolume = 0.6f,
                mouseSensitivity = 1.0f,
                invertY = false,
                showHUD = true
            };
        }
        
        /// <summary>
        /// Reset config to default values.
        /// </summary>
        public static void ResetToDefaults()
        {
            _instance = CreateDefault();
            Save();
            Debug.Log("[GameConfig]  Config reset to defaults");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Get config file path.
        /// </summary>
        public static string GetConfigPath()
        {
            return configPath;
        }
        
        /// <summary>
        /// Check if config file exists.
        /// </summary>
        public static bool ConfigExists()
        {
            return File.Exists(configPath);
        }
        
        /// <summary>
        /// Open config file in default text editor (for modding).
        /// </summary>
        public static void OpenInEditor()
        {
            if (File.Exists(configPath))
            {
                System.Diagnostics.Process.Start(configPath);
                Debug.Log($"[GameConfig]  Opened config in editor: {configPath}");
            }
        }
        
        #endregion
    }
}
