using NUnit.Framework;
using Systems.SimpleSkills.Data.Internal;

namespace Systems.SimpleSkills.Tests
{
    public sealed class SkillStateMachineTests : SimpleSkillsTestBase
    {
        [Test]
        public void IsValidTransition_AllowsDocumentedTransitions()
        {
            SkillState[] fromStates =
            {
                SkillState.Charging,
                SkillState.Charging,
                SkillState.Charging,
                SkillState.Charging,
                SkillState.Channeling,
                SkillState.Channeling,
                SkillState.Channeling,
                SkillState.Complete,
                SkillState.Interrupted,
                SkillState.Cancelled
            };

            SkillState[] toStates =
            {
                SkillState.Channeling,
                SkillState.Complete,
                SkillState.Interrupted,
                SkillState.Cancelled,
                SkillState.Complete,
                SkillState.Interrupted,
                SkillState.Cancelled,
                SkillState.Cooldown,
                SkillState.Cooldown,
                SkillState.Cooldown
            };

            for (int i = 0; i < fromStates.Length; i++)
            {
                Assert.IsTrue(SkillCastStateMachine.IsValidTransition(fromStates[i], toStates[i]));
            }
        }

        [Test]
        public void TryTransitionTo_RejectsInvalidTransitionWithoutChangingState()
        {
            SkillCastStateMachine stateMachine = new SkillCastStateMachine(SkillState.Complete);

            bool transitioned = stateMachine.TryTransitionTo(SkillState.Cancelled);

            Assert.IsFalse(transitioned);
            Assert.AreEqual(SkillState.Complete, stateMachine.CurrentState);
        }

        [Test]
        public void ForceTransitionTo_ChangesStateWithoutValidation()
        {
            SkillCastStateMachine stateMachine = new SkillCastStateMachine(SkillState.Complete);

            stateMachine.ForceTransitionTo(SkillState.Cancelled);

            Assert.AreEqual(SkillState.Cancelled, stateMachine.CurrentState);
        }
    }
}
