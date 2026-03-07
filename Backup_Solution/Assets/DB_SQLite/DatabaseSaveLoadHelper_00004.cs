// DatabaseSaveLoadHelper.cs
// Helper utilities for save/load operations in Unity 6
// Cross-platform: Windows, Linux, macOS
// UTF-8 encoding - Unix line endings

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Code.Lavos;
using Code.Lavos.Core;

namespace Code.Lavos.DB
{
    /// <summary>
    /// Provides convenient save/load operations with progress tracking.
    /// Attach to a GameObject or use via DatabaseManager.Instance.
    /// </summary>
    public class DatabaseSaveLoadHelper : MonoBehaviour
{
    private static DatabaseSaveLoadHelper _instance;
    public static DatabaseSaveLoadHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("DatabaseSaveLoadHelper");
                _instance = go.AddComponent<DatabaseSaveLoadHelper>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public bool IsSaving { get; private set; }
    public bool IsLoading { get; private set; }
    public float SaveProgress { get; private set; }
    public float LoadProgress { get; private set; }

    public System.Action<float> OnSaveProgressChanged;
    public System.Action<float> OnLoadProgressChanged;
    public System.Action<bool, string> OnSaveCompleted;
    public System.Action<bool, string> OnLoadCompleted;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Convert IReadOnlyList to List for database operations.
    /// </summary>
    private List<InventorySlot> ConvertInventorySlots(System.Collections.Generic.IReadOnlyList<InventorySlot> slots)
    {
        if (slots == null) return null;
        return new List<InventorySlot>(slots);
    }

    /// <summary>
    /// Save game data with progress tracking.
    /// </summary>
    /// <param name="saveSlot">Save slot number (1-3 recommended)</param>
    /// <param name="player">Player GameObject with PersistentPlayerData</param>
    /// <param name="inventory">Inventory component</param>
    public void SaveGame(int saveSlot, GameObject player = null, Inventory inventory = null)
    {
        if (IsSaving)
        {
            Debug.LogWarning("[DatabaseSaveLoadHelper] Save already in progress");
            return;
        }

        StartCoroutine(SaveGameCoroutine(saveSlot, player, inventory));
    }

    private IEnumerator SaveGameCoroutine(int saveSlot, GameObject player, Inventory inventory)
    {
        IsSaving = true;
        SaveProgress = 0f;

        // Step 1: Gather player data
        yield return new WaitForEndOfFrame();
        SaveProgress = 0.2f;
        OnSaveProgressChanged?.Invoke(SaveProgress);

        PersistentPlayerData playerData = null;
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        if (player != null)
        {
            playerData = player.GetComponent<PersistentPlayerData>();
            if (playerData == null)
            {
                playerData = player.GetComponentInChildren<PersistentPlayerData>();
            }

            position = player.transform.position;
            rotation = player.transform.rotation;
        }
        else
        {
            // Try to find player in scene
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerData = playerObj.GetComponent<PersistentPlayerData>();
                position = playerObj.transform.position;
                rotation = playerObj.transform.rotation;
            }
        }

        // Step 2: Gather inventory data
        yield return new WaitForEndOfFrame();
        SaveProgress = 0.4f;
        OnSaveProgressChanged?.Invoke(SaveProgress);

        // Step 3: Update database
        if (playerData != null)
        {
            DatabaseManager.Instance.SetPlayerData(playerData, position, rotation);
        }

        if (inventory != null)
        {
            DatabaseManager.Instance.SetInventory(ConvertInventorySlots(inventory.Slots));
        }
        else
        {
            var inv = FindFirstObjectByType<Inventory>();
            if (inv != null)
            {
                DatabaseManager.Instance.SetInventory(ConvertInventorySlots(inv.Slots));
            }
        }

        // Step 4: Save to file
        yield return new WaitForEndOfFrame();
        SaveProgress = 0.7f;
        OnSaveProgressChanged?.Invoke(SaveProgress);

        DatabaseManager.Instance.SaveAllData(saveSlot);

        // Complete
        SaveProgress = 1f;
        OnSaveProgressChanged?.Invoke(SaveProgress);

        yield return new WaitForSeconds(0.1f);

        IsSaving = false;
        SaveProgress = 0f;

        string timestamp = DatabaseManager.Instance.GetLastSaveTimestamp();
        OnSaveCompleted?.Invoke(true, $"Saved at {timestamp}");

