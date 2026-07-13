using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleQuests.Data;
using Systems.SimpleQuests.Data.Enums;
using Systems.SimpleQuests.Operations;
using Systems.SimpleQuests.Utility;
using UnityEngine;
using UnityEngine.TestTools;

namespace Systems.SimpleQuests.Tests
{
    public sealed class QuestAPITests : SimpleQuestsTestBase
    {
        [Test]
        public void TryStartQuest_WhenQuestIsMissing_ReturnsQuestNotFound()
        {
            QuestInstance instance;
            LogAssert.Expect(LogType.Error, "Quest of type TestQuest was not found in database");

            OperationResult result = QuestAPI.TryStartQuest<TestQuest>(out instance);

            AssertSimilar(QuestOperations.QuestNotFound(), result);
            Assert.IsNull(instance);
        }

        [Test]
        public void TryStartQuest_WhenQuestRejectsStart_ReturnsFailureAndInvokesFailureCallback()
        {
            TestQuest quest = CreateRegisteredQuest<TestQuest>();
            quest.RejectStart = true;
            QuestInstance instance;

            OperationResult result = QuestAPI.TryStartQuest<TestQuest>(out instance);

            AssertSimilar(QuestOperations.QuestAlreadyStarted(), result);
            Assert.IsNull(instance);
            Assert.AreEqual(1, quest.StartCheckCount);
            Assert.AreEqual(0, quest.StartedCount);
            Assert.AreEqual(1, quest.StartFailedCount);
            Assert.AreEqual(QuestOperations.SYSTEM_QUESTS, quest.LastFailureSystemCode & 0x7FFF);
            Assert.AreEqual(QuestOperations.ALREADY_STARTED, quest.LastFailureResultCode);
        }

        [Test]
        public void TryStartQuest_WhenPermitted_StartsAndExposesActiveQuestQueries()
        {
            TestQuest quest = CreateRegisteredQuest<TestQuest>();
            TestObjective objective = new TestObjective();
            quest.AddObjective(objective);
            QuestInstance instance;

            OperationResult result = QuestAPI.TryStartQuest<TestQuest>(out instance);

            AssertSimilar(QuestOperations.Started(), result);
            Assert.IsNotNull(instance);
            Assert.AreSame(quest, instance.Quest);
            Assert.AreEqual(QuestState.InProgress, instance.State);
            Assert.AreEqual(1, quest.StartCheckCount);
            Assert.AreEqual(1, quest.StartedCount);
            Assert.AreEqual(QuestState.InProgress, objective.State);
            Assert.AreSame(quest, QuestAPI.GetFirstActiveQuestOfType<TestQuest>());
            Assert.AreSame(instance, QuestAPI.GetFirstQuestInstanceOf(quest));

            ROListAccess<TestQuest> activeQuests = QuestAPI.GetAllActiveQuestsOfType<TestQuest>();
            ROListAccess<QuestInstance> instances = QuestAPI.GetAllInstancesOf(quest);
            Assert.AreEqual(1, activeQuests.List.Count);
            Assert.AreEqual(1, instances.List.Count);
            Assert.AreSame(quest, activeQuests.List[0]);
            Assert.AreSame(instance, instances.List[0]);
            activeQuests.Release();
            instances.Release();
        }

        [Test]
        public void TickForTests_MovesCompletedQuestToFinishedQueries()
        {
            TestQuest quest = CreateRegisteredQuest<TestQuest>();
            TestObjective objective = new TestObjective {CompleteOnCheck = true};
            quest.AddObjective(objective);
            QuestInstance instance;
            QuestAPI.TryStartQuest<TestQuest>(out instance);

            QuestAPI.TickForTests(0.2f);

            Assert.AreEqual(QuestState.Completed, instance.State);
            Assert.IsTrue(QuestAPI.IsQuestCompleted<TestQuest>());
            Assert.IsFalse(QuestAPI.IsQuestFailed<TestQuest>());
            Assert.AreSame(quest, QuestAPI.GetFirstFinishedQuestOfType<TestQuest>());
            Assert.IsNull(QuestAPI.GetFirstActiveQuestOfType<TestQuest>());
            Assert.AreEqual(1, QuestAPI.FinishedQuests.Count);
            Assert.AreSame(instance, QuestAPI.FinishedQuests[0]);

            ROListAccess<TestQuest> finishedQuests = QuestAPI.GetAllFinishedQuestsOfType<TestQuest>();
            Assert.AreEqual(1, finishedQuests.List.Count);
            Assert.AreSame(quest, finishedQuests.List[0]);
            finishedQuests.Release();
        }

        [Test]
        public void CompleteQuest_ForcesCompletionImmediatelyAndFinishedMoveOnNextTick()
        {
            TestQuest quest = CreateRegisteredQuest<TestQuest>();
            TestObjective objective = new TestObjective();
            quest.AddObjective(objective);
            QuestInstance instance;
            QuestAPI.TryStartQuest<TestQuest>(out instance);

            bool completed = QuestAPI.CompleteQuest<TestQuest>();

            Assert.IsTrue(completed);
            Assert.AreEqual(QuestState.Completed, instance.State);
            Assert.AreEqual(QuestState.Completed, objective.State);
            Assert.AreEqual(1, objective.CompleteCount);
            Assert.AreEqual(1, quest.CompletedCount);
            Assert.AreEqual(0, QuestAPI.FinishedQuests.Count);

            QuestAPI.TickForTests(0.1f);

            Assert.AreEqual(1, QuestAPI.FinishedQuests.Count);
            Assert.IsTrue(QuestAPI.IsQuestCompleted<TestQuest>());
            Assert.IsFalse(QuestAPI.CompleteQuest<TestQuest>());
        }

