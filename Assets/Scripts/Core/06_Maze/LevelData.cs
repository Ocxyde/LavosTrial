// Copyright (C) 2026 CodeDotLavos
// This file is part of PeuImporte.
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using System;
using UnityEngine;

namespace Code.Lavos.Core
{
	/// <summary>
	/// Represents a complete level configuration with all procedural parameters.
	/// Can be serialized to JSON and persisted in database.
	/// </summary>
	[System.Serializable]
	public class LevelData
	{
		public int LevelNumber;
		public int Seed;
		public string LevelName;
		public string LevelDescription;
		public float DifficultyFactor;
		
		public MazeParameters MazeParams;
		public DifficultyParameters DifficultyParams;
		public PopulationParameters PopulationParams;
		
		public DateTime CreatedAt;
		public DateTime LastGeneratedAt;
		public bool IsGenerated;
		
		public static LevelData CreateDefault(int levelNumber)
		{
			return new LevelData
			{
				LevelNumber = levelNumber,
				Seed = levelNumber * 12345,
				LevelName = $"Level {levelNumber}",
				LevelDescription = $"Procedurally generated level {levelNumber}",
				DifficultyFactor = 1.0f + (levelNumber * 0.25f),
				MazeParams = MazeParameters.CreateDefault(),
				DifficultyParams = DifficultyParameters.CreateDefault(),
				PopulationParams = PopulationParameters.CreateDefault(),
				CreatedAt = DateTime.Now,
				LastGeneratedAt = DateTime.MinValue,
				IsGenerated = false
			};
		}
	}

	/// <summary>
	/// Maze generation parameters scaled by level.
	/// </summary>
	[System.Serializable]
	public class MazeParameters
	{
		public int Width;
		public int Height;
		public int CellSize;
		public float WallHeight;
		public float WallThickness;
		public int SpawnRoomSize;
		public int MinRoomSize;
		public int MaxRoomSize;
		public int MinRoomCount;
		public int MaxRoomCount;
		public float CorridorRandomness;
		public bool EnableDiagonalWalls;
		public bool GeneratePerimeterCorridor;
		
		public static MazeParameters CreateDefault()
		{
			return new MazeParameters
			{
				Width = 21,
				Height = 21,
				CellSize = 6,
				WallHeight = 4.0f,
				WallThickness = 0.5f,
				SpawnRoomSize = 5,
				MinRoomSize = 3,
				MaxRoomSize = 8,
				MinRoomCount = 3,
				MaxRoomCount = 8,
				CorridorRandomness = 0.3f,
				EnableDiagonalWalls = true,
				GeneratePerimeterCorridor = true
			};
		}
		
		public MazeParameters Clone()
		{
			return new MazeParameters
			{
				Width = this.Width,
				Height = this.Height,
				CellSize = this.CellSize,
				WallHeight = this.WallHeight,
				WallThickness = this.WallThickness,
				SpawnRoomSize = this.SpawnRoomSize,
				MinRoomSize = this.MinRoomSize,
				MaxRoomSize = this.MaxRoomSize,
				MinRoomCount = this.MinRoomCount,
				MaxRoomCount = this.MaxRoomCount,
				CorridorRandomness = this.CorridorRandomness,
				EnableDiagonalWalls = this.EnableDiagonalWalls,
				GeneratePerimeterCorridor = this.GeneratePerimeterCorridor
			};
		}
	}

	/// <summary>
	/// Difficulty scaling parameters with exponential growth.
	/// </summary>
	[System.Serializable]
	public class DifficultyParameters
	{
		public float BaseFactor;
		public float ExponentPerLevel;
		public float DamageMultiplier;
		public float HealthMultiplier;
		public float EnemyHealthMultiplier;
		public float EnemyDamageMultiplier;
		public float SpeedMultiplier;
		public float StaminaDrainMultiplier;
		public float TrapDensity;
		public float LockedDoorChance;
		public float SecretDoorChance;
		
		public static DifficultyParameters CreateDefault()
		{
			return new DifficultyParameters
			{
				BaseFactor = 1.0f,
				ExponentPerLevel = 0.3f,
				DamageMultiplier = 1.0f,
				HealthMultiplier = 1.0f,
				EnemyHealthMultiplier = 1.0f,
				EnemyDamageMultiplier = 1.0f,
				SpeedMultiplier = 1.0f,
				StaminaDrainMultiplier = 1.0f,
				TrapDensity = 0.1f,
				LockedDoorChance = 0.2f,
				SecretDoorChance = 0.05f
			};
		}
	}

	/// <summary>
	/// Enemy, treasure, and object population parameters.
	/// </summary>
	[System.Serializable]
	public class PopulationParameters
	{
		public float EnemyDensity;
		public float TreasureChestDensity;
		public float TorchDensity;
		public float TrapDensity;
		public float LegendaryItemDropChance;
		public float EpicItemDropChance;
		public float RareItemDropChance;
		public bool EnableBossRoom;
		public bool EnableEnvironmentalHazards;
		public bool EnableStatusEffects;
		public int MaxEnemiesPerRoom;
		
		public static PopulationParameters CreateDefault()
		{
			return new PopulationParameters
			{
				EnemyDensity = 0.2f,
				TreasureChestDensity = 0.15f,
				TorchDensity = 0.6f,
				TrapDensity = 0.1f,
				LegendaryItemDropChance = 0.01f,
				EpicItemDropChance = 0.05f,
				RareItemDropChance = 0.15f,
				EnableBossRoom = false,
				EnableEnvironmentalHazards = false,
				EnableStatusEffects = false,
				MaxEnemiesPerRoom = 3
			};
		}
	}

