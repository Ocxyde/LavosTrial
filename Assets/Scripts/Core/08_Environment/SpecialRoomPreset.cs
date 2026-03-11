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
    /// Assets  Create  Special Room Preset
    /// 
    /// Use presets to quickly configure SpecialRoom instances.
    /// Note: Uses RoomType enum from RoomGenerator.cs
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
}
