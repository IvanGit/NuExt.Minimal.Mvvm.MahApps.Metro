using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Minimal.Mvvm;
using Minimal.Mvvm.Windows;
using MovieWpfApp.Models;
using MovieWpfApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using static AccessModifier;

namespace MovieWpfApp.ViewModels
{
    internal sealed partial class MoviesViewModel : DocumentContentViewModelBase
    {
        #region Properties

        [Notify(Setter = Private)]
        private ObservableCollection<MovieModelBase>? _movies;

        [Notify(Setter = Private)]
        private ListCollectionView? _moviesView;

        [Notify(CallbackName = nameof(OnSelectedItemChanged))]
        private MovieModelBase? _selectedItem;

        #endregion

        #region Services

        private IDialogCoordinator DialogCoordinator => GetService<IDialogCoordinator>()!;

        private IAsyncDialogService? DialogService => GetService<IAsyncDialogService>();

        public EnvironmentService EnvironmentService => GetService<EnvironmentService>()!;

        public ILogger Logger => GetService<ILogger>()!;

        private MoviesService MoviesService => GetService<MoviesService>()!;

        private new MainWindowViewModel? ParentViewModel => base.ParentViewModel as MainWindowViewModel;

        private SettingsService? SettingsService => GetService<SettingsService>();

        #endregion

        #region Event Handlers

        private void OnSelectedItemChanged(MovieModelBase? oldSelectedItem)
        {
            var newSelectedItem = SelectedItem;
            if (newSelectedItem != null)
            {
            }
            NewGroupCommand?.RaiseCanExecuteChanged();
            NewMovieCommand?.RaiseCanExecuteChanged();
            EditCommand?.RaiseCanExecuteChanged();
            DeleteCommand?.RaiseCanExecuteChanged();
            OpenMovieCommand?.RaiseCanExecuteChanged();
            OpenMovieExternalCommand?.RaiseCanExecuteChanged();
        }

        #endregion

        #region Methods

        protected override async ValueTask DisposeAsyncCore()
        {
            Settings?.SelectedPath = SelectedItem?.GetPath();

            await base.DisposeAsyncCore().ConfigureAwait(false);
        }

        protected override async void OnError(Exception ex, [CallerMemberName] string? callerName = null)
        {
            if (!CheckAccess())
            {
                Invoke(() => OnError(ex, callerName));
                return;
            }

            VerifyAccess();

#pragma warning disable IDE0079
#pragma warning disable CA2254
            if (Logger.IsEnabled(LogLevel.Error))
            {
                Logger.LogError(ex, Loc.An_error_has_occurred_in__Caller____Exception_, callerName, ex.Message);
            }
#pragma warning restore CA2254
#pragma warning restore IDE0079
            var dialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = Loc.OK,
                CancellationToken = CancellationToken.None,
            };
            await DialogCoordinator.ShowMessageAsync(this, Loc.Error, string.Format(Loc.An_error_has_occurred_in_Arg0_Arg1, callerName, ex.Message), MessageDialogStyle.Affirmative, dialogSettings).ConfigureAwait(false);
        }

        protected override async Task InitializeAsyncCore(CancellationToken cancellationToken)
        {
            Debug.Assert(DialogCoordinator != null, $"{nameof(DialogCoordinator)} is null");
            Debug.Assert(DialogService != null, $"{nameof(DialogService)} is null");
            Debug.Assert(EnvironmentService != null, $"{nameof(EnvironmentService)} is null");
            Debug.Assert(Logger != null, $"{nameof(Logger)} is null");
            Debug.Assert(MoviesService != null, $"{nameof(MoviesService)} is null");
            Debug.Assert(ParentViewModel != null, $"{nameof(ParentViewModel)} is null");
            Debug.Assert(SettingsService != null, $"{nameof(SettingsService)} is null");

            await base.InitializeAsyncCore(cancellationToken);

            Movies = [];
            Lifetime.Add(Movies.Clear);

            MoviesView = new ListCollectionView(Movies);
            Lifetime.Add(MoviesView.DetachFromSourceCollection);

            await ReloadMoviesAsync(cancellationToken);
        }

        private async ValueTask ReloadMoviesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Movies!.Clear();
            var movies = await MoviesService.GetMoviesAsync(cancellationToken);
            movies.ForEach(Movies.Add);
            Movies.OfType<MovieGroupModel>().FirstOrDefault()?.Expand();
        }

        #endregion
    }
}
