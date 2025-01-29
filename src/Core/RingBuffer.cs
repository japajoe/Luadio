using System;
using System.Collections.Generic;

namespace Luadio
{
    public class RingBuffer<T>
    {
        private List<T> items;
        private int maxSize;
        private int startIndex;
        private int endIndex;
        private int itemCount;

        public int Count => itemCount;
        public int Size => maxSize;

        public RingBuffer() : this(100) { }

        public RingBuffer(int maxMessages)
        {
            maxSize = maxMessages;
            items = new List<T>(new T[maxSize]);
            startIndex = 0;
            endIndex = 0;
            itemCount = 0;
        }

        public void Add(T item)
        {
            items[endIndex] = item;
            endIndex = (endIndex + 1) % maxSize;

            if (itemCount < maxSize)
            {
                itemCount++;
            }
            else
            {
                startIndex = (startIndex + 1) % maxSize;
            }
        }

        public T GetAt(int index)
        {
            if (index >= 0 && index < itemCount)
            {
                int idx = (startIndex + index) % maxSize;
                return items[idx];
            }
            throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");
        }

        public void Clear()
        {
            startIndex = 0;
            endIndex = 0;
            itemCount = 0;
        }
    }
}