using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleCore.Operations;
using Systems.SimplePermissions.Abstract;
using Systems.SimplePermissions.Data;
using Systems.SimplePermissions.Data.Context;
using Systems.SimplePermissions.Operations;
using UnityEngine;

namespace Systems.SimplePermissions.Components
{
    /// <summary>
    ///     Stores explicit permission overrides for one GameObject.
    /// </summary>
    /// <remarks>
    ///     A denied override takes precedence if malformed serialized data contains both an allowed and denied entry.
    ///     Normal API operations always keep the two override lists mutually exclusive.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class PermissionStorage : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField, HideInInspector] private List<PermissionEntry> _allowedPermissions = new List<PermissionEntry>();
        [SerializeField, HideInInspector] private List<PermissionEntry> _deniedPermissions = new List<PermissionEntry>();

        [NonSerialized] private bool _areIndexesBuilt;

        /// <summary>
        ///     Determines whether the configured permission is currently available on this GameObject.
        /// </summary>
        public bool HasPermission<TPermission>()
            where TPermission : PermissionBase, new()
        {
            if (!TryGetPermission(out TPermission permission)) return false;

            EnsureStorage();
            if (ContainsPermission<TPermission>(_deniedPermissions)) return false;
            if (ContainsPermission<TPermission>(_allowedPermissions)) return true;
            return permission is IAllowedByDefault;
        }

        /// <summary>
        ///     Determines whether the configured permission has an explicit allowed override.
        /// </summary>
        public bool IsExplicitlyGranted<TPermission>()
            where TPermission : PermissionBase, new()
        {
            if (!TryGetPermission(out TPermission permission)) return false;
            EnsureStorage();
            return ContainsPermission<TPermission>(_allowedPermissions);
        }

        /// <summary>
        ///     Determines whether the configured permission has an explicit denied override.
        /// </summary>
        public bool IsExplicitlyDenied<TPermission>()
            where TPermission : PermissionBase, new()
        {
            if (!TryGetPermission(out TPermission permission)) return false;
            EnsureStorage();
            return ContainsPermission<TPermission>(_deniedPermissions);
        }

        /// <summary>
        ///     Explicitly allows a configured permission, replacing a possible denied override.
        /// </summary>
        public OperationResult TryGrant<TPermission>()
            where TPermission : PermissionBase, new()
        {
            if (!TryGetPermission(out TPermission permission)) return PermissionOperations.PermissionNotConfigured();

            EnsureStorage();
            if (ContainsPermission<TPermission>(_allowedPermissions) && !ContainsPermission<TPermission>(_deniedPermissions))
                return PermissionOperations.AlreadyGranted();

            PermissionContext context = new PermissionContext(this);
            OperationResult grantResult = permission.TryGrant(in context);
            if (!grantResult) return grantResult;

            RemovePermission<TPermission>(_allowedPermissions);
            RemovePermission<TPermission>(_deniedPermissions);
            InsertPermission(_allowedPermissions, permission);
            return grantResult;
        }

        /// <summary>
        ///     Explicitly denies a configured permission, replacing a possible allowed override.
        /// </summary>
        public OperationResult TryDeny<TPermission>()
            where TPermission : PermissionBase, new()
        {
            if (!TryGetPermission(out TPermission permission)) return PermissionOperations.PermissionNotConfigured();

            EnsureStorage();
            if (ContainsPermission<TPermission>(_deniedPermissions) && !ContainsPermission<TPermission>(_allowedPermissions))
                return PermissionOperations.AlreadyDenied();

            PermissionContext context = new PermissionContext(this);
            OperationResult denyResult = permission.TryDeny(in context);
            if (!denyResult) return denyResult;

            RemovePermission<TPermission>(_allowedPermissions);
            RemovePermission<TPermission>(_deniedPermissions);
            InsertPermission(_deniedPermissions, permission);
            return denyResult;
        }

        /// <summary>
        ///     Removes a configured permission's explicit override and restores its default behavior.
        /// </summary>
        public OperationResult TryRevoke<TPermission>()
            where TPermission : PermissionBase, new()
        {
            if (!TryGetPermission(out TPermission permission)) return PermissionOperations.PermissionNotConfigured();

            EnsureStorage();
            if (!ContainsPermission<TPermission>(_allowedPermissions) && !ContainsPermission<TPermission>(_deniedPermissions))
                return PermissionOperations.AlreadyRevoked();

            PermissionContext context = new PermissionContext(this);
            OperationResult revokeResult = permission.TryRevoke(in context);
            if (!revokeResult) return revokeResult;

            RemovePermission<TPermission>(_allowedPermissions);
            RemovePermission<TPermission>(_deniedPermissions);
            return revokeResult;
        }

        private static bool TryGetPermission<TPermission>([CanBeNull] out TPermission permission)
            where TPermission : PermissionBase, new()
        {
            permission = PermissionDatabase.GetExact<TPermission>();
            return !ReferenceEquals(permission, null) && permission;
        }

        private void EnsureStorage()
        {
            if (ReferenceEquals(_allowedPermissions, null)) _allowedPermissions = new List<PermissionEntry>();
            if (ReferenceEquals(_deniedPermissions, null)) _deniedPermissions = new List<PermissionEntry>();
            if (_areIndexesBuilt) return;

            BuildIndex(_allowedPermissions);
            BuildIndex(_deniedPermissions);
            _areIndexesBuilt = true;
        }

