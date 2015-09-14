using System;
using System.Collections.Generic;
using System.Text;

namespace Util
{
    public interface IFrameSplitter
    {
        /// <summary>
        /// 处理后的完整数据帧
        /// </summary>
        Action<byte[]> FrameReceived { get; set; }

        /// <summary>
        /// 处理收到的数据
        /// </summary>
        /// <param name="buffer"></param>
        void PushData(byte[] buffer);

        /// <summary>
        /// 复位数据缓存（设置到空数据状态）
        /// </summary>
        void Reset();

    }
}
