using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Code.Lavos;
using Code.Lavos.Status;

namespace Unity6.LavosTrial.HUD
{
    /// <summary>
    /// HUDSystem â€” SystÃ¨me HUD unifiÃ© + Status Effects + Hotbar pixel art
    ///
    /// SETUP Unity :
    ///   1. GameObject vide "HUDSystem" â†’ attache ce script â†’ c'est tout.
    ///   2. Tout se construit automatiquement au runtime.
    ///
    /// NouveautÃ©s v2 :
    ///   - Status effects : icÃ´nes colorÃ©es + barre de durÃ©e + stacks (bas-gauche)
    ///   - Hotbar pixel art : 5 slots bas-centre, touches 1-5, slot actif surlignÃ©
    ///
    /// Unity6 Integration:
    ///   - Namespace: Unity6.LavosTrial.HUD
    ///   - Input System: Detection runtime + fallback
    ///   - Keyboard mapping: via JSON config (Assets/Input/Unity6.LavosTrial.InputMap.json)
    /// </summary>
    public class HUDSystem : MonoBehaviour
    {
        // - Singleton -
        public static HUDSystem Instance { get; private set; }

        // --------
        //  INSPECTOR (tout optionnel)
        // --------
        [Header("Canvas (auto-crÃ©Ã© si vide)")]
        [SerializeField] private Canvas targetCanvas;

        [Header("Barres de statut")]
        [SerializeField] private Image healthFill;
        [SerializeField] private Image manaFill;
        [SerializeField] private Image staminaFill;

