// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using System;
using UnityEngine;

namespace LavosTrial.Core.Maze
{
    // ─────────────────────────────────────────────────────────────
    //  GameConfig8  —  runtime config for the 8-axis maze system
    //
    //  No hardcoded values. All fields sourced from:
    //    Config/GameConfig8-default.json
    //
    //  Usage:
    //   (a) MonoBehaviour found via FindFirstObjectByType<GameConfig8>
    //   (b) Static factory GameConfig8.FromJson(jsonString)
    // ─────────────────────────────────────────────────────────────
    [Serializable]
    public sealed class GameConfig8 : MonoBehaviour
    {
        [Header("Maze Geometry")]
        public float CellSize          = 6.0f;
        public float WallHeight        = 4.0f;

        [Header("Player")]
        public float PlayerEyeHeight   = 1.7f;
        public float PlayerSpawnOffset = 0.5f;

        [Header("Maze Generation — 8 Axis")]
        public MazeConfig8 MazeCfg = new MazeConfig8();

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

        public static GameConfig8 FromJson(string json)
        {
            var p   = JsonUtility.FromJson<JsonProxy>(json);
            var cfg = new GameConfig8
            {
                CellSize          = p.cellSize          > 0 ? p.cellSize          : 6.0f,
                WallHeight        = p.wallHeight        > 0 ? p.wallHeight        : 4.0f,
                PlayerEyeHeight   = p.playerEyeHeight   > 0 ? p.playerEyeHeight   : 1.7f,
                PlayerSpawnOffset = p.playerSpawnOffset > 0 ? p.playerSpawnOffset : 0.5f,
                MazeCfg = new MazeConfig8
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
