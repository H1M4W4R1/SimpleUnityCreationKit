using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Data.Enums;
using Systems.SimpleSkills.Operations;

namespace Systems.SimpleSkills.Tests
{
    public sealed class SkillValidationAndFlagsTests : SimpleSkillsTestBase
    {
        [Test]
        public void TryCastSkill_WhenTargetRequiredAndMissing_FailsBeforeRegistration()
        {
            TestSkillCaster caster = CreateCaster();
            TestSkill skill = CreateSkill<TestSkill>();
            skill.RequiresTargetValue = true;

            OperationResult result = caster.TryCastSkill(skill, null);

            AssertSimilar(SkillOperations.NoTargetSelected(), result);
            Assert.AreEqual(1, skill.FailedCount);
            Assert.AreEqual(SkillOperations.ERROR_NO_TARGET_SELECTED, skill.LastFailureResultCode);
            Assert.AreEqual(0, skill.RegisteredCount);
        }

        [Test]
        public void TryCastSkill_WhenTargetProvided_PreservesTargetInCallbacks()
        {
            TestSkillCaster caster = CreateCaster();
            TestSkillTarget target = CreateTarget();
            TestSkill skill = CreateSkill<TestSkill>();
            skill.RequiresTargetValue = true;

            caster.TryCastSkill(skill, target);
            caster.ExecuteTick(0.1f);

            Assert.AreSame(target, skill.LastRegisteredTarget);
            Assert.AreSame(target, skill.LastStartedTarget);
            Assert.AreSame(target, skill.LastEndedTarget);
            Assert.AreSame(target, skill.LastRemovedTarget);
        }

        [Test]
        public void TryCastSkill_WhenAvailabilityDenied_IgnoreAvailabilityAllowsCast()
        {
            TestSkillCaster caster = CreateCaster();
            TestSkill skill = CreateSkill<TestSkill>();
            skill.AvailabilityPermitted = false;

            OperationResult deniedResult = caster.TryCastSkill(skill, caster);

            AssertSimilar(SkillOperations.Denied(), deniedResult);
            Assert.AreEqual(1, skill.FailedCount);
            Assert.AreEqual(0, skill.RegisteredCount);

            OperationResult ignoredResult = caster.TryCastSkill(skill, caster, SkillCastFlags.IgnoreAvailability);

            AssertSimilar(SkillOperations.Casted(), ignoredResult);
            Assert.AreEqual(1, skill.RegisteredCount);
        }

        [Test]
        public void TryCastSkill_WhenResourcesDenied_IgnoreCostsAllowsCastAndDoNotConsumeSkipsConsumption()
        {
            TestSkillCaster caster = CreateCaster();
            TestSkill skill = CreateSkill<TestSkill>();
            skill.ResourcesPermitted = false;

            OperationResult deniedResult = caster.TryCastSkill(skill, caster);

            AssertSimilar(SkillOperations.Denied(), deniedResult);
            Assert.AreEqual(1, skill.FailedCount);
            Assert.AreEqual(0, skill.ResourcesConsumedCount);

            OperationResult ignoredResult = caster.TryCastSkill(
                skill,
                caster,
                SkillCastFlags.IgnoreCosts | SkillCastFlags.DoNotConsumeResources);

            AssertSimilar(SkillOperations.Casted(), ignoredResult);
            Assert.AreEqual(0, skill.ResourcesConsumedCount);
        }

        [Test]
        public void TryCastSkill_WhenAttemptFails_RefundsOnlyWhenFlagIsSet()
        {
            TestSkillCaster caster = CreateCaster();
            TestSkill skill = CreateSkill<TestSkill>();
            skill.AttemptPermitted = false;

            OperationResult firstResult = caster.TryCastSkill(skill, caster);

            AssertSimilar(SkillOperations.Denied(), firstResult);
            Assert.AreEqual(1, skill.ResourcesConsumedCount);
            Assert.AreEqual(0, skill.ResourcesRefundedCount);

            OperationResult secondResult = caster.TryCastSkill(
                skill,
                caster,
                SkillCastFlags.RefundResourcesOnFailure);

            AssertSimilar(SkillOperations.Denied(), secondResult);
            Assert.AreEqual(2, skill.ResourcesConsumedCount);
            Assert.AreEqual(1, skill.ResourcesRefundedCount);
        }

        [Test]
        public void TryCastSkill_FailureAlwaysInvokesCallbacks()
        {
            TestSkillCaster caster = CreateCaster();
            TestSkill skill = CreateSkill<TestSkill>();
            skill.AvailabilityPermitted = false;

            OperationResult result = caster.TryCastSkill(skill, caster, SkillCastFlags.None);

            AssertSimilar(SkillOperations.Denied(), result);
            Assert.AreEqual(1, skill.FailedCount);
        }

        [Test]
        public void TryCastSkill_StackingFlag_AllowsOnlyUpToMaxStacks()
        {
            TestSkillCaster caster = CreateCaster();
            TestChannelSkill skill = CreateSkill<TestChannelSkill>();
            skill.DurationValue = 10f;
            skill.MaxStacksValue = 2;

            OperationResult firstResult = caster.TryCastSkill(skill, caster);
            OperationResult blockedResult = caster.TryCastSkill(skill, caster);
            OperationResult secondResult = caster.TryCastSkill(skill, caster, SkillCastFlags.AllowStacking);
            OperationResult maxStackResult = caster.TryCastSkill(skill, caster, SkillCastFlags.AllowStacking);

            AssertSimilar(SkillOperations.Casted(), firstResult);
            AssertSimilar(SkillOperations.SkillAlreadyBeingCast(), blockedResult);
            AssertSimilar(SkillOperations.Casted(), secondResult);
            AssertSimilar(SkillOperations.SkillMaxStacks(), maxStackResult);
            Assert.AreEqual(2, caster.GetActiveStackCountForTests(skill));
        }

        [Test]
        public void TryCastSkill_ResetOnRecast_CancelsPreviousInstantCompleteEntry()
        {
            TestSkillCaster caster = CreateCaster();
            TestSkill skill = CreateSkill<TestSkill>();

            caster.TryCastSkill(skill, caster);
            OperationResult result = caster.TryCastSkill(skill, caster, SkillCastFlags.ResetOnRecast);
            caster.ExecuteTick(0.1f);

            AssertSimilar(SkillOperations.Casted(), result);
            Assert.AreEqual(2, skill.RegisteredCount);
            Assert.AreEqual(2, skill.StartedCount);
            Assert.AreEqual(1, skill.EndedCount);
            Assert.AreEqual(2, skill.RemovedCount);
            Assert.AreEqual(0, caster.CurrentlyCastedSkills.Count);
        }
    }
}
