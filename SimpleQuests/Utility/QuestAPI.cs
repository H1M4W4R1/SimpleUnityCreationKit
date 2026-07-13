using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleCore.Timing;
using Systems.SimpleQuests.Abstract;
using Systems.SimpleQuests.Abstract.Markers;
using Systems.SimpleQuests.Data;
using Systems.SimpleQuests.Data.Enums;
using Systems.SimpleQuests.Operations;
using UnityEngine;

namespace Systems.SimpleQuests.Utility
{
    public static class QuestAPI
    {
        /// <summary>
        ///     Variable to check if the tick system is already hooked
        /// </summary>
        private static bool _isTickSystemHooked;

        /// <summary>
        ///     List of current quests
        /// </summary>
        private static readonly List<QuestInstance> _currentQuests = new List<QuestInstance>();

        /// <summary>
        ///     List of finished quests (completed or failed)
        /// </summary>
        private static readonly List<QuestInstance> _finishedQuests = new List<QuestInstance>();

        /// <summary>
        ///     Read-only access to finished quests
        /// </summary>
        public static IReadOnlyList<QuestInstance> FinishedQuests => _finishedQuests;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            if (_isTickSystemHooked)
            {
                TickSystem.UnregisterHandler(OnTick);
            }

            _isTickSystemHooked = false;
            _currentQuests.Clear();
            _finishedQuests.Clear();
        }

        /// <summary>
        ///     Removes all quests from the list
        /// </summary>
        public static void ClearAllQuests()
        {
            _currentQuests.Clear();
            _finishedQuests.Clear();
            if (_isTickSystemHooked)
            {
                TickSystem.UnregisterHandler(OnTick);
                _isTickSystemHooked = false;
            }
        }

