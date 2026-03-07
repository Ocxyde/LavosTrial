// AddFpsMazeTestComponents.cs
// Editor script to add all required components to FpsMazeTest scene
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Select the GameObject with FpsMazeTest component
//   2. Tools → Add FpsMazeTest Components
//   3. All required components will be added automatically

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    public class AddFpsMazeTestComponents : EditorWindow
    {
        [MenuItem("Tools/Add FpsMazeTest Components")]
        public static void AddComponents()
        {
            // Find FpsMazeTest in scene
            var fpsMazeTest = UnityEngine.Object.FindFirstObjectByType<FpsMazeTest>();
            if (fpsMazeTest == null)
            {
                Debug.LogError("[Editor] No FpsMazeTest found in scene!");
                Debug.Log("[Editor] Please create a GameObject named 'MazeTest' and add FpsMazeTest component");
                return;
            }

            GameObject go = fpsMazeTest.gameObject;
            int addedCount = 0;

            // Add MazeGenerator
            if (go.GetComponent<MazeGenerator>() == null)
            {
                go.AddComponent<MazeGenerator>();
                Debug.Log("[Editor] ✅ Added MazeGenerator");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor] ✓ MazeGenerator already exists");
            }

            // Add MazeRenderer
            if (go.GetComponent<MazeRenderer>() == null)
            {
                go.AddComponent<MazeRenderer>();
                Debug.Log("[Editor] ✅ Added MazeRenderer");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor] ✓ MazeRenderer already exists");
            }

            // Add MazeIntegration
            if (go.GetComponent<MazeIntegration>() == null)
            {
                go.AddComponent<MazeIntegration>();
                Debug.Log("[Editor] ✅ Added MazeIntegration");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor] ✓ MazeIntegration already exists");
            }

            // Add SpatialPlacer
            if (go.GetComponent<SpatialPlacer>() == null)
            {
                go.AddComponent<SpatialPlacer>();
                Debug.Log("[Editor] ✅ Added SpatialPlacer");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor] ✓ SpatialPlacer already exists");
            }

            // Add TorchPool
            if (go.GetComponent<TorchPool>() == null)
            {
                go.AddComponent<TorchPool>();
                Debug.Log("[Editor] ✅ Added TorchPool");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor] ✓ TorchPool already exists");
            }

            // Add LightPlacementEngine
            if (go.GetComponent<LightPlacementEngine>() == null)
            {
                go.AddComponent<LightPlacementEngine>();
                Debug.Log("[Editor] ✅ Added LightPlacementEngine");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor] ✓ LightPlacementEngine already exists");
            }

            // Add LightEngine
            if (go.GetComponent<LightEngine>() == null)
            {
                go.AddComponent<LightEngine>();
                Debug.Log("[Editor] ✅ Added LightEngine");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor] ✓ LightEngine already exists");
            }

            Debug.Log($"[Editor] Added {addedCount} new components to '{go.name}'");
            Debug.Log("[Editor] Press Play to test!");
        }

        [MenuItem("Tools/Fix FpsMazeTest Scene")]
        public static void FixScene()
        {
            // Create new GameObject if none exists
            var fpsMazeTest = UnityEngine.Object.FindFirstObjectByType<FpsMazeTest>();
            if (fpsMazeTest == null)
            {
                GameObject go = new GameObject("MazeTest");
                fpsMazeTest = go.AddComponent<FpsMazeTest>();
                Debug.Log("[Editor] ✅ Created 'MazeTest' GameObject with FpsMazeTest");
            }
            else
            {
                Debug.Log("[Editor] ✓ FpsMazeTest already exists");
            }

            // Add all components
            AddComponents();
        }
    }
}
