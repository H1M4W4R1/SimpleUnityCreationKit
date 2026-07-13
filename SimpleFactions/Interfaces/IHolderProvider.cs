namespace Systems.SimpleFactions.Interfaces
{
    /// <summary>
    ///     Internal bridge interface. <see cref="Abstract.FactionMembershipBase{THolder}"/> implements
    ///     this so that <see cref="Abstract.FactionBase{TFactionObject}"/> can extract a typed holder
    ///     reference from the non-generic <see cref="Data.Context.JoinFactionContext"/> (and similar
    ///     contexts) without making the base tier generic.
    /// </summary>
    /// <typeparam name="THolder">Type of the holder object.</typeparam>
    internal interface IHolderProvider<out THolder> where THolder : class
    {
        /// <summary>
        ///     Returns the typed holder this component represents.
        /// </summary>
        THolder Holder { get; }
    }
}
