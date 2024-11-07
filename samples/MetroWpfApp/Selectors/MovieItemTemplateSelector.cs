using MetroWpfApp.Models;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MetroWpfApp.Selectors
{
    public sealed class MovieItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? GroupDataTemplate { get; set; }

        public DataTemplate? ItemDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Debug.Assert(container is FrameworkElement);
            return item switch
            {
                MovieGroupModel => GroupDataTemplate,
                MovieModel => ItemDataTemplate,
                _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
            } ?? throw new ArgumentNullException(nameof(item));
        }
    }
}
