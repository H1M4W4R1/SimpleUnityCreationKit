using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleRelations.Abstract;
using UnityEngine;

namespace Systems.SimpleFactions.Data
{
    /// <summary>Registry used to resolve persistent faction relation targets after loading a save.</summary>
    public static class FactionRuntimeObjectRegistry
    {
        [NotNull] private static readonly Dictionary<Snowflake128, IRelatable> RuntimeTargets =
            new Dictionary<Snowflake128, IRelatable>();

        /// <summary>Registers a live runtime target whose identifier can be restored from a faction save.</summary>
        public static bool Register<TTarget>([NotNull] TTarget target)
            where TTarget : Object, IRelatable, IIdentifiable<Snowflake128>
        {
            if (ReferenceEquals(target, null) || !target) return false;

            Snowflake128 identifier = target.Identifier;
            if (!identifier.IsCreated) return false;

            RuntimeTargets[identifier] = target;
            return true;
        }

        /// <summary>Removes a previously registered runtime target.</summary>
        public static void Unregister<TTarget>([CanBeNull] TTarget target)
            where TTarget : Object, IRelatable, IIdentifiable<Snowflake128>
        {
            if (ReferenceEquals(target, null)) return;

            Snowflake128 identifier = target.Identifier;
            if (!identifier.IsCreated) return;
            if (!RuntimeTargets.TryGetValue(identifier, out IRelatable registered)) return;
            if (!ReferenceEquals(registered, target)) return;

            RuntimeTargets.Remove(identifier);
        }

        /// <summary>Attempts to resolve a registered runtime target by its stable identifier.</summary>
        public static bool TryGet(Snowflake128 identifier, [CanBeNull] out IRelatable target)
        {
            target = null;
            if (!identifier.IsCreated) return false;
            if (!RuntimeTargets.TryGetValue(identifier, out IRelatable registered)) return false;
            Object registeredObject = registered as Object;
            if (ReferenceEquals(registeredObject, null) || !registeredObject)
            {
                RuntimeTargets.Remove(identifier);
                return false;
            }

            target = registered;
            return true;
        }

        internal static void ClearForTests()
        {
            RuntimeTargets.Clear();
        }
    }
}
