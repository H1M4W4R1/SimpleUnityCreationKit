using Systems.SimplePermissions.Abstract;
using UnityEngine;

namespace Systems.SimplePermissions.Examples
{
    /// <summary>
    ///     Requirement used by the example scene to gate a feature until level three.
    /// </summary>
    public sealed class ExampleMinimumLevelRequirement : RequirementBase<int>
    {
        [SerializeField] private int _minimumLevel = 3;

        public override bool IsMet(int context)
        {
            return context >= _minimumLevel;
        }
    }
}
