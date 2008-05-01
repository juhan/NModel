//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace NModel.Internals
{   
    /// <summary>
    /// A helper class. See <see cref="CollectionValue{T}"/>.
    /// </summary>
    abstract public class CollectionValue : CompoundValue 
    {            
    }  


    /// <summary>
    /// Immutable collection of elements with structural equality. Value collections include
    /// sets, maps, mags (multisets) and sequences.
    /// </summary>
    /// <typeparam name="T">The type of element contained</typeparam>
    /// <remarks>   
    /// Value collections are parameterized by type T. Formally, we interpret a value collection
    /// as the pair (T, collection) where T is the type and collection is an untyped collection 
    /// (such as a set or a map). This has implications for reasoning about such types-- in particular,
    /// if <c>a = new Set&lt;int&gt;()</c> and <c>b = new Set&lt;string&gt;()</c> then <c>a != b</c>. To understand this, we can see that
    ///  
    ///               <c>(typeof(int), {}) != (typeof(string), {})</c>
    ///  </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    abstract public class CollectionValue<T> :  Internals.CollectionValue, ICollectionValue<T>
    {
        #region IValueCollection<T> Members

        /// <summary>
        /// Returns the number of elements in the collection value.
        /// </summary>
        abstract public int Count { get; }

        /// <summary>
        /// Returns true if the collection value has no elements. False otherwise.
        /// </summary>
        public bool IsEmpty
        {
            get { return (Count == 0); }
        } 

        /// <summary>
        /// Tests whether the given element is found in the collection value.
        /// </summary>
        /// <param name="item">The item to find</param>
        /// <returns>True, if the item is in this collection value, false otherwise.</returns>
        abstract public bool Contains(T item);

        /// <summary>
        /// Selects an arbitrary value from the collection using internal choice.        
        /// </summary>
        /// <returns>An element of the collection.</returns>
        /// <exception cref="System.ArgumentException">Thrown if the collection is empty.</exception>
        /// <remarks>
        /// This method may return different values whenever it is invoked. (This is not a pure function.)  
        /// The method makes changes to the state of an internal chooser. The state of the
        /// internal chooser is available via the property <see cref="HashAlgorithms.GlobalChoiceController" />.
        /// </remarks>
        /// <seealso cref="HashAlgorithms.GlobalChoiceController"/>
        /// <overloads>
        /// <summary>Selects an arbitrary value from the collection.</summary>
        /// </overloads>
        public virtual T Choose()
        //^ requires this.Count > 0;
        {
           if (this.Count < 1) throw new ArgumentException(MessageStrings.ChooseInvalidArgument);
           return this.Choose(HashAlgorithms.GlobalChoiceController.Next(0, this.Count));
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
        public virtual T Choose(int i)
        //^ requires 0 <= i && i < this.Count;
        {
            int len = this.Count;
            if (len > 0)
            {
                int j = 0;
                foreach (T val in this)
                {
                    if (j == i)
                        return val;
                    j += 1;
                }
            }

            throw new ArgumentException(MessageStrings.ChooseInvalidArgument);
        }

        /// <summary>
        /// Copies all the items in the collection into an array.
        /// </summary>
        /// <param name="array">Array to copy to.</param>
        /// <param name="arrayIndex">Starting index in <paramref name="array"/> to copy to.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            int count = this.Count;

            if (count == 0)
                return;

            if (array == null)
                throw new ArgumentNullException("array");

            //^ assume count > 0;
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, MessageStrings.ArgMustNotBeNegative);
            if (arrayIndex >= array.Length || count > array.Length - arrayIndex)
                throw new ArgumentException("arrayIndex", MessageStrings.ArrayTooSmall);

            int index = arrayIndex = 0;
            int i = 0;
            foreach (T item in (IEnumerable<T>)this)
            {
                //^ assume i < count;

                array[index] = item;
                index += 1;
                i += 1;
            }
        }

        /// <summary>
        /// Universal quantification. Returns true if the predicate is true for all elements of the collection value and 
        /// false otherwise.
        /// </summary>
        /// <param name="predicate">Boolean-valued delegate to be applied to each value</param>
        /// <returns>True, if the predicate is true for all elements of the collection value, false otherwise </returns>
        public bool Forall(Predicate<T> predicate)
        {
            foreach (T val in this)
                if (!predicate(val)) return false;
            return true;
        }

        /// <summary>
        /// Existential quantification. Returns true if the predicate is true for at least one element of the collection value.
        /// </summary>
        /// <param name="predicate">Boolean-valued delegate to be applied to each value</param>
        /// <returns>True, if the predicate is true for at least one of elements of the collection value, false otherwise</returns>
        public bool Exists(Predicate<T> predicate)
        {
            foreach (T val in this)
                if (predicate(val)) return true;
            return false;
        }

        /// <summary>
        /// Unique quantification. Returns true if the predicate is true for exactly one element 
        /// of the collection, false otherwise.
        /// </summary>
        /// <param name="predicate">Boolean-valued delegate to be applied to each value</param>
        /// <returns>True, if the predicate is true for at exactly one of elements 
        /// of the collection value, false otherwise</returns>
        public bool ExistsOne(Predicate<T> predicate)
        {
            int i = 0;
            foreach (T val in this)
                if (predicate(val))
                    i += 1;
            return (i == 1);
        }

        /// <summary>
        /// Returns the least value in the collection under the term ordering 
        /// defined by <see cref="HashAlgorithms.CompareValues" />.
        /// </summary>
        /// <returns>The minimal element</returns>
        /// <exception cref="System.ArgumentException">Thrown if the collection is empty.</exception>
        /// <seealso cref="HashAlgorithms.CompareValues" />
        public T Minimum()
        {
            bool isFirst = true;
            T minVal = default(T);

            foreach (T val in this)
            {
                if (isFirst || (HashAlgorithms.CompareValues(val, minVal) == -1))
                    minVal = val;

                isFirst = false;
            }

            if (isFirst)
                throw new ArgumentException(MessageStrings.MinimumInvalidArgument);
            else
                return minVal;
        }

        /// <summary>
        /// Returns the least value in the collection under the term ordering 
        /// defined by <see cref="HashAlgorithms.CompareValues" />.
        /// </summary>
        /// <returns>The minimal element</returns>
        /// <exception cref="System.ArgumentException">Thrown if the collection is empty.</exception>
        /// <seealso cref="HashAlgorithms.CompareValues" />
        public T Maximum()
        {
            bool isFirst = true;
            T maxVal = default(T);

            foreach (T val in this)
            {
                if (isFirst || (HashAlgorithms.CompareValues(val, maxVal) == 1))
                    maxVal = val;

                isFirst = false;
            }

            if (isFirst)
                throw new ArgumentException(MessageStrings.MaximumInvalidArgument);
            else
                return maxVal;
        }

        /// <summary>
        /// Iteratively applies a reducing function to map a collection value to a single summarized value.
        /// </summary>
        /// <typeparam name="S">The type of the result; also, the return type of the reducer function.</typeparam>
        /// <param name="r">The reducer function to be iteratively applied</param>
        /// <param name="initialValue">The value passed on the first iteration of the reducing function</param>
        /// <returns>The value returned by the last iteration of the reducing function.</returns>
        /// <seealso cref="Reducer&lt;T, S&gt;" />
        /// <example>
        /// This example uses Reduce to calculate the sum of all elements in a set of integers. 
        /// <code>
        /// void SetReduceExample()
        /// {
        ///    Set&lt;int&gt; set1 = new Set&lt;string&gt;(1, 2, 3, 4, 5);
        ///    int expected = 15;
        ///    int actual = set1.Reduce(delegate(int i, int sum) { return i + sum; }, 0);
        ///    
        ///    Assert.AreEqual(expected, actual);
        /// }
        /// </code>
        /// </example>
        public S Reduce<S>(Reducer<T, S> r, S initialValue)
        {
            S result = initialValue;
            foreach (T val in this)
                result = r(val, result);
            return result;
        }

        #endregion

        #region CompoundValue Members

        /// <summary>
        /// Returns an enumeration of the field values of collection
        /// </summary>
        /// <returns>An enumeration of the field values of this compound value</returns>
        /// <remarks>
        /// This is a pure function and has the further requirement that
        /// its value must always be the same regardless of the context in which it is invoked. 
        /// In other words, no state update may change the values returned by this
        /// enumeration.
        /// 
        /// This is provided to make equality testing and hashing efficient. The values returned
        /// may be encodings of the internal data structures used to implement the collection
        /// value; other accessors such as <see cref="GetEnumerator" /> provide enumeration
        /// capabilities for general use.
        /// </remarks>
        override public IEnumerable<IComparable> FieldValues()
        {
            foreach(IComparable obj in this)
                yield return obj;
        }
        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Enumerates each value of the collection.
        /// </summary>
        /// <returns>An enumerator of values in the collection. </returns>
        abstract public IEnumerator<T> GetEnumerator();

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Enumerates each value of the collection.
        /// </summary>
        /// <returns>An enumerator of values in the collection. </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Term order. Comparision is based on type and recursively on fields.
        /// </summary>
        /// <param name="obj">The object to be compared with this collection</param>
        /// <returns>-1 if less than, 0 if equal, 1 if greater than</returns>
        /// <remarks>Collections are ordered by size. If equal size, then they are 
        /// ordered by pairwise comparison.</remarks>
        public override int CompareTo(object obj)
        {
            // Collections are ordered by size. If equal size, then they are 
            // ordered by pairwise comparison.
            CollectionValue<T> other = obj as CollectionValue<T>;
            if (null != other)
            {
                int flag = this.Count.CompareTo(other.Count);
                return (flag != 0) ? flag : base.CompareTo(obj);
            }
            return base.CompareTo(obj);
        }

        /// <summary>
        /// Represents this collection as a .NET array
        /// </summary>
        public T[] AsArray()
        {

            T[] result = new T[this.Count];
            this.CopyTo(result, 0);
            return result;
        }
    }
}

