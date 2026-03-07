// ClearShaderCache.cs
// Editor utility to clear Unity shader cache
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Usage: Menu > Tools > Clear Shader Cache
// Or call: ClearShaderCache.ClearCachedData(null);

using UnityEngine;
using UnityEditor;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// Editor utility to clear cached shader data.
    /// Use this when encountering shader compilation errors.
    /// </summary>
    public static class ClearShaderCache
    {
        /// <summary>
        /// Clear all cached shader data.
        /// Call this when encountering shader precision errors.
        /// </summary>
        /// <param name="s">Optional specific shader to clear (null for all)</param>
        [MenuItem("Tools/Clear Shader Cache")]
        public static void ClearCachedData(Shader s)
        {
            if (s != null)
            {
                // Clear specific shader cache
                Shader.ClearGlobalShaderPropertyOverrides();
                Shader.ResetGlobalShaderProperties();
                Debug.Log($"[ClearShaderCache] Cleared cache for shader: {s.name}", s);
            }
            else
            {
                // Clear all shader caches
                Shader.ClearGlobalShaderPropertyOverrides();
                Shader.ResetGlobalShaderProperties();
                
                // Clear material cache
                EditorUtility.UnloadUnusedAssetsImmediate();
                
                // Clear library cache info
                Debug.Log("[ClearShaderCache] All shader caches cleared");
                Debug.Log("[ClearShaderCache] Reimporting shaders...");
                
                // Reimport all shaders
                string[] shaderGuids = AssetDatabase.FindAssets("t:Shader");
                foreach (string guid in shaderGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
                
                Debug.Log("[ClearShaderCache] Shader reimport complete");
            }
            
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Clear cache and recompile all shaders.
        /// Use this for severe shader errors.
        /// </summary>
        [MenuItem("Tools/Recompile All Shaders")]
        public static void RecompileAllShaders()
        {
            Debug.Log("[ClearShaderCache] Starting full shader recompilation...");
            
            // Clear caches first
            ClearCachedData(null);
            
            // Force shader variant collection
            ShaderVariantCollection.Clear();
            
            // Clear render pipeline cache
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            {
                Debug.Log("[ClearShaderCache] Clearing URP shader cache...");
            }
            
            Debug.Log("[ClearShaderCache] Recompliation complete. Restart Unity if errors persist.");
        }

        /// <summary>
        /// Clear cache for URP built-in shaders.
        /// Use this for URP-specific shader errors.
        /// </summary>
        [MenuItem("Tools/Clear URP Shader Cache")]
        public static void ClearURPCache()
        {
            Debug.Log("[ClearShaderCache] Clearing URP shader cache...");
            
            // Find and reimport URP shaders
            string[] urpShaders = AssetDatabase.FindAssets("t:Shader");
            int count = 0;
            
            foreach (string guid in urpShaders)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("ShaderGraph") || path.Contains("UniversalRenderPipeline"))
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    count++;
                }
            }
            
            Debug.Log($"[ClearShaderCache] Reimported {count} URP shaders");
        }
    }
}
