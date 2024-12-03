using Minimal.Mvvm;

namespace MovieWpfApp.Models
{
    public sealed partial class AppSettings : SettingsBase
    {
        [Notify]
        private string? _appTheme;
    }
}