        private static bool ContainsPermission<TPermission>([NotNull] List<PermissionEntry> permissions)
            where TPermission : PermissionBase, new()
        {
            return FindPermissionIndex<TPermission>(permissions) >= 0;
        }

        private static void InsertPermission<TPermission>(
            [NotNull] List<PermissionEntry> permissions,
            [NotNull] TPermission permission)
            where TPermission : PermissionBase, new()
        {
            HashIdentifier hashIdentifier = HashIdentifier.New(typeof(TPermission));
            int insertionIndex = FindInsertionIndex(permissions, hashIdentifier);
            permissions.Insert(insertionIndex, new PermissionEntry(permission, hashIdentifier));
        }

        private static void RemovePermission<TPermission>([NotNull] List<PermissionEntry> permissions)
            where TPermission : PermissionBase, new()
        {
            HashIdentifier hashIdentifier = HashIdentifier.New(typeof(TPermission));
            int firstPermissionIndex = FindFirstHashIndex(permissions, hashIdentifier);
            if (firstPermissionIndex < 0) return;

            for (int permissionIndex = firstPermissionIndex;
                 permissionIndex < permissions.Count &&
                 permissions[permissionIndex].HashIdentifier.CompareTo(hashIdentifier) == 0;)
            {
                PermissionBase permission = permissions[permissionIndex].Permission;
                if (permission is TPermission)
                {
                    permissions.RemoveAt(permissionIndex);
                    continue;
                }

                permissionIndex++;
            }
        }

        private static void BuildIndex([NotNull] List<PermissionEntry> permissions)
        {
            for (int permissionIndex = permissions.Count - 1; permissionIndex >= 0; permissionIndex--)
            {
                PermissionEntry entry = permissions[permissionIndex];
                if (!entry.RefreshHashIdentifier())
                {
                    permissions.RemoveAt(permissionIndex);
                    continue;
                }

                permissions[permissionIndex] = entry;
            }

            permissions.Sort(PermissionEntryComparer.Instance);
        }

        private static int FindPermissionIndex<TPermission>([NotNull] List<PermissionEntry> permissions)
            where TPermission : PermissionBase, new()
        {
            HashIdentifier hashIdentifier = HashIdentifier.New(typeof(TPermission));
            int firstPermissionIndex = FindFirstHashIndex(permissions, hashIdentifier);
            if (firstPermissionIndex < 0) return -1;

            for (int permissionIndex = firstPermissionIndex;
                 permissionIndex < permissions.Count &&
                 permissions[permissionIndex].HashIdentifier.CompareTo(hashIdentifier) == 0;
                 permissionIndex++)
            {
                PermissionBase permission = permissions[permissionIndex].Permission;
                if (permission is TPermission) return permissionIndex;
            }

            return -1;
        }

        private static int FindInsertionIndex(
            [NotNull] List<PermissionEntry> permissions,
            in HashIdentifier hashIdentifier)
        {
            int low = 0;
            int high = permissions.Count;
            while (low < high)
            {
                int mid = low + ((high - low) >> 1);
                PermissionEntry entry = permissions[mid];
                if (entry.HashIdentifier.CompareTo(hashIdentifier) <= 0)
                    low = mid + 1;
                else
                    high = mid;
            }

            return low;
        }

        private static int FindFirstHashIndex(
            [NotNull] List<PermissionEntry> permissions,
            in HashIdentifier hashIdentifier)
        {
            int low = 0;
            int high = permissions.Count - 1;
            int foundIndex = -1;
            while (low <= high)
            {
                int mid = low + ((high - low) >> 1);
                PermissionEntry entry = permissions[mid];
                int comparison = entry.HashIdentifier.CompareTo(hashIdentifier);
                if (comparison == 0)
                {
                    foundIndex = mid;
                    high = mid - 1;
                    continue;
                }

                if (comparison < 0)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return foundIndex;
        }

        private void Awake()
        {
            EnsureStorage();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _areIndexesBuilt = false;
        }

        [Serializable]
        private struct PermissionEntry
        {
            [SerializeField] private PermissionBase _permission;
            [NonSerialized] private HashIdentifier _hashIdentifier;

            [CanBeNull] public PermissionBase Permission => _permission;
            public HashIdentifier HashIdentifier => _hashIdentifier;

            public PermissionEntry([NotNull] PermissionBase permission, in HashIdentifier hashIdentifier)
            {
                _permission = permission;
                _hashIdentifier = hashIdentifier;
            }

            public bool RefreshHashIdentifier()
            {
                if (ReferenceEquals(_permission, null) || !_permission) return false;
                _hashIdentifier = HashIdentifier.New(_permission.GetType());
                return true;
            }
        }

        private sealed class PermissionEntryComparer : IComparer<PermissionEntry>
        {
            public static readonly PermissionEntryComparer Instance = new PermissionEntryComparer();

            public int Compare(PermissionEntry left, PermissionEntry right)
            {
                return left.HashIdentifier.CompareTo(right.HashIdentifier);
            }
        }
    }
}
