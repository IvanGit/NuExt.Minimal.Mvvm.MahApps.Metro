using Minimal.Mvvm;

namespace MetroWpfApp.Models
{
    public sealed partial class MoviesSettings: SettingsBase
    {
        [Notify]
        private string? _selectedPath;
    }
}
