using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity6.LavosTrial;

namespace Unity6.LavosTrial.HUD
{
    /// <summary>
    /// UIBarsSystem — Responsive UI bars positioned around screen edges
    /// 
    /// Layout per TODO:
    ///   - HealthBar (left border): vertical, 75% screen height
    ///   - ManaBar (right border): vertical, 75% screen height
    ///   - StaminaBar (bottom border): horizontal, 75% screen width
    ///   - Status Effects: top center
    /// 
    /// Features:
    ///   - Runtime screen resolution detection
    ///   - Responsive anchoring
    ///   - Pixel-perfect borders
    /// </summary>
    public class UIBarsSystem : MonoBehaviour
    {
        public static UIBarsSystem Instance { get; internal set; }

        [Header("Bar Settings")]
        [SerializeField] private float barThickness = 8f;
        [SerializeField] private float barMargin = 20f;
        [SerializeField] private float barHeightPercent = 0.75f;
        [SerializeField] private float barWidthPercent = 0.75f;

        [Header("Colors")]
        [SerializeField] private Color healthColorHigh = new Color(0.2f, 0.85f, 0.3f);
        [SerializeField] private Color healthColorLow = new Color(0.85f, 0.15f, 0.1f);
        [SerializeField] private Color manaColor = new Color(0.2f, 0.5f, 1f);
        [SerializeField] private Color staminaColor = new Color(1f, 0.75f, 0.1f);
        [SerializeField] private Color borderColor = new Color(0.15f, 0.15f, 0.18f);
        [SerializeField] private Color backgroundColor = new Color(0.05f, 0.05f, 0.07f, 0.85f);

        [Header("Status Effects")]
        [SerializeField] private float effectIconSize = 40f;
        [SerializeField] private float effectSpacing = 6f;

        private Canvas _canvas;
        private RectTransform _healthBarRoot;
        private RectTransform _manaBarRoot;
        private RectTransform _staminaBarRoot;
        private RectTransform _effectsRoot;

        private Image _healthFill;
        private Image _manaFill;
        private Image _staminaFill;

        private Transform _effectsContainer;
        private readonly Dictionary<string, EffectIconData> _effectIcons = new();

        private struct EffectIconData
        {
            public GameObject Root;
            public Image FillBar;
            public TextMeshProUGUI StackText;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            BuildBars();

            PlayerStats.OnHealthChanged += OnHealthChanged;
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnManaChanged += OnManaChanged;
                PlayerStats.Instance.OnStaminaChanged += OnStaminaChanged;
            }
        }

        private void OnHealthChanged(float current, float max) => SetHealth(current, max);
        private void OnManaChanged(float current, float max) => SetMana(current, max);
        private void OnStaminaChanged(float current, float max) => SetStamina(current, max);

