# SimpleLoot

Weighted drop-table system with rarity tiers for Unity. Provides abstract base classes for loot tables, loot generators, and rarities. All concrete types are auto-registered as Addressable ScriptableObject assets and retrieved via singleton databases at runtime.

---

## Concepts

| Concept | Role |
|---|---|
| `RarityBase` | Designer-created SO that defines a drop weight (`Chance`). Attach to items or override per table entry. |
| `LootTableBase<TLoot>` | SO that holds a list of `LootTableEntry<TLoot>` — the items that can drop and their optional rarity overrides. |
| `LootDropGeneratorBase<TSelf, TLoot>` | SO that implements the selection algorithm (weighted, equal, custom). |
| `LootAPI` | Static entry point. Assembles `LootGenerationContext` internally and calls the generator. |

---

## Quick Start

### 1 — Define a rarity

```csharp
public sealed class CommonRarity : RarityBase
{
    [SerializeField] private float _chance = 100f;
    public override float Chance => _chance;
}
```

The asset is auto-created at `Assets/Generated/Rarity/CommonRarity.asset` and labeled `SimpleLoot.Rarities`.

### 2 — Create a loot item

Items can supply their own chance via `IWithChance`, delegate to a `RarityBase` via `IWithRarity`, or have neither (rarity can be assigned per-entry in the table instead).

```csharp
// Option A — direct chance on the item
[CreateAssetMenu(...)]
public sealed class MyItem : ScriptableObject, IWithChance
{
    [SerializeField] private float _chance = 20f;
    public float Chance => _chance;
}

// Option B — rarity on the item
[CreateAssetMenu(...)]
public sealed class MyItem : ScriptableObject, IWithRarity
{
    [SerializeField] private RarityBase _rarity;
    public RarityBase Rarity => _rarity;
}
```

### 3 — Create a loot table

```csharp
[CreateAssetMenu(menuName = "MyGame/My Item Loot Table")]
public sealed class MyItemLootTable : LootTableBase<MyItem> { }
```

Create the asset in the Project window. Add entries via the inspector. Each entry exposes an optional **Rarity Override** — if set, it takes precedence over the item's own chance.

### 4 — Create a generator

Subclass either `WeightedLootDropGenerator` (chance-based) or `EqualLootDropGenerator` (uniform random):

```csharp
public sealed class MyWeightedGenerator
    : WeightedLootDropGenerator<MyWeightedGenerator, MyItem>
{
    protected override void OnLootGenerated(
        IReadOnlyList<MyItem> loot, in LootGenerationContext<MyItem> context) { }

    protected override void OnLootGenerationFailed(
        in LootGenerationContext<MyItem> context) { }
}
```

The asset is auto-created at `Assets/Generated/LootGenerators/MyWeightedGenerator.asset`.

### 5 — Generate loot

```csharp
// budget = number of rolls
ROListAccess<MyItem> loot = LootAPI.GenerateLoot<MyWeightedGenerator, MyItem>(myTable, 3);

for (int itemIndex = 0; itemIndex < loot.List.Count; itemIndex++)
{
    MyItem item = loot.List[itemIndex];
    Debug.Log(item.name);
}

loot.Release(); // always release — backed by a pool
```

---

## Chance Resolution Order

For each entry the generator evaluates chance in this order:

1. **Entry's `RarityOverride`** (set in the table inspector) — if non-null, use `RarityOverride.Chance`.
2. **`IWithChance` on the item** — if the item implements `IWithChance`, use `item.Chance`.
3. **`IWithRarity` on the item** — if the item implements `IWithRarity` and `item.Rarity != null`, use `item.Rarity.Chance`.
4. **`0f` fallback** — item is excluded from weighted selection (still reachable via `EqualLootDropGenerator` if all weights are zero, or if `IgnoreConditions` is used).

---

Negative, infinite, and `NaN` weights are ignored by weighted selection. If all remaining weights are zero,
the generator falls back to equal weights.

## LootGenerationFlags

Pass as the optional third argument to `LootAPI.GenerateLoot`:

| Flag | Effect |
|---|---|
| `None` (default) | Normal generation, `CanGenerateItem` is evaluated. |
| `IgnoreConditions` | Skips all `CanGenerateItem` checks — all entries are considered valid. |

```csharp
LootAPI.GenerateLoot<MyGenerator, MyItem>(table, 5, LootGenerationFlags.IgnoreConditions);
```

---

## Custom Condition Checks

Override `CanGenerateItem` to block specific entries. Use `LootOperations` factory methods for return values:

```csharp
protected override OperationResult CanGenerateItem(
    LootTableEntry<MyItem> entry, in LootGenerationContext<MyItem> context)
{
    if (entry.Item.RequiredLevel > PlayerLevel)
        return LootOperations.ItemConditionFailed();

    return LootOperations.Permitted();
}
```

