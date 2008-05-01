//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace NModel.Algorithms
{
    /// <summary>
    /// Implements a thread-safe unbounded queue of elements of type T.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class TimedQueue<T> 
    {
        /// <summary>
        /// Shared queue of T elements
        /// </summary>
        private Queue<T> queue;

        /// <summary>
        /// Construct an instance with an initially empty queue
        /// </summary>
        public TimedQueue()
        {
            this.queue = new Queue<T>();
        }

        /// <summary>
        /// Remove all entries from the queue
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                queue.Clear();
            }
        }

        /// See if there is an element in the queue.
        /// If there is one return true
        /// and put it in the out parameter elem, 
        /// otherwise return false and put default(T) in elem.
        public bool TryPeek(out T elem)
        {
            lock (this)
            {
                if (queue.Count == 0)
                {
                    elem = default(T);
                    return false;
                }
                else
                {
                    elem = queue.Peek();
                    return true;
                }
            }
        }

        /// <summary>
        /// Like TryPeek, but wait for at most 'waitAtMost' amount of time 
        /// to see if something arrives in the queue and remove it from the queue.
        /// A negative 'waitAtMost' means wait idefinitely.
        /// </summary>
        public bool TryDequeue(TimeSpan waitAtMost, out T elem)
        {
            lock (this)
            {
                if (queue.Count == 0)              //queue is empty
                {
                    //start waiting for the signal
                    //wait at most the given amount of time
                    //release the lock before going to sleep
                    Monitor.Wait(this, waitAtMost);
                }

                if (queue.Count == 0) 
                {
                    //timeout occurred, queue is still empty
                    elem = default(T);
                    return false;
                }
                else
                {
                    //the queue is nonempty or an element arrived
                    elem = queue.Dequeue();
                    return true;
                }
            }
        }

        /// <summary>
        /// Enqueue the given element.
        /// </summary>
        public void Enqueue(T elem)
        {
            lock (this)
            {
                this.queue.Enqueue(elem);
                Monitor.Pulse(this); //wake up a potential dequeuer 
            }
        }

        /// <summary>
        /// True if the queue is empty
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                lock (this)
                {
                    return queue.Count == 0;
                }
            }
        }

        /// <summary>
        /// Dequeues the first term from the queue.
        /// Requires that the queue is nonempty
        /// </summary>
        public T Dequeue()
        {
            lock (this)
            {
                if (queue.Count == 0)
                    throw new InvalidOperationException("Queue is empty and cannot be dequeued");
                return queue.Dequeue();
            }
        }
    }
}
