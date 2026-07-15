using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using Systems.SimplePermissions.Data;
using Systems.SimplePermissions.Data.Context;
using Systems.SimplePermissions.Operations;
using UnityEngine;

namespace Systems.SimplePermissions.Abstract
{
    /// <summary>
    ///     Addressable configuration asset that defines one permission.
    /// </summary>
    /// <remarks>
    ///     Concrete permissions are generated under <c>Assets/Generated/Permissions/</c> and loaded through
    ///     <see cref="PermissionDatabase"/>. Store per-owner state on <see cref="Components.PermissionStorage"/>,
    ///     not on this shared asset.
    /// </remarks>
    [AutoCreate("Permissions", PermissionDatabase.LABEL)]
    public abstract class PermissionBase : ScriptableObject
    {
        internal OperationResult TryGrant(in PermissionContext context)
        {
            OperationResult canGrantResult = CanBeGranted(in context);
            if (!canGrantResult)
            {
                OnGrantFailed(in context, in canGrantResult);
                return canGrantResult;
            }

            OperationResult grantedResult = PermissionOperations.Granted();
            OnGranted(in context, in grantedResult);
            return grantedResult;
        }

        internal OperationResult TryDeny(in PermissionContext context)
        {
            OperationResult canDenyResult = CanBeDenied(in context);
            if (!canDenyResult)
            {
                OnDenyFailed(in context, in canDenyResult);
                return canDenyResult;
            }

            OperationResult deniedResult = PermissionOperations.Denied();
            OnDenied(in context, in deniedResult);
            return deniedResult;
        }

        internal OperationResult TryRevoke(in PermissionContext context)
        {
            OperationResult canRevokeResult = CanBeRevoked(in context);
            if (!canRevokeResult)
            {
                OnRevokeFailed(in context, in canRevokeResult);
                return canRevokeResult;
            }

            OperationResult revokedResult = PermissionOperations.Revoked();
            OnRevoked(in context, in revokedResult);
            return revokedResult;
        }

        /// <summary>
        ///     Determines whether this permission can be explicitly granted for the context owner.
        /// </summary>
        protected virtual OperationResult CanBeGranted(in PermissionContext context) => PermissionOperations.Permitted();

        /// <summary>
        ///     Called after this permission becomes explicitly granted.
        /// </summary>
        protected virtual void OnGranted(in PermissionContext context, in OperationResult result)
        {
        }

        /// <summary>
        ///     Called when an explicit grant is rejected.
        /// </summary>
        protected virtual void OnGrantFailed(in PermissionContext context, in OperationResult result)
        {
        }

        /// <summary>
        ///     Determines whether this permission can be explicitly denied for the context owner.
        /// </summary>
        protected virtual OperationResult CanBeDenied(in PermissionContext context) => PermissionOperations.Permitted();

        /// <summary>
        ///     Called after this permission becomes explicitly denied.
        /// </summary>
        protected virtual void OnDenied(in PermissionContext context, in OperationResult result)
        {
        }

        /// <summary>
        ///     Called when an explicit denial is rejected.
        /// </summary>
        protected virtual void OnDenyFailed(in PermissionContext context, in OperationResult result)
        {
        }

        /// <summary>
        ///     Determines whether this permission's explicit override can be removed for the context owner.
        /// </summary>
        protected virtual OperationResult CanBeRevoked(in PermissionContext context) => PermissionOperations.Permitted();

        /// <summary>
        ///     Called after this permission's explicit override is removed.
        /// </summary>
        protected virtual void OnRevoked(in PermissionContext context, in OperationResult result)
        {
        }

        /// <summary>
        ///     Called when revoking this permission's explicit override is rejected.
        /// </summary>
        protected virtual void OnRevokeFailed(in PermissionContext context, in OperationResult result)
        {
        }
    }
}