        /// <summary>
        ///     Forces a quest to finish
        /// </summary>
        /// <typeparam name="TQuest">The quest to finish</typeparam>
        /// <returns>True if the quest was found and finished, false otherwise</returns>
        /// <remarks>
        ///     The new() constraint prevents using abstract Quest types.
        /// </remarks>
        public static bool CompleteQuest<TQuest>()
            where TQuest : Quest, new()
        {
            // Find instance and finish it
            for (int i = 0; i < _currentQuests.Count; i++)
            {
                if (_currentQuests[i].Quest is not TQuest) continue;
                _currentQuests[i].ForceFinish();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Forces a quest to finish
        /// </summary>
        /// <param name="quest">Quest to finish</param>
        /// <returns>True if the quest was found and finished, false otherwise</returns>
        public static bool CompleteQuest([NotNull] Quest quest)
        {
            // Find instance and finish it
            for (int i = 0; i < _currentQuests.Count; i++)
            {
                if (!ReferenceEquals(_currentQuests[i].Quest, quest)) continue;
                _currentQuests[i].ForceFinish();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Forces a quest to finish
        /// </summary>
        /// <typeparam name="TQuest">The quest to finish</typeparam>
        /// <returns>True if the quest was found and finished, false otherwise</returns>
        public static bool FailQuest<TQuest>()
            where TQuest : Quest, new()
        {
            // Find instance and finish it
            for (int i = 0; i < _currentQuests.Count; i++)
            {
                if (_currentQuests[i].Quest is not TQuest) continue;
                _currentQuests[i].ForceFail();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Forces a quest to finish
        /// </summary>
        /// <param name="quest">Quest to finish</param>
        /// <returns>True if the quest was found and finished, false otherwise</returns>
        public static bool FailQuest([NotNull] Quest quest)
        {
            // Find instance and finish it
            for (int i = 0; i < _currentQuests.Count; i++)
            {
                if (!ReferenceEquals(_currentQuests[i].Quest, quest)) continue;
                _currentQuests[i].ForceFail();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Tries to start a quest.
        ///     If the quest implements <see cref="Systems.SimpleQuests.Abstract.Markers.IUniqueQuest"/>,
        ///     the attempt fails when an active instance of the same type already exists.
        /// </summary>
        /// <remarks>
        ///     The new() constraint prevents using abstract Quest types.
        /// </remarks>
        public static OperationResult TryStartQuest<TQuest>(
            [CanBeNull] out QuestInstance instance,
            in StartQuestFlags flags = default)
            where TQuest : Quest, new()
        {
            // Ensure tick system is hooked properly
            if (!_isTickSystemHooked)
            {
                TickSystem.RegisterHandler(OnTick);
                _isTickSystemHooked = true;
            }

            // Create new instance
            TQuest quest = QuestDatabase.GetExact<TQuest>();
            if (ReferenceEquals(quest, null))
            {
                Debug.LogError($"Quest of type {typeof(TQuest).Name} was not found in database");
                instance = null;
                return QuestOperations.QuestNotFound();
            }

            // Enforce single-instance constraint for IUniqueQuest implementations
            if (quest is IUniqueQuest)
            {
                // Handle running instances
                if ((flags & StartQuestFlags.AllowStartUniqueIfRunning) == 0)
                {
                    for (int i = 0; i < _currentQuests.Count; i++)
                    {
                        if (_currentQuests[i].Quest is not TQuest) continue;
                        instance = null;
                        return QuestOperations.QuestAlreadyStarted();
                    }
                }

                // Handle finished instances
                if ((flags & StartQuestFlags.AllowStartUniqueIfFinished) == 0)
                {
                    for (int i = 0; i < _finishedQuests.Count; i++)
                    {
                        if (_finishedQuests[i].Quest is not TQuest) continue;
                        instance = null;
                        return QuestOperations.QuestAlreadyFinished();
                    }
                }
            }

            // Check if quest can be started
            OperationResult result = quest.CanBeStarted();
            if (!result)
            {
                quest.OnQuestStartFailed(result);
                instance = null;
                return result;
            }

            // Create instance, start it and add to list
            instance = QuestInstance.FromQuest(quest);
            instance.Start();
            _currentQuests.Add(instance);
            return QuestOperations.Started();
        }

        /// <summary>
        ///     Gets all quest objects of the specified type from active quests
        /// </summary>
        public static ROListAccess<TQuest> GetAllActiveQuestsOfType<TQuest>()
            where TQuest : Quest
        {
            RWListAccess<TQuest> list = RWListAccess<TQuest>.Create();
            List<TQuest> refList = list.List;

            for (int i = 0; i < _currentQuests.Count; i++)
            {
                if (_currentQuests[i].Quest is TQuest requestedQuest) refList.Add(requestedQuest);
            }

            return list.ToReadOnly();
        }

        /// <summary>
        ///     Gets all quest objects of the specified type from finished quests
        /// </summary>
        public static ROListAccess<TQuest> GetAllFinishedQuestsOfType<TQuest>()
            where TQuest : Quest
        {
            RWListAccess<TQuest> list = RWListAccess<TQuest>.Create();
            List<TQuest> refList = list.List;

            for (int i = 0; i < _finishedQuests.Count; i++)
            {
                if (_finishedQuests[i].Quest is TQuest requestedQuest) refList.Add(requestedQuest);
            }

            return list.ToReadOnly();
        }

        /// <summary>
        ///     Gets all quest instances of a quest
        /// </summary>
        public static ROListAccess<QuestInstance> GetAllInstancesOf([NotNull] Quest quest)
        {
            RWListAccess<QuestInstance> list = RWListAccess<QuestInstance>.Create();
            List<QuestInstance> refList = list.List;

            for (int i = 0; i < _currentQuests.Count; i++)
            {
                if (ReferenceEquals(_currentQuests[i].Quest, quest)) refList.Add(_currentQuests[i]);
            }

            return list.ToReadOnly();
        }

        /// <summary>
        ///     Gets the first quest of the specified type from active quests
        /// </summary>
        [CanBeNull] public static TQuest GetFirstActiveQuestOfType<TQuest>()
            where TQuest : Quest
        {
            for (int i = 0; i < _currentQuests.Count; i++)
            {
                if (_currentQuests[i].Quest is TQuest requestedQuest) return requestedQuest;
            }

            return null;
        }

        /// <summary>
        ///     Gets the first quest of the specified type from finished quests
        /// </summary>
        [CanBeNull] public static TQuest GetFirstFinishedQuestOfType<TQuest>()
            where TQuest : Quest
        {
            for (int i = 0; i < _finishedQuests.Count; i++)
            {
                if (_finishedQuests[i].Quest is TQuest requestedQuest) return requestedQuest;
            }

            return null;
        }

        /// <summary>
        ///     Returns true if at least one finished instance of the specified quest type is completed
        /// </summary>
        public static bool IsQuestCompleted<TQuest>()
            where TQuest : Quest
        {
            for (int i = 0; i < _finishedQuests.Count; i++)
            {
                if (_finishedQuests[i].Quest is TQuest && _finishedQuests[i].IsCompleted) return true;
            }

            return false;
        }

        /// <summary>
        ///     Returns true if at least one finished instance of the specified quest type is failed
        /// </summary>
        public static bool IsQuestFailed<TQuest>()
            where TQuest : Quest
        {
            for (int i = 0; i < _finishedQuests.Count; i++)
            {
                if (_finishedQuests[i].Quest is TQuest && _finishedQuests[i].IsFailed) return true;
            }

            return false;
        }

        /// <summary>
        ///     Gets the first quest instance of a quest
        /// </summary>
        [CanBeNull] public static QuestInstance GetFirstQuestInstanceOf([NotNull] Quest quest)
        {
            for (int i = 0; i < _currentQuests.Count; i++)
            {
                if (ReferenceEquals(_currentQuests[i].Quest, quest)) return _currentQuests[i];
            }

            return null;
        }

        /// <summary>
        ///     Should be called every frame or every turn to update objectives and check if quest is completed
        ///     or failed
        /// </summary>
        private static void OnTick(float deltaTime)
        {
            for (int i = _currentQuests.Count - 1; i >= 0; i--)
            {
                _currentQuests[i].Tick(deltaTime);
                if (_currentQuests[i].State is QuestState.Completed or QuestState.Failed)
                {
                    _finishedQuests.Add(_currentQuests[i]);
                    _currentQuests.RemoveAt(i);
                }
            }
        }

#if UNITY_INCLUDE_TESTS
        internal static void TickForTests(float deltaTime)
        {
            OnTick(deltaTime);
        }
#endif
    }
}
