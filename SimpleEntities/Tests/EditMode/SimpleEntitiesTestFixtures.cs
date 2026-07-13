using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleEntities.Components;
using Systems.SimpleEntities.Data.Affinity;
using Systems.SimpleEntities.Data.Context;
using Systems.SimpleEntities.Data.Enums;
using Systems.SimpleEntities.Data.Resistances;
using Systems.SimpleEntities.Data.Resistances.Markers;
using Systems.SimpleEntities.Data.Status.Abstract;
using Systems.SimpleEntities.Operations;
using Systems.SimpleStats.Abstract;
using Systems.SimpleStats.Abstract.Modifiers;
using Systems.SimpleStats.Data;
using Systems.SimpleStats.Data.Statistics;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleEntities.Tests
{
    public abstract class SimpleEntitiesTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int objectIndex = _createdObjects.Count - 1; objectIndex >= 0; objectIndex--)
            {
                Object createdObject = _createdObjects[objectIndex];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        protected TestEntity CreateEntity(long maxHealth = 100, long currentHealth = -1)
        {
            GameObject gameObject = Track(new GameObject("Test Entity"));
            gameObject.SetActive(false);

            TestEntity entity = gameObject.AddComponent<TestEntity>();
            entity.SetHealthForTests(maxHealth, maxHealth);
            entity.InvokeAwakeForTests();

            if (currentHealth >= 0)
                entity.SetHealthForTests(maxHealth, currentHealth);

            return entity;
        }

        protected TestEntity CreateUninitializedEntity(long maxHealth = 100, long currentHealth = 100)
        {
            GameObject gameObject = Track(new GameObject("Uninitialized Test Entity"));
            gameObject.SetActive(false);

            TestEntity entity = gameObject.AddComponent<TestEntity>();
            entity.SetHealthForTests(maxHealth, currentHealth);
            return entity;
        }

        protected TStatusType CreateStatus<TStatusType>(int maxStack = 1)
            where TStatusType : StatusBase
        {
            TStatusType status = Track(ScriptableObject.CreateInstance<TStatusType>());
            status.name = typeof(TStatusType).Name;
            SetInt(status, "<MaxStack>k__BackingField", maxStack);
            return status;
        }

        protected TAffinityType CreateAffinity<TAffinityType>()
            where TAffinityType : AffinityType
        {
            TAffinityType affinity = Track(ScriptableObject.CreateInstance<TAffinityType>());
            affinity.name = typeof(TAffinityType).Name;
            return affinity;
        }

        protected TResistanceType CreateResistance<TResistanceType>()
            where TResistanceType : TestResistanceBase
        {
            TResistanceType resistance = Track(ScriptableObject.CreateInstance<TResistanceType>());
            resistance.name = typeof(TResistanceType).Name;
            resistance.SetBaseValueForTests(0f);
            return resistance;
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject)
            where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected static void AssertSimilar(OperationResult expected, OperationResult actual)
        {
            Assert.IsTrue(
                OperationResult.AreSimilar(expected, actual),
                "Expected similar result to " + expected + " but received " + actual);
        }

        private static void SetInt(Object target, string propertyName, int value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.IsFalse(ReferenceEquals(property, null), propertyName);
            property.intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    public sealed class TestEntity : AliveEntityBase, IWithStatModifiers
    {
        public bool RejectDamage { get; set; }
        public bool RejectHeal { get; set; }
        public bool SaveFromDeath { get; set; }
        public long SavedHealth { get; set; } = 1;

        public int AssignComponentsCount { get; private set; }
        public int InitializedCount { get; private set; }
        public int SetupCompleteCount { get; private set; }
        public int ActivatedCount { get; private set; }
        public int DeactivatedCount { get; private set; }
        public int TeardownCount { get; private set; }
        public int TickCount { get; private set; }
        public int DamageReceivedCount { get; private set; }
        public int DamageFailedCount { get; private set; }
        public int HealReceivedCount { get; private set; }
        public int HealFailedCount { get; private set; }
        public int DeathCount { get; private set; }
        public int SavedFromDeathCount { get; private set; }
        public int ModifierExpiredCount { get; private set; }
        public long LastHealthLost { get; private set; }
        public long LastHealthAdded { get; private set; }
        public long LastDeathHealthLost { get; private set; }
        public long LastSavedHealth { get; private set; }

        public void SetHealthForTests(long maxHealth, long currentHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
        }

        public void InvokeAwakeForTests()
        {
            Awake();
        }

        public void InvokeStartForTests()
        {
            Start();
        }

        public void InvokeEnableForTests()
        {
            OnEnable();
        }

        public void InvokeDisableForTests()
        {
            OnDisable();
        }

        public void TickForTests(float deltaTime)
        {
            OnTick(deltaTime);
        }

        public void AddModifierForTests(IStatModifier modifier)
        {
            statModifiers.Add(modifier);
        }

        void IWithStatModifiers.OnModifierExpired(in ModifierContext context, in OperationResult result)
        {
            ModifierExpiredCount++;
        }

        protected override void AssignComponents()
        {
            AssignComponentsCount++;
            base.AssignComponents();
        }

        protected override void OnInitialized()
        {
            InitializedCount++;
            base.OnInitialized();
        }

        protected override void OnEntitySetupComplete()
        {
            SetupCompleteCount++;
            base.OnEntitySetupComplete();
        }

        protected override void OnEntityActivated()
        {
            ActivatedCount++;
            base.OnEntityActivated();
        }

        protected override void OnEntityDeactivated()
        {
            DeactivatedCount++;
            base.OnEntityDeactivated();
        }

        protected override void OnTeardown()
        {
            TeardownCount++;
            base.OnTeardown();
        }

        protected override void OnTick(float deltaTime)
        {
            TickCount++;
            base.OnTick(deltaTime);
        }

        protected override OperationResult CanBeDamaged(in DamageContext context)
        {
            if (RejectDamage) return EntityOperations.NotPermitted();
            return base.CanBeDamaged(context);
        }

        protected override OperationResult CanBeHealed(in HealContext context)
        {
            if (RejectHeal) return EntityOperations.NotPermitted();
            return base.CanBeHealed(context);
        }

        protected override DeathSaveContext CanSaveFromDeath(in DamageContext context)
        {
            if (SaveFromDeath) return new DeathSaveContext(true, SavedHealth);
            return base.CanSaveFromDeath(context);
        }

        protected override void OnDamageReceived(
            in DamageContext context,
            in OperationResult result,
            long healthLost)
        {
            DamageReceivedCount++;
            LastHealthLost = healthLost;
            base.OnDamageReceived(context, result, healthLost);
        }

        protected override void OnDamageFailed(in DamageContext context, in OperationResult result)
        {
            DamageFailedCount++;
            base.OnDamageFailed(context, result);
        }

        protected override void OnHealReceived(in HealContext context, in OperationResult result, long healthAdded)
        {
            HealReceivedCount++;
            LastHealthAdded = healthAdded;
            base.OnHealReceived(context, result, healthAdded);
        }

        protected override void OnHealFailed(in HealContext context, in OperationResult result)
        {
            HealFailedCount++;
            base.OnHealFailed(context, result);
        }

        protected override void OnSavedFromDeath(
            in DamageContext damageContext,
            in DeathSaveContext context,
            in OperationResult result,
            long healthSet)
        {
            SavedFromDeathCount++;
            LastSavedHealth = healthSet;
            base.OnSavedFromDeath(damageContext, context, result, healthSet);
        }

        protected override void OnDeath(in DamageContext context, in OperationResult result, long healthLost)
        {
            DeathCount++;
            LastDeathHealthLost = healthLost;
            base.OnDeath(context, result, healthLost);
        }
    }

    public sealed class TestStatus : StatusBase
    {
        public bool RejectApply { get; set; }
        public bool RejectRemove { get; set; }
        public bool RemoveSelfOnTick { get; set; }

        public int CanApplyCount { get; private set; }
        public int CanRemoveCount { get; private set; }
        public int AppliedCount { get; private set; }
        public int ApplyFailedCount { get; private set; }
        public int RemovedCount { get; private set; }
        public int RemoveFailedCount { get; private set; }
        public int StackChangedCount { get; private set; }
        public int TickCount { get; private set; }
        public int LastExpectedStackCount { get; private set; }
        public int LastCurrentStackCount { get; private set; }
        public float LastDeltaTime { get; private set; }

        protected override OperationResult CanApply(in StatusContext context)
        {
            CanApplyCount++;
            LastExpectedStackCount = context.expectedStackCount;
            if (RejectApply) return StatusOperations.InvalidStatus();
            return base.CanApply(context);
        }

        protected override OperationResult CanRemove(in StatusContext context)
        {
            CanRemoveCount++;
            LastExpectedStackCount = context.expectedStackCount;
            if (RejectRemove) return StatusOperations.InvalidStatus();
            return base.CanRemove(context);
        }

        protected override void OnStatusApplied(
            in StatusContext context,
            in OperationResult result,
            int currentStacks)
        {
            AppliedCount++;
            LastExpectedStackCount = context.expectedStackCount;
            LastCurrentStackCount = currentStacks;
        }

        protected override void OnStatusApplicationFailed(
            in StatusContext context,
            in OperationResult result)
        {
            ApplyFailedCount++;
            LastExpectedStackCount = context.expectedStackCount;
        }

        protected override void OnStatusRemoved(in StatusContext context, in OperationResult result)
        {
            RemovedCount++;
            LastExpectedStackCount = context.expectedStackCount;
        }

        protected override void OnStatusRemovalFailed(
            in StatusContext context,
            in OperationResult result)
        {
            RemoveFailedCount++;
            LastExpectedStackCount = context.expectedStackCount;
        }

        protected override void OnStatusStackChanged(
            in StatusContext context,
            in OperationResult result,
            int currentStacks)
        {
            StackChangedCount++;
            LastExpectedStackCount = context.expectedStackCount;
            LastCurrentStackCount = currentStacks;
        }

        protected override void OnStatusTick(in StatusContext context, float deltaTime)
        {
            TickCount++;
            LastExpectedStackCount = context.expectedStackCount;
            LastDeltaTime = deltaTime;

            if (!RemoveSelfOnTick) return;

            context.entity.RemoveStatus(
                this,
                context.expectedStackCount,
                StatusModificationFlags.IgnoreConditions | StatusModificationFlags.IgnoreStackLimit);
        }
    }

    public sealed class TestOtherStatus : StatusBase
    {
        public int TickCount { get; private set; }

        protected override void OnStatusTick(in StatusContext context, float deltaTime)
        {
            TickCount++;
        }
    }

    public sealed class TestAffinity : AffinityType
    {
        public bool RejectDamage { get; set; }
        public bool RejectHeal { get; set; }
        public bool SaveFromDeath { get; set; }
        public long SavedHealth { get; set; } = 1;

        public int DamageReceivedCount { get; private set; }
        public int DamageFailedCount { get; private set; }
        public int HealReceivedCount { get; private set; }
        public int HealFailedCount { get; private set; }
        public int DeathCount { get; private set; }
        public int SavedFromDeathCount { get; private set; }
        public long LastHealthLost { get; private set; }
        public long LastHealthAdded { get; private set; }
        public long LastSavedHealth { get; private set; }

        protected override OperationResult CanBeDamaged(in DamageContext context)
        {
            if (RejectDamage) return EntityOperations.NotPermitted();
            return base.CanBeDamaged(context);
        }

        protected override OperationResult CanBeHealed(in HealContext context)
        {
            if (RejectHeal) return EntityOperations.NotPermitted();
            return base.CanBeHealed(context);
        }

        protected override DeathSaveContext CanSaveFromDeath(in DamageContext context)
        {
            if (SaveFromDeath) return new DeathSaveContext(true, SavedHealth);
            return base.CanSaveFromDeath(context);
        }

        protected override void OnDamageReceived(
            in DamageContext context,
            in OperationResult result,
            long healthLost)
        {
            DamageReceivedCount++;
            LastHealthLost = healthLost;
        }

        protected override void OnDamageFailed(in DamageContext context, in OperationResult result)
        {
            DamageFailedCount++;
        }

        protected override void OnDeath(in DamageContext context, in OperationResult result, long healthLost)
        {
            DeathCount++;
            LastHealthLost = healthLost;
        }

        protected override void OnHealingReceived(
            in HealContext context,
            in OperationResult result,
            long healthAdded)
        {
            HealReceivedCount++;
            LastHealthAdded = healthAdded;
        }

        protected override void OnHealingFailed(in HealContext context, in OperationResult result)
        {
            HealFailedCount++;
        }

        protected override void OnSavedFromDeath(
            in DamageContext damageContext,
            in DeathSaveContext context,
            in OperationResult result,
            long healthSet)
        {
            SavedFromDeathCount++;
            LastSavedHealth = healthSet;
        }
    }

    public sealed class OtherTestAffinity : AffinityType
    {
    }

    public abstract class TestResistanceBase : ResistanceBase
    {
        public void SetBaseValueForTests(float value)
        {
            BaseValue = value;
        }
    }

    public sealed class TestFireResistance : TestResistanceBase, IResistance<TestAffinity>
    {
    }

    public sealed class TestOtherResistance : TestResistanceBase, IResistance<OtherTestAffinity>
    {
    }

    public sealed class DirectStatModifier : IStatModifier
    {
        private readonly StatisticBase _statistic;
        private readonly float _amount;

        public DirectStatModifier(StatisticBase statistic, float amount)
        {
            _statistic = statistic;
            _amount = amount;
        }

        public int Order => 0;

        public void Apply(ref float currentFloat)
        {
            currentFloat += _amount;
        }

        public bool IsValidFor<TStatisticType>()
            where TStatisticType : StatisticBase
        {
            return _statistic is TStatisticType;
        }

        public bool IsValidFor(StatisticBase statistic)
        {
            return ReferenceEquals(_statistic, statistic);
        }

        public StatisticBase GetStatistic()
        {
            return _statistic;
        }
    }

    public sealed class TestTimedModifier : IStatModifier, ITimedModifier
    {
        public TestTimedModifier(float duration)
        {
            TotalDuration = duration;
            TimeRemaining = duration;
        }

        public int Order => 0;
        public float TimeRemaining { get; set; }
        public float TotalDuration { get; }

        public void Apply(ref float currentFloat)
        {
        }

        public bool IsValidFor<TStatisticType>()
            where TStatisticType : StatisticBase
        {
            return false;
        }

        public bool IsValidFor(StatisticBase statistic)
        {
            return false;
        }

        public StatisticBase GetStatistic()
        {
            return null;
        }
    }
}
