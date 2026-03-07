// PersistentUI.cs
// Persistent UI manager that survives scene loads
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Persistent UI for plug-in-and-out system

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Persistent UI manager - survives scene loads.
    /// Attach to any UI GameObject that should persist across scenes.
    /// </summary>
    public class PersistentUI : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}