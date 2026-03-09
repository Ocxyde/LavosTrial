// TestDeadEndConfig.cs
// Temporary debug script to verify config values
// DELETE THIS AFTER TESTING
// NOTE: This is an Editor script - must be in Editor folder or have UNITY_EDITOR define

using UnityEngine;
using Code.Lavos.Core;

#if UNITY_EDITOR
using UnityEditor;

namespace Code.Lavos.Core
{
    public class TestDeadEndConfig : MonoBehaviour
    {
        [MenuItem("Tools/DEBUG: Check Dead-End Config")]
        public static void CheckConfig()
        {
            Debug.Log("=== DEAD-END CONFIG CHECK ===");

            // Create default config
            var config = DeadEndCorridorSystem.CreateDefaultConfig();

            Debug.Log($"BaseDensity: {config.BaseDensity} (should be 0.30)");
            Debug.Log($"MaxGridPercentage: {config.MaxGridPercentage} (should be 0.35)");
            Debug.Log($"MinLength: {config.MinLength} (should be 3)");
            Debug.Log($"MaxLength: {config.MaxLength} (should be 8)");
            Debug.Log($"AllowBranching: {config.AllowBranching} (should be true)");

            if (config.BaseDensity < 0.5f)
            {
                Debug.LogWarning($"BaseDensity is {config.BaseDensity:P1} (expected 30% base, scales to 75% at level 39)");
                Debug.Log("Check: Config/DeadEndCorridorConfig.json");
            }
            else
            {
                Debug.Log("✅ CONFIG CORRECT!");
            }

            Debug.Log("=== END CHECK ===");
        }
    }
}
#endif
