using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Data;
using Systems.SimpleFactions.Data.Context;
using Systems.SimpleFactions.Interfaces;
using Systems.SimpleFactions.Operations;
using UnityEditor;
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
            ReputationLevelDatabase.ClearForTests();
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
            ReputationLevelDatabase.ClearForTests();
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
            faction.name = typeof(TFactionType).Name;
            FactionDatabase.RegisterForTests(faction);
            return faction;
        }

        protected TLevelType CreateRegisteredLevel<TLevelType>(
            bool automaticPromotion = false,
            long promotionThreshold = 0L,
            bool automaticDemotion = false,
            long demotionThreshold = 0L)
            where TLevelType : ReputationLevelBase
        {
            TLevelType level = Track(ScriptableObject.CreateInstance<TLevelType>());
            level.name = typeof(TLevelType).Name;
            ConfigureLevel(level, automaticPromotion, promotionThreshold, automaticDemotion, demotionThreshold);
            ReputationLevelDatabase.RegisterForTests(level);
            return level;
        }

        protected static void ConfigureLevel(
            ReputationLevelBase level,
            bool automaticPromotion,
            long promotionThreshold,
            bool automaticDemotion,
            long demotionThreshold)
        {
            SerializedObject serializedObject = new SerializedObject(level);

            SerializedProperty automaticPromotionProperty = serializedObject.FindProperty("_automaticPromotion");
            Assert.IsNotNull(automaticPromotionProperty);
            automaticPromotionProperty.boolValue = automaticPromotion;

            SerializedProperty promotionThresholdProperty = serializedObject.FindProperty("_promotionThreshold");
            Assert.IsNotNull(promotionThresholdProperty);
            promotionThresholdProperty.longValue = promotionThreshold;

            SerializedProperty automaticDemotionProperty = serializedObject.FindProperty("_automaticDemotion");
            Assert.IsNotNull(automaticDemotionProperty);
            automaticDemotionProperty.boolValue = automaticDemotion;

            SerializedProperty demotionThresholdProperty = serializedObject.FindProperty("_demotionThreshold");
            Assert.IsNotNull(demotionThresholdProperty);
            demotionThresholdProperty.longValue = demotionThreshold;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject)
            where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected static void AssertSimilar(OperationResult expected, OperationResult actual)
        {
            Assert.IsTrue(
                OperationResult.AreSimilar(expected, actual),
                "Expected similar result to " + expected + " but received " + actual);
        }

    }

    public sealed class TestFactionHolder : MonoBehaviour
    {
    }

    public sealed class TestFactionMembership : FactionMembershipBase<TestFactionHolder>
    {
        public bool RejectJoin { get; set; }
        public bool RejectLeave { get; set; }
        public bool RejectReputationChange { get; set; }
        public bool RejectPromotion { get; set; }
        public bool RejectDemotion { get; set; }

        public int JoinCheckCount { get; private set; }
        public int LeaveCheckCount { get; private set; }
        public int ReputationCheckCount { get; private set; }
        public int PromotionCheckCount { get; private set; }
        public int DemotionCheckCount { get; private set; }
        public int JoinedCount { get; private set; }
        public int JoinFailedCount { get; private set; }
        public int LeftCount { get; private set; }
        public int LeaveFailedCount { get; private set; }
        public int ReputationChangedCount { get; private set; }
        public int ReputationFailedCount { get; private set; }
        public FactionBase LastFaction { get; private set; }
        public TestFactionHolder LastMember { get; private set; }
        public long LastPreviousReputation { get; private set; }
        public long LastAmountRequested { get; private set; }
        public ushort LastSystemCode { get; private set; }
        public ushort LastResultCode { get; private set; }

        protected override OperationResult CanJoinFaction<TFactionType>(
            in JoinFactionContext<TestFactionHolder> context)
        {
            JoinCheckCount++;
            CaptureContext(context.member, context.faction);
            if (RejectJoin) return FactionOperations.Denied();
            return base.CanJoinFaction<TFactionType>(context);
        }

        protected override OperationResult CanLeaveFaction<TFactionType>(
            in LeaveFactionContext<TestFactionHolder> context)
        {
            LeaveCheckCount++;
            CaptureContext(context.member, context.faction);
            if (RejectLeave) return FactionOperations.Denied();
            return base.CanLeaveFaction<TFactionType>(context);
        }

        protected override OperationResult CanChangeReputation<TFactionType>(
            in ReputationChangeContext<TestFactionHolder> context)
        {
            ReputationCheckCount++;
            LastPreviousReputation = context.previousReputation;
            LastAmountRequested = context.amountRequested;
            CaptureContext(context.member, context.faction);
            if (RejectReputationChange) return FactionOperations.Denied();
            return base.CanChangeReputation<TFactionType>(context);
        }

        protected override OperationResult CanBePromoted<TFactionType>(
            in FactionLevelChangeContext<TestFactionHolder> context)
        {
            PromotionCheckCount++;
            CaptureContext(context.member, context.faction);
            if (RejectPromotion) return FactionOperations.PromotionDenied();
            return base.CanBePromoted<TFactionType>(context);
        }

        protected override OperationResult CanBeDemoted<TFactionType>(
            in FactionLevelChangeContext<TestFactionHolder> context)
        {
            DemotionCheckCount++;
            CaptureContext(context.member, context.faction);
            if (RejectDemotion) return FactionOperations.DemotionDenied();
            return base.CanBeDemoted<TFactionType>(context);
        }

        protected override void OnJoinedFaction<TFactionType>(
            in JoinFactionContext<TestFactionHolder> context,
            in OperationResult result)
        {
            JoinedCount++;
            CaptureContext(context.member, context.faction);
            CaptureResult(result);
            base.OnJoinedFaction<TFactionType>(context, result);
        }

        protected override void OnJoinFailed<TFactionType>(
            in JoinFactionContext<TestFactionHolder> context,
            in OperationResult result)
        {
            JoinFailedCount++;
            CaptureContext(context.member, context.faction);
            CaptureResult(result);
            base.OnJoinFailed<TFactionType>(context, result);
        }

        protected override void OnLeftFaction<TFactionType>(
            in LeaveFactionContext<TestFactionHolder> context,
            in OperationResult result)
        {
            LeftCount++;
            CaptureContext(context.member, context.faction);
            CaptureResult(result);
            base.OnLeftFaction<TFactionType>(context, result);
        }

        protected override void OnLeaveFailed<TFactionType>(
            in LeaveFactionContext<TestFactionHolder> context,
            in OperationResult result)
        {
            LeaveFailedCount++;
            CaptureContext(context.member, context.faction);
            CaptureResult(result);
            base.OnLeaveFailed<TFactionType>(context, result);
        }

        protected override void OnReputationChanged<TFactionType>(
            in ReputationChangeContext<TestFactionHolder> context,
            in OperationResult result)
        {
            ReputationChangedCount++;
            LastPreviousReputation = context.previousReputation;
            LastAmountRequested = context.amountRequested;
            CaptureContext(context.member, context.faction);
            CaptureResult(result);
            base.OnReputationChanged<TFactionType>(context, result);
        }

        protected override void OnReputationChangeFailed<TFactionType>(
            in ReputationChangeContext<TestFactionHolder> context,
            in OperationResult result)
        {
            ReputationFailedCount++;
            LastPreviousReputation = context.previousReputation;
            LastAmountRequested = context.amountRequested;
            CaptureContext(context.member, context.faction);
            CaptureResult(result);
            base.OnReputationChangeFailed<TFactionType>(context, result);
        }

        private void CaptureContext(TestFactionHolder member, FactionBase faction)
        {
            LastMember = member;
            LastFaction = faction;
        }

        private void CaptureResult(OperationResult result)
        {
            LastSystemCode = result.systemCode;
            LastResultCode = result.resultCode;
        }
    }

    public sealed class TestFaction : FactionBase<TestFactionHolder>
    {
        public bool RejectJoin { get; set; }
        public bool RejectLeave { get; set; }
        public bool RejectReputationChange { get; set; }
        public bool RejectPromotion { get; set; }
        public bool RejectDemotion { get; set; }

        public int JoinCheckCount { get; private set; }
        public int LeaveCheckCount { get; private set; }
        public int ReputationCheckCount { get; private set; }
        public int PromotionCheckCount { get; private set; }
        public int DemotionCheckCount { get; private set; }
        public int JoinedCount { get; private set; }
        public int JoinFailedCount { get; private set; }
        public int LeftCount { get; private set; }
        public int LeaveFailedCount { get; private set; }
        public int ReputationChangedCount { get; private set; }
        public int ReputationFailedCount { get; private set; }
        public int LevelChangedCount { get; private set; }
        public FactionBase LastFaction { get; private set; }
        public TestFactionHolder LastMember { get; private set; }
        public ReputationLevelBase LastPreviousLevel { get; private set; }
        public ReputationLevelBase LastNewLevel { get; private set; }

        protected internal override OperationResult CanJoin(in JoinFactionContext<TestFactionHolder> context)
        {
            JoinCheckCount++;
            CaptureContext(context.member, context.faction);
            if (RejectJoin) return FactionOperations.Denied();
            return base.CanJoin(context);
        }

        protected internal override OperationResult CanLeave(in LeaveFactionContext<TestFactionHolder> context)
        {
            LeaveCheckCount++;
            CaptureContext(context.member, context.faction);
            if (RejectLeave) return FactionOperations.Denied();
            return base.CanLeave(context);
        }

        protected internal override OperationResult CanChangeReputation(
            in ReputationChangeContext<TestFactionHolder> context)
        {
            ReputationCheckCount++;
            CaptureContext(context.member, context.faction);
            if (RejectReputationChange) return FactionOperations.Denied();
            return base.CanChangeReputation(context);
        }

        protected internal override OperationResult CanBePromoted(
            in FactionLevelChangeContext<TestFactionHolder> context)
        {
            PromotionCheckCount++;
            CaptureContext(context.member, context.faction);
            if (RejectPromotion) return FactionOperations.PromotionDenied();
            return base.CanBePromoted(context);
        }

        protected internal override OperationResult CanBeDemoted(
            in FactionLevelChangeContext<TestFactionHolder> context)
        {
            DemotionCheckCount++;
            CaptureContext(context.member, context.faction);
            if (RejectDemotion) return FactionOperations.DemotionDenied();
            return base.CanBeDemoted(context);
        }

        protected internal override void OnJoined(
            in JoinFactionContext<TestFactionHolder> context,
            in OperationResult result)
        {
            JoinedCount++;
            CaptureContext(context.member, context.faction);
        }

        protected internal override void OnJoinFailed(
            in JoinFactionContext<TestFactionHolder> context,
            in OperationResult result)
        {
            JoinFailedCount++;
            CaptureContext(context.member, context.faction);
        }

        protected internal override void OnLeft(
            in LeaveFactionContext<TestFactionHolder> context,
            in OperationResult result)
        {
            LeftCount++;
            CaptureContext(context.member, context.faction);
        }

        protected internal override void OnLeaveFailed(
            in LeaveFactionContext<TestFactionHolder> context,
            in OperationResult result)
        {
            LeaveFailedCount++;
            CaptureContext(context.member, context.faction);
        }

        protected internal override void OnReputationChanged(
            in ReputationChangeContext<TestFactionHolder> context,
            in OperationResult result)
        {
            ReputationChangedCount++;
            CaptureContext(context.member, context.faction);
        }

        protected internal override void OnReputationChangeFailed(
            in ReputationChangeContext<TestFactionHolder> context,
            in OperationResult result)
        {
            ReputationFailedCount++;
            CaptureContext(context.member, context.faction);
        }

        protected internal override void OnLevelChanged(
            in FactionLevelChangeContext<TestFactionHolder> context,
            in OperationResult result)
        {
            LevelChangedCount++;
            CaptureContext(context.member, context.faction);
            LastPreviousLevel = context.previousLevel;
            LastNewLevel = context.newLevel;
        }

        private void CaptureContext(TestFactionHolder member, FactionBase faction)
        {
            LastMember = member;
            LastFaction = faction;
        }
    }

    public sealed class OtherTestFaction : FactionBase<TestFactionHolder>
    {
    }

    public class TestReputationLevel : ReputationLevelBase
    {
        public bool RejectPromoteTo { get; set; }
        public bool RejectDemoteFrom { get; set; }
        public bool RejectDemoteTo { get; set; }

        public int CanPromoteToCount { get; private set; }
        public int CanDemoteFromCount { get; private set; }
        public int CanDemoteToCount { get; private set; }
        public int AchievedCount { get; private set; }
        public int IncreasedCount { get; private set; }
        public int DecreasedCount { get; private set; }
        public int ChangedCount { get; private set; }

        protected internal override OperationResult CanPromoteTo(in FactionLevelChangeContext context)
        {
            CanPromoteToCount++;
            if (RejectPromoteTo) return FactionOperations.PromotionDenied();
            return base.CanPromoteTo(context);
        }

        protected internal override OperationResult CanDemoteFrom(in FactionLevelChangeContext context)
        {
            CanDemoteFromCount++;
            if (RejectDemoteFrom) return FactionOperations.DemotionDenied();
            return base.CanDemoteFrom(context);
        }

        protected internal override OperationResult CanDemoteTo(in FactionLevelChangeContext context)
        {
            CanDemoteToCount++;
            if (RejectDemoteTo) return FactionOperations.DemotionDenied();
            return base.CanDemoteTo(context);
        }

        protected internal override void OnLevelAchieved(
            in FactionLevelChangeContext context,
            in OperationResult result)
        {
            AchievedCount++;
        }

        protected internal override void OnLevelIncreased(
            in FactionLevelChangeContext context,
            in OperationResult result)
        {
            IncreasedCount++;
        }

        protected internal override void OnLevelDecreased(
            in FactionLevelChangeContext context,
            in OperationResult result)
        {
            DecreasedCount++;
        }

        protected internal override void OnLevelChanged(
            in FactionLevelChangeContext context,
            in OperationResult result)
        {
            ChangedCount++;
        }
    }

    public sealed class OtherTestReputationLevel : TestReputationLevel
    {
    }

    public sealed class TestAssignedReputationLevel : TestReputationLevel, IForFaction<TestFaction>
    {
    }
}
