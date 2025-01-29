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

using System.Threading;
using MiniAudioEx;

namespace Luadio
{
    public sealed class AudioData
    {
        private AtomicBool isLocked;
        private float[] data;
        private int length;

        public int Length
        {
            get
            {
                return length;
            }
        }

        public float[] Data
        {
            get
            {
                return data;
            }
        }

        public AudioData(int bufferSize)
        {
            this.data = new float[bufferSize];
            this.length = bufferSize;
            this.isLocked = new AtomicBool(false);
        }

        public void SetData(AudioBuffer<float> buffer)
        {
            if(isLocked.Value)
                return;

            if(buffer.Length > data.Length)
                data = new float[buffer.Length];

            if(buffer.Length > 0 && buffer.Length <= data.Length)
            {
                this.length = buffer.Length;
                
                for(int i = 0; i < buffer.Length; i++)
                {
                    data[i] = buffer[i];
                }
            }
        }

        public void SetLock(bool value)
        {
            isLocked.Value = value;
        }
    }

    public sealed class AtomicBool
    {
        private int _value; // 0 means false, 1 means true

        public AtomicBool(bool initialValue)
        {
            _value = initialValue ? 1 : 0;
        }

        public bool Value
        {
            get { return _value == 1; }
            set { Interlocked.Exchange(ref _value, value ? 1 : 0); }
        }
    }
}