using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace PowerShift.Services;

public static class Localization
{
    private static readonly Dictionary<string, string> Strings = new();

    static Localization()
    {
        LoadStrings();
    }

    private static void LoadStrings()
    {
        try
        {
            // Determine language
            var culture = CultureInfo.CurrentUICulture.Name; // e.g. "zh-CN", "en-US"
            
            // Try exact match first (e.g. "zh-CN")
            if (!TryLoadLanguage(culture))
            {
                // Try two-letter code (e.g. "zh")
                var twoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                if (!TryLoadLanguage(twoLetter))
                {
                    // Fallback to English
                    TryLoadLanguage("en");
                }
            }
        }
        catch
        {
            // If all fails, use empty strings (better than crashing)
        }
    }

    private static bool TryLoadLanguage(string langCode)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"PowerShift.i18n.{langCode}.json";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return false;

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            
            if (dict != null)
            {
                foreach (var kvp in dict)
                {
                    Strings[kvp.Key] = kvp.Value;
                }
                return true;
            }
        }
        catch
        {
            // Ignore and try next
        }
        return false;
    }

    private static string Get(string key, string fallback = "")
    {
        return Strings.TryGetValue(key, out var value) ? value : fallback;
    }

    // Menu items
    public static string MenuStartOnBoot => Get("MenuStartOnBoot", "Start on Boot");
    public static string MenuStatusAC => Get("MenuStatusAC", "Status: AC Power");
    public static string MenuStatusDC => Get("MenuStatusDC", "Status: Battery");
    public static string MenuEfficiency => Get("MenuEfficiency", "Best Efficiency");
    public static string MenuBalanced => Get("MenuBalanced", "Balanced");
    public static string MenuPerformance => Get("MenuPerformance", "Best Performance");
    public static string MenuExit => Get("MenuExit", "Exit");

    // Messages
    public static string AppName => Get("AppName", "PowerShift");
    public static string ErrorNotSupported => Get("ErrorNotSupported", 
        "Your device does not support power mode switching.\n(ActiveOverlayAcPowerScheme not detected)");
    public static string ErrorNotSupportedTitle => Get("ErrorNotSupportedTitle", "Not Supported");
}
