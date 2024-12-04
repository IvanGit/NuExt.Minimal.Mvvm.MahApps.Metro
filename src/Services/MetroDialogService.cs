using MahApps.Metro.Controls.Dialogs;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides asynchronous methods to show and manage dialogs using MahApps.Metro dialog coordinator.
    /// Extends DialogServiceBase and implements IAsyncDialogService interface.
    /// </summary>
    public class MetroDialogService : DialogServiceBase, IAsyncDialogService
    {
        #region Dependency Properties

        /// <summary>Identifies the <see cref="DialogContentMargin"/> dependency property.</summary>
        public static readonly DependencyProperty DialogContentMarginProperty = DependencyProperty.Register(
            nameof(DialogContentMargin), typeof(GridLength), typeof(MetroDialogService),
            new PropertyMetadata(new GridLength(25, GridUnitType.Star)));

        /// <summary>Identifies the <see cref="DialogContentWidth"/> dependency property.</summary>
        public static readonly DependencyProperty DialogContentWidthProperty = DependencyProperty.Register(
            nameof(DialogContentWidth), typeof(GridLength), typeof(MetroDialogService),
            new PropertyMetadata(new GridLength(50, GridUnitType.Star)));

        /// <summary>
        /// Identifies the <see cref="DialogCoordinator"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DialogCoordinatorProperty = DependencyProperty.Register(
            nameof(DialogCoordinator), typeof(IDialogCoordinator), typeof(MetroDialogService));

        /// <summary>Identifies the <see cref="DialogTitleFontSize"/> dependency property.</summary>
        public static readonly DependencyProperty DialogTitleFontSizeProperty = DependencyProperty.Register(
            nameof(DialogTitleFontSize), typeof(double),  typeof(MetroDialogService), new PropertyMetadata(double.NaN));

        /// <summary>Identifies the <see cref="DialogMessageFontSize"/> dependency property.</summary>
        public static readonly DependencyProperty DialogMessageFontSizeProperty = DependencyProperty.Register(
            nameof(DialogMessageFontSize), typeof(double), typeof(MetroDialogService), new PropertyMetadata(double.NaN));

        /// <summary>Identifies the <see cref="DialogButtonFontSize"/> dependency property.</summary>
        public static readonly DependencyProperty DialogButtonFontSizeProperty = DependencyProperty.Register(
            nameof(DialogButtonFontSize), typeof(double), typeof(MetroDialogService), new PropertyMetadata(double.NaN));

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
        /// Gets or sets the left and right margin for the dialog content.
        /// </summary>
        public GridLength DialogContentMargin
        {
            get => (GridLength)GetValue(DialogContentMarginProperty);
            set => SetValue(DialogContentMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the width for the dialog content.
        /// </summary>
        public GridLength DialogContentWidth
        {
            get => (GridLength)GetValue(DialogContentWidthProperty);
            set => SetValue(DialogContentWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the DialogCoordinator used to manage dialog interactions.
        /// </summary>
        public IDialogCoordinator? DialogCoordinator
        {
            get => (IDialogCoordinator)GetValue(DialogCoordinatorProperty);
            set => SetValue(DialogCoordinatorProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size of the dialog title.
        /// </summary>
        public double DialogTitleFontSize
        {
            get => (double)GetValue(DialogTitleFontSizeProperty);
            set => SetValue(DialogTitleFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size of the dialog message text.
        /// </summary>
        public double DialogMessageFontSize
        {
            get => (double)GetValue(DialogMessageFontSizeProperty);
            set => SetValue(DialogMessageFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size of any dialog buttons.
        /// </summary>
        public double DialogButtonFontSize
        {
            get => (double)GetValue(DialogButtonFontSizeProperty);
            set => SetValue(DialogButtonFontSizeProperty, value);
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

            var dialogSettings = new MetroDialogSettings
            {
                DialogTitleFontSize = DialogTitleFontSize,
                DialogMessageFontSize = DialogMessageFontSize,
                DialogButtonFontSize = DialogButtonFontSize,
                CancellationToken = cancellationToken 
            };
            var dialog = new MetroDialog(dialogSettings)
            {
                Title = title,
                Content = view,
                CommandsSource = dialogCommands
            };

            BindingOperations.SetBinding(dialog, MetroDialog.ValidatesOnDataErrorsProperty, new Binding()
            {
                Path = new PropertyPath(ValidatesOnDataErrorsProperty),
                Source = this,
                Mode = BindingMode.OneWay
            });
            BindingOperations.SetBinding(dialog, MetroDialog.ValidatesOnNotifyDataErrorsProperty, new Binding()
            {
                Path = new PropertyPath(ValidatesOnNotifyDataErrorsProperty),
                Source = this,
                Mode = BindingMode.OneWay
            });
            BindingOperations.SetBinding(dialog, BaseMetroDialog.DialogContentMarginProperty, new Binding() 
            {
                Path = new PropertyPath(DialogContentMarginProperty),
                Source = this,
                Mode = BindingMode.OneWay
            });
            BindingOperations.SetBinding(dialog, BaseMetroDialog.DialogContentWidthProperty, new Binding()
            {
                Path = new PropertyPath(DialogContentWidthProperty),
                Source = this,
                Mode = BindingMode.OneWay
            });

            var dialogCoordinator = DialogCoordinator ?? MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance;
            await dialogCoordinator.ShowMetroDialogAsync(this, dialog);

            try
            {
                return await dialog.WaitForButtonPressAsync();
            }
            finally
            {
                await dialogCoordinator.HideMetroDialogAsync(this, dialog);
                BindingOperations.ClearBinding(dialog, MetroDialog.ValidatesOnDataErrorsProperty);
                BindingOperations.ClearBinding(dialog, MetroDialog.ValidatesOnNotifyDataErrorsProperty);
                BindingOperations.ClearBinding(dialog, BaseMetroDialog.DialogContentMarginProperty);
                BindingOperations.ClearBinding(dialog, BaseMetroDialog.DialogContentWidthProperty);
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
