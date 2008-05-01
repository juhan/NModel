//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace NModel
{
    /// <summary>
    /// Delegate type for functions that summarize a value of type T in terms another type S. Used 
    /// as an argument to <see cref="Internals.ICollectionValue{T}.Reduce{S}" />.
    /// </summary>
    /// <typeparam name="T">Type of the object being summarized</typeparam>
    /// <typeparam name="S">Type of the reduction</typeparam>
    /// <param name="obj">The value being summarized</param>
    /// <param name="sum">The initial value of the reduction</param>
    /// <returns>The reduction calculated from the initial value <paramref name="sum"/>
    /// and the value being summarized, <paramref name="obj"/>.</returns>
    /// <seealso cref="Internals.ICollectionValue{T}.Reduce{S}" />
    public delegate S Reducer<T, S>(T obj, S sum);

    namespace Internals
    {
        /// <summary>
        /// Collection of elements with no update operations provided. Similiar to ICollection,
        /// but without the update operations.
        /// </summary>
        /// <typeparam name="T">The type of the element contained</typeparam>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        public interface ICollectionValue<T> : IEnumerable<T>
        {
            /// <summary>
            /// Returns the number of elements in the collection value.
            /// </summary>
            int Count { get; }

            /// <summary>
            /// Returns true if the collection value has no elements. False otherwise.
            /// </summary>
            bool IsEmpty { get; }

            /// <summary>
            /// Tests whether the given element is found in the collection value.
            /// </summary>
            /// <param name="item">The item to find</param>
            /// <returns>True, if the <paramref name="item"/> is in this collection value, 
            /// false otherwise.</returns>
            bool Contains(T item);

            /// <summary>
            /// Selects an arbitrary value from the collection using internal choice.        
            /// </summary>
            /// <returns>An element of the collection.</returns>
            /// <exception cref="System.ArgumentException">Thrown if the collection is empty.</exception>
            /// <remarks>
            /// This method may return different values whenever it is invoked. (This is not 
            /// a pure function.)  
            /// The method makes changes to the state of an internal chooser. The state of the
            /// internal chooser is available via the property <see cref="HashAlgorithms.GlobalChoiceController" />.
            /// </remarks>
            /// <seealso cref="HashAlgorithms.GlobalChoiceController"/>
            /// <overloads>
            /// <summary>Selects an arbitrary value from the collection.</summary>
            /// </overloads>
            T Choose();

            /// <summary>
            /// Select an arbitrary value from the collection, with external choice.
            /// </summary>
            /// <param name="i">An arbitrary integer in the interval [0, this.Count).</param>
            /// <returns>An element of the collection.</returns>
            /// <exception cref="System.ArgumentException">Thrown if <paramref name="i"/> is outside 
            /// the interval [0, this.Count).</exception>
            T Choose(int i);

            /// <summary>
            /// Copies all the items in the collection into an array.
            /// </summary>
            /// <param name="array">Array to copy to.</param>
            /// <param name="arrayIndex">Starting index in <paramref name="array"/> to copy to.</param>
            void CopyTo(T[] array, int arrayIndex);

            /// <summary>
            /// Universal quantification. Returns true if the predicate is true for all elements of the 
            /// collection value and 
            /// false otherwise.
            /// </summary>
            /// <param name="predicate">Boolean-valued delegate to be applied to each value</param>
            /// <returns>True, if the predicate is true for all elements of the collection value, 
            /// false otherwise </returns>
            bool Forall(Predicate<T> predicate);

            /// <summary>
            /// Existential quantification. Returns true if the predicate is true for at least one 
            /// element of the collection value.
            /// </summary>
            /// <param name="predicate">Boolean-valued delegate to be applied to each value</param>
            /// <returns>True, if the predicate is true for at least one of elements of the collection 
            /// value, false otherwise</returns>
            bool Exists(Predicate<T> predicate);

            /// <summary>
            /// Unique quantification. Returns true if the predicate is true for exactly one element 
            /// of the collection, false otherwise.
            /// </summary>
            /// <param name="predicate">Boolean-valued delegate to be applied to each value</param>
            /// <returns>True, if the predicate is true for at exactly one of elements 
            /// of the collection value, false otherwise</returns>
            bool ExistsOne(Predicate<T> predicate);

            /// <summary>
            /// Returns the least value in the collection under the term ordering 
            /// defined by <see cref="HashAlgorithms.CompareValues" />.
            /// </summary>
            /// <returns>The minimal element</returns>
            /// <exception cref="System.ArgumentException">Thrown if the collection is empty.</exception>
            /// <seealso cref="HashAlgorithms.CompareValues" />
            T Minimum();

            /// <summary>
            /// Returns the least value in the collection under the term ordering 
            /// defined by <see cref="HashAlgorithms.CompareValues" />.
            /// </summary>
            /// <returns>The minimal element</returns>
            /// <exception cref="System.ArgumentException">Thrown if the collection is empty.</exception>
            /// <seealso cref="HashAlgorithms.CompareValues" />
            T Maximum();

            /// <summary>
            /// Iteratively applies a reducing function to map a collection value to a single summarized value.
            /// </summary>
            /// <typeparam name="S">The type of the result; also, the return type of the reducer function.</typeparam>
            /// <param name="r">The reducer function to be iteratively applied</param>
            /// <param name="initialValue">The value passed on the first iteration of the reducing function</param>
            /// <returns>The value returned by the last iteration of the reducing function.</returns>
            /// <seealso cref="Reducer&lt;T, S&gt;" />
            S Reduce<S>(Reducer<T, S> r, S initialValue);

            /// <summary>
            /// Returns this collection as a .NET array
            /// </summary>
            T[] AsArray();

        }
    }
}
