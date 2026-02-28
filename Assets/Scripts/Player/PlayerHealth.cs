using UnityEngine;

/// <summary>
/// PLAYERHEALTH â€” Gestion de la vie du joueur
/// 
/// SETUP dans Unity :
///  1. Attache ce script sur ton GameObject joueur
///  2. Configure maxHealth dans l'Inspector
///  3. Appelle TakeDamage(float) depuis n'importe quel ennemi ou piÃ¨ge
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    // â”€â”€â”€ ParamÃ¨tres Inspector â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    [Header("Vie")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invincibilityTime = 0.5f; // Temps invincible aprÃ¨s un coup

    [Header("RÃ©gÃ©nÃ©ration (optionnel)")]
    [SerializeField] private bool regenEnabled = false;
    [SerializeField] private float regenAmount = 5f;   // HP par seconde
    [SerializeField] private float regenDelay = 3f;   // DÃ©lai avant regen (secondes)

    // â”€â”€â”€ Ã‰tat â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    private float _lastDamageTime;
    private bool _isInvincible;

    // â”€â”€â”€ Ã‰vÃ©nements â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public static event System.Action<float, float> OnHealthChanged; // (current, max)
    public static event System.Action OnPlayerDied;
    public static event System.Action<float> OnPlayerDamaged; // (dÃ©gÃ¢ts reÃ§us)

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    void Start()
    {
        // Notifie l'UI dÃ¨s le dÃ©part
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void Update()
    {
        // Fin de l'invincibilitÃ© temporaire
        if (_isInvincible && Time.time - _lastDamageTime > invincibilityTime)
            _isInvincible = false;

        // RÃ©gÃ©nÃ©ration
        if (regenEnabled && !IsDead && CurrentHealth < maxHealth)
        {
            if (Time.time - _lastDamageTime >= regenDelay)
                Heal(regenAmount * Time.deltaTime);
        }
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  MÃ‰THODES PUBLIQUES
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Inflige des dÃ©gÃ¢ts au joueur.</summary>
    public void TakeDamage(float damage)
    {
        if (IsDead || _isInvincible) return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);
        _lastDamageTime = Time.time;
        _isInvincible = true;

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnPlayerDamaged?.Invoke(damage);

        Debug.Log($"[PlayerHealth] DÃ©gÃ¢ts reÃ§us : {damage} | Vie restante : {CurrentHealth}");

        if (CurrentHealth <= 0f)
            Die();
    }

    /// <summary>Soigne le joueur.</summary>
    public void Heal(float amount)
    {
        if (IsDead) return;

        CurrentHealth += amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>Soigne complÃ¨tement le joueur.</summary>
    public void FullHeal()
    {
        Heal(maxHealth);
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  MORT
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        Debug.Log("[PlayerHealth] Le joueur est mort !");
        OnPlayerDied?.Invoke();
        GameManager.Instance?.TriggerGameOver();

        // DÃ©sactive les contrÃ´les
        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;
    }
}
