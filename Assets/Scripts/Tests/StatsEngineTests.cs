// StatsEngineTests.cs
// Unit tests for StatsEngine
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Test file - validates stat calculations

// StatsEngineTests.cs
// Unit tests for StatsEngine
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Test file - validates stat calculations
using NUnit.Framework;
using Code.Lavos.Status;
using UnityEngine;

namespace Code.Lavos.Tests
{
    /// <summary>
    /// Unit tests for StatsEngine class
    /// Tests stat calculations, modifiers, damage, and resource management
    /// </summary>
    [TestFixture]
    public class StatsEngineTests
    {
        private StatsEngine _stats;

        [SetUp]
        public void Setup()
        {
            _stats = new StatsEngine();
            _stats.SetBaseStats(1000f, 500f, 200f, 10f, 20f, 30f);
        }

        #region Initialization Tests

        [Test]
        public void Constructor_InitializesWithDefaultStats()
        {
            var engine = new StatsEngine();
            
            Assert.That(engine.MaxHealth, Is.GreaterThan(0));
            Assert.That(engine.MaxMana, Is.GreaterThan(0));
            Assert.That(engine.MaxStamina, Is.GreaterThan(0));
        }

        [Test]
        public void SetBaseStats_UpdatesMaxValues()
        {
            _stats.SetBaseStats(2000f, 1000f, 400f);
            
            Assert.That(_stats.MaxHealth, Is.EqualTo(2000f));
            Assert.That(_stats.MaxMana, Is.EqualTo(1000f));
            Assert.That(_stats.MaxStamina, Is.EqualTo(400f));
        }

        [Test]
        public void Constructor_FullHealsPlayer()
        {
            _stats.SetBaseStats(1000f, 500f, 200f);
            
            Assert.That(_stats.CurrentHealth, Is.EqualTo(1000f));
            Assert.That(_stats.CurrentMana, Is.EqualTo(500f));
            Assert.That(_stats.CurrentStamina, Is.EqualTo(200f));
        }

        #endregion

        #region Resource Management Tests

        [Test]
        public void UseMana_SufficientMana_ReturnsTrue()
        {
            bool result = _stats.UseMana(100f);
            
            Assert.IsTrue(result);
            Assert.That(_stats.CurrentMana, Is.EqualTo(400f));
        }

        [Test]
        public void UseMana_InsufficientMana_ReturnsFalse()
        {
            bool result = _stats.UseMana(600f);
            
            Assert.IsFalse(result);
            Assert.That(_stats.CurrentMana, Is.EqualTo(500f));
        }

        [Test]
        public void UseStamina_DeductsCorrectAmount()
        {
            _stats.UseStamina(50f);
            
            Assert.That(_stats.CurrentStamina, Is.EqualTo(150f));
        }

        [Test]
        public void RestoreMana_CapsAtMax()
        {
            _stats.UseMana(300f);
            _stats.RestoreMana(500f);
            
            Assert.That(_stats.CurrentMana, Is.EqualTo(500f));
        }

        [Test]
        public void ModifyHealth_ClampsToZero()
        {
            _stats.ModifyHealth(-1500f);
            
            Assert.That(_stats.CurrentHealth, Is.EqualTo(0f));
        }

        [Test]
        public void ModifyHealth_ClampsToMax()
        {
            _stats.ModifyHealth(500f);
            
            Assert.That(_stats.CurrentHealth, Is.EqualTo(1000f));
        }

        [Test]
        public void CanAfford_AllResourcesAvailable_ReturnsTrue()
        {
            bool result = _stats.CanAfford(100f, 50f, 20f);
            
            Assert.IsTrue(result);
        }

        [Test]
        public void CanAfford_InsufficientHealth_ReturnsFalse()
        {
            _stats.ModifyHealth(-950f);
            bool result = _stats.CanAfford(100f, 0f, 0f);
            
            Assert.IsFalse(result);
        }

        #endregion

        #region Damage Calculation Tests

        [Test]
        public void CalculateDamage_NoResistances_ReturnsBaseDamage()
        {
            var damageInfo = new DamageInfo(100f, DamageType.Physical);
            
            float result = _stats.CalculateDamage(damageInfo);
            
            Assert.That(result, Is.EqualTo(100f).Within(0.01f));
        }

        [Test]
        public void CalculateDamage_WithResistance_ReducedDamage()
        {
            _stats.SetBaseResistance(DamageType.Fire, 0.5f);
            var damageInfo = new DamageInfo(100f, DamageType.Fire);
            
            float result = _stats.CalculateDamage(damageInfo);
            
            Assert.That(result, Is.LessThan(100f));
        }

        [Test]
        public void CalculateDamage_WithWeakness_IncreasedDamage()
        {
            _stats.SetBaseResistance(DamageType.Ice, -0.25f);
            var damageInfo = new DamageInfo(100f, DamageType.Ice);
            
            float result = _stats.CalculateDamage(damageInfo);
            
            Assert.That(result, Is.GreaterThan(100f));
        }

        [Test]
        public void GetResistanceMultiplier_ZeroResistance_ReturnsOne()
        {
            float multiplier = _stats.GetResistanceMultiplier(DamageType.Physical);
            
            Assert.That(multiplier, Is.EqualTo(1f));
        }

        [Test]
        public void GetResistanceMultiplier_FiftyPercentResistance_ReturnsPointFive()
        {
            _stats.SetBaseResistance(DamageType.Fire, 0.5f);
            float multiplier = _stats.GetResistanceMultiplier(DamageType.Fire);
            
            Assert.That(multiplier, Is.EqualTo(0.5f));
        }