	/// <summary>
	/// Calculates difficulty scaling with exponential growth function.
	/// Formula: difficulty = base * exp(exponent * level)
	/// </summary>
	public static class LevelDifficultyScaler
	{
		/// <summary>
		/// Calculate exponential difficulty factor for a level.
		/// </summary>
		public static float CalculateDifficultyFactor(int levelNumber, float baseExponent = 0.3f)
		{
			if (levelNumber < 1) levelNumber = 1;
			return 1.0f + Mathf.Exp(baseExponent * levelNumber);
		}

		/// <summary>
		/// Scale maze parameters based on level and base parameters.
		/// </summary>
		public static MazeParameters ScaleMazeParameters(
			int levelNumber,
			MazeParameters baseParams,
			float difficultyFactor)
		{
			var scaled = baseParams.Clone();
			
			// Exponential growth of maze size
			float sizeGrowth = 1.0f + (0.15f * levelNumber);
			scaled.Width = (int)(baseParams.Width * sizeGrowth);
			scaled.Height = (int)(baseParams.Height * sizeGrowth);
			
			// Clamp to reasonable bounds
			scaled.Width = Mathf.Clamp(scaled.Width, 15, 128);
			scaled.Height = Mathf.Clamp(scaled.Height, 15, 128);
			
			// More rooms at higher levels
			float roomGrowth = 1.0f + (0.25f * levelNumber);
			scaled.MinRoomCount = (int)(baseParams.MinRoomCount * roomGrowth);
			scaled.MaxRoomCount = (int)(baseParams.MaxRoomCount * roomGrowth);
			
			// Slightly larger rooms
			scaled.MaxRoomSize = (int)(baseParams.MaxRoomSize * (1.0f + 0.1f * levelNumber));
			
			// More random corridors at higher levels
			scaled.CorridorRandomness = Mathf.Clamp01(
				baseParams.CorridorRandomness + (0.05f * levelNumber)
			);
			
			// Enable diagonal walls at level 2+
			if (levelNumber >= 2)
			{
				scaled.EnableDiagonalWalls = true;
			}
			
			return scaled;
		}

		/// <summary>
		/// Scale difficulty parameters exponentially.
		/// </summary>
		public static DifficultyParameters ScaleDifficultyParameters(
			int levelNumber,
			DifficultyParameters baseParams,
			float difficultyFactor)
		{
			var scaled = new DifficultyParameters
			{
				BaseFactor = difficultyFactor,
				ExponentPerLevel = baseParams.ExponentPerLevel,
				DamageMultiplier = baseParams.DamageMultiplier * difficultyFactor,
				HealthMultiplier = baseParams.HealthMultiplier * (0.8f + difficultyFactor * 0.5f),
				EnemyHealthMultiplier = baseParams.EnemyHealthMultiplier * difficultyFactor,
				EnemyDamageMultiplier = baseParams.EnemyDamageMultiplier * difficultyFactor,
				SpeedMultiplier = Mathf.Clamp(baseParams.SpeedMultiplier * (1.0f + 0.05f * levelNumber), 0.8f, 1.5f),
				StaminaDrainMultiplier = baseParams.StaminaDrainMultiplier * (1.0f + 0.1f * levelNumber),
				TrapDensity = Mathf.Clamp01(baseParams.TrapDensity + (0.05f * levelNumber)),
				LockedDoorChance = Mathf.Clamp01(baseParams.LockedDoorChance + (0.05f * levelNumber)),
				SecretDoorChance = Mathf.Clamp01(baseParams.SecretDoorChance + (0.02f * levelNumber))
			};
			
			return scaled;
		}

		/// <summary>
		/// Scale population parameters with difficulty.
		/// </summary>
		public static PopulationParameters ScalePopulationParameters(
			int levelNumber,
			PopulationParameters baseParams,
			float difficultyFactor)
		{
			var scaled = new PopulationParameters
			{
				EnemyDensity = Mathf.Clamp01(baseParams.EnemyDensity + (0.05f * levelNumber)),
				TreasureChestDensity = Mathf.Clamp01(baseParams.TreasureChestDensity + (0.02f * levelNumber)),
				TorchDensity = Mathf.Clamp01(baseParams.TorchDensity - (0.02f * levelNumber)),
				TrapDensity = Mathf.Clamp01(baseParams.TrapDensity + (0.05f * levelNumber)),
				LegendaryItemDropChance = baseParams.LegendaryItemDropChance + (0.005f * levelNumber),
				EpicItemDropChance = baseParams.EpicItemDropChance + (0.01f * levelNumber),
				RareItemDropChance = baseParams.RareItemDropChance + (0.02f * levelNumber),
				EnableBossRoom = (levelNumber >= 5) || baseParams.EnableBossRoom,
				EnableEnvironmentalHazards = (levelNumber >= 3) || baseParams.EnableEnvironmentalHazards,
				EnableStatusEffects = (levelNumber >= 2) || baseParams.EnableStatusEffects,
				MaxEnemiesPerRoom = (int)(baseParams.MaxEnemiesPerRoom * (1.0f + 0.25f * levelNumber))
			};
			
			return scaled;
		}
	}

	/// <summary>
	/// Container for level generation statistics.
	/// </summary>
	[System.Serializable]
	public class LevelGenerationStats
	{
		public int LevelNumber;
		public int Seed;
		public float GenerationTimeMs;
		public int RoomCount;
		public int WallCount;
		public int DoorCount;
		public int LockedDoorCount;
		public int SecretDoorCount;
		public int EnemyCount;
		public int TreasureCount;
		public int TrapCount;
		public int TorchCount;
		public int TotalObjectCount;
		public float DifficultyFactor;
		public DateTime GeneratedAt;
		
		public int GetTotalCount() => RoomCount + WallCount + DoorCount + EnemyCount + TreasureCount + TrapCount + TorchCount;
	}
}
