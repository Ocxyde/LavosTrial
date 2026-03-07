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
            Debug.Log("[MazeConsole]  Console commands registered");
            Debug.Log("[MazeConsole]  Type 'maze.help' for available commands");
        }

        // Command: maze.verbosity [full|short|mute]
        // REMOVED: Verbosity system removed - all logs show by default
        public static void SetVerbosity(string level)
        {
            Debug.Log("[MazeConsole] Verbosity removed - all logs show by default");
        }

        // Command: maze.generate
        public static void GenerateMaze()
        {
            var builder = Object.FindFirstObjectByType<CompleteMazeBuilder>();
            if (builder != null)
            {
                Debug.Log("[MazeConsole] Generating maze...");
                builder.GenerateMaze();
            }
            else
            {
                Debug.LogError("[MazeConsole]  CompleteMazeBuilder not found in scene");
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
                Debug.Log($"  Level: {builder.CurrentLevel}");
                Debug.Log($"  Maze Size: {builder.MazeSize}x{builder.MazeSize}");
                Debug.Log($"  Seed: {builder.CurrentSeed}");
                Debug.Log($"  Generation Time: {builder.LastGenMs:F2}ms");
                Debug.Log("═══════════════════════════════════════");
            }
            else
            {
                Debug.LogError("[MazeConsole] CompleteMazeBuilder not found in scene");
            }
        }

        // Command: maze.export
        public static void ExportMaze()
        {
            var builder = Object.FindFirstObjectByType<CompleteMazeBuilder>();
            if (builder != null)
            {
                string code = MazeShareSystem.ExportCode(builder.CurrentSeed, builder.CurrentLevel);
                Debug.Log($"═══════════════════════════════════════");
                Debug.Log($"  MAZE EXPORTED");
                Debug.Log($"═══════════════════════════════════════");
                Debug.Log($"  Code: {code}");
                Debug.Log($"  Copied to clipboard!");
                Debug.Log($"═══════════════════════════════════════");

                MazeShareSystem.CopyToClipboard(code);
            }
            else
            {
                Debug.LogError("[MazeConsole] CompleteMazeBuilder not found in scene");
            }
        }

        // Command: maze.import [code]
        public static void ImportMaze(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                Debug.LogError("[MazeConsole] Usage: maze.import [code]");
                Debug.LogError("[MazeConsole] Example: maze.import LAVOS-12345-L5-S0-ABCDEF12");
                return;
            }

            if (MazeShareSystem.ImportCode(code, out int seed, out int level, out int subSeed))
            {
                Debug.Log($"═══════════════════════════════════════");
                Debug.Log($"  MAZE IMPORTED");
                Debug.Log($"═══════════════════════════════════════");
                Debug.Log($"  Seed: {seed}");
                Debug.Log($"  Level: {level}");
                Debug.Log($"  Sub-Seed: {subSeed}");
                Debug.Log($"  Generating maze...");
                Debug.Log($"═══════════════════════════════════════");

                // Note: You'll need to add methods to set seed/level in CompleteMazeBuilder
                // For now, just log the values
            }
            else
            {
                Debug.LogError("[MazeConsole] Invalid maze code! Check format and try again.");
                Debug.LogError("[MazeConsole] Expected format: LAVOS-seed-level-subSeed-checksum");
            }
        }

        // Command: maze.share
        public static void ShareMaze()
        {
            var builder = Object.FindFirstObjectByType<CompleteMazeBuilder>();
            if (builder != null)
            {
                string code = MazeShareSystem.ExportCode(builder.CurrentSeed, builder.CurrentLevel);
                string qrUrl = MazeShareSystem.GenerateQRDataUrl(code);

                Debug.Log($"═══════════════════════════════════════");
                Debug.Log($"  SHARE MAZE");
                Debug.Log($"═══════════════════════════════════════");
                Debug.Log($"  Code: {code}");
                Debug.Log($"  QR Code: {qrUrl}");
                Debug.Log($"  Copied to clipboard!");
                Debug.Log($"═══════════════════════════════════════");

                MazeShareSystem.CopyToClipboard(code);
            }
            else
            {
                Debug.LogError("[MazeConsole] CompleteMazeBuilder not found in scene");
            }
        }

        // Command: maze.help
        public static void ShowHelp()
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("  MAZE CONSOLE COMMANDS");
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("  maze.generate         - Generate new maze");
            Debug.Log("  maze.status           - Show current maze info");
            Debug.Log("  maze.export           - Export maze to shareable code");
            Debug.Log("  maze.import [code]    - Import maze from code");
            Debug.Log("  maze.share            - Share maze (code + QR)");
            Debug.Log("  maze.help             - Show this help");
            Debug.Log("═══════════════════════════════════════");
        }
    }
}
                Debug.Log("  Seed: Random (procedural each load)");
                Debug.Log("═══════════════════════════════════════");
            }
            else
            {
                Debug.LogError("[MazeConsole]  CompleteMazeBuilder not found");
            }
        }

        // Command: maze.help
        public static void ShowHelp()
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("  MAZE CONSOLE COMMANDS");
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("  maze.generate    → Generate new maze");
            Debug.Log("  maze.status      → Show current status");
            Debug.Log("  maze.help        → Show this help");
            Debug.Log("═══════════════════════════════════════");
        }
    }
}
