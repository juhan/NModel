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
    /// A <c>Filter</c> is a function that replaces a value of type <typeparamref name="T"/>. 
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="value">The value</param>
    /// <returns>A replacement value of type <typeparamref name="T"/></returns>
    /// <remarks>The function should not make any updates to state. Filters are used in 
    /// the <seealso cref="Map{T,S}.Override(T,Filter{S})"/> method.</remarks>
    public delegate T Filter<T>(T value);

    /// <summary>
    /// Immutable type that represents a finite mapping of keys to values.
    /// </summary> 
    /// <remarks>
    /// Maps associate keys with values. Maps are similar to <see>Dictionary</see>
    /// but are immutable and use structural equality. Add and Remove operations return new maps.
    /// </remarks>
    /// <example>
    /// <code>
    /// public void MapExample()
    /// {
    ///     Map&lt;string, int&gt; m1 = Map&lt;string, int&gt;.EmptyMap;
    ///     Map&lt;string, int&gt; m2 = new Map&lt;string, int&gt;("abc", 1, "def", 2);
    ///     Map&lt;string, int&gt; m3 = new Map&lt;string, int&gt;("efg", 3, "def", 2);
    ///     Map&lt;string, int&gt; m4 = m2.Merge(m3);
    ///     Map&lt;string, int&gt; m5 = m4.Add("hij", 4);
    ///     Map&lt;string, int&gt; m6 = m5.RemoveKey("efg");
    ///     Map&lt;string, int&gt; m7 = m6.Override("abc", -1);
    ///     Map&lt;string, int&gt; m8 = new Map&lt;string, int&gt;("abc", -1, "def", 2, "hij", 4);
    ///     Assert.AreEqual(m8, m7); 
    /// }
    /// </code>
    /// </example>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public sealed class Map<T, S> : CachedHashCollectionValue<Pair<T, S>> where T : IComparable where S : IComparable  
    {
        int count;                       // cache: the number of elements
        LobTree<Maplet>/*?*/ elems;      // tree of maplet lists, ordered by hash code of key
        Set<T>/*?*/ keyCache = null;     // cache: if nonnull, the domain of the map
        Set<S>/*?*/ valuesCache = null;  // cache: if nonnull, the range of the map

        #region Maplet Helper Class
        /// <summary>
        /// <para>Auxiliary type needed for overriding Equals method.</para>
        /// <para>NOTE: This type has an equality defined only to make the LobTree 
        /// used in its implementation work. Do not use the Maplet type for anything else,
        /// instead, extract the Pair out of it. </para>
        /// </summary>
        sealed private class Maplet : IComparable
        {
            public Pair<T, S> d;

            public Maplet(Pair<T, S> d)
            {
                this.d = d;
            }

            public override bool Equals(Object o)
            {
                Maplet/*?*/ m = o as Maplet;
                if (m == null) return false;
                return Object.Equals(this.d.First, m.d.First);
            }

            public override int GetHashCode()
            {
                return HashAlgorithms.GetHashCode(d.First);
            }


            #region IComparable<Maplet> Members

            public int CompareTo(object obj)
            {
                Maplet other = obj as Maplet;
                if ((object)other == null) return 1;
                int f1 = HashAlgorithms.CompareValues(this.d.First, other.d.First);
                if (f1 == 1 || f1 == -1) return f1;
                return 0;
            }

            #endregion
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor, provided for convenience only. The static property <see cref="EmptyMap" /> is 
        /// preferred.
        /// </summary>
        public Map() { }

        private Map(int count, LobTree<Maplet>/*?*/ elems) { this.count = count; this.elems = elems; }

        static Map<T, S> emptyMap = new Map<T, S>();

        /// <summary>
        /// The empty map of sort &lt;T, S&gt;
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static Map<T, S> EmptyMap { get { return emptyMap; } }

        /// <summary>
        /// Creates a map from the key and value given as arguments.
        /// </summary>
        /// <param name="key1">The key</param>
        /// <param name="value1">The value corresponding to <paramref name="key1"/></param>
        public Map(T key1, S value1)
        {
            LobTree<Maplet>/*?*/ newElems = null;
            bool added;

            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key1, value1)), true, out added);
            //^ assert added;
            this.count = 1;
            this.elems = newElems;
        }

        /// <summary>
        /// Creates a map from the keys and values given as arguments.
        /// </summary>
        /// <param name="key1">The first key</param>
        /// <param name="value1">The value corresponding to <paramref name="key1"/></param>
        /// <param name="key2">The second key</param>
        /// <param name="value2">The value corresponding to <paramref name="key2"/></param>
        public Map(T key1, S value1, T key2, S value2)
        {
            LobTree<Maplet>/*?*/ newElems = null;
            bool added;
            int newCount = 1;

            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key1, value1)), true, out added);
            //^ assert added;
            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key2, value2)), true, out added);
            if (added) newCount += 1;

            this.count = newCount;
            this.elems = newElems;
        }

        /// <summary>
        /// Creates a map from the keys and values given as arguments.
        /// </summary>
        /// <param name="key1">The first key</param>
        /// <param name="value1">The value corresponding to <paramref name="key1"/></param>
        /// <param name="key2">The second key</param>
        /// <param name="value2">The value corresponding to <paramref name="key2"/></param>
        /// <param name="key3">The third key</param>
        /// <param name="value3">The value corresponding to <paramref name="key3"/></param>
        public Map(T key1, S value1, T key2, S value2, T key3, S value3)
        {
            LobTree<Maplet>/*?*/ newElems = null;
            bool added;
            int newCount = 1;

            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key1, value1)), true, out added);
            //^ assert added;
            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key2, value2)), true, out added);
            if (added) newCount += 1;
            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key3, value3)), true, out added);
            if (added) newCount += 1;
            this.count = newCount;
            this.elems = newElems;
        }

        /// <summary>
        /// Creates a map from the keys and values given as arguments.
        /// </summary>
        /// <param name="key1">The first key</param>
        /// <param name="value1">The value corresponding to <paramref name="key1"/></param>
        /// <param name="key2">The second key</param>
        /// <param name="value2">The value corresponding to <paramref name="key2"/></param>
        /// <param name="key3">The third key</param>
        /// <param name="value3">The value corresponding to <paramref name="key3"/></param>
        /// <param name="key4">The fourth key</param>
        /// <param name="value4">The value corresponding to <paramref name="key4"/></param>
        public Map(T key1, S value1, T key2, S value2, T key3, S value3, T key4, S value4)
        {
            LobTree<Maplet>/*?*/ newElems = null;
            bool added;
            int newCount = 1;

            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key1, value1)), true, out added);
            //^ assert added;
            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key2, value2)), true, out added);
            if (added) newCount += 1;
            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key3, value3)), true, out added);
            if (added) newCount += 1;
            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key4, value4)), true, out added);
            if (added) newCount += 1;
            this.count = newCount;
            this.elems = newElems;
        }

        /// <summary>
        /// Creates a map from the keys and values given as arguments.
        /// </summary>
        /// <param name="key1">The first key</param>
        /// <param name="value1">The value corresponding to <paramref name="key1"/></param>
        /// <param name="key2">The second key</param>
        /// <param name="value2">The value corresponding to <paramref name="key2"/></param>
        /// <param name="key3">The third key</param>
        /// <param name="value3">The value corresponding to <paramref name="key3"/></param>
        /// <param name="key4">The fourth key</param>
        /// <param name="value4">The value corresponding to <paramref name="key4"/></param>
        /// <param name="key5">The fifth key</param>
        /// <param name="value5">The value corresponding to <paramref name="key5"/></param>
        public Map(T key1, S value1, T key2, S value2, T key3, S value3, T key4, S value4, T key5, S value5)
        {
            LobTree<Maplet>/*?*/ newElems = null;
            bool added;
            int newCount = 1;

            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key1, value1)), true, out added);
            //^ assert added;
            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key2, value2)), true, out added);
            if (added) newCount += 1;
            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key3, value3)), true, out added);
            if (added) newCount += 1;
            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key4, value4)), true, out added);
            if (added) newCount += 1;
            newElems = LobTree<Maplet>.Insert(newElems, new Maplet(new Pair<T, S>(key5, value5)), true, out added);
            if (added) newCount += 1;
            this.count = newCount;
            this.elems = newElems;
        }

        /// <summary>
        /// Creates a map from pairs of keys and values
        /// </summary>
        /// <param name="keyValuePairs">An enumeration of key/value pairs</param>
        /// <remarks>
        /// Construct map with each key value pairs. If duplicates exist,
        /// the last key-value pair is used.
        /// <para>Note: this constructor is intended to allow the creation of map from 
        /// Dictionary, Map or other enumerator of key value pairs. Typical use would 
        /// be to use a Dictionary as a "map builder" and then create
        /// the map from the dictionary.</para>
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Map(IEnumerable<Pair<T, S>> keyValuePairs)
        {
            LobTree<Maplet>/*?*/ newElems = null;
            int newCount = 0;

            foreach (Pair<T, S> d in keyValuePairs)
            {
                bool added;
                newElems = LobTree<Maplet>.Insert(newElems, new Maplet(d), true, out added);
                if (added) newCount += 1;
            }

            this.count = newCount;
            this.elems = newElems;
        }

        /// <summary>
        /// Create a map with a <c>params</c> array of key/value pairs
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <remarks>
        /// Construct map with each key value pair. If duplicates exist,
        /// the last key-value pair is used.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Map(params Pair<T, S>[] keyValuePairs)
        {
            LobTree<Maplet>/*?*/ newElems = null;
            int newCount = 0;

            foreach (Pair<T, S> d in keyValuePairs)
            {
                bool added;
                newElems = LobTree<Maplet>.Insert(newElems, new Maplet(d), true, out added);
                if (added) newCount += 1;
            }

            this.count = newCount;
            this.elems = newElems;
        }

        internal static IComparable ConstructValue(Sequence<IComparable> args)
        {
            Map<T, S> result = EmptyMap;
            IEnumerator<IComparable> e = args.GetEnumerator();

            while (e.MoveNext())
            {
                T/*?*/ key = (T)e.Current;
                e.MoveNext();
                S/*?*/ value = (S)e.Current;

                result = result.Add(key, value);
            }
            return result;
        }

        #endregion

        #region CollectionValue Member Overrides
        /// <summary>
        /// Returns the Gets the number of key-and-value pairs contained in the map
        /// </summary>
        public override int Count
        {
            //^ [Pure]
            get { return count; }
        }

        /// <summary>
        /// Tests whether the given key-value pair is found in the map.
        /// <note>See <see cref="ContainsKey" /> instead if you want to check if a given key is in the map.</note>
        /// </summary>
        /// <param name="item">The item to find</param>
        /// <returns>True, if the <paramref name="item"/> is in this collection value, false otherwise.</returns>
        /// <remarks>
        /// Complexity: O(log(this.Count))
        /// </remarks>
        public override bool Contains(Pair<T, S> item)
        {
            S/*?*/ val;
            bool found = TryGetValue(item.First, out val);
            return found && Object.Equals(val, item.Second);
        }


        /// <summary>
        /// Returns a collection value of the same type (set, map, etc.) as this collection value. The result contains
        /// the result of applying the given converter function to each value of this collection.
        /// </summary>
        /// <param name="converter">The converter function to be applied</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Map<P, Q> Convert<P, Q>(Converter<Pair<T, S>, Pair<P, Q>> converter) where P : IComparable where Q : IComparable
        {
            Map<P, Q> result = new Map<P, Q>();
            foreach (Pair<T, S> kv in this)
            {
                Pair<P, Q> convertedKeyValue = converter(kv);
                Q val;
                bool found = result.TryGetValue(convertedKeyValue.First, out val);
                if (found && !Object.Equals(val, convertedKeyValue.Second))
                    throw new ArgumentException(MessageStrings.MapDomainErrorOnConvert);
                else
                    result = result.Override(convertedKeyValue);
            }
            return result;
        }

        /// <summary>
        /// Applies <paramref name="selector"/> to each key-value pair of this map and collects all values where 
        /// the selector function returns true.
        /// </summary>
        /// <param name="selector">A Boolean-valued delegate that acts as the inclusion test. True means
        /// include; false means exclude.</param>
        /// <returns>The map of all key-value pairs of this map that satisfy the <paramref name="selector"/></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Map<T, S> Select(Predicate<Pair<T, S>> selector)
        {
            Map<T, S> result = new Map<T, S>();
            foreach (Pair<T, S> keyValue in this)
                if (selector(keyValue)) result = result.Add(keyValue);
            return result;
        }

        /// <summary>
        /// Enumerates the key-value pairs in this map
        /// </summary>
        /// <returns>The enumerator of key-value pairs</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public override IEnumerator<Pair<T, S>> GetEnumerator()
        {
            if (this.elems != null)
                foreach (Maplet m in elems)
                    yield return m.d;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The set of keys in the map (i.e., the domain of the map)
        /// </summary>
        public Set<T>/*!*/ Keys
        {
            get
            {
                if (null != this.keyCache)
                    return this.keyCache;
                else if (this.elems != null)
                {
                    Set<T> result = Set<T>.EmptySet;
                    foreach (Maplet m in this.elems)
                        result = result.Add(m.d.First);
                    this.keyCache = result;
                    return result;
                }
                else
                    return Set<T>.EmptySet;
            }
        }

        /// <summary>
        /// The set of values in the map (i.e., the range of the map). 
        /// </summary>
        /// <remarks>The number of elements enumerated is the number of unique values. This may be 
        /// less than the number of key/value pairs found in the map if some values appear more than once.
        /// </remarks>
        public Set<S>/*!*/ Values
        {
            get
            {
                if (null != this.valuesCache)
                    return this.valuesCache;
                else if (this.elems != null)
                {
                    Set<S> result = Set<S>.EmptySet;
                    foreach (Maplet m in this.elems)
                        result = result.Add(m.d.Second);
                    this.valuesCache = result;
                    return result;
                }
                else
                    return Set<S>.EmptySet;
            }
        }
        #endregion

        #region Equality and Submap Methods

        /// <summary>
        /// Is this a submap of t? That is, is every key-value pair of s also found in t?
        /// </summary>
        /// <param name="t">The map to be tested with respect to this map</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public bool IsSubmapOf(Map<T, S> t)
        {
            Map<T, S> s = this;
            if ((object)s.elems == null) 
                return true;
            if ((object)t.elems == null) 
                return false;

            foreach (Maplet o in s.elems)
            {
                Maplet/*?*/ p;
                bool foundp = t.elems.TryGetValue(o, out p);                
                if (!foundp || foundp && !Object.Equals(p.d.Second, o.d.Second))
                    return false;
            }
            return true;
        }
        
        #endregion

        #region Lookup Operations
        /// <summary>
        /// Looks up a value associated with a given key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value associated with this key (out parameter), or the default value if not found</param>
        /// <returns>True if there a value associated with the key was found, false otherwise.</returns>
        public bool TryGetValue(T key, out S value)
        {
            Maplet/*?*/ p = null;
            bool foundp = ((object)elems == null ? false : elems.TryGetValue(new Maplet(new Pair<T, S>(key, default(S))), out p));
            value = foundp ? p.d.Second : default(S);
            return foundp;
        }

        /// <summary>
        /// Get returns the value of the provided key stored in the map, provided it exists, otherwise it abrupts; 
        /// Set includes/overrides the key-value pair in the map [Time: log(this.Count)]
        /// </summary>
        public S this[T/*!*/ key]
        {
            get
            {
                S/*?*/ result;
                bool foundp = TryGetValue(key, out result);
                if (foundp)
                    return result;
                else
                    throw new ArgumentException(MessageStrings.LocalizedFormat(MessageStrings.MapKeyNotFound, key.ToString()));
            }
        }
        #endregion

        #region Functional Add-Remove Methods
        /// <summary>
        /// Produces a map that contains all the key/value pairs of this map, plus the given key/value pair.
        /// </summary>
        /// <param name="key">The key to be added. An exception will be thrown if this map has this key.</param>
        /// <param name="value">The value to be associated with the key in the result map</param>
        /// <returns>A map containing the key/value pairs of this map, plus the given key/value pair</returns>
        public Map<T, S> Add(T/*!*/ key, S value)
        {
            Map<T, S> result = new Map<T, S>(this.count, this.elems);
            result.InPlaceAdd(key, value);
            return result;
        }

        /// <summary>
        /// Produces a map that contains all the key/value pairs of this map, plus the given key/value pair.
        /// </summary>
        /// <param name="d">The key-value pair to be added</param>
        /// <returns>A map containing the key/value pairs of this map, plus the given key/value pair</returns>
        /// <exception cref="ArgumentException">Thrown if the key already has a value in the map.</exception>
        public Map<T, S> Add(Pair<T, S> d)
        {
            Map<T, S> result = new Map<T, S>(this.count, this.elems);
            result.InPlaceAdd(d);
            return result;
        }

        /// <summary>
        /// Adds an element with the specified key and value into the map
        /// </summary>
        public Map<T, S> Override(T key, S value)
        {
            Map<T, S> result = new Map<T, S>(this.count, this.elems);
            result.InPlaceOverride(new Pair<T, S>(key, value));
            return result;
        }

        /// <summary>
        /// Map override. Returns the map that contains all key-value pairs of <paramref name="s"/>
        /// and those key-value pairs of <paramref name="this"/> for which there is no corresponding
        /// key in <paramref name="s"/>. In other words, this operation combines <paramref name="this"/>
        /// and <paramref name="s"/> in a way that gives priority to <paramref name="s"/>.
        /// </summary>
        /// <param name="s">The map containing the override values.</param>
        /// <returns>A new map with overridden values.</returns>
        public Map<T, S> Override(Map<T, S> s)
        {
            // return this + s;
            Map<T, S> result = new Map<T, S>(this.count, this.elems);
            foreach (Pair<T, S> keyValue in s)
                result.InPlaceOverride(keyValue);
            return result;
        }

        /// <summary>
        /// Map override. Returns the map that contains all key-value pairs of <paramref name="this"/>
        /// along with key-value pair <paramref name="d"/>, except that if the key of <paramref name="d"/>
        /// is in <paramref name="this"/>, the value of <paramref name="d"/> will replace (i.e., override)
        /// the corresponding value of <paramref name="this"/>.
        /// </summary>
        /// <param name="d">The key-value pair to add.</param>
        /// <returns>A new map with overridden values.</returns>
        public Map<T, S> Override(Pair<T, S> d)
        {
            Map<T, S> result = new Map<T, S>(this.count, this.elems);
            result.InPlaceOverride(d);
            return result;
        }

        /// <summary>
        /// Map override. Returns the mapt that contains all key-value pairs of <paramref name="this"/>,
        /// except that the value for key <paramref name="key"/> will be substituted by the value of
        /// <paramref name="updateFunction"/> applied to the old value associated with <paramref name="key"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="updateFunction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if this map does not contain key <paramref name="key"/>.</exception>
        public Map<T, S> Override(T key, Filter<S> updateFunction)
        {
            S oldValue;
            if (this.TryGetValue(key, out oldValue))
            {
                return this.Override(key, updateFunction(oldValue));
            }
            else
            {
                throw new ArgumentException("NModel.Map.Override: key not found");
            }
        }

        /// <summary>
        /// Removes the element with the specified key from the map
        /// </summary>
        public Map<T, S> RemoveKey(T/*!*/ key)
        {
            Map<T, S> result = new Map<T, S>(this.count, this.elems);
            result.InPlaceRemoveKey(key);
            return result;
        }

        /// <summary>
        /// Removes the key-value pair from the map. If the key is present but is
        /// associated with a different value, then an exception is thrown.
        /// </summary>
        /*internal*/
        public Map<T, S> Remove(Pair<T, S> d)
        {
            Map<T, S> result = new Map<T, S>(this.count, this.elems);
            S m2Value;
            bool found = this.TryGetValue(d.First, out m2Value);
            if (found && !Object.Equals(d.Second, m2Value))
            {
                throw new ArgumentException(MessageStrings.MapRemoveInvalidArgument);
            }
            else if (found)
            {
                result.InPlaceRemove(d);
            }
            return result;
        }
        #endregion

        #region In-place Add-Remove Methods
        /// <summary>
        /// Adds an element with the specified key and value into the map [Time: log(this.Count)]
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")] // want to use "found" var
        private void InPlaceAdd(T key, S value)
        {
            bool added;
            LobTree<Maplet> newElems = LobTree<Maplet>.Insert(elems, new Maplet(new Pair<T, S>(key, value)), true, out added);

            if (!added)
            {
                S previousValue;
                bool found = this.TryGetValue(key, out previousValue);
                //^ assume found;
                if (!Object.Equals(value, previousValue))
                    throw new ArgumentException(MessageStrings.LocalizedFormat(MessageStrings.MapAddInvalidArgument, key.ToString()));
            }
            else
            {
                this.count += 1;
            }
            this.elems = newElems;
            this.InvalidateCache();
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")] // want to use "found" var
        private void InPlaceAdd(Pair<T, S> d)
        {
            bool added;
            LobTree<Maplet> newElems = LobTree<Maplet>.Insert(elems, new Maplet(d), true, out added);

            if (!added)
            {
                S previousValue;
                T key = d.First;
                bool found = this.TryGetValue(key, out previousValue);
                //^ assume found;
                if (!Object.Equals(d.Second, previousValue))
                    throw new ArgumentException(MessageStrings.LocalizedFormat(MessageStrings.MapAddInvalidArgument, key.ToString()));
            }
            else
            {
                this.count += 1;
            }
            this.elems = newElems;
            this.InvalidateCache();
        }

        //private void InPlaceAdd(Maplet/*!*/ m)
        //{
        //    bool added;
        //    this.elems = LobTree<Maplet>.Insert(elems, m, true, out added);
        //    hashCode = null;
        //    if (added)
        //        count++;
        //    else
        //    {
        //        throw new Exception(String.Format("Map.Add: Duplicate key '{0}'", (/*^(!)^*/m.d.First).ToString()));
        //    }
        //}
        /// <summary>
        /// Adds an element with the specified key and value into the map [Time: log(this.Count)]
        /// </summary>
        private void InPlaceOverride(Pair<T, S> d)
        {
            bool added;
            this.elems = LobTree<Maplet>.Insert(elems, new Maplet(d), true, out added);
            this.InvalidateCache();
            if (added)
                count += 1;
        }

        /// <summary>
        /// Removes the element with the specified key from the map
        /// </summary>
        private void InPlaceRemoveKey(T/*!*/ key)
        {
            bool deleted;
            this.elems = LobTree<Maplet>.Remove(elems, new Maplet(new Pair<T, S>(key, default(S))), out deleted);
            this.InvalidateCache();
            if (deleted)
                count -= 1;
        }

        /// <summary>
        /// Removes the key-value pair from the map
        /// </summary>
        /*internal*/
        private void InPlaceRemove(Pair<T, S> d)
        {
            bool deleted = false;
            this.elems = LobTree<Maplet>.Remove(elems, new Maplet(d), out deleted);
            this.InvalidateCache();
            if (deleted)
                count -= 1;
        }

        #endregion


        //ToString - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Returns the map formatted in the form "Map(key_1 -> value_1, ..., key_n -> value_n)" 
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Map(");
            bool isFirst = true;
            if (this.elems != null)
                foreach (Maplet keyValue in this.elems)
                {
                    if (!isFirst) sb.Append(", ");
                    // sb.Append("(");
                    PrettyPrinter.Format(sb, keyValue.d.First);
                    sb.Append(" -> ");
                    PrettyPrinter.Format(sb, keyValue.d.Second);
                    // sb.Append(")");
                    isFirst = false;
                }
            sb.Append(")");
            return sb.ToString();
        } 
        

        // Comparison operations - - - - - - - - - - - - - - - - - - - - - - -

        /// <summary>
        /// True, if the map contains a specific key, false otherwise [Time: log(this.Count)]
        /// </summary>
        public bool ContainsKey(T/*?*/ key)
        {
            S/*?*/ value;
            return TryGetValue(key, out value);
        }


        //Ordinary map operations- - - - - - - - - - - - - - - - - - - - - - - - 

        /// <summary>
        /// Map merge. Returns a map that contains every key-value pair 
        /// of map <paramref name="s"/> and map <paramref name="t"/>, 
        /// provided that for all shared keys the values agree.
        /// </summary>
        /// <param name="s">The first map</param>
        /// <param name="t">The second map</param>
        /// <returns>A map containing the merged key-value pairs</returns>
        /// <exception cref="ArgumentException">Thrown if map <paramref name="s"/> and map <paramref name="t"/> 
        /// have shared keys with inconsistent values.</exception>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        // satisfied with Map.Override
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        [SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
        public static Map<T, S> operator +(Map<T, S> s, Map<T, S> t)
        {
            if ((object)s == null) throw new ArgumentNullException("s");
            if ((object)t == null) throw new ArgumentNullException("t");
            return s.Override(t);
        }

        /// <summary>
        /// Map merge. Returns a map that contains every key-value pair 
        /// of <paramref name="this"/> and map <paramref name="t"/>, 
        /// provided that for all shared keys the values agree.
        /// </summary>
        /// <param name="t">The second map</param>
        /// <returns>A map containing the merged key-value pairs</returns>
        /// <exception cref="ArgumentException">Thrown if this map and map <paramref name="t"/> have 
        /// shared keys with inconsistent values.</exception>
        public Map<T, S> Merge(Map<T, S> t)
        {
            Map<T, S> m1 = this.count > t.count ? this : t;
            Map<T, S> m2 = this.count > t.count ? t : this;
            Map<T, S> r = new Map<T, S>(m1.count, m1.elems);
            foreach (Pair<T, S> o in m2)
            {
                S m1Value;
                bool found = m1.TryGetValue(o.First, out m1Value);
                if (found && !Object.Equals(o.Second, m1Value))
                {
                    throw new ArgumentException(MessageStrings.MergeInvalidArgument);
                }
                else
                {
                    r.InPlaceAdd(o);
                }
            }
            return r;
        }

        /// <summary>
        /// Map intersection
        /// </summary>
        /// <param name="t">The second map</param>
        /// <returns>A map containing only those key/value pairs that occur in this map and <paramref name="t"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if this map and map <paramref name="t"/> have 
        /// shared keys with inconsistent values.</exception>
        public Map<T, S> Intersect(Map<T, S> t)
        {
            Map<T, S> m1 = this.count < t.count ? this : t;
            Map<T, S> m2 = this.count < t.count ? t : this;
            Map<T, S> r = new Map<T, S>(m1.count, m1.elems);
            foreach (Pair<T, S> o in m1)
            {
                S m2Value;
                bool found = m2.TryGetValue(o.First, out m2Value);
                if (found && !Object.Equals(o.Second, m2Value))
                {
                    throw new ArgumentException(MessageStrings.MapIntersectInvalidArgument);
                }
                else if (!found)
                {
                    r.InPlaceRemove(o);
                }
            }
            return r;
        }

        /// <summary>
        /// Map difference
        /// </summary>
        /// <param name="t">The map whose elements will be not present in the result</param>
        /// <returns>The map containing all the key/value pairs of this map, except the key/value 
        /// pairs given in the map provided as an argument.</returns>
        /// <exception cref="ArgumentException">Thrown if this map and map <paramref name="t"/> have 
        /// shared keys with inconsistent values.</exception>
        public Map<T, S> Difference(Map<T, S> t)
        {
            Map<T, S> r = new Map<T, S>(this.count, this.elems);
            foreach (Pair<T, S> o in t)
            {
                S m2Value;
                bool found = this.TryGetValue(o.First, out m2Value);
                if (found && !Object.Equals(o.Second, m2Value))
                {
                    throw new ArgumentException(MessageStrings.MapDifferenceInvalidArgument);
                }
                else if (found)
                {
                    r.InPlaceRemove(o);
                }
            }
            return r;
        }



        /// <summary>
        /// Returns a map consisting of the elements in map whose key is not in keys
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public Map<T, S> RestrictKeys(IEnumerable<T> keys)
        {
            Map<T, S> map = this;
            Map<T, S> r = new Map<T, S>(map.count, map.elems); 
            foreach (T o in keys)
                r.InPlaceRemoveKey(o);
            return r;
        }

        /// <summary>
        /// Returns a map consisting of the elements in map whose value is not in values
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public Map<T, S> RestrictValues(IEnumerable<S> values)
        {
            Map<T, S> map = this;
            Set<S> s = new Set<S>(values);
            Map<T, S> r = new Map<T, S>();
            foreach (Pair<T, S> d in map)
                if (!s.Contains(d.Second))
                    r.InPlaceAdd(d);
            return r;
        }

        /// <summary>
        /// Map difference
        /// </summary>
        /// <param name="s">The map whose elements will be present in the result</param>
        /// <param name="t">The map whose elements will be not present in the result</param>
        /// <returns>The map containing all the key/value pairs of this map, except the key/value pairs given in the map provided as an argument.</returns>
        /// <exception cref="ArgumentException">Thrown if map <paramref name="s"/> and map <paramref name="t"/> 
        /// have shared keys with inconsistent values.</exception>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
        // CA2225 satisified with "Difference()" method
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static Map<T, S> operator -(Map<T, S> s, Map<T, S> t)
        {
            if ((object)s == null) throw new ArgumentNullException("s");
            if ((object)t == null) throw new ArgumentNullException("t");
            return s.Difference(t);
        }

        /// <summary>
        /// Map intersection
        /// </summary>
        /// <param name="s">The first map</param>
        /// <param name="t">The second map</param>
        /// <returns>A map containing only those key/value pairs that occur in map <paramref name="s"/> and map <paramref name="t"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if map <paramref name="s"/> and map <paramref name="t"/> 
        /// have shared keys with inconsistent values.</exception>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        // CA2225 satisified with "Intersect()" method
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static Map<T, S> operator *(Map<T, S> s, Map<T, S> t)
        {

            if ((object)s == null) throw new ArgumentNullException("s");
            if ((object)t == null) throw new ArgumentNullException("t");
            return s.Intersect(t);
        }

        /// <summary>
        /// Distributed Merge: Returns the mapping that is constructed by merging all the mappings in s.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Map<T, S> BigMerge(IEnumerable<Map<T, S>> s)
        {
            if (null == s) throw new ArgumentNullException("s");

            Map<T, S> r = new Map<T, S>(); int i = 0;
            foreach (Map<T, S> ks in s)
            {
                if (i == 0)
                    r = ks;
                else
                    r = r.Merge(ks);
                i++;
            }
            return r;
        }
        /// <summary>
        /// Distributed Override: Returns the mapping that is constructed by overriding all the mappings in s.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Map<T, S> BigOverride(IEnumerable<Map<T, S>> s)
        {
            if (null == s) throw new ArgumentNullException("s");

            Map<T, S> r = new Map<T, S>(); int i = 0;
            foreach (Map<T, S> ks in s)
            {
                if (i == 0)
                    r = ks;
                else
                    r = r + ks;
                i++;
            }
            return r;
        }


        #region CompoundValue Overrides

        /// <summary>
        /// Returns an enumeration of the field values of map. This
        /// is the count of the collection followed by the keys and values
        /// of the map in alternating order.
        /// <note>This is a pure function and has the further requirement that
        /// its value must always be the same regardless of the context in which it is invoked. 
        /// In other words, no state update may change the values returned by this
        /// enumeration.</note>
        /// <note>This is provided to make equality testing and hashing efficient. The values returned
        /// may be encodings of the internal data structures used to implement the collection
        /// value; other accessors such as <see cref="GetEnumerator" /> provide enumeration
        /// capabilities for general use.</note>
        /// </summary>
        /// <returns>An enumeration of the field values of this map.</returns>
        override public IEnumerable<IComparable> FieldValues()
        {
            if (this.elems != null)
              foreach (Maplet m in this.elems)
                {
                    yield return m.d.First;
                    yield return m.d.Second;
                }
        }
        #endregion

        /// <exclude />
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Map<T,S> other = obj as Map<T,S>;
            if ((object)other == null) return false;
            if (this.Count != other.Count) return false;
            return base.Equals(obj);
        }

        /// <summary>
        /// The hash code as calculated in the base class.
        /// </summary>
        /// <returns>The hash code (either from the cache or calculated)</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }


}
