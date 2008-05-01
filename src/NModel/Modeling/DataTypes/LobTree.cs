//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using NModel.Internals;


namespace NModel.Internals
{
    /// <summary>
    /// Low-order bit tree (LobTree) is a binary tree with self-balancing properties for hash code keys.
    /// 
    /// This implementation has a unique representation whenever the contents are equal. It uses 
    /// element ordering (from IComparable) to do this when hash codes coincide.
    /// 
    /// Trees are immutable; insertion and deletion operations return a new tree.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed class LobTree<T> : CollectionValue<T> where T : IComparable
    {
        Base representation;

        #region Constructors

        private LobTree(Base representation) { this.representation = representation; }

        /// <summary>
        /// The empty tree
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly LobTree<T> EmptyTree = new LobTree<T>(new Empty());

        #endregion

        #region CompoundValue Overrides
        /// <summary>
        /// Enumerates the field values
        /// </summary>
        public override IEnumerable<IComparable> FieldValues()
        {
            foreach (T value in this)
                yield return value;
        }
        #endregion

        #region Representation Types and Fields
        abstract private partial class Base { }

        private partial class Empty : Base { }

        private partial class Fork : Base
        {
            public Base left, right;
        }

        abstract private partial class Leaf : Base { }

        private partial class SingleNode : Leaf
        {
            public T value;
        }

        private partial class ListNode : Leaf
        {
            public T value;
            public ListNode/*?*/ next;
        }
        #endregion

        #region Representation Constructors
        partial class Base
        {
            public static Base EmptyTree = new Empty();
        }

        partial class Empty
        {
            public Empty() { }
        }

        partial class Fork : Base
        {
            public Fork(Base left, Base right)
            {
                this.left = left;
                this.right = right;
            }
        }

        partial class SingleNode
        {
            public SingleNode(T value) { this.value = value; }
        }

        partial class ListNode
        {
            public ListNode(T value, ListNode/*?*/ next)
            //^ requires next != null ? HashAlgorithms.GetHashCode(value) == HashAlgorithms.GetHashCode(next.value) : true;
            //^ requires next != null ? HashAlgorithms.CompareValues(value, next.value) == -1 : true;
            {
                this.value = value;
                this.next = next;
            }
        }

        #endregion

        #region BitManipulationUtilities
        partial class Base
        {
            // set the nth bit to 1
            protected static int SetBit(int inputBits, int offset)
            // requires 0 <= offset && offset < 32;
            {
                unchecked
                {
                    uint u = (uint)inputBits;
                    u = u | (1u << offset);
                    return (int)u;
                }
            }

            // set the nth bit to 0
            protected static int ClearBit(int inputBits, int offset)
            // requires 0 <= offset && offset < 32;
            {
                unchecked
                {
                    uint u = (uint)inputBits;
                    u = u & (0xFFFFFFFF ^ (1u << offset));
                    return (int)u;
                }
            }

            // are the n lowest-order bits the same?
            protected static bool MaskEqual(int inputBits, int key, int nBitsToTest)
            // requires 0 <= nBitsToTest && nBitsToTest < 32;
            {
                unchecked
                {
                    uint u = (uint)inputBits;
                    uint v = (uint)key;
                    for (int i = 0; i < nBitsToTest; i += 1)
                        if ((u & (1u << i)) != (v & (1u << i)))
                            return false;
                    return true;
                }
            }

            // is the nth bit set to 1?
            protected static bool BitTest(int inputBits, int offset)
            {
                //^ requires 0 <= offset && offset < 32;
                unchecked
                {
                    uint u = (uint)inputBits;
                    return !((u & (1u << offset)) == 0u);
                }
            }
        }

        #endregion

        #region Invariant

        /// <summary>
        /// The invariant is a Boolean condition that is guaranteed by the implementation to hold.
        /// 
        /// May be used for debugging and testing.
        /// </summary>
        /// <returns></returns>
        public bool InvariantHolds() 
        //^ ensures result == true;
        { 
            return this.representation.InvariantHolds(0, 0); 
        }

        partial class Base
        {
            // requires 0 <= depth && depth < 32;  // how do you constrain an abstract method in Spec#
            abstract public bool InvariantHolds(int depth, int bitsSoFar);
        }

        partial class Empty
        {
            override public bool InvariantHolds(int depth, int bitsSoFar)
            {
                return true;
            }
        }

        partial class Fork
        {
            override public bool InvariantHolds(int depth, int bitsSoFar)
            {
                return (!(this.left == Base.EmptyTree && this.right == Base.EmptyTree) &&
                         !(this.left is Leaf && this.right == Base.EmptyTree) &&
                         !(this.left == Base.EmptyTree && this.right is Leaf) &&
                        this.left != null && this.left.InvariantHolds(depth + 1, ClearBit(bitsSoFar, depth)) &&
                        this.right != null && this.right.InvariantHolds(depth + 1, SetBit(bitsSoFar, depth)));
            }
        }

        partial class SingleNode
        {
            override public bool InvariantHolds(int depth, int bitsSoFar)
            {
                return MaskEqual(bitsSoFar, HashAlgorithms.GetHashCode(this.value), depth);
            }
        }

        partial class ListNode
        {
            override public bool InvariantHolds(int depth, int bitsSoFar)
            {
                return InvariantHolds_1(depth, bitsSoFar, true);
            }

            bool InvariantHolds_1(int depth, int bitsSoFar, bool isFirst)
            {
                return MaskEqual(bitsSoFar, HashAlgorithms.GetHashCode(this.value), depth) &&
                       (this.next != null ? (HashAlgorithms.CompareValues(this.value, this.next.value) == -1) : true) &&
                       (this.next != null ? (HashAlgorithms.GetHashCode(this.value) ==
                                             HashAlgorithms.GetHashCode(this.next.value)) : true) &&
                       (this.next != null ? this.next.InvariantHolds_1(depth, bitsSoFar, false) : true) &&
                       (isFirst ? this.next != null : true);
            }
        }

        #endregion

        #region Insertion
        /// <summary>
        /// Insert an element o 
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static LobTree<T> Insert(LobTree<T>/*?*/ tree, T o, bool replace, out bool added)
        //^ ensures result.Contains(o);
        //^ ensures added <==> tree != null && result.Count == tree.Count + 1;
        //^ ensures added <==> !tree.Contains(o);
        //^ ensures !added <==> Object.Equals(tree, result);              // object equals when o is contained regardless of replace
        //^ ensures (!added && !replace) <==> ((object)tree == result);   // pointer equals iff !replace and !added
        {
            int hashKey = HashAlgorithms.GetHashCode(o);

            if ((object)tree == null)
                return new LobTree<T>(Base.EmptyTree.InsertHelper(o, hashKey, replace, out added, 0));
            else
            {
                Base result = tree.representation.InsertHelper(o, hashKey, replace, out added, 0);
                return ((object)result != (object)(tree.representation)) ? new LobTree<T>(result) : tree;
            }
        }

        partial class Base
        {
            // how do you constrain abstract methods in Spec#?

            // requires HashAlgorithms.GetHashCode(o) == hashKey;
            // requires 0 <= depth && depth <= 32;
            // ensures result.Contains(o);
            // ensures added <==> result.Count == this.Count + 1;
            // ensures added <==> !this.Contains(o);
            // ensures !added <==> this.Equals(result);                      // object equals when o is contained regardless of replace
            // ensures (!added && !replace) <==> ((object)this == result);   // pointer equals iff !replace and !added
            // ensures result.InvariantHolds(hashKey, depth);
            abstract public Base InsertHelper(T o, int hashKey, bool replace, out bool added, int depth);
        }

        partial class Empty
        {
            /// <summary>
            /// Case 1: Insert into empty tree.
            /// 
            /// Return single node containing element to be added.
            /// </summary>
            override public Base InsertHelper(T o, int hashKey, bool replace, out bool added, int depth)          
            {
                added = true;
                return new SingleNode(o);
            }
        }

        partial class Fork
        {
            /// <summary>
            /// Case 2: Insert into branch point
            /// 
            /// Insert left or right depending on the hash bit at offset "depth".
            /// If no change, return this branch. Otherwise, construct a new tree with the results.
            /// Note: the NewFork method creates the canonical form (despite the name, it isn't necessarily a fork).
            /// </summary>
            override public Base InsertHelper(T o, int hashKey, bool replace, out bool added, int depth)
            //^ requires depth < 32;
            {
                Base result;
                if (BitTest(hashKey, depth) == false)
                {
                    Base newLeft = this.left.InsertHelper(o, hashKey, replace, out added, depth + 1);
                    result = (object)newLeft != this.left ? NewFork(newLeft, this.right) : this;
                }
                else
                {
                    Base newRight = this.right.InsertHelper(o, hashKey, replace, out added, depth + 1);
                    result = (object)newRight != this.right ? NewFork(this.left, newRight) : this;
                }
                return result;
            }

        }

        partial class SingleNode
        {
            /// <summary>
            /// Case 3: Insert into a tree containing a single element.
            /// 
            /// Several subcases: 
            ///     a) insert with same hash code but unequal object (returns a list node sorted in IComparable order)
            ///     b) override existing object when "replace" is specified (returns a new single node tree)
            ///     c) don't override existing (returns this tree)
            ///     d) insert with different hash code (returns a new fork)
            ///     
            /// </summary>
            override public Base InsertHelper(T o, int hashKey, bool replace, out bool added, int depth)
            {
                int key2 = HashAlgorithms.GetHashCode(this.value);
                if (hashKey == key2)
                {
                    // Hash key of element to insert matches hash key of this node's contained element.
                    // May need to split single node into list of nodes
                    switch (HashAlgorithms.CompareValues(o, this.value))
                    {
                        case -1:
                            //^ assert !Object.Equals(o, this.value);
                            added = true;
                            return new ListNode(o, new ListNode(this.value, null));

                        case 0:
                            //^ assert Object.Equals(o, this.value);
                            added = false;
                            return replace ? new SingleNode(o) : this;

                        case 1:
                            //^ assert !Object.Equals(o, this.value);
                            added = true;
                            return new ListNode(this.value, new ListNode(o, null));

                        default:
                            // C# requires this; will never happen
                            throw new ArgumentException("o");
                    }
                }
                else
                {
                    // Hash key of element to insert does not match hash key of this node's contained element
                    // Need to split leaf node into fork
                    added = true;
                    return Fork.MakeBranch(this, key2, new SingleNode(o), hashKey, depth);

                }
            }
        }

        partial class ListNode
        {
            /// <summary>
            /// Case 4: insert into list of nodes sharing the same hash code
            /// 
            /// Several subcases: 
            ///     a) insert with same hash code but unequal object (returns a list node sorted in IComparable order)
            ///     b) override existing object when "replace" is specified (returns list node of same length as this)
            ///     c) don't override existing (returns this tree)
            ///     d) insert with different hash code (returns a new fork)
            /// </summary>
            /// <remarks>
            /// This code shares the tail memory (when insertion occurs early in the list, this saves space).
            /// This is probably overkill, since lists only occur on hash collisions and will tend to happen rarely
            /// and only contain a few elements. Consider reworking to copy lists entirely before in-place insertion
            /// to make testing easier.
            /// </remarks>
            override public Base InsertHelper(T o, int hashKey, bool replace, out bool added, int depth)
            {
                int key2 = HashAlgorithms.GetHashCode(this.value);
                if (hashKey == key2)
                {
                    // Hash key of element to insert matches hash key of this node's contained element.
                    // May need to insert new element into list of nodes (unless not replacing and element is already there)
                    ListNode/*?*/ cur = this;
                    ListNode/*?*/ copyHead = null;
                    ListNode/*?*/ copyTail = null;
                    while (cur != null)
                    {
                        switch (HashAlgorithms.CompareValues(o, cur.value))
                        {
                            case -1:
                                //^ assert !Object.Equals(o, cur.value);
                                added = true;
                                // insert before cur
                                ListNode inserted = new ListNode(o, cur);
                                if (copyHead == null)
                                {
                                    return inserted;
                                }
                                else
                                {
                                    //^ assert tail != null;
                                    copyTail.next = inserted;
                                    return copyHead;
                                }

                            case 0:
                                //^ assert Object.Equals(o, cur.value);
                                added = false;
                                if (replace)
                                {
                                    // skip cur, insert before cur.next
                                    ListNode inserted2 = new ListNode(o, cur.next);
                                    if (copyHead == null)
                                    {
                                        return inserted2;
                                    }
                                    else
                                    {
                                        //^ assert tail != null;
                                        copyTail.next = inserted2;
                                        return copyHead;
                                    }
                                }
                                else
                                {
                                    added = false;
                                    return this;
                                }


                            case 1:
                                //^ assert !Object.Equals(o, cur.value);
                                ListNode copied = new ListNode(cur.value, null);
                                if (copyHead == null) copyHead = copied;
                                if (copyTail != null) copyTail.next = copied;
                                copyTail = copied;
                                cur = cur.next;
                                break;

                        }
                    }
                    // o is greater than all elements in list; append to copied tail
                    added = true;
                    ListNode inserted3 = new ListNode(o, null);
                    //^ assert copyHead != null && copyTail != null;
                    copyTail.next = inserted3;
                    return copyHead;
                }
                else
                {
                    // Hash key of element to insert does not match hash key of this node's contained element
                    // Need to split leaf node into fork
                    added = true;
                    return Fork.MakeBranch(this, key2, new SingleNode(o), hashKey, depth);
                }
            }
        }

        partial class Fork
        {
            public static Base MakeBranch(Leaf leaf1, int hashKey1,
                                                Leaf leaf2, int hashKey2,
                                                int depth)

            //^ requires hashKey1 != hashKey2;
            //^ requires leaf1 is SingleNode ==> HashAlgorithms.GetHashCode((SingleNode)leaf1.value) == hashKey1;
            //^ requires leaf1 is ListNode ==> HashAlgorithms.GetHashCode((ListNode)leaf1.value) == hashKey1;
            //^ requires leaf2 is SingleNode ==> HashAlgorithms.GetHashCode((SingleNode)leaf2.value) == hashKey1;
            //^ requires leaf2 is ListNode ==> HashAlgorithms.GetHashCode((ListNode)leaf2.value) == hashKey1;
            //^ requires MaskEqual(hashKey1, hashKey2, depth);
            //^ requires 0 <= depth && depth < 32;
            {
                bool bit1 = BitTest(hashKey1, depth);
                bool bit2 = BitTest(hashKey2, depth);
                if (!bit1 && !bit2)
                {
                    //^ assert depth < 31;
                    return new Fork(MakeBranch(leaf1, hashKey1, leaf2, hashKey2, depth + 1), EmptyTree);
                }
                else if (!bit1 && bit2)
                    return new Fork(leaf1, leaf2);

                else if (bit1 && !bit2)
                    return new Fork(leaf2, leaf1);

                else
                {
                    //^ assert bit1 && bit2;
                    //^ assert depth < 31;
                    return new Fork(EmptyTree, MakeBranch(leaf1, hashKey1, leaf2, hashKey2, depth + 1));
                }
            }
        }

        #endregion

        #region Lookup

        /// <summary>
        /// Looks up a value associated with a given key
        /// </summary>
        /// <param name="o">The key</param>
        /// <param name="valueFound">The value associated with this key (out parameter), or the default value of type T if not found</param>
        /// <returns>True if there a value associated with the key was found, false otherwise.</returns>
        public bool TryGetValue(T o, out T/*?*/ valueFound)
        //^ ensures result <==> this.Contains(o);
        //^ ensures !result ==> valueFound == default(T);
        {
            return this.representation.TryGetValue(o, HashAlgorithms.GetHashCode(o), 0, out valueFound);
        }

        partial class Base
        {
            abstract public bool TryGetValue(T o, int hashKey, int depth, out T/*?*/ result);
        }

        partial class Empty
        {
     
            /// <summary>
            /// Case 1: Empty tree
            /// 
            /// No op, returns false;
            /// </summary>
            override public bool TryGetValue(T/*?*/ o, int hashKey, int depth, out T/*?*/ result)
            {
                result = default(T);
                return false;
            }
        }

        partial class Fork
        {
            /// <summary>
            /// Case 2: Branch point
            /// 
            /// Search left or right depending on the hash bit at offset "depth".
            /// </summary>
            override public bool TryGetValue(T/*?*/ o, int hashKey, int depth, out T/*?*/ result)
            {
                return (BitTest(hashKey, depth)) ?
                    this.right.TryGetValue(o, hashKey, depth + 1, out result) :
                    this.left.TryGetValue(o, hashKey, depth + 1, out result);
            }
        }

        partial class SingleNode
        {
            /// <summary>
            /// Case 3: Tree consisting of single node
            /// 
            /// If the object being looked for is contained, return true. Otherwise false.
            /// </summary>
            override public bool TryGetValue(T/*?*/ o, int hashKey, int depth, out T/*?*/ result)
            {
                if (Object.Equals(this.value, o))
                {
                    //^ assert HashAlgorithms.GetHashCode(this.value) == hashKey;
                    result = this.value;
                    return true;
                }
                else
                {
                    result = default(T);
                    return false;
                }
            }
        }

        partial class ListNode
        {
            /// Case 3: Tree consisting of list of nodes containing distinct elements with the same hash code
            /// 
            /// If the object being looked for is contained in the list, return true. Otherwise false.
            override public bool TryGetValue(T/*?*/ o, int hashKey, int depth, out T/*?*/ result)
            {
                ListNode/*?*/ cur = this;
                while (cur != null)
                {
                    if (Object.Equals(cur.value, o))
                    {
                        //^ assert HashAlgorithms.GetHashCode(cur.value) == hashKey;
                        result = cur.value;
                        return true;
                    }
                    cur = cur.next;
                }
                result = default(T);
                return false;
            }
        }
        #endregion

        #region Remove

        /// <summary>
        /// Removes an element from the tree
        /// </summary>
        /// <param name="t">The tree</param>
        /// <param name="o">The element</param>
        /// <param name="deleted">Output parameter, set to true if the element was removed by the operation, false otherwise</param>
        /// <returns>New tree with element removed</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static LobTree<T>/*?*/ Remove(LobTree<T> t, T o, out bool deleted)
        //^ ensures result != null ==> !result.Contains(o);
        //^ ensures deleted ==> result != null && t != null && t.Count = result.Count + 1;
        {
            if ((object)t == null)
            {
                deleted = false;
                return null;
            }
            else
            {
                Base result = t.representation.Remove(o, HashAlgorithms.GetHashCode(o), 0, out deleted);
                return deleted ? new LobTree<T>(result) : t;
            }
        }

        partial class Base
        {
            // TODO: how do you constrain an abstract method in Spec#?

            // requires 0 <= depth && depth < 32;
            // ensures result.InvariantHolds(hashKey, depth);
            // ensures !result.Contains(o);
            // ensures deleted ==> this.Count = result.Count + 1;
            // ensures !deleted <==> (object)this == result;   // pointer equal iff not deleted
            abstract public Base Remove(T o, int hashKey, int depth, out bool deleted);
        }

        partial class Empty
        {
            /// Case 1: Empty tree
            /// 
            /// No op, returns false;
            override public Base Remove(T o, int hashKey, int depth, out bool deleted)
            {
                deleted = false;
                return this;
            }
        }
        
        partial class Base
        {
            /// <summary>
            /// Helper method used in insertion and removal.
            /// 
            /// Creates the canonical branch form (despite the name, it isn't necessarily a fork).
            /// </summary>
            public static Base NewFork(Base left, Base right)
            {
                bool leftEmpty = (left is Empty);
                bool rightEmpty = (right is Empty);
                if (leftEmpty && rightEmpty)
                    return EmptyTree;
                else if (left is Leaf && rightEmpty)
                    return left;
                else if (leftEmpty && right is Leaf)
                    return right;
                else
                    return new Fork(left, right);
            }
        }

        partial class Fork
        {
            /// <summary>
            /// Case 2: Remove from branch point
            /// 
            /// Remove left or right depending on the hash bit at offset "depth".
            /// If no change, return this branch. Otherwise, construct a new tree with the results.
            /// Note: the NewFork method creates the canonical form (despite the name, it isn't necessarily a fork).
            /// </summary>
            override public Base Remove(T o, int hashKey, int depth, out bool deleted)
            {
                //^ assert depth == 31 ==> !(this.left is Fork);
                //^ assert depth == 31 ==> !(this.right is Fork);
                Base tmp;
                if (BitTest(hashKey, depth) == false)
                {
                    tmp = this.left.Remove(o, hashKey, depth + 1, out deleted);
                    return (deleted ? NewFork(tmp, right) : this);
                }
                else
                {
                    tmp = this.right.Remove(o, hashKey, depth + 1, out deleted);
                    return (deleted ? NewFork(left, tmp) : this);
                }
            }
        }

        partial class SingleNode
        {
            /// <summary>
            /// Case 3: single node tree
            /// 
            /// Returns empty tree if element to be removed matches contained element, this tree otherwise.
            /// </summary>
            override public Base Remove(T o, int hashKey, int depth, out bool deleted)
            {
                if (Object.Equals(this.value, o))
                {
                    deleted = true;
                    return EmptyTree;
                }
                else
                {
                    deleted = false;
                    return this;
                }
            }
        }

        partial class ListNode
        {
            /// Case 3: list of nodes containing distinct elements sharing the same hash code
            /// 
            /// Returns a copy of the list without the element to removed
            override public Base Remove(T o, int hashKey, int depth, out bool deleted)
            {
                ListNode cur = this;
                ListNode/*?*/ copiedHead = null;
                ListNode/*?*/ copiedTail = null;
                while (cur != null)
                {
                    if (Object.Equals(cur.value, o))
                    {
                        deleted = true;
                        if (copiedHead == null)
                        {
                            //^ assert cur.next != null;
                            if (cur.next.next == null)
                                return new SingleNode(cur.next.value);
                            else
                                return cur.next;
                        }
                        else
                        {
                            //^ assert copiedTail != null;
                            copiedTail.next = cur.next;
                            if (copiedHead.next == null)
                                return new SingleNode(copiedHead.value);
                            else
                                return copiedHead;
                        }
                    }
                    else
                    {
                        ListNode copiedNode = new ListNode(cur.value, null);
                        if (copiedHead == null)
                        {
                            copiedHead = copiedNode;
                        }
                        else
                        {
                            //^ assert copiedTail != null;
                            copiedTail.next = copiedNode;
                        }
                        copiedTail = copiedNode;
                        cur = cur.next;
                    }
                }
                deleted = false;
                return this;
            }
        }
        #endregion

        #region Enumerators

        /// <summary>
        /// Note: elements returned in a fixed order, since tree is ordered by hash code and IComparable comparision.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<T> GetEnumerator()
        {
            LinkedList<Base> queue = new LinkedList<Base>();
            queue.AddFirst(this.representation);

            while (queue.Count > 0)
            {
                Base cur = queue.First.Value;

                queue.RemoveFirst();

                while (true)
                {
                    SingleNode singleNode = cur as SingleNode;
                    if (singleNode != null)
                    {
                        yield return singleNode.value;
                        break;
                    }      
                    else
                    {
                        Fork fork = cur as Fork;
                        if (fork != null)
                        {
                            if (fork.left == Base.EmptyTree)
                            {
                                cur = fork.right;
                            }
                            else if (fork.right == Base.EmptyTree)
                            {
                                cur = fork.left;
                            }
                            else
                            {
                                cur = fork.left;
                                queue.AddLast(fork.right);
                            }
                        }
       
                        else
                        {
                            ListNode listNode = cur as ListNode;
                            if (listNode != null)
                            {
                                ListNode curList = listNode;
                                while (curList != null)
                                {
                                    yield return curList.value;
                                    curList = curList.next;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Returns the number of elements in the collection value.
        /// </summary>
        public override int Count
        {
            get
            {
                int count = 0;
                IEnumerator<T> e = GetEnumerator();
                while (e.MoveNext())
                    count += 1;
                return count;
            }
        }

        /// <summary>
        /// Tests whether the given element is found in the collection value.
        /// </summary>
        /// <param name="item">The item to find</param>
        /// <returns>True, if the <paramref name="item"/> is in this collection value, false otherwise.</returns>
        /// <remarks>
        /// Complexity: O(log(this.Count))
        /// </remarks>
        public override bool Contains(T item) 
        {
            T/*?*/ val;
            return this.representation.TryGetValue(item, HashAlgorithms.GetHashCode(item), 0, out val);
        }

        /// <summary>
        /// Convert the elements to type S using the given converter
        /// </summary>
        public LobTree<S> Convert<S>(Converter<T, S> converter) where S : IComparable
        {
            LobTree<S> result = LobTree<S>.EmptyTree;
            foreach (T val in this)
            {
                bool added;
                result = LobTree<S>.Insert(result, converter(val), false, out added);
            }
            return result;
        }

        /// <summary>
        /// Returns an enumeration of hash codes for each key in the tree, where each element is
        /// a commutative mixing (xor) of hash codes from a user-provided hash
        /// function applied to each node with the same key. 
        /// </summary>
        /// <param name="hashProvider">The alternative hash function to be applied to nodes with the same key</param>
        /// <returns></returns>
        public IEnumerable<int> GetOrderedHashCodes(Converter<T, int> hashProvider)
        {
            foreach (T value in this)
                yield return hashProvider(value);
        }
    }

}
