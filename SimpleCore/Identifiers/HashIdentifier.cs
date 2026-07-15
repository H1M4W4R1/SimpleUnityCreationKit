using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleCore.Identifiers.Abstract;

namespace Systems.SimpleCore.Identifiers
{
    /// <summary>
    ///     Hash-based identifier
    /// </summary>
    public readonly struct HashIdentifier : INumberIdentifier<ulong>,
        IEquatable<HashIdentifier>, IComparable<HashIdentifier>
    {
        public readonly ulong value;
        
        public bool IsCreated => value != 0;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public HashIdentifier(ulong value)
        {
            this.value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static HashIdentifier New([NotNull] Type type)
        {
            return new HashIdentifier(ComputeTypeHash(type));
        }
 
        /// <summary>
        /// Computes a deterministic 64-bit hash for the given type.
        /// </summary>
        /// <remarks>
        /// The hash is derived from the fully qualified type name and simple assembly name, so it is
        /// stable across application sessions as long as those names do not change. Rename or move a
        /// persisted type only with an explicit save-data migration.
        /// </remarks>
        public static ulong ComputeTypeHash([NotNull] Type type)
        {
            string typeName = type.FullName;
            string assemblyName = type.Assembly.GetName().Name;
            ulong hash = ComputeTextHash(assemblyName);
            hash ^= 0xFFUL;
            hash *= 1099511628211UL;
            return ComputeTextHash(typeName, hash);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ComputeTextHash([NotNull] string value)
        {
            const ulong OFFSET = 14695981039346656037UL;
            return ComputeTextHash(value, OFFSET);
        }

        private static ulong ComputeTextHash([NotNull] string value, ulong hash)
        {
            const ulong PRIME = 1099511628211UL;
            for (int index = 0; index < value.Length; index++)
            {
                hash ^= value[index];
                hash *= PRIME;
            }

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Equals(HashIdentifier other)
            => value == other.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public int CompareTo(HashIdentifier other)
            => value.CompareTo(other.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override bool Equals(object obj)
            => obj is HashIdentifier other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override int GetHashCode()
            => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)] [NotNull] public override string ToString()
            => value.ToString("X");
        
        /// <inheritdoc/>
        public ulong Value => value;
    }
}
