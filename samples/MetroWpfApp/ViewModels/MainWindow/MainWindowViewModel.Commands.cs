using ControlzEx.Theming;
using Minimal.Mvvm;
using Minimal.Mvvm.Windows;
using MovieWpfApp.Models;
using MovieWpfApp.Views;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using static AccessModifier;

namespace MovieWpfApp.ViewModels
{
    partial class MainWindowViewModel
    {
        #region Commands

        [Notify(Setter = Private)]
        private ICommand? _activeDocumentChangedCommand;

        [Notify(Setter = Private)]
        private ICommand? _activeWindowChangedCommand;

        #endregion

        #region Command Methods

        private bool CanCloseActiveDocument()
        {
            return IsUsable && _lastActiveDocument != null;
        }

        [Notify(Setter = Private)]
        private async Task CloseActiveDocumentAsync()
        {
            if (_lastActiveDocument != null)
            {
                await _lastActiveDocument.CloseAsync(false);
            }
        }

        private bool CanShowHideActiveDocument(bool show)
        {
            return IsUsable && _lastActiveDocument != null;
        }

        [Notify(Setter = Private)]
        private void ShowHideActiveDocument(bool show)
        {
            if (show)
            {
                _lastActiveDocument?.Show();
                //ActiveDocument = _lastActiveDocument;
            }
            else
            {
                _lastActiveDocument?.Hide();
            }
        }

        private bool CanCloseActiveWindow()
        {
            return IsUsable && _lastActiveWindow != null;
        }

        [Notify(Setter = Private)]
        private async Task CloseActiveWindowAsync()
        {
            if (_lastActiveWindow != null)
            {
                await _lastActiveWindow.CloseAsync(false);
            }
        }

        private bool CanShowHideActiveWindow(bool show)
        {
            return IsUsable && _lastActiveWindow != null;
        }

        [Notify(Setter = Private)]
        private void ShowHideActiveWindow(bool show)
        {
            if (show)
            {
                _lastActiveWindow?.Show();
                //ActiveWindow = _lastActiveWindow;
            }
            else
            {
                _lastActiveWindow?.Hide();
            }
        }

        private bool CanShowMovies()
        {
            return IsUsable;
        }

        [Notify(Setter = Private)]
        private async Task ShowMoviesAsync()
        {
            var cancellationToken = GetCurrentCancellationToken();

            var document = await DocumentManagerService!.FindDocumentByIdOrCreateAsync(default(Movies),
                async x =>
                {
                    var vm = new MoviesViewModel() { Title = Loc.Movies };
                    try
                    {
                        var doc = await x.CreateDocumentAsync(nameof(MoviesView), vm, this, null, cancellationToken);
                        doc.DisposeOnClose = true;
                        return doc;
                    }
                    catch (Exception ex)
                    {
                        Debug.Assert(ex is OperationCanceledException, ex.Message);
                        await vm.DisposeAsync();
                        throw;
                    }
                });
            document.Show();
        }

        private bool CanChangeAccentColor(string? colorScheme)
        {
            return IsUsable;
        }

        [Notify(Setter = Private)]
        private static void ChangeAccentColor(string? colorScheme)
        {
            if (colorScheme is not null)
            {
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, colorScheme);
            }
        }

        private bool CanChangeAppTheme(string? baseColorScheme)
        {
            return IsUsable;
        }

        [Notify(Setter = Private)]
        private static void ChangeAppTheme(string? baseColorScheme)
        {
            if (baseColorScheme is not null)
            {
                ThemeManager.Current.ChangeThemeBaseColor(Application.Current, baseColorScheme);
            }
        }

        private bool CanOpenMovie(MovieModel movie)
        {
            return IsUsable;
        }

