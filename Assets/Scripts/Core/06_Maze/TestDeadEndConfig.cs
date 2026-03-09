// TestDeadEndConfig.cs
// Temporary debug script to verify config values
// DELETE THIS AFTER TESTING

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;

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
            
            Debug.Log($"baseDensity: {config.baseDensity} (should be 0.85)");
            Debug.Log($"maxGridPercentage: {config.maxGridPercentage} (should be 0.35)");
            Debug.Log($"minLength: {config.minLength} (should be 3)");
            Debug.Log($"maxLength: {config.maxLength} (should be 8)");
            Debug.Log($"allowBranching: {config.allowBranching} (should be true)");
            
            if (config.baseDensity < 0.5f)
            {
                Debug.LogError("❌ CONFIG WRONG! baseDensity should be 0.85 but is " + config.baseDensity);
                Debug.LogError("Check: Config/DeadEndCorridorConfig.json");
            }
            else
            {
                Debug.Log("✅ CONFIG CORRECT!");
            }
            
            Debug.Log("=== END CHECK ===");
        }
    }
}
