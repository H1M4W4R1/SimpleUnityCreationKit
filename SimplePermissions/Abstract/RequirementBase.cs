using Systems.SimpleCore.Automation.Attributes;
using Systems.SimplePermissions.Data;
using UnityEngine;

namespace Systems.SimplePermissions.Abstract
{
    /// <summary>
    ///     Addressable configuration asset that evaluates whether an operation requirement is met.
    /// </summary>
    [AutoCreate("Requirements", RequirementDatabase.LABEL)]
    public abstract class RequirementBase : ScriptableObject
    {
        /// <summary>
        ///     Evaluates an untyped context. Prefer <see cref="RequirementBase{TContext}"/> for new requirements.
        /// </summary>
        public abstract bool IsMetUnsafe(object context);
    }

    /// <summary>
    ///     Strongly typed requirement configuration asset.
    /// </summary>
    public abstract class RequirementBase<TContext> : RequirementBase
    {
        /// <summary>
        ///     Evaluates whether this requirement is met for <paramref name="context"/>.
        /// </summary>
        public abstract bool IsMet(TContext context);

        /// <inheritdoc />
        public sealed override bool IsMetUnsafe(object context)
        {
            if (context is TContext safeContext) return IsMet(safeContext);
            return false;
        }
    }
}
