// DeleteBinaryFiles.cs
// Editor tool to delete old binary files
// Place in: Assets/Editor/DeleteBinaryFiles.cs

using UnityEngine;
using UnityEditor;
using System.IO;

public class DeleteBinaryFiles : EditorWindow
{
    [MenuItem("Tools/Delete Binary Files")]
    public static void DeleteFiles()
    {
        string binaryPath = Path.Combine(Application.dataPath, "StreamingWorkFlow/MazeData");
        
        if (Directory.Exists(binaryPath))
        {
            string[] files = Directory.GetFiles(binaryPath, "*.bytes");
            
            if (files.Length == 0)
            {
                Debug.Log("[Delete Binary] No binary files found in StreamingWorkFlow/MazeData/");
                EditorUtility.DisplayDialog("Delete Binary Files", 
                    "No binary files found in StreamingWorkFlow/MazeData/", "OK");
                return;
            }
            
            // Delete all .bytes files
            foreach (string file in files)
            {
                File.Delete(file);
                Debug.Log($"[Delete Binary] Deleted: {Path.GetFileName(file)}");
            }
            
            Debug.Log($"[Delete Binary] Deleted {files.Length} binary file(s)");
            EditorUtility.DisplayDialog("Delete Binary Files", 
                $"Deleted {files.Length} binary file(s) from StreamingWorkFlow/MazeData/\n\nTorches will be recalculated on next Play!", "OK");
        }
        else
        {
            Debug.Log("[Delete Binary] Directory not found: StreamingWorkFlow/MazeData/");
            EditorUtility.DisplayDialog("Delete Binary Files", 
                "Directory not found: StreamingWorkFlow/MazeData/", "OK");
        }
    }
}
