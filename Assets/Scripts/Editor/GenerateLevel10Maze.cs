// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US
// 
// GenerateLevel10Maze.cs
// Editor tool to generate a Level 10 maze with all features
// - Cardinal-only passages (DFS + A*)
// - Scaled dead-end corridors (21% density at level 10)
// - Guaranteed path from spawn to exit
// - All values from JSON config

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// Editor tool to generate a Level 10 maze for testing.
    /// 
    /// Features:
    /// - Level 10 difficulty scaling
    /// - Cardinal-only DFS maze generation
    /// - A* guaranteed pathfinding
    /// - Dead-end corridors with 21% density (up from 15% base)
    /// - All configuration from JSON
    /// 
    /// Usage:
    /// 1. Open scene MazeLav8s_v1-0_1_5.unity
    /// 2. Tools -> Generate Level 10 Maze
    /// 3. Or press Ctrl+Alt+L
    /// </summary>
    public static class GenerateLevel10Maze
    {
        private const int LEVEL_10 = 10;
        private const string MENU_PATH = "Tools/Generate Level 10 Maze";
        private const string SHORTCUT = "%&L"; // Ctrl+Alt+L
        
        [MenuItem(MENU_PATH, false, 1)]
        public static void GenerateLevel10()
        {
            // Find CompleteMazeBuilder8 in scene
            var mazeBuilder = Object.FindFirstObjectByType<CompleteMazeBuilder8>();

            if (mazeBuilder == null)
            {
                EditorUtility.DisplayDialog(
                    "Level 10 Maze Generator",
                    "CompleteMazeBuilder8 not found in scene!\n\n" +
                    "Please open scene: Assets/Scenes/MazeLav8s_v1-0_1_5.unity",
                    "OK");
                return;
            }

            // Generate random seed for this level
            int seed = Random.Range(int.MinValue, int.MaxValue);

            // Set level and seed
            mazeBuilder.SetLevelAndSeed(LEVEL_10, seed);

            Debug.Log($"[Level10Generator] === GENERATING LEVEL {LEVEL_10} MAZE ===");
            Debug.Log($"[Level10Generator] Seed: {seed}");
            Debug.Log($"[Level10Generator] Expected maze size: ~22x22 (level 10)");
            Debug.Log($"[Level10Generator] Corridor width: 1 cell (6m)");
            Debug.Log($"[Level10Generator] Dead-end density: ~21% (scaled from 15% base)");
            Debug.Log($"[Level10Generator] Difficulty factor: ~1.75x");

            // Generate the maze
            mazeBuilder.GenerateMaze();

            Debug.Log($"[Level10Generator] === LEVEL {LEVEL_10} MAZE GENERATED ===");

            // Show confirmation dialog
            EditorUtility.DisplayDialog(
                "Level 10 Maze Generated",
                $"Level {LEVEL_10} maze generated successfully!\n\n" +
                $"Seed: {seed}\n" +
                $"Expected size: ~22x22\n" +
                $"Corridor width: 1 cell (6m)\n" +
                $"Dead-end density: ~21%\n" +
                $"Difficulty factor: ~1.75x\n\n" +
                $"Press Play to test!",
                "OK");
        }
        
        [MenuItem("Tools/Legacy/Generate Level 10 Maze (Fixed Seed)", false, 2)]
        public static void GenerateLevel10FixedSeed()
        {
            // Find CompleteMazeBuilder8 in scene
            var mazeBuilder = Object.FindFirstObjectByType<CompleteMazeBuilder8>();

            if (mazeBuilder == null)
            {
                EditorUtility.DisplayDialog(
                    "Level 10 Maze Generator",
                    "CompleteMazeBuilder8 not found in scene!\n\n" +
                    "Please open scene: Assets/Scenes/MazeLav8s_v1-0_1_5.unity",
                    "OK");
                return;
            }

            // Use a fixed seed for reproducible testing
            int fixedSeed = 42424242; // Magic number for level 10 testing

            // Set level and seed
            mazeBuilder.SetLevelAndSeed(LEVEL_10, fixedSeed);

            Debug.Log($"[Level10Generator] === GENERATING LEVEL {LEVEL_10} MAZE (FIXED SEED) ===");
            Debug.Log($"[Level10Generator] Fixed Seed: {fixedSeed}");
            Debug.Log($"[Level10Generator] Corridor width: 1 cell (6m)");
            Debug.Log($"[Level10Generator] This seed can be used to reproduce the same maze");

            // Generate the maze
            mazeBuilder.GenerateMaze();

            Debug.Log($"[Level10Generator] === LEVEL {LEVEL_10} MAZE GENERATED (FIXED SEED) ===");

            // Show confirmation dialog
            EditorUtility.DisplayDialog(
                "Level 10 Maze Generated (Fixed Seed)",
                $"Level {LEVEL_10} maze generated with fixed seed!\n\n" +
                $"Seed: {fixedSeed}\n" +
                $"Corridor width: 1 cell (6m)\n" +
                $"Use this seed to reproduce the same maze layout\n\n" +
                $"Press Play to test!",
                "OK");
        }
        
        [MenuItem("Tools/Legacy/Level 10 Maze Info", false, 10)]
        public static void ShowLevel10Info()
        {
            string info = @"Level 10 Maze Information

DIFFICULTY SCALING:
- Level: 10
- Difficulty Factor: ~1.75x
- Maze Size: ~22x22 (base 21 + level scaling)

CORRIDOR SETTINGS:
- Width: 1 cell (6 meters)
- Cardinal-only (N, S, E, W)
- No diagonal passages

DEAD-END CORRIDORS:
- Base Density: 15%
- Scaled Density: ~21% (at level 10)
- Multiplier: 1.4x (progressive scaling)
- Max Density: 37.5% (at level 39)

MAZE FEATURES:
- DFS (Recursive Backtracker)
- A* guaranteed pathfinding
- Dead-end corridors (2-5 cells long)
- 50% chest at dead-end
- 30% enemy at dead-end

CONFIGURATION:
- Config file: Config/GameConfig8-Level10.json
- Cell size: 6m x 6m
- Wall height: 3m
- Corridor width: 1 cell (6m)
- Torch chance: 25%
- Enemy density: 3% (scaled)
- Chest density: 5% (scaled)

TO GENERATE:
1. Open scene: MazeLav8s_v1-0_1_5.unity
2. Select MazeBuilder GameObject
3. Tools -> Generate Level 10 Maze
4. Or press Ctrl+Alt+L
5. Press Play to test";

            EditorUtility.DisplayDialog("Level 10 Maze Info", info, "OK");
        }
    }
}
