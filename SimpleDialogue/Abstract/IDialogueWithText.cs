using JetBrains.Annotations;
using Systems.SimpleDialogue.Data;
using UnityEngine.Localization;

namespace Systems.SimpleDialogue.Abstract
{
    /// <summary>
    ///     Contract for dialogue nodes that provide localized display text.
    /// </summary>
    public interface IDialogueWithText
    {
        /// <summary>
        ///     Gets the localized display text for the current dialogue context.
        /// </summary>
        [NotNull] LocalizedString GetText(in DialogueContext context);
    }
}
