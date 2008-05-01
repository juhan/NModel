//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NModel.Algorithms
{
    /// <summary>
    /// Priority queue of keys with given values
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class PriorityQueue<TKey, TValue>
    {
        private readonly List<KeyValuePair<TKey, TValue>> heap = new List<KeyValuePair<TKey, TValue>>();
        private readonly Dictionary<TKey, int> indexes = new Dictionary<TKey, int>();

        private readonly IComparer<TValue> comparer;
        private readonly bool invert;

        
        //^ [Microsoft.Contracts.NotDelayed]
        /// <summary>
        /// Constructs an empty priority queue
        /// </summary>
        public PriorityQueue()
            : this(false)
        {
        }

        //^ [Microsoft.Contracts.NotDelayed]
        /// <summary>
        /// Constructs an empty priority queue with a boolean indicating whether 
        /// the priority order is inverted
        /// </summary>
        /// <param name="invert"></param>
        public PriorityQueue(bool invert)
            : this(/*^(!)^*/ Comparer<TValue>.Default)
        {
            this.invert = invert;
        }

        //^ [Microsoft.Contracts.NotDelayed]
        /// <summary>
        /// Constructs a priority queue with the given comparer
        /// </summary>
        public PriorityQueue(IComparer<TValue> comparer)
        {
            this.comparer = comparer;
            //^ base();
            heap.Add(default(KeyValuePair<TKey, TValue>));
        }

        /// <summary>
        /// Enqueue a new element into the queue with the given priority
        /// </summary>
        public void Enqueue(TKey item, TValue priority)
        {
            KeyValuePair<TKey, TValue> tail = new KeyValuePair<TKey, TValue>(item, priority);
            heap.Add(tail);

            MoveUp(tail, Count);
        }

        /// <summary>
        /// Dequeue an element with highest priority from the queue
        /// </summary>
        public KeyValuePair<TKey, TValue> Dequeue()
        {
            int bound = Count;
            if (bound < 1)
                throw new InvalidOperationException("Queue is empty.");

            KeyValuePair<TKey, TValue> head = heap[1];
            KeyValuePair<TKey, TValue> tail = heap[bound];

            heap.RemoveAt(bound);

            if (bound > 1)
                MoveDown(tail, 1);

            indexes.Remove(head.Key);

            return head;
        }

        /// <summary>
        /// Peek what is the next element in the queue
        /// </summary>
        public KeyValuePair<TKey, TValue> Peek()
        {
            if (Count < 1)
                throw new InvalidOperationException("Queue is empty.");

            return heap[1];
        }

        /// <summary>
        /// Try to get the priority of the given element
        /// </summary>
        public bool TryGetValue(TKey item, out TValue priority)
        {
            int index;
            if (indexes.TryGetValue(item, out index))
            {
                priority = heap[indexes[item]].Value;
                return true;
            }
            else
            {
                priority = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Get the value associated with the given key
        /// </summary>
        public TValue this[TKey item]
        {
            get
            {
                return heap[indexes[item]].Value;
            }
            set
            {
                int index;

                if (indexes.TryGetValue(item, out index))
                {
                    int order = comparer.Compare(value, heap[index].Value);
                    if (order != 0)
                    {
                        if (invert)
                            order = ~order;

                        KeyValuePair<TKey, TValue> element = new KeyValuePair<TKey, TValue>(item, value);
                        if (order < 0)
                            MoveUp(element, index);
                        else
                            MoveDown(element, index);
                    }
                }
                else
                {
                    KeyValuePair<TKey, TValue> element = new KeyValuePair<TKey, TValue>(item, value);
                    heap.Add(element);

                    MoveUp(element, Count);
                }
            }
        }

        /// <summary>
        /// The number of elements
        /// </summary>
        public int Count
        {
            get
            {
                return heap.Count - 1;
            }
        }

        private void MoveUp(KeyValuePair<TKey, TValue> element, int index)
        {
            while (index > 1)
            {
                int parent = index >> 1;

                if (IsPrior(heap[parent], element))
                    break;

                heap[index] = heap[parent];
                indexes[heap[parent].Key] = index;

                index = parent;
            }

            heap[index] = element;
            indexes[element.Key] = index;
        }

        private void MoveDown(KeyValuePair<TKey, TValue> element, int index)
        {
            int count = heap.Count;

            while (index << 1 < count)
            {
                int child = index << 1;
                int sibling = child | 1;

                if (sibling < count && IsPrior(heap[sibling], heap[child]))
                    child = sibling;

                if (IsPrior(element, heap[child]))
                    break;

                heap[index] = heap[child];
                indexes[heap[child].Key] = index;

                index = child;
            }

            heap[index] = element;
            indexes[element.Key] = index;
        }

        private bool IsPrior(KeyValuePair<TKey, TValue> element1, KeyValuePair<TKey, TValue> element2)
        {
            int order =  comparer.Compare(element1.Value, element2.Value); 
            if (invert)
                order = ~order;
            return order < 0;
        }
    }
}