        [Header("Textes barres")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI manaText;
        [SerializeField] private TextMeshProUGUI staminaText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI interactionPrompt;

        [Header("Popup")]
        [SerializeField] private TextMeshProUGUI popupText;
        [SerializeField] private float popupDuration = 2f;

        [Header("Panneaux")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;

        [Header("Couleurs barres")]
        [SerializeField] private Color colorHealthHigh = new Color(0.2f, 0.85f, 0.3f);
        [SerializeField] private Color colorHealthLow = new Color(0.85f, 0.15f, 0.1f);
        [SerializeField] private Color colorMana = new Color(0.2f, 0.5f, 1f);
        [SerializeField] private Color colorStamina = new Color(1f, 0.75f, 0.1f);

        [Header("Hotbar")]
        [SerializeField] private int hotbarSlotCount = 5;
        [SerializeField] private float hotbarSlotSize = 52f;
        [SerializeField] private Color hotbarBgColor = new Color(0.08f, 0.08f, 0.08f, 0.88f);
        [SerializeField] private Color hotbarBorderColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        [SerializeField] private Color hotbarActiveColor = new Color(1f, 0.82f, 0.2f, 1f);
        [SerializeField] private Color hotbarInactiveColor = new Color(0.22f, 0.22f, 0.22f, 1f);

        [Header("Status Effects")]
        [SerializeField] private float effectIconSize = 40f;

        [Header("Input Config")]
        [SerializeField] private string inputMapPath = "Assets/Input/Unity6.LavosTrial.InputMap.json";

        // ------------
        //  Ã‰TAT INTERNE
        // ------------
        private GameObject _hudRoot;

        // â€” Hotbar â€”
        private int _activeSlot = 0;
        private Image[] _hotbarBorders;   // cadre externe (couleur active/inactive)
        private Image[] _hotbarIconImages; // icÃ´ne de l'item
        private TextMeshProUGUI[] _hotbarQtyTexts;  // quantité
        private TextMeshProUGUI[] _hotbarKeyTexts;  // label touche (1-5)

        // â€” Input System â€”
        private Keyboard _kb;
        private Mouse _mouse;
        private InputMapConfig _inputConfig;
        private Dictionary<string, string[]> _actionKeyMap;

        // â€” Status Effects â€”
        private Transform _effectRow;
        private Dictionary<string, EffectIconData> _effectIcons = new();

        private struct EffectIconData
        {
            public GameObject Root;
            public Image FillBar;
            public TextMeshProUGUI StackText;
            public float MaxDuration;
        }

        // — Popup —
        private Coroutine _popupRoutine;

        // — Subscribed instances —
        private static Inventory _inventory;

        // ------------
        //  INPUT CONFIGURATION
        // ------------
        [System.Serializable]
        public class InputMapConfig
        {
            public string version = "1.0";
            public string name;
            public string description;
            public string locale = "en-US";
            public string layout = "QWERTY";
            public ActionMap actions;
            public FallbackConfig fallback;
        }

        [System.Serializable]
        public class ActionMap
        {
            public string[] MoveUp;
            public string[] MoveDown;
            public string[] MoveLeft;
            public string[] MoveRight;
            public string[] Jump;
            public string[] Fire;
            public string[] Aim;
            public string[] Reload;
            public string[] Interact;
            public string[] Inventory;
            public string[] Pause;
            public string[] Sprint;
            public string[] Crouch;
        }

        [System.Serializable]
        public class FallbackConfig
        {
            public bool enabled;
            public string layout;
            public ActionMap actions;
        }

        // --------
        //  CYCLE DE VIE
        // --------
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Initialize input system early to avoid null refs in Update
            InitializeInputSystem();
            DetectInputSystem();
            LoadInputConfig();
            EnsureCanvas();
            BuildHUDIfNeeded();
        }

        void Start() => SubscribeToEvents();
        void OnDestroy()
        {
            UnsubscribeFromEvents();
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            _kb ??= Keyboard.current;
            _mouse ??= Mouse.current;
            HandleHotbarInput();
            RefreshEffectTimers();
        }

        // ------------
        //  INPUT SYSTEM DETECTION
        // ------------
        private void DetectInputSystem()
        {
#if UNITY_INPUT_SYSTEM_ENABLED
            Debug.Log($"[HUDSystem] New Input System detected: {Keyboard.current != null}");
#else
            Debug.Log("[HUDSystem] New Input System not enabled - using legacy Input");
#endif
        }
        // Initialize input system early to avoid null refs in Update
        private void InitializeInputSystem()
        {
            _kb = Keyboard.current;
            _mouse = Mouse.current;
        }

        private void LoadInputConfig()
        {
            _actionKeyMap = new Dictionary<string, string[]>();

            string fullPath = Path.Combine(Application.dataPath, "..", inputMapPath);
            if (File.Exists(fullPath))
            {
                try
                {
                    string json = File.ReadAllText(fullPath);
                    _inputConfig = JsonUtility.FromJson<InputMapConfig>(json);

                    if (_inputConfig?.actions != null)
                    {
                        MapAction("MoveUp", _inputConfig.actions.MoveUp);
                        MapAction("MoveDown", _inputConfig.actions.MoveDown);
                        MapAction("MoveLeft", _inputConfig.actions.MoveLeft);
                        MapAction("MoveRight", _inputConfig.actions.MoveRight);
                        MapAction("Jump", _inputConfig.actions.Jump);
                        MapAction("Fire", _inputConfig.actions.Fire);
                        MapAction("Aim", _inputConfig.actions.Aim);
                        MapAction("Reload", _inputConfig.actions.Reload);
                        MapAction("Interact", _inputConfig.actions.Interact);
                        MapAction("Inventory", _inputConfig.actions.Inventory);
                        MapAction("Pause", _inputConfig.actions.Pause);
                        MapAction("Sprint", _inputConfig.actions.Sprint);
                        MapAction("Crouch", _inputConfig.actions.Crouch);
                    }

                    Debug.Log($"[HUDSystem] Input map loaded: {_inputConfig?.layout ?? "default"}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[HUDSystem] Failed to load input config: {ex.Message}");
                    LoadDefaultInputMap();
                }
            }
            else
            {
                Debug.LogWarning($"[HUDSystem] Input config not found at {fullPath}, using defaults");
                LoadDefaultInputMap();
            }
        }

        private void LoadDefaultInputMap()
        {
            _actionKeyMap = new Dictionary<string, string[]>
            {
                { "MoveUp", new[] { "W", "UpArrow" } },
                { "MoveDown", new[] { "S", "DownArrow" } },
                { "MoveLeft", new[] { "A", "LeftArrow" } },
                { "MoveRight", new[] { "D", "RightArrow" } },
                { "Jump", new[] { "Space" } },
                { "Fire", new[] { "LeftCtrl", "Mouse0" } },
                { "Aim", new[] { "Mouse1" } },
                { "Reload", new[] { "R" } },
                { "Interact", new[] { "E" } },
                { "Inventory", new[] { "I" } },
                { "Pause", new[] { "Escape" } },
                { "Sprint", new[] { "LeftShift" } },
                { "Crouch", new[] { "C" } }
            };
        }

        private void MapAction(string action, string[] keys)
        {
            if (keys != null && keys.Length > 0)
                _actionKeyMap[action] = keys;
        }

        public bool IsKeyPressed(string action)
        {
            if (_kb == null || string.IsNullOrEmpty(action)) return false;

            if (_actionKeyMap.TryGetValue(action, out var keys))
            {
                foreach (var key in keys)
                {
                    if (TryGetKey(key, out var unityKey) && _kb[unityKey].isPressed)
                        return true;
                    if (IsMouseButtonPressed(key))
                        return true;
                }
            }
            return false;
        }

        public bool WasKeyPressedThisFrame(string action)
        {
            if (_kb == null || string.IsNullOrEmpty(action)) return false;

            if (_actionKeyMap.TryGetValue(action, out var keys))
            {
                foreach (var key in keys)
                {
                    if (TryGetKey(key, out var unityKey) && _kb[unityKey].wasPressedThisFrame)
                        return true;
                    if (WasMouseButtonPressedThisFrame(key))
                        return true;
                }
            }
            return false;
        }

        private bool TryGetKey(string keyName, out Key result)
        {
            result = Key.None;
            if (string.IsNullOrEmpty(keyName)) return false;

            switch (keyName.ToUpperInvariant())
            {
                case "W": case "UPARROW": result = Key.UpArrow; return true;
                case "S": case "DOWNARROW": result = Key.DownArrow; return true;
                case "A": case "LEFTARROW": result = Key.LeftArrow; return true;
                case "D": case "RIGHTARROW": result = Key.RightArrow; return true;
                case "SPACE": result = Key.Space; return true;
                case "LEFTCTRL": result = Key.LeftCtrl; return true;
                case "RIGHTCTRL": result = Key.RightCtrl; return true;
                case "R": result = Key.R; return true;
                case "E": result = Key.E; return true;
                case "I": result = Key.I; return true;
                case "ESCAPE": result = Key.Escape; return true;
                case "LEFTSHIFT": result = Key.LeftShift; return true;
                case "RIGHTSHIFT": result = Key.RightShift; return true;
                case "C": result = Key.C; return true;
                case "1": result = Key.Digit1; return true;
                case "2": result = Key.Digit2; return true;
                case "3": result = Key.Digit3; return true;
                case "4": result = Key.Digit4; return true;
                case "5": result = Key.Digit5; return true;
                default: return false;
            }
        }

        private bool IsMouseButtonPressed(string buttonName)
        {
            if (_mouse == null || string.IsNullOrEmpty(buttonName)) return false;

            switch (buttonName.ToUpperInvariant())
            {
                case "MOUSE0": return _mouse.leftButton.isPressed;
                case "MOUSE1": return _mouse.rightButton.isPressed;
                case "MOUSE2": return _mouse.middleButton.isPressed;
                default: return false;
            }
        }

        private bool WasMouseButtonPressedThisFrame(string buttonName)
        {
            if (_mouse == null || string.IsNullOrEmpty(buttonName)) return false;

            switch (buttonName.ToUpperInvariant())
            {
                case "MOUSE0": return _mouse.leftButton.wasPressedThisFrame;
                case "MOUSE1": return _mouse.rightButton.wasPressedThisFrame;
                case "MOUSE2": return _mouse.middleButton.wasPressedThisFrame;
                default: return false;
            }
        }

        // ---------
        //  ABONNEMENTS ÉVÉNEMENTS
        // ---------
        private void SubscribeToEvents()
        {
            PlayerStats.OnHealthChanged += OnHealthChanged;
            PlayerStats.OnPlayerDied += OnPlayerDied;

            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnManaChanged += OnManaChanged;
                PlayerStats.Instance.OnStaminaChanged += OnStaminaChanged;
                PlayerStats.Instance.OnEffectAdded += OnEffectAdded;
                PlayerStats.Instance.OnEffectRemoved += OnEffectRemoved;
            }

            GameManager.OnScoreChanged += OnScoreChanged;
            GameManager.OnGameStateChanged += OnGameStateChanged;

            _inventory = Inventory.Instance;
            if (_inventory != null)
                _inventory.OnInventoryChanged += RefreshHotbar;
        }

