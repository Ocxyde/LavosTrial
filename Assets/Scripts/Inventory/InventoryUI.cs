using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private KeyCode toggleKey = KeyCode.I;
    [SerializeField] private bool showOnStart = false;

    private GameObject[] _slotObjects;
    private bool _isOpen = false;

    private void Start()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogWarning("[InventoryUI] No Inventory found!");
            return;
        }

        CreateSlots();
        
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged += UpdateUI;

        if (!showOnStart)
            CloseInventory();
        else
            OpenInventory();

        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    private void CreateSlots()
    {
        int slotCount = Inventory.Instance.Capacity;
        
        _slotObjects = new GameObject[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
            slotObj.GetComponent<InventorySlotUI>().Initialize(i, OnSlotClicked, OnSlotHovered, OnSlotHoverEnded);
            _slotObjects[i] = slotObj;
        }
    }

    public void UpdateUI()
    {
        if (Inventory.Instance == null) return;

        for (int i = 0; i < Inventory.Instance.Capacity; i++)
        {
            InventorySlot slot = Inventory.Instance.GetSlot(i);
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
        if (Inventory.Instance == null) return;

        InventorySlot slot = Inventory.Instance.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty) return;

        if (slot.item.itemType == ItemType.Consumable)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Inventory.Instance.UseItem(slotIndex, player);
            }
        }
    }

    private void OnSlotHovered(int slotIndex)
    {
        if (Inventory.Instance == null) return;

        InventorySlot slot = Inventory.Instance.GetSlot(slotIndex);
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
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged -= UpdateUI;
    }
}
