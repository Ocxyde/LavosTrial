// DoorSystemSetup.cs
// Editor helper script to verify and setup door system components
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Usage: Add to Maze GameObject, run VerifySetup() in Context Menu

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

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

            // Check DoorHolePlacer
            var holePlacer = GetComponent<DoorHolePlacer>();
            if (holePlacer == null)
            {
                Debug.LogError("❌ DoorHolePlacer component missing! Add it to place door holes.");
                allGood = false;
            }
            else
            {
                Debug.Log("✅ DoorHolePlacer found");

                // Verify hole settings
                if (holePlacer.DoorChancePerWall <= 0f)
                    Debug.LogWarning("⚠️ Door chance is 0 - no holes will be placed");
                if (holePlacer.HoleWidth < 2f)
                    Debug.LogWarning("⚠️ Hole width may be too small for doors");
                if (holePlacer.HoleHeight < 2.5f)
                    Debug.LogWarning("⚠️ Hole height may be too small for doors");
            }

            // Check RoomDoorPlacer
            var doorPlacer = GetComponent<RoomDoorPlacer>();
            if (doorPlacer == null)
            {
                Debug.LogError("❌ RoomDoorPlacer component missing! Add it to place doors.");
                allGood = false;
            }
            else
            {
                Debug.Log("✅ RoomDoorPlacer found");

                // Verify door settings
                if (doorPlacer.AvailableVariants == null || doorPlacer.AvailableVariants.Length == 0)
                    Debug.LogWarning("⚠️ No door variants assigned - using defaults");
                if (doorPlacer.WallTextureSets == null || doorPlacer.WallTextureSets.Length == 0)
                    Debug.LogWarning("⚠️ No wall texture sets assigned - doors will use default textures");
            }

            // Check MazeRenderer
            var mazeRenderer = GetComponent<IMazeRenderer>();
            if (mazeRenderer == null)
            {
                Debug.LogError("❌ MazeRenderer component missing!");
                allGood = false;
            }
            else
            {
                Debug.Log("✅ MazeRenderer found");
            }

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
                gameObject.AddComponent<MazeRenderer>();
                Debug.Log("✅ Added MazeRenderer");
            }

            Debug.Log("=== All Components Added ===");
            Debug.Log("▶️ Run 'Verify Door System Setup' to configure");
        }

        [ContextMenu("Reset to Default Settings")]
        public void ResetToDefaults()
        {
            var holePlacer = GetComponent<DoorHolePlacer>();
            if (holePlacer != null)
            {
                holePlacer.HoleWidth = 2.5f;
                holePlacer.HoleHeight = 3f;
                holePlacer.HoleDepth = 0.5f;
                holePlacer.DoorChancePerWall = 0.6f;
                holePlacer.CarveHolesInWalls = true;
                Debug.Log("✅ DoorHolePlacer reset to defaults");
            }

            var doorPlacer = GetComponent<RoomDoorPlacer>();
            if (doorPlacer != null)
            {
                doorPlacer.PlaceDoorsInHoles = true;
                doorPlacer.RandomizeWallTextures = true;
                Debug.Log("✅ RoomDoorPlacer reset to defaults");
            }

            Debug.Log("=== Settings Reset ===");
        }
    }
}
#endif
