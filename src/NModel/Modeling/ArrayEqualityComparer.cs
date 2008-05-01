//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NModel.Internals
{
    /// <summary>
    /// Compares array elements for order and equality.
    /// </summary>
    /// <typeparam name="T">Type of elements contained by arrays to be compared.</typeparam>
    internal class ArrayEqualityComparer<T> : EqualityComparer<T[]>
    {
        /// <summary>
        /// Stores the comparer for individual array elements.
        /// </summary>
        private static EqualityComparer<T> elementComparer = /*^(!)^*/EqualityComparer<T>.Default;

        /// <summary>
        /// Stores the default comparer in the same pattern as EqualityComparer.
        /// </summary>
        private static ArrayEqualityComparer<T> defaultComparer = new ArrayEqualityComparer<T>();

        /// <summary>
        /// Returns a default equality comparer for arrays with the element type specified by the generic argument. 
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        new public static ArrayEqualityComparer<T> Default
        {
            get
            {
                return defaultComparer;
            }
        }

        /// <summary>
        /// Instances of this class are only available through the Default property, as per the pattern of EqualityComparer.
        /// </summary>
        protected ArrayEqualityComparer()
        {
        }

        /// <summary>
        /// Determines whether the specified arrays have equal elements in the same order. 
        /// </summary>
        /// <param name="x">The first array of type <typeparamref name="T"/>[] to compare.</param>
        /// <param name="y">The second array of type <typeparamref name="T"/>[] to compare.</param>
        /// <returns><c>true</c> if the specified arrays have equal elements in the same order; otherwise, <c>false</c>.</returns>
        public override bool Equals(T[] x, T[] y)
        {
            if (x == y)
                return true;

            if (x == null || y == null)
                return false;

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
                if (!elementComparer.Equals(x[i], y[i]))
                    return false;

            return true;
        }

        /// <summary>
        /// Returns a hash code for the specified array. 
        /// </summary>
        /// <param name="obj">The array for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified array.</returns>
        public override int GetHashCode(T[] obj)
        {
            return TypedHash<ArrayEqualityComparer<T>>.ComputeEnumeratedHash(obj == null ? null : obj.GetEnumerator());
        }
    }
}
