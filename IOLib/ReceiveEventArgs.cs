using System;
using System.Collections.Generic;
using System.Text;

namespace IOLib
{
    public class ReceiveEventArgs : EventArgs
    {
        public byte[] Buffer { get; private set; }
        public string PortName { get; set; }

        public ReceiveEventArgs(byte[] buffer, int offset, int count)
        {
            this.Buffer = new byte[count];

            Array.Copy(buffer, offset, this.Buffer, 0, count);

        }

    }

}
