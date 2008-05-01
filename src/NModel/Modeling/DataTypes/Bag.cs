//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using SC = System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using NModel.Internals;


namespace NModel
{
    /// <summary>
    /// Immutable type for an unordered collection of possibly repeating elements. This is 
    /// also known as a multiset.
    /// </summary>
    /// <typeparam name="T">The sort of element contained in the bag. Must be a subtype of <see cref="IComparable" />.</typeparam>
    /// <remarks>
    /// <para>For any value x, the multiplicity of x is the number of times x occurs 
    /// in the bag, or zero if x is not in the bag.</para>
    /// <para>The data type is immutable; add/remove operations return a new bag.</para>
    /// <para>Equality is structural. Two bags are equal if they contain the same elements with
    /// the same multiplicities. Order does not affect equality.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// static void BagExample()
    /// {
    ///     Bag&lt;int&gt; b1 = Bag&lt;int&gt;.EmptyBag;
    ///     Bag&lt;int&gt; b2 = new Bag&lt;int&gt;(1, 2, 1, 2, 2);
    ///     Bag&lt;int&gt; b3 = new Bag&lt;int&gt;(3, 2, 2, 2);
    ///     Bag&lt;int&gt; b4 = b2.Union(b3);
    ///     Assert.IsTrue(b2.Contains(2));
    ///     Assert.IsTrue(b2.CountItem(2) == 3);
    ///     Assert.IsTrue(b4.CountItem(3) == 6);
    /// }
    /// </code></example>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed class Bag<T> : CollectionValue<T> where T : IComparable
    {
        int count;                      // cache: The number of elements in the bag (sum of multiplicities).
        Map<T, int> representation;     // Mapping of elements to corresponding multiplicities

        // invariant count == Sum(representation.Values);
        // invariant Forall{int i in representation.Values; i > 0};

        #region Constructors

        /// <summary>
        /// Empty bag, provided as a convenience. The static field <see cref="EmptyBag" /> is preferred
        /// instead of using this form of the constructor.
        /// </summary>
        public Bag()
        {
            this.representation = Map<T, int>.EmptyMap;
            this.count = 0;
        }

        /// <summary>
        /// The bag of type T that contains no elements.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static Bag<T> EmptyBag = new Bag<T>();

        /// <summary>
        /// A bag with elements taken from a collection. The size of the bag
        /// is the same as the number of values in the collection.
        /// </summary>
        /// <param name="col">The values to add to the bag.</param>
        public Bag(IEnumerable<T> col)
        {
            int newCount = 0;
            Map<T, int> newRepresentation = Map<T,int>.EmptyMap;

            foreach (T val in col)
            {
                int outputCount;
                Map<T, int> outputRepresentation;
                AddInternal(newCount, newRepresentation, val, 1, out outputCount, out outputRepresentation);
                newRepresentation = outputRepresentation;
                newCount = outputCount;
            }
            this.count = newCount;
            this.representation = newRepresentation;
        }

        /// <summary>
        /// A bag with elements given as a "params" argument. The size of the bag
        /// is the same as the number of values given as parameters.
        /// </summary>
        /// <param name="contents">The values to add to the bag.</param>
        public Bag(params T[] contents)
        {
            int newCount = 0;
            Map<T, int> newRepresentation = Map<T, int>.EmptyMap;

            foreach (T val in contents)
            {
                int outputCount;
                Map<T, int> outputRepresentation;
                AddInternal(newCount, newRepresentation, val, 1, out outputCount, out outputRepresentation);
                newRepresentation = outputRepresentation;
                newCount = outputCount;
            }
            this.count = newCount;
            this.representation = newRepresentation;
        }

        /// <summary>
        /// A bag with elements and their corresponding multiplicities given as a pair enumeration.
        /// </summary>
        /// <param name="contents">Enumeration of key/value pairs representing elements and their corresponding multiplicities.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Bag(IEnumerable<Pair<T, int>> contents)
        {
            int newCount = 0;
            Map<T, int> newRepresentation = Map<T, int>.EmptyMap;

            foreach (Pair<T,int> keyValue in contents)
            {
                if (keyValue.Second > 0)
                {
                    int outputCount;
                    Map<T, int> outputRepresentation;
                    AddInternal(newCount, newRepresentation, keyValue.First, keyValue.Second, out outputCount, out outputRepresentation);
                    newRepresentation = outputRepresentation;
                    newCount = outputCount;
                }
            }
            this.count = newCount;
            this.representation = newRepresentation;
        }

        /// <summary>
        /// A bag with elements and their corresponding multiplicities given as a pair enumeration.
        /// </summary>
        /// <param name="contents">Params array of key/value pairs representing elements and their corresponding multiplicities.</param>       
        public Bag(params Pair<T, int>[] contents)
        {
            int newCount = 0;
            Map<T, int> newRepresentation = Map<T, int>.EmptyMap;

            foreach (Pair<T, int> keyValue in contents)
            {
                if (keyValue.Second > 0)
                {
                    int outputCount;
                    Map<T, int> outputRepresentation;
                    AddInternal(newCount, newRepresentation, keyValue.First, keyValue.Second, out outputCount, out outputRepresentation);
                    newRepresentation = outputRepresentation;
                    newCount = outputCount;
                }
            }
            this.count = newCount;
            this.representation = newRepresentation;
        }

        /// <summary>
        /// Private constructor with explicit field initializations
        /// </summary>
        private Bag(int count, Map<T, int> representation)
        {
            this.count = count;
            this.representation = representation;
        }

        internal static IComparable ConstructValue(Sequence<IComparable> args)
        {
            Bag<T> result = EmptyBag;
            IEnumerator<IComparable> e = args.GetEnumerator();

            while (e.MoveNext())
            {
                T/*?*/ key = (T)e.Current;
                e.MoveNext();
                int multiplicity = (int)e.Current;

                result = result.AddMultiple(key, multiplicity);
            }
            return result;
        }

        #endregion


        #region CollectionValue overrides

        /// <summary>
        /// Tests whether the given element is found in this bag.
        /// </summary>
        /// <param name="item">The item to find</param>
        /// <returns>True, if the <paramref name="item"/> is in this bag, false otherwise.</returns>
        /// <remarks>
        /// Complexity: O(log(this.CountUnique))
        /// </remarks>
        public override bool Contains(T/*?*/ item)
        {
            int multiplicity;
            bool found = representation.TryGetValue(item, out multiplicity);
            //^ assert found ==> multiplicity > 0;
            return found;
        }

        /// <summary>
        /// Returns the number of elements in the bag. This is sum of all multiplicities 
        /// of the unique elements in this bag..
        /// </summary>
        public override int Count
        {
            get
            {
                return count;
            }
        }

        /// <summary>
        /// Converts a bag of one sort to another using a user-provided conversion function.
        /// </summary>
        /// <typeparam name="S">The sort of the resulting bag.</typeparam>
        /// <param name="converter">A <see cref="System.Converter{T,S}" /> function that maps values of type T to values of type S.</param>
        /// <returns>A new bag with the number of elements as this bag.</returns>
        /// <example>
        /// This example converts a bag of strings into a bag of integers that contains the length of each string. 
        /// Note that order does not matter within a bag.
        /// <code>
        /// public void BagConvertExample()
        /// {
        ///    Bag&lt;string&gt; bag1 = new Bag&lt;string&gt;("abc", "def", "hello", "def");
        ///
        ///    Converter&lt;string, int&gt; converter = delegate(string s) { return s == null ? 0 : s.Length; };
        /// 
        ///    Bag&lt;int&gt; expected = new Bag&lt;int&gt;(3, 5, 3, 3);
        ///    Bag&lt;int&gt; actual = bag1.Convert(converter);
        /// 
        ///    Assert.AreEqual(expected, actual);
        /// }
        /// </code>
        /// </example>
        public Bag<S> Convert<S>(Converter<T, S> converter) where S : IComparable
        //^ ensures result.Count == this.Count;
        {
            int newCount = 0;
            Map<S, int> newRepresentation = new Map<S, int>();
            foreach (Pair<T, int> keyValue1 in this.representation)
            {
                T x = keyValue1.First;
                int n = keyValue1.Second;

                int outputCount;
                Map<S, int> outputRepresentation;

                Bag<S>.AddInternal(newCount, newRepresentation, converter(x), n, out outputCount, out outputRepresentation);
                newRepresentation = outputRepresentation;
                newCount = outputCount;
            }
            return new Bag<S>(newCount, newRepresentation);
        }

        /// <summary>
        /// Applies <paramref name="selector"/> to each element of this bag and collects all values where 
        /// the selector function returns true.
        /// </summary>
        /// <param name="selector">A Boolean-valued delegate that acts as the inclusion test. True means
        /// include; false means exclude.</param>
        /// <returns>The bag of all elements of this bag that satisfy the <paramref name="selector"/>. Multiplicity
        /// of selected elements is preserved.</returns>
        public Bag<T> Select(Predicate<T> selector)
        {
            int newCount = 0;
            Map<T, int> newRepresentation = new Map<T, int>();
            foreach (Pair<T, int> keyValue1 in this.representation)
            {
                T x = keyValue1.First;
                int n = keyValue1.Second;

                int outputCount;
                Map<T, int> outputRepresentation;

                if (selector(x))
                {
                    Bag<T>.AddInternal(newCount, newRepresentation, x, n, out outputCount, out outputRepresentation);
                    newRepresentation = outputRepresentation;
                    newCount = outputCount;
                }
            }
            return new Bag<T>(newCount, newRepresentation);
        }

        /// <summary>
        /// Select an arbitrary value from the bag, with external choice.
        /// </summary>
        /// <param name="i">An externally chosen integer in the interval [0, this.Count).</param>
        /// <returns>An element of the bag.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="i"/> is outside 
        /// the interval [0, this.Count).</exception>
        /// <remarks>As a pure function, this method will always return the same value 
        /// for each pair of arguments (<paramref name="this"/> and <paramref name="i"/>).</remarks>
        public override T Choose(int i)
        {
            int j = 0;
            foreach (Pair<T, int> keyValue in this.representation)
            {
                T key = keyValue.First;
                int multiplicity = keyValue.Second;

                if (j <= i && i < (j + multiplicity)) return key;
                j += multiplicity;
            }     

            throw new ArgumentException(MessageStrings.ChooseInvalidArgument);
        }

        #endregion

        #region Query operations specific to Bag
        
        /// <summary>
        /// Returns the number of times <paramref name="x"/> appears in this bag.
        /// </summary>
        /// <param name="x">The element to find</param>
        /// <returns>The number of occurrences of <paramref name="x"/>, or <c>0</c> 
        /// if <paramref name="x"/> is not in this bag.</returns>
        public int CountItem(T x)
        {
            int multiplicity;

            bool found = representation.TryGetValue(x, out multiplicity);
            return found ? multiplicity : 0;
        }

        /// <summary>
        /// Returns the number of unique elements in this bag. (Note: This is less than
        /// the number of elements given by Count if some elements appear more than once 
        /// in the bag.)
        /// </summary>
        public int CountUnique
        {
            get
            {
                return this.representation.Count;
            }
        }

        /// <summary>
        /// Returns a set of all elements with multiplicity greater than zero
        /// </summary>
        public Set<T> Keys { get { return this.representation.Keys; } }

        #endregion

        //#region Object overrides
        //public override bool Equals(object obj)
        //{
        //    Bag<T> other = obj as Bag<T>;
        //    if ((object)other == null) return false;
        //    if (other.Count != this.Count) return false;
        //    return this.representation.Equals(other.representation);
        //}

        ///// <summary>
        ///// Returns the hashcode of its key-value pairs 
        ///// </summary>
        //public override int GetHashCode()
        //{
        //    // TO DO: verify that representation has no keys with value 0.
        //    // These must be omitted to keep hash(a) != hash(b) ==> a != b
        //    return TypedHash<Bag<T>>.ComputeHash(this.representation);
        //} 
        //#endregion

        #region Add and Remove operations
        /// <summary>
        /// Creates a new bag that is the same as this bag except that the multiplicity of x is one larger
        /// </summary>
        /// <param name="x">The item to add</param>
        /// <returns>A new bag with all the elements of this bag plus the item given as an argument.</returns>
        /// <remarks>Note that the Add operation returns a new bag. Bag is an immutable type.</remarks>
        public Bag<T> Add(T x)
        //^ ensures result.Count == this.Count + 1;
        {
            int newCount;
            Map<T, int> newRepresentation;
            AddInternal(this.count, this.representation, x, 1, out newCount, out newRepresentation);
            return new Bag<T>(newCount, newRepresentation);
        }

        /// <summary>
        /// Creates a new bag that is the same as this bag, except that
        /// the multiplicity of x is n larger (or smaller if n is less than zero).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public Bag<T> AddMultiple(T x, int n)
        //^ ensures n >= 0 ==> result.Count == this.Count + n;
        //^ ensures n < 0 ==> result.Count <= this.Count;
        {
            int newCount;
            Map<T, int> newRepresentation;
            AddInternal(this.count, this.representation, x, n, out newCount, out newRepresentation);
            return new Bag<T>(newCount, newRepresentation);
        }

        /// <summary>
        /// Creates a new bag with the same elements as this bag, except that the multiplicity of x
        /// is decremented by one if nonzero
        /// </summary>
        /// <param name="x">The value to be removed</param>
        /// <returns></returns>
        public Bag<T> Remove(T x)
        //^ ensures this.Occurrences(x) == 0 ==> this.Equals(result);
        //^ ensures this.Occurrences(x)  > 0 ==> result.Occurrences(x) == this.Occurrences(x) - 1;
        //^ ensures this.Occurrences(x)  > 0 ==> result.Count == this.Count - 1;
        //^ ensures this.Occurrences(x) == 1 ==> result.UniqueKeys == this.UniqueKeys - 1;
        //^ ensures this.Occurrences(x)  > 1 ==> result.UniqueKeys == this.UniqueKeys;        
        {
            int newCount;
            Map<T, int> newRepresentation;
            AddInternal(this.count, this.representation, x, -1, out newCount, out newRepresentation);
            return new Bag<T>(newCount, newRepresentation);
        }

        /// <summary>
        /// Creates a new bag with the same elements as this bag, except that all occurrences of x are omitted.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public Bag<T> RemoveAll(T x)
        //^ ensures this.Occurrences(x) == 0 ==> this.Equals(result);
        //^ ensures result.Count == this.Count - this.Occurrences(x);
        //^ ensures this.Occurrences(x) > 0 ==> result.UniqueKeys == this.UniqueKeys - 1;
        //^ ensures result.Occurrences(x) == 0;
        {
            int newCount;
            Map<T, int> newRepresentation;
            AddInternal(this.count, this.representation, x, -CountItem(x), out newCount, out newRepresentation);
            return new Bag<T>(newCount, newRepresentation);
        }

        /// <summary>
        /// Calculates a new representations with where multiplicity of x is incremented by n
        /// (n can be negative to remove elements or lower the multiplicity.)
        /// </summary>
        /// <param name="oldTotalCount">Initial sum of all multiplicities</param>
        /// <param name="oldRepresentation"></param>
        /// <param name="x">Element being added</param>
        /// <param name="n">Number of times to add (or remove if negative)</param>
        /// <param name="newTotalCount">Resulting sum of all multiplicities</param>
        /// <param name="newRepresentation">Resulting mapping of elements to multiplicities</param>
        private static void AddInternal(int oldTotalCount, Map<T, int> oldRepresentation,
                                            T x, int n, out int newTotalCount, out Map<T, int> newRepresentation)
        {
            if (oldRepresentation.ContainsKey(x))
            {
                int oldMultiplicity = oldRepresentation[x];
                if (n > 0 && int.MaxValue - n < oldMultiplicity)
                {
                    throw new ArgumentException("Map: operation would make count > int.MaxValue");
                }
                int newMultiplicity = oldMultiplicity + n;
                if (newMultiplicity > 0)
                {
                    //^ assume oldMultiplicity > 0 && oldTotalCount > 0;
                    int tmp = oldTotalCount - oldMultiplicity;
                    if (int.MaxValue - newMultiplicity < tmp)
                        throw new ArgumentException("Map: operation would make count > int.MaxValue");
                    else
                    {
                        newTotalCount = tmp + newMultiplicity;
                        newRepresentation = oldRepresentation.Override(x, newMultiplicity);
                    }
                }
                else
                {
                    newTotalCount = oldTotalCount - oldMultiplicity;
                    newRepresentation = oldRepresentation.RemoveKey(x);
                }
            }
            else if (n > 0)
            {
                if (int.MaxValue - n < oldTotalCount)
                    throw new ArgumentException("Map: operation would make count > int.MaxValue");
                else
                {
                    newTotalCount = oldTotalCount + n;
                    newRepresentation = oldRepresentation.Add(x, n);
                }
            }
            else
            {
                newTotalCount = oldTotalCount;
                newRepresentation = oldRepresentation;
            }
        }
        #endregion

        #region Intersection, Difference and Union operations
        /// <summary>
        /// Difference: Returns the bag containing all the elements from s that are not in t 
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        // satisified with "Difference()" method
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        [SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
        public static Bag<T> operator -(Bag<T> s, Bag<T> t)
        {
            if ((object)s == null) throw new ArgumentNullException("s");
            if ((object)t == null) throw new ArgumentNullException("t");
            return s.Difference(t);
        }

        /// <summary>
        /// Creates a new bag where the multiplicity of each element is the difference
        /// of the multiplicities in this and s. That is, it returns a new bag where all
        /// the elements of s have been removed from this bag.
        /// </summary>
        /// <param name="s">Elements to be removed</param>
        /// <returns>A new bag that is the difference of this bag and the bag given as a paramter.</returns>
        public Bag<T> Difference(Bag<T> s)
        //^ ensures result.Count <= this.Count;
        //^ ensures result.UniqueCount <= this.UniqueCount;
        //^ ensures Forall{T x in s; this.Occurrences(x) > 0 ==> result.Occurrences(x) < this.Occurrences(x)};
        {
            if ((object)s == null) throw new ArgumentNullException("s");
            int newCount = this.Count;
            Map<T, int> newRepresentation = this.representation;
            if (this.CountUnique > s.CountUnique)
            {
                // more keys in this than in s
                foreach (Pair<T, int> keyValue1 in s.representation)
                {
                    T x = keyValue1.First;
                    int n = keyValue1.Second;
                    int outputCount;
                    Map<T, int> outputRepresentation;

                    //^ assert n > 0;
                    AddInternal(newCount, newRepresentation, x, -n, out outputCount, out outputRepresentation);
                    newRepresentation = outputRepresentation;
                    newCount = outputCount;
                }
            }
            else
            {
                // fewer keys (or equal) in this than in s
                foreach (Pair<T, int> keyValue1 in this.representation)
                {
                    T x = keyValue1.First;
                    int n = s.CountItem(x);
                    if (n > 0)
                    {
                        int outputCount;
                        Map<T, int> outputRepresentation;

                        AddInternal(newCount, newRepresentation, x, -n, out outputCount, out outputRepresentation);
                        newRepresentation = outputRepresentation;
                        newCount = outputCount;
                    }
                }
            }
            return new Bag<T>(newCount, newRepresentation);
        }

        /// <summary>
        /// Intersection: Returns the set containing the elements thar are both in s and t [Time: max(s.Count,t.Count)*log(s.Count,t.Count)]
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        // satisified with "Intersect()" method
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static Bag<T> operator *(Bag<T> s, Bag<T> t)
        {
            if ((object)s == null) throw new ArgumentNullException("s");
            if ((object)t == null) throw new ArgumentNullException("t");
            return s.Intersect(t);
        }

        /// <summary>
        /// Creates a new bag where the multiplicity of each element is the minimum
        /// of the multiplicities in this and s.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public Bag<T> Intersect(Bag<T> s)
        {
            if ((object)s == null) throw new ArgumentNullException("s");
            Map<T, int> m1 = (this.CountUnique < s.CountUnique ? this.representation : s.representation);
            Map<T, int> m2 = (this.CountUnique < s.CountUnique ? s.representation : this.representation);
            int newCount = 0;
            Map<T, int> newRepresentation = new Map<T, int>();
            foreach (Pair<T, int> keyValue1 in m1)
            {
                T x = keyValue1.First;
                int n1 = keyValue1.Second;
                int n2;
                bool found = m2.TryGetValue(x, out n2);
                if (found && n2 > 0)
                {
                    //^ assert n1 > 0;
                    int outputCount;
                    Map<T, int> outputRepresentation;

                    AddInternal(newCount, newRepresentation, x, (n1 < n2 ? n1 : n2), out outputCount, out outputRepresentation);
                    newRepresentation = outputRepresentation;
                    newCount = outputCount;
                }
            }
            return new Bag<T>(newCount, newRepresentation);
        }

        /// <summary>
        /// Bag union. Returns a bag where the multiplicities of each element are the
        /// sum of the multiplicities of the elements of this bag and s.
        /// ensures result.Count == this.Count + s.Count;
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        [SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
        public static Bag<T> operator +(Bag<T> s, Bag<T> t)
        //^ ensures result.Count == s.Count + t.Count;
        //^ ensures result.UniqueCount >= s.UniqueCount;
        //^ ensures result.UniqueCount >= t.UniqueCount;
        {
            if ((object)s == null) throw new ArgumentNullException("s");
            if ((object)t == null) throw new ArgumentNullException("t");
            return s.Union(t);
        }

        /// <summary>
        /// Creates a new bag where the multiplicities of each element are the
        /// sum of the multiplicities of the elements of this bag and s.
        /// Ensures result.Count == this.Count + s.Count;
        /// </summary>
        /// <param name="s">The other bag</param>
        /// <returns>A bag containing all of the elements of this and <paramref name="s"/>. For shared elements,
        /// the multiplicities will the sum.</returns>
        public Bag<T> Union(Bag<T> s)
        //^ ensures result.Count == s.Count + t.Count;
        //^ ensures result.UniqueCount >= s.UniqueCount;
        //^ ensures result.UniqueCount >= t.UniqueCount;
        {
            if ((object)s == null) throw new ArgumentNullException("s");

            Map<T, int> m2;
            int newCount;
            Map<T, int> newRepresentation;
            if (this.CountUnique > s.CountUnique)
            {
                newRepresentation = this.representation;
                newCount = this.Count;
                m2 = s.representation;
            }
            else
            {
                newRepresentation = s.representation;
                newCount = s.Count;
                m2 = this.representation;
            }
            foreach (Pair<T, int> keyValue1 in m2)
            {
                T x = keyValue1.First;
                int n = keyValue1.Second;

                int outputCount;
                Map<T, int> outputRepresentation;

                AddInternal(newCount, newRepresentation, x, n, out outputCount, out outputRepresentation);
                newRepresentation = outputRepresentation;
                newCount = outputCount;
            }
            return new Bag<T>(newCount, newRepresentation);
        }

        #endregion


        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection 
        /// </summary>
        /// <returns>An enumerator that iterates through the collection</returns>
        public override IEnumerator<T> GetEnumerator()
        {
            foreach (Pair<T, int> keyValue in this.representation)
                for (int i = 0; i < keyValue.Second; i += 1)
                    yield return keyValue.First;
        }

        #endregion


        #region CompoundValue Overrides

        /// <summary>
        /// Returns an enumeration of all (readonly) field values of this compound value in a fixed order.
        /// </summary>
        /// <returns>An enumeration of the field values of this compound value.</returns>
        /// <remarks>See <see cref="CompoundValue.FieldValues"/> for more information.</remarks>
        override public IEnumerable<IComparable> FieldValues()
        {
            foreach (Pair<T, int> keyValue in this.representation)
            {
                yield return keyValue.First;
                yield return keyValue.Second;
            }
        }
        #endregion


        #region Object overrides

        /// <exclude />
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Bag<T> other = obj as Bag<T>;
            if ((object)other == null) return false;
            if (this.Count != other.Count) return false;
            return Object.Equals(this.representation, other.representation);
        }

        /// <exclude />
        public override int GetHashCode()
        {
            return TypedHash<Bag<T>>.ComputeHash(this.representation);
        }

        /// <exclude />
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Bag(");
            bool isFirst = true;
            foreach (Pair<T, int> keyValue in this.representation)
            {
                if (!isFirst) sb.Append(", ");
                PrettyPrinter.Format(sb, keyValue.First);
                if (keyValue.Second > 1)
                {
                    sb.Append(" (");
                    PrettyPrinter.Format(sb, keyValue.Second);
                    sb.Append(")");
                }
                isFirst = false;
            }
            sb.Append(")");
            return sb.ToString();
        }
        #endregion

    }

}
