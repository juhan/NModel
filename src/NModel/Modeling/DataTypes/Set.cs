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
    /// Immutable type representing an unordered collection of distinct values
    /// </summary>    
    /// <typeparam name="T">The sort of element</typeparam>    
    /// <remarks>
    /// <para>Set is an immutable type; Add/remove operations return a new set. Comparison
    /// for equality uses Object.Equals.</para>
    /// 
    /// <para>Formally, this data type denotes a pair (elementType, untyped set of values),
    /// where the element type is given by the type parameter T. As a consequence, sets are only
    /// equal if they are of the same sort (element type) and contain the same elements. 
    /// For example, the empty Set&lt;int&gt; != Set&lt;string&gt;</para>
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public sealed class Set<T> : CachedHashCollectionValue<T> where T : IComparable
    {
        int count;
        LobTree<T>/*?*/ elems;

        #region Constructors

        /// <summary>
        /// Constructs an empty set of type T
        /// </summary>
        public Set() { }

        private Set(int count, LobTree<T>/*?*/ elems)
        {
            this.count = count;
            this.elems = elems;
        }

        /// <summary>
        /// The empty set of sort T. Note: If S and T are different types,
        /// Object.Equals(Set&lt;T&gt;.EmptySet, Set&lt;S&gt;.EmptySet) == false.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Set<T> EmptySet = new Set<T>();

        /// <summary>
        /// Construct a set containing all enumerated elements
        /// </summary>
        /// <param name="elements">The enumerable object containing the elements that will be members of the set</param>
        public Set(IEnumerable<T> elements)
        {
            int newCount = 0;
            LobTree<T>/*?*/ newElems = null;
            bool added = false;
            foreach (T o in elements)
            {
                newElems = LobTree<T>.Insert(newElems, o, false, out added);
                if (added) newCount += 1;
            }
            this.count = newCount;
            this.elems = newElems;
        }

        /// <summary>
        /// Construct a set from the elements given in the argument list
        /// </summary>
        /// <param name="elements">The argument list</param>
        public Set(params T[] elements)
        {
            int newCount = 0;
            LobTree<T>/*?*/ newElems = null;
            bool added = false;
            foreach (T o in elements)
            {
                newElems = LobTree<T>.Insert(newElems, o, false, out added);
                if (added) newCount += 1;
            }
            this.count = newCount;
            this.elems = newElems;
        }

        internal static IComparable ConstructValue(Sequence<IComparable> args)
        {
            Set<T> result = EmptySet;
            foreach(IComparable arg in args)
            {
               result = result.Add((T)arg);
            }
            return result;
        }

        #endregion

        #region CollectionValue Members
        /// <summary>
        /// Returns the number of elements in the set (also known as the cardinality of the set). [Time: 1]
        /// </summary>
        public override int Count
        {
            get { return count; }
        }

        /// <summary>
        /// Tests whether the given element is found in the set.
        /// </summary>
        /// <param name="item">The item to find</param>
        /// <returns>True, if the <paramref name="item"/> is in this set, false otherwise.</returns>
        /// <remarks>
        /// Complexity: O(log(this.Count))
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
        /// <param name="t">The superset</param>
        /// <returns>True if this every element of this set is also found in set <paramref name="t"/>; false otherwise.</returns>
        public bool IsSubsetOf(Set<T> t)
        {
            Set<T> s = this;
            if (!(s.Count <= t.Count)) return false;
            foreach (T o in s)
                if (!t.Contains(o)) return false;
            return true;
        }
       
        /// <summary>
        /// Proper subset relation
        /// </summary>
        /// <param name="t">The superset</param>
        /// <returns>True if this every element of this set is found in set <paramref name="t"/>
        /// and there exists at least one element in <paramref name="t"/> that is not found in this set; 
        /// false otherwise.</returns>
        public bool IsProperSubsetOf(Set<T> t)
        {
            Set<T> s = this;
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
        public bool IsSupersetOf(Set<T> t)
        {
            if ((object)t == null) throw new ArgumentNullException("t");
            return t.IsSubsetOf(this);
        }

        /// <summary>
        /// Proper superset relation
        /// </summary>
        /// <param name="t">The subset</param>
        /// <returns>True if this every element of set <paramref name="t"/> is also found in this set
        /// and there exists at least one element in this set that is not found in <paramref name="t"/>; 
        /// false otherwise.</returns>
        public bool IsProperSupersetOf(Set<T> t)
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
        /// <returns>The new set that does not contain <paramref name="value"/>.</returns>
        public Set<T> Remove(T value)
        //^ ensures !this.Contains(value) <==> ((object)this == result);  // pointer equals iff value not contained
        //^ ensures this.Contains(value) <==> (result.Count + 1 == this.Count);
        {
            bool deleted = false;
            LobTree<T>/*?*/ newElems = LobTree<T>.Remove(this.elems, value, out deleted);
            return (deleted ? new Set<T>(count - 1, newElems) : this);
        }

        /// <summary>
        /// Returns a new set with all of the elements of this set, plus the value given as an argument [Time: log(this.Count)]
        /// </summary>
        /// <param name="value">The value to add</param>
        /// <returns>The new set containing <paramref name="value"/>.</returns>
        public Set<T> Add(T value)
        {
            bool added;
            LobTree<T> newElems = LobTree<T>.Insert(elems, value, false, out added);
            return (added ? new Set<T>(count + 1, newElems) : this);
        }

        private void InPlaceRemove(T value)
        {
            bool deleted = false;
            elems = LobTree<T>.Remove(elems, value, out deleted);
            if (deleted)
            {
                count -= 1;
                this.InvalidateCache();
            }
        }

        private int InPlaceAdd(T value)
        {
            bool added;
            elems = LobTree<T>.Insert(elems, value, false, out added);
            if (added)
            {
                count += 1;
                this.InvalidateCache();
            }
            return count;
        }

        /// <summary>
        /// Pretty printer
        /// </summary>
        /// <returns>A human-readable representation of the this set.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Set(");
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
        /// Set union [Time: max(s.Count,t.Count)*log(max(s.Count, t.Count))]
        /// </summary>
        /// <param name="s">A set</param>
        /// <param name="t">A set</param>
        /// <returns>The set containing all the elements from both <paramref name="s"/> and <paramref name="t"/></returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        // satisified with "Union()" method
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        [SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
        public static Set<T> operator +(Set<T> s, Set<T> t)
        {
            Set<T> r;
            if (s.Count > t.Count)
            {
                r = new Set<T>(s.count, s.elems);
                foreach (T o in t)
                    r.InPlaceAdd(o);
            }
            else
            {
                r = new Set<T>(t.count, t.elems);
                foreach (T o in s)
                   r.InPlaceAdd(o);
            }
            return r;
        }

        /// <summary>
        /// Same as operator + (set union). 
        /// </summary>
        /// <param name="t">A set to be unioned with this set</param>
        /// <returns>The set containing all the elements from both <paramref name="this"/> and <paramref name="t"/></returns>
        public Set<T> Union(Set<T> t)
        {
            return this + t;
        }

        /// <summary>
        /// Set difference [Time: t.Count * log(s.Count)] 
        /// </summary>
        /// <param name="s">The set containing potentially unwanted elements</param>
        /// <param name="t">The set of unwanted elements</param>
        /// <returns>The set containing all the elements from <paramref name="s"/> that are not in <paramref name="t"/></returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        // satisified with "Difference()" method
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        [SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
        public static Set<T> operator -(Set<T> s, Set<T> t)
        {
            Set<T> r = new Set<T>(s.count, s.elems);
            foreach (T o in t)
                r.InPlaceRemove(o);
            return r;
        }

        /// <summary>
        /// Set difference. Same as operator -
        /// </summary>
        /// <param name="t">The set of unwanted elements to be removed from this set</param>
        /// <returns>The set containing all the elements from this that are not in <paramref name="t"/></returns>
        public Set<T> Difference(Set<T> t)
        {
            return this - t;
        }

        /// <summary>
        /// Set intersection  [Time: max(s.Count,t.Count)*log(max(s.Count, t.Count))]
        /// </summary>
        /// <param name="s">A set</param>
        /// <param name="t">A set</param>
        /// <returns>The set of all elements that are shared in common by <paramref name="s"/> and <paramref name="t"/></returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        // satisified with "Intersect()" method
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static Set<T> operator *(Set<T> s, Set<T> t)
        {
            Set<T> r;
            if (s.Count < t.Count)
            {
                r = new Set<T>(s.count, s.elems);
                foreach (T o in s)
                    if (!t.Contains(o))
                        r.InPlaceRemove(o);
            }
            else
            {
                r = new Set<T>(t.count, t.elems);
                foreach (T o in t)
                    if (!s.Contains(o))
                        r.InPlaceRemove(o);
            }
            return r;
        }

        /// <summary>
        /// Set intersection. Same as operator *.  [Time: max(s.Count,t.Count)*log(max(s.Count, t.Count))]
        /// </summary>
        /// <param name="t">A set to be intersected with this set.</param>
        /// <returns>The set of all elements shared by this and t</returns>
        public Set<T> Intersect(Set<T> t)
        {
            return this * t;
        }

        /// <summary>
        /// Distributed set union. [Time: s1.Count * s2.Count * ... * sn.Count where n is the number of sets in s]
        /// </summary>
        /// <param name="s">A set of sets to be combined by set union</param>
        /// <returns>The union of all the sets in <paramref name="s"/>. That is, the set containing all the elements 
        /// of all the elements of <paramref name="s"/>. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static Set<T> BigUnion(Set<Set<T>> s)
        {
            Set<T> r = EmptySet;
            int i = 0;
            foreach (Set<T> ks in s)
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
        /// <param name="s">A set of sets to be combined by set intersection</param>
        /// <returns>The intersection of all the sets in s. That is, the set containing only those elements 
        /// that are shared by all of the elements of s</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static Set<T> BigIntersect(Set<Set<T>> s)
        {
            Set<T> r = EmptySet; int i = 0;
            foreach (Set<T> ks in s)
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
        /// <example>
        /// This example projects a set of strings into a set of integers by taking the length of each string. 
        /// Note that the number of elements in the result is smaller than the original set because some of 
        /// the strings are of the same length.
        /// <code>
        /// void SetConvertExample()
        /// {
        ///    Set&lt;string&gt; set1 = new Set&lt;string&gt;("abc", "def", "g", "h", "i", "j", "klm");
        ///    Set&lt;int&gt; expected = new Set&lt;int&gt;(1, 3);
        ///    Set&lt;int&gt; actual = set1.Convert(delegate(string s) { return (s != null ? s.Length : 0); });
        ///    
        ///    Assert.AreEqual(expected, actual);
        /// }
        /// </code>
        /// </example>
        public Set<S> Convert<S>(Converter<T, S> converter) where S : IComparable
        {
            Set<S> result = new Set<S>();
            foreach (T val in this)
                result.InPlaceAdd(converter(val));
            return result;
        }

        /// <summary>
        /// Applies <paramref name="selector"/> to each element of this set and collects all values where 
        /// the selector function returns true.
        /// </summary>
        /// <param name="selector">A Boolean-valued delegate that acts as the inclusion test. True means
        /// include; false means exclude.</param>
        /// <returns>The set of all elements of this set that satisfy the <paramref name="selector"/></returns>
        public Set<T> Select(Predicate<T> selector)
        {
            Set<T> result = new Set<T>();
            foreach (T val in this)
                if (selector(val)) result.InPlaceAdd(val);
            return result;
        }

        /// <summary>
        /// Checks that internal assumptions in the implementation of sets hold for this set. 
        /// Used for debugging and verification only.
        /// </summary>
        /// <returns>true</returns>
        public bool InvariantHolds()
        {
            return (this.elems != null) ? this.count == this.elems.Count && this.elems.InvariantHolds() : this.count == 0;
        }


        #region Subset Choice
 
        // Surely this is defined somewhere in a system library
        static int PowerOfTwo(int i)
        //^ requires i <= 30;
        {
            int result = 1;
            while (i-- > 0) result *= 2;
            return result;
        }

        /// <summary>
        /// Returns the number of subsets of this set or System.Int32.MaxValue if the number of
        /// subsets exceeds the representation of System.Int32.
        /// </summary>
        public int CountSubsets 
        {
            get 
            {
                return (this.count > 30) ? System.Int32.MaxValue : PowerOfTwo(this.count);
            }
        }

        /// <summary>
        /// Returns a chosen subset of this set, with externally provided choice given by <paramref name="i"/>. 
        /// </summary>
        /// <param name="i">An nonnegative integer that is less than <c>this.CountSubsets</c>, or an arbitrary integer. Integers
        /// within the range of [0, this.CountSubsets) will enumerate subsets; outside this range will choose an arbitrary 
        /// subset.</param>
        /// <returns>The chosen subset</returns>
        public Set<T> ChooseSubset(int i)
        {
            Set<T> result = Set<T>.EmptySet;
            IEnumerator<T> se = this.GetEnumerator();

            unchecked
            {
                uint tmp = (uint) i;
                int bitCount = 0;
                while (se.MoveNext())
                {
                    if ((tmp & 0x1u) != 0u)
                        result = result.Add(se.Current);
                    tmp = tmp >> 1;
                    bitCount += 1;
                    if (bitCount > 32)
                    {
                       tmp = (uint) TypedHash<Set<T>>.ComputeHash(result, i);
                       bitCount = 0;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns an arbitrarily chosen subset of this set. This is not a pure function; successive calls
        /// will produce differing values and update the internal state of <seealso cref="HashAlgorithms.GlobalChoiceController"/>.
        /// </summary>
        /// <returns>A subset of this set</returns>
        public Set<T> ChooseSubset()
        {
            return this.ChooseSubset(HashAlgorithms.GlobalChoiceController.Next(int.MinValue, int.MaxValue));
        }

        /// <summary>
        /// Returns a chosen nonempty subset of this set, with externally provided choice given by <paramref name="i"/>. 
        /// </summary>
        /// <param name="i">An nonnegative integer that is less than <c>this.CountSubsets - 1</c>, or an arbitrary integer. Integers
        /// within the range of [0, this.CountSubsets - 1) will enumerate nonempty subsets; outside this range will choose an arbitrary 
        /// subset.</param>
        /// <returns>The chosen subset</returns>
        public Set<T> ChooseNonemptySubset(int i) 
        //^ requires !this.IsEmpty;
        //^ ensures !result.IsEmpty;
        {
            if (this.IsEmpty)
                throw new InvalidOperationException("ChooseNonemptySubset: this set must be nonempty.");
            else
            {
                int j = (i < int.MaxValue ? i + 1 : i);
                while (true)
                {
                    Set<T> subset = this.ChooseSubset(j);
                    if (!subset.IsEmpty) return subset;
                    j = TypedHash<Set<T>>.ComputeHash(j);
                }
            }
        }

        /// <summary>
        /// Returns an arbitrarily chosen nonempty subset of this set. This is not a pure function; successive calls
        /// will produce differing values and update the internal state of <seealso cref="HashAlgorithms.GlobalChoiceController"/>.
        /// This set must nonempty.
        /// </summary>
        /// <returns>A subset of this set</returns>
        /// <exception cref="InvalidOperationException">Thrown if the number of elements in this set is not in the interval [1, 30].</exception>
        public Set<T> ChooseNonemptySubset()
        {
            return this.ChooseNonemptySubset(HashAlgorithms.GlobalChoiceController.Next(int.MinValue, int.MaxValue));
        }


        /// <summary>
        /// Enumerates the subsets of this set.
        /// </summary>
        /// <returns>An IEnumerable that will return each possible subset of this set</returns>
        /// <remarks>The enumeration created by method has exponential complexity, O(2 ^ this.Count). </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the number of elements in this set is not in the interval [0, 30].</exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IEnumerable<Set<T>> AllSubsets()
        {
            if (this.Count > 30) throw new InvalidOperationException("NModel.Set.AllSubsets: Set has too many elements for this operation. Must have 30 or fewer elements.");
            int nSubsets = PowerOfTwo(this.Count);
            for (int i = 0; i < nSubsets; i += 1)
                yield return this.ChooseSubset(i);
        } 
        #endregion
    }
}
