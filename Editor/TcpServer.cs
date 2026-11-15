using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    
    [InitializeOnLoad]
    public class TcpServer
    {
        #region Connection lifecycle

        private static TcpListener _Listener;
        private static TcpSession _CurrentSession;
        
        [Obsolete]private static TcpClient _Client;
        [Obsolete]private static NetworkStream _Stream;

        
        private static bool _Running;
        private static bool _IsStarting;
        
        

        static TcpServer()
        {
            EditorApplication.quitting += Stop;
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            
            _ = StartListen(HierarchySettingWindow.port);
        }

        
        private static async Task StartListen(int port)
        {
            _IsStarting = true;
            
            try
            {
                _Listener = new TcpListener(IPAddress.Loopback, port);
                _Listener.Start();
            }
            catch (SocketException e)
            {
                Debug.LogError($"The port may already be in use.\n{e.Message}");
                Stop();
                return;
            }
            
            _Running = true;
            _IsStarting = false;
            
            while (_Running)
            {
                var session = new TcpSession(await _Listener.AcceptTcpClientAsync());
                _CurrentSession = session;
                
                Debug.Log("成功连接到客户端");
                HierarchyExporter.SendCurrentHierarchy();

                // 启动心跳检测以及接收消息循环
                _ = HeartbeatLoop(session);
                await ReceiveLoop(session);

                session.Dispose();
                Debug.Log("旧客户端已断开，等待二次连接……");
            }
        }

        #endregion
        

        #region Sending

        /// <summary>
        /// 发送消息到客户端（供外部调用）
        /// </summary>
        /// <param name="type">MessageType</param>
        /// <param name="message">String</param>
        public static Task SendMessage(MessageType type, string message = "")
        {
            return SendMessage(_CurrentSession, type, message);
        }
        
        /// <summary>
        /// 向指定客户端发送消息（供内部调用）
        /// </summary>
        /// <param name="session">TcpSession</param>
        /// <param name="type">MessageType</param>
        /// <param name="message">String</param>
        private static async Task SendMessage(TcpSession session, MessageType type, string message = "")
        {
            // 防止在等待过程中状态变化
            if (session == null || !ReferenceEquals(_CurrentSession, session) || session.State != SessionState.Connected)
                return;

            // 根据消息类型处理消息内容
            switch (type)
            {
                case MessageType.None:
                    return;
                
                case MessageType.XML:
                    if (string.IsNullOrEmpty(message))
                    {
                        Debug.Log("XML message is empty");
                        return;
                    }
                    break;
                
                case MessageType.HeartbeatPing:
                case MessageType.HeartbeatPong:
                case MessageType.Close:
                    message = String.Empty;
                    break;
                
                default:
                    Debug.Log("Unknown message type");
                    return;
            }

            int bodyLength = Encoding.UTF8.GetByteCount(message);

            message = $"[{(int)type}][{bodyLength}]" + message;
            byte[] data = Encoding.UTF8.GetBytes(message);
            
            // 防止在信息处理过程中状态变化
            if (!ReferenceEquals(_CurrentSession, session) || session.State != SessionState.Connected)
                return;

            await session.Stream.WriteAsync(data);
            await session.Stream.FlushAsync();
        }

        #endregion

        #region Reconnection

        
        /// <summary>
        /// 重启TcpListen
        /// </summary>
        /// <param name="port">int</param>
        public static void RetryStart(int port)
        {
            if (_Running || _IsStarting)
            {
                return;
            }

            Debug.Log("尝试重连中……");
            _ = StartListen(port);
        }

        #endregion

        #region Receiving
        
        

        // 监听检测是否释放客户端
        private static async Task ReceiveLoop(TcpSession session)
        {
            byte[] buffer = new byte[1024];

            // 当心跳停止时，ReadAsync会返回异常，通过try-catch捕获异常并结束循环
            try
            {
                while (true)
                {
                    Array.Clear(buffer, 0, buffer.Length); // 清理残余缓存
                
                    int length = await session.Stream.ReadAsync(buffer);
                    if (length == 0) break; // FIN返回
                    
                    
                    string message = Encoding.UTF8.GetString(buffer, 0, length);

                    if (message == "[3][0]") // 收到PONG
                    {
                        session.WaitingForPong = false;
                    }
                    else
                    {
                        Debug.Log($"Message: {message}");
                    }
                } 
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        #endregion

        #region Heartbeat
        
        
        // 心跳检测
        private static async Task HeartbeatLoop(TcpSession session)
        {
            try
            {
                // 保证新旧客户端切换时不会干扰，且在客户端断开连接时停止心跳
                while (_Running && ReferenceEquals(_CurrentSession, session) && session.State == SessionState.Connected)
                {
                    await Task.Delay(HierarchySettingWindow.heartbeatIntervalSeconds * 1000);
                    
                    // 防止等待过程中状态变化
                    if (!_Running || !ReferenceEquals(_CurrentSession, session) || session.State != SessionState.Connected)
                        break;

                    session.WaitingForPong = true;
                    await SendMessage(MessageType.HeartbeatPing);
                    await Task.Delay(HierarchySettingWindow.heartbeatIntervalSeconds * 1000);  // 等待PONG响应
                    
                    // 防止等待过程中状态变化
                    if (!_Running || !ReferenceEquals(_CurrentSession, session) || session.State != SessionState.Connected)
                        break;

                    if (session.WaitingForPong)
                    {
                        Debug.Log("客户端无响应，断开连接");
                        _CurrentSession.Dispose();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        #endregion

        #region Shutdown
        

        // 关闭编辑器、重新编译后结束旧生命周期残余内容
        private static void Stop()
        {
            _Running = false;
            _IsStarting = false;
            
            _CurrentSession?.Dispose();
            _Listener?.Stop();

            _CurrentSession = null;
            _Listener = null;
        }

        #endregion
    }

    
}
