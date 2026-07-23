using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// 限制类型 （1-XML; 2-Heartbeat; 3-Close; 0-None）
    /// </summary>
    public enum MessageType
    {
        None = 0,
        XML = 1,
        Heartbeat = 2,
        Close = 3
    }

    
    [InitializeOnLoad]
    public class TcpServer
    {
        private static TcpListener _Listener;
        private static TcpClient _Client;
        private static NetworkStream _Stream;

        private static bool _Running;

        static TcpServer()
        {
            EditorApplication.quitting += Stop;
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            
            _ = StartListen(HierarchySettingWindow.port);
        }

        
        private static async Task StartListen(int port)
        {
            _Listener = new TcpListener(IPAddress.Loopback, port);
            _Listener.Start();

            _Running = true;
            
            while (_Running)
            {
                _Client = await _Listener.AcceptTcpClientAsync();
                _Stream = _Client.GetStream();
                
                // 检测是否还在连接状态
                await ReceiveLoop();
            
                _Stream.Close();
                _Client.Close();
            }
        }


        /// <summary>
        /// 发送数据到客户端 - TCP
        /// </summary>
        /// <param name="message">string</param>
        /// <param name="type">MessageType</param>
        public static async Task SendMessage(MessageType type, string message)
        {
            if (_Client == null || _Stream == null)
            {
                Debug.Log("No Client Connected.");
                return;
            }

            message = $"{(int)type}|" + message;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

            await _Stream.WriteAsync(data);
            await _Stream.FlushAsync();
        }

        // 监听检测是否释放客户端
        private static async Task ReceiveLoop()
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                Array.Clear(buffer, 0, buffer.Length); // 清理残余缓存
                
                int length = await _Stream.ReadAsync(buffer);

                // FIN返回
                if (length == 0)
                {
                    break;
                }

                string message = System.Text.Encoding.UTF8.GetString(buffer, 0, length);
                Debug.Log($"Receive {message}");
            }
        }

        
        // 关闭编辑器、重新编译后结束旧生命周期残余内容
        private static void Stop()
        {
            _Running = false;
            
            _Stream?.Close();
            _Client?.Close();
            _Listener?.Stop();

            _Stream = null;
            _Client = null;
            _Listener = null;
        }
    }

    
}