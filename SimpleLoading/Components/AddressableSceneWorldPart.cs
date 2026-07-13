using Systems.SimpleCore.Operations;
using Systems.SimpleLoading.Data;
using Systems.SimpleLoading.Operations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Systems.SimpleLoading.Components
{
    /// <summary>Distance-driven world part that streams an Addressable scene additively.</summary>
    public sealed class AddressableSceneWorldPart : DynamicWorldPart
    {
        [SerializeField] private AssetReference _sceneReference;

        private AsyncOperationHandle<SceneInstance> _loadHandle;
        private AsyncOperationHandle<SceneInstance> _unloadHandle;
        private bool _hasLoadHandle;
        private bool _hasUnloadHandle;

        protected override bool HasWorldRoot() => true;

        protected override OperationResult BeginLoadingWorldPart(in DynamicWorldPartContext context)
        {
            if (ReferenceEquals(_sceneReference, null) || !_sceneReference.RuntimeKeyIsValid())
                return LoadingOperations.StageOperationMissing();

            _loadHandle = _sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
            _hasLoadHandle = true;
            return LoadingOperations.Permitted();
        }

        protected override OperationResult BeginUnloadingWorldPart(in DynamicWorldPartContext context)
        {
            if (!_hasLoadHandle || !_loadHandle.IsValid()) return LoadingOperations.Permitted();
            _unloadHandle = Addressables.UnloadSceneAsync(_loadHandle, true);
            _hasUnloadHandle = true;
            return LoadingOperations.Permitted();
        }

        protected override bool IsWorldPartLoadComplete() => !_hasLoadHandle || _loadHandle.IsDone;
        protected override bool HasWorldPartLoadFailed()
            => _hasLoadHandle && _loadHandle.IsDone && _loadHandle.Status != AsyncOperationStatus.Succeeded;
        protected override bool IsWorldPartUnloadComplete() => !_hasUnloadHandle || _unloadHandle.IsDone;
        protected override bool HasWorldPartUnloadFailed()
            => _hasUnloadHandle && _unloadHandle.IsDone && _unloadHandle.Status != AsyncOperationStatus.Succeeded;
    }
}
