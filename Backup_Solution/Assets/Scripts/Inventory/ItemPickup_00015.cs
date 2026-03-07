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
// ItemPickup.cs
// Pickup behavior for inventory items
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Inventory system - world pickup

using UnityEngine;

namespace Code.Lavos.Core
{
    public class ItemPickup : MonoBehaviour
    {
    [Header("Item Settings")]
    [SerializeField] private ItemData item;
    [SerializeField] private int quantity = 1;
    [SerializeField] private float pickupRadius = 1.5f;
    [SerializeField] private float respawnTime = 30f;
    [SerializeField] private bool autoPickup = false;

    [Header("Visual")]
    [SerializeField] private float rotateSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;

    [Header("Feedback")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffect;

    private Vector3 _startPosition;
    private bool _canPickup = true;
    private Renderer _renderer;
    private Collider _collider;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
        _startPosition = transform.position;
    }

    private void Start()
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemPickup] No item assigned to pickup!");
            return;
        }

        var inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning("[ItemPickup] No Inventory found!");
            return;
        }
    }

    private void Update()
    {
        if (!_canPickup) return;

        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        float newY = _startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (autoPickup)
        {
            TryAutoPickup();
        }
    }

    private void TryAutoPickup()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= pickupRadius)
        {
            Pickup(player);
        }
    }

    public void Pickup(GameObject picker)
    {
        if (!_canPickup || item == null) return;

        // Check Inventory exists
        var inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning("[ItemPickup] Cannot pickup - Inventory not found!");
            return;
        }

        if (inventory.AddItem(item, quantity))
        {
            item.OnPickup(picker);

            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            Debug.Log($"[ItemPickup] Picked up {quantity}x {item.itemName}");

            if (respawnTime > 0)
                StartCoroutine(RespawnRoutine());
            else
                Destroy(gameObject);
        }
        else
        {
            Debug.Log("[ItemPickup] Inventory full!");
        }
    }

    private System.Collections.IEnumerator RespawnRoutine()
    {
        _canPickup = false;
        if (_renderer != null) _renderer.enabled = false;
        if (_collider != null) _collider.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        _canPickup = true;
        if (_renderer != null) _renderer.enabled = true;
        if (_collider != null) _collider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Pickup(other.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
    }
}
