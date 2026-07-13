using NUnit.Framework;
using Systems.SimpleDialogue.Data;
using Systems.SimpleDialogue.UI;
using UnityEngine;

namespace Systems.SimpleDialogue.Tests
{
    public sealed class DialogueRendererTests
    {
        [Test]
        public void ClearOptions_WhenContainerHasOptions_ProvidesEmptyContext()
        {
            GameObject gameObject = new GameObject("Answer Container", typeof(RectTransform), typeof(Canvas));
            try
            {
                SimpleDialogueAnswerContainer container = gameObject.AddComponent<SimpleDialogueAnswerContainer>();
                DialogueOption option = default;
                DialogueOption[] options = {option};
                DialogueOptionListContext optionsContext = new DialogueOptionListContext(options);
                container.SetOptions(optionsContext);

                bool hadOptions = container.TryGetContext(out DialogueOptionListContext currentContext);

                Assert.IsTrue(hadOptions);
                Assert.AreEqual(1, currentContext.Count);

                container.ClearOptions();

                bool hasEmptyOptions = container.TryGetContext(out DialogueOptionListContext emptyContext);

                Assert.IsTrue(hasEmptyOptions);
                Assert.AreEqual(0, emptyContext.Count);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
