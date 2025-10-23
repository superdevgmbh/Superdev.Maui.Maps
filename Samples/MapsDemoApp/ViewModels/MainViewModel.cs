using System.Windows.Input;
using MapsDemoApp.Services.Navigation;
using Superdev.Maui.Maps;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace MapsDemoApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly ILauncher launcher;

        private IAsyncRelayCommand appearingCommand;
        private bool isInitialized;
        private IAsyncRelayCommand<string> navigateToPageCommand;
        private IAsyncRelayCommand<string> navigateToModalPageCommand;
        private IAsyncRelayCommand<string> openUrlCommand;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            INavigationService navigationService,
            IDialogService dialogService,
            ILauncher launcher)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.launcher = launcher;
        }

        public IAsyncRelayCommand AppearingCommand
        {
            get => this.appearingCommand ??= new AsyncRelayCommand(this.OnAppearingAsync);
        }

        private async Task OnAppearingAsync()
        {
            if (!this.isInitialized)
            {
                await this.InitializeAsync();
                this.isInitialized = true;
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "InitializeAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", "Initialization failed", "OK");
            }
        }

        public IAsyncRelayCommand<string> NavigateToPageCommand
        {
            get => this.navigateToPageCommand ??= new AsyncRelayCommand<string>(this.NavigateToPageAsync);
        }

        private async Task NavigateToPageAsync(string page)
        {
            await this.navigationService.PushAsync(page);
        }

        public IAsyncRelayCommand<string> NavigateToModalPageCommand
        {
            get => this.navigateToModalPageCommand ??= new AsyncRelayCommand<string>(this.NavigateToModalPageAsync);
        }

        private async Task NavigateToModalPageAsync(string page)
        {
            await this.navigationService.PushModalAsync(page);
        }

        public IAsyncRelayCommand<string> OpenUrlCommand
        {
            get => this.openUrlCommand ??= new AsyncRelayCommand<string>(this.OpenUrlAsync);
        }

        private async Task OpenUrlAsync(string url)
        {
            try
            {
                await this.launcher.TryOpenAsync(url);
            }
            catch
            {
                // Ignore exceptions
            }
        }
    }
}