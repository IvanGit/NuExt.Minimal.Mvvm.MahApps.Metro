﻿using ControlzEx.Theming;
using MetroWpfApp.Models;
using MetroWpfApp.Services;
using MetroWpfApp.Views;
using Minimal.Mvvm;
using Minimal.Mvvm.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MetroWpfApp.ViewModels
{
    internal sealed partial class MainWindowViewModel : WindowViewModel
    {
        #region Properties

        [Notify(CallbackName = nameof(OnActiveDocumentChanged))]
        private IAsyncDocument? _activeDocument;

        public ObservableCollection<MenuItemViewModel> MenuItems { get; } = [];

        #endregion

        #region Services

        public IAsyncDocumentManagerService? DocumentManagerService => GetService<IAsyncDocumentManagerService>("Documents");

        public EnvironmentService EnvironmentService => GetService<EnvironmentService>()!;

        private MoviesService MoviesService => GetService<MoviesService>()!;

        private SettingsService? SettingsService => GetService<SettingsService>();

        #endregion

        #region Event Handlers

        private void OnActiveDocumentChanged(IAsyncDocument? oldActiveDocument)
        {
            ShowHideActiveDocumentCommand?.RaiseCanExecuteChanged();
            CloseActiveDocumentCommand?.RaiseCanExecuteChanged();
        }

        #endregion

        #region Methods

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
                        new() { Header = Loc.Close_Active_Document, Command = CloseActiveDocumentCommand }
                    })
                }
            };
            menuItems.ForEach(MenuItems.Add);
            return default;
        }

        protected override async ValueTask OnDisposeAsync()
        {
            var doc = DocumentManagerService?.FindDocumentById(default(Movies));
            Settings!.MoviesOpened = doc is not null;

            await base.OnDisposeAsync();
        }

        protected override void OnError(Exception ex, [CallerMemberName] string? callerName = null)
        {
            base.OnError(ex, callerName);
            MessageBox.Show(string.Format(Loc.An_error_has_occurred_in_Arg0_Arg1, callerName, ex.Message), Loc.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(DocumentManagerService != null, $"{nameof(DocumentManagerService)} is null");
            Debug.Assert(EnvironmentService != null, $"{nameof(EnvironmentService)} is null");
            Debug.Assert(MoviesService != null, $"{nameof(MoviesService)} is null");
            Debug.Assert(SettingsService != null, $"{nameof(SettingsService)} is null");

            if (DocumentManagerService is IAsyncDisposable asyncDisposable)
            {
                Lifetime.AddAsyncDisposable(asyncDisposable);
            }

            return base.OnInitializeAsync(cancellationToken);
        }

        private void UpdateTitle()
        {
            var sb = new ValueStringBuilder();
            var doc = ActiveDocument;
            if (doc != null)
            {
                sb.Append($"{doc.Title} - ");
            }
            sb.Append($"{AssemblyInfo.Product} v{AssemblyInfo.Version?.ToString(3)}");
            Title = sb.ToString();
        }

        #endregion
    }
}
