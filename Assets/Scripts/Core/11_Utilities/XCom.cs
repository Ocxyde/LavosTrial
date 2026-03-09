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
//
// XCom.cs
// Tactical command console for maze system - extensible command pattern
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   Call XCom.RegisterCommands() on game start
//   Then call XCom.Execute("command", args) from anywhere
//
// COMMANDS:
//   maze.generate       → Generate new maze
//   maze.status         → Show current maze status
//   maze.export         → Export maze to shareable code
//   maze.import [code]  → Import maze from code
//   maze.share          → Share maze (code + QR)
//   maze.help           → Show available commands
//
// LOCATION: Assets/Scripts/Core/11_Utilities/

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Lavos.Core
{
    /// <summary>
    /// XCom - Tactical command console for maze system.
    /// Extensible command pattern with plug-in-out architecture.
    /// Finds components, never creates.
    /// </summary>
    public static class XCom
    {
        // Command registry
        private static readonly Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>();
        private static bool _isInitialized = false;

        // Command prefix for console output
        private const string PREFIX = "[XCom]";

        /// <summary>
        /// Register all available commands.
        /// Call once on game start from GameManager or similar.
        /// </summary>
        public static void RegisterCommands()
        {
            if (_isInitialized)
            {
                Debug.LogWarning($"{PREFIX} Commands already registered");
                return;
            }

            // Register maze commands
            RegisterCommand("maze.generate", GenerateMaze);
            RegisterCommand("maze.status", ShowStatus);
            RegisterCommand("maze.export", ExportMaze);
            RegisterCommand("maze.import", ImportMaze);
            RegisterCommand("maze.share", ShareMaze);
            RegisterCommand("maze.help", ShowHelp);

            _isInitialized = true;
            Debug.Log($"{PREFIX} Commands registered - Type 'maze.help' for available commands");
        }

        /// <summary>
        /// Register a custom command.
        /// Extensible: Add your own commands from anywhere.
        /// </summary>
        /// <param name="command">Command name (e.g., "maze.generate").</param>
        /// <param name="action">Action to execute (receives args array).</param>
        public static void RegisterCommand(string command, Action<string[]> action)
        {
            if (_commands.ContainsKey(command))
            {
                Debug.LogWarning($"{PREFIX} Command '{command}' already registered - overwriting");
            }
            _commands[command] = action;
        }

        /// <summary>
        /// Execute a command.
        /// Plug-in-out: Finds components, never creates.
        /// </summary>
        /// <param name="command">Full command string (e.g., "maze.generate" or "maze.import ABC-123").</param>
        /// <returns>True if command executed, false if not found or invalid.</returns>
        public static bool Execute(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                Debug.LogError($"{PREFIX} Command is empty");
                return false;
            }

            // Parse command and arguments
            string[] parts = command.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string cmd = parts[0].ToLowerInvariant();
            string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            // Execute if registered
            if (_commands.TryGetValue(cmd, out Action<string[]> action))
            {
                try
                {
                    action(args);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"{PREFIX} Command '{cmd}' failed: {ex.Message}");
                    return false;
                }
            }
            else
            {
                Debug.LogError($"{PREFIX} Unknown command: '{cmd}' - Type 'maze.help' for available commands");
                return false;
            }
        }

        /// <summary>
        /// Execute a command with arguments.
        /// </summary>
        /// <param name="command">Command name.</param>
        /// <param name="args">Arguments array.</param>
        /// <returns>True if executed, false otherwise.</returns>
        public static bool Execute(string command, params string[] args)
        {
            if (_commands.TryGetValue(command.ToLowerInvariant(), out Action<string[]> action))
            {
                try
                {
                    action(args);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"{PREFIX} Command '{command}' failed: {ex.Message}");
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a command is registered.
        /// </summary>
        /// <param name="command">Command name.</param>
        /// <returns>True if registered, false otherwise.</returns>
        public static bool IsCommandRegistered(string command)
        {
            return _commands.ContainsKey(command.ToLowerInvariant());
        }

        /// <summary>
        /// Get list of all registered commands.
        /// </summary>
        /// <returns>Array of command names.</returns>
        public static string[] GetRegisteredCommands()
        {
            string[] cmds = new string[_commands.Count];
            _commands.Keys.CopyTo(cmds, 0);
            return cmds;
        }

        // ─────────────────────────────────────────────────────────────
        //  COMMAND IMPLEMENTATIONS
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Command: maze.generate
        /// Generate a new maze procedurally.
        /// Plug-in-out: Finds CompleteMazeBuilder8, never creates.
        /// </summary>
        private static void GenerateMaze(string[] args)
        {
            var builder = Object.FindFirstObjectByType<CompleteMazeBuilder8>();
            if (builder != null)
            {
                Debug.Log($"{PREFIX} Generating maze...");
                builder.GenerateMaze();
            }
            else
            {
                Debug.LogError($"{PREFIX} CompleteMazeBuilder8 not found in scene");
            }
        }

        /// <summary>
        /// Command: maze.status
        /// Show current maze information.
        /// Plug-in-out: Finds CompleteMazeBuilder8, never creates.
        /// </summary>
        private static void ShowStatus(string[] args)
        {
            var builder = Object.FindFirstObjectByType<CompleteMazeBuilder8>();
            if (builder != null)
            {
                var cfg = GameConfig.Instance;
                int mazeSize = cfg.DifficultyCfg.MazeSize(
                    builder.CurrentLevel,
                    cfg.MazeCfg.BaseSize,
                    cfg.MazeCfg.MinSize,
                    cfg.MazeCfg.MaxSize
                );

                Debug.Log("═══════════════════════════════════════");
                Debug.Log("  MAZE STATUS");
                Debug.Log("═══════════════════════════════════════");
                Debug.Log($"  Level: {builder.CurrentLevel}");
                Debug.Log($"  Maze Size: {mazeSize}x{mazeSize}");
                Debug.Log($"  Seed: {builder.CurrentSeed}");
                Debug.Log("═══════════════════════════════════════");
            }
            else
            {
                Debug.LogError($"{PREFIX} CompleteMazeBuilder8 not found in scene");
            }
        }

        /// <summary>
        /// Command: maze.export
        /// Export current maze to shareable code and copy to clipboard.
        /// Plug-in-out: Finds CompleteMazeBuilder8, never creates.
        /// </summary>
        private static void ExportMaze(string[] args)
        {
            var builder = Object.FindFirstObjectByType<CompleteMazeBuilder8>();
            if (builder != null)
            {
                string code = ShareSystem.ExportCode((uint)builder.CurrentSeed, builder.CurrentLevel);
                ShareSystem.CopyToClipboard(code);

                Debug.Log("═══════════════════════════════════════");
                Debug.Log("  MAZE EXPORTED");
                Debug.Log("═══════════════════════════════════════");
                Debug.Log($"  Code: {code}");
                Debug.Log("  Copied to clipboard!");
                Debug.Log("═══════════════════════════════════════");
            }
            else
            {
                Debug.LogError($"{PREFIX} CompleteMazeBuilder8 not found in scene");
            }
        }

        /// <summary>
        /// Command: maze.import [code]
        /// Import maze from shareable code.
        /// Validates format and checksum.
        /// </summary>
        private static void ImportMaze(string[] args)
        {
            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                Debug.LogError($"{PREFIX} Usage: maze.import [code]");
                Debug.LogError($"{PREFIX} Example: maze.import LAVOS-12345-5-ABCDEF12");
                return;
            }

            string code = args[0];

            if (ShareSystem.ImportCode(code, out uint seed, out int level))
            {
                Debug.Log("═══════════════════════════════════════");
                Debug.Log("  MAZE IMPORTED");
                Debug.Log("═══════════════════════════════════════");
                Debug.Log($"  Seed: {seed}");
                Debug.Log($"  Level: {level}");
                Debug.Log("  Ready to generate maze");
                Debug.Log("═══════════════════════════════════════");
            }
            else
            {
                Debug.LogError($"{PREFIX} Invalid maze code! Check format and try again.");
                Debug.LogError($"{PREFIX} Expected format: LAVOS-seed-level-checksum");
            }
        }

        /// <summary>
        /// Command: maze.share
        /// Share current maze (export code + generate QR URL).
        /// Plug-in-out: Finds CompleteMazeBuilder8, never creates.
        /// </summary>
        private static void ShareMaze(string[] args)
        {
            var builder = Object.FindFirstObjectByType<CompleteMazeBuilder8>();
            if (builder != null)
            {
                string code = ShareSystem.ExportCode((uint)builder.CurrentSeed, builder.CurrentLevel);
                string qrUrl = ShareSystem.GenerateQRDataUrl(code);

                Debug.Log("═══════════════════════════════════════");
                Debug.Log("  SHARE MAZE");
                Debug.Log("═══════════════════════════════════════");
                Debug.Log($"  Code: {code}");
                Debug.Log($"  QR Code: {qrUrl}");
                Debug.Log("  Copied to clipboard!");
                Debug.Log("═══════════════════════════════════════");

                ShareSystem.CopyToClipboard(code);
            }
            else
            {
                Debug.LogError($"{PREFIX} CompleteMazeBuilder8 not found in scene");
            }
        }

        /// <summary>
        /// Command: maze.help
        /// Show available console commands.
        /// </summary>
        private static void ShowHelp(string[] args)
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
