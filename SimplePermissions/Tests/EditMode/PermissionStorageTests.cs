using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimplePermissions.Abstract;
using Systems.SimplePermissions.Components;
using Systems.SimplePermissions.Data;
using Systems.SimplePermissions.Data.Context;
using Systems.SimplePermissions.Operations;
using Systems.SimplePermissions.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimplePermissions.Tests
{
    public sealed class PermissionStorageTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();
        private GameObject _storageObject;
        private PermissionStorage _storage;

        [SetUp]
        public void SetUp()
        {
            PermissionDatabase.ClearForTests();
            RequirementDatabase.ClearForTests();
            TrackingPermission.ResetTrackingState();

            _storageObject = new GameObject("Permission Storage Test");
            _storage = _storageObject.AddComponent<PermissionStorage>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_storageObject) Object.DestroyImmediate(_storageObject);

            int objectCount = _createdObjects.Count;
            for (int objectIndex = 0; objectIndex < objectCount; objectIndex++)
            {
                Object createdObject = _createdObjects[objectIndex];
                if (ReferenceEquals(createdObject, null) || !createdObject) continue;
                Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
            PermissionDatabase.ClearForTests();
            RequirementDatabase.ClearForTests();
        }

        [Test]
        public void HasPermission_UsesConfiguredDefaultUntilAnOverrideIsApplied()
        {
            RegisterPermission<DefaultAllowedPermission>();

            Assert.IsTrue(_storage.HasPermission<DefaultAllowedPermission>());
            Assert.IsFalse(_storage.IsExplicitlyGranted<DefaultAllowedPermission>());
            Assert.IsFalse(_storage.IsExplicitlyDenied<DefaultAllowedPermission>());

            OperationResult denyResult = _storage.TryDeny<DefaultAllowedPermission>();

            Assert.IsTrue(denyResult);
            Assert.IsFalse(_storage.HasPermission<DefaultAllowedPermission>());
            Assert.IsTrue(_storage.IsExplicitlyDenied<DefaultAllowedPermission>());

            OperationResult revokeResult = _storage.TryRevoke<DefaultAllowedPermission>();

            Assert.IsTrue(revokeResult);
            Assert.IsTrue(_storage.HasPermission<DefaultAllowedPermission>());
            Assert.IsFalse(_storage.IsExplicitlyDenied<DefaultAllowedPermission>());
        }

        [Test]
        public void TryGrantAndTryDeny_ReplaceThePreviousOverride()
        {
            RegisterPermission<DefaultDeniedPermission>();

            OperationResult grantResult = _storage.TryGrant<DefaultDeniedPermission>();

            Assert.IsTrue(grantResult);
            Assert.IsTrue(_storage.HasPermission<DefaultDeniedPermission>());
            Assert.IsTrue(_storage.IsExplicitlyGranted<DefaultDeniedPermission>());

            OperationResult denyResult = _storage.TryDeny<DefaultDeniedPermission>();

            Assert.IsTrue(denyResult);
            Assert.IsFalse(_storage.HasPermission<DefaultDeniedPermission>());
            Assert.IsFalse(_storage.IsExplicitlyGranted<DefaultDeniedPermission>());
            Assert.IsTrue(_storage.IsExplicitlyDenied<DefaultDeniedPermission>());
        }

        [Test]
        public void TryGrant_WhenPermissionRejectsTheOperation_PreservesStateAndInvokesFailureCallback()
        {
            TrackingPermission trackingPermission = RegisterPermission<TrackingPermission>();
            trackingPermission.AllowGrant = false;

            OperationResult grantResult = _storage.TryGrant<TrackingPermission>();

            Assert.IsFalse(grantResult);
            Assert.IsFalse(_storage.HasPermission<TrackingPermission>());
            Assert.IsFalse(_storage.IsExplicitlyGranted<TrackingPermission>());
            Assert.AreEqual(0, TrackingPermission.GrantedCount);
            Assert.AreEqual(1, TrackingPermission.GrantFailedCount);
            Assert.AreSame(_storage, TrackingPermission.LastStorage);
        }

        [Test]
        public void PermissionLifecycle_WhenOverridesChange_InvokesTheMatchingSuccessCallbacks()
        {
            RegisterPermission<TrackingPermission>();

            OperationResult grantResult = _storage.TryGrant<TrackingPermission>();
            OperationResult denyResult = _storage.TryDeny<TrackingPermission>();
            OperationResult revokeResult = _storage.TryRevoke<TrackingPermission>();

            Assert.IsTrue(grantResult);
            Assert.IsTrue(denyResult);
            Assert.IsTrue(revokeResult);
            Assert.AreEqual(1, TrackingPermission.GrantedCount);
            Assert.AreEqual(1, TrackingPermission.DeniedCount);
            Assert.AreEqual(1, TrackingPermission.RevokedCount);
            Assert.AreSame(_storage, TrackingPermission.LastStorage);
        }

        [Test]
        public void TryGrant_WhenDefinitionIsMissing_ReturnsConfigurationError()
        {
            OperationResult grantResult = _storage.TryGrant<DefaultDeniedPermission>();

            Assert.IsFalse(grantResult);
            Assert.IsTrue(OperationResult.AreSimilar(
                grantResult, PermissionOperations.PermissionNotConfigured()));
            Assert.IsFalse(_storage.HasPermission<DefaultDeniedPermission>());
        }

        [Test]
        public void RequirementAPI_EvaluatesConfiguredTypedAndUntypedRequirements()
        {
            MinimumLevelRequirement minimumLevelRequirement = RegisterRequirement<MinimumLevelRequirement>();
            minimumLevelRequirement.MinimumLevel = 5;

            Assert.IsTrue(RequirementAPI.IsMet<MinimumLevelRequirement, int>(5));
            Assert.IsTrue(RequirementAPI.IsMetUnsafe<MinimumLevelRequirement>(6));
            Assert.IsFalse(RequirementAPI.IsMet<MinimumLevelRequirement, int>(4));
            Assert.IsFalse(RequirementAPI.IsMetUnsafe<MinimumLevelRequirement>("6"));
        }

        [Test]
        public void RequirementAPI_WhenDefinitionIsMissing_EvaluatesToFalse()
        {
            Assert.IsFalse(RequirementAPI.IsMet<MinimumLevelRequirement, int>(5));
            Assert.IsFalse(RequirementAPI.IsMetUnsafe<MinimumLevelRequirement>(5));
        }

        private TPermission RegisterPermission<TPermission>()
            where TPermission : PermissionBase
        {
            TPermission permission = ScriptableObject.CreateInstance<TPermission>();
            _createdObjects.Add(permission);
            PermissionDatabase.RegisterForTests(permission);
            return permission;
        }

        private TRequirement RegisterRequirement<TRequirement>()
            where TRequirement : RequirementBase
        {
            TRequirement requirement = ScriptableObject.CreateInstance<TRequirement>();
            _createdObjects.Add(requirement);
            RequirementDatabase.RegisterForTests(requirement);
            return requirement;
        }

        public sealed class DefaultAllowedPermission : PermissionBase, IAllowedByDefault
        {
        }

        public sealed class DefaultDeniedPermission : PermissionBase
        {
        }

        public sealed class TrackingPermission : PermissionBase
        {
            public static int GrantedCount { get; private set; }
            public static int DeniedCount { get; private set; }
            public static int GrantFailedCount { get; private set; }
            public static int RevokedCount { get; private set; }
            public static PermissionStorage LastStorage { get; private set; }

            public bool AllowGrant { get; set; } = true;

            public static void ResetTrackingState()
            {
                GrantedCount = 0;
                DeniedCount = 0;
                GrantFailedCount = 0;
                RevokedCount = 0;
                LastStorage = null;
            }

            protected override OperationResult CanBeGranted(in PermissionContext context)
            {
                return AllowGrant ? PermissionOperations.Permitted() : PermissionOperations.PermissionRejected();
            }

            protected override void OnGranted(in PermissionContext context, in OperationResult result)
            {
                GrantedCount++;
                LastStorage = context.storage;
            }

            protected override void OnGrantFailed(in PermissionContext context, in OperationResult result)
            {
                GrantFailedCount++;
                LastStorage = context.storage;
            }

            protected override void OnDenied(in PermissionContext context, in OperationResult result)
            {
                DeniedCount++;
                LastStorage = context.storage;
            }

            protected override void OnRevoked(in PermissionContext context, in OperationResult result)
            {
                RevokedCount++;
                LastStorage = context.storage;
            }
        }

        public sealed class MinimumLevelRequirement : RequirementBase<int>
        {
            public int MinimumLevel { get; set; }

            public override bool IsMet(int context)
            {
                return context >= MinimumLevel;
            }
        }
    }
}
