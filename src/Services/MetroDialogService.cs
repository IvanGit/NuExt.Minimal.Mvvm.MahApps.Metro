using MahApps.Metro.Controls.Dialogs;
using System.Diagnostics;
using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides asynchronous methods to show and manage dialogs using MahApps.Metro dialog coordinator.
    /// Extends DialogServiceBase and implements IAsyncDialogService interface.
    /// </summary>
    public class MetroDialogService : DialogServiceBase, IAsyncDialogService
    {
        #region Dependency Properties

        /// <summary>
        /// Identifies the <see cref="DialogCoordinator"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DialogCoordinatorProperty = DependencyProperty.Register(
            nameof(DialogCoordinator), typeof(IDialogCoordinator), typeof(MetroDialogService));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DialogCoordinator used to manage dialog interactions.
        /// </summary>
        public IDialogCoordinator? DialogCoordinator
        {
            get => (IDialogCoordinator)GetValue(DialogCoordinatorProperty);
            set => SetValue(DialogCoordinatorProperty, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Displays a dialog asynchronously with the specified parameters.
        /// </summary>
        /// <param name="dialogCommands">A collection of UICommand objects representing the buttons available in the dialog.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="documentType">The type of the view to display within the dialog.</param>
        /// <param name="viewModel">The ViewModel associated with the view.</param>
        /// <param name="parentViewModel">The parent ViewModel for context.</param>
        /// <param name="parameter">The optional parameter for context.</param>
        /// <param name="cancellationToken">A token to cancel the dialog operation if needed.</param>
        /// <returns>A <see cref="ValueTask{UICommand}"/> representing the command selected by the user.</returns>
        public async ValueTask<UICommand?> ShowDialogAsync(IEnumerable<UICommand> dialogCommands, string? title, string? documentType, object? viewModel, object? parentViewModel, object? parameter, CancellationToken cancellationToken = default)
        {
            Debug.Assert(DialogCoordinator != null, $"{nameof(DialogCoordinator)} is null");

            cancellationToken.ThrowIfCancellationRequested();
            var view = await CreateAndInitializeViewAsync(documentType, viewModel, parentViewModel, parameter, cancellationToken);

            var dialogSettings = new MetroDialogSettings { CancellationToken = cancellationToken };
            var dialog = new MetroDialog(dialogSettings)
            {
                Title = title,
                Content = view,
                CommandsSource = dialogCommands,
            };

            var dialogCoordinator = DialogCoordinator ?? MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance;
            await dialogCoordinator.ShowMetroDialogAsync(this, dialog);

            try
            {
                return await dialog.WaitForButtonPressAsync();
            }
            finally
            {
                await dialogCoordinator.HideMetroDialogAsync(this, dialog);
            }
        }

        /// <summary>
        /// Displays a dialog asynchronously with the specified parameters.
        /// </summary>
        /// <param name="dialogButtons">The buttons to be displayed in the dialog.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="documentType">The type of the view to display within the dialog.</param>
        /// <param name="viewModel">The ViewModel associated with the view.</param>
        /// <param name="parentViewModel">The parent ViewModel for context.</param>
        /// <param name="parameter">The optional parameter for context.</param>
        /// <param name="cancellationToken">A token to cancel the dialog operation if needed.</param>
        /// <returns>A <see cref="ValueTask{MessageBoxResult}"/> representing the user's action.</returns>
        public async ValueTask<MessageBoxResult> ShowDialogAsync(MessageBoxButton dialogButtons, string? title, string? documentType, object? viewModel, object? parentViewModel, object? parameter, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetMessageBoxResult(await ShowDialogAsync(GetUICommands(dialogButtons), title, documentType, viewModel, parentViewModel, parameter, cancellationToken));
        }

        #endregion
    }
}
