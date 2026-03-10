// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using System;
using UnityEngine;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Core
{
    // ─────────────────────────────────────────────────────────────
    //  DifficultyScaler
    //
    //  Computes a smooth progressive difficulty factor over the
    //  full level range, scaling from 1.0 → MaxFactor (default 3.0).
    //
    //  Curve formula (power curve):
    //    t      = Clamp01(level / MaxLevel)
    //    factor = 1 + (MaxFactor - 1) * t ^ Exponent
    //
    //  Exponent behaviour:
    //    < 1.0  → fast early ramp, slow late (logarithmic feel)
    //    = 1.0  → perfectly linear 1× → 3×
    //    > 1.0  → slow early ramp, fast late (exponential feel)
    //    = 2.0  → smooth quadratic acceleration (recommended)
    //
    //  At level 0   : factor = 1.00   (baseline)
    //  At half way  : factor ≈ 1.75   (exponent 2.0)
    //  At MaxLevel  : factor = MaxFactor (always)
    //
    //  Scaled outputs (consumed by GridMazeGenerator8):
    //    MazeSize        = BaseSize + (int)(level × SizeRamp × factor)
    //    EnemyDensity    = BaseEnemyDensity    × factor
    //    ChestDensity    = BaseChestDensity    × (1 / factor)  ← rarer at high level
    //    TorchChance     = BaseTorchChance     × Lerp(1, TorchMaxMult, t)
    //    WallPenalty     = BaseWallPenalty     × factor        ← A* harder paths
    //    DeadEndDensity  = BaseDeadEndDensity  × Lerp(1, DeadEndMaxMult, t)
    // ─────────────────────────────────────────────────────────────
    [Serializable]
    public sealed class DifficultyScaler
    {
        // ── Tunable via JSON ──────────────────────────────────────
        public int   MaxLevel        = 39;
        public float MaxFactor       = 3.0f;
        public float Exponent        = 2.0f;   // power-curve shaping
        public float SizeRamp        = 1.0f;   // extra size growth multiplier
        public float TorchMaxMult    = 1.5f;   // torch cap at max level
        public float DeadEndMaxMult  = 2.5f;   // dead-end density multiplier at max level

        // ── Core factor ───────────────────────────────────────────

        /// <summary>
        /// Returns the difficulty factor for the given level.
        /// Range: [1.0 .. MaxFactor].
        /// </summary>
        public float Factor(int level)
        {
            if (level <= 0)        return 1.0f;
            if (level >= MaxLevel) return MaxFactor;

            float t = (float)level / MaxLevel;                     // 0..1
            float curved = Mathf.Pow(t, Exponent);                 // shape
            return 1.0f + (MaxFactor - 1.0f) * curved;            // 1..MaxFactor
        }

        /// <summary>Normalized progress [0..1] for the given level.</summary>
        public float NormalizedT(int level)
            => Mathf.Clamp01((float)level / MaxLevel);

        // ── Derived parameters ────────────────────────────────────

        /// <summary>Maze grid size for this level.</summary>
        public int MazeSize(int level, int baseSize, int minSize, int maxSize)
        {
            float f    = Factor(level);
            int   size = baseSize + Mathf.RoundToInt(level * SizeRamp * f);
            size = Mathf.Clamp(size, minSize, maxSize);
            if (size % 2 == 0) size++;     // odd grid required
            return size;
        }

        /// <summary>Enemy spawn probability — grows with difficulty.</summary>
        public float EnemyDensity(float baseDensity, int level)
            => Mathf.Clamp01(baseDensity * Factor(level));

        /// <summary>Chest spawn probability — shrinks with difficulty (rarer rewards).</summary>
        public float ChestDensity(float baseDensity, int level)
            => Mathf.Clamp01(baseDensity / Factor(level));

        /// <summary>Torch chance — grows moderately toward cap.</summary>
        public float TorchChance(float baseChance, int level)
        {
            float t = NormalizedT(level);
            float mult = Mathf.Lerp(1.0f, TorchMaxMult, t);
            return Mathf.Clamp01(baseChance * mult);
        }

        /// <summary>
        /// A* wall crossing penalty — higher difficulty means walls are
        /// more expensive to cross, forcing longer but more faithful paths.
        /// </summary>
        public int WallCrossPenalty(int basePenalty, int level)
            => Mathf.RoundToInt(basePenalty * Factor(level));

        /// <summary>
        /// Dead-end corridor density — increases with level.
        /// Higher levels get more dead-end corridors for complexity.
        /// Scales from base density → base × DeadEndMaxMult at max level.
        /// </summary>
        public float DeadEndDensity(float baseDensity, int level)
        {
            float t = NormalizedT(level);
            float mult = Mathf.Lerp(1.0f, DeadEndMaxMult, t);
            return Mathf.Clamp01(baseDensity * mult);
        }

        /// <summary>
        /// Room count — scales from minimum to maximum rooms based on level.
        /// Level 0: MinRooms (1-2 rooms)
        /// Level 12: ~4-6 rooms
        /// Level 20: ~6-8 rooms
        /// Level 39: MaxRooms (10-14 rooms)
        /// Formula: MinRooms + (MaxRooms - MinRooms) × t^RoomExponent
        /// </summary>
        public int RoomCount(int minRooms, int maxRooms, int level)
        {
            float t = NormalizedT(level);
            // Use room exponent for smooth curve (default 1.5 for gradual ramp)
            float curved = Mathf.Pow(t, 1.5f);
            int rooms = minRooms + Mathf.RoundToInt((maxRooms - minRooms) * curved);
            return Mathf.Clamp(rooms, minRooms, maxRooms);
        }

        /// <summary>
        /// Room size — scales proportionally to maze size based on difficulty.
        /// Formula: RoomSize = (MazeSize × RoomSizeRatio) clamped to min/max
        /// 
        /// Level 0: Maze 13×13 → Room ~3×3 to 5×5 (small, cozy)
        /// Level 12: Maze 25×25 → Room ~5×5 to 7×7 (medium)
        /// Level 20: Maze 32×32 → Room ~7×7 to 9×9 (large)
        /// Level 39: Maze 51×51 → Room ~9×9 to 13×13 (boss/monster rooms)
        /// 
        /// Room size is proportional to maze size, not fixed steps.
        /// More dangerous levels = larger rooms (more space for enemies/traps)
        /// </summary>
        public int RoomSize(int mazeSize, int level, int minRoomSize = 3, int maxRoomSize = 13)
        {
            float t = NormalizedT(level);
            
            // Base room size is proportional to maze size (20% to 30% of maze dimension)
            float minRatio = 0.20f;  // Small rooms at low levels
            float maxRatio = 0.30f;  // Large rooms at high levels
            float ratio = Mathf.Lerp(minRatio, maxRatio, t);
            
            // Calculate room size from maze size
            int baseRoomSize = Mathf.RoundToInt(mazeSize * ratio);
            
            // Ensure room size is odd (for symmetric center)
            if (baseRoomSize % 2 == 0) baseRoomSize++;
            
            // Clamp to min/max bounds
            int roomSize = Mathf.Clamp(baseRoomSize, minRoomSize, maxRoomSize);
            
            // Ensure odd
            if (roomSize % 2 == 0) roomSize--;
            
            return roomSize;
        }

        /// <summary>
        /// Door complexity — determines door types available at this level.
        /// Level 0-5: Normal doors only
        /// Level 6-15: + Locked doors (20% chance)
        /// Level 16-30: + Secret doors (10% chance)
        /// Level 31-39: All types (40% locked, 20% secret)
        /// </summary>
        public float LockedDoorChance(int level)
        {
            if (level < 6) return 0f;
            if (level < 16) return 0.20f;
            if (level < 31) return 0.30f;
            return 0.40f;
        }

        public float SecretDoorChance(int level)
        {
            if (level < 16) return 0f;
            if (level < 31) return 0.10f;
            return 0.20f;
        }

        // ── Debug / display ───────────────────────────────────────

        /// <summary>Human-readable snapshot of all scaled values at a given level.</summary>
        public string Describe(int level, DungeonMazeConfig cfg)
        {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));

            float f         = Factor(level);
            int   size      = MazeSize(level, cfg.BaseSize, cfg.MinSize, cfg.MaxSize);
            float enemies   = EnemyDensity(cfg.EnemyDensity, level);
            float chests    = ChestDensity(cfg.ChestDensity, level);
            float torches   = TorchChance(cfg.TorchChance, level);
            int   wallPenalty = WallCrossPenalty(cfg.BaseWallPenalty, level);
            float deadEnds  = DeadEndDensity(cfg.DeadEndDensity, level);
            
            // Room system uses defaults (not in DungeonMazeConfig yet)
            int   rooms     = RoomCount(2, 12, level);  // MinRooms=2, MaxRooms=12
            int   roomSize  = RoomSize(size, level);    // Proportional to maze size
            float lockedDoors = LockedDoorChance(level);
            float secretDoors = SecretDoorChance(level);

            return $"Level {level} | factor={f:F3} | size={size} | rooms={rooms} (size={roomSize}) | " +
                   $"enemies={enemies:P1} | chests={chests:P1} | torches={torches:P1} | " +
                   $"lockedDoors={lockedDoors:P0} | secretDoors={secretDoors:P0} | " +
                   $"wallPenalty={wallPenalty} | deadEnds={deadEnds:P1}";
        }
    }
}
