// SceneSetupHelper.cs
// Editor tool to verify and auto-setup complete scene
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Usage: Menu > Tools > PeuImporte > Verify/Setup Scene

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Code.Lavos.Core;
using Code.Lavos.HUD;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// Scene setup verification and auto-setup tool.
    /// Ensures all required GameObjects and components are present.
    /// </summary>
    public class SceneSetupHelper : EditorWindow
    {
        private Vector2 _scrollPosition;

        [MenuItem("Tools/PeuImporte/Verify/Setup Scene")]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneSetupHelper>("Scene Setup");
            window.minSize = new Vector2(400, 500);
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Header
            GUILayout.Space(10);
            GUILayout.Label("PeuImporte Scene Setup", EditorStyles.boldLabel);
            GUILayout.Label("Verify and auto-setup all required components", EditorStyles.miniLabel);

            GUILayout.Space(10);

            // Verify button
            if (GUILayout.Button("🔍 Verify Scene Setup", GUILayout.Height(30)))
            {
                VerifySceneSetup();
            }

            // Auto-setup button
            if (GUILayout.Button("⚡ Auto-Setup Missing Components", GUILayout.Height(30)))
            {
                AutoSetupScene();
            }

            GUILayout.Space(15);

            // Status sections
            DrawSectionHeader("🎮 Core Systems");
            DrawStatusItem("GameManager", CheckGameManager(), "Main game state singleton");
            DrawStatusItem("EventHandler", CheckEventHandler(), "Central event hub");
            DrawStatusItem("ItemEngine", CheckItemEngine(), "Item registry manager");

            GUILayout.Space(10);

            DrawSectionHeader("👤 Player System");
            DrawStatusItem("Player GameObject", CheckPlayer(), "Player with CharacterController");
            DrawStatusItem("PlayerController", CheckPlayerController(), "Movement and input");
            DrawStatusItem("PlayerStats", CheckPlayerStats(), "Health/Mana/Stamina");
            DrawStatusItem("Player Camera", CheckPlayerCamera(), "Main camera assigned to player");

            GUILayout.Space(10);

            DrawSectionHeader("🎨 UI System");
            DrawStatusItem("Canvas", CheckCanvas(), "UI Canvas for all HUD elements");
            DrawStatusItem("UIBarsSystem", CheckUIBars(), "Health/Mana/Stamina bars");
            DrawStatusItem("HUDSystem", CheckHUDSystem(), "Main HUD manager");

            GUILayout.Space(10);

            DrawSectionHeader("🏰 Maze System");
            DrawStatusItem("MazeGenerator", CheckMazeGenerator(), "Procedural maze generation");
            DrawStatusItem("RoomGenerator", CheckRoomGenerator(), "Room generation");
            DrawStatusItem("DoorHolePlacer", CheckDoorHolePlacer(), "Door hole placement");
            DrawStatusItem("RoomDoorPlacer", CheckRoomDoorPlacer(), "Door placement in rooms");
            DrawStatusItem("MazeRenderer", CheckMazeRenderer(), "Maze visualization");
            DrawStatusItem("MazeIntegration", CheckMazeIntegration(), "Maze system integration");
            DrawStatusItem("SeedManager", CheckSeedManager(), "Procedural seed management");
            DrawStatusItem("SpawnPlacerEngine", CheckSpawnPlacerEngine(), "Item placement engine");

            GUILayout.Space(10);

            DrawSectionHeader("⚔️ Combat & Items");
            DrawStatusItem("CombatSystem", CheckCombatSystem(), "Damage calculation");
            DrawStatusItem("Inventory", CheckInventory(), "Player inventory");

            GUILayout.Space(20);

            // Quick setup buttons
            GUILayout.Label("Quick Setup", EditorStyles.boldLabel);

            if (GUILayout.Button("📦 Setup Complete Game Scene"))
            {
                SetupCompleteGameScene();
            }

            if (GUILayout.Button("🎮 Setup Player Only"))
            {
                SetupPlayer();
            }

            if (GUILayout.Button("🎨 Setup UI Only"))
            {
                SetupUI();
            }

            if (GUILayout.Button("🏰 Setup Maze System Only"))
            {
                AddDoorSystemToMaze();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Space(5);
            var style = new GUIStyle(EditorStyles.boldLabel) { fontSize = 11 };
            GUILayout.Label(title, style);
        }

        private void DrawStatusItem(string name, bool ok, string description)
        {
            EditorGUILayout.BeginHorizontal();

            // Status icon
            GUILayout.Label(ok ? "✅" : "❌", GUILayout.Width(30));

            // Name
            GUILayout.Label(name, GUILayout.Width(150));

            // Description
            GUILayout.Label(description, EditorStyles.miniLabel);

            // Fix button if missing
            if (!ok)
            {
                if (GUILayout.Button("Fix", GUILayout.Width(50)))
                {
                    FixMissingComponent(name);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        #region Verification Methods

        private bool CheckGameManager() => FindFirstObjectByType<GameManager>() != null;
        private bool CheckEventHandler() => FindFirstObjectByType<EventHandler>() != null;
        private bool CheckItemEngine() => FindFirstObjectByType<ItemEngine>() != null;

        private bool CheckPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            return player != null && player.GetComponent<CharacterController>() != null;
        }

        private bool CheckPlayerController() => FindFirstObjectByType<PlayerController>() != null;
        private bool CheckPlayerStats() => FindFirstObjectByType<PlayerStats>() != null;

        private bool CheckPlayerCamera()
        {
            var camera = Camera.main;
            return camera != null;
        }

        private bool CheckCanvas() => FindFirstObjectByType<Canvas>() != null;
        private bool CheckUIBars() => FindFirstObjectByType<UIBarsSystem>() != null;
        private bool CheckHUDSystem() => FindFirstObjectByType<HUDSystem>() != null;

        private bool CheckMazeGenerator() => FindFirstObjectByType<MazeGenerator>() != null;
        private bool CheckRoomGenerator() => FindFirstObjectByType<RoomGenerator>() != null;
        private bool CheckDoorHolePlacer() => FindFirstObjectByType<DoorHolePlacer>() != null;
        private bool CheckRoomDoorPlacer() => FindFirstObjectByType<RoomDoorPlacer>() != null;
        private bool CheckMazeRenderer() => FindFirstObjectByType<MazeRenderer>() != null;
        private bool CheckMazeIntegration() => FindFirstObjectByType<MazeIntegration>() != null;
        private bool CheckSeedManager() => FindFirstObjectByType<SeedManager>() != null;
        private bool CheckSpawnPlacerEngine() => FindFirstObjectByType<SpawnPlacerEngine>() != null;

        private bool CheckCombatSystem() => FindFirstObjectByType<CombatSystem>() != null;
        private bool CheckInventory() => FindFirstObjectByType<Inventory>() != null;

        #endregion

        #region Fix Methods

        private void FixMissingComponent(string name)
        {
            switch (name)
            {
                case "GameManager":
                    SetupGameManager();
                    break;
                case "EventHandler":
                    SetupEventHandler();
                    break;
                case "ItemEngine":
                    SetupItemEngine();
                    break;
                case "Player GameObject":
                    SetupPlayer();
                    break;
                case "Canvas":
                    SetupCanvas();
                    break;
                case "UIBarsSystem":
                case "HUDSystem":
                case "HUDEngine":
                    SetupUI();
                    break;
                case "MazeGenerator":
                case "RoomGenerator":
                case "DoorHolePlacer":
                case "RoomDoorPlacer":
                case "MazeRenderer":
                case "MazeIntegration":
                case "SeedManager":
                case "SpawnPlacerEngine":
                    AddDoorSystemToMaze();
                    break;
                default:
                    Debug.LogWarning($"[SceneSetupHelper] Auto-fix not implemented for: {name}");
                    break;
            }

            // Refresh verification
            VerifySceneSetup();
        }

        #endregion

        #region Setup Methods

        private void VerifySceneSetup()
        {
            Debug.Log("=============================");
            Debug.Log("🔍 Verifying Scene Setup...");
            Debug.Log("=============================");

            int missing = 0;

            // Core
            LogStatus("GameManager", CheckGameManager(), ref missing);
            LogStatus("EventHandler", CheckEventHandler(), ref missing);
            LogStatus("ItemEngine", CheckItemEngine(), ref missing);

            // Player
            LogStatus("Player GameObject", CheckPlayer(), ref missing);
            LogStatus("PlayerController", CheckPlayerController(), ref missing);
            LogStatus("PlayerStats", CheckPlayerStats(), ref missing);
            LogStatus("Player Camera", CheckPlayerCamera(), ref missing);

            // UI
            LogStatus("Canvas", CheckCanvas(), ref missing);
            LogStatus("UIBarsSystem", CheckUIBars(), ref missing);
            LogStatus("HUDSystem", CheckHUDSystem(), ref missing);

            // Note: HUDEngine is an alternative to HUDSystem, not required

            // Maze
            LogStatus("MazeGenerator", CheckMazeGenerator(), ref missing);
            LogStatus("RoomGenerator", CheckRoomGenerator(), ref missing);
            LogStatus("DoorHolePlacer", CheckDoorHolePlacer(), ref missing);
            LogStatus("RoomDoorPlacer", CheckRoomDoorPlacer(), ref missing);
            LogStatus("MazeRenderer", CheckMazeRenderer(), ref missing);
            LogStatus("MazeIntegration", CheckMazeIntegration(), ref missing);
            LogStatus("SeedManager", CheckSeedManager(), ref missing);
            LogStatus("SpawnPlacerEngine", CheckSpawnPlacerEngine(), ref missing);

            // Combat
            LogStatus("CombatSystem", CheckCombatSystem(), ref missing);
            LogStatus("Inventory", CheckInventory(), ref missing);

            Debug.Log("=============================");
            if (missing == 0)
            {
                Debug.Log("✅ All systems verified! Scene is ready.");
            }
            else
            {
                Debug.LogWarning($"⚠️ {missing} component(s) missing. Use Auto-Setup to fix.");
            }
            Debug.Log("=============================");
        }

        private void AutoSetupScene()
        {
            Debug.Log("=============================");
            Debug.Log("⚡ Auto-Setting Up Scene...");
            Debug.Log("=============================");

            SetupCoreSystems();
            SetupPlayer();
            SetupUI();
            AddDoorSystemToMaze();

            Debug.Log("=============================");
            Debug.Log("✅ Auto-Setup Complete!");
            Debug.Log("=============================");
            Debug.Log("▶️ Press Play to test!");
        }

        private void SetupCoreSystems()
        {
            SetupGameManager();
            SetupEventHandler();
            SetupItemEngine();
        }

        private void SetupGameManager()
        {
            if (CheckGameManager())
            {
                Debug.Log("✅ GameManager already exists");
                return;
            }

            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            Debug.Log("✅ Created GameManager");
        }

        private void SetupEventHandler()
        {
            if (CheckEventHandler())
            {
                Debug.Log("✅ EventHandler already exists");
                return;
            }

            var go = new GameObject("EventHandler");
            go.AddComponent<EventHandler>();
            Debug.Log("✅ Created EventHandler");
        }

        private void SetupItemEngine()
        {
            if (CheckItemEngine())
            {
                Debug.Log("✅ ItemEngine already exists");
                return;
            }

            var go = new GameObject("ItemEngine");
            go.AddComponent<ItemEngine>();
            Debug.Log("✅ Created ItemEngine");
        }

        private void SetupPlayer()
        {
            if (CheckPlayer())
            {
                Debug.Log("✅ Player already exists");
                return;
            }

            // Create Player GameObject
            var player = new GameObject("Player");
            player.tag = "Player";

            // Add CharacterController
            var controller = player.AddComponent<CharacterController>();
            controller.skinWidth = 0.08f;
            controller.minMoveDistance = 0.001f;

            // Add PlayerController
            player.AddComponent<PlayerController>();

            // Add PlayerStats
            player.AddComponent<PlayerStats>();

            // Add Inventory
            player.AddComponent<Inventory>();

            // Add CombatSystem
            player.AddComponent<CombatSystem>();

            // Create and assign camera
            var camera = new GameObject("PlayerCamera");
            camera.AddComponent<Camera>();
            camera.transform.SetParent(player.transform);
            camera.transform.localPosition = new Vector3(0f, 0.75f, 0f);

            Debug.Log("✅ Created Player with all components");
        }

        private void SetupUI()
        {
            SetupCanvas();
            SetupHUDSystems();
        }

        private void SetupCanvas()
        {
            if (CheckCanvas())
            {
                Debug.Log("✅ Canvas already exists");
                return;
            }

            var canvas = new GameObject("Canvas");
            var canvasComp = canvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();

            Debug.Log("✅ Created Canvas");
        }

        private void SetupHUDSystems()
        {
            // Find or create Canvas
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                SetupCanvas();
                canvas = FindFirstObjectByType<Canvas>();
            }

            // Add UIBarsSystem (required for health/mana/stamina bars)
            if (!CheckUIBars())
            {
                var uiBars = new GameObject("UIBarsSystem");
                uiBars.AddComponent<UIBarsSystem>();
                Debug.Log("✅ Added UIBarsSystem");
            }
            else
            {
                Debug.Log("✅ UIBarsSystem already exists");
            }

            // Add HUDSystem (main HUD manager - status effects, hotbar, dialogs)
            if (!CheckHUDSystem())
            {
                var hudSystem = new GameObject("HUDSystem");
                hudSystem.AddComponent<HUDSystem>();
                Debug.Log("✅ Added HUDSystem");
            }
            else
            {
                Debug.Log("✅ HUDSystem already exists");
            }

            // Note: HUDEngine is an alternative modular HUD system
            // Only use if you prefer modular architecture over HUDSystem
            // Not added by default to avoid redundancy
        }

        private void SetupCompleteGameScene()
        {
            Debug.Log("=============================");
            Debug.Log("📦 Setting Up Complete Game Scene...");
            Debug.Log("=============================");

            SetupCoreSystems();
            SetupPlayer();
            SetupUI();
            AddDoorSystemToMaze();

            Debug.Log("=============================");
            Debug.Log("✅ Complete Game Scene Ready!");
            Debug.Log("=============================");
            Debug.Log("▶️ Press Play to test!");
        }

        private void AddDoorSystemToMaze()
        {
            // Find or create Maze
            var mazeGen = FindFirstObjectByType<MazeGenerator>();
            GameObject maze = null;

            if (mazeGen != null)
            {
                maze = mazeGen.gameObject;
                Debug.Log("✅ Found existing Maze");
            }
            else
            {
                maze = new GameObject("Maze");
                maze.AddComponent<MazeGenerator>();
                Debug.Log("✅ Created Maze GameObject");
            }

            // Add missing components
            AddComponentIfMissing<RoomGenerator>(maze);
            AddComponentIfMissing<DoorHolePlacer>(maze);
            AddComponentIfMissing<RoomDoorPlacer>(maze);
            AddComponentIfMissing<MazeRenderer>(maze);
            AddComponentIfMissing<MazeIntegration>(maze);
            AddComponentIfMissing<SeedManager>(maze);
            AddComponentIfMissing<DrawingPool>(maze);
            AddComponentIfMissing<SpawnPlacerEngine>(maze);

            Selection.activeGameObject = maze;
        }

        private void AddComponentIfMissing<T>(GameObject go) where T : Component
        {
            if (go.GetComponent<T>() == null)
            {
                go.AddComponent<T>();
                Debug.Log($"✅ Added {typeof(T).Name}");
            }
            else
            {
                Debug.Log($"✅ {typeof(T).Name} already exists");
            }
        }

        private void LogStatus(string name, bool ok, ref int missing)
        {
            if (ok)
            {
                Debug.Log($"✅ {name}");
            }
            else
            {
                Debug.LogWarning($"❌ {name} - MISSING");
                missing++;
            }
        }

        #endregion
    }
}
#endif
