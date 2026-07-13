using Systems.SimpleCore.Operations;
using Systems.SimpleLoading.Data;
using Systems.SimpleLoading.Operations;
using Systems.SimpleLoading.Utility;
using UnityEngine;

namespace Systems.SimpleLoading.Components
{
    /// <summary>
    ///     Loads or unloads a world-part root as its target crosses configured distance thresholds.
    /// </summary>
    /// <remarks>
    ///     Keep this component outside <see cref="_worldRoot"/> so it can observe the target after the world part
    ///     is unloaded. Derived types can asynchronously load Addressable scenes by overriding the operation hooks.
    /// </remarks>
    [DisallowMultipleComponent]
    public class DynamicWorldPart : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private GameObject _worldRoot;
        [SerializeField, Min(0f)] private float _loadDistance = 75f;
        [SerializeField, Min(0f)] private float _unloadDistance = 100f;
        [SerializeField] private bool _evaluateOnEnable = true;

        private Transform _transform;
        private bool _isLoaded;
        private bool _isLoading;
        private bool _isUnloading;

        /// <summary>Whether the part is fully available.</summary>
        public bool IsLoaded => _isLoaded;

        /// <summary>Sets the object whose position determines world-part availability.</summary>
        public void SetTarget(Transform target)
        {
            _target = target;
            Evaluate();
        }

        /// <summary>Configures a root-based dynamic world part without requiring a custom subclass.</summary>
        public void Configure(Transform target, GameObject worldRoot, float loadDistance, float unloadDistance)
        {
            _target = target;
            _worldRoot = worldRoot;
            _loadDistance = loadDistance;
            _unloadDistance = Mathf.Max(loadDistance, unloadDistance);
            _isLoaded = _worldRoot && _worldRoot.activeSelf;
            Evaluate();
        }

        /// <summary>Immediately begins loading this part, independent of target position.</summary>
        public OperationResult LoadNow()
        {
            if (_isLoaded || _isLoading) return LoadingOperations.Permitted();
            DynamicWorldPartContext context = new DynamicWorldPartContext(this, _target);
            OperationResult canLoadResult = CanLoadWorldPart(in context);
            if (!canLoadResult)
            {
                OnWorldPartLoadFailed(in context, in canLoadResult);
                return canLoadResult;
            }

            _isLoading = true;
            OnWorldPartLoadStarted(in context);
            OperationResult loadResult = BeginLoadingWorldPart(in context);
            if (!loadResult)
            {
                _isLoading = false;
                OnWorldPartLoadFailed(in context, in loadResult);
                return loadResult;
            }

            TryCompleteLoading(in context);
            return loadResult;
        }

        /// <summary>Immediately begins unloading this part, independent of target position.</summary>
        public OperationResult UnloadNow()
        {
            if (!_isLoaded || _isUnloading) return LoadingOperations.Permitted();
            DynamicWorldPartContext context = new DynamicWorldPartContext(this, _target);
            OperationResult canUnloadResult = CanUnloadWorldPart(in context);
            if (!canUnloadResult)
            {
                OnWorldPartUnloadFailed(in context, in canUnloadResult);
                return canUnloadResult;
            }

            _isUnloading = true;
            OnWorldPartUnloadStarted(in context);
            OperationResult unloadResult = BeginUnloadingWorldPart(in context);
            if (!unloadResult)
            {
                _isUnloading = false;
                OnWorldPartUnloadFailed(in context, in unloadResult);
                return unloadResult;
            }

            TryCompleteUnloading(in context);
            return unloadResult;
        }

        /// <summary>Evaluates the current target position against the load and unload distances.</summary>
        public void Evaluate()
        {
            if (!_target || _isLoading || _isUnloading) return;
            Transform cachedTransform = GetCachedTransform();
            bool shouldBeLoaded = LoadingAPI.ShouldLoadWorldPart(
                _target.position,
                cachedTransform.position,
                _loadDistance,
                _unloadDistance,
                _isLoaded);

            if (shouldBeLoaded && !_isLoaded)
                LoadNow();
            else if (!shouldBeLoaded && _isLoaded)
                UnloadNow();
        }

