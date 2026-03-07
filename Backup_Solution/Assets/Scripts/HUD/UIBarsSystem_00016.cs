// UIBarsSystem.cs
// Responsive UI bars positioned around screen edges per TODO.md specifications
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Layout Configuration:
//   - Bars positioned at 75% of screen width/height from center
//   - Bars anchored to middle-center of screen
//   - HealthBar (Left Border): vertical, 75% screen height, red color
//   - ManaBar (Right Border): vertical, 75% screen height, blue color
//   - StaminaBar (Bottom Border): horizontal, 75% screen width, yellow/green color
//   - Status Effects: top center

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Code.Lavos;
using Code.Lavos.Status;
using Code.Lavos.Core;

namespace Code.Lavos.HUD
{
    /// <summary>
    /// Screen-space responsive UI bars system.
    /// </summary>
    public class UIBarsSystem : MonoBehaviour
    {
        public static UIBarsSystem Instance { get; private set; }

        [Header("Bar Settings")]
        [SerializeField] private float barThickness = 25f;
        [SerializeField] private float barMargin = 20f;
        [SerializeField][Range(0.5f, 0.95f)] private float barSizePercent = 0.75f;

        [Header("Colors")]
        // 8-bit style colors - no alpha, brightness indicates resource level
        [SerializeField] private Color healthColorHigh = new Color(0.2f, 0.9f, 0.3f);    // Bright green (100%)
        [SerializeField] private Color healthColorLow = new Color(0.9f, 0.7f, 0.1f);     // Yellow-orange (50%)
        [SerializeField] private Color healthColorCritical = new Color(0.9f, 0.1f, 0.1f); // Red (0%)
        [SerializeField] private Color manaColor = new Color(0.2f, 0.5f, 1.0f);          // Bright blue (100%)
        [SerializeField] private Color manaColorLow = new Color(0.1f, 0.25f, 0.5f);      // Dark blue (0%)
        [SerializeField] private Color staminaColor = new Color(1.0f, 0.8f, 0.2f);       // Bright yellow (100%)
        [SerializeField] private Color staminaColorLow = new Color(0.5f, 0.4f, 0.1f);    // Dark yellow/brown (0%)
        [SerializeField] private Color borderColor = new Color(0.3f, 0.3f, 0.35f);       // Dark gray border
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.12f);   // Dark background

        [Header("Status Effects")]
        [SerializeField] private float effectIconSize = 48f;
        [SerializeField] private float effectSpacing = 8f;
        [SerializeField] private float effectRowMargin = 15f;

        [Header("Text Settings")]
        [SerializeField] private int barTextFontSize = 36;
        [SerializeField] private Color textColor = Color.black;
        [SerializeField] private bool showTextOutline = false;
        [SerializeField] private Color outlineColor = Color.black;
        [SerializeField] private float outlineWidth = 4f;

        private Canvas _canvas;
        private RectTransform _healthBarRoot;
        private RectTransform _manaBarRoot;
        private RectTransform _staminaBarRoot;
        private RectTransform _effectsRoot;

        private Image _healthFill;
        private Image _manaFill;
        private Image _staminaFill;

        private TextMeshProUGUI _healthText;
        private TextMeshProUGUI _manaText;
        private TextMeshProUGUI _staminaText;

        private Transform _effectsContainer;
        private readonly Dictionary<string, EffectIconData> _effectIcons = new();

        private struct EffectIconData
        {
            public GameObject Root;
            public Image FillBar;
            public TextMeshProUGUI StackText;
            public TextMeshProUGUI DurationText;
        }

        // Current values for smooth updates
        private float _currentHealth = 1000f;
        private float _maxHealth = 100f;
        private float _currentMana = 100f;
        private float _maxMana = 100f;
        private float _currentStamina = 100f;
        private float _maxStamina = 100f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            
            // Ensure this is a root GameObject before calling DontDestroyOnLoad
            if (transform.parent != null)
            {
                transform.SetParent(null, true);
            }
            
