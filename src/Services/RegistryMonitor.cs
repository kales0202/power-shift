using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace PowerShift.Services;

public class RegistryMonitor : IDisposable
{
    private const int REG_NOTIFY_CHANGE_LAST_SET = 0x00000004;
    
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegNotifyChangeKeyValue(
        IntPtr hKey,
        bool bWatchSubtree,
        int dwNotifyFilter,
        IntPtr hEvent,
        bool fAsynchronous);

    private readonly RegistryKey _registryKey;
    private readonly AutoResetEvent _eventHandle;
    private readonly Thread _monitorThread;
    private bool _running;

    public event EventHandler? Changed;

    public RegistryMonitor(RegistryKey registryKey)
    {
        _registryKey = registryKey;
        _eventHandle = new AutoResetEvent(false);
        _running = true;
        
        _monitorThread = new Thread(MonitorLoop)
        {
            IsBackground = true,
            Name = "RegistryMonitor"
        };
        _monitorThread.Start();
    }

    private void MonitorLoop()
    {
        try
        {
            IntPtr hKey = _registryKey.Handle.DangerousGetHandle();
            
            while (_running)
            {
                // Register for change notification
                int result = RegNotifyChangeKeyValue(
                    hKey,
                    false, // Don't watch subtree
                    REG_NOTIFY_CHANGE_LAST_SET, // Watch for value changes
                    _eventHandle.SafeWaitHandle.DangerousGetHandle(),
                    true); // Asynchronous

                if (result != 0)
                {
                    // Error, stop monitoring
                    break;
                }

                // Wait for change
                if (_eventHandle.WaitOne())
                {
                    if (!_running) break;
                    
                    // Notify subscribers
                    Changed?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        catch
        {
            // Silently handle errors
        }
    }

    public void Dispose()
    {
        _running = false;
        _eventHandle.Set(); // Wake up the thread
        
        if (_monitorThread.IsAlive)
        {
            _monitorThread.Join(1000); // Wait up to 1 second
        }
        
        _eventHandle.Dispose();
        _registryKey.Dispose();
    }
}