---

## Callbacks

Both methods are `protected abstract` — every concrete generator must implement them.

| Method | When it fires |
|---|---|
| `OnLootGenerated(IReadOnlyList<TLoot>, in context)` | After every successful `GenerateDrops` call. |
| `OnLootGenerationFailed(in context)` | When no valid items could be selected (empty table, all blocked, `budget <= 0`). |

`IReadOnlyList<TLoot>` is a view into the pooled result list — it is valid only for the duration of the callback. Do not store it beyond the method.

---

## LootGenerationContext

`readonly ref struct` — created exclusively by `LootAPI`. Contains the generation parameters:

| Field | Type | Description |
|---|---|---|
| `Table` | `LootTableBase<TLoot>` | The table being sampled. |
| `Budget` | `long` | Number of rolls requested. |
| `Flags` | `LootGenerationFlags` | Active generation flags. |

---

## Pity System Pattern

Override `GenerateDrops`, set a flag before delegating to `base.GenerateDrops`, then check the flag inside `CanGenerateItem`:

```csharp
public sealed class MyPityGenerator
    : WeightedLootDropGenerator<MyPityGenerator, MyItem>
{
    private int _rollsSinceRare;
    private bool _isPityActive;

    public override ROListAccess<MyItem> GenerateDrops(in LootGenerationContext<MyItem> context)
    {
        _isPityActive = (_rollsSinceRare >= 10);
        return base.GenerateDrops(context); // weighted algorithm calls CanGenerateItem
    }

    protected override OperationResult CanGenerateItem(
        LootTableEntry<MyItem> entry, in LootGenerationContext<MyItem> context)
    {
        if (_isPityActive && !entry.Item.IsRare)
            return LootOperations.ItemConditionFailed();
        return LootOperations.Permitted();
    }

    protected override void OnLootGenerated(IReadOnlyList<MyItem> loot,
        in LootGenerationContext<MyItem> context)
    {
        bool rareDropped = false;
        for (int i = 0; i < loot.Count; i++)
            if (loot[i].IsRare) { rareDropped = true; break; }

        _rollsSinceRare = rareDropped ? 0 : _rollsSinceRare + (int)context.Budget;
    }

    protected override void OnLootGenerationFailed(in LootGenerationContext<MyItem> context) { }
}
```

See `Examples/Scripts/ExamplePityItemGenerator.cs` for a full commented version.

---

## LootOperations Reference

`Systems.SimpleLoot.Operations.LootOperations` — system code `0x000A`.

| Method | Meaning |
|---|---|
| `Permitted()` | Item can drop (default `CanGenerateItem` return). |
| `Denied()` | Item is generically denied. |
| `ItemConditionFailed()` | Item failed a custom condition check. |
| `GeneratorNotFound()` | Generator asset missing from `LootGeneratorDatabase`. |
| `NoValidItems()` | All table entries were blocked. |
| `LootGenerated()` | Successful generation result code. |

---

## Auto-Registration

The `[AutoCreate]` attribute on `RarityBase` and `LootDropGeneratorBase` causes the editor to create `.asset` files for every concrete sealed subclass automatically on domain reload:

- Rarities → `Assets/Generated/Rarity/` — label `SimpleLoot.Rarities`
- Generators → `Assets/Generated/LootGenerators/` — label `SimpleLoot.LootGenerators`

Loot tables are **not** auto-created — make them with `[CreateAssetMenu]` and place them anywhere in your project.

---

## Examples

Open `Examples/SimpleLoot - Example Scene.unity` and enter Play Mode to use the runtime UI for weighted, conditional, ignore-condition, and pity generation cases. The scene is wired to `Examples/Assets/Example Item Loot Table.asset`, which contains common, rare, and locked sample items.

Located in `Examples/`:

| File | Demonstrates |
|---|---|
| `SimpleLoot - Example Scene.unity` | Playable runtime UI scene for all loot generator examples |
| `Assets/Example Item Loot Table.asset` | Sample table used by the scene |
| `Scripts/ExampleLootItem.cs` | `IWithChance` integration, `IsRare` and `IsLocked` flags |
| `Scripts/ExampleItemLootTable.cs` | Minimal `LootTableBase<T>` subclass |
| `Scripts/ExampleLootScene.cs` | Runtime Unity UI for weighted, conditional, ignore-condition, and pity generation cases |
| `Scripts/ExampleWeightedItemGenerator.cs` | Minimal weighted generator, empty callbacks |
| `Scripts/ExampleConditionalItemGenerator.cs` | `CanGenerateItem` with `IsLocked` filtering |
| `Scripts/ExamplePityItemGenerator.cs` | Pity counter, `GenerateDrops` override, counter reset |
