// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Code.Lavos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Code.Lavos.  If not, see <https://www.gnu.org/licenses/>.
// SpecialRoom.cs
// Special room with unique atmosphere (fog, ambient color)
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ENVIRONMENT: Special rooms in maze with unique visual effects
// Location: Assets/Scripts/Core/08_Environment/

using System.Collections;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// SpecialRoom - A room in the maze with unique atmospheric effects.
    /// 
    /// Features:
    /// - Custom ambient color
    /// - Local fog volume
    /// - Open entrance (no door)
    /// - Trigger-based entry detection
    /// 
    /// Usage:
    /// 1. Place in maze at desired location
    /// 2. Set room dimensions
    /// 3. Configure fog and color
    /// 4. Room activates when player enters
    /// </summary>
    public class SpecialRoom : MonoBehaviour
    {
        [Header("Room Dimensions")]
        [SerializeField] private float roomWidth = 10f;
        [SerializeField] private float roomHeight = 6f;
        [SerializeField] private float roomDepth = 10f;

        [Header("Atmosphere")]
        [Tooltip("Ambient color inside the room")]
        [SerializeField] public Color AmbientColor = new Color(0.3f, 0.2f, 0.4f);

        [Tooltip("Fog color inside the room")]
        [SerializeField] public Color FogColor = new Color(0.4f, 0.3f, 0.5f);

        [Tooltip("Fog density (0 = no fog, 1 = thick fog)")]
        [Range(0f, 1f)]
        [SerializeField] public float FogDensity = 0.3f;

        [Tooltip("Fog start distance from camera")]
        [SerializeField] public float FogStartDistance = 2f;

        [Tooltip("Fog end distance from camera")]
        [SerializeField] public float FogEndDistance = 15f;

        [Header("Lighting")]
        [SerializeField] public Color RoomLightColor = new Color(0.8f, 0.6f, 1f);
        [SerializeField] public float RoomLightIntensity = 0.5f;
        [SerializeField] public float LightRange = 15f;

        [Header("Visual Bounds")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color gizmoColor = Color.magenta;

        // Private state
        private bool _isPlayerInside = false;
        private Color originalAmbientColor;
        private Color originalFogColor;
        private float originalFogDensity;
        private FogMode originalFogMode;
        private Light roomLight;
        private BoxCollider roomTrigger;

        void Awake()
        {
            // Create room trigger volume
            CreateRoomTrigger();
            
            // Create room light
            CreateRoomLight();
        }

        void Start()
        {
            // Store original render settings
            StoreOriginalSettings();
        }

        void CreateRoomTrigger()
        {
            var triggerObj = new GameObject("SpecialRoomTrigger");
            triggerObj.transform.parent = transform;
            triggerObj.transform.localPosition = Vector3.zero;
            
            roomTrigger = triggerObj.AddComponent<BoxCollider>();
            roomTrigger.isTrigger = true;
            roomTrigger.size = new Vector3(roomWidth, roomHeight, roomDepth);
            
            Debug.Log($"[SpecialRoom] Trigger volume created: {roomWidth}x{roomHeight}x{roomDepth}");
        }

        void CreateRoomLight()
        {
            var lightObj = new GameObject("SpecialRoomLight");
            lightObj.transform.parent = transform;
            lightObj.transform.localPosition = new Vector3(0, roomHeight / 2f, 0);
            
            roomLight = lightObj.AddComponent<Light>();
            roomLight.type = LightType.Point;
            roomLight.color = RoomLightColor;
            roomLight.intensity = 0f; // Start off, fade in when player enters
            roomLight.range = LightRange;
            roomLight.shadows = LightShadows.Soft;
        }

        void StoreOriginalSettings()
        {
            originalAmbientColor = RenderSettings.ambientLight;
            originalFogColor = RenderSettings.fogColor;
            originalFogDensity = RenderSettings.fogDensity;
            originalFogMode = RenderSettings.fogMode;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                EnterRoom();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                ExitRoom();
            }
        }

        void EnterRoom()
        {
            _isPlayerInside = true;

            Debug.Log($"[SpecialRoom] Player entered - Activating atmosphere");
            
            // Apply custom ambient color
            RenderSettings.ambientLight = AmbientColor;

            // Apply custom fog
            RenderSettings.fog = true;
            RenderSettings.fogColor = FogColor;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogDensity = FogDensity;
            RenderSettings.fogStartDistance = FogStartDistance;
            RenderSettings.fogEndDistance = FogEndDistance;

            // Fade in room light using coroutine
            StopAllCoroutines();
            StartCoroutine(FadeLightCoroutine(0f, RoomLightIntensity, 1f));
        }

        void ExitRoom()
        {
            _isPlayerInside = false;
            
            Debug.Log($"[SpecialRoom] Player exited - Restoring original settings");
            
            // Restore original settings
            RenderSettings.ambientLight = originalAmbientColor;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogDensity = originalFogDensity;
            RenderSettings.fogMode = originalFogMode;
            
            // Fade out room light using coroutine
            StopAllCoroutines();
            StartCoroutine(FadeLightCoroutine(RoomLightIntensity, 0f, 0.5f));
        }

        System.Collections.IEnumerator FadeLightCoroutine(float from, float to, float duration)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = Mathf.SmoothStep(0, 1, t);
                
                float intensity = Mathf.Lerp(from, to, t);
                
                if (roomLight != null)
                {
                    roomLight.intensity = intensity;
                }
                
                yield return null;
            }
            
            // Ensure final value
            if (roomLight != null)
            {
                roomLight.intensity = to;
            }
        }

        void OnDestroy()
        {
            // Always restore settings when room is destroyed
            if (!_isPlayerInside) return;

            RenderSettings.ambientLight = originalAmbientColor;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogDensity = originalFogDensity;
            RenderSettings.fogMode = originalFogMode;
        }

        void OnDrawGizmos()
        {
            if (!showGizmos) return;
            
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(transform.position, new Vector3(roomWidth, roomHeight, roomDepth));
            
            // Draw entrance (open side - no door!)
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                transform.position + new Vector3(-roomWidth / 2f, -roomHeight / 2f, roomDepth / 2f),
                transform.position + new Vector3(-roomWidth / 2f, roomHeight / 2f, roomDepth / 2f)
            );
        }
    }
}
