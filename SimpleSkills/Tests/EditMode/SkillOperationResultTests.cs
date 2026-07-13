using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Operations;

namespace Systems.SimpleSkills.Tests
{
    public sealed class SkillOperationResultTests : SimpleSkillsTestBase
    {
        [Test]
        public void SuccessFactories_CreateExpectedSkillSystemResults()
        {
            OperationResult permitted = SkillOperations.Permitted();
            OperationResult casted = SkillOperations.Casted();
            OperationResult deactivated = SkillOperations.SkillDeactivated();

            Assert.IsTrue(permitted);
            Assert.IsTrue(casted);
            Assert.IsTrue(deactivated);
            Assert.AreEqual(SkillOperations.SYSTEM_SKILL, permitted.systemCode);
            Assert.AreEqual(SkillOperations.SYSTEM_SKILL, casted.systemCode);
            Assert.AreEqual(SkillOperations.SYSTEM_SKILL, deactivated.systemCode);
            Assert.AreEqual(OperationResult.SUCCESS_PERMITTED, permitted.resultCode);
            Assert.AreEqual(SkillOperations.SUCCESS_CAST_STARTED, casted.resultCode);
            Assert.AreEqual(SkillOperations.SUCCESS_SKILL_DEACTIVATED, deactivated.resultCode);
        }

        [Test]
        public void ErrorFactories_CreateExpectedSkillSystemResults()
        {
            ushort[] expectedCodes =
            {
                OperationResult.ERROR_DENIED,
                SkillOperations.ERROR_COOLDOWN_NOT_FINISHED,
                SkillOperations.ERROR_SKILL_NOT_CASTED,
                SkillOperations.ERROR_SKILL_ALREADY_ACTIVE,
                SkillOperations.ERROR_SKILL_MAX_STACKS,
                SkillOperations.ERROR_SKILL_NOT_FOUND,
                SkillOperations.ERROR_NO_TARGET_SELECTED,
                SkillOperations.ERROR_NO_CHARGES_AVAILABLE,
                SkillOperations.ERROR_GROUP_COOLDOWN_NOT_FINISHED,
                SkillOperations.ERROR_PASSIVE_NOT_ACTIVE,
                SkillOperations.ERROR_FORBIDDEN
            };

            for (int index = 0; index < expectedCodes.Length; index++)
            {
                OperationResult result = CreateErrorResult(index);
                Assert.IsFalse(result);
                Assert.IsTrue(OperationResult.IsError(result));
                Assert.IsTrue(OperationResult.IsFromSystem(result, SkillOperations.SYSTEM_SKILL));
                Assert.AreEqual(expectedCodes[index], result.resultCode);
            }
        }

        private static OperationResult CreateErrorResult(int index)
        {
            switch (index)
            {
                case 0:
                    return SkillOperations.Denied();
                case 1:
                    return SkillOperations.CooldownNotFinished();
                case 2:
                    return SkillOperations.SkillNotCasted();
                case 3:
                    return SkillOperations.SkillAlreadyBeingCast();
                case 4:
                    return SkillOperations.SkillMaxStacks();
                case 5:
                    return SkillOperations.SkillNotFound();
                case 6:
                    return SkillOperations.NoTargetSelected();
                case 7:
                    return SkillOperations.NoChargesAvailable();
                case 8:
                    return SkillOperations.GroupCooldownNotFinished();
                case 9:
                    return SkillOperations.PassiveNotActive();
                case 10:
                    return SkillOperations.Forbidden();
                default:
                    Assert.Fail("Unhandled skill operation result index " + index);
                    return SkillOperations.Forbidden();
            }
        }
    }
}
