using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.IO.Ports;
using Util;
using IOLib;

namespace IOMapServer
{
    class PortData
    {
        static string SerialPortFileName = "SerialPortInfo.ini";

        byte[] rxBuffer = new byte[1024 * 10];

        object lockObj = new object();
        
        public event EventHandler<ReceiveEventArgs> OnReceiveData;

        public PortData()
        {
        }


        Dictionary<string, SerialPort> spList = new Dictionary<string, SerialPort>();

        public void LoadConfig_SerialPort()
        {
            spList.Clear();
            string fileName = Path.Combine(System.Windows.Forms.Application.StartupPath, SerialPortFileName);

            IniFile ini = new IniFile(fileName);
            List<string> secList = ini.ReadAllSections();

            foreach (string section in secList)
            {
                List<string> keyList = ini.ReadSection(section);
                foreach (string key in keyList)
                {
                    string itemValue = ini.ReadString(section, key, string.Empty);
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
                            }
                            else
                            {
                                System.Windows.Forms.MessageBox.Show(string.Format("串口{0}重复配置！", portName));
                            }
                        }
                    }
                }
                
            }

        }

        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
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

        public void Send(string portName, byte[] buffer)
        {
            if (spList.ContainsKey(portName))
            {
                SerialPort osp = spList[portName] as SerialPort;
                if (osp.IsOpen)
                {
                    osp.Write(buffer, 0, buffer.Length);
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

            valueList.Add(string.Format("  {0} {1} {2}", "端口名".PadRight(6, ' '), "波特率".PadRight(6, ' '), "当前状态"));
            foreach (SerialPort osp in spList.Values)
            {
                valueList.Add(string.Format("  {0} {1} {2}", osp.PortName.PadRight(10, ' '), osp.BaudRate.ToString().PadRight(10, ' '), osp.IsOpen ? "打开" : "关闭"));

            }
            
            return valueList;

        }

    }

}
