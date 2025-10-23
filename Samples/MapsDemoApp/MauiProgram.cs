using MapsDemoApp.Services.Logging;
using MapsDemoApp.Services.Navigation;
using MapsDemoApp.ViewModels;
using MapsDemoApp.Views;
using Superdev.Maui.Maps;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace MapsDemoApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSuperdevMauiMaps()
                .UseMauiCommunityToolkit(o =>
                {
                    o.SetPopupDefaults(new DefaultPopupSettings
                    {
                        Margin = 0,
                        Padding = 0,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        CanBeDismissedByTappingOutsideOfPopup = false,
                    });
                    o.SetPopupOptionsDefaults(new DefaultPopupOptionsSettings
                    {
                        PageOverlayColor = Colors.Black.WithAlpha(0.5f),
                        Shadow = null,
                        Shape = null,
                        CanBeDismissedByTappingOutsideOfPopup = false,
                    });
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("IBMPlexMono-Bold.ttf", "IBMPlexMonoBold");
                    fonts.AddFont("IBMPlexMono-Regular.ttf", "IBMPlexMonoRegular");
                    fonts.AddFont("IBMPlexSans-Bold.ttf", "IBMPlexSansBold");
                    fonts.AddFont("IBMPlexSans-Regular.ttf", "IBMPlexSansRegular");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddLogging(b =>
            {
                b.ClearProviders();
                b.SetMinimumLevel(LogLevel.Trace);
                b.AddNLog(NLogLoggerConfiguration.GetLoggingConfiguration());
                b.AddSentry(SentryConfiguration.Configure);
            });

            // Register services
            builder.Services.AddSingleton<IPageResolver, PageResolver>();
            builder.Services.AddSingleton<INavigationService, MauiNavigationService>();
            builder.Services.AddSingleton<IPopupService2, PopupService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<ILauncher>(_ => Launcher.Default);
            builder.Services.AddSingleton<IMediaPicker>(_ => MediaPicker.Default);
            builder.Services.AddSingleton<IClipboard>(_ => Clipboard.Default);
            builder.Services.AddSingleton<IShare>(_ => Share.Default);

            // Register pages and view models
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainViewModel>();

            builder.Services.AddTransient<MapDemoPage>();
            builder.Services.AddTransient<MapDemoViewModel>();

            return builder.Build();
        }
    }
}
