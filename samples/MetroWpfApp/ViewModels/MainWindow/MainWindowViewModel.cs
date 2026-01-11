using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Minimal.Mvvm;
using Minimal.Mvvm.Windows;
using MovieWpfApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MovieWpfApp.ViewModels
{
    internal sealed partial class MainWindowViewModel : WindowViewModel
    {
        private IAsyncDocument? _lastActiveDocument;
        private IAsyncDocument? _lastActiveWindow;

        #region Properties

        [Notify(CallbackName = nameof(OnActiveDocumentChanged))]
        private IAsyncDocument? _activeDocument;

        [Notify(CallbackName = nameof(OnActiveWindowChanged))]
        private IAsyncDocument? _activeWindow;

        public ObservableCollection<MenuItemViewModel> MenuItems { get; } = [];

        #endregion

        #region Services

        private IDialogCoordinator? DialogCoordinator => GetService<IDialogCoordinator>();

        public IAsyncDocumentManagerService? DocumentManagerService => GetService<IAsyncDocumentManagerService>("Documents");

        public EnvironmentService EnvironmentService => GetService<EnvironmentService>()!;

        public ILogger Logger => GetService<ILogger>()!;

        private MoviesService MoviesService => GetService<MoviesService>()!;

        private SettingsService? SettingsService => GetService<SettingsService>();

        public IAsyncDocumentManagerService? WindowManagerService => GetService<IAsyncDocumentManagerService>("Windows");

        #endregion

        #region Event Handlers

        private void DocumentManagerService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not IAsyncDocumentManagerService documentManagerService) return;
            if (e.PropertyName == nameof(documentManagerService.Count))
            {
                if (documentManagerService.Count == 0)
                {
                    _lastActiveDocument = null;
                }
            }
        }

        private void OnActiveDocumentChanged(IAsyncDocument? oldActiveDocument)
        {
            ShowHideActiveDocumentCommand?.RaiseCanExecuteChanged();
            CloseActiveDocumentCommand?.RaiseCanExecuteChanged();

            if (ActiveDocument != null)
            {
                _lastActiveDocument = ActiveDocument;
            }
        }

        private void OnActiveWindowChanged(IAsyncDocument? oldActiveWindow)
        {
            ShowHideActiveWindowCommand?.RaiseCanExecuteChanged();
            CloseActiveWindowCommand?.RaiseCanExecuteChanged();

            if (ActiveWindow != null)
            {
                _lastActiveWindow = ActiveWindow;
            }
        }

        private void WindowManagerService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not IAsyncDocumentManagerService documentManagerService) return;
            if (e.PropertyName == nameof(documentManagerService.Count))
            {
                if (documentManagerService.Count == 0)
                {
                    _lastActiveWindow = null;
                }
            }
        }

        #endregion

        #region Methods

        protected override async ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            VerifyAccess();

            if (DialogCoordinator == null)
            {
                MessageBoxResult dialogResult = MessageBox.Show(WindowService?.Window,
                    string.Format(Loc.Are_you_sure_you_want_to_close__Arg0__, $"{AssemblyInfo.Current.Product}"),
                    Loc.Confirmation,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (dialogResult != MessageBoxResult.Yes)
                {
                    return false;
                }
            }
            else
            {
                var dialogSettings = new MetroDialogSettings
                {
                    CancellationToken = cancellationToken,
                    AffirmativeButtonText = Loc.Yes,
                    NegativeButtonText = Loc.No,
                };

                var dialogResult = await DialogCoordinator.ShowMessageAsync(this, Loc.Confirmation,
                    string.Format(Loc.Are_you_sure_you_want_to_close__Arg0__, $"{AssemblyInfo.Current.Product}"),
                    MessageDialogStyle.AffirmativeAndNegative, dialogSettings);
                if (dialogResult != MessageDialogResult.Affirmative)
                {
                    return false;
                }
            }
            return await base.CanCloseAsync(cancellationToken);
        }

        private ValueTask LoadMenuAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            MenuItems.Clear();
            var menuItems = new MenuItemViewModel[]
            {
                new()
                {
                    Header = Loc.File,
                    SubMenuItems=new ObservableCollection<MenuItemViewModel?>(new MenuItemViewModel?[]
                    {
                        new() { Header = Loc.Movies, Command = ShowMoviesCommand },
                        null,
                        new() { Header = Loc.Exit, Command = CloseCommand }
                    })
                },
                new()
                {
                    Header = Loc.View,
                    SubMenuItems=new ObservableCollection<MenuItemViewModel?>(new MenuItemViewModel?[]
                    {
                        new()
                        {
                            Header = Loc.Theme,
                            SubMenuItems=new ObservableCollection<MenuItemViewModel?>(ThemeManager.Current.Themes
                                .GroupBy(x => x.BaseColorScheme)
                                .Select(x => x.First())
                                .Select(a => new AppThemeMenuItemViewModel { Header = a.BaseColorScheme, BorderColorBrush = (a.Resources["MahApps.Brushes.ThemeForeground"] as Brush)!, ColorBrush = (a.Resources["MahApps.Brushes.ThemeBackground"] as Brush)!, Command = ChangeAppThemeCommand, CommandParameter = a.BaseColorScheme}))
                        },
                        new()
                        {
                            Header = Loc.Accent,
                            SubMenuItems=new ObservableCollection<MenuItemViewModel?>(ThemeManager.Current.Themes
                                .GroupBy(x => x.ColorScheme)
                                .OrderBy(a => a.Key)
                                .Select(a => new AccentColorMenuItemViewModel { Header = a.Key, ColorBrush = a.First().ShowcaseBrush, Command = ChangeAccentColorCommand, CommandParameter = a.Key }))
                        },
                        null,
                        new() { Header = Loc.Hide_Active_Document, CommandParameter = false, Command = ShowHideActiveDocumentCommand },
                        new() { Header = Loc.Show_Active_Document, CommandParameter = true, Command = ShowHideActiveDocumentCommand },
                        new() { Header = Loc.Close_Active_Document, Command = CloseActiveDocumentCommand },
                        null,
                        new() { Header = Loc.Hide_Active_Window, CommandParameter = false, Command = ShowHideActiveWindowCommand },
                        new() { Header = Loc.Show_Active_Window, CommandParameter = true, Command = ShowHideActiveWindowCommand },
                        new() { Header = Loc.Close_Active_Window, Command = CloseActiveWindowCommand },
                    })
                }
            };
            menuItems.ForEach(MenuItems.Add);
            return default;
        }

        protected override async ValueTask DisposeAsyncCore()
        {
            var doc = DocumentManagerService?.FindDocumentById(default(Movies));
            Settings!.MoviesOpened = doc is not null;

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
            if (DialogCoordinator == null)
            {
                MessageBox.Show(WindowService?.Window, string.Format(Loc.An_error_has_occurred_in_Arg0_Arg1, callerName, ex.Message), Loc.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var dialogSettings = new MetroDialogSettings
                {
                    AffirmativeButtonText = Loc.OK,
                    CancellationToken = CancellationToken,
                };
                await DialogCoordinator.ShowMessageAsync(this, Loc.Error, string.Format(Loc.An_error_has_occurred_in_Arg0_Arg1, callerName, ex.Message), MessageDialogStyle.Affirmative, dialogSettings).ConfigureAwait(false);
            }
        }

        protected override Task InitializeAsyncCore(CancellationToken cancellationToken)
        {
            Debug.Assert(DialogCoordinator != null, $"{nameof(DialogCoordinator)} is null");
            Debug.Assert(DocumentManagerService != null, $"{nameof(DocumentManagerService)} is null");
            Debug.Assert(EnvironmentService != null, $"{nameof(EnvironmentService)} is null");
            Debug.Assert(Logger != null, $"{nameof(Logger)} is null");
            Debug.Assert(MoviesService != null, $"{nameof(MoviesService)} is null");
            Debug.Assert(SettingsService != null, $"{nameof(SettingsService)} is null");
            Debug.Assert(WindowManagerService != null, $"{nameof(WindowManagerService)} is null");

            if (DocumentManagerService is IAsyncDisposable asyncDisposable)
            {
                Lifetime.AddAsyncDisposable(asyncDisposable);
            }
            if (DocumentManagerService is INotifyPropertyChanged notifyPropertyChanged)
            {
                Lifetime.AddBracket(
                    () => notifyPropertyChanged.PropertyChanged += DocumentManagerService_PropertyChanged,
                    () => notifyPropertyChanged.PropertyChanged -= DocumentManagerService_PropertyChanged);
            }

            if (WindowManagerService is IAsyncDisposable asyncDisposable1)
            {
                Lifetime.AddAsyncDisposable(asyncDisposable1);
            }
            if (WindowManagerService is INotifyPropertyChanged notifyPropertyChanged1)
            {
                Lifetime.AddBracket(
                    () => notifyPropertyChanged1.PropertyChanged += WindowManagerService_PropertyChanged,
                    () => notifyPropertyChanged1.PropertyChanged -= WindowManagerService_PropertyChanged);
            }

            return base.InitializeAsyncCore(cancellationToken);
        }

        private void UpdateTitle()
        {
            var sb = new ValueStringBuilder(stackalloc char[200]);
            var doc = ActiveDocument;
            if (doc != null)
            {
                sb.Append($"{doc.Title} - ");
            }
            sb.Append($"{AssemblyInfo.Current.Product} v{AssemblyInfo.Current.Version?.ToString(3)}");
            var window = ActiveWindow;
            if (window != null)
            {
                sb.Append($" [{window.Title}]");
            }
            Title = sb.ToString();
        }

        #endregion
    }
}
