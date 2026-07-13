using NUnit.Framework;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleQuests.Abstract;
using Systems.SimpleQuests.Data;
using Systems.SimpleQuests.Data.Enums;
using Systems.SimpleQuests.Objectives;
using UnityEngine;
using UnityEngine.TestTools;

namespace Systems.SimpleQuests.Tests
{
    public sealed class QuestInstanceLifecycleTests : SimpleQuestsTestBase
    {
        [Test]
        public void Start_ActivatesPrecedingOptionalObjectivesAndFirstRequiredObjective()
        {
            TestQuest quest = CreateQuest<TestQuest>();
            TestObjective optionalBefore = new TestObjective {Required = false};
            TestObjective firstRequired = new TestObjective();
            TestObjective optionalAfter = new TestObjective {Required = false};
            TestObjective secondRequired = new TestObjective();
            quest.AddObjective(optionalBefore)
                .AddObjective(firstRequired)
                .AddObjective(optionalAfter)
                .AddObjective(secondRequired);

            QuestInstance instance = StartInstance(quest);

            Assert.AreEqual(QuestState.InProgress, instance.State);
            Assert.AreEqual(QuestState.InProgress, optionalBefore.State);
            Assert.AreEqual(QuestState.InProgress, firstRequired.State);
            Assert.AreEqual(QuestState.Inactive, optionalAfter.State);
            Assert.AreEqual(QuestState.Inactive, secondRequired.State);
            Assert.AreEqual(1, quest.StartedCount);
            Assert.AreEqual(1, optionalBefore.StartCount);
            Assert.AreEqual(1, firstRequired.StartCount);
            Assert.AreEqual(0, optionalAfter.StartCount);
        }

        [Test]
        public void Tick_CompletesCurrentObjectivesAndActivatesNextObjectiveGroup()
        {
            TestQuest quest = CreateQuest<TestQuest>();
            TestObjective optionalBefore = new TestObjective {Required = false, CompleteOnCheck = true};
            TestObjective firstRequired = new TestObjective {CompleteOnCheck = true};
            TestObjective optionalAfter = new TestObjective {Required = false};
            TestObjective secondRequired = new TestObjective();
            quest.AddObjective(optionalBefore)
                .AddObjective(firstRequired)
                .AddObjective(optionalAfter)
                .AddObjective(secondRequired);
            QuestInstance instance = StartInstance(quest);

            instance.Tick(0.25f);

            Assert.AreEqual(QuestState.InProgress, instance.State);
            Assert.AreEqual(QuestState.Completed, optionalBefore.State);
            Assert.AreEqual(QuestState.Completed, firstRequired.State);
            Assert.AreEqual(QuestState.InProgress, optionalAfter.State);
            Assert.AreEqual(QuestState.InProgress, secondRequired.State);
            Assert.AreEqual(1, optionalBefore.TickCount);
            Assert.AreEqual(0.25f, optionalBefore.LastDeltaTime);
            Assert.AreEqual(1, optionalAfter.StartCount);
            Assert.AreEqual(1, secondRequired.StartCount);
        }

        [Test]
        public void Tick_WhenCompletionAndFailureAreBothTrue_FailsObjectiveAndQuest()
        {
            TestQuest quest = CreateQuest<TestQuest>();
            TestObjective firstRequired = new TestObjective
            {
                CompleteOnCheck = true,
                FailOnCheck = true
            };
            TestObjective secondRequired = new TestObjective();
            quest.AddObjective(firstRequired).AddObjective(secondRequired);
            QuestInstance instance = StartInstance(quest);

            instance.Tick(0.1f);

            Assert.AreEqual(QuestState.Failed, firstRequired.State);
            Assert.AreEqual(QuestState.Inactive, secondRequired.State);
            Assert.AreEqual(QuestState.Failed, instance.State);
            Assert.AreEqual(1, firstRequired.FailCount);
            Assert.AreEqual(0, firstRequired.CompleteCount);
            Assert.AreEqual(1, quest.FailedCount);
            Assert.AreEqual(0, quest.CompletedCount);
        }

        [Test]
        public void Tick_WhenOnlyRequiredObjectivesComplete_CompletesQuestWithoutActivatingTrailingOptional()
        {
            TestQuest quest = CreateQuest<TestQuest>();
            TestObjective required = new TestObjective {CompleteOnCheck = true};
            TestObjective trailingOptional = new TestObjective {Required = false};
            quest.AddObjective(required).AddObjective(trailingOptional);
            QuestInstance instance = StartInstance(quest);

            instance.Tick(0.1f);

            Assert.AreEqual(QuestState.Completed, instance.State);
            Assert.AreEqual(QuestState.Completed, required.State);
            Assert.AreEqual(QuestState.Inactive, trailingOptional.State);
            Assert.AreEqual(1, quest.CompletedCount);
            Assert.AreEqual(0, trailingOptional.StartCount);
        }

