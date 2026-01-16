using System;
using System.IO;

namespace PowerShift.Services;

public static class Logger
{
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "power-shift.log");

    [System.Diagnostics.Conditional("DEBUG")]
    public static void Log(string message)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
        }
        catch { }
    }
}
