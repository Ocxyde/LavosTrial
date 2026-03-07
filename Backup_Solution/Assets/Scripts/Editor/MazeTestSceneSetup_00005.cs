// MazeTestSceneSetup.cs
// Editor script to auto-configure test scene
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Usage: Tools → Maze Test → Setup Test Scene
//
// Location: Assets/Scripts/Editor/

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// MazeTestSceneSetup - Auto-configure test scene for maze with torches.
    /// </summary>
    public class MazeTestSceneSetup : EditorWindow
    {
        private bool includeRooms = true;
        private bool includeDoors = true;
        private bool includeTorches = true;
        private int mazeSize = 21;

        [Header("Test Player")]
        [Tooltip("Spawn a test player with camera")]
        [SerializeField] private bool spawnTestPlayer = true;

        [Tooltip("Camera distance from player (eye level)")]
        [SerializeField] private float cameraDistance = 3.5f;

        [MenuItem("Tools/Maze Test/Setup Test Scene")]
        public static void ShowWindow()
        {
            var window = GetWindow<MazeTestSceneSetup>("Maze Test Setup");
            window.minSize = new Vector2(300, 400);
        }

        void OnGUI()
        {
            GUILayout.Label("Maze Test Scene Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            includeRooms = EditorGUILayout.Toggle("Include Rooms", includeRooms);
            includeDoors = EditorGUILayout.Toggle("Include Doors", includeDoors);
            includeTorches = EditorGUILayout.Toggle("Include Torches", includeTorches);

            GUILayout.Space(10);
            mazeSize = EditorGUILayout.IntField("Maze Size", mazeSize);
            mazeSize = Mathf.Clamp(mazeSize, 15, 31);

            GUILayout.Space(20);

            if (GUILayout.Button("Create Test GameObject", GUILayout.Height(30)))
            {
                CreateTestGameObject();
            }

            if (GUILayout.Button("Add Missing Components to Selected", GUILayout.Height(30)))
            {
                AddComponentsToSelected();
            }

            GUILayout.Space(20);
            GUILayout.Label("Instructions:", EditorStyles.boldLabel);
            GUILayout.Label("1. Create empty GameObject");
            GUILayout.Label("2. Select it");
            GUILayout.Label("3. Click 'Add Missing Components'");
            GUILayout.Label("4. Configure components in Inspector");
            GUILayout.Label("5. Press Play");

            GUILayout.Space(20);
            GUILayout.Label("Or use the quick setup button to create everything at once.");
        }

        private void CreateTestGameObject()
        {
            var go = new GameObject("MazeTest");
            Undo.RegisterCreatedObjectUndo(go, "Create Maze Test GameObject");

            AddAllComponents(go);

            Selection.activeGameObject = go;
            Debug.Log("[MazeTestSetup] Created MazeTest GameObject with all components!");
        }

        private void AddComponentsToSelected()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select a GameObject first!", "OK");
                return;
            }

            AddAllComponents(selected);
            Debug.Log($"[MazeTestSetup] Added components to {selected.name}!");
        }

        private void AddAllComponents(GameObject go)
        {
            // Core components - using string type names to avoid assembly issues
            EnsureComponentByName(go, "MazeGenerator");
            EnsureComponentByName(go, "MazeRenderer");
            EnsureComponentByName(go, "TorchPool");
            EnsureComponentByName(go, "SpatialPlacer");  // Universal placement PLUG-IN
            EnsureComponentByName(go, "MazeIntegration");
            EnsureComponentByName(go, "MazeTorchTest");

            // Optional components
            if (includeRooms)
            {
                EnsureComponentByName(go, "RoomGenerator");
            }

            if (includeDoors)
            {
                EnsureComponentByName(go, "DoorHolePlacer");
                EnsureComponentByName(go, "RoomDoorPlacer");
            }

            // Configure SpatialPlacer (Universal PLUG-IN)
            var spatialPlacer = GetComponentByName(go, "SpatialPlacer");
            if (spatialPlacer != null)
            {
                var placeTorchesField = spatialPlacer.GetType().GetField("placeTorches");
                var torchCountField = spatialPlacer.GetType().GetField("torchCount");
                var minDistField = spatialPlacer.GetType().GetField("minDistanceBetweenTorches");

                if (placeTorchesField != null) placeTorchesField.SetValue(spatialPlacer, includeTorches);
                if (torchCountField != null) torchCountField.SetValue(spatialPlacer, 15);
                if (minDistField != null) minDistField.SetValue(spatialPlacer, 6f);
            }

            // Configure MazeTorchTest
            var testScript = GetComponentByName(go, "MazeTorchTest");
            if (testScript != null)
            {
                var widthField = testScript.GetType().GetField("mazeWidth");
                var heightField = testScript.GetType().GetField("mazeHeight");
                var torchesField = testScript.GetType().GetField("enableTorches");

                if (widthField != null) widthField.SetValue(testScript, mazeSize);
                if (heightField != null) heightField.SetValue(testScript, mazeSize);
                if (torchesField != null) torchesField.SetValue(testScript, includeTorches);
            }

            // Configure MazeIntegration
            var mazeIntegration = GetComponentByName(go, "MazeIntegration");
            if (mazeIntegration != null)
            {
                var widthField = mazeIntegration.GetType().GetField("mazeWidth");
                var heightField = mazeIntegration.GetType().GetField("mazeHeight");
                var placeTorchesField = mazeIntegration.GetType().GetField("placeTorches");

                if (widthField != null) widthField.SetValue(mazeIntegration, mazeSize);
                if (heightField != null) heightField.SetValue(mazeIntegration, mazeSize);
                if (placeTorchesField != null) placeTorchesField.SetValue(mazeIntegration, includeTorches);
            }

            Debug.Log("[MazeTestSetup] All components configured!");
        }

        private static Component EnsureComponentByName(GameObject go, string typeName)
        {
            var component = go.GetComponent(typeName);
            if (component == null)
            {
                var type = System.Type.GetType($"Code.Lavos.Core.{typeName}, Code.Lavos.Core");
                if (type != null)
                {
                    component = go.AddComponent(type);
                    Debug.Log($"[MazeTestSetup] Added {typeName}");
                }
                else
                {
                    Debug.LogWarning($"[MazeTestSetup] Type {typeName} not found!");
                }
            }
            return component;
        }

        private static Component GetComponentByName(GameObject go, string typeName)
        {
            var type = System.Type.GetType($"Code.Lavos.Core.{typeName}, Code.Lavos.Core");
            return type != null ? go.GetComponent(type) : null;
        }

        [MenuItem("Tools/Maze Test/Quick Setup (Current Scene)")]
        public static void QuickSetup()
        {
            var go = new GameObject("MazeGenerator");
            Undo.RegisterCreatedObjectUndo(go, "Quick Maze Setup");

            // Add components by type name
            string[] componentTypes = new string[]
            {
                "MazeGenerator", "MazeRenderer", "TorchPool", "SpatialPlacer",
                "MazeIntegration", "MazeTorchTest", "RoomGenerator",
                "DoorHolePlacer", "RoomDoorPlacer"
            };

            foreach (var typeName in componentTypes)
            {
                var type = System.Type.GetType($"Code.Lavos.Core.{typeName}, Code.Lavos.Core");
                if (type != null)
                {
                    go.AddComponent(type);
                }
            }

            // Configure components via reflection (default: 21x21 maze, torches enabled)
            ConfigureComponent(go, "MazeGenerator", 21);
            ConfigureComponent(go, "SpatialPlacer", 21, true);
            ConfigureComponent(go, "MazeIntegration", 21, true);
            ConfigureComponent(go, "MazeTorchTest", 21, true, 3.5f);

            Selection.activeGameObject = go;
            Debug.Log("[MazeTestSetup] Quick setup complete! Press Play to test.");
        }

        private static void ConfigureComponent(GameObject go, string typeName, int mazeSize = 21, bool includeTorches = true, float cameraDistance = 3.5f)
        {
            var type = System.Type.GetType($"Code.Lavos.Core.{typeName}, Code.Lavos.Core");
            if (type == null) return;

            var component = go.GetComponent(type);
            if (component == null) return;

            if (typeName == "MazeGenerator")
            {
                var widthField = type.GetField("width");
                var heightField = type.GetField("height");
                if (widthField != null) widthField.SetValue(component, mazeSize);
                if (heightField != null) heightField.SetValue(component, mazeSize);
            }
            else if (typeName == "SpatialPlacer")
            {
                var placeTorchesField = type.GetField("placeTorches");
                var torchCountField = type.GetField("torchCount");
                var minDistField = type.GetField("minDistanceBetweenTorches");
                
                if (placeTorchesField != null) placeTorchesField.SetValue(component, includeTorches);
                if (torchCountField != null) torchCountField.SetValue(component, 15);
                if (minDistField != null) minDistField.SetValue(component, 6f);
            }
            else if (typeName == "MazeIntegration")
            {
                var widthField = type.GetField("mazeWidth");
                var heightField = type.GetField("mazeHeight");
                var placeTorchesField = type.GetField("placeTorches");
                if (widthField != null) widthField.SetValue(component, mazeSize);
                if (heightField != null) heightField.SetValue(component, mazeSize);
                if (placeTorchesField != null) placeTorchesField.SetValue(component, includeTorches);
            }
            else if (typeName == "MazeTorchTest")
            {
                var widthField = type.GetField("mazeWidth");
                var heightField = type.GetField("mazeHeight");
                var placeTorchesField = type.GetField("enableTorches");
                var autoGenField = type.GetField("autoGenerateOnStart");
                var spawnPlayerField = type.GetField("spawnTestPlayer");
                var cameraDistField = type.GetField("cameraDistance");
                
                if (widthField != null) widthField.SetValue(component, mazeSize);
                if (heightField != null) heightField.SetValue(component, mazeSize);
                if (placeTorchesField != null) placeTorchesField.SetValue(component, includeTorches);
                if (autoGenField != null) autoGenField.SetValue(component, true);
                if (spawnPlayerField != null) spawnPlayerField.SetValue(component, spawnPlayer);
                if (cameraDistField != null) cameraDistField.SetValue(component, cameraDistance);
            }
        }
    }
}
#endif
