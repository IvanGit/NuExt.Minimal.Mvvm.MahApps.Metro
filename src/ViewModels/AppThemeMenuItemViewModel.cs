using System.Windows.Media;

namespace Minimal.Mvvm.Windows
{
    public partial class AccentColorMenuItemViewModel : MenuItemViewModel
    {
        [Notify] private Brush? _borderColorBrush;
        [Notify] private Brush? _colorBrush;
    }

    public class AppThemeMenuItemViewModel : AccentColorMenuItemViewModel
    {

    }
}
