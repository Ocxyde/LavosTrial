// UIBarsSystemInitializer.cs
// Deprecated - Use UIBarsSystemStandalone.cs instead
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// This file is kept for backward compatibility only.
// New projects should use UIBarsSystemStandalone.cs

using UnityEngine;
using UnityEngine.UI;

namespace Unity6.LavosTrial.HUD
{
    /// <summary>
    /// Deprecated initializer for UIBarsSystem.
    /// Use UIBarsSystemStandalone instead.
    /// </summary>
    [System.Obsolete("Use UIBarsSystemStandalone instead")]
    public class UIBarsSystemInitializer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject player;

        private void Awake()
        {
            Debug.LogWarning("[UIBarsSystemInitializer] This component is deprecated. Use UIBarsSystemStandalone instead.");
            
            // Auto-create UIBarsSystem
            var uiGO = new GameObject("UIBarsSystem");
            uiGO.AddComponent<UIBarsSystem>();
            DontDestroyOnLoad(uiGO);
        }
    }
}
