# IDE Hierarchy Viewer

A Unity Editor package that exports the current Scene Hierarchy to external IDE plugins through TCP.

用于将 Unity Hierarchy 数据同步给 Rider、Visual Studio 等外部 IDE 插件（暂仅支持单连接）。

> 当前状态：Unity 服务端 MVP 已完成，IDE 客户端基本实现。

## Features

- 监听 Unity Hierarchy 变化
- 递归构建 GameObject 层级结构
- 将数据序列化为 UTF-8 XML
- 通过本地 TCP 发送数据
- 支持客户端重新连接
- 支持 Ping/Pong 心跳检测
- 支持自定义端口和心跳间隔

## Project Detail MD

[Projects/Project 1 - Hierarchy Tool](https://github.com/NightToona/UnityClient-Engineering-Notes/blob/0089f13df53eada2bcab9eb2dff6a382e4bb3bfd/Projects/Project%201%20-%20Hierarchy%20Tool.md)

## Usage

安装 Package 后，TCP Server 会自动启动。

界面样式：

<img width="540" height="173" alt="image" src="https://github.com/user-attachments/assets/195f3c75-2259-4b69-a689-d53cbcad9cfb" />


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
该项目为 Unity侧 服务端 Demo/MVP。
待完成：
- TCP 接收拆包处理
- 结构与性能优化

IDE端项目已上线开放：
[Rider Client](https://github.com/NightToona/UnityHierarchyViewerClient)