        [Notify(Setter = Private)]
        private async Task OpenMovieAsync(MovieModel movie)
        {
            var cancellationToken = GetCurrentCancellationToken();

            var document = await DocumentManagerService!.FindDocumentByIdOrCreateAsync(new MovieDocument(movie), async x =>
            {
                var vm = new MovieViewModel();
                try
                {
                    var doc = await x.CreateDocumentAsync(nameof(MovieView), vm, this, movie, cancellationToken);
                    doc.DisposeOnClose = true;
                    //doc.HideInsteadOfClose = true;
                    return doc;
                }
                catch (Exception ex)
                {
                    Debug.Assert(ex is OperationCanceledException, ex.Message);
                    await vm.DisposeAsync();
                    throw;
                }
            });
            document.Show();
        }

        [Notify(Setter = Private)]
        private async Task OpenMovieExternalAsync(MovieModel movie)
        {
            var cancellationToken = GetCurrentCancellationToken();

            var document = await WindowManagerService!.FindDocumentByIdOrCreateAsync(new MovieDocument(movie), async x =>
            {
                var vm = new MovieViewModel();
                try
                {
                    var doc = await x.CreateDocumentAsync(nameof(MovieView), vm, this, movie, cancellationToken);
                    doc.DisposeOnClose = true;
                    //doc.HideInsteadOfClose = true;
                    return doc;
                }
                catch (Exception ex)
                {
                    Debug.Assert(ex is OperationCanceledException, ex.Message);
                    await vm.DisposeAsync();
                    throw;
                }
            });
            document.Show();
        }

        private bool CanCloseMovie(MovieModel movie) => CanOpenMovie(movie);

        [Notify(Setter = Private)]
        private async Task CloseMovieAsync(MovieModel movie)
        {
            var doc = DocumentManagerService!.FindDocumentById(new MovieDocument(movie));
            if (doc == null) return;
            await doc.CloseAsync().ConfigureAwait(false);
        }

        #endregion

        #region Methods

        protected override void CreateCommands()
        {
            base.CreateCommands();
            ActiveDocumentChangedCommand = RegisterCommand(UpdateTitle);
            ActiveWindowChangedCommand = RegisterCommand(UpdateTitle);
            ChangeAccentColorCommand = RegisterCommand<string?>(ChangeAccentColor, CanChangeAccentColor);
            ChangeAppThemeCommand = RegisterCommand<string?>(ChangeAppTheme, CanChangeAppTheme);
            ShowMoviesCommand = RegisterAsyncCommand(ShowMoviesAsync, CanShowMovies);
            ShowHideActiveDocumentCommand = RegisterCommand<bool>(ShowHideActiveDocument, CanShowHideActiveDocument);
            ShowHideActiveWindowCommand = RegisterCommand<bool>(ShowHideActiveWindow, CanShowHideActiveWindow);
            CloseActiveDocumentCommand = RegisterAsyncCommand(CloseActiveDocumentAsync, CanCloseActiveDocument);
            CloseActiveWindowCommand = RegisterAsyncCommand(CloseActiveWindowAsync, CanCloseActiveWindow);
            OpenMovieCommand = RegisterAsyncCommand<MovieModel>(OpenMovieAsync, CanOpenMovie);
            OpenMovieExternalCommand = RegisterAsyncCommand<MovieModel>(OpenMovieExternalAsync, CanOpenMovie);
            CloseMovieCommand = RegisterAsyncCommand<MovieModel>(CloseMovieAsync, CanCloseMovie);
        }

        protected override async ValueTask OnContentRenderedAsync(CancellationToken cancellationToken)
        {
            await base.OnContentRenderedAsync(cancellationToken);

            Debug.Assert(CheckAccess());
            cancellationToken.ThrowIfCancellationRequested();

            await LoadMenuAsync(cancellationToken);

            await MoviesService.InitializeAsync(cancellationToken);
            RaiseCanExecuteChanged();

            Debug.Assert(Settings!.IsSuspended == false);
            if (Settings.MoviesOpened)
            {
                ShowMoviesCommand?.Execute(null);
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            CreateSettings();
            UpdateTitle();
        }

        #endregion
    }
}
