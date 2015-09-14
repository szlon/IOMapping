using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.IO.Ports;
using IOLib;
using Util;

namespace IOMapClient
{
    class PortData
    {
        static string SerialPortFileName = "IOMap.ini";

        byte[] rxBuffer = new byte[1024 * 10];

        object lockObj = new object();
        
        public event EventHandler<ReceiveEventArgs> OnReceiveData;

        public PortData()
        {
        }


        Dictionary<string, SerialPort> spList = new Dictionary<string, SerialPort>();
        Dictionary<string, string> RemoteToLocalList = new Dictionary<string, string>();    //远端 - 本地 端口映射
        Dictionary<string, string> LocalToRemoteList = new Dictionary<string, string>();    //远端 - 本地 端口映射


        public void LoadConfig_SerialPort()
        {
            spList.Clear();
            RemoteToLocalList.Clear();
            LocalToRemoteList.Clear();

            string fileName = Path.Combine(System.Windows.Forms.Application.StartupPath, SerialPortFileName);

            IniFile ini = new IniFile(fileName);

            string sectionName = "MapList";
            List<string> mapList = ini.ReadSection(sectionName);


            foreach (string key in mapList)
            {
                string itemValue = ini.ReadString(sectionName, key, string.Empty);

                if (!string.IsNullOrEmpty(itemValue))
                {
                    string[] paramList = itemValue.Split(new char[] { ',' });
                    if (paramList.Length == 2)
                    {
                        string portName = paramList[0].Trim();
                        int baudRate = int.Parse(paramList[1].Trim());

                        if (!spList.ContainsKey(portName))
                        {
                            SerialPort sp = new SerialPort(portName, baudRate);
                            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
                            spList.Add(portName, sp);
                            RemoteToLocalList.Add(key, portName);
                            LocalToRemoteList.Add(portName, key);
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(string.Format("串口{0}重复配置！", portName));
                        }
                    }
                }
            }

        }
        

        /// <summary>
        /// 获取本地端口映射到远程端口
        /// </summary>
        /// <param name="portName"></param>
        /// <returns></returns>
        public string GetRemotePortName(string portName)
        {
            string remotePortName = string.Empty;

            if (LocalToRemoteList.ContainsKey(portName))
            {
                remotePortName = LocalToRemoteList[portName];
            }

            return remotePortName;
        }

        /// <summary>
        /// 获取远程端口映射到本地端口
        /// </summary>
        /// <param name="portName"></param>
        /// <returns></returns>
        public string GetLocalPortName(string portName)
        {
            string localPortName = string.Empty;

            if (RemoteToLocalList.ContainsKey(portName))
            {
                localPortName = RemoteToLocalList[portName];
            }

            return localPortName;
        }

        public void Send(string portName, byte[] buffer)
        {
            //收到远程端口的数据，需要将远程端口转换为本地端口 
            if (spList.ContainsKey(portName))
            {
                SerialPort osp = spList[portName] as SerialPort;
                if (osp.IsOpen)
                {
                    osp.Write(buffer, 0, buffer.Length);
                }
            }
        }

        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //收到本地端口的数据，需要将本地端口转换为远程端口
            lock (lockObj)
            {
                SerialPort serialPort = sender as SerialPort;

                if (OnReceiveData != null)
                {
                    int count = serialPort.BytesToRead;

                    serialPort.Read(rxBuffer, 0, count);

                    OnReceiveData(sender, new ReceiveEventArgs(rxBuffer, 0, count));
                }
            }

        }

        public void OpenAll()
        {
            foreach (SerialPort osp in spList.Values)
            {
                try
                {
                    if (!osp.IsOpen)
                    {
                        osp.Open();
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }          
        }

        public void CloseAll()
        {
            foreach (SerialPort osp in spList.Values)
            {
                try
                {
                    if (osp.IsOpen)
                    {
                        osp.Close();
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }    
        }

        public List<string> GetSerialPortStatus()
        {
            List<string> valueList = new List<string>();

            valueList.Add(string.Format("  {0} {1} {2} {3}", "本地".PadRight(8, ' '), "映射".PadRight(6, ' '), "波特率".PadRight(6, ' '), "当前状态"));
            foreach (SerialPort osp in spList.Values)
            {
                string remotePort = LocalToRemoteList[osp.PortName];

                valueList.Add(string.Format("  {0} <--> {1} {2} {3}", osp.PortName.PadRight(4, ' '), remotePort.PadRight(10, ' '), osp.BaudRate.ToString().PadRight(10, ' '), osp.IsOpen ? "打开" : "关闭"));

            }
            
            return valueList;

        }

    }

}
