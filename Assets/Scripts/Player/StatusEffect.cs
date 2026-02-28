using UnityEngine;

public enum StatusEffectType { Buff, Debuff }

[System.Serializable]
public class StatusEffect
{
    public string id;
    public string effectName;
    public StatusEffectType type;
    public Sprite icon;
    public float duration;
    public float intensity;
    public int maxStacks = 1;
    public float tickRate;

    // Runtime (non sÃ©rialisÃ©)
    [System.NonSerialized] public float remainingTime;
    [System.NonSerialized] public int currentStacks;
    [System.NonSerialized] public float nextTickTime;

    // UtilisÃ© par HUDSystem pour calculer le fill de la barre de durÃ©e
    public float MaxDuration => duration;

    public bool IsExpired => remainingTime <= 0f;
}
