using Minimal.Mvvm;
using System.Diagnostics;
using static AccessModifier;

namespace MovieWpfApp.ViewModels
{
    partial class MovieViewModel
    {
        #region Command Methods

        private bool CanClose()
        {
            return IsWindowed;
        }

        [Notify(Setter = Private)]
        private void Close()
        {
            Debug.Assert(WindowService != null, $"{nameof(WindowService)} is null");
            WindowService?.Close();
        }

        [Notify(Setter = Private)]
        private void ContentRendered()
        {
            IsWindowed = true;
            CloseCommand?.RaiseCanExecuteChanged();
        }

        #endregion

        #region Methods

        protected override void CreateCommands()
        {
            base.CreateCommands();
            ContentRenderedCommand = RegisterCommand(ContentRendered);
            CloseCommand = RegisterCommand(Close, CanClose);
        }

        #endregion
    }
}
