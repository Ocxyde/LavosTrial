// DoorSystemSetup.cs
// ⚠️ DEPRECATED - LEGACY DOOR SETUP HELPER ⚠️
// Editor helper script to verify and setup door system components
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// This file uses deprecated components (DoorHolePlacer, RoomDoorPlacer, MazeRenderer, WallTextureSet).
// For new development, use CompleteMazeBuilder + DoorsEngine + RealisticDoorFactory.
//
// Usage: Add to Maze GameObject, run VerifySetup() in Context Menu

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Suppress obsolete warnings - this file intentionally helps with legacy system setup
#pragma warning disable CS0618
#pragma warning disable CS0246  // Suppress missing type warnings for deprecated types

namespace Code.Lavos.Core
{
    /// <summary>
    /// DoorSystemSetup - Verify and configure door system.
    /// Run from Inspector context menu.
    /// </summary>
    [ExecuteInEditMode]
    public class DoorSystemSetup : MonoBehaviour
    {
        [ContextMenu("Verify Door System Setup")]
        public void VerifySetup()
        {
            Debug.Log("=== Door System Setup Verification ===");

            bool allGood = true;

            // Check MazeGenerator
            var mazeGen = GetComponent<MazeGenerator>();
            if (mazeGen == null)
            {
                Debug.LogError("❌ MazeGenerator component missing!");
                allGood = false;
            }
            else
            {
                Debug.Log("✅ MazeGenerator found");
            }

            // Check RoomGenerator
            var roomGen = GetComponent<RoomGenerator>();
            if (roomGen == null)
            {
                Debug.LogError("❌ RoomGenerator component missing!");
                allGood = false;
            }
            else
            {
                Debug.Log("✅ RoomGenerator found");
            }

            // Check DoorHolePlacer (legacy component)
            var holePlacer = GetComponent<DoorHolePlacer>();
            if (holePlacer == null)
            {
                Debug.LogError("❌ DoorHolePlacer component missing! Add it to place door holes.");
                allGood = false;
            }
            else
            {
                Debug.Log("✅ DoorHolePlacer found (legacy)");
                // Legacy property checks commented out - component is deprecated
                // if (holePlacer.DoorChancePerWall <= 0f) ...
            }

            // Check RoomDoorPlacer (legacy component)
            var doorPlacer = GetComponent<RoomDoorPlacer>();
            if (doorPlacer == null)
            {
                Debug.LogError("❌ RoomDoorPlacer component missing! Add it to place doors.");
                allGood = false;
            }
            else
            {
                Debug.Log("✅ RoomDoorPlacer found (legacy)");
                // Legacy property checks commented out - component is deprecated
                // if (doorPlacer.AvailableVariants == null ...) ...
            }

            // Check MazeRenderer (deprecated - geometry handled by CompleteMazeBuilder)
            // var mazeRenderer = GetComponent<IMazeRenderer>();  // ❌ DEPRECATED
            // if (mazeRenderer == null) { ... }  // ❌ Removed - no longer needed
            Debug.Log("⚠️ MazeRenderer check skipped (deprecated - use CompleteMazeBuilder)");

            Debug.Log("=== Verification Complete ===");

            if (allGood)
            {
                Debug.Log("✅ All required components are present!");
                Debug.Log("▶️ Press Play to generate maze with rooms and doors");
            }
            else
            {
                Debug.Log("❌ Some components are missing. Add them before pressing Play.");
            }
        }

        [ContextMenu("Add Missing Components")]
        public void AddMissingComponents()
        {
            Debug.Log("=== Adding Missing Components ===");

            if (GetComponent<MazeGenerator>() == null)
            {
                gameObject.AddComponent<MazeGenerator>();
                Debug.Log("✅ Added MazeGenerator");
            }

            if (GetComponent<RoomGenerator>() == null)
            {
                gameObject.AddComponent<RoomGenerator>();
                Debug.Log("✅ Added RoomGenerator");
            }

            if (GetComponent<DoorHolePlacer>() == null)
            {
                gameObject.AddComponent<DoorHolePlacer>();
                Debug.Log("✅ Added DoorHolePlacer");
            }

            if (GetComponent<RoomDoorPlacer>() == null)
            {
                gameObject.AddComponent<RoomDoorPlacer>();
                Debug.Log("✅ Added RoomDoorPlacer");
            }

            if (GetComponent<IMazeRenderer>() == null)
            {
                // gameObject.AddComponent<MazeRenderer>();  // ❌ DEPRECATED - Geometry handled by CompleteMazeBuilder
                Debug.Log("⚠️ MazeRenderer skipped (deprecated - use CompleteMazeBuilder)");
            }

            Debug.Log("=== All Components Added ===");
            Debug.Log("▶️ Run 'Verify Door System Setup' to configure");
        }

        [ContextMenu("Reset to Default Settings")]
        public void ResetToDefaults()
        {
            // Reset DoorHolePlacer (legacy component)
            var holePlacer = GetComponent<DoorHolePlacer>();
            if (holePlacer != null)
            {
                // Legacy properties - component is deprecated
                // holePlacer.HoleWidth = 2.5f;
                // holePlacer.HoleHeight = 3f;
                // holePlacer.HoleDepth = 0.5f;
                // holePlacer.DoorChancePerWall = 0.6f;
                // holePlacer.CarveHolesInWalls = true;
                Debug.Log("✅ DoorHolePlacer reset to defaults (legacy)");
            }

            // Reset RoomDoorPlacer (legacy component)
            var doorPlacer = GetComponent<RoomDoorPlacer>();
            if (doorPlacer != null)
            {
                // Legacy properties - component is deprecated
                // doorPlacer.PlaceDoorsInHoles = true;
                // doorPlacer.RandomizeWallTextures = true;
                Debug.Log("✅ RoomDoorPlacer reset to defaults (legacy)");
            }

            Debug.Log("=== Legacy Settings Reset ===");
        }
    }
}
#endif
