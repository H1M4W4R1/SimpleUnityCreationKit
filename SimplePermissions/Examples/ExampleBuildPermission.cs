using Systems.SimplePermissions.Abstract;

namespace Systems.SimplePermissions.Examples
{
    /// <summary>
    ///     Permission used by the example scene. It starts allowed until the storage denies it.
    /// </summary>
    public sealed class ExampleBuildPermission : PermissionBase, IAllowedByDefault
    {
    }
}
