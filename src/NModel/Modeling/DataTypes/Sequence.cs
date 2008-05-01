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
    /// Immutable type representing an ordered collection of (possibly repeating) values
     /// </summary>
    /// <remarks>
    /// Sequences contain indexable elements. Sequences are similar to ArrayLists, but unlike ArrayLists, they are immutable.
    /// Sequences are implmented as double linked list (concatenation to the beginning or end is constant time)
    /// Lookup is linear time; if possible, callers should use foreach(T val in sequence) ... instead of 
    /// for(int i = 0; i &lt; sequence.Count; i += 1) { ... sequence[i] ... }
    /// </remarks>
   
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed class Sequence<T> : CollectionValue<T> where T : IComparable
    {
        int count;
        DoubleLinkedNode/*?*/ head, last;

        //^ invariant count > 0 ==> head != null;
        //^ invariant count > 0 ==> tail != null;

        private class DoubleLinkedNode
        {
            internal T elem;
            internal DoubleLinkedNode/*?*/ next, prev;
            internal DoubleLinkedNode(T e) { elem = e; }
            internal DoubleLinkedNode(T e, DoubleLinkedNode/*?*/ prev) { this.elem = e; this.prev = prev; }
        }

        /// <summary>
        /// Constructs an empty sequence
        /// </summary>
        public Sequence() { }

        /// <summary>
        /// The empty set of sort T. Note: If S and T are different types,
        /// Object.Equals(Sequence&lt;T&gt;.EmptySequence, Sequence&lt;S&gt;.EmptySequence) == false.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Sequence<T> EmptySequence = new Sequence<T>();

        /// <summary>
        /// Constructs a sequence with given elements
        /// </summary>
        public Sequence(IEnumerable<T> elems)
        {
            DoubleLinkedNode/*?*/ current = null;
            DoubleLinkedNode/*?*/ prev = null;
            DoubleLinkedNode/*?*/ head = null;
            int cnt = 0;
            foreach (T val in elems)
            {
                cnt += 1;
                current = new DoubleLinkedNode(val, current);
                if (prev != null) prev.next = current;
                if (head == null) head = current;
                prev = current;
            }
            this.count = cnt;
            this.head = head;
            this.last = prev;
        }

        /// <summary>
        /// Constructs a sequence with given elements
        /// </summary>
        /// <param name="elems"></param>
        public Sequence(params T[] elems)
        {
            DoubleLinkedNode/*?*/ current = null;
            DoubleLinkedNode/*?*/ prev = null;
            DoubleLinkedNode/*?*/ head = null;
            int cnt = 0;
            foreach (T val in elems)
            {
                cnt += 1;
                current = new DoubleLinkedNode(val, current);
                if (prev != null) prev.next = current;
                if (head == null) head = current;
                prev = current;
            }
            this.count = cnt;
            this.head = head;
            this.last = prev;
        }

        internal static IComparable ConstructValue(Sequence<IComparable> args)
        {
            Sequence<T> result = EmptySequence;
            foreach (IComparable arg in args)
            {
                T val = (T)arg;
                result = result.AddLast(val);
            }
            return result;
        }

        /// <summary>
        /// Appends element to front of a sequence
        /// </summary>
        /// <param name="value">The element to append</param>
        /// <returns>A new sequence with element appended at the front</returns>
        public Sequence<T> AddFirst(T value)
        {
            return (new Sequence<T>(value)).Concatentate(this); 
        }

        /// <summary>
        /// Appends element to end of a sequence
        /// </summary>
        /// <param name="value">The element to append</param>
        /// <returns>A new sequence with element appended at the end</returns>
        public Sequence<T> AddLast(T value)
        {
            return this.Concatentate(new Sequence<T>(value));
        }
 
        /// <summary>
        /// Gets the number of elements actually contained in the sequence.
        /// </summary>
        public override int Count
        {
            //^ [Pure]
            get { return count; }
        }

        /// <summary>
        /// Select an arbitrary value from the sequence, with external choice.
        /// </summary>
        /// <param name="i">An externally chosen integer in the interval [0, this.Count).</param>
        /// <returns>An element of the sequence.</returns>
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
                return this[i];
            }

            throw new ArgumentException(MessageStrings.ChooseInvalidArgument);
        }

        private Sequence(DoubleLinkedNode head, DoubleLinkedNode last, int count)
        {
            this.head = head; this.last = last; this.count = count;
        }


        //IList - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        /// <summary>
        /// Gets or sets the element at the specified index [Time: this.Count/2]
        /// </summary>
        public T this[int index]
        {
            get
            //^ requires index > 0 && index < Count;
            {
                if (index < 0 || index >= this.count)
                    throw new ArgumentException(MessageStrings.SequenceIndexOutOfRange);
                if (index < count / 2)                           // was (index > count / 2)
                {
                    DoubleLinkedNode cur = head;
                    for (int i = 0; i < index; i++)
                        cur = cur.next;
                    return cur.elem;
                }
                else
                {
                    DoubleLinkedNode cur = last;
                    for (int i = count - 1; i > index; i--)
                        cur = cur.prev;
                    return cur.elem;
                }
            }
        }

        /// <summary>
        /// Tests whether the given element is found in the sequence.
        /// </summary>
        /// <param name="item">The item to find</param>
        /// <returns>True, if the <paramref name="item"/> is in this sequence, false otherwise.</returns>
        /// <remarks>
        /// Complexity: O(this.Count)
        /// </remarks>
        public override bool Contains(T item)
        {
            if (count == 0) return false;
            DoubleLinkedNode cur = head;
            for (int i = 0; i < count; i++)
                if (Object.Equals(item, cur.elem))
                    return true;
                else
                    cur = cur.next;
            return false;
        }

        //public int Add(object o){ return _seq.Add(o);}
        //public bool IsReadOnly{ get {return _seq.IsReadOnly; } }
        //public bool IsFixedSize { get {return _seq.IsFixedSize; }}

        /// <summary>
        /// Returns the zero-based index of the first occurrence of the given object in the sequence, -1 if it doesn't occur.
        /// </summary>
        /// <param name="o">The item to be bound</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="o"/></returns>
        /// <remarks>[Complexity: O(this.Count)]</remarks>
        public int IndexOf(T o)
        {
            DoubleLinkedNode cur = this.head;
            for (int i = 0; i < this.count; i++)
                if (Object.Equals(o, cur.elem))
                    return i;
                else
                    cur = cur.next;
            return -1;
        }

        /// <summary>
        /// Returns the zero-based index of the last occurrence of the given object in the Vector, -1 if it doesn't occur (linear time).
        /// </summary>
        public int LastIndexOf(T o)
        {
            DoubleLinkedNode cur = last;
            for (int i = count - 1; i >= 0; i--)
                if (Object.Equals(o, cur.elem))
                    return i;
                else
                    cur = cur.prev;
            return -1;
        }
         
        /// <summary>
        /// String representation of a sequence
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Sequence(");
                      
            if (this.count > 0)
            {
                //^ assume head != null;
                DoubleLinkedNode cur = head;
                int i = 0;
                bool isFirst = true;
                while (i < this.count)
                {
                    if (!isFirst) sb.Append(", ");
                    PrettyPrinter.Format(sb, cur.elem);
                    isFirst = false;
                    cur = cur.next;
                    i += 1;
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// Returns true if the given sequence is a prefix of this sequence
        /// </summary>
        public bool IsPrefixOf(Sequence<T> t)
        {
            Sequence<T> s = this;
            DoubleLinkedNode sCur = s.head, tCur = t.head;
            int i;

            for (i = 0; i < s.count && i < t.count; i++)
            {
                if (!Object.Equals(sCur.elem, tCur.elem))
                    return false;
                else
                {
                    sCur = sCur.next;
                    tCur = tCur.next;
                }
            }
            return (i == s.count);
        }
        
        /// <summary>
        /// Return the first element of the seq
        /// </summary>
        public T Head
        {
            get
            //^ requires Count > 0;
            {
                if (count < 1) throw new ArgumentException(MessageStrings.CantTakeHeadOfEmptySequence);
                return head.elem;
            }
        }
        /// <summary>
        /// Return the last element of the seq
        /// </summary>
        public T Last
        {
            get
            //^ requires Count > 0;
            {
                if (count < 1) throw new ArgumentException(MessageStrings.CantTakeTailOfEmptySequence);
                return last.elem;
            }
        }

        /// <summary>
        /// Return the subsequence of the seq where the first element is removed
        /// </summary>
        public Sequence<T> Tail
        {
            get
            //^ requires Count > 0;
            {
                if (this.count < 1) throw new ArgumentException(MessageStrings.NonEmptySequenceRequired);
                return new Sequence<T>(head.next, last, count - 1);
            }
        }
        /// <summary>
        /// Return the subsequence of the seq where the last element is removed
        /// </summary>
        public Sequence<T> Front
        {
            get
            //^ requires Count > 0;
            {
                if (count < 1) throw new ArgumentException(MessageStrings.CantTakeHeadOfEmptySequence);
                return new Sequence<T>(head, last.prev, count - 1);
            }
        }

        /// <summary>
        /// Distributed Concatenation: Returns the sequence where the elements of s 
        /// (these are sequences themselves) are appended, the first, the second and so on
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static Sequence<T> BigConcatenate(IEnumerable<Sequence<T>> s)
        {            
            Sequence<T> r = new Sequence<T>();
            foreach (Sequence<T> subseq in s)
                r = r.Concatentate(subseq);
            return r;
        }

        /// <summary>
        /// Returns the sequence where the elements of s are in reverse order, 
        /// i.e. the last becomes the first, the second last becomes the second, ans so on
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public Sequence<T> Reverse()
        {
            Sequence<T> s = this;
            if (s.count == 0) return s;
            DoubleLinkedNode cur = s.last;
            Sequence<T> r = new Sequence<T>();
            for (int i = 0; i < s.count; i++)
            {
                r.InplaceAdd(new DoubleLinkedNode(cur.elem));
                cur = cur.prev;
            }
            return r;
        }
        private Sequence<T> Dup()
        {
            Sequence<T> r = new Sequence<T>();
            DoubleLinkedNode cur = this.head;
            for (int i = 0; i < count; i++)
            {
                r.InplaceAdd(new DoubleLinkedNode(cur.elem));
                cur = cur.next;
            }
            return r;
        }

        /// <summary>
        /// Concatenates the given sequence t at the end of this sequence
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Sequence<T> Concatentate(Sequence<T> t)
        {
            if (t == null) throw new ArgumentNullException("t");
            Sequence<T> s = this;
            if (s.count == 0) return t;
            if (t.count == 0) return s;
            if (s.last.next != null)
                s = s.Dup();
            if (t.head.prev != null)
                t = t.Dup();
            Sequence<T> r = new Sequence<T>(s.head, t.last, s.count + t.count);
            s.last.next = t.head;
            t.head.prev = s.last;
            return r;
        }

        /// <summary>
        /// Append: Returns the sequence consistsing of the eleents of s followed by those of t in order.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        // CA2225 satisified with "Concatenate()" method
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        [SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
        public static Sequence<T> operator +(Sequence<T> s, Sequence<T> t)
        {
            if (s == null) throw new ArgumentNullException("s");
            return s.Concatentate(t);
        }

        /// <summary>
        /// Adds the object to the sequence [Time: constant]
        /// </summary>
        private int InplaceAdd(T o)
        {
            if (count == 0 || last.next == null)
            {
                InplaceAdd(new DoubleLinkedNode(o));
            }
            else
            {
                Sequence<T> s = this.Dup();
                s.InplaceAdd(new DoubleLinkedNode(o));
                this.count = s.count;
                this.head = s.head;
                this.last = s.last;
            }
            return this.count;
        }
        private void InplaceAdd(DoubleLinkedNode/*!*/ cur)
        {
            if (count == 0)
            {
                head = cur;
                last = cur;
                count = 1;
            }
            else
            {
                last.next = cur;
                cur.prev = last;
                last = cur;
                count++;
            }
        }
        /// <summary>
        /// Removes the first occurrence of a specific object from the sequence [Time this.Count]
        /// </summary>
        public Sequence<T> Remove(T o)
        {
            Sequence<T> r = new Sequence<T>();
            DoubleLinkedNode cur = this.head;
            int i = 0;
            for (; i < count; i++)
            {
                if (!Object.Equals(cur.elem, o))
                {
                    r.InplaceAdd(new DoubleLinkedNode(cur.elem));
                    cur = cur.next;
                }
                else
                    break;
            }
            if (i != count)
            {
                cur = cur.next; i++;
                for (; i < count; i++)
                {
                    r.InplaceAdd(new DoubleLinkedNode(cur.elem));
                    cur = cur.next;
                }
            }
            return r;
        }

        /// <summary>
        /// Returns this - t
        /// </summary>
        public Sequence<T> Difference(Sequence<T> t)
        {
            return this - t;
        }

        /// <summary>
        /// Returns a subsequence of s, by removing t's elements from s in order.  [Time this.Count]
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        // satisified with "Difference()" method
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        [SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
        public static Sequence<T> operator -(Sequence<T> s, Sequence<T> t)
        {
            int i = 0, j = 0;
            DoubleLinkedNode sCur = s.head, tCur = t.head;
            Sequence<T> r = new Sequence<T>();
            while (i < s.count && j < t.count)
            {
                if (Object.Equals(sCur.elem, tCur.elem))
                {
                    i++; j++;
                    sCur = sCur.next; tCur = tCur.next;
                }
                else
                {
                    r.InplaceAdd(new DoubleLinkedNode(sCur.elem));
                    i++;
                    sCur = sCur.next;
                }
            }
            return r;
        }

        /// <summary>
        /// Returns the sequence of pairs of elements from s1 and s2
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static Sequence<Pair<T,T2>> Zip<T2>(Sequence<T> s, Sequence<T2> t)  where T2 : IComparable
        {
            Sequence<T>.DoubleLinkedNode sCur = s.head;
            Sequence<T2>.DoubleLinkedNode tCur = t.head;
            int m = Math.Min(s.count, t.count);
            Sequence<Pair<T,T2>> r = new Sequence<Pair<T,T2>>();
            for (int i = 0; i < m; i++, sCur = sCur.next, tCur = tCur.next)
                r.InplaceAdd(new Sequence<Pair<T,T2>>.DoubleLinkedNode(new Pair<T, T2>(sCur.elem, tCur.elem)));
            return r;
        }
        /// <summary>
        /// Returns a pair of sequences which elements are drawn from a sequqnce of pairs 
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static Pair<Sequence<T>, Sequence<T2>> Unzip<T2>(Sequence<Pair<T, T2>> s) where T2 : IComparable
        {
            Sequence<Pair<T,T2>>.DoubleLinkedNode cur = s.head;
            Sequence<T> r = new Sequence<T>();
            Sequence<T2> t = new Sequence<T2>();
            for (int i = 0; i < s.count; i++, cur = cur.next)
            {
                //^ assume cur.elem is Pair; //TODO: provide invariants that guarantee this
                Pair<T, T2> p = (Pair<T, T2>)cur.elem;
                r.InplaceAdd(new Sequence<T>.DoubleLinkedNode(p.First));
                t.InplaceAdd(new Sequence<T2>.DoubleLinkedNode(p.Second));
            }
            return new Pair<Sequence<T>, Sequence<T2>>(r, t);
        }

        #region IEnumerable<T> Members

        /// <summary>
        /// Enumerates the elements in the sequence in the order they appear in the sequence
        /// </summary>
        public override IEnumerator<T> GetEnumerator()
        {
            DoubleLinkedNode cur = head;
            for (int i = 0; i < count; i++)
            {
                yield return cur.elem;
                cur = cur.next;
            }
        }
        #endregion


        /// <summary>
        /// Converts this sequence to a sequence of elements of type S using the given converter
        /// </summary>
        public Sequence<S> Convert<S>(Converter<T, S> converter) where S : IComparable
        {
            Sequence<S> result = new Sequence<S>();
            foreach (T val in this)
                result.InplaceAdd(converter(val));
            return result;
        }

        /// <summary>
        /// Applies <paramref name="selector"/> to each element of this sequence and collects all values,
        /// preserving sequence order, where the selector function returns true.
        /// </summary>
        /// <param name="selector">A Boolean-valued delegate that acts as the inclusion test. True means
        /// include; false means exclude.</param>
        /// <returns>The sequence of all elements from this sequence that satisfy the <paramref name="selector"/></returns>
        public Sequence<T> Select(Predicate<T> selector)
        {
            Sequence<T> result = new Sequence<T>();
            foreach (T val in this)
                if (selector(val)) result.InplaceAdd(val);
            return result;
        }

        /// <summary>
        /// Chooses an arbitrary reordering of this sequence, using <paramref name="i"/> to provide choice, based 
        /// on the ordering given by <see cref="Combinatorics.ChoosePermutation"/>.
        /// </summary>
        /// <param name="i">An arbitrary integer that will be used as a seed to select the permutation.</param>
        /// <returns>A sequence with the same elements as this sequence but in a potentially different order.</returns>
        public Sequence<T> ChoosePermutation(int i)
        {
            Sequence<T> s = this;
            int[] arr = Combinatorics.ChoosePermutation(s.Count, i);
            T[] resultOrder = new T[s.Count];
            IEnumerator<T> se = s.GetEnumerator();
            foreach (int j in arr)
            {
                //^ assume j <= 0 && j < resultOrder.Length;
                se.MoveNext();
                resultOrder[j] = se.Current;
            }
            return new Sequence<T>(resultOrder);
        }



}

}
