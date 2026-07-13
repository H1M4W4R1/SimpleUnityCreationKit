using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;

namespace Systems.SimpleBuilding.Examples
{
    /// <summary>
    ///     One-slot building used to demonstrate <see cref="ISlotBuilding"/> placement.
    /// </summary>
    public sealed class ExampleSlotBuilding : BuildingBase, ISlotBuilding
    {
        public int SlotCount => 1;
    }
}
