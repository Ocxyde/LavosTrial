// InventoryUI.cs
// Inventory UI display and management
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Inventory system - UI controller

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Lavos.Core
{
    public class InventoryUI : MonoBehaviour
    {
    [Header("References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI quantityText;

    [Header("Settings")]
    [SerializeField] private Key toggleKey = Key.I;
    [SerializeField] private bool showOnStart = false;

    private GameObject[] _slotObjects;
    private bool _isOpen = false;
    private Keyboard _keyboard;

    private Inventory _inventory;

    private void Start()
    {
        _keyboard = Keyboard.current;

        // Cache inventory reference
        _inventory = FindFirstObjectByType<Inventory>();
        
        // Validate required references
        if (_inventory == null)
        {
            Debug.LogWarning("[InventoryUI] No Inventory found!");
            return;
        }

        if (slotsContainer == null)
        {
            Debug.LogError("[InventoryUI] slotsContainer is not assigned!");
            enabled = false;
            return;
        }

        if (slotPrefab == null)
        {
            Debug.LogError("[InventoryUI] slotPrefab is not assigned!");
            enabled = false;
            return;
        }

        // Initialize arrays with safe defaults
        _slotObjects = new GameObject[_inventory.Capacity];

        CreateSlots();

        if (_inventory != null)
            _inventory.OnInventoryChanged += UpdateUI;

        if (!showOnStart)
            CloseInventory();
        else
            OpenInventory();

        UpdateUI();
    }

    private void Update()
    {
        if (_keyboard != null && _keyboard[toggleKey].wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    private void CreateSlots()
    {
        if (_inventory == null)
        {
            Debug.LogError("[InventoryUI] Cannot create slots - Inventory not initialized");
            return;
        }

        int slotCount = _inventory.Capacity;

        _slotObjects = new GameObject[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
            var slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                slotUI.Initialize(i, OnSlotClicked, OnSlotHovered, OnSlotHoverEnded);
            }
            _slotObjects[i] = slotObj;
        }
    }

    public void UpdateUI()
    {
        if (_inventory == null) return;

        for (int i = 0; i < _inventory.Capacity; i++)
        {
            InventorySlot slot = _inventory.GetSlot(i);
            if (slot != null && _slotObjects[i] != null)
            {
                _slotObjects[i].GetComponent<InventorySlotUI>().UpdateSlot(slot);
            }
        }
    }

    public void ToggleInventory()
    {
        if (_isOpen)
            CloseInventory();
        else
            OpenInventory();
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        _isOpen = true;
        UpdateUI();
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        _isOpen = false;
    }

    private void OnSlotClicked(int slotIndex)
    {
        if (_inventory == null) return;

        InventorySlot slot = _inventory.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty) return;

        if (slot.item.itemType == InventoryItemType.Consumable)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _inventory.UseItem(slotIndex, player);
            }
        }
    }

    private void OnSlotHovered(int slotIndex)
    {
        if (_inventory == null) return;

        InventorySlot slot = _inventory.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty) return;

        if (itemNameText != null)
            itemNameText.text = slot.item.itemName;

        if (itemDescriptionText != null)
            itemDescriptionText.text = slot.item.description;

        if (itemTypeText != null)
            itemTypeText.text = slot.item.itemType.ToString();

        if (quantityText != null)
            quantityText.text = slot.quantity > 1 ? $"x{slot.quantity}" : "";
    }

    private void OnSlotHoverEnded()
    {
        if (itemNameText != null)
            itemNameText.text = "";

        if (itemDescriptionText != null)
            itemDescriptionText.text = "";

        if (itemTypeText != null)
            itemTypeText.text = "";

        if (quantityText != null)
            quantityText.text = "";
    }

    private void OnDestroy()
    {
        if (_inventory != null)
            _inventory.OnInventoryChanged -= UpdateUI;
    }
}