        void OnDestroy()
        {
            PlayerStats.OnHealthChanged -= OnHealthChanged;
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnManaChanged -= OnManaChanged;
                PlayerStats.Instance.OnStaminaChanged -= OnStaminaChanged;
            }
        }

        void OnEnable() => UpdateBarPositions();

        void LateUpdate() => UpdateBarPositions();

        private void UpdateBarPositions()
        {
            if (_canvas == null) return;

            Rect canvasRect = _canvas.GetComponent<RectTransform>().rect;
            float screenWidth = canvasRect.width;
            float screenHeight = canvasRect.height;

            float barLength = barHeightPercent * screenHeight;
            float thickness = barThickness;

            if (_healthBarRoot != null)
            {
                _healthBarRoot.sizeDelta = new Vector2(thickness, barLength);
                _healthBarRoot.anchoredPosition = new Vector2(barMargin + thickness / 2, -barMargin - barLength / 2);
            }

            if (_manaBarRoot != null)
            {
                _manaBarRoot.sizeDelta = new Vector2(thickness, barLength);
                _manaBarRoot.anchoredPosition = new Vector2(-barMargin - thickness / 2, -barMargin - barLength / 2);
            }

            float staminaWidth = barWidthPercent * screenWidth;
            if (_staminaBarRoot != null)
            {
                _staminaBarRoot.sizeDelta = new Vector2(staminaWidth, thickness);
                _staminaBarRoot.anchoredPosition = new Vector2(0, -barMargin - thickness / 2);
            }
        }

        private void BuildBars()
        {
            EnsureCanvas();
            var canvasTransform = _canvas.transform;

            _healthBarRoot = CreateBarContainer("HealthBar", canvasTransform as Transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 1f));
            _healthFill = CreateBarFill(_healthBarRoot, healthColorHigh, Image.FillMethod.Vertical);

            _manaBarRoot = CreateBarContainer("ManaBar", canvasTransform as Transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(0.5f, 1f));
            _manaFill = CreateBarFill(_manaBarRoot, manaColor, Image.FillMethod.Vertical);

            _staminaBarRoot = CreateBarContainer("StaminaBar", canvasTransform as Transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0f));
            _staminaFill = CreateBarFill(_staminaBarRoot, staminaColor, Image.FillMethod.Horizontal);

            BuildEffectsRow(canvasTransform as Transform);

            Debug.Log("[UIBarsSystem] Bars built with responsive layout");
        }

        private RectTransform CreateBarContainer(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;

            var bg = go.AddComponent<Image>();
            bg.color = backgroundColor;

            CreateBarBorder(go.transform, borderColor);

            return rt;
        }

        private void CreateBarBorder(Transform parent, Color color)
        {
            float borderSize = 2f;

            var top = new GameObject("Border_Top");
            top.transform.SetParent(parent, false);
            var topRT = top.AddComponent<RectTransform>();
            topRT.anchorMin = new Vector2(0, 1);
            topRT.anchorMax = new Vector2(1, 1);
            topRT.offsetMin = Vector2.zero;
            topRT.offsetMax = new Vector2(0, borderSize);
            topRT.localScale = Vector3.one;
            top.AddComponent<Image>().color = color;

            var bottom = new GameObject("Border_Bottom");
            bottom.transform.SetParent(parent, false);
            var botRT = bottom.AddComponent<RectTransform>();
            botRT.anchorMin = new Vector2(0, 0);
            botRT.anchorMax = new Vector2(1, 0);
            botRT.offsetMin = new Vector2(0, -borderSize);
            botRT.offsetMax = Vector2.zero;
            botRT.localScale = Vector3.one;
            bottom.AddComponent<Image>().color = color;

            var left = new GameObject("Border_Left");
            left.transform.SetParent(parent, false);
            var leftRT = left.AddComponent<RectTransform>();
            leftRT.anchorMin = new Vector2(0, 0);
            leftRT.anchorMax = new Vector2(0, 1);
            leftRT.offsetMin = new Vector2(-borderSize, 0);
            leftRT.offsetMax = new Vector2(0, 0);
            leftRT.localScale = Vector3.one;
            left.AddComponent<Image>().color = color;

            var right = new GameObject("Border_Right");
            right.transform.SetParent(parent, false);
            var rightRT = right.AddComponent<RectTransform>();
            rightRT.anchorMin = new Vector2(1, 0);
            rightRT.anchorMax = new Vector2(1, 1);
            rightRT.offsetMin = Vector2.zero;
            rightRT.offsetMax = new Vector2(borderSize, 0);
            rightRT.localScale = Vector3.one;
            right.AddComponent<Image>().color = color;
        }

        private Image CreateBarFill(RectTransform parent, Color fillColor, Image.FillMethod fillDir)
        {
            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(parent, false);

            var rt = fillGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(2, 2);
            rt.offsetMax = new Vector2(-2, -2);

            var img = fillGO.AddComponent<Image>();
            img.color = fillColor;
            img.type = Image.Type.Filled;
            img.fillMethod = fillDir;
            img.fillAmount = 1f;

            return img;
        }

        public void SetHealth(float current, float max)
        {
            if (_healthFill != null && max > 0)
                _healthFill.fillAmount = current / max;
        }

        public void SetMana(float current, float max)
        {
            if (_manaFill != null && max > 0)
                _manaFill.fillAmount = current / max;
        }

        public void SetStamina(float current, float max)
        {
            if (_staminaFill != null && max > 0)
                _staminaFill.fillAmount = current / max;
        }

        private void BuildEffectsRow(Transform parent)
        {
            var go = new GameObject("StatusEffects");
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -barMargin - effectIconSize);
            rt.sizeDelta = new Vector2(400f, effectIconSize + 12f);

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = effectSpacing;
            layout.childControlHeight = true;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;

            _effectsContainer = go.transform;
        }

        private void EnsureCanvas()
        {
            _canvas = FindFirstObjectByType<Canvas>();
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
            }
        }
    }
}
