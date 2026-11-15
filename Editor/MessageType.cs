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
}

