using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PowerShift.Services;

/// <summary>
/// Monitor display power state changes
/// by Ai.Coding
/// </summary>
public class DisplayMonitorService : NativeWindow, IDisposable
{
    // GUID_CONSOLE_DISPLAY_STATE
    private static readonly Guid GUID_CONSOLE_DISPLAY_STATE = new("6fe69556-704a-47a0-8f24-c28d936fda47");

    private const int WM_POWERBROADCAST = 0x0218;
    private const int PBT_POWERSETTINGCHANGE = 0x8013;

    private IntPtr _notificationHandle = IntPtr.Zero;

    public event Action<DisplayState>? DisplayStateChanged;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterPowerSettingNotification(IntPtr Handle);

    [StructLayout(LayoutKind.Sequential)]
    private struct POWERBROADCAST_SETTING
    {
        public Guid PowerSetting;
        public uint DataLength;
        public byte Data;
    }

    public DisplayMonitorService()
    {
        CreateHandle(new CreateParams());
        var guid = GUID_CONSOLE_DISPLAY_STATE;
        _notificationHandle = RegisterPowerSettingNotification(Handle, ref guid, 0);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_POWERBROADCAST && m.WParam.ToInt32() == PBT_POWERSETTINGCHANGE)
        {
            var setting = Marshal.PtrToStructure<POWERBROADCAST_SETTING>(m.LParam);
            if (setting.PowerSetting == GUID_CONSOLE_DISPLAY_STATE)
            {
                var state = (DisplayState)setting.Data;
                DisplayStateChanged?.Invoke(state);
            }
        }
        base.WndProc(ref m);
    }

    public void Dispose()
    {
        if (_notificationHandle != IntPtr.Zero)
        {
            UnregisterPowerSettingNotification(_notificationHandle);
            _notificationHandle = IntPtr.Zero;
        }
        DestroyHandle();
    }
}

public enum DisplayState
{
    Off = 0,
    On = 1,
    Dimmed = 2
}
