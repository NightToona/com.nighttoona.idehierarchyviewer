using System;
using System.Net.Sockets;

namespace Editor
{
    public enum SessionState
    {
        Connected,
        Closing,
        Closed
    }
    
    
    public class TcpSession : IDisposable
    {
        public TcpClient Client { get; }
        public NetworkStream Stream { get; }
        
        public SessionState State { get; private set; }
        public bool WaitingForPong { get; set; } = false;

        
        public TcpSession(TcpClient client)
        {
            Client = client;
            Stream = Client.GetStream();
            State = SessionState.Connected;
        }
        
        
        public void Dispose()
        {
            // 释放资源
            if (State == SessionState.Closed) return;

            State = SessionState.Closing;
            Client.Dispose();

            State = SessionState.Closed;
        }
    }
}