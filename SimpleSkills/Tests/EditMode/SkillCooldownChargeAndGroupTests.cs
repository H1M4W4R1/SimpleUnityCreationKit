using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Data.Enums;
using Systems.SimpleSkills.Operations;

namespace Systems.SimpleSkills.Tests
{
    public sealed class SkillCooldownChargeAndGroupTests : SimpleSkillsTestBase
    {
        [Test]
        public void TryCastSkill_StandardCooldown_BlocksUntilCooldownFinishes()
        {
            TestSkillCaster caster = CreateCaster();
            TestSkill skill = CreateSkill<TestSkill>();
            skill.CooldownTimeValue = 1f;

            OperationResult firstResult = caster.TryCastSkill(skill, caster);
            caster.ExecuteTick(0.25f);
            OperationResult blockedResult = caster.TryCastSkill(skill, caster);
            caster.ExecuteTick(0.75f);
            OperationResult secondResult = caster.TryCastSkill(skill, caster);

            AssertSimilar(SkillOperations.Casted(), firstResult);
            AssertSimilar(SkillOperations.CooldownNotFinished(), blockedResult);
            AssertSimilar(SkillOperations.Casted(), secondResult);
            Assert.AreEqual(1, skill.FailedCount);
        }

        [Test]
        public void CooldownQueries_ReportProgressTimeLeftAndBlockingState()
        {
            TestSkillCaster caster = CreateCaster();
            TestSkill skill = CreateSkill<TestSkill>();
            skill.CooldownTimeValue = 1f;

            caster.TryCastSkill(skill, caster);
            caster.ExecuteTick(0.25f);

            Assert.AreEqual(1f, caster.GetSkillEffectiveCooldown(skill), 0.001f);
            Assert.AreEqual(0.25f, caster.GetSkillEffectiveCooldownPercentage(skill), 0.001f);
            Assert.AreEqual(0.75f, caster.GetSkillCooldownTimeLeft(skill), 0.001f);
            Assert.IsTrue(caster.IsSkillOnAnyCooldown(skill));

            caster.ExecuteTick(0.75f);

            Assert.AreEqual(1f, caster.GetSkillEffectiveCooldownPercentage(skill), 0.001f);
            Assert.AreEqual(0f, caster.GetSkillCooldownTimeLeft(skill), 0.001f);
            Assert.IsFalse(caster.IsSkillOnAnyCooldown(skill));
        }

        [Test]
        public void TryCastSkill_ChargeSkill_ConsumesAndRestoresChargesIndependently()
        {
            TestSkillCaster caster = CreateCaster();
            TestChargeSkill skill = CreateSkill<TestChargeSkill>();
            skill.MaxChargesValue = 2;
            skill.CooldownTimeValue = 1f;

            OperationResult firstResult = caster.TryCastSkill(skill, caster);
            OperationResult secondResult = caster.TryCastSkill(skill, caster);
            OperationResult blockedResult = caster.TryCastSkill(skill, caster);

            AssertSimilar(SkillOperations.Casted(), firstResult);
            AssertSimilar(SkillOperations.Casted(), secondResult);
            AssertSimilar(SkillOperations.NoChargesAvailable(), blockedResult);
            Assert.AreEqual(0, caster.GetAvailableChargesForTests(skill, skill.MaxCharges));

            caster.ExecuteTick(0.5f);

            Assert.AreEqual(0, caster.GetAvailableChargesForTests(skill, skill.MaxCharges));
            Assert.AreEqual(2, caster.CurrentlyCastedSkills.Count);

            caster.ExecuteTick(0.5f);

            Assert.AreEqual(2, caster.GetAvailableChargesForTests(skill, skill.MaxCharges));
            Assert.AreEqual(0, caster.CurrentlyCastedSkills.Count);
        }

        [Test]
        public void TryCastSkill_GroupCooldown_BlocksOtherSkillInSameGroup()
        {
            TestSkillCaster caster = CreateCaster();
            TestGroupedSkill firstSkill = CreateSkill<TestGroupedSkill>();
            TestOtherGroupedSkill secondSkill = CreateSkill<TestOtherGroupedSkill>();

            OperationResult firstResult = caster.TryCastSkill(firstSkill, caster);
            OperationResult blockedResult = caster.TryCastSkill(secondSkill, caster);
            OperationResult ignoredResult = caster.TryCastSkill(secondSkill, caster, SkillCastFlags.IgnoreCooldown);

            AssertSimilar(SkillOperations.Casted(), firstResult);
            AssertSimilar(SkillOperations.GroupCooldownNotFinished(), blockedResult);
            AssertSimilar(SkillOperations.Casted(), ignoredResult);
            Assert.IsTrue(caster.IsGroupOnCooldown<TestSharedSkillGroup>());

            caster.ExecuteTick(2f);

            Assert.IsFalse(caster.IsGroupOnCooldown<TestSharedSkillGroup>());
        }

        [Test]
        public void GroupCooldownQueries_UseLeastProgressedBlockingCooldown()
        {
            TestSkillCaster caster = CreateCaster();
            TestGroupedSkill firstSkill = CreateSkill<TestGroupedSkill>();
            TestOtherGroupedSkill secondSkill = CreateSkill<TestOtherGroupedSkill>();
            secondSkill.CooldownTimeValue = 0.5f;

            caster.TryCastSkill(firstSkill, caster);
            caster.ExecuteTick(0.5f);

            Assert.AreEqual(2f, caster.GetSkillEffectiveCooldown(secondSkill), 0.001f);
            Assert.AreEqual(0.25f, caster.GetSkillEffectiveCooldownPercentage(secondSkill), 0.001f);
            Assert.AreEqual(1.5f, caster.GetSkillCooldownTimeLeft(secondSkill), 0.001f);
            Assert.IsTrue(caster.IsSkillOnAnyCooldown(secondSkill));
        }
    }
}
