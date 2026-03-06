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
// RoomType.cs
// Room type enumeration for special room configurations
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE: Room type definitions for special rooms
// Location: Assets/Scripts/Core/06_Maze/

namespace Code.Lavos.Core
{
    /// <summary>
    /// Types of rooms for special behaviors.
    /// Used by SpecialRoomPreset and future room generation systems.
    /// </summary>
    public enum RoomType
    {
        Normal,     // Standard room
        Treasure,   // Contains chests/loot
        Combat,     // Enemy spawning room
        Trap,       // Trap-filled room
        Safe,       // No enemies/traps (rest area)
        Boss,       // Boss battle room
        Secret,     // Hidden room
        Puzzle      // Puzzle room
    }
}