        Debug.Log($"[DatabaseSaveLoadHelper] Game saved to slot {saveSlot}");
    }

    /// <summary>
    /// Load game data with progress tracking.
    /// </summary>
    /// <param name="saveSlot">Save slot number to load</param>
    /// <param name="player">Player GameObject to apply data to</param>
    /// <param name="inventory">Inventory component to apply data to</param>
    public void LoadGame(int saveSlot, GameObject player = null, Inventory inventory = null)
    {
        if (IsLoading)
        {
            Debug.LogWarning("[DatabaseSaveLoadHelper] Load already in progress");
            return;
        }

        StartCoroutine(LoadGameCoroutine(saveSlot, player, inventory));
    }

    private IEnumerator LoadGameCoroutine(int saveSlot, GameObject player, Inventory inventory)
    {
        IsLoading = true;
        LoadProgress = 0f;

        // Check if save exists
        if (!DatabaseManager.Instance.HasSaveFile())
        {
            IsLoading = false;
            OnLoadCompleted?.Invoke(false, "No save file found");
            yield break;
        }

        // Step 1: Load from file
        yield return new WaitForEndOfFrame();
        LoadProgress = 0.2f;
        OnLoadProgressChanged?.Invoke(LoadProgress);

        bool success = DatabaseManager.Instance.LoadAllData(saveSlot);

        if (!success)
        {
            IsLoading = false;
            LoadProgress = 0f;
            OnLoadCompleted?.Invoke(false, "Failed to load save file");
            yield break;
        }

        // Step 2: Apply player data
        yield return new WaitForEndOfFrame();
        LoadProgress = 0.5f;
        OnLoadProgressChanged?.Invoke(LoadProgress);

        PersistentPlayerData playerData = DatabaseManager.Instance.GetPlayerData();
        Vector3 position = DatabaseManager.Instance.GetPlayerPosition();
        Quaternion rotation = DatabaseManager.Instance.GetPlayerRotation();

        if (player != null)
        {
            // Apply position and rotation
            player.transform.position = position;
            player.transform.rotation = rotation;

            // Apply stats if component exists
            var existingPlayerData = player.GetComponent<PersistentPlayerData>();
            if (existingPlayerData != null && playerData != null)
            {
                existingPlayerData.currentHealth = playerData.currentHealth;
                existingPlayerData.maxHealth = playerData.maxHealth;
                existingPlayerData.currentMana = playerData.currentMana;
                existingPlayerData.maxMana = playerData.maxMana;
                existingPlayerData.currentStamina = playerData.currentStamina;
                existingPlayerData.maxStamina = playerData.maxStamina;
                existingPlayerData.level = playerData.level;
                existingPlayerData.experience = playerData.experience;
                existingPlayerData.movementSpeed = playerData.movementSpeed;
                existingPlayerData.jumpForce = playerData.jumpForce;
            }
        }

        // Step 3: Apply inventory data
        yield return new WaitForEndOfFrame();
        LoadProgress = 0.8f;
        OnLoadProgressChanged?.Invoke(LoadProgress);

        // Note: Inventory application depends on your inventory system implementation
        // You may need to refresh the UI after loading

        // Complete
        LoadProgress = 1f;
        OnLoadProgressChanged?.Invoke(LoadProgress);

        yield return new WaitForSeconds(0.1f);

        IsLoading = false;
        LoadProgress = 0f;

        OnLoadCompleted?.Invoke(true, "Game loaded successfully");

        Debug.Log($"[DatabaseSaveLoadHelper] Game loaded from slot {saveSlot}");
    }

    /// <summary>
    /// Quick save without progress tracking.
    /// </summary>
    public void QuickSave()
    {
        SaveGame(1);
    }

    /// <summary>
    /// Quick load without progress tracking.
    /// </summary>
    public void QuickLoad()
    {
        LoadGame(1);
    }

    /// <summary>
    /// Check if a save exists in the specified slot.
    /// </summary>
    public bool HasSave(int saveSlot)
    {
        return DatabaseManager.Instance.HasSaveFile();
    }

    /// <summary>
    /// Get save info for UI display.
    /// </summary>
    public string GetSaveInfo(int saveSlot)
    {
        if (!HasSave(saveSlot))
            return "Empty Slot";

        return $"Save from: {DatabaseManager.Instance.GetLastSaveTimestamp()}";
    }
}
}
