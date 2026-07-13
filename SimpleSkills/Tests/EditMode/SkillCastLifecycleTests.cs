using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Data.Internal;
using Systems.SimpleSkills.Operations;

namespace Systems.SimpleSkills.Tests
{
    public sealed class SkillCastLifecycleTests : SimpleSkillsTestBase
    {
        [Test]
        public void TryCastSkill_InstantSkill_RegistersStartsEndsAndRemovesOnTick()
        {
            TestSkillCaster caster = CreateCaster();
            TestSkill skill = CreateSkill<TestSkill>();

            OperationResult result = caster.TryCastSkill(skill, caster);

            AssertSimilar(SkillOperations.Casted(), result);
            Assert.AreEqual(1, skill.RegisteredCount);
            Assert.AreEqual(1, skill.StartedCount);
            Assert.AreEqual(0, skill.EndedCount);
            Assert.AreEqual(1, caster.CurrentlyCastedSkills.Count);
            Assert.AreEqual(SkillState.Complete, caster.CurrentlyCastedSkills[0].skillState);

            caster.ExecuteTick(0.1f);

            Assert.AreEqual(1, skill.EndedCount);
            Assert.AreEqual(1, skill.CooldownTickCount);
            Assert.AreEqual(1, skill.RemovedCount);
            Assert.AreEqual(0, caster.CurrentlyCastedSkills.Count);
        }

        [Test]
        public void TryCastSkill_ChargedSkill_StartsOnlyAfterChargeCompletes()
        {
            TestSkillCaster caster = CreateCaster();
            TestSkill skill = CreateSkill<TestSkill>();
            skill.ChargingTimeValue = 1f;

            OperationResult result = caster.TryCastSkill(skill, caster);

            AssertSimilar(SkillOperations.Casted(), result);
            Assert.AreEqual(1, skill.RegisteredCount);
            Assert.AreEqual(0, skill.StartedCount);
            Assert.AreEqual(SkillState.Charging, caster.CurrentlyCastedSkills[0].skillState);

            caster.ExecuteTick(0.5f);

            Assert.AreEqual(1, skill.ChargingTickCount);
            Assert.AreEqual(0, skill.StartedCount);
            Assert.AreEqual(0.5f, caster.CurrentlyCastedSkills[0].ChargingProgress, 0.001f);

            caster.ExecuteTick(0.5f);

            Assert.AreEqual(2, skill.ChargingTickCount);
            Assert.AreEqual(1, skill.StartedCount);
            Assert.AreEqual(1, skill.EndedCount);
            Assert.AreEqual(1, skill.RemovedCount);
            Assert.AreEqual(0, caster.CurrentlyCastedSkills.Count);
        }

        [Test]
        public void TryCastSkill_ChannelingSkill_TicksUntilDurationCompletes()
        {
            TestSkillCaster caster = CreateCaster();
            TestChannelSkill skill = CreateSkill<TestChannelSkill>();
            skill.DurationValue = 1f;

            OperationResult result = caster.TryCastSkill(skill, caster);

            AssertSimilar(SkillOperations.Casted(), result);
            Assert.AreEqual(1, skill.StartedCount);
            Assert.AreEqual(SkillState.Channeling, caster.CurrentlyCastedSkills[0].skillState);

            caster.ExecuteTick(0.4f);

            Assert.AreEqual(1, skill.ChannelingTickCount);
            Assert.AreEqual(0, skill.EndedCount);
            Assert.AreEqual(0.4f, caster.CurrentlyCastedSkills[0].ChannelingProgress, 0.001f);

            caster.ExecuteTick(0.6f);

            Assert.AreEqual(2, skill.ChannelingTickCount);
            Assert.AreEqual(1, skill.EndedCount);
            Assert.AreEqual(1, skill.RemovedCount);
            Assert.AreEqual(0, caster.CurrentlyCastedSkills.Count);
        }

        [Test]
        public void TryCastSkill_InfiniteChannelingSkill_RemainsUntilInterrupted()
        {
            TestSkillCaster caster = CreateCaster();
            TestChannelSkill skill = CreateSkill<TestChannelSkill>();
            skill.DurationValue = 0f;
            skill.InterruptPermitted = true;

            caster.TryCastSkill(skill, caster);
            caster.ExecuteTick(10f);

            Assert.AreEqual(1, skill.ChannelingTickCount);
            Assert.AreEqual(0, skill.EndedCount);
            Assert.AreEqual(1, caster.CurrentlyCastedSkills.Count);
            Assert.AreEqual(-1f, caster.CurrentlyCastedSkills[0].ChannelingProgress);

            OperationResult cancelResult = caster.TryCancelSkill(skill);
            caster.ExecuteTick(0.1f);

            AssertSimilar(SkillOperations.Permitted(), cancelResult);
            Assert.AreEqual(1, skill.InterruptedCount);
            Assert.IsTrue(skill.LastInterruptWasCancellation);
            Assert.AreEqual(1, skill.RemovedCount);
        }
    }
}
