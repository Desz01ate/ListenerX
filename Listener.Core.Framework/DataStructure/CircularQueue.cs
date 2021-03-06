﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listener.Core.Framework.DataStructure
{
    /// <summary>
    /// Fixed-size queue which automatically dequeue when elements are going to exceed the limit.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularQueue<T> : ICollection<T>
    {
        private readonly ConcurrentQueue<T> q;
        protected readonly int limit;
        public int Count => this.limit;

        public bool IsReadOnly => true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="limitSize"></param>
        public CircularQueue(int limitSize)
        {
            if (limitSize <= 0) throw new ArgumentOutOfRangeException(nameof(limitSize));
            limit = limitSize;
            q = new ConcurrentQueue<T>(new T[limit]);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="limitSize"></param>
        public CircularQueue(IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            limit = source.Count();
            q = new ConcurrentQueue<T>(source);
        }
        /// <summary>
        /// Enqueue element into the last of sequence.
        /// </summary>
        /// <param name="element">data to enqueue</param>
        public void Enqueue(T element)
        {
            if (q.Count >= limit)
            {
                q.TryDequeue(out _);
            }
            q.Enqueue(element);
        }
        /// <summary>
        /// Enqueue element into the last of sequence.
        /// </summary>
        /// <param name="element">data to enqueue</param>
        public bool TryEnqueue(T element, out T firstElement)
        {
            try
            {
                T res = default;
                if (q.Count >= limit)
                {
                    q.TryDequeue(out res);
                }
                firstElement = res;
                q.Enqueue(element);
                return true;
            }
            catch
            {
                firstElement = default;
                return false;
            }
        }
        /// <summary>
        /// Dequeue element from the start of the sequence.
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            if (q.TryDequeue(out var result))
            {
                return result;
            }
            return default;
        }
        /// <summary>
        /// return an object from the beginning of the queue
        /// without removing it.
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            if (q.TryPeek(out var result))
            {
                return result;
            }
            return default;
        }
        /// <summary>
        /// Shift all elements by 1 index to the left.
        /// </summary>
        public void ShiftLeft()
        {
            if (q.TryDequeue(out var element))
            {
                q.Enqueue(element);
            }
        }
        /// <summary>
        /// Shift all elements by 1 index to the right.
        /// </summary>
        public void ShiftRight()
        {
            for (var i = 0; i < limit - 1; i++)
            {
                ShiftLeft();
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var data in q)
            {
                yield return data;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(T item)
        {
            this.Enqueue(item);
        }

        public void Clear()
        {
            while (!q.IsEmpty)
            {
                this.q.TryDequeue(out _);
            }
        }

        public bool Contains(T item)
        {
            return this.q.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.q.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }
    }
}
