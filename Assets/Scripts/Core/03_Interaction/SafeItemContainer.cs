// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SafeItemContainer.cs
// Manages safe item storage and distribution through EventHandler
// Unity 6000.10f1 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Handles item registry, treasure distribution, and EventHandler integration (plug-in-out)

using UnityEngine;
using System.Collections.Generic;
using Code.Lavos.Core;

namespace Code.Lavos.Interaction
{
    public class SafeItemContainer : MonoBehaviour
    {
        [System.Serializable]
        public struct TreasureItem
        {
            public string itemId;
            public string itemName;
            public int quantity;
            public float rarity;
        }

        [Header("Treasure Configuration")]
        [SerializeField] private List<TreasureItem> _treasureItems = new List<TreasureItem>();
        [SerializeField] private bool _debugMode = false;

        private EventHandler _eventHandler;
        private bool _hasDistributed = false;

        private void Start()
        {
            FindEventHandler();

            if (_treasureItems.Count == 0 && _debugMode)
                Debug.LogWarning("[SafeItemContainer] No treasure items configured. Add items via inspector.");
        }

        private void FindEventHandler()
        {
            _eventHandler = FindFirstObjectByType<EventHandler>();

            if (_eventHandler == null && _debugMode)
                Debug.LogWarning("[SafeItemContainer] EventHandler not found in scene");
        }

        public void DistributeItems()
        {
            if (_hasDistributed)
                return;

            _hasDistributed = true;

            if (_debugMode)
                Debug.Log($"[SafeItemContainer] Distributing {_treasureItems.Count} treasure items");

            foreach (TreasureItem item in _treasureItems)
            {
                DistributeSingleItem(item);
            }
        }

        private void DistributeSingleItem(TreasureItem item)
        {
            if (_eventHandler == null)
            {
                if (_debugMode)
                    Debug.LogWarning($"[SafeItemContainer] Cannot distribute item {item.itemName}: EventHandler is null");
                return;
            }

            // Create ItemData reference for the event
            // Note: This requires the itemId to match a valid ItemData asset in Resources
            ItemData itemData = Resources.Load<ItemData>($"Items/{item.itemId}");

            if (itemData == null)
            {
                if (_debugMode)
                    Debug.LogWarning($"[SafeItemContainer] ItemData not found for itemId: {item.itemId}. Creating temporary ItemData.");

                // Fallback: create a minimal ItemData at runtime (not ideal but prevents crash)
                itemData = ScriptableObject.CreateInstance<ItemData>();
                itemData.id = item.itemId;
                itemData.itemName = item.itemName;
            }

            // Use InvokeItemPickedUp method instead of directly invoking the event
            _eventHandler.InvokeItemPickedUp(itemData, item.quantity);
            
            if (_debugMode)
                Debug.Log($"[SafeItemContainer] Distributed: {item.itemName} (x{item.quantity})");
        }

        public void AddTreasureItem(string itemId, string itemName, int quantity, float rarity)
        {
            TreasureItem newItem = new TreasureItem
            {
                itemId = itemId,
                itemName = itemName,
                quantity = quantity,
                rarity = rarity
            };

            _treasureItems.Add(newItem);

            if (_debugMode)
                Debug.Log($"[SafeItemContainer] Added treasure: {itemName}");
        }

        public void ClearTreasure()
        {
            _treasureItems.Clear();

            if (_debugMode)
                Debug.Log("[SafeItemContainer] All treasure cleared");
        }

        public int GetTreasureCount()
        {
            return _treasureItems.Count;
        }

        public List<TreasureItem> GetTreasureList()
        {
            return new List<TreasureItem>(_treasureItems);
        }
    }
}
