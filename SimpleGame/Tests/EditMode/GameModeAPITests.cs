using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleGame.Data.Enums;
using Systems.SimpleGame.Operations;
using Systems.SimpleGame.Utility;

namespace Systems.SimpleGame.Tests
{
    public sealed class GameModeAPITests : SimpleGameTestBase
    {
        [Test]
        public void TrySet_WhenModeIsMissing_ReturnsNotFound()
        {
            OperationResult result = GameModeAPI.TrySet<TestGameMode>();

            AssertSimilar(GameModeOperations.GameModeNotFound(), result);
            Assert.IsFalse(GameModeAPI.HasCurrent);
        }

        [Test]
        public void TrySetAndClear_WhenPermitted_TracksModeAndInvokesLifecycle()
        {
            TestGameMode mode = CreateRegisteredGameMode<TestGameMode>();

            OperationResult setResult = GameModeAPI.TrySet<TestGameMode>();
            Assert.AreSame(mode, GameModeAPI.Current);
            GameModeAPI.TickForTests(0.25f);
            OperationResult clearResult = GameModeAPI.TryClear();

            AssertSimilar(GameModeOperations.Changed(), setResult);
            AssertSimilar(GameModeOperations.Cleared(), clearResult);
            Assert.AreEqual(1, mode.EntryChecks);
            Assert.AreEqual(1, mode.EnteredCount);
            Assert.AreEqual(1, mode.TickCount);
            Assert.IsFalse(GameModeAPI.HasCurrent);
        }

        [Test]
        public void TrySet_WhenForceFlagIsUsed_SkipsModeEntryCheck()
        {
            TestGameMode mode = CreateRegisteredGameMode<TestGameMode>();
            mode.RejectEntry = true;

            OperationResult result = GameModeAPI.TrySet<TestGameMode>(GameModeChangeFlags.IgnoreConditions);

            AssertSimilar(GameModeOperations.Changed(), result);
            Assert.AreSame(mode, GameModeAPI.Current);
            Assert.AreEqual(0, mode.EntryChecks);
            Assert.AreEqual(1, mode.EnteredCount);
        }
    }
}
