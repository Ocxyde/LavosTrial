// MazeConsoleCommands.cs
// Console commands for maze system control
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   Press ~ (tilde) to open console, then type:
//   - maze.verbosity full   → Show all debug messages
//   - maze.verbosity short  → Show only critical messages
//   - maze.verbosity mute   → No console output
//   - maze.generate         → Generate new maze
//   - maze.status           → Show current maze status
//
// Location: Assets/Scripts/Core/06_Maze/

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// MazeConsoleCommands - Runtime console commands for maze system.
    /// Press ~ (tilde) to open Unity console, then type commands.
    /// </summary>
    public static class MazeConsoleCommands
    {
        // Register commands (call from GameManager or similar)
        public static void RegisterCommands()
        {
            Debug.Log("[MazeConsole] 📟 Console commands registered");
            Debug.Log("[MazeConsole] 📟 Type 'maze.help' for available commands");
        }

        // Command: maze.verbosity [full|short|mute]
        public static void SetVerbosity(string level)
        {
            CompleteMazeBuilder.SetVerbosity(level);
        }

        // Command: maze.generate
        public static void GenerateMaze()
        {
            var builder = Object.FindFirstObjectByType<CompleteMazeBuilder>();
            if (builder != null)
            {
                Debug.Log("[MazeConsole] 🏗️ Generating maze...");
                builder.GenerateMaze();
            }
            else
            {
                Debug.LogError("[MazeConsole] ❌ CompleteMazeBuilder not found in scene");
            }
        }

        // Command: maze.status
        public static void ShowStatus()
        {
            var builder = Object.FindFirstObjectByType<CompleteMazeBuilder>();
            if (builder != null)
            {
                Debug.Log("═══════════════════════════════════════");
                Debug.Log("  MAZE STATUS");
                Debug.Log("═══════════════════════════════════════");
                Debug.Log($"  Verbosity: {CompleteMazeBuilder.CurrentVerbosity}");
                Debug.Log("═══════════════════════════════════════");
            }
            else
            {
                Debug.LogError("[MazeConsole] ❌ CompleteMazeBuilder not found");
            }
        }

        // Command: maze.help
        public static void ShowHelp()
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("  MAZE CONSOLE COMMANDS");
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("  maze.verbosity full   → All debug messages");
            Debug.Log("  maze.verbosity short  → Critical only");
            Debug.Log("  maze.verbosity mute   → No output");
            Debug.Log("  maze.generate         → Generate new maze");
            Debug.Log("  maze.status           → Show status");
            Debug.Log("  maze.help             → Show this help");
            Debug.Log("═══════════════════════════════════════");
        }
    }
}
