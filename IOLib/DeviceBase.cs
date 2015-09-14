using System;
using System.Collections.Generic;
using System.Text;

namespace IOLib
{
    public abstract class DeviceBase : IDevice
    {
        string deviceName = "DeviceBase";
        public string DeviceName
        {
            get { return deviceName; }
            set { deviceName = value; }
        }

        PortIO port;
        public PortIO Port
        {
            get { return port; }
            set { port = value; }
        }

        #region IDevice

        public virtual void Send(string value)
        {
            if (this.port != null)
            {
                this.port.Send(value);
            }
        }

        public virtual void Send(byte[] buffer)
        {
            if (this.port != null)
            {
                this.port.Send(buffer);
            }
        }

        public virtual void OnSend(object sender, SendEventArgs e)
        {
        }

        public virtual void OnReceived(object sender, ReceiveEventArgs e)
        {       
        }

        #endregion

    }

}
