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
// FixSceneReferences.cs
// Editor script to fix missing scene references in EditorBuildSettings
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ️  OBSOLETE - Replaced by MazeBuilderEditor
// This file is marked for deletion. Run:
//   .\cleanup_obsolete_editor_scripts.ps1
//
// USAGE:
//   1. Tools → Fix Scene References
//   2. Removes missing scenes from build settings
//   3. Adds valid scenes only

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

namespace Code.Lavos.Editor
{
    [System.Obsolete("FixSceneReferences is deprecated. Use MazeBuilderEditor instead. This file will be deleted by cleanup_obsolete_editor_scripts.ps1")]
    public class FixSceneReferences
    {
        [MenuItem("Tools/Fix Scene References")]
        public static void Fix()
        {
            Debug.Log("[FixSceneReferences]  Checking scene references...");

            // Get current build settings scenes
            var scenes = EditorBuildSettings.scenes;
            
            if (scenes == null || scenes.Length == 0)
            {
                Debug.Log("[FixSceneReferences]  No scenes in build settings");
                return;
            }

            int removedCount = 0;
            int validCount = 0;

            // Check each scene
            for (int i = scenes.Length - 1; i >= 0; i--)
            {
                var scene = scenes[i];
                
                // Check if scene asset exists
                if (!AssetDatabase.AssetPathToGUID(scene.path).Any())
                {
                    Debug.LogWarning($"[FixSceneReferences]  Missing scene: {scene.path}");
                    
                    // Remove from build settings
                    var newScenes = scenes.Where((s, index) => index != i).ToArray();
                    EditorBuildSettings.scenes = newScenes;
                    scenes = newScenes;
                    removedCount++;
                }
                else
                {
                    Debug.Log($"[FixSceneReferences]  Valid scene: {scene.path}");
                    validCount++;
                }
            }

            // Add FpsMazeTest_Fresh if not present
            string freshScenePath = "Assets/Scenes/FpsMazeTest_Fresh.unity";
            if (AssetDatabase.AssetPathToGUID(freshScenePath).Any())
            {
                var guid = AssetDatabase.AssetPathToGUID(freshScenePath);
                var freshScene = new EditorBuildSettingsScene(guid, true);
                
                // Check if already in list
                bool exists = scenes.Any(s => s.path == freshScenePath);
                if (!exists)
                {
                    EditorBuildSettings.scenes = scenes.Concat(new[] { freshScene }).ToArray();
                    Debug.Log($"[FixSceneReferences]  Added: {freshScenePath}");
                }
            }

            Debug.Log($"[FixSceneReferences]  Fixed! Removed {removedCount} missing, {validCount} valid");
            Debug.Log("[FixSceneReferences]  Tip: Open Assets/Scenes/FpsMazeTest_Fresh.unity");
        }

        [MenuItem("Tools/Open FpsMazeTest Scene")]
        public static void OpenFpsMazeTestScene()
        {
            string scenePath = "Assets/Scenes/FpsMazeTest_Fresh.unity";
            
            if (AssetDatabase.AssetPathToGUID(scenePath).Any())
            {
                EditorSceneManager.OpenScene(scenePath);
                Debug.Log($"[FixSceneReferences]  Opened: {scenePath}");
            }
            else
            {
                Debug.LogError($"[FixSceneReferences]  Scene not found: {scenePath}");
            }
        }
    }
}