        #endregion

        #region Stat Modifier Tests

        [Test]
        public void AddModifier_AdditiveIncreasesStat()
        {
            _stats.AddModifier("health", "test_buff", "source1", ModifierType.Additive, 500f);
            
            Assert.That(_stats.MaxHealth, Is.EqualTo(1500f));
        }

        [Test]
        public void RemoveModifiersBySource_RemovesAllFromSource()
        {
            _stats.AddModifier("health", "buff1", "source1", ModifierType.Additive, 200f);
            _stats.AddModifier("health", "buff2", "source1", ModifierType.Additive, 300f);
            _stats.AddModifier("health", "buff3", "source2", ModifierType.Additive, 100f);
            
            _stats.RemoveModifiersBySource("source1");
            
            Assert.That(_stats.MaxHealth, Is.EqualTo(1100f));
        }

        [Test]
        public void AddModifier_MultiplicativeAppliesCorrectly()
        {
            _stats.AddModifier("damage", "crit_buff", "source1", ModifierType.Multiplicative, 0.5f);
            
            // Base damage multiplier is 1f, multiplicative adds 0.5f
            Assert.That(_stats.DamageMultiplier, Is.EqualTo(1.5f).Within(0.01f));
        }

        #endregion

        #region Status Effect Tests

        [Test]
        public void ApplyEffect_AddsEffectToList()
        {
            var effectData = CreateTestEffect("test_effect", 10f);
            
            bool result = _stats.ApplyEffect(effectData);
            
            Assert.IsTrue(result);
            Assert.That(_stats.ActiveEffects.Count, Is.EqualTo(1));
        }

        [Test]
        public void HasEffect_ReturnsTrueForActiveEffect()
        {
            var effectData = CreateTestEffect("test_effect", 10f);
            _stats.ApplyEffect(effectData);
            
            bool result = _stats.HasEffect("test_effect");
            
            Assert.IsTrue(result);
        }

        [Test]
        public void RemoveEffect_RemovesFromList()
        {
            var effectData = CreateTestEffect("test_effect", 10f);
            _stats.ApplyEffect(effectData);
            
            _stats.RemoveEffect("test_effect");
            
            Assert.That(_stats.ActiveEffects.Count, Is.EqualTo(0));
        }

        [Test]
        public void ClearAllEffects_RemovesAllEffects()
        {
            _stats.ApplyEffect(CreateTestEffect("effect1", 10f));
            _stats.ApplyEffect(CreateTestEffect("effect2", 10f));
            _stats.ApplyEffect(CreateTestEffect("effect3", 10f));
            
            _stats.ClearAllEffects();
            
            Assert.That(_stats.ActiveEffects.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetDebuffs_ReturnsOnlyDebuffs()
        {
            var buff = CreateTestEffectWithCategory("buff", "Buff", EffectType.Buff, 10f);
            var debuff = CreateTestEffectWithCategory("debuff", "Debuff", EffectType.Debuff, 10f);
            
            _stats.ApplyEffect(buff);
            _stats.ApplyEffect(debuff);
            
            var debuffs = _stats.GetDebuffs();
            
            Assert.That(debuffs.Count, Is.EqualTo(1));
            Assert.That(debuffs[0].id, Is.EqualTo("debuff"));
        }

        #endregion

        #region Regeneration Tests

        [Test]
        public void ApplyRegeneration_IncreasesHealth()
        {
            _stats.ModifyHealth(-100f);
            float healthBefore = _stats.CurrentHealth;
            
            _stats.ApplyRegeneration(1f);
            
            Assert.That(_stats.CurrentHealth, Is.GreaterThan(healthBefore));
        }

        [Test]
        public void ApplyRegeneration_DoesNotExceedMax()
        {
            _stats.ApplyRegeneration(1000f);
            
            Assert.That(_stats.CurrentHealth, Is.LessThanOrEqualTo(_stats.MaxHealth));
        }

        #endregion

        #region Utility Tests

        [Test]
        public void GetBuffs_ReturnsOnlyBuffs()
        {
            var buff = CreateTestEffectWithCategory("buff", "Buff", EffectType.Buff, 10f);
            var debuff = CreateTestEffectWithCategory("debuff", "Debuff", EffectType.Debuff, 10f);
            
            _stats.ApplyEffect(buff);
            _stats.ApplyEffect(debuff);
            
            var buffs = _stats.GetBuffs();
            
            Assert.That(buffs.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetEffectIntensity_ReturnsCorrectValue()
        {
            var effectData = CreateTestEffect("test_effect", 10f);
            effectData.intensity = 2.5f;
            _stats.ApplyEffect(effectData);
            
            float intensity = _stats.GetEffectIntensity("test_effect");
            
            Assert.That(intensity, Is.EqualTo(2.5f));
        }

        #endregion

        #region Helper Methods

        private StatusEffectData CreateTestEffect(string id, float duration)
        {
            var effect = new StatusEffectData
            {
                id = id,
                duration = duration,
                effectType = EffectType.Buff
            };
            return effect;
        }

        private StatusEffectData CreateTestEffectWithCategory(string id, string name, EffectType type, float duration)
        {
            var effect = new StatusEffectData
            {
                id = id,
                effectName = name,
                effectType = type,
                category = type.ToString(),
                duration = duration
            };
            return effect;
        }

        #endregion
    }
}
