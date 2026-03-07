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
// TorchManualActivator.cs
// Manual torch activation helper - activate torches one by one with key press
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Add this component to same GameObject as LightPlacementEngine
//   2. Press Play - first torch ON, others OFF
//   3. Press [T] to activate next torch (one at a time)
//   4. Press [H] to activate all remaining torches at once
//   5. Press [X] to turn off all torches
//
// Location: Assets/Scripts/Tests/

using UnityEngine;
using UnityEngine.InputSystem;
using Code.Lavos.Core;  // Reference Core namespace for LightPlacementEngine

namespace Code.Lavos.Tests
{
    /// <summary>
    /// TorchManualActivator - Manual torch activation helper.
    /// Activate torches one by one with key presses.
    /// </summary>
    public class TorchManualActivator : MonoBehaviour
    {
        #region Inspector Settings

        [Header("References")]
        [Tooltip("Reference to LightPlacementEngine")]
        [SerializeField] private LightPlacementEngine lightPlacementEngine;

        [Header("Debug")]
        [Tooltip("Show debug messages")]
        [SerializeField] private bool showDebugMessages = true;

        #endregion
        
        #region Private Fields
        
        private int _nextTorchIndex = 0;
        private System.Collections.Generic.List<string> _torchGuids;
        
        #endregion
        
        #region Unity Lifecycle
        
        void Start()
        {
            // Auto-find LightPlacementEngine if not assigned
            if (lightPlacementEngine == null)
            {
                lightPlacementEngine = FindFirstObjectByType<LightPlacementEngine>();
            }
            
            if (lightPlacementEngine == null)
            {
                Debug.LogError("[TorchManualActivator] LightPlacementEngine not found!");
                enabled = false;
                return;
            }
            
            // Get all torch GUIDs
            _torchGuids = lightPlacementEngine.GetAllTorchGuids();
            _nextTorchIndex = 1;  // Start from 2nd torch (first is already ON)
            
            if (showDebugMessages)
            {
                Debug.Log($"[TorchManualActivator] Ready - Press [T] to activate torches one by one (1st is ON)");
                Debug.Log("[TorchManualActivator] Controls: [T] Next torch (1 at a time) | [H] All torches | [X] Turn off all");
            }
        }
        
        void Update()
        {
            if (lightPlacementEngine == null) return;
            if (Keyboard.current == null) return;

            var keyboard = Keyboard.current;

            // Activate next torch
            if (keyboard.tKey.wasPressedThisFrame)
            {
                ActivateNextTorch();
            }

            // Activate all torches
            if (keyboard.hKey.wasPressedThisFrame)
            {
                ActivateAllTorches();
            }

            // Turn off all torches
            if (keyboard.xKey.wasPressedThisFrame)
            {
                TurnOffAllTorches();
            }
        }
        
        #endregion
        
        #region Manual Activation
        
        /// <summary>
        /// Activate the next torch in the list.
        /// </summary>
        public void ActivateNextTorch()
        {
            if (_torchGuids == null || _torchGuids.Count == 0)
            {
                Debug.LogWarning("[TorchManualActivator] No torches available!");
                return;
            }
            
            if (_nextTorchIndex >= _torchGuids.Count)
            {
                if (showDebugMessages)
                {
                    Debug.Log("[TorchManualActivator] All torches already activated!");
                }
                return;
            }
            
            // Get next torch GUID
            string guid = _torchGuids[_nextTorchIndex];
            
            // Turn on torch
            lightPlacementEngine.SetTorchState(guid, true);
            
            if (showDebugMessages)
            {
                Debug.Log($"[TorchManualActivator] Torch {_nextTorchIndex + 1}/{_torchGuids.Count} activated ({guid})");
            }
            
            _nextTorchIndex++;
        }
        
        /// <summary>
        /// Activate all remaining torches at once.
        /// </summary>
        public void ActivateAllTorches()
        {
            if (_torchGuids == null || _torchGuids.Count == 0)
            {
                Debug.LogWarning("[TorchManualActivator] No torches available!");
                return;
            }
            
            // Turn on all remaining torches
            for (int i = _nextTorchIndex; i < _torchGuids.Count; i++)
            {
                string guid = _torchGuids[i];
                lightPlacementEngine.SetTorchState(guid, true);
            }
            
            if (showDebugMessages)
            {
                int activated = _torchGuids.Count - _nextTorchIndex;
                Debug.Log($"[TorchManualActivator] All {activated} remaining torches activated!");
            }
            
            _nextTorchIndex = _torchGuids.Count;
        }
        
        /// <summary>
        /// Turn off all torches.
        /// </summary>
        public void TurnOffAllTorches()
        {
            if (_torchGuids == null || _torchGuids.Count == 0)
            {
                return;
            }
            
            // Turn off all torches
            foreach (string guid in _torchGuids)
            {
                lightPlacementEngine.SetTorchState(guid, false);
            }
            
            _nextTorchIndex = 0;
            
            if (showDebugMessages)
            {
                Debug.Log("[TorchManualActivator] All torches turned OFF");
            }
        }
        
        #endregion
        
        #region Debug
        
        void OnGUI()
        {
            if (!showDebugMessages) return;
            
            GUILayout.BeginArea(new Rect(10, 200, 400, 150));
            GUILayout.Label($"[TorchManualActivator] Total Torches: {_torchGuids?.Count ?? 0}");
            GUILayout.Label($"[TorchManualActivator] Activated: {_nextTorchIndex}");
            GUILayout.Label($"[TorchManualActivator] Remaining: {(_torchGuids?.Count ?? 0) - _nextTorchIndex}");
            GUILayout.Space(10);
            GUILayout.Label("Controls:");
            GUILayout.Label("[T] - Activate next torch (one at a time)");
            GUILayout.Label("[H] - Activate all torches");
            GUILayout.Label("[X] - Turn off all torches");
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
