using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

namespace IOLib
{
    public class PortSender
    {
        SerialPort serialPort;

        public event EventHandler<SendEventArgs> OnSendData;

        public PortSender(SerialPort port)
        {
            this.serialPort = port;
        }

        public void Send(byte[] buffer)
        {
            if (serialPort.IsOpen && OnSendData != null)
            {
                SendEventArgs eventArgs = new SendEventArgs(buffer);
                eventArgs.PortName = serialPort.PortName;

                OnSendData(this, eventArgs);                          
                
                serialPort.Write(buffer, 0, buffer.Length);
            }

        }

        public void Send(string value)
        {
            if (serialPort.IsOpen && OnSendData != null)
            {
                SendEventArgs eventArgs = new SendEventArgs(value);
                eventArgs.PortName = serialPort.PortName;

                OnSendData(this, eventArgs);   
            
                serialPort.Write(value);

            }

        }

    }



}
