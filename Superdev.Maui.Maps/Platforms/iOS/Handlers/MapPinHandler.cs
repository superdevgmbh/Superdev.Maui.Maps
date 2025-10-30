using MapKit;
using Microsoft.Maui.Maps.Handlers;
using Superdev.Maui.Maps.Controls;
using UIKit;
using Superdev.Maui.Maps.Platforms.Extensions;
using Superdev.Maui.Maps.Platforms.Utils;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    using PM = PropertyMapper<Pin, MapPinHandler>;

    public class MapPinHandler : Microsoft.Maui.Maps.Handlers.MapPinHandler
    {
        public new static readonly PM Mapper = new PM(Microsoft.Maui.Maps.Handlers.MapPinHandler.Mapper)
        {
            [nameof(Pin.ImageSource)] = MapImageSource,
            [nameof(Pin.IsSelected)] = MapIsSelected
        };

        private WeakReference<MKMapView> mapViewRef;

        public MapPinHandler()
            : base(Mapper)
        {
        }

        public MapPinHandler(IPropertyMapper mapper = null)
            : base(mapper ?? Mapper)
        {
        }

        private new Pin VirtualView => (Pin)base.VirtualView;

        private static void MapImageSource(MapPinHandler mapPinHandler, Pin pin)
        {
            mapPinHandler.UpdateAnnotation(mapPinHandler, pin);
        }

        private void UpdateAnnotation(MapPinHandler _, Pin pin)
        {
            if (pin.Map.TryGetTarget(out var map) && map.Handler is MapHandler mapHandler)
            {
                if (pin.MarkerId is IMKAnnotation annotation)
                {
                    var mapView = mapHandler.PlatformView;
                    var annotationView = mapView.ViewForAnnotation(annotation);
                    if (annotationView != null)
                    {
                        if (pin.ImageSource is ImageSource imageSource && this.MauiContext is MauiContext mauiContext)
                        {
                            var image = ImageCache.GetImage(imageSource, mauiContext);
                            annotationView.Image = image;
                        }
                        else
                        {
                            annotationView.Image = null;
                        }
                    }
                }
            }
        }

        private static void MapIsSelected(MapPinHandler mapPinHandler, Pin pin)
        {
            if (pin.Map.TryGetTarget(out var map))
            {
                map.Handler?.UpdateValue(nameof(Pin.IsSelected));
            }
        }
    }
}