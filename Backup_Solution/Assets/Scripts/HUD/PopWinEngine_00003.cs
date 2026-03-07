// PopWinEngine.cs
// Popup Window Engine for UI Panels, Inventories, and Dialogs
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Reusable system for popup windows and panels
// - Inventory windows with slots
// - Dialog boxes
// - Shop/store windows
// - Status panels
// - Custom popup panels

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Code.Lavos.Core;

namespace Code.Lavos.HUD
{
    /// <summary>
    /// Central engine for all popup windows and panels.
    /// Provides inventory windows, dialogs, shops, and custom panels.
    /// </summary>
    public class PopWinEngine : MonoBehaviour
    {
        public static PopWinEngine Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private Transform canvasParent;
        [SerializeField] private bool dontDestroyOnLoad = true;

        [Header("Window Prefabs")]
        [SerializeField] private Sprite windowBackground;
        [SerializeField] private Color windowBgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        [SerializeField] private Color titleBgColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        [SerializeField] private Color textColor = Color.white;

        [Header("Inventory Settings")]
        [SerializeField] private int defaultInventoryColumns = 5;
        [SerializeField] private int defaultInventoryRows = 4;
        [SerializeField] private float slotSize = 64f;
        [SerializeField] private float slotSpacing = 8f;

        [Header("Animation")]
        [SerializeField] private float openDuration = 0.3f;
        [SerializeField] private float closeDuration = 0.2f;

        private Transform _windowParent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            CreateWindowParent();
        }

        private void CreateWindowParent()
        {
            var winGO = new GameObject("PopupWindows");
            winGO.transform.SetParent(canvasParent != null ? canvasParent : transform, false);
            _windowParent = winGO.AddComponent<RectTransform>().transform;
        }

        #region Basic Window

        /// <summary>
        /// Create a basic popup window.
        /// </summary>
        public GameObject CreateWindow(string title, float width = 400f, float height = 300f)
        {
            var windowGO = new GameObject($"Window_{title}");
            windowGO.transform.SetParent(_windowParent, false);

            var rect = windowGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            // Background
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(windowGO.transform, false);
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = windowBgColor;

            // Title bar
            var titleGO = new GameObject("TitleBar");
            titleGO.transform.SetParent(windowGO.transform, false);
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = new Vector2(0, 40);
            titleRect.pivot = new Vector2(0.5f, 1);
            var titleImage = titleGO.AddComponent<Image>();
            titleImage.color = titleBgColor;

            // Title text
            var titleTextGO = new GameObject("TitleText");
            titleTextGO.transform.SetParent(titleGO.transform, false);
            var titleTextRect = titleTextGO.AddComponent<RectTransform>();
            titleTextRect.anchorMin = Vector2.zero;
            titleTextRect.anchorMax = Vector2.one;
            titleTextRect.offsetMin = new Vector2(10, 5);
            titleTextRect.offsetMax = new Vector2(-10, -5);
            var titleText = titleTextGO.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 20;
            titleText.fontStyle = TMPro.FontStyles.Bold;
            titleText.color = textColor;
            titleText.alignment = TextAlignmentOptions.Center;

            // Close button
            var closeGO = new GameObject("CloseButton");
            closeGO.transform.SetParent(titleGO.transform, false);
            var closeRect = closeGO.AddComponent<RectTransform>();
            closeRect.sizeDelta = new Vector2(30, 30);
            closeRect.anchorMin = new Vector2(1, 0.5f);
            closeRect.anchorMax = new Vector2(1, 0.5f);
            closeRect.pivot = new Vector2(1, 0.5f);
            closeRect.anchoredPosition = new Vector2(-5, 0);
            var closeImage = closeGO.AddComponent<Image>();
            closeImage.color = new Color(0.8f, 0.2f, 0.2f);
            
            var closeTextGO = new GameObject("X");
            closeTextGO.transform.SetParent(closeGO.transform, false);
            var closeTextRect = closeTextGO.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            var closeText = closeTextGO.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = 18;
            closeText.fontStyle = TMPro.FontStyles.Bold;
            closeText.color = Color.white;
            closeText.alignment = TextAlignmentOptions.Center;

            // Content area
            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(windowGO.transform, false);
            var contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(10, 10);
            contentRect.offsetMax = new Vector2(-10, -50);

            // Store reference for later use
            var popWinData = windowGO.AddComponent<PopWinData>();
            popWinData.titleText = titleText;
            popWinData.contentArea = contentRect;
            popWinData.closeButton = closeGO;

            // Animate open
            StartCoroutine(OpenWindowAnimation(windowGO));

            return windowGO;
        }

        /// <summary>
        /// Close a window with animation.
        /// </summary>
        public void CloseWindow(GameObject window)
        {
            if (window == null) return;
            StartCoroutine(CloseWindowAnimation(window));
        }

