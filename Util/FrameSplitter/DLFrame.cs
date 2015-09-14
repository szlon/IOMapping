using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Lon.Common;

namespace Util
{
    /// <summary>
    /// 处理0x10 0x02开头, 0x10 0x03结尾的帧数据
    /// </summary>
    public class DLFrame : FrameSplitterBase
    {
        public static byte DLE = 0x10;
        public static byte STX = 0x02;
        public static byte ETX = 0x03;

        /// <summary>
        /// 处理以0x10, 0x02 ... 0x10, 0x03格式的数据帧
        /// </summary>
        /// <param name="buffer"></param>
        public override void PushData(byte[] buffer)
        {
            int preTag = 0;
            int curTag = 0;
            int nxtTag = 0;
            bool frameStart = false;

            lock (lockObj)
            {
                int count = RxBufferOffset + buffer.Length;
                if (count >= RX_BUFFER_SIZE)
                {
                    //如果接受缓存超过最大值，则从开始
                    RxBufferOffset = 0;
                    count = buffer.Length;
                }

                Array.Copy(buffer, 0, RxBuffer, RxBufferOffset, buffer.Length);

                int beginIndex = -1;
                int endIndex = -1;

                for (int i = 0; i < count - 1; i++)
                {
                    preTag = (i== 0 ? -1 : RxBuffer[i - 1]);
                    curTag = RxBuffer[i];
                    nxtTag = RxBuffer[i + 1];


                    if (preTag != DLE)
                    {
                        if (!frameStart && curTag == DLE && nxtTag == STX)
                        {
                            frameStart = true; //新的一帧
                            beginIndex = i;
                        }
                        else if (frameStart && curTag == DLE && nxtTag == ETX)
                        {
                            endIndex = i + 1;
                            frameStart = false;

                            //提取完整帧
                            byte[] frameData = new byte[endIndex - beginIndex + 1];

                            Array.Copy(RxBuffer, beginIndex, frameData, 0, frameData.Length);

                            if (FrameReceived != null) FrameReceived(frameData);
                                                     
                        }
                    }

                }

                RxBufferOffset = 0;
                endIndex++;
                if (endIndex < count)
                {
                    //剩余的数据移动到前面，与下一次收到的数据拼接
                    int len = count - endIndex;
                    Array.Copy(RxBuffer, endIndex, RxBuffer, 0, len);
                    RxBufferOffset = len;
                }
            }
        }


        /// <summary>
        /// 打包数据：添加头尾，CRC，转义处理
        /// </summary>
        /// <param name="buffer">数据</param>
        /// <param name="isEsc">true:转义, false:不转义</param>
        /// <returns></returns>
        public static byte[] ToFrame(byte[] buffer, bool isEsc)
        {
            byte[] dataBuffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((byte)0x10);
                    bw.Write((byte)0x02);

                    if (!isEsc)
                    {
                        //不转义
                        bw.Write(buffer);
                    }
                    else
                    {
                        //转义处理
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            byte value = buffer[i];
                            bw.Write(value);
                            if (value == 0x10) bw.Write(value); //如果有0x10则再添加一个0x10字节
                        }
                    }

                    UInt16 crc = Crc16.GetCRC16(buffer);
                    crc = DataConvert.InvWord(crc);
                    bw.Write(crc);

                    bw.Write((byte)0x10);
                    bw.Write((byte)0x03);
                    bw.Flush();

                    dataBuffer = ms.ToArray();
                }
            }

            return dataBuffer;
        }

        /// <summary>
        /// 打包数据：添加头尾，CRC，转义处理
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] ToFrame(byte[] buffer)
        {
            return ToFrame(buffer, true);
        }

        /// <summary>
        /// 解包数据：去头尾，CRC，反转义处理
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static byte[] DeFrame(byte[] buffer)
        {
            if (buffer.Length < 6) return null;
            if (!(buffer[0] == 0x10 && buffer[1] == 0x02 && buffer[buffer.Length - 2] == 0x10 && buffer[buffer.Length - 1] == 0x03)) return null;

            byte[] dataBuffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {

                    for (int i = 2; i < buffer.Length - 4; i++)
                    {
                        byte value = buffer[i];
                        byte nextValue = buffer[i + 1];

                        if (value == 0x10 && nextValue == 0x10) i++;
                        bw.Write(value);
                    }

                    bw.Flush();
                    dataBuffer = ms.ToArray();

                    //计算crc
                    UInt16 crc = Crc16.GetCRC16(dataBuffer);
                    crc = DataConvert.InvWord(crc);

                    UInt16 crcRaw = DataConvert.BytestoUInt16(buffer, buffer.Length - 4);
                    if (crc != crcRaw)
                    {
                        dataBuffer = null;
                    }
                }
            }


            return dataBuffer;
        }
    }

}
