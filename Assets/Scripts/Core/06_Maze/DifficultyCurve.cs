// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 | Locale: en_US
// Cell-Based Maze Generation System - Phase 2: Difficulty Curve

using System;
using UnityEngine;

namespace Code.Lavos.Core.Maze
{
    /// <summary>
    /// DifficultyCurve - Tracks min/max/actual path lengths for difficulty scaling.
    /// </summary>
    [Serializable]
    public struct DifficultyCurve
    {
        // Path Lengths
        [Tooltip("Minimum path length (A* shortest)")] public int minPathLength;
        [Tooltip("Maximum path length (longest snake)")] public int maxPathLength;
        [Tooltip("Actual path length")] public int actualPathLength;
        [Tooltip("Target path length")] public int targetPathLength;
        
        // Difficulty Metrics
        [Tooltip("Difficulty (0-1)")] public float difficulty;
        [Tooltip("Decoy density (0-1)")] public float decoyDensity;
        [Tooltip("Room count")] public int roomCount;
        
        // Properties
        public float PathEfficiency => maxPathLength > 0 ? (float)actualPathLength / maxPathLength : 0f;
        public float WindingFactor => minPathLength > 0 ? (float)actualPathLength / minPathLength : 1f;
        
        public static DifficultyCurve FromLevel(int level, int mazeSize)
        {
            float difficulty = Mathf.Clamp01(level / 39.0f);
            int minPath = Mathf.RoundToInt(mazeSize * 1.5f);
            int maxPath = Mathf.RoundToInt(mazeSize * mazeSize * 0.8f);
            int targetPath = Mathf.RoundToInt(Mathf.Lerp(minPath, maxPath, difficulty));
            float decoyDensity = Mathf.Lerp(0.2f, 0.75f, difficulty);
            int roomCount = Mathf.RoundToInt(Mathf.Lerp(1f, 5f, difficulty));
            
            return new DifficultyCurve
            {
                minPathLength = minPath, maxPathLength = maxPath, targetPathLength = targetPath,
                difficulty = difficulty, decoyDensity = decoyDensity, roomCount = roomCount
            };
        }
        
        public bool Validate() => minPathLength > 0 && maxPathLength >= minPathLength && 
                                   difficulty >= 0f && difficulty <= 1f;
        
        public void Log() => Debug.Log($"[DifficultyCurve] Lvl{difficulty*39:F0} Min:{minPathLength} Max:{maxPathLength} Target:{targetPathLength} Decoys:{decoyDensity:P0} Rooms:{roomCount}");
        
        public override string ToString() => $"DifficultyCurve(Lvl{difficulty*39:F0}, Min:{minPathLength}, Max:{maxPathLength})";
    }
}
