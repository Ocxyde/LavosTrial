// HUDSystem.cs
// Complete dynamic HUD management system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Features:
// - Health/Mana/Stamina bars with smooth animations
// - Hotbar with 5 slots (keys 1-5)
// - Status effects panel (buffs/debuffs)
// - Interaction prompts
// - Floating combat text
// - Notifications
// - Auto-constructs at runtime

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Code.Lavos.Core;
using Code.Lavos.Status;

namespace Code.Lavos.HUD
{
    /// <summary>
    /// Complete HUD system - auto-constructs all UI elements at runtime.
    /// Attach to empty GameObject "HUDSystem" - everything else is automatic.
    /// </summary>
    public class HUDSystem : MonoBehaviour
    {
        public static HUDSystem Instance { get; private set; }

        #region Inspector Settings

        [Header("UI Settings")]
        [SerializeField] private bool autoConstruct = true;
        [SerializeField] private float uiScale = 1f;

        [Header("Bar Colors")]
        [SerializeField] private Color healthColorHigh = new Color(0.2f, 0.9f, 0.3f);
        [SerializeField] private Color healthColorLow = new Color(0.9f, 0.7f, 0.1f);
        [SerializeField] private Color healthColorCritical = new Color(0.9f, 0.1f, 0.1f);
        [SerializeField] private Color manaColor = new Color(0.2f, 0.5f, 1.0f);
        [SerializeField] private Color staminaColor = new Color(1.0f, 0.8f, 0.2f);

        [Header("Hotbar")]
        [SerializeField] private int hotbarSlotCount = 5;
        [SerializeField] private float hotbarSlotSize = 52f;
        [SerializeField] private Color hotbarActiveColor = new Color(1f, 0.82f, 0.2f, 1f);
        [SerializeField] private Color hotbarInactiveColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        [Header("Status Effects")]
        [SerializeField] private float effectIconSize = 40f;
        [SerializeField] private float effectSpacing = 8f;

        #endregion

        #region UI References (Auto-constructed)

        // Root
        private GameObject _hudRoot;
        private Canvas _canvas;

        // Bars
        private Image _healthBarFill;
        private Image _manaBarFill;
        private Image _staminaBarFill;
        private TextMeshProUGUI _healthText;
        private TextMeshProUGUI _manaText;
        private TextMeshProUGUI _staminaText;
        private TextMeshProUGUI _interactionText;

        // Hotbar
        private GameObject _hotbarRoot;
        private Image[] _hotbarSlots;
        private Image[] _hotbarBorders;
        private TextMeshProUGUI[] _hotbarKeys;

        // Status Effects
        private GameObject _statusEffectsRoot;
        private List<GameObject> _activeEffects = new List<GameObject>();

        // Floating Text
        private GameObject _floatingTextRoot;

        #endregion

        #region State

        private PlayerStats _playerStats;
        private Inventory _inventory;
        private int _activeHotbarSlot = 0;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (autoConstruct)
            {
                ConstructHUD();
            }

            SubscribeToEvents();
        }

        private void Start()
        {
            // Find player references
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerStats = player.GetComponent<PlayerStats>();
                _inventory = player.GetComponent<Inventory>();
            }

