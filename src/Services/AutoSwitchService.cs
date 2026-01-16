using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace PowerShift.Services;

/// <summary>
/// Auto switch power mode service: switch to efficiency when display off, performance when display on
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
        if (!isAc && _delayTimer.Enabled)
        {
            Logger.Log("Switched to DC power, stopping timer");
            _delayTimer.Stop();
        }
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

        if (!_isEnabled || !PowerService.IsAcPower()) return;

        switch (state)
        {
            case DisplayState.Off:
                if (!_delayTimer.Enabled)
                {
                    Logger.Log("Display off, starting delay timer");
                    _delayTimer.Start();
                }
                break;
            case DisplayState.On:
                if (_delayTimer.Enabled)
                {
                    Logger.Log("Display on, stopping timer");
                    _delayTimer.Stop();
                }
                Logger.Log("Display on, switching to Performance");
                PowerService.SetMode(PowerMode.Performance);
                break;
            // DisplayState.Dimmed - no action
        }
    }

    private void OnDelayTimerTick(object? sender, EventArgs e)
    {
        _delayTimer.Stop();
        if (_isEnabled && PowerService.IsAcPower())
        {
            Logger.Log("Timer elapsed, switching to Efficiency");
            PowerService.SetMode(PowerMode.Efficiency);
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
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        _delayTimer.Stop();
        _delayTimer.Dispose();
        _displayMonitor.DisplayStateChanged -= OnDisplayStateChanged;
        _displayMonitor.Dispose();
    }
}
