using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleGame.Abstract;
using Systems.SimpleGame.Data;
using Systems.SimpleGame.Data.Context;
using Systems.SimpleGame.Operations;
using Systems.SimpleGame.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleGame.Tests
{
    public abstract class SimpleGameTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            GameStateAPI.ClearForTests();
            GameModeAPI.ClearForTests();
            GameStateDatabase.ClearForTests();
            GameModeDatabase.ClearForTests();
        }

        [TearDown]
        public void TearDown()
        {
            GameStateAPI.ClearForTests();
            GameModeAPI.ClearForTests();

            for (int objectIndex = _createdObjects.Count - 1; objectIndex >= 0; objectIndex--)
            {
                Object createdObject = _createdObjects[objectIndex];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
            GameStateDatabase.ClearForTests();
            GameModeDatabase.ClearForTests();
        }

        protected TGameState CreateRegisteredGameState<TGameState>()
            where TGameState : GameStateBase, new()
        {
            TGameState gameState = Track(ScriptableObject.CreateInstance<TGameState>());
            gameState.name = typeof(TGameState).Name;
            GameStateDatabase.RegisterForTests(gameState);
            return gameState;
        }

        protected TGameMode CreateRegisteredGameMode<TGameMode>()
            where TGameMode : GameModeBase, new()
        {
            TGameMode gameMode = Track(ScriptableObject.CreateInstance<TGameMode>());
            gameMode.name = typeof(TGameMode).Name;
            GameModeDatabase.RegisterForTests(gameMode);
            return gameMode;
        }

        protected static void AssertSimilar(OperationResult expected, OperationResult actual)
        {
            Assert.IsTrue(OperationResult.AreSimilar(expected, actual));
        }

        private TUnityObject Track<TUnityObject>(TUnityObject unityObject) where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }
    }

    public class TestGameState : GameStateBase
    {
        public bool RejectEntry { get; set; }
        public bool RejectExit { get; set; }
        public int EntryChecks { get; private set; }
        public int ExitChecks { get; private set; }
        public int EnteredCount { get; private set; }
        public int EntryFailedCount { get; private set; }
        public int ExitedCount { get; private set; }
        public int ExitFailedCount { get; private set; }
        public int TickCount { get; private set; }

        protected internal override OperationResult CanEnterGameState(in GameStateTransitionContext context)
        {
            EntryChecks++;
            return RejectEntry ? GameStateOperations.Denied() : base.CanEnterGameState(in context);
        }

        protected internal override OperationResult CanExitGameState(in GameStateTransitionContext context)
        {
            ExitChecks++;
            return RejectExit ? GameStateOperations.Denied() : base.CanExitGameState(in context);
        }

        protected internal override void OnGameStateEntered(
            in GameStateTransitionContext context, in OperationResult result)
        {
            EnteredCount++;
        }

        protected internal override void OnGameStateEnterFailed(
            in GameStateTransitionContext context, in OperationResult result)
        {
            EntryFailedCount++;
        }

        protected internal override void OnGameStateExited(
            in GameStateTransitionContext context, in OperationResult result)
        {
            ExitedCount++;
        }

        protected internal override void OnGameStateExitFailed(
            in GameStateTransitionContext context, in OperationResult result)
        {
            ExitFailedCount++;
        }

        protected internal override void OnGameStateTick(float deltaTime)
        {
            TickCount++;
        }
    }

    public sealed class OtherTestGameState : TestGameState
    {
    }

    public class TestGameMode : GameModeBase
    {
        public bool RejectEntry { get; set; }
        public int EntryChecks { get; private set; }
        public int EnteredCount { get; private set; }
        public int TickCount { get; private set; }

        protected internal override OperationResult CanEnterGameMode(in GameModeTransitionContext context)
        {
            EntryChecks++;
            return RejectEntry ? GameModeOperations.Denied() : base.CanEnterGameMode(in context);
        }

        protected internal override void OnGameModeEntered(
            in GameModeTransitionContext context, in OperationResult result)
        {
            EnteredCount++;
        }

        protected internal override void OnGameModeTick(float deltaTime)
        {
            TickCount++;
        }
    }
}
