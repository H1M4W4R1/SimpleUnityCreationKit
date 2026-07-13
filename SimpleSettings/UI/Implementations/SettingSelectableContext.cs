using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleUI.Context.Selectors;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Concrete <see cref="SelectableContext{TListObject}"/> used internally by
    ///     <see cref="UIDropdownSetting{TValue}"/> to supply options from an
    ///     <see cref="ISelectableSetting{TValue}"/> to the underlying
    ///     <see cref="Systems.SimpleUI.Components.Selectors.Implementations.Dropdown.UIDropdownSelectorBase{TObjectType}"/>.
    /// </summary>
    internal sealed class SettingSelectableContext<TValue> : SelectableContext<TValue>
    {
        /// <inheritdoc/>
        public SettingSelectableContext([NotNull] IReadOnlyList<TValue> data,
                                        int defaultIndex = -1)
            : base(data, defaultIndex) { }
    }
}
