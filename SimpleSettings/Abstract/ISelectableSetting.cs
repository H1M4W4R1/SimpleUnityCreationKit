using System.Collections.Generic;
using JetBrains.Annotations;

namespace Systems.SimpleSettings.Abstract
{
    /// <summary>
    ///     Non-generic UI hint: this setting should be represented as a dropdown / selector.
    /// </summary>
    public interface ISelectableSetting
    {
        /// <summary>Currently selected index in the options list.</summary>
        int SelectedIndex { get; }

        /// <summary>Returns all available options as boxed objects.</summary>
        [NotNull] IReadOnlyList<object> GetOptions();

        /// <summary>Returns a display-friendly label for the given boxed option.</summary>
        [NotNull] string GetOptionLabel([NotNull] object option);
    }

    /// <summary>
    ///     Generic UI hint: this setting should be represented as a dropdown / selector.
    /// </summary>
    /// <typeparam name="TValue">Type of each selectable option.</typeparam>
    public interface ISelectableSetting<TValue> : ISelectableSetting
    {
        /// <summary>Returns all available options as strongly-typed values.</summary>
        [NotNull] IReadOnlyList<TValue> GetTypedOptions();

        /// <summary>Returns a display-friendly label for the given typed option.</summary>
        [NotNull] string GetTypedOptionLabel(TValue option);
    }
}
