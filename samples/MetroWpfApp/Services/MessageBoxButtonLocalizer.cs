using Minimal.Mvvm.Windows;
using System.Windows;

namespace MetroWpfApp.Services
{
    public sealed class MessageBoxButtonLocalizer : MessageBoxButtonLocalizerBase
    {
        public override string Localize(MessageBoxResult button)
        {
            return button switch
            {
                MessageBoxResult.OK => Loc.OK,
                MessageBoxResult.Cancel => Loc.Cancel,
                MessageBoxResult.Yes => Loc.Yes,
                MessageBoxResult.No => Loc.No,
                _ => string.Empty
            };
        }
    }
}
