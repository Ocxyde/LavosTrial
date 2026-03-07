// HUDEngine.cs
// Central HUD management system - Plug-in-and-Out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Architecture:
//   HUDEngine (central manager)
//   ├── HUDModule (base class for all HUD components)
//   │   ├── BarsModule (health, mana, stamina bars)
//   │   ├── StatusEffectsModule (buff/debuff icons)
//   │   ├── HotbarModule (item slots)
//   │   ├── PopupModule (damage numbers, notifications)
//   │   ├── PauseMenuModule (pause UI)
//   │   └── Custom modules...
//   └── HUDCanvas (canvas management)

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Code.Lavos.Status;

namespace Code.Lavos.HUD
{
    /// <summary>
    /// Central HUD management engine.
    /// Handles module registration, lifecycle, and global HUD state.
    /// Designed for plug-in-and-out modularity.
    /// </summary>
    public class HUDEngine : MonoBehaviour
    {
        private static HUDEngine _instance;
        public static HUDEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<HUDEngine>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("HUDEngine");
                        _instance = go.AddComponent<HUDEngine>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("HUD Settings")]
        [SerializeField] private bool enableHUD = true;
        [SerializeField] private bool showDebugInfo = false;

        [Header("Canvas Settings")]
        [SerializeField] private RenderMode canvasRenderMode = RenderMode.ScreenSpaceOverlay;
        [SerializeField] private int sortingOrder = 100;
        [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);

        [Header("Module Settings")]
        [SerializeField] private bool autoRegisterModules = true;

        // Module registry
        private Dictionary<System.Type, HUDModule> _modules = new();
        private List<HUDModule> _activeModules = new();

        // Canvas
        private Canvas _canvas;
        private RectTransform _canvasRect;
        private CanvasScaler _scaler;
        private GraphicRaycaster _raycaster;

        // Module container transforms
        private Transform _moduleRoot;

        // Events
        public System.Action<HUDModule> OnModuleRegistered;
        public System.Action<HUDModule> OnModuleUnregistered;
        public System.Action OnHUDInitialized;
        public System.Action OnHUDDisabled;
        public System.Action OnHUDEnabled;

        public bool IsEnabled => enableHUD;
        public Canvas Canvas => _canvas;
        public RectTransform CanvasRect => _canvasRect;
        public int ModuleCount => _modules?.Count ?? 0;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            _modules = new Dictionary<System.Type, HUDModule>();
            _activeModules = new List<HUDModule>();

            CreateCanvas();
            CreateModuleRoot();

            if (autoRegisterModules)
            {
                RegisterDefaultModules();
            }

