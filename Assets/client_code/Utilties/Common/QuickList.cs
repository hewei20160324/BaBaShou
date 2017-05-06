﻿
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This improved version of the System.Collections.Generic.List that doesn't release the buffer on Clear(), resulting in better performance and less garbage collection.
/// </summary>

namespace Common
{
    public class QuickList<T>
    {
        /// <summary>
        /// Direct access to the buffer. Note that you should not use its 'Length' parameter, but instead use BetterList.size.
        /// </summary>

        public T[] buffer;

        /// <summary>
        /// Direct access to the buffer's size. Note that it's only public for speed and efficiency. You shouldn't modify it.
        /// </summary>

        public int size = 0;

        public int Count
        {
            get { return size; }
        }


        public QuickList(int size)
        {
            buffer = new T[size];
        }


        public QuickList()
        {
            buffer = new T[8];
        }

        /// <summary>
        /// For 'foreach' functionality.
        /// </summary>

        public IEnumerator<T> GetEnumerator()
        {
            if (buffer != null)
            {
                for (int i = 0; i < size; ++i)
                {
                    yield return buffer[i];
                }
            }
        }

        /// <summary>
        /// Convenience function. I recommend using .buffer instead.
        /// </summary>

        public T this[int i]
        {
            get { return buffer[i]; }
            set { buffer[i] = value; }
        }

        /// <summary>
        /// Helper function that expands the size of the array, maintaining the content.
        /// </summary>

        void AllocateMore()
        {
            T[] newList = (buffer != null) ? new T[Mathf.Max(buffer.Length << 1, 32)] : new T[32];
            if (buffer != null && size > 0) buffer.CopyTo(newList, 0);
            buffer = newList;
        }

        /// <summary>
        /// Trim the unnecessary memory, resizing the buffer to be of 'Length' size.
        /// Call this function only if you are sure that the buffer won't need to resize anytime soon.
        /// </summary>

        void Trim()
        {
            if (size > 0)
            {
                if (size < buffer.Length)
                {
                    T[] newList = new T[size];
                    for (int i = 0; i < size; ++i) newList[i] = buffer[i];
                    buffer = newList;
                }
            }
            else buffer = null;
        }

        /// <summary>
        /// Clear the array by resetting its size to zero. Note that the memory is not actually released.
        /// </summary>

        public void Clear()
        {
            if (size != 0 && buffer != null)
            {
                buffer.Initialize();
            }

            size = 0;
        }

        /// <summary>
        /// Clear the array and release the used memory.
        /// </summary>

        public void Release()
        {
            size = 0;
            buffer = null;
        }

        /// <summary>
        /// Add the specified item to the end of the list.
        /// </summary>

        public void Add(T item)
        {
            if (buffer == null || size == buffer.Length) AllocateMore();
            buffer[size++] = item;
        }

        /// <summary>
        /// Insert an item at the specified index, pushing the entries back.
        /// </summary>

        public void Insert(int index, T item)
        {
            if (buffer == null || size == buffer.Length) AllocateMore();

            if (index < size)
            {
                for (int i = size; i > index; --i) buffer[i] = buffer[i - 1];
                buffer[index] = item;
                ++size;
            }
            else Add(item);
        }

        /// <summary>
        /// Remove the specified item from the list. Note that RemoveAt() is faster and is advisable if you already know the index.
        /// </summary>

        public bool Remove(T item)
        {
            if (buffer != null)
            {
                EqualityComparer<T> comp = EqualityComparer<T>.Default;

                for (int i = 0; i < size; ++i)
                {
                    if (comp.Equals(buffer[i], item))
                    {
                        --size;
                        buffer[i] = default(T);
                        for (int b = i; b < size; ++b) buffer[b] = buffer[b + 1];
                        buffer[size] = default(T);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Remove an item at the specified index.
        /// </summary>

        public void RemoveAt(int index)
        {
            if (buffer != null && index < size)
            {
                --size;
                buffer[index] = default(T);
                for (int b = index; b < size; ++b) buffer[b] = buffer[b + 1];
                buffer[size] = default(T);
            }
        }

        /// <summary>
        /// Mimic List's ToArray() functionality, except that in this case the list is resized to match the current size.
        /// </summary>

        public T[] ToArray() { Trim(); return buffer; }
    }
}