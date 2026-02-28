using UnityEngine;
 // Unity6 compatibility: ensure no problematic newer APIs are used in data container

[System.Serializable]
public class StatusEffectData
{
    public string id;
    public string effectName;
    public StatusEffectType type;
    public Sprite icon;
    public float duration;
    public float intensity;
    public int maxStacks = 1;
    public int currentStacks;
    public float remainingTime;
    public float tickRate;
    
    public StatusEffectData() {}
    
    public StatusEffectData(StatusEffect effect)
    {
        if (effect == null) return;
        
        id = effect.id;
        effectName = effect.effectName;
        icon = effect.icon;
        type = effect.type;
        duration = effect.duration;
        intensity = effect.intensity;
        maxStacks = effect.maxStacks;
        currentStacks = effect.currentStacks;
        remainingTime = effect.remainingTime;
        tickRate = effect.tickRate;
    }
    
    public StatusEffect ToStatusEffect()
    {
        StatusEffect effect = new StatusEffect
        {
            id = id,
            effectName = effectName,
            icon = icon,
            type = type,
            duration = duration,
            intensity = intensity,
            maxStacks = maxStacks,
            currentStacks = currentStacks,
            remainingTime = remainingTime,
            tickRate = tickRate,
            nextTickTime = Time.time
        };
        return effect;
    }
}
