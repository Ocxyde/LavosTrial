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
// Ennemi.cs
// Basic enemy behavior - deals damage on collision
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Simple enemy that attaches to any GameObject with a Collider
// Deals damage when colliding with the player

using UnityEngine;

namespace Code.Lavos.Core
{
    public class Ennemi : MonoBehaviour
    {
    [Header("Combat")]
    [SerializeField] private float damage = 20f;

    private void OnCollisionEnter(Collision col)
    {
        if (!col.gameObject.CompareTag("Player")) return;

        var stats = col.gameObject.GetComponent<PlayerStats>();
        if (stats == null)
        {
            Debug.LogWarning($"[Ennemi] {gameObject.name} : pas de PlayerStats sur le joueur.");
            return;
        }
        stats.TakeDamage(damage);
    }
    }
}
