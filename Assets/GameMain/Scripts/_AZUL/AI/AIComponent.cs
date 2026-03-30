using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class AIComponent : GameFrameworkComponent
    {
        private bool m_Active = false;
        private AINetwork m_AINetwork = null;
        private CancellationTokenSource m_CancellationTokenSource = null;

        protected override void Awake()
        {
            base.Awake();
            m_Active = false;
        }

        private void OnDestroy()
        {
            Stop();
        }

        /// <summary>
        /// 启动AI
        /// </summary>
        public async void Run()
        {
            if (m_Active)
            {
                Log.Warning("AI已经在运行中");
                return;
            }

            m_Active = true;
            m_CancellationTokenSource = new CancellationTokenSource();
            m_AINetwork = new AINetwork();

            try
            {
                await m_AINetwork.Run(m_CancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Log.Error($"AI运行异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 停止AI
        /// </summary>
        public void Stop()
        {
            if (!m_Active)
            {
                return;
            }

            m_Active = false;
            m_CancellationTokenSource?.Cancel();
            m_CancellationTokenSource?.Dispose();
            m_CancellationTokenSource = null;
            m_AINetwork = null;
        }

        public bool IsActive()
        {
            return m_Active;
        }

        /// <summary>
        /// 发送消息给AI服务器
        /// </summary>
        public void SendNetworkMessage(string message)
        {
            if (!m_Active || m_AINetwork == null)
            {
                Log.Warning("AI未运行，无法发送消息");
                return;
            }

            m_AINetwork.EnqueueMessage(message);
        }
    }

    public class AINetwork
    {
        private static readonly string SERVER_IP = "127.0.0.1";
        private static readonly int SERVER_PORT = 8888;
        private static readonly int BUFFER_SIZE = 4096;

        // 发送给AI服务器的消息队列
        private Queue<string> m_MessageQueue = new Queue<string>();
        private object m_QueueLock = new object();

        public AINetwork()
        {
            m_MessageQueue.Clear();
        }

        /// <summary>
        /// 添加消息到发送队列
        /// </summary>
        public void EnqueueMessage(string message)
        {
            lock (m_QueueLock)
            {
                m_MessageQueue.Enqueue(message);
            }
        }

        /// <summary>
        /// 运行AI网络（不阻塞主线程）
        /// </summary>
        public async Task Run(CancellationToken cancellationToken)
        {
            TcpClient client = null;
            NetworkStream stream = null;

            try
            {
                client = new TcpClient();
                await client.ConnectAsync(SERVER_IP, SERVER_PORT);
                Log.Info($"已连接到AI服务器 {SERVER_IP}:{SERVER_PORT}");

                stream = client.GetStream();

                // 启动接收任务
                Task receiveTask = ReceiveMessagesAsync(stream, cancellationToken);

                // 发送消息循环
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 尝试从消息队列中获取消息
                    string message = null;
                    lock (m_QueueLock)
                    {
                        if (m_MessageQueue.Count > 0)
                        {
                            message = m_MessageQueue.Dequeue();
                        }
                    }

                    if (message != null)
                    {
                        // 发送消息长度（4字节）+ 消息内容
                        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                        byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

                        await stream.WriteAsync(lengthBytes, 0, lengthBytes.Length, cancellationToken);
                        await stream.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken);
                        await stream.FlushAsync(cancellationToken);

                        Log.Info($"已发送消息给AI服务器: {message}");
                    }
                    else
                    {
                        // 队列为空，等待一段时间避免CPU空转
                        await Task.Delay(100, cancellationToken);
                    }
                }

                // 等待接收任务完成
                await receiveTask;
            }
            catch (OperationCanceledException)
            {
                Log.Info("AI网络已停止");
            }
            catch (Exception ex)
            {
                Log.Error($"AI网络错误: {ex.Message}");
            }
            finally
            {
                stream?.Close();
                client?.Close();
                Log.Info("已断开AI服务器连接");
            }
        }

        /// <summary>
        /// 接收AI服务器消息（异步）
        /// </summary>
        private async Task ReceiveMessagesAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[BUFFER_SIZE];

            try
            {
                while (!cancellationToken.IsCancellationRequested && stream.CanRead)
                {
                    // 读取消息长度（4字节）
                    byte[] lengthBytes = new byte[4];
                    int lengthBytesRead = await stream.ReadAsync(lengthBytes, 0, 4, cancellationToken);

                    if (lengthBytesRead == 0)
                    {
                        // 连接已关闭
                        break;
                    }

                    int messageLength = BitConverter.ToInt32(lengthBytes, 0);

                    if (messageLength <= 0 || messageLength > BUFFER_SIZE)
                    {
                        Log.Warning($"接收到无效的消息长度: {messageLength}");
                        continue;
                    }

                    // 读取消息内容
                    int totalBytesRead = 0;
                    while (totalBytesRead < messageLength)
                    {
                        int bytesRead = await stream.ReadAsync(
                            buffer, 
                            totalBytesRead, 
                            messageLength - totalBytesRead, 
                            cancellationToken);

                        if (bytesRead == 0)
                        {
                            break;
                        }

                        totalBytesRead += bytesRead;
                    }

                    if (totalBytesRead == messageLength)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, messageLength);
                        Log.Info($"收到AI服务器消息: {message}");

                        // 触发消息接收事件（在主线程处理）
                        GameEntry.BoardGame.ParseAIAction(message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception ex)
            {
                Log.Error($"接收消息错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理接收到的消息（在主线程中调用）
        /// </summary>
        private void OnMessageReceived(string message)
        {
            // 在这里处理AI返回的消息
            // 例如：解析AI决策，触发游戏事件等
            Log.Info($"处理AI消息: {message}");

            // TODO: 触发自定义事件或调用回调函数
        }
    }

    ///// <summary>
    ///// Unity主线程调度器（需要添加到场景中）
    ///// </summary>
    //public class UnityMainThreadDispatcher : MonoBehaviour
    //{
    //    private static UnityMainThreadDispatcher _instance;
    //    private static Queue<Action> _executionQueue = new Queue<Action>();

    //    public static UnityMainThreadDispatcher Instance
    //    {
    //        get
    //        {
    //            if (_instance == null)
    //            {
    //                GameObject obj = new GameObject("UnityMainThreadDispatcher");
    //                _instance = obj.AddComponent<UnityMainThreadDispatcher>();
    //                DontDestroyOnLoad(obj);
    //            }
    //            return _instance;
    //        }
    //    }

    //    void Update()
    //    {
    //        lock (_executionQueue)
    //        {
    //            while (_executionQueue.Count > 0)
    //            {
    //                _executionQueue.Dequeue().Invoke();
    //            }
    //        }
    //    }

    //    public static void Enqueue(Action action)
    //    {
    //        lock (_executionQueue)
    //        {
    //            _executionQueue.Enqueue(action);
    //        }
    //    }
    //}
}
