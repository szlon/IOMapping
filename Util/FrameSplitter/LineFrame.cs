using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Util
{
    /// <summary>
    /// 帧数据分割，默认以0x0A为帧结束标志
    /// </summary>
    public class LineFrame : FrameSplitterBase
    {                
        public byte SplitTag { get; set; }

        public LineFrame()
        {
            SplitTag = 0x0A;
        }
        
        /// <summary>
        /// 添加数据，并以指定结束字符（默认为0x0A)分割数据帧，分割的数据帧通过FrameReceived事件回调。 
        /// </summary>
        /// <param name="buffer">收到的数据</param>
        public override void PushData(byte[] buffer)
        {
            lock (lockObj)
            {
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
                    
                    beginIndex = 0;

                    for (int i = 0; i < count; i++)
                    {
                        byte value = RxBuffer[i];

                        if (value == SplitTag)
                        {
                            //将之前的数据分割
                            endIndex = i + 1;
                            
                            //提取数据帧
                            byte[] frameData = new byte[endIndex - beginIndex];

                            Array.Copy(RxBuffer, beginIndex, frameData, 0, frameData.Length);
                            
                            if (FrameReceived != null) FrameReceived(frameData);

                            beginIndex = endIndex;  //准备检测下一帧
                        }


                    }

                    //---------------
                    RxBufferOffset = 0;

                    if (endIndex < count)
                    {
                        //剩余的数据移动到前面，与下一次收到的数据拼接
                        if (endIndex < 0) endIndex = 0;
                        int len = count - endIndex;

                        Array.Copy(RxBuffer, endIndex, RxBuffer, 0, len);
                        RxBufferOffset = len;
                    }

                }

            }

        }
    }
}
