// ============================================================================
// ⚠️  DEPRECATED - SAFE TO DELETE
// ============================================================================
// This file is marked for deletion. It references UIBarsSystemStandalone 
// which does not exist in this project. UIBarsSystem.cs is the active 
// implementation and does not require this initializer.
//
// Reason: Obsolete wrapper with no functional purpose
// Safe to delete: YES
// Date marked: 2026-03-02
// ============================================================================

// UIBarsSystemInitializer.cs
// DEPRECATED - Use UIBarsSystem.cs directly
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// This file is kept for backward compatibility only.
// New projects should use UIBarsSystem.cs directly.

using UnityEngine;
using UnityEngine.UI;

namespace Code.Lavos.HUD
{
    /// <summary>
    /// ⚠️  DEPRECATED: This initializer is obsolete.
    /// Use UIBarsSystem directly instead.
    /// </summary>
    [System.Obsolete("Use UIBarsSystem directly - this wrapper is obsolete")]
    public class UIBarsSystemInitializer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject player;

        private void Awake()
        {
            Debug.LogWarning("[UIBarsSystemInitializer] ⚠️  DEPRECATED: This component is obsolete. Use UIBarsSystem directly instead.");

            // Auto-create UIBarsSystem
            var uiGO = new GameObject("UIBarsSystem");
            uiGO.AddComponent<UIBarsSystem>();
            DontDestroyOnLoad(uiGO);
        }
    }
}
