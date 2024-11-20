using MetroWpfApp.Models;
using Minimal.Mvvm;
using System.Diagnostics;

namespace MetroWpfApp.ViewModels
{
    partial class MoviesViewModel
    {
        #region Properties

        [Notify(Setter = AccessModifier.Private)]
        private MoviesSettings? _settings;

        #endregion

        #region Methods

        private void CreateSettings()
        {
            if (Settings != null)
            {
                return;
            }
            Settings = new MoviesSettings();
            Lifetime.AddBracket(Settings.Initialize, Settings.Uninitialize);
            Lifetime.AddBracket(LoadSettings, SaveSettings);
        }

        private void LoadSettings()
        {
            Debug.Assert(IsInitialized, $"{GetType().FullName} ({DisplayName ?? "Unnamed"}) ({GetHashCode()}) is not initialized.");
            Debug.Assert(SettingsService != null, $"{nameof(SettingsService)} is null");
            Debug.Assert(Settings != null, $"{nameof(Settings)} is null");
            using (Settings!.SuspendDirty())
            {
                SettingsService!.LoadSettings(Settings);
            }
        }

        private void SaveSettings()
        {
            Debug.Assert(SettingsService != null, $"{nameof(SettingsService)} is null");
            Debug.Assert(Settings != null, $"{nameof(Settings)} is null");
            if (Settings!.IsDirty && SettingsService!.SaveSettings(Settings))
            {
                Settings.ResetDirty();
            }
        }

        #endregion
    }
}