        private void UnsubscribeFromEvents()
        {
            PlayerStats.OnHealthChanged -= OnHealthChanged;
            PlayerStats.OnPlayerDied -= OnPlayerDied;

            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnManaChanged -= OnManaChanged;
                PlayerStats.Instance.OnStaminaChanged -= OnStaminaChanged;
                PlayerStats.Instance.OnEffectAdded -= OnEffectAdded;
                PlayerStats.Instance.OnEffectRemoved -= OnEffectRemoved;
            }

            GameManager.OnScoreChanged -= OnScoreChanged;
            GameManager.OnGameStateChanged -= OnGameStateChanged;

            if (_inventory != null)
            {
                _inventory.OnInventoryChanged -= RefreshHotbar;
            }
        }

        // ------------
        //  CALLBACKS â€” BARRES
        // ------------
        private void OnHealthChanged(float current, float max)
        {
            if (healthFill != null)
            {
                float t = max > 0f ? current / max : 0f;
                healthFill.fillAmount = t;
                healthFill.color = Color.Lerp(colorHealthLow, colorHealthHigh, t);
            }
            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        private void OnManaChanged(float current, float max)
        {
            if (manaFill != null) manaFill.fillAmount = max > 0f ? current / max : 0f;
            if (manaText != null) manaText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        private void OnStaminaChanged(float current, float max)
        {
            if (staminaFill != null) staminaFill.fillAmount = max > 0f ? current / max : 0f;
            if (staminaText != null) staminaText.text = $"{Mathf.CeilToInt(current)}";
        }

        private void OnScoreChanged(int score)
        {
            if (scoreText != null) scoreText.text = $"Score : {score}";
        }

        private void OnInteractableChanged(string prompt)
        {
            if (interactionPrompt == null) return;
            interactionPrompt.text = string.IsNullOrEmpty(prompt) ? "" : $"[E] {prompt}";
            interactionPrompt.enabled = !string.IsNullOrEmpty(prompt);
        }

        private void OnPlayerDied() { /* GameManager â†’ TriggerGameOver â†’ OnGameStateChanged */ }

        private void OnGameStateChanged(GameManager.GameState state)
        {
            bool playing = state == GameManager.GameState.Playing;
            bool paused = state == GameManager.GameState.Paused;
            bool gameOver = state == GameManager.GameState.GameOver;
            bool victory = state == GameManager.GameState.Victory;

            _hudRoot?.SetActive(playing || paused);
            pausePanel?.SetActive(paused);
            gameOverPanel?.SetActive(gameOver);
            victoryPanel?.SetActive(victory);

            bool cursorFree = paused || gameOver || victory;
            Cursor.lockState = cursorFree ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = cursorFree;
        }

        // ------------
        //  STATUS EFFECTS
        // ------------
        private void OnEffectAdded(StatusEffectData effect)
        {
            if (_effectIcons.ContainsKey(effect.id)) return;

            var icon = BuildEffectIcon(effect);
            _effectIcons[effect.id] = icon;
        }

        private void OnEffectRemoved(StatusEffectData effect)
        {
            if (!_effectIcons.TryGetValue(effect.id, out var data)) return;
            Destroy(data.Root);
            _effectIcons.Remove(effect.id);
        }

        private void RefreshEffectTimers()
        {
            if (PlayerStats.Instance == null) return;

            foreach (var effect in PlayerStats.Instance.ActiveEffects)
            {
                if (!_effectIcons.TryGetValue(effect.id, out var data)) continue;

                float t = effect.MaxDuration > 0f ? effect.remainingTime / effect.MaxDuration : 0f;
                data.FillBar.fillAmount = Mathf.Clamp01(t);

                if (data.StackText != null && effect.maxStacks > 1)
                    data.StackText.text = effect.currentStacks > 1 ? $"x{effect.currentStacks}" : "";
            }
        }

        private EffectIconData BuildEffectIcon(StatusEffectData effect)
        {
            Color iconColor = effect.effectType == EffectType.Debuff
                ? new Color(0.9f, 0.25f, 0.1f)
                : new Color(0.2f, 0.85f, 0.5f);

            switch (effect.id)
            {
                case "poison": iconColor = new Color(0.5f, 0.9f, 0.1f); break;
                case "regeneration": iconColor = new Color(0.2f, 0.9f, 0.4f); break;
                case "mana_regen": iconColor = new Color(0.3f, 0.5f, 1.0f); break;
                case "burn": iconColor = new Color(1.0f, 0.4f, 0.0f); break;
                case "freeze": iconColor = new Color(0.5f, 0.9f, 1.0f); break;
                case "stun": iconColor = new Color(1.0f, 0.9f, 0.1f); break;
            }

            var root = MakeGO($"Effect_{effect.id}", _effectRow);
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(effectIconSize, effectIconSize + 10f);

            var border = MakeGO("Border", root.transform);
            var bRT = border.AddComponent<RectTransform>();
            bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
            bRT.offsetMin = Vector2.zero; bRT.offsetMax = Vector2.zero;
            border.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f);

            var bg = MakeGO("BG", root.transform);
            var bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = new Vector2(2, 2); bgRT.offsetMax = new Vector2(-2, -2);
            bg.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.18f);

            var iconGO = MakeGO("Icon", root.transform);
            var iconRT = iconGO.AddComponent<RectTransform>();
            float pad = effectIconSize * 0.15f;
            iconRT.anchorMin = new Vector2(0f, 0.25f);
            iconRT.anchorMax = new Vector2(1f, 1f);
            iconRT.offsetMin = new Vector2(pad, 2f);
            iconRT.offsetMax = new Vector2(-pad, -2f);
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.color = iconColor;

            if (effect.icon != null) iconImg.sprite = effect.icon;

            var barBG = MakeGO("DurBarBG", root.transform);
            var barBGRT = barBG.AddComponent<RectTransform>();
            barBGRT.anchorMin = new Vector2(0f, 0f);
            barBGRT.anchorMax = new Vector2(1f, 0f);
            barBGRT.pivot = new Vector2(0.5f, 0f);
            barBGRT.offsetMin = new Vector2(2f, 2f);
            barBGRT.offsetMax = new Vector2(-2f, 2f);
            barBGRT.sizeDelta = new Vector2(0f, 6f);
            barBG.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f);

