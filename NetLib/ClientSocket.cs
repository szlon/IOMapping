using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace NetLib
{
    public class ClientSocket
    {
        static int BUFFER_SIZE = 256 * 1024;  

        byte[] frameBuffer = new byte[BUFFER_SIZE];
        byte[] receiveBuffer = new byte[BUFFER_SIZE];

        Socket socket = null;

        public event SocketReceivedHandler OnSocketReceived = null;
        public event SocketStateHandler OnSocketState = null;
        public string serverIP = "127.0.0.1";

        public int serverPort = 7777;

        public int ReconectCount = -1;
        public int ReconnectTime = 5;

        private int connectCount = 0; 
        private bool isInit = true;
        private bool connect = false;
        public bool Connect
        {
            get { return connect; }
        }

        private bool isRun = false;
        Thread threadData;

        public ClientSocket()
        {
        }

        public ClientSocket(string ip, int port)
        {
            this.serverIP = ip;

            this.serverPort = port;
        }

        public void Start()
        {
            isRun = true;

            threadData = new Thread(new ThreadStart(ThreadProc));

            threadData.IsBackground = true;

            threadData.Start();

        }

        public void Stop()
        {
            isRun = false;
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                threadData.Abort();
                threadData = null;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public void Send(byte[] buffer)
        {
            if (connect)
            {
                socket.Send(buffer, buffer.Length, SocketFlags.None);
            }
        }

        private void ThreadProc()
        {
            while (isRun)
            {
                try
                {
                    IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(this.serverIP), this.serverPort);

                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    socket.Connect(ipe);

                    connect = true;

                    if (OnSocketState != null) OnSocketState(this, SocketState.Connected);

                    SocketDataReceived();

                    connect = false;

                }
                catch
                {
                    isInit = false;
                    connect = false;
                }

                if (OnSocketState != null) OnSocketState(this, SocketState.Disconnected);

                if (ReconectCount > -1 && ++connectCount > ReconectCount) break;

                Thread.Sleep(ReconnectTime * 1000);
            }

        }


        private void SocketDataReceived()
        {
            while (isRun)
            {
                try
                {
                    if (socket.Poll(-1, SelectMode.SelectRead))
                    {
                        int length = socket.Receive(receiveBuffer);

                        if (length <= 0) break;

                        byte[] buf = new byte[length];

                        Array.Copy(receiveBuffer, 0, buf, 0, length);

                        OnSocketReceived(this, buf);
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);

                    if (OnSocketState != null) OnSocketState(this, SocketState.Unkown);

                    break;
                }
            }
        }
    }

    public enum SocketState
    {
        Connected,
        Disconnected,
        Unkown,
    }

    public delegate void SocketReceivedHandler(object sender, byte[] buffer);
    public delegate void SocketStateHandler(object sender, SocketState state);


}
