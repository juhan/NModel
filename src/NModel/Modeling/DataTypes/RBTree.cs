//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using SC = System.Collections;
using System.Diagnostics.CodeAnalysis;
using NModel.Internals;

namespace NModel.Internals
{
    /// <summary>
    /// <para>Symmetric binary B-tree. This is a  nearly-balanced tree that uses 
    /// an extra bit per node to maintain balance. No leaf is more than twice as far from the 
    /// root as any other. An r-b tree with n internal nodes has height at most 2 * log2(n+1). 
    /// This implementation provides the tree as an immutable value type, in the style of functional programming.
    /// </para>
    /// <para>Insertion and lookup are O(log2(n)). The IComparable interface is used for ordering.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The datatype of each node in the tree.</typeparam> 
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    internal sealed class RedBlackTree<T> : CollectionValue<T> 
    {
        private enum Color { R, B };

        Color color;
        T value;
        RedBlackTree<T>/*?*/ left, right;

        public bool InvariantHolds() { return true; }
         

        private RedBlackTree(Color c, T e, RedBlackTree<T>/*?*/ l, RedBlackTree<T>/*?*/ r)
        {
            color = c; value = e; left = l; right = r;
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static RedBlackTree<T>/*?*/ Remove(RedBlackTree<T>/*?*/ t, T/*?*/ o, out bool deleted)
        {
            RedBlackTree<T>/*?*/ root = RemoveHelper(t, o, out deleted);
            if (root != null)
                root.color = Color.B;
            return root;
        }
        
        private static RedBlackTree<T>/*?*/ RemoveHelper(RedBlackTree<T>/*?*/ t, T/*?*/ o, out bool deleted)
        {
            if ((object)t == null)
            {
                deleted = false;
                return t;
            }
            else
            {
                int k = HashAlgorithms.CompareValues(o, t.value);
                if (k == 0)
                {
                    deleted = true;
                    if (t.left == null)
                        return t.right;
                    else if (t.right == null)
                        return t.left;
                    else
                        return Relink(t.left, t.right);
                }
                else if (k < 0)
                    return Balance(t.color, t.value, RemoveHelper(t.left, o, out deleted), t.right);
                else
                    return Balance(t.color, t.value, t.left, RemoveHelper(t.right, o, out deleted));
            }
        }

        
        private static RedBlackTree<T> Relink(RedBlackTree<T> t, RedBlackTree<T>/*?*/ s)
        {
            if ((object)s == null)
                return t;
            else if (HashAlgorithms.CompareValues(t.value, s.value) < 0)
                return Balance(s.color, s.value, Relink(t, s.left), s.right);
            else
                return Balance(s.color, s.value, s.left, Relink(t, s.right));
        }


        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static RedBlackTree<T> Insert(RedBlackTree<T> t, T/*!*/ o, bool replace, out bool added)
        {
            RedBlackTree<T>/*!*/ root = InsertHelper(t, o, replace, out added);
            root.color = Color.B;
            return root;
        }

        private static RedBlackTree<T>/*!*/ InsertHelper(RedBlackTree<T>/*?*/ t, T/*?*/ o, bool replace, out bool added)
        {
            if ((object)t == null)
            {
                added = true;
                return new RedBlackTree<T>(Color.R, o, null, null);
            }
            else
            {
                int k = HashAlgorithms.CompareValues(o, t.value);
                if (k == 0)
                {
                    added = false;
                    return replace ? new RedBlackTree<T>(t.color, o, t.left, t.right) : t;
                }
                else if (k < 0)
                {
                    return Balance(t.color, t.value, InsertHelper(t.left, o, replace, out added), t.right);
                }
                else
                {
                    return Balance(t.color, t.value, t.left, InsertHelper(t.right, o, replace, out added));
                }
            }
        }

        private static RedBlackTree<T>/*!*/ Balance(Color c, T e, RedBlackTree<T>/*?*/ l, RedBlackTree<T>/*?*/ r)
        {
            if (c == Color.R)
            {
                return new RedBlackTree<T>(c, e, l, r);
            }
            else
            {
                /*c == Color.Black*/
                if (l != null && l.color == Color.R && l.left != null && l.left.color == Color.R)
                {
                    RedBlackTree<T> ll = l.left;
                    RedBlackTree<T> newl = new RedBlackTree<T>(Color.B, ll.value, ll.left, ll.right);
                    RedBlackTree<T> newr = new RedBlackTree<T>(Color.B, e, l.right, r);
                    return new RedBlackTree<T>(Color.R, l.value, newl, newr);
                }
                else if (l != null && l.color == Color.R && l.right != null && l.right.color == Color.R)
                {
                    RedBlackTree<T> newl = new RedBlackTree<T>(Color.B, l.value, l.left, l.right.left);
                    RedBlackTree<T> newr = new RedBlackTree<T>(Color.B, e, l.right.right, r);
                    return new RedBlackTree<T>(Color.R, l.right.value, newl, newr);
                }
                else if (r != null && r.color == Color.R && r.left != null && r.left.color == Color.R)
                {
                    RedBlackTree<T> newl = new RedBlackTree<T>(Color.B, e, l, r.left.left);
                    RedBlackTree<T> newr = new RedBlackTree<T>(Color.B, r.value, r.left.right, r.right);
                    return new RedBlackTree<T>(Color.R, r.left.value, newl, newr);
                }
                else if (r != null && r.color == Color.R && r.right != null && r.right.color == Color.R)
                {
                    RedBlackTree<T> rr = r.right;
                    RedBlackTree<T> newl = new RedBlackTree<T>(Color.B, e, l, r.left);
                    RedBlackTree<T> newr = new RedBlackTree<T>(Color.B, rr.value, rr.left, rr.right);
                    return new RedBlackTree<T>(Color.R, r.value, newl, newr);
                }
                else
                {
                    return new RedBlackTree<T>(c, e, l, r);
                }
            }
        }

        /// <summary>
        /// Looks up a value associated with a given key
        /// </summary>
        /// <param name="o">The key</param>
        /// <param name="result">The value associated with this key (out parameter), or the default value if not found</param>
        /// <returns>True if there a value associated with the key was found, false otherwise.</returns>
        public bool TryGetValue(T/*?*/ o, out T/*?*/ result)
        {
            int k = HashAlgorithms.CompareValues(o, this.value);
            result = default(T);
            if (k == 0)
            {
                result = this.value;
                return true;
            }
            else if (k < 0)
                return ((object)this.left == null ? false : this.left.TryGetValue(o, out result));

            else //if (k > 0)
                return ((object)this.right == null ? false : this.right.TryGetValue(o, out result));
        }

        public override int GetHashCode()
        {
            
            return HashAlgorithms.CombineHashCodes(TypedHash<RedBlackTree<T>>.StaticTypeHash, 
                                                   GetHashCodeKeys().GetEnumerator());
        }

        public override string ToString()
        {
            string s = "(" + color + ", ";
            if (left != null)
                s += left.ToString() + ", ";
            else
                s += "null, ";
            s += value.ToString() + ", ";
            if (right != null)
                s += right.ToString();
            else
                s += "null";
            return s + ")";
        }

        #region IEnumerable<T> Members
 
        public override IEnumerator<T> GetEnumerator()
        {
            // left values
            if (this.left != null)
            {
                foreach (T lval in this.left)
                    yield return lval;
            }
            // current value
            yield return this.value;

            // right values
            if (this.right != null)
            {
                foreach (T rval in this.right)
                    yield return rval;
            }
        }

        #endregion

        #region IEnumerable Members

        //SC.IEnumerator SC.IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}

        #endregion

        public IEnumerable<S> GetProjectionEnumerator<S>(Converter<T, S> converter)
        {
            foreach (T val in this)
                yield return converter(val);
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
            // recurse for left values
            if (this.left != null)
            {
                foreach (int lval in this.left.GetOrderedHashCodes(hashProvider))
                    yield return lval;
            }

            yield return hashProvider(this.value);

            // right values
            if (this.right != null)
            {
                foreach (int rval in this.right.GetOrderedHashCodes(hashProvider))
                    yield return rval;
            }
        }

        /// <summary>
        /// Returns a stream of hash codes for each element in the tree. Used for hashing. 
        /// </summary>
        /// <returns></returns>
        private IEnumerable<int> GetHashCodeKeys()
        {
            // recurse for left values
            if (this.left != null)
               foreach (int lval in this.left.GetHashCodeKeys())
                    yield return lval;
     
            // return the key 
            yield return this.value.GetHashCode();

            // right values
            if (this.right != null)
                foreach (int rval in this.right.GetHashCodeKeys())
                    yield return rval;           
        }

        /// <summary>
        /// Returns the number of elements in the collection value.
        /// </summary>
        public override int Count
        {
            get 
            { 
                int count = 0;
                IEnumerator<T> e = GetEnumerator();
                while(e.MoveNext())
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
        public override bool Contains(T/*?*/ item)
        {
            T/*?*/ val;
            return TryGetValue(item, out val);
        }
        
        //public RedBlackTree<S> Convert<S>(Converter<T, S> converter) 
        //{
        //    throw new NotImplementedException();
        //}
    }

}
