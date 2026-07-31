using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// 限制类型 （1-XML; 2-HeartbeatPing; 3-HeartbeatPong; 4-Close; 0-None）
    /// </summary>
    public enum MessageType
    {
        None = 0,
        XML = 1,
        HeartbeatPing = 2,
        HeartbeatPong = 3,
        Close = 4
    }

    
    [InitializeOnLoad]
    public class TcpServer
    {
        private static TcpListener _Listener;
        private static TcpClient _Client;
        private static NetworkStream _Stream;

        private static bool _Running;
        private static bool _IsStarting;
        
        private static bool _WaitingForPong;
        
        

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
                _Client = await _Listener.AcceptTcpClientAsync();
                _Stream = _Client.GetStream();
                _WaitingForPong = false;
                
                Debug.Log("成功连接到客户端");

                // 启动心跳检测以及接收消息循环
                _ = HeartbeatLoop(_Client);
                await ReceiveLoop();
            
                _Stream.Close();
                _Client.Close();
                Debug.Log("旧客户端已断开，等待二次连接……");
            }
        }

        

        /// <summary>
        /// 发送消息到客户端
        /// </summary>
        /// <param name="message">String or null</param>
        /// <param name="type">MessageType</param>
        public static async Task SendMessage(MessageType type, string message = "")
        {
            if (_Client == null || _Stream == null) return;
            

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

            await _Stream.WriteAsync(data);
            await _Stream.FlushAsync();
        }
        
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
        
        

        // 监听检测是否释放客户端
        private static async Task ReceiveLoop()
        {
            byte[] buffer = new byte[1024];

            // 当心跳停止时，ReadAsync会返回异常，通过try-catch捕获异常并结束循环
            try
            {
                while (true)
                {
                    Array.Clear(buffer, 0, buffer.Length); // 清理残余缓存
                
                    int length = await _Stream.ReadAsync(buffer);
                    if (length == 0) break; // FIN返回
                    
                    
                    string message = Encoding.UTF8.GetString(buffer, 0, length);

                    if (message == "[3][0]") // 收到PONG
                    {
                        _WaitingForPong = false;
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
        
        
        // 心跳检测
        private static async Task HeartbeatLoop(TcpClient client)
        {
            try
            {
                // 保证新旧客户端切换时不会干扰，且在客户端断开连接时停止心跳
                while (_Running && _Client == client && _Stream != null)
                {
                    await Task.Delay(HierarchySettingWindow.heartbeatIntervalSeconds * 1000);
                    if (!_Running || _Client != client || _Stream == null) break; // 防止等待期间状态变化

                    _WaitingForPong = true;
                    await SendMessage(MessageType.HeartbeatPing);

                    await Task.Delay(5000); // 等待5秒钟，如果没有收到Pong则认为客户端断开连接
                    if (!_Running || _Client != client || _Stream == null) break; // 防止等待期间状态变化

                    if (_WaitingForPong)
                    {
                        Debug.Log("Client didn't respond. Closing connection.");
                        _Stream?.Close();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        

        // 关闭编辑器、重新编译后结束旧生命周期残余内容
        private static void Stop()
        {
            _Running = false;
            _IsStarting = false;
            
            _Stream?.Close();
            _Client?.Close();
            _Listener?.Stop();

            _Stream = null;
            _Client = null;
            _Listener = null;
        }
    }

    
}