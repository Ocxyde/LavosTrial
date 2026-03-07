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
// FixFloorMaterials.cs
// Editor script to fix floor material texture references
// Run from Unity: Menu > Tools > Fix Floor Materials
//
// ️  OBSOLETE - FloorMaterialFactory (core) handles this now
// This file is marked for deletion. Run:
//   .\cleanup_obsolete_editor_scripts.ps1

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    [System.Obsolete("FixFloorMaterials is deprecated. FloorMaterialFactory (core) handles material creation. This file will be deleted by cleanup_obsolete_editor_scripts.ps1")]
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

                Debug.Log($"[FixFloorMaterials]  Fixed: {type}_Floor.mat");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FixFloorMaterials]  Fixed {fixedCount} materials!");
            EditorUtility.DisplayDialog("Fix Floor Materials",
                $"Successfully fixed {fixedCount} floor materials!", "OK");
        }
    }
}
