// PurgeOldGround.cs
// Aggressive cleanup of old ground materials and textures
// Place in: Assets/Editor/PurgeOldGround.cs

using UnityEngine;
using UnityEditor;
using System.IO;

public class PurgeOldGround : EditorWindow
{
    [MenuItem("Tools/Purge Old Ground & Materials")]
    public static void PurgeAll()
    {
        int deletedCount = 0;
        
        Debug.Log("═══════════════════════════════════════════");
        Debug.Log("  AGGRESSIVE GROUND CLEANUP");
        Debug.Log("═══════════════════════════════════════════");
        
        // 1. Delete ALL ground/ceiling objects from scene
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj.name.Contains("Ground") || obj.name.Contains("Ceiling") || 
                obj.name.Contains("Plane") || obj.name.Contains("Quad"))
            {
                // Check if it's a primitive
                MeshFilter mf = obj.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    string meshName = mf.sharedMesh.name;
                    if (meshName.Contains("Plane") || meshName.Contains("Quad") || 
                        meshName.Contains("Cube"))
                    {
                        Debug.Log($"[Purge] Deleting: {obj.name} (mesh: {meshName})");
                        DestroyImmediate(obj);
                        deletedCount++;
                    }
                }
            }
        }
        
        // 2. Delete old materials from Resources
        string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
        foreach (string guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Ground") || path.Contains("Ceiling"))
            {
                Debug.Log($"[Purge] Deleting material: {path}");
                AssetDatabase.DeleteAsset(path);
                deletedCount++;
            }
        }
        
        // 3. Delete old textures from Resources
        string[] texGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
        foreach (string guid in texGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Ground") || path.Contains("Ceiling"))
            {
                Debug.Log($"[Purge] Deleting texture: {path}");
                AssetDatabase.DeleteAsset(path);
                deletedCount++;
            }
        }
        
        // 4. Clear binary files
        string binaryPath = Path.Combine(Application.dataPath, "StreamingWorkFlow/MazeData");
        if (Directory.Exists(binaryPath))
        {
            string[] files = Directory.GetFiles(binaryPath, "*.bytes");
            foreach (string file in files)
            {
                File.Delete(file);
                Debug.Log($"[Purge] Deleted binary: {Path.GetFileName(file)}");
                deletedCount++;
            }
        }
        
        // 5. Clear Unity cache
        Resources.UnloadUnusedAssets();
        
        Debug.Log("═══════════════════════════════════════════");
        Debug.Log($"[Purge] ✅ Complete! Deleted {deletedCount} items");
        Debug.Log("═══════════════════════════════════════════");
        
        EditorUtility.DisplayDialog("Purge Complete", 
            $"Deleted {deletedCount} items:\n\n" +
            "- Old ground/ceiling objects\n" +
            "- Old materials\n" +
            "- Old textures\n" +
            "- Binary files\n\n" +
            "Save scene and press Play for FRESH cubes!", "OK");
    }
}