            Debug.Log("[HUDEngine] Initialized");
            OnHUDInitialized?.Invoke();
        }

        #region Canvas Management

        private void CreateCanvas()
        {
            // Try to find existing canvas (Unity 6 standard)
            _canvas = FindFirstObjectByType<Canvas>();

            if (_canvas == null)
            {
                GameObject go = new GameObject("HUD_Canvas");
                go.transform.SetParent(transform);

                _canvas = go.AddComponent<Canvas>();
                _canvas.renderMode = canvasRenderMode;
                _canvas.sortingOrder = sortingOrder;

                _scaler = go.AddComponent<CanvasScaler>();
                _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _scaler.referenceResolution = referenceResolution;
                _scaler.matchWidthOrHeight = 0.5f;

                _raycaster = go.AddComponent<GraphicRaycaster>();
            }

            _canvasRect = _canvas.GetComponent<RectTransform>();
        }

        private void CreateModuleRoot()
        {
            GameObject go = new GameObject("ModuleRoot");
            go.transform.SetParent(_canvas.transform, false);
            _moduleRoot = go.GetComponent<RectTransform>();

            // Full screen anchor
            var rt = _moduleRoot as RectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        #endregion

        #region Module Management

        /// <summary>
        /// Register a HUD module.
        /// </summary>
        public void RegisterModule(HUDModule module)
        {
            if (module == null)
            {
                Debug.LogError("[HUDEngine] Cannot register null module");
                return;
            }

            var type = module.GetType();
            if (_modules.ContainsKey(type))
            {
                Debug.LogWarning($"[HUDEngine] Module {type.Name} already registered");
                return;
            }

            _modules[type] = module;
            _activeModules.Add(module);

            // Initialize module
            module.Initialize(this);
            module.OnEnable();

            OnModuleRegistered?.Invoke(module);
            Debug.Log($"[HUDEngine] Registered module: {type.Name}");
        }

        /// <summary>
        /// Unregister a HUD module.
        /// </summary>
        public void UnregisterModule<T>() where T : HUDModule
        {
            var type = typeof(T);
            if (!_modules.TryGetValue(type, out var module))
            {
                Debug.LogWarning($"[HUDEngine] Module {type.Name} not found");
                return;
            }

            module.OnDisable();
            module.Cleanup();

            _modules.Remove(type);
            _activeModules.Remove(module);

            OnModuleUnregistered?.Invoke(module);
            Debug.Log($"[HUDEngine] Unregistered module: {type.Name}");
        }

        /// <summary>
        /// Get a registered module by type.
        /// </summary>
        public T GetModule<T>() where T : HUDModule
        {
            var type = typeof(T);
            if (_modules.TryGetValue(type, out var module))
            {
                return module as T;
            }
            return null;
        }

        /// <summary>
        /// Check if a module is registered.
        /// </summary>
        public bool HasModule<T>() where T : HUDModule
        {
            return _modules.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Register default modules (bars, status effects, hotbar, popup).
        /// </summary>
        private void RegisterDefaultModules()
        {
            // Bars Module
            var barsGO = new GameObject("BarsModule");
            barsGO.transform.SetParent(_moduleRoot);
            var barsModule = barsGO.AddComponent<BarsModule>();
            RegisterModule(barsModule);

            // Status Effects Module
            var effectsGO = new GameObject("StatusEffectsModule");
            effectsGO.transform.SetParent(_moduleRoot);
            var effectsModule = effectsGO.AddComponent<StatusEffectsModule>();
            RegisterModule(effectsModule);

            // Hotbar Module
            var hotbarGO = new GameObject("HotbarModule");
            hotbarGO.transform.SetParent(_moduleRoot);
            var hotbarModule = hotbarGO.AddComponent<HotbarModule>();
            RegisterModule(hotbarModule);

            // Popup Module
            var popupGO = new GameObject("PopupModule");
            popupGO.transform.SetParent(_moduleRoot);
            var popupModule = popupGO.AddComponent<PopupModule>();
            RegisterModule(popupModule);

            Debug.Log($"[HUDEngine] Registered {_modules.Count} default modules");
        }

        #endregion

        #region HUD State Management

        /// <summary>
        /// Enable the entire HUD.
        /// </summary>
        public void EnableHUD()
        {
            enableHUD = true;
            _canvas.enabled = true;

            foreach (var module in _activeModules)
            {
                module.OnEnable();
            }

            Debug.Log("[HUDEngine] HUD enabled");
            OnHUDEnabled?.Invoke();
        }

        /// <summary>
        /// Disable the entire HUD.
        /// </summary>
        public void DisableHUD()
        {
            enableHUD = false;
            _canvas.enabled = false;

            foreach (var module in _activeModules)
            {
                module.OnDisable();
            }

            Debug.Log("[HUDEngine] HUD disabled");
            OnHUDDisabled?.Invoke();
        }

        /// <summary>
        /// Toggle HUD visibility.
        /// </summary>
        public void ToggleHUD()
        {
            if (enableHUD)
                DisableHUD();
            else
                EnableHUD();
        }

        /// <summary>
        /// Show/hide specific module.
        /// </summary>
        public void SetModuleVisible<T>(bool visible) where T : HUDModule
        {
            var module = GetModule<T>();
            if (module != null)
            {
                module.gameObject.SetActive(visible);
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get canvas RectTransform for module positioning.
        /// </summary>
        public RectTransform GetCanvasRect() => _canvasRect;

        /// <summary>
        /// Get module root transform.
        /// </summary>
        public Transform GetModuleRoot() => _moduleRoot;

        /// <summary>
        /// Get all registered modules.
        /// </summary>
        public List<HUDModule> GetAllModules() => new List<HUDModule>(_activeModules);

        /// <summary>
        /// Get HUD statistics.
        /// </summary>
        public HUDStatistics GetStatistics()
        {
            var stats = new HUDStatistics
            {
                totalModules = _modules.Count,
                activeModules = _activeModules.Count,
                isHUDVisible = enableHUD && _canvas.enabled
            };

            foreach (var module in _activeModules)
            {
                if (module is BarsModule) stats.hasBars = true;
                if (module is StatusEffectsModule) stats.hasStatusEffects = true;
                if (module is HotbarModule) stats.hasHotbar = true;
                if (module is PopupModule) stats.hasPopup = true;
            }

            return stats;
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!showDebugInfo || _canvasRect == null)
                return;

            // Draw canvas bounds
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Vector3 center = _canvasRect.position;
            Vector3 size = new Vector3(_canvasRect.rect.width, _canvasRect.rect.height, 0);
            Gizmos.DrawWireCube(center, size);

            // Draw module count
#if UNITY_EDITOR
            var labelStyle = new GUIStyle()
            {
                normal = { textColor = Color.green }
            };
            UnityEditor.Handles.Label(center + Vector3.up * (_canvasRect.rect.height / 2 + 20),
                $"HUD Modules: {_modules.Count}", labelStyle);
#endif
        }

        #endregion

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            // Cleanup all modules
            foreach (var module in _activeModules)
            {
                if (module != null)
                {
                    module.Cleanup();
                }
            }

            _modules?.Clear();
            _activeModules?.Clear();
        }
    }

    #region Statistics

    public struct HUDStatistics
    {
        public int totalModules;
        public int activeModules;
        public bool isHUDVisible;
        public bool hasBars;
        public bool hasStatusEffects;
        public bool hasHotbar;
        public bool hasPopup;
    }

    #endregion
}