            var barFillGO = MakeGO("DurBarFill", barBG.transform);
            var barFillRT = barFillGO.AddComponent<RectTransform>();
            barFillRT.anchorMin = Vector2.zero; barFillRT.anchorMax = Vector2.one;
            barFillRT.offsetMin = Vector2.zero; barFillRT.offsetMax = Vector2.zero;
            var barFill = barFillGO.AddComponent<Image>();
            barFill.color = iconColor;
            barFill.type = Image.Type.Filled;
            barFill.fillMethod = Image.FillMethod.Horizontal;
            barFill.fillAmount = 1f;

            TextMeshProUGUI stackTmp = null;
            if (effect.maxStacks > 1)
            {
                var stackGO = MakeGO("Stacks", root.transform);
                var stackRT = stackGO.AddComponent<RectTransform>();
                stackRT.anchorMin = new Vector2(0f, 0.55f);
                stackRT.anchorMax = new Vector2(1f, 1f);
                stackRT.offsetMin = Vector2.zero;
                stackRT.offsetMax = Vector2.zero;
                stackTmp = stackGO.AddComponent<TextMeshProUGUI>();
                stackTmp.fontSize = 10f;
                stackTmp.color = Color.white;
                stackTmp.alignment = TextAlignmentOptions.BottomRight;
            }

