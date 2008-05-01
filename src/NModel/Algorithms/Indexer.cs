//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;

//namespace NModel.Algorithms
//{
//    /// <summary>
//    /// Provides integral indexes to unique objects.
//    /// </summary>
//    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
//    public class Indexer<T> : IEnumerable<KeyValuePair<T, int>>
//    {
//        private Dictionary<T, int> indices = new Dictionary<T, int>();
//        private List<int> set = new List<int>();
//        private int offset;

//        /// <summary>
//        /// Creates a new instance of this class where indices start from zero.
//        /// </summary>
//        public Indexer()
//        {
//        }

//        /// <summary>
//        /// Creates a new instance of this class where indices start from <paramref name="start"/>.
//        /// </summary>
//        /// <param name="start">Value to be the index of the first item.</param>
//        public Indexer(int start)
//        {
//            offset = start;
//        }

//        /// <summary>
//        /// Returns a unique index for <paramref name="item"/> unless the index has been set for the item explicitly.
//        /// </summary>
//        /// <param name="item">Item to index.</param>
//        /// <returns>Unique index of the item unless the index was set explicitly.</returns>
//        public int this[T item]
//        {
//            get
//            {
//                int index;
//                if (!indices.TryGetValue(item, out index))
//                {
//                    index = indices.Count + offset;
//                    while (set.Contains(index))
//                        index++;
//                    indices[item] = index;
//                }
//                return index;
//            }
//            set
//            {
//                set.Add(value);
//                indices[item] = value;
//            }
//        }

//        /// <summary>
//        /// Detmines whether the indexer contains an index for the given item.
//        /// </summary>
//        /// <param name="item">Item to check.</param>
//        /// <returns><c>true</c> if <paramref name="item"/> has been indexed; otherwise, false.</returns>
//        public bool Contains(T item)
//        {
//            return indices.ContainsKey(item);
//        }

//        #region IEnumerable<KeyValuePair<T,int>> Members

//        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
//        public IEnumerator<KeyValuePair<T, int>> GetEnumerator()
//        {
//            return indices.GetEnumerator();
//        }

//        #endregion

//        #region IEnumerable Members

//        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }

//        #endregion
//    }
//}
