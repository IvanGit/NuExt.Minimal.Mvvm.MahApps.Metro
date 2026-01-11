using MahApps.Metro.Controls.Dialogs;
using Minimal.Mvvm;
using Minimal.Mvvm.Windows;
using MovieWpfApp.Models;
using System.ComponentModel;
using System.Diagnostics;
using static AccessModifier;

namespace MovieWpfApp.ViewModels
{
    internal sealed partial class MovieViewModel : DocumentContentViewModelBase
    {
        #region Properties

        [Notify(Setter = Private)]
        private bool _isWindowed;

        public MovieModel Movie => (MovieModel)Parameter!;

        #endregion

        #region Services

        private IDialogCoordinator DialogCoordinator => GetService<IDialogCoordinator>()!;

        private WindowService? WindowService => GetService<WindowService>();

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

        public override async ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            VerifyAccess();

            Debug.Assert(DialogCoordinator != null, $"{nameof(DialogCoordinator)} is null");

            var dialogSettings = new MetroDialogSettings
            {
                CancellationToken = cancellationToken,
                AffirmativeButtonText = Loc.Yes,
                NegativeButtonText = Loc.No,
            };

            var dialogResult = await DialogCoordinator!.ShowMessageAsync(this, Loc.Confirmation,
                string.Format(Loc.Are_you_sure_you_want_to_close__Arg0__, Movie.Name),
                MessageDialogStyle.AffirmativeAndNegative, dialogSettings);
            if (dialogResult != MessageDialogResult.Affirmative)
            {
                return false;
            }

            return await base.CanCloseAsync(cancellationToken).ConfigureAwait(false);
        }

        protected override async Task InitializeAsyncCore(CancellationToken cancellationToken)
        {
            Debug.Assert(Parameter is MovieModel);
            await base.InitializeAsyncCore(cancellationToken);
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
