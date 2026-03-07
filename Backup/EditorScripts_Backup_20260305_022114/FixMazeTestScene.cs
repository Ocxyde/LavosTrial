// Copyright (C) 2026 Ocxyde
//
// This file is part of PeuImporte.
//
// PeuImporte is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeuImporte is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeuImporte.  If not, see <https://www.gnu.org/licenses/>.
// FixMazeTestScene.cs
// Editor script to fix the FpsMazeTest.unity scene configuration
// Place in: Assets/Editor/FixMazeTestScene.cs
//
// ️  OBSOLETE - Replaced by MazeBuilderEditor
// This file is marked for deletion. Run:
//   .\cleanup_obsolete_editor_scripts.ps1

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;
using System.Reflection;

[System.Obsolete("FixMazeTestScene is deprecated. Use MazeBuilderEditor (Tools → Maze → Generate Maze) instead. This file will be deleted by cleanup_obsolete_editor_scripts.ps1")]
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
            Debug.Log("[Fix Scene]  LightPlacementEngine assigned to SpatialPlacer");
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
            Debug.Log("[Fix Scene]  Torch prefab assigned");
        }
        
        // Set startOn to true
        var startOnField = lightEngine.GetType().GetField("startOn", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (startOnField != null)
        {
            startOnField.SetValue(lightEngine, true);
            Debug.Log("[Fix Scene]  startOn set to true (all torches ON)");
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
        
        Debug.Log("[Fix Scene]  Scene fixed successfully!");
        Debug.Log("[Fix Scene] Components on MazeTest:");
        Debug.Log($"  - SpatialPlacer: ");
        Debug.Log($"  - LightPlacementEngine: ");
        Debug.Log($"  - MazePlacementEngine: ");
        Debug.Log($"  - lightPlacementEngine assigned: {(spatialPlacer.GetComponent<LightPlacementEngine>() != null ? "" : "")}");
    }
}
