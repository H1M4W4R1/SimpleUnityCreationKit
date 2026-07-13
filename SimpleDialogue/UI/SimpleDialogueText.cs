using JetBrains.Annotations;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Components.Text;

namespace Systems.SimpleDialogue.UI
{
    /// <summary>
    ///     TextMeshPro-backed SimpleUI text element used by dialogue renderers.
    /// </summary>
    public sealed class SimpleDialogueText : UITextObject, IWithLocalContext<string>
    {
        private string _text = string.Empty;

        public void SetText([CanBeNull] string text)
        {
            _text = ReferenceEquals(text, null) ? string.Empty : text;
            RequestRefresh();
        }

        public bool TryGetContext(out string context)
        {
            context = _text;
            return true;
        }
    }
}
