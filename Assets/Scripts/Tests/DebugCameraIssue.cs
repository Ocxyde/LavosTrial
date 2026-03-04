// DebugCameraIssue.cs
// Add this to MazeTest GameObject to debug camera issues
// Place in: Assets/Scripts/Tests/DebugCameraIssue.cs

using UnityEngine;
using Code.Lavos.Core;

namespace Code.Lavos.Tests
{
    public class DebugCameraIssue : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("═══════════════════════════════════════════════");
            Debug.Log("  DEBUG CAMERA ISSUE");
            Debug.Log("═══════════════════════════════════════════════");
            
            // Check components
            Debug.Log($"[Debug] FpsMazeTest: {GetComponent<FpsMazeTest>() != null}");
            Debug.Log($"[Debug] MazeGenerator: {GetComponent<MazeGenerator>() != null}");
            Debug.Log($"[Debug] MazeRenderer: {GetComponent<MazeRenderer>() != null}");
            Debug.Log($"[Debug] MazeIntegration: {GetComponent<MazeIntegration>() != null}");
            Debug.Log($"[Debug] SpatialPlacer: {GetComponent<SpatialPlacer>() != null}");
            Debug.Log($"[Debug] TorchPool: {GetComponent<TorchPool>() != null}");
            Debug.Log($"[Debug] LightPlacementEngine: {GetComponent<LightPlacementEngine>() != null}");
            
            // Check FpsMazeTest settings
            var fpsTest = GetComponent<FpsMazeTest>();
            if (fpsTest != null)
            {
                Debug.Log($"[Debug] FpsMazeTest.autoGenerateOnStart: SHOULD BE TRUE");
                Debug.Log($"[Debug] FpsMazeTest.spawnTestPlayer: SHOULD BE TRUE");
            }
            
            // Check cameras in scene
            Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            Debug.Log($"[Debug] Total cameras in scene: {allCameras.Length}");
            foreach (var cam in allCameras)
            {
                Debug.Log($"[Debug]   - Camera: {cam.gameObject.name}, enabled={cam.enabled}, tag={cam.tag}");
            }
            
            // Check if player exists
            var player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log($"[Debug] Player exists: {player != null}");
            if (player != null)
            {
                var fpsCam = player.transform.Find("FPSCamera");
                Debug.Log($"[Debug]   FPSCamera child: {fpsCam != null}");
                if (fpsCam != null)
                {
                    Debug.Log($"[Debug]   FPSCamera.enabled: {fpsCam.GetComponent<Camera>()?.enabled}");
                }
            }
            
            Debug.Log("═══════════════════════════════════════════════");
        }
    }
}
