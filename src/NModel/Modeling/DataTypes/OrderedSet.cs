//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using SC = System.Collections;
using System.Diagnostics.CodeAnalysis;
using NModel.Internals;

namespace NModel
{
    /// <summary>
    /// A collection of distinct values, maintained in order according to the <see cref="IComparable"/> interface
    /// </summary>
    /// <typeparam name="T">The sort of element</typeparam>
    /// <remarks>
    /// <para>Enumeration occurs in IComparable order.</para>
    /// 
    /// <para>OrderedSet is an immutable type; Add/remove operations return a new set. Comparison
    /// for equality uses Object.Equals.</para>
    /// 
    /// <para>Formally, this data type denotes a pair (elementType, untyped sequence of distinct values),
    /// where the element type is given by the type parameter T. As a consequence, ordered sets are only
    /// equal if they are of the same sort (element type) and contain the same elements. 
    /// For example, the empty OrderedSet&lt;int&gt; != OrderedSet&lt;string&gt;</para>
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public sealed class OrderedSet<T> : CachedHashCollectionValue<T> 
    {
        int count;
        RedBlackTree<T>/*?*/ elems;

        #region Constructors

        /// <summary>
        /// Constructs an empty ordered set
        /// </summary>
        public OrderedSet() { }

        private OrderedSet(int count, RedBlackTree<T>/*?*/ elems)
        {
            this.count = count;
            this.elems = elems;
        }

        /// <summary>
        /// The empty set of sort T. Note: If S and T are different types,
        /// Object.Equals(OrderedSet&lt;T&gt;.EmptyOrderedSet, OrderedSet&lt;S&gt;.EmptySet) == false.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly OrderedSet<T> EmptySet = new OrderedSet<T>();

        /// <summary>
        /// Construct a set containing all enumerated elements
        /// </summary>
        /// <param name="os">The enumerable object containing the elements that will be members of the set</param>
        public OrderedSet(IEnumerable<T> os)
        {
            int newCount = 0;
            RedBlackTree<T>/*?*/ newElems = null;
            bool added = false;
            foreach (T o in os)
            {
                newElems = RedBlackTree<T>.Insert(newElems, o, false, out added);
                if (added) newCount += 1;
            }
            this.count = newCount;
            this.elems = newElems;
        }

        /// <summary>
        /// Construct a set from the elements given in the argument list
        /// </summary>
        /// <param name="contents">The argument list</param>
        public OrderedSet(params T[] contents)
        {
            int newCount = 0;
            RedBlackTree<T>/*?*/ newElems = null;
            bool added = false;
            foreach (T o in contents)
            {
                newElems = RedBlackTree<T>.Insert(newElems, o, false, out added);
                if (added) newCount += 1;
            }
            this.count = newCount;
            this.elems = newElems;
        }

        internal static IComparable ConstructValue(Sequence<IComparable> args)
        {
            OrderedSet<T> result = EmptySet;
            foreach (IComparable arg in args)
            {
                T val = (T)arg;
                result = result.Add(val);
            }
            return result;
        }
        #endregion

        #region CollectionValue Members
        /// <summary>
        /// Returns the number of elements in the set (also known as the cardinality of the set). 
        /// </summary>
        /// <remarks>
        /// Complexity: O(1)
        /// </remarks>
        public override int Count
        {
            get { return count; }
        }

        /// <summary>
        /// Tests whether the given element is found in the collection value.
        /// </summary>
        /// <param name="item">The item to find</param>
        /// <returns>True, if the <paramref name="item"/> is in this collection value, false otherwise.</returns>
        /// <remarks>
        /// Complexity: log(this.count)
        /// </remarks>
        public override bool Contains(T item)
        {
            T/*?*/ value;
            return ((object)elems == null ? false : elems.TryGetValue(item, out value));
        }

        #endregion

        #region Subset and superset tests

        /// <summary>
        /// Subset relation
        /// </summary>
        /// <param name="t"></param>
        /// <returns>True if this every element of this set is also found in set <paramref name="t"/>; false otherwise.</returns>
        public bool IsSubsetOf(OrderedSet<T> t)
        {
            OrderedSet<T> s = this;
            if (!(s.Count <= t.Count)) return false;
            foreach (T o in s)
                if (!t.Contains(o)) return false;
            return true;
        }

        /// <summary>
        /// Proper subset relation
        /// </summary>
        /// <param name="t"></param>
        /// <returns>True if this every element of this set is found in set <paramref name="t"/>
        /// and there exists at least one element in <paramref name="t"/> that is not found in this set; 
        /// false otherwise.</returns>
        public bool IsProperSubsetOf(OrderedSet<T> t)
        {
            OrderedSet<T> s = this;
            if (!(s.Count < t.Count)) return false;
            foreach (T o in s)
                if (!t.Contains(o)) return false;
            return true;
        }

        /// <summary>
        /// Superset relation
        /// </summary>
        /// <param name="t"></param>
        /// <returns>True if this every element of set <paramref name="t"/> is also found in this set; false otherwise.</returns>
        public bool IsSupersetOf(OrderedSet<T> t)
        {
            if ((object)t == null) throw new ArgumentNullException("t");
            return t.IsSubsetOf(this);
        }

        /// <summary>
        /// Proper superset relation
        /// </summary>
        /// <param name="t"></param>
        /// <returns>True if this every element of set <paramref name="t"/> is also found in this set
        /// and there exists at least one element in this set that is not found in <paramref name="t"/>; 
        /// false otherwise.</returns>
        public bool IsProperSupersetOf(OrderedSet<T> t)
        {
            if ((object)t == null) throw new ArgumentNullException("t");
            return t.IsProperSubsetOf(this);
        }
        #endregion

        /// <summary>
        /// Returns a new set containing every element of this set except the value given as a parameter.
        /// If value is not in this set, then returns this set. [Time: log(this.Count)]
        /// </summary>
        /// <param name="value">The value to be deleted.</param>
        /// <returns>The new set</returns>
        public OrderedSet<T> Remove(T value)
        //^ ensures !this.Contains(value) <==> ((object)this == result);  // pointer equals iff value not contained
        //^ ensures this.Contains(value) <==> (result.Count + 1 == this.Count);
        {
            bool deleted = false;
            RedBlackTree<T>/*?*/ newElems = RedBlackTree<T>.Remove(this.elems, value, out deleted);
            return (deleted ? new OrderedSet<T>(count - 1, newElems) : this);
        }

        /// <summary>
        /// Returns a new set with all of the elements of this set, plus the value given as an argument [Time: log(this.Count)]
        /// </summary>
        /// <param name="value">The value to add</param>
        /// <returns>The new set containing value.</returns>
        public OrderedSet<T> Add(T value)
        {
            bool added;
            RedBlackTree<T> newElems = RedBlackTree<T>.Insert(elems, value, false, out added);
            return (added ? new OrderedSet<T>(count + 1, newElems) : this);
        }


        ///// <summary>
        ///// Set membership test. [Time: log(this.Count)]
        ///// </summary>
        ///// <param name="item">The element to find</param>
        ///// <returns>True if this contains item, false otherwise</returns>
        //public bool this[T item]
        //{
        //    get
        //    {
        //        return this.Contains(item);
        //    }
        //}

        private void InPlaceRemove(T value)
        {
            bool deleted = false;
            elems = RedBlackTree<T>.Remove(elems, value, out deleted);
            if (deleted)
            {
                count -= 1;
                this.InvalidateCache();
            }
        }

        private int InPlaceAdd(T value)
        {
            bool added;
            elems = RedBlackTree<T>.Insert(elems, value, false, out added);
            if (added)
            {
                count += 1;
                this.InvalidateCache();
            }
            return count;
        }

        /// <summary>
        /// String representation of an ordered set
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("OrderedSet(");
            bool isFirst = true;
            if (this.elems != null)
                foreach (T val in this.elems)
                {
                    if (!isFirst) sb.Append(", ");
                    PrettyPrinter.Format(sb, val);
                    isFirst = false;
                }
            sb.Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// OrderedSet union [Time: max(s.Count,t.Count)*log(max(s.Count, t.Count))]
        /// </summary>
        /// <param name="s">A set</param>
        /// <param name="t">A set</param>
        /// <returns>The set containing all the elements from both s and t</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        // satisified with "Union()" method
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        [SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
        public static OrderedSet<T> operator +(OrderedSet<T> s, OrderedSet<T> t)
        {
            OrderedSet<T> r;
            if (s.Count > t.Count)
            {
                r = new OrderedSet<T>(s.count, s.elems);
                foreach (T o in t)
                    r.InPlaceAdd(o);
            }
            else
            {
                r = new OrderedSet<T>(t.count, t.elems);
                foreach (T o in s)
                    r.InPlaceAdd(o);
            }
            return r;
        }

        /// <summary>
        /// Same as operator + (set union). 
        /// </summary>
        /// <param name="t">A set to be unioned with this set</param>
        /// <returns>The set containing all the elements from both this and t</returns>
        public OrderedSet<T> Union(OrderedSet<T> t)
        {
            return this + t;
        }

        /// <summary>
        /// OrderedSet difference [Time: t.Count * log(s.Count)] 
        /// </summary>
        /// <param name="s">The set containing potentially unwanted elements</param>
        /// <param name="t">The set of unwanted elements</param>
        /// <returns>The set containing all the elements from s that are not in t</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        // satisified with "Difference()" method
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        [SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
        public static OrderedSet<T> operator -(OrderedSet<T> s, OrderedSet<T> t)
        {
            OrderedSet<T> r = new OrderedSet<T>(s.count, s.elems);
            foreach (T o in t)
                r.InPlaceRemove(o);
            return r;
        }

        /// <summary>
        /// OrderedSet difference. Same as operator -
        /// </summary>
        /// <param name="t">The set of unwanted elements to be removed from this set</param>
        /// <returns>The set containing all the elements from this that are not in t</returns>
        public OrderedSet<T> Difference(OrderedSet<T> t)
        {
            return this - t;
        }

        /// <summary>
        /// OrderedSet intersection  [Time: max(s.Count,t.Count)*log(max(s.Count, t.Count))]
        /// </summary>
        /// <param name="s">A set</param>
        /// <param name="t">A set</param>
        /// <returns>The set of all elements that are shared by s and t</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        // satisified with "Intersect()" method
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static OrderedSet<T> operator *(OrderedSet<T> s, OrderedSet<T> t)
        {
            OrderedSet<T> r;
            if (s.Count < t.Count)
            {
                r = new OrderedSet<T>(s.count, s.elems);
                foreach (T o in s)
                    if (!t.Contains(o))
                        r.InPlaceRemove(o);
            }
            else
            {
                r = new OrderedSet<T>(t.count, t.elems);
                foreach (T o in t)
                    if (!s.Contains(o))
                        r.InPlaceRemove(o);
            }
            return r;
        }

        /// <summary>
        /// OrderedSet intersection. Same as operator *.  [Time: max(s.Count,t.Count)*log(max(s.Count, t.Count))]
        /// </summary>
        /// <param name="t">A set to be intersected with this.</param>
        /// <returns>The set of all elements that shared by this and t</returns>
        public OrderedSet<T> Intersect(OrderedSet<T> t)
        {
            return this * t;
        }

        /// <summary>
        /// Distributed set union. [Time: s1.Count * s2.Count * ... * sn.Count where n is the number of sets in s]
        /// </summary>
        /// <param name="s">A set of sets to be combined by set union</param>
        /// <returns>The union of all the sets in s. That is, the set containing all the elements 
        /// of all the elements of s </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static OrderedSet<T> BigUnion(OrderedSet<OrderedSet<T>> s)
        {
            OrderedSet<T> r = EmptySet;
            int i = 0;
            foreach (OrderedSet<T> ks in s)
            {
                if (i == 0)
                    r = ks;
                else
                    r = r + ks;
                i++;
            }
            return r;
        }

        /// <summary>
        /// Distributed Intersection: The resulting set is the intersection of all the elements of s, 
        /// i.e. it contains the elements that are in all elements [Time: max(s.Count,t.Count)*log(s.Count,t.Count)]
        /// </summary>
        /// <summary>
        /// Distributed set intersection. [Time: max(s1.Count,  s2.Count, ..., sn.Count) * log(max(s1.Count, s2.Count, ..., sn.Coung))
        /// where n is the number of sets in s]
        /// </summary>
        /// <param name="s">A set of sets to be combined by set intersection</param>
        /// <returns>The intersection of all the sets in s. That is, the set containing only those elements 
        /// that are shared by all of the elements of s</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static OrderedSet<T> BigIntersect(OrderedSet<OrderedSet<T>> s)
        {
            OrderedSet<T> r = EmptySet; int i = 0;
            foreach (OrderedSet<T> ks in s)
            {
                if (i == 0)
                    r = ks;
                else
                    r = r * ks;
                i++;
            }
            return r;
        }

        #region IEnumerable<T> Members

        /// <summary>
        /// Enumerator of each element of this set. If two sets are equal, then their enumerations are in the same order.
        /// (This is a fixed order with no external significance.)
        /// </summary>
        /// <returns>The enumerator of this set</returns>
        public override IEnumerator<T> GetEnumerator()
        {
            return ((object)elems == null ? new List<T>().GetEnumerator() : elems.GetEnumerator());
        }

        #endregion

        /// <summary>
        /// Transforms a set of one sort to a set of another sort by invoking a given delegate on each element. 
        /// If the mapping is injective, then the number of elements in the result will be the same as the number
        /// of elements in this. Otherwise, (if not injective) the number of elements will be fewer.
        /// </summary>
        /// <typeparam name="S">The sort of the result</typeparam>
        /// <param name="converter">A pure (side-effect free) function that maps an element of T to an element of S</param>
        /// <returns>The set</returns>
        public OrderedSet<S> Convert<S>(Converter<T, S> converter) 
        {
            OrderedSet<S> result = new OrderedSet<S>();
            foreach (T val in this)
                result.InPlaceAdd(converter(val));
            return result;
        }

        /// <summary>
        /// Checks that internal assumptions in the implementation of sets hold for this set. 
        /// Used for debugging and verification only.
        /// </summary>
        /// <returns>true</returns>
        public bool InvariantHolds()
        {
            return (this.elems != null) ? 
                this.count == this.elems.Count && 
                this.elems.InvariantHolds() 
                : this.count == 0;
        }
    }
}

