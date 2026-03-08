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
using Code.Lavos.Core.Procedural;

namespace Code.Lavos.Tools.Procedural
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
		private float _customEnemyDensity = -1f;
		private float _customTrapDensity = -1f;
		private float _customTreasureDensity = -1f;

		// Batch Generation
		private int _batchStartLevel = 1;
		private int _batchEndLevel = 5;
		private bool _generateBatch = false;

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
					GUILayout.Label("Level Number:", GUILayout.Width(120));
					_targetLevelNumber = EditorGUILayout.IntSlider(_targetLevelNumber, 1, 50);
				}
				EditorGUILayout.EndHorizontal();

				// Difficulty Multiplier
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Difficulty Mult:", GUILayout.Width(120));
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
						GUILayout.Label("Seed:", GUILayout.Width(120));
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

			if (GUILayout.Button("GENERATE LEVEL", GUILayout.Height(60)))
			{
				GenerateSingleLevel();
			}

			GUI.backgroundColor = prevColor;

			// Quick Actions
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Load from Storage", GUILayout.Height(30)))
				{
					LoadLevelFromStorage();
				}

				if (GUILayout.Button("Clear Scene", GUILayout.Height(30)))
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
					GUILayout.Label("Start Level:", GUILayout.Width(120));
					_batchStartLevel = EditorGUILayout.IntField(_batchStartLevel, GUILayout.Width(60));

					GUILayout.Label("End Level:", GUILayout.Width(100));
					_batchEndLevel = EditorGUILayout.IntField(_batchEndLevel);
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.HelpBox(
					$"Will generate {Mathf.Max(0, _batchEndLevel - _batchStartLevel + 1)} levels\n" +
					$"From Level {_batchStartLevel} to Level {_batchEndLevel}",
					MessageType.Info);

				_saveToDisk = EditorGUILayout.Toggle("Save All to Disk", _saveToDisk);
				_saveToDatabase = EditorGUILayout.Toggle("Save All to Database", _saveToDatabase);
			}
			EditorGUILayout.EndVertical();

			GUILayout.Space(15);

			var prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(1f, 0.7f, 0f);

			if (GUILayout.Button("GENERATE BATCH", GUILayout.Height(50)))
			{
				GenerateBatchLevels();
			}

			GUI.backgroundColor = prevColor;
		}

		private void DrawStorageManagerTab()
		{
			GUILayout.Label("Storage & Database Management", EditorStyles.boldLabel);

			EditorGUILayout.BeginVertical("box");
			{
				var stats = _databaseManager.GetStorageStats();
				EditorGUILayout.LabelField("Cached Levels:", stats.CachedLevels.ToString());
				EditorGUILayout.LabelField("Disk Levels:", stats.DiskLevels.ToString());
				EditorGUILayout.LabelField("Storage Size:", FormatBytes(stats.StorageSize));

				EditorGUILayout.Space();

				if (GUILayout.Button("Refresh Storage", GUILayout.Height(25)))
				{
					LoadCachedLevels();
				}

				if (GUILayout.Button("Open Storage Folder", GUILayout.Height(25)))
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
			EditorGUILayout.TextArea(_statusMessage, GUILayout.Height(60));
			GUI.color = prevColor;

			if (_logMessages.Count > 0)
			{
				EditorGUILayout.LabelField("Recent Messages:");
				EditorGUILayout.BeginVertical("box");
				{
					var recentLogs = _logMessages.TakeLast(5);
					foreach (var log in recentLogs)
					{
						EditorGUILayout.LabelField(log, EditorStyles.miniLabel);
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

			int count = _batchEndLevel - _batchStartLevel + 1;
			if (count <= 0)
			{
				SetStatus("Invalid level range", Color.red);
				return;
			}

			_isGenerating = true;

			try
			{
				SetStatus($"Generating {count} levels...", Color.yellow);

				for (int i = _batchStartLevel; i <= _batchEndLevel; i++)
				{
					_generationProgress = ((i - _batchStartLevel) / (float)count) * 100f;

					_levelGenerator.GenerateLevel(i);

					var levelData = _levelGenerator.GetCurrentLevelData();
					if (levelData != null && (_saveToDisk || _saveToDatabase))
					{
						_databaseManager.SaveLevel(levelData, _saveToDisk);
					}

					EditorUtility.DisplayProgressBar("Batch Generation", 
						$"Level {i}/{_batchEndLevel}", _generationProgress / 100f);
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
			SetStatus($"Instantiating Level {levelData.LevelNumber} from storage...", Color.yellow);

			try
			{
				_levelGenerator.GenerateLevel(levelData.LevelNumber, levelData.Seed);
				SetStatus($"Level {levelData.LevelNumber} instantiated", Color.green);
			}
			catch (Exception ex)
			{
				SetStatus($"ERROR: {ex.Message}", Color.red);
			}
		}

		private void ClearGeneratedLevel()
		{
			var mazeBuilder = FindObjectOfType<CompleteMazeBuilder8>();
			if (mazeBuilder != null)
			{
				DestroyImmediate(mazeBuilder.gameObject);
				SetStatus("Scene cleared", Color.green);
			}
		}

		private void FindOrCreateManagers()
		{
			_levelGenerator = ProceduralLevelGenerator.Instance;
			_databaseManager = LevelDatabaseManager.Instance;

			_levelGenerator.OnGenerationLog += msg =>
			{
				AddLog(msg);
			};

			_levelGenerator.OnGenerationProgress += (step, progress) =>
			{
				_generationProgress = progress * 100f;
			};
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
			if (_logMessages.Count > 100)
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
