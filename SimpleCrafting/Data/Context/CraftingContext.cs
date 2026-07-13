using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleCrafting.Abstract;

namespace Systems.SimpleCrafting.Data.Context
{
    public readonly ref struct CraftingContext
    {
        [CanBeNull] public readonly CraftingRecipeBase recipe;
        [CanBeNull] public readonly CraftingStationBase station;
        [CanBeNull] public readonly IReadOnlyList<CraftingStationBase> stations;
        [CanBeNull] public readonly ICraftingUser user;
        public readonly ActionSource actionSource;

        public CraftingContext(
            [CanBeNull] CraftingRecipeBase recipe,
            [CanBeNull] CraftingStationBase station = null,
            [CanBeNull] ICraftingUser user = null,
            ActionSource actionSource = ActionSource.External)
        {
            this.recipe = recipe;
            this.station = station;
            stations = null;
            this.user = user;
            this.actionSource = actionSource;
        }

        public CraftingContext(
            [CanBeNull] CraftingRecipeBase recipe,
            [CanBeNull] IReadOnlyList<CraftingStationBase> stations,
            [CanBeNull] ICraftingUser user = null,
            ActionSource actionSource = ActionSource.External)
        {
            this.recipe = recipe;
            station = null;
            this.stations = stations;
            this.user = user;
            this.actionSource = actionSource;
        }
    }
}
