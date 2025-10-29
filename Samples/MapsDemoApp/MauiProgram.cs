using MapsDemoApp.Services.Logging;
using MapsDemoApp.ViewModels;
using MapsDemoApp.Views;
using Superdev.Maui.Maps;
using CommunityToolkit.Maui;
using MapsDemoApp.Services;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Superdev.Maui;

namespace MapsDemoApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSuperdevMaui()
                .UseSuperdevMauiMaps()
                .UseMauiCommunityToolkit()
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
            builder.Services.AddSingleton<ILauncher>(_ => Launcher.Default);
            builder.Services.AddSingleton<IMediaPicker>(_ => MediaPicker.Default);
            builder.Services.AddSingleton<IClipboard>(_ => Clipboard.Default);
            builder.Services.AddSingleton<IShare>(_ => Share.Default);
            builder.Services.AddSingleton<IGeolocation>(_ => Geolocation.Default);
            builder.Services.AddSingleton<IParkingLotService, ParkingLotService>();

            // Register pages and view models
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainViewModel>();

            builder.Services.AddTransient<MauiMapDemoPage>();
            builder.Services.AddTransient<MauiMapDemoViewModel>();

            builder.Services.AddTransient<MapDemoPage>();
            builder.Services.AddTransient<MapDemoViewModel>();

            return builder.Build();
        }
    }
}