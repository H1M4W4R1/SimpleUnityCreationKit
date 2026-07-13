using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleLoading.Abstract;
using Systems.SimpleLoading.Data;
using Systems.SimpleLoading.Operations;
using UnityEngine;

namespace Systems.SimpleLoading.Utility
{
    /// <summary>Static facade for staged data loading and world-part distance checks.</summary>
    public static class LoadingAPI
    {
        private sealed class LoadingRequest
        {
            public LoadingHandle handle;
            public LoadingSequenceBase sequence;
            public UnityEngine.Object target;
            public object userData;
            public ILoadingStageOperation currentOperation;
            public int stageIndex;
            public float stageProgress;
            public float totalWeight;
            public float completedWeight;
            public LoadingStatus status;
            public LoadingCompletion completion;
        }

        private static readonly List<LoadingRequest> Requests = new();
        private static int _nextHandleId;

        /// <summary>
        ///     Starts a configured sequence and returns its typed handle. Use <see cref="TryLoad{TSequence}"/> when
        ///     the caller needs to inspect a rejected start result.
        /// </summary>
        public static LoadingSequenceHandle<TSequence> Load<TSequence>(
            [CanBeNull] TSequence sequence,
            [CanBeNull] UnityEngine.Object target = null,
            [CanBeNull] object userData = null)
            where TSequence : LoadingSequenceBase
        {
            LoadingSequenceStartResult<TSequence> startResult = TryLoad(sequence, target, userData);
            return startResult.handle;
        }

        /// <summary>Starts a configured loading sequence for optional target and caller-owned data.</summary>
        public static LoadingSequenceStartResult<TSequence> TryLoad<TSequence>(
            [CanBeNull] TSequence sequence,
            [CanBeNull] UnityEngine.Object target = null,
            [CanBeNull] object userData = null)
            where TSequence : LoadingSequenceBase
        {
            LoadingSequenceHandle<TSequence> invalidHandle = default;
            if (ReferenceEquals(sequence, null) || !sequence)
            {
                OperationResult missingSequenceResult = LoadingOperations.SequenceIsNull();
                return new LoadingSequenceStartResult<TSequence>(in missingSequenceResult, in invalidHandle);
            }

            LoadingHandle handle = new(++_nextHandleId, 1);
            LoadingContext context = new LoadingContext(handle, target, userData);
            OperationResult canStartResult = sequence.CanStartLoading(in context);
            if (!canStartResult)
            {
                sequence.OnLoadingFailed(in context, in canStartResult);
                return new LoadingSequenceStartResult<TSequence>(in canStartResult, in invalidHandle);
            }

            LoadingRequest request = new LoadingRequest
            {
                handle = handle,
                sequence = sequence,
                target = target,
                userData = userData,
                status = LoadingStatus.Running
            };
            request.totalWeight = CalculateTotalWeight(sequence);
            Requests.Add(request);

            sequence.OnLoadingStarted(in context);

            if (sequence.GetStageCount() == 0)
            {
                OperationResult completedResult = LoadingOperations.Completed();
                CompleteRequest(request, in completedResult);
            }
            else
            {
                StartCurrentStage(request);
            }

            OperationResult startedResult = LoadingOperations.Started();
            LoadingSequenceHandle<TSequence> typedHandle = new LoadingSequenceHandle<TSequence>(in handle);
            return new LoadingSequenceStartResult<TSequence>(in startedResult, in typedHandle);
        }

        /// <summary>Cancels the running request represented by a typed sequence handle.</summary>
        public static OperationResult AbortLoading<TSequence>(in LoadingSequenceHandle<TSequence> handle)
            where TSequence : LoadingSequenceBase
            => Cancel(in handle.handle);

        /// <summary>Returns whether the request completed successfully.</summary>
        public static bool IsLoadingComplete<TSequence>(in LoadingSequenceHandle<TSequence> handle)
            where TSequence : LoadingSequenceBase
            => GetStatus(in handle.handle) == LoadingStatus.Completed;

        /// <summary>Gets the current request state through its type-safe handle.</summary>
        public static LoadingStatus GetStatus<TSequence>(in LoadingSequenceHandle<TSequence> handle)
            where TSequence : LoadingSequenceBase
            => GetStatus(in handle.handle);

        /// <summary>Gets the current zero-based stage index, or -1 when terminal or invalid.</summary>
        public static int GetCurrentStage<TSequence>(in LoadingSequenceHandle<TSequence> handle)
            where TSequence : LoadingSequenceBase
            => GetCurrentStage(in handle.handle);

        /// <summary>Gets progress of the active stage in the 0..1 range.</summary>
        public static float GetCurrentPercentage<TSequence>(in LoadingSequenceHandle<TSequence> handle)
            where TSequence : LoadingSequenceBase
        {
            LoadingRequest request = FindRequest(in handle.handle);
            return ReferenceEquals(request, null) ? 0f : request.stageProgress;
        }

