namespace Systems.SimpleBuilding.Abstract
{
    /// <summary>
    ///     Marks a building prefab as requiring a fixed number of unoccupied <c>BuildingSlot</c>s.
    /// </summary>
    public interface ISlotBuilding
    {
        int SlotCount { get; }

        /// <summary>
        ///     Whether placement should use the selected slot transform automatically.
        /// </summary>
        bool SnapToSlot => true;
    }
}
