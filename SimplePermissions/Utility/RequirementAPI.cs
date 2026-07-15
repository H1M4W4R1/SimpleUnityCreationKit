using JetBrains.Annotations;
using Systems.SimplePermissions.Abstract;
using Systems.SimplePermissions.Data;

namespace Systems.SimplePermissions.Utility
{
    /// <summary>
    ///     Provides cached, addressable lookups for requirement evaluation.
    /// </summary>
    public static class RequirementAPI
    {
        /// <summary>
        ///     Evaluates the configured typed requirement. Missing or destroyed assets evaluate to <c>false</c>.
        /// </summary>
        public static bool IsMet<TRequirement, TContext>([CanBeNull] TContext context = default)
            where TRequirement : RequirementBase<TContext>, new()
        {
            TRequirement requirement = RequirementDatabase.GetExact<TRequirement>();
            if (ReferenceEquals(requirement, null) || !requirement) return false;
            return requirement.IsMet(context);
        }

        /// <summary>
        ///     Evaluates the configured requirement without a compile-time context type.
        /// </summary>
        public static bool IsMetUnsafe<TRequirement>([CanBeNull] object context = null)
            where TRequirement : RequirementBase, new()
        {
            TRequirement requirement = RequirementDatabase.GetExact<TRequirement>();
            if (ReferenceEquals(requirement, null) || !requirement) return false;
            return requirement.IsMetUnsafe(context);
        }
    }
}
