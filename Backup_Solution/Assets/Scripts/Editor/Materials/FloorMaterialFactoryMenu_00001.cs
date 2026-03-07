// FloorMaterialFactoryMenu.cs
// Editor menu for floor material generation
// Place in: Assets/Editor/FloorMaterialFactoryMenu.cs

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;

public class FloorMaterialFactoryMenu : EditorWindow
{
    [MenuItem("Tools/Floor Materials/Generate All Floor Materials")]
    public static void GenerateAll()
    {
        FloorMaterialFactory.GenerateAllFloorMaterials();
        EditorUtility.DisplayDialog(
            "Floor Materials Generated",
            "✅ All floor materials generated and saved to:\n\n" +
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
