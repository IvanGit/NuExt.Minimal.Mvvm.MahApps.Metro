using Minimal.Mvvm;

namespace MovieWpfApp.Models
{
    public sealed partial class MoviesSettings : SettingsBase
    {
        [Notify]
        private string? _selectedPath;
    }
}
