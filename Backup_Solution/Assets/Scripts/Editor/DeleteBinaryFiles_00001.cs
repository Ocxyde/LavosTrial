// DeleteBinaryFiles.cs
// Editor tool to delete old binary files and clean old ground/ceiling
// Place in: Assets/Editor/DeleteBinaryFiles.cs

using UnityEngine;
using UnityEditor;
using System.IO;

public class DeleteBinaryFiles : EditorWindow
{
    [MenuItem("Tools/Delete Binary Files & Clean Scene")]
    public static void DeleteFiles()
    {
        int deletedCount = 0;
        
        // 1. Delete binary files
        string binaryPath = Path.Combine(Application.dataPath, "StreamingWorkFlow/MazeData");
        
        if (Directory.Exists(binaryPath))
        {
            string[] files = Directory.GetFiles(binaryPath, "*.bytes");
            
            if (files.Length == 0)
            {
                Debug.Log("[Clean] No binary files found");
            }
            else
            {
                foreach (string file in files)
                {
                    File.Delete(file);
                    Debug.Log($"[Clean] Deleted binary: {Path.GetFileName(file)}");
                    deletedCount++;
                }
            }
        }
        else
        {
            Debug.Log("[Clean] Binary directory not found (will be created on first run)");
        }
        
        // 2. Delete old ground/ceiling from scene
        GameObject oldGround = GameObject.Find("GroundPlane");
        if (oldGround != null)
        {
            Debug.Log("[Clean] Found old GroundPlane in scene - DELETING");
            GameObject.DestroyImmediate(oldGround);
            deletedCount++;
        }
        
        GameObject oldCeiling = GameObject.Find("CeilingPlane");
        if (oldCeiling != null)
        {
            Debug.Log("[Clean] Found old CeilingPlane in scene - DELETING");
            GameObject.DestroyImmediate(oldCeiling);
            deletedCount++;
        }
        
        // 3. Find any quad/plane objects that might be old ground
        MeshRenderer[] allRenderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        foreach (var renderer in allRenderers)
        {
            if (renderer.gameObject.name.Contains("Plane") || renderer.gameObject.name.Contains("Quad"))
            {
                if (renderer.gameObject.GetComponent<MeshFilter>() != null)
                {
                    string meshName = renderer.gameObject.GetComponent<MeshFilter>().sharedMesh.name;
                    if (meshName.Contains("Plane") || meshName.Contains("Quad"))
                    {
                        Debug.Log($"[Clean] Found old quad/plane: {renderer.gameObject.name} - DELETING");
                        GameObject.DestroyImmediate(renderer.gameObject);
                        deletedCount++;
                    }
                }
            }
        }
        
        Debug.Log($"═══════════════════════════════════════════");
        Debug.Log($"[Clean] ✅ Cleanup complete!");
        Debug.Log($"[Clean] Deleted {deletedCount} object(s)/file(s)");
        Debug.Log($"═══════════════════════════════════════════");
        
        EditorUtility.DisplayDialog("Cleanup Complete", 
            $"Deleted {deletedCount} object(s)/file(s):\n\n" +
            $"- Old binary files\n" +
            $"- Old GroundPlane\n" +
            $"- Old CeilingPlane\n" +
            $"- Old quads/planes\n\n" +
            $"Save scene and press Play for fresh cubes!", "OK");
    }
}
