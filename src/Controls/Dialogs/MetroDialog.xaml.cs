using Minimal.Mvvm.Windows;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MahApps.Metro.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for MetroDialog.xaml
    /// </summary>
    public partial class MetroDialog : BaseMetroDialog
    {
        #region Dependency Properties

        /// <summary>Identifies the <see cref="CommandsSource"/> dependency property.</summary>
        public static readonly DependencyProperty CommandsSourceProperty = DependencyProperty.Register(
            nameof(CommandsSource), typeof(IEnumerable), typeof(MetroDialog));

        /// <summary>
        /// Identifies the <see cref="ValidatesOnDataErrors"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidatesOnDataErrorsProperty = DependencyProperty.Register(
            nameof(ValidatesOnDataErrors), typeof(bool), typeof(MetroDialog), new PropertyMetadata(false));

        #endregion

        private readonly TaskCompletionSource<UICommand?> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public MetroDialog() : this(null, null)
        {
        }

        public MetroDialog(MetroWindow? parentWindow) : this(parentWindow, null)
        {
        }

        public MetroDialog(MetroDialogSettings? settings) : this(null, settings)
        {
        }

        public MetroDialog(MetroWindow? parentWindow, MetroDialogSettings? settings) : base(parentWindow, settings)
        {
            InitializeComponent();
        }

        #region UI Commands

        private UICommand? CancelCommand => CommandsSource?.Cast<UICommand>().FirstOrDefault(c => c.IsCancel);

        private UICommand? DefaultCommand => CommandsSource?.Cast<UICommand>().FirstOrDefault(c => c.IsDefault);

        #endregion

        #region Properties

        /// <summary>
        /// UI commands.
        /// </summary>
        public IEnumerable? CommandsSource
        {
            get => (IEnumerable)GetValue(CommandsSourceProperty);
            set => SetValue(CommandsSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the dialog should check for validation errors
        /// when closing. If true, the dialog will prevent closing if there are validation errors.
        /// This applies only if the ViewModel implements the <see cref="IDataErrorInfo"/> interface.
        /// </summary>
        public bool ValidatesOnDataErrors
        {
            get => (bool)GetValue(ValidatesOnDataErrorsProperty);
            set => SetValue(ValidatesOnDataErrorsProperty, value);
        }

        #endregion

        #region Event Handlers

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button { DataContext: UICommand command })
            {
                if (command != DefaultCommand || !HasValidationErrors())
                {
                    _tcs.TrySetResult(command);
                }
                e.Handled = true;
            }
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e is { Key: Key.System, SystemKey: Key.F4 })
            {
                _tcs.TrySetResult(CancelCommand);

                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                var result = DefaultCommand;
                if (e.OriginalSource is Button { DataContext: UICommand command })
                {
                    result = command;
                }

                if (result != DefaultCommand || !HasValidationErrors())
                {
                    _tcs.TrySetResult(result);
                }

                e.Handled = true;
            }
        }

        #endregion

        #region Methods

        private bool HasValidationErrors()
        {
            if (!ValidatesOnDataErrors)
            {
                return false;
            }
            var viewModel = ViewModelHelper.GetViewModelFromView(Content);
            if (viewModel is not IDataErrorInfo errorInfo)
            {
                return false;
            }
            return !string.IsNullOrEmpty(errorInfo.Error);
        }

        private Lifetime SubscribeMetroDialog()
        {
            var lifetime = new Lifetime();

            if (DialogSettings.CancellationToken.CanBeCanceled)
            {
                lifetime.AddDisposable(DialogSettings.CancellationToken.Register(() => _tcs.TrySetCanceled(), useSynchronizationContext: false));
            }

            if (DialogBottom != null && DialogButtons != null)
            {
                foreach (Button button in DependencyObjectExtensions/*do not change to avoid using TreeHelper*/.FindChildren<Button>(DialogButtons))
                {
                    if (button.Command is null && button.DataContext is UICommand command)
                    {
                        lifetime.AddBracket(() => button.Click += OnButtonClick, () => button.Click -= OnButtonClick);
                        lifetime.AddBracket(() => button.KeyDown += OnKeyDownHandler, () => button.KeyDown += OnKeyDownHandler);
                    }
                }
            }

            lifetime.AddBracket(() => KeyDown += OnKeyDownHandler, () => KeyDown -= OnKeyDownHandler);

            return lifetime;
        }

        public async ValueTask<UICommand?> WaitForButtonPressAsync()
        {
            using var subscription = SubscribeMetroDialog();
            return await _tcs.Task.ConfigureAwait(false);
        }

        #endregion
    }
}
