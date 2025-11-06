#if ANDROID
using Android.Gms.Common;
using Android.Gms.Maps;
#endif

using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.LifecycleEvents;
using Superdev.Maui.Maps.Controls;

namespace Superdev.Maui.Maps
{
    /// <summary>
    /// This class contains the Map's <see cref="MauiAppBuilder"/> extensions.
    /// </summary>
    public static class MauiAppBuilderExtensions
    {
        /// <summary>
        /// Configures <see cref="MauiAppBuilder"/> to add support for the <see cref="Map"/> control.
        /// </summary>
        /// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
        /// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
        public static MauiAppBuilder UseSuperdevMauiMaps(this MauiAppBuilder builder)
        {
#if (ANDROID || IOS)
            builder
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers.AddMauiMaps();
                })
                .ConfigureLifecycleEvents(events =>
                {
#if ANDROID
                    events.AddAndroid(android => android
                        .OnCreate((a, b) =>
                        {
                            Superdev.Maui.Maps.Platforms.Handlers.MapHandler.Bundle = b;
                            if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(a) == ConnectionResult.Success)
                            {
                                try
                                {
                                    MapsInitializer.Initialize(a);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Google Play Services Not Found");
                                    Console.WriteLine("Exception: {0}", e);
                                }
                            }
                        }));
#endif
                });

            //builder.Services.TryAddSingleton<IGeolocatorService>(_ => IGeolocatorService.Current);
#endif

            return builder;
        }

        /// <summary>
        /// Registers the .NET MAUI Maps handlers that are needed to render the map control.
        /// </summary>
        /// <param name="handlers">An instance of <see cref="IMauiHandlersCollection"/> on which to register the map handlers.</param>
        /// <returns>The provided <see cref="IMauiHandlersCollection"/> object with the registered map handlers for subsequent registration calls.</returns>
        public static IMauiHandlersCollection AddMauiMaps(this IMauiHandlersCollection handlers)
        {
#if (ANDROID || IOS)
            handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, Microsoft.Maui.Maps.Handlers.MapHandler>();
            handlers.AddHandler<Superdev.Maui.Maps.Controls.Map, Superdev.Maui.Maps.Platforms.Handlers.MapHandler>();

            handlers.AddHandler<Microsoft.Maui.Controls.Maps.Pin, Microsoft.Maui.Maps.Handlers.MapPinHandler>();
            handlers.AddHandler<Superdev.Maui.Maps.Controls.Pin, Superdev.Maui.Maps.Platforms.Handlers.MapPinHandler>();

            handlers.AddHandler<MapElement, Microsoft.Maui.Maps.Handlers.MapElementHandler>();
#endif

            return handlers;
        }
    }
}