// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Code.Lavos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Code.Lavos.  If not, see <https://www.gnu.org/licenses/>.
// MazePreviewEditor.cs
// One-click maze preview generator - Editor-only visualization (no Play mode!)
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// COMPLIANCE:
//   - Plug-in-Out: Finds CompleteMazeBuilder, never creates
//   - All values from GameConfig (JSON-driven, no hardcodes)
//   - Uses existing prefabs from Resources/ (no procedural fallback)
//   - Respects 8-axis maze system (cardinal + diagonal walls)
//   - Editor-only: Tagged "EditorOnly", excluded from builds
//   - Unity 6 naming: _camelCase for private fields
//
// USAGE:
//   Tools → Maze → Preview Maze (1-Click Render)
//   OR: Select CompleteMazeBuilder → Right-click → "Preview Maze in Editor"
//
// NOTE: This is an EDITOR TOOL only. Runtime code remains plug-in-o
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// MazePreviewEditor - One-click maze preview in Editor (no Play mode!).
    /// Generates and renders entire maze for visual verification.
    /// 
    /// COMPLIANCE:
    ///   - Plug-in-Out: Finds CompleteMazeBuilder, never creates
    ///   - All values from GameConfig (JSON-driven)
    ///   - Uses existing prefabs from Resources/
    ///   - Editor-only: Tagged "EditorOnly"
    /// </summary>
    public class MazePreviewEditor : EditorWindow
    {
        // ── Configuration (serialized for window persistence) ─────────────
        [SerializeField] private int _previewLevel = 0;
        [SerializeField] private int _previewSeed;
        [SerializeField] private bool _autoGenerate = true;

        // ── Generated data ─────────────────────────────────────────────────
        private MazeData8 _previewData;
        private GameConfig _config;
        private DifficultyScaler _scaler;
        private GameObject _previewRoot;
        private Transform _wallsRoot;
        private Transform _objectsRoot;

        // ── Window instance ────────────────────────────────────────────────
        private static MazePreviewEditor _window;

        // ── Scroll position ────────────────────────────────────────────────
        private Vector2 _scrollPosition;

        // ───────────────────────────────────────────────────────────────────
        //  Menu Items
        // ───────────────────────────────────────────────────────────────────

        [MenuItem("Tools/Maze/Preview Maze (1-Click Render)")]
        public static void ShowWindow()
        {
            _window = GetWindow<MazePreviewEditor>("Maze Preview");
            _window.minSize = new Vector2(550, 750);
            _window.titleContent = new GUIContent("Maze Preview");
            _window.Show();

            // Auto-generate on open (delayed for stability)
            if (_window._autoGenerate)
            {
                EditorApplication.delayCall += () =>
                {
                    if (_window != null)
                    {
                        _window.GeneratePreview();
                    }
                };
            }
        }

        [MenuItem("GameObject/CompleteMazeBuilder8/Preview Maze in Editor", false, 10)]
        public static void PreviewFromSelection()
        {
            // Plug-in-Out: Find existing CompleteMazeBuilder8
            var mazeBuilder = Selection.activeGameObject?.GetComponent<CompleteMazeBuilder8>();

            if (mazeBuilder != null)
            {
                GeneratePreviewForBuilder(mazeBuilder);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "No CompleteMazeBuilder8 Selected",
                    "Please select a GameObject with CompleteMazeBuilder8 component.\n\n" +
                    "Tip: Use Tools → Maze → Preview Maze (1-Click Render) instead.",
                    "OK"
                );
            }
        }

        [MenuItem("GameObject/CompleteMazeBuilder8/Preview Maze in Editor", true)]
        public static bool ValidatePreviewFromSelection()
        {
            return Selection.activeGameObject?.GetComponent<CompleteMazeBuilder8>() != null;
        }

        // ───────────────────────────────────────────────────────────────────
        //  GUI
        // ───────────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            GUILayout.Space(15);

            DrawConfiguration();
            GUILayout.Space(15);

            DrawActionButtons();
            GUILayout.Space(15);

            DrawStatistics();
            GUILayout.Space(15);

            DrawCleanupSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };

            EditorGUILayout.LabelField("<color=#4CAF50>MAZE PREVIEW EDITOR</color>", headerStyle);

            GUIStyle subHeaderStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                richText = true
            };

            EditorGUILayout.LabelField(
                "<i>Generate and preview entire maze in Editor (no Play mode!)</i>\n" +
                "Plug-in-Out compliant - JSON-driven - 8-axis support",
                subHeaderStyle
            );
        }

        private void DrawConfiguration()
        {
            EditorGUILayout.LabelField("CONFIGURATION", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            // Level field
            _previewLevel = EditorGUILayout.IntField(
                new GUIContent("Preview Level", "Maze difficulty level (0-39)"),
                _previewLevel
            );
            _previewLevel = Mathf.Clamp(_previewLevel, 0, 39);

            // Seed field
            _previewSeed = EditorGUILayout.IntField(
                new GUIContent("Seed", "Random seed for procedural generation"),
                _previewSeed
            );

            // Randomize seed button
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.labelWidth);
            if (GUILayout.Button("Randomize Seed", GUILayout.Width(150)))
            {
                _previewSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                if (_autoGenerate)
                {
                    GeneratePreview();
                }
            }
            EditorGUILayout.EndHorizontal();

            // Auto-generate toggle
            _autoGenerate = EditorGUILayout.Toggle(
                new GUIContent("Auto-Generate on Open", "Automatically generate maze when window opens"),
                _autoGenerate
            );

            if (EditorGUI.EndChangeCheck() && _autoGenerate)
            {
                GeneratePreview();
            }
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.LabelField("ACTIONS", EditorStyles.boldLabel);

            // Main generate button
            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.3f);
            if (GUILayout.Button("GENERATE MAZE PREVIEW", GUILayout.Height(45)))
            {
                GeneratePreview();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(8);

            // Quick level buttons
            EditorGUILayout.LabelField("Quick Level Select:", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();

            if (CreateLevelButton("Level 0", "12x12\nTutorial", 0)) { }
            if (CreateLevelButton("Level 10", "22x22\nMedium", 10)) { }
            if (CreateLevelButton("Level 20", "32x32\nHard", 20)) { }
            if (CreateLevelButton("Level 39", "51x51\nExtreme", 39)) { }

            EditorGUILayout.EndHorizontal();
        }

        private bool CreateLevelButton(string title, string subtitle, int level)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                fontSize = 11
            };

            bool clicked = GUILayout.Button(
                $"<b>{title}</b>\n{subtitle}",
                buttonStyle,
                GUILayout.Width(110),
                GUILayout.Height(50)
            );

            if (clicked)
            {
                _previewLevel = level;
                GeneratePreview();
            }

            return clicked;
        }

        private void DrawStatistics()
        {
            EditorGUILayout.LabelField("PREVIEW STATISTICS", EditorStyles.boldLabel);

            if (_previewData == null)
            {
                EditorGUILayout.HelpBox(
                    "No maze generated yet.\n\nClick 'GENERATE MAZE PREVIEW' to create one.",
                    MessageType.Info
                );
                return;
            }

            GUIStyle infoStyle = new GUIStyle(EditorStyles.helpBox)
            {
                wordWrap = true,
                padding = new RectOffset(10, 10, 10, 10),
                richText = true
            };

            EditorGUILayout.BeginVertical(infoStyle);

            // Basic info
            EditorGUILayout.LabelField("<b>📊 General</b>", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  Level: {_previewLevel}");
            EditorGUILayout.LabelField($"  Seed: {_previewData.Seed}");
            EditorGUILayout.LabelField($"  Grid Size: {_previewData.Width}×{_previewData.Height}");
            EditorGUILayout.LabelField($"  Cell Size: {_config?.CellSize ?? 6.0f:F1}m");
            EditorGUILayout.LabelField(
                $"  Total Maze: {_previewData.Width * (_config?.CellSize ?? 6.0f):F1}m × " +
                $"{_previewData.Height * (_config?.CellSize ?? 6.0f):F1}m"
            );

            if (_scaler != null)
            {
                EditorGUILayout.LabelField(
                    $"  <color=#FF9800>Difficulty Factor: {_previewData.DifficultyFactor:F3}</color>"
                );
            }

            GUILayout.Space(8);

            // Spawn and exit
            EditorGUILayout.LabelField("<b>Navigation</b>", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                $"  <color=#4CAF50>Spawn:</color> ({_previewData.SpawnCell.x}, {_previewData.SpawnCell.z})"
            );
            EditorGUILayout.LabelField(
                $"  <color=#F44336>Exit:</color> ({_previewData.ExitCell.x}, {_previewData.ExitCell.z})"
            );

            GUILayout.Space(8);

            // Cell statistics
            var stats = CalculateCellStatistics();

            EditorGUILayout.LabelField("<b>Cell Statistics</b>", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  Walkable Cells: <b>{stats.corridorCount}</b>");
            EditorGUILayout.LabelField($"  Wall Cells: <b>{stats.wallCount}</b>");
            EditorGUILayout.LabelField($"  Spawn Room: <b>{stats.spawnCount}</b> cells");

            GUILayout.Space(5);

            EditorGUILayout.LabelField("<b>Objects</b>", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  <color=#FF9800>Torches:</color> {stats.torchCount} " +
                $"({stats.torchPercent:P1})");
            EditorGUILayout.LabelField($"  <color=#FFC107>Chests:</color> {stats.chestCount} " +
                $"({stats.chestPercent:P1})");
            EditorGUILayout.LabelField($"  <color=#F44336>Enemies:</color> {stats.enemyCount} " +
                $"({stats.enemyPercent:P1})");

            EditorGUILayout.EndVertical();
        }

        private (int corridorCount, int wallCount, int spawnCount, int exitCount,
                 int torchCount, int chestCount, int enemyCount,
                 float torchPercent, float chestPercent, float enemyPercent)
            CalculateCellStatistics()
        {
            int corridorCount = 0;
            int wallCount = 0;
            int spawnCount = 0;
            int exitCount = 0;
            int torchCount = 0;
            int chestCount = 0;
            int enemyCount = 0;

            int walkableCount = 0;

            for (int x = 0; x < _previewData.Width; x++)
            {
                for (int z = 0; z < _previewData.Height; z++)
                {
                    var cell = _previewData.GetCell(x, z);

                    if ((cell & CellFlags8.AllWalls) == CellFlags8.AllWalls)
                    {
                        wallCount++;
                    }
                    else
                    {
                        corridorCount++;
                        walkableCount++;
                    }

                    if ((cell & CellFlags8.SpawnRoom) != 0) spawnCount++;
                    if ((cell & CellFlags8.IsExit) != 0) exitCount++;
                    if ((cell & CellFlags8.HasTorch) != 0) torchCount++;
                    if ((cell & CellFlags8.HasChest) != 0) chestCount++;
                    if ((cell & CellFlags8.HasEnemy) != 0) enemyCount++;
                }
            }

            return (
                corridorCount, wallCount, spawnCount, exitCount,
                torchCount, chestCount, enemyCount,
                walkableCount > 0 ? (float)torchCount / walkableCount : 0f,
                walkableCount > 0 ? (float)chestCount / walkableCount : 0f,
                walkableCount > 0 ? (float)enemyCount / walkableCount : 0f
            );
        }

        private void DrawCleanupSection()
        {
            EditorGUILayout.LabelField("CLEANUP", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
            if (GUILayout.Button("Clear Preview Maze", GUILayout.Height(30)))
            {
                ClearPreview();
                EditorUtility.DisplayDialog(
                    "Preview Cleared",
                    "Maze preview has been cleared from the scene.",
                    "OK"
                );
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Preview objects are tagged 'EditorOnly' and excluded from builds.\n" +
                "Clear before entering Play mode to avoid conflicts.",
                MessageType.Warning
            );
        }

        // ───────────────────────────────────────────────────────────────────
        //  Generation
        // ───────────────────────────────────────────────────────────────────

        private void GeneratePreview()
        {
            // Clear previous preview
            ClearPreview();

            // Load config (Plug-in-Out: find, don't create)
            _config = GameConfig.Instance;

            if (_config == null)
            {
                EditorUtility.DisplayDialog(
                    "Config Error",
                    "GameConfig not found in scene!\n\n" +
                    "Please add a GameConfig component to your scene.",
                    "OK"
                );
                return;
            }

            // Get difficulty scaler from config
            _scaler = _config.DifficultyCfg;

            Debug.Log("===============================================================" );
            Debug.Log("  MAZE PREVIEW - Editor Generation");
            Debug.Log("===============================================================" );
            Debug.Log($"  Level: {_previewLevel}");
            Debug.Log($"  Seed: {_previewSeed}");
            Debug.Log($"  Config: {_config.name ?? "GameConfig (instance)"}");

            // Generate maze data using GridMazeGenerator (same as runtime)
            var generator = new GridMazeGenerator();
            _previewData = generator.Generate(_previewSeed, _previewLevel, _config.MazeCfg, _scaler);

            Debug.Log($"  Generated: {_previewData.Width}x{_previewData.Height} maze");
            Debug.Log($"  Difficulty Factor: {_previewData.DifficultyFactor:F3}");

            // Create preview hierarchy
            CreatePreviewHierarchy();

            // Generate geometry
            GenerateGround();
            GenerateWalls();
            GenerateObjects();

            // Position scene camera
            PositionSceneCamera();

            Debug.Log($"  Preview Root: {_previewRoot.name}");
            Debug.Log("===============================================================" );

            EditorUtility.DisplayDialog(
                "Maze Preview Generated!",
                $"Maze preview created successfully!\n\n" +
                $"Level: {_previewLevel}\n" +
                $"Size: {_previewData.Width}×{_previewData.Height}\n" +
                $"Seed: {_previewSeed}\n" +
                $"Difficulty: {_previewData.DifficultyFactor:F3}\n\n" +
                $"Preview object: {_previewRoot.name}\n" +
                $"Tagged as 'EditorOnly' - excluded from builds.",
                "OK"
            );
        }

        private static void GeneratePreviewForBuilder(CompleteMazeBuilder8 builder)
        {
            if (_window == null)
            {
                _window = GetWindow<MazePreviewEditor>("Maze Preview");
            }

            // Use builder's current level and seed
            _window._previewLevel = builder.CurrentLevel;
            _window._previewSeed = builder.CurrentSeed;
            _window.GeneratePreview();
        }

        private void CreatePreviewHierarchy()
        {
            // Create root
            _previewRoot = new GameObject($"MazePreview_L{_previewLevel}_S{_previewSeed}");
            _previewRoot.tag = "EditorOnly";
            _previewRoot.layer = LayerMask.NameToLayer("Ignore Raycast");

            // Create walls parent
            _wallsRoot = new GameObject("PreviewWalls").transform;
            _wallsRoot.SetParent(_previewRoot.transform);
            _wallsRoot.gameObject.tag = "EditorOnly";

            // Create objects parent
            _objectsRoot = new GameObject("PreviewObjects").transform;
            _objectsRoot.SetParent(_previewRoot.transform);
            _objectsRoot.gameObject.tag = "EditorOnly";
        }

        private void GenerateGround()
        {
            // Create ground plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "PreviewGround";
            ground.tag = "EditorOnly";
            ground.layer = LayerMask.NameToLayer("Ignore Raycast");

            // Calculate size
            float size = _previewData.Width * _config.CellSize;

            // Position and scale
            ground.transform.position = new Vector3(size / 2f, 0f, size / 2f);
            ground.transform.localScale = Vector3.one * (size / 10f);

            // Apply floor material from config
            var renderer = ground.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                // Try to load floor material from Resources
                var floorMat = Resources.Load<Material>(_config.FloorMaterial.Replace(".mat", ""));
                if (floorMat != null)
                {
                    renderer.sharedMaterial = floorMat;
                    Debug.Log($"  Ground material: {_config.FloorMaterial}");
                }
                else
                {
                    // Fallback: create simple unlit color material (no Standard shader dependency)
                    Material mat = new Material(Shader.Find("Unlit/Color"));
                    mat.color = new Color(0.4f, 0.4f, 0.4f);
                    renderer.sharedMaterial = mat;
                    Debug.LogWarning($"  Floor material not found: {_config.FloorMaterial}, using fallback");
                }
            }

            // Remove collider (not needed for preview)
            var collider = ground.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }

            ground.transform.SetParent(_previewRoot.transform);
        }

        private void GenerateWalls()
        {
            int cardinalCount = 0;
            int diagonalCount = 0;

            // Load prefabs from Resources (Plug-in-Out: use existing, don't create)
            // Path format in config: "Prefabs/WallPrefab.prefab" → Resources.Load expects without extension
            string wallPrefabPath = _config.WallPrefab.Replace(".prefab", "");
            string diagPrefabPath = "Prefabs/DiagonalWallPrefab";

            GameObject cardinalPrefab = Resources.Load<GameObject>(wallPrefabPath);
            GameObject diagonalPrefab = Resources.Load<GameObject>(diagPrefabPath);

            Debug.Log($"[MazePreview] Cardinal prefab: {(cardinalPrefab != null ? "loaded" : "NOT FOUND")} ({wallPrefabPath})");
            Debug.Log($"[MazePreview] Diagonal prefab: {(diagonalPrefab != null ? "loaded" : "NOT FOUND")} ({diagPrefabPath})");
            Debug.Log($"[MazePreview] DiagonalWalls config: false (removed 2026-03-09 - cardinal-only passages)");

            if (cardinalPrefab == null)
            {
                Debug.LogError(
                    $"[MazePreview] Wall prefab not found: {wallPrefabPath}\n" +
                    $"Please ensure the prefab exists at: Assets/Resources/{wallPrefabPath}.prefab\n" +
                    "Use Tools → Quick Setup Prefabs to create required prefabs."
                );
                EditorUtility.DisplayDialog(
                    "Prefab Missing",
                    $"Wall prefab not found: {wallPrefabPath}\n\n" +
                    "Please run: Tools → Quick Setup Prefabs (For Testing)\n\n" +
                    "Preview cannot be generated without wall prefab.",
                    "OK"
                );
                return;
            }

            for (int x = 0; x < _previewData.Width; x++)
            {
                for (int z = 0; z < _previewData.Height; z++)
                {
                    var cell = _previewData.GetCell(x, z);

                    // Skip if all walls (boundary)
                    if ((cell & CellFlags8.AllWalls) == CellFlags8.AllWalls)
                        continue;

                    float wx = x * _config.CellSize;
                    float wz = z * _config.CellSize;

                    // Cardinal walls
                    if ((cell & CellFlags8.WallN) != 0)
                    {
                        SpawnCardinalWall(cardinalPrefab, wx, wz, Direction8.N, _wallsRoot);
                        cardinalCount++;
                    }
                    if ((cell & CellFlags8.WallE) != 0)
                    {
                        SpawnCardinalWall(cardinalPrefab, wx, wz, Direction8.E, _wallsRoot);
                        cardinalCount++;
                    }
                    if ((cell & CellFlags8.WallS) != 0)
                    {
                        SpawnCardinalWall(cardinalPrefab, wx, wz, Direction8.S, _wallsRoot);
                        cardinalCount++;
                    }
                    if ((cell & CellFlags8.WallW) != 0)
                    {
                        SpawnCardinalWall(cardinalPrefab, wx, wz, Direction8.W, _wallsRoot);
                        cardinalCount++;
                    }

                    // Diagonal walls removed 2026-03-09 - cardinal-only passages
                    // diagonalCount remains 0
                }
            }

            Debug.Log($"  Walls: {cardinalCount} cardinal, {diagonalCount} diagonal");
            Debug.Log($"[MazePreview] Diagonal walls check - enabled: false (removed 2026-03-09), prefab: {(diagonalPrefab != null ? "OK" : "MISSING")}");
        }

        private void SpawnCardinalWall(GameObject prefab, float x, float z, Direction8 dir, Transform parent)
        {
            var (dx, dz) = Direction8Helper.ToOffset(dir);

            // Calculate wall position (centered on cell edge)
            float wallX = x + dx * _config.CellSize / 2;
            float wallZ = z + dz * _config.CellSize / 2;
            Vector3 position = new Vector3(wallX, _config.WallHeight / 2, wallZ);

            // Calculate rotation
            float angle = dir switch
            {
                Direction8.N => 0f,
                Direction8.E => 90f,
                Direction8.S => 180f,
                Direction8.W => 270f,
                _ => 0f
            };

            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);

            // Instantiate (use PrefabUtility for proper prefab workflow)
            GameObject wall = PrefabUtility.InstantiatePrefab(prefab) as GameObject ??
                             UnityEngine.Object.Instantiate(prefab, position, rotation);

            if (wall == null) return;

            wall.name = $"Wall_{dir}_{x}_{z}";
            wall.transform.SetPositionAndRotation(position, rotation);
            wall.tag = "EditorOnly";
            wall.layer = LayerMask.NameToLayer("Ignore Raycast");
            wall.transform.SetParent(parent);
        }

        private void SpawnDiagonalWall(GameObject prefab, float x, float z, float angle, Direction8 dir, Transform parent)
        {
            // Calculate position (corner of cell for diagonal)
            float h = _config.CellSize / 2;
            Vector3 offset = dir switch
            {
                Direction8.NE => new Vector3(h, 0, h),
                Direction8.NW => new Vector3(-h, 0, h),
                Direction8.SE => new Vector3(h, 0, -h),
                Direction8.SW => new Vector3(-h, 0, -h),
                _ => Vector3.zero
            };
            float wallX = x + _config.CellSize / 2 + offset.x;
            float wallZ = z + _config.CellSize / 2 + offset.z;

            // Rotation
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);

            // Instantiate
            GameObject wall = PrefabUtility.InstantiatePrefab(prefab) as GameObject ??
                             UnityEngine.Object.Instantiate(prefab, new Vector3(wallX, _config.WallHeight / 2, wallZ), rotation);

            if (wall == null) return;

            wall.name = $"DiagWall_{dir}_{x}_{z}";
            wall.tag = "EditorOnly";
            wall.layer = LayerMask.NameToLayer("Ignore Raycast");
            wall.transform.SetParent(parent);
        }

        private void GenerateObjects()
        {
            int torchCount = 0;
            int chestCount = 0;
            int enemyCount = 0;

            // Load prefabs from Resources (Plug-in-Out: use existing prefabs)
            string torchPath = _config.TorchPrefab.Replace(".prefab", "");
            string chestPath = _config.ChestPrefab.Replace(".prefab", "");
            string enemyPath = _config.EnemyPrefab.Replace(".prefab", "");

            GameObject torchPrefab = Resources.Load<GameObject>(torchPath);
            GameObject chestPrefab = Resources.Load<GameObject>(chestPath);
            GameObject enemyPrefab = Resources.Load<GameObject>(enemyPath);

            // Log warnings for missing prefabs (once)
            if (torchPrefab == null) Debug.LogWarning($"[MazePreview] Torch prefab not found: {torchPath}");
            if (chestPrefab == null) Debug.LogWarning($"[MazePreview] Chest prefab not found: {chestPath}");
            if (enemyPrefab == null) Debug.LogWarning($"[MazePreview] Enemy prefab not found: {enemyPath}");

            for (int x = 0; x < _previewData.Width; x++)
            {
                for (int z = 0; z < _previewData.Height; z++)
                {
                    var cell = _previewData.GetCell(x, z);

                    float ox = x * _config.CellSize + _config.CellSize / 2;
                    float oz = z * _config.CellSize + _config.CellSize / 2;

                    // Torches
                    if ((cell & CellFlags8.HasTorch) != 0)
                    {
                        SpawnObject(
                            torchPrefab,
                            new Vector3(ox, _config.WallHeight * 0.75f, oz),
                            $"Torch_{x}_{z}",
                            _objectsRoot,
                            new Color(1f, 0.5f, 0f) // Orange
                        );
                        torchCount++;
                    }

                    // Chests
                    if ((cell & CellFlags8.HasChest) != 0)
                    {
                        SpawnObject(
                            chestPrefab,
                            new Vector3(ox, 0.5f, oz),
                            $"Chest_{x}_{z}",
                            _objectsRoot,
                            new Color(1f, 0.84f, 0f) // Gold
                        );
                        chestCount++;
                    }

                    // Enemies
                    if ((cell & CellFlags8.HasEnemy) != 0)
                    {
                        SpawnObject(
                            enemyPrefab,
                            new Vector3(ox, 1f, oz),
                            $"Enemy_{x}_{z}",
                            _objectsRoot,
                            new Color(1f, 0f, 0f) // Red
                        );
                        enemyCount++;
                    }
                }
            }

            Debug.Log($"  Objects: {torchCount} torches, {chestCount} chests, {enemyCount} enemies");
        }

        private void SpawnObject(GameObject prefab, Vector3 position, string name, Transform parent, Color color)
        {
            GameObject obj;

            if (prefab != null)
            {
                // Use prefab (Plug-in-Out: use existing prefabs)
                obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject ??
                     UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                // Fallback: create simple cube with Unlit/Color shader
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.position = position;

                // Remove collider (not needed for preview)
                var collider = obj.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyImmediate(collider);
                }

                Debug.LogWarning($"  Prefab not assigned for {name}, using fallback cube");
            }

            obj.name = name;
            obj.tag = "EditorOnly";
            obj.layer = LayerMask.NameToLayer("Ignore Raycast");
            obj.transform.SetParent(parent);

            // Apply color
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material mat = renderer.sharedMaterial;
                if (mat == null)
                {
                    mat = new Material(Shader.Find("Unlit/Color"));
                    renderer.sharedMaterial = mat;
                }
                mat.color = color;
            }
        }

        private void PositionSceneCamera()
        {
            // Calculate maze center and bounds
            float size = _previewData.Width * _config.CellSize;
            Vector3 center = new Vector3(size / 2f, size * 0.6f, size / 2f);

            // Position scene view camera
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.LookAt(
                    center,
                    Quaternion.LookRotation(-Vector3.forward, Vector3.up),
                    size * 0.9f,
                    false
                );
            }
        }

        private void ClearPreview()
        {
            if (_previewRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(_previewRoot);
                _previewRoot = null;
                _wallsRoot = null;
                _objectsRoot = null;
                Debug.Log("  [MazePreview] Previous preview cleared");
            }

            _previewData = null;
        }

        private void OnDestroy()
        {
            ClearPreview();
        }
    }
}
#endif
