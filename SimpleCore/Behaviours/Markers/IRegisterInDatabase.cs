namespace Systems.SimpleCore.Behaviours.Markers
{
    /// <summary>
    ///     Marks a behaviour for automatic registration in one runtime database. The selected database remains an
    ///     implementation detail of the contract; consumers only invoke the registration lifecycle.
    /// </summary>
    public interface IRegisterInDatabase
    {
        /// <summary>Registers <paramref name="item"/> in the contract-selected database.</summary>
        bool RegisterInDatabase(object item);

        /// <summary>Removes <paramref name="item"/> from the contract-selected database.</summary>
        void UnregisterFromDatabase(object item);
    }
}
