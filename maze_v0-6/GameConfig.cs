// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details

using System;
using UnityEngine;

namespace LavosTrial.Core.Maze
{
    // ─────────────────────────────────────────────────────────────
    //  GameConfig  —  JSON-driven, no hardcoded values
    //
    //  Can be used as:
    //   (a) MonoBehaviour component found via FindFirstObjectByType
    //   (b) Plain C# class deserialized from JSON (Resources load)
    //
    //  Maps 1:1 with Config/GameConfig-default.json
    // ─────────────────────────────────────────────────────────────
    [Serializable]
    public sealed class GameConfig : MonoBehaviour
    {
        // ── Maze geometry ─────────────────────────────────────────
        [Header("Maze Geometry")]
        public float CellSize          = 6.0f;
        public float WallHeight        = 4.0f;

        // ── Player ────────────────────────────────────────────────
        [Header("Player")]
        public float PlayerEyeHeight   = 1.7f;
        public float PlayerSpawnOffset = 0.5f;

        // ── Maze generation ───────────────────────────────────────
        [Header("Maze Generation")]
        public MazeConfig MazeCfg = new MazeConfig();

        // ── Serialization support (JSON field names mirror JSON) ──
        // Unity's JsonUtility will fill these when deserializing

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
        }

        public static GameConfig FromJson(string json)
        {
            var proxy = JsonUtility.FromJson<JsonProxy>(json);
            var cfg   = new GameConfig
            {
                CellSize          = proxy.cellSize          > 0 ? proxy.cellSize          : 6.0f,
                WallHeight        = proxy.wallHeight        > 0 ? proxy.wallHeight        : 4.0f,
                PlayerEyeHeight   = proxy.playerEyeHeight   > 0 ? proxy.playerEyeHeight   : 1.7f,
                PlayerSpawnOffset = proxy.playerSpawnOffset > 0 ? proxy.playerSpawnOffset : 0.5f,
                MazeCfg = new MazeConfig
                {
                    BaseSize      = proxy.mazeBaseSize  > 0 ? proxy.mazeBaseSize  : 12,
                    MinSize       = proxy.mazeMinSize   > 0 ? proxy.mazeMinSize   : 12,
                    MaxSize       = proxy.mazeMaxSize   > 0 ? proxy.mazeMaxSize   : 51,
                    SpawnRoomSize = proxy.spawnRoomSize > 0 ? proxy.spawnRoomSize : 5,
                    TorchChance   = proxy.torchChance   > 0 ? proxy.torchChance   : 0.30f,
                    ChestDensity  = proxy.chestDensity  > 0 ? proxy.chestDensity  : 0.03f,
                    EnemyDensity  = proxy.enemyDensity  > 0 ? proxy.enemyDensity  : 0.05f,
                }
            };
            return cfg;
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Stub so CompleteMazeBuilder compiles without the real one
    // ─────────────────────────────────────────────────────────────
    public sealed class PlayerController : MonoBehaviour { }
}
