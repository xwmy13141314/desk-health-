# DeskHealth - 桌面健康提醒工具

> 一款极简、无干扰的桌面健康提醒工具

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)

## ✨ 特性

- **零配置**：安装即用，默认开启最优策略
- **零打扰**：静音提醒，不抢占焦点，自动消失
- **轻量级**：资源占用极低（内存 < 20MB）
- **半透明提示**：柔和的视觉效果，不影响工作流

## 📦 功能

### MVP 核心功能

- ⏱️ 定时提醒
  
  - 喝水提醒：每 30 分钟
  - 休息提醒：每 60 分钟

- 💧 半透明悬浮窗
  
  - 显示位置：屏幕右下角
  - 尺寸：200 × 80 像素
  - 透明度：70%
  - 自动关闭：10 秒后消失

- 🔔 系统托盘
  
  - 开机自启动
  - 暂停提醒：1 小时 / 2 小时
  - 版本信息展示
  - 一键退出

## 🚀 快速开始

### 开发环境要求

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11 (64位)
- Visual Studio 2022 (推荐) 或 VS Code

### 构建项目

```bash
# 克隆项目
git clone <repository-url>
cd DeskHealth

# 还原依赖
dotnet restore

# 构建项目
dotnet build --configuration Release

# 运行
dotnet run --project src/DeskHealth.App
```

### 发布单文件

```bash
dotnet publish src/DeskHealth.App -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

输出文件位于：`src/DeskHealth.App/bin/Release/net8.0-windows/win-x64/publish/`

## 📁 项目结构

```
DeskHealth/
├── src/
│   ├── DeskHealth.Core/          # 领域层
│   │   ├── Entities/             # 实体
│   │   ├── Events/               # 事件
│   │   └── Interfaces/           # 接口
│   │
│   ├── DeskHealth.Services/      # 服务层
│   │   ├── ConfigService.cs      # 配置服务
│   │   └── TimerService.cs       # 计时器服务
│   │
│   └── DeskHealth.App/           # 表示层 (WPF)
│       ├── Views/                # 窗口
│       ├── Services/             # 应用服务
│       └── Resources/            # 资源
│
├── docs/                         # 文档
│   └── 技术设计文档.md
│
└── tests/                        # 测试
```

## 🎨 技术栈

- **框架**：.NET 8 + WPF
- **架构**：三层架构（领域层、服务层、表示层）
- **依赖注入**：Microsoft.Extensions.DependencyInjection
- **托盘图标**：Hardcodet.Wpf.TaskbarNotification

## 📄 许可证

MIT License

## 🙏 致谢

感谢所有为这个项目做出贡献的人！
