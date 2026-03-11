// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// SetupTorchPrefab.cs - Editor tool to apply materials to TORCH.fbx prefab

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// Editor tool to apply 8-bit pixel art materials to TORCH.fbx prefab.
    /// Run this AFTER generating textures with TorchPixelArtGenerator.
    /// </summary>
    public static class SetupTorchPrefab
    {
        private const string TORCH_PREFAB_PATH = "Assets/Prefabs/TORCHE.fbx";
        private const string HANDLE_MATERIAL_PATH = "Assets/Materials/Torch/Torch_Handle.mat";
        private const string FLAME_MATERIAL_PATH = "Assets/Materials/Torch/Torch_Flame.mat";
        private const string BASE_MATERIAL_PATH = "Assets/Materials/Torch/Torch_Base.mat";

        [MenuItem("Tools/Lavos/Setup Torch Prefab (Apply Materials)")]
        public static void ApplyMaterialsToTorchPrefab()
        {
            // Check if prefab exists
            if (!File.Exists(TORCH_PREFAB_PATH))
            {
                Debug.LogError($"[TorchSetup] TORCH.fbx not found at {TORCH_PREFAB_PATH}!");
                return;
            }

            // Load materials
            Material handleMat = AssetDatabase.LoadAssetAtPath<Material>(HANDLE_MATERIAL_PATH);
            Material flameMat = AssetDatabase.LoadAssetAtPath<Material>(FLAME_MATERIAL_PATH);
            Material baseMat = AssetDatabase.LoadAssetAtPath<Material>(BASE_MATERIAL_PATH);

            if (handleMat == null)
            {
                Debug.LogError($"[TorchSetup] Handle material not found! Run 'Generate Torch Pixel Art Textures' first.");
                return;
            }

            if (flameMat == null)
            {
                Debug.LogError($"[TorchSetup] Flame material not found! Run 'Generate Torch Pixel Art Textures' first.");
                return;
            }

            if (baseMat == null)
            {
                Debug.LogError($"[TorchSetup] Base material not found! Run 'Generate Torch Pixel Art Textures' first.");
                return;
            }

            // Load prefab
            GameObject torchPrefab = AssetDatabase.LoadMainAssetAtPath(TORCH_PREFAB_PATH) as GameObject;
            if (torchPrefab == null)
            {
                Debug.LogError($"[TorchSetup] Failed to load TORCH.fbx!");
                return;
            }

            // Get all MeshRenderer components
            MeshRenderer[] renderers = torchPrefab.GetComponentsInChildren<MeshRenderer>(true);
            
            Debug.Log($"[TorchSetup] Found {renderers.Length} mesh renderers in TORCH.fbx");

            int materialsApplied = 0;

            foreach (MeshRenderer renderer in renderers)
            {
                string rendererName = renderer.gameObject.name.ToLower();

                // Determine which material to apply based on object name
                Material targetMat = null;

                if (rendererName.Contains("handle") || rendererName.Contains("stick") || rendererName.Contains("rod"))
                {
                    targetMat = handleMat;
                    Debug.Log($"[TorchSetup] Applying HANDLE material to: {renderer.gameObject.name}");
                }
                else if (rendererName.Contains("flame") || rendererName.Contains("fire") || rendererName.Contains("glow"))
                {
                    targetMat = flameMat;
                    Debug.Log($"[TorchSetup] Applying FLAME material to: {renderer.gameObject.name}");
                }
                else if (rendererName.Contains("base") || rendererName.Contains("mount") || rendererName.Contains("wall"))
                {
                    targetMat = baseMat;
                    Debug.Log($"[TorchSetup] Applying BASE material to: {renderer.gameObject.name}");
                }
                else
                {
                    // Default to handle material for unknown parts
                    targetMat = handleMat;
                    Debug.Log($"[TorchSetup] Applying DEFAULT (handle) material to: {renderer.gameObject.name}");
                }

                if (targetMat != null)
                {
                    renderer.sharedMaterial = targetMat;
                    materialsApplied++;
                }
            }

            // Save prefab
            EditorUtility.SetDirty(torchPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[TorchSetup] ✅ Applied {materialsApplied}/{renderers.Length} materials to TORCH.fbx!");
            Debug.Log($"[TorchSetup] ✅ Torch prefab is now ready for use!");
        }

        [MenuItem("Tools/Lavos/Complete Torch Setup (Generate + Apply)")]
        public static void CompleteTorchSetup()
        {
            Debug.Log("[TorchSetup] 🔄 Starting complete torch setup...");
            
            // Step 1: Generate textures and materials
            TorchPixelArtGenerator.GenerateTorchTextures();
            
            // Step 2: Apply materials to prefab
            ApplyMaterialsToTorchPrefab();

            Debug.Log("[TorchSetup] ✅ Complete torch setup finished!");
        }
    }
}
#endif
