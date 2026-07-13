#if UNITY_EDITOR
using Systems.SimpleCore.Editor.Utility;
using Systems.SimpleDialogue.Data;
using UnityEditor;

namespace Systems.SimpleDialogue.Editor
{
    /// <summary>
    ///     Configures the SimpleDialogue table for CSV workflows.
    /// </summary>
    [InitializeOnLoad]
    public static class DialogueLocalizationSetup
    {
        static DialogueLocalizationSetup()
        {
            EnsureSetup();
        }

        private static void EnsureSetup()
        {
            LocalizationEditorAPI.EnsureCsvStringTableCollection(DialogueLocalization.TABLE_COLLECTION_NAME);
        }
    }
}
#endif
