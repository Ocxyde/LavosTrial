// FixFloorMaterials.cs
// Editor script to fix floor material texture references
// Run from Unity: Menu > Tools > Fix Floor Materials

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// FixFloorMaterials - Editor utility to fix floor material textures.
    /// </summary>
    public class FixFloorMaterials : EditorWindow
    {
        [MenuItem("Tools/Fix Floor Materials")]
        public static void FixMaterials()
        {
            string materialsFolder = "Assets/Materials/Floor";
            string[] floorTypes = { "Stone", "Wood", "Tile", "Brick", "Marble" };
            int fixedCount = 0;

            foreach (string type in floorTypes)
            {
                string matPath = $"{materialsFolder}/{type}_Floor.mat";
                string texPath = $"{materialsFolder}/{type}_Floor_Texture.png";

                // Load texture
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (texture == null)
                {
                    Debug.LogError($"[FixFloorMaterials] Texture not found: {texPath}");
                    continue;
                }

                // Load material
                Material material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (material == null)
                {
                    Debug.LogError($"[FixFloorMaterials] Material not found: {matPath}");
                    continue;
                }

                // Assign texture
                material.mainTexture = texture;
                material.mainTextureScale = new Vector2(1f, 1f);

                // For URP
                material.SetTexture("_BaseMap", texture);
                material.SetTexture("_MainTex", texture);

                EditorUtility.SetDirty(material);
                fixedCount++;

                Debug.Log($"[FixFloorMaterials] ✅ Fixed: {type}_Floor.mat");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FixFloorMaterials] ✅ Fixed {fixedCount} materials!");
            EditorUtility.DisplayDialog("Fix Floor Materials",
                $"Successfully fixed {fixedCount} floor materials!", "OK");
        }
    }
}