        protected virtual OperationResult CanLoadWorldPart(in DynamicWorldPartContext context)
        {
            if (!HasWorldRoot()) return LoadingOperations.WorldPartRootMissing();
            if (_loadDistance < 0f || _unloadDistance < _loadDistance)
                return LoadingOperations.WorldPartDistanceInvalid();
            return LoadingOperations.Permitted();
        }

        protected virtual OperationResult CanUnloadWorldPart(in DynamicWorldPartContext context)
            => HasWorldRoot() ? LoadingOperations.Permitted() : LoadingOperations.WorldPartRootMissing();

        protected virtual OperationResult BeginLoadingWorldPart(in DynamicWorldPartContext context)
        {
            _worldRoot.SetActive(true);
            return LoadingOperations.WorldPartLoaded();
        }

        protected virtual OperationResult BeginUnloadingWorldPart(in DynamicWorldPartContext context)
        {
            _worldRoot.SetActive(false);
            return LoadingOperations.WorldPartUnloaded();
        }

        protected virtual bool IsWorldPartLoadComplete() => true;
        protected virtual bool HasWorldPartLoadFailed() => false;
        protected virtual bool IsWorldPartUnloadComplete() => true;
        protected virtual bool HasWorldPartUnloadFailed() => false;
        protected virtual bool HasWorldRoot() => _worldRoot;

        protected virtual void OnWorldPartLoadStarted(in DynamicWorldPartContext context) { }
        protected virtual void OnWorldPartLoaded(in DynamicWorldPartContext context, in OperationResult result) { }
        protected virtual void OnWorldPartLoadFailed(in DynamicWorldPartContext context, in OperationResult result) { }
        protected virtual void OnWorldPartUnloadStarted(in DynamicWorldPartContext context) { }
        protected virtual void OnWorldPartUnloaded(in DynamicWorldPartContext context, in OperationResult result) { }
        protected virtual void OnWorldPartUnloadFailed(in DynamicWorldPartContext context, in OperationResult result) { }

        private void Awake()
        {
            _transform = transform;
            _isLoaded = _worldRoot && _worldRoot.activeSelf;
        }

        private void OnEnable()
        {
            if (_evaluateOnEnable) Evaluate();
        }

        private void Update()
        {
            DynamicWorldPartContext context = new DynamicWorldPartContext(this, _target);
            if (_isLoading)
                TryCompleteLoading(in context);
            else if (_isUnloading)
                TryCompleteUnloading(in context);
            else
                Evaluate();
        }

        private void OnValidate()
        {
            _loadDistance = Mathf.Max(0f, _loadDistance);
            _unloadDistance = Mathf.Max(_loadDistance, _unloadDistance);
        }

        private void TryCompleteLoading(in DynamicWorldPartContext context)
        {
            if (HasWorldPartLoadFailed())
            {
                _isLoading = false;
                OperationResult failedResult = LoadingOperations.StageOperationMissing();
                OnWorldPartLoadFailed(in context, in failedResult);
                return;
            }

            if (!IsWorldPartLoadComplete()) return;
            _isLoading = false;
            _isLoaded = true;
            OperationResult loadedResult = LoadingOperations.WorldPartLoaded();
            OnWorldPartLoaded(in context, in loadedResult);
        }

        private void TryCompleteUnloading(in DynamicWorldPartContext context)
        {
            if (HasWorldPartUnloadFailed())
            {
                _isUnloading = false;
                OperationResult failedResult = LoadingOperations.StageOperationMissing();
                OnWorldPartUnloadFailed(in context, in failedResult);
                return;
            }

            if (!IsWorldPartUnloadComplete()) return;
            _isUnloading = false;
            _isLoaded = false;
            OperationResult unloadedResult = LoadingOperations.WorldPartUnloaded();
            OnWorldPartUnloaded(in context, in unloadedResult);
        }

        private Transform GetCachedTransform()
        {
            if (!ReferenceEquals(_transform, null)) return _transform;
            _transform = transform;
            return _transform;
        }
    }
}
