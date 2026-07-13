using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Timing;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleQuests.Abstract;
using Systems.SimpleQuests.Abstract.Markers;
using Systems.SimpleQuests.Data;
using Systems.SimpleQuests.Data.Enums;
using Systems.SimpleQuests.Operations;
using Systems.SimpleQuests.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleQuests.Tests
{
    public abstract class SimpleQuestsTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            QuestAPI.ClearAllQuests();
            QuestDatabase.ClearForTests();
        }

        [TearDown]
        public void TearDown()
        {
            QuestAPI.ClearAllQuests();
            QuestDatabase.ClearForTests();

            for (int objectIndex = _createdObjects.Count - 1; objectIndex >= 0; objectIndex--)
            {
                Object createdObject = _createdObjects[objectIndex];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();

            TickSystem[] tickSystems = Object.FindObjectsByType<TickSystem>(FindObjectsInactive.Include);
            for (int tickSystemIndex = 0; tickSystemIndex < tickSystems.Length; tickSystemIndex++)
            {
                TickSystem tickSystem = tickSystems[tickSystemIndex];
                if (ReferenceEquals(tickSystem, null)) continue;
                Object.DestroyImmediate(tickSystem.gameObject);
            }
        }

        protected TQuestType CreateQuest<TQuestType>()
            where TQuestType : Quest
        {
            TQuestType quest = Track(ScriptableObject.CreateInstance<TQuestType>());
            quest.name = typeof(TQuestType).Name;
            return quest;
        }

        protected TQuestType CreateRegisteredQuest<TQuestType>()
            where TQuestType : Quest
        {
            TQuestType quest = CreateQuest<TQuestType>();
            QuestDatabase.RegisterForTests(quest);
            return quest;
        }

        protected static QuestInstance StartInstance(TestQuestBase quest)
        {
            QuestInstance instance = QuestInstance.FromQuest(quest);
            instance.Start();
            return instance;
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

        protected static int CountAndRelease<TItemType>(ROListAccess<TItemType> access)
        {
            int count = access.List.Count;
            access.Release();
            return count;
        }
    }

    public abstract class TestQuestBase : Quest
    {
        private readonly List<QuestObjective> _objectives = new List<QuestObjective>();

        public bool RejectStart { get; set; }
        public int StartCheckCount { get; private set; }
        public int StartedCount { get; private set; }
        public int StartFailedCount { get; private set; }
        public int CompletedCount { get; private set; }
        public int FailedCount { get; private set; }
        public ushort LastFailureSystemCode { get; private set; }
        public ushort LastFailureResultCode { get; private set; }
        public QuestInstance LastInstance { get; private set; }

        public TestQuestBase AddObjective(QuestObjective objective)
        {
            _objectives.Add(objective);
            return this;
        }

        public override QuestInstance Create()
        {
            QuestInstance instance = base.Create();
            for (int objectiveIndex = 0; objectiveIndex < _objectives.Count; objectiveIndex++)
            {
                instance.WithObjective(_objectives[objectiveIndex]);
            }

            return instance;
        }

        protected internal override OperationResult CanBeStarted()
        {
            StartCheckCount++;
            if (RejectStart) return QuestOperations.QuestAlreadyStarted();
            return base.CanBeStarted();
        }

        protected internal override void OnQuestStarted(QuestInstance instance)
        {
            StartedCount++;
            LastInstance = instance;
        }

        protected internal override void OnQuestStartFailed(OperationResult reason)
        {
            StartFailedCount++;
            LastFailureSystemCode = reason.systemCode;
            LastFailureResultCode = reason.resultCode;
        }

        protected internal override void OnQuestCompleted(QuestInstance instance)
        {
            CompletedCount++;
            LastInstance = instance;
        }

        protected internal override void OnQuestFailed(QuestInstance instance)
        {
            FailedCount++;
            LastInstance = instance;
        }
    }

    public sealed class TestQuest : TestQuestBase
    {
    }

    public sealed class TestUniqueQuest : TestQuestBase, IUniqueQuest
    {
    }

    public sealed class OtherTestQuest : TestQuestBase
    {
    }

    public class TestObjective : QuestObjective
    {
        public bool Required { get; set; } = true;
        public bool CompleteOnCheck { get; set; }
        public bool FailOnCheck { get; set; }
        public int StartCount { get; private set; }
        public int CompleteCount { get; private set; }
        public int FailCount { get; private set; }
        public int TickCount { get; private set; }
        public int CompletionCheckCount { get; private set; }
        public int FailureCheckCount { get; private set; }
        public float LastDeltaTime { get; private set; }
        public QuestInstance LastQuest { get; private set; }

        public override bool IsRequired => Required;

        public override bool ShouldBeComplete()
        {
            CompletionCheckCount++;
            return CompleteOnCheck;
        }

        public override bool ShouldBeFailed()
        {
            FailureCheckCount++;
            return FailOnCheck;
        }

        protected internal override void OnQuestObjectiveStarted(QuestInstance quest)
        {
            StartCount++;
            LastQuest = quest;
        }

        protected internal override void OnQuestObjectiveCompleted(QuestInstance quest)
        {
            CompleteCount++;
            LastQuest = quest;
        }

        protected internal override void OnQuestObjectiveFailed(QuestInstance quest)
        {
            FailCount++;
            LastQuest = quest;
        }

        protected internal override void OnQuestObjectiveTick(QuestInstance questInstance, float deltaTime)
        {
            TickCount++;
            LastDeltaTime = deltaTime;
            LastQuest = questInstance;
        }
    }

    public sealed class OtherTestObjective : TestObjective
    {
    }
}
