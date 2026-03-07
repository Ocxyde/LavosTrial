// ReorganizeEditorScripts.cs
// Editor script to organize tool scripts into subfolders by category
// Unity 6 compatible - UTF-8 encoding - Unix line endings
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
    public class ReorganizeEditorScripts
    {
        [MenuItem("Tools/Reorganize Editor Scripts")]
        public static void Reorganize()
        {
            Debug.Log("[ReorganizeEditor] ════════════════════════════════════════");
            Debug.Log("[ReorganizeEditor] 📁 Organizing editor scripts...");
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
            Debug.Log("[ReorganizeEditor] ✅ Organization complete!");
            Debug.Log("[ReorganizeEditor] ════════════════════════════════════════");
            Debug.Log("[ReorganizeEditor] 📁 New structure:");
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
                Debug.Log($"[ReorganizeEditor] 📁 Created: {path}");
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
                    Debug.Log($"[ReorganizeEditor] ✓ {filename} already in {Path.GetFileName(destFolder)}/");
                    return;
                }

                // Move file
                AssetDatabase.MoveAsset(sourcePath, destPath);
                Debug.Log($"[ReorganizeEditor] 📄 Moved: {filename} → {Path.GetFileName(destFolder)}/");

                // Move meta file if exists
                if (File.Exists(metaPath))
                {
                    AssetDatabase.MoveAsset(metaPath, $"{destPath}.meta");
                }
            }
            else
            {
                Debug.LogWarning($"[ReorganizeEditor] ⚠️ Not found: {filename}");
            }
        }
    }
}
