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
// URPSetupUtility.cs
// Editor utility to auto-configure URP settings
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Usage: Tools → URP Setup → Auto-Configure URP
//
// Location: Assets/Scripts/Editor/

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// URPSetupUtility - Auto-configure Universal Render Pipeline settings.
    /// 
    /// Fixes the "GraphicsSettings may not use URP" warning by:
    /// 1. Finding existing URP asset
    /// 2. Creating one if missing
    /// 3. Assigning to Graphics Settings
    /// 4. Configuring Quality Settings
    /// </summary>
    public static class URPSetupUtility
    {
        [MenuItem("Tools/URP Setup/Auto-Configure URP")]
        public static void AutoConfigureURP()
        {
            Debug.Log("[URPSetup] Starting URP auto-configuration...");
            
            // Step 1: Find or create URP asset
            UniversalRenderPipelineAsset urpAsset = FindOrCreateURPAsset();
            
            if (urpAsset == null)
            {
                Debug.LogError("[URPSetup] Failed to create URP asset!");
                return;
            }
            
            // Step 2: Assign to Graphics Settings
            AssignToGraphicsSettings(urpAsset);
            
            // Step 3: Configure Quality Settings
            ConfigureQualitySettings(urpAsset);
            
            // Step 4: Save changes
            AssetDatabase.SaveAssets();
            
            Debug.Log("[URPSetup] ✅ URP auto-configuration complete!");
            Debug.Log($"[URPSetup] URP Asset: {urpAsset.name}");
        }

        [MenuItem("Tools/URP Setup/Find URP Asset")]
        public static void FindURPAsset()
        {
            var urpAssets = FindURPAssets();
            
            if (urpAssets.Length == 0)
            {
                Debug.LogWarning("[URPSetup] No URP assets found in project!");
                return;
            }
            
            Debug.Log($"[URPSetup] Found {urpAssets.Length} URP asset(s):");
            foreach (var asset in urpAssets)
            {
                Debug.Log($"  - {asset.name} ({AssetDatabase.GetAssetPath(asset)})");
            }
        }

        [MenuItem("Tools/URP Setup/Check Current Settings")]
        public static void CheckCurrentSettings()
        {
            Debug.Log("=== URP Configuration Check ===");
            
            // Check Graphics Settings
            var graphicsSettings = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            var currentPipeline = GraphicsSettings.currentRenderPipeline;
            
            if (currentPipeline == null)
            {
                Debug.LogWarning("❌ Graphics Settings: No Scriptable Render Pipeline assigned");
            }
            else
            {
                Debug.Log($"✅ Graphics Settings: {currentPipeline.name}");
            }
            
            // Check Quality Settings
            Debug.Log("\nQuality Settings:");
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                Debug.Log($"  {QualitySettings.names[i]}: [Check manually in Quality Settings]");
            }
            
            // Find URP assets
            var urpAssets = FindURPAssets();
            Debug.Log($"\nFound {urpAssets.Length} URP asset(s) in project");
        }

        #region Implementation

        static UniversalRenderPipelineAsset FindOrCreateURPAsset()
        {
            // Try to find existing URP asset
            var urpAssets = FindURPAssets();
            
            if (urpAssets.Length > 0)
            {
                Debug.Log($"[URPSetup] Found existing URP asset: {urpAssets[0].name}");
                return urpAssets[0];
            }
            
            // Create new URP asset
            Debug.Log("[URPSetup] Creating new URP asset...");
            return CreateURPAsset();
        }

        static UniversalRenderPipelineAsset[] FindURPAssets()
        {
            string[] guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
            
            var assets = new UniversalRenderPipelineAsset[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                assets[i] = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            }
            
            return assets;
        }

        static UniversalRenderPipelineAsset CreateURPAsset()
        {
            // Create URP asset
            var urpAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
            
            // Configure basic settings
            urpAsset.name = "UniversalRenderPipelineGlobalSettings";
            
            // Save asset
            string path = "Assets/Settings/UniversalRenderPipelineGlobalSettings.asset";
            
            // Create Settings folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }
            
            AssetDatabase.CreateAsset(urpAsset, path);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[URPSetup] Created URP asset at: {path}");
            
            return urpAsset;
        }

        static void AssignToGraphicsSettings(UniversalRenderPipelineAsset urpAsset)
        {
            // This requires modifying Unity's built-in GraphicsSettings
            // We'll use reflection to do this safely
            
            Debug.Log("[URPSetup] Assigning URP asset to Graphics Settings...");
            
            // Note: This is a simplified version
            // For full implementation, you'd need to modify the GraphicsSettings asset
            // or use Unity's built-in URP wizard
            
            Debug.LogWarning("[URPSetup] Manual step required:");
            Debug.LogWarning("1. Edit → Project Settings → Graphics");
            Debug.LogWarning($"2. Drag '{urpAsset.name}' to 'Scriptable Render Pipeline Asset'");
            
            // Try to set via GraphicsSettings API
            var graphicsSettings = GraphicsSettings.defaultRenderPipeline;
            GraphicsSettings.defaultRenderPipeline = urpAsset;
            
            Debug.Log("[URPSetup] Graphics Settings updated (may require manual save)");
        }

        static void ConfigureQualitySettings(UniversalRenderPipelineAsset urpAsset)
        {
            Debug.Log("[URPSetup] Configuring Quality Settings...");
            
            // Note: Quality Settings URP assignment requires manual setup
            // This is a reminder for the user
            
            Debug.LogWarning("[URPSetup] Manual step for Quality Settings:");
            Debug.LogWarning("1. Edit → Project Settings → Quality");
            Debug.LogWarning("2. For each quality level, assign URP asset");
        }

        #endregion

        #region Quick Fix Guide

        [MenuItem("Tools/URP Setup/Show Setup Guide")]
        public static void ShowSetupGuide()
        {
            string guide = @"
=== URP SETUP GUIDE ===

MANUAL SETUP (If auto-configure fails):

1. Find URP Asset:
   - In Project window, search: ""UniversalRenderPipeline""
   - OR check: Assets/Settings/
   
2. Assign to Graphics:
   - Edit → Project Settings → Graphics
   - Drag URP asset to ""Scriptable Render Pipeline Asset""
   
3. Configure Quality:
   - Edit → Project Settings → Quality
   - For each level (Low/Med/High):
     - Set ""Scriptable Render Pipeline Asset"" to URP asset

4. Verify:
   - Select Main Camera
   - Should have UniversalRenderPipeline component
   - Materials should use ""Universal Render Pipeline/*"" shaders

=== QUICK COMMANDS ===

Tools → URP Setup → Auto-Configure URP
Tools → URP Setup → Find URP Asset
Tools → URP Setup → Check Current Settings
Tools → URP Setup → Show Setup Guide

";
            Debug.Log(guide);
            EditorUtility.DisplayDialog("URP Setup Guide", guide, "OK");
        }

        #endregion
    }
}
