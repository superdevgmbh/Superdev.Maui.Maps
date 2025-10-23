using Superdev.Maui.Maps;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Services;

namespace MapsDemoApp.ViewModels
{
    public class MapDemoViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly IDialogService dialogService;

        private bool isScannerPause;
        private bool isScannerEnabled;
        private IRelayCommand startCameraCommand;
        private IRelayCommand stopCameraCommand;
        private IRelayCommand toggleTorchCommand;
        private bool torchOn;

        public MapDemoViewModel(
            ILogger<MapDemoViewModel> logger,
            IDialogService dialogService)
        {
            this.logger = logger;
            this.dialogService = dialogService;

            this.IsScannerEnabled = true;

            // _ = Enumerable.Range(1, count: 3).Select(async i =>
            // {
            //     await Task.Delay(i * 1000);
            //     this.IsScannerEnabled = true;
            //     this.IsScannerEnabled = false;
            //     this.IsScannerEnabled = true;
            // }).ToArray();
        }

        public bool IsScannerEnabled
        {
            get => this.isScannerEnabled;
            private set => this.SetProperty(ref this.isScannerEnabled, value);
        }

        public bool IsScannerPause
        {
            get => this.isScannerPause;
            private set => this.SetProperty(ref this.isScannerPause, value);
        }


        public IRelayCommand StartCameraCommand
        {
            get => this.startCameraCommand ??= new RelayCommand(this.StartCamera);
        }

        private void StartCamera()
        {
            this.IsScannerEnabled = true;
            this.IsScannerPause = false;
        }

        public IRelayCommand StopCameraCommand
        {
            get => this.stopCameraCommand ??= new RelayCommand(this.StopCamera);
        }

        private void StopCamera()
        {
            this.IsScannerEnabled = false;
        }

        public bool TorchOn
        {
            get => this.torchOn;
            set => this.SetProperty(ref this.torchOn, value);
        }

        public IRelayCommand ToggleTorchCommand
        {
            get => this.toggleTorchCommand ??= new RelayCommand(this.ToggleTorch);
        }

        private void ToggleTorch()
        {
            this.TorchOn = !this.TorchOn;
        }
    }
}
