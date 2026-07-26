# IDE Hierarchy Viewer

A Unity Editor package that exports the current Scene Hierarchy to external IDE plugins through TCP.

用于将 Unity Hierarchy 数据同步给 Rider、Visual Studio 等外部 IDE 插件。

> 当前状态：Unity 服务端 MVP 已完成，IDE 客户端仍在开发中。

## Features

- 监听 Unity Hierarchy 变化
- 递归构建 GameObject 层级结构
- 将数据序列化为 UTF-8 XML
- 通过本地 TCP 发送数据
- 支持客户端重新连接
- 支持 Ping/Pong 心跳检测
- 支持自定义端口和心跳间隔

## Usage

安装 Package 后，TCP Server 会自动启动。

默认地址：

```text
127.0.0.1:44571
```

设置入口：
```
Tools > IDE Hierarchy Viewer Setting > Settings
```

消息格式：
```
[MessageType][UTF8BodyLength]Body
```

## Status
目前为单客户端 Demo/MVP。
待完成：
- TCP 完整拆包处理
- IDE 客户端插件
- 连接与消息发送结构优化