        /// <summary>Gets weighted request progress in the 0..1 range.</summary>
        public static float GetCurrentTotalPercentage<TSequence>(in LoadingSequenceHandle<TSequence> handle)
            where TSequence : LoadingSequenceBase
            => GetProgress(in handle.handle);

        /// <summary>Removes the state retained for a typed sequence handle.</summary>
        public static OperationResult Release<TSequence>(in LoadingSequenceHandle<TSequence> handle)
            where TSequence : LoadingSequenceBase
            => Release(in handle.handle);

        /// <summary>Cancels a running request. Terminal requests are left available for inspection.</summary>
        public static OperationResult Cancel(in LoadingHandle handle)
        {
            LoadingRequest request = FindRequest(in handle);
            if (ReferenceEquals(request, null)) return LoadingOperations.HandleInvalid();
            if (request.status != LoadingStatus.Running) return LoadingOperations.HandleNotRunning();

            LoadingContext context = CreateContext(request);
            if (!ReferenceEquals(request.currentOperation, null)) request.currentOperation.Cancel(in context);
            request.currentOperation = null;
            request.status = LoadingStatus.Cancelled;
            OperationResult result = LoadingOperations.Cancelled();
            request.completion = CreateCompletion(LoadingStatus.Cancelled, in result);
            request.sequence.OnLoadingCancelled(in context);
            return result;
        }

        /// <summary>Removes completed, failed, or cancelled request state after it is no longer needed.</summary>
        public static OperationResult Release(in LoadingHandle handle)
        {
            for (int requestIndex = 0; requestIndex < Requests.Count; requestIndex++)
            {
                LoadingRequest request = Requests[requestIndex];
                if (!IsSameHandle(request.handle, handle)) continue;
                if (request.status == LoadingStatus.Running) Cancel(in handle);
                Requests.RemoveAt(requestIndex);
                return LoadingOperations.Released();
            }

            return LoadingOperations.HandleInvalid();
        }

        /// <summary>Gets the current status, or <see cref="LoadingStatus.Invalid"/> for an unknown handle.</summary>
        public static LoadingStatus GetStatus(in LoadingHandle handle)
        {
            LoadingRequest request = FindRequest(in handle);
            return ReferenceEquals(request, null) ? LoadingStatus.Invalid : request.status;
        }

        /// <summary>Gets weighted total progress in the 0..1 range.</summary>
        public static float GetProgress(in LoadingHandle handle)
        {
            LoadingRequest request = FindRequest(in handle);
            if (ReferenceEquals(request, null)) return 0f;
            if (request.status == LoadingStatus.Completed) return 1f;
            return request.totalWeight <= 0f
                ? 0f
                : Mathf.Clamp01((request.completedWeight + request.stageProgress * GetCurrentWeight(request)) / request.totalWeight);
        }

        /// <summary>Gets the current zero-based stage index, or -1 when the request is unknown or terminal.</summary>
        public static int GetCurrentStage(in LoadingHandle handle)
        {
            LoadingRequest request = FindRequest(in handle);
            return ReferenceEquals(request, null) || request.status != LoadingStatus.Running ? -1 : request.stageIndex;
        }

        /// <summary>Gets the terminal result information without retaining a stack-only <see cref="OperationResult"/>.</summary>
        public static LoadingCompletion GetCompletion(in LoadingHandle handle)
        {
            LoadingRequest request = FindRequest(in handle);
            return ReferenceEquals(request, null) ? default : request.completion;
        }

        /// <summary>
        ///     Determines whether a world part should be loaded. The unload radius supplies hysteresis and must be
        ///     greater than or equal to the load radius.
        /// </summary>
        public static bool ShouldLoadWorldPart(
            Vector3 targetPosition,
            Vector3 worldPartPosition,
            float loadDistance,
            float unloadDistance,
            bool isCurrentlyLoaded)
        {
            float evaluatedDistance = isCurrentlyLoaded ? unloadDistance : loadDistance;
            if (loadDistance < 0f || unloadDistance < loadDistance) return isCurrentlyLoaded;
            return (targetPosition - worldPartPosition).sqrMagnitude <= evaluatedDistance * evaluatedDistance;
        }

        /// <summary>Advances all running requests. Host code can call this for a custom update loop.</summary>
        public static void Advance(float deltaTime)
        {
            int initialRequestCount = Requests.Count;
            for (int requestIndex = 0; requestIndex < initialRequestCount && requestIndex < Requests.Count; requestIndex++)
            {
                LoadingRequest request = Requests[requestIndex];
                if (request.status != LoadingStatus.Running) continue;
                AdvanceRequest(request, Mathf.Max(0f, deltaTime));
            }

        }

