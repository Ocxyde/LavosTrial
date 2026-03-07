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
// FloorMaterialFactoryMenu.cs
// Editor menu for floor material generation
// Place in: Assets/Editor/FloorMaterialFactoryMenu.cs
//
// ️  OBSOLETE - Use FloorMaterialFactory.GenerateAllFloorMaterials() directly
// This file is marked for deletion. Run:
//   .\cleanup_obsolete_editor_scripts.ps1

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;

[System.Obsolete("FloorMaterialFactoryMenu is deprecated. Use FloorMaterialFactory.GenerateAllFloorMaterials() directly. This file will be deleted by cleanup_obsolete_editor_scripts.ps1")]
public class FloorMaterialFactoryMenu : EditorWindow
{
    [MenuItem("Tools/Floor Materials/Generate All Floor Materials")]
    public static void GenerateAll()
    {
        FloorMaterialFactory.GenerateAllFloorMaterials();
        EditorUtility.DisplayDialog(
            "Floor Materials Generated",
            " All floor materials generated and saved to:\n\n" +
            "Assets/Materials/Floor/\n\n" +
            "Types:\n" +
            "- Stone_Floor.mat\n" +
            "- Wood_Floor.mat\n" +
            "- Tile_Floor.mat\n" +
            "- Brick_Floor.mat\n" +
            "- Marble_Floor.mat\n\n" +
            "You can now assign these materials in MazeRenderer!",
            "OK"
        );
    }
    
    [MenuItem("Tools/Floor Materials/Open Materials Folder")]
    public static void OpenFolder()
    {
        string path = "Assets/Materials/Floor";
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
        EditorUtility.RevealInFinder(path);
    }
    
    [MenuItem("Tools/Floor Materials/Select Stone Floor")]
    public static void SelectStone()
    {
        SelectFloorMaterial(FloorMaterialFactory.FloorType.Stone);
    }
    
    [MenuItem("Tools/Floor Materials/Select Wood Floor")]
    public static void SelectWood()
    {
        SelectFloorMaterial(FloorMaterialFactory.FloorType.Wood);
    }
    
    [MenuItem("Tools/Floor Materials/Select Tile Floor")]
    public static void SelectTile()
    {
        SelectFloorMaterial(FloorMaterialFactory.FloorType.Tile);
    }
    
    private static void SelectFloorMaterial(FloorMaterialFactory.FloorType type)
    {
        string path = $"Assets/Materials/Floor/{type}_Floor.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null)
        {
            Selection.activeObject = mat;
            EditorGUIUtility.PingObject(mat);
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Material Not Found",
                $"Material not found at:\n{path}\n\n" +
                "Generate all materials first via:\n" +
                "Tools → Floor Materials → Generate All",
                "OK"
            );
        }
    }
}
