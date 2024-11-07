using ControlzEx.Theming;
using MetroWpfApp.Views;
using Minimal.Mvvm.Windows;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace MetroWpfApp.ViewModels
{
    partial class MainWindowViewModel
    {
        #region Commands

        private ICommand? _activeDocumentChangedCommand;
        public ICommand? ActiveDocumentChangedCommand
        {
            get => _activeDocumentChangedCommand;
            private set => SetProperty(ref _activeDocumentChangedCommand, value);
        }

        private ICommand? _changeAccentColorCommand;
        public ICommand? ChangeAccentColorCommand
        {
            get => _changeAccentColorCommand;
            private set => SetProperty(ref _changeAccentColorCommand, value);
        }

        private ICommand? _changeAppThemeCommand;
        public ICommand? ChangeAppThemeCommand
        {
            get => _changeAppThemeCommand;
            private set => SetProperty(ref _changeAppThemeCommand, value);
        }

        private ICommand? _closeActiveDocumentCommand;
        public ICommand? CloseActiveDocumentCommand
        {
            get => _closeActiveDocumentCommand;
            private set => SetProperty(ref _closeActiveDocumentCommand, value);
        }

        private ICommand? _showHideActiveDocumentCommand;
        public ICommand? ShowHideActiveDocumentCommand
        {
            get => _showHideActiveDocumentCommand;
            private set => SetProperty(ref _showHideActiveDocumentCommand, value);
        }

        private ICommand? _showMoviesCommand;
        public ICommand? ShowMoviesCommand
        {
            get => _showMoviesCommand;
            private set => SetProperty(ref _showMoviesCommand, value);
        }

        #endregion

        #region Command Methods

        private bool CanCloseActiveDocument()
        {
            return IsUsable && ActiveDocument != null;
        }

        private async Task CloseActiveDocumentAsync()
        {
            await ActiveDocument!.CloseAsync();
        }

        private bool CanShowHideActiveDocument(bool show)
        {
            return IsUsable && ActiveDocument != null;
        }

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

        private async Task ShowMoviesAsync()
        {
            var cancellationToken = GetCurrentCancellationToken();

            var document = await DocumentManagerService!.FindDocumentByIdOrCreateAsync(default(Movies),
                async x =>
                {
                    var vm = new MoviesViewModel() { Title = "Movies" };
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

        private static void ChangeAppTheme(string? baseColorScheme)
        {
            if (baseColorScheme is not null)
            {
                ThemeManager.Current.ChangeThemeBaseColor(Application.Current, baseColorScheme);
            }
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
        }

        protected override async ValueTask OnContentRenderedAsync(CancellationToken cancellationToken)
        {
            await base.OnContentRenderedAsync(cancellationToken);
            Debug.Assert(DocumentManagerService != null, $"{nameof(DocumentManagerService)} is null");
            Debug.Assert(SettingsService != null, $"{nameof(SettingsService)} is null");

            Debug.Assert(CheckAccess());
            cancellationToken.ThrowIfCancellationRequested();

            if (DocumentManagerService is IAsyncDisposable asyncDisposable)
            {
                Lifetime.AddAsyncDisposable(asyncDisposable);
            }

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
