//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using NModel.Internals;


namespace NModel
{
    /// <summary>
    /// Immutable type that provides structural equality for arrays.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public sealed class ValueArray<T> : CachedHashCollectionValue<T> where T : IComparable
    {
        T[] contents;

        /// <summary>
        /// Constructs a value array with given contents
        /// </summary>
        public ValueArray(T[] contents)
        {
            this.contents = (T[])contents.Clone();
        }

        private ValueArray(T[] contents, bool copy)
        {
            this.contents = copy ? (T[])contents.Clone() : contents;
        }

        internal static IComparable ConstructValue(Sequence<IComparable> args)
        {
            T[] values = new T[args.Count];
            int i = 0;
            foreach (IComparable arg in args)
            {
                T/*?*/ val = (T)arg;
                values[i] = val;
                i += 1;
            }
            return new ValueArray<T>(values, false);
        }

        /// <summary>
        /// The contents of the value array. Note: the caller must never modfiy this array.
        /// </summary>
        public T[] Contents()
        {
          return contents; 
        }

        /// <summary>
        /// Returns the number of elements in the collection value.
        /// </summary>
        public override int Count
        {
            get { return this.contents.Length; }
        }

        /// <summary>
        /// Number of elements in the value array
        /// </summary>
        public int Length
        {
            get { return this.contents.Length; }
        }

        /// <summary>
        /// Tests whether the given element is found in the value array.
        /// </summary>
        /// <param name="item">The item to find</param>
        /// <returns>True, if the <paramref name="item"/> is in this value array, false otherwise.</returns>
        /// <remarks>
        /// Complexity: O(this.Count)
        /// </remarks>
        public override bool Contains(T item)
        {
            for (int i = 0; i < this.contents.Length; i += 1)
                if (Object.Equals(this.contents[i], item))
                    return true;
            return false;
        }

        /// <summary>
        /// Returns the i'th element
        /// </summary>
        public T this[int i]
        {
            get
            //^ requires 0 <= i && i < count;
            {
                return this.contents[i];
            }
        }

        /// <summary>
        /// Enumerate the values
        /// </summary>
        public override IEnumerator<T> GetEnumerator()
        {
            foreach (T value in this.contents)
                yield return value;
        }

        /// <summary>
        /// Convert the value array to values of type T using the given converter
        /// </summary>
        public ValueArray<S> Convert<S>(Converter<T, S> converter) where S : IComparable
        //^ ensures result.Count == this.Count;
        {
            S[] newRepresentation = new S[this.contents.Length];
            for (int i = 0; i < this.contents.Length; i += 1)
            {
                newRepresentation[i] = converter(this.contents[i]);
            }
            return new ValueArray<S>(newRepresentation, false);
        }

        /// <summary>
        /// Applies <paramref name="selector"/> to each element of this array and collects all values where 
        /// the selector function returns true.
        /// </summary>
        /// <param name="selector">A Boolean-valued delegate that acts as the inclusion test. True means
        /// include; false means exclude.</param>
        /// <returns>The array of all elements of this array that satisfy the <paramref name="selector"/>. Order of
        /// selected elements is preserved.</returns>
        public ValueArray<T> Select(Predicate<T> selector)
        {
            Sequence<T> tmp = new Sequence<T>();
            int len = this.Count;
            for (int i = 0; i < len; i++)
            {
                T val = this.contents[i];
                if (selector(val)) tmp = tmp.AddLast(val);
            }
            T[] result = new T[tmp.Count];
            tmp.CopyTo(result, 0);
            return new ValueArray<T>(result, false);
        }

        /// <summary>
        /// Select an arbitrary value from the collection, with external choice.
        /// </summary>
        /// <param name="i">An externally chosen integer in the interval [0, this.Count).</param>
        /// <returns>An element of the collection.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="i"/> is outside 
        /// the interval [0, this.Count).</exception>
        /// <remarks>As a pure function, this method will always return the same value 
        /// for each pair of arguments (<paramref name="this"/> and <paramref name="i"/>).</remarks>
        public override T Choose(int i)
        //^ requires 0 <= i && i < this.Count;
        {
            int len = this.Count;
            if (0 <= i && i < len)
            {
                return this.contents[i];
            }

            throw new ArgumentException(MessageStrings.ChooseInvalidArgument);
        }

    }
}
