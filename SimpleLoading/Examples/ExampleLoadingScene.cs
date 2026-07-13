using Systems.SimpleLoading.Components;
using Systems.SimpleLoading.Data;
using Systems.SimpleLoading.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleLoading.Examples
{
    /// <summary>Runs the staged loading and dynamic-world-part demonstration in the package example scene.</summary>
    [DisallowMultipleComponent]
    public sealed class ExampleLoadingScene : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private DynamicWorldPart _worldPart;
        [SerializeField] private GameObject _loadingScreenRoot;
        [SerializeField] private Slider _loadingProgressBar;

        private ExampleLoadingSequence _sequence;
        private LoadingSequenceHandle<ExampleLoadingSequence> _handle;

        /// <summary>Configures the generated example scene without storing editor-only setup code in the runtime path.</summary>
        public void Configure(
            Transform player,
            DynamicWorldPart worldPart,
            GameObject loadingScreenRoot,
            Slider loadingProgressBar)
        {
            _player = player;
            _worldPart = worldPart;
            _loadingScreenRoot = loadingScreenRoot;
            _loadingProgressBar = loadingProgressBar;
        }

        private void Start()
        {
            _worldPart.Configure(_player, _worldPart.transform.GetChild(0).gameObject, 8f, 12f);
            _sequence = ScriptableObject.CreateInstance<ExampleLoadingSequence>();
            if (_loadingScreenRoot) _loadingScreenRoot.SetActive(true);
            _handle = LoadingAPI.Load(_sequence, _player);
            if (!_handle.IsValid) Debug.LogError("[SimpleLoading] Example sequence could not start.");
        }

        private void Update()
        {
            if (!_handle.IsValid) return;
            if (_loadingProgressBar) _loadingProgressBar.value = LoadingAPI.GetCurrentTotalPercentage(_handle);
            LoadingStatus status = LoadingAPI.GetStatus(_handle);
            if (status == LoadingStatus.Running) return;
            if (_loadingScreenRoot) _loadingScreenRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_sequence) Destroy(_sequence);
        }
    }
}
