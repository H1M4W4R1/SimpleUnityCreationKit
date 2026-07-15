using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleRelations.Data;
using UnityEngine;

namespace Systems.SimpleRelations.Abstract
{
    /// <summary>
    ///     Configuration for one independently tracked kind of relationship, such as trust,
    ///     affinity, fear, friendship, rivalry, or hostility.
    /// </summary>
    /// <remarks>
    ///     Each concrete relation type is generated as an addressable asset. The asset supplies only
    ///     the value used when a relation is first created; interpretation of the resulting value is
    ///     deliberately left to game logic or a progression system.
    /// </remarks>
    [AutoCreate("Relations", RelationTypeDatabase.LABEL)]
    public abstract class RelationTypeBase : ScriptableObject
    {
        /// <summary>Value assigned to a newly created relation of this type.</summary>
        protected internal virtual int InitialValue => 0;
    }
}
