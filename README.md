# PowerShift

English | [中文文档](README_zh-CN.md)

A lightweight Windows system tray utility for quick power mode switching. Seamlessly switch between **Best Power Efficiency**, **Balanced**, and **Best Performance** modes without navigating through the Settings app.

![License](https://img.shields.io/badge/license-MIT-blue.svg) ![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg) ![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)

## ✨ Features

- [x] **🚀 Quick Mode Switching**: Toggle between power modes directly from the system tray
- [x] **🔄 Instant Feedback**: Visual icon indicators for each power mode
- [x] **⚡ Auto Switch**: Automatically switch to efficiency mode when screen off (5min delay), performance mode when screen on (AC power only)
- [x] **🎯 Startup Support**: Optional "Start on Boot" to launch automatically with Windows
- [x] **🌍 Internationalization**: Built-in support for multiple languages (English, Chinese)
- [x] **💾 Lightweight**: Single-file executable with minimal resource footprint

## 📋 Requirements

- **Operating System**: Windows 10/11
- **.NET Runtime**: [.NET 8.0 Runtime (Windows Desktop)](https://dotnet.microsoft.com/download/dotnet/8.0)

> **Note**: Not all Windows devices support the power mode switching feature. PowerShift will display an error message if your device doesn't support this functionality.

## 🚀 Installation

1. Download the latest release from the [Releases](../../releases) page
2. Install [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) if not already installed
3. Run `PowerShift.exe`

## 🌍 Localization

PowerShift supports multiple languages through JSON files in `src/i18n/`:

- `en.json` - English
- `zh-CN.json` - Simplified Chinese

To add a new language, create a new JSON file following the existing format.

## 🛠️ Development

### Project Structure

```
power-shift/
├── src/
│   ├── PowerShift.csproj      # Project file
│   ├── Program.cs             # Entry point
│   ├── ShiftContext.cs        # Main application context
│   ├── Services/
│   │   ├── PowerService.cs    # Power mode management
│   │   ├── BootService.cs     # Startup management
│   │   ├── Localization.cs    # i18n support
│   │   ├── RegistryMonitor.cs # Registry change detection
│   │   ├── AutoSwitchService.cs    # Auto switch on display state
│   │   ├── DisplayMonitorService.cs # Display state monitoring
│   │   └── Logger.cs          # Debug logging (DEBUG build only)
│   ├── Utils/
│   │   └── IconGenerator.cs   # Dynamic icon generation
│   └── i18n/
│       ├── en.json            # English translations
│       └── zh-CN.json         # Chinese translations
└── README.md
```

### Building

```bash
# Clone the repository
git clone https://github.com/kales0202/power-shift.git
cd power-shift

# Development build
dotnet build src/PowerShift.csproj

# Release build
dotnet build src/PowerShift.csproj -c Release

# Publish as single-file executable
dotnet publish src/PowerShift.csproj -c Debug -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish/debug
dotnet publish src/PowerShift.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish/release
```

The compiled executables will be located in `publish/debug/` and `publish/release/`.

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [.NET Windows Forms](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)
- Uses Windows Power Management API (`powrprof.dll`)
- Inspired by the need for quick power mode switching
