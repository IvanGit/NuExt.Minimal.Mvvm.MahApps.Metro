using Minimal.Mvvm;

namespace MetroWpfApp.Models
{
    public sealed class MainWindowSettings : SettingsBase
    {
        private bool _moviesOpened;
        public bool MoviesOpened
        {
            get => _moviesOpened;
            set => SetProperty(ref _moviesOpened, value);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            MoviesOpened = true;
        }
    }
}
