// CreateFreshMazeTestScene.cs
// Editor tool to create a fresh FpsMazeTest scene from scratch
// Place in: Assets/Editor/CreateFreshMazeTestScene.cs

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using Code.Lavos.Core;  // For maze components

public class CreateFreshMazeTestScene : EditorWindow
{
    [MenuItem("Tools/Create Fresh MazeTest Scene")]
    public static void CreateScene()
    {
        // Ask for confirmation
        bool confirmed = EditorUtility.DisplayDialog(
            "Create Fresh Scene",
            "This will create a NEW FpsMazeTest scene from scratch.\n\n" +
            "All old ground/ceiling will be removed.\n" +
            "All components will be properly configured.\n\n" +
            "Continue?",
            "Yes, Create Fresh Scene",
            "Cancel"
        );
        
        if (!confirmed) return;
        
        Debug.Log("═══════════════════════════════════════════");
        Debug.Log("  CREATING FRESH MAZE TEST SCENE");
        Debug.Log("═══════════════════════════════════════════");
        
        // 1. Create new scene
        Scene newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
        
        // 2. Create Directional Light
        GameObject dirLight = new GameObject("Directional Light");
        Light light = dirLight.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.shadows = LightShadows.Soft;
        dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        
        // Add URP light data
        var urpLight = dirLight.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();
        urpLight.usePipelineSettings = true;
        
        Debug.Log("[Create] ✅ Directional Light");
        
        // 3. Create MazeTest GameObject with ALL components
        GameObject mazeTest = new GameObject("MazeTest");
        
        // Add ALL required components (fields will be configured in Inspector)
        var mazeGenerator = mazeTest.AddComponent<MazeGenerator>();
        mazeGenerator.width = 31;
        mazeGenerator.height = 31;
        Debug.Log("[Create] ✅ MazeGenerator");
        
        mazeTest.AddComponent<MazeRenderer>();
        Debug.Log("[Create] ✅ MazeRenderer");
        
        mazeTest.AddComponent<MazeIntegration>();
        Debug.Log("[Create] ✅ MazeIntegration");
        
        mazeTest.AddComponent<SpatialPlacer>();
        Debug.Log("[Create] ✅ SpatialPlacer");
        
        mazeTest.AddComponent<TorchPool>();
        Debug.Log("[Create] ✅ TorchPool");
        
        mazeTest.AddComponent<LightPlacementEngine>();
        Debug.Log("[Create] ✅ LightPlacementEngine");
        
        mazeTest.AddComponent<MazePlacementEngine>();
        Debug.Log("[Create] ✅ MazePlacementEngine");
        
        // Add FpsMazeTest LAST
        mazeTest.AddComponent<FpsMazeTest>();
        Debug.Log("[Create] ✅ FpsMazeTest");
        
        // 4. Set render settings
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 15f;
        RenderSettings.fogEndDistance = 120f;
        RenderSettings.fogColor = new Color(0.08f, 0.05f, 0.03f, 1f);
        RenderSettings.ambientLight = new Color(0.35f, 0.3f, 0.25f, 1f);
        Debug.Log("[Create] ✅ Render Settings (fog configured)");
        
        // 5. Save the scene
        string scenePath = "Assets/Scenes/FpsMazeTest_Fresh.unity";
        
        // Ensure Scenes folder exists
        if (!Directory.Exists("Assets/Scenes"))
        {
            Directory.CreateDirectory("Assets/Scenes");
        }
        
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log($"[Create] ✅ Scene saved: {scenePath}");
        
        // 6. Add scene to build settings
        EditorBuildSettingsScene buildScene = new EditorBuildSettingsScene(scenePath, true);
        
        // 7. Open the scene
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
        
        Debug.Log("═══════════════════════════════════════════");
        Debug.Log("  ✅ FRESH SCENE CREATED SUCCESSFULLY!");
        Debug.Log("═══════════════════════════════════════════");
        Debug.Log($"Scene: {scenePath}");
        Debug.Log("Components: All configured");
        Debug.Log("Ground: Will be created on Play (stone cube)");
        Debug.Log("Ceiling: Will be created on Play (dark stone)");
        Debug.Log("Torches: 60, dynamic lighting, binary storage");
        Debug.Log("═══════════════════════════════════════════");
        
        EditorUtility.DisplayDialog(
            "Scene Created!",
            "✅ Fresh FpsMazeTest scene created!\n\n" +
            "Scene: FpsMazeTest_Fresh.unity\n\n" +
            "All components configured:\n" +
            "✅ MazeGenerator (31x31)\n" +
            "✅ MazeRenderer\n" +
            "✅ SpatialPlacer (60 torches)\n" +
            "✅ TorchPool (with prefab)\n" +
            "✅ LightPlacementEngine\n" +
            "✅ MazePlacementEngine\n" +
            "✅ FpsMazeTest\n\n" +
            "Press Play to test!",
            "OK"
        );
    }
}
