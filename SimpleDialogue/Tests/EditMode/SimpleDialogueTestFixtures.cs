using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Components;
using Systems.SimpleDialogue.Data;
using Systems.SimpleDialogue.Implementations;
using UnityEngine;

namespace Systems.SimpleDialogue.Tests
{
    public abstract class SimpleDialogueTestBase
    {
        [NotNull] private readonly List<Object> _createdObjects = new();

        protected Dialogue CreateDialogue(DialogueGraph graph, [CanBeNull] IDialogueRenderer renderer = null)
        {
            GameObject gameObject = new("Dialogue Test");
            _createdObjects.Add(gameObject);
            Dialogue dialogue = gameObject.AddComponent<Dialogue>();
            dialogue.InitializeForTests(graph, renderer);
            return dialogue;
        }

        protected DialogueGraph CreateGraph()
        {
            DialogueGraph graph = ScriptableObject.CreateInstance<DialogueGraph>();
            _createdObjects.Add(graph);
            return graph;
        }

        [TearDown]
        public void TearDown()
        {
            for (int objectIndex = _createdObjects.Count - 1; objectIndex >= 0; objectIndex--)
            {
                Object.DestroyImmediate(_createdObjects[objectIndex]);
            }

            _createdObjects.Clear();
        }

        protected static void AssertSimilar(OperationResult expected, OperationResult actual)
        {
            Assert.IsTrue(
                OperationResult.AreSimilar(in expected, in actual),
                $"Expected {expected.systemCode}:{expected.resultCode}, got {actual.systemCode}:{actual.resultCode}.");
        }

        protected static void Connect(DialogueInteractionNode from, string outputPortName, DialogueInteractionNode to)
        {
            from.GetOutputPort(outputPortName).Connect(to.GetInputPort(nameof(DialogueInteractionNode.input)));
        }
    }
}
