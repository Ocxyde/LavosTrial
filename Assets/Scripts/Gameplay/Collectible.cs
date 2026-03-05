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
// Collectible.cs
// Collectible item system - coins, potions, bonuses
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// SETUP in Unity:
//  1. Create a GameObject (e.g., sphere)
//  2. Attach this script
//  3. Add Collider with "Is Trigger" enabled
//  4. Select type in Inspector

using UnityEngine;
using Code.Lavos.HUD;
using Code.Lavos.Status;

namespace Code.Lavos.Core
{
    /// <summary>
    /// COLLECTIBLE — Pickable object (coin, potion, bonus...)
    ///
    /// SETUP in Unity:
    ///  1. Create a GameObject (e.g., a sphere)
    ///  2. Attach this script
    ///  3. Add Collider with "Is Trigger" mode
    ///  4. Select type in Inspector
    /// </summary>
    public class Collectible : MonoBehaviour
    {
    public enum CollectibleType { Score, Health, Mana, Stamina }

    [Header("Type et valeur")]
    [SerializeField] private CollectibleType type = CollectibleType.Score;
    [SerializeField] private float value = 50f;
    [SerializeField] private string popupMsg = "+50 pts";

    [Header("Animation")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 1.5f;
    [SerializeField] private float bobHeight = 0.3f;

    [Header("Effets")]
    [SerializeField] private GameObject collectVFX;

    private Vector3 _startPosition;

    void Start() => _startPosition = transform.position;

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        float newY = _startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Collect(other.gameObject);
    }

    private void Collect(GameObject player)
    {
        var stats = player.GetComponent<PlayerStats>();

        switch (type)
        {
            case CollectibleType.Score:
                // GameManager.Instance?.AddScore((int)value); // [REMOVED] Use EventHandler instead
                break;

            case CollectibleType.Health:
                stats?.Heal(value);
                break;

            case CollectibleType.Mana:
                stats?.RestoreMana(value);
                break;

            case CollectibleType.Stamina:
                stats?.RestoreStamina(value);
                break;
        }

        if (!string.IsNullOrEmpty(popupMsg))
            HUDSystem.Instance?.ShowFloatingText(popupMsg, Color.white, 2f);

        if (collectVFX != null)
            Instantiate(collectVFX, transform.position, Quaternion.identity);

        Debug.Log($"[Collectible] Ramassé : {type} +{value}");
        Destroy(gameObject);
    }
    }
}
