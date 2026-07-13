using System.Text;
using Systems.SimpleCore.Examples;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleLoot.Abstract.Generator;
using Systems.SimpleLoot.Data;
using Systems.SimpleLoot.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleLoot.Examples.Scripts
{
    [DisallowMultipleComponent]
    public sealed class ExampleLootScene : MonoBehaviour
    {
        [SerializeField] private ExampleItemLootTable _lootTable;
        [SerializeField] private long _budget = 3L;
        [SerializeField] private bool _createRuntimeUI = true;

        private ExampleRuntimePanel _panel;
        private string _lastDrops = "none";

        private void Start()
        {
            if (_createRuntimeUI)
            {
                CreateRuntimeUI();
            }

            RefreshStatus("Ready. Generate loot with one of the example generators.");
        }

        [ContextMenu("Generate Weighted Loot")]
        private void GenerateWeightedLoot()
        {
            GenerateLoot<ExampleWeightedItemGenerator>("Weighted", LootGenerationFlags.None);
        }

        [ContextMenu("Generate Conditional Loot")]
        private void GenerateConditionalLoot()
        {
            GenerateLoot<ExampleConditionalItemGenerator>("Conditional", LootGenerationFlags.None);
        }

        [ContextMenu("Generate Conditional Loot Ignoring Conditions")]
        private void GenerateConditionalLootIgnoringConditions()
        {
            GenerateLoot<ExampleConditionalItemGenerator>("Conditional ignore conditions", LootGenerationFlags.IgnoreConditions);
        }

        [ContextMenu("Generate Pity Loot")]
        private void GeneratePityLoot()
        {
            GenerateLoot<ExamplePityItemGenerator>("Pity", LootGenerationFlags.None);
        }

        private void GenerateLoot<TLootGenerator>(string label, LootGenerationFlags flags)
            where TLootGenerator : LootDropGeneratorBase<TLootGenerator, ExampleLootItem>, new()
        {
            if (ReferenceEquals(_lootTable, null) || !_lootTable)
            {
                Debug.LogWarning("[SimpleLoot] Example loot table is not assigned.");
                RefreshStatus("Example loot table is not assigned.");
                return;
            }

            ROListAccess<ExampleLootItem> loot = LootAPI.GenerateLoot<TLootGenerator, ExampleLootItem>(_lootTable, _budget, flags);
            StringBuilder builder = new StringBuilder();
            for (int lootIndex = 0; lootIndex < loot.List.Count; lootIndex++)
            {
                ExampleLootItem item = loot.List[lootIndex];
                if (ReferenceEquals(item, null) || !item)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(item.name);
            }

            _lastDrops = builder.Length == 0 ? "none" : builder.ToString();
            loot.Release();
            Debug.Log("[SimpleLoot] " + label + " drops: " + _lastDrops);
            RefreshStatus(label + " generation completed.");
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleLoot Example",
                "Navigate weighted, conditional, ignore-condition, and pity loot generation cases.");

            _panel.AddSection("Generators");
            Button weightedButton = _panel.AddButton("Generate Weighted Loot");
            weightedButton.onClick.AddListener(GenerateWeightedLoot);

            Button conditionalButton = _panel.AddButton("Generate Conditional Loot");
            conditionalButton.onClick.AddListener(GenerateConditionalLoot);

            Button ignoreButton = _panel.AddButton("Ignore Conditions");
            ignoreButton.onClick.AddListener(GenerateConditionalLootIgnoringConditions);

            Button pityButton = _panel.AddButton("Generate Pity Loot");
            pityButton.onClick.AddListener(GeneratePityLoot);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null))
            {
                return;
            }

            _panel.SetStatus(
                message +
                "\nBudget: " + _budget +
                "\nLast drops: " + _lastDrops);
        }
    }
}
