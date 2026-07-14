using System;
using System.Collections.Generic;
using Systems.SimpleBrain.Abstract;
using Systems.SimpleBrain.Data.Context;
using Systems.SimpleBrain.Operations;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleCore.Operations;
using UnityEngine;

namespace Systems.SimpleBrain.Components
{
    /// <summary>
    ///     Unity-serializable, hash-sorted storage for knowledge learned by one brain.
    /// </summary>
    [Serializable]
    internal sealed class BrainKnowledgeStorage
    {
        [Serializable]
        private sealed class KnowledgeEntry
        {
            [SerializeReference] public KnowledgeBase knowledge;
            [NonSerialized] public HashIdentifier hashIdentifier;

            public KnowledgeEntry(KnowledgeBase knowledge)
            {
                this.knowledge = knowledge;
                hashIdentifier = HashIdentifier.New(knowledge.GetType());
            }
        }

        private sealed class KnowledgeEntryComparer : IComparer<KnowledgeEntry>
        {
            public int Compare(KnowledgeEntry first, KnowledgeEntry second)
            {
                HashIdentifier firstHash = ReferenceEquals(first, null) ? default : first.hashIdentifier;
                HashIdentifier secondHash = ReferenceEquals(second, null) ? default : second.hashIdentifier;
                return firstHash.CompareTo(secondHash);
            }
        }

        private static readonly KnowledgeEntryComparer _entryComparer = new KnowledgeEntryComparer();

        [SerializeField] private List<KnowledgeEntry> _knowledge = new List<KnowledgeEntry>();

        [NonSerialized] private bool _isSorted;

        public bool HasLearned<TKnowledge>()
            where TKnowledge : KnowledgeBase
        {
            return TryGetKnowledge(out TKnowledge knowledge);
        }

        public bool Knows<TKnowledge>(BrainBase brain)
            where TKnowledge : KnowledgeBase
        {
            if (!TryGetKnowledge(out TKnowledge knowledge)) return false;

            BrainContext context = new BrainContext(brain);
            return knowledge.IsKnownBy(context);
        }

        public bool TryGetKnowledge<TKnowledge>(out TKnowledge knowledge)
            where TKnowledge : KnowledgeBase
        {
            int knowledgeIndex = GetKnowledgeIndex<TKnowledge>();
            if (knowledgeIndex < 0)
            {
                knowledge = null;
                return false;
            }

            knowledge = (TKnowledge) _knowledge[knowledgeIndex].knowledge;
            return true;
        }

        public OperationResult TryLearn<TKnowledge>(BrainBase brain)
            where TKnowledge : KnowledgeBase, new()
        {
            if (HasLearned<TKnowledge>()) return BrainOperations.KnowledgeAlreadyLearned();

            TKnowledge knowledge = new TKnowledge();
            BrainContext context = new BrainContext(brain);
            OperationResult canLearnResult = knowledge.CanBeLearnedBy(context);
            if (!canLearnResult)
            {
                knowledge.NotifyLearningFailed(context, canLearnResult);
                return canLearnResult;
            }

            _knowledge.Add(new KnowledgeEntry(knowledge));
            _isSorted = false;

            OperationResult learnedResult = BrainOperations.KnowledgeLearned();
            knowledge.NotifyLearned(context, learnedResult);
            return learnedResult;
        }

        private int GetKnowledgeIndex<TKnowledge>()
            where TKnowledge : KnowledgeBase
        {
            EnsureSorted();

            Type knowledgeType = typeof(TKnowledge);
            HashIdentifier targetHash = HashIdentifier.New(knowledgeType);
            int low = 0;
            int high = _knowledge.Count - 1;
            int foundIndex = -1;

            while (low <= high)
            {
                int middle = (low + high) >> 1;
                KnowledgeEntry middleEntry = _knowledge[middle];
                int comparison = middleEntry.hashIdentifier.CompareTo(targetHash);
                if (comparison == 0)
                {
                    foundIndex = middle;
                    break;
                }

                if (comparison < 0)
                    low = middle + 1;
                else
                    high = middle - 1;
            }

            if (foundIndex < 0) return -1;

            while (foundIndex > 0 &&
                   _knowledge[foundIndex - 1].hashIdentifier.CompareTo(targetHash) == 0)
                foundIndex--;

            int knowledgeCount = _knowledge.Count;
            for (int knowledgeIndex = foundIndex;
                 knowledgeIndex < knowledgeCount &&
                 _knowledge[knowledgeIndex].hashIdentifier.CompareTo(targetHash) == 0;
                 knowledgeIndex++)
            {
                KnowledgeBase knowledge = _knowledge[knowledgeIndex].knowledge;
                if (ReferenceEquals(knowledge, null)) continue;
                if (knowledge.GetType() != knowledgeType) continue;
                return knowledgeIndex;
            }

            return -1;
        }

        private void EnsureSorted()
        {
            if (_isSorted) return;

            for (int knowledgeIndex = _knowledge.Count - 1; knowledgeIndex >= 0; knowledgeIndex--)
            {
                KnowledgeEntry entry = _knowledge[knowledgeIndex];
                if (!ReferenceEquals(entry, null) && !ReferenceEquals(entry.knowledge, null)) continue;
                _knowledge.RemoveAt(knowledgeIndex);
            }

            int knowledgeCount = _knowledge.Count;
            for (int knowledgeIndex = 0; knowledgeIndex < knowledgeCount; knowledgeIndex++)
            {
                KnowledgeEntry entry = _knowledge[knowledgeIndex];
                entry.hashIdentifier = HashIdentifier.New(entry.knowledge.GetType());
            }

            _knowledge.Sort(_entryComparer);
            _isSorted = true;
        }
    }
}