        private IEnumerator OpenWindowAnimation(GameObject window)
        {
            var rect = window.GetComponent<RectTransform>();
            float elapsed = 0f;
            Vector3 startScale = Vector3.zero;
            Vector3 endScale = Vector3.one;

            rect.localScale = startScale;

            while (elapsed < openDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / openDuration;
                t = t * (2 - t); // Ease out
                rect.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            rect.localScale = endScale;
        }

        private IEnumerator CloseWindowAnimation(GameObject window)
        {
            var rect = window.GetComponent<RectTransform>();
            float elapsed = 0f;
            Vector3 startScale = Vector3.one;
            Vector3 endScale = Vector3.zero;

            while (elapsed < closeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / closeDuration;
                rect.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            Destroy(window);
        }

        #endregion

        #region Inventory Window

        /// <summary>
        /// Create an inventory window with slots.
        /// </summary>
        public GameObject CreateInventoryWindow(string title, int columns = 5, int rows = 4, Action<int, int> onSlotClicked = null)
        {
            columns = columns > 0 ? columns : defaultInventoryColumns;
            rows = rows > 0 ? rows : defaultInventoryRows;

            float width = columns * (slotSize + slotSpacing) + slotSpacing * 2;
            float height = rows * (slotSize + slotSpacing) + slotSpacing * 2 + 80; // +80 for title bar

            var window = CreateWindow(title, width, height);
            var popWinData = window.GetComponent<PopWinData>();

            // Create grid of slots
            var contentArea = popWinData.contentArea;
            var slotContainer = new GameObject("SlotContainer");
            slotContainer.transform.SetParent(contentArea, false);

            var containerRect = slotContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            // Create slots
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    int slotIndex = row * columns + col;
                    var slot = CreateInventorySlot(slotIndex, containerRect, col, row, onSlotClicked);
                }
            }

            return window;
        }

        private GameObject CreateInventorySlot(int index, RectTransform parent, int col, int row, Action<int, int> onSlotClicked = null)
        {
            var slotGO = new GameObject($"Slot_{index}");
            slotGO.transform.SetParent(parent, false);

            var rect = slotGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(slotSize, slotSize);
            
            float x = slotSpacing + col * (slotSize + slotSpacing);
            float y = slotSpacing + row * (slotSize + slotSpacing);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);

            // Slot background
            var bgImage = slotGO.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            // Slot border
            var borderGO = new GameObject("Border");
            borderGO.transform.SetParent(slotGO.transform, false);
            var borderRect = borderGO.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            var borderImage = borderGO.AddComponent<Image>();
            borderImage.color = new Color(0.4f, 0.4f, 0.45f, 1f);
            borderImage.type = Image.Type.Sliced;
            borderImage.sprite = windowBackground;

            // Item icon placeholder
            var iconGO = new GameObject("ItemIcon");
            iconGO.transform.SetParent(slotGO.transform, false);
            var iconRect = iconGO.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(slotSize * 0.7f, slotSize * 0.7f);
            var iconImage = iconGO.AddComponent<Image>();
            iconImage.color = new Color(1, 1, 1, 0); // Hidden by default

            // Quantity text
            var qtyGO = new GameObject("Quantity");
            qtyGO.transform.SetParent(slotGO.transform, false);
            var qtyRect = qtyGO.AddComponent<RectTransform>();
            qtyRect.anchorMin = new Vector2(1, 0);
            qtyRect.anchorMax = new Vector2(1, 0);
            qtyRect.pivot = new Vector2(1, 0);
            qtyRect.anchoredPosition = new Vector2(-5, 5);
            qtyRect.sizeDelta = new Vector2(50, 20);
            var qtyText = qtyGO.AddComponent<TextMeshProUGUI>();
            qtyText.text = "";
            qtyText.fontSize = 14;
            qtyText.fontStyle = TMPro.FontStyles.Bold;
            qtyText.color = Color.white;
            qtyText.alignment = TextAlignmentOptions.Right;
            qtyText.shadowColor = Color.black;
            qtyText.enableWordWrapping = false;

            // Button for interaction
            var button = slotGO.AddComponent<Button>();
            if (onSlotClicked != null)
            {
                button.onClick.AddListener(() => onSlotClicked(index, slotIndex));
            }

            // Store slot data
            var slotData = slotGO.AddComponent<InventorySlotData>();
            slotData.slotIndex = index;
            slotData.iconImage = iconImage;
            slotData.quantityText = qtyText;

