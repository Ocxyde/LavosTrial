// HUDSystem.cs
// Complete dynamic HUD management system - Plug-in-and-Out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Central UI manager that plugs into EventHandler
// Features:
// - Health/Mana/Stamina bars with smooth animations (via EventHandler)
// - Hotbar with 5 slots (keys 1-5)
// - Status effects panel (buffs/debuffs via EventHandler)
// - Interaction prompts
// - Floating combat text (damage/heal via EventHandler)
// - Notifications
// - Auto-constructs at runtime
//
// Plug-in-and-Out: This system plugs into EventHandler for all game events

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
    /// Plugs into EventHandler for all stat updates and game events.
    /// 
    /// SETUP: Attach to empty GameObject "HUDSystem" - everything else is automatic.
    /// </summary>
    public class HUDSystem : MonoBehaviour
    {
        public static HUDSystem Instance { get; private set; }

        #region Inspector Settings

        [Header("UI Settings")]
        [SerializeField] private bool autoConstruct = true;

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

        #endregion

        #region UI References (Auto-constructed)

        // Root
        private GameObject _hudRoot;
        private Canvas _canvas;

        // Bars (UIBarsSystem-style)
        private RectTransform _healthBarRoot;
        private RectTransform _manaBarRoot;
        private RectTransform _staminaBarRoot;
        private Image _healthFill;
        private Image _manaFill;
        private Image _staminaFill;
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
        private Transform _effectsContent;
        private Dictionary<string, GameObject> _activeEffects = new Dictionary<string, GameObject>();

        // Floating Text
        private GameObject _floatingTextRoot;

        #endregion

        #region State

        private PlayerStats _playerStats;
        private Inventory _inventory;
        private int _activeHotbarSlot = 0;
        private bool _isSubscribed = false;

        // Current values for smooth interpolation
        private float _currentHealth, _maxHealth;
        private float _currentMana, _maxMana;
        private float _currentStamina, _maxStamina;

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

            Debug.Log("[HUDSystem] Initialized");
        }

        private void Start()
        {
            // Find player references
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerStats = player.GetComponent<PlayerStats>();
                _inventory = player.GetComponent<Inventory>();
                
                // Initialize with current values
                if (_playerStats != null)
                {
                    _currentHealth = _playerStats.CurrentHealth;
                    _maxHealth = _playerStats.MaxHealth;
                    _currentMana = _playerStats.CurrentMana;
                    _maxMana = _playerStats.MaxMana;
                    _currentStamina = _playerStats.CurrentStamina;
                    _maxStamina = _playerStats.MaxStamina;
                    
                    UpdateAllBars();
                }
            }

            SubscribeToEvents();
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

        private void OnEnable()
        {
            // Re-subscribe if component is re-enabled
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region HUD Construction

        private void ConstructHUD()
        {
            CreateCanvas();
            CreateBars();
            CreateHotbar();
            CreateStatusEffectsPanel();
            CreateFloatingTextRoot();
        }

        private void CreateCanvas()
        {
            // Setup Canvas on this GameObject
            _canvas = gameObject.GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
            }
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            // Canvas Scaler
            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            // Graphic Raycaster
            if (gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            // Create root container for HUD elements
            _hudRoot = new GameObject("HUDContainer");
            _hudRoot.transform.SetParent(transform);
            _hudRoot.transform.SetAsFirstSibling(); // Keep at top of hierarchy
            
            Debug.Log("[HUDSystem] Canvas and HUDContainer created successfully");
        }

        private void CreateBars()
        {
            // Ensure _hudRoot exists
            if (_hudRoot == null)
            {
                Debug.LogError("[HUDSystem] HUDContainer not created! Cannot create bars.");
                return;
            }
            
            // Ensure Canvas exists
            if (_canvas == null)
            {
                Debug.LogError("[HUDSystem] Canvas not created! Cannot create bars.");
                return;
            }

            Debug.Log("[HUDSystem] Creating bars with screen-edge layout...");

            // Initialize colors with defaults if not set
            Color healthCol = (healthColorHigh.a > 0) ? healthColorHigh : Color.red;
            Color manaCol = (manaColor.a > 0) ? manaColor : Color.blue;
            Color staminaCol = (staminaColor.a > 0) ? staminaColor : Color.yellow;

            // Health Bar (Left edge - Vertical) - using UIBarsSystem proven method
            _healthBarRoot = CreateBarContainerEdge(
                "HealthBar", _hudRoot.transform,
                Image.FillMethod.Vertical, healthCol,
                BarPosition.LeftEdge
            );
            _healthFill = _healthBarRoot.Find("Fill").GetComponent<Image>();
            _healthText = CreateBarText(_healthBarRoot, "HealthText");
            Debug.Log($"[HUDSystem] Health bar created: {_healthBarRoot != null}");

            // Mana Bar (Right edge - Vertical)
            _manaBarRoot = CreateBarContainerEdge(
                "ManaBar", _hudRoot.transform,
                Image.FillMethod.Vertical, manaCol,
                BarPosition.RightEdge
            );
            _manaFill = _manaBarRoot.Find("Fill").GetComponent<Image>();
            _manaText = CreateBarText(_manaBarRoot, "ManaText");
            Debug.Log($"[HUDSystem] Mana bar created: {_manaBarRoot != null}");

            // Stamina Bar (Bottom edge - Horizontal)
            _staminaBarRoot = CreateBarContainerEdge(
                "StaminaBar", _hudRoot.transform,
                Image.FillMethod.Horizontal, staminaCol,
                BarPosition.BottomEdge
            );
            _staminaFill = _staminaBarRoot.Find("Fill").GetComponent<Image>();
            _staminaText = CreateBarText(_staminaBarRoot, "StaminaText");
            Debug.Log($"[HUDSystem] Stamina bar created: {_staminaBarRoot != null}");

            Debug.Log("[HUDSystem] All bars created with screen-edge layout!");
        }

        private enum BarPosition { LeftEdge, RightEdge, BottomEdge }

        /// <summary>
        /// Create a bar container positioned on screen edge (proven UIBarsSystem method).
        /// </summary>
        private RectTransform CreateBarContainerEdge(
            string name, Transform parent,
            Image.FillMethod fillMethod, Color fillColor,
            BarPosition position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            // Use center anchors for absolute positioning
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;

            // Set position and size based on bar type
            if (fillMethod == Image.FillMethod.Vertical)
            {
                // Vertical bars (Health/Mana) - 70% screen height
                rt.sizeDelta = new Vector2(25f, 1080f * 0.70f);
                
                if (position == BarPosition.LeftEdge)
                {
                    rt.anchoredPosition = new Vector2(-960f + 25f, 0f); // Left edge
                }
                else if (position == BarPosition.RightEdge)
                {
                    rt.anchoredPosition = new Vector2(960f - 25f, 0f); // Right edge
                }
            }
            else
            {
                // Horizontal bar (Stamina) - 70% screen width
                rt.sizeDelta = new Vector2(1920f * 0.70f, 25f);
                rt.anchoredPosition = new Vector2(0f, -540f + 25f); // Bottom edge
            }

            // Background
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // Border
            CreateBarBorder(go.transform, new Color(0.3f, 0.3f, 0.35f));

            // Fill
            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(go.transform, false);

            var fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = new Vector2(2, 2);
            fillRT.offsetMax = new Vector2(-2, -2);

            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = fillMethod;
            fillImg.fillOrigin = (fillMethod == Image.FillMethod.Vertical) ? 0 : 0;
            fillImg.fillAmount = 1f;

            return rt;
        }

        /// <summary>
        /// Create a 4-pixel dark border around the bar.
        /// </summary>
        private void CreateBarBorder(Transform parent, Color color)
        {
            float borderSize = 4f;

            // Top border
            CreateBorderEdge("Border_Top", parent,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -borderSize), Vector2.zero, color);

            // Bottom border
            CreateBorderEdge("Border_Bottom", parent,
                new Vector2(0, 0), new Vector2(1, 0),
                Vector2.zero, new Vector2(0, borderSize), color);

            // Left border
            CreateBorderEdge("Border_Left", parent,
                new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(-borderSize, 0), Vector2.zero, color);

            // Right border
            CreateBorderEdge("Border_Right", parent,
                new Vector2(1, 0), new Vector2(1, 1),
                Vector2.zero, new Vector2(borderSize, 0), color);
        }

        private void CreateBorderEdge(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            rt.localScale = Vector3.one;

            go.AddComponent<Image>().color = color;
        }

            // Interaction prompt - Top center
            var interactionGO = new GameObject("InteractionPrompt");
            if (_hudRoot != null)
                interactionGO.transform.SetParent(_hudRoot.transform);
            var interactionRect = interactionGO.AddComponent<RectTransform>();
            interactionRect.anchorMin = new Vector2(0.5f, 1f);
            interactionRect.anchorMax = new Vector2(0.5f, 1f);
            interactionRect.pivot = new Vector2(0.5f, 1f);
            interactionRect.anchoredPosition = new Vector2(0, -100);
            interactionRect.sizeDelta = new Vector2(400, 50);

            _interactionText = interactionGO.AddComponent<TextMeshProUGUI>();
            _interactionText.fontSize = 24;
            _interactionText.alignment = TextAlignmentOptions.Center;
            _interactionText.color = Color.white;
            _interactionText.text = "";
            _interactionText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private TextMeshProUGUI CreateOverlayText(GameObject parent, string name, string text)
        {
            var textGO = new GameObject(name);
            textGO.transform.SetParent(parent.transform);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            var textComp = textGO.AddComponent<TextMeshProUGUI>();
            textComp.fontSize = 16;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.color = Color.white;
            textComp.text = text;
            textComp.textWrappingMode = TextWrappingModes.NoWrap;

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

            Debug.Log("[HUDSystem] Status effects panel created");
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

        /// <summary>
        /// Subscribe to all relevant EventHandler events.
        /// This is the plug-in point for the HUD system.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_isSubscribed) return;
            
            if (EventHandler.Instance == null)
            {
                Debug.LogWarning("[HUDSystem] EventHandler not found - will retry in 0.5s");
                Invoke(nameof(SubscribeToEvents), 0.5f);
                return;
            }

            // Player stat events
            EventHandler.Instance.OnPlayerHealthChanged += OnHealthChanged;
            EventHandler.Instance.OnPlayerDamaged += OnPlayerDamaged;
            EventHandler.Instance.OnPlayerHealed += OnPlayerHealed;
            EventHandler.Instance.OnPlayerManaChanged += OnManaChanged;
            EventHandler.Instance.OnPlayerStaminaChanged += OnStaminaChanged;

            // Status effect events
            EventHandler.Instance.OnStatChanged += OnStatChanged;

            // Floating text / notifications
            EventHandler.Instance.OnFloatingTextRequested += ShowFloatingText;
            EventHandler.Instance.OnNotificationRequested += ShowNotification;

            _isSubscribed = true;
            Debug.Log("[HUDSystem] Plugged into EventHandler - All events subscribed");
        }

        private void UnsubscribeFromEvents()
        {
            if (EventHandler.Instance == null) return;

            // Player stat events
            EventHandler.Instance.OnPlayerHealthChanged -= OnHealthChanged;
            EventHandler.Instance.OnPlayerDamaged -= OnPlayerDamaged;
            EventHandler.Instance.OnPlayerHealed -= OnPlayerHealed;
            EventHandler.Instance.OnPlayerManaChanged -= OnManaChanged;
            EventHandler.Instance.OnPlayerStaminaChanged -= OnStaminaChanged;

            // Status effect events
            EventHandler.Instance.OnStatChanged -= OnStatChanged;

            // Floating text / notifications
            EventHandler.Instance.OnFloatingTextRequested -= ShowFloatingText;
            EventHandler.Instance.OnNotificationRequested -= ShowNotification;
        }

        #endregion

        #region Event Handlers

        private void OnHealthChanged(float current, float max)
        {
            _currentHealth = current;
            _maxHealth = max;
            UpdateBar(_healthBarFill, _healthText, current, max, healthColorHigh, healthColorLow, healthColorCritical);
        }

        private void OnPlayerDamaged(float amount)
        {
            // Flash health bar red
            StartCoroutine(FlashBar(_healthBarFill, Color.red, 0.3f));
            
            // Show damage floating text
            ShowFloatingText($"-{Mathf.CeilToInt(amount)}", Color.red, 1.5f);
        }

        private void OnPlayerHealed(float amount)
        {
            // Show heal floating text
            ShowFloatingText($"+{Mathf.CeilToInt(amount)}", Color.green, 1.5f);
        }

        private void OnManaChanged(float current, float max)
        {
            _currentMana = current;
            _maxMana = max;
            UpdateBar(_manaBarFill, _manaText, current, max, manaColor, manaColor, manaColor);
        }

        private void OnStaminaChanged(float current, float max)
        {
            _currentStamina = current;
            _maxStamina = max;
            UpdateBar(_staminaBarFill, _staminaText, current, max, staminaColor, staminaColor, staminaColor);
        }

        private void OnStatChanged(string statName, float newValue)
        {
            // Handle custom stat changes if needed (silent)
        }

        private void UpdateAllBars()
        {
            UpdateBar(_healthBarFill, _healthText, _currentHealth, _maxHealth, healthColorHigh, healthColorLow, healthColorCritical);
            UpdateBar(_manaBarFill, _manaText, _currentMana, _maxMana, manaColor, manaColor, manaColor);
            UpdateBar(_staminaBarFill, _staminaText, _currentStamina, _maxStamina, staminaColor, staminaColor, staminaColor);
        }

        private void UpdateBar(Image fill, TextMeshProUGUI text, float current, float max, Color high, Color mid, Color low)
        {
            if (fill == null || text == null) return;

            float percent = max > 0 ? current / max : 0;
            fill.fillAmount = Mathf.Clamp01(percent);
            text.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";

            // Color interpolation based on resource level
            if (percent > 0.6f)
                fill.color = Color.Lerp(mid, high, (percent - 0.6f) / 0.4f);
            else if (percent > 0.3f)
                fill.color = Color.Lerp(low, mid, (percent - 0.3f) / 0.3f);
            else
                fill.color = low;
        }

        private IEnumerator FlashBar(Image bar, Color flashColor, float duration)
        {
            Color originalColor = bar.color;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bar.color = Color.Lerp(originalColor, flashColor, Mathf.Sin(elapsed / duration * Mathf.PI));
                yield return null;
            }
            
            bar.color = originalColor;
        }

        public void ShowFloatingText(string text, Color color, float duration = 1.5f)
        {
            if (string.IsNullOrEmpty(text)) return;
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
            textComp.fontSize = 24;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.color = color;
            textComp.text = text;
            textComp.textWrappingMode = TextWrappingModes.NoWrap;

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

        private void ShowNotification(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            StartCoroutine(NotificationRoutine(message));
        }

        private IEnumerator NotificationRoutine(string message)
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
            textComp.fontSize = 20;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.color = Color.white;
            textComp.text = message;

            yield return new WaitForSeconds(3f);
            Destroy(notifGO);
        }

        #endregion

        #region Hotbar System

        private void HandleHotbarInput()
        {
            // Use Keyboard.current from New Input System
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;

            for (int i = 0; i < hotbarSlotCount; i++)
            {
                var key = keyboard[i == 0 ? UnityEngine.InputSystem.Key.Digit1 : 
                                   i == 1 ? UnityEngine.InputSystem.Key.Digit2 :
                                   i == 2 ? UnityEngine.InputSystem.Key.Digit3 :
                                   i == 3 ? UnityEngine.InputSystem.Key.Digit4 :
                                   UnityEngine.InputSystem.Key.Digit5];
                
                if (key != null && key.wasPressedThisFrame)
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

        #region Status Effects System

        /// <summary>
        /// Add a status effect to the display.
        /// Called via EventHandler when status effects are applied.
        /// </summary>
        public void AddStatusEffect(StatusEffectData effectData, float duration, int stacks = 1)
        {
            if (_effectsContent == null) return;

            string effectKey = effectData.effectName;
            
            // If effect already exists, update stacks
            if (_activeEffects.ContainsKey(effectKey))
            {
                UpdateStatusEffect(effectKey, stacks, duration);
                return;
            }

            // Create new effect icon
            var effectGO = CreateEffectIcon(effectData, stacks);
            _activeEffects[effectKey] = effectGO;
        }

        /// <summary>
        /// Remove a status effect from the display.
        /// </summary>
        public void RemoveStatusEffect(string effectName)
        {
            if (_activeEffects.ContainsKey(effectName))
            {
                Destroy(_activeEffects[effectName]);
                _activeEffects.Remove(effectName);
            }
        }

        /// <summary>
        /// Update an existing status effect.
        /// </summary>
        private void UpdateStatusEffect(string effectKey, int stacks, float duration)
        {
            var effectGO = _activeEffects[effectKey];
            if (effectGO == null) return;

            // Update stacks text
            var stackText = effectGO.GetComponentInChildren<TextMeshProUGUI>();
            if (stackText != null && stacks > 1)
            {
                stackText.text = stacks.ToString();
            }

            // Update duration bar if present
            var durationBar = effectGO.GetComponentInChildren<Image>();
            if (durationBar != null)
            {
                StartCoroutine(UpdateDurationBar(durationBar, duration));
            }
        }

        private GameObject CreateEffectIcon(StatusEffectData effectData, int stacks)
        {
            var effectGO = new GameObject("Effect_" + effectData.effectName);
            effectGO.transform.SetParent(_effectsContent);
            var rect = effectGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(effectIconSize, effectIconSize);

            // Icon background
            var bg = effectGO.AddComponent<Image>();
            bg.color = effectData.effectType == EffectType.Buff ? 
                new Color(0.2f, 0.5f, 0.2f, 0.8f) : 
                new Color(0.5f, 0.2f, 0.2f, 0.8f);

            // Duration bar (child)
            var barGO = new GameObject("DurationBar");
            barGO.transform.SetParent(effectGO.transform);
            var barRect = barGO.AddComponent<RectTransform>();
            barRect.anchorMin = Vector2.zero;
            barRect.anchorMax = new Vector2(1, 0);
            barRect.pivot = new Vector2(0, 0);
            barRect.anchoredPosition = new Vector2(0, 0);
            barRect.sizeDelta = new Vector2(0, 4);
            var barImage = barGO.AddComponent<Image>();
            barImage.color = effectData.effectType == EffectType.Buff ? Color.green : Color.red;
            barImage.type = Image.Type.Filled;
            barImage.fillMethod = Image.FillMethod.Horizontal;
            barImage.fillAmount = 1f;

            // Stacks text (if > 1)
            if (stacks > 1)
            {
                var stackTextGO = new GameObject("Stacks");
                stackTextGO.transform.SetParent(effectGO.transform);
                var stackRect = stackTextGO.AddComponent<RectTransform>();
                stackRect.anchorMin = new Vector2(1, 0);
                stackRect.anchorMax = new Vector2(1, 0);
                stackRect.pivot = new Vector2(1, 0);
                stackRect.anchoredPosition = new Vector2(-5, 5);
                stackRect.sizeDelta = new Vector2(30, 30);
                var stackText = stackTextGO.AddComponent<TextMeshProUGUI>();
                stackText.fontSize = 14;
                stackText.color = Color.white;
                stackText.text = stacks.ToString();
            }

            return effectGO;
        }

        private IEnumerator UpdateDurationBar(Image bar, float duration)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bar.fillAmount = 1f - (elapsed / duration);
                yield return null;
            }
            bar.fillAmount = 0f;
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
