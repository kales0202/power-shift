using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace PowerShift.Services;

public enum PowerMode
{
    Efficiency,
    Balanced,
    Performance,
    Unknown
}

public static class PowerService
{
    // GUIDs
    private static readonly Guid EfficiencyGuid = new Guid("961cc777-2547-4f9d-8174-7d86181b8a7a");
    private static readonly Guid BalancedGuid = new Guid("00000000-0000-0000-0000-000000000000");
    private static readonly Guid PerformanceGuid = new Guid("ded574b5-45a0-4f42-8737-46345c09c238");

    // Registry Paths
    private const string PowerSchemesPath = @"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes";

    public static bool IsOverlaySupported()
    {
        using var key = Registry.LocalMachine.OpenSubKey(PowerSchemesPath);
        // Check if the AC overlay key exists, which indicates support for the slider
        return key?.GetValue("ActiveOverlayAcPowerScheme") != null;
    }

    // P/Invoke
    [DllImport("powrprof.dll", EntryPoint = "PowerSetActiveOverlayScheme")]
    private static extern uint PowerSetActiveOverlayScheme(Guid OverlaySchemeGuid);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS lpSystemPowerStatus);

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEM_POWER_STATUS
    {
        public byte ACLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte SystemStatusFlag;
        public int BatteryLifeTime;
        public int BatteryFullLifeTime;
    }

    public static bool IsAcPower()
    {
        if (GetSystemPowerStatus(out var status))
        {
            return status.ACLineStatus == 1;
        }
        return true; // Default to AC if unknown
    }

    public static PowerMode GetCurrentMode()
    {
        try
        {
            bool isAc = IsAcPower();
            string valueName = isAc ? "ActiveOverlayAcPowerScheme" : "ActiveOverlayDcPowerScheme";

            using var key = Registry.LocalMachine.OpenSubKey(PowerSchemesPath);
            if (key != null)
            {
                string? guidStr = key.GetValue(valueName) as string;
                if (Guid.TryParse(guidStr, out Guid guid))
                {
                    if (guid == EfficiencyGuid) return PowerMode.Efficiency;
                    if (guid == BalancedGuid) return PowerMode.Balanced;
                    if (guid == PerformanceGuid) return PowerMode.Performance;
                }
            }
        }
        catch
        {
            // Ignore errors, return Unknown
        }
        return PowerMode.Unknown;
    }

    public static void SetMode(PowerMode mode)
    {
        Guid targetGuid = mode switch
        {
            PowerMode.Efficiency => EfficiencyGuid,
            PowerMode.Balanced => BalancedGuid,
            PowerMode.Performance => PerformanceGuid,
            _ => BalancedGuid
        };

        // 1. Call API to set effective mode immediately
        PowerSetActiveOverlayScheme(targetGuid);

        // 2. Update Registry for persistence (Only for the CURRENT power source)
        // Note: The API usually updates the registry, but we do it to be sure or if we want to force a specific state.
        // Actually, PowerSetActiveOverlayScheme DOES update the registry for the current scheme (AC or DC).
        // So we might not need to write to registry manually if the API works.
        // However, the user's log showed RegSetValue, which implies the API call triggers that.
        // We will trust the API first.
    }
}
