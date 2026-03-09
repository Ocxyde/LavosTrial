// Copyright (C) 2026 CodeDotLavos
// This file is part of PeuImporte.
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using Code.Lavos.Core;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Tools
{
	/// <summary>
	/// Universal Level Generator Editor Tool
	///
	/// Complete system for procedurally generating dungeons with:
	/// - Exponential difficulty scaling
	/// - Seed-based deterministic generation
	/// - Full database persistence
	/// - RAM/Disk storage management
	/// - Real-time progress tracking
	/// - Comprehensive statistics
	///
	/// Usage: Tools > Level Generator > Procedural Level Builder
	/// </summary>
	public sealed class UniversalLevelGeneratorTool : EditorWindow
	{
		private const int DEFAULT_SEED_BASE = 20260308;
		
		// UI Layout Constants
		private const float LABEL_WIDTH = 120f;
		private const float BUTTON_HEIGHT_LARGE = 60f;
		private const float BUTTON_HEIGHT_MEDIUM = 50f;
		private const float BUTTON_HEIGHT_SMALL = 30f;
		private const float BUTTON_HEIGHT_TINY = 25f;
		private const float STATUS_BAR_HEIGHT = 60f;
		private const int MAX_LOG_MESSAGES = 100;
		private const int RECENT_LOG_COUNT = 5;

		// UI State
		private Vector2 _scrollPosition = Vector2.zero;
		private int _selectedTab = 0;
		private bool _showAdvancedSettings = false;
		private bool _showStorageManager = false;
		private bool _showStatistics = false;

		// Generation Parameters
		private int _targetLevelNumber = 1;
		private int _customSeed = DEFAULT_SEED_BASE;
		private bool _useCustomSeed = false;
		private float _difficultyMultiplier = 1.0f;
		private bool _autoSpawnPlayer = true;
		private bool _saveToDisk = true;
		private bool _saveToDatabase = true;

		// Advanced Parameters
		private bool _enableDiagonalWalls = true;
		private bool _enableBossRoom = false;
		private bool _enableEnvironmentalHazards = false;
		private bool _enableStatusEffects = false;
		private bool _usePassageFirst = false;  // Passage-first generation toggle
		private float _customEnemyDensity = -1f;
		private float _customTrapDensity = -1f;
		private float _customTreasureDensity = -1f;

		// Batch Generation (separate state from single generation)
		private int _batchStartLevel = 1;
		private int _batchEndLevel = 5;
		private bool _batchSaveToDisk = true;
		private bool _batchSaveToDatabase = true;

		// Status & Logging
		private string _statusMessage = "Ready";
		private Color _statusColor = Color.green;
		private List<string> _logMessages = new List<string>();
		private bool _isGenerating = false;
		private float _generationProgress = 0f;

		// References
		private ProceduralLevelGenerator _levelGenerator;
		private LevelDatabaseManager _databaseManager;
		private List<LevelData> _cachedLevels = new List<LevelData>();
		private LevelGenerationStats _lastStats;
		
		// Event handlers stored for proper unsubscription
		private Action<string> _onGenerationLogHandler;
		private Action<int, float> _onGenerationProgressHandler;

		[MenuItem("Tools/Level Generator/Procedural Level Builder")]
		public static void ShowWindow()
		{
			var window = GetWindow<UniversalLevelGeneratorTool>("Procedural Levels");
			window.minSize = new Vector2(700, 800);
		}

		private void OnEnable()
		{
			FindOrCreateManagers();
			LoadCachedLevels();
		}

		private void OnDisable()
		{
			// Unsubscribe from events to prevent memory leaks
			if (_levelGenerator != null)
			{
				_levelGenerator.OnGenerationLog -= _onGenerationLogHandler;
				_levelGenerator.OnGenerationProgress -= _onGenerationProgressHandler;
			}
			
			// Clear progress bar if still showing
			EditorUtility.ClearProgressBar();
		}

		private void OnGUI()
		{
			DrawHeader();

			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

			DrawTabsNavigation();
			GUILayout.Space(10);

			switch (_selectedTab)
			{
				case 0:
					DrawSingleGenerationTab();
					break;
				case 1:
					DrawBatchGenerationTab();
					break;
				case 2:
					DrawStorageManagerTab();
					break;
				case 3:
					DrawStatisticsTab();
					break;
			}

			GUILayout.Space(20);
			DrawStatusBar();

			GUILayout.EndScrollView();
		}

		private void DrawHeader()
		{
			var headerStyle = new GUIStyle(EditorStyles.boldLabel)
			{
				fontSize = 18
			};

			GUILayout.Label("UNIVERSAL PROCEDURAL LEVEL GENERATOR", headerStyle);
			GUILayout.Label("Exponential Difficulty Scaling | Seed-Based Generation | Full Database Support", 
				EditorStyles.miniLabel);
			GUILayout.Space(15);
		}

		private void DrawTabsNavigation()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			{
				_selectedTab = GUILayout.Toolbar(_selectedTab, new[] 
				{
					"Single Level",
					"Batch Generation",
					"Storage Manager",
					"Statistics"
				});
			}
			GUILayout.EndHorizontal();
		}

		private void DrawSingleGenerationTab()
		{
			GUILayout.Label("Single Level Generation", EditorStyles.boldLabel);

			EditorGUILayout.BeginVertical("box");
			{
				// Level Number
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Level Number:", GUILayout.Width(LABEL_WIDTH));
					_targetLevelNumber = EditorGUILayout.IntSlider(_targetLevelNumber, 1, 50);
				}
				EditorGUILayout.EndHorizontal();

				// Difficulty Multiplier
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Difficulty Mult:", GUILayout.Width(LABEL_WIDTH));
					_difficultyMultiplier = EditorGUILayout.Slider(_difficultyMultiplier, 0.5f, 2.5f);
				}
				EditorGUILayout.EndHorizontal();

				// Seed Options
				EditorGUILayout.Space();
				_useCustomSeed = EditorGUILayout.Toggle("Custom Seed", _useCustomSeed);
				if (_useCustomSeed)
				{
					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.Label("Seed:", GUILayout.Width(LABEL_WIDTH));
						_customSeed = EditorGUILayout.IntField(_customSeed);

						if (GUILayout.Button("Random", GUILayout.Width(80)))
						{
							_customSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
						}
					}
					EditorGUILayout.EndHorizontal();
				}

				// Options
				EditorGUILayout.Space();
				_autoSpawnPlayer = EditorGUILayout.Toggle("Auto-Spawn Player", _autoSpawnPlayer);
				_saveToDisk = EditorGUILayout.Toggle("Save to Disk", _saveToDisk);
				_saveToDatabase = EditorGUILayout.Toggle("Save to Database", _saveToDatabase);
			}
			EditorGUILayout.EndVertical();

			GUILayout.Space(10);

			// Advanced Settings Foldout
			_showAdvancedSettings = EditorGUILayout.Foldout(_showAdvancedSettings, "Advanced Settings");
			if (_showAdvancedSettings)
			{
				EditorGUI.indentLevel++;

				EditorGUILayout.BeginVertical("box");
				{
					_enableDiagonalWalls = EditorGUILayout.Toggle("Enable Diagonal Walls", _enableDiagonalWalls);
					_enableBossRoom = EditorGUILayout.Toggle("Enable Boss Room", _enableBossRoom);
					_enableEnvironmentalHazards = EditorGUILayout.Toggle("Environmental Hazards",
						_enableEnvironmentalHazards);
					_enableStatusEffects = EditorGUILayout.Toggle("Status Effects", _enableStatusEffects);
					
					EditorGUILayout.Space();
					_usePassageFirst = EditorGUILayout.Toggle("Passage-First Mode", _usePassageFirst);
					if (_usePassageFirst)
					{
						EditorGUILayout.HelpBox(
							"Passage-First creates a clear path from Entrance (A) to Exit (B) first,\n" +
							"then adds walls around it. Guarantees walkable maze!",
							MessageType.Info);
					}

					EditorGUILayout.Space();
					GUILayout.Label("Custom Densities (leave -1 for auto):");

					_customEnemyDensity = EditorGUILayout.Slider("Enemy Density", _customEnemyDensity, -1f, 1f);
					_customTrapDensity = EditorGUILayout.Slider("Trap Density", _customTrapDensity, -1f, 1f);
					_customTreasureDensity = EditorGUILayout.Slider("Treasure Density", _customTreasureDensity, -1f, 1f);
				}
				EditorGUILayout.EndVertical();

				EditorGUI.indentLevel--;
			}

			GUILayout.Space(15);

			// Main Generation Button
			var prevColor = GUI.backgroundColor;
			GUI.backgroundColor = Color.green;

			if (GUILayout.Button("GENERATE LEVEL", GUILayout.Height(BUTTON_HEIGHT_LARGE)))
			{
				GenerateSingleLevel();
			}

			GUI.backgroundColor = prevColor;

			// Quick Actions
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Load from Storage", GUILayout.Height(BUTTON_HEIGHT_SMALL)))
				{
					LoadLevelFromStorage();
				}

				if (GUILayout.Button("Clear Scene", GUILayout.Height(BUTTON_HEIGHT_SMALL)))
				{
					ClearGeneratedLevel();
				}
			}
			EditorGUILayout.EndHorizontal();

			// Progress Bar
			if (_isGenerating)
			{
				EditorGUILayout.HelpBox($"Generating... {_generationProgress:F0}%", MessageType.Info);
				Repaint();
			}
		}

		private void DrawBatchGenerationTab()
		{
			GUILayout.Label("Batch Level Generation", EditorStyles.boldLabel);

			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Start Level:", GUILayout.Width(LABEL_WIDTH));
					_batchStartLevel = EditorGUILayout.IntField(_batchStartLevel, GUILayout.Width(60));

					GUILayout.Label("End Level:", GUILayout.Width(100));
					_batchEndLevel = EditorGUILayout.IntField(_batchEndLevel);
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.HelpBox(
					$"Will generate {Mathf.Max(0, _batchEndLevel - _batchStartLevel + 1)} levels\n" +
					$"From Level {_batchStartLevel} to Level {_batchEndLevel}",
					MessageType.Info);

				_batchSaveToDisk = EditorGUILayout.Toggle("Save All to Disk", _batchSaveToDisk);
				_batchSaveToDatabase = EditorGUILayout.Toggle("Save All to Database", _batchSaveToDatabase);
			}
			EditorGUILayout.EndVertical();

			GUILayout.Space(15);

			var prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(1f, 0.7f, 0f);

			if (GUILayout.Button("GENERATE BATCH", GUILayout.Height(BUTTON_HEIGHT_MEDIUM)))
			{
				GenerateBatchLevels();
			}

			GUI.backgroundColor = prevColor;
		}

		private void DrawStorageManagerTab()
		{
			GUILayout.Label("Storage & Database Management", EditorStyles.boldLabel);
			
			// Validate manager
			if (_databaseManager == null)
			{
				EditorGUILayout.HelpBox("Database manager not initialized", MessageType.Error);
				return;
			}

			EditorGUILayout.BeginVertical("box");
			{
				var stats = _databaseManager.GetStorageStats();
				EditorGUILayout.LabelField("Cached Levels:", stats.CachedLevels.ToString());
				EditorGUILayout.LabelField("Disk Levels:", stats.DiskLevels.ToString());
				EditorGUILayout.LabelField("Storage Size:", FormatBytes(stats.StorageSize));

				EditorGUILayout.Space();

				if (GUILayout.Button("Refresh Storage", GUILayout.Height(BUTTON_HEIGHT_TINY)))
				{
					LoadCachedLevels();
				}

				if (GUILayout.Button("Open Storage Folder", GUILayout.Height(BUTTON_HEIGHT_TINY)))
				{
					var storageDir = Application.persistentDataPath;
					EditorUtility.RevealInFinder(storageDir);
				}
			}
			EditorGUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.Label("Cached Levels", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical("box");
			{
				if (_cachedLevels.Count == 0)
				{
					EditorGUILayout.HelpBox("No levels cached", MessageType.Info);
				}
				else
				{
					foreach (var level in _cachedLevels)
					{
						EditorGUILayout.BeginHorizontal("box");
						{
							EditorGUILayout.LabelField($"Level {level.LevelNumber}: {level.LevelName}");

							if (GUILayout.Button("Delete", GUILayout.Width(60)))
							{
								if (EditorUtility.DisplayDialog("Confirm Delete", 
									$"Delete Level {level.LevelNumber}?", "Yes", "No"))
								{
									_databaseManager.DeleteLevel(level.LevelNumber);
									LoadCachedLevels();
								}
							}
						}
						EditorGUILayout.EndHorizontal();
					}
				}
			}
			EditorGUILayout.EndVertical();
		}

		private void DrawStatisticsTab()
		{
			GUILayout.Label("Generation Statistics", EditorStyles.boldLabel);

			if (_lastStats == null)
			{
				EditorGUILayout.HelpBox("No generation statistics available yet", MessageType.Info);
				return;
			}

			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.LabelField("Level Number:", _lastStats.LevelNumber.ToString());
				EditorGUILayout.LabelField("Seed:", _lastStats.Seed.ToString());
				EditorGUILayout.LabelField("Generation Time:", $"{_lastStats.GenerationTimeMs:F2}ms");
				EditorGUILayout.LabelField("Difficulty Factor:", $"{_lastStats.DifficultyFactor:F2}");

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Generated At:", _lastStats.GeneratedAt.ToString());

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("POPULATION STATISTICS", EditorStyles.boldLabel);

				EditorGUILayout.LabelField("Rooms:", _lastStats.RoomCount.ToString());
				EditorGUILayout.LabelField("Walls:", _lastStats.WallCount.ToString());
				EditorGUILayout.LabelField("Doors:", _lastStats.DoorCount.ToString());
				EditorGUILayout.LabelField("  - Locked:", _lastStats.LockedDoorCount.ToString());
				EditorGUILayout.LabelField("  - Secret:", _lastStats.SecretDoorCount.ToString());
				EditorGUILayout.LabelField("Enemies:", _lastStats.EnemyCount.ToString());
				EditorGUILayout.LabelField("Treasures:", _lastStats.TreasureCount.ToString());
				EditorGUILayout.LabelField("Traps:", _lastStats.TrapCount.ToString());
				EditorGUILayout.LabelField("Torches:", _lastStats.TorchCount.ToString());

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Total Objects:", _lastStats.GetTotalCount().ToString());
			}
			EditorGUILayout.EndVertical();
		}

		private void DrawStatusBar()
		{
			GUILayout.Label("Status Log", EditorStyles.boldLabel);

			var prevColor = GUI.color;
			GUI.color = _statusColor;
			EditorGUILayout.TextArea(_statusMessage, GUILayout.Height(STATUS_BAR_HEIGHT));
			GUI.color = prevColor;

			if (_logMessages.Count > 0)
			{
				EditorGUILayout.LabelField("Recent Messages:");
				EditorGUILayout.BeginVertical("box");
				{
					// Optimized: avoid LINQ allocation by iterating directly
					int startIndex = Mathf.Max(0, _logMessages.Count - RECENT_LOG_COUNT);
					for (int i = startIndex; i < _logMessages.Count; i++)
					{
						EditorGUILayout.LabelField(_logMessages[i], EditorStyles.miniLabel);
					}
				}
				EditorGUILayout.EndVertical();
			}
		}

		private void GenerateSingleLevel()
		{
			if (_isGenerating)
			{
				SetStatus("Already generating, please wait", Color.yellow);
				return;
			}
			
			// Validate managers are available
			if (_levelGenerator == null || _databaseManager == null)
			{
				SetStatus("ERROR: Managers not initialized", Color.red);
				return;
			}

			_isGenerating = true;
			_generationProgress = 0f;

			try
			{
				SetStatus($"Generating Level {_targetLevelNumber}...", Color.yellow);

				int seed = _useCustomSeed ? _customSeed : (_targetLevelNumber * DEFAULT_SEED_BASE);

				_levelGenerator.GenerateLevel(_targetLevelNumber, seed);

				var stats = _levelGenerator.GetLastGenerationStats();
				if (stats != null)
				{
					_lastStats = stats;
				}

				if (_saveToDisk || _saveToDatabase)
				{
					var levelData = _levelGenerator.GetCurrentLevelData();
					if (levelData != null)
					{
						_databaseManager.SaveLevel(levelData, _saveToDisk);
						LoadCachedLevels();
					}
				}

				SetStatus($"Level {_targetLevelNumber} generated successfully!", Color.green);
			}
			catch (Exception ex)
			{
				SetStatus($"ERROR: {ex.Message}", Color.red);
				Debug.LogError(ex);
			}
			finally
			{
				_isGenerating = false;
				_generationProgress = 100f;
			}
		}

		private void GenerateBatchLevels()
		{
			if (_isGenerating)
			{
				SetStatus("Already generating, please wait", Color.yellow);
				return;
			}
			
			// Validate managers are available
			if (_levelGenerator == null || _databaseManager == null)
			{
				SetStatus("ERROR: Managers not initialized", Color.red);
				return;
			}

			int count = _batchEndLevel - _batchStartLevel + 1;
			if (count <= 0)
			{
				SetStatus("Invalid level range", Color.red);
				return;
			}
			
			// Validate level range
			if (_batchStartLevel < 1 || _batchEndLevel < 1)
			{
				SetStatus("ERROR: Level numbers must be positive", Color.red);
				return;
			}
			
			if (_batchEndLevel < _batchStartLevel)
			{
				SetStatus("ERROR: End level must be >= start level", Color.red);
				return;
			}

			_isGenerating = true;

			try
			{
				SetStatus($"Generating {count} levels...", Color.yellow);

				for (int i = _batchStartLevel; i <= _batchEndLevel; i++)
				{
					float progress = (i - _batchStartLevel + 1) / (float)count;
					_generationProgress = progress * 100f;

					_levelGenerator.GenerateLevel(i);

					var levelData = _levelGenerator.GetCurrentLevelData();
					if (levelData != null && (_batchSaveToDisk || _batchSaveToDatabase))
					{
						_databaseManager.SaveLevel(levelData, _batchSaveToDisk);
					}

					EditorUtility.DisplayProgressBar("Batch Generation",
						$"Level {i}/{_batchEndLevel}", progress);
				}

				EditorUtility.ClearProgressBar();
				LoadCachedLevels();

				SetStatus($"Batch generation complete: {count} levels", Color.green);
			}
			catch (Exception ex)
			{
				EditorUtility.ClearProgressBar();
				SetStatus($"ERROR: {ex.Message}", Color.red);
				Debug.LogError(ex);
			}
			finally
			{
				_isGenerating = false;
			}
		}

		private void LoadLevelFromStorage()
		{
			var menu = new GenericMenu();

			foreach (var level in _cachedLevels)
			{
				menu.AddItem(
					new GUIContent($"Level {level.LevelNumber}"),
					false,
					() => InstantiateLevel(level)
				);
			}

			if (_cachedLevels.Count == 0)
			{
				menu.AddDisabledItem(new GUIContent("No levels available"));
			}

			menu.ShowAsContext();
		}

		private void InstantiateLevel(LevelData levelData)
		{
			if (levelData == null)
			{
				SetStatus("ERROR: Invalid level data", Color.red);
				return;
			}
			
			if (_levelGenerator == null)
			{
				SetStatus("ERROR: Level generator not initialized", Color.red);
				return;
			}
			
			SetStatus($"Instantiating Level {levelData.LevelNumber} from storage...", Color.yellow);

			try
			{
				_levelGenerator.GenerateLevel(levelData.LevelNumber, levelData.Seed);
				SetStatus($"Level {levelData.LevelNumber} instantiated", Color.green);
			}
			catch (Exception ex)
			{
				SetStatus($"ERROR: {ex.Message}", Color.red);
				Debug.LogError(ex);
			}
		}

		private void ClearGeneratedLevel()
		{
			var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder8>();
			if (mazeBuilder != null)
			{
				Undo.DestroyObjectImmediate(mazeBuilder.gameObject);
				Undo.SetCurrentGroupName("Clear Generated Level");
				SetStatus("Scene cleared", Color.green);
			}
		}

		private void FindOrCreateManagers()
		{
			_levelGenerator = ProceduralLevelGenerator.Instance;
			_databaseManager = LevelDatabaseManager.Instance;
			
			// Validate singletons are available
			if (_levelGenerator == null)
			{
				Debug.LogError("[UniversalLevelGeneratorTool] ProceduralLevelGenerator instance not found!");
				return;
			}
			
			if (_databaseManager == null)
			{
				Debug.LogError("[UniversalLevelGeneratorTool] LevelDatabaseManager instance not found!");
				return;
			}
			
			// Store handlers for proper unsubscription
			_onGenerationLogHandler = msg => AddLog(msg);
			_onGenerationProgressHandler = (step, progress) =>
			{
				// Thread-safe update via EditorApplication.update
				EditorApplication.delayCall += () =>
				{
					_generationProgress = progress * 100f;
				};
			};
			
			_levelGenerator.OnGenerationLog += _onGenerationLogHandler;
			_levelGenerator.OnGenerationProgress += _onGenerationProgressHandler;
		}

		private void LoadCachedLevels()
		{
			_cachedLevels = _databaseManager.LoadAllLevels();
		}

		private void SetStatus(string message, Color color)
		{
			_statusMessage = message;
			_statusColor = color;
			AddLog(message);
		}

		private void AddLog(string message)
		{
			_logMessages.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
			if (_logMessages.Count > MAX_LOG_MESSAGES)
			{
				_logMessages.RemoveAt(0);
			}
		}

		private string FormatBytes(long bytes)
		{
			string[] sizes = { "B", "KB", "MB", "GB" };
			double len = bytes;
			int order = 0;
			while (len >= 1024 && order < sizes.Length - 1)
			{
				order++;
				len = len / 1024;
			}
			return $"{len:0.##} {sizes[order]}";
		}
	}
}

#endif
