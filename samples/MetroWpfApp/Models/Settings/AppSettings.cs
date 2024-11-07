using Minimal.Mvvm;

namespace MetroWpfApp.Models
{
    public sealed class AppSettings : SettingsBase
    {
        private string? _appTheme;
        public string? AppTheme
        {
            get => _appTheme;
            set => SetProperty(ref _appTheme, value);
        }
    }
}
