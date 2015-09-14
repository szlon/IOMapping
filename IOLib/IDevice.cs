using System;
using System.Collections.Generic;
using System.Text;

namespace IOLib
{
    public interface IDevice
    {
        PortIO Port { get; set; }
        
        void Send(string value);
        void Send(byte[] buffer);

        void OnSend(object sender, SendEventArgs e);
        void OnReceived(object sender, ReceiveEventArgs e);

    }

}
