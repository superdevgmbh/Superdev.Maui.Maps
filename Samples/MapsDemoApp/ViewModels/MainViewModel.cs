using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Services;
using Microsoft.Extensions.Logging;

namespace MapsDemoApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly INavigationService navigationService;
        private readonly ILauncher launcher;

        private IAsyncRelayCommand<string>? navigateToPageCommand;
        private IAsyncRelayCommand<string>? openUrlCommand;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            INavigationService navigationService,
            ILauncher launcher)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.launcher = launcher;
        }

        public IAsyncRelayCommand<string> NavigateToPageCommand
        {
            get => this.navigateToPageCommand ??= new AsyncRelayCommand<string>(this.NavigateToPageAsync);
        }

        private async Task NavigateToPageAsync(string page)
        {
            var stopwatch = Stopwatch.StartNew();
            await this.navigationService.PushAsync(page);
            this.logger.LogTrace($"NavigateToPageAsync finished in {stopwatch.ElapsedMilliseconds}ms");
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