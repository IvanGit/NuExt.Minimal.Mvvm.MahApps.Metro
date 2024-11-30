using System.ComponentModel;
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

        /// <summary>
        /// Identifies the <see cref="ValidatesOnDataErrors"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidatesOnDataErrorsProperty = DependencyProperty.Register(
            nameof(ValidatesOnDataErrors), typeof(bool), typeof(MetroDialogService), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="ValidatesOnNotifyDataErrors"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidatesOnNotifyDataErrorsProperty = DependencyProperty.Register(
            nameof(ValidatesOnNotifyDataErrors), typeof(bool), typeof(MetroDialogService), new PropertyMetadata(false));

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

        /// <summary>
        /// Gets or sets a value indicating whether the service should check for validation errors
        /// when closing the dialog. If true, the service will prevent the dialog from closing if there are validation errors.
        /// This applies only if the ViewModel implements the <see cref="IDataErrorInfo"/> interface.
        /// </summary>
        public bool ValidatesOnDataErrors
        {
            get => (bool)GetValue(ValidatesOnDataErrorsProperty);
            set => SetValue(ValidatesOnDataErrorsProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the dialog should check for validation errors
        /// when closing. If true, the dialog will prevent closing if there are validation errors.
        /// This applies only if the ViewModel implements the <see cref="INotifyDataErrorInfo"/> interface.
        /// </summary>
        public bool ValidatesOnNotifyDataErrors
        {
            get => (bool)GetValue(ValidatesOnNotifyDataErrorsProperty);
            set => SetValue(ValidatesOnNotifyDataErrorsProperty, value);
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
                ValidatesOnDataErrors = ValidatesOnDataErrors,
                ValidatesOnNotifyDataErrors = ValidatesOnNotifyDataErrors
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