        [Test]
        public void Tick_WhenOptionalObjectiveFails_DoesNotFailQuest()
        {
            TestQuest quest = CreateQuest<TestQuest>();
            TestObjective optional = new TestObjective {Required = false, FailOnCheck = true};
            TestObjective required = new TestObjective {CompleteOnCheck = true};
            quest.AddObjective(optional).AddObjective(required);
            QuestInstance instance = StartInstance(quest);

            instance.Tick(0.1f);

            Assert.AreEqual(QuestState.Failed, optional.State);
            Assert.AreEqual(QuestState.Completed, required.State);
            Assert.AreEqual(QuestState.Completed, instance.State);
            Assert.AreEqual(1, quest.CompletedCount);
            Assert.AreEqual(0, quest.FailedCount);
        }

        [Test]
        public void CombinedObjective_StartsChildrenTogetherAndCompletesFromRequiredChildren()
        {
            TestQuest quest = CreateQuest<TestQuest>();
            CombinedQuestObjective combined = new CombinedQuestObjective();
            TestObjective requiredChild = new TestObjective {CompleteOnCheck = true};
            TestObjective optionalChild = new TestObjective {Required = false, FailOnCheck = true};
            combined.WithObjective(requiredChild).WithObjective(optionalChild);
            quest.AddObjective(combined);
            QuestInstance instance = StartInstance(quest);

            instance.Tick(0.1f);

            Assert.AreEqual(QuestState.Completed, combined.State);
            Assert.AreEqual(QuestState.Completed, requiredChild.State);
            Assert.AreEqual(QuestState.Failed, optionalChild.State);
            Assert.AreEqual(QuestState.Completed, instance.State);
            Assert.AreEqual(1, requiredChild.StartCount);
            Assert.AreEqual(1, optionalChild.StartCount);
        }

        [Test]
        public void ObjectiveLookup_ReturnsFirstAndAllObjectivesOfRequestedType()
        {
            TestQuest quest = CreateQuest<TestQuest>();
            TestObjective first = new TestObjective();
            OtherTestObjective second = new OtherTestObjective();
            TestObjective third = new TestObjective();
            quest.AddObjective(first).AddObjective(second).AddObjective(third);
            QuestInstance instance = QuestInstance.FromQuest(quest);

            TestObjective firstFound = instance.GetObjective<TestObjective>();
            ROListAccess<TestObjective> objectives = instance.GetObjectives<TestObjective>();

            Assert.AreSame(first, firstFound);
            Assert.AreEqual(3, objectives.List.Count);
            Assert.AreSame(first, objectives.List[0]);
            Assert.AreSame(second, objectives.List[1]);
            Assert.AreSame(third, objectives.List[2]);
            objectives.Release();
        }

        [Test]
        public void TryCompleteAndFailObjective_RequireContainedInProgressObjectives()
        {
            TestQuest quest = CreateQuest<TestQuest>();
            TestObjective first = new TestObjective();
            TestObjective second = new TestObjective();
            TestObjective foreign = new TestObjective();
            quest.AddObjective(first).AddObjective(second);
            QuestInstance instance = StartInstance(quest);
            foreign.State = QuestState.InProgress;

            Assert.IsFalse(instance.TryCompleteObjective(foreign));
            Assert.IsTrue(instance.TryCompleteObjective<TestObjective>());
            Assert.AreEqual(QuestState.Completed, first.State);
            Assert.IsFalse(instance.TryCompleteObjective<TestObjective>());

            instance.Tick(0.1f);

            Assert.AreEqual(QuestState.InProgress, second.State);
            Assert.IsTrue(instance.TryFailObjective(second));
            Assert.AreEqual(QuestState.Failed, second.State);
            Assert.AreEqual(1, second.FailCount);
        }

        [Test]
        public void ForceMethods_DoNotChangeObjectivesAlreadyInOppositeTerminalState()
        {
            TestQuest failQuest = CreateQuest<TestQuest>();
            TestObjective completedObjective = new TestObjective();
            failQuest.AddObjective(completedObjective);
            QuestInstance failInstance = StartInstance(failQuest);
            completedObjective.State = QuestState.Completed;

            failInstance.ForceFail();

            Assert.AreEqual(QuestState.Failed, failInstance.State);
            Assert.AreEqual(QuestState.Completed, completedObjective.State);
            Assert.AreEqual(0, completedObjective.FailCount);

            TestQuest completeQuest = CreateQuest<TestQuest>();
            TestObjective failedObjective = new TestObjective();
            completeQuest.AddObjective(failedObjective);
            QuestInstance completeInstance = StartInstance(completeQuest);
            failedObjective.State = QuestState.Failed;

            completeInstance.ForceFinish();

            Assert.AreEqual(QuestState.Completed, completeInstance.State);
            Assert.AreEqual(QuestState.Failed, failedObjective.State);
            Assert.AreEqual(0, failedObjective.CompleteCount);
        }

        [Test]
        public void WithObjective_WhenObjectiveIsNull_LogsErrorAndSkipsObjective()
        {
            TestQuest quest = CreateQuest<TestQuest>();
            QuestInstance instance = QuestInstance.FromQuest(quest);
            LogAssert.Expect(LogType.Error, "Tried to add null objective to quest " + quest.name);

            instance.WithObjective(null);

            Assert.AreEqual(0, instance.Objectives.Count);
        }
    }
}
