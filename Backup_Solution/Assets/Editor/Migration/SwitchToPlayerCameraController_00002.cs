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
