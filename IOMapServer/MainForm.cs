using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using Lon.Common;
using System.Net;
using NetLib;
using Util;

namespace IOMapServer
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
            SafeOutText("===================================");
            foreach (string item in valueList)
            {
                SafeOutText(item);
            }
            SafeOutText("===================================");
        }

        void portData_OnReceiveData(object sender, IOLib.ReceiveEventArgs e)
        {
            //收到串口数据
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

        //使用Invoke显示UI信息
        private delegate void TreeViewUpdater(TreeView tv, List<ClientObject> clientList);

        private void ShowClientList()
        {
            if (tcpServer == null) return;
            TreeViewUpdater updateTreeView = new TreeViewUpdater(UpdateTreeView);
            tvClientList.Invoke(updateTreeView, new object[] { tvClientList, tcpServer.ClientList });
        }

        private void UpdateTreeView(TreeView tv, List<ClientObject> clientList)
        {
            tv.Nodes.Clear();

            for (int i = 0; i < clientList.Count; i++)
            {
                ClientObject clientObj = clientList[i];
                IPEndPoint iep = (IPEndPoint)clientObj.Client.RemoteEndPoint;

                TreeNode node = tv.Nodes.Add(iep.Address.ToString());
                TreeNode subNode1 = new TreeNode("Port: " + iep.Port.ToString());
                subNode1.ImageIndex = 1;
                subNode1.SelectedImageIndex = 2;

                node.Nodes.Add(subNode1);

            }
        }
        #endregion

        #region  分割网络数据包

        PPFrame ppTcpReceiver = new PPFrame();

        void OnFrameSplit(byte[] buffer)
        {
            //收到分割好的数据帧加入接收队列
            dataQueue.EnqueueReceive(buffer);
        }


        #endregion

        #region 数据队列
        DataQueue<byte[]> dataQueue;

        void QueueReceive(byte[] buffer)
        {
            //处理收到的数据
            ReceivedFrame(buffer);
            rxLed.Blink();            
        }

        void QueueSend(byte[] buffer)
        {
            //处理要发送数据       
            tcpServer.BroadcastData(ClientLevel.None, buffer);
            txLed.Blink();
        }
        
        #endregion

        #region 网络数据
        TcpServer tcpServer;
        Thread tcpThread;

        public void TcpStart()
        {
            tcpServer = new TcpServer(ServerIP, ServerPort);
            tcpThread = new Thread(new ThreadStart(tcpServer.StartListening));

            tcpServer.OnClientConnect += new DataEventHandler(tcpServer_OnClientConnect);
            tcpServer.OnClientDisconnect += new DataEventHandler(tcpServer_OnClientDisconnect);
            tcpServer.OnServerFull += new DataEventHandler(tcpServer_OnServerFull);
            tcpServer.OnClientDataAvailable += new DataEventHandler(tcpServer_OnClientDataAvailable);
            tcpThread.Start();

            while (!tcpThread.IsAlive) ;

            SafeOutText(string.Format("{0:G}\r\nTCP服务器( {0}: {1} )已启动!\r\n", DateTime.Now, ServerIP, ServerPort));

        }

        public void TcpClose()
        {
            //TCP停止
            if (tcpServer != null)
            {
                tcpThread.Abort();
                tcpServer.Close();
            }

        }

        void tcpServer_OnClientDataAvailable(object sender, DataEventArgs e)
        {
            //收到tcp数据，将数据分割后进入接收队列
            ppTcpReceiver.PushData(e.Data);                       
        }

        void tcpServer_OnServerFull(object sender, DataEventArgs e)
        {
            //TCP服务器连接已满
            SafeOutText("[" + DateTime.Now.ToString() + "]");
            SafeOutText("[TCP]服务器连接已满!");
            SafeOutText("");
        }

        void tcpServer_OnClientDisconnect(object sender, DataEventArgs e)
        {
            //TCP客户端断开
            IPEndPoint iep = ((IPEndPoint)e.Client.RemoteEndPoint);

            SafeOutText("[" + DateTime.Now.ToString() + "]");
            SafeOutText("[TCP] " + iep.Address.ToString() + ":" + iep.Port.ToString() + "断开! ("
                + tcpServer.ClientList.Count.ToString() + "/" + tcpServer.MaxClient.ToString() + ")");
            SafeOutText("");

            //更新用户连接列表
            ShowClientList();
        }

        void tcpServer_OnClientConnect(object sender, DataEventArgs e)
        {
            //TCP客户端连接
            IPEndPoint iep = ((IPEndPoint)e.Client.RemoteEndPoint);

            SafeOutText("[" + DateTime.Now.ToString() + "]");
            SafeOutText("[TCP] " + iep.Address.ToString() + ":" + iep.Port.ToString() + "连接! ("
                + tcpServer.ClientList.Count.ToString() + "/" + tcpServer.MaxClient.ToString() + ")");
            SafeOutText("");

            //更新用户连接列表
            ShowClientList();
        }

        #endregion

        #region 收发数据
        void SendFrame(string portName, byte[] buffer)
        {
            //数据区： 串口名（6B，不足以空白字节补充） + buffer(nB)

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
            //收到数据后，将其发送到对应的串口
            if (rawData == null || rawData.Length < 6) return;

            byte[] frameBuffer = PPFrame.DeFrame(rawData);

            byte[] bufffer = new byte[frameBuffer.Length - 6];
            Array.Copy(frameBuffer, 6, bufffer, 0, bufffer.Length);
            string portName = Encoding.ASCII.GetString(frameBuffer, 0, 6).TrimEnd();

            if (IsShowInfo)
            {
                SafeOutText(string.Format("[RX] {0:G}, {1}\r\n{2}\r\n", DateTime.Now, portName, DataConvert.ByteToHexStr(rawData)));
            }

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
