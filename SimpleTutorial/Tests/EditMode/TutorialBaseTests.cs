using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleTutorial.Abstract;
using Systems.SimpleTutorial.Components;
using Systems.SimpleTutorial.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleTutorial.Tests
{
    public sealed class TutorialBaseTests
    {
        private readonly List<TutorialStep> _createdSteps = new List<TutorialStep>();
        private GameObject _tutorialObject;
        private TestTutorial _tutorial;

        [SetUp]
        public void SetUp()
        {
            _tutorialObject = new GameObject("Tutorial Test");
            _tutorial = _tutorialObject.AddComponent<TestTutorial>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_tutorialObject) Object.DestroyImmediate(_tutorialObject);

            int stepCount = _createdSteps.Count;
            for (int stepIndex = 0; stepIndex < stepCount; stepIndex++)
            {
                TutorialStep tutorialStep = _createdSteps[stepIndex];
                if (tutorialStep) Object.DestroyImmediate(tutorialStep);
            }
        }

        [Test]
        public void StartTutorial_ActivatesFirstVisibleStep()
        {
            TestStep firstStep = CreateStep();
            TestStep secondStep = CreateStep();
            _tutorial.Configure(firstStep, secondStep);

            _tutorial.StartTutorial();

            Assert.AreEqual(firstStep, _tutorial.ActiveStep);
            Assert.AreEqual(0, _tutorial.ActiveStepIndex);
            Assert.IsTrue(_tutorial.IsRunning);
            Assert.AreEqual(1, firstStep.StartedCount);
            Assert.AreEqual(1, _tutorial.StartedStepIndices.Count);
            Assert.AreEqual(0, _tutorial.StartedStepIndices[0]);
        }

        [Test]
        public void TickTutorial_WhenStepCompletes_AdvancesAndCompletesInOrder()
        {
            TestStep firstStep = CreateStep();
            TestStep secondStep = CreateStep();
            _tutorial.Configure(firstStep, secondStep);
            _tutorial.StartTutorial();

            firstStep.IsReady = true;
            _tutorial.Tick();

            Assert.AreEqual(secondStep, _tutorial.ActiveStep);
            Assert.AreEqual(1, firstStep.CompletedCount);
            Assert.AreEqual(1, _tutorial.CompletedStepIndices.Count);
            Assert.AreEqual(0, _tutorial.CompletedStepIndices[0]);
            Assert.AreEqual(1, secondStep.StartedCount);

            secondStep.IsReady = true;
            _tutorial.Tick();

            Assert.IsFalse(_tutorial.IsRunning);
            Assert.IsTrue(_tutorial.IsComplete);
            Assert.IsNull(_tutorial.ActiveStep);
            Assert.AreEqual(1, secondStep.CompletedCount);
            Assert.AreEqual(1, _tutorial.CompletedTutorialCount);
        }

        [Test]
        public void StartTutorial_WhenStepCannotShow_SkipsItForCurrentRun()
        {
            TestStep hiddenStep = CreateStep();
            hiddenStep.CanBeShown = false;
            TestStep visibleStep = CreateStep();
            _tutorial.Configure(hiddenStep, visibleStep);

            _tutorial.StartTutorial();

            Assert.AreEqual(visibleStep, _tutorial.ActiveStep);
            Assert.AreEqual(1, _tutorial.ActiveStepIndex);
            Assert.AreEqual(0, hiddenStep.StartedCount);
            Assert.AreEqual(1, visibleStep.StartedCount);
        }

        [Test]
        public void RestartTutorial_ReevaluatesSkippedSteps()
        {
            TestStep step = CreateStep();
            step.CanBeShown = false;
            _tutorial.Configure(step);
            _tutorial.StartTutorial();

            Assert.IsTrue(_tutorial.IsComplete);

            step.CanBeShown = true;
            _tutorial.RestartTutorial();

            Assert.IsTrue(_tutorial.IsRunning);
            Assert.AreEqual(step, _tutorial.ActiveStep);
            Assert.AreEqual(1, step.StartedCount);
        }

        [Test]
        public void TickTutorial_WhenStepDependsOnAnotherStep_UsesCurrentRunCompletionState()
        {
            PrerequisiteStep prerequisiteStep = CreatePrerequisiteStep();
            DependentStep dependentStep = CreateDependentStep();
            _tutorial.Configure(prerequisiteStep, dependentStep);
            _tutorial.StartTutorial();

            prerequisiteStep.IsReady = true;
            _tutorial.Tick();

            Assert.AreEqual(dependentStep, _tutorial.ActiveStep);
            Assert.IsFalse(dependentStep.LastPrerequisiteCompletionState);

            _tutorial.Tick();

            Assert.IsTrue(dependentStep.LastPrerequisiteCompletionState);
            Assert.IsTrue(_tutorial.IsComplete);
        }

        private TestStep CreateStep()
        {
            TestStep tutorialStep = ScriptableObject.CreateInstance<TestStep>();
            _createdSteps.Add(tutorialStep);
            return tutorialStep;
        }

        private PrerequisiteStep CreatePrerequisiteStep()
        {
            PrerequisiteStep tutorialStep = ScriptableObject.CreateInstance<PrerequisiteStep>();
            _createdSteps.Add(tutorialStep);
            return tutorialStep;
        }

        private DependentStep CreateDependentStep()
        {
            DependentStep tutorialStep = ScriptableObject.CreateInstance<DependentStep>();
            _createdSteps.Add(tutorialStep);
            return tutorialStep;
        }

        private sealed class TestTutorial : TutorialBase
        {
            public readonly List<int> StartedStepIndices = new List<int>();
            public readonly List<int> CompletedStepIndices = new List<int>();

            public int CompletedTutorialCount { get; private set; }

            public void Configure(params TutorialStep[] tutorialSteps)
            {
                SetSteps(tutorialSteps);
            }

            public void Tick()
            {
                TickTutorial();
            }

            protected override void OnTutorialStepStarted(TutorialStep tutorialStep, int stepIndex)
            {
                StartedStepIndices.Add(stepIndex);
            }

            protected override void OnTutorialStepCompleted(TutorialStep tutorialStep, int stepIndex)
            {
                CompletedStepIndices.Add(stepIndex);
            }

            protected override void OnTutorialCompleted()
            {
                CompletedTutorialCount++;
            }
        }

        private sealed class TestStep : TutorialStep
        {
            public bool CanBeShown { get; set; } = true;
            public bool IsReady { get; set; }
            public int StartedCount { get; private set; }
            public int CompletedCount { get; private set; }

            protected override bool CanShow(in TutorialStepContext context)
            {
                return CanBeShown;
            }

            protected override bool IsComplete(in TutorialStepContext context)
            {
                return IsReady;
            }

            protected override void OnTutorialStepStarted(in TutorialStepContext context)
            {
                StartedCount++;
            }

            protected override void OnTutorialStepCompleted(in TutorialStepContext context)
            {
                CompletedCount++;
            }
        }

        private sealed class PrerequisiteStep : TutorialStep
        {
            public bool IsReady { get; set; }

            protected override bool IsComplete(in TutorialStepContext context)
            {
                return IsReady;
            }
        }

        private sealed class DependentStep : TutorialStep
        {
            public bool LastPrerequisiteCompletionState { get; private set; }

            protected override bool IsComplete(in TutorialStepContext context)
            {
                LastPrerequisiteCompletionState = IsStepComplete<PrerequisiteStep>(in context);
                return LastPrerequisiteCompletionState;
            }
        }
    }
}
