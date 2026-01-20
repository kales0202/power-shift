using Microsoft.Win32;
using PowerShift.Services;
using PowerShift.Utils;
using System;
using System.Windows.Forms;

namespace PowerShift;

/// <summary>
/// Tray application context
/// by Ai.Coding
/// </summary>
public class ShiftContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private RegistryMonitor? _registryMonitor;
    private readonly AutoSwitchService _autoSwitchService;

    // Menu Items
    private readonly ToolStripMenuItem _itemBoot;
    private readonly ToolStripMenuItem _itemStatus;
    private readonly ToolStripMenuItem _itemEfficiency;
    private readonly ToolStripMenuItem _itemBalanced;
    private readonly ToolStripMenuItem _itemPerformance;
    private readonly ToolStripMenuItem _itemAutoSwitch;

    public ShiftContext()
    {
        // 1. Initialize Services
        _autoSwitchService = new AutoSwitchService();

        // 2. Initialize Menu
        _contextMenu = new ContextMenuStrip
        {
            Renderer = new BorderlessCheckRenderer()
        };

        _itemBoot = new ToolStripMenuItem(Localization.MenuStartOnBoot, null, OnToggleBoot);
        _itemStatus = new ToolStripMenuItem("...");
        _itemStatus.Enabled = false; // Info only

        _itemEfficiency = new ToolStripMenuItem(Localization.MenuEfficiency, IconGenerator.GenerateBitmap(PowerMode.Efficiency), (s, e) => SetMode(PowerMode.Efficiency));
        _itemBalanced = new ToolStripMenuItem(Localization.MenuBalanced, IconGenerator.GenerateBitmap(PowerMode.Balanced), (s, e) => SetMode(PowerMode.Balanced));
        _itemPerformance = new ToolStripMenuItem(Localization.MenuPerformance, IconGenerator.GenerateBitmap(PowerMode.Performance), (s, e) => SetMode(PowerMode.Performance));

        _itemAutoSwitch = new ToolStripMenuItem(Localization.MenuAutoSwitch, null, OnToggleAutoSwitch);
        var itemAutoSwitchDesc = new ToolStripMenuItem(Localization.MenuAutoSwitchDesc) { Enabled = false };

        var itemExit = new ToolStripMenuItem(Localization.MenuExit, null, OnExit);

        _contextMenu.Items.Add(_itemBoot);
        _contextMenu.Items.Add(_itemStatus);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(_itemAutoSwitch);
        _contextMenu.Items.Add(itemAutoSwitchDesc);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(_itemEfficiency);
        _contextMenu.Items.Add(_itemBalanced);
        _contextMenu.Items.Add(_itemPerformance);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(itemExit);

        // 3. Initialize Icon
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        var versionString = version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v0.0.1";
        
        _notifyIcon = new NotifyIcon
        {
            Text = $"{Localization.AppName} {versionString}",
            Icon = IconGenerator.Generate(PowerMode.Unknown),
            ContextMenuStrip = _contextMenu,
            Visible = true
        };

        // 4. Bind System Events
        SystemEvents.PowerModeChanged += OnSystemPowerChanged;
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        
        // 5. Start Registry Monitor for instant updates
        try
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes");
            if (key != null)
            {
                _registryMonitor = new RegistryMonitor(key);
                _registryMonitor.Changed += (s, e) => RefreshState();
            }
        }
        catch
        {
            // If registry monitoring fails, rely on events only
        }

        // 6. Initial State
        BootService.SelfHeal(); // Fix registry path if needed
        RefreshState();
    }

    private void RefreshState()
    {
        // Update Boot Checkbox
        _itemBoot.Checked = BootService.IsBootEnabled();

        // Update Auto Switch Checkbox
        _itemAutoSwitch.Checked = _autoSwitchService.IsEnabled;

        // Update Power Source Status
        bool isAc = PowerService.IsAcPower();
        _itemStatus.Text = isAc ? Localization.MenuStatusAC : Localization.MenuStatusDC;

        // Update Mode Selection - read current power source's mode
        var currentMode = PowerService.GetCurrentMode();
        
        _itemEfficiency.Checked = currentMode == PowerMode.Efficiency;
        _itemBalanced.Checked = currentMode == PowerMode.Balanced;
        _itemPerformance.Checked = currentMode == PowerMode.Performance;

        // Update Icon
        var newIcon = IconGenerator.Generate(currentMode);
        var oldIcon = _notifyIcon.Icon;
        _notifyIcon.Icon = newIcon;
        
        // Dispose old icon to prevent leak
        if (oldIcon != null && oldIcon != newIcon)
        {
            oldIcon.Dispose();
        }
    }

    private void SetMode(PowerMode mode)
    {
        Logger.Log($"Manual: SetMode to {mode}");
        PowerService.SetMode(mode);
        RefreshState();
    }

    private void OnToggleBoot(object? sender, EventArgs e)
    {
        bool newState = !_itemBoot.Checked;
        Logger.Log($"Manual: ToggleBoot to {newState}");
        BootService.SetBoot(newState);
        RefreshState();
    }

    private void OnToggleAutoSwitch(object? sender, EventArgs e)
    {
        var newState = !_autoSwitchService.IsEnabled;
        Logger.Log($"Manual: ToggleAutoSwitch to {newState}");
        _autoSwitchService.IsEnabled = newState;
        RefreshState();
    }

    private void OnSystemPowerChanged(object sender, PowerModeChangedEventArgs e)
    {
        // When user plugs/unplugs, or system wakes up
        RefreshState();
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        // General settings change, good time to refresh just in case
        if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Power)
        {
            RefreshState();
        }
    }

    private void OnExit(object? sender, EventArgs e)
    {
        _notifyIcon.Visible = false;
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SystemEvents.PowerModeChanged -= OnSystemPowerChanged;
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            _registryMonitor?.Dispose();
            _autoSwitchService.Dispose();
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
        }
        base.Dispose(disposing);
    }
}
