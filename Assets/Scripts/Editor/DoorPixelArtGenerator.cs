// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// DoorPixelArtGenerator - Creates 8-bit pixel art door textures.
    /// 
    /// Generates brown wooden door textures with:
    /// - Wood planks (vertical or horizontal)
    /// - Door frame/border
    /// - Metal hinges
    /// - Door handle/knocker
    /// - Wood grain details
    /// 
    /// Usage:
    /// Tools > Generate Door Pixel Art Textures
    /// </summary>
    public class DoorPixelArtGenerator : EditorWindow
    {
        // Texture settings
        private int textureSize = 64;  // 64x64 pixel art
        private int pixelsPerUnit = 16; // For proper scaling
        private string folderPath = "Assets/Textures/Doors";
        
        // Door colors (8-bit palette)
        private Color woodDark = new Color32(101, 67, 33, 255);
        private Color woodMid = new Color32(139, 90, 43, 255);
        private Color woodLight = new Color32(181, 118, 52, 255);
        private Color metalDark = new Color32(64, 64, 64, 255);
        private Color metalLight = new Color32(128, 128, 128, 255);
        private Color frameColor = new Color32(82, 51, 24, 255);

        // Door style
        private enum DoorStyle
        {
            WoodPlanks,
            PanelDoor,
            Reinforced,
            Secret
        }

        private DoorStyle selectedStyle = DoorStyle.WoodPlanks;

        [MenuItem("Tools/Generate Door Pixel Art Textures")]
        public static void ShowWindow()
        {
            var window = GetWindow<DoorPixelArtGenerator>("Door Pixel Art");
            window.minSize = new Vector2(400, 600);
        }

        private void OnGUI()
        {
            GUILayout.Label("8-Bit Pixel Art Door Texture Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Style selection
            selectedStyle = (DoorStyle)EditorGUILayout.EnumPopup("Door Style", selectedStyle);
            
            GUILayout.Space(10);

            // Texture preview
            GUILayout.Label("Preview:", EditorStyles.boldLabel);
            Texture2D preview = GeneratePreviewTexture();
            if (preview != null)
            {
                GUILayout.Box(preview, GUILayout.Width(128), GUILayout.Height(128));
            }

            GUILayout.Space(20);

            // Generate buttons
            GUILayout.Label("Generate Textures:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Generate All Door Textures", GUILayout.Height(40)))
            {
                GenerateAllDoorTextures();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Generate Wood Door"))
                GenerateDoorTexture("Door_Wood", woodMid, DoorStyle.WoodPlanks);
            
            if (GUILayout.Button("Generate Locked Door (Metal)"))
                GenerateDoorTexture("Door_Metal", metalLight, DoorStyle.Reinforced);
            
            if (GUILayout.Button("Generate Secret Door"))
                GenerateDoorTexture("Door_Secret", woodDark, DoorStyle.Secret);
            
            if (GUILayout.Button("Generate Exit Door (Stone)"))
                GenerateDoorTexture("Door_Exit", new Color(0.5f, 0.5f, 0.5f), DoorStyle.PanelDoor);

            GUILayout.Space(20);

            // Instructions
            GUILayout.Label("Instructions:", EditorStyles.boldLabel);
            GUILayout.TextArea(
                "1. Select door style\n" +
                "2. Click 'Generate All' or individual buttons\n" +
                "3. Textures saved to Assets/Textures/Doors/\n" +
                "4. Create materials using generated textures\n" +
                "5. Assign materials to door prefabs",
                GUILayout.Height(100)
            );
        }

        private Texture2D GeneratePreviewTexture()
        {
            return GenerateDoorTextureInternal(selectedStyle, woodMid, 64);
        }

        private void GenerateAllDoorTextures()
        {
            // Ensure folder exists
            string folderPath = "Assets/Textures/Doors";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Textures", "Doors");
            }

            // Generate each door type
            GenerateDoorTexture("Door_Wood", woodMid, DoorStyle.WoodPlanks);
            GenerateDoorTexture("Door_Metal", metalLight, DoorStyle.Reinforced);
            GenerateDoorTexture("Door_Secret", woodDark, DoorStyle.Secret);
            GenerateDoorTexture("Door_Exit", new Color(0.5f, 0.5f, 0.5f), DoorStyle.PanelDoor);

            AssetDatabase.Refresh();
            
            Debug.Log("[DoorPixelArtGenerator] All door textures generated!");
            
            EditorUtility.DisplayDialog(
                "Door Textures Generated",
                "All 4 door textures created in Assets/Textures/Doors/\n\n" +
                "Next: Create materials and assign to door prefabs!",
                "OK"
            );
        }

        private void GenerateDoorTexture(string name, Color baseColor, DoorStyle style)
        {
            Texture2D texture = GenerateDoorTextureInternal(style, baseColor, textureSize);
            
            // Save texture
            string path = $"{folderPath}/{name}.png";
            SaveTexture(texture, path);
            
            Debug.Log($"[DoorPixelArtGenerator] Generated: {path}");
        }

        private Texture2D GenerateDoorTextureInternal(DoorStyle style, Color baseColor, int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point; // Pixel art!
            
            // Clear to transparent
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
            
            // Draw door based on style
            switch (style)
            {
                case DoorStyle.WoodPlanks:
                    DrawWoodPlankDoor(texture, size, baseColor);
                    break;
                case DoorStyle.PanelDoor:
                    DrawPanelDoor(texture, size, baseColor);
                    break;
                case DoorStyle.Reinforced:
                    DrawReinforcedDoor(texture, size, baseColor);
                    break;
                case DoorStyle.Secret:
                    DrawSecretDoor(texture, size, baseColor);
                    break;
            }
            
            texture.Apply();
            return texture;
        }

        private void DrawWoodPlankDoor(Texture2D tex, int size, Color baseColor)
        {
            int plankHeight = size / 6;
            
            for (int y = 0; y < size; y++)
            {
                int plankIndex = y / plankHeight;
                bool isGap = (y % plankHeight) == 0 || (y % plankHeight) == plankHeight - 1;
                
                for (int x = 0; x < size; x++)
                {
                    // Door frame (border)
                    if (x < 4 || x >= size - 4 || y < 4 || y >= size - 4)
                    {
                        tex.SetPixel(x, y, frameColor);
                        continue;
                    }
                    
                    // Gap between planks (dark line)
                    if (isGap)
                    {
                        tex.SetPixel(x, y, new Color(baseColor.r * 0.5f, baseColor.g * 0.5f, baseColor.b * 0.5f));
                        continue;
                    }
                    
                    // Wood plank with grain
                    float grain = Mathf.PerlinNoise(x * 0.3f, plankIndex * 2.0f) * 0.2f;
                    Color plankColor = baseColor + new Color(grain, grain, grain);
                    
                    // Add vertical wood grain lines
                    if (Random.value < 0.1f)
                    {
                        plankColor *= 0.8f;
                    }
                    
                    tex.SetPixel(x, y, plankColor);
                }
            }
            
            // Add hinges (metal circles on left side)
            DrawHinges(tex, size, true);
            
            // Add door handle/knocker
            DrawDoorHandle(tex, size);
        }

        private void DrawPanelDoor(Texture2D tex, int size, Color baseColor)
        {
            // Draw frame
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (x < 6 || x >= size - 6 || y < 6 || y >= size - 6)
                    {
                        tex.SetPixel(x, y, frameColor);
                    }
                    else
                    {
                        tex.SetPixel(x, y, baseColor);
                    }
                }
            }
            
            // Draw recessed panels (2x3 grid)
            int panelWidth = (size - 20) / 2;
            int panelHeight = (size - 20) / 3;
            
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 2; col++)
                {
                    int px = 8 + col * (panelWidth + 4);
                    int py = 8 + row * (panelHeight + 4);
                    
                    // Draw recessed panel (darker)
                    for (int y = py; y < py + panelHeight - 2; y++)
                    {
                        for (int x = px; x < px + panelWidth - 2; x++)
                        {
                            tex.SetPixel(x, y, baseColor * 0.8f);
                        }
                    }
                }
            }
            
            // Add hinges and handle
            DrawHinges(tex, size, true);
            DrawDoorHandle(tex, size);
        }

        private void DrawReinforcedDoor(Texture2D tex, int size, Color baseColor)
        {
            // Base wood
            DrawWoodPlankDoor(tex, size, woodMid);
            
            // Add metal bands (horizontal stripes)
            int bandHeight = size / 4;
            for (int band = 1; band < 4; band++)
            {
                int y = band * bandHeight;
                for (int by = y - 2; by <= y + 2; by++)
                {
                    if (by >= 0 && by < size)
                    {
                        for (int x = 6; x < size - 6; x++)
                        {
                            // Metal band with rivets
                            if ((x - 6) % 8 < 2)
                            {
                                tex.SetPixel(x, by, metalDark); // Rivet
                            }
                            else
                            {
                                tex.SetPixel(x, by, metalLight); // Band
                            }
                        }
                    }
                }
            }
        }

        private void DrawSecretDoor(Texture2D tex, int size, Color baseColor)
        {
            // Looks like regular wall/wood but slightly different
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (x < 4 || x >= size - 4 || y < 4 || y >= size - 4)
                    {
                        tex.SetPixel(x, y, frameColor);
                    }
                    else
                    {
                        // Subtle wood pattern (no obvious door features)
                        float noise = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.1f;
                        tex.SetPixel(x, y, baseColor + new Color(noise, noise, noise));
                    }
                }
            }
            
            // Hidden handle (barely visible)
            int handleX = size - 12;
            int handleY = size / 2;
            tex.SetPixel(handleX, handleY, new Color(baseColor.r * 0.7f, baseColor.g * 0.7f, baseColor.b * 0.7f));
        }

        private void DrawHinges(Texture2D tex, int size, bool leftSide)
        {
            int hingeWidth = 6;
            int hingeHeight = 8;
            
            // Top hinge
            int hinge1Y = size / 4;
            // Bottom hinge
            int hinge2Y = 3 * size / 4;
            
            int hingeX = leftSide ? 2 : (size - hingeWidth - 2);
            
            DrawHinge(tex, hingeX, hinge1Y, hingeWidth, hingeHeight);
            DrawHinge(tex, hingeX, hinge2Y, hingeWidth, hingeHeight);
        }

        private void DrawHinge(Texture2D tex, int x, int y, int width, int height)
        {
            for (int hy = y - height/2; hy < y + height/2; hy++)
            {
                for (int hx = x; hx < x + width; hx++)
                {
                    if (hx >= 0 && hx < tex.width && hy >= 0 && hy < tex.height)
                    {
                        // Rivets on hinge
                        if ((hx - x) % 3 == 0 && (hy - y) % 4 == 0)
                        {
                            tex.SetPixel(hx, hy, metalDark);
                        }
                        else
                        {
                            tex.SetPixel(hx, hy, metalLight);
                        }
                    }
                }
            }
        }

        private void DrawDoorHandle(Texture2D tex, int size)
        {
            int handleX = size - 10;
            int handleY = size / 2;
            int handleSize = 4;
            
            // Draw circular handle
            for (int y = handleY - handleSize; y <= handleY + handleSize; y++)
            {
                for (int x = handleX - handleSize; x <= handleX + handleSize; x++)
                {
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    {
                        float dist = Mathf.Sqrt(Mathf.Pow(x - handleX, 2) + Mathf.Pow(y - handleY, 2));
                        if (dist <= handleSize)
                        {
                            tex.SetPixel(x, y, metalLight);
                        }
                        else if (dist <= handleSize + 1)
                        {
                            tex.SetPixel(x, y, metalDark); // Ring
                        }
                    }
                }
            }
        }

        private void SaveTexture(Texture2D texture, string path)
        {
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(path, pngData);
            AssetDatabase.ImportAsset(path);
            
            // Set import settings for pixel art
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.filterMode = FilterMode.Point;
                importer.compressionQuality = 0;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
    }
}
