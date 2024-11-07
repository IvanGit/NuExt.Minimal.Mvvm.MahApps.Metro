using MetroWpfApp.Models;
using System.Diagnostics;

namespace MetroWpfApp.ViewModels
{
    internal partial class MainWindowViewModel
    {
        #region Properties

        private MainWindowSettings? _settings;
        public MainWindowSettings? Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        #endregion

        #region Methods

        private void CreateSettings()
        {
            if (Settings != null)
            {
                return;
            }
            Settings = new MainWindowSettings();
            Settings.Initialize();
            Settings.SuspendChanges();
            Lifetime.AddBracket(LoadSettings, SaveSettings);
        }

        private void LoadSettings()
        {
            Debug.Assert(IsInitialized, $"{GetType().FullName} ({DisplayName ?? "Unnamed"}) ({GetHashCode()}) is not initialized.");
            Debug.Assert(SettingsService != null, $"{nameof(SettingsService)} is null");
            Debug.Assert(Settings != null, $"{nameof(Settings)} is null");
            if (Settings!.IsSuspended)
            {
                Settings.ResumeChanges();
                Debug.Assert(Settings.IsSuspended == false);
                using (Settings.SuspendDirty())
                {
                    SettingsService!.LoadSettings(Settings);
                }
            }
        }

        private void SaveSettings()
        {
            Debug.Assert(SettingsService != null, $"{nameof(SettingsService)} is null");
            Debug.Assert(Settings != null, $"{nameof(Settings)} is null");
            if (Settings!.IsDirty)
            {
                if (SettingsService!.SaveSettings(Settings))
                {
                    Settings.ResetDirty();
                }
            }
        }

        #endregion

    }
}
