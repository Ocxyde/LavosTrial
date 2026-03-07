// SpecialRoom.cs
// Special room with unique atmosphere (fog, ambient color)
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ENVIRONMENT: Special rooms in maze with unique visual effects
// Location: Assets/Scripts/Core/08_Environment/

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
        [SerializeField] private Color ambientColor = new Color(0.3f, 0.2f, 0.4f);
        
        [Tooltip("Fog color inside the room")]
        [SerializeField] private Color fogColor = new Color(0.4f, 0.3f, 0.5f);
        
        [Tooltip("Fog density (0 = no fog, 1 = thick fog)")]
        [Range(0f, 1f)]
        [SerializeField] private float fogDensity = 0.3f;
        
        [Tooltip("Fog start distance from camera")]
        [SerializeField] private float fogStartDistance = 2f;
        
        [Tooltip("Fog end distance from camera")]
        [SerializeField] private float fogEndDistance = 15f;

        [Header("Lighting")]
        [SerializeField] private Color roomLightColor = new Color(0.8f, 0.6f, 1f);
        [SerializeField] private float roomLightIntensity = 0.5f;
        [SerializeField] private float lightRange = 15f;

        [Header("Visual Bounds")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color gizmoColor = Color.magenta;

        // Private state
        private bool isPlayerInside = false;
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
            roomLight.color = roomLightColor;
            roomLight.intensity = 0f; // Start off, fade in when player enters
            roomLight.range = lightRange;
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
            isPlayerInside = true;
            
            Debug.Log($"[SpecialRoom] Player entered - Activating atmosphere");
            
            // Apply custom ambient color
            RenderSettings.ambientLight = ambientColor;
            
            // Apply custom fog
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
            
            // Fade in room light
            LeanTween.cancel(gameObject);
            LeanTween.value(gameObject, UpdateLightIntensity, 0f, roomLightIntensity, 1f)
                .setEase(LeanTweenType.easeInOutQuad);
        }

        void ExitRoom()
        {
            isPlayerInside = false;
            
            Debug.Log($"[SpecialRoom] Player exited - Restoring original settings");
            
            // Restore original settings
            RenderSettings.ambientLight = originalAmbientColor;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogDensity = originalFogDensity;
            RenderSettings.fogMode = originalFogMode;
            
            // Fade out room light
            LeanTween.cancel(gameObject);
            LeanTween.value(gameObject, UpdateLightIntensity, roomLightIntensity, 0f, 0.5f)
                .setEase(LeanTweenType.easeInOutQuad);
        }

        void UpdateLightIntensity(float intensity)
        {
            if (roomLight != null)
            {
                roomLight.intensity = intensity;
            }
        }

        void OnDestroy()
        {
            // Always restore settings when room is destroyed
            if (!isPlayerInside) return;
            
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
