using System.Windows.Media;

namespace Minimal.Mvvm.Windows
{
    public class AccentColorMenuItemViewModel : MenuItemViewModel
    {
        public Brush? BorderColorBrush { get; set; }

        public Brush? ColorBrush { get; set; }
    }

    public class AppThemeMenuItemViewModel : AccentColorMenuItemViewModel
    {

    }
}
