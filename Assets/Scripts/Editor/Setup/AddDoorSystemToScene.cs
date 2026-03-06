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
// AddDoorSystemToScene.cs
// ️ DEPRECATED - LEGACY DOOR SYSTEM SETUP ️
// Editor script to add LEGACY door system to existing maze
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ️ WARNING: This adds DEPRECATED components!
// For NEW development, use CompleteMazeBuilder + DoorsEngine + RealisticDoorFactory.
//
// Usage: Select Maze GameObject, run "Add Door System Components"

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

// Import the namespace where legacy components are defined
using Code.Lavos.Core;

// Suppress obsolete warnings - this file intentionally helps with legacy system setup
#pragma warning disable CS0618

namespace Code.Lavos.Editor
{
    /// <summary>
    /// ️ DEPRECATED - AddDoorSystemToScene - Legacy door system setup.
    /// For new development, use CompleteMazeBuilder + DoorsEngine + RealisticDoorFactory.
    /// </summary>
    public class AddDoorSystemToScene : MonoBehaviour
    {
        [MenuItem("Tools/PeuImporte/Add Door System to Maze (LEGACY)")]
        public static void AddDoorSystem()
        {
            // Find maze object
            GameObject mazeObj = Selection.activeGameObject;

            if (mazeObj == null)
            {
                // Try to find existing maze
                mazeObj = GameObject.Find("Maze");
                
                if (mazeObj == null)
                {
                    // Create new maze object
                    mazeObj = new GameObject("Maze");
                    Debug.Log(" Created new Maze GameObject");
                }
            }

            Debug.Log($"=== Adding LEGACY Door System to {mazeObj.name} ===");
            Debug.Log("️  WARNING: These are DEPRECATED components!");
            Debug.Log("");

            int added = 0;

            // Add MazeGenerator if not present
            if (mazeObj.GetComponent<MazeGenerator>() == null)
            {
                mazeObj.AddComponent<MazeGenerator>();
                Debug.Log(" Added MazeGenerator");
                added++;
            }
            else
            {
                Debug.Log(" MazeGenerator already present");
            }

            // Add RoomGenerator if not present
            if (mazeObj.GetComponent<RoomGenerator>() == null)
            {
                mazeObj.AddComponent<RoomGenerator>();
                Debug.Log(" Added RoomGenerator");
                added++;
            }
            else
            {
                Debug.Log(" RoomGenerator already present");
            }

            // Add DoorHolePlacer if not present
            if (mazeObj.GetComponent<DoorHolePlacer>() == null)
            {
                mazeObj.AddComponent<DoorHolePlacer>();
                Debug.Log(" Added DoorHolePlacer");
                added++;
            }
            else
            {
                Debug.Log(" DoorHolePlacer already present");
            }

            // Add RoomDoorPlacer if not present
            if (mazeObj.GetComponent<RoomDoorPlacer>() == null)
            {
                mazeObj.AddComponent<RoomDoorPlacer>();
                Debug.Log(" Added RoomDoorPlacer");
                added++;
            }
            else
            {
                Debug.Log(" RoomDoorPlacer already present");
            }

            // MazeRenderer is deprecated - skip
            Debug.Log("️  MazeRenderer skipped (deprecated)");

            // Add MazeIntegration if not present
            if (mazeObj.GetComponent<MazeIntegration>() == null)
            {
                mazeObj.AddComponent<MazeIntegration>();
                Debug.Log(" Added MazeIntegration");
                added++;
            }
            else
            {
                Debug.Log(" MazeIntegration already present");
            }

            Debug.Log("");
            Debug.Log("=== Summary ===");
            Debug.Log($" Added {added} LEGACY components");
            Debug.Log("️  These components are DEPRECATED");
            Debug.Log(" For new projects, use:");
            Debug.Log("   - CompleteMazeBuilder (maze generation)");
            Debug.Log("   - DoorsEngine (door behavior)");
            Debug.Log("   - RealisticDoorFactory (door creation)");
            Debug.Log("================");
        }
    }
}
#endif
