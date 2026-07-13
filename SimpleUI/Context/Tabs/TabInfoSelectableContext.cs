using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleUI.Components.Selectors.Tabs;
using Systems.SimpleUI.Context.Selectors;

namespace Systems.SimpleUI.Context.Tabs
{
    /// <summary>
    ///     Basic tab info selector context
    /// </summary>
    public sealed class TabInfoSelectableContext : SelectableContext<UITab>
    {
        public TabInfoSelectableContext([NotNull] IReadOnlyList<UITab> data, int defaultIndex = 0) : 
            base(data, defaultIndex)
        {
        }
    }
}