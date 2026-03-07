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
// LightPlacementEngine.cs
// Batch instantiation engine for light-emitting objects
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT ARCHITECTURE:
// - Loads light positions from encrypted binary storage
// - Instantiates ALL lights at once (no runtime teleportation)
// - Manages light states (ON/OFF) without position changes
// - Supports torches, candles, lamps, and all light-emitting types
//
// USAGE:
//   1. Add LightPlacementEngine component to a GameObject in scene
//   2. Assign prefabs in Inspector (Torch, Candle, Lamp, etc.)
//   3. Call LoadAndInstantiate(mazeId, seed) after maze generation
//   4. Control individual lights via SetLightState(guid, isOn)
//
// PERFORMANCE:
//   - Batch instantiation: ~100 torches in <10ms
//   - No runtime teleportation
//   - Memory efficient binary loading
//   - State changes only affect flame/light, not position
//
// Location: Assets/Scripts/Core/10_Resources/

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Code.Lavos.Core
{
    /// <summary>
    /// LightPlacementEngine - Batch instantiation and management of light-emitting objects.
    /// Loads positions from encrypted binary storage and instantiates all lights at once.
    /// </summary>
    public class LightPlacementEngine : MonoBehaviour
    {
        #region Inspector Settings
        
        [Header("Prefabs")]
        [Tooltip("Torch prefab for wall mounting")]
        [SerializeField] private GameObject torchPrefab;
        
        [Tooltip("Candle prefab")]
        [SerializeField] private GameObject candlePrefab;
        
        [Tooltip("Lamp prefab")]
        [SerializeField] private GameObject lampPrefab;
        
        [Tooltip("Lantern prefab")]
        [SerializeField] private GameObject lanternPrefab;
        
        [Tooltip("Brazier prefab")]
        [SerializeField] private GameObject brazierPrefab;
        
        [Header("Settings")]
        [Tooltip("Parent object for all instantiated lights")]
        [SerializeField] private string parentName = "Lights";

        [Tooltip("Start with lights ON")]
        [SerializeField] private bool startOn = false;  // Default OFF for manual activation
        
        [Tooltip("Show debug info")]
        [SerializeField] private bool showDebugInfo = true;
        
        #endregion
        
        #region Private Fields
        
        private Transform _lightsParent;
        private readonly Dictionary<string, TorchController> _torchControllers = new Dictionary<string, TorchController>();
        private readonly Dictionary<string, LightEmittingController> _lightControllers = new Dictionary<string, LightEmittingController>();
        private readonly List<GameObject> _instantiatedLights = new List<GameObject>();
        private int _currentSeed;
        private string _currentMazeId;
        
        #endregion
        
        #region Unity Lifecycle

        private void Awake()
        {
            // Don't initialize in editor pause mode
            if (!Application.isPlaying) return;

            // Auto-find torch prefab from TorchPool FIRST (most reliable at runtime)
            if (torchPrefab == null)
            {
                // Try to find TorchPool component anywhere in scene (not just same GameObject)
                var torchPool = FindFirstObjectByType<TorchPool>();
                if (torchPool != null)
                {
                    torchPrefab = torchPool.TorchHandlePrefab;  // Use public property
                    if (torchPrefab != null)
                    {
                        Debug.Log("[LightPlacementEngine]  TorchPrefab assigned from TorchPool");
                    }
                }
            }

            // If still null, try to load from GameConfig
            if (torchPrefab == null)
            {
                var cfg = GameConfig.Instance;
                if (!string.IsNullOrEmpty(cfg.torchPrefab))
                {
                    Debug.Log($"[LightPlacementEngine]  Loading torch prefab from GameConfig: {cfg.torchPrefab}");
                    torchPrefab = Resources.Load<GameObject>(cfg.torchPrefab.Replace("Assets/Resources/", "").Replace(".prefab", ""));
                    
                    if (torchPrefab != null)
                    {
                        Debug.Log($"[LightPlacementEngine]  TorchPrefab loaded: {torchPrefab.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[LightPlacementEngine]  TorchPrefab not found at: {cfg.torchPrefab}");
                    }
                }
            }

            // If still null, try to load from Resources folder
            if (torchPrefab == null)
            {
                Debug.Log("[LightPlacementEngine]  TorchPrefab not assigned - trying Resources.Load...");
                torchPrefab = Resources.Load<GameObject>("TorchHandlePrefab");

                if (torchPrefab != null)
                {
                    Debug.Log("[LightPlacementEngine]  TorchPrefab loaded from Resources folder");
                }
            }

            // If still null, try to find in scene (look for torch prefabs)
            if (torchPrefab == null)
            {
                Debug.Log("[LightPlacementEngine]  TorchPrefab not in Resources - searching scene...");
                var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (var obj in allObjects)
                {
                    if (obj != null && obj.name.Contains("TorchHandle"))
                    {
                        torchPrefab = obj;
                        Debug.Log($"[LightPlacementEngine]  TorchPrefab found in scene: {obj.name}");
                        break;
                    }
                }
            }

            // Final check
            if (torchPrefab == null)
            {
                Debug.LogError("[LightPlacementEngine] No torchPrefab assigned!");
                Debug.LogError("[LightPlacementEngine] Please ensure:");
                Debug.LogError("[LightPlacementEngine]   1. Assign torchPrefab in Inspector, OR");
                Debug.LogError("[LightPlacementEngine]   2. Set torchPrefab path in GameConfig.json, OR");
                Debug.LogError("[LightPlacementEngine]   3. Add TorchHandlePrefab to Resources folder, OR");
                Debug.LogError("[LightPlacementEngine]   4. Have a TorchHandlePrefab in the scene");
                Debug.LogWarning("[LightPlacementEngine] Torches will NOT spawn until prefab is assigned!");
                return;
            }

            // Create parent object for organization
            _lightsParent = new GameObject(parentName).transform;

            Debug.Log("[LightPlacementEngine] Initialized");
        }

        /// <summary>
        /// Set torch prefab from editor tool (for testing).
        /// Loads prefab from Resources path specified in GameConfig.
        /// </summary>
        /// <param name="prefab">Torch prefab instance</param>
        public void SetTorchPrefab(GameObject prefab)
        {
            torchPrefab = prefab;
            Debug.Log($"[LightPlacementEngine]  TorchPrefab set via editor tool: {prefab.name}");
        }

        private void OnDestroy()
        {
            ClearAllLights();
        }

        #endregion
        
        #region Public API - Batch Loading
        
        /// <summary>
        /// Load torch positions from binary and instantiate all at once.
        /// If binary doesn't exist, caller should save first then call this.
        /// </summary>
        /// <param name="mazeId">Unique maze identifier</param>
        /// <param name="seed">Seed for decryption</param>
        /// <returns>True if binary was loaded, false if needs to be saved first</returns>
        public bool LoadAndInstantiateTorches(string mazeId, int seed)
        {
            _currentMazeId = mazeId;
            _currentSeed = seed;

            Debug.Log($"[LightPlacementEngine] Trying to load torches for maze: {mazeId}");
            Debug.Log($"[LightPlacementEngine] Seed: {seed}");

            // Try to load from binary file
            byte[] data = LightPlacementData.LoadFromFile(mazeId);

            if (data == null || data.Length == 0)
            {
                Debug.LogWarning($"[LightPlacementEngine] No binary data found for maze '{mazeId}'. Needs to be saved first.");
                return false;  // Signal that binary needs to be saved
            }

            Debug.Log($"[LightPlacementEngine] Binary file loaded: {data.Length} bytes");

            // Decrypt and load torch records
            List<WallPositionArchitect.TorchRecord> torches = LightPlacementData.LoadTorches(data, seed);

            if (torches.Count == 0)
            {
                Debug.LogError("[LightPlacementEngine] No torches in data after decryption!");
                return false;
            }

            Debug.Log($"[LightPlacementEngine] Decrypted {torches.Count} torches from binary");

            // Instantiate all torches at once
            InstantiateTorchesBatch(torches);
            return true;  // Successfully loaded from binary
        }
        
        /// <summary>
        /// Save current torch positions to encrypted binary (call after maze generation).
        /// </summary>
        /// <param name="mazeId">Unique maze identifier</param>
        /// <param name="seed">Seed for encryption</param>
        /// <param name="torchRecords">List of torch positions from SpatialPlacer</param>
        public void SaveTorches(string mazeId, int seed, List<WallPositionArchitect.TorchRecord> torchRecords)
        {
            if (torchRecords == null || torchRecords.Count == 0)
            {
                Debug.LogWarning("[LightPlacementEngine] No torch records to save");
                return;
            }
            
            // Encrypt and save to binary
            byte[] data = LightPlacementData.SaveTorches(torchRecords, seed);
            LightPlacementData.SaveToFile(data, mazeId);
            
            _currentMazeId = mazeId;
            _currentSeed = seed;
            
            Debug.Log($"[LightPlacementEngine] Saved {torchRecords.Count} torches for maze '{mazeId}'");
        }
        
        /// <summary>
        /// Instantiate torches from a list of records (immediate batch instantiation).
        /// </summary>
        public void InstantiateTorchesBatch(List<WallPositionArchitect.TorchRecord> torchRecords)
        {
            if (torchPrefab == null)
            {
                Debug.LogError("[LightPlacementEngine]  Torch prefab is NULL!");
                Debug.LogError("[LightPlacementEngine]  Please assign torchPrefab in Inspector!");
                return;
            }

            Debug.Log($"[LightPlacementEngine] Prefab: {torchPrefab.name}");
            Debug.Log($"[LightPlacementEngine] Has TorchController: {torchPrefab.GetComponent<TorchController>() != null}");
            Debug.Log($"[LightPlacementEngine] Has BraseroFlame: {torchPrefab.GetComponentInChildren<BraseroFlame>() != null}");
            
            // Clear existing
            ClearAllLights();

            if (showDebugInfo)
            {
                Debug.Log($"[LightPlacementEngine] Instantiating {torchRecords.Count} torches...");
            }

            float startTime = Time.realtimeSinceStartup;

            // Batch instantiate all torches
            foreach (var torchRecord in torchRecords)
            {
                GameObject torchObj = Instantiate(torchPrefab, torchRecord.position, torchRecord.rotation, _lightsParent);
                _instantiatedLights.Add(torchObj);

                // Get controller and store reference
                TorchController controller = torchObj.GetComponent<TorchController>();
                if (controller == null)
                {
                    Debug.LogError($"[LightPlacementEngine]  TorchController missing on torch at {torchRecord.position}!");
                    continue;
                }

                string guid = string.IsNullOrEmpty(torchRecord.guid) ?
                             $"torch_{_torchControllers.Count:D4}" : torchRecord.guid;

                _torchControllers[guid] = controller;

                // ALL torches ON by default
                controller.TurnOn();
                Debug.Log($"[LightPlacementEngine]  Torch {guid} turned ON at {torchRecord.position}");
            }

            float elapsed = Time.realtimeSinceStartup - startTime;

            if (showDebugInfo)
            {
                Debug.Log($"[LightPlacementEngine] Instantiated {torchRecords.Count} torches (ALL ON) in {elapsed * 1000:F2}ms");
            }
        }

        #endregion

        #region Public API - Manual Activation
        
        /// <summary>
        /// Turn on ALL torches at once (for testing/emergency).
        /// </summary>
        [ContextMenu("Turn On All Torches")]
        public void TurnOnAllTorches()
        {
            foreach (var kvp in _torchControllers)
            {
                kvp.Value.TurnOn();
            }
            
            Debug.Log($"[LightPlacementEngine] All {_torchControllers.Count} torches turned ON");
        }
        
        /// <summary>
        /// Turn off ALL torches at once.
        /// </summary>
        [ContextMenu("Turn Off All Torches")]
        public void TurnOffAllTorches()
        {
            foreach (var kvp in _torchControllers)
            {
                kvp.Value.TurnOff();
            }
            
            Debug.Log($"[LightPlacementEngine] All {_torchControllers.Count} torches turned OFF");
        }
        
        /// <summary>
        /// Get all torch GUIDs.
        /// </summary>
        public System.Collections.Generic.List<string> GetAllTorchGuids()
        {
            return new System.Collections.Generic.List<string>(_torchControllers.Keys);
        }
        
        /// <summary>
        /// Get torch count.
        /// </summary>
        public int GetTorchCount() => _torchControllers.Count;
        
        #endregion

        #region Public API - Light State Control
        
        /// <summary>
        /// Set a specific torch ON or OFF (state change only, no position update).
        /// </summary>
        /// <param name="guid">Torch GUID</param>
        /// <param name="isOn">Turn on or off</param>
        public void SetTorchState(string guid, bool isOn)
        {
            if (_torchControllers.TryGetValue(guid, out TorchController controller))
            {
                if (isOn)
                {
                    controller.TurnOn();
                }
                else
                {
                    controller.TurnOff();
                }
            }
            else
            {
                Debug.LogWarning($"[LightPlacementEngine] Torch not found: {guid}");
            }
        }
        
        /// <summary>
        /// Toggle a specific torch.
        /// </summary>
        public void ToggleTorch(string guid)
        {
            if (_torchControllers.TryGetValue(guid, out TorchController controller))
            {
                if (controller.IsOn)
                {
                    controller.TurnOff();
                }
                else
                {
                    controller.TurnOn();
                }
            }
        }
        
        /// <summary>
        /// Set all torches ON or OFF at once.
        /// </summary>
        public void SetAllTorches(bool isOn)
        {
            foreach (var kvp in _torchControllers)
            {
                if (isOn)
                {
                    kvp.Value.TurnOn();
                }
                else
                {
                    kvp.Value.TurnOff();
                }
            }
            
            Debug.Log($"[LightPlacementEngine] Set all {_torchControllers.Count} torches {(isOn ? "ON" : "OFF")}");
        }
        
        /// <summary>
        /// Toggle all torches.
        /// </summary>
        public void ToggleAllTorches()
        {
            bool newState = _torchControllers.Count > 0 && !_torchControllers.Values.GetEnumerator().Current.IsOn;
            SetAllTorches(newState);
        }
        
        #endregion
        
        #region Public API - Generic Light Sources
        
        /// <summary>
        /// Load and instantiate generic light sources (candles, lamps, etc.).
        /// </summary>
        public void LoadAndInstantiateLights(string mazeId, int seed, LightEmittingType lightType)
        {
            byte[] data = LightPlacementData.LoadFromFile($"{mazeId}_{lightType}");
            
            if (data == null || data.Length == 0)
            {
                Debug.LogWarning($"[LightPlacementEngine] No data for {lightType} lights");
                return;
            }
            
            List<LightPlacementData.LightData> lights = LightPlacementData.LoadLights(data, seed);
            InstantiateLightsBatch(lights, lightType);
        }
        
        /// <summary>
        /// Instantiate generic lights from a list of records.
        /// </summary>
        public void InstantiateLightsBatch(List<LightPlacementData.LightData> lights, LightEmittingType lightType)
        {
            GameObject prefab = GetLightPrefab(lightType);
            if (prefab == null)
            {
                Debug.LogError($"[LightPlacementEngine] No prefab assigned for {lightType}");
                return;
            }
            
            foreach (var lightData in lights)
            {
                GameObject lightObj = Instantiate(prefab, lightData.position, lightData.rotation, _lightsParent);
                _instantiatedLights.Add(lightObj);
                
                LightEmittingController controller = lightObj.GetComponent<LightEmittingController>();
                if (controller != null)
                {
                    string guid = $"light_{lightType}_{_lightControllers.Count:D4}";
                    _lightControllers[guid] = controller;
                    
                    if (startOn)
                    {
                        controller.TurnOn();
                    }
                    else
                    {
                        controller.TurnOff();
                    }
                }
            }
        }
        
        /// <summary>
        /// Set a generic light source ON or OFF.
        /// </summary>
        public void SetLightState(string guid, bool isOn)
        {
            if (_lightControllers.TryGetValue(guid, out LightEmittingController controller))
            {
                if (isOn)
                {
                    controller.TurnOn();
                }
                else
                {
                    controller.TurnOff();
                }
            }
        }
        
        #endregion
        
        #region Public API - Cleanup
        
        /// <summary>
        /// Clear all instantiated lights.
        /// </summary>
        public void ClearAllLights()
        {
            foreach (var light in _instantiatedLights)
            {
                if (light != null)
                {
                    Destroy(light);
                }
            }
            
            _instantiatedLights.Clear();
            _torchControllers.Clear();
            _lightControllers.Clear();
            
            Debug.Log("[LightPlacementEngine] Cleared all lights");
        }
        
        /// <summary>
        /// Remove a specific light by GUID.
        /// </summary>
        public void RemoveLight(string guid)
        {
            if (_torchControllers.TryGetValue(guid, out TorchController controller))
            {
                _torchControllers.Remove(guid);
                _instantiatedLights.Remove(controller.gameObject);
                Destroy(controller.gameObject);
            }
            else if (_lightControllers.TryGetValue(guid, out LightEmittingController lightController))
            {
                _lightControllers.Remove(guid);
                _instantiatedLights.Remove(lightController.gameObject);
                Destroy(lightController.gameObject);
            }
        }
        
        #endregion
        
        #region Private Helpers
        
        private GameObject GetLightPrefab(LightEmittingType type)
        {
            return type switch
            {
                LightEmittingType.Candle => candlePrefab,
                LightEmittingType.Lamp => lampPrefab,
                LightEmittingType.Lantern => lanternPrefab,
                LightEmittingType.Brazier => brazierPrefab,
                LightEmittingType.Torch => torchPrefab,
                _ => torchPrefab
            };
        }
        
        #endregion
        
        #region Debug
        
        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"[LightPlacementEngine] Torches: {_torchControllers.Count}");
            GUILayout.Label($"[LightPlacementEngine] Other Lights: {_lightControllers.Count}");
            GUILayout.Label($"[LightPlacementEngine] Total Objects: {_instantiatedLights.Count}");
            GUILayout.Label($"[LightPlacementEngine] Maze ID: {_currentMazeId}");
            GUILayout.Label($"[LightPlacementEngine] Seed: {_currentSeed}");
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
