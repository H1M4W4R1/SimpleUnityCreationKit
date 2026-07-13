using Systems.SimpleStats.Abstract;
using Systems.SimpleStats.Abstract.Modifiers;

namespace Systems.SimpleStats.Data
{
    /// <summary>
    ///     Zero-allocation context for modifier operations.
    ///     Passed to validation, events, and conditional checks.
    /// </summary>
    public readonly ref struct ModifierContext
    {
        /// <summary>
        ///     The modifier being added, removed, or evaluated
        /// </summary>
        public readonly IStatModifier modifier;

        /// <summary>
        ///     The stats collection owner
        /// </summary>
        public readonly IWithStatModifiers owner;

        public ModifierContext(IStatModifier modifier, IWithStatModifiers owner)
        {
            this.modifier = modifier;
            this.owner = owner;
        }
    }
}
