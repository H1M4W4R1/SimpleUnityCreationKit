# Simple Economy
SimpleEconomy is a lightweight, type-safe currency and wallet system for Unity games. It provides robust management of in-game resources (gold, mana, points, etc.) with built-in overflow/underflow protection, customizable conditions, and override-based operation callbacks.

## Requirements

SimpleEconomy requires the following dependencies:

- **Unity Engine**: Standard MonoBehaviour and ScriptableObject support
- **Unity.Burst**: Performance optimization
- **Unity.Collections**: Collection utilities
- **Unity.Mathematics**: Math operations (particularly `math.min`)
- **SimpleCore**: Core operation results, database system, and action source enumerations

All dependencies are declared in `SimpleEconomy.asmdef`.

## Usage

### Create a Custom Currency

Extend `CurrencyBase` to define a new currency type:

```csharp
using Systems.SimpleEconomy.Currencies;
using Systems.SimpleEconomy.Data.Context;
using Systems.SimpleCore.Operations;

public class GoldCurrency : CurrencyBase
{
    // Optionally override validation and callback methods
    protected internal override OperationResult CanBeAdded(in CurrencyAddContext context)
    {
        // Custom add conditions
        return base.CanBeAdded(context);
    }
}
```

### Create a Currency Wallet

Extend `CurrencyWalletBase<TCurrencyType>` to create a wallet for your currency:

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimpleEconomy.Data.Context;
using Systems.SimpleEconomy.Wallets;
using UnityEngine;

public class PlayerGoldWallet : CurrencyWalletBase<GoldCurrency>
{
    // Optionally override callback methods to respond to currency changes
    protected override void OnCurrencyAdded(
        in CurrencyAddContext context,
        in OperationResult result,
        long amountLeft)
    {
        // React to currency addition (e.g., update UI)
        Debug.Log($"Gold added: {context.amount}");
    }
}
```

Attach this component to a GameObject in your scene or reference it directly.

### Add and Take Currency

```csharp
// Get wallet reference
PlayerGoldWallet wallet = GetComponent<PlayerGoldWallet>();

// Add currency with overflow protection (default)
OperationResult addResult = wallet.TryAdd(100);
if (addResult)
{
    Debug.Log("Successfully added 100 gold");
}

// Take currency
OperationResult takeResult = wallet.TryTake(50);
if (takeResult)
{
    Debug.Log("Successfully took 50 gold");
}

// Check balance
if (wallet.Has(100))
{
    Debug.Log("Wallet has at least 100 gold");
}
```

### Global Wallet (Singleton Pattern)

Create a shared wallet across your entire game:

```csharp
public class GlobalGoldWallet : GlobalCurrencyWalletBase<GlobalGoldWallet, GoldCurrency>
{
}

// Access from anywhere
GlobalGoldWallet.Instance.TryAdd(1000);
```

### Custom Conditions

Override `CanBeTaken` and `CanBeAdded` in your currency to enforce custom rules:

```csharp
public class RestrictedCurrency : CurrencyBase
{
    protected internal override OperationResult CanBeTaken(in CurrencyTakeContext context)
    {
        // Example: prevent taking during a specific game state
        if (IsGamePaused) return EconomyOperations.NotEnoughCurrency();
        return EconomyOperations.Permitted();
    }
}
```

### Behavior Flags

Use `ModifyWalletCurrencyFlags` to modify operation behavior:

```csharp
// Ignore custom conditions; overflow/underflow limits still apply.
// On TryTake with insufficient balance: takes as much as possible and returns CurrencyTakenPartial.
wallet.TryAdd(100, ModifyWalletCurrencyFlags.IgnoreConditions);
wallet.TryTake(100, ModifyWalletCurrencyFlags.IgnoreConditions);

// Disable overflow protection on add
wallet.TryAdd(long.MaxValue, ModifyWalletCurrencyFlags.IgnoreBalanceLimits);

// Allow balance to go negative on take (for systems like debt)
wallet.TryTake(500, ModifyWalletCurrencyFlags.IgnoreBalanceLimits);

// Ignore all checks
wallet.TryTake(500,
    ModifyWalletCurrencyFlags.IgnoreConditions |
    ModifyWalletCurrencyFlags.IgnoreBalanceLimits);
```

### Action Source Tracking

Track whether operations come from internal or external sources:

```csharp
using Systems.SimpleCore.Utility.Enums;

// Internal operation (callbacks will not fire)
wallet.TryAdd(50, actionSource: ActionSource.Internal);

// External operation (callbacks will fire normally)
wallet.TryAdd(50, actionSource: ActionSource.External);
```

## Operation Callbacks

Respond to currency operations by overriding callback methods in your wallet or currency:

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimpleEconomy.Data.Context;
using Systems.SimpleEconomy.Wallets;

public class CustomWallet : CurrencyWalletBase<MyCurrency>
{
    protected override void OnCurrencyAdded(
        in CurrencyAddContext context,
        in OperationResult result,
        long amountLeft)
    {
        // Handle successful addition
    }

    protected override void OnCurrencyAddFailed(
        in CurrencyAddContext context,
        in OperationResult result)
    {
        // Handle failed addition
    }

    protected override void OnCurrencyTaken(
        in CurrencyTakeContext context,
        in OperationResult result,
        long amountLeft)
    {
        // Handle successful take
    }

    protected override void OnCurrencyTakeFailed(
        in CurrencyTakeContext context,
        in OperationResult result)
    {
        // Handle failed take
    }
}
```

## Thread Safety

Currency wallets use internal locking to ensure thread-safe balance modifications. All `TryAdd` and `TryTake` operations are atomic.

## Examples included

- `Scene - Economy.unity`: exposes runtime Unity UI for grant, spend, overspend, debt-spend, reset, and full wallet-flow cases.
- `ExampleEconomyScene`: scene driver with runtime buttons and a context menu action for replaying the example.
- `ExampleGoldCurrency` and `ExampleGoldWallet`: minimal typed currency and wallet setup.
