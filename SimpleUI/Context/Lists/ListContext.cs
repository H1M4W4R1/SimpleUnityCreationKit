using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Context.Lists
{
    /// <summary>
    ///     Context for all lists containing data
    /// </summary>
    /// <typeparam name="TListObject">List object type</typeparam>
    public abstract class ListContext<TListObject>
    {
        /// <summary>
        ///     Data for selector
        /// </summary>
        [NotNull] public IReadOnlyList<TListObject> DataArray { get; }

        /// <summary>
        ///     List count
        /// </summary>
        public int Count => DataArray.Count;

        /// <summary>
        ///     Checks if the index is valid
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValidIndex(int index) => index >= 0 && index < DataArray.Count;

        /// <summary>
        ///     Access to data by index
        /// </summary>
        /// <param name="index">Index of item</param>
        public TListObject this[int index]
        {
            get
            {
                Assert.IsFalse(!IsValidIndex(index), "Index out of range");
                return DataArray[index];
            }
        }
        
        public ListContext([NotNull] IReadOnlyList<TListObject> data)
        {
            DataArray = data;
        }
    }
}