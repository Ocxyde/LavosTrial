﻿﻿﻿﻿﻿﻿﻿// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// SetupMazeSceneV2.cs - Automated scene setup for A-Maze-Lav8s_2.0.0
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
// 1. Open Unity Editor
// 2. Go to Tools > Setup Maze Scene v2.0.0
// 3. Scene will be created automatically with all components
//

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using Code.Lavos.Core;
using Code.Lavos.HUD;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// Automated scene setup tool for A-Maze-Lav8s_2.0.0
    /// Creates complete maze scene with all required components.
    /// </summary>
    public static class SetupMazeSceneV2
    {
        private const string SCENE_NAME = "A-Maze-Lav8s_2.0.0";
        private const string SCENE_PATH = "Assets/Scenes/" + SCENE_NAME + ".unity";

        [MenuItem("Tools/Setup Maze Scene V2.0.0")]
        public static void SetupMazeScene()
        {
            // Check if scene already exists
            if (AssetDatabase.AssetPathToGUID(SCENE_PATH) != "")
            {
                if (!EditorUtility.DisplayDialog("Scene Exists",
                    $"Scene '{SCENE_NAME}' already exists. Do you want to overwrite it?",
                    "Yes", "No"))
                {
                    return;
                }
            }

            // Create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

            Debug.Log($"[SetupMazeSceneV2] Creating scene: {SCENE_NAME}");

            // Create all required GameObjects
            CreateGameConfig();
            CreateEventHandler();
            CreateGameManager();
            CreateMazeGenerator();
            CreateLightEngine();
            CreateTorchPool();
            CreateSpatialPlacer();
            CreateLightPlacementEngine();
            CreateDoorsEngine();
            CreatePlayer();
            CreateLighting();
            CreateUI();

            // Save scene
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AssetDatabase.Refresh();

            Debug.Log($"[SetupMazeSceneV2] Scene '{SCENE_NAME}' created successfully at {SCENE_PATH}");
            Debug.Log("[SetupMazeSceneV2] Press Play to generate maze!");
        }

        private static void CreateGameConfig()
        {
            var go = new GameObject("GameConfig");
            var config = go.AddComponent<GameConfig>();

            // Configure maze settings
            config.CellSize = 6f;
            config.WallHeight = 4f;
            config.WallThickness = 0.2f;
            config.PlayerEyeHeight = 1.7f;
            config.PlayerSpawnOffset = 0.5f;

            // Maze generation config
            config.MazeCfg.BaseSize = 12;
            config.MazeCfg.MinSize = 12;
            config.MazeCfg.MaxSize = 51;
            config.MazeCfg.SpawnRoomSize = 5;
            config.MazeCfg.TorchChance = 0.30f;
            config.MazeCfg.ChestDensity = 0.03f;
            config.MazeCfg.EnemyDensity = 0.05f;
            config.MazeCfg.DeadEndDensity = 0.10f;
            config.MazeCfg.BaseWallPenalty = 100;
            config.MazeCfg.UseCorridorFlowSystem = false;  // Use GridMazeGenerator

            // Difficulty scaling
            config.DifficultyCfg.MaxLevel = 39;
            config.DifficultyCfg.MaxFactor = 1.75f;
            config.DifficultyCfg.Exponent = 3.09f;
            config.DifficultyCfg.SizeRamp = 1.12f;
            config.DifficultyCfg.TorchMaxMult = 1.5f;
            config.DifficultyCfg.DeadEndMaxMult = 8.01f;

            config.ShareSalt = "LAVOS_SECRET_SALT_2026";

            Debug.Log("[SetupMazeSceneV2] Created GameConfig");
        }

        private static void CreateMazeGenerator()
        {
            var go = new GameObject("MazeGenerator");
            var builder = go.AddComponent<CompleteMazeBuilder8>();

            // Configure seed system
            // useFixedSeed: false = random seed each run
            // useFixedSeed: true = use fixedSeed value
            // Access in Inspector: Set useFixedSeed = true, fixedSeed = 42857 for reproducible mazes

            // Generator options
            builder.UseGuaranteedPathGenerator = false;  // Use GridMazeGenerator with winding path
            builder.UsePassageFirstGenerator = false;

            // Assign prefabs from Resources (Plug-in-Out compliant)
            builder.wallPrefab = Resources.Load<GameObject>("Prefabs/WallPrefab");
            builder.doorPrefab = Resources.Load<GameObject>("Prefabs/DoorPrefab");
            builder.torchPrefab = Resources.Load<GameObject>("Prefabs/TorchHandlePrefab");
            builder.chestPrefab = Resources.Load<GameObject>("Prefabs/ChestPrefab");
            builder.enemyPrefab = Resources.Load<GameObject>("Prefabs/EnemyPrefab");
            builder.floorPrefab = Resources.Load<GameObject>("Prefabs/FloorTilePrefab");
            builder.playerPrefab = Resources.Load<GameObject>("Prefabs/Player");

            // Assign wall material from GameConfig or Resources
            if (Application.isPlaying && GameConfig.Instance != null)
            {
                builder.wallMaterial = Resources.Load<Material>(GameConfig.Instance.WallMaterial);
            }
            builder.wallMaterial ??= Resources.Load<Material>("Materials/WallMaterial");

            // Validate assignments
            if (builder.wallPrefab == null)
                Debug.LogError("[SetupMazeSceneV2] WallPrefab not found in Resources!");
            if (builder.playerPrefab == null)
                Debug.LogWarning("[SetupMazeSceneV2] Player prefab not found - will spawn from code");

            Debug.Log("[SetupMazeSceneV2] Created MazeGenerator (CompleteMazeBuilder8) with prefabs assigned");
        }

        private static void CreatePlayer()
        {
            // NOTE: Player is NOT created in scene initially.
            // CompleteMazeBuilder8 spawns player AFTER maze generation.
            // This follows Plug-in-Out architecture - components find each other.
            //
            // To manually create player for testing:
            // 1. Use Tools > Create Player menu
            // 2. Or add Player prefab via CompleteMazeBuilder8 inspector
            //
            // Player will be spawned at:
            // - Position: SpawnPoint cell from maze generation
            // - Y: 1.7m (eye height)
            // - After all maze geometry is placed

            Debug.Log("[SetupMazeSceneV2] Player will be spawned by CompleteMazeBuilder8 at runtime");
            Debug.Log("[SetupMazeSceneV2] To manually create player: Tools > Create Player");
        }

        private static void CreateLighting()
        {
            // Directional Light (sun)
            var sunObj = new GameObject("Directional Light");
            var light = sunObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.8f);
            light.intensity = 1f;
            light.shadows = LightShadows.Soft;
            sunObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Add URP additional light data if using URP (optional)
            // This is commented out - URP will auto-configure if needed
            // var urpLightData = sunObj.AddComponent<UniversalAdditionalLightData>();

            Debug.Log("[SetupMazeSceneV2] Created Directional Light");
        }

        private static void CreateUI()
        {
            // Create Canvas for UI
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            // Add CanvasScaler for resolution independence
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Add GraphicRaycaster
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create EventSystem with New Input System (if available)
            var eventSystemObj = new GameObject("EventSystem");
            var eventSystem = eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            
            // Try to use Input System package module first, fallback to standalone
            var inputSystemType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, UnityEngine.InputSystem.UI");
            if (inputSystemType != null)
            {
                eventSystemObj.AddComponent(inputSystemType);
                Debug.Log("[SetupMazeSceneV2] Using New Input System module");
            }
            else
            {
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[SetupMazeSceneV2] Using Standalone Input Module (fallback)");
            }

            // Add UIBarsSystem for HUD (health, mana, stamina)
            var barsObj = new GameObject("UIBarsSystem");
            barsObj.transform.SetParent(canvasObj.transform, false);
            barsObj.AddComponent<UIBarsSystem>();

            Debug.Log("[SetupMazeSceneV2] Created Canvas, EventSystem, and UIBarsSystem");
        }

        private static void CreateEventHandler()
        {
            var go = new GameObject("EventHandler");
            go.AddComponent<EventHandler>();

            // EventHandler is singleton - persists across scenes
            // Note: DontDestroyOnLoad only works in play mode, not editor scripts
            // The EventHandler component will handle persistence at runtime

            Debug.Log("[SetupMazeSceneV2] Created EventHandler");
        }

        private static void CreateGameManager()
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();

            Debug.Log("[SetupMazeSceneV2] Created GameManager");
        }

        private static void CreateLightEngine()
        {
            var go = new GameObject("LightEngine");
            go.AddComponent<LightEngine>();

            Debug.Log("[SetupMazeSceneV2] Created LightEngine");
        }

        private static void CreateTorchPool()
        {
            var go = new GameObject("TorchPool");
            go.AddComponent<TorchPool>();

            Debug.Log("[SetupMazeSceneV2] Created TorchPool");
        }

        private static void CreateSpatialPlacer()
        {
            var go = new GameObject("SpatialPlacer");
            go.AddComponent<SpatialPlacer>();

            Debug.Log("[SetupMazeSceneV2] Created SpatialPlacer");
        }

        private static void CreateLightPlacementEngine()
        {
            var go = new GameObject("LightPlacementEngine");
            go.AddComponent<LightPlacementEngine>();

            Debug.Log("[SetupMazeSceneV2] Created LightPlacementEngine");
        }

        private static void CreateDoorsEngine()
        {
            var go = new GameObject("DoorsEngine");
            go.AddComponent<DoorsEngine>();

            Debug.Log("[SetupMazeSceneV2] Created DoorsEngine");
        }

        [MenuItem("Tools/Setup Maze Scene V2.0.0", true)]
        public static bool ValidateSetupMazeScene()
        {
            // Always allow menu item
            return true;
        }
    }
}
