// ClearShaderCache.cs
// Unity Editor script to clear shader cache and fix compilation errors
// Place in: Assets/Editor/ClearShaderCache.cs
// Then: Tools  Clear Shader Cache

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    public static class ClearShaderCache
    {
        [MenuItem("Tools/Clear Shader Cache")]
        public static void ClearShaderCacheMenu()
        {
            Debug.Log("[ClearShaderCache] Starting shader cache cleanup...");

            // Stop any compilation
            EditorApplication.isPlaying = false;

            // Clear shader cache
            Shader.WarmupAllShaders();

            // Delete Library/ShaderCache folder
            string shaderCachePath = Path.Combine(Application.dataPath, "..", "Library", "ShaderCache");
            if (Directory.Exists(shaderCachePath))
            {
                try
                {
                    Directory.Delete(shaderCachePath, true);
                    Debug.Log($"[ClearShaderCache] Deleted: {shaderCachePath}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ClearShaderCache] Could not delete shader cache: {e.Message}");
                }
            }

            // Delete Library/PackageCache (will be re-imported)
            string packageCachePath = Path.Combine(Application.dataPath, "..", "Library", "PackageCache");
            if (Directory.Exists(packageCachePath))
            {
                try
                {
                    // Only delete shader graph related folders
                    string[] shaderGraphFolders = Directory.GetDirectories(packageCachePath, "*shadergraph*");
                    foreach (string folder in shaderGraphFolders)
                    {
                        Directory.Delete(folder, true);
                        Debug.Log($"[ClearShaderCache] Deleted: {folder}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ClearShaderCache] Could not clear package cache: {e.Message}");
                }
            }

            // Clear script compilation cache
            EditorUtility.RequestScriptReload();

            Debug.Log("[ClearShaderCache] Shader cache cleared! Scripts will recompile...");
            Debug.Log("[ClearShaderCache] Please wait for Unity to reimport assets.");
        }

        [MenuItem("Tools/Clear All Library Cache")]
        public static void ClearAllLibraryCache()
        {
            if (EditorUtility.DisplayDialog(
                "Clear All Library Cache?",
                "This will delete the entire Library folder and force a full reimport.\n\n" +
                "This may take several minutes.\n\n" +
                "Are you sure?",
                "Yes, Clear Everything",
                "Cancel"))
            {
                Debug.Log("[ClearLibraryCache] Starting full library cleanup...");

                EditorApplication.isPlaying = false;

                string libraryPath = Path.Combine(Application.dataPath, "..", "Library");
                if (Directory.Exists(libraryPath))
                {
                    try
                    {
                        Directory.Delete(libraryPath, true);
                        Debug.Log("[ClearLibraryCache] Library folder deleted.");
                        Debug.Log("[ClearLibraryCache] Please restart Unity to reimport everything.");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[ClearLibraryCache] Could not delete Library: {e.Message}");
                    }
                }
            }
        }
    }
}
