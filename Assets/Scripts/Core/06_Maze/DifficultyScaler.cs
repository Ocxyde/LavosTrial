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
// DifficultyScaler.cs
// Smooth progressive difficulty scaling for maze generation
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System;
using UnityEngine;

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
    //  At MaxLevel  : factor = 3.00   (always)
    //
    //  Scaled outputs (consumed by GridMazeGenerator8):
    //    MazeSize      = BaseSize + (int)(level × SizeRamp × factor)
    //    EnemyDensity  = BaseEnemyDensity  × factor
    //    ChestDensity  = BaseChestDensity  × (1 / factor)  ← rarer at high level
    //    TorchChance   = BaseTorchChance   × Lerp(1, TorchMaxMult, t)
    //    WallPenalty   = BaseWallPenalty   × factor        ← A* harder paths
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

        // ── Debug / display ───────────────────────────────────────

        /// <summary>Human-readable snapshot of all scaled values at a given level.</summary>
        public string Describe(int level, MazeConfig cfg)
        {
            float f = Factor(level);
            return
                $"Level {level}  |  factor={f:F3}  |  " +
                $"size={MazeSize(level, cfg.BaseSize, cfg.MinSize, cfg.MaxSize)}  |  " +
                $"enemies={EnemyDensity(cfg.EnemyDensity, level):P1}  |  " +
                $"chests={ChestDensity(cfg.ChestDensity, level):P1}  |  " +
                $"torches={TorchChance(cfg.TorchChance, level):P1}  |  " +
                $"wallPenalty={WallCrossPenalty(cfg.BaseWallPenalty, level)}";
        }
    }
}
