using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides dialog coordination services using MahApps.Metro dialogs.
    /// Extends WindowAwareServiceBase and implements IDialogCoordinator interface.
    /// </summary>
    public class DialogCoordinatorService : WindowServiceBase, IDialogCoordinator
    {
        #region Methods

        /// <summary>
        /// Shows an asynchronous input dialog.
        /// </summary>
        /// <param name="context">The context in which the dialog is shown.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message or question presented to the user.</param>
        /// <param name="settings">Optional settings to customize the dialog appearance and behavior.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user input or null if cancelled.</returns>
        public Task<string?> ShowInputAsync(object context, string title, string message, MetroDialogSettings? settings = null)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.ShowInputAsync(title, message, settings));
        }

        /// <summary>
        /// Shows a modal input dialog externally.
        /// </summary>
        /// <param name="context">The context in which the dialog is shown.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message or question presented to the user.</param>
        /// <param name="metroDialogSettings">Optional settings to customize the dialog appearance and behavior.</param>
        /// <returns>The user input or null if cancelled.</returns>
        public string? ShowModalInputExternal(object context, string title, string message, MetroDialogSettings? metroDialogSettings = null)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.ShowModalInputExternal(title, message, metroDialogSettings);
        }

        /// <summary>
        /// Shows an asynchronous login dialog.
        /// </summary>
        /// <param name="context">The context in which the dialog is shown.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message or prompt for login information.</param>
        /// <param name="settings">Optional settings to customize the dialog appearance and behavior.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the login data or null if cancelled.</returns>
        public Task<LoginDialogData?> ShowLoginAsync(object context, string title, string message, LoginDialogSettings? settings = null)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.ShowLoginAsync(title, message, settings));
        }

        /// <summary>
        /// Shows a modal login dialog externally.
        /// </summary>
        /// <param name="context">The context in which the dialog is shown.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message or prompt for login information.</param>
        /// <param name="settings">Optional settings to customize the dialog appearance and behavior.</param>
        /// <returns>The login data or null if cancelled.</returns>
        public LoginDialogData? ShowModalLoginExternal(object context, string title, string message, LoginDialogSettings? settings = null)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.ShowModalLoginExternal(title, message, settings);
        }

        /// <summary>
        /// Shows an asynchronous message dialog.
        /// </summary>
        /// <param name="context">The context in which the dialog is shown.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message presented to the user.</param>
        /// <param name="style">The style of the message dialog (e.g., Affirmative, AffirmativeAndNegative).</param>
        /// <param name="settings">Optional settings to customize the dialog appearance and behavior.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user's decision.</returns>
        public Task<MessageDialogResult> ShowMessageAsync(object context, string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings? settings = null)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.ShowMessageAsync(title, message, style, settings));
        }

        /// <summary>
        /// Shows a modal message dialog externally.
        /// </summary>
        /// <param name="context">The context in which the dialog is shown.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message presented to the user.</param>
        /// <param name="style">The style of the message dialog (e.g., Affirmative, AffirmativeAndNegative).</param>
        /// <param name="settings">Optional settings to customize the dialog appearance and behavior.</param>
        /// <returns>The user's decision.</returns>
        public MessageDialogResult ShowModalMessageExternal(object context, string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings? settings = null)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.ShowModalMessageExternal(title, message, style, settings);
        }

        /// <summary>
        /// Shows an asynchronous progress dialog.
        /// </summary>
        /// <param name="context">The context in which the dialog is shown.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message presented to the user.</param>
        /// <param name="isCancelable">Indicates whether the progress dialog can be cancelled by the user.</param>
        /// <param name="settings">Optional settings to customize the dialog appearance and behavior.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the progress dialog controller.</returns>
        public Task<ProgressDialogController> ShowProgressAsync(object context, string title, string message, bool isCancelable = false, MetroDialogSettings? settings = null)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.ShowProgressAsync(title, message, isCancelable, settings));
        }

        /// <summary>
        /// Shows an asynchronous custom dialog.
        /// </summary>
        /// <param name="context">The context in which the dialog is shown.</param>
        /// <param name="dialog">The custom dialog to be shown.</param>
        /// <param name="settings">Optional settings to customize the dialog appearance and behavior.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates when the dialog is closed.</returns>
        public Task ShowMetroDialogAsync(object context, BaseMetroDialog dialog, MetroDialogSettings? settings = null)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.ShowMetroDialogAsync(dialog, settings));
        }

        /// <summary>
        /// Hides an asynchronous custom dialog.
        /// </summary>
        /// <param name="context">The context in which the dialog is shown.</param>
        /// <param name="dialog">The custom dialog to be hidden.</param>
        /// <param name="settings">Optional settings to customize the dialog appearance and behavior.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates when the dialog is closed.</returns>
        public Task HideMetroDialogAsync(object context, BaseMetroDialog dialog, MetroDialogSettings? settings = null)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.HideMetroDialogAsync(dialog, settings));
        }

        /// <summary>
        /// Retrieves the current active dialog of type TDialog asynchronously.
        /// </summary>
        /// <typeparam name="TDialog">The type of the dialog.</typeparam>
        /// <param name="context">The context in which the dialog is shown.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the current dialog or null if none is active.</returns>
        public Task<TDialog?> GetCurrentDialogAsync<TDialog>(object context)
            where TDialog : BaseMetroDialog
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.GetCurrentDialogAsync<TDialog>());
        }

        /// <summary>
        /// Gets the MetroWindow instance from the provided context.
        /// </summary>
        /// <param name="context">The context from which to retrieve the MetroWindow instance.</param>
        /// <returns>The MetroWindow associated with the given context.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the context is null.</exception>
        private MetroWindow GetMetroWindow(object context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return (Window as MetroWindow)!;
        }

        #endregion
    }
}
