using JetBrains.Annotations;
using UnityEngine.InputSystem;

namespace Systems.SimpleSettings.Abstract
{
    /// <summary>
    ///     UI hint: this setting represents a single input binding and should be
    ///     represented as a keybind button that triggers the InputSystem rebind flow.
    /// </summary>
    public interface IKeybindSetting
    {
        /// <summary>The InputAction this binding belongs to.</summary>
        [NotNull] [UsedImplicitly] InputAction Action { get; }

        /// <summary>Index of the binding within <see cref="Action"/>.</summary>
        [UsedImplicitly] int BindingIndex { get; }
    }
}
