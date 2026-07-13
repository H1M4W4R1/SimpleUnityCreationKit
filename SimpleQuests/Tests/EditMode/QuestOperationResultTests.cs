using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleQuests.Operations;

namespace Systems.SimpleQuests.Tests
{
    public sealed class QuestOperationResultTests : SimpleQuestsTestBase
    {
        [Test]
        public void QuestFactories_UseQuestSystemCodes()
        {
            OperationResult permitted = QuestOperations.Permitted();
            OperationResult started = QuestOperations.Started();
            OperationResult alreadyStarted = QuestOperations.QuestAlreadyStarted();
            OperationResult alreadyFinished = QuestOperations.QuestAlreadyFinished();
            OperationResult notFound = QuestOperations.QuestNotFound();

            Assert.IsTrue(OperationResult.IsSuccess(permitted));
            Assert.AreEqual(QuestOperations.SYSTEM_QUESTS, permitted.systemCode);
            Assert.AreEqual(OperationResult.SUCCESS_PERMITTED, permitted.resultCode);

            Assert.IsTrue(OperationResult.IsSuccess(started));
            Assert.AreEqual(QuestOperations.SYSTEM_QUESTS, started.systemCode);
            Assert.AreEqual(QuestOperations.SUCCESS_STARTED, started.resultCode);

            Assert.IsTrue(OperationResult.IsError(alreadyStarted));
            Assert.IsTrue(OperationResult.IsFromSystem(alreadyStarted, QuestOperations.SYSTEM_QUESTS));
            Assert.AreEqual(QuestOperations.ALREADY_STARTED, alreadyStarted.resultCode);

            Assert.IsTrue(OperationResult.IsError(alreadyFinished));
            Assert.IsTrue(OperationResult.IsFromSystem(alreadyFinished, QuestOperations.SYSTEM_QUESTS));
            Assert.AreEqual(QuestOperations.ALREADY_FINISHED, alreadyFinished.resultCode);

            Assert.IsTrue(OperationResult.IsError(notFound));
            Assert.IsTrue(OperationResult.IsFromSystem(notFound, QuestOperations.SYSTEM_QUESTS));
            Assert.AreEqual(QuestOperations.QUEST_NOT_FOUND, notFound.resultCode);
        }
    }
}