        [Test]
        public void FailQuest_WithOnlyOptionalObjectives_ForcesFailureInsteadOfCompleting()
        {
            TestQuest quest = CreateRegisteredQuest<TestQuest>();
            TestObjective optional = new TestObjective {Required = false};
            quest.AddObjective(optional);
            QuestInstance instance;
            QuestAPI.TryStartQuest<TestQuest>(out instance);

            bool failed = QuestAPI.FailQuest(quest);

            Assert.IsTrue(failed);
            Assert.AreEqual(QuestState.Failed, instance.State);
            Assert.AreEqual(QuestState.InProgress, optional.State);
            Assert.AreEqual(1, quest.FailedCount);
            Assert.AreEqual(0, quest.CompletedCount);

            QuestAPI.TickForTests(0.1f);

            Assert.IsTrue(QuestAPI.IsQuestFailed<TestQuest>());
            Assert.IsFalse(QuestAPI.IsQuestCompleted<TestQuest>());
        }

        [Test]
        public void CompleteAndFailQuest_ByReferenceOnlyMatchTheRequestedQuestAsset()
        {
            TestQuest quest = CreateRegisteredQuest<TestQuest>();
            OtherTestQuest otherQuest = CreateRegisteredQuest<OtherTestQuest>();
            QuestInstance questInstance;
            QuestInstance otherInstance;
            QuestAPI.TryStartQuest<TestQuest>(out questInstance);
            QuestAPI.TryStartQuest<OtherTestQuest>(out otherInstance);

            bool completedOther = QuestAPI.CompleteQuest(otherQuest);
            bool failedQuest = QuestAPI.FailQuest(quest);

            Assert.IsTrue(completedOther);
            Assert.IsTrue(failedQuest);
            Assert.AreEqual(QuestState.Failed, questInstance.State);
            Assert.AreEqual(QuestState.Completed, otherInstance.State);
            Assert.AreEqual(1, quest.FailedCount);
            Assert.AreEqual(1, otherQuest.CompletedCount);
        }

        [Test]
        public void UniqueQuest_WhenRunning_PreventsSecondStartUnlessFlagAllowsIt()
        {
            CreateRegisteredQuest<TestUniqueQuest>();
            QuestInstance firstInstance;
            QuestInstance blockedInstance;
            QuestInstance allowedInstance;
            QuestAPI.TryStartQuest<TestUniqueQuest>(out firstInstance);

            OperationResult blocked = QuestAPI.TryStartQuest<TestUniqueQuest>(out blockedInstance);
            OperationResult allowed = QuestAPI.TryStartQuest<TestUniqueQuest>(
                out allowedInstance,
                StartQuestFlags.AllowStartUniqueIfRunning);

            AssertSimilar(QuestOperations.QuestAlreadyStarted(), blocked);
            Assert.IsNull(blockedInstance);
            AssertSimilar(QuestOperations.Started(), allowed);
            Assert.IsNotNull(allowedInstance);

            ROListAccess<TestUniqueQuest> activeQuests = QuestAPI.GetAllActiveQuestsOfType<TestUniqueQuest>();
            Assert.AreEqual(2, activeQuests.List.Count);
            activeQuests.Release();
        }

        [Test]
        public void UniqueQuest_WhenFinished_PreventsRestartUnlessFlagAllowsIt()
        {
            TestUniqueQuest quest = CreateRegisteredQuest<TestUniqueQuest>();
            TestObjective objective = new TestObjective {CompleteOnCheck = true};
            quest.AddObjective(objective);
            QuestInstance firstInstance;
            QuestInstance blockedInstance;
            QuestInstance allowedInstance;
            QuestAPI.TryStartQuest<TestUniqueQuest>(out firstInstance);
            QuestAPI.TickForTests(0.1f);

            OperationResult blocked = QuestAPI.TryStartQuest<TestUniqueQuest>(out blockedInstance);
            OperationResult allowed = QuestAPI.TryStartQuest<TestUniqueQuest>(
                out allowedInstance,
                StartQuestFlags.AllowStartUniqueIfFinished);

            AssertSimilar(QuestOperations.QuestAlreadyFinished(), blocked);
            Assert.IsNull(blockedInstance);
            AssertSimilar(QuestOperations.Started(), allowed);
            Assert.IsNotNull(allowedInstance);
            Assert.AreEqual(1, QuestAPI.FinishedQuests.Count);
            Assert.AreSame(firstInstance, QuestAPI.FinishedQuests[0]);
        }

        [Test]
        public void ClearAllQuests_RemovesActiveAndFinishedQuestState()
        {
            TestQuest quest = CreateRegisteredQuest<TestQuest>();
            TestObjective objective = new TestObjective {CompleteOnCheck = true};
            quest.AddObjective(objective);
            QuestInstance instance;
            QuestAPI.TryStartQuest<TestQuest>(out instance);
            QuestAPI.TickForTests(0.1f);

            QuestAPI.ClearAllQuests();

            Assert.AreEqual(0, QuestAPI.FinishedQuests.Count);
            Assert.IsFalse(QuestAPI.IsQuestCompleted<TestQuest>());
            Assert.IsNull(QuestAPI.GetFirstActiveQuestOfType<TestQuest>());
            Assert.IsNull(QuestAPI.GetFirstFinishedQuestOfType<TestQuest>());
        }
    }
}
