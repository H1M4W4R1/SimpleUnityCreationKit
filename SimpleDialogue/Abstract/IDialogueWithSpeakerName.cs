using JetBrains.Annotations;
using Systems.SimpleDialogue.Data;
using UnityEngine.Localization;

namespace Systems.SimpleDialogue.Abstract
{
    /// <summary>
    ///     Contract for dialogue nodes that provide a localized speaker name.
    /// </summary>
    public interface IDialogueWithSpeakerName
    {
        /// <summary>
        ///     Gets the localized speaker name for the current dialogue context.
        /// </summary>
        [NotNull] LocalizedString GetSpeakerName(in DialogueContext context);
    }
}
