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
// HUDModule.cs
// Base class for all HUD modules in HUDEngine system
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Code.Lavos.HUD
{
    /// <summary>
    /// Base class for HUD modules.
    /// All HUD components should inherit from this class.
    /// </summary>
    public abstract class HUDModule : MonoBehaviour
    {
        protected HUDEngine _engine;
        protected RectTransform _rectTransform;
        protected Canvas _canvas;

        /// <summary>
        /// Called when module is registered with HUDEngine.
        /// </summary>
        public virtual void Initialize(HUDEngine engine)
        {
            _engine = engine;
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponent<Canvas>();
        }

        /// <summary>
        /// Called when HUD is enabled.
        /// </summary>
        public virtual void OnEnable()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Called when HUD is disabled.
        /// </summary>
        public virtual void OnDisable()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Called when module is unregistered.
        /// </summary>
        public virtual void Cleanup()
        {
            // Override for cleanup logic
        }

        /// <summary>
        /// Get canvas for spawning UI elements.
        /// </summary>
        protected Canvas GetCanvas() => _engine?.Canvas;

        /// <summary>
        /// Get canvas RectTransform for positioning.
        /// </summary>
        protected RectTransform GetCanvasRect() => _engine?.CanvasRect;

        /// <summary>
        /// Get module root transform.
        /// </summary>
        protected Transform GetModuleRoot() => _engine?.GetModuleRoot();

        /// <summary>
        /// Create a UI GameObject with RectTransform.
        /// </summary>
        protected GameObject CreateUIElement(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;

            return go;
        }

        /// <summary>
        /// Create an Image component for UI elements.
        /// </summary>
        protected Image CreateImage(GameObject go, Color color)
        {
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        /// <summary>
        /// Set module visibility.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// Check if module is visible.
        /// </summary>
        public bool IsVisible() => gameObject.activeSelf;
    }

    /// <summary>
    /// Bars module - Health, Mana, Stamina bars.
    /// </summary>
    public class BarsModule : HUDModule
    {
        [Header("Bar Settings")]
        [SerializeField] private float barThickness = 25f;

        [Header("References")]
        [SerializeField] private RectTransform healthBarRoot;
        [SerializeField] private RectTransform manaBarRoot;
        [SerializeField] private RectTransform staminaBarRoot;

        [Header("Bars")]
        [SerializeField] private Image healthFill;
        [SerializeField] private Image manaFill;
        [SerializeField] private Image staminaFill;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI manaText;
        [SerializeField] private TextMeshProUGUI staminaText;

        private float _currentHealth = 1000f;
        private float _maxHealth = 100f;
        private float _currentMana = 100f;
        private float _maxMana = 100f;
        private float _currentStamina = 100f;
        private float _maxStamina = 100f;

        public override void Initialize(HUDEngine engine)
        {
            base.Initialize(engine);
            BuildBars();
            SubscribeToEvents();
        }

        private void BuildBars()
        {
            var canvasTransform = GetCanvas().transform;

            // Health Bar (Left - Vertical)
            healthBarRoot = CreateBarContainer("HealthBar", canvasTransform, Image.FillMethod.Vertical, Color.green);
            healthFill = healthBarRoot.Find("Fill").GetComponent<Image>();
            healthText = CreateBarText(healthBarRoot, "HealthText");

            // Mana Bar (Right - Vertical)
            manaBarRoot = CreateBarContainer("ManaBar", canvasTransform, Image.FillMethod.Vertical, Color.blue);
            manaFill = manaBarRoot.Find("Fill").GetComponent<Image>();
            manaText = CreateBarText(manaBarRoot, "ManaText");

            // Stamina Bar (Bottom - Horizontal)
            staminaBarRoot = CreateBarContainer("StaminaBar", canvasTransform, Image.FillMethod.Horizontal, Color.yellow);
            staminaFill = staminaBarRoot.Find("Fill").GetComponent<Image>();
            staminaText = CreateBarText(staminaBarRoot, "StaminaText");
        }

        private RectTransform CreateBarContainer(string name, Transform parent, Image.FillMethod fillMethod, Color fillColor)
        {
            var go = CreateUIElement(name, parent);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(fillMethod == Image.FillMethod.Vertical ? barThickness : 300f,
                                        fillMethod == Image.FillMethod.Vertical ? 400f : barThickness);

            // Background
            var bg = CreateImage(go, new Color(0.1f, 0.1f, 0.12f));

            // Fill
            var fillGO = CreateUIElement("Fill", go.transform);
            var fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = new Vector2(2, 2);
            fillRT.offsetMax = new Vector2(-2, -2);

            var fillImg = CreateImage(fillGO, fillColor);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = fillMethod;

            return rt;
        }

        private TextMeshProUGUI CreateBarText(RectTransform parent, string name)
        {
            var go = CreateUIElement(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 36;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }

        private void SubscribeToEvents()
        {
            // Use reflection to subscribe to PlayerStats events (avoids circular dependency)
            var statsType = System.Type.GetType("Code.Lavos.Status.PlayerStats, Code.Lavos.Status");
            if (statsType != null)
            {
                var onHealthEvent = statsType.GetEvent("OnHealthChanged", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (onHealthEvent != null) onHealthEvent.AddEventHandler(null, new System.Action<float, float>(OnHealthChanged));

                var instanceProp = statsType.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                var playerStatsInstance = instanceProp?.GetValue(null) as MonoBehaviour;

                if (playerStatsInstance != null)
                {
                    var onManaEvent = statsType.GetEvent("OnManaChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    var onStaminaEvent = statsType.GetEvent("OnStaminaChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

                    if (onManaEvent != null) onManaEvent.AddEventHandler(playerStatsInstance, new System.Action<float, float>(OnManaChanged));
                    if (onStaminaEvent != null) onStaminaEvent.AddEventHandler(playerStatsInstance, new System.Action<float, float>(OnStaminaChanged));
                }
            }
        }

        private void OnHealthChanged(float current, float max) => SetHealth(current, max);
        private void OnManaChanged(float current, float max) => SetMana(current, max);
        private void OnStaminaChanged(float current, float max) => SetStamina(current, max);

        public void SetHealth(float current, float max)
        {
            _currentHealth = Mathf.Clamp(current, 0, max);
            _maxHealth = max > 0 ? max : 1;

            if (healthFill != null)
            {
                healthFill.fillAmount = _currentHealth / _maxHealth;
            }
            if (healthText != null)
            {
                healthText.text = $"{(_currentHealth / _maxHealth * 100):F0}%";
            }
        }

        public void SetMana(float current, float max)
        {
            _currentMana = Mathf.Clamp(current, 0, max);
            _maxMana = max > 0 ? max : 1;

            if (manaFill != null)
            {
                manaFill.fillAmount = _currentMana / _maxMana;
            }
            if (manaText != null)
            {
                manaText.text = $"{(_currentMana / _maxMana * 100):F0}%";
            }
        }

        public void SetStamina(float current, float max)
        {
            _currentStamina = Mathf.Clamp(current, 0, max);
            _maxStamina = max > 0 ? max : 1;

            if (staminaFill != null)
            {
                staminaFill.fillAmount = _currentStamina / _maxStamina;
            }
            if (staminaText != null)
            {
                staminaText.text = $"{(_currentStamina / _maxStamina * 100):F0}%";
            }
        }

        public override void Cleanup()
        {
            // Use reflection to unsubscribe from PlayerStats events (avoids circular dependency)
            var statsType = System.Type.GetType("Code.Lavos.Status.PlayerStats, Code.Lavos.Status");
            if (statsType != null)
            {
                var onHealthEvent = statsType.GetEvent("OnHealthChanged", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (onHealthEvent != null) onHealthEvent.RemoveEventHandler(null, new System.Action<float, float>(OnHealthChanged));

                var instanceProp = statsType.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                var playerStatsInstance = instanceProp?.GetValue(null) as MonoBehaviour;

                if (playerStatsInstance != null)
                {
                    var onManaEvent = statsType.GetEvent("OnManaChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    var onStaminaEvent = statsType.GetEvent("OnStaminaChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

                    if (onManaEvent != null) onManaEvent.RemoveEventHandler(playerStatsInstance, new System.Action<float, float>(OnManaChanged));
                    if (onStaminaEvent != null) onStaminaEvent.RemoveEventHandler(playerStatsInstance, new System.Action<float, float>(OnStaminaChanged));
                }
            }
        }
    }

    /// <summary>
    /// Status effects module - Buff/Debuff icons.
    /// </summary>
    public class StatusEffectsModule : HUDModule
    {
        [Header("Settings")]
        [SerializeField] private float effectIconSize = 40f;
        [SerializeField] private float effectSpacing = 8f;

        private Transform _effectsContainer;
        private System.Collections.Generic.Dictionary<string, EffectIconData> _effectIcons = new();

        private struct EffectIconData
        {
            public GameObject Root;
            public Image FillBar;
            public TextMeshProUGUI StackText;
        }

        public override void Initialize(HUDEngine engine)
        {
            base.Initialize(engine);
            BuildEffectsRow();
            SubscribeToEvents();
        }

        private void BuildEffectsRow()
        {
            var go = CreateUIElement("StatusEffectsRow", GetModuleRoot());
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -15f);
            rt.sizeDelta = new Vector2(600f, effectIconSize + 16f);

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = effectSpacing;
            layout.childAlignment = TextAnchor.UpperCenter;

            _effectsContainer = go.transform;
        }

        private void SubscribeToEvents()
        {
            // Use reflection to subscribe to PlayerStats events (avoids circular dependency)
            var statsType = System.Type.GetType("Code.Lavos.Status.PlayerStats, Code.Lavos.Status");
            if (statsType != null)
            {
                var instanceProp = statsType.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                var playerStatsInstance = instanceProp?.GetValue(null) as MonoBehaviour;

                if (playerStatsInstance != null)
                {
                    var onEffectAddedEvent = statsType.GetEvent("OnEffectAdded", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    var onEffectRemovedEvent = statsType.GetEvent("OnEffectRemoved", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

                    if (onEffectAddedEvent != null) onEffectAddedEvent.AddEventHandler(playerStatsInstance, new System.Action<Code.Lavos.Status.StatusEffectData>(OnEffectAdded));
                    if (onEffectRemovedEvent != null) onEffectRemovedEvent.AddEventHandler(playerStatsInstance, new System.Action<Code.Lavos.Status.StatusEffectData>(OnEffectRemoved));
                }
            }
        }

        private void OnEffectAdded(Code.Lavos.Status.StatusEffectData effect)
        {
            if (_effectIcons.ContainsKey(effect.id)) return;

            var icon = BuildEffectIcon(effect);
            _effectIcons[effect.id] = icon;
        }

        private void OnEffectRemoved(Code.Lavos.Status.StatusEffectData effect)
        {
            if (!_effectIcons.TryGetValue(effect.id, out var data)) return;
            Destroy(data.Root);
            _effectIcons.Remove(effect.id);
        }

        private EffectIconData BuildEffectIcon(Code.Lavos.Status.StatusEffectData effect)
        {
            var root = CreateUIElement($"Effect_{effect.id}", _effectsContainer);
            var rt = root.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(effectIconSize, effectIconSize + 10f);

            // Icon
            var iconGO = CreateUIElement("Icon", root.transform);
            var iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            var iconImg = CreateImage(iconGO, effect.effectType == Code.Lavos.Status.EffectType.Debuff ? Color.red : Color.green);

            // Duration bar
            var barBG = CreateUIElement("DurBarBG", root.transform);
            var barBGRT = barBG.AddComponent<RectTransform>();
            barBGRT.anchorMin = new Vector2(0f, 0f);
            barBGRT.anchorMax = new Vector2(1f, 0f);
            barBGRT.sizeDelta = new Vector2(0f, 6f);

            var barFillGO = CreateUIElement("DurBarFill", barBG.transform);
            var barFillRT = barFillGO.AddComponent<RectTransform>();
            barFillRT.anchorMin = Vector2.zero;
            barFillRT.anchorMax = Vector2.one;
            var barFill = CreateImage(barFillGO, Color.white);
            barFill.type = Image.Type.Filled;
            barFill.fillMethod = Image.FillMethod.Horizontal;

            return new EffectIconData { Root = root, FillBar = barFill, StackText = null };
        }

        public override void Cleanup()
        {
            // Use reflection to unsubscribe from PlayerStats events (avoids circular dependency)
            var statsType = System.Type.GetType("Code.Lavos.Status.PlayerStats, Code.Lavos.Status");
            if (statsType != null)
            {
                var instanceProp = statsType.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                var playerStatsInstance = instanceProp?.GetValue(null) as MonoBehaviour;

                if (playerStatsInstance != null)
                {
                    var onEffectAddedEvent = statsType.GetEvent("OnEffectAdded", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    var onEffectRemovedEvent = statsType.GetEvent("OnEffectRemoved", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

                    if (onEffectAddedEvent != null) onEffectAddedEvent.RemoveEventHandler(playerStatsInstance, new System.Action<Code.Lavos.Status.StatusEffectData>(OnEffectAdded));
                    if (onEffectRemovedEvent != null) onEffectRemovedEvent.RemoveEventHandler(playerStatsInstance, new System.Action<Code.Lavos.Status.StatusEffectData>(OnEffectRemoved));
                }
            }
        }
    }

    /// <summary>
    /// Hotbar module - Item slots (1-5 keys).
    /// </summary>
    public class HotbarModule : HUDModule
    {
        [Header("Settings")]
        [SerializeField] private int slotCount = 5;
        [SerializeField] private float slotSize = 52f;

        private Image[] _slotBorders;
        private int _activeSlot = 0;

        public override void Initialize(HUDEngine engine)
        {
            base.Initialize(engine);
            BuildHotbar();
        }

        private void BuildHotbar()
        {
            var go = CreateUIElement("Hotbar", GetModuleRoot());
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 20f);
            rt.sizeDelta = new Vector2(slotCount * (slotSize + 4f), slotSize + 8f);

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.LowerCenter;

            _slotBorders = new Image[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                var slot = CreateUIElement($"Slot_{i + 1}", go.transform);
                var slotRT = slot.AddComponent<RectTransform>();
                slotRT.sizeDelta = new Vector2(slotSize, slotSize);

                var border = CreateImage(slot, Color.gray);
                _slotBorders[i] = border;

                // Key label
                var keyGO = CreateUIElement($"Key_{i + 1}", slot.transform);
                var keyRT = keyGO.AddComponent<RectTransform>();
                keyRT.anchorMin = new Vector2(1f, 1f);
                keyRT.anchorMax = new Vector2(1f, 1f);
                keyRT.offsetMin = new Vector2(-20f, -20f);
                keyRT.offsetMax = new Vector2(0f, 0f);

                var keyText = keyGO.AddComponent<TextMeshProUGUI>();
                keyText.text = (i + 1).ToString();
                keyText.fontSize = 14;
                keyText.color = Color.white;
            }

            UpdateActiveSlot();
        }

        public void SetActiveSlot(int slot)
        {
            _activeSlot = Mathf.Clamp(slot, 0, slotCount - 1);
            UpdateActiveSlot();
        }

        private void UpdateActiveSlot()
        {
            for (int i = 0; i < _slotBorders.Length; i++)
            {
                if (_slotBorders[i] != null)
                {
                    _slotBorders[i].color = i == _activeSlot ? Color.yellow : Color.gray;
                }
            }
        }
    }

    /// <summary>
    /// Popup module - Damage numbers, notifications.
    /// </summary>
    public class PopupModule : HUDModule
    {
        [Header("Settings")]
        [SerializeField] private float popupDuration = 2f;
        [SerializeField] private float popupSpeed = 1f;

        public override void Initialize(HUDEngine engine)
        {
            base.Initialize(engine);
        }

        public void ShowPopup(string text, Vector3 position, Color color)
        {
            var go = CreateUIElement("Popup", GetModuleRoot());
            var rt = go.AddComponent<RectTransform>();
            rt.position = position;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.color = color;
            tmp.fontStyle = TMPro.FontStyles.Bold;

            // Simple fade out coroutine
            _engine.StartCoroutine(FadeOutPopup(go, tmp));
        }

        private System.Collections.IEnumerator FadeOutPopup(GameObject go, TextMeshProUGUI text)
        {
            float elapsed = 0f;
            Color original = text.color;

            while (elapsed < popupDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / popupDuration;

                var rt = go.GetComponent<RectTransform>();
                rt.anchoredPosition += Vector2.up * popupSpeed * Time.deltaTime * 100f;

                text.color = new Color(original.r, original.g, original.b, original.a * (1 - t));
                yield return null;
            }

            Destroy(go);
        }
    }
}
