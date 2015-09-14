using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Util
{
    /// <summary>
    /// 处理0x7E开头, 0x7E结尾的帧数据
    /// </summary>
    public class PPFrame : FrameSplitterBase
    {                
         /// <summary>
        /// 添加数据，并根据0x7E分割数据帧，分割的数据帧通过FrameReceived事件回调。 
        /// </summary>
        /// <param name="buffer">收到的数据</param>
        public override void PushData(byte[] buffer)
        {
            lock (lockObj)
            {
                bool frameStart = false;
                int beginIndex = -1;
                int endIndex = -1;

                if (buffer != null && buffer.Length > 0)
                {
                    int count = RxBufferOffset + buffer.Length;
                    if (count >= RX_BUFFER_SIZE)
                    {
                        //如果接受缓存超过最大值，则从开始
                        RxBufferOffset = 0;
                        count = buffer.Length;
                    }

                    Array.Copy(buffer, 0, RxBuffer, RxBufferOffset, buffer.Length);

                    for (int i = 0; i < count; i++)
                    {
                        byte value = RxBuffer[i];

                        if (!frameStart && value == 0x7E)
                        {
                            frameStart = true; //新的一帧
                            beginIndex = i;
                        }
                        else if (frameStart && value == 0x7E)
                        {
                            endIndex = i + 1;
                            frameStart = false;

                            //提取完整帧
                            byte[] frameData = new byte[endIndex - beginIndex];

                            Array.Copy(RxBuffer, beginIndex, frameData, 0, frameData.Length);

                            if (FrameReceived != null) FrameReceived(frameData);

                        }
                    }

                    //---------------
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

        }

        /// <summary>
        /// 转换成数据帧，以0x7E开始，以0x7E结束。
        /// 转义: 0x7D --> 0x7D, 0x5D 
        ///       0x7E --> 0x7D, 0x5E
        /// </summary>
        /// <param name="buffer">原始数据</param>
        /// <returns>帧数据</returns>
        public static byte[] ToFrame(byte[] buffer)
        {
            //1、每一个数据帧由一个标志字段(0x7E)开始，一个标志字段(0x7E)结束。
            //2、当信息字段中出现和标志字段一样的比特（0x7E）时，采用转义的做法：
            //  1）将信息字段中出现的每一个0x7E的字节，转变成2字节序列（0x7D，0x5E）。
            //  2）将信息字段中出现的每一个0x7D的字节，转变成2字节序列（0x7D，0x5D）。
            //  3）信息字段中出现和控制字符相同的ASCII码字符时，在该字符前面加一个0x7D字节（防止信息中的ASCII码被错误地解释为控制字符）。

            byte[] frameBuffer = null;

            if (buffer != null && buffer.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write((byte)0x7E);   //添加帧头
                        for (int i = 0; i < buffer.Length; i++) //添加数据并进行转义处理
                        {
                            byte value = buffer[i];
                                                        
                            if (value == 0x7D)
                            {
                                bw.Write((byte)0x7D);
                                bw.Write((byte)0x5D);
                            }
                            else if (value == 0x7E)
                            {
                                bw.Write((byte)0x7D);
                                bw.Write((byte)0x5E);
                            }
                            else
                            {
                                bw.Write(value);
                            }
                        }

                        bw.Write((byte)0x7E);   //添加帧尾
                        bw.Flush();                        
                        frameBuffer = ms.ToArray();
                    }
                }

            }

            return frameBuffer;

        }

        /// <summary>
        /// 从数据帧提取数据，去掉帧头和帧尾（0x7E），对数据进行去转义处理后的原始数据
        /// 去转义:  0x7D, 0x5E --> 0x7E
        ///          0x7D, 0x5D --> 0x7D
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] DeFrame(byte[] buffer)
        {
            //1、每一个数据帧由一个标志字段(0x7E)开始，一个标志字段(0x7E)结束。
            //2、当信息字段中出现和标志字段一样的比特（0x7E）时，采用转义的做法：
            //  1）将信息字段中出现的每一个0x7E的字节，转变成2字节序列（0x7D，0x5E）。
            //  2）将信息字段中出现的每一个0x7D的字节，转变成2字节序列（0x7D，0x5D）。
            //  3）信息字段中出现和控制字符相同的ASCII码字符时，在该字符前面加一个0x7D字节（防止信息中的ASCII码被错误地解释为控制字符）。
            
            byte[] pureBuffer = null;

            if (buffer != null && buffer.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        for (int i = 1; i < buffer.Length - 1; i++) //添加数据并进行去转义处理
                        {
                            byte value = buffer[i];
                            byte nextValue = buffer[i + 1];

                            if (value == 0x7D && nextValue == 0x5D)
                            {
                                bw.Write((byte)0x7D);
                                i++;
                            }
                            else if (value == 0x7D && nextValue == 0x5E)
                            {
                                bw.Write((byte)0x7E);
                                i++;
                            }
                            else
                            {
                                bw.Write(value);
                            }

                        }

                        bw.Flush();
                        pureBuffer = ms.ToArray();

                    }
                }
            }

            return pureBuffer;
        }

    }
}
