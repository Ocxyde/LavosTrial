// TorchDiagnostics.cs
// Diagnostic tools for torch system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Ressources system - debugging

// TorchDiagnostics.cs
// Diagnostic tools for torch system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Ressources system - debugging
using UnityEngine;
using Code.Lavos.Core;

namespace Code.Lavos.Core
{
// Lightweight runtime diagnostics for the TorchPool system
// Logs counts of active torches and pooled torches to help verify pool behavior.
public class TorchDiagnostics : MonoBehaviour
{
    void Start()
    {
        var pool = UnityEngine.Object.FindFirstObjectByType<TorchPool>();
        if (pool != null)
        {
            Debug.Log("[TorchDiagnostics] Initial: Active=" + pool.ActiveCount + ", Pooled=" + pool.PooledCount);
        }
        else
        {
            Debug.Log("[TorchDiagnostics] TorchPool not found in scene at Start");
        }
    }

    void Update()
    {
        // Periodically log for ongoing visibility without being too noisy
        if (Time.frameCount % 300 == 0)
        {
            var pool = UnityEngine.Object.FindFirstObjectByType<TorchPool>();
            if (pool != null)
            {
                Debug.Log("[TorchDiagnostics] Tick: Active=" + pool.ActiveCount + ", Pooled=" + pool.PooledCount);
            }
        }
    }
    }
}
