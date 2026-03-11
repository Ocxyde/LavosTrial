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
// 8-axis maze runtime configuration - JSON-driven
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System;
using UnityEngine;

namespace Code.Lavos.Core
{
    // 
    //  GameConfig    runtime config for the 8-axis maze system
    //
    //  No hardcoded values. All fields sourced from:
    //    Config/GameConfig8-default.json
    //
    //  Usage:
    //   (a) MonoBehaviour found via FindFirstObjectByType<GameConfig>
    //   (b) Static factory GameConfig.FromJson(jsonString)
    //   (c) Static singleton GameConfig.Instance (auto-finds in scene)
    //
    //  IMPORTANT: Must be added to scene manually.
    //  Do NOT rely on auto-creation (plug-in-out violation).
    // 
    [Serializable]
    public sealed class GameConfig : MonoBehaviour
    {
        //  Singleton pattern (scene-based) 
        private static GameConfig _instance;
        public static GameConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameConfig>();
                    if (_instance == null)
                    {
                        // Only log error in play mode - editor tools may call this without GameConfig in scene
                        if (Application.isPlaying)
                        {
                            Debug.LogError("[GameConfig] No instance found in scene! Add GameConfig GameObject manually.");
                        }
                        return null;
                    }
                }
                return _instance;
            }
        }

        //  Backward compatibility aliases (for legacy code) 
        public float DefaultCellSize => CellSize;
        public float DefaultWallHeight => WallHeight;
        public float DefaultPlayerEyeHeight => PlayerEyeHeight;
        public float DefaultPlayerSpawnOffset => PlayerSpawnOffset;
        public int DefaultRoomSize => MazeCfg.SpawnRoomSize;
        public int DefaultGridSize => MazeCfg.BaseSize;
        public int MinMazeSize => MazeCfg.MinSize;
        public int MaxMazeSize => MazeCfg.MaxSize;
        public float DefaultDoorSpawnChance => 0.6f;
        public int MaxRooms => 8;
        public bool GenerateRooms => true;

        // Door system aliases
        public float DefaultDoorWidth => 2.5f;
        public float DefaultDoorHeight => 3.0f;
        public float DefaultDoorDepth => 0.5f;
        public float DefaultDoorHoleDepth => 0.5f;
        public bool ShowDebugGizmos => false;
        public bool RandomizeWallTextures => true;
        public bool EnableTrappedDoors => true;
        public float DefaultTrapChance => 0.2f;
        public bool EnableLockedDoors => true;
        public bool EnableSecretDoors => true;
        public float DefaultLockedDoorChance => 0.3f;
        public float DefaultSecretDoorChance => 0.1f;

        // Corridor system aliases
        public int DefaultCorridorWidth => 1;
        public float CorridorRandomness => 0.3f;
        public bool GeneratePerimeterCorridor => true;

        // Player settings aliases
        public float MouseSensitivity => 1.0f;

        // Prefab paths (for editor tools)
        public string WallPrefab => "Prefabs/WallPrefab.prefab";
        public string DoorPrefab => "Prefabs/DoorPrefab.prefab";
        public string TorchPrefab => "Prefabs/TorchHandlePrefab.prefab";
        public string ChestPrefab => "Prefabs/ChestPrefab.prefab";
        public string EnemyPrefab => "Prefabs/EnemyPrefab.prefab";

        // Material paths (for editor tools)
        public string WallMaterial => "Materials/WallMaterial.mat";
        public string FloorMaterial => "Materials/Floor/Stone_Floor.mat";
        public string GroundTexture => "Textures/floor_texture.png";
        public string WallTexture => "Textures/wall_texture.png";

        // Wall thickness (for scaling - no hardcoded values in CompleteMazeBuilder)
        public float DefaultWallThickness => 0.2f;
        public float DefaultDiagonalWallThickness => 0.5f;

        [Header("Maze Geometry")]
        public float CellSize          = 6.0f;
        public float WallHeight        = 4.0f;
        public float WallThickness     = 0.2f;

        [Header("Player")]
        public float PlayerEyeHeight   = 1.7f;
        public float PlayerSpawnOffset = 0.5f;

        [Header("Maze Generation  8 Axis")]
        public MazeConfig MazeCfg = new MazeConfig();

        [Header("Difficulty Scaling  8 Axis")]
        public DifficultyScaler DifficultyCfg = new DifficultyScaler();

        [Header("Share System")]
        public string ShareSalt = "LAVOS_SECRET_SALT_2026";  // Salt for maze code checksum

        //  JSON deserialization proxy 
#pragma warning disable CS0649  // Fields assigned via JsonUtility.FromJson
        [Serializable]
        private struct JsonProxy
        {
            public float cellSize;
            public float wallHeight;
            public float playerEyeHeight;
            public float playerSpawnOffset;
            public int   mazeBaseSize;
            public int   mazeMaxSize;
            public int   spawnRoomSize;
            public float torchChance;
            public float chestDensity;
            public float enemyDensity;
            // diagonalWalls removed 2026-03-09 - cardinal-only passages
            public int   baseWallPenalty;

            // DifficultyScaler fields
            public int   diffMaxLevel;
            public float diffMaxFactor;
            public float diffExponent;
            public float diffSizeRamp;
            public float diffTorchMaxMult;

            // Share system fields
            public string shareSalt;
        }
#pragma warning restore CS0649

        public static GameConfig FromJson(string json)
        {
            var p   = JsonUtility.FromJson<JsonProxy>(json);
            var cfg = new GameConfig
            {
                CellSize          = p.cellSize          > 0 ? p.cellSize          : 6.0f,
                WallHeight        = p.wallHeight        > 0 ? p.wallHeight        : 4.0f,
                PlayerEyeHeight   = p.playerEyeHeight   > 0 ? p.playerEyeHeight   : 1.7f,
                PlayerSpawnOffset = p.playerSpawnOffset > 0 ? p.playerSpawnOffset : 0.5f,
                ShareSalt         = !string.IsNullOrEmpty(p.shareSalt) ? p.shareSalt : "LAVOS_SECRET_SALT_2026",
                MazeCfg = new MazeConfig
                {
                    BaseSize       = p.mazeBaseSize  > 0 ? p.mazeBaseSize  : 12,
                    MinSize        = 12, // mazeMinSize removed - use default
                    MaxSize        = p.mazeMaxSize   > 0 ? p.mazeMaxSize   : 51,
                    SpawnRoomSize  = p.spawnRoomSize > 0 ? p.spawnRoomSize : 5,
                    TorchChance    = p.torchChance   > 0 ? p.torchChance   : 0.30f,
                    ChestDensity   = p.chestDensity  > 0 ? p.chestDensity  : 0.03f,
                    EnemyDensity   = p.enemyDensity  > 0 ? p.enemyDensity  : 0.05f,
                    BaseWallPenalty = p.baseWallPenalty > 0 ? p.baseWallPenalty : 100,
                },
                DifficultyCfg = new DifficultyScaler
                {
                    MaxLevel     = p.diffMaxLevel   > 0 ? p.diffMaxLevel   : 39,
                    MaxFactor    = p.diffMaxFactor  > 0 ? p.diffMaxFactor  : 3.0f,
                    Exponent     = p.diffExponent   > 0 ? p.diffExponent   : 2.0f,
                    SizeRamp     = p.diffSizeRamp   > 0 ? p.diffSizeRamp   : 1.0f,
                    TorchMaxMult = p.diffTorchMaxMult > 0 ? p.diffTorchMaxMult : 1.5f,
                }
            };
            return cfg;
        }
    }
}
