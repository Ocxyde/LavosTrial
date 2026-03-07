// AutoFixMazeTest.cs
// One-click fix for FpsMazeTest missing components
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Open Unity Editor
//   2. Tools → Auto-Fix MazeTest Setup
//   3. Everything is created and configured automatically

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    public class AutoFixMazeTest
    {
        [MenuItem("Tools/Auto-Fix MazeTest Setup")]
        public static void Fix()
        {
            Debug.Log("[AutoFix] Starting FpsMazeTest setup...");

            // Find or create GameObject
            var fpsMazeTest = UnityEngine.Object.FindFirstObjectByType<FpsMazeTest>();
            GameObject go;

            if (fpsMazeTest == null)
            {
                go = new GameObject("MazeTest");
                fpsMazeTest = go.AddComponent<FpsMazeTest>();
                Debug.Log("[AutoFix] ✅ Created 'MazeTest' GameObject");
            }
            else
            {
                go = fpsMazeTest.gameObject;
                Debug.Log("[AutoFix] ✓ Found existing FpsMazeTest");
            }

            // Add all required components
            AddComponent<MazeGenerator>(go);
            AddComponent<MazeRenderer>(go);
            AddComponent<MazeIntegration>(go);
            AddComponent<SpatialPlacer>(go);
            AddComponent<TorchPool>(go);
            AddComponent<LightPlacementEngine>(go);

            // LightEngine (singleton, check if exists in scene)
            var lightEngine = UnityEngine.Object.FindFirstObjectByType<LightEngine>();
            if (lightEngine == null)
            {
                var lightGO = new GameObject("LightEngine");
                lightGO.AddComponent<LightEngine>();
                Debug.Log("[AutoFix] ✅ Created LightEngine");
            }
            else
            {
                Debug.Log("[AutoFix] ✓ LightEngine already exists");
            }

            // Configure FpsMazeTest
            fpsMazeTest.GetType().GetField("mazeWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(fpsMazeTest, 21);
            fpsMazeTest.GetType().GetField("mazeHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(fpsMazeTest, 21);

            Debug.Log("[AutoFix] ✅ Configured maze size: 21x21");

            // Configure MazeIntegration
            var mazeIntegration = go.GetComponent<MazeIntegration>();
            if (mazeIntegration != null)
            {
                var miType = mazeIntegration.GetType();
                miType.GetField("mazeWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(mazeIntegration, 21);
                miType.GetField("mazeHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(mazeIntegration, 21);
                Debug.Log("[AutoFix] ✅ Configured MazeIntegration");
            }

            Debug.Log("[AutoFix]");
            Debug.Log("[AutoFix] ════════════════════════════════════════");
            Debug.Log("[AutoFix]  ✅ SETUP COMPLETE!");
            Debug.Log("[AutoFix] ════════════════════════════════════════");
            Debug.Log("[AutoFix]  Components added to '" + go.name + "':");
            Debug.Log("[AutoFix]    • FpsMazeTest");
            Debug.Log("[AutoFix]    • MazeGenerator");
            Debug.Log("[AutoFix]    • MazeRenderer");
            Debug.Log("[AutoFix]    • MazeIntegration");
            Debug.Log("[AutoFix]    • SpatialPlacer");
            Debug.Log("[AutoFix]    • TorchPool");
            Debug.Log("[AutoFix]    • LightPlacementEngine");
            Debug.Log("[AutoFix]    • LightEngine (separate GameObject)");
            Debug.Log("[AutoFix] ════════════════════════════════════════");
            Debug.Log("[AutoFix]  Press Play to test!");
            Debug.Log("[AutoFix] ════════════════════════════════════════");
        }

        private static void AddComponent<T>(GameObject go) where T : Component
        {
            if (go.GetComponent<T>() == null)
            {
                go.AddComponent<T>();
                Debug.Log($"[AutoFix] ✅ Added {typeof(T).Name}");
            }
            else
            {
                Debug.Log($"[AutoFix] ✓ {typeof(T).Name} already exists");
            }
        }
    }
}
