using System.Collections.Generic;
using Systems.SimpleUI.Components.Selectors.Tabs;
using Systems.SimpleUI.Context.Abstract;
using Systems.SimpleUI.Context.Tabs;
using UnityEngine;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Provides <see cref="TabInfoSelectableContext"/> for <see cref="UISettingsTabSelector"/>
    ///     by discovering all <see cref="UISettingsTab"/> children at startup.
    /// </summary>
    /// <remarks>
    ///     Attach on the same GameObject as <see cref="UISettingsTabSelector"/>.
    ///     Each <see cref="UISettingsTab"/> in the hierarchy (including inactive objects) is
    ///     registered as a tab in the order returned by <c>GetComponentsInChildren</c>.
    /// </remarks>
    public sealed class UISettingsTabContextProvider : ContextProviderBase<TabInfoSelectableContext>
    {
        [SerializeField] private int _defaultTabIndex;

        private TabInfoSelectableContext _context;

        private void Awake()
        {
            UISettingsTab[] tabs = GetComponentsInChildren<UISettingsTab>(true);
            List<UITab> list = new(tabs);
            _context = new TabInfoSelectableContext(list, _defaultTabIndex);
        }

        public override TabInfoSelectableContext GetContext() => _context;
    }
}
