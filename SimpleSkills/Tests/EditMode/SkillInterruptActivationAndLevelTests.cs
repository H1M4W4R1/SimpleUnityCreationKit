using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Data.Enums;
using Systems.SimpleSkills.Operations;
using UnityEngine.TestTools;

namespace Systems.SimpleSkills.Tests
{
    public sealed class SkillInterruptActivationAndLevelTests : SimpleSkillsTestBase
    {
        [Test]
        public void TryCancelSkill_WhenInterruptDenied_ReportsFailureWithoutChangingCast()
        {
            TestSkillCaster caster = CreateCaster();
            TestChannelSkill skill = CreateSkill<TestChannelSkill>();
            skill.DurationValue = 10f;
            skill.InterruptPermitted = false;

            caster.TryCastSkill(skill, caster);
            OperationResult result = caster.TryCancelSkill(skill);

            AssertSimilar(SkillOperations.Denied(), result);
            Assert.AreEqual(1, skill.InterruptFailedCount);
            Assert.AreEqual(OperationResult.ERROR_DENIED, skill.LastInterruptFailureResultCode);
            Assert.AreEqual(1, caster.CurrentlyCastedSkills.Count);
        }

        [Test]
        public void TryCancelSkill_WithIgnoreRequirements_InterruptsAndRemovesWithoutCooldownWhenMultiplierIsZero()
        {
            TestSkillCaster caster = CreateCaster();
            TestChannelSkill skill = CreateSkill<TestChannelSkill>();
            skill.DurationValue = 10f;
            skill.CooldownTimeValue = 10f;
            skill.InterruptedCooldownMultiplierValue = 0f;

            caster.TryCastSkill(skill, caster);
            OperationResult result = caster.TryCancelSkill(skill, SkillInterruptFlags.IgnoreRequirements);
            caster.ExecuteTick(0.1f);

            AssertSimilar(SkillOperations.Denied(), result);
            Assert.AreEqual(1, skill.InterruptedCount);
            Assert.IsTrue(skill.LastInterruptWasCancellation);
            Assert.AreEqual(1, skill.RemovedCount);
            Assert.AreEqual(0, caster.CurrentlyCastedSkills.Count);
        }

        [Test]
        public void TryInterruptSkill_WithExternalSource_RecordsNonCancellationInterrupt()
        {
            TestSkillCaster caster = CreateCaster();
            TestChannelSkill skill = CreateSkill<TestChannelSkill>();
            object source = new object();
            skill.DurationValue = 10f;
            skill.InterruptPermitted = true;

            caster.TryCastSkill(skill, caster);
            OperationResult result = caster.TryInterruptSkill(skill, source);

            AssertSimilar(SkillOperations.Permitted(), result);
            Assert.AreEqual(1, skill.InterruptedCount);
            Assert.IsFalse(skill.LastInterruptWasCancellation);
        }

        [Test]
        public void TryCancelSkill_WhenSkillIsOnCooldown_ReturnsCooldownNotFinished()
        {
            TestSkillCaster caster = CreateCaster();
            TestSkill skill = CreateSkill<TestSkill>();
            skill.CooldownTimeValue = 1f;

            caster.TryCastSkill(skill, caster);
            caster.ExecuteTick(0.1f);
            OperationResult result = caster.TryCancelSkill(skill);

            AssertSimilar(SkillOperations.CooldownNotFinished(), result);
            Assert.AreEqual(1, skill.InterruptFailedCount);
        }

        [Test]
        public void InterruptedCooldown_UsesSkillMultiplier()
        {
            TestSkillCaster caster = CreateCaster();
            TestChannelSkill skill = CreateSkill<TestChannelSkill>();
            skill.DurationValue = 10f;
            skill.CooldownTimeValue = 10f;
            skill.InterruptedCooldownMultiplierValue = 0.5f;
            skill.InterruptPermitted = true;

            caster.TryCastSkill(skill, caster);
            caster.TryCancelSkill(skill);
            caster.ExecuteTick(1f);

            Assert.AreEqual(4f, caster.GetSkillCooldownTimeLeft(skill), 0.001f);
            Assert.IsTrue(caster.IsSkillOnAnyCooldown(skill));

            caster.ExecuteTick(4f);

            Assert.IsFalse(caster.IsSkillOnAnyCooldown(skill));
            Assert.AreEqual(1, skill.RemovedCount);
        }

        [Test]
        public void ActivatedSkill_CastTogglesActivationAndPreservesTarget()
        {
            TestSkillCaster caster = CreateCaster();
            TestSkillTarget target = CreateTarget();
            TestActivatedSkill skill = CreateSkill<TestActivatedSkill>();

            OperationResult castResult = caster.TryCastSkill(skill, target);
            caster.ExecuteTick(0.1f);
            caster.ExecuteTick(0.25f);
            OperationResult deactivateResult = caster.TryCastSkill(skill, target);

            AssertSimilar(SkillOperations.Casted(), castResult);
            AssertSimilar(SkillOperations.SkillDeactivated(), deactivateResult);
            Assert.AreEqual(1, skill.ActivatedCount);
            Assert.AreEqual(1, skill.DeactivatedCount);
            Assert.AreEqual(2, skill.ActiveTickCount);
            Assert.AreEqual(0.25f, skill.LastTickDeltaTime, 0.001f);
            Assert.AreSame(target, skill.LastActivatedTarget);
            Assert.AreSame(target, skill.LastDeactivatedTarget);
            Assert.AreSame(caster, skill.LastTickTarget);
            Assert.IsFalse(caster.IsSkillActivatedForTests(skill));
        }

        [Test]
        public void ActivateSkill_WhenSkillAlsoHasCharges_ReturnsForbidden()
        {
            TestSkillCaster caster = CreateCaster();
            TestActivatedChargeSkill skill = CreateSkill<TestActivatedChargeSkill>();
            LogAssert.Expect(UnityEngine.LogType.Error, "Activated skill " + skill.name + " cannot have charges");

            OperationResult result = caster.ActivateForTests(skill, caster);

            AssertSimilar(SkillOperations.Forbidden(), result);
            Assert.IsFalse(caster.IsSkillActivatedForTests(skill));
        }

        [Test]
        public void TryCastSkill_LeveledSkill_UsesCasterLevelAndResolvedSkill()
        {
            TestSkillCaster caster = CreateCaster();
            TestManualLeveledSkill rootSkill = CreateSkill<TestManualLeveledSkill>();
            TestSkill resolvedSkill = CreateSkill<TestSkill>();
            rootSkill.SkillForLevel = resolvedSkill;
            caster.LevelToReturn = 3;

            OperationResult result = caster.TryCastSkill(rootSkill, caster);

            AssertSimilar(SkillOperations.Casted(), result);
            Assert.AreEqual(3, rootSkill.RequestedLevel);
            Assert.AreEqual(0, rootSkill.RegisteredCount);
            Assert.AreEqual(1, resolvedSkill.RegisteredCount);
        }

        [Test]
        public void TryCastSkill_WhenLeveledSkillCannotResolve_ReturnsSkillNotFound()
        {
            TestSkillCaster caster = CreateCaster();
            TestManualLeveledSkill rootSkill = CreateSkill<TestManualLeveledSkill>();
            rootSkill.SkillForLevel = null;

            OperationResult result = caster.TryCastSkill(rootSkill, caster);

            AssertSimilar(SkillOperations.SkillNotFound(), result);
            Assert.AreEqual(0, rootSkill.RegisteredCount);
        }
    }
}