            return new EffectIconData
            {
                Root = root,
                FillBar = barFill,
                StackText = stackTmp,
                MaxDuration = effect.duration,
            };
        }

        // ------------
        //  HOTBAR
        // ------------
        private void HandleHotbarInput()
        {
            if (_kb == null) return;
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

            Key[] numKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5 };
            for (int i = 0; i < Mathf.Min(numKeys.Length, hotbarSlotCount); i++)
            {
                if (_kb[numKeys[i]].wasPressedThisFrame)
                {
                    SetActiveSlot(i);
                    UseActiveSlotItem();
                    break;
                }
            }
        }

        private void SetActiveSlot(int index)
        {
            _activeSlot = index;
            for (int i = 0; i < hotbarSlotCount; i++)
            {
                if (_hotbarBorders == null || i >= _hotbarBorders.Length) break;
                _hotbarBorders[i].color = (i == _activeSlot) ? hotbarActiveColor : hotbarBorderColor;
            }
        }

        private void UseActiveSlotItem()
        {
            var inv = Inventory.Instance;
            if (inv == null) return;

            var player = FindFirstObjectByType<PlayerController>();
            if (player == null) return;

            inv.UseItem(_activeSlot, player.gameObject);
        }

        private void RefreshHotbar()
        {
            var inv = Inventory.Instance;
            if (inv == null || _hotbarIconImages == null) return;

            for (int i = 0; i < hotbarSlotCount; i++)
            {
                var slot = inv.GetSlot(i);
                bool hasItem = slot != null && !slot.IsEmpty;

                _hotbarIconImages[i].enabled = hasItem;
                if (hasItem && slot.item?.icon != null)
                    _hotbarIconImages[i].sprite = slot.item.icon;

                if (hasItem)
                    _hotbarIconImages[i].color = RarityColor(slot.item.rarity);

                if (_hotbarQtyTexts[i] != null)
                {
                    _hotbarQtyTexts[i].text = (hasItem && slot.quantity > 1) ? slot.quantity.ToString() : "";
                    _hotbarQtyTexts[i].enabled = hasItem && slot.quantity > 1;
                }
            }
        }

        private Color RarityColor(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Common => new Color(0.75f, 0.75f, 0.75f),
            ItemRarity.Uncommon => new Color(0.30f, 0.80f, 0.30f),
            ItemRarity.Rare => new Color(0.20f, 0.45f, 0.95f),
            ItemRarity.Epic => new Color(0.65f, 0.20f, 0.85f),
            ItemRarity.Legendary => new Color(1.00f, 0.55f, 0.05f),
            _ => Color.white,
        };

        // ------------
        //  API PUBLIQUE
        // ------------
        public void ShowPopup(string message)
        {
            if (popupText == null) return;
            if (_popupRoutine != null) StopCoroutine(_popupRoutine);
            _popupRoutine = StartCoroutine(PopupRoutine(message));
        }

        private IEnumerator PopupRoutine(string message)
        {
            popupText.text = message;
            popupText.enabled = true;
            yield return new WaitForSeconds(popupDuration);
            popupText.enabled = false;
        }

        // --------
        //  CONSTRUCTION HUD
        // --------
        private void EnsureCanvas()
        {
            if (targetCanvas != null) return;
            targetCanvas = FindFirstObjectByType<Canvas>();
            if (targetCanvas == null)
            {
                var go = new GameObject("HUDCanvas");
                targetCanvas = go.AddComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                targetCanvas.sortingOrder = 100;
                var scaler = go.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                go.AddComponent<GraphicRaycaster>();
            }
        }

        private void BuildHUDIfNeeded()
        {
            if (healthFill != null && manaFill != null && staminaFill != null) return;

            var canvas = targetCanvas.transform;

            _hudRoot = MakeGO("HUD_Root", canvas);
            var rootRT = _hudRoot.AddComponent<RectTransform>();
            Anchor(rootRT, Vector2.up, Vector2.up, new Vector2(0f, 1f));
            rootRT.anchoredPosition = new Vector2(20f, -20f);
            rootRT.sizeDelta = new Vector2(280f, 110f);

            healthFill = BuildBar(_hudRoot.transform, "HealthBar", colorHealthHigh, 0f);
            manaFill = BuildBar(_hudRoot.transform, "ManaBar", colorMana, 32f);
            staminaFill = BuildBar(_hudRoot.transform, "StaminaBar", colorStamina, 64f);

            healthText = BuildBarLabel(_hudRoot.transform, "HealthText", 0f);
            manaText = BuildBarLabel(_hudRoot.transform, "ManaText", 32f);
            staminaText = BuildBarLabel(_hudRoot.transform, "StaminaText", 64f);

            BuildPixelBarLabel(_hudRoot.transform, "\u2665", colorHealthHigh, 0f);
            BuildPixelBarLabel(_hudRoot.transform, "\u2666", colorMana, 32f);
            BuildPixelBarLabel(_hudRoot.transform, "\u2663", colorStamina, 64f);

            scoreText = BuildLabel("ScoreText", canvas,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-20f, -20f), new Vector2(200f, 36f), 22, TextAlignmentOptions.TopRight);

            interactionPrompt = BuildLabel("InteractPrompt", canvas,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 100f), new Vector2(400f, 36f), 20, TextAlignmentOptions.Center);
            interactionPrompt.enabled = false;

            popupText = BuildLabel("PopupText", canvas,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 120f), new Vector2(500f, 50f), 26, TextAlignmentOptions.Center);
            popupText.color = new Color(1f, 0.92f, 0.3f);
            popupText.enabled = false;

            BuildEffectRow(canvas);
            BuildHotbar(canvas);

            pausePanel = BuildOverlayPanel("PausePanel", canvas, new Color(0f, 0f, 0f, 0.65f), "PAUSE");
            gameOverPanel = BuildOverlayPanel("GameOverPanel", canvas, new Color(0.4f, 0f, 0f, 0.75f), "GAME OVER");
            victoryPanel = BuildOverlayPanel("VictoryPanel", canvas, new Color(0f, 0.3f, 0f, 0.75f), "VICTOIRE !");
            pausePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            victoryPanel.SetActive(false);

            Debug.Log("[HUDSystem] HUD v2 construit (barres + effects + hotbar).");
        }

        private void BuildEffectRow(Transform canvas)
        {
            var go = MakeGO("EffectRow", canvas);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(20f, 80f);
            rt.sizeDelta = new Vector2(300f, effectIconSize + 12f);

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlHeight = true;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;

            _effectRow = go.transform;
        }

        private void BuildHotbar(Transform canvas)
        {
            _hotbarBorders = new Image[hotbarSlotCount];
            _hotbarIconImages = new Image[hotbarSlotCount];
            _hotbarQtyTexts = new TextMeshProUGUI[hotbarSlotCount];
            _hotbarKeyTexts = new TextMeshProUGUI[hotbarSlotCount];

            float totalW = hotbarSlotCount * (hotbarSlotSize + 4f) + 8f;

            var bar = MakeGO("Hotbar", canvas);
            var barRT = bar.AddComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0.5f, 0f);
            barRT.anchorMax = new Vector2(0.5f, 0f);
            barRT.pivot = new Vector2(0.5f, 0f);
            barRT.anchoredPosition = new Vector2(0f, 18f);
            barRT.sizeDelta = new Vector2(totalW, hotbarSlotSize + 20f);

            BuildPixelFrame(bar.transform, hotbarBgColor, new Color(0.05f, 0.05f, 0.05f));

            var layout = bar.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 10, 4);
            layout.spacing = 4f;
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.childAlignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < hotbarSlotCount; i++)
            {
                int slotIdx = i;
                var slot = MakeGO($"Slot_{i + 1}", bar.transform);
                var slotRT = slot.AddComponent<RectTransform>();
                slotRT.sizeDelta = new Vector2(hotbarSlotSize, hotbarSlotSize);

                var border = MakeGO("Border", slot.transform);
                var bRT = border.AddComponent<RectTransform>();
                bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
                bRT.offsetMin = Vector2.zero; bRT.offsetMax = Vector2.zero;
                var borderImg = border.AddComponent<Image>();
                borderImg.color = (i == 0) ? hotbarActiveColor : hotbarBorderColor;
                _hotbarBorders[i] = borderImg;

                var inner = MakeGO("Inner", slot.transform);
                var iRT = inner.AddComponent<RectTransform>();
                iRT.anchorMin = Vector2.zero; iRT.anchorMax = Vector2.one;
                iRT.offsetMin = new Vector2(2, 2); iRT.offsetMax = new Vector2(-2, -2);
                inner.AddComponent<Image>().color = hotbarBgColor;

                var iconGO = MakeGO("Icon", slot.transform);
                var iconRT = iconGO.AddComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(0.1f, 0.15f);
                iconRT.anchorMax = new Vector2(0.9f, 0.9f);
                iconRT.offsetMin = Vector2.zero; iconRT.offsetMax = Vector2.zero;
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.enabled = false;
                iconImg.preserveAspect = true;
                _hotbarIconImages[i] = iconImg;

                var qty = MakeGO("Qty", slot.transform);
                var qtyRT = qty.AddComponent<RectTransform>();
                qtyRT.anchorMin = new Vector2(0.5f, 0f);
                qtyRT.anchorMax = new Vector2(1f, 0.45f);
                qtyRT.offsetMin = Vector2.zero; qtyRT.offsetMax = Vector2.zero;
                var qtyTmp = qty.AddComponent<TextMeshProUGUI>();
                qtyTmp.fontSize = 11f;
                qtyTmp.color = Color.white;
                qtyTmp.alignment = TextAlignmentOptions.BottomRight;
                qtyTmp.enabled = false;
                _hotbarQtyTexts[i] = qtyTmp;

                var key = MakeGO("Key", slot.transform);
                var keyRT = key.AddComponent<RectTransform>();
                keyRT.anchorMin = Vector2.zero;
                keyRT.anchorMax = new Vector2(0.55f, 0.42f);
                keyRT.offsetMin = new Vector2(3f, 3f);
                keyRT.offsetMax = Vector2.zero;
                var keyTmp = key.AddComponent<TextMeshProUGUI>();
                keyTmp.text = (i + 1).ToString();
                keyTmp.fontSize = 10f;
                keyTmp.color = new Color(0.7f, 0.7f, 0.7f);
                keyTmp.alignment = TextAlignmentOptions.TopLeft;
                _hotbarKeyTexts[i] = keyTmp;
            }

            RefreshHotbar();
        }

        // ------------
        //  HELPERS DE CONSTRUCTION
        // ------------
        private Image BuildBar(Transform parent, string id, Color fillColor, float yOffset)
        {
            var bgGO = MakeGO(id + "_BG", parent);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0f, 1f);
            bgRT.anchorMax = new Vector2(0f, 1f);
            bgRT.pivot = new Vector2(0f, 1f);
            bgRT.anchoredPosition = new Vector2(18f, -yOffset);
            bgRT.sizeDelta = new Vector2(200f, 22f);
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.06f, 0.06f, 0.08f, 0.85f);

            BuildPixelFrame(bgGO.transform, new Color(0, 0, 0, 0), new Color(0.25f, 0.25f, 0.28f));

            var fillGO = MakeGO(id + "_Fill", bgGO.transform);
            var fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = new Vector2(2f, 2f);
            fillRT.offsetMax = new Vector2(-2f, -2f);
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1f;
            return fillImg;
        }

        private void BuildPixelBarLabel(Transform parent, string symbol, Color color, float yOffset)
        {
            var go = MakeGO("BarIcon_" + symbol, parent);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(0f, -yOffset);
            rt.sizeDelta = new Vector2(16f, 22f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = symbol;
            tmp.fontSize = 13f;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Midline;
        }

        private TextMeshProUGUI BuildBarLabel(Transform parent, string id, float yOffset)
        {
            var go = MakeGO(id, parent);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(222f, -yOffset);
            rt.sizeDelta = new Vector2(60f, 22f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 11f;
            tmp.color = new Color(0.8f, 0.8f, 0.8f);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            return tmp;
        }

        private TextMeshProUGUI BuildLabel(string id, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size, float fontSize, TextAlignmentOptions alignment)
        {
            var go = MakeGO(id, parent);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos; rt.sizeDelta = size;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = alignment;
            return tmp;
        }

        private GameObject BuildOverlayPanel(string id, Transform parent, Color bgColor, string title)
        {
            var go = MakeGO(id, parent);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = bgColor;

            var lblGO = MakeGO("Title", go.transform);
            var lblRT = lblGO.AddComponent<RectTransform>();
            lblRT.anchorMin = new Vector2(0.5f, 0.5f); lblRT.anchorMax = new Vector2(0.5f, 0.5f);
            lblRT.pivot = new Vector2(0.5f, 0.5f); lblRT.anchoredPosition = Vector2.zero;
            lblRT.sizeDelta = new Vector2(600f, 80f);
            var lbl = lblGO.AddComponent<TextMeshProUGUI>();
            lbl.text = title; lbl.fontSize = 52f; lbl.color = Color.white;
            lbl.alignment = TextAlignmentOptions.Center;
            return go;
        }

        private void BuildPixelFrame(Transform parent, Color bgColor, Color borderColor)
        {
            var bg = MakeGO("PixBG", parent);
            bg.transform.SetAsFirstSibling();
            var bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = bgColor;

            foreach (var (anchor0, anchor1, off0, off1) in new[]
            {
                (new Vector2(0f,1f), new Vector2(1f,1f), new Vector2(0f,-2f), new Vector2(0f,0f)),
                (new Vector2(0f,0f), new Vector2(1f,0f), new Vector2(0f,0f),  new Vector2(0f,2f)),
                (new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(0f,0f),  new Vector2(2f,0f)),
                (new Vector2(1f,0f), new Vector2(1f,1f), new Vector2(-2f,0f), new Vector2(0f,0f)),
            })
            {
                var edge = MakeGO("Edge", parent);
                var eRT = edge.AddComponent<RectTransform>();
                eRT.anchorMin = anchor0; eRT.anchorMax = anchor1;
                eRT.offsetMin = off0; eRT.offsetMax = off1;
                edge.AddComponent<Image>().color = borderColor;
            }
        }

        // --------
        //  UTILITAIRES
        // --------
        private static GameObject MakeGO(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot)
        {
            rt.anchorMin = min; rt.anchorMax = max; rt.pivot = pivot;
        }
    }
}
