using JetBrains.Annotations;
using Systems.SimplePermissions.Components;

namespace Systems.SimplePermissions.Data.Context
{
    /// <summary>
    ///     Per-operation data passed to permission lifecycle callbacks.
    /// </summary>
    public readonly ref struct PermissionContext
    {
        [NotNull] public readonly PermissionStorage storage;

        public PermissionContext([NotNull] PermissionStorage storage)
        {
            this.storage = storage;
        }
    }
}
