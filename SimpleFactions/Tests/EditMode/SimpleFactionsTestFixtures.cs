using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Data;
using Systems.SimpleFactions.Data.Context;
using Systems.SimpleFactions.Operations;
using Systems.SimpleRelations.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleFactions.Tests
{
    public abstract class SimpleFactionsTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            FactionDatabase.ClearForTests();
            RelationTypeDatabase.ClearForTests();
            RelatableObjectDatabase.ClearForTests();
        }

        [TearDown]
        public void TearDown()
        {
            for (int objectIndex = _createdObjects.Count - 1; objectIndex >= 0; objectIndex--)
            {
                Object createdObject = _createdObjects[objectIndex];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
            FactionDatabase.ClearForTests();
            RelationTypeDatabase.ClearForTests();
            RelatableObjectDatabase.ClearForTests();
        }

        protected TestFactionMembership CreateMembership()
        {
            GameObject gameObject = Track(new GameObject("Test Faction Member"));
            gameObject.SetActive(false);
            gameObject.AddComponent<TestFactionHolder>();
            return gameObject.AddComponent<TestFactionMembership>();
        }

        protected TFactionType CreateRegisteredFaction<TFactionType>()
            where TFactionType : FactionBase<TestFactionHolder>, new()
        {
            TFactionType faction = Track(ScriptableObject.CreateInstance<TFactionType>());
            FactionDatabase.RegisterForTests(faction);
            return faction;
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject) where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected static void AssertSimilar(OperationResult expected, OperationResult actual)
        {
            Assert.IsTrue(OperationResult.AreSimilar(expected, actual));
        }
    }

    public sealed class TestFactionHolder : MonoBehaviour { }

    public sealed class TestFactionMembership : FactionMembershipBase<TestFactionHolder>
    {
        public bool RejectJoin { get; set; }
        public bool RejectLeave { get; set; }
        public int JoinCheckCount { get; private set; }
        public int LeaveCheckCount { get; private set; }
        public int JoinedCount { get; private set; }
        public int JoinFailedCount { get; private set; }
        public int LeftCount { get; private set; }
        public int LeaveFailedCount { get; private set; }
        public FactionBase LastFaction { get; private set; }
        public TestFactionHolder LastMember { get; private set; }

        protected override OperationResult CanJoinFaction<TFactionType>(in JoinFactionContext<TestFactionHolder> context)
        {
            JoinCheckCount++;
            Capture(context.member, context.faction);
            return RejectJoin ? FactionOperations.Denied() : base.CanJoinFaction<TFactionType>(in context);
        }

        protected override OperationResult CanLeaveFaction<TFactionType>(in LeaveFactionContext<TestFactionHolder> context)
        {
            LeaveCheckCount++;
            Capture(context.member, context.faction);
            return RejectLeave ? FactionOperations.Denied() : base.CanLeaveFaction<TFactionType>(in context);
        }

        protected override void OnJoinedFaction<TFactionType>(in JoinFactionContext<TestFactionHolder> context, in OperationResult result)
        {
            JoinedCount++;
            Capture(context.member, context.faction);
            base.OnJoinedFaction<TFactionType>(in context, in result);
        }

        protected override void OnJoinFailed<TFactionType>(in JoinFactionContext<TestFactionHolder> context, in OperationResult result)
        {
            JoinFailedCount++;
            Capture(context.member, context.faction);
            base.OnJoinFailed<TFactionType>(in context, in result);
        }

        protected override void OnLeftFaction<TFactionType>(in LeaveFactionContext<TestFactionHolder> context, in OperationResult result)
        {
            LeftCount++;
            Capture(context.member, context.faction);
            base.OnLeftFaction<TFactionType>(in context, in result);
        }

        protected override void OnLeaveFailed<TFactionType>(in LeaveFactionContext<TestFactionHolder> context, in OperationResult result)
        {
            LeaveFailedCount++;
            Capture(context.member, context.faction);
            base.OnLeaveFailed<TFactionType>(in context, in result);
        }

        private void Capture(TestFactionHolder member, FactionBase faction)
        {
            LastMember = member;
            LastFaction = faction;
        }
    }

    public sealed class TestFaction : FactionBase<TestFactionHolder>
    {
        public bool RejectJoin { get; set; }
        public bool RejectLeave { get; set; }
        public int JoinCheckCount { get; private set; }
        public int LeaveCheckCount { get; private set; }
        public int JoinedCount { get; private set; }
        public int JoinFailedCount { get; private set; }
        public int LeftCount { get; private set; }
        public int LeaveFailedCount { get; private set; }

        protected internal override OperationResult CanJoin(in JoinFactionContext<TestFactionHolder> context)
        {
            JoinCheckCount++;
            return RejectJoin ? FactionOperations.Denied() : base.CanJoin(in context);
        }

        protected internal override OperationResult CanLeave(in LeaveFactionContext<TestFactionHolder> context)
        {
            LeaveCheckCount++;
            return RejectLeave ? FactionOperations.Denied() : base.CanLeave(in context);
        }

        protected internal override void OnJoined(in JoinFactionContext<TestFactionHolder> context, in OperationResult result)
        {
            JoinedCount++;
        }

        protected internal override void OnJoinFailed(in JoinFactionContext<TestFactionHolder> context, in OperationResult result)
        {
            JoinFailedCount++;
        }

        protected internal override void OnLeft(in LeaveFactionContext<TestFactionHolder> context, in OperationResult result)
        {
            LeftCount++;
        }

        protected internal override void OnLeaveFailed(in LeaveFactionContext<TestFactionHolder> context, in OperationResult result)
        {
            LeaveFailedCount++;
        }
    }

    public sealed class OtherTestFaction : FactionBase<TestFactionHolder> { }
}
