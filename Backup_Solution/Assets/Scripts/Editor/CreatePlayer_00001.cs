// CreatePlayer.cs
// Editor tool to create player with camera properly configured
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Tools → Create Player
//   2. Player GameObject created with:
//      - PlayerController component
//      - Main Camera as child (positioned at eye height)
//      - Proper tags and layers
//
// NOTE: This is an EDITOR TOOL - use for scene setup only.
// Runtime code should find player via FindFirstObjectByType.

using UnityEngine;
using UnityEditor;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// CreatePlayer - Editor tool to create properly configured player.
    /// Creates Player GameObject with PlayerController and Camera child.
    /// </summary>
    public class CreatePlayer : EditorWindow
    {
        [MenuItem("Tools/Create Player")]
        public static void CreatePlayerObject()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  CREATE PLAYER - Setting up player for testing");
            Debug.Log("═══════════════════════════════════════════");

            // Check if player already exists
            var existingPlayer = Object.FindFirstObjectByType<PlayerController>();
            
            if (existingPlayer != null)
            {
                Debug.Log("⚠️  Player already exists in scene!");
                Debug.Log($"   Found: {existingPlayer.gameObject.name} at {existingPlayer.transform.position}");
                Debug.Log("═══════════════════════════════════════════");
                return;
            }

            // Create player GameObject
            GameObject playerGO = new GameObject("Player");
            playerGO.transform.position = Vector3.zero;
            playerGO.transform.rotation = Quaternion.identity;

            // Add PlayerController component
            PlayerController playerController = playerGO.AddComponent<PlayerController>();
            Debug.Log("✅ Added PlayerController component");

            // Create camera as child
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.transform.SetParent(playerGO.transform);
            
            // Set camera to eye height (1.7m for average adult)
            cameraGO.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            cameraGO.transform.localRotation = Quaternion.identity;

            // Add Camera component
            Camera camera = cameraGO.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.fieldOfView = 60f;

            Debug.Log("✅ Added Main Camera (eye height: 1.7m)");
            Debug.Log("✅ Camera tagged as 'MainCamera'");

            // Set player rotation (face forward - along Z axis)
            playerGO.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            Debug.Log("✅ Player rotation set to (0, 0, 0) - facing forward");

            // Summary
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  ✅ PLAYER CREATED SUCCESSFULLY!");
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log($"  📍 Position: {playerGO.transform.position}");
            Debug.Log($"  🧭 Rotation: {playerGO.transform.rotation.eulerAngles}");
            Debug.Log($"  📷 Camera: (0, 1.7, 0) - Eye height");
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  💡 Press Play to test player controls");
            Debug.Log("  💡 WASD to move, Mouse to look");
            Debug.Log("═══════════════════════════════════════════");
        }

        [MenuItem("Tools/Create Player with Spawn Point")]
        public static void CreatePlayerAtSpawn()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  CREATE PLAYER AT SPAWN POINT");
            Debug.Log("═══════════════════════════════════════════");

            // Find CompleteMazeBuilder to get spawn point
            var mazeBuilder = Object.FindFirstObjectByType<CompleteMazeBuilder>();
            
            if (mazeBuilder == null)
            {
                Debug.LogWarning("⚠️  No CompleteMazeBuilder in scene!");
                Debug.LogWarning("💡 Run: Tools → Generate Maze first");
                Debug.Log("═══════════════════════════════════════════");
                return;
            }

            // Create player at spawn point
            GameObject playerGO = new GameObject("Player");
            playerGO.transform.position = mazeBuilder.transform.position;  // Will be set by mazeBuilder
            playerGO.transform.rotation = Quaternion.identity;

            // Add PlayerController
            PlayerController playerController = playerGO.AddComponent<PlayerController>();
            Debug.Log("✅ Added PlayerController component");

            // Create camera as child
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.transform.SetParent(playerGO.transform);
            cameraGO.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            cameraGO.transform.localRotation = Quaternion.identity;

            // Add Camera component
            Camera camera = cameraGO.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.fieldOfView = 60f;

            Debug.Log("✅ Added Main Camera (eye height: 1.7m)");

            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  ✅ PLAYER CREATED AT SPAWN POINT!");
            Debug.Log("═══════════════════════════════════════════");
        }

        // Editor window
        [MenuItem("Tools/Player Setup Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<CreatePlayer>("Player Setup");
            window.minSize = new Vector2(300, 200);
        }

        private void OnGUI()
        {
            GUILayout.Label("Player Setup", EditorStyles.boldLabel);

            GUILayout.Space(10);

            GUILayout.Label("Create player with properly configured camera:", EditorStyles.wordWrappedMiniLabel);

            GUILayout.Space(10);

            if (GUILayout.Button("Create Player", GUILayout.Height(40)))
            {
                CreatePlayerObject();
            }

            if (GUILayout.Button("Create Player at Spawn Point", GUILayout.Height(40)))
            {
                CreatePlayerAtSpawn();
            }

            GUILayout.Space(20);

            GUILayout.Label("Camera Settings:", EditorStyles.boldLabel);
            GUILayout.Label("  • Eye Height: 1.7m (average adult)");
            GUILayout.Label("  • FOV: 60 degrees");
            GUILayout.Label("  • Near Clip: 0.1m");
            GUILayout.Label("  • Far Clip: 1000m");
        }
    }
}
