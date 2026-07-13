using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Systems.SimpleCore.Examples;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleInventory.Abstract.Items;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Examples.Equipment;
using Systems.SimpleInventory.Examples.Items.Armour;
using Systems.SimpleInventory.Examples.Items.Armour.Abstract;
using Systems.SimpleInventory.Examples.Items.Food;
using Systems.SimpleInventory.Examples.Items.Food.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleInventory.Examples.Inventory
{
    [RequireComponent(typeof(ExampleEquipment))] public sealed class ExampleInventory : InventoryBase
    {
        [SerializeField] private bool _createRuntimeUI = true;

        [CanBeNull] private ExampleEquipment _equipment;
        [CanBeNull] private ExampleRuntimePanel _panel;

        private void Start()
        {
            _equipment = GetComponent<ExampleEquipment>();

            // Add example items to inventory

            // Leather armor
            TryAdd<ExampleLeatherBoots>(1, out _);
            TryAdd<ExampleLeatherPants>(1, out _);
            TryAdd<ExampleLeatherTunic>(1, out _);
            TryAdd<ExampleLeatherCap>(1, out _);

            // Steel armor
            TryAdd<ExampleSteelHelmet>(1, out _);
            TryAdd<ExampleSteelChestplate>(1, out _);
            TryAdd<ExampleSteelLeggings>(1, out _);
            TryAdd<ExampleSteelBoots>(1, out _);

            // Food 
            TryAdd<ExampleApple>(1, out _);
            TryAdd<ExampleBread>(1, out _);

            // Print database item count
            Debug.Log("Database entries count: " + ItemsDatabase.Count);

            if (_createRuntimeUI)
            {
                CreateRuntimeUI();
            }

            RefreshStatus("Inventory populated with food and armor examples.");
        }

        [ContextMenu("Use first food")]
        public void UseFirstFood()
        {
            UseAnyItem<ExampleFoodBase>();
            RefreshStatus("Used first available food item.");
        }

        [ContextMenu("Use best food")]
        public void UseBestFoodExample()
        {
            UseBestItem<ExampleFoodBase>();
            RefreshStatus("Used best available food item.");
        }

        [ContextMenu("Equip leather armor")] public void EquipLeatherArmor()
        {
            if (!_equipment) return;
            EquipAnyItem<ExampleLeatherBoots>(_equipment);
            EquipAnyItem<ExampleLeatherPants>(_equipment);
            EquipAnyItem<ExampleLeatherTunic>(_equipment);
            EquipAnyItem<ExampleLeatherCap>(_equipment);

            PrintEquippedArmor();
            RefreshStatus("Equipped leather armor set.");
        }

        [ContextMenu("Equip steel armor")] public void EquipSteelArmor()
        {
            if (!_equipment) return;
            EquipAnyItem<ExampleSteelHelmet>(_equipment);
            EquipAnyItem<ExampleSteelChestplate>(_equipment);
            EquipAnyItem<ExampleSteelLeggings>(_equipment);
            EquipAnyItem<ExampleSteelBoots>(_equipment);

            PrintEquippedArmor();
            RefreshStatus("Equipped steel armor set.");
        }

        [ContextMenu("Unequip armor")] public void UnequipArmor()
        {
            if (!_equipment) return;
            UnequipAnyItem<BootsItemBase>(_equipment);
            UnequipAnyItem<LeggingsItemBase>(_equipment);
            UnequipAnyItem<ChestplateItemBase>(_equipment);
            UnequipAnyItem<HelmetItemBase>(_equipment);

            PrintEquippedArmor();
            RefreshStatus("Unequipped armor.");
        }

        private void PrintEquippedArmor()
        {
            if (!_equipment) return;
            HelmetItemBase helmet = _equipment.GetFirstEquippedBaseItemFor<HelmetItemBase>();
            ChestplateItemBase chestplate = _equipment.GetFirstEquippedBaseItemFor<ChestplateItemBase>();
            LeggingsItemBase leggings = _equipment.GetFirstEquippedBaseItemFor<LeggingsItemBase>();
            BootsItemBase boots = _equipment.GetFirstEquippedBaseItemFor<BootsItemBase>();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Helmet: {(helmet ? helmet.name : "None")}");
            sb.AppendLine($"Chestplate: {(chestplate ? chestplate.name : "None")}");
            sb.AppendLine($"Leggings: {(leggings ? leggings.name : "None")}");
            sb.AppendLine($"Boots: {(boots ? boots.name : "None")}");
            Debug.Log(sb.ToString());
        }

        [ContextMenu("Print all equippable items")] public void PrintAllEquippableItems()
        {
            ROListAccess<EquippableItemBase> databaseItems = ItemsDatabase.GetAll<EquippableItemBase>();
            IReadOnlyList<EquippableItemBase> listAccess = databaseItems.List;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < listAccess.Count; i++)
            {
                sb.AppendLine($"{listAccess[i].name}");
            }

            Debug.Log(sb.ToString());
            databaseItems.Release();
            RefreshStatus("Printed all equippable database items.");
        }

        protected override void OnItemAdded(in AddItemContext context, in OperationResult result, int amountLeft)
        {
            base.OnItemAdded(in context, result, amountLeft);
            Debug.Log($"Item added: {context.itemInstance.Item.name}");
            RefreshStatus("Item added: " + context.itemInstance.Item.name);
        }

        protected override void OnItemAddFailed(in AddItemContext context, in OperationResult result)
        {
            base.OnItemAddFailed(in context, result);
            Debug.Log($"Item add failed: {context.itemInstance.Item.name}");
        }

        protected override void OnItemTaken(in TakeItemContext context, in OperationResult result, int amountLeft)
        {
            base.OnItemTaken(in context, result, amountLeft);
            Debug.Log($"Item taken: {context.itemInstance.name}");
        }

        protected override void OnItemTakeFailed(in TakeItemContext context, in OperationResult result)
        {
            base.OnItemTakeFailed(in context, result);
            Debug.Log($"Item take failed: {context.itemInstance.name}");
            RefreshStatus("Item take failed: " + context.itemInstance.name);
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleInventory Example",
                "Navigate food use, armor equipment, unequip, and database inspection cases.");

            _panel.AddSection("Food");
            Button firstFoodButton = _panel.AddButton("Use First Food");
            firstFoodButton.onClick.AddListener(UseFirstFood);

            Button bestFoodButton = _panel.AddButton("Use Best Food");
            bestFoodButton.onClick.AddListener(UseBestFoodExample);

            _panel.AddSection("Equipment");
            Button leatherButton = _panel.AddButton("Equip Leather Armor");
            leatherButton.onClick.AddListener(EquipLeatherArmor);

            Button steelButton = _panel.AddButton("Equip Steel Armor");
            steelButton.onClick.AddListener(EquipSteelArmor);

            Button unequipButton = _panel.AddButton("Unequip Armor");
            unequipButton.onClick.AddListener(UnequipArmor);

            Button printButton = _panel.AddButton("Print Equippable Items");
            printButton.onClick.AddListener(PrintAllEquippableItems);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null))
            {
                return;
            }

            string equippedText = BuildEquippedArmorStatus();
            int foodCount = Count<ExampleApple>() + Count<ExampleBread>();
            _panel.SetStatus(
                message +
                "\nFood items: " + foodCount +
                "\n" + equippedText);
        }

        private string BuildEquippedArmorStatus()
        {
            if (!_equipment)
            {
                return "Equipment component missing.";
            }

            HelmetItemBase helmet = _equipment.GetFirstEquippedBaseItemFor<HelmetItemBase>();
            ChestplateItemBase chestplate = _equipment.GetFirstEquippedBaseItemFor<ChestplateItemBase>();
            LeggingsItemBase leggings = _equipment.GetFirstEquippedBaseItemFor<LeggingsItemBase>();
            BootsItemBase boots = _equipment.GetFirstEquippedBaseItemFor<BootsItemBase>();

            string helmetName = helmet ? helmet.name : "None";
            string chestplateName = chestplate ? chestplate.name : "None";
            string leggingsName = leggings ? leggings.name : "None";
            string bootsName = boots ? boots.name : "None";
            return "Armor: " + helmetName + ", " + chestplateName + ", " + leggingsName + ", " + bootsName;
        }
    }
}
