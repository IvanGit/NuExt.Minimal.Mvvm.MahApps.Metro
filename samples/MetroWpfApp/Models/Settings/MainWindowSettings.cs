﻿using Minimal.Mvvm;

namespace MetroWpfApp.Models
{
    public sealed partial class MainWindowSettings : SettingsBase
    {
        [Notify]
        private bool _moviesOpened;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            MoviesOpened = true;
        }
    }
}
