// SpecialRoomPreset.cs
// ScriptableObject for storing special room configurations
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ENVIRONMENT: Room preset definitions
// Location: Assets/Scripts/Core/08_Environment/

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// SpecialRoomPreset - ScriptableObject for storing room configurations.
    /// 
    /// Create presets via:
    /// Assets → Create → Special Room Preset
    /// 
    /// Use presets to quickly configure SpecialRoom instances.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRoomPreset", menuName = "Special Room Preset", order = 1)]
    public class SpecialRoomPreset : ScriptableObject
    {
        [Header("Room Type")]
        public RoomType roomType = RoomType.Treasure;
        
        [Header("Atmosphere")]
        public Color ambientColor = new Color(0.3f, 0.2f, 0.4f);
        public Color fogColor = new Color(0.4f, 0.3f, 0.5f);
        [Range(0f, 1f)]
        public float fogDensity = 0.3f;
        public float fogStartDistance = 2f;
        public float fogEndDistance = 15f;
        
        [Header("Lighting")]
        public Color lightColor = new Color(0.8f, 0.6f, 1f);
        public float lightIntensity = 0.5f;
        public float lightRange = 15f;
        
        [Header("Dimensions")]
        public float width = 10f;
        public float height = 6f;
        public float depth = 10f;
    }
    
    public enum RoomType
    {
        Treasure,
        Trap,
        Boss,
        Secret,
        Custom
    }
}
