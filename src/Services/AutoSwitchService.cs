using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace PowerShift.Services;

/// <summary>
/// Auto switch power mode service: switch to efficiency when display off, restore previous mode when display on
/// by Ai.Coding
/// </summary>
public class AutoSwitchService : IDisposable
{
    private const string SettingsKeyPath = @"Software\PowerShift";
    private const string AutoSwitchValueName = "AutoSwitch";
    private const int DelayMinutes = 5;

    private readonly DisplayMonitorService _displayMonitor;
    private readonly System.Windows.Forms.Timer _delayTimer;
    private bool _isEnabled;
    private bool _initialized;
    private bool _efficiencyApplied;
    private bool _disposed;
    private PowerMode? _savedAcMode;
    private PowerMode? _savedDcMode;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            SaveSetting(value);
            Logger.Log($"AutoSwitch: {(value ? "enabled" : "disabled")}");
            if (!value && _delayTimer.Enabled)
            {
                Logger.Log("AutoSwitch disabled, stopping timer");
                _delayTimer.Stop();
            }
        }
    }

    public AutoSwitchService()
    {
        _isEnabled = LoadSetting();
        Logger.Log($"AutoSwitchService initialized, IsEnabled={_isEnabled}");
        _displayMonitor = new DisplayMonitorService();
        _displayMonitor.DisplayStateChanged += OnDisplayStateChanged;

        _delayTimer = new System.Windows.Forms.Timer { Interval = DelayMinutes * 60 * 1000 };
        _delayTimer.Tick += OnDelayTimerTick;

        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        var isAc = PowerService.IsAcPower();
        Logger.Log($"PowerModeChanged: {e.Mode}, IsAC={isAc}");
        // Don't stop timer or clear saved mode - let it work on both AC and DC
    }

    private void OnDisplayStateChanged(DisplayState state)
    {
        Logger.Log($"DisplayStateChanged: {state}, IsEnabled={_isEnabled}, IsAC={PowerService.IsAcPower()}");

        // Ignore initial display state notification
        if (!_initialized)
        {
            _initialized = true;
            Logger.Log("Ignoring initial display state");
            return;
        }

        if (!_isEnabled) return;

        var isAc = PowerService.IsAcPower();

        switch (state)
        {
            case DisplayState.Off:
                if (!_delayTimer.Enabled)
                {
                    // Save both AC and DC modes
                    var acMode = PowerService.GetMode(true);
                    var dcMode = PowerService.GetMode(false);

                    if (acMode != PowerMode.Efficiency)
                    {
                        _savedAcMode = acMode;
                        Logger.Log($"Display off, saved AC mode: {acMode}");
                    }
                    else
                    {
                        Logger.Log($"Display off, AC mode: {acMode}, not saved");
                    }

                    if (dcMode != PowerMode.Efficiency)
                    {
                        _savedDcMode = dcMode;
                        Logger.Log($"Display off, saved DC mode: {dcMode}");
                    }
                    else
                    {
                        Logger.Log($"Display off, DC mode: {dcMode}, not saved");
                    }

                    _delayTimer.Start();
                }
                break;
            case DisplayState.On:
                if (_delayTimer.Enabled)
                {
                    Logger.Log("Display on, timer stopped");
                    _delayTimer.Stop();
                }

                // Only restore if efficiency mode was actually applied
                if (_efficiencyApplied)
                {
                    var savedMode = isAc ? _savedAcMode : _savedDcMode;
                    if (savedMode != null)
                    {
                        Logger.Log($"Display on ({(isAc ? "AC" : "DC")}), restoring mode: {savedMode}");
                        PowerService.SetMode(savedMode.Value);
                    }
                    else
                    {
                        Logger.Log($"Display on ({(isAc ? "AC" : "DC")}), no saved mode, restoring to Balanced");
                        PowerService.SetMode(PowerMode.Balanced);
                    }
                }

                // Clear saved modes and reset flag
                _savedAcMode = null;
                _savedDcMode = null;
                _efficiencyApplied = false;
                break;
            // DisplayState.Dimmed - no action
        }
    }

    private void OnDelayTimerTick(object? sender, EventArgs e)
    {
        _delayTimer.Stop();
        if (_isEnabled)
        {
            Logger.Log("Timer elapsed, switching to Efficiency");
            if (PowerService.SetMode(PowerMode.Efficiency))
            {
                _efficiencyApplied = true;
            }
        }
    }

    private static bool LoadSetting()
    {
        using var key = Registry.CurrentUser.OpenSubKey(SettingsKeyPath);
        return key?.GetValue(AutoSwitchValueName) is int val && val == 1;
    }

    private static void SaveSetting(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(SettingsKeyPath);
        key?.SetValue(AutoSwitchValueName, enabled ? 1 : 0, RegistryValueKind.DWord);
    }

    public void Dispose()
    {
        if (_disposed) return;

        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        _delayTimer.Stop();
        _delayTimer.Dispose();
        _displayMonitor.DisplayStateChanged -= OnDisplayStateChanged;
        _displayMonitor.Dispose();

        _disposed = true;
    }
}
