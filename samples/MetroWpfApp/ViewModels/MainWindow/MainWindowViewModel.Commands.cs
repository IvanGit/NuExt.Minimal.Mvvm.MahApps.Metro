﻿using ControlzEx.Theming;
using MetroWpfApp.Models;
using MetroWpfApp.Views;
using Minimal.Mvvm;
using Minimal.Mvvm.Windows;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using static AccessModifier;

namespace MetroWpfApp.ViewModels
{
    partial class MainWindowViewModel
    {
        #region Commands

        [Notify(Setter = Private)]
        private ICommand? _activeDocumentChangedCommand;

        #endregion

        #region Command Methods

        private bool CanCloseActiveDocument()
        {
            return IsUsable && ActiveDocument != null;
        }

        [Notify(Setter = Private)]
        private async Task CloseActiveDocumentAsync()
        {
            await ActiveDocument!.CloseAsync();
        }

        private bool CanShowHideActiveDocument(bool show)
        {
            return IsUsable && ActiveDocument != null;
        }

        [Notify(Setter = Private)]
        private void ShowHideActiveDocument(bool show)
        {
            if (show)
            {
                ActiveDocument?.Show();
            }
            else
            {
                ActiveDocument?.Hide();
            }
        }

        private bool CanShowMovies()
        {
            return IsUsable && DocumentManagerService != null;
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
            return IsUsable && DocumentManagerService != null;
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
            ChangeAccentColorCommand = RegisterCommand<string?>(ChangeAccentColor, CanChangeAccentColor);
            ChangeAppThemeCommand = RegisterCommand<string?>(ChangeAppTheme, CanChangeAppTheme);
            ShowMoviesCommand = RegisterAsyncCommand(ShowMoviesAsync, CanShowMovies);
            ShowHideActiveDocumentCommand = RegisterCommand<bool>(ShowHideActiveDocument, CanShowHideActiveDocument);
            CloseActiveDocumentCommand = RegisterAsyncCommand(CloseActiveDocumentAsync, CanCloseActiveDocument);
            OpenMovieCommand = RegisterAsyncCommand<MovieModel>(OpenMovieAsync, CanOpenMovie);
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