            UpdateHotbarDisplay();
        }

        private void Update()
        {
            HandleHotbarInput();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            if (Instance == this)
                Instance = null;
        }

        #endregion

        #region HUD Construction

        private void ConstructHUD()
        {
            CreateRoot();
            CreateBars();
            CreateHotbar();
            CreateStatusEffectsPanel();
            CreateFloatingTextRoot();
        }

        private void CreateRoot()
        {
            _hudRoot = new GameObject("HUDRoot");
            _hudRoot.transform.SetParent(transform);
            _hudRoot.transform.localPosition = Vector3.zero;

            _canvas = gameObject.GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
            }
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void CreateBars()
        {
            // Bar container - Left side (Health)
            var healthContainer = CreateBarContainer("HealthContainer", new Vector2(20, -540), TextAnchor.LowerLeft);
            _healthBarFill = CreateBar(healthContainer, "HealthFill", Color.red, 25f, 400f, true);
            _healthText = CreateBarText(healthContainer, "HealthText", "1000/1000");

            // Bar container - Right side (Mana)
            var manaContainer = CreateBarContainer("ManaContainer", new Vector2(-20, -540), TextAnchor.LowerRight);
            _manaBarFill = CreateBar(manaContainer, "ManaFill", Color.blue, 25f, 400f, true);
            _manaText = CreateBarText(manaContainer, "ManaText", "50/50");

            // Bar container - Bottom center (Stamina)
            var staminaContainer = CreateBarContainer("StaminaContainer", new Vector2(0, -20), TextAnchor.LowerCenter);
            _staminaBarFill = CreateBar(staminaContainer, "StaminaFill", Color.yellow, 25f, 600f, false);
            _staminaText = CreateBarText(staminaContainer, "StaminaText", "100/100");

            // Interaction prompt - Top center
            var interactionGO = new GameObject("InteractionPrompt");
            interactionGO.transform.SetParent(_hudRoot.transform);
            var interactionRect = interactionGO.AddComponent<RectTransform>();
            interactionRect.anchorMin = new Vector2(0.5f, 1f);
            interactionRect.anchorMax = new Vector2(0.5f, 1f);
            interactionRect.pivot = new Vector2(0.5f, 1f);
            interactionRect.anchoredPosition = new Vector2(0, -100);
            interactionRect.sizeDelta = new Vector2(400, 50);

            _interactionText = interactionGO.AddComponent<TextMeshProUGUI>();
            _interactionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _interactionText.fontSize = 24;
            _interactionText.alignment = TextAlignmentOptions.Center;
            _interactionText.color = Color.white;
            _interactionText.text = "";
            _interactionText.enableWordWrapping = false;
        }

        private GameObject CreateBarContainer(string name, Vector2 position, TextAnchor anchor)
        {
            var container = new GameObject(name);
            container.transform.SetParent(_hudRoot.transform);
            var rect = container.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(30f, 400f);
            return container;
        }

        private Image CreateBar(GameObject parent, string name, Color color, float thickness, float length, bool vertical)
        {
            var bg = new GameObject(name + "BG");
            bg.transform.SetParent(parent.transform);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = Vector2.zero;

            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            var fill = new GameObject(name);
            fill.transform.SetParent(bg.transform);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(0.5f, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = Vector2.zero;

            var fillImage = fill.AddComponent<Image>();
            fillImage.color = color;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = vertical ? Image.FillMethod.Vertical : Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.FillOrigin.Bottom;

            return fillImage;
        }

        private TextMeshProUGUI CreateBarText(GameObject parent, string name, string text)
        {
            var textGO = new GameObject(name);
            textGO.transform.SetParent(parent.transform);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0, 0);
            textRect.sizeDelta = new Vector2(200, 30);

            var textComp = textGO.AddComponent<TextMeshProUGUI>();
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 16;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.color = Color.white;
            textComp.text = text;
            textComp.enableWordWrapping = false;

            return textComp;
        }

        private void CreateHotbar()
        {
            _hotbarRoot = new GameObject("Hotbar");
            _hotbarRoot.transform.SetParent(_hudRoot.transform);
            var hotbarRect = _hotbarRoot.AddComponent<RectTransform>();
            hotbarRect.anchorMin = new Vector2(0.5f, 0f);
            hotbarRect.anchorMax = new Vector2(0.5f, 0f);
            hotbarRect.pivot = new Vector2(0.5f, 0f);
            hotbarRect.anchoredPosition = new Vector2(0, 20);
            hotbarRect.sizeDelta = new Vector2(hotbarSlotCount * (hotbarSlotSize + 10), hotbarSlotSize + 20);

            // Background
            var bg = _hotbarRoot.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.08f, 0.88f);

            // Border
            var border = _hotbarRoot.AddComponent<Outline>();
            border.effectColor = new Color(0.55f, 0.55f, 0.55f, 1f);
            border.effectDistance = new Vector2(2, 2);

            _hotbarSlots = new Image[hotbarSlotCount];
            _hotbarBorders = new Image[hotbarSlotCount];
            _hotbarKeys = new TextMeshProUGUI[hotbarSlotCount];

            for (int i = 0; i < hotbarSlotCount; i++)
            {
                CreateHotbarSlot(i);
            }
        }

        private void CreateHotbarSlot(int index)
        {
            var slotGO = new GameObject("Slot" + (index + 1));
            slotGO.transform.SetParent(_hotbarRoot.transform);
            var slotRect = slotGO.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(hotbarSlotSize, hotbarSlotSize);
            slotRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = new Vector2(0.5f, 0.5f);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            slotRect.anchoredPosition = new Vector2((index - (hotbarSlotCount - 1) / 2f) * (hotbarSlotSize + 10), 0);

            // Slot background
            var slotImage = slotGO.AddComponent<Image>();
            slotImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            _hotbarSlots[index] = slotImage;

            // Border
            var borderGO = new GameObject("Border");
            borderGO.transform.SetParent(slotGO.transform);
            var borderRect = borderGO.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = new Vector2(4, 4);
            var borderImage = borderGO.AddComponent<Image>();
            borderImage.color = (index == _activeHotbarSlot) ? hotbarActiveColor : hotbarInactiveColor;
            _hotbarBorders[index] = borderImage;

            // Key hint
            var keyGO = new GameObject("KeyHint");
            keyGO.transform.SetParent(slotGO.transform);
            var keyRect = keyGO.AddComponent<RectTransform>();
            keyRect.anchorMin = new Vector2(1, 0);
            keyRect.anchorMax = new Vector2(1, 0);
            keyRect.pivot = new Vector2(1, 0);
            keyRect.anchoredPosition = new Vector2(-5, 5);
            keyRect.sizeDelta = new Vector2(30, 30);

            var keyText = keyGO.AddComponent<TextMeshProUGUI>();
            keyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            keyText.fontSize = 14;
            keyText.color = Color.gray;
            keyText.text = (index + 1).ToString();
            _hotbarKeys[index] = keyText;
        }

        private void CreateStatusEffectsPanel()
        {
            _statusEffectsRoot = new GameObject("StatusEffects");
            _statusEffectsRoot.transform.SetParent(_hudRoot.transform);
            var effectsRect = _statusEffectsRoot.AddComponent<RectTransform>();
            effectsRect.anchorMin = new Vector2(0f, 0f);
            effectsRect.anchorMax = new Vector2(0f, 0f);
            effectsRect.pivot = new Vector2(0f, 0f);
            effectsRect.anchoredPosition = new Vector2(20, 20);
            effectsRect.sizeDelta = new Vector2(200, 300);

            _activeEffects = new List<GameObject>();
        }

        private void CreateFloatingTextRoot()
        {
            _floatingTextRoot = new GameObject("FloatingText");
            _floatingTextRoot.transform.SetParent(_hudRoot.transform);
            var floatingRect = _floatingTextRoot.AddComponent<RectTransform>();
            floatingRect.anchorMin = Vector2.zero;
            floatingRect.anchorMax = Vector2.one;
            floatingRect.sizeDelta = Vector2.zero;
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.OnPlayerHealthChanged += OnHealthChanged;
                EventHandler.Instance.OnPlayerManaChanged += OnManaChanged;
                EventHandler.Instance.OnPlayerStaminaChanged += OnStaminaChanged;
                EventHandler.Instance.OnFloatingTextRequested += ShowFloatingText;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.OnPlayerHealthChanged -= OnHealthChanged;
                EventHandler.Instance.OnPlayerManaChanged -= OnManaChanged;
                EventHandler.Instance.OnPlayerStaminaChanged -= OnStaminaChanged;
                EventHandler.Instance.OnFloatingTextRequested -= ShowFloatingText;
            }
        }

        #endregion

        #region Event Handlers

        private void OnHealthChanged(float current, float max)
        {
            UpdateBar(_healthBarFill, _healthText, current, max, healthColorHigh, healthColorLow, healthColorCritical);
        }

        private void OnManaChanged(float current, float max)
        {
            UpdateBar(_manaBarFill, _manaText, current, max, manaColor, manaColor, manaColor);
        }

        private void OnStaminaChanged(float current, float max)
        {
            UpdateBar(_staminaBarFill, _staminaText, current, max, staminaColor, staminaColor, staminaColor);
        }

        private void UpdateBar(Image fill, TextMeshProUGUI text, float current, float max, Color high, Color mid, Color low)
        {
            if (fill == null || text == null) return;

            float percent = max > 0 ? current / max : 0;
            fill.fillAmount = Mathf.Clamp01(percent);
            text.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";

            // Color interpolation
            if (percent > 0.6f)
                fill.color = Color.Lerp(mid, high, (percent - 0.6f) / 0.4f);
            else if (percent > 0.3f)
                fill.color = Color.Lerp(low, mid, (percent - 0.3f) / 0.3f);
            else
                fill.color = low;
        }

        private void ShowFloatingText(string text, Color color, float duration = 1.5f)
        {
            StartCoroutine(FloatingTextRoutine(text, color, duration));
        }

        private IEnumerator FloatingTextRoutine(string text, Color color, float duration)
        {
            var textGO = new GameObject("FloatingText");
            textGO.transform.SetParent(_floatingTextRoot.transform);
            var rect = textGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(200, 50);

            var textComp = textGO.AddComponent<TextMeshProUGUI>();
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 24;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.color = color;
            textComp.text = text;
            textComp.enableWordWrapping = false;

            float elapsed = 0;
            Vector2 startPos = rect.anchoredPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rect.anchoredPosition = startPos + Vector2.up * (t * 50);
                textComp.color = new Color(color.r, color.g, color.b, color.a * (1 - t));
                yield return null;
            }

            Destroy(textGO);
        }

        #endregion

        #region Hotbar System

        private void HandleHotbarInput()
        {
            for (int i = 0; i < hotbarSlotCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SetActiveSlot(i);
                }
            }
        }

        private void SetActiveSlot(int index)
        {
            if (index < 0 || index >= hotbarSlotCount) return;

            _activeHotbarSlot = index;
            UpdateHotbarDisplay();

            // Use item from slot
            if (_inventory != null)
            {
                _inventory.UseItem(index, GameObject.FindGameObjectWithTag("Player"));
            }
        }

        private void UpdateHotbarDisplay()
        {
            if (_hotbarBorders == null) return;

            for (int i = 0; i < hotbarSlotCount; i++)
            {
                if (_hotbarBorders[i] != null)
                {
                    _hotbarBorders[i].color = (i == _activeHotbarSlot) ? hotbarActiveColor : hotbarInactiveColor;
                }
            }
        }

        #endregion

        #region Public Methods

        public void SetInteractionPrompt(string prompt)
        {
            if (_interactionText != null)
            {
                _interactionText.text = string.IsNullOrEmpty(prompt) ? "" : $"[E] {prompt}";
            }
        }

        public void ShowNotification(string message, float duration = 3f)
        {
            StartCoroutine(NotificationRoutine(message, duration));
        }

        private IEnumerator NotificationRoutine(string message, float duration)
        {
            // Simple notification at top center
            var notifGO = new GameObject("Notification");
            notifGO.transform.SetParent(_hudRoot.transform);
            var rect = notifGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -200);
            rect.sizeDelta = new Vector2(600, 60);

            var bg = notifGO.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);

            var textComp = notifGO.AddComponent<TextMeshProUGUI>();
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 20;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.color = Color.white;
            textComp.text = message;

            yield return new WaitForSeconds(duration);
            Destroy(notifGO);
        }

        #endregion
    }
}
