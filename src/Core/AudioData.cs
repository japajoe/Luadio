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
using MathNet.Numerics;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace Luadio
{
    public sealed class AudioData
    {
        private AtomicBool isLocked;
        private float[] data;
        private float[] fftData;
        private Complex[] fft;
        private int length;
        private int fftLength;

        public int Length
        {
            get
            {
                return length;
            }
        }

        public int FFTLength
        {
            get
            {
                return fftLength;
            }
        }

        public float[] Data
        {
            get
            {
                return data;
            }
        }

        public float[] FFTData
        {
            get
            {
                return fftData;
            }
        }

        public AudioData(int bufferSize)
        {
            this.data = new float[bufferSize];
            this.length = bufferSize;
            int length = NextPowerOfTwo(bufferSize);
            fft = new Complex[length];
            fftData = new float[length / 2];
            this.fftLength = NextPowerOfTwo(length / 2);
            this.isLocked = new AtomicBool(false);
        }

        public void SetData(AudioBuffer<float> buffer)
        {
            if(isLocked.Value)
                return;

            if(buffer.Length > data.Length)
            {
                data = new float[buffer.Length];
            }

            if(buffer.Length > 0 && buffer.Length <= data.Length)
            {
                this.length = buffer.Length;
                
                for(int i = 0; i < buffer.Length; i++)
                {
                    data[i] = buffer[i];
                }
            }
        }

        public void SetDataWithFFT(AudioBuffer<float> buffer)
        {
            if(isLocked.Value)
                return;

            if(buffer.Length > data.Length)
            {
                data = new float[buffer.Length];
                int length = NextPowerOfTwo(buffer.Length);
                fft = new Complex[length];
                fftData = new float[length / 2];                
            }

            if(buffer.Length > 0 && buffer.Length <= data.Length)
            {
                this.length = buffer.Length;
                this.fftLength = NextPowerOfTwo(length / 2);
                
                for(int i = 0; i < buffer.Length; i++)
                {
                    data[i] = buffer[i];
                    fft[i] = new Complex(data[i], 0);
                }

                Fourier.Forward(fft, FourierOptions.Default);

                for (int i = 0; i < fftData.Length; i++)
                {
                    fftData[i] = (float)fft[i].Magnitude; // Get magnitude
                }
            }
        }

        public void SetLock(bool value)
        {
            isLocked.Value = value;
        }

        private int NextPowerOfTwo(int value) 
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value++;
            return value;
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