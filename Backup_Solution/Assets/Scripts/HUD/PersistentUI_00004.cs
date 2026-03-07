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
// PersistentUI.cs
// Persistent UI manager that survives scene loads
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Persistent UI for plug-in-and-out system

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Persistent UI manager - survives scene loads.
    /// Attach to any UI GameObject that should persist across scenes.
    /// </summary>
    public class PersistentUI : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}