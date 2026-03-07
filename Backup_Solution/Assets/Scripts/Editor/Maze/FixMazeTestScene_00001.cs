// FixMazeTestScene.cs
// Editor script to fix the FpsMazeTest.unity scene configuration
// Place in: Assets/Editor/FixMazeTestScene.cs

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;
using System.Reflection;

public class FixMazeTestScene : EditorWindow
{
    [MenuItem("Tools/Fix MazeTest Scene")]
    public static void FixScene()
    {
        // Find the MazeTest GameObject
        GameObject mazeTest = GameObject.Find("MazeTest");
        
        if (mazeTest == null)
        {
            Debug.LogError("[Fix Scene] MazeTest GameObject not found!");
            return;
        }
        
        Debug.Log("[Fix Scene] Found MazeTest GameObject");
        
        // Get SpatialPlacer
        var spatialPlacer = mazeTest.GetComponent<SpatialPlacer>();
        if (spatialPlacer == null)
        {
            Debug.LogError("[Fix Scene] SpatialPlacer not found!");
            return;
        }
        
        // Get or create LightPlacementEngine
        var lightEngine = mazeTest.GetComponent<LightPlacementEngine>();
        if (lightEngine == null)
        {
            Debug.Log("[Fix Scene] Creating LightPlacementEngine...");
            lightEngine = mazeTest.AddComponent<LightPlacementEngine>();
        }
        
        // Assign the reference using SerializedObject (works with private fields)
        Debug.Log("[Fix Scene] Assigning LightPlacementEngine to SpatialPlacer...");
        var serializedSpatial = new SerializedObject(spatialPlacer);
        var lightEngineField = serializedSpatial.FindProperty("lightPlacementEngine");
        if (lightEngineField != null)
        {
            lightEngineField.objectReferenceValue = lightEngine;
            serializedSpatial.ApplyModifiedProperties();
            Debug.Log("[Fix Scene] ✅ LightPlacementEngine assigned to SpatialPlacer");
        }
        else
        {
            Debug.LogError("[Fix Scene] lightPlacementEngine field not found in SpatialPlacer!");
        }
        
        // Configure LightPlacementEngine using reflection (private fields)
        Debug.Log("[Fix Scene] Configuring LightPlacementEngine...");
        
        // Set torchPrefab
        var torchPrefabField = lightEngine.GetType().GetField("torchPrefab", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (torchPrefabField != null)
        {
            var torchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/TorchHandlePrefab.prefab"
            );
            torchPrefabField.SetValue(lightEngine, torchPrefab);
            Debug.Log("[Fix Scene] ✅ Torch prefab assigned");
        }
        
        // Set startOn to true
        var startOnField = lightEngine.GetType().GetField("startOn", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (startOnField != null)
        {
            startOnField.SetValue(lightEngine, true);
            Debug.Log("[Fix Scene] ✅ startOn set to true (all torches ON)");
        }
        
        // Get or create MazePlacementEngine
        var mazeEngine = mazeTest.GetComponent<MazePlacementEngine>();
        if (mazeEngine == null)
        {
            Debug.Log("[Fix Scene] Creating MazePlacementEngine...");
            mazeEngine = mazeTest.AddComponent<MazePlacementEngine>();
        }
        
        // Save changes
        EditorUtility.SetDirty(mazeTest);
        EditorUtility.SetDirty(lightEngine);
        EditorUtility.SetDirty(mazeEngine);
        
        Debug.Log("[Fix Scene] ✅ Scene fixed successfully!");
        Debug.Log("[Fix Scene] Components on MazeTest:");
        Debug.Log($"  - SpatialPlacer: ✅");
        Debug.Log($"  - LightPlacementEngine: ✅");
        Debug.Log($"  - MazePlacementEngine: ✅");
        Debug.Log($"  - lightPlacementEngine assigned: {(spatialPlacer.GetComponent<LightPlacementEngine>() != null ? "✅" : "❌")}");
    }
}
