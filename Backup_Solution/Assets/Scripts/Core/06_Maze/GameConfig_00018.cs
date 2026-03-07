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
    // ─────────────────────────────────────────────────────────────
    //  GameConfig  —  runtime config for the 8-axis maze system
    //
    //  No hardcoded values. All fields sourced from:
    //    Config/GameConfig8-default.json
    //
    //  Usage:
    //   (a) MonoBehaviour found via FindFirstObjectByType<GameConfig>
    //   (b) Static factory GameConfig.FromJson(jsonString)
    //   (c) Static singleton GameConfig.Instance (auto-finds in scene)
    // ─────────────────────────────────────────────────────────────
    [Serializable]
    public sealed class GameConfig : MonoBehaviour
    {
        // ── Singleton pattern (scene-based) ───────────────────────
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
                        Debug.LogWarning("[GameConfig] Instance not found in scene - creating fallback");
                        var go = new GameObject("GameConfig");
                        _instance = go.AddComponent<GameConfig>();
                    }
                }
                return _instance;
            }
        }

        // ── Backward compatibility aliases (for legacy code) ──────
        public float defaultCellSize => CellSize;
        public float defaultWallHeight => WallHeight;
        public float defaultPlayerEyeHeight => PlayerEyeHeight;
        public float defaultPlayerSpawnOffset => PlayerSpawnOffset;
        public int defaultRoomSize => MazeCfg.SpawnRoomSize;
        public int defaultGridSize => MazeCfg.BaseSize;
        public int minMazeSize => MazeCfg.MinSize;
        public int maxMazeSize => MazeCfg.MaxSize;
        public float defaultDoorSpawnChance => 0.6f;
        public int maxRooms => 8;
        public bool generateRooms => true;

        [Header("Maze Geometry")]
        public float CellSize          = 6.0f;
        public float WallHeight        = 4.0f;

        [Header("Player")]
        public float PlayerEyeHeight   = 1.7f;
        public float PlayerSpawnOffset = 0.5f;

        [Header("Maze Generation — 8 Axis")]
        public MazeConfig MazeCfg = new MazeConfig();

        // ── JSON deserialization proxy ────────────────────────────
        [Serializable]
        private struct JsonProxy
        {
            public float cellSize;
            public float wallHeight;
            public float playerEyeHeight;
            public float playerSpawnOffset;
            public int   mazeBaseSize;
            public int   mazeMinSize;
            public int   mazeMaxSize;
            public int   spawnRoomSize;
            public float torchChance;
            public float chestDensity;
            public float enemyDensity;
            public bool  diagonalWalls;
        }

        public static GameConfig FromJson(string json)
        {
            var p   = JsonUtility.FromJson<JsonProxy>(json);
            var cfg = new GameConfig
            {
                CellSize          = p.cellSize          > 0 ? p.cellSize          : 6.0f,
                WallHeight        = p.wallHeight        > 0 ? p.wallHeight        : 4.0f,
                PlayerEyeHeight   = p.playerEyeHeight   > 0 ? p.playerEyeHeight   : 1.7f,
                PlayerSpawnOffset = p.playerSpawnOffset > 0 ? p.playerSpawnOffset : 0.5f,
                MazeCfg = new MazeConfig
                {
                    BaseSize      = p.mazeBaseSize  > 0 ? p.mazeBaseSize  : 12,
                    MinSize       = p.mazeMinSize   > 0 ? p.mazeMinSize   : 12,
                    MaxSize       = p.mazeMaxSize   > 0 ? p.mazeMaxSize   : 51,
                    SpawnRoomSize = p.spawnRoomSize > 0 ? p.spawnRoomSize : 5,
                    TorchChance   = p.torchChance   > 0 ? p.torchChance   : 0.30f,
                    ChestDensity  = p.chestDensity  > 0 ? p.chestDensity  : 0.03f,
                    EnemyDensity  = p.enemyDensity  > 0 ? p.enemyDensity  : 0.05f,
                    DiagonalWalls = p.diagonalWalls,
                }
            };
            return cfg;
        }
    }
}
