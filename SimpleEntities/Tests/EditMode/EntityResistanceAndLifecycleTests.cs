using NUnit.Framework;
using Systems.SimpleEntities.Data.Context;
using Systems.SimpleStats.Abstract.Modifiers;

namespace Systems.SimpleEntities.Tests
{
    public sealed class EntityResistanceAndLifecycleTests : SimpleEntitiesTestBase
    {
        [Test]
        public void DamageContext_AppliesResistanceReductionAndClampsNegativeDamage()
        {
            TestEntity entity = CreateEntity();
            TestAffinity affinity = CreateAffinity<TestAffinity>();

            DamageContext reduced = new DamageContext(entity, null, affinity, 0.25f, 80);
            DamageContext immune = new DamageContext(entity, null, affinity, 1.5f, 80);
            DamageContext vulnerable = new DamageContext(entity, null, affinity, -0.5f, 80);

            Assert.AreEqual(60, reduced.amount);
            Assert.AreEqual(0, immune.amount);
            Assert.AreEqual(120, vulnerable.amount);
        }

        [Test]
        public void HealContext_AppliesResistanceAmplificationAndClampsNegativeHealing()
        {
            TestEntity entity = CreateEntity();
            TestAffinity affinity = CreateAffinity<TestAffinity>();

            HealContext amplified = new HealContext(entity, null, affinity, 0.5f, 80);
            HealContext blocked = new HealContext(entity, null, affinity, -1.5f, 80);
            HealContext reduced = new HealContext(entity, null, affinity, -0.25f, 80);

            Assert.AreEqual(120, amplified.amount);
            Assert.AreEqual(0, blocked.amount);
            Assert.AreEqual(60, reduced.amount);
        }

        [Test]
        public void GetResistance_SumsModifiersForMatchingResistanceOnly()
        {
            TestEntity entity = CreateEntity();
            TestFireResistance fireResistance = CreateResistance<TestFireResistance>();
            TestOtherResistance otherResistance = CreateResistance<TestOtherResistance>();
            entity.AddModifierForTests(new DirectStatModifier(fireResistance, 0.25f));
            entity.AddModifierForTests(new DirectStatModifier(fireResistance, 0.5f));
            entity.AddModifierForTests(new DirectStatModifier(otherResistance, 1f));

            float fireValue = entity.GetResistance<TestAffinity>();
            float otherValue = entity.GetResistance<OtherTestAffinity>();

            Assert.AreEqual(0.75f, fireValue);
            Assert.AreEqual(1f, otherValue);
        }

        [Test]
        public void Tick_UpdatesTimedModifiersAndRemovesExpiredOnes()
        {
            TestEntity entity = CreateEntity();
            TestTimedModifier expiringModifier = new TestTimedModifier(0.5f);
            TestTimedModifier activeModifier = new TestTimedModifier(2f);
            entity.AddModifierForTests(expiringModifier);
            entity.AddModifierForTests(activeModifier);

            entity.TickForTests(0.75f);

            Assert.AreEqual(1, entity.ModifierExpiredCount);
            Assert.AreEqual(1, entity.GetAllModifiers().Count);
            IStatModifier remainingModifier = entity.GetAllModifiers()[0];
            Assert.AreSame(activeModifier, remainingModifier);
            Assert.AreEqual(1.25f, activeModifier.TimeRemaining);
        }

        [Test]
        public void LifecycleHooks_RunInExpectedOrderAndResetHealthOnAwake()
        {
            TestEntity entity = CreateUninitializedEntity(maxHealth: 75, currentHealth: 1);

            entity.InvokeEnableForTests();
            entity.InvokeAwakeForTests();
            entity.InvokeStartForTests();
            entity.InvokeDisableForTests();

            Assert.AreEqual(1, entity.ActivatedCount);
            Assert.AreEqual(1, entity.AssignComponentsCount);
            Assert.AreEqual(1, entity.InitializedCount);
            Assert.AreEqual(1, entity.SetupCompleteCount);
            Assert.AreEqual(1, entity.DeactivatedCount);
            Assert.AreEqual(75, entity.CurrentHealth);
        }
    }
}
