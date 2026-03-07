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
// TorchDiagnostics.cs
// Diagnostic tools for torch system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Ressources system - debugging

using UnityEngine;
using Code.Lavos.Core;

namespace Code.Lavos.Core
{
    // Lightweight runtime diagnostics for the TorchPool system
    // Logs counts of active torches to help verify behavior.
    public class TorchDiagnostics : MonoBehaviour
    {
        void Start()
        {
            var pool = UnityEngine.Object.FindFirstObjectByType<TorchPool>();
            if (pool != null)
            {
                Debug.Log("[TorchDiagnostics] Initial: Active=" + pool.ActiveCount);
            }
            else
            {
                Debug.Log("[TorchDiagnostics] TorchPool not found in scene at Start");
            }
        }

        void Update()
        {
            // Periodically log for ongoing visibility without being too noisy
            if (Time.frameCount % 300 == 0)
            {
                var pool = UnityEngine.Object.FindFirstObjectByType<TorchPool>();
                if (pool != null)
                {
                    Debug.Log("[TorchDiagnostics] Tick: Active=" + pool.ActiveCount);
                }
            }
        }
    }
}
