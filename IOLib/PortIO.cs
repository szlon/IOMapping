using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using Util;

namespace IOLib
{
    /// <summary>
    /// 串口数据收发器
    /// 说明：  每个串口构建一对收发线程用于处理缓存数据
    ///         收到数据：存入接收缓存，然后依次转发到所有的接收监听对象
    ///         发送数据：数据先发送到发送缓存，然后依次发送到串口
    /// </summary>
    public class PortIO
    {
        SerialPort serialPort;
        public PortSender portSender;
        public PortReceiver portReceiver;

        public PortIO(SerialPort sp)
        {
            PortInit(sp);
        }

        public PortIO(string portName, int baudRate)
        {
            PortInit(new SerialPort(portName, baudRate));
        }

        #region 数据队列
        DataQueue<EventArgsPackage> dataQueue;

        void InitDataQueue()
        {
            dataQueue = new DataQueue<EventArgsPackage>();
            dataQueue.ActionReceive = QueueReceive;
            dataQueue.ActionSend = QueueSend;
            dataQueue.Start();
        }

        void QueueReceive(EventArgsPackage e)
        {
            //处理收到的数据，将数据转发到所有的监听对象
            if (receiverListener.Values.Count <= 0) return;

            foreach (IDevice item in receiverListener.Values)
            {
                try
                {
                    item.OnReceived(e.Sender, e.Args as ReceiveEventArgs);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        void QueueSend(EventArgsPackage e)
        {
            //处理要发送数据      
            SendEventArgs args = e.Args as SendEventArgs;

            try
            {
                if (args.Buffer != null)
                {
                    this.portSender.Send(args.Buffer);
                }
                else if (!string.IsNullOrEmpty(args.Text))
                {
                    this.portSender.Send(args.Text);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        #endregion

        void PortInit(SerialPort sp)
        {
            this.serialPort = sp;
            this.portSender = new PortSender(sp);
            this.portReceiver = new PortReceiver(sp);

            this.portSender.OnSendData += new EventHandler<SendEventArgs>(portSender_OnSendData);
            this.portReceiver.OnReceiveData += new EventHandler<ReceiveEventArgs>(portReceiver_OnReceiveData);
        }

        void portSender_OnSendData(object sender, SendEventArgs e)
        {
            if (receiverListener.Values.Count <= 0) return;

            foreach (IDevice item in receiverListener.Values)
            {
                try
                {
                    item.OnSend(sender, e);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        void portReceiver_OnReceiveData(object sender, ReceiveEventArgs e)
        {
            //将收到的串口数据放入队列
            dataQueue.EnqueueReceive(new EventArgsPackage(sender, e));
        }
        

        public void Start()
        {
            InitDataQueue();
            if (!this.serialPort.IsOpen)
            {
                try
                {
                    this.serialPort.Open();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.Write(ex.Message);
                }
            }
        }

        public void Close()
        {
            dataQueue.Stop();
            if (this.serialPort.IsOpen)
            {
                try
                {
                    this.serialPort.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.Write(ex.Message);
                }
            }

        }

        public void Send(byte[] buffer)
        {
            //存储到发送缓存
            dataQueue.EnqueueSend(new EventArgsPackage(serialPort, new SendEventArgs(buffer)));
        }

        public void Send(string value)
        {
            dataQueue.EnqueueSend(new EventArgsPackage(serialPort, new SendEventArgs(value)));
        }


        Dictionary<string, IDevice> receiverListener = new Dictionary<string, IDevice>();

        public void Register(string name, IDevice item)
        {
            if (!receiverListener.ContainsKey(name))
            {
                item.Port = this;
                receiverListener.Add(name, item);
            }
        }

        public void UnRegister(string name)
        {
            if (receiverListener.ContainsKey(name))
            {
                IDevice item = receiverListener[name];
                receiverListener.Remove(name);
            }           

        }


    }

    class EventArgsPackage
    {
        public object Sender;
        public EventArgs Args;

        public EventArgsPackage(object sender, EventArgs args)
        {
            this.Sender = sender;
            this.Args = args;
        }

    }

}
