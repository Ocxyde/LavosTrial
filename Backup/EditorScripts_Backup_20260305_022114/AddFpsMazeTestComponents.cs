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
// AddFpsMazeTestComponents.cs
// Editor script to add all required components to FpsMazeTest scene
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ️  OBSOLETE - Replaced by MazeBuilderEditor
// This file is marked for deletion. Run:
//   .\cleanup_obsolete_editor_scripts.ps1
//
// USAGE:
//   1. Select the GameObject with FpsMazeTest component
//   2. Tools → Add FpsMazeTest Components
//   3. All required components will be added automatically

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    [System.Obsolete("AddFpsMazeTestComponents is deprecated. Use MazeBuilderEditor instead. This file will be deleted by cleanup_obsolete_editor_scripts.ps1")]
    public class AddFpsMazeTestComponents : EditorWindow
    {
        [MenuItem("Tools/Add FpsMazeTest Components")]
        public static void AddComponents()
        {
            // Find FpsMazeTest in scene
            var fpsMazeTest = UnityEngine.Object.FindFirstObjectByType<FpsMazeTest>();
            if (fpsMazeTest == null)
            {
                Debug.LogError("[Editor] No FpsMazeTest found in scene!");
                Debug.Log("[Editor] Please create a GameObject named 'MazeTest' and add FpsMazeTest component");
                return;
            }

            GameObject go = fpsMazeTest.gameObject;
            int addedCount = 0;

            // Add MazeGenerator
            if (go.GetComponent<MazeGenerator>() == null)
            {
                go.AddComponent<MazeGenerator>();
                Debug.Log("[Editor]  Added MazeGenerator");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor]  MazeGenerator already exists");
            }

            // Add MazeRenderer
            if (go.GetComponent<MazeRenderer>() == null)
            {
                go.AddComponent<MazeRenderer>();
                Debug.Log("[Editor]  Added MazeRenderer");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor]  MazeRenderer already exists");
            }

            // Add MazeIntegration
            if (go.GetComponent<MazeIntegration>() == null)
            {
                go.AddComponent<MazeIntegration>();
                Debug.Log("[Editor]  Added MazeIntegration");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor]  MazeIntegration already exists");
            }

            // Add SpatialPlacer
            if (go.GetComponent<SpatialPlacer>() == null)
            {
                go.AddComponent<SpatialPlacer>();
                Debug.Log("[Editor]  Added SpatialPlacer");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor]  SpatialPlacer already exists");
            }

            // Add TorchPool
            if (go.GetComponent<TorchPool>() == null)
            {
                go.AddComponent<TorchPool>();
                Debug.Log("[Editor]  Added TorchPool");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor]  TorchPool already exists");
            }

            // Add LightPlacementEngine
            if (go.GetComponent<LightPlacementEngine>() == null)
            {
                go.AddComponent<LightPlacementEngine>();
                Debug.Log("[Editor]  Added LightPlacementEngine");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor]  LightPlacementEngine already exists");
            }

            // Add LightEngine
            if (go.GetComponent<LightEngine>() == null)
            {
                go.AddComponent<LightEngine>();
                Debug.Log("[Editor]  Added LightEngine");
                addedCount++;
            }
            else
            {
                Debug.Log("[Editor]  LightEngine already exists");
            }

            Debug.Log($"[Editor] Added {addedCount} new components to '{go.name}'");
            Debug.Log("[Editor] Press Play to test!");
        }

        [MenuItem("Tools/Fix FpsMazeTest Scene")]
        public static void FixScene()
        {
            // Create new GameObject if none exists
            var fpsMazeTest = UnityEngine.Object.FindFirstObjectByType<FpsMazeTest>();
            if (fpsMazeTest == null)
            {
                GameObject go = new GameObject("MazeTest");
                fpsMazeTest = go.AddComponent<FpsMazeTest>();
                Debug.Log("[Editor]  Created 'MazeTest' GameObject with FpsMazeTest");
            }
            else
            {
                Debug.Log("[Editor]  FpsMazeTest already exists");
            }

            // Add all components
            AddComponents();
        }
    }
}
