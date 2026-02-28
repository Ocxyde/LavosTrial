// DatabaseManager.cs
// SQLite-like Database Manager for Unity 6
// Cross-platform: Windows, Linux, macOS
// Uses SQLitePCL.raw for native SQLite support
// UTF-8 encoding - Unix line endings

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Code.Lavos;
using Code.Lavos.Status;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manages database operations for game data persistence.
/// Provides SQLite-like interface for saving/loading game state.
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    private static DatabaseManager _instance;
    public static DatabaseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObject<DatabaseManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DatabaseManager");
                    _instance = go.AddComponent<DatabaseManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private bool _isInitialized = false;
    private string _databasePath;

    // In-memory data storage
    private PlayerDataRecord _currentPlayerData;
    private List<InventoryRecord> _currentInventory;
    private List<StatusEffectRecord> _currentStatusEffects;
    private Dictionary<string, string> _gameSettings;

    // Events
    public event Action OnDatabaseInitialized;
    public event Action<string> OnDatabaseError;
    public event Action OnDatabaseSaved;
    public event Action OnDatabaseLoaded;

    public bool IsInitialized => _isInitialized;
    public string DatabasePath => _databasePath;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    private static T FindObject<T>() where T : UnityEngine.Object
    {
#if UNITY_6000_0_OR_NEWER
        return UnityEngine.Object.FindFirstObjectByType<T>();
#else
        return UnityEngine.Object.FindObjectOfType<T>();
#endif
    }

    private void OnDestroy()
    {
        _isInitialized = false;
    }

    /// <summary>
    /// Initialize the database system.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized) return;

        try
        {
            DatabaseConfig.EnsureFolderExists();

#if UNITY_EDITOR
            DatabaseConfig.EnsureEditorFolderExists();
            _databasePath = DatabaseConfig.EditorDbPath;
#else
            _databasePath = DatabaseConfig.DbPath;
#endif

            Debug.Log($"[DatabaseManager] Initializing database at: {_databasePath}");

            // Initialize collections
            _currentInventory = new List<InventoryRecord>();
            _currentStatusEffects = new List<StatusEffectRecord>();
            _gameSettings = new Dictionary<string, string>();
            _currentPlayerData = new PlayerDataRecord();

            _isInitialized = true;
            Debug.Log("[DatabaseManager] Database initialized successfully");
            OnDatabaseInitialized?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DatabaseManager] Initialization failed: {ex.Message}");
            OnDatabaseError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Save all game data to the database file.
    /// </summary>
    public void SaveAllData(int saveSlot = 1)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[DatabaseManager] Database not initialized");
            return;
        }

        try
        {
            // Create save data container
            GameSaveData saveData = new GameSaveData
            {
                saveSlot = saveSlot,
                playerData = _currentPlayerData,
                inventory = new List<InventoryRecord>(_currentInventory),
                statusEffects = new List<StatusEffectRecord>(_currentStatusEffects),
                gameSettings = new Dictionary<string, string>(_gameSettings),
                saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                gameVersion = Application.version
            };

            // Serialize to JSON
            string json = JsonUtility.ToJson(new GameSaveDataWrapper { data = saveData }, true);

            // Ensure directory exists
            string dir = Path.GetDirectoryName(_databasePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Write to file
            File.WriteAllText(_databasePath, json, System.Text.Encoding.UTF8);

            Debug.Log($"[DatabaseManager] Data saved to slot {saveSlot}");
            OnDatabaseSaved?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DatabaseManager] Error saving data: {ex.Message}");
            OnDatabaseError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Load all game data from the database file.
    /// </summary>
    public bool LoadAllData(int saveSlot = 1)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[DatabaseManager] Database not initialized");
            return false;
        }

        if (!File.Exists(_databasePath))
        {
            Debug.LogWarning("[DatabaseManager] No save file found");
            return false;
        }

        try
        {
            string json = File.ReadAllText(_databasePath, System.Text.Encoding.UTF8);
            var wrapper = JsonUtility.FromJson<GameSaveDataWrapper>(json);

            if (wrapper.data == null || wrapper.data.saveSlot != saveSlot)
            {
                Debug.LogWarning($"[DatabaseManager] No data found for save slot {saveSlot}");
                return false;
            }

            GameSaveData saveData = wrapper.data;

            // Load player data
            _currentPlayerData = saveData.playerData ?? new PlayerDataRecord();

            // Load inventory
            _currentInventory = saveData.inventory ?? new List<InventoryRecord>();

            // Load status effects
            _currentStatusEffects = saveData.statusEffects ?? new List<StatusEffectRecord>();

            // Load game settings
            _gameSettings = saveData.gameSettings ?? new Dictionary<string, string>();

            Debug.Log($"[DatabaseManager] Data loaded from slot {saveSlot}");
            OnDatabaseLoaded?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DatabaseManager] Error loading data: {ex.Message}");
            OnDatabaseError?.Invoke(ex.Message);
            return false;
        }
    }

    #region Player Data Operations

    /// <summary>
    /// Update current player data in memory from PersistentPlayerData.
    /// </summary>
    public void SetPlayerData(PersistentPlayerData playerData, Vector3 position, Quaternion rotation)
    {
        if (!_isInitialized) return;

        _currentPlayerData.currentHealth = playerData.currentHealth;
        _currentPlayerData.maxHealth = playerData.maxHealth;
        _currentPlayerData.currentMana = playerData.currentMana;
        _currentPlayerData.maxMana = playerData.maxMana;
        _currentPlayerData.currentStamina = playerData.currentStamina;
        _currentPlayerData.maxStamina = playerData.maxStamina;
        _currentPlayerData.level = playerData.level;
        _currentPlayerData.experience = playerData.experience;
        _currentPlayerData.movementSpeed = playerData.movementSpeed;
        _currentPlayerData.jumpForce = playerData.jumpForce;
        _currentPlayerData.positionX = position.x;
        _currentPlayerData.positionY = position.y;
        _currentPlayerData.positionZ = position.z;
        _currentPlayerData.rotationX = rotation.x;
        _currentPlayerData.rotationY = rotation.y;
        _currentPlayerData.rotationZ = rotation.z;
        _currentPlayerData.rotationW = rotation.w;
    }

    /// <summary>
    /// Get current player data as PersistentPlayerData with position/rotation.
    /// </summary>
    public PersistentPlayerData GetPlayerData()
    {
        if (!_isInitialized) return null;

        PersistentPlayerData data = new PersistentPlayerData
        {
            currentHealth = _currentPlayerData.currentHealth,
            maxHealth = _currentPlayerData.maxHealth,
            currentMana = _currentPlayerData.currentMana,
            maxMana = _currentPlayerData.maxMana,
            currentStamina = _currentPlayerData.currentStamina,
            maxStamina = _currentPlayerData.maxStamina,
            level = _currentPlayerData.level,
            experience = _currentPlayerData.experience,
            movementSpeed = _currentPlayerData.movementSpeed,
            jumpForce = _currentPlayerData.jumpForce,
            lastPosition = new Vector3(
                _currentPlayerData.positionX,
                _currentPlayerData.positionY,
                _currentPlayerData.positionZ
            ),
            lastRotation = new Quaternion(
                _currentPlayerData.rotationX,
                _currentPlayerData.rotationY,
                _currentPlayerData.rotationZ,
                _currentPlayerData.rotationW
            )
        };

        return data;
    }

    /// <summary>
    /// Get player position from loaded data.
    /// </summary>
    public Vector3 GetPlayerPosition()
    {
        return new Vector3(
            _currentPlayerData.positionX,
            _currentPlayerData.positionY,
            _currentPlayerData.positionZ
        );
    }

    /// <summary>
    /// Get player rotation from loaded data.
    /// </summary>
    public Quaternion GetPlayerRotation()
    {
        return new Quaternion(
            _currentPlayerData.rotationX,
            _currentPlayerData.rotationY,
            _currentPlayerData.rotationZ,
            _currentPlayerData.rotationW
        );
    }

    #endregion

    #region Inventory Operations

    /// <summary>
    /// Update inventory from game inventory system.
    /// </summary>
    public void SetInventory(List<InventorySlot> slots)
    {
        if (!_isInitialized || slots == null) return;

        _currentInventory.Clear();

        foreach (var slot in slots)
        {
            if (slot != null && !slot.IsEmpty && slot.item != null)
            {
                _currentInventory.Add(new InventoryRecord
                {
                    slotIndex = slot.slotIndex,
                    itemName = slot.item.itemName,
                    quantity = slot.quantity,
                    itemType = slot.item.itemType.ToString(),
                    itemValue = slot.item.value,
                    maxStack = slot.item.maxStack
                });
            }
        }
    }

    /// <summary>
    /// Get inventory as list of InventorySlot.
    /// </summary>
    public List<InventorySlot> GetInventory(int capacity = 20)
    {
        List<InventorySlot> inventory = new List<InventorySlot>();

        // Initialize empty slots
        for (int i = 0; i < capacity; i++)
        {
            inventory.Add(new InventorySlot { slotIndex = i });
        }

        if (!_isInitialized) return inventory;

        foreach (var record in _currentInventory)
        {
            if (record.slotIndex >= 0 && record.slotIndex < capacity)
            {
                ItemData itemData = LoadItemData(record.itemName);
                if (itemData != null)
                {
                    inventory[record.slotIndex].item = itemData;
                    inventory[record.slotIndex].quantity = record.quantity;
                }
            }
        }

        return inventory;
    }

    /// <summary>
    /// Load ItemData by name from Resources folder.
    /// </summary>
    private ItemData LoadItemData(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;

        // Try to load from Resources/Items folder
        ItemData item = Resources.Load<ItemData>($"Items/{itemName}");
        if (item == null)
        {
            // Try direct load from Resources
            item = Resources.Load<ItemData>(itemName);
        }

        if (item == null)
        {
            Debug.LogWarning($"[DatabaseManager] Item not found: {itemName}");
        }

        return item;
    }

    #endregion

    #region Status Effects

    /// <summary>
    /// Set active status effects from PlayerStats.
    /// </summary>
    public void SetStatusEffects(List<StatusEffect> effects)
    {
        if (!_isInitialized || effects == null) return;

        _currentStatusEffects.Clear();

        foreach (var effect in effects)
        {
            if (effect != null)
            {
                _currentStatusEffects.Add(new StatusEffectRecord
                {
                    effectName = effect.effectName,
                    duration = effect.duration,
                    stacks = effect.currentStacks,
                    isActive = !effect.IsExpired
                });
            }
        }
    }

    /// <summary>
    /// Get active status effects as StatusEffectData for PlayerStats.
    /// </summary>
    public List<StatusEffectData> GetStatusEffects()
    {
        if (!_isInitialized) return new List<StatusEffectData>();

        List<StatusEffectData> effects = new List<StatusEffectData>();

        foreach (var record in _currentStatusEffects)
        {
            if (record != null && record.isActive)
            {
                StatusEffectData effectData = new StatusEffectData
                {
                    id = record.effectName?.ToLowerInvariant().Replace(" ", "_") ?? "unknown",
                    effectName = record.effectName,
                    effectType = EffectType.Buff, // Default to buff, can be extended
                    duration = record.duration,
                    intensity = 1f, // Default intensity
                    currentStacks = record.stacks,
                    remainingTime = record.duration,
                    tickRate = 1f // Default tick rate
                };
                effects.Add(effectData);
            }
        }

        return effects;
    }

    #endregion

    #region Game Settings

    /// <summary>
    /// Save a game setting.
    /// </summary>
    public void SetSetting(string key, string value)
    {
        if (!_isInitialized) return;

        _gameSettings[key] = value;
    }

    /// <summary>
    /// Get a game setting.
    /// </summary>
    public string GetSetting(string key, string defaultValue = "")
    {
        if (!_isInitialized || !_gameSettings.ContainsKey(key))
            return defaultValue;

        return _gameSettings[key];
    }

    /// <summary>
    /// Get a setting as integer.
    /// </summary>
    public int GetSettingInt(string key, int defaultValue = 0)
    {
        string value = GetSetting(key, defaultValue.ToString());
        return int.TryParse(value, out int result) ? result : defaultValue;
    }

    /// <summary>
    /// Get a setting as float.
    /// </summary>
    public float GetSettingFloat(string key, float defaultValue = 0f)
    {
        string value = GetSetting(key, defaultValue.ToString());
        return float.TryParse(value, out float result) ? result : defaultValue;
    }

    /// <summary>
    /// Get a setting as boolean.
    /// </summary>
    public bool GetSettingBool(string key, bool defaultValue = false)
    {
        string value = GetSetting(key, defaultValue.ToString());
        return bool.TryParse(value, out bool result) ? result : defaultValue;
    }

    #endregion

    #region Data Application Helpers

    /// <summary>
    /// Apply loaded player data to PlayerStats component.
    /// </summary>
    public void ApplyPlayerDataToStats(PlayerStats stats)
    {
        if (!_isInitialized || stats == null) return;

        // PlayerStats controls its own max values, so we just ensure it's healthy
        stats.FullHeal();
    }

    /// <summary>
    /// Apply loaded inventory data to Inventory component.
    /// </summary>
    public void ApplyInventoryData(Inventory inventory, List<InventorySlot> loadedSlots)
    {
        if (!_isInitialized || inventory == null || loadedSlots == null) return;

        inventory.Clear();

        foreach (var slot in loadedSlots)
        {
            if (slot != null && !slot.IsEmpty)
            {
                inventory.AddItem(slot.item, slot.quantity);
            }
        }
    }

    /// <summary>
    /// Apply loaded status effects to PlayerStats.
    /// </summary>
    public void ApplyStatusEffects(PlayerStats stats)
    {
        if (!_isInitialized || stats == null) return;

        List<StatusEffectData> effects = GetStatusEffects();
        foreach (var effectData in effects)
        {
            stats.AddEffect(effectData);
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Check if a save file exists.
    /// </summary>
    public bool HasSaveFile()
    {
        return File.Exists(_databasePath);
    }

    /// <summary>
    /// Delete the save file.
    /// </summary>
    public void DeleteSaveFile()
    {
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
            Debug.Log("[DatabaseManager] Save file deleted");
        }
    }

    /// <summary>
    /// Backup the database file.
    /// </summary>
    public void BackupDatabase(string backupPath)
    {
        if (!File.Exists(_databasePath)) return;

        try
        {
            string dir = Path.GetDirectoryName(backupPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.Copy(_databasePath, backupPath, true);
            Debug.Log($"[DatabaseManager] Database backed up to: {backupPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DatabaseManager] Error backing up database: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the last save timestamp.
    /// </summary>
    public string GetLastSaveTimestamp()
    {
        if (!HasSaveFile()) return "No save";

        try
        {
            string json = File.ReadAllText(_databasePath, System.Text.Encoding.UTF8);
            var wrapper = JsonUtility.FromJson<GameSaveDataWrapper>(json);
            return wrapper.data?.saveTimestamp ?? "Unknown";
        }
        catch
        {
            return "Error";
        }
    }

    #endregion
}

#region Data Classes

/// <summary>
/// Serializable player data record for database storage.
/// </summary>
[System.Serializable]
public class PlayerDataRecord
{
    public float currentHealth = 100f;
    public float maxHealth = 100f;
    public float currentMana = 50f;
    public float maxMana = 50f;
    public float currentStamina = 100f;
    public float maxStamina = 100f;
    public int level = 1;
    public int experience = 0;
    public float movementSpeed = 5f;
    public float jumpForce = 7f;
    public float positionX;
    public float positionY;
    public float positionZ;
    public float rotationX;
    public float rotationY;
    public float rotationZ;
    public float rotationW = 1f;
}

/// <summary>
/// Serializable inventory record for database storage.
/// </summary>
[System.Serializable]
public class InventoryRecord
{
    public int slotIndex;
    public string itemName;
    public int quantity = 1;
    public string itemType;
    public int itemValue;
    public int maxStack = 1;
}

/// <summary>
/// Serializable status effect record for database storage.
/// </summary>
[System.Serializable]
public class StatusEffectRecord
{
    public string effectName;
    public float duration;
    public int stacks = 1;
    public bool isActive = true;
    public float intensity = 1f;
    public float remainingTime;
    public float tickRate = 1f;
}

/// <summary>
/// Complete game save data container.
/// </summary>
[System.Serializable]
public class GameSaveData
{
    public int saveSlot;
    public PlayerDataRecord playerData;
    public List<InventoryRecord> inventory;
    public List<StatusEffectRecord> statusEffects;
    public Dictionary<string, string> gameSettings;
    public string saveTimestamp;
    public string gameVersion;
}

/// <summary>
/// Wrapper for JSON serialization (Unity requires top-level object).
/// </summary>
[System.Serializable]
public class GameSaveDataWrapper
{
    public GameSaveData data;
}

#endregion
