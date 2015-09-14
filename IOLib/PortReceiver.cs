using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO.Ports;

namespace IOLib
{
    public class PortReceiver
    {
        SerialPort serialPort;
        object lockObj = new object();
        
        byte[] rxBuffer = new byte[1024 * 10];

        public event EventHandler<ReceiveEventArgs> OnReceiveData;

        public PortReceiver(SerialPort port)
        {
            this.serialPort = port;
            this.serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
        }

        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (lockObj)
            {
                if (OnReceiveData != null)
                {
                    int count = serialPort.BytesToRead;

                    serialPort.Read(rxBuffer, 0, count);
                    
                    ReceiveEventArgs eventArgs = new ReceiveEventArgs(rxBuffer, 0, count);
                    eventArgs.PortName = (sender as SerialPort).PortName;

                    OnReceiveData(this, eventArgs);
                }
            }

        }

    }



}
