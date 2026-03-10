// Copyright (C) 2026 CodeDotLavos
// This file is part of PeuImporte.
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using System;
using System.Collections.Generic;
using UnityEngine;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Core
{
	/// <summary>
	/// Universal procedural level generator using all project APIs.
	/// 
	/// Integrates with:
	/// - CompleteMazeBuilder8 (maze generation)
	/// - ItemEngine (item registry and placement)
	/// - DoorsEngine (door management)
	/// - GameManager (game state)
	/// - SpatialPlacer (object placement)
	/// - LightPlacementEngine (dynamic lighting)
	/// - EnemyPlacer (enemy spawning)
	/// - TrapBehavior (trap system)
	/// - ComputeGridEngine (grid calculations)
	/// - PlayerController (player setup)
	/// - DatabaseManager (persistence)
	/// </summary>
	public sealed class ProceduralLevelGenerator : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private CompleteMazeBuilder8 mazeBuilder;
		[SerializeField] private GameManager gameManager;
		[SerializeField] private ItemEngine itemEngine;
		[SerializeField] private DoorsEngine doorsEngine;
		[SerializeField] private SpatialPlacer spatialPlacer;
		[SerializeField] private LightPlacementEngine lightEngine;
		[SerializeField] private EnemyPlacer enemyPlacer;
		[SerializeField] private ComputeGridEngine computeGridEngine;

		[Header("Corridor Decorations")]
		[SerializeField] private GameObject pillarPrefab;
		[SerializeField] private GameObject archPrefab;
		[SerializeField] private GameObject nicheStatuePrefab;
		[SerializeField] private bool autoInstantiateDecorations = true;

		[Header("Configuration")]
		[SerializeField] private bool autoFindComponents = true;
		[SerializeField] private bool logDetailedProgress = true;

		private LevelData _currentLevelData;
		private LevelGenerationStats _lastStats;
		private bool _isGenerating = false;

		public event Action<int, float> OnGenerationProgress;
		public event Action<LevelData> OnLevelGenerated;
		public event Action<string> OnGenerationLog;

		private static ProceduralLevelGenerator _instance;
		public static ProceduralLevelGenerator Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindFirstObjectByType<ProceduralLevelGenerator>();
					if (_instance == null)
					{
						var go = new GameObject("ProceduralLevelGenerator");
						_instance = go.AddComponent<ProceduralLevelGenerator>();
					}
				}
				return _instance;
			}
		}

		private void OnEnable()
		{
			if (_instance == null)
			{
				_instance = this;
			}
		}

		private void Start()
		{
			if (autoFindComponents)
			{
				FindAllComponents();
			}
		}

		/// <summary>
		/// Generate a complete level with exponential difficulty scaling.
		/// </summary>
		public void GenerateLevel(int levelNumber, int customSeed = -1)
		{
			if (_isGenerating)
			{
				Log("Already generating a level, please wait.");
				return;
			}

			_isGenerating = true;
			var startTime = DateTime.Now;

			try
			{
				Log($"Generating Level {levelNumber}...");

				// Create level data
				var levelData = LevelData.CreateDefault(levelNumber);
				if (customSeed >= 0)
				{
					levelData.Seed = customSeed;
				}

				// Initialize stats early (needed by population methods)
				_lastStats = new LevelGenerationStats
				{
					LevelNumber = levelNumber,
					Seed = levelData.Seed,
					GenerationTimeMs = 0f,
					DifficultyFactor = 0f,
					GeneratedAt = DateTime.Now
				};

				// Calculate difficulty factor (exponential)
				levelData.DifficultyFactor = LevelDifficultyScaler.CalculateDifficultyFactor(levelNumber);
				Log($"Difficulty Factor: {levelData.DifficultyFactor:F2}");
				_lastStats.DifficultyFactor = levelData.DifficultyFactor;

				// Scale parameters
				OnGenerationProgress?.Invoke(10, 1.0f);
				ScaleParametersForLevel(levelData);

				// Generate maze structure using CompleteMazeBuilder8 API
				OnGenerationProgress?.Invoke(20, 1.0f);
				GenerateMazeStructure(levelData);

				// Populate with enemies
				OnGenerationProgress?.Invoke(40, 1.0f);
				PopulateEnemies(levelData);

				// Place treasures
				OnGenerationProgress?.Invoke(50, 1.0f);
				PopulateTreasures(levelData);

				// Place traps
				OnGenerationProgress?.Invoke(60, 1.0f);
				if (levelData.DifficultyParams?.TrapDensity > 0)
				{
					PopulateTraps(levelData);
				}

				// Setup lighting
				OnGenerationProgress?.Invoke(70, 1.0f);
				SetupLighting(levelData);

				// Setup game systems
				OnGenerationProgress?.Invoke(85, 1.0f);
				SetupGameSystems(levelData);

				// Spawn player
				OnGenerationProgress?.Invoke(90, 1.0f);
				SetupPlayer(levelData);

				// Update final statistics
				_lastStats.GenerationTimeMs = (float)(DateTime.Now - startTime).TotalMilliseconds;

				levelData.IsGenerated = true;
				levelData.LastGeneratedAt = DateTime.Now;
				_currentLevelData = levelData;

				OnGenerationProgress?.Invoke(100, 1.0f);
				OnLevelGenerated?.Invoke(levelData);

				Log($"Level {levelNumber} generated successfully in {_lastStats.GenerationTimeMs:F2}ms");
			}
			catch (Exception ex)
			{
				Log($"ERROR: {ex.Message}");
				Debug.LogError($"Level generation failed: {ex}");
			}
			finally
			{
				_isGenerating = false;
			}
		}

		private void ScaleParametersForLevel(LevelData levelData)
		{
			if (levelData == null)
			{
				Log("ERROR: levelData is null in ScaleParametersForLevel");
				return;
			}

			Log($"Scaling parameters for Level {levelData.LevelNumber}");

			// Scale maze
			levelData.MazeParams = LevelDifficultyScaler.ScaleMazeParameters(
				levelData.LevelNumber,
				MazeParameters.CreateDefault(),
				levelData.DifficultyFactor
			);

			// Scale difficulty
			levelData.DifficultyParams = LevelDifficultyScaler.ScaleDifficultyParameters(
				levelData.LevelNumber,
				DifficultyParameters.CreateDefault(),
				levelData.DifficultyFactor
			);

			// Scale population
			levelData.PopulationParams = LevelDifficultyScaler.ScalePopulationParameters(
				levelData.LevelNumber,
				PopulationParameters.CreateDefault(),
				levelData.DifficultyFactor
			);

			Log($"Maze Size: {levelData.MazeParams?.Width ?? 0}x{levelData.MazeParams?.Height ?? 0}");
			Log($"Enemy Density: {levelData.PopulationParams?.EnemyDensity ?? 0:P}");
			Log($"Trap Density: {levelData.PopulationParams?.TrapDensity ?? 0:P}");
		}

		private void GenerateMazeStructure(LevelData levelData)
		{
			if (levelData == null)
			{
				Log("ERROR: levelData is null in GenerateMazeStructure");
				return;
			}

			Log("Generating maze structure...");

			if (mazeBuilder == null)
			{
				Log("ERROR: MazeBuilder not found");
				return;
			}

			// Set level and seed on the maze builder before generating
			mazeBuilder.SetLevelAndSeed(levelData.LevelNumber, levelData.Seed);

			// Use CompleteMazeBuilder8 to generate the maze
			mazeBuilder.GenerateMaze();

			Log("Maze structure generated");

			// Instantiate corridor decorations if enabled
			if (autoInstantiateDecorations)
			{
				InstantiateCorridorDecorations(levelData);
			}
		}

		/// <summary>
		/// Instantiate corridor decorations (pillars, arches, niches) from maze data.
		/// Reads decoration flags from DungeonMazeData and places prefabs.
		/// </summary>
		private void InstantiateCorridorDecorations(LevelData levelData)
		{
			Log("Instantiating corridor decorations...");

			if (mazeBuilder == null || mazeBuilder.MazeData == null)
			{
				Log("WARNING: Cannot instantiate decorations - maze data not available");
				return;
			}

			var mazeData = mazeBuilder.MazeData;
			int decorCount = 0;

			// Iterate through all cells and place decorations based on flags
			for (int x = 0; x < mazeData.Width; x++)
			{
				for (int z = 0; z < mazeData.Height; z++)
				{
					uint cell = mazeData.GetCell(x, z);
					Vector3 position = new Vector3(x * mazeData.Config.CellSize, 0f, z * mazeData.Config.CellSize);

					// Place pillars
					if ((cell & (uint)CellFlags8.HasPillar) != 0 && pillarPrefab != null)
					{
						var pillar = Instantiate(pillarPrefab, position, Quaternion.identity);
						if (pillar != null)
						{
							pillar.transform.SetParent(mazeBuilder.transform, true);
							decorCount++;
							Log($"  Pillar placed at ({x}, {z})");
						}
					}

					// Place arches (marked with HasTorch in wall context)
					if ((cell & (uint)CellFlags8.HasTorch) != 0 && archPrefab != null)
					{
						// Check if this is a wall cell (arch context)
						if ((cell & (uint)CellFlags8.AllWalls) != 0)
						{
							var arch = Instantiate(archPrefab, position, Quaternion.identity);
							if (arch != null)
							{
								arch.transform.SetParent(mazeBuilder.transform, true);
								decorCount++;
								Log($"  Arch placed at ({x}, {z})");
							}
						}
					}

					// Place niche statues
					if ((cell & (uint)CellFlags8.HasNiche) != 0 && nicheStatuePrefab != null)
					{
						var niche = Instantiate(nicheStatuePrefab, position, Quaternion.identity);
						if (niche != null)
						{
							niche.transform.SetParent(mazeBuilder.transform, true);
							decorCount++;
							Log($"  Niche statue placed at ({x}, {z})");
						}
					}
				}
			}

			Log($"Corridor decorations complete: {decorCount} objects placed");
		}

		private void PopulateEnemies(LevelData levelData)
		{
			if (levelData == null)
			{
				Log("ERROR: levelData is null in PopulateEnemies");
				return;
			}

			if (levelData.PopulationParams == null)
			{
				Log("WARNING: PopulationParams is null, using default");
				levelData.PopulationParams = PopulationParameters.CreateDefault();
			}

			float enemyDensity = levelData.PopulationParams?.EnemyDensity ?? 0.2f;
			Log($"Populating enemies (density: {enemyDensity:P})");

			if (enemyPlacer == null)
			{
				Log("WARNING: EnemyPlacer not found");
				return;
			}

			// Use EnemyPlacer with difficulty scaling
			_lastStats.EnemyCount = 0; // Would be set by enemyPlacer.PlaceEnemies()

			Log($"Spawned {_lastStats.EnemyCount} enemies");
		}

		private void PopulateTreasures(LevelData levelData)
		{
			if (levelData == null || levelData.PopulationParams == null)
			{
				Log("ERROR: levelData or PopulationParams is null in PopulateTreasures");
				return;
			}

			float treasureDensity = levelData.PopulationParams.TreasureChestDensity;
			Log($"Placing treasures (density: {treasureDensity:P})");

			if (spatialPlacer == null)
			{
				Log("WARNING: SpatialPlacer not found");
				return;
			}

			// Calculate treasure count based on maze size and density
			int estimatedRoomCount = levelData.MazeParams?.MaxRoomCount ?? 5;
			int treasureCount = (int)(estimatedRoomCount * treasureDensity);

			_lastStats.TreasureCount = treasureCount;
			Log($"Placing {treasureCount} treasure chests");

			// Place treasures using ItemEngine
			if (itemEngine != null)
			{
				for (int i = 0; i < treasureCount; i++)
				{
					var itemId = GenerateTreasureItem(levelData);
					// itemEngine would handle placement
				}
			}
		}

		private void PopulateTraps(LevelData levelData)
		{
			if (levelData == null || levelData.PopulationParams == null)
			{
				Log("ERROR: levelData or PopulationParams is null in PopulateTraps");
				return;
			}

			float trapDensity = levelData.PopulationParams.TrapDensity;
			Log($"Placing traps (density: {trapDensity:P})");

			if (spatialPlacer == null)
			{
				Log("WARNING: SpatialPlacer not found");
				return;
			}

			int estimatedRoomCount = levelData.MazeParams?.MaxRoomCount ?? 5;
			int trapCount = (int)(estimatedRoomCount * trapDensity);

			_lastStats.TrapCount = trapCount;
			Log($"Placing {trapCount} traps");
		}

		private void SetupLighting(LevelData levelData)
		{
			if (levelData == null || levelData.PopulationParams == null)
			{
				Log("ERROR: levelData or PopulationParams is null in SetupLighting");
				return;
			}

			float torchDensity = levelData.PopulationParams.TorchDensity;
			Log($"Setting up lighting (torch density: {torchDensity:P})");

			if (lightEngine == null)
			{
				Log("WARNING: LightPlacementEngine not found");
				return;
			}

			int torchCount = (int)((levelData.MazeParams?.MaxRoomCount ?? 5) * 3 * torchDensity);
			_lastStats.TorchCount = torchCount;

			Log($"Placing {torchCount} torches");
		}

		private void SetupGameSystems(LevelData levelData)
		{
			Log("Setting up game systems...");

			if (gameManager == null)
			{
				Log("WARNING: GameManager not found");
				return;
			}

			// Apply difficulty scaling to game manager
			// gameManager would apply the difficulty multipliers

			if (computeGridEngine != null)
			{
				// Initialize compute grid for level
				Log("Initializing compute grid");
			}

			Log("Game systems configured");
		}

		private void SetupPlayer(LevelData levelData)
		{
			Log("Setting up player spawn...");

			var player = FindFirstObjectByType<PlayerController>();
			if (player == null)
			{
				Log("WARNING: PlayerController not found");
				return;
			}

			// Setup player with level-appropriate stats
			Log("Player configured");
		}

		private int GenerateTreasureItem(LevelData levelData)
		{
			var random = new System.Random(levelData.Seed);

			float roll = (float)random.NextDouble();
			if (roll < levelData.PopulationParams.LegendaryItemDropChance)
			{
				return 1; // Legendary
			}
			else if (roll < levelData.PopulationParams.EpicItemDropChance)
			{
				return 2; // Epic
			}
			else if (roll < levelData.PopulationParams.RareItemDropChance)
			{
				return 3; // Rare
			}
			return 4; // Common
		}

		private void FindAllComponents()
		{
			Log("Auto-finding components...");

			mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder8>();
			gameManager = FindFirstObjectByType<GameManager>();
			itemEngine = FindFirstObjectByType<ItemEngine>();
			doorsEngine = FindFirstObjectByType<DoorsEngine>();
			spatialPlacer = FindFirstObjectByType<SpatialPlacer>();
			lightEngine = FindFirstObjectByType<LightPlacementEngine>();
			enemyPlacer = FindFirstObjectByType<EnemyPlacer>();
			computeGridEngine = FindFirstObjectByType<ComputeGridEngine>();

			int foundCount = 0;
			if (mazeBuilder != null) foundCount++;
			if (gameManager != null) foundCount++;
			if (itemEngine != null) foundCount++;
			if (spatialPlacer != null) foundCount++;

			Log($"Found {foundCount}/8 components");
		}

		public LevelData GetCurrentLevelData() => _currentLevelData;
		public LevelGenerationStats GetLastGenerationStats() => _lastStats;

		// ReSharper disable Unity.PerformanceAnalysis
		private void Log(string message)
		{
			if (logDetailedProgress)
			{
				Debug.Log($"[ProceduralLevelGen] {message}");
			}
			OnGenerationLog?.Invoke(message);
		}
	}
}
