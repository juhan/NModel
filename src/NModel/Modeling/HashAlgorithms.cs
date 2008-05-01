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
    /// <para>Produces hash values with static (type-based) and dynamic (via arguments) information encoded in the hash.
    /// Calculates per-type (static) hash only once per type instantiation.</para>
    /// 
    /// <para>USAGE: Use this class to implement GetHashCode in new types.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class TypedHash<T>
    {
        static int typeHash = typeof(T).GetHashCode();

        /// <summary>
        /// Provides the static hash code of type parameter T
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        static public int StaticTypeHash
        {
            get { return typeHash; }
        }

        /// <summary>
        /// <para>Calculates the hash code of every value in the params array and combines it
        /// with the static hash of type parameter <c>T</c>.</para>
        /// </summary>
        /// <param name="array">The values to be hashed.</param>
        /// <returns>The hash value calculated.</returns>
        /// <example>
        /// In this example, the resulting hash will combine the type name <c>Foo</c> with
        /// the instantiated type <c>T</c> and the hash codes of <c>field1</c> and <c>field2</c>. This is 
        /// what is typically needed for structural equality.
        /// <code>
        /// class Foo&lt;T&gt; {                                                      
        ///   string field1;                                                  
        ///   T      field2;                                                 
        ///   public override int GetHashCode()
        ///   { 
        ///      return TypedHash&lt;Foo&lt;T&gt;&gt;.ComputeHash(field1, field2);
        ///   }         
        /// }    
        /// </code>
        /// </example>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static int ComputeHash(params object/*?*/[] array)
        {
            return ComputeEnumeratedHash(array.GetEnumerator());
        }

        /// <summary>
        /// Calculates a hash code for each element in the given collection and combines it
        /// with the static hash of type parameter T.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static int ComputeEnumeratedHash(System.Collections.IEnumerator/*?*/ values)
        {
            return HashAlgorithms.ComputeEnumeratedHash(typeHash, values);
        }
    }
    
    /// <summary>
    /// Contains a set of hash algorithms.
    /// </summary>
    public static class HashAlgorithms
    {
        /// <summary>
        /// Calculates the hash code of an object, or a default hash if obj is null
        /// </summary>
        /// <param name="obj">The object, or null</param>
        /// <returns>Hash code of object, or a default hash if null</returns>
        public static int GetHashCode(object/*?*/ obj)
        {
            return (obj == null ? -831227886 : obj.GetHashCode());
        }

        static Random globalChoiceController = new Random(typeof(HashAlgorithms).GetHashCode());

        /// <summary>
        /// The global choice controller used by the <see cref="ICollectionValue&lt;T&gt;.Choose()" /> method.
        /// </summary>
        /// <seealso cref="ICollectionValue&lt;T&gt;.Choose()" />
        public static Random GlobalChoiceController
        {
            get { return globalChoiceController; }
            set { globalChoiceController = value; }
        }

        ///// <summary>
        ///// Calculates value order (for hashing of unordered types like sets and maps)
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="value1"></param>
        ///// <param name="value2"></param>
        ///// <returns></returns>
        //public static int Compare<T>(T/*?*/ value1, T/*?*/ value2) <T>
        ////^ ensures result == 0 ==> Object.Equals(value1, value2);
        ////^ ensures result == -1 || result == 0 || result == 1;
        //{
        //    bool isNull1 = ((object)value1 == null);
        //    bool isNull2 = ((object)value2 == null);
        //    if (isNull1 && !isNull2) return -1;  // null is always the minimal element
        //    if (!isNull1 && isNull2) return 1;
        //    if (isNull1 && isNull2) return 0;
        //    return value1.CompareTo(value2);
        //}

        /// <summary>
        /// Calculates a total ordering of terms. There are eight cases:
        /// <list>
        ///     <item>The items are (pointer) equal</item>
        ///     <item>One of the values is null; null is treated as the minimal element</item>
        ///     <item>Both of the values are of type <see cref="LabeledInstance" />; comparison of labels is used</item>
        ///     <!--<item>Both of the values are of type <see cref="EnumeratedInstance" />; comparison of labels is used</item>-->
        ///     <item>For values of unequal types, we use dictionary order of type names</item>
        ///     <item>For equal types, the <see cref="IComparable" /> interface is used if available.</item>
        ///     <item>For equal types, where <c>IComparable</c>cannot be used, we check for <c>Object.Equals</c></item>
        ///     <item>As a default, in the case where none of the preceding can be used to order values, we 
        /// intern the values in a global table using <see cref="HashAlgorithms.InternObject" /> and do
        /// comparision on the integer tokens returned from the intern operation.</item>
        /// </list>
        /// </summary>
        /// <param name="value1">The first value</param>
        /// <param name="value2">The second value</param>
        /// <returns>-1 if <paramref name="value1"/> is less than <paramref name="value2"/>, 0 if equal, 1 otherwise.</returns>
        /// <remarks>
        /// One of the uses of term ordering is to disambiguate distinct items with the same hash code
        /// within the representation of unordered data types like sets.
        /// </remarks>
        // Bogus FxCop message-- complained that value1 and value2 were not checked for null; they are.
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public static int CompareValues(object/*?*/ value1, object/*?*/ value2)
        //^ ensures result == 0 <==> Object.Equals(value1, value2);
        //^ ensures result == -1 || result == 0 || result == 1;
        {
            // Case 1: objects are pointer-equal
            if ((object)value1 == value2) return 0;

            // Case 2: one of the values is null: treat null as the minimal element
            bool isNull1 = (value1 == null);
            bool isNull2 = (value2 == null);
            if (isNull1 && !isNull2) return -1;
            if (!isNull1 && isNull2) return 1;

            // Case 3: both of the values are instances of classes with user-provided labeling: use labels to compare
            LabeledInstance i1 = value1 as LabeledInstance;
            if ((object)i1 != null && value2 is LabeledInstance)
                return i1.CompareTo(value2);

            //// TODO: factor LabeledIntance and EnumeratedInstance to share a common base class
            //EnumeratedInstance e1 = value1 as EnumeratedInstance;
            //if ((object)e1 != null && value2 is EnumeratedInstance)
            //    return e1.CompareTo(value2);

            // Case 5: objects are of unequal types: do type compare in dictionary order 
            Type t1 = value1.GetType();
            Type t2 = value2.GetType();
            if (t1 != t2)
                return t1.ToString().CompareTo(t2.ToString());

            // Case 6: objects are of the same type and implement IComparable; use IComparable to order
            IComparable comparable = value1 as IComparable;
            if ((object)comparable != null)
                return comparable.CompareTo(value2);

            // Case 7: objects are types; use string compare of type names
            if (value1 is Type)
                return value1.ToString().CompareTo(value2.ToString());


            // Case 8: objects are of the same type, do not support IComparable but are Equal but not reference equal
            // This is not supported. There is no way to order instances of a class that overrides Equals but 
            // does not also implement IComparable.
            if (Object.Equals(value1, value2))
                throw new ArgumentException("Cannot apply value comparision to value of type that overrides Object.Equals but does not implement IComparable.");

            // Case 9 (default): use a table to order objects. This is introduces a memory leak if the user
            // does not explicitly call UninternObject, but it acts as an ordering of last resort
            // for types that do not override Equals and GetHashCode. It can be a source of hidden internal nondeterminism.
            // It is recommended that this feature not be used.
            int int1 = InternObject(value1);
            int int2 = InternObject(value2);
            return int1.CompareTo(int2);
        }

        // TO DO: maybe use per-type counters
        static int objectCounter = 0;
        // static int NextObjectId(object o) { return objectCounter++; }

        /// <summary>
        /// Establishes a total ordering on objects by interning them in a global table. 
        /// Used for hashing and term ordering. 
        /// </summary>
        /// <param name="o">The object to intern</param>
        /// <returns>An integer representing the value id.</returns>
        /// <seealso cref="HashAlgorithms.CompareValues"/>
        public static int InternObject(object o) 
        { 
            int result;
            if (objectIdCache.TryGetValue(o, out result))
                return result;
            else
            {
                result = objectCounter++;
                objectIdCache[o] = result;
                return result;
            }
        }

        /// <summary>
        /// Frees memory used to order objects interned via the 
        /// <seeref cref="InternObject"/> method. May only be called when
        /// <paramref name="o"/> is no longer referenced by any data structure
        /// such a set or a map that might have previously used the interning
        /// for term ordering.
        /// </summary>
        /// <param name="o"></param>
        public static void UninternObject(object o)
        {
            objectIdCache.Remove(o);
        }

        static Dictionary<object, int> objectIdCache = new Dictionary<object, int>();


        /// <summary>
        /// Calculates an order-dependent hash code of an enumeration of values.
        /// </summary>
        /// <remarks>
        /// Lookup2c hash algorithm (public domain).
        /// Ref. http://burtleburtle.net/bob/hash/index.html
        /// This is the algorithm used by the SPIN model checker for state hashing. It has the
        /// property that every output bit is dependent on every input bit and is considered
        /// to be a fast good hash.
        /// </remarks>
        /// <param name="initialValue"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int ComputeEnumeratedHash(int initialValue, System.Collections.IEnumerator/*?*/ values)
        {
            unchecked
            {
                uint a = 0x9e3779b9;            /* the golden ratio; an arbitrary value */
                uint b = 0x9e3779b9;
                uint c = (uint)initialValue;    /* the previous hash value */
                int length = 0;
                bool repeat = true;

                if (values == null) return initialValue;

                while (repeat)
                {
                    repeat = false;
                    if (values.MoveNext())
                    {
                        a += (uint)GetHashCode(values.Current);
                        length += 1;
                    }
                    if (values.MoveNext())
                    {
                        b += (uint)GetHashCode(values.Current);
                        length += 1;
                    }
                    if (values.MoveNext())
                    {
                        c += (uint)GetHashCode(values.Current);
                        length += 1;
                        repeat = true;
                    }
                    else
                    {
                        c += (uint)length * 4u;
                    }
                    a -= b; a -= c; a ^= (c >> 13);
                    b -= c; b -= a; b ^= (a << 8);
                    c -= a; c -= b; c ^= (b >> 13);
                    a -= b; a -= c; a ^= (c >> 12);
                    b -= c; b -= a; b ^= (a << 16);
                    c -= a; c -= b; c ^= (b >> 5);
                    a -= b; a -= c; a ^= (c >> 3);
                    b -= c; b -= a; b ^= (a << 10);
                    c -= a; c -= b; c ^= (b >> 15);
                }

                return (int)c;
            }
        }
        /// <summary>
        /// Calculates an order-dependent combination from an enumeration of hash codes.
        /// </summary>
        /// <param name="initialValue">The starting value of the hash</param>
        /// <param name="hashCodes">The enumerator of hash codes to be combined</param>
        /// <returns>The order-dependent combination of values</returns>
        public static int CombineHashCodes(int initialValue, IEnumerator<int> hashCodes)
        {
            unchecked
            {
                uint a = 0x9e3779b9;            /* the golden ratio; an arbitrary value */
                uint b = 0x9e3779b9;
                uint c = (uint)initialValue;    /* the previous hash value */
                int length = 0;
                bool repeat = true;

                if (hashCodes == null) return initialValue;

                while (repeat)
                {
                    repeat = false;
                    if (hashCodes.MoveNext())
                    {
                        a += (uint)(hashCodes.Current);
                        length += 1;
                    }
                    if (hashCodes.MoveNext())
                    {
                        b += (uint)(hashCodes.Current);
                        length += 1;
                    }
                    if (hashCodes.MoveNext())
                    {
                        c += (uint)(hashCodes.Current);
                        length += 1;
                        repeat = true;
                    }
                    else
                    {
                        c += (uint)length * 4u;
                    }
                    a -= b; a -= c; a ^= (c >> 13);
                    b -= c; b -= a; b ^= (a << 8);
                    c -= a; c -= b; c ^= (b >> 13);
                    a -= b; a -= c; a ^= (c >> 12);
                    b -= c; b -= a; b ^= (a << 16);
                    c -= a; c -= b; c ^= (b >> 5);
                    a -= b; a -= c; a ^= (c >> 3);
                    b -= c; b -= a; b ^= (a << 10);
                    c -= a; c -= b; c ^= (b >> 15);
                }

                return (int)c;
            }
        } 

        ///// <summary>
        ///// Combines previously calculate hash values.
        ///// </summary>
        ///// <returns>The hash value calculated.</returns>
        //public static int CombineHash(int initialValue, params int[] otherValues)
        //{
        //    return Lookup2cHash(otherValues, initialValue);
        //}
        
        ///// <summary>
        ///// Calculates the hash fingerprint of an integer array.
        ///// </summary>
        ///// <param name="k">The key.</param>
        ///// <param name="initval">The previous hash, or an arbitrary value.</param>
        ///// <returns>The hash value calculated.</returns>
        //private static int Lookup2cHash(int[] k, int initialValue)
        //{
        //    unchecked
        //    {
        //        uint initval = (uint)initialValue;
        //        uint a = 0x9e3779b9; /* the golden ratio; an arbitrary value */
        //        uint b = 0x9e3779b9;
        //        uint c = initval;    /* the previous hash value */
        //        int length = k.Length;
        //        uint len = (uint)length;
        //        int i = 0;

        //        while (len >= 3)
        //        {
        //            a += (uint)k[i];
        //            b += (uint)k[i + 1];
        //            c += (uint)k[i + 2];
        //            a -= b; a -= c; a ^= (c >> 13);
        //            b -= c; b -= a; b ^= (a << 8);
        //            c -= a; c -= b; c ^= (b >> 13);
        //            a -= b; a -= c; a ^= (c >> 12);
        //            b -= c; b -= a; b ^= (a << 16);
        //            c -= a; c -= b; c ^= (b >> 5);
        //            a -= b; a -= c; a ^= (c >> 3);
        //            b -= c; b -= a; b ^= (a << 10);
        //            c -= a; c -= b; c ^= (b >> 15);
        //            i += 3; len -= 3;
        //        }

        //        if (len > 0) a += (uint)k[i];
        //        if (len > 1) b += (uint)k[i + 1];
        //        c += (uint)length * 4u;
        //        a -= b; a -= c; a ^= (c >> 13);
        //        b -= c; b -= a; b ^= (a << 8);
        //        c -= a; c -= b; c ^= (b >> 13);
        //        a -= b; a -= c; a ^= (c >> 12);
        //        b -= c; b -= a; b ^= (a << 16);
        //        c -= a; c -= b; c ^= (b >> 5);
        //        a -= b; a -= c; a ^= (c >> 3);
        //        b -= c; b -= a; b ^= (a << 10);
        //        c -= a; c -= b; c ^= (b >> 15);

        //        return (int)c;
        //    }
        //} 

        
    }
}
