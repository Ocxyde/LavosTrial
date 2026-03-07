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
// ReorganizeEditorScripts.cs
// Editor script to organize tool scripts into subfolders by category
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ️  OBSOLETE - One-time use script (organization already done)
// This file is marked for deletion. Run:
//   .\cleanup_obsolete_editor_scripts.ps1
//
// USAGE:
//   1. Tools → Reorganize Editor Scripts
//   2. Scripts organized into subfolders:
//      - Maze/
//      - Materials/
//      - Cleanup/
//      - Build/
//      - Setup/

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    [System.Obsolete("ReorganizeEditorScripts is a one-time use script. Organization is complete. This file will be deleted by cleanup_obsolete_editor_scripts.ps1")]
    public class ReorganizeEditorScripts
    {
        [MenuItem("Tools/Reorganize Editor Scripts")]
        public static void Reorganize()
        {
            Debug.Log("[ReorganizeEditor] ════════════════════════════════════════");
            Debug.Log("[ReorganizeEditor]  Organizing editor scripts...");
            Debug.Log("[ReorganizeEditor] ════════════════════════════════════════");

            string editorFolder = "Assets/Scripts/Editor";

            // Create subfolders
            string mazeFolder = $"{editorFolder}/Maze";
            string materialsFolder = $"{editorFolder}/Materials";
            string cleanupFolder = $"{editorFolder}/Cleanup";
            string buildFolder = $"{editorFolder}/Build";
            string setupFolder = $"{editorFolder}/Setup";

            EnsureFolder(mazeFolder);
            EnsureFolder(materialsFolder);
            EnsureFolder(cleanupFolder);
            EnsureFolder(buildFolder);
            EnsureFolder(setupFolder);

            // Move scripts to appropriate folders
            MoveScript("CreateMazePrefabs.cs", editorFolder, mazeFolder);
            MoveScript("AutoFixMazeTest.cs", editorFolder, mazeFolder);
            MoveScript("AddFpsMazeTestComponents.cs", editorFolder, mazeFolder);
            MoveScript("CreateFreshMazeTestScene.cs", editorFolder, mazeFolder);
            MoveScript("QuickSceneSetup.cs", editorFolder, mazeFolder);
            MoveScript("FixMazeTestScene.cs", editorFolder, mazeFolder);
            MoveScript("FixSceneReferences.cs", editorFolder, mazeFolder);

            MoveScript("FixFloorMaterials.cs", editorFolder, materialsFolder);
            MoveScript("FixSceneTexturesAndPrefabs.cs", editorFolder, materialsFolder);
            MoveScript("FloorMaterialFactoryMenu.cs", editorFolder, materialsFolder);

            MoveScript("DeleteAllGroundObjects.cs", editorFolder, cleanupFolder);
            MoveScript("PurgeOldGround.cs", editorFolder, cleanupFolder);
            MoveScript("DeleteBinaryFiles.cs", editorFolder, cleanupFolder);

            MoveScript("BuildScript.cs", editorFolder, buildFolder);

            MoveScript("AddDoorSystemToScene.cs", editorFolder, setupFolder);
            MoveScript("URPSetupUtility.cs", editorFolder, setupFolder);

            // Keep these in root Editor folder (general purpose)
            // - CreateMazePrefabs.cs (already in Maze/)
            // - FixSceneReferences.cs (already in Maze/)

            Debug.Log("[ReorganizeEditor] ════════════════════════════════════════");
            Debug.Log("[ReorganizeEditor]  Organization complete!");
            Debug.Log("[ReorganizeEditor] ════════════════════════════════════════");
            Debug.Log("[ReorganizeEditor]  New structure:");
            Debug.Log("[ReorganizeEditor]   Assets/Scripts/Editor/");
            Debug.Log("[ReorganizeEditor]     ├── Maze/ (7 scripts)");
            Debug.Log("[ReorganizeEditor]     ├── Materials/ (3 scripts)");
            Debug.Log("[ReorganizeEditor]     ├── Cleanup/ (3 scripts)");
            Debug.Log("[ReorganizeEditor]     ├── Build/ (1 script)");
            Debug.Log("[ReorganizeEditor]     ├── Setup/ (2 scripts)");
            Debug.Log("[ReorganizeEditor]     └── (general scripts)");
            Debug.Log("[ReorganizeEditor] ════════════════════════════════════════");

            AssetDatabase.Refresh();
        }

        private static void EnsureFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"[ReorganizeEditor]  Created: {path}");
            }
        }

        private static void MoveScript(string filename, string sourceFolder, string destFolder)
        {
            string sourcePath = $"{sourceFolder}/{filename}";
            string destPath = $"{destFolder}/{filename}";
            string metaPath = $"{sourcePath}.meta";

            if (File.Exists(sourcePath))
            {
                // Check if already moved
                if (File.Exists(destPath))
                {
                    Debug.Log($"[ReorganizeEditor]  {filename} already in {Path.GetFileName(destFolder)}/");
                    return;
                }

                // Move file
                AssetDatabase.MoveAsset(sourcePath, destPath);
                Debug.Log($"[ReorganizeEditor]  Moved: {filename} → {Path.GetFileName(destFolder)}/");

                // Move meta file if exists
                if (File.Exists(metaPath))
                {
                    AssetDatabase.MoveAsset(metaPath, $"{destPath}.meta");
                }
            }
            else
            {
                Debug.LogWarning($"[ReorganizeEditor] ️ Not found: {filename}");
            }
        }
    }
}
