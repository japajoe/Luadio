// MIT License

// Copyright (c) 2025 W.M.R Jap-A-Joe

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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