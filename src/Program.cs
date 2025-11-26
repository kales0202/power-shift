using System;
using System.Threading;
using System.Windows.Forms;

namespace PowerShift;

static class Program
{
    private static Mutex? _mutex = null;

    [STAThread]
    static void Main()
    {
        const string appName = "Global\\PowerShift_961cc777-2547-4f9d-8174-7d86181b8a7a";
        bool createdNew;

        _mutex = new Mutex(true, appName, out createdNew);

        if (!createdNew)
        {
            // App is already running
            return;
        }

        if (!PowerShift.Services.PowerService.IsOverlaySupported())
        {
            MessageBox.Show(PowerShift.Services.Localization.ErrorNotSupported, 
                            $"{PowerShift.Services.Localization.AppName} - {PowerShift.Services.Localization.ErrorNotSupportedTitle}", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new ShiftContext());
    }
}
