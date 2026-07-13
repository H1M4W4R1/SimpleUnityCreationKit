using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleGame.Data.Enums;
using Systems.SimpleGame.Operations;
using Systems.SimpleGame.Utility;

namespace Systems.SimpleGame.Tests
{
    public sealed class GameStateAPITests : SimpleGameTestBase
    {
        [Test]
        public void TrySet_WhenStateIsMissing_ReturnsNotFound()
        {
            OperationResult result = GameStateAPI.TrySet<TestGameState>();

            AssertSimilar(GameStateOperations.GameStateNotFound(), result);
            Assert.IsFalse(GameStateAPI.HasCurrent);
        }

        [Test]
        public void TrySet_WhenPermitted_TracksStateAndInvokesLifecycle()
        {
            TestGameState firstState = CreateRegisteredGameState<TestGameState>();
            OtherTestGameState secondState = CreateRegisteredGameState<OtherTestGameState>();

            OperationResult firstResult = GameStateAPI.TrySet<TestGameState>();
            OperationResult secondResult = GameStateAPI.TrySet<OtherTestGameState>();

            AssertSimilar(GameStateOperations.Changed(), firstResult);
            AssertSimilar(GameStateOperations.Changed(), secondResult);
            Assert.AreSame(secondState, GameStateAPI.Current);
            Assert.IsTrue(GameStateAPI.IsCurrent<OtherTestGameState>());
            Assert.AreEqual(1, firstState.EntryChecks);
            Assert.AreEqual(1, firstState.EnteredCount);
            Assert.AreEqual(1, firstState.ExitChecks);
            Assert.AreEqual(1, firstState.ExitedCount);
            Assert.AreEqual(1, secondState.EntryChecks);
            Assert.AreEqual(1, secondState.EnteredCount);
        }

        [Test]
        public void TrySet_WhenExitIsDenied_PreservesCurrentStateAndInvokesFailure()
        {
            TestGameState firstState = CreateRegisteredGameState<TestGameState>();
            CreateRegisteredGameState<OtherTestGameState>();
            GameStateAPI.TrySet<TestGameState>();
            firstState.RejectExit = true;

            OperationResult result = GameStateAPI.TrySet<OtherTestGameState>();

            AssertSimilar(GameStateOperations.Denied(), result);
            Assert.AreSame(firstState, GameStateAPI.Current);
            Assert.AreEqual(1, firstState.ExitFailedCount);
        }

        [Test]
        public void TrySet_WhenForceFlagIsUsed_SkipsChecksButRunsCallbacks()
        {
            TestGameState state = CreateRegisteredGameState<TestGameState>();
            state.RejectEntry = true;

            OperationResult result = GameStateAPI.TrySet<TestGameState>(GameStateChangeFlags.IgnoreConditions);

            AssertSimilar(GameStateOperations.Changed(), result);
            Assert.AreSame(state, GameStateAPI.Current);
            Assert.AreEqual(0, state.EntryChecks);
            Assert.AreEqual(1, state.EnteredCount);
        }

        [Test]
        public void TickAndClear_ForwardTickThenClearTheCurrentState()
        {
            TestGameState state = CreateRegisteredGameState<TestGameState>();
            GameStateAPI.TrySet<TestGameState>();

            GameStateAPI.TickForTests(0.25f);
            OperationResult result = GameStateAPI.TryClear();

            Assert.AreEqual(1, state.TickCount);
            Assert.AreEqual(1, state.ExitedCount);
            AssertSimilar(GameStateOperations.Cleared(), result);
            Assert.IsFalse(GameStateAPI.HasCurrent);
        }
    }
}
