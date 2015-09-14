using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;

namespace Util
{
    /// <summary>
    /// 数据收发队列
    /// </summary>
    public class DataQueue<T>
    {
        Queue<T> queueFrameSend = new Queue<T>();
        Queue<T> queueFrameReceived = new Queue<T>();

        ManualResetEvent eventFrameSend = new ManualResetEvent(false);
        ManualResetEvent eventFrameReceived = new ManualResetEvent(false);

        public Action<T> ActionReceive { get; set; }
        public Action<T> ActionSend { get; set; }


        public bool IsRun { get; private set; }

        public void Start()
        {
            IsRun = true;
            Thread threadFrameReceived = new Thread(new ThreadStart(QueueReceivedProc));
            threadFrameReceived.Name = "DataQueue.QueueReceivedProc";
            threadFrameReceived.IsBackground = true;
            threadFrameReceived.Start();

            Thread threadFrameSend = new Thread(new ThreadStart(QueueSendProc));
            threadFrameSend.Name = "DataQueue.QueueSendProc";
            threadFrameSend.IsBackground = true;
            threadFrameSend.Start();
        }

        public void Stop()
        {
            IsRun = true;
        }

        /// <summary>
        /// 获取队列信息
        /// </summary>
        /// <param name="txCount">发送队列数</param>
        /// <param name="rxCount">接收队列数</param>
        public void GetCount(ref int txCount, ref int rxCount)
        {
            lock (queueFrameSend)
            {
                txCount = queueFrameSend.Count;
            }

            lock (queueFrameReceived)
            {
                rxCount = queueFrameReceived.Count;
            }

        }

        /// <summary>
        /// 清空收发队列
        /// </summary>
        public void ClearQueue()
        {
            lock (queueFrameSend)
            {
                queueFrameSend.Clear();
            }

            lock (queueFrameReceived)
            {
                queueFrameReceived.Clear();
            }

        }

        public void EnqueueReceive(T pkgData)
        {
            //将数据加入接收队列
            lock (queueFrameReceived)
            {
                queueFrameReceived.Enqueue(pkgData);
                eventFrameReceived.Set();
            }

        }

        public void EnqueueSend(T pkgData)
        {
            //将数据加入发送队列
            lock (queueFrameSend)
            {
                queueFrameSend.Enqueue(pkgData);
                eventFrameSend.Set();
            }
        }

        private void QueueReceivedProc()
        {
            //处理接收的数据帧队列   
            while (IsRun)
            {
                try
                {
    

                    if (!eventFrameReceived.WaitOne(3000, true))
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    T pkgData = default(T);
                    bool boDequeue = false;
                    lock (queueFrameReceived)
                    {
                        if (queueFrameReceived.Count > 0)
                        {
                            pkgData = queueFrameReceived.Dequeue();
                            boDequeue = true;
                        }
                        else
                        {
                            eventFrameReceived.Reset();
                        }
                    }

                    if (ActionReceive != null && boDequeue)
                    {
                        //处理协议帧数据           
                        ActionReceive(pkgData);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void QueueSendProc()
        {
            //处理数据发送队列
            while (IsRun)
            {
                try
                {

                    if (!eventFrameSend.WaitOne(3000, true))
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    lock (queueFrameSend)
                    {
                        if (queueFrameSend.Count > 0)
                        {
                            T pkgData = queueFrameSend.Dequeue();

                            if (ActionSend != null)
                            {
                                //发送数据
                                ActionSend(pkgData);
                            }

                        }
                        else
                        {
                            eventFrameSend.Reset(); //等待队列中的所有数据读取完毕，挂起信号
                        }
                    }

                }
                catch
                {
                }
            }

        }

    }
}
