using System.Collections.Generic;
using Systems.SimpleCore.Examples;
using Systems.SimpleCore.Operations;
using Systems.SimpleSpawn.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleSpawn.Examples.Scripts
{
    [DisallowMultipleComponent]
    public sealed class ExampleSpawnScene : MonoBehaviour
    {
        [SerializeField] private ExampleSingleSpawner _spawner;
        [SerializeField] private int _spawnCount = 3;
        [SerializeField] private bool _createRuntimeUI = true;
        [SerializeField] private bool _runExampleOnStart;

        private ExampleRuntimePanel _panel;
        private string _lastResult = "none";

        private void Awake()
        {
            if (!_spawner)
            {
                _spawner = FindAnyObjectByType<ExampleSingleSpawner>(FindObjectsInactive.Include);
            }
        }

        private void Start()
        {
            if (_createRuntimeUI)
            {
                CreateRuntimeUI();
            }

            if (_runExampleOnStart)
            {
                RunExample();
            }
            else
            {
                RefreshStatus("Ready. Spawn or despawn entities.");
            }
        }

        [ContextMenu("Run Spawn Example")]
        public void RunExample()
        {
            if (!_spawner)
            {
                Debug.LogWarning("[SimpleSpawn] Example spawner is not assigned.");
                RefreshStatus("Example spawner is not assigned.");
                return;
            }

            for (int spawnIndex = 0; spawnIndex < _spawnCount; spawnIndex++)
            {
                OperationResult result = _spawner.TrySpawnSingle();
                _lastResult = ExampleRuntimePanel.FormatResult(result);
                Debug.Log("[SimpleSpawn] Spawn " + spawnIndex + " result: " + result);
            }

            RefreshStatus("Spawned configured batch.");
        }

        private void SpawnSingle()
        {
            if (!ValidateSpawner()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(_spawner.TrySpawnSingle());
            RefreshStatus("Spawned one entity.");
        }

        private void DespawnLatest()
        {
            if (!ValidateSpawner()) return;

            IReadOnlyList<ISpawnableEntity> spawnedEntities = _spawner.SpawnedEntities;
            if (spawnedEntities.Count == 0)
            {
                RefreshStatus("No spawned entities to despawn.");
                return;
            }

            _lastResult = ExampleRuntimePanel.FormatResult(_spawner.TryDespawn(spawnedEntities[spawnedEntities.Count - 1]));
            RefreshStatus("Despawned latest tracked entity.");
        }

        private void DespawnAll()
        {
            if (!ValidateSpawner()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(_spawner.DespawnAll());
            RefreshStatus("Despawned all tracked entities.");
        }

        private bool ValidateSpawner()
        {
            if (_spawner)
            {
                return true;
            }

            Debug.LogWarning("[SimpleSpawn] Example spawner is not assigned.");
            RefreshStatus("Example spawner is not assigned.");
            return false;
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleSpawn Example",
                "Navigate single spawns, batch spawning, latest despawn, and full cleanup.");

            _panel.AddSection("Spawner");
            Button spawnOneButton = _panel.AddButton("Spawn One");
            spawnOneButton.onClick.AddListener(SpawnSingle);

            Button spawnBatchButton = _panel.AddButton("Spawn Batch");
            spawnBatchButton.onClick.AddListener(RunExample);

            Button despawnLatestButton = _panel.AddButton("Despawn Latest");
            despawnLatestButton.onClick.AddListener(DespawnLatest);

            Button despawnAllButton = _panel.AddButton("Despawn All");
            despawnAllButton.onClick.AddListener(DespawnAll);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null))
            {
                return;
            }

            int spawnedCount = _spawner ? _spawner.SpawnedEntities.Count : 0;
            _panel.SetStatus(
                message +
                "\nTracked spawned entities: " + spawnedCount +
                "\nLast result: " + _lastResult);
        }
    }
}
