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
    ///     Unity-serializable runtime state for subprocesses owned by one brain.
    /// </summary>
    [Serializable]
    internal sealed class BrainSubprocessStorage
    {
        private enum SubprocessState : byte
        {
            Stopped,
            Running,
            Paused
        }

        [Serializable]
        private sealed class SubprocessEntry
        {
            [SerializeReference] public BrainSubprocessBase subprocess;
            public SubprocessState state;
            public bool pausedByComa;
            [NonSerialized] public HashIdentifier hashIdentifier;

            public SubprocessEntry(BrainSubprocessBase subprocess)
            {
                this.subprocess = subprocess;
                state = SubprocessState.Stopped;
                hashIdentifier = HashIdentifier.New(subprocess.GetType());
            }
        }

        private sealed class SubprocessEntryComparer : IComparer<SubprocessEntry>
        {
            public int Compare(SubprocessEntry first, SubprocessEntry second)
            {
                HashIdentifier firstHash = ReferenceEquals(first, null) ? default : first.hashIdentifier;
                HashIdentifier secondHash = ReferenceEquals(second, null) ? default : second.hashIdentifier;
                return firstHash.CompareTo(secondHash);
            }
        }

        private static readonly SubprocessEntryComparer _entryComparer = new SubprocessEntryComparer();

        [SerializeField] private List<SubprocessEntry> _entries = new List<SubprocessEntry>();

        [NonSerialized] private List<SubprocessEntry> _tickEntries;
        [NonSerialized] private bool _isSorted;

        public bool IsCreated<TSubprocess>()
            where TSubprocess : BrainSubprocessBase
        {
            return GetEntryIndex<TSubprocess>() >= 0;
        }

        public bool IsRunning<TSubprocess>()
            where TSubprocess : BrainSubprocessBase
        {
            int entryIndex = GetEntryIndex<TSubprocess>();
            if (entryIndex < 0) return false;

            SubprocessEntry entry = _entries[entryIndex];
            return entry.state == SubprocessState.Running;
        }

        public bool IsPaused<TSubprocess>()
            where TSubprocess : BrainSubprocessBase
        {
            int entryIndex = GetEntryIndex<TSubprocess>();
            if (entryIndex < 0) return false;

            SubprocessEntry entry = _entries[entryIndex];
            return entry.state == SubprocessState.Paused;
        }

        public OperationResult TryStart<TSubprocess>(BrainBase brain)
            where TSubprocess : BrainSubprocessBase, new()
        {
            SubprocessEntry entry = GetOrCreate<TSubprocess>();
            if (entry.state == SubprocessState.Running) return BrainOperations.SubprocessAlreadyRunning();
            if (entry.state == SubprocessState.Paused) return BrainOperations.SubprocessIsPaused();

            BrainSubprocessContext context = new BrainSubprocessContext(brain);
            OperationResult canStartResult = entry.subprocess.CanBeStartedBy(context);
            if (!canStartResult)
            {
                entry.subprocess.NotifyStartFailed(context, canStartResult);
                return canStartResult;
            }

            entry.state = SubprocessState.Running;
            entry.pausedByComa = false;
            OperationResult startedResult = BrainOperations.SubprocessStarted();
            entry.subprocess.NotifyStarted(context, startedResult);
            return startedResult;
        }

        public OperationResult TryStop<TSubprocess>(BrainBase brain)
            where TSubprocess : BrainSubprocessBase
        {
            int entryIndex = GetEntryIndex<TSubprocess>();
            if (entryIndex < 0)
                return BrainOperations.SubprocessNotCreated();

            SubprocessEntry entry = _entries[entryIndex];
            if (entry.state == SubprocessState.Stopped) return BrainOperations.SubprocessAlreadyStopped();

            BrainSubprocessContext context = new BrainSubprocessContext(brain);
            OperationResult canStopResult = entry.subprocess.CanBeStoppedBy(context);
            if (!canStopResult)
            {
                entry.subprocess.NotifyStopFailed(context, canStopResult);
                return canStopResult;
            }

            entry.state = SubprocessState.Stopped;
            entry.pausedByComa = false;
            OperationResult stoppedResult = BrainOperations.SubprocessStopped();
            entry.subprocess.NotifyStopped(context, stoppedResult);
            return stoppedResult;
        }

        public OperationResult TryPause<TSubprocess>(BrainBase brain)
            where TSubprocess : BrainSubprocessBase
        {
            int entryIndex = GetEntryIndex<TSubprocess>();
            if (entryIndex < 0)
                return BrainOperations.SubprocessNotCreated();

            SubprocessEntry entry = _entries[entryIndex];
            if (entry.state != SubprocessState.Running) return BrainOperations.SubprocessNotRunning();

            BrainSubprocessContext context = new BrainSubprocessContext(brain);
            OperationResult canPauseResult = entry.subprocess.CanBePausedBy(context);
            if (!canPauseResult)
            {
                entry.subprocess.NotifyPauseFailed(context, canPauseResult);
                return canPauseResult;
            }

            entry.state = SubprocessState.Paused;
            entry.pausedByComa = false;
            OperationResult pausedResult = BrainOperations.SubprocessPaused();
            entry.subprocess.NotifyPaused(context, pausedResult);
            return pausedResult;
        }

        public OperationResult TryResume<TSubprocess>(BrainBase brain)
            where TSubprocess : BrainSubprocessBase
        {
            int entryIndex = GetEntryIndex<TSubprocess>();
            if (entryIndex < 0)
                return BrainOperations.SubprocessNotCreated();

            SubprocessEntry entry = _entries[entryIndex];
            if (entry.state != SubprocessState.Paused) return BrainOperations.SubprocessNotPaused();

            BrainSubprocessContext context = new BrainSubprocessContext(brain);
            OperationResult canResumeResult = entry.subprocess.CanBeResumedBy(context);
            if (!canResumeResult)
            {
                entry.subprocess.NotifyResumeFailed(context, canResumeResult);
                return canResumeResult;
            }

            entry.state = SubprocessState.Running;
            entry.pausedByComa = false;
            OperationResult resumedResult = BrainOperations.SubprocessResumed();
            entry.subprocess.NotifyResumed(context, resumedResult);
            return resumedResult;
        }

        public OperationResult TryFinish(BrainBase brain, BrainSubprocessBase subprocess)
        {
            Type subprocessType = subprocess.GetType();
            int entryIndex = GetEntryIndex(subprocessType);
            if (entryIndex < 0)
                return BrainOperations.SubprocessNotOwned();

            SubprocessEntry entry = _entries[entryIndex];
            if (!ReferenceEquals(entry.subprocess, subprocess)) return BrainOperations.SubprocessNotOwned();
            if (entry.state != SubprocessState.Running) return BrainOperations.SubprocessNotRunning();

            entry.state = SubprocessState.Stopped;
            entry.pausedByComa = false;
            OperationResult finishedResult = BrainOperations.SubprocessFinished();
            BrainSubprocessContext context = new BrainSubprocessContext(brain);
            subprocess.NotifyFinished(context, finishedResult);
            return finishedResult;
        }

        public void PauseForComa(BrainBase brain)
        {
            EnsureTickStorage();
            int entryCount = _entries.Count;
            for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
            {
                SubprocessEntry entry = _entries[entryIndex];
                if (ReferenceEquals(entry, null) || ReferenceEquals(entry.subprocess, null)) continue;
                if (entry.state != SubprocessState.Running) continue;
                if (entry.subprocess is ISubprocessAllowedInComa) continue;

                BrainSubprocessContext context = new BrainSubprocessContext(brain, isComaInduced: true);
                OperationResult canPauseResult = entry.subprocess.CanBePausedBy(context);
                if (!canPauseResult)
                {
                    entry.subprocess.NotifyPauseFailed(context, canPauseResult);
                    continue;
                }

                entry.state = SubprocessState.Paused;
                entry.pausedByComa = true;
                OperationResult pausedResult = BrainOperations.SubprocessPaused();
                entry.subprocess.NotifyPaused(context, pausedResult);
            }
        }

        public void ResumeAfterComa(BrainBase brain)
        {
            EnsureTickStorage();
            int entryCount = _entries.Count;
            for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
            {
                SubprocessEntry entry = _entries[entryIndex];
                if (ReferenceEquals(entry, null) || ReferenceEquals(entry.subprocess, null)) continue;
                if (entry.state != SubprocessState.Paused || !entry.pausedByComa) continue;

                BrainSubprocessContext context = new BrainSubprocessContext(brain, isComaInduced: true);
                OperationResult canResumeResult = entry.subprocess.CanBeResumedBy(context);
                if (!canResumeResult)
                {
                    entry.subprocess.NotifyResumeFailed(context, canResumeResult);
                    continue;
                }

                entry.state = SubprocessState.Running;
                entry.pausedByComa = false;
                OperationResult resumedResult = BrainOperations.SubprocessResumed();
                entry.subprocess.NotifyResumed(context, resumedResult);
            }
        }

        public void Tick(BrainBase brain, float deltaTimeSeconds)
        {
            EnsureTickStorage();
            _tickEntries.Clear();
            int entryCount = _entries.Count;
            for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
            {
                SubprocessEntry entry = _entries[entryIndex];
                if (ReferenceEquals(entry, null) || ReferenceEquals(entry.subprocess, null)) continue;
                if (entry.state == SubprocessState.Running) _tickEntries.Add(entry);
            }

            int tickEntryCount = _tickEntries.Count;
            for (int entryIndex = 0; entryIndex < tickEntryCount; entryIndex++)
            {
                SubprocessEntry entry = _tickEntries[entryIndex];
                if (entry.state != SubprocessState.Running) continue;
                BrainSubprocessContext context = new BrainSubprocessContext(brain, deltaTimeSeconds: deltaTimeSeconds);
                entry.subprocess.Tick(context);
            }
        }

        public void TickAllowedInComa(BrainBase brain, float deltaTimeSeconds)
        {
            EnsureTickStorage();
            _tickEntries.Clear();
            int entryCount = _entries.Count;
            for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
            {
                SubprocessEntry entry = _entries[entryIndex];
                if (ReferenceEquals(entry, null) || ReferenceEquals(entry.subprocess, null)) continue;
                if (entry.state != SubprocessState.Running) continue;
                if (entry.subprocess is ISubprocessAllowedInComa) _tickEntries.Add(entry);
            }

            int tickEntryCount = _tickEntries.Count;
            for (int entryIndex = 0; entryIndex < tickEntryCount; entryIndex++)
            {
                SubprocessEntry entry = _tickEntries[entryIndex];
                if (entry.state != SubprocessState.Running) continue;
                BrainSubprocessContext context = new BrainSubprocessContext(
                    brain,
                    isComaInduced: true,
                    deltaTimeSeconds: deltaTimeSeconds);
                entry.subprocess.Tick(context);
            }
        }

        private SubprocessEntry GetOrCreate<TSubprocess>()
            where TSubprocess : BrainSubprocessBase, new()
        {
            Type subprocessType = typeof(TSubprocess);
            int existingEntryIndex = GetEntryIndex(subprocessType);
            if (existingEntryIndex >= 0) return _entries[existingEntryIndex];

            TSubprocess subprocess = new TSubprocess();
            SubprocessEntry newEntry = new SubprocessEntry(subprocess);
            _entries.Add(newEntry);
            _isSorted = false;
            return newEntry;
        }

        private int GetEntryIndex<TSubprocess>()
            where TSubprocess : BrainSubprocessBase
        {
            return GetEntryIndex(typeof(TSubprocess));
        }

        private int GetEntryIndex(Type subprocessType)
        {
            EnsureSorted();

            HashIdentifier targetHash = HashIdentifier.New(subprocessType);
            int low = 0;
            int high = _entries.Count - 1;
            int foundIndex = -1;

            while (low <= high)
            {
                int middle = (low + high) >> 1;
                SubprocessEntry middleEntry = _entries[middle];
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
                   _entries[foundIndex - 1].hashIdentifier.CompareTo(targetHash) == 0)
                foundIndex--;

            int entryCount = _entries.Count;
            for (int entryIndex = foundIndex;
                 entryIndex < entryCount &&
                 _entries[entryIndex].hashIdentifier.CompareTo(targetHash) == 0;
                 entryIndex++)
            {
                BrainSubprocessBase subprocess = _entries[entryIndex].subprocess;
                if (ReferenceEquals(subprocess, null)) continue;
                if (subprocess.GetType() != subprocessType) continue;
                return entryIndex;
            }

            return -1;
        }

        private void EnsureTickStorage()
        {
            if (!ReferenceEquals(_tickEntries, null)) return;
            _tickEntries = new List<SubprocessEntry>();
        }

        private void EnsureSorted()
        {
            if (_isSorted) return;

            for (int entryIndex = _entries.Count - 1; entryIndex >= 0; entryIndex--)
            {
                SubprocessEntry entry = _entries[entryIndex];
                if (!ReferenceEquals(entry, null) && !ReferenceEquals(entry.subprocess, null)) continue;
                _entries.RemoveAt(entryIndex);
            }

            int entryCount = _entries.Count;
            for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
            {
                SubprocessEntry entry = _entries[entryIndex];
                entry.hashIdentifier = HashIdentifier.New(entry.subprocess.GetType());
            }

            _entries.Sort(_entryComparer);
            _isSorted = true;
        }
    }
}
