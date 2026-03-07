using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Simple one-click migration: replace PlayerController references with PlayerCameraController
public class SwitchToPlayerCameraController : EditorWindow
{
    [MenuItem("Tools/Migrate to PlayerCameraController")]
    public static void ShowWindow()
    {
        GetWindow(typeof(SwitchToPlayerCameraController), false, "Migration: PlayerCameraController");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Migrate all PlayerController references to PlayerCameraController"))
        {
            MigrateAll();
        }
        GUILayout.Label("This is a best-effort migration. Review changes in scenes/prefabs.");
    }

    private static void MigrateAll()
    {
        var sceneGuids = Enumerable.Range(0, EditorSceneManager.sceneCount)
            .Select(i => EditorSceneManager.GetSceneAt(i))
            .Where(s => s.isDirty || true)
            .Select(s => AssetDatabase.AssetPathToGUID(s.path))
            .ToList();

        int replacements = 0;
        foreach (var path in AssetDatabase.GetAllAssetPaths())
        {
            if (!path.EndsWith(".unity")) continue;
            bool changed = false;
            var content = System.IO.File.ReadAllText(path);
            if (content.Contains("PlayerController"))
            {
                content = content.Replace("PlayerController", "PlayerCameraController");
                changed = true;
            }
            if (changed)
            {
                System.IO.File.WriteAllText(path, content);
                replacements++;
            }
        }
        AssetDatabase.Refresh();
        Debug.Log("Migration: replaced PlayerController with PlayerCameraController in assets. replacements=" + replacements);
    }
}
