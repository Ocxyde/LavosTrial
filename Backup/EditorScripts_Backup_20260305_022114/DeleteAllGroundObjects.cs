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
// DeleteAllGroundObjects.cs
// Editor tool to delete ALL ground-like objects from scene
// Place in: Assets/Editor/DeleteAllGroundObjects.cs
//
// ️  OBSOLETE - Replaced by CompleteMazeBuilder cleanup system
// This file is marked for deletion. Run:
//   .\cleanup_obsolete_editor_scripts.ps1

using UnityEngine;
using UnityEditor;
using System.Linq;

[System.Obsolete("DeleteAllGroundObjects is deprecated. CompleteMazeBuilder handles cleanup automatically. This file will be deleted by cleanup_obsolete_editor_scripts.ps1")]
public class DeleteAllGroundObjects : EditorWindow
{
    [MenuItem("Tools/Delete ALL Ground Objects (Nuclear Option)")]
    public static void DeleteAll()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "️ NUCLEAR CLEANUP ️",
            "This will delete ALL objects with names containing:\n\n" +
            "- Ground\n- Plane\n- Quad\n- Floor\n- Surface\n\n" +
            "This CANNOT be undone!\n\n" +
            "Continue?",
            "Yes, Delete Everything!",
            "Cancel"
        );
        
        if (!confirmed) return;
        
        Debug.Log("═══════════════════════════════════════════");
        Debug.Log("  NUCLEAR GROUND CLEANUP");
        Debug.Log("═══════════════════════════════════════════");
        
        int deletedCount = 0;
        
        // Find ALL objects in scene
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        string[] keywords = { "ground", "plane", "quad", "floor", "surface" };
        
        foreach (var obj in allObjects)
        {
            string objName = obj.name.ToLower();
            
            // Check if name contains any keyword
            foreach (string keyword in keywords)
            {
                if (objName.Contains(keyword))
                {
                    // Check if it has a MeshRenderer with a material
                    var renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        // Destroy material first
                        if (renderer.sharedMaterial != null)
                        {
                            Debug.Log($"[Nuclear] Destroying material: {renderer.sharedMaterial.name} on {obj.name}");
                            if (Application.isPlaying)
                                Object.Destroy(renderer.sharedMaterial);
                            else
                                Object.DestroyImmediate(renderer.sharedMaterial);
                        }
                    }
                    
                    // Destroy the object
                    Debug.Log($"[Nuclear] ️ Deleting: {obj.name}");
                    if (Application.isPlaying)
                        Object.Destroy(obj);
                    else
                        Object.DestroyImmediate(obj);
                    
                    deletedCount++;
                    break; // Don't delete twice
                }
            }
        }
        
        // Clear cache
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        
        Debug.Log("═══════════════════════════════════════════");
        Debug.Log($"[Nuclear]  Deleted {deletedCount} ground objects!");
        Debug.Log("═══════════════════════════════════════════");
        
        EditorUtility.DisplayDialog("Nuclear Cleanup Complete", 
            $"Deleted {deletedCount} ground-like objects.\n\n" +
            "Save scene and press Play for FRESH ground!", "OK");
    }
}
