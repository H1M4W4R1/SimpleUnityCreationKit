using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleEntities.Data.Context;
using Systems.SimpleEntities.Operations;

namespace Systems.SimpleEntities.Tests
{
    public sealed class EntityHealthTests : SimpleEntitiesTestBase
    {
        [Test]
        public void Damage_ReducesHealthByResistanceAdjustedAmountAndFiresCallbacks()
        {
            TestEntity entity = CreateEntity();
            TestAffinity affinity = CreateAffinity<TestAffinity>();
            DamageContext context = new DamageContext(entity, "source", affinity, 0.25f, 40);

            OperationResult result = entity.Damage(context);

            AssertSimilar(EntityOperations.Damaged(), result);
            Assert.AreEqual(70, entity.CurrentHealth);
            Assert.AreEqual(1, entity.DamageReceivedCount);
            Assert.AreEqual(30, entity.LastHealthLost);
            Assert.AreEqual(1, affinity.DamageReceivedCount);
            Assert.AreEqual(30, affinity.LastHealthLost);
        }

        [Test]
        public void Damage_WhenRejected_FailsWithoutChangingHealth()
        {
            TestEntity entity = CreateEntity();
            TestAffinity affinity = CreateAffinity<TestAffinity>();
            affinity.RejectDamage = true;
            DamageContext context = new DamageContext(entity, null, affinity, 0f, 25);

            OperationResult result = entity.Damage(context);

            AssertSimilar(EntityOperations.NotPermitted(), result);
            Assert.AreEqual(100, entity.CurrentHealth);
            Assert.AreEqual(1, entity.DamageFailedCount);
            Assert.AreEqual(1, affinity.DamageFailedCount);
        }

        [Test]
        public void Damage_WithInternalSource_SuppressesDamageCallbacks()
        {
            TestEntity entity = CreateEntity();
            TestAffinity affinity = CreateAffinity<TestAffinity>();
            DamageContext context = new DamageContext(entity, null, affinity, 0f, 10);

            OperationResult result = entity.Damage(context, ActionSource.Internal);

            AssertSimilar(EntityOperations.Damaged(), result);
            Assert.AreEqual(90, entity.CurrentHealth);
            Assert.AreEqual(0, entity.DamageReceivedCount);
            Assert.AreEqual(0, affinity.DamageReceivedCount);
        }

        [Test]
        public void Damage_WhenLethal_KillsEntityAndReportsLostHealthBeforeDeath()
        {
            TestEntity entity = CreateEntity(currentHealth: 20);
            TestAffinity affinity = CreateAffinity<TestAffinity>();
            DamageContext context = new DamageContext(entity, null, affinity, 0f, 50);

            OperationResult result = entity.Damage(context);

            AssertSimilar(EntityOperations.Killed(), result);
            Assert.AreEqual(0, entity.CurrentHealth);
            Assert.IsTrue(entity.IsDead);
            Assert.AreEqual(1, entity.DamageReceivedCount);
            Assert.AreEqual(20, entity.LastHealthLost);
            Assert.AreEqual(1, entity.DeathCount);
            Assert.AreEqual(20, entity.LastDeathHealthLost);
            Assert.AreEqual(1, affinity.DeathCount);
            Assert.AreEqual(20, affinity.LastHealthLost);
        }

        [Test]
        public void Damage_WhenDeathSaveApplies_RestoresSavedHealthAndSkipsDeathCallback()
        {
            TestEntity entity = CreateEntity(currentHealth: 10);
            TestAffinity affinity = CreateAffinity<TestAffinity>();
            affinity.SaveFromDeath = true;
            affinity.SavedHealth = 7;
            DamageContext context = new DamageContext(entity, null, affinity, 0f, 20);

            OperationResult result = entity.Damage(context);

            AssertSimilar(EntityOperations.SavedFromDeath(), result);
            Assert.AreEqual(7, entity.CurrentHealth);
            Assert.IsFalse(entity.IsDead);
            Assert.AreEqual(1, entity.SavedFromDeathCount);
            Assert.AreEqual(7, entity.LastSavedHealth);
            Assert.AreEqual(0, entity.DeathCount);
            Assert.AreEqual(1, affinity.SavedFromDeathCount);
            Assert.AreEqual(7, affinity.LastSavedHealth);
        }

        [Test]
        public void Kill_UsesEntityDeathSaveWhenNoAffinitySaves()
        {
            TestEntity entity = CreateEntity(currentHealth: 5);
            entity.SaveFromDeath = true;
            entity.SavedHealth = 2;
            TestAffinity affinity = CreateAffinity<TestAffinity>();
            DamageContext context = new DamageContext(entity, null, affinity, 0f, 1);

            OperationResult result = entity.Kill(context);

            AssertSimilar(EntityOperations.SavedFromDeath(), result);
            Assert.AreEqual(2, entity.CurrentHealth);
            Assert.AreEqual(1, entity.SavedFromDeathCount);
            Assert.AreEqual(0, entity.DeathCount);
        }

        [Test]
        public void Heal_RestoresResistanceAdjustedAmountAndClampsToMaxHealth()
        {
            TestEntity entity = CreateEntity(currentHealth: 80);
            TestAffinity affinity = CreateAffinity<TestAffinity>();
            HealContext context = new HealContext(entity, "source", affinity, 0.5f, 40);

            OperationResult result = entity.Heal(context);

            AssertSimilar(EntityOperations.Healed(), result);
            Assert.AreEqual(100, entity.CurrentHealth);
            Assert.AreEqual(1, entity.HealReceivedCount);
            Assert.AreEqual(20, entity.LastHealthAdded);
            Assert.AreEqual(1, affinity.HealReceivedCount);
            Assert.AreEqual(20, affinity.LastHealthAdded);
        }

        [Test]
        public void Heal_WhenDeadOrRejected_FailsWithoutChangingHealth()
        {
            TestEntity deadEntity = CreateEntity(currentHealth: 0);
            TestAffinity affinity = CreateAffinity<TestAffinity>();
            HealContext deadContext = new HealContext(deadEntity, null, affinity, 0f, 10);

            OperationResult deadResult = deadEntity.Heal(deadContext);

            AssertSimilar(EntityOperations.NotPermitted(), deadResult);
            Assert.AreEqual(0, deadEntity.CurrentHealth);
            Assert.AreEqual(1, deadEntity.HealFailedCount);
            Assert.AreEqual(1, affinity.HealFailedCount);

            TestEntity rejectedEntity = CreateEntity(currentHealth: 50);
            rejectedEntity.RejectHeal = true;
            HealContext rejectedContext = new HealContext(rejectedEntity, null, affinity, 0f, 10);

            OperationResult rejectedResult = rejectedEntity.Heal(rejectedContext);

            AssertSimilar(EntityOperations.NotPermitted(), rejectedResult);
            Assert.AreEqual(50, rejectedEntity.CurrentHealth);
            Assert.AreEqual(1, rejectedEntity.HealFailedCount);
        }

        [Test]
        public void Heal_WithInternalSource_SuppressesCallbacks()
        {
            TestEntity entity = CreateEntity(currentHealth: 60);
            TestAffinity affinity = CreateAffinity<TestAffinity>();
            HealContext context = new HealContext(entity, null, affinity, 0f, 10);

            OperationResult result = entity.Heal(context, ActionSource.Internal);

            AssertSimilar(EntityOperations.Healed(), result);
            Assert.AreEqual(70, entity.CurrentHealth);
            Assert.AreEqual(0, entity.HealReceivedCount);
            Assert.AreEqual(0, affinity.HealReceivedCount);
        }
    }
}
