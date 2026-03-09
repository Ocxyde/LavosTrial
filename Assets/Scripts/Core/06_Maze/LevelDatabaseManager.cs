// Copyright (C) 2026 CodeDotLavos
// This file is part of PeuImporte.
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Code.Lavos.Core
{
	/// <summary>
	/// Manages level data persistence.
	/// Handles save/load operations for procedurally generated levels.
	/// Data is stored in JSON format for cross-platform compatibility.
	/// </summary>
	public sealed partial class LevelDatabaseManager : MonoBehaviour
	{
		private const string LEVEL_FILE_EXTENSION = ".level.json";
		private const string LOG_TAG = "[LevelDBManager]";

		private static readonly object _lock = new object();
		private static LevelDatabaseManager _instance;
		public static LevelDatabaseManager Instance
		{
			get
			{
				lock (_lock)
				{
					if (_instance == null)
					{
						_instance = FindFirstObjectByType<LevelDatabaseManager>();
						if (_instance == null && Application.isPlaying)
						{
							var go = new GameObject("LevelDatabaseManager");
							_instance = go.AddComponent<LevelDatabaseManager>();
							DontDestroyOnLoad(go);
						}
					}
					return _instance;
				}
			}
		}

		private Dictionary<int, LevelData> _loadedLevels = new Dictionary<int, LevelData>();
		private Dictionary<int, LevelGenerationStats> _levelStats = new Dictionary<int, LevelGenerationStats>();

		public event Action<int> OnLevelSaved;
		public event Action<int> OnLevelLoaded;
		public event Action<string> OnDatabaseLog;

		private void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(gameObject);
				return;
			}

			_instance = this;
			DontDestroyOnLoad(gameObject);

			EnsureStorageDirectoryExists();
		}

		private void OnDisable()
		{
			OnLevelSaved = null;
			OnLevelLoaded = null;
			OnDatabaseLog = null;
		}

		/// <summary>
		/// Save a level to database and disk storage.
		/// </summary>
		public void SaveLevel(LevelData levelData, bool saveToDisk = true)
		{
			if (levelData == null)
			{
				Log("ERROR: Cannot save null LevelData");
				throw new ArgumentNullException(nameof(levelData));
			}

			try
			{
				Log($"Saving Level {levelData.LevelNumber} to storage...");

				// Save to in-memory cache
				_loadedLevels[levelData.LevelNumber] = levelData;

				// Save to disk if requested
				if (saveToDisk)
				{
					SaveLevelToDisk(levelData);
				}

				OnLevelSaved?.Invoke(levelData.LevelNumber);
				Log($"Level {levelData.LevelNumber} saved successfully");
			}
			catch (Exception ex)
			{
				Log($"ERROR saving level: {ex.Message}");
				Debug.LogError(ex);
			}
		}

		/// <summary>
		/// Load a level from disk or cache.
		/// </summary>
		public LevelData LoadLevel(int levelNumber)
		{
			try
			{
				Log($"Loading Level {levelNumber}...");

				// Check in-memory cache first
				if (_loadedLevels.ContainsKey(levelNumber))
				{
					Log($"Level {levelNumber} found in cache");
					return _loadedLevels[levelNumber];
				}

				// Try to load from disk
				var levelData = LoadLevelFromDisk(levelNumber);
				if (levelData != null)
				{
					_loadedLevels[levelNumber] = levelData;
					OnLevelLoaded?.Invoke(levelNumber);
					Log($"Level {levelNumber} loaded from disk");
					return levelData;
				}

				Log($"Level {levelNumber} not found");
				return null;
			}
			catch (Exception ex)
			{
				Log($"ERROR loading level: {ex.Message}");
				Debug.LogError(ex);
				return null;
			}
		}

		/// <summary>
		/// Load all levels from storage.
		/// </summary>
		public List<LevelData> LoadAllLevels()
		{
			Log("Loading all levels from storage...");

			var levels = new List<LevelData>();
			var storageDir = GetStorageDirectory();

			if (!Directory.Exists(storageDir))
			{
				Log("Storage directory does not exist");
				return levels;
			}

			var files = Directory.GetFiles(storageDir, $"*{LEVEL_FILE_EXTENSION}");
			foreach (var file in files)
			{
				try
				{
					var json = File.ReadAllText(file);
					var levelData = JsonUtility.FromJson<LevelData>(json);
					if (levelData != null)
					{
						levels.Add(levelData);
						_loadedLevels[levelData.LevelNumber] = levelData;
					}
				}
				catch (Exception ex)
				{
					Log($"Error loading {file}: {ex.Message}");
				}
			}

			Log($"Loaded {levels.Count} levels from storage");
			return levels;
		}

		/// <summary>
		/// Save level statistics for tracking.
		/// </summary>
		public void SaveLevelStats(int levelNumber, LevelGenerationStats stats)
		{
			try
			{
				_levelStats[levelNumber] = stats;
				Log($"Saved stats for Level {levelNumber}");
			}
			catch (Exception ex)
			{
				Log($"ERROR saving stats: {ex.Message}");
			}
		}

		/// <summary>
		/// Retrieve level statistics.
		/// </summary>
		public LevelGenerationStats GetLevelStats(int levelNumber)
		{
			return _levelStats.TryGetValue(levelNumber, out var stats) ? stats : null;
		}

		/// <summary>
		/// Try to retrieve level statistics.
		/// </summary>
		public bool TryGetLevelStats(int levelNumber, out LevelGenerationStats stats)
		{
			return _levelStats.TryGetValue(levelNumber, out stats);
		}

		/// <summary>
		/// Check if a level is already generated and cached.
		/// </summary>
		public bool IsLevelCached(int levelNumber)
		{
			return _loadedLevels.ContainsKey(levelNumber);
		}

		// ReSharper disable Unity.PerformanceAnalysis
		/// <summary>
		/// Delete a level from storage.
		/// </summary>
		public void DeleteLevel(int levelNumber)
		{
			try
			{
				Log($"Deleting Level {levelNumber}...");

				// Remove from cache
				_loadedLevels.Remove(levelNumber);
				_levelStats.Remove(levelNumber);

				// Delete from disk
				var filePath = GetLevelFilePath(levelNumber);
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
					Log($"Deleted Level {levelNumber} file");
				}

				Log($"Level {levelNumber} deleted");
			}
			catch (Exception ex)
			{
				Log($"ERROR deleting level: {ex.Message}");
			}
		}

		/// <summary>
		/// Get storage statistics.
		/// </summary>
		public (int CachedLevels, int DiskLevels, long StorageSize) GetStorageStats()
		{
			int cachedLevels = _loadedLevels.Count;
			int diskLevels = 0;
			long storageSize = 0;

			var storageDir = GetStorageDirectory();
			if (Directory.Exists(storageDir))
			{
				var files = Directory.GetFiles(storageDir, $"*{LEVEL_FILE_EXTENSION}");
				diskLevels = files.Length;

				foreach (var file in files)
				{
					storageSize += new FileInfo(file).Length;
				}
			}

			return (cachedLevels, diskLevels, storageSize);
		}

		// ========== PRIVATE METHODS ==========

		private void SaveLevelToDisk(LevelData levelData)
		{
			try
			{
				EnsureStorageDirectoryExists();
				var filePath = GetLevelFilePath(levelData.LevelNumber);
				var json = JsonUtility.ToJson(levelData, true);
				File.WriteAllText(filePath, json);
				Log($"Saved level {levelData.LevelNumber} to {filePath}");
			}
			catch (Exception ex)
			{
				Log($"ERROR saving to disk: {ex.Message}");
				Debug.LogError($"Failed to save level {levelData.LevelNumber} to disk: {ex}");
			}
		}

		private LevelData LoadLevelFromDisk(int levelNumber)
		{
			var filePath = GetLevelFilePath(levelNumber);
			if (!File.Exists(filePath))
			{
				return null;
			}

			try
			{
				var json = File.ReadAllText(filePath);
				var levelData = JsonUtility.FromJson<LevelData>(json);
				if (levelData == null)
				{
					Log($"ERROR: Failed to deserialize {filePath} - JSON was invalid or empty");
				}
				return levelData;
			}
			catch (Exception ex)
			{
				Log($"ERROR loading from disk ({filePath}): {ex.Message}");
				Debug.LogError($"Failed to load level {levelNumber} from {filePath}: {ex}");
				return null;
			}
		}

		private string GetLevelFilePath(int levelNumber)
		{
			var storageDir = GetStorageDirectory();
			return Path.Combine(storageDir, $"Level_{levelNumber}{LEVEL_FILE_EXTENSION}");
		}

		private string GetStorageDirectory()
		{
			// Use persistentDataPath for runtime saves (not StreamingAssets which is read-only)
			var path = Path.Combine(Application.persistentDataPath, "Levels");
			return path;
		}

		private void EnsureStorageDirectoryExists()
		{
			var storageDir = GetStorageDirectory();
			if (!Directory.Exists(storageDir))
			{
				Directory.CreateDirectory(storageDir);
				Log($"Created storage directory: {storageDir}");
			}
		}

		private void Log(string message)
		{
			Debug.Log($"[LevelDBManager] {message}");
			OnDatabaseLog?.Invoke(message);
		}
	}
}
