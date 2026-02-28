using System.Collections.Generic;
using Unity6.LavosTrial.HUD;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// PlayerStats â€” Source unique de vÃ©ritÃ© pour la santÃ©, la mana et la stamina.
///              Fusionne PlayerHealth.cs et l'ancien PlayerStats.cs.
///
/// SETUP Unity :
///   1. Attache ce script sur ton GameObject joueur (remplace PlayerHealth + PlayerStats).
///   2. Supprime PlayerHealth.cs de la scÃ¨ne et du projet.
///   3. PlayerController et HUDSystem s'abonnent aux Ã©vÃ©nements statiques/instance
///      sans modification nÃ©cessaire.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    // â”€â”€â”€ Singleton â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public static PlayerStats Instance { get; private set; }

    // â”€â”€â”€ Inspector â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    [Header("SantÃ©")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invincibilityTime = 0.5f;
    [SerializeField] private bool healthRegenEnabled = false;
    [SerializeField] private float healthRegenAmount = 5f;
    [SerializeField] private float healthRegenDelay = 3f;

    [Header("Mana")]
    [SerializeField] private float maxMana = 50f;
    [SerializeField] private float manaRegen = 10f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegen = 15f;

    [Header("Status Effects")]
    [SerializeField] private List<StatusEffect> activeEffects = new();

    // â”€â”€â”€ Ã‰tat interne â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private float _currentHealth;
    private float _currentMana;
    private float _currentStamina;
    private float _lastDamageTime;
    private bool _isInvincible;
    private bool _isDead;

    // â”€â”€â”€ PropriÃ©tÃ©s publiques â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;
    public float CurrentMana => _currentMana;
    public float MaxMana => maxMana;
    public float CurrentStamina => _currentStamina;
    public float MaxStamina => maxStamina;
    public bool IsDead => _isDead;
    public IReadOnlyList<StatusEffect> ActiveEffects => activeEffects;

    // â”€â”€â”€ Ã‰vÃ©nements statiques (rÃ©trocompatibilitÃ© PlayerHealth) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public static event System.Action<float, float> OnHealthChanged;   // (current, max)
    public static event System.Action OnPlayerDied;
    public static event System.Action<float> OnPlayerDamaged;   // dÃ©gÃ¢ts reÃ§us

    // â”€â”€â”€ Ã‰vÃ©nements d'instance (rÃ©trocompatibilitÃ© ancien PlayerStats) â”€â”€â”€â”€â”€â”€â”€
    public event System.Action<float, float> OnManaChanged;
    public event System.Action<float, float> OnStaminaChanged;
    public event System.Action<StatusEffect> OnEffectAdded;
    public event System.Action<StatusEffect> OnEffectRemoved;

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _currentHealth = maxHealth;
        _currentMana = maxMana;
        _currentStamina = maxStamina;

        SpawnUIBars();
    }

    private void SpawnUIBars()
    {
        if (UIBarsSystem.Instance != null) return;

        var oldHud = GameObject.Find("HUDSystem");
        if (oldHud != null) oldHud.SetActive(false);

        var uiGO = new GameObject("UIBarsSystem");
        try
        {
            var system = uiGO.AddComponent<UIBarsSystem>();
            // Instance is set internally by UIBarsSystem.Awake()
            Object.DontDestroyOnLoad(uiGO);
        }
        catch
        {
            Object.Destroy(uiGO);
            throw;
        }
    }

    void Start()
    {
        // Notifie le HUD de l'état initial
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        OnManaChanged?.Invoke(_currentMana, maxMana);
        OnStaminaChanged?.Invoke(_currentStamina, maxStamina);
    }

    void Update()
    {
        if (_isDead) return;

        HandleInvincibility();
        HandleHealthRegen();
        HandleManaRegen();
        HandleStaminaRegen();
        UpdateStatusEffects();
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  SANTÃ‰
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Inflige des dégâts (respecte l'invincibilitÃ©).</summary>
    public void TakeDamage(float damage)
    {
        if (_isDead || _isInvincible) return;

        _currentHealth = Mathf.Max(_currentHealth - damage, 0f);
        _lastDamageTime = Time.time;
        _isInvincible = true;

        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        OnPlayerDamaged?.Invoke(damage);

        Debug.Log($"[PlayerStats] DÃ©gÃ¢ts : {damage} | Vie : {_currentHealth}/{maxHealth}");

        if (_currentHealth <= 0f) Die();
    }

    /// <summary>Soigne le joueur d'un montant donnÃ©.</summary>
    public void Heal(float amount)
    {
        if (_isDead) return;
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    /// <summary>Soigne complÃ¨tement le joueur.</summary>
    public void FullHeal() => Heal(maxHealth);

    /// <summary>RamÃ¨ne le joueur Ã  la vie.</summary>
    public void Revive(float healthAmount)
    {
        _isDead = false;
        _isInvincible = false;
        _currentHealth = Mathf.Clamp(healthAmount, 0f, maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        Debug.Log("[PlayerStats] Joueur rÃ©animÃ©.");
    }

    private void HandleInvincibility()
    {
        if (_isInvincible && Time.time - _lastDamageTime > invincibilityTime)
            _isInvincible = false;
    }

    private void HandleHealthRegen()
    {
        if (!healthRegenEnabled || _currentHealth >= maxHealth) return;
        if (Time.time - _lastDamageTime < healthRegenDelay) return;

        _currentHealth = Mathf.Min(_currentHealth + healthRegenAmount * Time.deltaTime, maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        Debug.Log("[PlayerStats] Le joueur est mort !");
        OnPlayerDied?.Invoke();
        GameManager.Instance?.TriggerGameOver();

        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  MANA
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Tente de dÃ©penser de la mana. Retourne false si insuffisant.</summary>
    public bool UseMana(float amount)
    {
        if (_currentMana < amount) return false;
        _currentMana -= amount;
        OnManaChanged?.Invoke(_currentMana, maxMana);
        return true;
    }

    public void RestoreMana(float amount)
    {
        _currentMana = Mathf.Min(_currentMana + amount, maxMana);
        OnManaChanged?.Invoke(_currentMana, maxMana);
    }

    private void HandleManaRegen()
    {
        if (_currentMana >= maxMana || manaRegen <= 0f) return;
        _currentMana = Mathf.Min(_currentMana + manaRegen * Time.deltaTime, maxMana);
        OnManaChanged?.Invoke(_currentMana, maxMana);
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  STAMINA
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Tente de dÃ©penser de la stamina. Retourne false si insuffisant.</summary>
    public bool UseStamina(float amount)
    {
        if (_currentStamina < amount) return false;
        _currentStamina -= amount;
        OnStaminaChanged?.Invoke(_currentStamina, maxStamina);
        return true;
    }

    public void RestoreStamina(float amount)
    {
        _currentStamina = Mathf.Min(_currentStamina + amount, maxStamina);
        OnStaminaChanged?.Invoke(_currentStamina, maxStamina);
    }

    private void HandleStaminaRegen()
    {
        if (_currentStamina >= maxStamina || staminaRegen <= 0f) return;
        _currentStamina = Mathf.Min(_currentStamina + staminaRegen * Time.deltaTime, maxStamina);
        OnStaminaChanged?.Invoke(_currentStamina, maxStamina);
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  STATUS EFFECTS
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public void AddEffect(StatusEffect effect)
    {
        StatusEffect existing = activeEffects.Find(e => e.id == effect.id);
        if (existing != null)
        {
            if (existing.currentStacks < existing.maxStacks) existing.currentStacks++;
            existing.remainingTime = existing.duration;
        }
        else
        {
            effect.currentStacks = 1;
            effect.remainingTime = effect.duration;
            effect.nextTickTime = Time.time;
            activeEffects.Add(effect);
            OnEffectAdded?.Invoke(effect);
        }
    }

    public void RemoveEffect(StatusEffect effect)
    {
        if (activeEffects.Remove(effect))
            OnEffectRemoved?.Invoke(effect);
    }

    public void ClearEffects()
    {
        foreach (var e in activeEffects) OnEffectRemoved?.Invoke(e);
        activeEffects.Clear();
    }

    public bool HasEffect(string effectId) => activeEffects.Exists(e => e.id == effectId);
    public float GetEffectIntensity(string effectId)
    {
        var e = activeEffects.Find(x => x.id == effectId);
        return e != null ? e.intensity * e.currentStacks : 0f;
    }

    private void UpdateStatusEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = activeEffects[i];
            effect.remainingTime -= Time.deltaTime;

            if (effect.tickRate > 0f && Time.time >= effect.nextTickTime)
            {
                ApplyEffectTick(effect);
                effect.nextTickTime = Time.time + effect.tickRate;
            }

            if (effect.IsExpired) RemoveEffect(effect);
        }
    }

    private void ApplyEffectTick(StatusEffect effect)
    {
        switch (effect.id)
        {
            case "poison": TakeDamage(effect.intensity * effect.currentStacks); break;
            case "regeneration": Heal(effect.intensity * effect.currentStacks); break;
            case "mana_regen": RestoreMana(effect.intensity * effect.currentStacks); break;
        }
    }
}
