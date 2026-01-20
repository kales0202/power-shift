using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using PowerShift.Services;

namespace PowerShift.Utils;

public static class IconGenerator
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private static Color GetColor(PowerMode mode)
    {
        return mode switch
        {
            PowerMode.Efficiency => Color.FromArgb(0, 200, 80),      // Green
            PowerMode.Balanced => Color.FromArgb(0, 120, 215),       // Blue
            PowerMode.Performance => Color.FromArgb(232, 17, 35),    // Red
            _ => Color.Gray
        };
    }

    private static int GetDpiAwareSize(int baseSize)
    {
        // Get system DPI scaling
        using var g = Graphics.FromHwnd(IntPtr.Zero);
        float scale = g.DpiX / 96f; // 96 DPI is 100% scaling
        return (int)(baseSize * scale);
    }

    public static Icon Generate(PowerMode mode)
    {
        int size = GetDpiAwareSize(16); // System shift icons are typically 16x16 at 100% DPI
        using var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Color color = GetColor(mode);

        // Draw a simple filled circle
        using var brush = new SolidBrush(color);
        int margin = Math.Max(2, size / 8);
        g.FillEllipse(brush, margin, margin, size - margin * 2, size - margin * 2);

        IntPtr hIcon = bitmap.GetHicon();
        Icon icon = Icon.FromHandle(hIcon);
        
        // Clone the icon so we can destroy the handle immediately
        Icon clonedIcon = (Icon)icon.Clone();
        
        // Destroy the GDI handle to prevent leak
        DestroyIcon(hIcon);
        
        return clonedIcon;
    }
    
    public static Bitmap GenerateBitmap(PowerMode mode, int size = 16)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Color color = GetColor(mode);

        // Draw a simple filled circle
        using var brush = new SolidBrush(color);
        g.FillEllipse(brush, 2, 2, size - 4, size - 4);
        
        return bmp;
    }
}
