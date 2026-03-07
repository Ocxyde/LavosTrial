// DoorSystemSetup.cs
// ⚠️ DEPRECATED - LEGACY DOOR SETUP HELPER ⚠️
// Editor helper script to verify and setup LEGACY door system components
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ⚠️ WARNING: This helper uses DEPRECATED components!
// For NEW development, use CompleteMazeBuilder + DoorsEngine + RealisticDoorFactory.
//
// Usage: Add to Maze GameObject, run VerifySetup() in Context Menu

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Suppress obsolete warnings - this file intentionally helps with legacy system setup
#pragma warning disable CS0618

namespace Code.Lavos.Core
{
    /// <summary>
    /// ⚠️ DEPRECATED - DoorSystemSetup - Legacy door setup helper.
    /// For new development, use CompleteMazeBuilder + DoorsEngine + RealisticDoorFactory.
    /// </summary>
    public class DoorSystemSetup : MonoBehaviour
    {
        [ContextMenu("Verify Door System Setup (LEGACY)")]
        public void VerifySetup()
        {
            Debug.Log("=== Verifying LEGACY Door System ===");
            Debug.Log("⚠️  This checks DEPRECATED components!");
            Debug.Log("");

            bool allGood = true;

            // Check MazeGenerator
            if (GetComponent<MazeGenerator>() == null)
            {
                Debug.LogError("❌ MazeGenerator component missing!");
                allGood = false;
            }
            else
            {
                Debug.Log("✅ MazeGenerator found");
            }

            // Check RoomGenerator
            if (GetComponent<RoomGenerator>() == null)
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
                Debug.LogError("❌ DoorHolePlacer component missing!");
                allGood = false;
            }
            else
            {
                Debug.Log("✅ DoorHolePlacer found (legacy)");
            }

            // Check RoomDoorPlacer (legacy component)
            var doorPlacer = GetComponent<RoomDoorPlacer>();
            if (doorPlacer == null)
            {
                Debug.LogError("❌ RoomDoorPlacer component missing!");
                allGood = false;
            }
            else
            {
                Debug.Log("✅ RoomDoorPlacer found (legacy)");
            }

            // MazeRenderer is deprecated - skip check
            Debug.Log("⚠️  MazeRenderer check skipped (deprecated)");

            Debug.Log("=== Verification Complete ===");

            if (allGood)
            {
                Debug.Log("✅ All LEGACY door components present");
                Debug.Log("▶️ Run 'Generate Maze' to create maze with doors");
            }
            else
            {
                Debug.LogError("❌ Some components missing!");
                Debug.Log("💡 Run 'Tools → Create Maze Prefabs' to add components");
            }
        }

        [ContextMenu("Add Missing Components (LEGACY)")]
        public void AddMissingComponents()
        {
            Debug.Log("=== Adding LEGACY Door Components ===");

            int added = 0;

            if (GetComponent<MazeGenerator>() == null)
            {
                gameObject.AddComponent<MazeGenerator>();
                Debug.Log("✅ Added MazeGenerator");
                added++;
            }

            if (GetComponent<RoomGenerator>() == null)
            {
                gameObject.AddComponent<RoomGenerator>();
                Debug.Log("✅ Added RoomGenerator");
                added++;
            }

            if (GetComponent<DoorHolePlacer>() == null)
            {
                gameObject.AddComponent<DoorHolePlacer>();
                Debug.Log("✅ Added DoorHolePlacer");
                added++;
            }

            if (GetComponent<RoomDoorPlacer>() == null)
            {
                gameObject.AddComponent<RoomDoorPlacer>();
                Debug.Log("✅ Added RoomDoorPlacer");
                added++;
            }

            // MazeRenderer is deprecated - skip
            Debug.Log("⚠️  MazeRenderer skipped (deprecated)");

            if (GetComponent<MazeIntegration>() == null)
            {
                gameObject.AddComponent<MazeIntegration>();
                Debug.Log("✅ Added MazeIntegration");
                added++;
            }

            Debug.Log("=== Components Added ===");
            Debug.Log($"✅ Added {added} components");
            Debug.Log("⚠️  These are LEGACY components");
            Debug.Log("💡 For new projects, use CompleteMazeBuilder");
        }

        [ContextMenu("Reset to Default Settings (LEGACY)")]
        public void ResetToDefaults()
        {
            Debug.Log("=== Resetting LEGACY Door Settings ===");

            // Reset DoorHolePlacer (legacy component)
            var holePlacer = GetComponent<DoorHolePlacer>();
            if (holePlacer != null)
            {
                // Legacy properties - component is deprecated
                Debug.Log("✅ DoorHolePlacer reset to defaults (legacy)");
            }

            // Reset RoomDoorPlacer (legacy component)
            var doorPlacer = GetComponent<RoomDoorPlacer>();
            if (doorPlacer != null)
            {
                // Legacy properties - component is deprecated
                Debug.Log("✅ RoomDoorPlacer reset to defaults (legacy)");
            }

            Debug.Log("=== LEGACY Settings Reset ===");
            Debug.Log("⚠️  Consider migrating to CompleteMazeBuilder for new projects");
        }
    }
}
#endif
