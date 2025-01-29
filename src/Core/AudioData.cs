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