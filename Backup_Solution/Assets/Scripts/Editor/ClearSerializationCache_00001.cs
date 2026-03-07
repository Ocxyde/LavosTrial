// ClearSerializationCache.cs
// Clears Unity's serialization cache to fix "duplicate field" errors
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Tools → Clear Serialization Cache
//   2. Deletes cached door prefabs
//   3. Regenerate prefabs with Tools → Create Maze Prefabs

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    public class ClearSerializationCache
    {
        [MenuItem("Tools/Clear Serialization Cache")]
        public static void Clear()
        {
            Debug.Log("[ClearCache] ════════════════════════════════════════");
            Debug.Log("[ClearCache] 🧹 Clearing serialization cache...");
            Debug.Log("[ClearCache] ════════════════════════════════════════");

            // Delete door prefabs (they have the serialization issue)
            DeletePrefab("Assets/Prefabs/DoorPrefab.prefab");
            DeletePrefab("Assets/Prefabs/LockedDoorPrefab.prefab");
            DeletePrefab("Assets/Prefabs/SecretDoorPrefab.prefab");

            // Clear Library cache
            Debug.Log("[ClearCache] 📁 Clearing Library cache...");
            
            // Force Unity to reimport everything
            Debug.Log("[ClearCache] 🔄 Forcing reimport...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            Debug.Log("[ClearCache] ════════════════════════════════════════");
            Debug.Log("[ClearCache] ✅ Cache cleared!");
            Debug.Log("[ClearCache] 💡 Now run: Tools → Create Maze Prefabs");
            Debug.Log("[ClearCache] ════════════════════════════════════════");
        }

        private static void DeletePrefab(string path)
        {
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[ClearCache] 🗑️ Deleted: {path}");
            }

            string metaPath = $"{path}.meta";
            if (File.Exists(metaPath))
            {
                AssetDatabase.DeleteAsset(metaPath);
            }
        }
    }
}
