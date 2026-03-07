// UIBarsSystemStandalone.cs
// Standalone UIBarsSystem - auto-creates on scene load
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// SETUP: Add this script to any GameObject in your scene
// The UIBarsSystem will be created automatically

using UnityEngine;

namespace Unity6.LavosTrial.HUD
{
    /// <summary>
    /// Standalone initializer for UIBarsSystem.
    /// Automatically creates and configures UIBarsSystem on scene load.
    /// </summary>
    public class UIBarsSystemStandalone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool showDebugLogs = true;

        private static UIBarsSystemStandalone _instance;
        private static UIBarsSystem _uiBarsInstance;

        private void Awake()
        {
            if (_instance != null)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[UIBarsSystemStandalone] Instance already exists, destroying duplicate");
                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            // Auto-create UIBarsSystem
            var uiBarsGO = new GameObject("UIBarsSystem_Instance");
            _uiBarsInstance = uiBarsGO.AddComponent<UIBarsSystem>();
            
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(uiBarsGO);
            }

            if (showDebugLogs)
            {
                Debug.Log("[UIBarsSystemStandalone] UIBarsSystem created successfully");
                Debug.Log($"[UIBarsSystemStandalone] Instance: {_instance != null}");
                Debug.Log($"[UIBarsSystemStandalone] UIBarsSystem: {_uiBarsInstance != null}");
            }
        }

        public static UIBarsSystem GetUIBarsSystem()
        {
            if (_instance == null)
            {
                Debug.LogError("[UIBarsSystemStandalone] No instance found! Make sure UIBarsSystemStandalone is in your scene");
                return null;
            }

            if (_uiBarsInstance == null)
            {
                _uiBarsInstance = FindFirstObjectByType<UIBarsSystem>();
            }

            if (_uiBarsInstance == null)
            {
                Debug.LogError("[UIBarsSystemStandalone] UIBarsSystem not found! It may have been destroyed");
                return null;
            }

            return _uiBarsInstance;
        }
    }
}
