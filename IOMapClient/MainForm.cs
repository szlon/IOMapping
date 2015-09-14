using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Net;
using NetLib;
using Lon.Common;
using Util;


namespace IOMapClient
{
    public partial class MainForm : Form
    {
        PortData portData = new PortData();

        public bool IsShowInfo = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["ShowInfo"]);

        public string ServerIP = System.Configuration.ConfigurationManager.AppSettings["ServerIP"];
        public int ServerPort = int.Parse(System.Configuration.ConfigurationManager.AppSettings["ServerPort"]);

        public MainForm()
        {
   
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            Init();
            TcpStart();
            base.OnLoad(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            TcpClose();
            portData.CloseAll();
            base.OnClosed(e);
        }

        void Init()
        {
            InitLed();
            ppTcpReceiver.FrameReceived = OnFrameSplit;
      
            dataQueue = new DataQueue<byte[]>();
            dataQueue.ActionReceive = QueueReceive;
            dataQueue.ActionSend = QueueSend;
            dataQueue.Start();

            portData.OnReceiveData += new EventHandler<IOLib.ReceiveEventArgs>(portData_OnReceiveData);
            portData.LoadConfig_SerialPort();
            portData.OpenAll();

            List<string> valueList = portData.GetSerialPortStatus();

            SafeOutText(string.Format("{0}", DateTime.Now));
            SafeOutText("===========================================");
            foreach (string item in valueList)
            {
                SafeOutText(item);
            }
            SafeOutText("===========================================");
            

        }

        void portData_OnReceiveData(object sender, IOLib.ReceiveEventArgs e)
        {
            SerialPort serialPort = sender as SerialPort;
            SendFrame(serialPort.PortName, e.Buffer);
        }

        #region GUI显示

        public void SafeOutText(string text)
        {
            //输出数据
            try
            {
                if (txtInfo.InvokeRequired)
                {
                    txtInfo.Invoke(new SetTextCallback(SafeOutText), new object[] { text });
                }
                else
                {
                    OutText(text);
                }
            }
            catch
            {
            }
        }

        delegate void SetTextCallback(string text);

        private void OutText(string text)
        {
            //信息区文本
            txtInfo.AppendText(text + "\r\n");
            txtInfo.ScrollToCaret();
            
            if (txtInfo.Lines.Length > 1000)
            {
                txtInfo.Clear();
            }

        }

        public static string ByteToHexStr(byte[] da)
        {
            string s = "";
            for (int i = 0; i < da.Length; i++)
            {
                s += Convert.ToString(da[i], 16).PadLeft(2, '0') + " ";
            }
            return s;
        }


        #endregion

        #region  分割网络数据包
        PPFrame ppTcpReceiver = new PPFrame();

        void OnFrameSplit(byte[] buffer)
        {
            //收到分割好的数据帧加入接收队列
            Console.WriteLine("OnFrameSplit: " + buffer.Length);
            dataQueue.EnqueueReceive(buffer);
        }

        #endregion

        #region 数据队列
        DataQueue<byte[]> dataQueue;

        void QueueReceive(byte[] buffer)
        {
            //处理收到的数据
            Console.WriteLine("QueueReceive: " + buffer.Length);

            ReceivedFrame(buffer);
            rxLed.Blink();   
        }

        void QueueSend(byte[] buffer)
        {
            //处理要发送数据      
            tcpClient.Send(buffer);
            txLed.Blink();
        }

        #endregion

        #region 网络数据
        ClientSocket tcpClient = null;

        public void TcpStart()
        {                
            //客户端模式
            tcpClient = new ClientSocket(ServerIP, ServerPort);
            tcpClient.OnSocketReceived += new SocketReceivedHandler(tcpClient_OnSocketReceived);
            tcpClient.OnSocketState += new SocketStateHandler(tcpClient_OnSocketState);
            tcpClient.Start();
        }

        void tcpClient_OnSocketState(object sender, SocketState state)
        {
            string stateText = (state == SocketState.Connected ? "成功" : "失败");
            statusLabel1.Text = string.Format("连接服务器[ {0}：{1}] {2}!", ServerIP, ServerPort, stateText);

            if (state == SocketState.Connected)
            {
                SafeOutText(string.Format("{0:G}\r\n连接服务器[ {1}: {2}] {3}!\r\n", DateTime.Now, ServerIP, ServerPort, stateText));
            }
        }

        void tcpClient_OnSocketReceived(object sender, byte[] buffer)
        {
            //收到tcp数据，将数据分割后进入接收队列
            Console.WriteLine("OnSocketReceived: " + buffer.Length);

            ppTcpReceiver.PushData(buffer); 
        }

        public void TcpClose()
        {
            tcpClient.Stop();
        }


        #endregion

        #region 收发数据

        void SendFrame(string portName, byte[] buffer)
        {
            //数据区： 串口名（6B，不足以空白字节补充） + buffer(nB)

            portName = portData.GetRemotePortName(portName);    //将本地端口转换成远程端口

            if (portName.Length < 6)
            {
                portName = portName.PadRight(6, ' ');
            }

            byte[] dataBuffer = new byte[6 + buffer.Length];
            byte[] nameBytes = Encoding.ASCII.GetBytes(portName);

            Array.Copy(nameBytes, 0, dataBuffer, 0, nameBytes.Length);
            Array.Copy(buffer, 0, dataBuffer, 6, buffer.Length);
            
            byte[] rawData = PPFrame.ToFrame(dataBuffer);

            if (IsShowInfo)
            {
                SafeOutText(string.Format("[TX] {0:G}, {1}\r\n{2}\r\n", DateTime.Now, portName, DataConvert.ByteToHexStr(rawData)));
            }

            dataQueue.EnqueueSend(rawData); //将数据加入发送队列

        }

        void ReceivedFrame(byte[] rawData)
        {
            //收到TCP数据后，将其发送到对应的串口

            if (rawData == null || rawData.Length < 6) return;

            byte[] frameBuffer = PPFrame.DeFrame(rawData);

            byte[] bufffer = new byte[frameBuffer.Length - 6];
            Array.Copy(frameBuffer, 6, bufffer, 0, bufffer.Length);
            string portName = Encoding.ASCII.GetString(frameBuffer, 0, 6).TrimEnd();

            if (IsShowInfo)
            {
                SafeOutText(string.Format("[RX] {0:G}, {1}\r\n{2}\r\n", DateTime.Now, portName, DataConvert.ByteToHexStr(rawData)));
            }

            portName = portData.GetLocalPortName(portName);     //将远程端口转换为本地端口

            portData.Send(portName, bufffer);

        }

        #endregion

        #region LED

        LedLamp txLed, rxLed;
        void InitLed()
        {
            txLed = new LedLamp(this.picTxLED);
            rxLed = new LedLamp(this.picRxLED);
        }

        class LedLamp
        {
            Control ledCanvas;
            Color LampColor;
            public LedLamp(Control value)
            {
                this.ledCanvas = value;
                this.LampColor = this.ledCanvas.BackColor;
            }

            bool status = false;
            public bool Status
            {
                get
                {
                    return status;
                }

                set
                {
                    if (status != value)
                    {
                        this.status = value;
                        DrawStatus(value);
                    }
                }
            }

            public void Blink()
            {
                status = !status;
                DrawStatus(status);
            }

            void DrawStatus(bool value)
            {
                if (value)
                {
                    ledCanvas.BackColor = LampColor;
                }
                else
                {
                    ledCanvas.BackColor = Color.Black;
                }


                Application.DoEvents();

            }

        }

        #endregion
    }
}
