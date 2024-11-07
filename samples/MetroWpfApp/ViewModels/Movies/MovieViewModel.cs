using MetroWpfApp.Models;
using Minimal.Mvvm.Windows;
using System.ComponentModel;
using System.Diagnostics;

namespace MetroWpfApp.ViewModels
{
    internal sealed partial class MovieViewModel : DocumentContentViewModelBase
    {
        #region Properties

        public MovieModel Movie => (MovieModel)Parameter!;

        #endregion

        #region Event Handlers

        private void Movie_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(MovieModel.Name) or nameof(MovieModel.ReleaseDate))
            {
                UpdateTitle();
            }
        }

        #endregion

        #region Methods

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(Parameter is MovieModel);
            await base.OnInitializeAsync(cancellationToken);
            Lifetime.AddBracket(() => Movie.PropertyChanged += Movie_PropertyChanged,
                () => Movie.PropertyChanged -= Movie_PropertyChanged);
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            Title = $"{Movie.Name} [{Movie.ReleaseDate:yyyy}]";
        }

        #endregion
    }
}
