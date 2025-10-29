#if ANDROID
using Android.Gms.Common;
using Android.Gms.Maps;
#endif

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Maps.Handlers;
using Superdev.Maui.Maps.Controls;
using Map = Microsoft.Maui.Controls.Maps.Map;

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
                    // Log everything in this one
                    events.AddAndroid(android => android
                        .OnCreate((a, b) =>
                        {
                            Microsoft.Maui.Maps.Handlers.MapHandler.Bundle = b;
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
#if IOS
            handlers.AddHandler<Map, Platforms.Handlers.MapHandler>();

#endif
            handlers.AddHandler<Pin, MapPinHandler>();

            handlers.AddHandler<CustomMap, Platforms.Handlers.CustomMapHandler>();
            handlers.AddHandler<CustomPin, Platforms.Handlers.CustomMapPinHandler>();

            handlers.AddHandler<MapElement, MapElementHandler>();
#endif

            return handlers;
        }
    }
}