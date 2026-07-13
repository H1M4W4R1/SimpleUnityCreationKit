# SimpleShops

SimpleShops is a logic-level shop system for shop-local offers and reversible transactions. It does not render UI and does not depend on inventory, economy, factions, reputation, quests, or items.

Shops are extensible `MonoBehaviour` components. Offers are `ScriptableObject` definitions owned by each shop through its serialized offer list. Transaction side effects live in the shop component, so each project can decide how currency, reputation, items, stock, limits, and custom barter rules work.

## Setup

1. Create a shop component by inheriting from `ShopBase`.
2. Create customer types that implement `IShopCustomer`.
3. Create purchase offer assets by inheriting from `PurchaseOfferBase`.
4. Create sell offer assets by inheriting from `SellOfferBase`.
5. Add offer assets to the target shop's offer list.
6. Override the shop transaction hooks to handle project-owned resources.

Offers are deliberately shop-local. `ShopAPI` and `ShopBase` reject an offer that is not present in the shop's `Offers` list.

## Transaction Flow

`ShopAPI.CanPurchase` and `ShopAPI.CanSell` validate:

- the shop, offer, and customer references
- the offer type
- that the offer belongs to the shop
- shop-level `CanPurchaseOffer` or `CanSellOffer`
- offer-level `CanPurchase` or `CanSell`
- shop-level `CanPayTransactionCosts`
- shop-level `CanGrantTransactionReturns`

`ShopAPI.TryPurchase` and `ShopAPI.TrySell` then call:

1. `PayTransactionCosts`
2. `GrantTransactionReturns`
3. success callbacks

If `GrantTransactionReturns` fails after costs were paid, the API calls `RollbackTransactionReturns` and then `RefundTransactionCosts`. If either reversion step fails, the final result is `ShopOperations.RevertFailed()`.

Use `ShopTransactionFlags.IgnoreTransactionConditions` only to bypass the preflight checks. It does not skip `PayTransactionCosts`, `GrantTransactionReturns`, or their reversion hooks.

## Shop Hooks

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimpleShops.Components;
using Systems.SimpleShops.Data.Context;
using Systems.SimpleShops.Operations;

namespace Game.Shops
{
    public sealed class TownShop : ShopBase
    {
        protected internal override OperationResult CanPayTransactionCosts(
            in ShopTransactionContext context)
        {
            return HasRequiredPayment(context)
                ? ShopOperations.Permitted()
                : ShopOperations.TransactionCostUnavailable();
        }

        protected internal override OperationResult PayTransactionCosts(
            in ShopTransactionContext context)
        {
            return TakePayment(context)
                ? ShopOperations.Permitted()
                : ShopOperations.TransactionCostPaymentFailed();
        }

        protected internal override OperationResult RefundTransactionCosts(
            in ShopTransactionContext context)
        {
            return RefundPayment(context)
                ? ShopOperations.Permitted()
                : ShopOperations.TransactionCostRefundFailed();
        }

        protected internal override OperationResult CanGrantTransactionReturns(
            in ShopTransactionContext context)
        {
            return HasReturnAvailable(context)
                ? ShopOperations.Permitted()
                : ShopOperations.TransactionReturnUnavailable();
        }

        protected internal override OperationResult GrantTransactionReturns(
            in ShopTransactionContext context)
        {
            return GrantReturn(context)
                ? ShopOperations.Permitted()
                : ShopOperations.TransactionReturnGrantFailed();
        }

        protected internal override OperationResult RollbackTransactionReturns(
            in ShopTransactionContext context)
        {
            return RemoveGrantedReturn(context)
                ? ShopOperations.Permitted()
                : ShopOperations.TransactionReturnRollbackFailed();
        }

        private bool HasRequiredPayment(in ShopTransactionContext context) => true;
        private bool TakePayment(in ShopTransactionContext context) => true;
        private bool RefundPayment(in ShopTransactionContext context) => true;
        private bool HasReturnAvailable(in ShopTransactionContext context) => true;
        private bool GrantReturn(in ShopTransactionContext context) => true;
        private bool RemoveGrantedReturn(in ShopTransactionContext context) => true;
    }
}
```

## Offers

```csharp
using Systems.SimpleShops.Abstract;

namespace Game.Shops
{
    public sealed class IronSwordPurchaseOffer : PurchaseOfferBase
    {
    }

    public sealed class IronSwordSellOffer : SellOfferBase
    {
    }
}
```

Override `CanPurchase`, `CanSell`, `OnPurchased`, `OnSold`, and failure callbacks when behavior belongs to the offer definition rather than the shop instance.

## Customers

```csharp
using Systems.SimpleShops.Abstract;

namespace Game.Shops
{
    public sealed class PlayerShopCustomer : IShopCustomer
    {
    }
}
```

The core package only requires the marker interface. A project can implement it on an entity component, player account object, save-backed profile, or another customer model.

## Runtime Use

```csharp
OperationResult result = shop.TryPurchase(ironSwordPurchaseOffer, playerCustomer);

if (!result)
{
    // Inspect result.systemCode and result.resultCode for shop-specific failure data.
}
```

Use `shop.TrySell(sellOffer, customer)` for sell offers. Static `ShopAPI` overloads are available when caller code does not want to go through the shop instance methods directly.

## Examples included

- `Scene - Shops.unity`: exposes runtime Unity UI for purchase checks, purchases, sell checks, sells, reset, and full transaction-flow cases.
- `ExampleShopScene`: scene driver with runtime buttons and a context menu action for replaying the example.
- `ExampleShop`, `ExampleShopCustomer`, and potion offers: minimal shop/customer/offer setup used by the scene.

## Notes

- Rendering is intentionally external.
- Offers are not globally addressable by this package.
- The package depends on `SimpleCore` only.
- Shop implementations own all resource side effects and can integrate with SimpleEconomy, SimpleInventory, SimpleFactions, or project-specific systems without adding dependencies to SimpleShops.