            return slotGO;
        }

        /// <summary>
        /// Update an inventory slot with item data.
        /// </summary>
        public void UpdateInventorySlot(GameObject window, int slotIndex, Sprite icon, int quantity)
        {
            var slotData = window.GetComponentInChildren<InventorySlotData>();
            if (slotData == null || slotData.slotIndex != slotIndex) return;

            if (icon != null)
            {
                slotData.iconImage.sprite = icon;
                slotData.iconImage.color = Color.white;
            }
            else
            {
                slotData.iconImage.color = new Color(1, 1, 1, 0);
            }

            slotData.quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }

        #endregion

        #region Stats Board Window

        /// <summary>
        /// Create a stats board window showing player attributes.
        /// Prepend feature for future stat viewing and management.
        /// </summary>
        public GameObject CreateStatsBoardWindow(string title = "Character Stats")
        {
            var window = CreateWindow(title, 450f, 500f);
            var popWinData = window.GetComponent<PopWinData>();
            var contentArea = popWinData.contentArea;

            // Create scrollable content
            var scrollGO = new GameObject("ScrollContent");
            scrollGO.transform.SetParent(contentArea, false);
            var scrollRect = scrollGO.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;

            var scrollView = scrollGO.AddComponent<ScrollView>();
            scrollView.horizontal = false;
            scrollView.vertical = true;

            var viewport = scrollView.viewport;
            var content = scrollView.content;

            // Stats categories
            CreateStatCategory(content, "VITAL STATISTICS", new Vector2(10, -10));
            CreateStatRow(content, "Health", "1000 / 1000", 0);
            CreateStatRow(content, "Mana", "500 / 500", 1);
            CreateStatRow(content, "Stamina", "100 / 100", 2);

            CreateStatCategory(content, "ATTRIBUTES", new Vector2(10, -150));
            CreateStatRow(content, "Strength", "15", 3);
            CreateStatRow(content, "Agility", "12", 4);
            CreateStatRow(content, "Intelligence", "18", 5);
            CreateStatRow(content, "Vitality", "20", 6);
            CreateStatRow(content, "Dexterity", "14", 7);

            CreateStatCategory(content, "COMBAT STATS", new Vector2(10, -300));
            CreateStatRow(content, "Damage", "45-62", 8);
            CreateStatRow(content, "Defense", "28", 9);
            CreateStatRow(content, "Crit Chance", "5.2%", 10);
            CreateStatRow(content, "Crit Damage", "150%", 11);

            return window;
        }

        private void CreateStatCategory(RectTransform parent, string categoryName, Vector2 position)
        {
            var categoryGO = new GameObject($"Category_{categoryName}");
            categoryGO.transform.SetParent(parent, false);

            var rect = categoryGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(430, 30);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;

            var bgImage = categoryGO.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.35f, 1f);

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(categoryGO.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            var text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = categoryName;
            text.fontSize = 16;
            text.fontStyle = TMPro.FontStyles.Bold;
            text.color = new Color(1f, 0.9f, 0.3f);
            text.alignment = TextAlignmentOptions.Left;
        }

        private void CreateStatRow(RectTransform parent, string label, string value, int index)
        {
            var rowGO = new GameObject($"Stat_{label}");
            rowGO.transform.SetParent(parent, false);

            var rect = rowGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(430, 25);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(10, -35 - index * 30);

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(rowGO.transform, false);
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.5f, 1);
            labelRect.offsetMin = new Vector2(10, 2);
            labelRect.offsetMax = new Vector2(-5, -2);
            var labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 14;
            labelText.color = new Color(0.9f, 0.9f, 0.9f);
            labelText.alignment = TextAlignmentOptions.Left;

            var valueGO = new GameObject("Value");
            valueGO.transform.SetParent(rowGO.transform, false);
            var valueRect = valueGO.AddComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.5f, 0);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.offsetMin = new Vector2(5, 2);
            valueRect.offsetMax = new Vector2(-10, -2);
            var valueText = valueGO.AddComponent<TextMeshProUGUI>();
            valueText.text = value;
            valueText.fontSize = 14;
            valueText.fontStyle = TMPro.FontStyles.Bold;
            valueText.color = Color.white;
            valueText.alignment = TextAlignmentOptions.Right;
        }

        /// <summary>
        /// Update stats board with current player stats.
        /// </summary>
        public void UpdateStatsBoard(GameObject window, PlayerStats stats)
        {
            if (stats == null) return;

            // TODO: Implement dynamic stat updates from PlayerStats
            // This is a prepend feature for future implementation
        }

        #endregion

        #region Utility

        /// <summary>
        /// Get the content area of a window for custom content.
        /// </summary>
        public RectTransform GetContentArea(GameObject window)
        {
            var popWinData = window?.GetComponent<PopWinData>();
            return popWinData?.contentArea;
        }

        /// <summary>
        /// Set window title.
        /// </summary>
        public void SetWindowTitle(GameObject window, string newTitle)
        {
            var popWinData = window?.GetComponent<PopWinData>();
            if (popWinData?.titleText != null)
            {
                popWinData.titleText.text = newTitle;
            }
        }

        /// <summary>
        /// Clear all popup windows.
        /// </summary>
        public void ClearAllWindows()
        {
            foreach (Transform child in _windowParent)
            {
                Destroy(child.gameObject);
            }
        }

        #endregion

        #region Helper Classes

        [Serializable]
        public class PopWinData : MonoBehaviour
        {
            public TextMeshProUGUI titleText;
            public RectTransform contentArea;
            public GameObject closeButton;
        }

        [Serializable]
        public class InventorySlotData : MonoBehaviour
        {
            public int slotIndex;
            public Image iconImage;
            public TextMeshProUGUI quantityText;
        }

        #endregion
    }
}
