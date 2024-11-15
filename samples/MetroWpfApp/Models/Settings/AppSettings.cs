using Minimal.Mvvm;

namespace MetroWpfApp.Models
{
    public sealed partial class AppSettings : SettingsBase
    {
        [Notify]
        private string? _appTheme;
    }
}
