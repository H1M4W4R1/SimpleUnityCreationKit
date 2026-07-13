using NUnit.Framework;
using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Timing;
using UnityEngine;

namespace Systems.SimpleCore.Tests
{
    public sealed class AutomationAndTickTests
    {
        private TickSystem.TickHandler _registeredHandler;

        [TearDown]
        public void TearDown()
        {
            if (!ReferenceEquals(_registeredHandler, null))
            {
                TickSystem.UnregisterHandler(_registeredHandler);
                _registeredHandler = null;
            }

            TickSystem[] tickSystems = Object.FindObjectsByType<TickSystem>(
                FindObjectsInactive.Include);
            for (int tickSystemIndex = 0; tickSystemIndex < tickSystems.Length; tickSystemIndex++)
            {
                TickSystem tickSystem = tickSystems[tickSystemIndex];
                if (ReferenceEquals(tickSystem, null)) continue;
                Object.DestroyImmediate(tickSystem.gameObject);
            }
        }

        [Test]
        public void AutoAddressableObjectAttribute_StoresPathAndLabel()
        {
            AutoAddressableObjectAttribute attribute = new AutoAddressableObjectAttribute("Simple/Test", "Simple.Label");

            Assert.AreEqual("Simple/Test", attribute.Path);
            Assert.AreEqual("Simple.Label", attribute.Label);
        }

        [Test]
        public void AutoCreateAttribute_IsAddressableAttribute()
        {
            AutoCreateAttribute attribute = new AutoCreateAttribute("Generated/Test", null);

            Assert.IsInstanceOf<AutoAddressableObjectAttribute>(attribute);
            Assert.AreEqual("Generated/Test", attribute.Path);
            Assert.IsNull(attribute.Label);
        }

        [Test]
        public void TickSystem_RegisterHandlerCreatesSingletonAndStoresHandler()
        {
            TickSystem.TickHandler handler = TickProbe;
            _registeredHandler = handler;

            TickSystem.RegisterHandler(handler);

            TickSystem instance = Object.FindAnyObjectByType<TickSystem>(FindObjectsInactive.Include);

            Assert.IsFalse(ReferenceEquals(instance, null));
            Assert.IsTrue(instance.gameObject.activeSelf);
            Assert.IsTrue(instance.enabled);
        }

        [Test]
        public void TickSystem_UnregisterHandlerAllowsPreviouslyRegisteredHandlerToBeRemoved()
        {
            TickSystem.TickHandler handler = TickProbe;

            TickSystem.RegisterHandler(handler);
            TickSystem.UnregisterHandler(handler);

            TickSystem instance = Object.FindAnyObjectByType<TickSystem>(FindObjectsInactive.Include);

            Assert.IsFalse(ReferenceEquals(instance, null));
        }

        private static void TickProbe(float deltaTimeSeconds)
        {
        }
    }
}
