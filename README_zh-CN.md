# PowerShift

[English](README.md) | 中文文档

一个轻量级的 Windows 系统托盘工具，用于快速切换电源模式。无需进入设置应用，即可在**最佳能效**、**平衡**和**最佳性能**模式之间无缝切换。

![License](https://img.shields.io/badge/license-MIT-blue.svg) ![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg) ![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)

## ✨ 功能特性

- [x] **🚀 快速模式切换**：直接从系统托盘切换电源模式
- [x] **🔄 即时反馈**：每种电源模式都有可视化的图标指示
- [x] **🎯 开机自启**：可选“开机启动”，随 Windows 自动运行
- [x] **🌍 多语言支持**：内置多语言支持（英语、中文）
- [x] **💾 轻量级**：单文件可执行程序，资源占用极低

## 📋 系统要求

- **操作系统**：Windows 10/11
- **.NET 运行时**：[.NET 8.0 Runtime (Windows Desktop)](https://dotnet.microsoft.com/download/dotnet/8.0)

> **注意**：并非所有 Windows 设备都支持电源模式切换功能。如果您的设备不支持此功能，PowerShift 将显示错误消息。

## 🚀 安装指南

1. 从 [Releases](../../releases) 页面下载最新版本
2. 如果尚未安装，请安装 [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
4. 运行 `PowerShift.exe`

## 🌍 本地化

PowerShift 通过 `src/i18n/` 中的 JSON 文件支持多语言：

- `en.json` - 英语
- `zh-CN.json` - 简体中文

要添加新语言，请按照现有格式创建一个新的 JSON 文件。

## 🛠️ 开发

### 项目结构

```
power-mode/
├── src/
│   ├── PowerShift.csproj      # 项目文件
│   ├── Program.cs             # 入口点
│   ├── ShiftContext.cs        # 主应用程序上下文
│   ├── Services/
│   │   ├── PowerService.cs    # 电源模式管理
│   │   ├── BootService.cs     # 启动管理
│   │   ├── Localization.cs    # i18n 支持
│   │   └── RegistryMonitor.cs # 注册表更改检测
│   ├── Utils/
│   │   └── IconGenerator.cs   # 动态图标生成
│   └── i18n/
│       ├── en.json            # 英语翻译
│       └── zh-CN.json         # 中文翻译
└── README.md
```

### 构建

```bash
# 克隆仓库
git clone https://github.com/yourusername/power-mode.git
cd power-mode/src

# 开发构建
dotnet build

# 发布构建
dotnet build -c Release

# 发布为单文件可执行程序
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

编译后的可执行文件位于 `src/bin/Release/net8.0-windows/win-x64/publish/`。

## 📝 许可证

本项目采用 MIT 许可证 - 详情请参阅 [LICENSE](LICENSE) 文件。

## 🙏 致谢

- 基于 [.NET Windows Forms](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/) 构建
- 使用 Windows 电源管理 API (`powrprof.dll`)
- 灵感来自于对快速电源模式切换的需求
