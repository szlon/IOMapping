using System;
using System.Collections.Generic;
using System.Text;

namespace Util
{
    public abstract class FrameSplitterBase : IFrameSplitter
    {        
        public int RX_BUFFER_SIZE = 1024 * 100;
        public int RxBufferSize
        {
            get { return RX_BUFFER_SIZE; }
            set
            {
                if (value != RX_BUFFER_SIZE)
                {
                    RX_BUFFER_SIZE = value;
                    RxBuffer = new byte[RX_BUFFER_SIZE];
                }
            }
        }

        public Action<byte[]> FrameReceived { get; set; }

        protected byte[] RxBuffer;
        protected int RxBufferOffset;

        protected object lockObj = new object();

        public FrameSplitterBase()
        {
            RxBufferOffset = 0;
            RxBuffer = new byte[RX_BUFFER_SIZE];
        }

        public void Reset()
        {
            RxBufferOffset = 0;
        }

        public virtual void PushData(byte[] buffer)
        {
        }

    }

}
