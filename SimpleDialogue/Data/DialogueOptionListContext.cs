using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleUI.Context.Lists;

namespace Systems.SimpleDialogue.Data
{
    /// <summary>
    ///     SimpleUI list context for dialogue answer options.
    /// </summary>
    public sealed class DialogueOptionListContext : ListContext<DialogueOption>
    {
        public DialogueOptionListContext([NotNull] IReadOnlyList<DialogueOption> data) : base(data)
        {
        }
    }
}
