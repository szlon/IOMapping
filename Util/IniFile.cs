using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Util
{
    public class IniFile
    {
        #region Win32API��INI�ļ���д

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, Byte[] retVal, int size, string filePath);

        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string section, string key, string Value, string filePath);

        #endregion

        private string mFileName = null;

        //Ini���ʼ��
        public IniFile()
        {
        }

        public IniFile(string AIniFileName)
        {
            mFileName = AIniFileName;
        }

        ~IniFile()
        {
            mFileName = null;
            UpdateFile();
        }

        //����: �ļ���
        public string FileName
        {
            get { return mFileName; }
            set { mFileName = value; }
        }

        public void RewriteFile(bool ARewrite)
        {
            //��дIni�ļ�
            if (ARewrite)
            {
                if (File.Exists(mFileName))
                {
                    File.Delete(mFileName);
                }
            }
        }

        //��Ini�ļ�������
        public string ReadString(string ASection, string AKey, string ADefault)
        {
            Byte[] Buffer = new byte[2047];
            int BufLen = 0;
            string mStr;

            if ((BufLen = GetPrivateProfileString(ASection, AKey, ADefault, Buffer, Buffer.Length, mFileName)) != 0)
            {
                mStr = Encoding.Default.GetString(Buffer, 0, BufLen);

                return mStr.Trim();

            }
            else
            {
                return String.Empty;
            }

        }

        //д���ݵ�Ini�ļ�
        public bool WriteString(string ASection, string AKey, string AValue)
        {
            int RetCode = 0;
            try
            {
                RetCode = WritePrivateProfileString(ASection, AKey, AValue, mFileName);
            }
            catch (Exception e)
            {
                throw e;
            }

            return !(RetCode == 0);

        }

        //ɾ��ָ���ļ�
        public bool DeleteKey(string ASection, string AKey)
        {
            int RetCode = 0;
            try
            {
                RetCode = WritePrivateProfileString(ASection, AKey, null, mFileName);
            }
            catch (Exception e)
            {
                throw e;
            }

            return !(RetCode == 0);
        }

        //ɾ��ָ���Ľ�
        public bool DeleteSection(string ASection)
        {
            int RetCode = 0;
            try
            {
                RetCode = WritePrivateProfileString(ASection, null, null, mFileName);
            }
            catch (Exception e)
            {
                throw e;
            }

            return !(RetCode == 0);
        }

        //����Ini�ļ�
        public bool UpdateFile()
        {
            int RetCode = 0;
            try
            {
                RetCode = WritePrivateProfileString(null, null, null, mFileName);
            }
            catch (Exception e)
            {
                throw e;
            }

            return !(RetCode == 0);
        }

        //�ж�ĳ�����Ƿ����
        public bool ValueExists(string ASection, string AKey)
        {
            List<string> KeyList = ReadSection(ASection);
            return KeyList.IndexOf(AKey) > -1;
        }


        //��Ini��ȡ���е�Section��ArrayList
        public List<string> ReadAllSections()
        {
            List<string> strList = new List<string>();

            Byte[] Buffer = new byte[16384];
            int BufLen = 0, iStart = 0;
            string mStr;

            if ((BufLen = GetPrivateProfileString(null, null, null, Buffer, Buffer.Length, mFileName)) != 0)
            {

                for (int i = 0; i < BufLen; i++)
                {
                    if ((Buffer[i] == 0) && (Buffer[i + 1] > 0))
                    {
                        mStr = Encoding.Default.GetString(Buffer, iStart, i - iStart);

                        strList.Add(mStr.Trim());
                        iStart = i + 1;
                    }
                }

                mStr = Encoding.Default.GetString(Buffer, iStart, BufLen - iStart - 1);
                strList.Add(mStr);

            }

            return strList;
        }

        //��Ini��ȡָ��Section�����м���ArrayList
        public List<string> ReadSection(string ASection)
        {
            List<string> strList = new List<string>();

            Byte[] Buffer = new byte[16384];
            int BufLen = 0;
            int iStart = 0;

            string mStr;

            if ((BufLen = GetPrivateProfileString(ASection, null, null, Buffer, Buffer.Length, mFileName)) != 0)
            {
                for (int i = 0; i < BufLen; i++)
                {
                    if ((Buffer[i] == 0) && (Buffer[i + 1] > 0))
                    {
                        mStr = Encoding.Default.GetString(Buffer, iStart, i - iStart);

                        strList.Add(mStr.Trim());
                        iStart = i + 1;
                    }
                }

                mStr = Encoding.Default.GetString(Buffer, iStart, BufLen - iStart - 1);
                strList.Add(mStr);
            }

            return strList;
        }

        //��Ini��ȡָ��Section�����м�=��ֵ��NameValueCollection
        public List<KeyValuePair<string, string>> ReadSectionValues(string ASection)
        {
            List<KeyValuePair<string, string>> strValueList = new List<KeyValuePair<string, string>>();
            Byte[] Buffer = new byte[16384];
            int BufLen = 0;
            int iStart = 0;
            string mKey, mValue;

            if ((BufLen = GetPrivateProfileString(ASection, null, null, Buffer, Buffer.Length, mFileName)) != 0)
            {
                for (int i = 0; i < BufLen; i++)
                {
                    if ((Buffer[i] == 0) && (Buffer[i + 1] > 0))
                    {
                        mKey = Encoding.Default.GetString(Buffer, iStart, i - iStart).Trim();
                        mValue = ReadString(ASection, mKey, "").Trim();

                        //���ؼ�������ֵ�ļ���
                        strValueList.Add(new KeyValuePair<string,string>(mKey, mValue));

                        iStart = i + 1;
                    }
                }

                mKey = Encoding.Default.GetString(Buffer, iStart, BufLen - iStart - 1).Trim();
                mValue = ReadString(ASection, mKey, "").Trim();
                
                strValueList.Add(new KeyValuePair<string, string>(mKey, mValue));

            }

            return strValueList;
        }


        public string GetString(string ASection, string AKey, string ADefault)
        {
            return ReadString(ASection, AKey, ADefault);
        }

        public List<string> GetAllSections()
        {
            List<string> strList = new List<string>();

            Byte[] Buffer = new byte[16384];
            int BufLen = 0;
            int iStart = 0;

            string mStr;

            if ((BufLen = GetPrivateProfileString(null, null, null, Buffer, Buffer.Length, mFileName)) != 0)
            {
                for (int i = 0; i < BufLen; i++)
                {
                    if ((Buffer[i] == 0) && (Buffer[i + 1] > 0))
                    {
                        mStr = Encoding.Default.GetString(Buffer, iStart, i - iStart);

                        strList.Add(mStr.Trim());
                        iStart = i + 1;
                    }
                }

                mStr = Encoding.Default.GetString(Buffer, iStart, BufLen - iStart - 1);
                strList.Add(mStr);

            }

            return strList;
        }

        public List<string> GetSection(string ASection)
        {
            List<string> strList = new List<string>();

            Byte[] Buffer = new byte[16384];
            int BufLen = 0, iStart = 0;
            string mStr;

            if ((BufLen = GetPrivateProfileString(ASection, null, null, Buffer, Buffer.Length, mFileName)) != 0)
            {

                for (int i = 0; i < BufLen; i++)
                {
                    if ((Buffer[i] == 0) && (Buffer[i + 1] > 0))
                    {
                        mStr = Encoding.Default.GetString(Buffer, iStart, i - iStart);

                        strList.Add(mStr.Trim());
                        iStart = i + 1;
                    }
                }

                mStr = Encoding.Default.GetString(Buffer, iStart, BufLen - iStart - 1);
                strList.Add(mStr);

                return strList;
            }
            else
                return null;
        }

        public List<KeyValuePair<string, string>> GetSectionValues(string ASection)
        {
            List<KeyValuePair<string, string>> strValueList = new List<KeyValuePair<string, string>>();

            Byte[] Buffer = new byte[16384];
            int BufLen = 0, iStart = 0;
            string mKey, mValue;

            if ((BufLen = GetPrivateProfileString(ASection, null, null, Buffer, Buffer.Length, mFileName)) != 0)
            {
                for (int i = 0; i < BufLen; i++)
                {
                    if ((Buffer[i] == 0) && (Buffer[i + 1] > 0))
                    {
                        mKey = Encoding.Default.GetString(Buffer, iStart, i - iStart).Trim();
                        mValue = ReadString(ASection, mKey, "").Trim();

                        //���ؼ�������ֵ�ļ���
                        strValueList.Add(new KeyValuePair<string, string>(mKey, mValue));

                        iStart = i + 1;
                    }
                }
                mKey = Encoding.Default.GetString(Buffer, iStart, BufLen - iStart - 1).Trim();
                mValue = ReadString(ASection, mKey, "").Trim();
                strValueList.Add(new KeyValuePair<string, string>(mKey, mValue));

            }

            return strValueList;
        }


    }


}