        private static void AdvanceRequest(LoadingRequest request, float deltaTime)
        {
            if (ReferenceEquals(request.currentOperation, null)) return;
            LoadingContext context = CreateContext(request);
            LoadingStageUpdate update;
            try
            {
                update = request.currentOperation.Update(in context, deltaTime);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                OperationResult exceptionResult = LoadingOperations.StageOperationMissing();
                FailRequest(request, in exceptionResult);
                return;
            }

            request.stageProgress = Mathf.Clamp01(update.progress);
            float progress = GetProgress(request.handle);
            request.sequence.OnLoadingProgressed(in context, progress);
            if (!update.result)
            {
                FailRequest(request, in update.result);
                return;
            }

            if (!update.isComplete) return;

            request.sequence.OnStageCompleted(in context, request.stageIndex);
            request.completedWeight += GetCurrentWeight(request);
            request.stageIndex++;
            request.stageProgress = 0f;
            request.currentOperation = null;

            if (request.stageIndex >= request.sequence.GetStageCount())
            {
                OperationResult completedResult = LoadingOperations.Completed();
                CompleteRequest(request, in completedResult);
                return;
            }

            StartCurrentStage(request);
        }

        private static void StartCurrentStage(LoadingRequest request)
        {
            LoadingContext context = CreateContext(request);
            LoadingStageBase stage = request.sequence.GetStage(request.stageIndex);
            if (ReferenceEquals(stage, null))
            {
                OperationResult missingStageResult = LoadingOperations.StageMissing();
                FailRequest(request, in missingStageResult);
                return;
            }

            request.sequence.OnStageStarted(in context, request.stageIndex);
            try
            {
                request.currentOperation = stage.CreateOperation(in context);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                OperationResult operationMissingResult = LoadingOperations.StageOperationMissing();
                FailRequest(request, in operationMissingResult);
                return;
            }

            if (ReferenceEquals(request.currentOperation, null))
            {
                OperationResult operationMissingResult = LoadingOperations.StageOperationMissing();
                FailRequest(request, in operationMissingResult);
                return;
            }

            try
            {
                OperationResult beginResult = request.currentOperation.Begin(in context);
                if (!beginResult) FailRequest(request, in beginResult);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                OperationResult operationMissingResult = LoadingOperations.StageOperationMissing();
                FailRequest(request, in operationMissingResult);
            }
        }

        private static void CompleteRequest(LoadingRequest request, in OperationResult result)
        {
            LoadingContext context = CreateContext(request);
            request.status = LoadingStatus.Completed;
            request.stageProgress = 1f;
            request.completion = CreateCompletion(LoadingStatus.Completed, in result);
            request.sequence.OnLoadingCompleted(in context, in result);
        }

        private static void FailRequest(LoadingRequest request, in OperationResult result)
        {
            LoadingContext context = CreateContext(request);
            if (!ReferenceEquals(request.currentOperation, null)) request.currentOperation.Cancel(in context);
            request.currentOperation = null;
            request.status = LoadingStatus.Failed;
            request.completion = CreateCompletion(LoadingStatus.Failed, in result);
            request.sequence.OnLoadingFailed(in context, in result);
        }

        private static float CalculateTotalWeight(LoadingSequenceBase sequence)
        {
            float totalWeight = 0f;
            for (int stageIndex = 0; stageIndex < sequence.GetStageCount(); stageIndex++)
            {
                LoadingStageBase stage = sequence.GetStage(stageIndex);
                if (ReferenceEquals(stage, null)) continue;
                totalWeight += Mathf.Max(0.0001f, stage.TimeWeight);
            }

            return totalWeight;
        }

        private static float GetCurrentWeight(LoadingRequest request)
        {
            if (request.stageIndex < 0 || request.stageIndex >= request.sequence.GetStageCount()) return 0f;
            LoadingStageBase stage = request.sequence.GetStage(request.stageIndex);
            return ReferenceEquals(stage, null) ? 0f : Mathf.Max(0.0001f, stage.TimeWeight);
        }

        [CanBeNull] private static LoadingRequest FindRequest(in LoadingHandle handle)
        {
            if (!handle.IsValid) return null;
            for (int requestIndex = 0; requestIndex < Requests.Count; requestIndex++)
            {
                LoadingRequest request = Requests[requestIndex];
                if (IsSameHandle(request.handle, handle)) return request;
            }

            return null;
        }

        private static bool IsSameHandle(in LoadingHandle first, in LoadingHandle second)
            => first.id == second.id && first.version == second.version;

        private static LoadingContext CreateContext(LoadingRequest request)
            => new LoadingContext(request.handle, request.target, request.userData);

        private static LoadingCompletion CreateCompletion(LoadingStatus status, in OperationResult result)
            => new LoadingCompletion(status, result.systemCode, result.resultCode, result.userCode);

#if UNITY_INCLUDE_TESTS
        internal static void ClearForTests()
        {
            for (int requestIndex = Requests.Count - 1; requestIndex >= 0; requestIndex--)
                Requests.RemoveAt(requestIndex);
        }
#endif
    }
}
