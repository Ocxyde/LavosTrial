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
// InventorySlotUI.cs
// UI slot for inventory items
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Inventory system - UI component

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Lavos.Core
{
    public class InventorySlotUI : MonoBehaviour
    {
    [Header("References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private GameObject emptySlot;
    [SerializeField] private GameObject selectionHighlight;

    private int _slotIndex;
    private System.Action<int> _onClick;
    private System.Action<int> _onHover;
    private System.Action _onHoverEnd;

    public void Initialize(int slotIndex, System.Action<int> onClick, System.Action<int> onHover, System.Action onHoverEnd)
    {
        _slotIndex = slotIndex;
        _onClick = onClick;
        _onHover = onHover;
        _onHoverEnd = onHoverEnd;
    }

    public void UpdateSlot(InventorySlot slot)
    {
        if (slot.IsEmpty)
        {
            if (itemIcon != null)
                itemIcon.sprite = null;

            if (itemIcon != null)
                itemIcon.enabled = false;

            if (quantityText != null)
                quantityText.text = "";

            if (emptySlot != null)
                emptySlot.SetActive(true);
        }
        else
        {
            if (itemIcon != null && slot.item.icon != null)
            {
                itemIcon.sprite = slot.item.icon;
                itemIcon.enabled = true;
            }
            else if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.enabled = false;
            }

            if (quantityText != null && slot.quantity > 1)
            {
                quantityText.text = slot.quantity.ToString();
            }
            else if (quantityText != null)
            {
                quantityText.text = "";
            }

            if (emptySlot != null)
                emptySlot.SetActive(false);
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
            selectionHighlight.SetActive(selected);
    }

    public void OnPointerClick()
    {
        _onClick?.Invoke(_slotIndex);
    }

    public void OnPointerEnter()
    {
        _onHover?.Invoke(_slotIndex);
    }

    public void OnPointerExit()
    {
        _onHoverEnd?.Invoke();
    }
    }
}
