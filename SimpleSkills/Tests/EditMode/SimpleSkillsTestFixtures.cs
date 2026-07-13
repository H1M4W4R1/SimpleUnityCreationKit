using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Components;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Context;
using Systems.SimpleSkills.Data.Enums;
using Systems.SimpleSkills.Operations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleSkills.Tests
{
    public abstract class SimpleSkillsTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = _createdObjects.Count - 1; i >= 0; i--)
            {
                Object createdObject = _createdObjects[i];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        protected TestSkillCaster CreateCaster()
        {
            GameObject gameObject = Track(new GameObject("Test Skill Caster"));
            gameObject.SetActive(false);
            return gameObject.AddComponent<TestSkillCaster>();
        }

        protected TestSkillTarget CreateTarget()
        {
            GameObject gameObject = Track(new GameObject("Test Skill Target"));
            gameObject.SetActive(false);
            return gameObject.AddComponent<TestSkillTarget>();
        }

        protected TSkill CreateSkill<TSkill>()
            where TSkill : SkillBase
        {
            TSkill skill = Track(ScriptableObject.CreateInstance<TSkill>());
            skill.name = typeof(TSkill).Name;
            return skill;
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject)
            where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected static void AssertSimilar(in OperationResult expected, in OperationResult actual)
        {
            Assert.AreEqual(expected.systemCode, actual.systemCode);
            Assert.AreEqual(expected.resultCode, actual.resultCode);
        }
    }

    public sealed class TestSkillCaster : SkillCasterBase
    {
        public int LevelToReturn { get; set; } = 1;

        public void ExecuteTick(float deltaTime)
        {
            OnTickExecuted(deltaTime);
        }

        public OperationResult ActivateForTests(SkillBase skill, ISkillTarget target)
        {
            return ActivateSkill(skill, target);
        }

        public OperationResult DeactivateForTests(SkillBase skill, ISkillTarget target)
        {
            return DeactivateSkill(skill, target);
        }

        public int GetActiveStackCountForTests(SkillBase skill)
        {
            return GetActiveStackCount(skill);
        }

        public int GetAvailableChargesForTests(SkillBase skill, int maxCharges)
        {
            return GetAvailableCharges(skill, maxCharges);
        }

        public bool IsSkillActivatedForTests(SkillBase skill)
        {
            return IsSkillActivated(skill);
        }

        protected override int GetSkillLevel(ISkillWithLevels skill)
        {
            return LevelToReturn;
        }

        protected override void Update()
        {
        }
    }

    public sealed class TestSkillTarget : MonoBehaviour, ISkillTarget
    {
    }

    public class TestSkill : SkillBase
    {
        public float ChargingTimeValue { get; set; }
        public float CooldownTimeValue { get; set; }
        public int MaxStacksValue { get; set; } = 1;
        public float InterruptedCooldownMultiplierValue { get; set; } = 1f;
        public bool RequiresTargetValue { get; set; }
        public bool AvailabilityPermitted { get; set; } = true;
        public bool ResourcesPermitted { get; set; } = true;
        public bool AttemptPermitted { get; set; } = true;
        public bool InterruptPermitted { get; set; }
        public int AvailabilityCheckCount { get; private set; }
        public int ResourceCheckCount { get; private set; }
        public int AttemptCheckCount { get; private set; }
        public int InterruptCheckCount { get; private set; }
        public int ResourcesConsumedCount { get; private set; }
        public int ResourcesRefundedCount { get; private set; }
        public int StartedCount { get; private set; }
        public int ChargingTickCount { get; private set; }
        public int EndedCount { get; private set; }
        public int FailedCount { get; private set; }
        public int InterruptedCount { get; private set; }
        public int InterruptFailedCount { get; private set; }
        public int CooldownTickCount { get; private set; }
        public int RegisteredCount { get; private set; }
        public int RemovedCount { get; private set; }
        public ushort LastFailureResultCode { get; private set; }
        public ushort LastInterruptFailureResultCode { get; private set; }
        public bool LastInterruptWasCancellation { get; private set; }
        public ISkillTarget LastStartedTarget { get; private set; }
        public ISkillTarget LastEndedTarget { get; private set; }
        public ISkillTarget LastRegisteredTarget { get; private set; }
        public ISkillTarget LastRemovedTarget { get; private set; }

        public override float ChargingTime => ChargingTimeValue;
        public override float CooldownTime => CooldownTimeValue;
        public override int MaxStacks => MaxStacksValue;
        public override float InterruptedCooldownMultiplier => InterruptedCooldownMultiplierValue;
        public override bool RequiresTarget => RequiresTargetValue;

        protected override OperationResult IsSkillAvailable(in CastSkillContext context)
        {
            AvailabilityCheckCount++;
            return AvailabilityPermitted ? SkillOperations.Permitted() : SkillOperations.Denied();
        }

        protected override OperationResult HasEnoughResources(in CastSkillContext context)
        {
            ResourceCheckCount++;
            return ResourcesPermitted ? SkillOperations.Permitted() : SkillOperations.Denied();
        }

        protected override OperationResult CheckAttemptSuccess(in CastSkillContext context)
        {
            AttemptCheckCount++;
            return AttemptPermitted ? SkillOperations.Permitted() : SkillOperations.Denied();
        }

        protected override void ConsumeResources(in CastSkillContext context)
        {
            ResourcesConsumedCount++;
        }

        protected override void RefundResources(in CastSkillContext context)
        {
            ResourcesRefundedCount++;
        }

        protected override void OnCastStarted(in CastSkillContext context)
        {
            StartedCount++;
            LastStartedTarget = context.target;
        }

        protected override void OnCastTickWhenCharging(in CastSkillContext context)
        {
            ChargingTickCount++;
        }

        protected override void OnCastEnded(in CastSkillContext context)
        {
            EndedCount++;
            LastEndedTarget = context.target;
        }

        protected override void OnCastFailed(in CastSkillContext context, in OperationResult reason)
        {
            FailedCount++;
            LastFailureResultCode = reason.resultCode;
        }

        protected override OperationResult CanBeInterrupted(in InterruptSkillContext context)
        {
            InterruptCheckCount++;
            return InterruptPermitted ? SkillOperations.Permitted() : SkillOperations.Denied();
        }

        protected override void OnCastInterrupted(in InterruptSkillContext context, in OperationResult reason)
        {
            InterruptedCount++;
            LastInterruptWasCancellation = context.IsCancellation;
        }

        protected override void OnCastInterruptFailed(in InterruptSkillContext context, in OperationResult reason)
        {
            InterruptFailedCount++;
            LastInterruptFailureResultCode = reason.resultCode;
        }

        protected override void OnCooldownTick(in CastSkillContext context)
        {
            CooldownTickCount++;
        }

        protected override void OnCastRegistered(in CastSkillContext context)
        {
            RegisteredCount++;
            LastRegisteredTarget = context.target;
        }

        protected override void OnCastRemoved(in CastSkillContext context)
        {
            RemovedCount++;
            LastRemovedTarget = context.target;
        }
    }

    public sealed class TestChannelSkill : TestSkill, IChannelingSkillBase
    {
        public float DurationValue { get; set; } = 1f;
        public int ChannelingTickCount { get; private set; }
        public ISkillTarget LastChannelTarget { get; private set; }

        public float Duration => DurationValue;

        void IChannelingSkillBase.OnCastTickWhenChanneling(in CastSkillContext context)
        {
            ChannelingTickCount++;
            LastChannelTarget = context.target;
        }
    }

    public sealed class TestChargeSkill : TestSkill, ISkillWithCharges
    {
        public int MaxChargesValue { get; set; } = 2;

        public int MaxCharges => MaxChargesValue;
    }

    public sealed class TestActivatedSkill : TestSkill, IActivatedSkill
    {
        public int ActivatedCount { get; private set; }
        public int DeactivatedCount { get; private set; }
        public int ActiveTickCount { get; private set; }
        public float LastTickDeltaTime { get; private set; }
        public ISkillTarget LastActivatedTarget { get; private set; }
        public ISkillTarget LastDeactivatedTarget { get; private set; }
        public ISkillTarget LastTickTarget { get; private set; }

        void IActivatedSkill.OnActivated(ISkillTarget target)
        {
            ActivatedCount++;
            LastActivatedTarget = target;
        }

        void IActivatedSkill.OnDeactivated(ISkillTarget target)
        {
            DeactivatedCount++;
            LastDeactivatedTarget = target;
        }

        void IActivatedSkill.OnTickWhileActive(ISkillTarget target, float deltaTime)
        {
            ActiveTickCount++;
            LastTickTarget = target;
            LastTickDeltaTime = deltaTime;
        }
    }

    public sealed class TestActivatedChargeSkill : TestSkill, IActivatedSkill, ISkillWithCharges
    {
        public int MaxCharges => 2;
    }

    public struct TestSharedSkillGroup : ISkillGroup
    {
        public float Cooldown => 2f;
    }

    public sealed class TestGroupedSkill : TestSkill, IWithSkillGroup<TestSharedSkillGroup>
    {
    }

    public sealed class TestOtherGroupedSkill : TestSkill, IWithSkillGroup<TestSharedSkillGroup>
    {
    }

    public sealed class TestManualLeveledSkill : TestSkill, ISkillWithLevels
    {
        public int LevelValue { get; set; } = 1;
        public int RequestedLevel { get; private set; }
        public SkillBase SkillForLevel { get; set; }

        public int Level => LevelValue;

        public SkillBase GetSkillForLevel(int level)
        {
            RequestedLevel = level;
            return SkillForLevel;
        }
    }
}
