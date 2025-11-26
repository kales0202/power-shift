using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace PowerShift.Services;

public static class BootService
{
    private const string AppName = "PowerShift";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsBootEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
        return key?.GetValue(AppName) != null;
    }

    public static void SetBoot(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
        if (key == null) return;

        if (enable)
        {
            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? Application.ExecutablePath;
            key.SetValue(AppName, $"\"{exePath}\"");
        }
        else
        {
            key.DeleteValue(AppName, false);
        }
    }

    public static void SelfHeal()
    {
        // If boot is enabled, ensure the path is correct
        if (IsBootEnabled())
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key != null)
            {
                string? currentVal = key.GetValue(AppName) as string;
                string currentExe = Process.GetCurrentProcess().MainModule?.FileName ?? Application.ExecutablePath;
                string expectedVal = $"\"{currentExe}\"";

                if (!string.Equals(currentVal, expectedVal, StringComparison.OrdinalIgnoreCase))
                {
                    // Path mismatch (user moved the file), update it
                    key.SetValue(AppName, expectedVal);
                }
            }
        }
    }
}