            DontDestroyOnLoad(gameObject);
            Debug.Log("[UIBarsSystem] Awake - Instance initialized");
        }

        void Start()
        {
            Debug.Log("[UIBarsSystem] Start - Building bars...");
            BuildBars();
            SubscribeToEvents();
            Debug.Log($"[UIBarsSystem] Bars built - Health: {_healthFill != null}, Mana: {_manaFill != null}, Stamina: {_staminaFill != null}");

            // Set initial values after a short delay to ensure everything is initialized
            Invoke(nameof(SetInitialValues), 0.2f);

            // Subscribe to EventHandler for centralized event management
            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.OnPlayerHealthChanged += (current, max) => SetHealth(current, max, showFloatingText: true);
                EventHandler.Instance.OnPlayerManaChanged += (current, max) => SetMana(current, max, showFloatingText: true);
                EventHandler.Instance.OnPlayerStaminaChanged += (current, max) => SetStamina(current, max, showFloatingText: true);
                Debug.Log("[UIBarsSystem] Subscribed to EventHandler");
            }
        }
        
        void SetInitialValues()
        {
            Debug.Log("[UIBarsSystem] Setting initial values...");

            // Try to find PlayerStats using reflection to avoid circular dependency
            var statsType = System.Type.GetType("Code.Lavos.Core.PlayerStats, Assembly-CSharp");
            MonoBehaviour playerStatsInstance = null;

            if (statsType != null)
            {
                var instanceProp = statsType.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                playerStatsInstance = instanceProp?.GetValue(null) as MonoBehaviour;
            }

            if (playerStatsInstance != null)
            {
                var statsType2 = playerStatsInstance.GetType();
                var currentHealthProp = statsType2.GetProperty("CurrentHealth");
                var maxHealthProp = statsType2.GetProperty("MaxHealth");
                var currentManaProp = statsType2.GetProperty("CurrentMana");
                var maxManaProp = statsType2.GetProperty("MaxMana");
                var currentStaminaProp = statsType2.GetProperty("CurrentStamina");
                var maxStaminaProp = statsType2.GetProperty("MaxStamina");

                float ch = currentHealthProp?.GetValue(playerStatsInstance) is float chc ? chc : 1000f;
                float mh = maxHealthProp?.GetValue(playerStatsInstance) is float mhc ? mhc : 1000f;
                float cm = currentManaProp?.GetValue(playerStatsInstance) is float cmc ? cmc : 500f;
                float mm = maxManaProp?.GetValue(playerStatsInstance) is float mmc ? mmc : 500f;
                float cs = currentStaminaProp?.GetValue(playerStatsInstance) is float csc ? csc : 100f;
                float ms = maxStaminaProp?.GetValue(playerStatsInstance) is float msc ? msc : 100f;

                SetHealth(ch, mh);
                SetMana(cm, mm);
                SetStamina(cs, ms);
                
                Debug.Log($"[UIBarsSystem] Initial values from PlayerStats - HP: {ch}/{mh}, Stamina: {cs}/{ms}");
            }
            else
            {
                // Fallback to PlayerHealth
                var playerHealth = FindFirstObjectByType<PlayerHealth>();
                if (playerHealth != null)
                {
                    SetHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
                    SetMana(100f, 100f);
                    SetStamina(100f, 100f);
                    Debug.Log($"[UIBarsSystem] Initial values from PlayerHealth - HP: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
                }
                else
                {
                    // Default values
                    SetHealth(1000f, 1000f);
                    SetMana(100f, 100f);
                    SetStamina(100f, 100f);
                    Debug.Log("[UIBarsSystem] Using default values");
                }
            }
        }

        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        void LateUpdate()
        {
            UpdateBarPositions();
            RefreshEffectTimers();
        }

        private void SubscribeToEvents()
        {
            // Subscribe to PlayerStats events (unified system) using reflection
            var statsType = System.Type.GetType("Code.Lavos.Core.PlayerStats, Code.Lavos.Status");
            MonoBehaviour playerStatsInstance = null;
            
            if (statsType != null)
            {
                var instanceProp = statsType.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                playerStatsInstance = instanceProp?.GetValue(null) as MonoBehaviour;
            }
            
            if (playerStatsInstance != null)
            {
                var onHealthEvent = statsType.GetEvent("OnHealthChanged", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                var onManaEvent = playerStatsInstance.GetType().GetEvent("OnManaChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                var onStaminaEvent = playerStatsInstance.GetType().GetEvent("OnStaminaChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                var onEffectAddedEvent = playerStatsInstance.GetType().GetEvent("OnEffectAdded", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                var onEffectRemovedEvent = playerStatsInstance.GetType().GetEvent("OnEffectRemoved", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                
                if (onHealthEvent != null) onHealthEvent.AddEventHandler(null, new System.Action<float, float>(OnHealthChanged));
                if (onManaEvent != null) onManaEvent.AddEventHandler(playerStatsInstance, new System.Action<float, float>(OnManaChanged));
                if (onStaminaEvent != null) onStaminaEvent.AddEventHandler(playerStatsInstance, new System.Action<float, float>(OnStaminaChanged));
                if (onEffectAddedEvent != null) onEffectAddedEvent.AddEventHandler(playerStatsInstance, new System.Action<Code.Lavos.Status.StatusEffectData>(OnEffectAdded));
                if (onEffectRemovedEvent != null) onEffectRemovedEvent.AddEventHandler(playerStatsInstance, new System.Action<Code.Lavos.Status.StatusEffectData>(OnEffectRemoved));
                
                Debug.Log("[UIBarsSystem] Subscribed to PlayerStats events");
                return; // Exit early - no need to retry
            }

            // Fallback to PlayerHealth (legacy system)
            var playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                PlayerHealth.OnHealthChanged += OnHealthChangedLegacy;
                PlayerHealth.OnPlayerDied += OnPlayerDied;
                Debug.Log("[UIBarsSystem] Subscribed to PlayerHealth events (legacy mode)");
                return; // Exit early - no need to retry
            }
            
            Debug.LogWarning("[UIBarsSystem] No PlayerStats or PlayerHealth found! Will retry...");
            // Retry after a short delay in case components haven't initialized yet
            Invoke(nameof(SubscribeToEvents), 0.5f);
        }

        private void UnsubscribeFromEvents()
        {
            // Unsubscribe from PlayerStats using reflection
            var statsType = System.Type.GetType("Code.Lavos.Core.PlayerStats, Code.Lavos.Status");
            MonoBehaviour playerStatsInstance = null;
            
            if (statsType != null)
            {
                var instanceProp = statsType.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                playerStatsInstance = instanceProp?.GetValue(null) as MonoBehaviour;
            }
            
            if (playerStatsInstance != null)
            {
                var onHealthEvent = statsType.GetEvent("OnHealthChanged", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                var onManaEvent = playerStatsInstance.GetType().GetEvent("OnManaChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                var onStaminaEvent = playerStatsInstance.GetType().GetEvent("OnStaminaChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                var onEffectAddedEvent = playerStatsInstance.GetType().GetEvent("OnEffectAdded", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                var onEffectRemovedEvent = playerStatsInstance.GetType().GetEvent("OnEffectRemoved", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                
                if (onHealthEvent != null) onHealthEvent.RemoveEventHandler(null, new System.Action<float, float>(OnHealthChanged));
                if (onManaEvent != null) onManaEvent.RemoveEventHandler(playerStatsInstance, new System.Action<float, float>(OnManaChanged));
                if (onStaminaEvent != null) onStaminaEvent.RemoveEventHandler(playerStatsInstance, new System.Action<float, float>(OnStaminaChanged));
                if (onEffectAddedEvent != null) onEffectAddedEvent.RemoveEventHandler(playerStatsInstance, new System.Action<Code.Lavos.Status.StatusEffectData>(OnEffectAdded));
                if (onEffectRemovedEvent != null) onEffectRemovedEvent.RemoveEventHandler(playerStatsInstance, new System.Action<Code.Lavos.Status.StatusEffectData>(OnEffectRemoved));
            }

            // Unsubscribe from PlayerHealth
            var playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                PlayerHealth.OnHealthChanged -= OnHealthChangedLegacy;
                PlayerHealth.OnPlayerDied -= OnPlayerDied;
            }
        }

        // PlayerStats event handlers
        private void OnHealthChanged(float current, float max) => SetHealth(current, max, showFloatingText: true);
        private void OnManaChanged(float current, float max) => SetMana(current, max, showFloatingText: true);
        private void OnStaminaChanged(float current, float max) => SetStamina(current, max, showFloatingText: true);
        private void OnEffectAdded(StatusEffectData effect) => AddStatusEffect(effect);
        private void OnEffectRemoved(StatusEffectData effect) => RemoveStatusEffect(effect);
        
        // PlayerHealth legacy event handler (converts to PlayerStats-like format)
        private void OnHealthChangedLegacy(float current, float max)
        {
            SetHealth(current, max);
            // Set default mana and stamina for legacy mode
            SetMana(100f, 100f);
            SetStamina(100f, 100f);
        }
        
        private void OnPlayerDied() { /* Handle player death */ }

        /// <summary>
        /// Update bar positions based on screen resolution.
        /// Called every frame to handle dynamic resolution changes.
        /// </summary>
        private void UpdateBarPositions()
        {
            if (_canvas == null) return;

            var canvasRect = _canvas.GetComponent<RectTransform>();
            if (canvasRect == null) return;

            float screenWidth = canvasRect.rect.width;
            float screenHeight = canvasRect.rect.height;

            // Calculate bar dimensions
            // Health & Mana: 5% width, 65% height (vertical bars on edges) - VISIBLE PIXEL ART STYLE
            float verticalBarWidth = screenWidth * 0.05f;    // 5% of screen width (thicker)
            float verticalBarHeight = screenHeight * 0.65f;  // 65% of screen height (taller)

            // Stamina: 65% width, 5% height (horizontal bar at bottom) - VISIBLE PIXEL ART STYLE
            float horizontalBarWidth = screenWidth * 0.65f;  // 65% of screen width (wider)
            float horizontalBarHeight = screenHeight * 0.05f; // 5% of screen height (thicker)

            // Health Bar (Left edge - vertical)
            // Position: left edge, centered vertically
            if (_healthBarRoot != null)
            {
                _healthBarRoot.sizeDelta = new Vector2(verticalBarWidth, verticalBarHeight);
                _healthBarRoot.anchoredPosition = new Vector2(
                    -screenWidth / 2 + barMargin + verticalBarWidth / 2,
                    0
                );
            }

            // Mana Bar (Right edge - vertical)
            // Position: right edge, centered vertically
            if (_manaBarRoot != null)
            {
                _manaBarRoot.sizeDelta = new Vector2(verticalBarWidth, verticalBarHeight);
                _manaBarRoot.anchoredPosition = new Vector2(
                    screenWidth / 2 - barMargin - verticalBarWidth / 2,
                    0
                );
            }

            // Stamina Bar (Bottom edge - horizontal)
            // Position: bottom edge, centered horizontally
            if (_staminaBarRoot != null)
            {
                _staminaBarRoot.sizeDelta = new Vector2(horizontalBarWidth, horizontalBarHeight);
                _staminaBarRoot.anchoredPosition = new Vector2(
                    0,
                    -screenHeight / 2 + barMargin + horizontalBarHeight / 2
                );
            }

            // Status Effects (Top center)
            if (_effectsRoot != null)
            {
                _effectsRoot.anchoredPosition = new Vector2(
                    0,
                    screenHeight / 2 - effectRowMargin - effectIconSize / 2
                );
            }
        }

        /// <summary>
        /// Build all UI bars and status effects row.
        /// </summary>
        private void BuildBars()
        {
            Debug.Log("[UIBarsSystem] BuildBars - Starting...");
            EnsureCanvas();
            var canvasTransform = _canvas.transform;
            Debug.Log($"[UIBarsSystem] BuildBars - Canvas: {_canvas.name}, Children: {canvasTransform.childCount}");

            // Health Bar (Left - Vertical) - using center anchor, positioned absolutely
            _healthBarRoot = CreateBarContainerCenter(
                "HealthBar", canvasTransform,
                Image.FillMethod.Vertical,
                healthColorHigh
            );
            _healthFill = _healthBarRoot.Find("Fill").GetComponent<Image>();
            _healthText = CreateBarText(_healthBarRoot, "HealthText");
            Debug.Log($"[UIBarsSystem] BuildBars - HealthBar created: {_healthBarRoot != null}");

            // Mana Bar (Right - Vertical)
            _manaBarRoot = CreateBarContainerCenter(
                "ManaBar", canvasTransform,
                Image.FillMethod.Vertical,
                manaColor
            );
            _manaFill = _manaBarRoot.Find("Fill").GetComponent<Image>();
            _manaText = CreateBarText(_manaBarRoot, "ManaText");
            Debug.Log($"[UIBarsSystem] BuildBars - ManaBar created: {_manaBarRoot != null}");

            // Stamina Bar (Bottom - Horizontal)
            _staminaBarRoot = CreateBarContainerCenter(
                "StaminaBar", canvasTransform,
                Image.FillMethod.Horizontal,
                staminaColor
            );
            _staminaFill = _staminaBarRoot.Find("Fill").GetComponent<Image>();
            _staminaText = CreateBarText(_staminaBarRoot, "StaminaText");
            Debug.Log($"[UIBarsSystem] BuildBars - StaminaBar created: {_staminaBarRoot != null}");

            // Status Effects Row (Top Center)
            BuildEffectsRow(canvasTransform);

            // Set initial values
            SetHealth(_currentHealth, _maxHealth);
            SetMana(_currentMana, _maxMana);
            SetStamina(_currentStamina, _maxStamina);

            Debug.Log("[UIBarsSystem] Bars built with responsive screen-edge layout");
        }

        /// <summary>
        /// Create a bar container with center anchor for absolute positioning.
        /// </summary>
        private RectTransform CreateBarContainerCenter(
            string name, Transform parent,
            Image.FillMethod fillMethod, Color fillColor)
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

            // Set initial size based on fill direction
            if (fillMethod == Image.FillMethod.Vertical)
            {
                rt.sizeDelta = new Vector2(barThickness, barSizePercent * 1080 * 0.75f);
            }
            else
            {
                rt.sizeDelta = new Vector2(barSizePercent * 1920 * 0.75f, barThickness);
            }

            // Background
            var bg = go.AddComponent<Image>();
            bg.color = backgroundColor;

            // Border
            CreateBarBorder(go.transform, borderColor);

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
            var top = CreateBorderEdge("Border_Top", parent,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -borderSize), Vector2.zero);

            // Bottom border
            var bottom = CreateBorderEdge("Border_Bottom", parent,
                new Vector2(0, 0), new Vector2(1, 0),
                Vector2.zero, new Vector2(0, borderSize));

            // Left border
            var left = CreateBorderEdge("Border_Left", parent,
                new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(-borderSize, 0), Vector2.zero);

            // Right border
            var right = CreateBorderEdge("Border_Right", parent,
                new Vector2(1, 0), new Vector2(1, 1),
                Vector2.zero, new Vector2(borderSize, 0));
        }

        private GameObject CreateBorderEdge(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            rt.localScale = Vector3.one;

            go.AddComponent<Image>().color = borderColor;
            return go;
        }

        /// <summary>
        /// Create text label for bar values with uppercase bold black font.
        /// </summary>
        private TextMeshProUGUI CreateBarText(RectTransform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = barTextFontSize;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;

            // Add outline for better visibility
            if (showTextOutline)
            {
                var outline = go.AddComponent<Outline>();
                outline.effectColor = outlineColor;
                outline.effectDistance = new Vector2(outlineWidth, outlineWidth);
            }

            return tmp;
        }

        /// <summary>
        /// Build the status effects row at top center.
        /// </summary>
        private void BuildEffectsRow(Transform parent)
        {
            var go = new GameObject("StatusEffects");
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -effectRowMargin - effectIconSize / 2);
            rt.sizeDelta = new Vector2(600f, effectIconSize + 16f);

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = effectSpacing;
            layout.childControlHeight = true;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;
            layout.childAlignment = TextAnchor.UpperCenter;

            var contentFitter = go.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _effectsContainer = go.transform;
        }

        /// <summary>
        /// Ensure canvas exists and is properly configured.
        /// </summary>
        private void EnsureCanvas()
        {
            // Try to find existing canvas first (from HUDSystem or other)
            _canvas = FindFirstObjectByType<Canvas>();
            Debug.Log($"[UIBarsSystem] EnsureCanvas - Found existing canvas: {_canvas != null}");

            if (_canvas == null)
            {
                var go = new GameObject("UI_Canvas");
                _canvas = go.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 100;

                var scaler = go.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                go.AddComponent<GraphicRaycaster>();
                Debug.Log("[UIBarsSystem] EnsureCanvas - Created new canvas");
            }
            else
            {
                // Ensure canvas is configured for overlay
                if (_canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    Debug.LogWarning($"[UIBarsSystem] Existing canvas uses {_canvas.renderMode}, bars may not be visible");
                }
                Debug.Log($"[UIBarsSystem] EnsureCanvas - Using existing canvas '{_canvas.name}' with sorting order: {_canvas.sortingOrder}");
            }
        }

        #region Public API - Bar Updates

        /// <summary>
        /// Set health bar value with color interpolation and floating text.
        /// As health is spent, color dims from bright green to dark red.
        /// Shows floating damage/heal numbers.
        /// </summary>
        public void SetHealth(float current, float max, bool showFloatingText = true)
        {
            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Clamp(current, 0, max);
            _maxHealth = max > 0 ? max : 1;

            if (_healthFill != null)
            {
                float t = _currentHealth / _maxHealth;
                _healthFill.fillAmount = t;

                // Smooth color interpolation: full health = bright green, low health = dark red
                if (t >= 0.6f)
                {
                    float localT = (t - 0.6f) / 0.4f;
                    _healthFill.color = Color.Lerp(healthColorLow, healthColorHigh, localT);
                }
                else if (t >= 0.3f)
                {
                    float localT = (t - 0.3f) / 0.3f;
                    _healthFill.color = Color.Lerp(healthColorCritical, healthColorLow, localT);
                }
                else
                {
                    float dimFactor = t / 0.3f;
                    _healthFill.color = new Color(
                        healthColorCritical.r * dimFactor,
                        healthColorCritical.g * dimFactor,
                        healthColorCritical.b * dimFactor
                    );
                }
            }

            if (_healthText != null)
            {
                float percent = (_currentHealth / _maxHealth) * 100f;
                _healthText.text = $"{percent:F0}%".ToUpper();
                _healthText.enabled = true;
            }

            // Show floating damage/heal number
            if (showFloatingText && Mathf.Abs(_currentHealth - previousHealth) > 0.1f)
            {
                float delta = _currentHealth - previousHealth;
                if (delta < 0)
                {
                    ShowFloatingText(Mathf.FloorToInt(Mathf.Abs(delta)).ToString(), Color.red, "Damage");
                }
                else
                {
                    ShowFloatingText($"+{Mathf.FloorToInt(delta)}", Color.green, "Heal");
                }
            }
        }

        /// <summary>
        /// Set mana bar value with smooth color diminishing and floating text.
        /// Full mana = bright blue, empty mana = dark blue/gray.
        /// Shows floating mana gain/loss numbers.
        /// </summary>
        public void SetMana(float current, float max, bool showFloatingText = true)
        {
            float previousMana = _currentMana;
            _currentMana = Mathf.Clamp(current, 0, max);
            _maxMana = max > 0 ? max : 1;

            if (_manaFill != null)
            {
                float t = _currentMana / _maxMana;
                _manaFill.fillAmount = t;

                // Smooth color interpolation: full = bright blue, empty = dark blue
                _manaFill.color = Color.Lerp(manaColorLow, manaColor, t);
            }

            if (_manaText != null)
            {
                float percent = (_currentMana / _maxMana) * 100f;
                _manaText.text = $"{percent:F0}%".ToUpper();
                _manaText.enabled = true;
            }

            // Show floating mana gain/loss number
            if (showFloatingText && Mathf.Abs(_currentMana - previousMana) > 0.1f)
            {
                float delta = _currentMana - previousMana;
                if (delta < 0)
                {
                    ShowFloatingText(Mathf.FloorToInt(Mathf.Abs(delta)).ToString(), new Color(0.3f, 0.3f, 1f), "ManaLoss");
                }
                else
                {
                    ShowFloatingText($"+{Mathf.FloorToInt(delta)}", Color.cyan, "ManaGain");
                }
            }
        }

        /// <summary>
        /// Set stamina bar value with smooth color and floating text.
        /// Full stamina = bright yellow, empty = dark yellow/brown.
        /// Shows floating stamina gain/loss numbers (only for significant changes).
        /// </summary>
        public void SetStamina(float current, float max, bool showFloatingText = true)
        {
            float previousStamina = _currentStamina;
            _currentStamina = Mathf.Clamp(current, 0, max);
            _maxStamina = max > 0 ? max : 1;

            if (_staminaFill != null)
            {
                float t = _currentStamina / _maxStamina;
                _staminaFill.fillAmount = t;

                // Smooth color interpolation: full = bright yellow, empty = dark yellow
                _staminaFill.color = Color.Lerp(staminaColorLow, staminaColor, t);
            }

            if (_staminaText != null)
            {
                float percent = (_currentStamina / _maxStamina) * 100f;
                _staminaText.text = $"{percent:F0}%".ToUpper();
                _staminaText.enabled = true;
            }

            // Show floating stamina gain/loss number (only for significant changes > 1)
            // This prevents "0" glitch from tiny per-frame drain
            if (showFloatingText && Mathf.Abs(_currentStamina - previousStamina) > 1f)
            {
                float delta = _currentStamina - previousStamina;
                if (delta < 0)
                {
                    ShowFloatingText(Mathf.FloorToInt(Mathf.Abs(delta)).ToString(), new Color(1f, 0.8f, 0f), "StaminaLoss");
                }
                else
                {
                    ShowFloatingText($"+{Mathf.FloorToInt(delta)}", Color.yellow, "StaminaGain");
                }
            }
        }

        #endregion

        #region Floating Text System

        /// <summary>
        /// Show floating text (damage, heal, gain, loss, etc.).
        /// Reusable for any stat change visualization.
        /// </summary>
        private void ShowFloatingText(string text, Color color, string type)
        {
            if (_healthBarRoot == null) return;

            // Create floating text GameObject
            var go = new GameObject($"Floating_{type}");
            go.transform.SetParent(_healthBarRoot.parent, false);
            
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120, 45);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, 60);
            
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 32;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            
            // Outline for better visibility
            var outline = go.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(3, 3);
            
            // Shadow for depth
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, 2);
            
            // Fade out coroutine
            StartCoroutine(FadeOutFloatingText(go, tmp));
        }

        private System.Collections.IEnumerator FadeOutFloatingText(GameObject go, TextMeshProUGUI text)
        {
            float elapsed = 0f;
            float duration = 1.5f;
            Color original = text.color;
            Vector3 startPos = go.transform.localPosition;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Fade out
                text.color = new Color(original.r, original.g, original.b, original.a * (1 - t));
                
                // Float upward with slight curve
                go.transform.localPosition = startPos + Vector3.up * (elapsed * 40f) + Vector3.right * Mathf.Sin(elapsed * 2f) * 5f;
                
                // Scale up slightly
                float scale = 1f + t * 0.2f;
                go.transform.localScale = Vector3.one * scale;
                
                yield return null;
            }
            
            Destroy(go);
        }

        /// <summary>
        /// Show custom floating text at specific position.
        /// For dialogs, notifications, or custom messages.
        /// </summary>
        public void ShowCustomFloatingText(string text, Color color, float duration = 2f, Vector2? offset = null)
        {
            if (_healthBarRoot == null) return;

            var go = new GameObject($"CustomFloating_{text}");
            go.transform.SetParent(_healthBarRoot.parent, false);
            
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 50);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = offset ?? new Vector2(0, 100);
            
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 28;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            
            var outline = go.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(3, 3);
            
            StartCoroutine(FadeOutCustomText(go, tmp, duration));
        }

        private System.Collections.IEnumerator FadeOutCustomText(GameObject go, TextMeshProUGUI text, float duration)
        {
            float elapsed = 0f;
            Color original = text.color;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                text.color = new Color(original.r, original.g, original.b, original.a * (1 - t));
                yield return null;
            }
            
            Destroy(go);
        }

        #endregion

        #region Public API - Status Effects

        /// <summary>
        /// Add a status effect icon.
        /// </summary>
        public void AddStatusEffect(StatusEffectData effect)
        {
            if (effect == null || _effectIcons.ContainsKey(effect.id)) return;

            var icon = BuildEffectIcon(effect);
            _effectIcons[effect.id] = icon;
        }

        /// <summary>
        /// Remove a status effect icon.
        /// </summary>
        public void RemoveStatusEffect(StatusEffectData effect)
        {
            if (effect == null || !_effectIcons.TryGetValue(effect.id, out var data)) return;

            Destroy(data.Root);
            _effectIcons.Remove(effect.id);
        }

        /// <summary>
        /// Refresh all status effect timers.
        /// Call this in LateUpdate or via coroutine.
        /// </summary>
        public void RefreshEffectTimers()
        {
            // Use reflection to access PlayerStats
            var statsType = System.Type.GetType("Code.Lavos.Core.PlayerStats, Code.Lavos.Status");
            MonoBehaviour playerStatsInstance = null;
            
            if (statsType != null)
            {
                var instanceProp = statsType.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                playerStatsInstance = instanceProp?.GetValue(null) as MonoBehaviour;
            }
            
            if (playerStatsInstance == null) return;

            var activeEffectsProp = playerStatsInstance.GetType().GetProperty("ActiveEffects");
            var effects = activeEffectsProp?.GetValue(playerStatsInstance) as System.Collections.IList;
            if (effects == null) return;

            foreach (var effectObj in effects)
            {
                if (effectObj == null) continue;
                
                var effectIdProp = effectObj.GetType().GetProperty("id");
                var effectId = effectIdProp?.GetValue(effectObj) as string;
                
                if (effectId == null || !_effectIcons.TryGetValue(effectId, out var data)) continue;

                var maxDurationProp = effectObj.GetType().GetProperty("MaxDuration");
                var remainingTimeProp = effectObj.GetType().GetProperty("remainingTime");
                var maxStacksProp = effectObj.GetType().GetProperty("maxStacks");
                var currentStacksProp = effectObj.GetType().GetProperty("currentStacks");
                
                float maxDuration = maxDurationProp != null ? (float)maxDurationProp.GetValue(effectObj) : 0f;
                float remainingTime = remainingTimeProp != null ? (float)remainingTimeProp.GetValue(effectObj) : 0f;
                int maxStacks = maxStacksProp != null ? (int)maxStacksProp.GetValue(effectObj) : 1;
                int currentStacks = currentStacksProp != null ? (int)currentStacksProp.GetValue(effectObj) : 1;
                
                float t = maxDuration > 0 ? remainingTime / maxDuration : 0f;
                data.FillBar.fillAmount = Mathf.Clamp01(t);

                if (data.StackText != null && maxStacks > 1)
                    data.StackText.text = currentStacks > 1 ? $"x{currentStacks}" : "";

                if (data.DurationText != null && remainingTime > 0)
                    data.DurationText.text = $"{remainingTime:F1}s";
            }
        }

        /// <summary>
        /// Build a status effect icon.
        /// </summary>
        private EffectIconData BuildEffectIcon(StatusEffectData effect)
        {
            Color iconColor = GetEffectColor(effect);

            var root = new GameObject($"Effect_{effect.id}");
            root.transform.SetParent(_effectsContainer, false);

            var rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(effectIconSize, effectIconSize + 14f);

            // Border (variable used for potential future customization)
            var border = CreateEffectBorder(root.transform);
            _ = border; // Suppress unused variable warning

            // Background
            var bg = new GameObject("BG");
            bg.transform.SetParent(root.transform, false);
            var bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0.1f, 0.1f);
            bgRT.anchorMax = new Vector2(0.9f, 0.9f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.18f);

            // Icon
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(root.transform, false);
            var iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.15f, 0.25f);
            iconRT.anchorMax = new Vector2(0.85f, 0.9f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.color = iconColor;
            if (effect.icon != null)
            {
                iconImg.sprite = effect.icon;
                iconImg.type = Image.Type.Simple;
            }

            // Duration bar background
            var barBG = new GameObject("DurBarBG");
            barBG.transform.SetParent(root.transform, false);
            var barBGRT = barBG.AddComponent<RectTransform>();
            barBGRT.anchorMin = new Vector2(0.1f, 0.05f);
            barBGRT.anchorMax = new Vector2(0.9f, 0.18f);
            barBGRT.offsetMin = Vector2.zero;
            barBGRT.offsetMax = Vector2.zero;
            barBG.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f);

            // Duration bar fill
            var barFillGO = new GameObject("DurBarFill");
            barFillGO.transform.SetParent(barBG.transform, false);
            var barFillRT = barFillGO.AddComponent<RectTransform>();
            barFillRT.anchorMin = Vector2.zero;
            barFillRT.anchorMax = Vector2.one;
            barFillRT.offsetMin = Vector2.zero;
            barFillRT.offsetMax = Vector2.zero;
            var barFill = barFillGO.AddComponent<Image>();
            barFill.color = iconColor;
            barFill.type = Image.Type.Filled;
            barFill.fillMethod = Image.FillMethod.Horizontal;
            barFill.fillAmount = 1f;

            // Stack text
            TextMeshProUGUI stackText = null;
            if (effect.maxStacks > 1)
            {
                var stackGO = new GameObject("Stacks");
                stackGO.transform.SetParent(root.transform, false);
                var stackRT = stackGO.AddComponent<RectTransform>();
                stackRT.anchorMin = new Vector2(0.6f, 0.6f);
                stackRT.anchorMax = new Vector2(1f, 1f);
                stackRT.offsetMin = Vector2.zero;
                stackRT.offsetMax = Vector2.zero;
                stackText = stackGO.AddComponent<TextMeshProUGUI>();
                stackText.fontSize = 11f;
                stackText.color = Color.white;
                stackText.alignment = TextAlignmentOptions.TopRight;
                stackText.fontStyle = FontStyles.Bold;
            }

            // Duration text
            var durationGO = new GameObject("Duration");
            durationGO.transform.SetParent(root.transform, false);
            var durationRT = durationGO.AddComponent<RectTransform>();
            durationRT.anchorMin = new Vector2(0f, 0.05f);
            durationRT.anchorMax = new Vector2(0.5f, 0.2f);
            durationRT.offsetMin = Vector2.zero;
            durationRT.offsetMax = Vector2.zero;
            var durationText = durationGO.AddComponent<TextMeshProUGUI>();
            durationText.fontSize = 9f;
            durationText.color = new Color(1f, 1f, 1f, 0.8f);
            durationText.alignment = TextAlignmentOptions.BottomLeft;

            return new EffectIconData
            {
                Root = root,
                FillBar = barFill,
                StackText = stackText,
                DurationText = durationText
            };
        }

        private GameObject CreateEffectBorder(Transform parent)
        {
            var border = new GameObject("Border");
            border.transform.SetParent(parent, false);
            var borderRT = border.AddComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = Vector2.zero;
            borderRT.offsetMax = Vector2.zero;
            border.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.35f);
            return border;
        }

        private Color GetEffectColor(StatusEffectData effect)
        {
            // Default colors based on effect type
            return effect.effectType == EffectType.Debuff
                ? new Color(0.9f, 0.25f, 0.1f)  // Red/Orange for debuffs
                : new Color(0.2f, 0.85f, 0.5f);  // Green for buffs
        }

        #endregion
    }
}
