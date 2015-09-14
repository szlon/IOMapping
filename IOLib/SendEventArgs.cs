using System;
using System.Collections.Generic;
using System.Text;

namespace IOLib
{
    public class SendEventArgs : EventArgs
    {
        public string Text { get; private set; }
        public byte[] Buffer { get; private set; }
        public string PortName { get; set; }

        public SendEventArgs(byte[] buffer)
        {
            this.Buffer = buffer;
        }

        public SendEventArgs(string text)
        {
            this.Text = text;
        }
    }

}
