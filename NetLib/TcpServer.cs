using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetLib
{
    public class TcpServer
    {
        private Socket tcpListener;
     
        List<ClientObject> m_ClientList = new List<ClientObject>();

        private string m_Address;
        private int m_Port;

        private int m_MaxClient;
        private volatile bool m_Stop = false;

        private IPEndPoint m_LocalEndPoint;

        public event DataEventHandler OnClientConnect;          

        public event DataEventHandler OnClientDisconnect;       

        public event DataEventHandler OnClientDataAvailable;    

        public event DataEventHandler OnServerFull;             

        private ManualResetEvent allDone = new ManualResetEvent(false);

        public string Address
        {
            get { return m_Address; }
            set { m_Address = value; }
        }

        public int Port
        {
            get { return m_Port; }
            set { m_Port = value; }
        }

        public int MaxClient
        {
            get { return m_MaxClient; }
            set { m_MaxClient = value; }
        }

        public void Close()
        {
            m_Stop = true;
            tcpListener.Close();
        }


        public List<ClientObject> ClientList
        {
            get { return m_ClientList; }
        }

        public TcpServer(string address, int port)
        {
            m_Address = address;
            m_Port = port;

            m_MaxClient = 100;

            if ((m_Address == null) || (m_Address == "") || (m_Address == "*"))
            {
                m_LocalEndPoint = new IPEndPoint(IPAddress.Any, m_Port);
            }
            else
            {
                m_LocalEndPoint = new IPEndPoint(IPAddress.Parse(m_Address), m_Port);
            }

            BindLocal();

        }

        private void BindLocal()
        {
            tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (m_LocalEndPoint == null) return;

            try
            {
                tcpListener.Bind(m_LocalEndPoint);
                tcpListener.Listen(100);
            }
            catch
            {
            }

        }

        public void StartListening()
        {
            try
            {
                while (!m_Stop)
                {
                    try
                    {
                        allDone.Reset();

                        tcpListener.BeginAccept(new AsyncCallback(AcceptCallback), tcpListener);

                        allDone.WaitOne();
                    }
                    catch(SocketException)
                    {
                        BindLocal();
                        Thread.Sleep(250);
                    }
                }
            }
            finally
            {
                CloseAllClient();
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                allDone.Set();

                if (m_Stop)
                {
                    return;
                }

                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                if (m_ClientList.Count < m_MaxClient)
                {
                    if (!ClientExists(handler))
                    {
                        m_ClientList.Add(new ClientObject(handler));

                        if (OnClientConnect != null)
                        {
                            PackClientList();
                            OnClientConnect(this, new DataEventArgs(handler));
                        }
                    }

                    StateObject state = new StateObject();
                    state.workSocket = handler;
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    PackClientList();
                    if (OnServerFull != null) OnServerFull(this, new DataEventArgs(handler));
                    handler.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine(DateTime.Now.ToString());
                Console.WriteLine("");
            }

        }

        public void ReadCallback(IAsyncResult ar)
        {
            if ((m_Stop) || (ar == null))
            {
                return;
            }

            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    byte[] dat = new byte[bytesRead];

                    Array.Copy(state.buffer, dat, bytesRead);

                    if (OnClientDataAvailable != null)
                        OnClientDataAvailable(this, new DataEventArgs(handler, dat));

                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                       new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    RemoveClientSocket(handler);

                    PackClientList();

                    if (OnClientDisconnect != null)
                        OnClientDisconnect(this, new DataEventArgs(handler));

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine(DateTime.Now.ToString());
                Console.WriteLine("");
            }
        }

        public void Send(Socket handler, byte[] data)
        {
            if (handler == null) return;
            handler.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                handler.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine(DateTime.Now.ToString());
                Console.WriteLine("");
            }
        }


        public void UpdateClientList(Socket client, ClientLevel level)
        {
            for (int i = 0; i < m_ClientList.Count; i++)
            {
                ClientObject clientObj = m_ClientList[i];
                if (clientObj.Client == client)
                {
                    clientObj.Level = level;
                    break;
                }
            }
        }

        private bool ClientExists(Socket client)
        {
            if (client == null) return false;

            for (int i = 0; i < m_ClientList.Count; i++)
            {
                if (m_ClientList[i].Client == client)
                {
                    return true;
                }
            }

            return false;
        }

        private void RemoveClientSocket(Socket client)
        {
            if (client == null) return;
            ClientObject clientObj = null;
            bool find = false;
            for (int i = 0; i < m_ClientList.Count; i++)
            {
                clientObj = m_ClientList[i];
                if (clientObj.Client == client)
                {
                    find = true;
                    break;
                }
            }

            if (find && (clientObj != null))
                m_ClientList.Remove(clientObj);

        }

        public void CloseClient(Socket client)
        {
            try
            {
                RemoveClientSocket(client);
                if (client != null) client.Close();
            }
            catch
            {
            }

        }

        private void PackClientList()
        {
            int i = 0;
            while (i < m_ClientList.Count)
            {
                ClientObject clientObj = m_ClientList[i];
                try
                {
                    if (!clientObj.Client.Connected)
                    {
                        m_ClientList.Remove(clientObj);
                        if (clientObj.Client != null) clientObj.Client.Close();
                        i--;
                    }
                }
                catch
                {
                }

                i++;
            }

        }

        public void CloseAllClient()
        {
            while (m_ClientList.Count > 0)
            {
                Socket client = m_ClientList[0].Client;
                RemoveClientSocket(client);
                if (client != null)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }

            }

        }


        public void BroadcastData(ClientLevel dataLevel, byte[] data)
        {
            byte curLevelNo = (byte)dataLevel;
            for (int i = 0; i < m_ClientList.Count; i++)
            {
                try
                {
                    ClientObject clientObj = m_ClientList[i];
                    if (clientObj.Client.Connected)
                    {
                        if ((byte)clientObj.Level <= curLevelNo)
                        {
                            Send(clientObj.Client, data);
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine(DateTime.Now.ToString());
                    Console.WriteLine("");

                }
            }
        }

        public void SendMessageText(ClientObject clientObject, string msgText)
        {
            if (clientObject == null) return;
            byte[] msgData = Encoding.Default.GetBytes(msgText);
            Send(clientObject.Client, msgData);
        }
    }


    public class StateObject
    {
        public Socket workSocket;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
    }


    public class ClientObject
    {
        private Socket m_Client;
        private ClientLevel m_Level;

        public Socket Client
        {
            get { return m_Client; }
        }

        public ClientLevel Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }

        public ClientObject(Socket client)
        {
            m_Client = client;
        }

        public ClientObject(Socket client, ClientLevel level)
        {
            m_Client = client;
            m_Level = level;
        }

    }


    public delegate void DataEventHandler(object sender, DataEventArgs e);

    public class DataEventArgs : EventArgs
    {
        private Socket m_Client;
        private byte[] m_data;

        public Socket Client
        {
            get { return m_Client; }
        }

        public byte[] Data
        {
            get { return m_data; }
        }

        public DataEventArgs(Socket client)
        {
            m_Client = client;
        }

        public DataEventArgs(Socket client, byte[] data)
        {
            m_Client = client;
            m_data = data;
        }

    }

    public enum ClientLevel
    {
        None = 0x00,
        Super = 0x10,
        Admin = 0x11,
        User = 0x12,
        Guest = 0x13,
    }

  
}