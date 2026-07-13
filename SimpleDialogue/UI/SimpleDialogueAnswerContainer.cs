using System;
using Systems.SimpleDialogue.Data;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Components.Lists;

namespace Systems.SimpleDialogue.UI
{
    /// <summary>
    ///     SimpleUI list renderer for current dialogue answer options.
    /// </summary>
    public sealed class SimpleDialogueAnswerContainer :
        UIListBase<DialogueOptionListContext, DialogueOption>,
        IWithLocalContext<DialogueOptionListContext>
    {
        private static readonly DialogueOption[] EmptyOptions = Array.Empty<DialogueOption>();

        private readonly DialogueOptionListContext _emptyContext = new DialogueOptionListContext(EmptyOptions);
        private DialogueOptionListContext _context;

        public void SetOptions(DialogueOptionListContext context)
        {
            _context = context;
            RequestRefresh();
        }

        /// <summary>
        ///     Replaces the current answer list with an empty context and requests a SimpleUI refresh.
        /// </summary>
        public void ClearOptions()
        {
            SetOptions(_emptyContext);
            OnRender(_emptyContext);
        }

        public bool TryGetContext(out DialogueOptionListContext context)
        {
            context = _context;
            return !ReferenceEquals(context, null);
        }
    }
}
