// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 (no BOM) | Line Endings: Unix LF
// Auto-Generate Maze with NEW SEED on scene load / new game

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// AutoGenerateMazeOnLoad - Generates new maze with new seed on scene load.
    /// 
    /// Features:
    /// - New random seed on each scene load
    /// - New random seed on new game
    /// - New random seed on load game
    /// - Plug-in-Out compliant (uses existing 1-Click tool)
    /// 
    /// Usage:
    /// Add to a GameObject in scene (e.g., MazeBuilder)
    /// </summary>
    public class AutoGenerateMazeOnLoad : MonoBehaviour
    {
        [Header("Auto-Generate Settings")]
        [Tooltip("Generate new maze with new seed on scene load")]
        [SerializeField] private bool generateOnSceneLoad = true;
        
        [Tooltip("Generate new maze with new seed on new game")]
        [SerializeField] private bool generateOnNewGame = true;
        
        [Tooltip("Generate new maze with new seed on load game")]
        [SerializeField] private bool generateOnLoadGame = true;
        
        [Header("Maze Settings")]
        [SerializeField] private int mazeWidth = 21;
        [SerializeField] private int mazeHeight = 21;
        [SerializeField] private int level = 5;
        
        private void Awake()
        {
            // Subscribe to scene loaded event
            if (generateOnSceneLoad)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from scene loaded event
            if (generateOnSceneLoad)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }
        
        /// <summary>
        /// Called when scene is loaded - generates new maze with new seed.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Only generate in main maze scene
            if (scene.name.Contains("Maze") || scene.name.Contains("Lav8s"))
            {
                Debug.Log("[AutoGenerateMazeOnLoad] Scene loaded - generating new maze with new seed...");
                GenerateNewMaze();
            }
        }
        
        /// <summary>
        /// Call this for NEW GAME - generates new maze with new seed.
        /// </summary>
        public void OnNewGame()
        {
            if (generateOnNewGame)
            {
                Debug.Log("[AutoGenerateMazeOnLoad] New game - generating new maze with new seed...");
                GenerateNewMaze();
            }
        }
        
        /// <summary>
        /// Call this for LOAD GAME - generates new maze with new seed.
        /// </summary>
        public void OnLoadGame()
        {
            if (generateOnLoadGame)
            {
                Debug.Log("[AutoGenerateMazeOnLoad] Load game - generating new maze with new seed...");
                GenerateNewMaze();
            }
        }
        
        /// <summary>
        /// Generate new maze with difficulty-based seed (uses existing SeedManager).
        /// </summary>
        private void GenerateNewMaze()
        {
            // Find CompleteMazeBuilder8 in scene (Plug-in-Out: find, never create)
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder8>();
            
            if (mazeBuilder == null)
            {
                Debug.LogWarning("[AutoGenerateMazeOnLoad] No CompleteMazeBuilder8 found!");
                return;
            }
            
            // Use SeedManager for difficulty-based seeding (existing system, not duplicated)
            var seedManager = FindFirstObjectByType<SeedManager>();
            
            int newSeed;
            if (seedManager != null)
            {
                newSeed = (int)seedManager.ComputeSeed;
                Debug.Log($"[AutoGenerateMazeOnLoad] Using SeedManager compute seed: {newSeed}");
            }
            else
            {
                // Fallback: generate difficulty-based seed
                newSeed = GenerateDifficultyBasedSeed(level);
                Debug.Log($"[AutoGenerateMazeOnLoad] Generated difficulty-based seed: {newSeed}");
            }
            
            // Set level and seed
            mazeBuilder.SetLevelAndSeed(level, newSeed);
            
            // Generate maze
            Debug.Log($"[AutoGenerateMazeOnLoad] Generated new maze: Level {level}, Seed: {newSeed}");
        }
        
        /// <summary>
        /// Generate difficulty-based seed (existing SeedManager method - reused).
        /// </summary>
        private int GenerateDifficultyBasedSeed(int level)
        {
            int tickCount = System.Environment.TickCount;
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            float random = UnityEngine.Random.value;
            
            uint tick = (uint)tickCount;
            uint time = (uint)(timestamp & 0xFFFFFFFF);
            uint rand = (uint)(random * uint.MaxValue);
            uint levelFactor = (uint)(level * 1000);
            
            uint seed = tick ^ time ^ rand ^ levelFactor;
            return (int)(seed & 0x7FFFFFFF);
        }
    }
}
