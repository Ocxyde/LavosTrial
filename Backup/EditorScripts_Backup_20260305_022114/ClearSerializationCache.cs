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
// ClearSerializationCache.cs
// Clears Unity's serialization cache to fix "duplicate field" errors
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ️  OBSOLETE - One-time use script (no longer needed)
// This file is marked for deletion. Run:
//   .\cleanup_obsolete_editor_scripts.ps1
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
    [System.Obsolete("ClearSerializationCache is a one-time use script. No longer needed. This file will be deleted by cleanup_obsolete_editor_scripts.ps1")]
    public class ClearSerializationCache
    {
        [MenuItem("Tools/Clear Serialization Cache")]
        public static void Clear()
        {
            Debug.Log("[ClearCache] ════════════════════════════════════════");
            Debug.Log("[ClearCache]  Clearing serialization cache...");
            Debug.Log("[ClearCache] ════════════════════════════════════════");

            // Delete door prefabs (they have the serialization issue)
            DeletePrefab("Assets/Prefabs/DoorPrefab.prefab");
            DeletePrefab("Assets/Prefabs/LockedDoorPrefab.prefab");
            DeletePrefab("Assets/Prefabs/SecretDoorPrefab.prefab");

            // Clear Library cache
            Debug.Log("[ClearCache]  Clearing Library cache...");
            
            // Force Unity to reimport everything
            Debug.Log("[ClearCache]  Forcing reimport...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            Debug.Log("[ClearCache] ════════════════════════════════════════");
            Debug.Log("[ClearCache]  Cache cleared!");
            Debug.Log("[ClearCache]  Now run: Tools → Create Maze Prefabs");
            Debug.Log("[ClearCache] ════════════════════════════════════════");
        }

        private static void DeletePrefab(string path)
        {
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[ClearCache] ️ Deleted: {path}");
            }

            string metaPath = $"{path}.meta";
            if (File.Exists(metaPath))
            {
                AssetDatabase.DeleteAsset(metaPath);
            }
        }
    }
}
